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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal sealed class NalUnitParser : INalUnitParser, IDetectorColumnsInitializer
	{
		internal enum Attribute
		{
			ZeroByte,
			StartCodePrefix,
			ForbiddenZeroBit,
			NalRefIdc,
			NalUnitType,
			NalUnitLength,
			TrailingZeroBytes
		}

		/// <summary>
		/// Per NAL unit type code the capable parser
		/// </summary>
		private readonly IDictionary<NalUnitType, INalUnitPayloadParser> _nalUnitParsers;

		public NalUnitParser(IEnumerable<INalUnitPayloadParser> nalUnitParsers)
		{
			_nalUnitParsers = new Dictionary<NalUnitType, INalUnitPayloadParser>();

			foreach (INalUnitPayloadParser nalUnitParser in nalUnitParsers)
			{
				_nalUnitParsers[nalUnitParser.UnitType] = nalUnitParser;
			}
		}

		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			reader.GetFixedBits(1, 0, Attribute.ForbiddenZeroBit);
			reader.State.NalRefIdc = reader.GetBits(2, Attribute.NalRefIdc);
			reader.State.NalUnitType = (NalUnitType)reader.GetBits(5, Attribute.NalUnitType);

			INalUnitPayloadParser nalUnitPayloadParser;
			if (!_nalUnitParsers.TryGetValue(reader.State.NalUnitType, out nalUnitPayloadParser))
			{
				resultState.Invalidate();
				return;
			}

			//Parse remainder with specific nalunitparser (NOTE: Is not a sub-object)
			nalUnitPayloadParser.Parse(reader, resultState);

			if (!resultState.Valid)
			{
				return; // Stop processing of invalid NAL unit.
			}
			if (!CanParseNalUnitType(reader.State.SliceState, reader.State.NalUnitType))
			{
				// Forward to the estimated end of the NAL unit
				reader.Position = reader.Length;
				resultState.Invalidate(); // Invalidates the result
				resultState.Recover(); // Revalidates the reader state so parsing can continue!
				return;
			}

			if (reader.State.NalUnitType == NalUnitType.SequenceParameterSet)
			{
				return; // Note: SPS validate their own data!
			}
			if (reader.State.NalUnitType == NalUnitType.PictureParameterSet)
			{
				return; // Note: PPS validate their own data!
			}

			if (!IsSlice(reader.State.NalUnitType) || !reader.State.SliceState.PictureState.EntropyCodingMode)
			{
				if (reader.GetBits(1) != 1) // rbsp_stop_one_bit (equal to 1)
				{
					resultState.Invalidate();
					return;
				}
			}
			if (IsSlice(reader.State.NalUnitType) && resultState.Valid)
			{
				return; // Random (e.g. audio) data is allowed after a valid slice
			}

			reader.ReadZeroAlignmentBits();
		}

		private static bool CanParseNalUnitType(ISliceState sliceState, NalUnitType nalUnitType)
		{
			if (!IsSlice(nalUnitType))
			{
				return true;
			}
			if (sliceState == null)
			{
				return false; // Slice with no picture parameter set available
			}

			// FIXME: certain CAVLC and CABAC files do not work, e.g. separate colour plane YCrCb 4:4:4 files

			return true; // H.264 slice that can be parsed
		}

		private static bool IsSlice(NalUnitType nalUnitType)
		{
			return (nalUnitType == NalUnitType.CodedSliceOfANonIdrPicture) || (nalUnitType == NalUnitType.CodedSliceOfAnIdrPicture);
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			// Add default columns to include for all headers
			IDetectorColumnsBuilder defaultDetectorColumnsBuilder = builder.WithDefaultColumns(Enum.GetNames(typeof(Attribute)));

			// Add the columns for all headers and include the default columns (for each header)
			foreach (var nalUnitParser in _nalUnitParsers.Values)
			{
				nalUnitParser.AddColumnsTo(defaultDetectorColumnsBuilder);
			}
		}
	}
}
