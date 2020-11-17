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

using System.Collections.Generic;
using Defraser.DataStructures;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class Mpeg2VideoCarver : IDataBlockCarver
	{
		private static readonly IDictionary<IDataPacket, ISequenceState> NoDefaultHeaders = new ReadOnlyDictionary<IDataPacket, ISequenceState>(new Dictionary<IDataPacket, ISequenceState>());

		private readonly IMpeg2VideoReader _reader;
		private readonly IResultParser<IMpeg2VideoReader> _videoHeaderParser;
		private readonly IScanContext _scanContext;
		private readonly IMpeg2VideoState _state;
		private readonly uint _minHeaderCount;

		public Mpeg2VideoCarver(IMpeg2VideoReader reader, IResultParser<IMpeg2VideoReader> videoHeaderParser, IScanContext scanContext)
		{
			_reader = reader;
			_videoHeaderParser = videoHeaderParser;
			_scanContext = scanContext;
			_state = reader.State;
			_minHeaderCount = (uint)Mpeg2VideoDetector.Configurable[Mpeg2VideoDetector.ConfigurationKey.MinVideoHeaderCount];
		}

		public bool Carve(long offsetLimit)
		{
			_state.Reset();

			// Set default header(s) from detector configuration
			_reader.ReferenceHeaders = (_scanContext.ReferenceHeader as IDictionary<IDataPacket, ISequenceState>) ?? NoDefaultHeaders;

			// Note: This will not return extension, system end or user data start codes,
			//       as these cannot occur at the start of a block.

			uint startCode;
			while ((startCode = _reader.NextStartCode()) != 0)
			{
				// Note: 0x100 is for picture headers, range 0x101..0x1af is for slices
				if ((startCode <= 0x1af) || (startCode == SequenceHeader.SequenceStartCode) || (startCode == GroupOfPicturesHeader.GopStartCode))
				{
					return true;
				}

				_reader.FlushBits(24);
			}
			return false;
		}

		public void ParseHeader(IReaderState readerState)
		{
			readerState.Parse(_videoHeaderParser, _reader);
		}

		public bool ValidateDataBlock(IDataBlockBuilder dataBlockBuilder, long startOffset, long endOffset)
		{
			if (!IsValidResult()) return false;

			dataBlockBuilder.DataFormat = _state.IsMpeg2() ? CodecID.Mpeg2Video : CodecID.Mpeg1Video;
			dataBlockBuilder.IsFullFile = IsFullFile();
			dataBlockBuilder.IsFragmented = _state.IsFragmented;

			// Trim zero byte stuffing from last header (if any)
			if (_state.LastHeaderZeroByteStuffing > 0)
			{
				dataBlockBuilder.EndOffset = endOffset - _state.LastHeaderZeroByteStuffing;
			}
			if (_state.ReferenceHeader != null)
			{
				dataBlockBuilder.ReferenceHeaderOffset = _state.ReferenceHeaderPosition - startOffset;
				dataBlockBuilder.ReferenceHeader = _state.ReferenceHeader;
			}
			if (_state.Sequence.Initialized)
			{
				var sequenceState = new SequenceState();
				_state.Sequence.CopyTo(sequenceState);
				_scanContext.ReferenceHeader = sequenceState;
			}
			return true;
		}

		private bool IsValidResult()
		{
			if (_state.ParsedHeaderCount == _state.InvalidSliceCount)
			{
				// FIXME: Long sequences of invalid slices will have O(n^2) performance!!
				return false; // None of the result nodes are valid
			}

			// Next two rules reduce the number of false positives
			if (OnlyOnePictureHeaderOneSliceWithWrongVerticalPosition())
			{
				return false;
			}
			if (OneSliceFollowedByOneHeader())
			{
				return false;
			}
			if (OnlyUnparsableSlicesOnlyFollowedByPictureHeader())
			{
				// FIXME: Long sequences of invalid slices will have O(n^2) performance!!
				return false;
			}

			if (_state.ParsedHeaderCount >= _minHeaderCount)
			{
				// At least (two) headers found!
				return true;
			}
			if (_scanContext.IsFragmented)
			{
				// A single header is enough to fill a fragmented header
				return true;
			}

			if ((_state.FirstHeaderName == SequenceHeader.Name) && (_state.ParsedHeaderCount >= 1))
			{
				// This result can be used as a 'reference header', so it is valuable!!
				return true;
			}

			return false;
		}

		private bool OnlyOnePictureHeaderOneSliceWithWrongVerticalPosition()
		{
			if ((_state.ParsedHeaderCount != 2) || (_state.LastHeaderName != PictureHeader.Name))
			{
				return false; // Not exactly one picture header
			}
			if ((_state.ValidSliceCount + _state.InvalidSliceCount) != 1)
			{
				//    ((Slice)header.Children[0].Children[0]).SliceNumber != 1
				return false; // Not exactly one slice
			}

			return true;
		}

		private bool OneSliceFollowedByOneHeader()
		{
			if (_state.ParsedHeaderCount != 2)
			{
				return false; // Not exactly 2 headers
			}

			uint sliceCount = (_state.InvalidSliceCount + _state.ValidSliceCount);
			if (((sliceCount == 0) || ((sliceCount == 1) && (_state.LastHeaderName == Slice.Name))))
			{
				return false; // First header is not a slice
			}

			return true;
		}

		private bool OnlyUnparsableSlicesOnlyFollowedByPictureHeader()
		{
			if (_state.LastHeaderName != PictureHeader.Name)
			{
				return false; // Results does not end with a picture header
			}
			if (_state.ValidSliceCount > 0)
			{
				return false; // Result contains parsable slices
			}
			if (_state.InvalidSliceCount != (_state.ParsedHeaderCount - 1))
			{
				return false; // Result contains non-slices (apart from the picture header)
			}

			return true;
		}

		private bool IsFullFile()
		{
			return (_state.FirstHeaderName == SequenceHeader.Name) && (_state.LastHeaderName == SequenceEndCode.Name);
		}
	}
}
