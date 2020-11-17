/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

namespace Defraser.Detector.Avi
{
	/// <summary>
	/// ExampleHeader is a template class for detector development for Defraser. It searches for
	/// blocks of 1 or more consecutive bytes with the value 42, including the next byte. These
	/// blocks are called a Header. Each detected Header will have two parameters, an integer
	/// holding the length of the header and a byte containing the last value of the header.
	/// </summary>
	internal class VideoPropertiesHeader : AviChunk
	{
		/// <summary>The possible Attributes of a detected header.</summary>
		public new enum Attribute
		{
			VideoFormatToken,
			VideoStandard,
			VerticalRefreshRate,
			HorizontalTotalInT,
			VerticalTotalInLines,
			FrameAspectRatio,
			FrameWidthInPixels,
			FrameHeightInLines,
			FieldPerFrame,
			// Video field desc
			CompressedBitmapHeight,
			CompressedBitmapWidth,
			ValidBitmapHeight,
			ValidBitmapWidth,
			ValidBitmapXOffset,
			ValidBitmapYOffset,
			VideoXOffsetInT,
			VideoYValidStartLine
		}

		enum VideoFormatToken
		{
			Unknown,
			PalSquare,
			PalCcir601,
			NtscSquare,
			NtscCcir601,
			// ...
		}

		enum VideoStandard
		{
			Unknown,
			Pal,
			Ntsc,
			Secam,
			Video
		}

		public VideoPropertiesHeader(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.VideoPropertiesHeader)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			const int IntFieldsCount = 17;
			const int ExpectedSize = IntFieldsCount * sizeof(int);

			if (Size < ExpectedSize)	// invalidate the size attribute
			{
				parser.CheckAttribute(AviChunk.Attribute.Size, Size == ExpectedSize, false);
			}
			else if (Size > ExpectedSize)	// invalidate the complete header
			{
				parser.CheckAttribute(AviChunk.Attribute.Size, Size == ExpectedSize, true);
			}

			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VideoFormatToken, typeof(VideoFormatToken));
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VideoStandard, typeof(VideoStandard));
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VerticalRefreshRate);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.HorizontalTotalInT);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VerticalTotalInLines);
			if (parser.BytesRemaining >= 4) parser.GetFrameAspectRatio(Attribute.FrameAspectRatio);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.FrameWidthInPixels);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.FrameHeightInLines);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.FieldPerFrame);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.CompressedBitmapHeight);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.CompressedBitmapWidth);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.ValidBitmapHeight);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.ValidBitmapWidth);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.ValidBitmapXOffset);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.ValidBitmapYOffset);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VideoXOffsetInT);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.VideoYValidStartLine);

			return Valid;
		}
	}
}
