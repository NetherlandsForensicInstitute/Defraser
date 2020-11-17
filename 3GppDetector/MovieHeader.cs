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
	/// <summary>
	/// The movie header atom is used to specify the characteristics of an entire QuickTime movie.
	/// </summary>
	/// <remarks>Type = 'mvhd'</remarks>
	internal class MovieHeader : QtAtom
	{
		public new enum Attribute
		{
			CreationTime,
			ModificationTime,
			TimeScale,
			Duration,
			PreferredRate,
			PreferredVolume,
			Reserved,
			MatrixStructure,
			PreviewTime,
			PreviewDuration,
			PosterTime,
			SelectionTime,
			SelectionDuration,
			CurrentTime,
			NextTrackID
		}

		public MovieHeader(QtAtom previousHeader)
			: base(previousHeader, AtomName.MovieHeader)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			// This atom is not flagged a full atom to be able to parse the
			// version byte over here. To preserve memory,
			// the version byte is not made available in QtAtom.
			byte version = parser.GetByte(QtAtom.Attribute.Version);
			ParseFlags(parser);

			if (version == 1)
			{
				parser.GetLongDateTime(Attribute.CreationTime, "{0:F}");
				parser.GetLongDateTime(Attribute.ModificationTime, "{0:F}");
				parser.GetUInt(Attribute.TimeScale);
				parser.GetULong(Attribute.Duration);
			}
			else // version == 0
			{
				parser.GetDateTime(Attribute.CreationTime, "{0:F}");
				parser.GetDateTime(Attribute.ModificationTime, "{0:F}");
				parser.GetUInt(Attribute.TimeScale);
				parser.GetUInt(Attribute.Duration);
			}

			parser.GetFixed16_16(Attribute.PreferredRate);
			parser.GetFixed8_8(Attribute.PreferredVolume);
			parser.GetUShort(Attribute.Reserved);
			parser.GetUInt(Attribute.Reserved);
			parser.GetUInt(Attribute.Reserved);
			Matrix matrix = parser.GetMatrix(Attribute.MatrixStructure);
			// Matrix check from From ISO_IEC_15444-12_2005 Base Media File Format
			parser.CheckAttribute(Attribute.MatrixStructure, matrix.U == 0.0, false);
			parser.CheckAttribute(Attribute.MatrixStructure, matrix.V == 0.0, false);
			parser.CheckAttribute(Attribute.MatrixStructure, matrix.W == 1.0, false);
			parser.GetUInt(Attribute.PreviewTime);
			parser.GetUInt(Attribute.PreviewDuration);
			parser.GetUInt(Attribute.PosterTime);
			parser.GetUInt(Attribute.SelectionTime);
			parser.GetUInt(Attribute.SelectionDuration);
			parser.GetUInt(Attribute.CurrentTime);
			uint nextTrackID = parser.GetUInt(Attribute.NextTrackID);
			parser.CheckAttribute(Attribute.NextTrackID, nextTrackID != 0, false);

			return this.Valid;
		}
	}
}
