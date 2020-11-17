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
using System.Diagnostics;
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	public enum Endianness
	{
		Big,
		Little
	}

	/// <summary>
	/// Reads data as a stream of bytes. When more than one byte is requested
	/// at once, the bytes are assumed to have <em>big-endian</em> byte order.
	/// </summary>
	public class ByteStreamDataReader : BufferedDataReader
	{
		/// <summary>Default block size for streaming operation.</summary>
		private const int DefaultBlockSize = 16 * 1024;

		// Block size for streaming operation
		private readonly int _blockSize;

		public Endianness Endianness { get; set; }

		/// <summary>
		/// Constructs a new byte stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		public ByteStreamDataReader(IDataReader dataReader)
			: this(dataReader, DefaultBlockSize, Endianness.Big)
		{
		}

		/// <summary>
		/// Constructs a new byte stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		/// <param name="endianness">the endianness of the datareader (default is big).
		/// Bytes are swapped when reading integers in little endian mode.</param>
		public ByteStreamDataReader(IDataReader dataReader, Endianness endianness)
			: this(dataReader, DefaultBlockSize, endianness)
		{
		}

		/// <summary>
		/// Constructs a new byte stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		/// <param name="blockSize">the block size for streaming operation</param>
		public ByteStreamDataReader(IDataReader dataReader, int blockSize)
			: base(dataReader, 2 * blockSize)
		{
			_blockSize = blockSize;
			Endianness = Endianness.Big;
		}

		/// <summary>
		/// Constructs a new byte stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		/// <param name="blockSize">the block size for streaming operation</param>
		/// <param name="endianness">the endianness of the datareader (default is big).
		public ByteStreamDataReader(IDataReader dataReader, int blockSize, Endianness endianness)
			: base(dataReader, 2 * blockSize)
		{
			_blockSize = blockSize;
			Endianness = endianness;
		}

		/// <summary>
		/// Retrieves the next byte from the underlying data stream.
		/// </summary>
		/// <returns>the byte read</returns>
		public byte GetByte()
		{
			if ((_bytesInBuffer - _bufferOffset) >= 1)
			{
				return _buffer[_bufferOffset++];
			}
			else
			{
				return (byte)GetBytes(1);
			}
		}

		/// <summary>
		/// Retrieves the next short (2 bytes) from the underlying data stream.
		/// </summary>
		/// <returns>the short read</returns>
		public short GetShort()
		{
			short value;
			if ((_bytesInBuffer - _bufferOffset) >= 2)
			{
				value = (short)((_buffer[_bufferOffset] << 8) | _buffer[_bufferOffset + 1]);
				_bufferOffset += 2;
			}
			else
			{
				value = (short)GetBytes(2);
			}
			return (Endianness == Endianness.Big) ? value : (short)ByteSwap((ushort)value);
		}

		/// <summary>
		/// Retrieves the next 3 bytes from the underlying data stream.
		/// </summary>
		/// <remarks>The endianness is not taken into account in this method</remarks>
		/// <returns>the three bytes as an <c>int</c</returns>
		public int GetThreeBytes()
		{
			if ((_bytesInBuffer - _bufferOffset) >= 3)
			{
				int value = ((_buffer[_bufferOffset] << 16) |
							 (_buffer[_bufferOffset + 1] << 8) |
							  _buffer[_bufferOffset + 2]);
				_bufferOffset += 3;
				return value;
			}
			else
			{
				return (int)GetBytes(3);
			}
		}

		/// <summary>
		/// Retrieves the next int (4 bytes) from the underlying data stream.
		/// </summary>
		/// <returns>the int read</returns>
		public int GetInt()
		{
			return GetInt(Endianness);
		}

		/// <summary>
		/// Retrieves the next int (4 bytes) from the underlying data stream.
		/// </summary>
		/// <param name="endianness">the endianness of the int to read</param>
		/// <returns>the int read</returns>
		public int GetInt(Endianness endianness)
		{
			int value;
			if ((_bytesInBuffer - _bufferOffset) >= 4)
			{
				value = (_buffer[_bufferOffset] << 24) |
							(_buffer[_bufferOffset + 1] << 16) |
							(_buffer[_bufferOffset + 2] << 8) |
							 _buffer[_bufferOffset + 3];
				_bufferOffset += 4;
			}
			else
			{
				value = (int)GetBytes(4);
			}
			return (endianness == Endianness.Big) ? value : (int)ByteSwap((uint)value);
		}

		public Guid GetGuid()
		{
			byte[] guid = new byte[16];

			if ((_bytesInBuffer - _bufferOffset) >= 16)
			{
				for (int i = 0; i < 16; i++)
				{
					guid[i] = _buffer[_bufferOffset + i];
				}
				_bufferOffset += 16;
			}
			else
			{
				for (int i = 0; i < 16; i++)
				{
					guid[i] = GetByte();
				}
			}
			return new Guid(guid);
		}

		/// <summary>
		/// Retrieves the next long (8 bytes) from the underlying data stream.
		/// </summary>
		/// <returns>the long read</returns>
		public long GetLong()
		{
			return GetLong(Endianness);
		}

		public long GetLong(Endianness endianness)
		{
			long value;
			if ((_bytesInBuffer - _bufferOffset) >= 8)
			{
				value = ((long)_buffer[_bufferOffset] << 56) |
						((long)_buffer[_bufferOffset + 1] << 48) |
						((long)_buffer[_bufferOffset + 2] << 40) |
						((long)_buffer[_bufferOffset + 3] << 32) |
						((long)_buffer[_bufferOffset + 4] << 24) |
						((long)_buffer[_bufferOffset + 5] << 16) |
						((long)_buffer[_bufferOffset + 6] << 8) |
						 (long)_buffer[_bufferOffset + 7];
				_bufferOffset += 8;
			}
			else
			{
				value = GetBytes(8);
			}
			return (Endianness == Endianness.Big) ? value : (long)ByteSwap((ulong)value);
		}

		/// <summary>
		/// Retrieves the next <paramref name="size"/> byte(s) from the
		/// underlying data stream.
		/// </summary>
		/// <remarks>The endianness is not taken into account in this method</remarks>
		/// <param name="size">the number of bytes to read</param>
		/// <returns>the byte read</returns>
		private long GetBytes(int size)
		{
			Debug.Assert(size >= 1 && size <= 8);
			long value = 0;
			while (size-- > 0)
			{
				if (_bufferOffset == _bytesInBuffer)
				{
					FillBuffer(_blockSize);
				}
				
				value = (value << 8) | _buffer[_bufferOffset++];
			}
			return value;
		}

		
		/// <summary>
		/// Swaps the bytes of ulong <paramref name="value"/>.
		/// This method converts between little-endian and big-endian.
		/// </summary>
		/// <param name="value">the value to swap</param>
		/// <returns>the byte-swapped result</returns>
		private static ulong ByteSwap(ulong value) // TODO add test code
		{
			return	((value << 56) & 0xFF00000000000000L) |
					((value >> 56) & 0x00000000000000FFL) |
					((value << 40) & 0x00FF000000000000L) |
					((value >> 40) & 0x000000000000FF00L) |
					((value << 24) & 0x0000FF0000000000L) |
					((value >> 24) & 0x0000000000FF0000L) |
					((value << 8) & 0x000000FF00000000L) |
					((value >> 8) & 0x00000000FF000000L);
		}

		/// <summary>
		/// Swaps the bytes of uint <paramref name="value"/>.
		/// This method converts between little-endian and big-endian.
		/// </summary>
		/// <param name="value">the value to swap</param>
		/// <returns>the byte-swapped result</returns>
		private static uint ByteSwap(uint value)
		{
			return ((value << 24) & 0xFF000000) |
					((value << 8) & 0xFF0000) |
					((value >> 8) & 0xFF00) |
					((value >> 24) & 0xFF);
		}

		/// <summary>
		/// Swaps the bytes of ushort <paramref name="value"/>.
		/// This method converts between little-endian and big-endian.
		/// </summary>
		/// <param name="value">the value to swap</param>
		/// <returns>the byte-swapped result</returns>
		private static ushort ByteSwap(ushort value)
		{
			return (ushort)(((value << 8) & 0xFF00) | ((value >> 8) & 0xFF));
		}
	}
}
