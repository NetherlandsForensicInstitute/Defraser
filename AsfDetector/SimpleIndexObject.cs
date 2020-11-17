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

using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class SimpleIndexObject : AsfObject
	{
		#region Inner Classes
		private class Entry : CompositeAttribute<Attribute, string, AsfParser>
		{
			public enum LAttribute
			{
				PacketNumber,
				PacketCount
			}

			public Entry()
				: base(Attribute.Entry, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				int packetNumber = parser.GetInt(LAttribute.PacketNumber);
				short packetCount = parser.GetShort(LAttribute.PacketCount);

				TypedValue = string.Format(CultureInfo.CurrentCulture, "({0}, {1})", packetNumber, packetCount);

				return Valid;
			}
		}
		#endregion Inner Classes

		public new enum Attribute
		{
			FileID,
			IndexEntryTimeInterval,
			MaximumPacketCount,
			IndexEntriesCount,
			IndexEntries,
			Entry,
			EntryTable
		}

		public SimpleIndexObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.SimpleIndexObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.FileID);
			parser.GetLong(Attribute.IndexEntryTimeInterval);
			parser.GetInt(Attribute.MaximumPacketCount);

			parser.GetTable(Attribute.EntryTable, Attribute.IndexEntriesCount, NumberOfEntriesType.UInt, 6, () => new Entry(), parser.BytesRemaining);

			return Valid;
		}
	}
}
