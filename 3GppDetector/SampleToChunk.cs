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
	/// Maps samples to chunks in the media data stream. [stsc]
	/// </summary>
	internal class SampleToChunk : QtAtom
	{
		#region Inner classes
		public class SampleToChunkTableAttribute : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				/// <summary>
				/// The first chunk number using this table entry.
				/// </summary>
				FirstChunk,
				/// <summary>
				/// The number of samples in each chunk.
				/// </summary>
				SamplesPerChunk,
				/// <summary>
				/// The identification number associated with the sample description for the sample.
				/// </summary>
				SampleDescriptionID,
			}

			public SampleToChunkTableAttribute()
				: base(Attribute.SampleToChunk, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				uint firstChunk = parser.GetUInt(LAttribute.FirstChunk);
				uint samplesPerChunk = parser.GetUInt(LAttribute.SamplesPerChunk);
				uint sampleDescriptionID = parser.GetUInt(LAttribute.SampleDescriptionID);

				TypedValue = string.Format("({0}, {1}, {2})", firstChunk, samplesPerChunk, sampleDescriptionID);
				return Valid;
			}
		}

		/// <summary>
		/// Entry in the <c>SampleToChunkTable</c>.
		/// </summary>
		public class SampleToChunkTableEntry
		{
			#region Properties
			/// <summary>The first chunk number using this table entry.</summary>
			public uint FirstChunk { get; private set; }
			/// <summary>The number of samples in each chunk.</summary>
			public uint SamplesPerChunk { get; private set; }
			/// <summary>The identification number associated with the sample description for the sample.</summary>
			public uint SampleDescriptionID { get; private set; }
			#endregion Properties

			public SampleToChunkTableEntry(uint firstChunk, uint samplesPerChunk, uint sampleDescriptionID)
			{
				FirstChunk = firstChunk;
				SamplesPerChunk = samplesPerChunk;
				SampleDescriptionID = sampleDescriptionID;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			NumberOfEntries,
			SampleToChunkTable,
			SampleToChunk
		}

		public SampleToChunk(QtAtom previousHeader)
			: base(previousHeader, AtomName.SampleToChunk)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.SampleToChunkTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 12, () => new SampleToChunkTableAttribute(), parser.BytesRemaining);

			return Valid;
		}

		/// <summary>
		/// Create and return a table that maps samples to chunks.
		/// </summary>
		/// <param name="byteStreamDataReader">a byte stream reader to read the sample size table from.</param>
		/// <returns>the sample to chunk table</returns>
		internal IList<SampleToChunkTableEntry> GetSampleToChunkTable(ByteStreamDataReader byteStreamDataReader)
		{
			IList<SampleToChunkTableEntry> sampleToChunkTable = new List<SampleToChunkTableEntry>();
			if (!Valid) return sampleToChunkTable;

			// Set position to number of entries
			byteStreamDataReader.Position = Offset + 12;

			uint numberOfEntries = (uint) byteStreamDataReader.GetInt();

			for (int entryIndex = 0; entryIndex < numberOfEntries; entryIndex++)
			{
				uint firstChunk = (uint)byteStreamDataReader.GetInt();
				uint samplesPerChunk = (uint)byteStreamDataReader.GetInt();
				uint sampleDescriptionID = (uint)byteStreamDataReader.GetInt();

				SampleToChunkTableEntry sampleToChunkTableEntry = new SampleToChunkTableEntry(firstChunk, samplesPerChunk, sampleDescriptionID);
				sampleToChunkTable.Add(sampleToChunkTableEntry);
			}
			return sampleToChunkTable;
		}
	}
}
