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

namespace QtDetector
{
	/// <summary>
	/// Track reference atoms define relationships between tracks.
	/// Type of the Track Reference Atom Container: 'tref'
	/// Possible child types: tmcd, chap, sync, scpt, ssrc and hint.
	/// </summary>
	internal class TrackReferenceType : QtLeafAtom
	{
		#region Inner classes
		private class TrackIDTable : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute { TrackID }

			private long _numberOfEntries;

			public TrackIDTable(long numberOfEntries)
				: base(Attribute.TrackIDTable, string.Empty, "{0}")
			{
				_numberOfEntries = numberOfEntries;
			}

			public override bool Parse(QtParser parser)
			{
				for (int entryIndex = 0; entryIndex < _numberOfEntries; entryIndex++)
				{
					parser.GetInt(LAttribute.TrackID);
				}
				return this.Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			TrackIDTable
		}

		public TrackReferenceType(QtAtom previousHeader, AtomName atomName)
			: base(previousHeader, atomName)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			const int EntrySize = 4;

			long numberOfEntries = parser.BytesRemaining / EntrySize;
			parser.Parse(new TrackIDTable(numberOfEntries));

			return this.Valid;
		}
	}

	/// <summary>
	/// Time code. Usually references a time code track.
	/// </summary>
	/// <remarks>Type = 'tmcd'</remarks>
	internal class TimeCode : TrackReferenceType
	{
		public TimeCode(QtAtom previousHeader)
			: base(previousHeader, AtomName.TimeCode)
		{
		}
	}

	/// <summary>
	/// Chapter or scene list. Usually references a text track.
	/// </summary>
	/// <remarks>Type = 'chap'</remarks>
	internal class ChapterList : TrackReferenceType
	{
		public ChapterList(QtAtom previousHeader)
			: base(previousHeader, AtomName.ChapterList)
		{
		}
	}

	/// <summary>
	/// Synchronization. Usually between a video and sound track. Indicates that the two
	/// tracks are synchronized. The reference can be from either track to the other, or there
	/// may be two references.
	/// </summary>
	/// <remarks>Type = 'sync'</remarks>
	internal class Synchronization : TrackReferenceType
	{
		public Synchronization(QtAtom previousHeader)
			: base(previousHeader, AtomName.Synchronization)
		{
		}
	}

	/// <summary>
	/// Transcript. Usually references a text track.
	/// </summary>
	/// <remarks>Type = 'scpt'</remarks>
	internal class Transcript : TrackReferenceType
	{
		public Transcript(QtAtom previousHeader)
			: base(previousHeader, AtomName.Transcript)
		{
		}
	}

	/// <summary>
	/// Nonprimary source. Indicates that the referenced track should send its data to this
	/// track, rather than presenting it. The referencing track will use the data to modify
	/// how it presents its data.
	/// </summary>
	/// <remarks>Type = 'ssrc'</remarks>
	internal class NonprimarySource : TrackReferenceType
	{
		public NonprimarySource(QtAtom previousHeader)
			: base(previousHeader, AtomName.NonprimarySource)
		{
		}
	}

	/// <summary>
	/// The referenced tracks contain the original media for this hint track.
	/// </summary>
	/// <remarks>Type = 'ssrc'</remarks>
	internal class Hint : TrackReferenceType
	{
		public Hint(QtAtom previousHeader)
			: base(previousHeader, AtomName.Hint)
		{
		}
	}
}
