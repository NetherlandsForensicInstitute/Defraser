/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All rights reserved.
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
using System.Linq;
using Defraser.Interface;

namespace Defraser.DataStructures
{
	/// <summary>
	/// Provides extension methods to <c>IResultNode</c> implementations.
	/// </summary>
	public static class IResultNodeExtensions
	{
		/// <summary>
		/// Adds a <paramref name="childResult"/> to the <paramref name="result"/>
		/// node's <c>Children</c> and sets the <c>DataBlock</c> property of the
		/// child to <paramref name="result"/>.
		/// </summary>
		/// <param name="result">the result to add the child result to</param>
		/// <param name="childResult">the child result to add</param>
		public static void AddChild(this IResultNode result, IResultNode childResult)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			if (childResult == null)
			{
				throw new ArgumentNullException("childResult");
			}

			result.Children.Add(childResult);
			childResult.Parent = result;
		}

		/// <summary>
		/// Inserts a <paramref name="childResult"/> in the <paramref name="result"/>
		/// node's children and sets the <c>DataBlock</c> property of the child to
		/// <paramref name="result"/>.
		/// </summary>
		/// <param name="result">the result to insert the child result to</param>
		/// <param name="index">the index where to insert to child result</param>
		/// <param name="childResult">the child result to insert</param>
		public static void InsertChild(this IResultNode result, int index, IResultNode childResult)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			if (index < 0 || index > result.Children.Count)
			{
				throw new ArgumentException("index");
			}
			if (childResult == null)
			{
				throw new ArgumentNullException("childResult");
			}

			result.Children.Insert(index, childResult);
			childResult.Parent = result;
		}

		/// <summary>
		/// Removes a <paramref name="childResult"/> from this result node's
		/// children and clears the <c>DataBlock</c> property of the child.
		/// </summary>
		/// <param name="result">the result to remove the child result from</param>
		/// <param name="childResult">the child result to remove</param>
		public static void RemoveChild(this IResultNode result, IResultNode childResult)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			if (childResult == null)
			{
				throw new ArgumentNullException("childResult");
			}
			if (!result.Children.Remove(childResult))
			{
				throw new ArgumentException("No such child.", "childResult");
			}

			childResult.Parent = null;
		}

		/// <summary>
		/// Tests whether a <paramref name="result"/> has children.
		/// </summary>
		/// <returns>true if the result has children, false if it has none</returns>
		public static bool HasChildren(this IResultNode result)
		{
			return result.Children.Count >= 1;
		}

		/// <summary>
		/// Creates a copy of <paramref name="result"/> without its children.
		/// </summary>
		/// <param name="result">the result to copy</param>
		/// <return>the shallow copy</return>
		public static IResultNode ShallowCopy(this IResultNode result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			return new ResultNodeCopy(result);
		}

		/// <summary>
		/// Creates a deep copy of <paramref name="result"/> and its children.
		/// </summary>
		/// <param name="result">the result to copy</param>
		/// <return>the deep copy</return>
		public static IResultNode DeepCopy(this IResultNode result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}

			IResultNode resultCopy = result.ShallowCopy();

			// Create copies of children
			foreach (IResultNode childResult in result.Children)
			{
				resultCopy.AddChild(childResult.DeepCopy());
			}
			return resultCopy;
		}

		/// <summary>
		/// Determine whether <paramref name="resultNode"/> is a root node.
		/// </summary>
		/// <param name="resultNode">the result node to test for being the root</param>
		/// <returns>true when <paramref name="resultNode"/> is the root, else false.</returns>
		public static bool IsRoot(this IResultNode resultNode)
		{
			if (resultNode == null)
			{
				throw new ArgumentNullException("resultNode");
			}

			return (resultNode.Parent == null);
		}

		public static IResultNode GetLastDescendant(this IResultNode resultNode)
		{
			return resultNode.HasChildren() ? resultNode.GetLastChild().GetLastDescendant() : resultNode;
		}

		public static IResultNode GetFirstChild(this IResultNode resultNode)
		{
			return resultNode.Children.First();
		}

		public static IResultNode GetLastChild(this IResultNode resultNode)
		{
			return resultNode.Children.Last();
		}

		public static IResultNode FindChild(this IResultNode resultNode, string name)
		{
			if (resultNode.Name == name)
			{
				return resultNode;
			}
			foreach (IResultNode childResultNode in resultNode.Children)
			{
				IResultNode foundChild = childResultNode.FindChild(name);
				if (foundChild != null)
				{
					return foundChild;
				}
			}
			return null;
		}

		public static bool IsKeyframe(this IResultNode resultNode)
		{
			// At least one detector should consider this a key frame
			foreach (IDetector detector in resultNode.Detectors)
			{
				var codecDetector = detector as ICodecDetector;
				if ((codecDetector != null) && codecDetector.IsKeyFrame(resultNode))
				{
					return true;
				}
			}
			return false;
		}
	}
}
