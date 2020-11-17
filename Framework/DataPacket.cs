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
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Data packet described by file, offset and length.
	///<para>
	/// Instances of <see cref="DataPacket"/> describe a single contiguous block
	/// of data in an input file, i.e. an unfragmented data packet.
	/// </para>
	/// </summary>
	[DataContract]
	public class DataPacket : IDataPacket, IEquatable<DataPacket>
	{
		#region Inner classes
		public sealed class SerializationContext : ISerializationContext
		{
			private readonly Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;

			#region Properties
			public Type Type { get { return typeof(DataPacket); } }
			#endregion Properties

			public SerializationContext(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets)
			{
				PreConditions.Argument("appendDataPackets").Value(appendDataPackets).IsNotNull();

				_appendDataPackets = appendDataPackets;
			}

			public void CompleteDeserialization(Object obj)
			{
				((DataPacket)obj).CompleteDeserialization(_appendDataPackets);
			}
		}
		#endregion Inner classes

		private Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;

		#region Properties
		[DataMember]
		public IInputFile InputFile { get; private set; }
		[DataMember]
		public long StartOffset { get; private set; }
		public long EndOffset { get { return (StartOffset + Length); } }
		[DataMember]
		public long Length { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new data packet for the entire <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="appendDataPackets">
		/// The factory method for creating one data packet by appending two existing
		/// data packets. This delegate is used by <see cref="Append(IDataPacket)"/>.
		/// </param>
		/// <param name="inputFile">The input file</param>
		public DataPacket(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets, IInputFile inputFile)
			: this(appendDataPackets, inputFile, 0, inputFile.Length)
		{
		}

		/// <summary>
		/// Creates a new data packet for a part of <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">the input file</param>
		/// <param name="offset">the offset of the data in the file</param>
		/// <param name="length">the length of the data</param>
		public DataPacket(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets, IInputFile inputFile, long offset, long length)
		{
			PreConditions.Argument("appendDataPackets").Value(appendDataPackets).IsNotNull();
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();
			PreConditions.Argument("offset").Value(offset).IsNotNegative();
			PreConditions.Argument("length").Value(length).InRange(1L, (inputFile.Length - offset));

			_appendDataPackets = appendDataPackets;
			InputFile = inputFile;
			StartOffset = offset;
			Length = length;
		}

		private void CompleteDeserialization(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets)
		{
			_appendDataPackets = appendDataPackets;
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();

			if ((dataPacket is DataPacket) && dataPacket.InputFile.Equals(InputFile) && (dataPacket.StartOffset == EndOffset))
			{
				return new DataPacket(_appendDataPackets, InputFile, StartOffset, (Length + dataPacket.Length));
			}

			return _appendDataPackets(this, dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			return CreateSubPacket(offset, length);
		}

		public IDataPacket GetFragment(long offset)
		{
			return CreateSubPacket(offset, (Length - offset));
		}

		private IDataPacket CreateSubPacket(long offset, long length)
		{
			PreConditions.Argument("offset").Value(offset).InRange(0L, (Length - 1L));
			PreConditions.Argument("length").Value(length).InRange(1L, (Length - offset));

			if ((offset == 0L) && (length == Length))
			{
				return this;	// Sub-packet is the entire packet
			}

			return new DataPacket(_appendDataPackets, InputFile, (StartOffset + offset), length);
		}

		#region Equals() and GetHashCode()
		public override bool Equals(object obj)
		{
			return Equals(obj as DataPacket);
		}

		public bool Equals(DataPacket other)
		{
			if (other == null) return false;
			if (other == this) return true;

			if (!other.InputFile.Equals(InputFile)) return false;
			if (other.StartOffset != StartOffset) return false;
			if (other.Length != Length) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return InputFile.GetHashCode()
				.CombineHashCode(StartOffset)
				.CombineHashCode(Length);
		}
		#endregion Equals() and GetHashCode()
	}
}
