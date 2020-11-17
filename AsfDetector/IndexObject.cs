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
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class IndexObject :AsfObject
	{
		#region Inner Classes
		private class Entry : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				Offset
			}

			private readonly IndexObject _indexObject;
			public Entry(IndexObject indexObject)
				: base(Attribute.Entry, string.Empty, "{0}")
			{
				_indexObject = indexObject;
			}

			public override bool Parse(AsfParser parser)
			{
				short count = _indexObject.SpecifiersCount;
				int offset1;
				int offset2;
				int offset3;

				if (count >= 1)
				{
					offset1 = parser.GetInt(LAttribute.Offset);
					TypedValue = string.Format(CultureInfo.CurrentCulture, "({0})", offset1);
				}
				if (count >= 2)
				{
					offset2 = parser.GetInt(LAttribute.Offset);
					TypedValue += string.Format(CultureInfo.CurrentCulture, "(, {0})", offset2);
				}
				if (count >= 3)
				{
					offset3 = parser.GetInt(LAttribute.Offset);
					TypedValue += string.Format(CultureInfo.CurrentCulture, "(, {0})", offset3);
				}

				return Valid;
			}

		}
		#endregion Inner Classes

		public new enum Attribute
		{
			IndexEntryTimeInterval,
			IndexSpecifiersCount,
			IndexBlocksCount,
			IndexSpecifiers,
			IndexBlocks,
			StreamNumber,
			IndexType,
			IndexEntryCount,
			BlockPositions,
			IndexEntries,
			BlockTable,
			Entry,
			EntriesNotShown
		}
		
		private short SpecifiersCount{get; set;}

		public IndexObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.IndexObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetInt(Attribute.IndexEntryTimeInterval);
			SpecifiersCount = parser.GetShort(Attribute.IndexSpecifiersCount);
			int blocksCount = parser.GetInt(Attribute.IndexBlocksCount);

			for (int specifier = 0; specifier < SpecifiersCount; specifier++)
			{
				parser.GetShort(Attribute.StreamNumber);
				parser.GetShort(Attribute.IndexType);
			}

			for (int block = 0; block < blocksCount; block++)
			{
				uint numberOfEntries = (uint) parser.GetInt(Attribute.IndexEntryCount);

				for (int specifierNumber = 0; specifierNumber < SpecifiersCount; specifierNumber++)
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
