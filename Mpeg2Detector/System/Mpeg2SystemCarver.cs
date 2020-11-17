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

using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.System.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class Mpeg2SystemCarver : IDataBlockCarver
	{
		private readonly IMpeg2SystemReader _reader;
		private readonly IResultParser<IMpeg2SystemReader> _systemHeaderParser;
		private readonly IScanContext _scanContext;
		private readonly IMpeg2SystemState _state;
		private readonly uint _minHeaderCount;

		public Mpeg2SystemCarver(IMpeg2SystemReader reader, IResultParser<IMpeg2SystemReader> systemHeaderParser, IScanContext scanContext)
		{
			_reader = reader;
			_systemHeaderParser = systemHeaderParser;
			_scanContext = scanContext;
			_state = reader.State;
			_minHeaderCount = (uint)Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.MinSystemHeaderCount];
		}

		public bool Carve(long offsetLimit)
		{
			_state.Reset();

			// Note: Program end code (0x1b9) is not allowed at the start of a block!

			uint startCode;
			while ((startCode = _reader.NextStartCode()) != 0)
			{
				if (startCode >= 0x1ba)
				{
					return true;
				}

				_reader.FlushBits(24);
			}
			return false;
		}

		public void ParseHeader(IReaderState readerState)
		{
			// TODO: if a default header is available, try it here!

			readerState.Parse(_systemHeaderParser, _reader);
		}

		public bool ValidateDataBlock(IDataBlockBuilder dataBlockBuilder, long startOffset, long endOffset)
		{
			if (!IsValidResult()) return false;

			dataBlockBuilder.DataFormat = _state.IsMpeg2() ? CodecID.Mpeg2System : CodecID.Mpeg1System;
			dataBlockBuilder.IsFullFile = IsFullFile();
			dataBlockBuilder.IsFragmented = _state.IsFragmented;

			// Trim zero byte stuffing from last header (if any)
			if (_state.LastHeaderZeroByteStuffing > 0)
			{
				dataBlockBuilder.EndOffset = endOffset - _state.LastHeaderZeroByteStuffing;
			}
			foreach (ushort streamId in _state.Streams.StreamIds)
			{
				IDataPacket streamData = _state.Streams[streamId].GetStreamData();
				if (streamData != null)
				{
					string name = GetStreamName(streamId);
					if (name != null)
					{
						ICodecStreamBuilder codecStreamBuilder = dataBlockBuilder.AddCodecStream();
						codecStreamBuilder.Name = name;
						codecStreamBuilder.DataFormat = name.StartsWith("Video") ? CodecID.Mpeg2Video : CodecID.Unknown;
						codecStreamBuilder.StreamNumber = streamId;
						codecStreamBuilder.Data = streamData;
						codecStreamBuilder.AbsoluteStartOffset = codecStreamBuilder.Data.StartOffset;
					}
				}
			}
			return true;
		}

		private static string GetStreamName(ushort streamId)
		{
			if ((streamId >= 0xC0) && (streamId <= 0xDF))
			{
				return string.Format("Audio Stream {0}", (streamId - 0xBF));
			}
			if ((streamId >= 0xE0) && (streamId <= 0xEF))
			{
				return string.Format("Video Stream {0}", (streamId - 0xDF));
			}
			if ((streamId == 0xBD) || (streamId == 0xBF))
			{
				return string.Format("Private Stream {0}", (streamId == 0xBD) ? 1 : 2);
			}

			return null; // Not supported
		}

		private bool IsValidResult()
		{
			// TODO: false hit reduction ...

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

			return false;
		}

		private bool IsFullFile()
		{
			return (_state.FirstHeaderName == PackHeader.Name) && (_state.LastHeaderName == ProgramEndCode.Name);
		}
	}
}
