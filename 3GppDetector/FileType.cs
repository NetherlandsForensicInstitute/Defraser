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

using System.Collections.Generic;
using System.Text;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Identifies the file type. [ftyp]
	/// </summary>
	// TODO file type is not displayed for file 'MP4Video-05 Flow Layout Panel - CS-1.mp4'
	internal class FileType : QtAtom
	{
		public new enum Attribute
		{
			MajorBrand,
			MinorVersion,
			CompatibleBrands
		};

		/// <summary>
		/// Known brands.
		/// </summary>
		public enum Brand : uint
		{
			/// <summary>Apple QuickTime.</summary>
			qt__ = 0x71742020,
			/// <summary>Sony / Mobile QuickTime.</summary>
			mqt_ = 0x6D717420,
			/// <summary>3GPP Media (.3GP) Release 1.</summary>
			_3gp1 = 0x33677031,
			/// <summary>3GPP Media (.3GP) Release 2.</summary>
			_3gp2 = 0x33677032,
			/// <summary>3GPP Media (.3GP) Release 3.</summary>
			_3gp3 = 0x33677033,
			/// <summary>3GPP Media (.3GP) Release 4.</summary>
			_3gp4 = 0x33677034,
			/// <summary>3GPP Media (.3GP) Release 5.</summary>
			_3gp5 = 0x33677035,
			/// <summary>3GPP Media (.3GP) Release 6.</summary>
			_3gp6 = 0x33677036,
			/// <summary>3GPP2 Media (.3G2) compliant with 3GPP2 C.S0050-0 V1.0.</summary>
			_3g2a = 0x33673261,
            /// <summary>3GPP2 Media (.3G2) compliant with 3GPP2 C.S0050-A V1.0.0</summary>
			_3g2b = 0x33673262,
            /// <summary>3GPP2 Media (.3G2) compliant with 3GPP2 C.S0050-B V1.0.</summary>
			_3g2c = 0x33673263,
            /// <summary>3GPP Media (.3GP) Release 6 MBMS Extended Presentations</summary>
			_3ge6 = 0x33676536,
			/// <summary>3GPP Media (.3GP) Release 7 MBMS Extended Presentations</summary>
			_3ge7 = 0x33676537,
            /// <summary>3GPP Release 6 General Profile</summary>
			_3gg6 = 0x33676736,
			/// <summary>3GPP Media (.3GP) Release 7 Streaming Servers</summary>
			_3gs7 = 0x33677337,
			/// <summary>MPEG-4/3GPP Mobile Profile.</summary>
			mmp4 = 0x6D6D7034,
			/// <summary>MP4 Base Media v1 [IS0 14496-12:2003].</summary>
			isom = 0x69736F6D,
			/// <summary>MP4 Base Media v2 [ISO 14496-12:2005].</summary>
			iso2 = 0x69736F32,
			/// <summary>MP4 v1 [ISO 14496-1:ch13].</summary>
			mp41 = 0x6D703431,
			/// <summary>MP4 v2 [ISO 14496-14].</summary>
			mp42 = 0x6D703432,

			/// <summary>MP4 Base w/ AVC ext [ISO 14496-12:2005],</summary>
			avc1 = 0x61766331,
			/// <summary>Video for Adobe Flash Player 9+ (.F4V).</summary>
			F4V = 0x46345620,
			/// <summary>Protected Video for Adobe Flash Player 9+ (.F4P).</summary>
			F4P = 0x46345020,
			/// <summary>KDDI 3GPP2 EZmovie for KDDI 3G cellphones.</summary>
			KDDI = 0x4B444449,
			/// <summary>MP4 v2 [ISO 14496-14].</summary>
			M4V = 0x4D345620,
			/// <summary>M4VH Apple TV (.M4V).</summary>
			M4VH = 0x4D345648,
			/// <summary>M4VP Apple iPhone (.M4V).</summary>
			M4VP = 0x4D345650,
			/// <summary>MPEG-4 (.MP4) Nero Cinema Profile.</summary>
			NDSC = 0x4E445343,
			/// <summary>MPEG-4 (.MP4) Nero HDTV Profile.</summary>
			NDSH = 0x4E445348,
			/// <summary>MPEG-4 (.MP4) Nero Mobile Profile.</summary>
			NDSM = 0x4E44534D,
			/// <summary>MPEG-4 (.MP4) Nero Portable Profile.</summary>
			NDSP = 0x4E445350,
			/// <summary>MPEG-4 (.MP4) Nero Standard Profile.</summary>
			NDSS = 0x4E445353,
			/// <summary>H.264/MPEG-4 AVC (.MP4) Nero Cinema Profile.</summary>
			NDXC = 0x4E445843,
			/// <summary>H.264/MPEG-4 AVC (.MP4) Nero HDTV Profile.</summary>
			NDXH = 0x4E445848,
			/// <summary>H.264/MPEG-4 AVC (.MP4) Nero Mobile Profile.</summary>
			NDXM = 0x4E44584D,
			/// <summary>H.264/MPEG-4 AVC (.MP4) Nero Portable Profile.</summary>
			NDXP = 0x4E445850,
			/// <summary>H.264/MPEG-4 AVC (.MP4) Nero Standard Profile.</summary>
			NDXS = 0x4E445853,

//CAEP Canon Digital Camera Canon YES unknown   
//caqv Casio Digital Camera Casio YES unknown   
//CDes Convergent Design Convergent Design YES unknown   
//da0a DMB MAF w/ MPEG Layer II aud, MOT slides, DLS, JPG/PNG/MNG images ISO YES unknown [13] 
//da0b DMB MAF, extending DA0A, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//da1a DMB MAF audio with ER-BSAC audio, JPG/PNG/MNG images ISO YES unknown [13] 
//da1b DMB MAF, extending da1a, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//da2a DMB MAF aud w/ HE-AAC v2 aud, MOT slides, DLS, JPG/PNG/MNG images ISO YES unknown [13] 
//da2b DMB MAF, extending da2a, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//da3a DMB MAF aud with HE-AAC aud, JPG/PNG/MNG images ISO YES unknown [13] 
//da3b DMB MAF, extending da3a w/ BIFS, 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//dmb1 DMB MAF supporting all the components defined in the specification ISO YES unknown [13] 
//dmpf Digital Media Project DMP NO various [18] 
//drc1 Dirac (wavelet compression), encapsulated in ISO base media (MP4) BBC / Dirac NO unknown [20] 
//dv1a DMB MAF vid w/ AVC vid, ER-BSAC aud, BIFS, JPG/PNG/MNG images, TS ISO YES unknown [13] 
//dv1b DMB MAF, extending dv1a, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//dv2a DMB MAF vid w/ AVC vid, HE-AAC v2 aud, BIFS, JPG/PNG/MNG images, TS ISO YES unknown [13] 
//dv2b DMB MAF, extending dv2a, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//dv3a DMB MAF vid w/ AVC vid, HE-AAC aud, BIFS, JPG/PNG/MNG images, TS ISO YES unknown [13] 
//dv3b DMB MAF, extending dv3a, with 3GPP timed text, DID, TVA, REL, IPMP ISO YES unknown [13] 
//dvr1 DVB (.DVB) over RTP DVB YES video/vnd.dvb.file [12] 
//dvt1 DVB (.DVB) over MPEG-2 Transport Stream DVB YES video/vnd.dvb.file [12] 
//F4A Audio for Adobe Flash Player 9+ (.F4A) Adobe NO audio/mp4   
//F4B Audio Book for Adobe Flash Player 9+ (.F4B) Adobe NO audio/mp4   
//isc2 ISMACryp 2.0 Encrypted File ISMA YES ?/enc-isoff-generic   
//JP2 JPEG 2000 Image (.JP2) [ISO 15444-1 ?] ISO NO image/jp2   
//JP20 Unknown, from GPAC samples (prob non-existent) GPAC NO unknown [4] 
//jpm JPEG 2000 Compound Image (.JPM) [ISO 15444-6] ISO NO image/jpm [17] 
//jpx JPEG 2000 w/ extensions (.JPX) [ISO 15444-2] ISO NO image/jpx   
//M4A  Apple iTunes AAC-LC (.M4A) Audio Apple YES audio/x-m4a [9] 
//M4B  Apple iTunes AAC-LC (.M4B) Audio Book Apple YES audio/mp4 [9] 
//M4P  Apple iTunes AAC-LC (.M4P) AES Protected Audio Apple YES audio/mp4 [9] 
//mj2s Motion JPEG 2000 [ISO 15444-3] Simple Profile ISO YES video/mj2  
//mjp2 Motion JPEG 2000 [ISO 15444-3] General Profile ISO YES video/mj2  
//mp21 MPEG-21 [ISO/IEC 21000-9] ISO YES various  
//mp71 MP4 w/ MPEG-7 Metadata [per ISO 14496-12] ISO NO various  
//MPPI Photo Player, MAF [ISO/IEC 23000-3] ISO YES various  
//mqt Sony / Mobile QuickTime (.MQV)  US Patent 7,477,830 (Sony Corp) Sony / Apple NO video/quicktime  
//MSNV MPEG-4 (.MP4) for SonyPSP Sony NO audio/mp4   
//NDAS MP4 v2 [ISO 14496-14] Nero Digital AAC Audio Nero NO audio/mp4 [8] 
//odcf   OMA DCF DRM Format 2.0 (OMA-TS-DRM-DCF-V2_0-20060303-A) Open Mobile Alliance YES various  
//opf2  OMA PDCF DRM Format 2.1 (OMA-TS-DRM-DCF-V2_1-20070724-C) Open Mobile Alliance YES unknown  
//opx2   OMA PDCF DRM + XBS extensions (OMA-TS-DRM_XBS-V1_0-20070529-C) Open Mobile Alliance YES unknown  
//pana Panasonic Digital Camera Pansonic YES unknown   
//ROSS Ross Video Ross Video YES unknown   
//sdv SD Memory Card Video SD Card Association YES various?   
//ssc1 Samsung stereoscopic, single stream (patent pending, see notes) Samsung NO unknown [19] 
//ssc2 Samsung stereoscopic, dual stream (patent pending, see notes) Samsung NO unknown [19] 
		}

		public FileType(QtAtom previousHeader)
			: base(previousHeader, AtomName.FileType)
		{
		}

		public override bool Parse(QtParser parser)
		{
			uint maxUnparsedBytes = (uint)QtDetector.Configurable[QtDetector.ConfigurationKey.FileTypeMaxUnparsedBytes];
			if (!base.Parse(parser) || parser.BytesRemaining > maxUnparsedBytes) return false;

			Brand majorBrand = (Brand)parser.GetFourCC(Attribute.MajorBrand);
			parser.GetUInt(Attribute.MinorVersion, "{0:X8}");

			// Gets the list of compatible brands
			StringBuilder sb = new StringBuilder();
			List<Brand> compatibleBrands = new List<Brand>();

			// Remaining data of the atom are the compatible brands
			while (parser.BytesRemaining >= 4)
			{
				uint compatibleBrand = parser.GetUInt();

				if (compatibleBrand != 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					// Add 4CC
					sb.Append(compatibleBrand.ToString4CC());
					compatibleBrands.Add((Brand)compatibleBrand);
				}
			}

			// Create attribute with comma separated list of compatible brands
			parser.AddAttribute(new FormattedAttribute<Attribute, string>(Attribute.CompatibleBrands, sb.ToString()));

			_dataFormat = GetDataFormat((Brand)majorBrand);
			if (DataFormat == CodecID.Unknown)
			{
				// Scan compatible brands for known brand
				foreach (Brand brand in compatibleBrands)
				{
					_dataFormat = GetDataFormat(brand);

					if (DataFormat != CodecID.Unknown)
					{
						break;
					}
				}
			}

			// Unsupported formats, such as JPEG-2000, should be ignored
			parser.CheckAttribute(Attribute.MajorBrand, DataFormat != CodecID.Unknown);

			// We expect the major brand in the list of compatible brands
			parser.CheckAttribute(Attribute.CompatibleBrands, compatibleBrands.Contains(majorBrand), false);
			return this.Valid;
		}

		public override bool IsSuitableParent(QtAtom parent)
		{
			// File type should always be the first atom
			return parent.IsRoot && !parent.HasChildren();
		}

		/// <summary>
		/// Returns the data format for the given <paramref name="brand"/>.
		/// </summary>
		/// <param name="brand">the major or compatible brand</param>
		/// <returns>the data format for the given brand</returns>
		private CodecID GetDataFormat(Brand brand)
		{
			switch (brand)
			{
				case Brand.qt__:
				case Brand.mqt_:
					return CodecID.QuickTime;
				case Brand._3gp1:
				case Brand._3gp2:
				case Brand._3gp3:
				case Brand._3gp4:
				case Brand._3gp5:
				case Brand._3gp6:
				case Brand._3g2a:
				case Brand._3g2b:
				case Brand._3g2c:
				case Brand._3ge6:
				case Brand._3ge7:
				case Brand._3gg6:
				case Brand._3gs7:
				case Brand.mmp4:
				case Brand.KDDI:
					return CodecID.Itu3Gpp;
				case Brand.isom:
				case Brand.iso2:
				case Brand.mp41:
				case Brand.mp42:
				case Brand.avc1:
				case Brand.F4V:
				case Brand.F4P:
				case Brand.M4V:
				case Brand.M4VH:
				case Brand.M4VP:
				case Brand.NDSC:
				case Brand.NDSH:
				case Brand.NDSM:
				case Brand.NDSP:
				case Brand.NDSS:
				case Brand.NDXC:
				case Brand.NDXH:
				case Brand.NDXM:
				case Brand.NDXP:
				case Brand.NDXS:
					return CodecID.Mpeg4System;
				default:
					return CodecID.Unknown;
			}
		}
	}
}
