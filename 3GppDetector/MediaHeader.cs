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
	/// The media header atom specifies the characteristics of a media, including time scale and duration.
	/// </summary>
	internal class MediaHeader : QtAtom
	{
		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer that specifies (in seconds since midnight, January 1, 1904) when the media
			/// atom was created
			/// </summary>
			CreationTime,
			/// <summary>
			/// A 32-bit integer that specifies (in seconds since midnight, January 1, 1904) when the media
			/// atom was changed.
			/// </summary>
			ModificationTime,
			/// <summary>
			/// A time value that indicates the time scale for this media—that is, the number of time units that
			/// pass per second in its time coordinate system.
			/// </summary>
			Timescale,
			/// <summary>
			/// The duration of this media in units of its time scale.
			/// </summary>
			Duration,
			/// <summary>
			/// A 16-bit integer that specifies the language code for this media.
			/// </summary>
			Language,
			/// <summary>
			/// A 16-bit integer that specifies the media’s playback quality—that is, its suitability for playback
			/// in a given environment.
			/// </summary>
			Quality,
		}

		public MediaHeader(QtAtom previousHeader)
			: base(previousHeader, AtomName.MediaHeader)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetDateTime(Attribute.CreationTime, "{0:F}");
			parser.GetDateTime(Attribute.ModificationTime, "{0:F}");
			parser.GetUInt(Attribute.Timescale);
			parser.GetUInt(Attribute.Duration);
			// TODO Language
			parser.GetUShort(Attribute.Language);	// codes < 0x800 Macintosh language codes
													// codes >= 0x800 ISO language codes
			parser.GetUShort(Attribute.Quality);

			return this.Valid;
		}
	}
}
