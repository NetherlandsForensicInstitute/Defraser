/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Netherlands Forensic Institute nor the names 
 *    of its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	class AvcConfigurationBox : QtAtom
	{
		public new enum Attribute
		{
			ConfigurationVersion,	// uint8
			AvcProfileIndication,	// uint8
			ProfileCompatibility,	// uint8
			AvcLevelIndication,		// uint8
			// bit(6) reserved = ‘111111’b;
			// unsigned int(2) lengthSizeMinusOne;
			LengthSizeMinusOne,		// 2 bits
			// bit(3) reserved = ‘111’b;
			// unsigned int(5) numOfSequenceParameterSets;
			NumOfSequenceParameterSets,	// 5 bits
			NumOfPictureParameterSets,	// uint8
			SequenceParameterSets,
			PictureParameterSets,
		}

		public AvcConfigurationBox(QtAtom previousHeader)
			: base(previousHeader, AtomName.AvcConfigurationBox)
		{
		}

		public IDataPacket SequenceParameterSets { get; private set; }
		public IDataPacket PictureParameterSets { get; private set; }
		public IDataPacket ExtraData
		{
			get
			{
				if (SequenceParameterSets != null)
				{
					if (PictureParameterSets != null)
					{
						return SequenceParameterSets.Append(PictureParameterSets);
					}

					return SequenceParameterSets;
				}
				if (PictureParameterSets != null)
				{
					return PictureParameterSets;
				}

				return null;
			}
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			byte version = parser.GetByte(Attribute.ConfigurationVersion);
			if(!CheckValue(version, 1)) return false;

			parser.GetByte(Attribute.AvcProfileIndication);
			parser.GetByte(Attribute.ProfileCompatibility);
			parser.GetByte(Attribute.AvcLevelIndication);
			// bit(6) reserved = ‘111111’b;
			// unsigned int(2) lengthSizeMinusOne;
			byte lengthSizeMinusOne = parser.GetByte();
			if(!CheckReservedBits(lengthSizeMinusOne, 0xFC)) return false;
			lengthSizeMinusOne &= 0x03;
			parser.AddAttribute(new FormattedAttribute<Attribute, byte>(Attribute.LengthSizeMinusOne, lengthSizeMinusOne));

			// bit(3) reserved = ‘111’b;
			// unsigned int(5) numOfSequenceParameterSets;
			byte numOfSequenceParameterSets = parser.GetByte();

			if (((QtAtom)Parent).HeaderName == AtomName.AvcParameterSampleEntry)
			{
				if (!CheckValue(numOfSequenceParameterSets, 0)) return false;

				parser.AddAttribute(new FormattedAttribute<Attribute, byte>(Attribute.NumOfSequenceParameterSets, numOfSequenceParameterSets));
			}
			else
			{
				// Check the three msb
				//FIXME: if (!CheckReservedBits(numOfSequenceParameterSets, 0xE0)) return false;

				numOfSequenceParameterSets &= 0x1F;	// remove the three msb
				parser.AddAttribute(new FormattedAttribute<Attribute, byte>(Attribute.NumOfSequenceParameterSets, numOfSequenceParameterSets));
			}

			SequenceParameterSets = CreateNalUnit(parser, numOfSequenceParameterSets, Attribute.SequenceParameterSets);

			byte numOfPictureParameterSets = parser.GetByte(Attribute.NumOfPictureParameterSets);
			if (((QtAtom)Parent).HeaderName == AtomName.AvcParameterSampleEntry)
			{
				if (!CheckValue(numOfPictureParameterSets, 0)) return false;
			}

			PictureParameterSets = CreateNalUnit(parser, numOfPictureParameterSets, Attribute.PictureParameterSets);

			return Valid;
		}

		// for (i=0; i< numOfSequenceParameterSets; i++) {
		//   unsigned int(16) sequenceParameterSetLength ;
		//   bit(8*sequenceParameterSetLength) sequenceParameterSetNALUnit;
		// }
		//
		// for (i=0; i< numOfPictureParameterSets; i++) {
		//   unsigned int(16) pictureParameterSetLength;
		//   bit(8*pictureParameterSetLength) pictureParameterSetNALUnit;
		// }
		private static IDataPacket CreateNalUnit(QtParser parser, byte setCount, Attribute attribute)
		{
			long beginOffset = parser.Position;
			int totalSetLength = 0;
			for (int i = 0; i < setCount; i++)
			{
				ushort setLength = parser.GetUShort();
				parser.Position += setLength;
				totalSetLength += setLength + sizeof(ushort);
			}
			if (totalSetLength > 0)
			{
				parser.Position = beginOffset;
				parser.GetHexDump<Attribute>(attribute, totalSetLength);

				return parser.GetDataPacket(beginOffset, totalSetLength);
			}
			return null;
		}

		private bool CheckValue(byte valueToCheck, byte valueToCheckAgainst)
		{
			if (valueToCheck != valueToCheckAgainst)
			{
				Valid = false;
			}
			return Valid;
		}

		private bool CheckReservedBits(byte byteToTest, int filterByte)
		{
			if ((byteToTest & filterByte) != filterByte)
			{
				Valid = false;
			}
			return Valid;
		}
	}
}
