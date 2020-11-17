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

using System.Collections.Generic;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Asf
{
	class DataObjectWithoutStart : AsfObject, IDataObject
	{
		private readonly List<DataPacket> _dataPackets;

		public IEnumerable<DataPacket> DataPackets { get { return _dataPackets; } }

		public DataObjectWithoutStart(AsfObject previousObject)
			: base(previousObject, AsfObjectName.DataObjectWithoutStart)
		{
			_dataPackets = new List<DataPacket>();
		}

		public override bool Parse(AsfParser parser)
		{
			_offset = parser.Position;

			CreateDataPacketTable(parser);

			return true;
		}

		private void CreateDataPacketTable(AsfParser parser)
		{
			try
			{
				DataPacket dataPacket = new DataPacket();

				long endOffsetOfLastCorrectDataPacket = 0L;

				while (parser.Parse(dataPacket))
				{
					_dataPackets.Add(dataPacket);

					endOffsetOfLastCorrectDataPacket = parser.Position;

					dataPacket = new DataPacket();
				}
				parser.Position = endOffsetOfLastCorrectDataPacket;

				Size = (ulong)(parser.Position - Offset);
			}
			catch (ReadOverflowException)
			{
				Valid = false;
			}
		}
	}
}
