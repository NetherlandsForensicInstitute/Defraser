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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.DataStructures
{
	/// <summary>
	/// Reads data from a byte array.
	/// </summary>
	public sealed class ByteArrayDataReader : IDataReader
	{
		private readonly IDataPacket _dataPacket;
		private readonly byte[] _data;
		private long _position;
		private bool _disposed;

		#region Properties
		public long Position
		{
			get { return _position; }
			set
			{
				if ((value < 0) || (value > Length))
				{
					throw new ArgumentOutOfRangeException("Position", value, "Must be between 0 and " + Length);
				}

				_position = value;
			}
		}

		public long Length { get { return _dataPacket.Length; } }

		public DataReaderState State
		{
			get
			{
				if (_disposed)
				{
					return DataReaderState.Cancelled;
				}
				if (Position == Length)
				{
					return DataReaderState.EndOfInput;
				}

				return DataReaderState.Ready;
			}
		}
		#endregion Properties

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data">The byte buffer</param>
		/// <param name="dataPacket">The data packet in the original reader</param>
		public ByteArrayDataReader(byte[] data, IDataPacket dataPacket)
		{
			PreConditions.Argument("data").Value(data).IsNotNull();
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();

			if (dataPacket.Length > data.Length)
			{
				throw new ArgumentOutOfRangeException("dataPacket.Length", dataPacket.Length, "Should be at most " + data.Length);
			}

			_data = data;
			_dataPacket = dataPacket;
			_position = 0;
		}

		public void Dispose()
		{
			_disposed = true;
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataPacket.GetSubPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("ByteArrayDataReader has been disposed");
			}
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayOffset < 0 || count < 0 || count > array.Length || (arrayOffset + count) > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayOffset/count", "arrayOffset=" + arrayOffset + ", count=" + count + ", this is invalid for a reader-length of " + Length);
			}
			if ((Position + count) > Length)
			{
				throw new ArgumentOutOfRangeException("Position/count", "Position=" + Position + ", count=" + count + ", this is invalid for a readerlength of " + Length);
			}

			count = (int)Math.Min(count, (Length - Position));
			Array.Copy(_data, Position, array, arrayOffset, count);
			return count;
		}
	}
}
