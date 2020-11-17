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
using Defraser.Detector.Common;
using Defraser.Interface;
using System.Collections.Generic;
using System.Diagnostics;

namespace Defraser.Detector.Asf
{
	internal class Payload : CompositeAttribute<DataPacket.LAttribute, string, AsfParser>
	{
		public enum BAttribute
		{
			StreamNumber,
			KeyFrameBit,

			MediaObjectNumber,
			OffsetIntoMediaObject,
			ReplicatedDataLength,
			ReplicatedData,
			PayloadDataSize,
			PayloadData,

			PayloadLengthType,
			CompressedPayload,
			PresentationTime,
			PresentationTimeDelta
		}

		private readonly DataPacket _dataPacket;

		public byte StreamNumber { get; private set; }
		//public IDataPacket StreamData { get; private set; }
		public IList<IDataPacket> Streams { get; private set; }
		//public int DataLength { get; private set; }

		public Payload(DataPacket dataPacket)
			: base(DataPacket.LAttribute.Payload, string.Empty, "{0}")
		{
			_dataPacket = dataPacket;
			Streams = new List<IDataPacket>();
		}

		public override bool Parse(AsfParser parser)
		{
			StreamNumber = GetStreamNumber(parser);
			parser.GetLengthValue(BAttribute.MediaObjectNumber, _dataPacket.MediaObjectNumberLengthType);

			// Next value is
			// - 'Offset Into Media Object' for non-compressed payloads and
			// - 'Presentation Time' for compressed payloads
			// Compressed payloads have a replicatedDataLength with a value of '1'.
			int value = parser.GetLengthValue(_dataPacket.OffsetIntoMediaObjectLengthType);
			int replicatedDataLength = parser.GetLengthValue(_dataPacket.ReplicatedDataLengthType);

			if (CompressedPayloads(replicatedDataLength))
			{
				parser.AddAttribute(new FormattedAttribute<BAttribute, int>(BAttribute.PresentationTime, value));
			}
			else	// non-compressed
			{
				parser.AddAttribute(new FormattedAttribute<BAttribute, int>(BAttribute.OffsetIntoMediaObject, value));
			}

			parser.AddAttribute(new FormattedAttribute<BAttribute, int>(BAttribute.ReplicatedDataLength, replicatedDataLength));

			if (CompressedPayloads(replicatedDataLength))
			{
				parser.GetByte(BAttribute.PresentationTimeDelta);
			}
			else	// non-compressed
			{
				parser.GetHexDump(BAttribute.ReplicatedData, replicatedDataLength);
			}

			int payloadDataLength;

			if (_dataPacket.MultiplePayloadsPresent)
			{
				payloadDataLength = parser.GetLengthValue(BAttribute.PayloadDataSize, _dataPacket.PayloadLengthType);
			}
			else	// Single payload
			{
				payloadDataLength = parser.DataPacketLength - _dataPacket.PacketHeaderLength - _dataPacket.GetPayloadHeaderLength(replicatedDataLength) - _dataPacket.PaddingLength;
				Attributes.Add(new FormattedAttribute<BAttribute, int>(BAttribute.PayloadDataSize, payloadDataLength));
			}

			if (payloadDataLength < 0)
			{
				Valid = false;
				return false;
			}
			if (CompressedPayloads(replicatedDataLength))
			{
				int totalDataLength = 0;

				while (payloadDataLength > totalDataLength)
				{
					if (payloadDataLength == 0) break;

					int dataLength = parser.GetByte();
					long dataPacketStartOffset = parser.Position;
					//parser.Parse(new CompressedPayload(this));

					long croppedPayloadDataLength = Math.Min(dataLength, parser.Length - dataPacketStartOffset);
					if (croppedPayloadDataLength > 0)
					{
						Streams.Add(parser.GetDataPacket(dataPacketStartOffset, croppedPayloadDataLength));
					}
					parser.Position = Math.Min(parser.Position + dataLength, parser.Length);

					totalDataLength += dataLength + 1; // '1' for length of dataLength(Byte)
				}
			}
			else	// normal (uncompressed) payload data
			{
				long dataPacketStartOffset = parser.Position;
				long croppedPayloadDataLength = Math.Min(payloadDataLength, parser.Length - dataPacketStartOffset);
				if (croppedPayloadDataLength > 0)
				{
					Streams.Add(parser.GetDataPacket(dataPacketStartOffset, croppedPayloadDataLength));
				}
				parser.Position = Math.Min(parser.Position + payloadDataLength, parser.Length);
			}

			_dataPacket.Payloads.Add(this);

			return Valid;
		}

		private static bool CompressedPayloads(int replicatedDataLength)
		{
			return replicatedDataLength == 1;
		}

		private byte GetStreamNumber(AsfParser parser)
		{
			byte data = parser.GetByte();

			Attributes.Add(new FormattedAttribute<BAttribute, byte>(BAttribute.StreamNumber, (byte)(data & 0x7F)));
			Attributes.Add(new FormattedAttribute<BAttribute, bool>(BAttribute.KeyFrameBit, ((data ) & 0x80) != 0));

			return (byte) (data & 0x7F);
		}
	}
}
