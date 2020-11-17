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

using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class ContentBrandingObject : AsfObject
	{
		enum BannerImageDataType
		{
			NoBannerImageData = 0,
			Bitmap = 1,
			Jpeg = 2,
			Gif = 3,
		}

		public new enum Attribute
		{
			BannerImageType,
			BannerImageDataSize,
			BannerImageData,
			BannerImageUrlLength,
			BannerImageUrl,
			CopyrightUrlLength,
			CopyrightUrl,
		}

		public ContentBrandingObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.ContentBrandingObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetInt(Attribute.BannerImageType, typeof(BannerImageDataType));

			int bannerImageDataSize = parser.GetInt(Attribute.BannerImageDataSize);
			//parser.GetHexDump(Attribute.BannerImageData, BannerImageDataSize);
			parser.Position += bannerImageDataSize;
			parser.AddAttribute(new FormattedAttribute<Attribute, string>(Attribute.BannerImageData, string.Format(CultureInfo.CurrentCulture, "{0} bytes skipped", bannerImageDataSize)));

			int bannerImageUrlLength = parser.GetInt(Attribute.BannerImageUrlLength);
			parser.GetString(Attribute.BannerImageUrl, bannerImageUrlLength);

			int copyrightUrlLength = parser.GetInt(Attribute.CopyrightUrlLength);
			parser.GetString(Attribute.CopyrightUrl, copyrightUrlLength);

			return Valid;
		}
	}
}
