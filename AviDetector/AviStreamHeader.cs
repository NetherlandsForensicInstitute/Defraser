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
	// TODO use attributes
	public enum StreamType
	{
		//[FourCC("vids")]
		Video = 0x76696473,
		//[FourCC("auds")]
		Audio = 0x61756473,
		//[FourCC("txts")]
		Text = 0x74787473,
		//[FourCC("mids")]
		Midi = 0x6D696473
	}

	internal class AviStreamHeader : AviChunk
	{
		public new enum Attribute
		{
			StreamType,
			Handler,
			Flags,
			Priority,
			Language,
			InitialFrames,
			Scale,
			Rate,
			Start,
			Length,
			SuggestedBufferSize,
			Quality,
			SampleSize,
			Frame
		}

		public uint StreamType { get; private set; } // TODO create get method using parser
		public uint Handler { get; private set; } // TODO create get method using parser

		public AviStreamHeader(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.AviStreamHeader)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			const int FrameSize = 4 * sizeof(short);
			const int IntFieldCount = 11;
			const int ShortFieldCount = 2;
			const int ExpectedSize = IntFieldCount * sizeof(int) + ShortFieldCount * sizeof(short) + FrameSize;
			// One sample had 8 bytes more.
			// The assumption is made that the frame is written in four ints
			// instead of in four shorts.
			const int ExtraBytesAllowed = 8;

			if ((Size < ExpectedSize) ||
				(Size > ExpectedSize && Size <= ExpectedSize + ExtraBytesAllowed))	// invalidate the size attribute
			{
				parser.CheckAttribute(AviChunk.Attribute.Size, Size == ExpectedSize, false);
			}
			else if (Size > ExpectedSize + ExtraBytesAllowed)	// invalidate the complete header
			{
				parser.CheckAttribute(AviChunk.Attribute.Size, Size == ExpectedSize, true);
			}

			if (parser.BytesRemaining >= 4) StreamType = parser.GetFourCC(Attribute.StreamType);
			if (parser.BytesRemaining >= 4) Handler = parser.GetFourCC(Attribute.Handler);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Flags);
			// TODO parse flags
			//
			// AVISF_DISABLED = 0x00000001
			// Indicates this stream should not be enabled by default
			//
			// AVISF_VIDEO_PALCHANGES = 0x00010000
			// Indicates this video stream contains palette changes.
			// This flag warns the playback software that it will need to
			// animate the palette.
			//
			if (parser.BytesRemaining >= 2) parser.GetShort(Attribute.Priority);
			if (parser.BytesRemaining >= 2) parser.GetShort(Attribute.Language);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.InitialFrames);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Scale);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Rate);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Start);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Length);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.SuggestedBufferSize);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.Quality);
			if (parser.BytesRemaining >= 4) parser.GetInt(Attribute.SampleSize);
			if (Size == ExpectedSize)
			{
				if (parser.BytesRemaining >= 8) parser.GetFrame(Attribute.Frame, FrameFieldType.Short);
			}
			else if (Size == ExpectedSize + 4 * sizeof(short))
			{
				if (parser.BytesRemaining >= 16) parser.GetFrame(Attribute.Frame, FrameFieldType.Int);
			}

			return Valid;
		}
	}
}
