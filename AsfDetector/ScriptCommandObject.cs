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

namespace Defraser.Detector.Asf
{
	internal enum ScriptCommandObjectGuids
	{
		// No GuidAttribute
		Unknown,
		[Guid("4B1ACBE3-100B-11D0-A39B-00A0C90348F6")]
		Reserved3
	}

	internal class ScriptCommandObject : AsfObject
	{
		public new enum Attribute
		{
			Reserved,
			CommandsCount,
			CommandTypesCount,
			CommandTypes,
			CommandTypeNameLength,
			CommandTypeName,
			Commands,
			PresentationTime,
			TypeIndex,
			CommandNameLength,
			CommandName,
		}

		private static readonly IDictionary<Guid, ScriptCommandObjectGuids> NameForGuid = CreateNameForValueCollection<ScriptCommandObjectGuids>();

		public ScriptCommandObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.ScriptCommandObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.Reserved, NameForGuid);
			short commandsCount = parser.GetShort(Attribute.CommandsCount);
			short typesCount = parser.GetShort(Attribute.CommandTypesCount);

			for (int type = 0; type < typesCount; type++)
			{
				short typeNameLength = parser.GetShort(Attribute.CommandTypeNameLength);
				parser.GetUnicodeString(Attribute.CommandTypeName, typeNameLength);
			}

			for (int command = 0; command < commandsCount; command++)
			{
				parser.GetTime(Attribute.PresentationTime);
				parser.GetShort(Attribute.TypeIndex);
				short commandNameLenght = parser.GetShort(Attribute.CommandNameLength);
				parser.GetUnicodeString(Attribute.CommandName, commandNameLenght);
			}

			return Valid;
		}
	}
}
