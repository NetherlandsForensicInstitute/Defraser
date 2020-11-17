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
using System.IO;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Reads data as a stream of bits from the underlying reader.
	/// This implementation is able to read upto 32 bits at-a-time.
	/// </summary>
	public class BitStreamDataReader : BufferedDataReader
	{
		/// <summary>Default block size for streaming operation.</summary>
		public const int DefaultBlockSize = 32 * 1024;

		/// <summary>Holds (upto) 32 bits</summary>
		private uint _bitCache;
		/// <summary>Number of bits available in _bitCache</summary>
		private int _bitsInCache;
		/// <summary>Bit offset within the byte (in the buffer)</summary>
		private int _bitOffset;
		/// <summary>Block size for streaming operation</summary>
		private readonly int _blockSize;

		// Switch to enable/disable logging.
		// Use the file 'App.config' in GuiCe to enable/disable this switch (look for 'LogBitSteamDataReader').
#if DEBUG
		private BooleanSwitch _logSwitch = new BooleanSwitch("LogBitSteamDataReader", "Bit steam data reader");
#endif // DEBUG

		#region Properties
		/// <summary>The current location to read from, in bytes.</summary>
		public sealed override long Position
		{
			get
			{
				long position = _bufferPosition + _bufferOffset;

				// Rewind by bits in bitcache / forward by bit offset
				position += (_bitOffset - _bitsInCache + 7) >> 3;

				return (position > _length) ? _length : position;
			}
			set
			{
				if (_bitsInCache > 0)
				{
					FlushBitCache();
				}
				base.Position = value;
			}
		}

		public Pair<byte, long> BitPosition
		{
			/// <summary>Get the position taking the bit offset and byte position into account</summary>
			get { return new Pair<byte, long>(BitOffset, BytePosition); }
			/// <summary>Set the position taking the bit offset and byte position into account</summary>
			set
			{
				BitOffset = value.First;
				BytePosition = value.Second;
				FillBitCache();
			}
		}

		/// <summary>
		/// Return true when the stream is aligned to the next byte boundary.
		/// </summary>
		public bool ByteAligned
		{
			get { return BitOffset == 0; }
		}

		/// <summary>
		/// The byte position is different from the Position. The Position is the property from the 
		/// base class. The base class is a byte stream reader and does not take bits into account.
		/// Use this property in companion with the <code>BitOffset</code> to get the position of
		/// the bit stream reader.
		/// </summary>
		public long BytePosition
		{
			get
			{
				long position = _bufferPosition + _bufferOffset;

				// Rewind by bits in bitcache / forward by bit offset
				position += (_bitOffset - _bitsInCache) >> 3;

				return (position > _length) ? _length : position;
			}
			set
			{
				if (_bitsInCache > 0)
				{
					FlushBitCache();
				}
				base.Position = value;
			}
		}

		/// <summary>
		/// The bit offset (0..7) in the byte at the byte at position: <code>BytePosition</code>.
		/// </summary>
		public byte BitOffset
		{
			get { return (byte)((_bitOffset - _bitsInCache) & 0x7); }
			set
			{
				_bitCache = 0;
				_bitsInCache = 0;
				_bitOffset = value;
			}
		}

		/// <summary>
		/// Show the bits the same way as the MTS4EA Elementary Stream Analyzer does.
		/// This is easy for comparison the results of Defraser an the Tektronix tool.
		/// </summary>
		private byte BitOffsetTektronix
		{
			get { return (byte)(7 - ((_bitOffset - _bitsInCache) & 0x7)); }
		}
		#endregion Properties

		/// <summary>
		/// Constructs a new bit stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		public BitStreamDataReader(IDataReader dataReader)
			: this(dataReader, DefaultBlockSize)
		{
		}

		/// <summary>
		/// Constructs a new bit stream reader for the specified
		/// <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data stream</param>
		/// <param name="blockSize">the block size for streaming operation</param>
		public BitStreamDataReader(IDataReader dataReader, int blockSize)
			: base(dataReader, 2 * blockSize)
		{
			_blockSize = blockSize;

#if DEBUG
			if (_logSwitch.Enabled)
			{
				StreamWriter streamWriter = new StreamWriter(new FileStream(@"BitSteamDataReader.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite));

				Debug.Listeners.Clear();
				Debug.Listeners.Add(new TextWriterTraceListener(streamWriter));
			}
#endif // DEBUG
		}

		public override string ToString()
		{
			long byteOffset = BytePosition;	// TODO: this is the relative byte offset
			return string.Format("byte {0} (0x{1}), bit {2}", byteOffset, byteOffset.ToString("X"), BitOffset);
		}

		/// <summary>
		/// Retrieves the next byte from the data (byte-aligned) and
		/// increments <c>Position</c>.
		/// </summary>
		/// <returns>the byte read</returns>
		public byte GetByte()
		{
#if DEBUG
			BreakAtOffset();

			Debug.WriteIf(_logSwitch.Enabled, string.Format("GetByte: pos {0}+{1}", BytePosition, BitOffset));
#endif // DEBUG

			if (_bitsInCache > 0)
			{
				FlushBitCache();
			}
			if (_bufferOffset >= _bytesInBuffer)
			{
				FillBuffer(_blockSize);
			}
#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, string.Format(", value {0}", _buffer[_bufferOffset]));
#endif // DEBUG

			return _buffer[_bufferOffset++];
		}

		/// <summary>
		/// Retrieves given number of bits from the underlying data stream.
		/// </summary>
		/// <param name="numBits">the number of bits to retrieve</param>
		/// <returns>the bits</returns>
		public uint GetBits(int numBits)
		{
#if DEBUG
			Debug.WriteIf(_logSwitch.Enabled, string.Format("GetBits: num {0}, pos {1}+{2}", numBits, BytePosition, BitOffset));

			BreakAtOffset();
#endif // DEBUG

			Debug.Assert(numBits > 0 && numBits <= 32);
			uint result;

			if (numBits < _bitsInCache)
			{
				result = _bitCache >> (32 - numBits);
				_bitCache <<= numBits;
				_bitsInCache -= numBits;
			}
			else
			{
				FillBitCache();

				if (numBits == 32)
				{
					result = _bitCache;
					_bitCache = 0;
					_bitsInCache = 0;

					FillBitCache();
				}
				else
				{
					result = _bitCache >> (32 - numBits);
					_bitCache <<= numBits;
					_bitsInCache -= numBits;
				}
			}

#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, string.Format(", value {0}", result));
#endif // DEBUG

			return result;
		}

		/// <summary>
		/// Retrieves the value from <paramref name="vlcTable"/> corresponding
		/// to a VLC code from the underlying data stream.
		/// </summary>
		/// <param name="vlcTable">the VLC table</param>
		/// <returns>the value</returns>
		/// <remarks>returns default(T) for invalid bit patterns</remarks>
		public T GetVlc<T>(VlcTable<T> vlcTable)
		{
			Vlc vlc = vlcTable.GetVlc(ShowBits(vlcTable.MaxBits));

			if (vlc == null)
			{
				return vlcTable.DefaultValue;
			}
			else
			{
				GetBits(vlc.Length);	// Flush the bits
				return vlcTable[vlc];
			}
		}

		/// <summary>
		/// Shows (peeks) the next number of bits from the underlying
		/// data stream without advancing the stream pointer.
		/// </summary>
		/// <param name="numBits">the number of bits to show</param>
		/// <returns>the bits</returns>
		public uint ShowBitsAlign(int numBits)
		{
#if DEBUG
			Debug.WriteIf(_logSwitch.Enabled, string.Format("ShowBits: num {0}, pos {1}+{2}", numBits, BytePosition, BitOffset));
#endif // DEBUG

			Debug.Assert(numBits > 0 && numBits <= 24);
			if (_bitsInCache < numBits)
			{
				FillBitCache();
			}

#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, string.Format(", value {0}", _bitCache >> (32 - numBits)));
#endif // DEBUG

			return _bitCache >> (32 - (numBits + BitOffset));
		}

		/// <summary>
		/// Shows (peeks) the next number of bits from the underlying
		/// data stream without advancing the stream pointer.
		/// </summary>
		/// <param name="numBits">the number of bits to show</param>
		/// <returns>the bits</returns>
		public uint ShowBits(int numBits)
		{
#if DEBUG
			Debug.WriteIf(_logSwitch.Enabled, string.Format("ShowBits: num {0}, pos {1}+{2}", numBits, BytePosition, BitOffset));
#endif // DEBUG

			Debug.Assert(numBits > 0 && numBits <= 32);
			if (_bitsInCache < numBits)
			{
				FillBitCache();
			}

#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, string.Format(", value {0}", _bitCache >> (32 - numBits)));
#endif // DEBUG

			return _bitCache >> (32 - numBits);
		}

		/// <summary>
		/// Aligns the stream to the next byte boundary.
		/// </summary>
		public void ByteAlign()
		{
#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, ("ByteAlign"));
#endif // DEBUG
			FlushBitCache();
		}

		/// <summary>
		/// Finds and shows the next start code.
		/// 
		/// The start code is defined by a bit pattern, the start code prefix,
		/// optionally followed by some suffix bits. The total number of bits
		/// for a start code should not exceed 32 bits.
		/// </summary>
		/// <remarks>
		/// The stream pointer is advanced until before the start code, so
		/// that <code>GetBits(prefixBits + suffixBits)</code> will return the
		/// start code again.
		/// </remarks>
		/// <param name="prefixBits">the number of bits in the prefix</param>
		/// <param name="prefixCode">the start code prefix bit pattern</param>
		/// <param name="suffixBits">the number of bits after the prefix</param>
		/// <returns>the start code</returns>
		public uint NextStartCode(int prefixBits, uint prefixCode, int suffixBits)
		{
#if DEBUG
			Debug.WriteLineIf(_logSwitch.Enabled, "NextStartCode");
#endif // DEBUG
			if (prefixBits < 1 || prefixBits > 32)
			{
				throw new ArgumentOutOfRangeException("prefixBits", "Start code must fit in 32 bits.");
			}
			if (prefixCode > (uint)((1UL << prefixBits) - 1))
			{
				throw new ArgumentException("Invalid start code prefix.", "prefixCode");
			}
			if (suffixBits < 0 || suffixBits > 32 - prefixBits)
			{
				throw new ArgumentOutOfRangeException("suffixBits", "Start code must fit in 32 bits.");
			}

			int maskBits = 32 - prefixBits;
			// TODO: assumes that the upper bits of the start code are (0)
			// TODO: this assumes that the start code is in fact 32-bits
			uint startCode = 0xFFFFFFFF;

			FlushBitCache();

			while ((startCode >> maskBits) != prefixCode)
			{
#if DEBUG
				BreakAtOffset();
#endif // DEBUG

				if (_bufferOffset >= _bytesInBuffer)
				{
					if (State != DataReaderState.Ready)
					{
						return 0;
					}

					FillBuffer(_blockSize);
				}

				// Shift start code, read next byte
				startCode = (startCode << 8) | _buffer[_bufferOffset++];
			}

			// Check for end-of-input (zero padding)
			if (_bufferOffset > (_bytesInBuffer - _zeroPadding))
			{
				return 0;
			}

			// Save the start code to the bit cache and return
			_bitCache = startCode;
			_bitsInCache = 32;

			return startCode >> (32 - prefixBits - suffixBits);
		}

		/// <summary>
		/// Fills the cache with the next 32 bits.
		/// </summary>
		private void FillBitCache()
		{
			if (_bitsInCache < 32 && _bitOffset > 0)
			{
				int minBits = 24 + _bitOffset;

				_bitCache |= ((uint)_buffer[_bufferOffset] << minBits) >> _bitsInCache;

				// Use the remaining bits from the last byte that was (partially) read
				if (_bitsInCache > minBits)
				{
					_bitOffset += 32 - _bitsInCache;
					_bitsInCache = 32;
					return;
				}
				else
				{
					_bufferOffset++;
					_bitOffset = 0;
					_bitsInCache += 32 - minBits;
				}
			}

			// Refill the cache, one byte at-a-time
			while (_bitsInCache < 32)
			{
				if (_bufferOffset >= _bytesInBuffer)
				{
					FillBuffer(_blockSize);
				}
				if (_bitsInCache > 24)
				{
					_bitCache |= (uint)_buffer[_bufferOffset] >> (_bitsInCache - 24);
					_bitOffset = 32 - _bitsInCache;
					_bitsInCache = 32;
					return;
				}
				else
				{
					_bitCache |= (uint)_buffer[_bufferOffset++] << (24 - _bitsInCache);
					_bitsInCache += 8;
				}
			}
		}

		/// <summary>
		/// Clears the bit cache and aligns to the next byte position.
		/// </summary>
		private void FlushBitCache()
		{
			if (_bitsInCache > 0)
			{
				if (_bitsInCache < _bitOffset)
				{
					_bufferOffset++;
				}
				else
				{
					_bufferOffset -= (_bitsInCache - _bitOffset) >> 3;
				}

				_bitCache = 0;
				_bitsInCache = 0;
				_bitOffset = 0;
			}
		}

#if DEBUG
		public void BreakAtOffset()
		{
			return;
			//const long BreakPosition = 0x14620;
			//int correction = BytePosition == 0 ? 0 : 1; // To prevent reading beyond the stream
			//if (Debugger.IsAttached && GetDataPacket(BytePosition - correction, 1).Offset + correction == BreakPosition) Debugger.Break();
		}
#endif // DEBUG
	}
}
