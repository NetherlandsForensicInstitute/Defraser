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
	internal enum MarkerObjectGuids
	{
		// No GuidAttribute
		Unknown,
		[Guid("4CFEDB20-75F6-11CF-9C0F-00A0C90349CB")]
		Reserved4
	}

	internal class MarkerObject : AsfObject
	{
		public new enum Attribute
		{
			Reserved,
			MarkersCount,
			NameLength,
			Name,
			Offset,
			PresentationTime,
			EntryLength,
			SendTime,
			Flags,
			MarkerDescriptionLength,
			MarkerDescription
		}

		private static readonly IDictionary<Guid, MarkerObjectGuids> NameForGuid = CreateNameForValueCollection<MarkerObjectGuids>();

		public MarkerObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.MarkerObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.Reserved, NameForGuid);
			int markersCount = parser.GetInt(Attribute.MarkersCount);
			parser.GetShort(Attribute.Reserved);
			short nameLength = parser.GetShort(Attribute.NameLength);
			parser.GetUnicodeString(Attribute.Name, nameLength/2);

			for (int marker = 0; marker < markersCount; marker++)
			{
				parser.GetLong(Attribute.Offset);
				parser.GetLongTime(Attribute.PresentationTime, TimeUnit.HundredNanoSeconds);
				parser.GetShort(Attribute.EntryLength);
				parser.GetTime(Attribute.SendTime);
				parser.GetInt(Attribute.Flags);
				int descriptionLength = parser.GetInt(Attribute.MarkerDescriptionLength);
				parser.GetUnicodeString(Attribute.MarkerDescription, descriptionLength);
			}
			return Valid;
		}
	}
}
