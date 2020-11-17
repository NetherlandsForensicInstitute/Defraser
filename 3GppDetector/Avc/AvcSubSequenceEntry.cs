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
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	class AvcSubSequenceEntry : VisualSampleGroupEntry
	{
		#region Inner class
		private class DependencyInfo : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				SubSeqDirectionFlag,	// uint8
				LayerNumber,			// uint8
				SubSequenceIdentifier,	// uint16
			}

			public DependencyInfo()
				: base(Attribute.DependencyInfoEntry, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				byte subSeqDirectionFlag = parser.GetByte(LAttribute.SubSeqDirectionFlag);
				byte layerNumber = parser.GetByte(LAttribute.LayerNumber);
				ushort subSequenceIdentifier = parser.GetUShort(LAttribute.SubSequenceIdentifier);

				TypedValue = string.Format("({0}, {1}, {2})", subSeqDirectionFlag, layerNumber, subSequenceIdentifier);

				return Valid;
			}
		}
		#endregion Inner class

		public new enum Attribute
		{
			SubSequenceIdentifer,	// uint16
			LayerNumber,			// uint8
			DurationFlag,			// 1 bit
			AvgRateFlag,			// 1 bit
			Reserved = 0,			// 6 bits (0)
			Duration,				// uint32
			AccurateStatisticsFlag,	// uint8
			AvgBitRate,				// uint16
			AvgFrameRate,			// uint16
			NumReferences,			// uint8
			DependencyInfo,
			DependencyInfoEntry
		}

		public AvcSubSequenceEntry(QtAtom previousHeader)
			: base(previousHeader, AtomName.AvcSubSequenceEntry)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if(!base.Parse(parser)) return false;

			parser.GetUShort(Attribute.SubSequenceIdentifer);
			parser.GetByte(Attribute.LayerNumber);
			byte flags = parser.GetByte();
			bool durationFlag = (flags & 0x80) == 0x80;
			bool avgRateFlag = (flags & 0x40) == 0x40;
			byte reserved = (byte)(flags & 0x3F);

			parser.AddAttribute(new FormattedAttribute<Attribute, bool>(Attribute.DurationFlag, durationFlag));
			parser.AddAttribute(new FormattedAttribute<Attribute, bool>(Attribute.AvgRateFlag, avgRateFlag));
			parser.AddAttribute(new FormattedAttribute<Attribute, byte>(Attribute.Reserved, reserved));
			parser.CheckAttribute(Attribute.Reserved, reserved == 0, false);

			if(durationFlag)
			{
				parser.GetUInt(Attribute.Duration);
			}

			if(avgRateFlag)
			{
				parser.GetByte(Attribute.AccurateStatisticsFlag);
				parser.GetUShort(Attribute.AvgBitRate);
				parser.GetUShort(Attribute.AvgFrameRate);
			}

			parser.GetTable(Attribute.DependencyInfo, Attribute.NumReferences, NumberOfEntriesType.Byte, 32, () => new DependencyInfo(), parser.BytesRemaining);

			return Valid;
		}
	}
}
