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

using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal sealed class NalUnitStreamParser : IStreamParser
	{
		/// <summary>
		/// Per NAL unit type code the capable parser
		/// </summary>
		private readonly INalUnitParser _nalUnitParser;
		private readonly IH264State _state;
		private readonly bool[] _nalUnitStartByteLongLengthPrefix;
		private readonly bool[] _nalUnitStartByteShortLengthPrefix;
		private readonly uint _minSliceNalUnitLength;
		private readonly uint _maxSliceNalUnitLength;

		public NalUnitStreamParser(INalUnitParser nalUnitParser, IH264State state)
		{
			_nalUnitParser = nalUnitParser;
			_state = state;
			_nalUnitStartByteLongLengthPrefix = new bool[256];
			_nalUnitStartByteShortLengthPrefix = new bool[256];

			for (int nalRefIdc = 0; nalRefIdc <= 3; nalRefIdc++)
			{
				int nonIdrPictureSliceIndex = (nalRefIdc << 5) | (int)NalUnitType.CodedSliceOfANonIdrPicture;
				_nalUnitStartByteLongLengthPrefix[nonIdrPictureSliceIndex] = true;
			}
			for (int nalRefIdc = 1; nalRefIdc <= 3; nalRefIdc++)
			{
				int nalRefIdcBits = (nalRefIdc << 5);
				_nalUnitStartByteLongLengthPrefix[nalRefIdcBits | (int)NalUnitType.CodedSliceOfAnIdrPicture] = true;
				_nalUnitStartByteShortLengthPrefix[nalRefIdcBits | (int)NalUnitType.SequenceParameterSet] = true;
				_nalUnitStartByteShortLengthPrefix[nalRefIdcBits | (int)NalUnitType.PictureParameterSet] = true;
			}

			Configurable<H264Detector.ConfigurationKey> configurable = H264Detector.Configurable;
			_minSliceNalUnitLength = (uint)configurable[H264Detector.ConfigurationKey.MinSliceNalUnitLength];
			_maxSliceNalUnitLength = (uint)configurable[H264Detector.ConfigurationKey.MaxSliceNalUnitLength];
		}

		public void Parse(IH264Reader reader, IResultNodeState resultState)
		{
			long bytesRemaining = (reader.Length - reader.Position);
			if (bytesRemaining < 4)
			{
				resultState.Invalidate();
				return;
			}

			long startPosition = reader.Position;

			// Check whether a sequence parameter set and a picture parameter set are
			// separated by exactly one byte in a NAL unit stream.
			// This is the case for H.264 streams embedded in 3GPP files.
			ulong nextFiveBytes = reader.PeekFiveBytes();
			if ((_state.NalUnitType == NalUnitType.SequenceParameterSet) &&
				!IsShortLengthPrefixedNalUnit((uint)(nextFiveBytes >> 16), bytesRemaining) &&
				 IsShortLengthPrefixedNalUnit((uint)(nextFiveBytes >> 8) & 0xffffff, (bytesRemaining - 1)))
			{
				// Skip the byte that is part of the 3GPP container
				reader.Position++;
				bytesRemaining--;
				nextFiveBytes = reader.PeekFiveBytes();
			}
			if (IsShortLengthPrefixedNalUnit((uint)(nextFiveBytes >> 16), bytesRemaining))
			{
				if (ParseShortLengthPrefixedNalUnit(reader, resultState) && resultState.Valid)
				{
					return; // Successfully parse SPS or PPS NAL unit
				}

				resultState.Reset();
				reader.Position = startPosition;
			}
			if (IsLongLengthPrefixedNalUnit(nextFiveBytes, bytesRemaining))
			{
				if (ParseLongLengthPrefixedNalUnit(reader, resultState))
				{
					return; // Successfully parse slice NAL unit
				}
			}

			resultState.Invalidate();
		}

		private bool ParseShortLengthPrefixedNalUnit(IH264Reader reader, IResultNodeState resultState)
		{
			uint nalUnitLength = reader.GetUShort(NalUnitParser.Attribute.NalUnitLength);
			long nalUnitEndPosition = (reader.Position + nalUnitLength);
			if (nalUnitEndPosition > reader.Length)
			{
				return false;
			}

			//Parse remainder with specific nalunitparser (NOTE: Is not a sub-object)
			reader.ParseOneNalUnit(_nalUnitParser, resultState, nalUnitEndPosition);

			return (reader.Position == nalUnitEndPosition);
		}

		private bool ParseLongLengthPrefixedNalUnit(IH264Reader reader, IResultNodeState resultState)
		{
			uint nalUnitLength = reader.GetUInt(NalUnitParser.Attribute.NalUnitLength);
			long nalUnitEndPosition = (reader.Position + nalUnitLength);
			if (nalUnitEndPosition > reader.Length)
			{
				return false;
			}

			//Parse remainder with specific nalunitparser (NOTE: Is not a sub-object)
			reader.ParseOneNalUnit(_nalUnitParser, resultState, nalUnitEndPosition);

			return (reader.Position == nalUnitEndPosition);
		}

		internal bool IsShortLengthPrefixedNalUnit(uint nextThreeBytes, long bytesRemaining)
		{
			if (!_nalUnitStartByteShortLengthPrefix[nextThreeBytes & 0xff])
			{
				return false;
			}

			uint nalUnitLength = (nextThreeBytes >> 8);
			if (nalUnitLength > (bytesRemaining - 2))
			{
				return false;
			}
			if ((nextThreeBytes & 0x1f) == (int)NalUnitType.SequenceParameterSet)
			{
				return (nalUnitLength >= 5) && (nalUnitLength <= 100);
			}

			// PictureState parameter set
			return (nalUnitLength >= 2) && (nalUnitLength <= 20);
		}

		internal bool IsLongLengthPrefixedNalUnit(ulong nextFiveBytes, long bytesRemaining)
		{
			if (!_nalUnitStartByteLongLengthPrefix[(int)nextFiveBytes & 0xff])
			{
				return false;
			}

			var nalUnitLength = (uint)(nextFiveBytes >> 8);
			if (nalUnitLength > (bytesRemaining - 4))
			{
				return false;
			}

			return (nalUnitLength > _minSliceNalUnitLength) && (nalUnitLength < _maxSliceNalUnitLength);
		}
	}
}
