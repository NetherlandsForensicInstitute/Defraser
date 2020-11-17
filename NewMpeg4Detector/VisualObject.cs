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
using System.Globalization;
using System.Linq;
using Defraser.Detector.Common;

namespace Defraser.Detector.Mpeg4
{
	internal class VisualObject : Mpeg4Header
	{
		public uint VersionId { get; private set; }

		public enum ObjectType : uint
		{
			Video   = 1,
			Texture = 2,
			Mesh    = 3,
			Fba     = 4,
			Mesh_3D = 5
		}

		private enum VideoFormat : byte
		{
			Component = 0,
			Pal = 1,
			Ntsc = 2,
			Secam = 3,
			Mac = 4,
			UnspecifiedVideoFormat = 5,
			Reserved1 = 6,
			Reserved2 = 7,
		}

		public enum MatrixCoefficients
		{
			MP4_VIDEO_COLORS_FORBIDDEN         = 0,
			MP4_VIDEO_COLORS_ITU_R_BT_709      = 1,
			MP4_VIDEO_COLORS_UNSPECIFIED       = 2,
			MP4_VIDEO_COLORS_RESERVED          = 3,
			MP4_VIDEO_COLORS_ITU_R_BT_470_2_M  = 4,
			MP4_VIDEO_COLORS_ITU_R_BT_470_2_BG = 5,
			MP4_VIDEO_COLORS_SMPTE_170M        = 6,
			MP4_VIDEO_COLORS_SMPTE_240M        = 7,
			MP4_VIDEO_COLORS_GENERIC_FILM      = 8
		}

		public new enum Attribute
		{
			Type,
			Priority,
			Identifier,
			VersionID,

			VideoSignalType,			// 1 bit
			VideoFormat,				// 3 bits
			VideoRange,					// 1 bit
			ColourDescription,			// 1 bit
			ColourPrimaries,			// 8 bits
			TransferCharacteristics,	// 8 bits
			MatrixCoefficients,			// 8 bits
		}

		public VisualObject(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.VisualObject)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			bool isIdentifier = parser.GetBit(Attribute.Identifier);
			if (isIdentifier)
			{
				byte versionID = (byte)parser.GetBits(4);
				var versionIDAttribute = new FormattedAttribute<Attribute, uint>(Attribute.VersionID, versionID);
				byte[] validVersionIDs = new byte[] {1, 2, 4, 5};
				if (!validVersionIDs.Contains(versionID))
				{
					versionIDAttribute.Valid = false;
				}
				parser.AddAttribute(versionIDAttribute);

				parser.GetBits(3, Attribute.Priority);
			}
			else
			{
				VersionId = 1;
			}
			uint type = parser.GetBits(4, Attribute.Type);

			if (type > (uint)ObjectType.Mesh_3D) return false;

			if( type == (uint)ObjectType.Video || type == (uint)ObjectType.Texture)
			{	// Parse video signal type
				bool isVideoSignalType = parser.GetBit(Attribute.VideoSignalType);
				if (isVideoSignalType)
				{
					byte videoFormat = (byte)parser.GetBits(3);
					parser.AddAttribute(new FormattedAttribute<Attribute, string>(Attribute.VideoFormat, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", videoFormat.ToString(CultureInfo.CurrentCulture), Enum.GetName(typeof(VideoFormat), videoFormat))));

					parser.GetBit(Attribute.VideoRange);

					bool colourDescription = parser.GetBit(Attribute.ColourDescription);
					if(colourDescription)
					{
						parser.GetBits(8, Attribute.ColourPrimaries);
						parser.GetBits(8, Attribute.TransferCharacteristics);
						parser.GetBits(8, Attribute.MatrixCoefficients);
					}
				}
			}
			return true;
		}
	}
}
