﻿/*
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

using System.Collections.Generic;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	internal class DataObject : AsfObject, IDataObject
	{
		public new enum Attribute
		{
			FileID,
			TotalDataPackets,
			Reserved,
			DataPacket,
			DataPacketsTable,
			Payload,
			EntriesNotShown
		}

		private readonly List<DataPacket> _dataPackets;

		public IEnumerable<DataPacket> DataPackets { get { return _dataPackets; } }

		public DataObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.DataObject)
		{
			_dataPackets = new List<DataPacket>();
		}

		public override bool Parse(AsfParser parser)
		{
			// Call the Parse method of the parent to initialise the object and
			// allow parsing the containing object if there is one.
			if (!base.Parse(parser)) return false;

			parser.GetGuid(Attribute.FileID);
			long totalDataPackets = parser.GetLong(Attribute.TotalDataPackets);
			parser.GetShort(Attribute.Reserved);

			if (!Valid) return false;

			CreateDataPacketTable(parser, totalDataPackets);

			return true;
		}

		private void CreateDataPacketTable(AsfParser parser, long dataPacketCount)
		{
			try
			{
				for (uint entryIndex = 0; entryIndex < dataPacketCount; entryIndex++)
				{
					DataPacket dataPacket = new DataPacket();

					if (parser.Parse(dataPacket))
					{
						_dataPackets.Add(dataPacket);
					}
					else
					{
						break;
					}
				}
			}
			catch (ReadOverflowException)
			{
				Valid = false;
			}
		}

		public override bool ParseEnd(AsfParser parser)
		{
			// The DataObject is to important to just ignore it when 
			// the end of the data object is not in the file.
			// Instead try to fix it by temporary ignoring the value of
			// the ReadOverflow and Valid properties.
			// Flag it as invalid by setting the Valid property to false.
			// Get it added anyway by returning true.
			return ParseEnd(parser, true);
		}
	}
}
