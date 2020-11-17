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
using System.Linq;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class Mpeg2VideoHeaderParser : IResultParser<IMpeg2VideoReader>, IDetectorColumnsInitializer
	{
		internal enum Attribute
		{
			Marker,
			Reserved,
			StartCode,
			//ByteEntropy,
			ZeroByteStuffing
		}

		private readonly IDictionary<uint, IVideoHeaderParser> _headerParsers;

		public Mpeg2VideoHeaderParser(IEnumerable<IVideoHeaderParser> videoHeaderParsers, Slice slice)
		{
			_headerParsers = new Dictionary<uint, IVideoHeaderParser>();

			foreach (IVideoHeaderParser videoHeaderParser in videoHeaderParsers)
			{
				_headerParsers.Add(videoHeaderParser.StartCode, videoHeaderParser);
			}
			for (uint startCode = 0x101; startCode <= 0x1af; startCode++)
			{
				_headerParsers.Add(startCode, slice);
			}
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			// Add default columns to include for all headers
			IDetectorColumnsBuilder defaultDetectorColumnsBuilder = builder.WithDefaultColumns(Enum.GetNames(typeof(Attribute)));

			// Add the columns for all headers and include the default columns (for each header)
			foreach (var headerParser in _headerParsers.Values.Distinct())
			{
				headerParser.AddColumnsTo(defaultDetectorColumnsBuilder);
			}
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			IMpeg2VideoState state = reader.State;

			// Check start code and determine header-specific parsing strategy
			uint startCode = reader.GetBits(32, Attribute.StartCode, "{0:X8}");
			state.StartCode = startCode;

			IVideoHeaderParser headerParser;
			if (!resultState.Valid || !_headerParsers.TryGetValue(startCode, out headerParser))
			{
				resultState.Invalidate();
				return;
			}

			// Check consistent use of MPEG-2 extensions and detect version (MPEG-1 or MPEG-2)
			if ((state.LastHeaderName == PictureHeader.Name) || (state.LastHeaderName == SequenceHeader.Name))
			{
				var detectedFormat = (startCode == ExtensionParser.ExtensionStartCode) ? CodecID.Mpeg2Video : CodecID.Mpeg1Video;
				if (state.MpegFormat == CodecID.Unknown)
				{
					state.MpegFormat = detectedFormat;
				}
				else if (detectedFormat != state.MpegFormat)
				{
					// Inconsistent use of MPEG-2 extensions:
					// Extensions are not allowed in MPEG-1 streams and are mandatory in MPEG-2 streams!
					resultState.Invalidate();
					return;
				}
			}

			// Determine parent node for this result *BEFORE* parsing this result (and changing the internal state)!!
			resultState.ParentName = GetSuitableParent(reader.State);

			// Invoke the header-specific parsing strategy
			headerParser.Parse(reader, resultState);

			if (!reader.Valid) return;

			// Handle stuffing (leading 00's before the next start code)
			uint zeroByteStuffing = reader.GetZeroByteStuffing(Attribute.ZeroByteStuffing);

			// Break if the maximum number of headers has been reached
			var headerName = resultState.Name as string;
			if (IsFragmentBreakPoint(state, headerName))
			{
				reader.BreakFragment();
				return;
			}

			// Record the header and (possible) the extension
			if (state.LastHeaderName == null) state.FirstHeaderName = headerName;
			state.LastHeaderName = headerName;
			state.LastHeaderZeroByteStuffing = zeroByteStuffing;
			state.ParsedHeaderCount++;

			if (headerName == Slice.Name)
			{
				if (resultState.Valid)
				{
					state.ValidSliceCount++;
				}
				else
				{
					state.InvalidSliceCount++;
				}
			}
		}

		private static object GetSuitableParent(IMpeg2VideoState state)
		{
			uint startCode = state.StartCode;
			if (startCode == SequenceHeader.SequenceStartCode || startCode == SequenceEndCode.SequenceEndStartCode)
			{
				return null; // root
			}
			if (startCode == GroupOfPicturesHeader.GopStartCode)
			{
				return SequenceHeader.Name; // or root, if no sequence is open
			}
			if (startCode == PictureHeader.PictureStartCode)
			{
				return state.SeenGop ? GroupOfPicturesHeader.Name : SequenceHeader.Name; // or root, if no sequence is open
			}
			if (state.Picture.Initialized)
			{
				return PictureHeader.Name;
			}
			if (state.SeenGop)
			{
				return GroupOfPicturesHeader.Name;
			}
			if (state.Sequence.Initialized)
			{
				return SequenceHeader.Name;
			}

			return null; // Place the result in the root
		}

		private static bool IsFragmentBreakPoint(IMpeg2VideoState state, string headerName)
		{
			if (state.ParsedHeaderCount <= state.Configuration.MaxVideoHeaderCount)
			{
				return false;
			}
			if (headerName == SequenceHeader.Name)
			{
				return true;
			}
			if (headerName == GroupOfPicturesHeader.Name)
			{
				return !state.Sequence.Initialized;
			}
			if (headerName == PictureHeader.Name)
			{
				return !state.Sequence.Initialized && !state.SeenGop;
			}
			if (headerName == Slice.Name)
			{
				return !state.Picture.Initialized;
			}

			return false;
		}
	}
}
