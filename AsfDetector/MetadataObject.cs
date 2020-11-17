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
	internal enum DataType
	{
		String,
		Byte,
		Bool,
		DWord,
		QWord,
		Word
	}

	class MetadataObject  : AsfObject
	{
		#region Inner Classes
		private class DescriptionRecord : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				DescriptionRecords,
				Reserved,
				NameLength,
				DataType,
				DataLength,
				StreamNumber,
				Name,
				Data
			}

			public DescriptionRecord()
				: base(Attribute.DescriptionRecords, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				parser.GetShort(LAttribute.Reserved);
				short streamNumber = parser.GetShort(LAttribute.StreamNumber);
				short nameLength = parser.GetShort(LAttribute.NameLength);
				DataType dataType = (DataType)parser.GetShort(LAttribute.DataType, typeof(DataType));
				int dataLength = parser.GetInt(LAttribute.DataLength);
				string name = parser.GetUnicodeString(LAttribute.Name, (nameLength / 2));
			
				switch(dataType)
				{
					case DataType.String: parser.GetUnicodeString(LAttribute.Data,(dataLength/2));
						break;
					case DataType.Byte: parser.GetHexDump(LAttribute.Data, dataLength);
						break;
					case DataType.Bool: int value = parser.GetShort();
										bool temp = false;
										if (value == 1) temp = true;
										Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.Data, temp));
										break;
					case DataType.DWord: parser.GetInt(LAttribute.Data);
						break;
					case DataType.QWord: parser.GetLong(LAttribute.Data);
						break;
					case DataType.Word: parser.GetShort(LAttribute.Data);
						break;
					default:
						Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum DataType not handled.", Enum.GetName(typeof(DataType), dataType)));
						break;
				}

				TypedValue = string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", streamNumber, name, dataType);
				return Valid;
			}
		}
		#endregion Inner Classes

		public new enum Attribute
		{
			DescriptionRecordsCount,
			DescriptionRecordsTable,
			DescriptionRecords
		}
		
		public MetadataObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.MetadataObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.DescriptionRecordsTable, Attribute.DescriptionRecordsCount, NumberOfEntriesType.UShort, /*EntrySize: */ 0 /* 0 to indicate variable length */, () => new DescriptionRecord(), parser.BytesRemaining);

			return Valid;
		}
	}
}
