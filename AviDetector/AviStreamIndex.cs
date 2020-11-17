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

using System.Text;
using Defraser.Detector.Common;

namespace Defraser.Detector.Avi
{
	internal class AviStreamIndex : AviChunk
	{
		#region Inner class
		public class IndexTableAttribute : CompositeAttribute<Attribute, string, AviParser>
		{
			#region Inner inner class
			private class IndexTableEntryAttribute : CompositeAttribute<Attribute, string, AviParser>
			{
				enum LAttribute
				{
					IndexEntry
				}

				private readonly int _numberOfEntriesPerEntry;

				public IndexTableEntryAttribute(int numberOfEntriesPerEntry)
					: base(Attribute.Index, string.Empty, "{0}")
				{
					_numberOfEntriesPerEntry = numberOfEntriesPerEntry;
				}

				public override bool Parse(AviParser parser)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int entryIndex = 0; entryIndex < _numberOfEntriesPerEntry; entryIndex++)
					{
						// The long (or DWORD) from the AVI specifiction is represented by an 'int' in C#.
						stringBuilder.AppendFormat("{0} ", parser.GetInt(LAttribute.IndexEntry));
					}
					TypedValue = stringBuilder.ToString();
					return Valid;
				}
			}
			#endregion Inner inner class

			private readonly ushort _longsPerEntry;
			private readonly ulong _totalBytesToRead;

			public IndexTableAttribute(ushort longsPerEntry, ulong totalBytesToRead)
				: base(Attribute.IndexTable, string.Empty, "{0}")
			{
				_longsPerEntry = longsPerEntry;
				_totalBytesToRead = totalBytesToRead;
			}

			public override bool Parse(AviParser parser)
			{
				ulong numberOfEntriesInTable = _totalBytesToRead/(uint)(_longsPerEntry*4);

				uint maxTableEntryCount = (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.AviStreamIndexMaxTableEntryCount];

				if (numberOfEntriesInTable > maxTableEntryCount) return false;	// Sanity check

				for (uint totalEntryCountIndex = 0; totalEntryCountIndex < numberOfEntriesInTable; totalEntryCountIndex++)
				{
					parser.Parse(new IndexTableEntryAttribute(_longsPerEntry));
				}
				return Valid;
			}
		}
		#endregion Inner class

		public new enum Attribute
		{
			/// <summary>
			/// every aIndex[i] has a size of 4*wLongsPerEntry bytes. (the 
			/// structure of each aIndex[i] depends on the special type of index)
			/// </summary>
			LongsPerEntry,//short
			/// <summary>
			/// defines the type of the index
			/// </summary>
			IndexSubType,//byte
			/// <summary>
			/// defines the type of the index
			/// </summary>
			IndexType,//byte
			/// <summary>
			/// aIndex[0]..aIndex[nEntriesInUse-1] are valid
			/// </summary>
			EntriesInUse,//int
			/// <summary>
			/// ID of the stream the index points into, for example '00dc'.
			/// Consequently, one such index chunk can only point to data of
			/// one and the same stream.
			/// </summary>
			ChunkId,//int
			Reserved,//int[3]
			// index entry: array of longs per entry
			//struct _aviindex_entry {
			//DWORD adw[wLongsPerEntry];
			//} aIndex [ ];
			IndexTable,
			Index
		}

		// Test with file: '024123.avi'

		public uint Type { get; set; }

		public AviStreamIndex(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.AviStreamIndex)
		{
		}

		public override bool Parse(AviParser parser)
		{
			const short MaxLongsPerEntryCount = 16;	// Random choice. The only value I've seen up till now is 4.
			if (!base.Parse(parser)) return false;

			ushort longsPerEntry = (ushort)parser.GetShort(Attribute.LongsPerEntry);
			if (longsPerEntry > MaxLongsPerEntryCount) return false;
			parser.GetByte(Attribute.IndexSubType);
			parser.GetByte(Attribute.IndexType);
			parser.GetInt(Attribute.EntriesInUse);
			parser.GetInt(Attribute.ChunkId);
			parser.GetInt(Attribute.Reserved);
			parser.GetInt(Attribute.Reserved);
			parser.GetInt(Attribute.Reserved);

			if(longsPerEntry > 0)
			{
				ulong bytesRemaining = parser.BytesRemaining;
				parser.Parse(new IndexTableAttribute(longsPerEntry, bytesRemaining));
				ulong numberOfEntriesInTable = bytesRemaining / (uint)(longsPerEntry * 4);
				parser.CheckAttribute(Attribute.IndexTable, numberOfEntriesInTable * longsPerEntry * 4 == bytesRemaining, false);
			}

			return Valid;
		}
	}
}
