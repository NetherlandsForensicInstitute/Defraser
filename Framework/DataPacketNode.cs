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
using System.Collections.Generic;
using Defraser.DataStructures;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="IDataPacket"/> implementation for fragmented data packets.
	/// <para>
	/// This class concatenates two data packets for a hierarchical data packet
	/// structure. The hierarchical structure guarantees that most methods of
	/// this class run in <em>O(log(n))</em> time, where <em>n</em> is the total
	/// number of fragments in the data packet tree.
	/// </para>
	/// </summary>
	public sealed class DataPacketNode : IDataPacket, IEquatable<DataPacketNode>
	{
		private readonly IDataPacket _firstDataPacket;
		private readonly IDataPacket _secondDataPacket;
		private readonly long _length;

		#region Properties
		public IInputFile InputFile { get { return _firstDataPacket.InputFile; } }
		public long StartOffset { get { return Math.Min(_firstDataPacket.StartOffset, _secondDataPacket.StartOffset); } }
		public long EndOffset { get { return Math.Max(_firstDataPacket.EndOffset, _secondDataPacket.EndOffset); } }
		public long Length { get { return _length; } }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="DataPacketNode"/> that concatenates two data packets.
		/// </summary>
		/// <param name="first">The first data packet</param>
		/// <param name="second">The second data packet</param>
		public DataPacketNode(IDataPacket first, IDataPacket second)
		{
			PreConditions.Argument("first").Value(first).IsNotNull();
			PreConditions.Argument("second").Value(second).IsNotNull();

			_firstDataPacket = first;
			_secondDataPacket = second;
			_length = (first.Length + second.Length);
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();

			if ((_secondDataPacket.Length + dataPacket.Length) <= _firstDataPacket.Length)
			{
				return new DataPacketNode(_firstDataPacket, _secondDataPacket.Append(dataPacket));
			}

			return new DataPacketNode(this, dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			PreConditions.Argument("offset").Value(offset).InRange(0L, (Length - 1L));
			PreConditions.Argument("length").Value(length).InRange(1L, (Length - offset));

			if ((offset == 0) && (length == Length))
			{
				return this;	// Sub-packet is the entire packet
			}

			long firstLength = _firstDataPacket.Length;
			if (offset >= firstLength)
			{
				return _secondDataPacket.GetSubPacket((offset - firstLength), length);
			}

			long relativeEndOffset = (offset + length);
			if (relativeEndOffset <= firstLength)
			{
				return _firstDataPacket.GetSubPacket(offset, length);
			}

			IDataPacket firstSubPacket = _firstDataPacket.GetSubPacket(offset, (firstLength - offset));
			IDataPacket secondSubPacket = _secondDataPacket.GetSubPacket(0, (relativeEndOffset - firstLength));
			return firstSubPacket.Append(secondSubPacket);
		}

		public IDataPacket GetFragment(long offset)
		{
			PreConditions.Argument("offset").Value(offset).InRange(0L, (Length - 1L));

			long firstLength = _firstDataPacket.Length;
			if (offset < firstLength)
			{
				return _firstDataPacket.GetFragment(offset);
			}

			return _secondDataPacket.GetFragment((offset - firstLength));
		}

		#region Equals() and GetHashCode()
		public override bool Equals(object obj)
		{
			return Equals(obj as DataPacketNode);
		}

		public bool Equals(DataPacketNode other)
		{
			if (other == null) return false;
			if (other == this) return true;

			if (!other._firstDataPacket.Equals(_firstDataPacket)) return false;
			if (!other._secondDataPacket.Equals(_secondDataPacket)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return _firstDataPacket.GetHashCode()
				.CombineHashCode(_secondDataPacket);
		}
		#endregion Equals() and GetHashCode()
	}
}
