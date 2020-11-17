/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Netherlands Forensic Institute nor the names 
 *    of its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Defraser.DataStructures;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	internal sealed class H264ResultNodeCallback : IResultNodeCallback
	{
		private static readonly string FirstMacroblockInSliceName = Enum.GetName(typeof(SliceHeader.Attribute), SliceHeader.Attribute.FirstMacroblockInSlice);

		private readonly IH264State _state;
		private readonly IDictionary<string, List<string>> _possibleChildHeaders;
		private IResultNode _lastHeader;

		public H264ResultNodeCallback(IH264State state)
		{
			_state = state;

			_possibleChildHeaders = new Dictionary<string, List<string>>();
			_possibleChildHeaders.Add(SequenceParameterSet.Name, new List<string> { PictureParameterSet.Name });
			_possibleChildHeaders.Add(PictureParameterSet.Name, new List<string> { SupplementalEnhancementInformation.Name, IdrPictureSlice.Name, NonIdrPictureSlice.Name });
		}

		public void AddNode(IResultNode rootNode, IResultNode newChildNode, IResultNodeState resultState)
		{
			if (IsSlice(newChildNode))
			{
				_state.SliceCount++;
			}

			// Check occurance of new child node (at this point)
			if (!IsSuitableSibling(_lastHeader, newChildNode))
			{
				resultState.Invalidate();
				return;
			}

			// Add new child node to the tree
			var parent = GetParentFor(rootNode, newChildNode);
			parent.AddChild(newChildNode);
			_lastHeader = newChildNode;
			_state.ParsedHeaderCount++;
		}

		public void Reset()
		{
			_lastHeader = null;
		}

		private static bool IsSuitableSibling(IResultNode prev, IResultNode next)
		{
			if (prev == null)
			{
				return true; // (broken) H.264 streams can start with any NAL unit
			}
			if (IsUnparsableSlice(next) && !CheckFirstMacroblockInSliceSequence(prev, next))
			{
				return false;
			}

			// TODO: implement these rules

			//switch (next.Name)
			//{
			//    case SequenceParameterSet.Name:
			//        return IsSlice(prev) || (prev.Name == EndOfSequence.Name);

			//    case PictureParameterSet.Name:
			//        return prev.Name == SequenceParameterSet.Name;

			//    case EndOfSequence.Name:
			//        return IsSlice(prev);

			//    case EndOfStream.Name:
			//        return (prev.Name == EndOfSequence.Name);

			//    case IdrPictureSlice.Name:
			//    case NonIdrPictureSlice.Name:
			//        return IsSlice(prev) || (prev.Name == PictureParameterSet.Name);
			//}

			return true;
		}

		private static bool CheckFirstMacroblockInSliceSequence(IResultNode prev, IResultNode next)
		{
			// Determine sequence of up to 16 values for 'first macroblock in slice'
			IList<uint> seq = new List<uint>();
			var firstFirstMacroblockInSlice = GetFirstMacroblockInSlice(next);
			if (firstFirstMacroblockInSlice == null)
			{
				return false;
			}

			seq.Add(firstFirstMacroblockInSlice.Value);

			for (IResultNode node = prev; (node != null) && (seq.Count < 16); node = GetPreviousResult(node))
			{
				var firstMacroblockInSlice = GetFirstMacroblockInSlice(node);
				if (firstMacroblockInSlice == null)
				{
					return false; // Every slice should have this field!
				}
				seq.Add(firstMacroblockInSlice.Value);
			}

			seq = new List<uint>(seq.Reverse());

			// Now check the sequence ...
			if ((seq[0] == seq[1]) && (seq[0] != 0))
			{
				return false; // Subsequent elements that are equal but not '0', e.g. {5,5,5}, are not allowed!
			}

			var maxVal = seq.Max();
			var stepSize = ((seq.Count() == 2) || (seq[1] != 0)) ? (seq[1] - seq[0]) : (seq[2] - seq[1]);

			for (int i = 1; i < seq.Count; i++)
			{
				uint current = seq[i], previous = seq[i - 1];
				if (previous != ((current == 0) ? maxVal : (current - stepSize)))
				{
					return false;
				}
			}
			return true;
		}

		private static IResultNode GetPreviousResult(IResultNode node)
		{
			if (node == null)
			{
				return null;
			}

			IResultNode parentNode = node.Parent;
			if (parentNode == null)
			{
				return null;
			}

			var children = parentNode.Children;
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] == node)
				{
					if (i == 0)
					{
						return GetPreviousResult(parentNode);
					}

					return children[i - 1].GetLastDescendant();
				}
			}
			return null;
		}

		private static bool IsSlice(IResult result)
		{
			return (result.Name == IdrPictureSlice.Name) || (result.Name == NonIdrPictureSlice.Name);
		}

		private static bool IsUnparsableSlice(IResult result)
		{
			return IsSlice(result) && !result.Valid;
		}

		private IResultNode GetParentFor(IResultNode rootNode, IResult nextHeader)
		{
			string nextHeaderName = nextHeader.Name;
			for (IResultNode parent = rootNode.GetLastDescendant(); parent != null; parent = parent.Parent)
			{
				string parentName = parent.Name;
				if (_possibleChildHeaders.ContainsKey(parentName) && _possibleChildHeaders[parentName].Contains(nextHeaderName))
				{
					return parent;
				}
			}
			return rootNode; // Any valid header can occur in the root
		}

		private static uint? GetFirstMacroblockInSlice(IResultNode resultNode)
		{
			if (resultNode == null)
			{
				return null;
			}

			var firstMacroblockInSlice = resultNode.FindAttributeByName(FirstMacroblockInSliceName);
			if (firstMacroblockInSlice == null)
			{
				return null;
			}

			return (uint)firstMacroblockInSlice.Value;
		}
	}
}
