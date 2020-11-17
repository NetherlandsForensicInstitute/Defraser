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

namespace Defraser.Detector.Asf
{
	class MetadataLibraryObject	:AsfObject
	{
		public new enum Attribute
		{
			DescriptionRecordsCount,
			DescriptionRecords,
			LanguageListIndex,
			StreamNumber,
			NameLength,
			DataType,
			DataLength,
			Name,
			Data,
		}

		public MetadataLibraryObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.MetadataLibraryObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			short recordsCount = parser.GetShort(Attribute.DescriptionRecordsCount);

			for (int record = 0; record < recordsCount; record++)
			{
				parser.GetShort(Attribute.LanguageListIndex);
				parser.GetShort(Attribute.StreamNumber);
				short nameLength = parser.GetShort(Attribute.NameLength);
				short dataType = parser.GetShort(Attribute.DataType);
				int dataLength = parser.GetInt(Attribute.DataLength);
				parser.GetUnicodeString(Attribute.Name, (nameLength / 2));

				switch (dataType)
				{
					case 0: parser.GetUnicodeString(Attribute.Data, dataLength);
						break;
					case 1: parser.GetHexDump(Attribute.Data, dataLength);
						break;
					case 2: parser.GetShort(Attribute.Data);
						break;
					case 3: parser.GetInt(Attribute.Data);
						break;
					case 4: parser.GetLong(Attribute.Data);
						break;
					case 5: parser.GetShort(Attribute.Data);
						break;
					case 6: parser.GetGuid(Attribute.Data);
						break;
					default:
						Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum DataType not handled.", Enum.GetName(typeof(DataType), dataType)));
						break;
				}
			}

			return Valid;
		}
	}
}
