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

using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	internal class EditList : QtAtom
	{
		#region Inner classes
		private class EditListTableEntry : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				/// <summary>
				/// A 32-bit integer that specifies the duration of this edit segment in units of the movie’s time
				/// scale.
				/// </summary>
				TrackDuration,
				/// <summary>
				/// A 32-bit integer containing the starting time within the media of this edit segment (in media
				/// timescale units). If this field is set to –1, it is an empty edit. The last edit in a track should never
				/// be an empty edit. Any difference between the movie’s duration and the track’s duration is
				/// expressed as an implicit empty edit.
				/// </summary>
				MediaTime,
				/// <summary>
				/// A 32-bit fixed-point number that specifies the relative rate at which to play the media
				/// corresponding to this edit segment. This rate value cannot be 0 or negative.
				/// </summary>
				MediaRate,
			}

			public EditListTableEntry()
				: base(Attribute.EditListTableEntry, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				int trackDuration = parser.GetInt(LAttribute.TrackDuration);
				int mediaTime = parser.GetInt(LAttribute.MediaTime);
				double mediaRate = parser.GetFixed16_16(LAttribute.MediaRate);
				parser.CheckAttribute(LAttribute.MediaRate, mediaRate > 0.0);

				TypedValue = string.Format("({0}, {1}, {2})", trackDuration, mediaTime, mediaRate);

				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			NumberOfEntries,
			EditListTable,
			EditListTableEntry,
		}

		public EditList(QtAtom previousHeader)
			: base(previousHeader, AtomName.EditList)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.EditListTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 12, () => new EditListTableEntry(), parser.BytesRemaining);

			// TODO add check: The last edit in a track should never be an empty edit
			// Empty edits have the media time set to -1

			return Valid;
		}
	}
}
