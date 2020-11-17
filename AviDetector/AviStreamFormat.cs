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

namespace Defraser.Detector.Avi
{
	internal class AviStreamFormat : AviChunk
	{
		/* WAVE form wFormatTag IDs */
		public enum FormatTag	// : ushort	// Do not make this enum an ushort.
		{									// The method 'public short GetShort<T>(T attributeName, Type enumRepresentation)'
											// will fail on it.
			WAVE_FORMAT_UNKNOWN = 0x0000,  /*  Microsoft Corporation  */
			WAVE_FORMAT_PCM = 0x0001,
			WAVE_FORMAT_ADPCM = 0x0002,  /*  Microsoft Corporation  */
			WAVE_FORMAT_IEEE_FLOAT = 0x0003,  /*  Microsoft Corporation  */
			/*  IEEE754: range (+1, -1]  */
			/*  32-bit/64-bit format as defined by */
			/*  MSVC++ float/double type */
			WAVE_FORMAT_IBM_CVSD = 0x0005,  /*  IBM Corporation  */
			WAVE_FORMAT_ALAW = 0x0006,  /*  Microsoft Corporation  */
			WAVE_FORMAT_MULAW = 0x0007,  /*  Microsoft Corporation  */
			WAVE_FORMAT_OKI_ADPCM = 0x0010,  /*  OKI  */
			WAVE_FORMAT_DVI_ADPCM = 0x0011,  /*  Intel Corporation  */
			//WAVE_FORMAT_IMA_ADPCM = (WAVE_FORMAT_DVI_ADPCM) /*  Intel Corporation  */
			WAVE_FORMAT_MEDIASPACE_ADPCM = 0x0012,  /*  Videologic  */
			WAVE_FORMAT_SIERRA_ADPCM = 0x0013,  /*  Sierra Semiconductor Corp  */
			WAVE_FORMAT_G723_ADPCM = 0x0014,  /*  Antex Electronics Corporation  */
			WAVE_FORMAT_DIGISTD = 0x0015,  /*  DSP Solutions, Inc.  */
			WAVE_FORMAT_DIGIFIX = 0x0016,  /*  DSP Solutions, Inc.  */
			WAVE_FORMAT_DIALOGIC_OKI_ADPCM = 0x0017,  /*  Dialogic Corporation  */
			WAVE_FORMAT_MEDIAVISION_ADPCM = 0x0018,  /*  Media Vision, Inc. */
			WAVE_FORMAT_CU_CODEC = 0x0019, /* HP CU */
			WAVE_FORMAT_YAMAHA_ADPCM = 0x0020,  /*  Yamaha Corporation of America  */
			WAVE_FORMAT_SONARC = 0x0021,  /*  Speech Compression  */
			WAVE_FORMAT_DSPGROUP_TRUESPEECH = 0x0022,  /*  DSP Group, Inc  */
			WAVE_FORMAT_ECHOSC1 = 0x0023,  /*  Echo Speech Corporation  */
			WAVE_FORMAT_AUDIOFILE_AF36 = 0x0024,  /* AudioFile AF10 */
			WAVE_FORMAT_APTX = 0x0025,  /*  Audio Processing Technology  */
			WAVE_FORMAT_AUDIOFILE_AF10 = 0x0026, /* AudioFile AF10 */
			WAVE_FORMAT_PROSODY_1612 = 0x0027, /* Prosody 1612 */
			WAVE_FORMAT_LRC = 0x0028, /* LRC */
			WAVE_FORMAT_DOLBY_AC2 = 0x0030,  /*  Dolby Laboratories  */
			WAVE_FORMAT_GSM610 = 0x0031,  /*  Microsoft Corporation  */
			WAVE_FORMAT_MSNAUDIO = 0x0032,  /*  Microsoft Corporation  */
			WAVE_FORMAT_ANTEX_ADPCME = 0x0033,  /*  Antex Electronics Corporation  */
			WAVE_FORMAT_CONTROL_RES_VQLPC = 0x0034,  /*  Control Resources Limited  */
			WAVE_FORMAT_DIGIREAL = 0x0035,  /*  DSP Solutions, Inc.  */
			WAVE_FORMAT_DIGIADPCM = 0x0036,  /*  DSP Solutions, Inc.  */
			WAVE_FORMAT_CONTROL_RES_CR10 = 0x0037,  /*  Control Resources Limited  */
			WAVE_FORMAT_NMS_VBXADPCM = 0x0038,  /*  Natural MicroSystems  */
			WAVE_FORMAT_CS_IMAADPCM = 0x0039, /* Crystal Semiconductor IMA ADPCM */
			WAVE_FORMAT_ECHOSC3 = 0x003A, /* Echo Speech Corporation */
			WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B,  /* Rockwell International */
			WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C,  /* Rockwell International */
			WAVE_FORMAT_XEBEC = 0x003D,  /* Xebec Multimedia Solutions Limited */
			WAVE_FORMAT_G721_ADPCM = 0x0040,  /*  Antex Electronics Corporation  */
			WAVE_FORMAT_G728_CELP = 0x0041,  /*  Antex Electronics Corporation  */
			WAVE_FORMAT_MSG723 = 0x0042, /* MSG723 */
			WAVE_FORMAT_MPEG = 0x0050,  /*  Microsoft Corporation  */
			WAVE_FORMAT_RT24 = 0x0051, /* RT24 */
			//WAVE_FORMAT_PAC = 0x0051, /* PAC */
			WAVE_FORMAT_MPEGLAYER3 = 0x0055,  /*  ISO/MPEG Layer3 Format Tag */
			WAVE_FORMAT_CIRRUS = 0x0059, /* Cirrus */
			WAVE_FORMAT_CIRRUS_LOGIC = 0x0060,  /*  Cirrus Logic  */
			WAVE_FORMAT_ESPCM = 0x0061,  /*  ESS Technology  */
			WAVE_FORMAT_VOXWARE = 0x0062,  /*  Voxware Inc  */
			WAVE_FORMAT_CANOPUS_ATRAC = 0x0063,  /*  Canopus, co., Ltd.  */
			WAVE_FORMAT_G726_ADPCM = 0x0064,  /*  APICOM  */
			WAVE_FORMAT_G722_ADPCM = 0x0065,  /*  APICOM      */
			WAVE_FORMAT_DSAT = 0x0066,  /*  Microsoft Corporation  */
			WAVE_FORMAT_DSAT_DISPLAY = 0x0067,  /*  Microsoft Corporation  */
			WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069, /* Voxware Byte Aligned (obsolete) */
			WAVE_FORMAT_VOXWARE_AC8 = 0x0070, /* Voxware AC8 (obsolete) */
			WAVE_FORMAT_VOXWARE_AC10 = 0x0071, /* Voxware AC10 (obsolete) */
			WAVE_FORMAT_VOXWARE_AC16 = 0x0072, /* Voxware AC16 (obsolete) */
			WAVE_FORMAT_VOXWARE_AC20 = 0x0073, /* Voxware AC20 (obsolete) */
			WAVE_FORMAT_VOXWARE_RT24 = 0x0074, /* Voxware MetaVoice (obsolete) */
			WAVE_FORMAT_VOXWARE_RT29 = 0x0075, /* Voxware MetaSound (obsolete) */
			WAVE_FORMAT_VOXWARE_RT29HW = 0x0076, /* Voxware RT29HW (obsolete) */
			WAVE_FORMAT_VOXWARE_VR12 = 0x0077, /* Voxware VR12 (obsolete) */
			WAVE_FORMAT_VOXWARE_VR18 = 0x0078, /* Voxware VR18 (obsolete) */
			WAVE_FORMAT_VOXWARE_TQ40 = 0x0079, /* Voxware TQ40 (obsolete) */
			WAVE_FORMAT_SOFTSOUND = 0x0080,  /*  Softsound, Ltd.      */
			WAVE_FORMAT_VOXWARE_TQ60 = 0x0081, /* Voxware TQ60 (obsolete) */
			WAVE_FORMAT_MSRT24 = 0x0082, /* MSRT24 */
			WAVE_FORMAT_G729A = 0x0083, /* G.729A */
			WAVE_FORMAT_MVI_MV12 = 0x0084, /* MVI MV12 */
			WAVE_FORMAT_DF_G726 = 0x0085, /* DF G.726 */
			WAVE_FORMAT_DF_GSM610 = 0x0086, /* DF GSM610 */
			WAVE_FORMAT_ISIAUDIO = 0x0088, /* ISIAudio */
			WAVE_FORMAT_ONLIVE = 0x0089, /* Onlive */
			WAVE_FORMAT_SBC24 = 0x0091, /* SBC24 */
			WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092, /* Dolby AC3 SPDIF */
			WAVE_FORMAT_ZYXEL_ADPCM = 0x0097, /* ZyXEL ADPCM */
			WAVE_FORMAT_PHILIPS_LPCBB = 0x0098, /* Philips LPCBB */
			WAVE_FORMAT_PACKED = 0x0099, /* Packed */
			WAVE_FORMAT_RHETOREX_ADPCM = 0x0100,  /*  Rhetorex Inc  */
			WAVE_FORMAT_IRAT = 0x0101, /* BeCubed Software's IRAT */
			WAVE_FORMAT_VIVO_G723 = 0x0111, /* Vivo G.723 */
			WAVE_FORMAT_VIVO_SIREN = 0x0112, /* Vivo Siren */
			WAVE_FORMAT_DIGITAL_G723 = 0x0123, /* Digital G.723 */
			WAVE_FORMAT_CREATIVE_ADPCM = 0x0200,  /*  Creative Labs, Inc  */
			WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202,  /*  Creative Labs, Inc  */
			WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203,  /*  Creative Labs, Inc  */
			WAVE_FORMAT_QUARTERDECK = 0x0220, /*  Quarterdeck Corporation  */
			WAVE_FORMAT_FM_TOWNS_SND = 0x0300,  /*  Fujitsu Corp.  */
			WAVE_FORMAT_BTV_DIGITAL = 0x0400,  /*  Brooktree Corporation  */
			WAVE_FORMAT_VME_VMPCM = 0x0680, /* VME VMPCM */
			WAVE_FORMAT_OLIGSM = 0x1000,  /*  Ing C. Olivetti & C., S.p.A.  */
			WAVE_FORMAT_OLIADPCM = 0x1001,  /*  Ing C. Olivetti & C., S.p.A.  */
			WAVE_FORMAT_OLICELP = 0x1002,  /*  Ing C. Olivetti & C., S.p.A.  */
			WAVE_FORMAT_OLISBC = 0x1003,  /*  Ing C. Olivetti & C., S.p.A.  */
			WAVE_FORMAT_OLIOPR = 0x1004,  /*  Ing C. Olivetti & C., S.p.A.  */
			WAVE_FORMAT_LH_CODEC = 0x1100,  /*  Lernout & Hauspie  */
			WAVE_FORMAT_NORRIS = 0x1400,  /*  Norris Communications, Inc.  */
			WAVE_FORMAT_ISIAUDIO2 = 0x1401, /* ISIAudio */
			WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500, /* Soundspace Music Compression */
			WAVE_FORMAT_DVM = 0x2000, /* DVM */
			//  the WAVE_FORMAT_DEVELOPMENT format tag can be used during the
			//  development phase of a new wave format.  Before shipping, you MUST
			//  acquire an official format tag from Microsoft.
			WAVE_FORMAT_DEVELOPMENT = 0xFFFF
		}

		public enum AviStreamFormatType
		{
			Unknown,
			Video,
			Audio
		}

		public new enum Attribute
		{
			// Video
			Size,
			Width,
			Height,
			Planes,
			BitCount,
			Compression,
			SizeImage,
			XPelsPerMeter,
			YPelsPerMeter,
			ClrUsed,
			ClrImportant,
			ExtraDataSize, // optional, but required for H.264

			// Audio
			FormatTag,
			Channels,
			SamplesPerSec,
			AvgBytesPerSec,
			BlockAlign,
			BitsPerSample,
			ExtraSize,
			// WAVE_FORMAT_MPEGLAYER3 attributes
			Id,
			Flags,
			BlockSize,
			FramesPerBlock,
			CodecDelay
		}

		private readonly AviStreamFormatType _aviStreamFormatType = AviStreamFormatType.Unknown;

		public short FormatTagValue { get; private set;}	// audio property
		public uint Compression { get; private set; }		// video property
		public IDataPacket ExtraData { get; private set; }

		public AviStreamFormat(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.AviStreamFormat)
		{
			AviStreamHeader aviStreamHeader = previousHeader as AviStreamHeader;
			if (aviStreamHeader != null)
			{
				if (aviStreamHeader.StreamType == (uint)StreamType.Video)
				{
					_aviStreamFormatType = AviStreamFormatType.Video;
				}
				if (aviStreamHeader.StreamType == (uint)StreamType.Audio)
				{
					_aviStreamFormatType = AviStreamFormatType.Audio;
				}
			}
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			switch(_aviStreamFormatType)
			{
				case AviStreamFormatType.Video:
					parser.GetInt(Attribute.Size);
					parser.GetInt(Attribute.Width);
					parser.GetInt(Attribute.Height);
					parser.GetShort(Attribute.Planes);
					parser.GetShort(Attribute.BitCount);
					Compression = parser.GetFourCC(Attribute.Compression);
					parser.GetInt(Attribute.SizeImage);
					parser.GetInt(Attribute.XPelsPerMeter);
					parser.GetInt(Attribute.YPelsPerMeter);
					parser.GetInt(Attribute.ClrUsed);
					parser.GetInt(Attribute.ClrImportant);

					ulong extraDataSize = parser.BytesRemaining;
					if ((extraDataSize >= 17) && (extraDataSize < 256))
					{
						ExtraData = parser.GetDataPacket(parser.Position, (int)extraDataSize);
						Attributes.Add(new FormattedAttribute<Attribute, ulong>(Attribute.ExtraDataSize, extraDataSize));
					}
					break;

				case AviStreamFormatType.Audio:
					FormatTagValue = parser.GetShort(Attribute.FormatTag, typeof(FormatTag));
					parser.GetShort(Attribute.Channels);
					parser.GetInt(Attribute.SamplesPerSec);
					parser.GetInt(Attribute.AvgBytesPerSec);
					parser.GetShort(Attribute.BlockAlign);
					parser.GetShort(Attribute.BitsPerSample);
					if (FormatTagValue != (short)FormatTag.WAVE_FORMAT_PCM &&
						parser.BytesRemaining > 0)
					{
						parser.GetShort(Attribute.ExtraSize);
					}

					// TODO parse the other audio formats
					switch (FormatTagValue)
					{
						case (short)FormatTag.WAVE_FORMAT_MPEGLAYER3:
							if (parser.BytesRemaining > 0)
							{
								parser.GetShort(Attribute.Id);
								parser.GetInt(Attribute.Flags); // TODO Parse the flags
								parser.GetShort(Attribute.BlockSize);
								parser.GetShort(Attribute.FramesPerBlock);
								// Some file I found did not have this last field
								if (parser.BytesRemaining > 0)
								{
									parser.GetShort(Attribute.CodecDelay);
								}
							}
							break;
					}
					break;

				default:
					break;
			}
			return Valid;
		}
	}
}
