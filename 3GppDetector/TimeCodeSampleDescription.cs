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
using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	internal class TimeCodeSampleDescription : QtAtom
	{
		private static readonly uint Tcmi = "tcmi".To4CC();

		#region Inner classes
		private sealed class Flags : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				DropFrame,
				TwentyFourHourMax,
				NegativeTimesOK,
				Counter,
			}

			public Flags()
				: base(Attribute.Flags, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				uint flags = parser.GetUInt();

				Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.DropFrame, ((flags & 0x00000001) == 0) ? false : true));
				Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.TwentyFourHourMax, ((flags & 0x00000002) == 0) ? false : true));
				Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.NegativeTimesOK, ((flags & 0x00000004) == 0) ? false : true));
				Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.Counter, ((flags & 0x00000008) == 0) ? false : true));

				this.TypedValue = string.Format("{0}", flags);

				return this.Valid;
			}
		}

		private sealed class Color : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				Red,
				Green,
				Blue,
			}

			public Color(Attribute attribute)
				: base(attribute, string.Empty)
			{
			}

			public override bool Parse(QtParser parser)
			{
				ushort red = parser.GetUShort(LAttribute.Red);
				ushort green = parser.GetUShort(LAttribute.Green);
				ushort blue = parser.GetUShort(LAttribute.Blue);

				TypedValue = string.Format("{0},{1},{2}", red, green, blue);

				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer that is reserved for future use. Set this field to 0.
			/// </summary>
			Reserved1,
			/// <summary>
			/// A 32-bit integer containing flags that identify some timecode characteristics. The following
			/// flags are defined.
			/// Drop frame
			/// Indicates whether the timecode is drop frame. Set it to 1 if the timecode is drop frame. This
			/// flag’s value is 0x0001.
			/// 24 hour max
			/// Indicates whether the timecode wraps after 24 hours. Set it to 1 if the timecode wraps. This
			/// flag’s value is 0x0002.
			/// Negative times OK
			/// Indicates whether negative time values are allowed. Set it to 1 if the timecode supports negative
			/// values. This flag’s value is 0x0004.
			/// Counter
			/// Indicates whether the time value corresponds to a tape counter value. Set it to 1 if the timecode
			/// values are tape counter values. This flag’s value is 0x0008.
			/// </summary>
			Flags,
			/// <summary>
			/// A 32-bit integer that specifies the time scale for interpreting the frame duration field.
			/// </summary>
			TimeScale,
			/// <summary>
			/// A 32-bit integer that indicates how long each frame lasts in real time.
			/// </summary>
			FrameDuration,
			/// <summary>
			/// An 8-bit integer that contains the number of frames per second for the timecode format. If the
			/// time is a counter, this is the number of frames for each counter tick.
			/// </summary>
			NumberOfFrames,
			/// <summary>
			/// A 24-bit quantity that must be set to 0.
			/// </summary>
			Reserved2,
			Reserved3,
			Reserved4,
			/// <summary>
			/// A user data atom containing information about the source tape. The only currently used user
			/// data list entry is the 'name' type. This entry contains a text item specifying the name of the
			/// source tape.
			/// </summary>
			SourceReference,

			// 'tcmi' atom fields
			TextFont,
			TextFace,
			TextSize,
			TcmiReserved,
			TextColor,
			BackgroundColor,
			FontName
		}

		public TimeCodeSampleDescription(QtAtom previousHeader)
			: base(previousHeader, AtomName.TimeCodeSampleDescription)
		{
		}

		private bool CheckComponentSubType(ComponentSubType componentSubType)
		{
			QtAtom media = FindParent(AtomName.Media);
			if (media != null)
			{
				HandlerReference handlerReference = media.FindChild(AtomName.HandlerReference) as HandlerReference;
				if (handlerReference != null && handlerReference.ComponentSubType != componentSubType)
				{
					return false;
				}
			}
			return true;
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (!CheckComponentSubType(ComponentSubType.tmcd))
			{
				Valid = false;
				return Valid;
			}

			uint reserved = parser.GetUInt();
			if (reserved == 0)
			{
				parser.AddAttribute(new FormattedAttribute<QtSampleDescriptionAtom.Attribute, int>(QtSampleDescriptionAtom.Attribute.Reserved, (int)reserved));
				parser.GetShort(QtSampleDescriptionAtom.Attribute.Reserved);
				parser.GetShort(QtSampleDescriptionAtom.Attribute.DataReferenceIndex);
				reserved = parser.GetUInt(Attribute.Reserved1);
				parser.CheckAttribute(Attribute.Reserved1, reserved == 0, true);
				parser.Parse(new Flags());
				parser.GetInt(Attribute.TimeScale);
				parser.GetInt(Attribute.FrameDuration);
				parser.GetByte(Attribute.NumberOfFrames);
				reserved = parser.GetThreeBytes(Attribute.Reserved2);
				parser.CheckAttribute(Attribute.Reserved2, reserved == 0, false);

				if (parser.BytesRemaining > 0)
				{
					parser.GetHexDump(QtAtom.Attribute.AdditionalData, (int)parser.BytesRemaining);
				}
			}
			else
			{
				ParseTcmiAtom(parser, reserved);
			}
			return Valid;
		}

		// See QuickTime File Format Specification, 2010-08-03, pg.1
		private static void ParseTcmiAtom(QtParser parser, uint size)
		{
			long tcmiPosition = (parser.Position - 4);
			parser.AddAttribute(new FormattedAttribute<QtAtom.Attribute, uint>(QtAtom.Attribute.Size, size));
			uint type = parser.GetFourCC(QtAtom.Attribute.Type);
			parser.CheckAttribute(QtAtom.Attribute.Type, type == Tcmi, false);
			parser.GetByte(QtAtom.Attribute.Version);
			parser.GetThreeBytes(QtAtom.Attribute.Flags);
			parser.GetUShort(Attribute.TextFont);
			parser.GetUShort(Attribute.TextFace);
			parser.GetUShort(Attribute.TextSize);
			parser.GetUShort(Attribute.TcmiReserved);
			parser.Parse(new Color(Attribute.TextColor));
			parser.Parse(new Color(Attribute.BackgroundColor));
			parser.GetPascalString(Attribute.FontName, 0);
			parser.CheckAttribute(QtAtom.Attribute.Size, size == (parser.Position - tcmiPosition), false);
		}
	}
}
