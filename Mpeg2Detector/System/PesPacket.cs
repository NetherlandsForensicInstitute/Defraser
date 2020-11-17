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

using System;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class PesPacket : ISystemHeaderParser
	{
		private enum Attribute
		{
			PesPacketLength,
			Mpeg2Flag,
			PesScramblingControl,
			PesPriority,
			DataAlignmentIndicator,
			Copyright,
			OriginalOrCopy,
			PtsDtsFlags,
			EscrFlag,
			ESRateFlag,
			DsmTrickModeFlag,
			AdditionalCopyInfoFlag,
			PesCrcFlag,
			PesExtensionFlag,
			PesHeaderDataLength,
			PStdBufferBoundScale,
			PStdBufferSizeBound,
			PresentationTimeStamp,
			DecodingTimeStamp,
			Escr,
			EscrExtension,
			ESRate,
			TrickModeControl,
			FieldId,
			IntraSliceRefresh,
			FrequencyTruncation,
			RepCntrl,
			TrickModeReserved,
			AdditionalCopyInfo,
			PreviousPesPacketCrc,
			PesPrivateDataFlag,
			PackHeaderFieldFlag,
			ProgramPacketSequenceCounterFlag,
			PStdBufferFlag,
			PesExtensionFlag2,
			PackFieldLength,
			PesPrivateData,
			ProgramPacketSequenceCounter,
			Mpeg1Mpeg2Identifier,
			OriginalStuffLength,
			PesExtensionFieldLength,
			PesExtensionField
		}

		private const string Name = "PesPacket";

		private readonly TimeStampAttribute<Attribute> _presentationTimeStampAttribute;
		private readonly TimeStampAttribute<Attribute> _decodingTimeStampAttribute;
		private readonly TimeStampAttribute<Attribute> _escrAttribute;

		#region Properties
		public uint StartCode { get { return 0x1bd; } }
		#endregion Properties

		public PesPacket()
		{
			_presentationTimeStampAttribute = new TimeStampAttribute<Attribute>(Attribute.PresentationTimeStamp, false);
			_decodingTimeStampAttribute = new TimeStampAttribute<Attribute>(Attribute.DecodingTimeStamp, false);
			_escrAttribute = new TimeStampAttribute<Attribute>(Attribute.Escr, true);
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2SystemReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;
			resultState.ParentName = PackHeader.Name;

			uint pesPacketLength = reader.GetBits(16, Attribute.PesPacketLength, n => n <= reader.BytesRemaining);
			long pesPacketEnd = reader.Position + pesPacketLength;
			resultState.Recover(); // Invalid PES packets should not end the current block!

			ushort streamId = (ushort)(reader.State.StartCode & 0xFF);
			if (streamId == 0xBE || streamId == 0xBF || streamId == 0xF0 ||
			    streamId == 0xF1 || streamId == 0xF2 || streamId == 0xF8)
			{
				// These streams do not contain further header details
			}
			else if (reader.ShowBits(2) == 2)
			{
				#region MPEG-2
				reader.SetMpegFormat(CodecID.Mpeg2System);
				reader.GetBits(2, Attribute.Mpeg2Flag, f => f == 0x2);
				reader.GetBits(2, Attribute.PesScramblingControl);
				reader.GetBits(1, Attribute.PesPriority);
				reader.GetFlag(Attribute.DataAlignmentIndicator);
				reader.GetFlag(Attribute.Copyright);
				reader.GetFlag(Attribute.OriginalOrCopy);
				uint ptsDtsFlags = reader.GetBits(2, Attribute.PtsDtsFlags, f => f != 0x1);
				bool escrFlag = reader.GetFlag(Attribute.EscrFlag);
				bool esRateFlag = reader.GetFlag(Attribute.ESRateFlag);
				bool dsmTrickModeFlag = reader.GetFlag(Attribute.DsmTrickModeFlag);
				bool additionalCopyInfoFlag = reader.GetFlag(Attribute.AdditionalCopyInfoFlag);
				bool pesCrcFlag = reader.GetFlag(Attribute.PesCrcFlag);
				bool pesExtensionFlag = reader.GetFlag(Attribute.PesExtensionFlag);

				uint bytesRemaining = pesPacketLength - 3;
				uint pesHeaderDataLength = reader.GetBits(8, Attribute.PesHeaderDataLength, n => n <= bytesRemaining);
				long pesHeaderDataEnd = reader.Position + pesHeaderDataLength;

				if (!resultState.Valid) return;

				// Decode optional header fields
				switch (ptsDtsFlags)
				{
					case 2:
						reader.GetBits(4, 0x2);
						reader.GetAttribute(_presentationTimeStampAttribute);
						break;
					case 3:
						reader.GetBits(4, 0x3);
						reader.GetAttribute(_presentationTimeStampAttribute);
						reader.GetBits(4, 0x1);
						reader.GetAttribute(_decodingTimeStampAttribute);
						break;
				}

				if (escrFlag)
				{
					reader.GetAttribute(_escrAttribute);
					reader.GetMarker();
				}
				if (esRateFlag)
				{
					reader.GetMarker();
					reader.GetBits(22, Attribute.ESRate);
					reader.GetMarker();
				}
				if (dsmTrickModeFlag)
				{
					switch (reader.GetBits(3, Attribute.TrickModeControl))
					{
						case 0:
						case 3:
							reader.GetBits(2, Attribute.FieldId);
							reader.GetFlag(Attribute.IntraSliceRefresh);
							reader.GetBits(1, Attribute.FrequencyTruncation); // TODO: is this a flag??
							break;
						case 1:
						case 4:
							reader.GetBits(5, Attribute.RepCntrl);
							break;
						case 2:
							reader.GetBits(2, Attribute.FieldId);
							reader.GetBits(3, Attribute.TrickModeReserved);
							break;
						default:
							reader.GetBits(5, Attribute.TrickModeReserved);
							break;
					}
				}
				if (additionalCopyInfoFlag)
				{
					reader.GetMarker();
					reader.GetBits(7, Attribute.AdditionalCopyInfo);
				}
				if (pesCrcFlag)
				{
					reader.GetBits(16, Attribute.PreviousPesPacketCrc);
				}
				if (pesExtensionFlag)
				{
					bool pesPrivateDataFlag = reader.GetFlag(Attribute.PesPrivateDataFlag);
					bool packHeaderFieldFlag = reader.GetFlag(Attribute.PackHeaderFieldFlag);
					bool programPacketSequenceCounterFlag = reader.GetFlag(Attribute.ProgramPacketSequenceCounterFlag);
					bool pStdBufferFlag = reader.GetFlag(Attribute.PStdBufferFlag);
					reader.GetReservedBits(3);
					bool pesExtensionFlag2 = reader.GetFlag(Attribute.PesExtensionFlag2);

					if (pesPrivateDataFlag)
					{
						reader.GetData(Attribute.PesPrivateData, 16);
					}
					if (packHeaderFieldFlag)
					{
						reader.GetBits(8, Attribute.PackFieldLength);
						// TODO: pack_header();
					}
					if (programPacketSequenceCounterFlag)
					{
						reader.GetMarker();
						reader.GetBits(7, Attribute.ProgramPacketSequenceCounter);
						reader.GetMarker();
						reader.GetBits(1, Attribute.Mpeg1Mpeg2Identifier);
						reader.GetBits(6, Attribute.OriginalStuffLength);
					}
					if (pStdBufferFlag)
					{
						reader.GetBits(2, 0x1);
						reader.GetBits(1, Attribute.PStdBufferBoundScale);
						reader.GetBits(13, Attribute.PStdBufferSizeBound);
					}
					if (pesExtensionFlag2)
					{
						reader.GetMarker();
						uint pesExtensionFieldLength = reader.GetBits(7, Attribute.PesExtensionFieldLength);
						reader.GetData(Attribute.PesExtensionField, (int)pesExtensionFieldLength);
					}
				}

				if (reader.Position > pesHeaderDataEnd || reader.Position < (pesHeaderDataEnd - 32))
				{
					// FIXME: reader.CheckAttribute(Attribute.PesHeaderDataLength, false);
					resultState.Invalidate();
					return;
				}

				int stuffingBytes = (int)(pesHeaderDataEnd - reader.Position);
				if (stuffingBytes > 0)
				{
					// TODO issue 2323 MPEG-2 systems detector does not check header stuffing of private stream 1
					if (streamId == 0xBD)
					{
						// Note: The format of these bytes is different!
						reader.SkipBytes(stuffingBytes);
					}
					else
					{
						reader.GetStuffingBytes(stuffingBytes);
					}
				}
				#endregion MPEG-2
			}
			else
			{
				#region MPEG-1
				reader.SetMpegFormat(CodecID.Mpeg1System);
				//FIXME: Attributes.Add(new FormattedAttribute<Attribute, bool>(Attribute.Mpeg2Flag, false));

				uint maxUnknownByteCount = (uint)Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.PesPacketMaxUnknownByteCount];

				int count = 0;	// Sanity check
				while (reader.ShowBits(1) == 1 && count++ < maxUnknownByteCount)
				{
					reader.GetBits(8);	// TODO: attribute?
				}
				if (reader.ShowBits(2) == 0x1)
				{
					reader.GetBits(2, 0x1);
					reader.GetBits(1, Attribute.PStdBufferBoundScale);
					reader.GetBits(13, Attribute.PStdBufferSizeBound);
				}

				switch (reader.GetBits(4, Attribute.PtsDtsFlags))
				{
					case 0:
						reader.GetBits(4, 0xF);
						break;
					case 2:
						reader.GetAttribute(_presentationTimeStampAttribute);
						break;
					case 3:
						reader.GetAttribute(_presentationTimeStampAttribute);
						reader.GetBits(4, 0x1);
						reader.GetAttribute(_decodingTimeStampAttribute);
						break;
					default:
						//reader.CheckAttribute(Attribute.PtsDtsFlags, false);
						break;
				}
				#endregion MPEG-1
			}

			if (reader.Position >= pesPacketEnd)
			{
				// FIXME: reader.CheckAttribute(Attribute.PesPacketLength, reader.Position <= pesPacketEnd);
				resultState.Invalidate();
				return;
			}

			int dataLength = (int)Math.Min((pesPacketEnd - reader.Position), reader.BytesRemaining);
			if (streamId == 0xBE)	// Padding stream
			{
				// Read padding bytes
				if (reader.ShowBits(8) == 0x0F)
				{
					reader.GetByte();	// MPEG-1 padding stream
					dataLength--;
				}
				for (int i = 0; i < dataLength; i++)
				{
					if (reader.GetByte() != 0xFF)
					{
						// Truncate packet
						// TODO: reader.CheckAttribute(Attribute.PesPacketLength, reader.GetByte() == 0xFF, false))
						pesPacketEnd = reader.Position - 1;
						reader.SkipBytes(dataLength - i - 1);
						break;
					}
				}
			}
			else
			{
				IDataPacket pesPacketData = reader.GetDataPacket(reader.Position, dataLength);
				reader.SkipBytes(dataLength);
				reader.State.Streams[streamId].AddPayload(pesPacketData);
			}

			// TODO: Check for truncated packet
		}
	}
}
