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

using System;
using System.Globalization;
using System.Text;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class MediaObjectIndexObject : AsfObject
	{
		#region Inner Classes
		private class Entry : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				Offset
			}

			private readonly MediaObjectIndexObject _mediaObjectIndexObject;

			public Entry(MediaObjectIndexObject mediaObjectIndexObject)
				: base(Attribute.Entry, string.Empty, "{0}")
			{
				_mediaObjectIndexObject = mediaObjectIndexObject;
			}

			public override bool Parse(AsfParser parser)
			{
				short count = _mediaObjectIndexObject._specifiersCount;

				StringBuilder typedValue = new StringBuilder();
				typedValue.Append("(");
				for (int i = 0; i < count; i++)
				{
					int offset = parser.GetInt(LAttribute.Offset);

					if(i > 0)
					{
						typedValue.Append(", ");
					}
					typedValue.Append(string.Format(CultureInfo.CurrentCulture, "{0}", offset));
				}
				typedValue.Append(")");
				TypedValue = typedValue.ToString();

				return Valid;
			}

		}
		#endregion Inner Classes

		public new enum Attribute
		{
			IndexEntryCountInterval,
			IndexSpecifiersCount,
			IndexBlocksCount,
			IndexSpecifiers,
			IndexBlocks,
			StreamNumber,
			IndexType,
			IndexEntryCount,
			BlockPositions,
			IndexEntries,
			Entry,
			BlockTable,
			EntriesNotShown
		}

		private short _specifiersCount;

		public MediaObjectIndexObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.MediaObjectIndexObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetInt(Attribute.IndexEntryCountInterval);
			_specifiersCount = parser.GetShort(Attribute.IndexSpecifiersCount);
			int blocksCount = parser.GetInt(Attribute.IndexBlocksCount);

			for (int specifier = 0; specifier < _specifiersCount; specifier++)
			{
				parser.GetShort(Attribute.StreamNumber);
				parser.GetShort(Attribute.IndexType);
			}

			for (int block = 0; block < blocksCount; block++)
			{
				uint numberOfEntries = (uint)parser.GetInt(Attribute.IndexEntryCount);

				for (int specifierNumber = 0; specifierNumber < _specifiersCount; specifierNumber++)
				{
					parser.GetLong(Attribute.BlockPositions);
				}

				CreateEntryTable(parser, numberOfEntries);
			}

			return Valid;
		}

		private void CreateEntryTable(AsfParser parser, uint numberOfEntries)
		{
			const int EntrySize = 16;
			const int MaxEntriesOnDisplay = 1000;
			uint entriesShown = Math.Min(numberOfEntries, MaxEntriesOnDisplay);
			uint entriesNotShown = (numberOfEntries - entriesShown);

			for (uint entryIndex = 0; entryIndex < entriesShown; entryIndex++)
			{
				parser.Parse(new Entry(this));
			}
			if (entriesNotShown > 0)
			{
				parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.EntriesNotShown, entriesNotShown));
				parser.Position += Math.Min(entriesNotShown * EntrySize, parser.Length - parser.Position);
			}
		}
	}
}
