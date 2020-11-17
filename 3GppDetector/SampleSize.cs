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
	/// Specifies the size of each sample in the media. [stsz]
	/// </summary>
	internal class SampleSize : QtAtom
	{
		#region Inner classes
		public class SampleSizeTableAttribute : CompositeAttribute<Attribute, string, QtParser>
		{
			public SampleSizeTableAttribute()
				: base(Attribute.SampleSize, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				TypedValue = string.Format("{0}", parser.GetUInt());
				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			NumberOfEntries,
			SampleSizeTable,
			/// <summary>
			/// A 32-bit integer specifying the sample size. If all the
			/// samples are the same size, this field contains that size
			/// value. If this field is set to 0, then the samples have
			/// different sizes, and those sizes are stored in the sample
			/// size table.
			/// </summary>
			SampleSize,
			/// <summary>
			/// A 32-bit integer that gives the number of samples in the track;
			/// if sample-size is 0, then it is also the number of entries in
			/// the sample size table.
			/// </summary>
			SampleCount
		}

		public SampleSize(QtAtom previousHeader)
			: base(previousHeader, AtomName.SampleSize)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			uint sampleSize = parser.GetUInt(Attribute.SampleSize);
			if (sampleSize == 0)	// If the sample size is 0 than only read the number of samples in the track
			{
				parser.GetTable(Attribute.SampleSizeTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 4, () => new SampleSizeTableAttribute(), parser.BytesRemaining);
			}
			else
			{
				parser.GetUInt(Attribute.SampleCount);	// Read the number on samples in the track
			}

			return Valid;
		}

		/// <summary>
		/// A 32-bit integer specifying the sample size. If all the
		/// samples are the same size, this field contains that size
		/// value. If this field is set to 0, then the samples have
		/// different sizes, and those sizes are stored in the sample
		/// size table. Get this table using method
		/// <code>GetSampleToChunkTable()</code>.
		/// </summary>
		/// <param name="byteStreamDataReader">a byte stream reader to read the sample size table from.</param>
		/// <returns>The sample size value.</returns>
		internal uint GetSampleSizeValue(ByteStreamDataReader byteStreamDataReader)
		{
			byteStreamDataReader.Position = Offset + 12;	// Set position to sample size value
			return (uint)byteStreamDataReader.GetInt();
		}

		/// <summary>
		/// A table containing the sample size information. The sample size
		/// table contains an entry for every sample in the media’s data
		/// stream. Each table entry contains a size field. The size field
		/// contains the size, in bytes, of the sample in question. The table
		/// is indexed by sample number—the first entry corresponds to the
		/// first sample, the second entry is for the second sample, and so on.
		/// </summary>
		/// <param name="byteStreamDataReader">a byte stream reader to read the sample size table from.</param>
		/// <returns>The sample size table.</returns>
		internal IList<uint> GetSampleSizeTable(ByteStreamDataReader byteStreamDataReader)
		{
			List<uint> sampleSizeTable = new List<uint>();
			if (!Valid) return sampleSizeTable;

			if (GetSampleSizeValue(byteStreamDataReader) == 0)
			{
				// Set position to number of entries
				byteStreamDataReader.Position = Offset + 16;

				uint numberOfEntries = (uint) byteStreamDataReader.GetInt();

				for (int entryIndex = 0; entryIndex < numberOfEntries; entryIndex++)
				{
					sampleSizeTable.Add((uint) byteStreamDataReader.GetInt());
				}
			}
			return sampleSizeTable;
		}
	}
}
