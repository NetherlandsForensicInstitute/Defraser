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

namespace Defraser.Detector.QT
{
	internal class VideoSampleDescription : QtSampleDescriptionAtom
	{
		public new enum Attribute
		{
			Version,
			RevisionLevel,
			Vendor,
			// Video sample description specific
			TemporalQuality,
			SpatialQuality,
			Width,
			Height,
			HorizontalResolution,
			VerticalResolution,
			DataSize,
			FrameCount,
			CompressorName,
			Depth,
			ColorTableId
		}

		public VideoSampleDescription(QtAtom previousHeader)
			: base(previousHeader, AtomName.VideoSampleDescription)
		{
		}

		public VideoSampleDescription(QtAtom previousHeader, AtomName atomName)
			: base(previousHeader, atomName)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (!CheckComponentSubType(ComponentSubType.vide))
			{
				Valid = false;
				return Valid;
			}

			// Video specific
			parser.GetUShort(Attribute.Version);
			ushort revisionLevel = parser.GetUShort(Attribute.RevisionLevel);
			parser.CheckAttribute(Attribute.RevisionLevel, revisionLevel == 0, false);
			parser.GetFourCC(Attribute.Vendor);
			int temporalQuality = parser.GetInt(Attribute.TemporalQuality);
			parser.CheckAttribute(Attribute.TemporalQuality, (temporalQuality >= 0 && temporalQuality <= 1023), false);
			int spatialQuality = parser.GetInt(Attribute.SpatialQuality);
			parser.CheckAttribute(Attribute.SpatialQuality, (spatialQuality >= 0 && spatialQuality <= 1024), false);
			parser.GetUShort(Attribute.Width);
			parser.GetUShort(Attribute.Height);
			parser.GetFixed16_16(Attribute.HorizontalResolution);
			parser.GetFixed16_16(Attribute.VerticalResolution);
			int dataSize = parser.GetInt(Attribute.DataSize);
			parser.CheckAttribute(Attribute.DataSize, dataSize == 0, false);
			parser.GetUShort(Attribute.FrameCount);
			parser.GetFixedLengthString(Attribute.CompressorName, 32);

			parser.GetShort(Attribute.Depth);
			short colorTableId = parser.GetShort(Attribute.ColorTableId);
			// If the color table ID is set to 0, a color table is contained within the sample description itself.
			// The color table immediately follows the color table ID field in the sample description.
			if (colorTableId == 0)
			{
				// TODO
			}
			return Valid;
		}
	}
}
