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

namespace Defraser.Detector.Asf
{
    // TODO: make sealed again
	internal class DataPacket : CompositeAttribute<DataObject.Attribute, string, AsfParser>
	{
		public enum LAttribute
		{
			ErrorCorrectionFlags,
			ErrorCorrectionData,
			LengthTypeFlags,
			PropertyFlags,
			PacketLength,
			Sequence,
			PaddingLength,
			SendTime,
			Duration,

			Payload,
			//PayloadFlags,
			NumberOfPayloads,
			PayloadLengthType,
			PaddingData,
		}

		private int _errorCorrectionLength;
		internal IList<Payload> Payloads { get; private set; }
		internal LengthType ReplicatedDataLengthType { get { return _propertyFlags.ReplicatedDataLengthType; } }
		internal LengthType OffsetIntoMediaObjectLengthType { get { return _propertyFlags.OffsetIntoMediaObjectLengthType; } }
		internal LengthType MediaObjectNumberLengthType { get { return _propertyFlags.MediaObjectNumberLengthType; } }
		internal LengthType PayloadLengthType { get; private set; }
		internal int PaddingLength { get; private set; }
		internal bool MultiplePayloadsPresent { get { return _lengthTypeFlags.MultiplePayloadsPresent; } }

		private LengthTypeFlags _lengthTypeFlags;
		private PropertyFlags _propertyFlags;

		internal int PacketHeaderLength
		{
			get
			{
				return _errorCorrectionLength +
					1 +		// Length Type Flags
					1 +		// Property Flags
					_lengthTypeFlags.PacketLengthType.GetLengthInBytes() +
					_lengthTypeFlags.SequenceType.GetLengthInBytes() +
					_lengthTypeFlags.PaddingLengthType.GetLengthInBytes() +
					4 +		// Send Time
					2;		// Duration
			}
		}

		internal int GetPayloadHeaderLength(int replicatedDataLength)
		{
			return 1 +	// Stream number
				_propertyFlags.MediaObjectNumberLengthType.GetLengthInBytes() +
				_propertyFlags.OffsetIntoMediaObjectLengthType.GetLengthInBytes() +
				_propertyFlags.ReplicatedDataLengthType.GetLengthInBytes() +
				replicatedDataLength;
		}

		public DataPacket()
			: base(DataObject.Attribute.DataPacket, string.Empty, "{0}")
		{
			Payloads = new List<Payload>();
		}

		public override bool Parse(AsfParser parser)
		{
			byte data = parser.GetByte();

			if (ErrorCorrectionPresent(data))
			{
				ParseErrorCorrectionData(parser, data);

				// Read the first byte of the payload parsing information
				data = parser.GetByte();
			}

			ParsePayloadParsingInformation(parser, data);

			ParsePayloadData(parser);

			if (PaddingLength > 0 && Valid)
			{
				parser.GetHexDump(LAttribute.PaddingData, PaddingLength);
			}

			return Valid;
		}

		private void ParsePayloadParsingInformation(AsfParser parser, byte data)
		{
			if(!Valid) return;

			_lengthTypeFlags = new LengthTypeFlags(data);
			Valid = parser.Parse(_lengthTypeFlags);
			if(Valid == false) return;

			_propertyFlags = new PropertyFlags();
			Valid = parser.Parse(_propertyFlags);
			if(Valid == false) return;

			parser.GetLengthValue(LAttribute.PacketLength, _lengthTypeFlags.PacketLengthType);
			parser.GetLengthValue(LAttribute.Sequence, _lengthTypeFlags.SequenceType);
			PaddingLength = parser.GetLengthValue(LAttribute.PaddingLength, _lengthTypeFlags.PaddingLengthType);
			parser.GetInt(LAttribute.SendTime);		// in milliseconds
			parser.GetShort(LAttribute.Duration);	// in milliseconds
		}

		private void ParseErrorCorrectionData(AsfParser parser, byte data)
		{
			if(!Valid) return;

			ErrorCorrectionFlags errorCorrectionFlags = new ErrorCorrectionFlags(data);
			Valid = parser.Parse(errorCorrectionFlags);
			if (Valid == false) return;

			parser.GetHexDump(LAttribute.ErrorCorrectionData, errorCorrectionFlags.ErrorCorrectionDataLength);

			_errorCorrectionLength = 1 /* Error Correction Flags */+ errorCorrectionFlags.ErrorCorrectionDataLength;
		}

		private int GetPayloadFlags(AsfParser parser)
		{
			byte payloadFlag = parser.GetByte();
			int numberOfPayloads = (payloadFlag & 0x3F);
			Attributes.Add(new FormattedAttribute<LAttribute, int>(LAttribute.NumberOfPayloads, numberOfPayloads));

			PayloadLengthType = (LengthType) ((payloadFlag >> 6) & 3);
			Attributes.Add(new FormattedAttribute<LAttribute, string>(LAttribute.PayloadLengthType, PayloadLengthType.PrettyPrint()));

			return numberOfPayloads;
		}

		private static bool ErrorCorrectionPresent(byte data)
		{
			return (data & 0x80) != 0;
		}

		private void ParsePayloadData(AsfParser parser)
		{
			if(!Valid) return;

			int numberOfPayloads = GetNumberOfPayloads(parser);

			for (uint entryIndex = 0; entryIndex < numberOfPayloads; entryIndex++)
			{
				if(!parser.Parse(new Payload(this)))
				{
					Valid = false;
					return;
				}
			}
		}

		private int GetNumberOfPayloads(AsfParser parser)
		{
			if (_lengthTypeFlags.MultiplePayloadsPresent)
			{
				return GetPayloadFlags(parser);
			}

			// Single payload
			return 1;
		}
	}
}
