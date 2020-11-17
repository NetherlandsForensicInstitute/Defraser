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
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Asf
{
	internal enum ObjectStreamTypeGuid
	{
		// No Guid attribute
		Unknown,
		[Guid("F8699E40-5B4D-11CF-A8FD-00805F5C442B")]
		AudioMedia,
		[Guid("BC19EFC0-5B4D-11CF-A8FD-00805F5C442B")]
		VideoMedia,
		[Guid("59DACFC0-59E6-11D0-A3AC-00A0C90348F6")]
		CommandMedia,
		[Guid("B61BE100-5B4E-11CF-A8FD-00805F5C442B")]
		JfifMedia,
		[Guid("35907DE0-E415-11CF-A917-00805F5C442B")]
		DegradableJpegMedia,
		[Guid("91BD222C-F21C-497A-8B6D-5AA86BFC0185")]
		FileTransferMedia,
		[Guid("3AFB65E2-47EF-40F2-AC2C-70A90D71D343")]
		BinaryMedia
	}

	internal class StreamPropertiesObject : AsfObject
	{
		#region Inner class
		private class StreamPropertiesFlags<TEnum> : CompositeAttribute<TEnum, string, AsfParser>
		{
			internal byte StreamNumber { get; private set; }

			public enum LAttribute
			{
				StreamNumber,
				Reserved,
				EncryptedContentFlag,
			}

			public StreamPropertiesFlags(TEnum attributeName)
				: base(attributeName, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				// Parse the flags word
				ushort flags = parser.GetUShort();
				StreamNumber = (byte)(flags & 0x7F);
				bool encryptedContentFlag = (flags & 0x8000) == 0 ? false : true;
				byte reserved = (byte)((flags >> 7) & 8);

				Attributes.Add(new FormattedAttribute<LAttribute, byte>(LAttribute.StreamNumber, StreamNumber));
				Attributes.Add(new FormattedAttribute<LAttribute, byte>(LAttribute.Reserved, reserved));
				Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.EncryptedContentFlag, encryptedContentFlag));

				TypedValue = string.Format("({0}, {1}, {2})", StreamNumber, reserved, encryptedContentFlag);

				return Valid;
			}
		}
		#endregion Inner class

		enum ObjectErrorCorrectionTypeGuid
		{
			// No Guid attribute
			Unknown,
			[Guid("20FB5700-5B55-11CF-A8FD-00805F5C442B")]
			NoErrorCorrection,
			[Guid("BFC3CD50-618F-11CF-8BB2-00AA00B4E220")]
			AudioSpread
		}

		private new enum Attribute
		{
			StreamType,
			ErrorCorrectionType,
			TimeOffset,
			TypeSpecificDataLength,
			ErrorCorrectionDataLength,
			Flags,
			Reserved,
			TypeSpecificData,
			ErrorCorrectionData,

			//TypeSpecificData
			FormatTag,
			NumberofChannels,
			SamplesPerSecond,
			AverageNumberofBytesPerSecond,
			BlockAlignment,
			BitsPerSample,
			CodecSpecificDataSize,
			EncodedImageWidth,
			EncodedImageHeight,
			ReservedFlags,
			FormatDataSize,
			FormatDataSize2,
			ImageWidth,
			ImageHeight,
			BitsPerPixelCount,
			CompressionID,
			ImageSize,
			HorizontalPixelsPerMeter,
			VerticalPixelsPerMeter,
			ColorsUsedCount,
			ImportantColorsCount,
			CodecSpecificData,
			InterchangeDataLength,
			InterchangeData,

			#region Error Specific Data - ASF Audio Spread
			/// <summary>
			/// Specifies the number of packets over which audio will be spread.
			/// Typically, this value should be set to 1.
			/// </summary>
			Span,
			/// <summary>
			/// Specifies the virtual packet length. The value of this field should be
			/// set to the size of the largest audio payload found in the audio stream.
			/// </summary>
			VirtualPacketLength,
			/// <summary>
			/// Specifies the virtual chunk length. The value of this field should be
			/// set to the size of the largest audio payload found in the audio stream.
			/// </summary>
			VirtualChunkLength,
			/// <summary>
			/// Specifies the number of bytes stored in the Silence Data field.
			/// This value should be set to 1. It is also valid for this value
			/// to equal the Block Alignment value (from the Audio Media Type).
			/// </summary>
			SilenceDataLength,
			/// <summary>
			/// Specifies an array of silence data bytes. This value should be
			/// set to 0 for the length of Silence Data Length.
			/// </summary>
			SilenceData
			#endregion Error Specific Data - ASF Audio Spread
		}

		private static readonly IDictionary<Guid, ObjectStreamTypeGuid> ObjectStreamTypeGuidNameForValue = CreateNameForValueCollection<ObjectStreamTypeGuid>();
		private static readonly IDictionary<Guid, ObjectErrorCorrectionTypeGuid> ObjectErrorCorrectionTypeGuidNameForValue = CreateNameForValueCollection<ObjectErrorCorrectionTypeGuid>();

		internal string CompressionId { get; private set; }
		internal short StreamNumber { get; private set; }
		internal ObjectStreamTypeGuid StreamType { get; private set; }
		internal IDataPacket CodecSpecificData { get; private set; }

		public StreamPropertiesObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.StreamPropertiesObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			StreamType = parser.GetGuid(Attribute.StreamType, ObjectStreamTypeGuidNameForValue);
			ObjectErrorCorrectionTypeGuid errorType = parser.GetGuid(Attribute.ErrorCorrectionType, ObjectErrorCorrectionTypeGuidNameForValue);
			parser.GetLong(Attribute.TimeOffset);
			parser.GetInt(Attribute.TypeSpecificDataLength);
			parser.GetInt(Attribute.ErrorCorrectionDataLength);
			StreamPropertiesFlags<Attribute> streamPropertiesFlags = new StreamPropertiesFlags<Attribute>(Attribute.Flags);
			parser.Parse(streamPropertiesFlags);
			StreamNumber = streamPropertiesFlags.StreamNumber;
			parser.GetInt(Attribute.Reserved);

			if (StreamType == ObjectStreamTypeGuid.AudioMedia)
			{
				parser.GetShort(Attribute.FormatTag);
				parser.GetShort(Attribute.NumberofChannels);
				parser.GetInt(Attribute.SamplesPerSecond);
				parser.GetInt(Attribute.AverageNumberofBytesPerSecond);
				parser.GetShort(Attribute.BlockAlignment);
				parser.GetShort(Attribute.BitsPerSample);
				short dataSize = parser.GetShort(Attribute.CodecSpecificDataSize);
				parser.GetHexDump(Attribute.CodecSpecificData, dataSize);
			}
			else if (StreamType == ObjectStreamTypeGuid.VideoMedia)
			{
				parser.GetInt(Attribute.EncodedImageWidth);
				parser.GetInt(Attribute.EncodedImageHeight);
				parser.GetByte(Attribute.ReservedFlags);
				short formatDataSize = parser.GetShort(Attribute.FormatDataSize);

				// Format data fields
				const short FormatDataFieldsSize = 4 + 4 + 4 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4;	// sum of all format data field sizes
				parser.GetInt(Attribute.FormatDataSize2);
				parser.GetInt(Attribute.ImageWidth);
				parser.GetInt(Attribute.ImageHeight);
				parser.GetShort(Attribute.Reserved);
				parser.GetShort(Attribute.BitsPerPixelCount);
				CompressionId = parser.GetString(Attribute.CompressionID, 4);
				parser.GetInt(Attribute.ImageSize);
				parser.GetInt(Attribute.HorizontalPixelsPerMeter);
				parser.GetInt(Attribute.VerticalPixelsPerMeter);
				parser.GetInt(Attribute.ColorsUsedCount);
				parser.GetInt(Attribute.ImportantColorsCount);
				long offset = parser.Position;
				int dataSize = formatDataSize - FormatDataFieldsSize;
				parser.GetHexDump(Attribute.CodecSpecificData, dataSize);
				// Create a data packet for the video codec specific data
				if (Valid && dataSize > 0)
				{
					CodecSpecificData = parser.GetDataPacket(offset, dataSize);
				}
			}
			else if (StreamType == ObjectStreamTypeGuid.JfifMedia)
			{
				parser.GetInt(Attribute.ImageWidth);
				parser.GetInt(Attribute.ImageHeight);
				parser.GetInt(Attribute.Reserved);
			}
			else if (StreamType == ObjectStreamTypeGuid.DegradableJpegMedia)
			{
				parser.GetInt(Attribute.ImageWidth);
				parser.GetInt(Attribute.ImageHeight);
				parser.GetShort(Attribute.Reserved);
				parser.GetShort(Attribute.Reserved);
				parser.GetShort(Attribute.Reserved);
				short dataLength = parser.GetShort(Attribute.InterchangeDataLength);
				parser.GetString(Attribute.InterchangeData, dataLength);
			}

			if (errorType == ObjectErrorCorrectionTypeGuid.AudioSpread)
			{
				parser.GetByte(Attribute.Span);
				parser.GetShort(Attribute.VirtualPacketLength);
				parser.GetShort(Attribute.VirtualChunkLength);
				short silenceDataLength = parser.GetShort(Attribute.SilenceDataLength);
				parser.GetHexDump(Attribute.SilenceData, silenceDataLength);
			}
			return Valid;
		}
	}
}
