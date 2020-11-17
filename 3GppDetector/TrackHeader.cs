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

namespace Defraser.Detector.QT
{
	internal class TrackHeader : QtAtom
	{
		[Flags]
		public enum Flag
		{
			TrackEnabled = 0x00001,
			TrackInMovie = 0x00002,
			TrackInPreview = 0x00004,
			TrackInPoster = 0x00008
		}

		public new enum Attribute
		{
			/// <summary>
			/// A32-bit integer that indicates the calendar date and time (expressed in seconds since midnight,
			/// January 1, 1904) when the track headerwas created. It is strongly recommended that this value
			/// should be specified using coordinated universal time (UTC).
			/// </summary>
			CreationTime,
			/// <summary>
			/// A32-bit integer that indicates the calendar date and time (expressed in seconds since midnight,
			/// January 1, 1904) when the track header was changed. It is strongly recommended that this
			/// value should be specified using coordinated universal time (UTC).
			/// </summary>
			ModificationTime,
			/// <summary>
			/// A 32-bit integer that uniquely identifies the track. The value 0 cannot be used.
			/// </summary>
			TrackID,
			/// <summary>
			/// A 32-bit integer that is reserved for use by Apple. Set this field to 0.
			/// </summary>
			Reserved1,
			/// <summary>
			/// A time value that indicates the duration of this track (in the movie’s time coordinate system).
			/// Note that this property is derived from the track’s edits. The value of this field is equal to the
			/// sum of the durations of all of the track’s edits. If there is no edit list, then the duration is the
			/// sum of the sample durations, converted into the movie timescale.
			/// </summary>
			Duration,
			/// <summary>
			/// An 8-byte value that is reserved for use by Apple. Set this field to 0.
			/// </summary>
			Reserved2,
			/// <summary>
			/// A 16-bit integer that indicates this track’s spatial priority in its movie. The QuickTime Movie
			/// Toolbox uses this value to determine how tracks overlay one another. Tracks with lower layer
			/// values are displayed in front of tracks with higher layer values.
			/// </summary>
			Layer,
			/// <summary>
			/// A 16-bit integer that specifies a collection of movie tracks that contain alternate data for one
			/// another. QuickTime chooses one track from the group to be used when the movie is played.
			/// The choice may be based on such considerations as playback quality, language, or the
			/// capabilities of the computer.
			/// </summary>
			AlternateGroup,
			/// <summary>
			/// A 16-bit fixed-point value that indicates how loudly this track’s sound is to be played. A value
			/// of 1.0 indicates normal volume.
			/// </summary>
			Volume,
			/// <summary>
			/// A 16-bit integer that is reserved for use by Apple. Set this field to 0.
			/// </summary>
			Reserved3,
			/// <summary>
			/// The matrix structure associated with this track.
			/// All values in the matrix are 32-bit fixed-point numbers divided as 16.16,
			/// except for the {u, v, w} column, which contains 32-bit fixed-point numbers divided as 2.30.
			/// </summary>
			MatrixStructure,
			/// <summary>
			/// A 32-bit fixed-point number that specifies the width of this track in pixels.
			/// </summary>
			TrackWidth,
			/// <summary>
			/// A 32-bit fixed-point number that indicates the height of this track in pixels.
			/// </summary>
			TrackHeight
		}

		public TrackHeader(QtAtom previousHeader)
			: base(previousHeader, AtomName.TrackHeader)
		{
		}

		public uint TrackID { get; private set; }

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetDateTime(Attribute.CreationTime, "{0:F}");
			parser.GetDateTime(Attribute.ModificationTime, "{0:F}");
			TrackID = parser.GetUInt(Attribute.TrackID);
			parser.GetUInt(Attribute.Reserved1);
			parser.GetUInt(Attribute.Duration);
			parser.GetULong(Attribute.Reserved2);
			parser.GetShort(Attribute.Layer);
			parser.GetShort(Attribute.AlternateGroup);
			parser.GetFixed8_8(Attribute.Volume);
			parser.GetUShort(Attribute.Reserved3);
			parser.GetMatrix(Attribute.MatrixStructure);
			parser.GetFixed16_16(Attribute.TrackWidth);
			parser.GetFixed16_16(Attribute.TrackHeight);

			return this.Valid;
		}

		public override void ParseFlags(QtParser parser)
		{
			parser.GetFlags<QtAtom.Attribute, Flag>(QtAtom.Attribute.Flags, 3);
		}
	}
}
