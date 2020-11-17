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
	internal class AviMainHeader : AviChunk
	{
		public new enum Attribute
		{
			MicroSecPerFrame,
			MaxBytesPerSec,
			PaddingGranularity,
			Flags,
			TotalFrames,
			InitialFrames,
			Streams,
			SuggestedBufferSize,
			Width,
			Height,
			Reserved
        }

		public AviMainHeader(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.AviMainHeader)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetInt(Attribute.MicroSecPerFrame);
			parser.GetInt(Attribute.MaxBytesPerSec);
			parser.GetInt(Attribute.PaddingGranularity);
			parser.GetInt(Attribute.Flags);
			// TODO parse flags
			// • AVIF_HASINDEX
			// The file has an index
			// • AVIF_MUSTUSEINDEX
			// The order in which the video and audio chunks must be replayed is determined by the
			// index and may differ from the order in which those chunks occur in the file.
			// • AVIF_ISINTERLEAVED
			// The streams are properly interleaved into each other
			// • AVIF_WASCAPTUREFILE
			// The file was captured. The interleave might be weird.
			// • AVIF_COPYRIGHTED
			// Ignore it
			// • AVIF_TRUSTCKTYPE (Open-DML only!)
			// This flag indicates that the keyframe flags in the index are reliable. If this flag is not
			// set in an Open-DML file, the keyframe flags could be defective without technically
			// rendering the file invalid.
			parser.GetInt(Attribute.TotalFrames);
			parser.GetInt(Attribute.InitialFrames);
			parser.GetInt(Attribute.Streams);
			parser.GetInt(Attribute.SuggestedBufferSize);
			parser.GetInt(Attribute.Width);
			parser.GetInt(Attribute.Height);
			parser.GetInt(Attribute.Reserved);
			parser.GetInt(Attribute.Reserved);
			parser.GetInt(Attribute.Reserved);
			parser.GetInt(Attribute.Reserved);

			return Valid;
		}
	}
}
