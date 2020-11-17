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

using System.Collections.Generic;
using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Indentifies the location of each chunk of data in the file.
	/// </summary>
	/// <remarks>Type = 'stco' | 'co64'</remarks>
	internal class ChunkOffset : QtAtom
	{
		public enum AtomType
		{
			ChunkOffset32Bit = 0x7374636F,	// 4CC = 'stco'
			ChunkOffset64Bit = 0x636F3634	// 4CC = 'co64'
		}

		#region Inner classes
		public class ChunkOffsetTableAttribute : CompositeAttribute<Attribute, string, QtParser>
		{
			private readonly AtomType _atomType;

			public ChunkOffsetTableAttribute(AtomType atomType)
				: base(Attribute.ChunkOffset, string.Empty, "{0}")
			{
				_atomType = atomType;
			}

			public override bool Parse(QtParser parser)
			{
				switch(_atomType)
				{
					case AtomType.ChunkOffset32Bit:
						TypedValue = string.Format("{0}", parser.GetUInt());
						break;
					case AtomType.ChunkOffset64Bit:
						TypedValue = string.Format("{0}", parser.GetULong());
						break;
				}
				return Valid;
			}
		}
		#endregion Inner classes

		private readonly AtomType _atomType;

		public new enum Attribute
		{
			NumberOfEntries,
			ChunkOffsetTable,
			ChunkOffset
		}

		public ChunkOffset(QtAtom previousHeader, AtomType atomType)
			: base(previousHeader, AtomName.ChunkOffset)
		{
			_atomType = atomType;
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			switch(_atomType)
			{
				case AtomType.ChunkOffset32Bit:
					parser.GetTable(Attribute.ChunkOffsetTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 4, () => new ChunkOffsetTableAttribute(AtomType.ChunkOffset32Bit), parser.BytesRemaining);
					break;
				case AtomType.ChunkOffset64Bit:
					parser.GetTable(Attribute.ChunkOffsetTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 8, () => new ChunkOffsetTableAttribute(AtomType.ChunkOffset64Bit), parser.BytesRemaining);
					break;
			}
			return Valid;
		}

		internal IList<long> GetOffsetTable(ByteStreamDataReader byteStreamDataReader)
		{
			List<long> offsetTable = new List<long>();
			if (!Valid) return offsetTable;

			// Set position to number of entries
			byteStreamDataReader.Position = Offset + 12;

			uint numberOfEntries = (uint) byteStreamDataReader.GetInt();

			for (int entryIndex = 0; entryIndex < numberOfEntries; entryIndex++)
			{
				switch(_atomType)
				{
					case AtomType.ChunkOffset32Bit:
						offsetTable.Add((uint)byteStreamDataReader.GetInt());
						break;
					case AtomType.ChunkOffset64Bit:
						offsetTable.Add(byteStreamDataReader.GetLong());
						break;
				}
			}
			return offsetTable;
		}
	}
}
