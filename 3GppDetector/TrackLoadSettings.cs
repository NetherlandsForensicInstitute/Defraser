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
	/// <summary>
	/// Track load settings atoms contain information that indicates how the track is to be used in its movie.
	/// Applications that read QuickTime files can use this information to process the movie data more
	/// efficiently. Track load settings atoms have an atom type value of 'load'.
	/// </summary>
	/// <remarks>Type = 'load'</remarks>
	internal class TrackLoadSettings : QtAtom
	{
		/// <summary>
		/// Flags governing the preload operation. These flags are mutually exclusive.
		/// </summary>
		[Flags]
		public enum PreloadFlags
		{
			/// <summary>Preload the track regardless of whether it is enabled.</summary>
			PreloadAlways = 0x00000001,
			/// <summary>Preload the track only if it is enabled.</summary>
			PreloadIfEnabled = 0x00000002
		}

		/// <summary>
		/// Playback hints. More than one flag may be enabled.
		/// </summary>
		[Flags]
		public enum DefaultHints
		{
			/// <summary>The track should be played using double-buffered I/O.</summary>
			DoubleBuffer = 0x00000020,
			/// <summary>The track should be displayed at highest possible quality, without regard to real-time performance considerations.</summary>
			HighQuality = 0x00000100
		}

		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer specifying the starting time, in the movie’s time coordinate system, of a segment
			/// of the track that is to be preloaded. Used in conjunction with the preload duration.
			/// </summary>
			/// <remarks>Type = 'load'</remarks>
			PreloadStartTime,
			/// <summary>
			/// A 32-bit integer specifying the duration, in the movie’s time coordinate system, of a segment
			/// of the track that is to be preloaded. If the duration is set to –1, it means that the preload segment
			/// extends from the preload start time to the end of the track. All media data in the segment of
			/// the track defined by the preload start time and preload duration values should be loaded into
			/// memory when the movie is to be played
			/// </summary>
			PreloadDuration,
			/// <summary>
			/// A 32-bit integer containing flags governing the preload operation.
			/// </summary>
			PreloadFlags,
			/// <summary>
			/// A 32-bit integer containing playback hints.
			/// </summary>
			DefaultHints,
		}

		public TrackLoadSettings(QtAtom previousHeader)
			: base(previousHeader, AtomName.TrackLoadSettings)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetUInt(Attribute.PreloadStartTime);
			parser.GetInt(Attribute.PreloadDuration);
			parser.GetFlags<Attribute, PreloadFlags>(Attribute.PreloadFlags, 4);
			parser.GetFlags<Attribute, DefaultHints>(Attribute.DefaultHints, 4);

			return this.Valid;
		}
	}
}
