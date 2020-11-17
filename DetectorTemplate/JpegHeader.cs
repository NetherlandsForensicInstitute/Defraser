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
using Defraser;
using DetectorCommon;

namespace DetectorTemplate
{
	internal enum JpegHeaderName
	{
		Root,
		StartOfImage,
		EndOfImage,
		StartOfScan,
		ApplicationSegment0
	}

	internal class JpegHeader : Header<JpegHeader, JpegHeaderName, JpegParser, ByteStreamDataReader>
	{
		public enum Attribute
		{
			Marker
		}


		public JpegHeader(IDetector detector)
			: base(detector, JpegHeaderName.Root)
		{
		}

		public JpegHeader(JpegHeader previousHeader, JpegHeaderName headerType)
			: base(previousHeader, headerType)
		{
		}

		public override bool Parse(JpegParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetUShort(Attribute.Marker, "{0:X4}");
			return this.Valid;
		}

		public override bool IsSuitableParent(JpegHeader parent)
		{
			switch (HeaderName)
			{
				case JpegHeaderName.StartOfImage:
				case JpegHeaderName.EndOfImage:
					return parent.HeaderName == JpegHeaderName.Root;
				case JpegHeaderName.StartOfScan:
					return parent.HeaderName == JpegHeaderName.StartOfImage;
				case JpegHeaderName.ApplicationSegment0:
					return parent.HeaderName == JpegHeaderName.StartOfImage;
				default:
					return false;
			}
		}

		public override bool IsBackToBack(JpegHeader header)
		{
			return true;	// don't care!
		}
	}
}
