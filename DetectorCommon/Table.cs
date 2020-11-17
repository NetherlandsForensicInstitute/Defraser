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

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Lists the entries in a table.
	/// This parses and creates composite attributes for the table entries.
	/// </summary>
	/// <typeparam name="TEnum">the attribute name enumeration type</typeparam>
	/// <typeparam name="THeader">the header type</typeparam>
	/// <typeparam name="THeaderName">the header name type</typeparam>
	/// <typeparam name="TParser">the parser type</typeparam>
	public class Table<TEnum, THeader, THeaderName, TParser> : CompositeAttribute<TEnum, string, TParser>
		where THeader : Header<THeader, THeaderName, TParser>
		where TParser : Parser<THeader, THeaderName, TParser>
	{
		/// <summary>
		/// Creates a new table entry.
		/// </summary>
		/// <typeparam name="TEnum">the attribute enumeration type for the table entry</typeparam>
		/// <returns>the table entry</returns>
		public delegate CompositeAttribute<TEnum, string, TParser> CreateTableEntryDelegate();

		/// <summary>
		/// The maximum number of entries on display.
		/// </summary>
		internal const int MaxEntriesOnDisplay = 1000;

		public enum LAttribute { EntriesNotShown }

		private readonly uint _numberOfEntries;
		private readonly uint _entrySize;
		private readonly CreateTableEntryDelegate _createTableEntry;

		/// <summary>
		/// Creates a new table with the default maximum number of entries on display.
		/// </summary>
		/// <param name="tableName">the attribute name for the table</param>
		/// <param name="numberOfEntries">the number of entries in the table</param>
		/// <param name="entrySize">the size of the entries in bytes</param>
		/// <param name="createTableEntry">the delegate for creating table entries</param>
		public Table(TEnum tableName, uint numberOfEntries, uint entrySize, CreateTableEntryDelegate createTableEntry)
			: base(tableName, string.Empty, "{0}")
		{
			_numberOfEntries = numberOfEntries;
			_entrySize = entrySize;
			_createTableEntry = createTableEntry;
		}

		public override bool Parse(TParser parser)
		{
			uint entriesShown = Math.Min(_numberOfEntries, MaxEntriesOnDisplay);
			uint entriesNotShown = (_numberOfEntries - entriesShown);

			for (uint entryIndex = 0; entryIndex < entriesShown; entryIndex++)
			{
				parser.Parse(_createTableEntry());
			}
			if (entriesNotShown > 0)
			{
				parser.AddAttribute(new FormattedAttribute<LAttribute, uint>(LAttribute.EntriesNotShown, entriesNotShown));
				parser.Position += Math.Min(entriesNotShown * _entrySize, (parser.Length - parser.Position));
			}
			return Valid;
		}
	}
}
