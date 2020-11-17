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
using Defraser.DataStructures;
using Defraser.Framework;
using Defraser.Interface;

namespace Defraser.Test
{
	/// <summary>
	/// Reads data from a byte array.
	/// </summary>
	public sealed class MockDataReader : IDataReader
	{
		private byte[] _data;
		private long _position;
		private readonly IInputFile _inputFile;

		#region Properties
		public long Position
		{
			get { return _position; }
			set
			{
				if (value < 0 || value > Length)
				{
					throw new ArgumentOutOfRangeException("Position");
				}

				_position = value;
			}
		}

		public long Length { get { return _data.Length; } }

		public DataReaderState State
		{
			get
			{
				if (_data == null)
				{
					return DataReaderState.Cancelled;
				}
				else if (Position == Length)
				{
					return DataReaderState.EndOfInput;
				}
				else
				{
					return DataReaderState.Ready;
				}
			}
		}
		#endregion Properties

		public MockDataReader(byte[] data)
			: this(data, TestFramework.CreateInputFile("<MockDataReader>"))
		{
		}

		public MockDataReader(byte[] data, IInputFile inputFile)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (inputFile == null)
			{
				throw new ArgumentNullException("inputFile");
			}

			_data = data;
			_position = 0;
			_inputFile = inputFile;
		}

		public void Dispose()
		{
			if (_data != null)
			{
				_data = null;
			}
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			if (_data == null)
			{
				throw new ObjectDisposedException("MockDataReader has been disposed");
			}
			if (offset < 0 || length < 0 || offset > this.Length || (offset + length) > this.Length)
			{
				throw new ArgumentOutOfRangeException("offset/length");
			}
			return new DataPacket((x, y) => new DataPacketNode(x, y), _inputFile, offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			if (_data == null)
			{
				throw new ObjectDisposedException("MockDataReader has been disposed");
			}
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayOffset < 0 || count < 0 || count > array.Length || (arrayOffset + count) > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayOffset/count");
			}

			count = (int)Math.Min(count, (Length - Position));
			Array.Copy(_data, Position, array, arrayOffset, count);
			return count;
		}
	}
}
