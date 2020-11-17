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
	class GroupMutualExclusionObject :AsfObject
	{
		public new enum Attribute
		{
			/// <summary>Specifies the nature of the mutual exclusion relationship. Use one of the GUIDs defined in section 10.11.</summary>
			ExclusionType,
			/// <summary>Specifies the number of entries in the Records list. This value should be at least 2.</summary>
			RecordCount,
			/// <summary>Specifies the number of streams in this record. Must be at least 1.</summary>
			StreamCount,
			/// <summary>Specifies the stream numbers for this record. Valid values are between 1 and 127.</summary>
			StreamNumbers
		}

		private static readonly IDictionary<Guid, MutualExclusionObjectExclusionTypeGuids> NameForGuid = CreateNameForValueCollection<MutualExclusionObjectExclusionTypeGuids>();

		public GroupMutualExclusionObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.GroupMutualExclusionObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.ExclusionType, NameForGuid);
			short recordsCount = parser.GetShort(Attribute.RecordCount);

			for(int record =0; record<recordsCount; record++)
			{
				short streamCount = parser.GetShort(Attribute.StreamCount);
				
				for(int stream = 0; stream<streamCount; stream++)
				{
					parser.GetShort(Attribute.StreamNumbers);
				}
			}

			return Valid;
		}
	}
}
