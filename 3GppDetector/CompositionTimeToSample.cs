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
	/// <summary>
	/// Provides the offset between decoding time and composition time.
	/// </summary>
	/// <remarks>
	/// Mandatory: no
	/// Quantity: zero or one
	/// </remarks>
	internal class CompositionTimeToSample : QtAtom
	{
		#region Inner classes
		private sealed class CompositionTimeToSampleTableEntry : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				SampleCount,
				Offset,
			}

			public CompositionTimeToSampleTableEntry()
				: base(Attribute.CompositionTimeToSampleTableEntry, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				int sampleCount = parser.GetInt(LAttribute.SampleCount);
				uint offset = parser.GetUInt(LAttribute.Offset);

				TypedValue = string.Format("({0}, {1})", sampleCount, offset);

				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			NumberOfEntries,
			CompositionTimeToSampleTable,
			CompositionTimeToSampleTableEntry,
		}

		public CompositionTimeToSample(QtAtom previousHeader)
			: base(previousHeader, AtomName.CompositionTimeToSample)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.CompositionTimeToSampleTable, Attribute.NumberOfEntries, NumberOfEntriesType.UInt, 8, () => new CompositionTimeToSampleTableEntry(), parser.BytesRemaining);

			return Valid;
		}
	}
}
