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
using Defraser;
using DetectorCommon;

namespace DetectorTemplate
{
	/// <summary>
	/// Header from the JPEG specification document.
	/// </summary>
	internal class ApplicationSegment0 : JpegHeader
	{
		public new enum Attribute
		{
			Length,
			Identifier,
			Version,
			Units,
			XDensity,
			YDensity,
			XThumbnail,
			YThumbnail,
			RGBn
		}


		public ApplicationSegment0(JpegHeader previousHeader)
			: base(previousHeader, JpegHeaderName.ApplicationSegment0)
		{
		}

		public override bool Parse(JpegParser dataReader)
		{
			if (!base.Parse(dataReader)) return false;

			JpegParser jpegDataReader = dataReader as JpegParser;
			jpegDataReader.GetUShort(Attribute.Length, "{0:X4}");
			jpegDataReader.GetFourCC(Attribute.Identifier);
			jpegDataReader.DataReader.GetByte();	// =0
			jpegDataReader.GetUShort(Attribute.Version, "{0:X4}");
			jpegDataReader.GetByte(Attribute.Units);
			jpegDataReader.GetUShort(Attribute.XDensity);
			jpegDataReader.GetUShort(Attribute.YDensity);
			int xThumbnail = jpegDataReader.GetByte(Attribute.XThumbnail);
			int yThumbnail = jpegDataReader.GetByte(Attribute.YThumbnail);

			// The following parameter shows that data from previous parameters can 
			// be used to calculate other parameters, such as header lengths etc.
			int rgbLength = 3 * xThumbnail * yThumbnail;
			
			// As the actual byte data might be large, a placeholder string is used instead
			Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.RGBn, "<RGB values for thumbnail>"));

			// Increment the Position for the packed RGB values of the thumbnail
			jpegDataReader.DataReader.Position += rgbLength;

			return this.Valid;
		}
	}
}
