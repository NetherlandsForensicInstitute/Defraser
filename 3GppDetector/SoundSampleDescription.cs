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

namespace Defraser.Detector.QT
{
	internal class SoundSampleDescription : QtSampleDescriptionAtom
	{
		public new enum Attribute
		{
			CompressedDataVersion,
			RevisionLevel,
			Vendor,
			// Sound sample description specific
			NumberOfChannels,
			SampleSize,
			CompressionID,
			PacketSize,
			SampleRate,
			// Sound sample description, version 1
			SamplePerPacket,
			BytesPerPacket,
			BytesPerFrame,
			BytesPerSample
		}

		public SoundSampleDescription(QtAtom previousHeader)
			: base(previousHeader, AtomName.SoundSampleDescription)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (!CheckComponentSubType(ComponentSubType.soun))
			{
				this.Valid = false;
				return this.Valid;
			}

			// Audio specific
			ushort version = parser.GetUShort(Attribute.CompressedDataVersion);
			ushort revisionLevel = parser.GetUShort(Attribute.RevisionLevel);
			parser.CheckAttribute(Attribute.RevisionLevel, revisionLevel == 0);
			uint vendor = parser.GetFourCC(Attribute.Vendor);
			parser.CheckAttribute(Attribute.Vendor, vendor == 0, false);	// Not always true
			ushort numberOfChannels = parser.GetUShort(Attribute.NumberOfChannels);
			parser.CheckAttribute(Attribute.NumberOfChannels, numberOfChannels == 1 || numberOfChannels == 2, false); // Example with value 6 found
			ushort sampleSize = parser.GetUShort(Attribute.SampleSize);
			parser.CheckAttribute(Attribute.SampleSize, sampleSize == 8 || sampleSize == 16 , false);
			short compressionID = parser.GetShort(Attribute.CompressionID);
			parser.CheckAttribute(Attribute.CompressionID, compressionID == 0 || compressionID == -1 || compressionID == -2);
			ushort packetSize = parser.GetUShort(Attribute.PacketSize);
			parser.CheckAttribute(Attribute.PacketSize, packetSize == 0);
			parser.GetFixed16_16(Attribute.SampleRate);

			if (version == 1)
			{
				parser.GetUInt(Attribute.SamplePerPacket);
				parser.GetUInt(Attribute.BytesPerPacket);
				parser.GetUInt(Attribute.BytesPerFrame);
				parser.GetUInt(Attribute.BytesPerSample);
			}
			return this.Valid;
		}
	}
}
