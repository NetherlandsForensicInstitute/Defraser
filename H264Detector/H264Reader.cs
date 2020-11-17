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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	internal sealed class H264Reader : IH264Reader
	{
		#region Inner classes
		private sealed class FixedBitsResultFormatter : IValidityResultFormatter
		{
			private readonly uint _bitCode;

			#region Properties
			public bool IsValid(object value)
			{
				return (uint)value == _bitCode;
			}
			#endregion Properties

			public FixedBitsResultFormatter(uint bitCode)
			{
				_bitCode = bitCode;
			}

			public string Format(object value)
			{
				return value.ToString();
			}
		}
		#endregion Inner classes

		private readonly BitStreamDataReader _dataReader;
		private readonly IH264State _state;
		private readonly IReaderState _readerState;
		private readonly uint _maxNalUnitDistance;

		#region Properties
		/// <summary>The current location to read from, in bytes.</summary>
		public long Position
		{
			get { return _dataReader.Position; }
			set { _dataReader.Position = value; }
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		/// <summary>
		/// The number of total bytes in the stream
		/// </summary>
		public long Length
		{
			get { return _dataReader.Length; }
		}

		public IDictionary<IDataPacket, IPictureState> ReferenceHeaders { get; set; }

		private IResultState Result { get { return _readerState.ActiveState as IResultState; } }
		#endregion Properties

		public H264Reader(BitStreamDataReader dataReader, IH264State state, IReaderState readerState)
		{
			_dataReader = dataReader;
			_state = state;
			_readerState = readerState;

			Configurable<H264Detector.ConfigurationKey> configurable = H264Detector.Configurable;
			uint maxGapBetweenPpsAndSlice = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenPpsAndSlice];
			uint maxGapBetweenPpsAndSps = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenPpsAndSps];
			uint maxGapBetweenNalUnits = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenNalUnits];
			uint maxBytesBetweenNalUnits = Math.Max(maxGapBetweenNalUnits, Math.Max(maxGapBetweenPpsAndSps, maxGapBetweenPpsAndSlice));
			_maxNalUnitDistance = maxBytesBetweenNalUnits + (uint)configurable[H264Detector.ConfigurationKey.MaxSliceNalUnitLength];
		}

		public void ParseOneNalUnit(INalUnitParser parser, IResultNodeState resultState, long nalUnitEndPosition)
		{
			long startPosition = Position;
			IList<uint> sanitizedPositions = new List<uint>();
			var unitReader = NalUnitReader(nalUnitEndPosition, sanitizedPositions);
			unitReader.Result = resultState;
			parser.Parse(unitReader, resultState);

			// Synchronize real data reader with end of NAL unit
			Position = startPosition + GetRawOffset(sanitizedPositions, (uint)unitReader.Position);
			//Console.WriteLine("(Non-)Validated NAL unit of {0} bytes", (Position - startPosition));

			ReadTrailingZeroBytes((uint)Math.Min(16, (nalUnitEndPosition - Position)));

			if (Position != nalUnitEndPosition)
			{
				// TODO: er zit 'troep' (bv. audio data) tussen het einde van de huidige en volgende NAL unit
				//Console.WriteLine("Still {0} junk bytes after NAL unit, starting at offset {1:x8}", (nalUnitEndPosition - Position), Position);
			}

			//Position = nalUnitEndPosition;
		}

		private void ReadTrailingZeroBytes(uint maxZeroBytes)
		{
			for (int i = 0; i < maxZeroBytes; i++)
			{
				if (_dataReader.ShowBitsAlign(8) == 0)
				{
					_dataReader.Position++;
				}
				else
				{
					break;
				}
			}
		}

		private static uint GetRawOffset(IEnumerable<uint> startCodePrefixEmulationPreventionByteOffsetsInRawBlock, uint offsetInSanitizedBlock)
		{
			uint sanitizedByteCount = 0;
			foreach (uint startCodePrefixEmulationPreventionByteOffset in startCodePrefixEmulationPreventionByteOffsetsInRawBlock)
			{
				if (startCodePrefixEmulationPreventionByteOffset <= (offsetInSanitizedBlock + sanitizedByteCount))
				{
					sanitizedByteCount++;
				}
				else
				{
					break;
				}
			}
			return offsetInSanitizedBlock + sanitizedByteCount;
		}

		/// <summary>
		/// Creates a reader which reads up till the next nalunit
		/// </summary>
		/// <returns>A reader on a sanitized stream</returns>
		private INalUnitReader NalUnitReader(long nalUnitEndPosition, IList<uint> sanitizedPositions)
		{
			var windowStart = Position;
			long windowEnd = nalUnitEndPosition;
			var nalUnitSize = (int)(windowEnd - windowStart);
			if (nalUnitSize == 0)
			{
				return new NalUnitReader(this, _dataReader, _state);
			}
			var sanitized = Sanitize(windowStart, nalUnitSize, sanitizedPositions);
			_dataReader.Position = windowEnd;
			return new NalUnitReader(this, new BitStreamDataReader(sanitized), _state);
		}

		private IDataReader Sanitize(long startOffset, int length, ICollection<uint> sanitizedPositions)
		{
			var window = new byte[length];
			_dataReader.Read(window, 0, length);
			//find 00 00 03 then discard the third byte (03)
			int dstPos = SanitizeWindow(window, sanitizedPositions);
			// Remove trailing zeros bytes
			// FIXME: this fragment contains a prevention for emtpy (zero byte) packets!!
			while ((dstPos > 1) && (window[dstPos - 1] == 0))
			{
				dstPos--;
			}
			return new ByteArrayDataReader(window, _dataReader.GetDataPacket(startOffset, dstPos));
		}

		private static int SanitizeWindow(byte[] window, ICollection<uint> sanitizedPositions)
		{
			int srcPos = 0, dstPos = 0, windowLength = (window.Length - 3);

			// Scan for start code prefix emultation prevention sequences, i.e. 00 00 03 0x
			while (srcPos < windowLength)
			{
				byte b = window[srcPos++];
				window[dstPos++] = b;

				if ((b == 0) && (window[srcPos] == 0) && ((window[srcPos + 1] & ~3) == 0))
				{
					window[dstPos++] = window[srcPos++];

					if ((window[srcPos] != 0x03) || ((window[srcPos + 1] & ~0x3) != 0))
					{
						return dstPos; // Illegal sequence encountered!
					}

					sanitizedPositions.Add((uint)srcPos);
					srcPos++; // Skip the prevention byte (03)
				}
			}

			// Copy remaining (upto 3) bytes
			windowLength += 3;
			if ((srcPos == windowLength) && ((window[srcPos] == 0) && (window[srcPos + 1] == 0) && ((window[srcPos + 2] & ~3) == 0)))
			{
				windowLength--; // Incomplete or invalid start code prefix emulation prevention sequence
			}
			while (srcPos < windowLength)
			{
				window[dstPos++] = window[srcPos++];
			}
			return dstPos;
		}

		public bool NextNalUnit()
		{
			// FIXME: 00 00 01 xx -OF- 00 00 00 01 xx voor volgende NAL unit (extra (0) hoort dus bij volgende NAL unit)

			// 00000000 00000000 00000001 0xxxxxxx
			long startPosition = _dataReader.Position;
			uint startCode = _dataReader.NextStartCode(25, 0x0000002, 7);

			if (_dataReader.Position > (startPosition + _maxNalUnitDistance))
			{
				Result.Invalidate();
				return false;
			}
			if (startCode != 0)
			{
				return true;
			}
			if (_dataReader.Position == _dataReader.Length)
			{
				return true;
			}

			return false; // This is unlikely to occur, but should return 'false'
		}

		public uint PeekUInt()
		{
			return _dataReader.ShowBits(32);
		}

		public ulong PeekFiveBytes()
		{
			ulong value = _dataReader.ShowBits(8) << 32;
			_dataReader.Position++;
			value |= _dataReader.ShowBits(32);
			_dataReader.Position--;
			return value;
		}

		public uint GetUShort<T>(T attribute)
		{
			uint value = _dataReader.GetBits(16);
			AddAttribute(attribute, value);
			return value;
		}

		public uint GetUInt<T>(T attribute)
		{
			uint value = _dataReader.GetBits(32);
			AddAttribute(attribute, value);
			return value;
		}

		public uint GetFixedByte<T>(byte bitCode, T attributeName)
		{
			return GetFixedBits(8, bitCode, attributeName);
		}

		public uint GetFixedThreeBytes<T>(uint bitCode, T attributeName)
		{
			return GetFixedBits(24, bitCode, attributeName);
		}

		private uint GetFixedBits<T>(int numBits, uint bitCode, T attributeName)
		{
			uint value = _dataReader.GetBits(numBits);
			Result.AddAttribute(attributeName, value, new FixedBitsResultFormatter(bitCode));
			return value;
		}

		private void AddAttribute<T>(T name, object value)
		{
			Result.AddAttribute(name, value);
		}
	}
}
