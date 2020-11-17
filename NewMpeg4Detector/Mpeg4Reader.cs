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

using System.Globalization;
using Defraser.Detector.Common;
using Defraser.Util;

namespace Defraser.Detector.Mpeg4
{
	public class Mpeg4Reader : IReader
	{
		private IReader _reader;
		private readonly BitStreamDataReader _dataReader;

		public Mpeg4Reader(BitStreamDataReader dataReader)
		{
			_dataReader = dataReader;
			_reader = new Reader(_dataReader);
		}

		private BitStreamDataReader DataReader
		{
			get
			{
				return _dataReader;
			}
		}

		internal Pair<byte, long> BitPosition
		{
			get { return DataReader.BitPosition; }
			set { DataReader.BitPosition = value; }
		}

		internal uint GetStartCode(bool shortStartCode)
		{
			if (! CheckRead(shortStartCode ? 3 : 4)) return 0;

			return GetBits(shortStartCode ? 22 /*ShortHeaderLengthInBits*/ : 32 /*HeaderLengthInBits*/);
		}

		internal uint GetStartCode<T>(T attributeName, bool shortStartCode)
		{
			if (!CheckRead(shortStartCode ? 3 : 4)) return 0;

			uint value = GetStartCode(shortStartCode);
			AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.InvariantCulture, "0x{0}", value.ToString(shortStartCode ? "X06" : "X08", CultureInfo.InvariantCulture))));
			return value;
		}

		/// <summary>
		/// Perform byte alignment. When there is already byte alignment,
		/// read the next byte when it has the value of 0x7F.
		/// </summary>
		internal void AlignBits7F()
		{
			if (DataReader.ByteAligned)
			{
				if (ShowBits(8) == 0x7F)
				{
					GetByte();
				}
			}
			else
			{
				ByteAlign();
			}
		}

		protected override uint MaxZeroByteStuffingLength
		{
			get { return (byte)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.ParserMaxZeroByteStuffingLength]; }
		}

		protected override bool IsStartCode()
		{
			return (ShowBits(24) == 1 ||	// Normal header
					ShowBits(22) == 0x20);	// Short (Picture) header
		}
		/// <summary>
		/// Valid stuffing:
		/// 0 (1bit)
		/// 01
		/// 011
		/// ...
		/// 0111111 (7bits)
		///
		/// When byte aligned, the next byte must contain the value
		/// '01111111' to be valid stuffing.
		///
		/// Stuffing is followed by a startcode. This method checks for the
		/// existance of this code.
		/// </summary>
		/// <returns></returns>
		internal bool ValidStuffing()
		{
			// Already byte aligned, next byte must be 0x7F for valid byte stuffing
			if (ByteAligned)
			{
				return ShowBits(8) == 0x7F;
			}

			// if the first != zero, no valid stuffing
			if (ShowBit() == true) return false;

			// Check the remaining bits for stuffing
			uint stuffing = ShowBits(8 - DataReader.BitOffset); // get the remaining bits for this byte
			// 0x3F || 0x1F || 0x0F || 0x07 || 0x3 || 0x01
			//if (stuffing == 0) return false;
			if ((stuffing & (stuffing + 1)) == 0) return true;

			return false;
		}

		/// <summary>
		/// From ISO-IEC 14496-2-2004-MPEG-4-Video, page 17:
		/// The function nextbits_bytealigned() returns a bit string starting from
		/// the next byte aligned position. This permits comparison of a bit string
		/// with the next byte aligned bits to be decoded in the bitstream. If the
		/// current location in the bitstream is already byte aligned and the 8 bits
		/// following the current location are ‘01111111’, the bits subsequent to
		/// these 8 bits are returned. The current location in the bitstream is not
		/// changed by this function.
		/// </summary>
		internal uint ShowNextBitsByteAligned(int numBits)
		{
			uint nextBitsByteAligned;

			// Store the current location
			Pair<byte, long> bitPosition = BitPosition;

			if (ByteAligned && ShowBits(8) == 0x7F)
			{
				FlushBits(8);

				nextBitsByteAligned = GetBits(numBits);
			}
			else
			{
				ByteAlign();

				nextBitsByteAligned = GetBits(numBits);
			}

			// Restore the location
			BitPosition = bitPosition;

			return nextBitsByteAligned;
		}

#if DEBUG
		internal void BreakAtOffset()
		{
			DataReader.BreakAtOffset();
		}
#endif // DEBUG

		/// <summary>
		/// See page 17 of ISO-IEC 14496-2-2004-MPEG-4-Video
		/// The next_resync_marker() function removes any zero bit and a string
		/// of 0 to 7 ‘1’ bits used for stuffing and locates the next resync
		/// marker; it thus performs similar operation as next_start_code() but
		/// for resync_marker.
		/// </summary>
		/// <returns></returns>
		internal bool NextResyncMarker()
		{
			if (!GetZeroBit()) return false;

			while (!ByteAligned)
			{
				if (!GetOneBit()) return false;
			}
			return true;
		}

		internal int GetDCSizeLum()
		{
			int code = (int)ShowBits(11);

			for (short i = 11; i > 3; i--)
			{
				if (code == 1)
				{
					FlushBits(i);
					return i + 1;
				}
				code >>= 1;
			}

			FlushBits((short)DcLuminanceTable[code].len);
			return (int)DcLuminanceTable[code].code;
		}

		internal int GetDCSizeChrom()
		{
			int code = (int)ShowBits(12);
			for (short i = 12; i > 2; i--)
			{
				if (code == 1)
				{
					FlushBits(i);
					return i;
				}
				code >>= 1;
			}

			return (int)(3 - GetBits(2));
		}

		internal void GetLuminanceChrominance(int index)
		{
			if (index < 4)
			{
				int luminance = GetDCSizeLum();
				if (luminance != 0)
				{
					GetBits((short)luminance);
					if (luminance > 8)
					{
						GetMarkerBit();
					}
				}
			}
			else
			{
				int chrominance = GetDCSizeChrom();
				if (chrominance != 0)
				{
					GetBits((short)chrominance);
					if (chrominance > 8)
					{
						GetMarkerBit();
					}
				}
			}
		}

		internal void GetMotionVector(uint fcode)
		{
			/* decode MVDx */
			MP4Vlc1 tab;
			uint factor = fcode - 1;
			uint code = ShowBits(12);
			if (code >= 128)
			{
				tab = mp4_MVD_B12_2[((code - 128) >> 5)];
			}
			else if (code >= 2)
			{
				tab = mp4_MVD_B12_1[(code - 2)];
			}
			else
			{
				return;
			}
			FlushBits((short)tab.len);
			if (tab.code != 0)
			{
				GetBit();
				if (factor != 0)
				{
					code = GetBits((short)factor);
				}
			}

			/* decode MVDy */
			code = ShowBits(12);
			if (code >= 128)
			{
				tab = mp4_MVD_B12_2[((code - 128) >> 5)];
			}
			else if (code >= 2)
			{
				tab = mp4_MVD_B12_1[(code - 2)];
			}
			else
			{
				return;
			}
			FlushBits((short)tab.len);
			if (tab.code != 0)
			{
				GetBit();
				if (factor > 0)
				{
					GetBits((short)factor);
				}
			}
		}

	}
}
