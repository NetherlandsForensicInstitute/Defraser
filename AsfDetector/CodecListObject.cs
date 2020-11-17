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
using System.Collections.Generic;
using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	internal enum CodecType
	{
		VideoCodec = 1,
		AudioCodec = 2,
		UnknownCodec = 0xFFFF,
	}

	internal enum CodecListObjectGuids
	{
		Unknown,
		[Guid("86D15241-311D-11D0-A3A4-00A0C90348F6")]
		Reserved2
	}

	class CodecListObject : AsfObject
	{
		#region Inner Classes
		private class Codec : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				Type,
				CodecNameLength,
				CodecName,
				CodecDescriptionLength,
				CodecDescription,
				CodecInformationLength,
				CodecInformation
			}

			private CodecListObject _outerClass;

			public Codec(CodecListObject codecList)
				: base(Attribute.Codec, string.Empty, "{0}")
			{
				_outerClass = codecList;
			}

			public override bool Parse(AsfParser parser)
			{
				CodecType codecType = (CodecType)parser.GetShort(LAttribute.Type, typeof(CodecType));
				short codecNameLength = parser.GetShort(LAttribute.CodecNameLength);
				string codecName = parser.GetUnicodeString(LAttribute.CodecName, codecNameLength);
				short codecDescriptionLength = parser.GetShort(LAttribute.CodecDescriptionLength);
				parser.GetUnicodeString(LAttribute.CodecDescription, codecDescriptionLength);
				short codecInformationLength = parser.GetShort(LAttribute.CodecInformationLength);
				parser.GetHexDump(LAttribute.CodecInformation, codecInformationLength);

				TypedValue = string.Format(CultureInfo.CurrentCulture, "({0}, {1})", codecType, codecName);

				_outerClass.CodecNames.Add(codecType, codecName);

				return Valid;
			}
		}
		#endregion internal class

		public new enum Attribute
		{
			Reserved,
			CodecEntriesCount,
			Codec,
			CodecTable
		}

		private static readonly IDictionary<Guid, CodecListObjectGuids> NameForGuid = CreateNameForValueCollection<CodecListObjectGuids>();

		public IDictionary<CodecType, string> CodecNames { get; private set; }

		public CodecListObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.CodecListObject)
		{
			CodecNames = new Dictionary<CodecType, string>();
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.Reserved, NameForGuid);

			parser.GetTable(Attribute.CodecTable, Attribute.CodecEntriesCount, NumberOfEntriesType.UInt, /*EntrySize: */ 0 /* 0 to indicate variable length */, () => new Codec(this), parser.BytesRemaining);
			return Valid;
		}
	}
}
