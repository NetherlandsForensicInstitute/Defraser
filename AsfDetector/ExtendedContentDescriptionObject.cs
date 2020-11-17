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
using System.Diagnostics;
using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	internal enum ValueType
	{
		String,
		Byte,
		Bool,
		DWord,
		QWord,
		Word
	}

	class ExtendedContentDescriptionObject : AsfObject
	{
		#region Inner Classes
		private class ContentDescription : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				DescriptorNameLength,
				DescriptorName,
				DescriptorValueDataType,
				DescriptorValueLength,
				DescriptorValue
			}

			public ContentDescription()
				: base(Attribute.ContentDescription, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				short nameLength = parser.GetShort(LAttribute.DescriptorNameLength);
				string name = parser.GetUnicodeString(LAttribute.DescriptorName, (nameLength / 2));
				ValueType descriptorValueType = (ValueType)parser.GetShort(LAttribute.DescriptorValueDataType, typeof(ValueType));
				short valueLenght = parser.GetShort(LAttribute.DescriptorValueLength);

				string descriptorValue = string.Empty;
				switch(descriptorValueType)
				{
					case ValueType.String: descriptorValue = parser.GetUnicodeString(LAttribute.DescriptorValue, (valueLenght / 2));
						break;
					case ValueType.Bool:
						bool value = parser.GetInt(Endianness.Little) == 0 ? false : true;
						Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.DescriptorValue, value));
						descriptorValue = value.ToString();
						break;
					case ValueType.Byte: parser.GetHexDump(LAttribute.DescriptorValue, valueLenght);
						break;
					case ValueType.Word: descriptorValue = parser.GetShort(LAttribute.DescriptorValue).ToString(CultureInfo.CurrentCulture);
						break;
					case ValueType.DWord: descriptorValue = parser.GetInt(LAttribute.DescriptorValue).ToString(CultureInfo.CurrentCulture);
						break;
					case ValueType.QWord: descriptorValue = parser.GetLong(LAttribute.DescriptorValue).ToString(CultureInfo.CurrentCulture);
						break;
					default:
						Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum ValueType not handled.", Enum.GetName(typeof(ValueType), descriptorValueType)));
						break;
				}

				if (string.IsNullOrEmpty(descriptorValue))
				{
					TypedValue = string.Format(CultureInfo.CurrentCulture, "({0}, {1})", name, descriptorValueType);
				}
				else
				{
					TypedValue = string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", name, descriptorValueType, descriptorValue);
				}

				return Valid;
			}
		}
		#endregion internal class

		public new enum Attribute
		{
			ContentDescriptorsCount,
			ContentDescription,
			ContentDescriptionTable
		}

		public ExtendedContentDescriptionObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.ExtendedContentDescriptionObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.ContentDescriptionTable, Attribute.ContentDescriptorsCount, NumberOfEntriesType.UShort, /*EntrySize: */ 0 /* 0 to indicate variable length */, () => new ContentDescription(), parser.BytesRemaining);

			return Valid;
		}
	}
}
