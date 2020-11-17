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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Defraser.Util;

namespace Defraser.Interface
{
	/// <summary>
	/// Specifies the codec or data format.
	/// Used for the detectors implemented for Defraser
	/// </summary>
	public enum CodecID
	{
		Unknown,

		// Video formats
		[CodecDescription("MPEG-1 Video", ".mpv")]
		Mpeg1Video,
		[CodecDescription("MPEG-2 Video", ".m2v")]
		Mpeg2Video,
		[CodecDescription("MPEG-4 Video", ".m4v")]
		Mpeg4Video,
		[CodecDescription("H.263", ".h263")]
		H263,
		[CodecDescription("H.264", ".h264")]
		H264,
		[CodecDescription("H.265", ".h265")]
		H265,

		// Image formats
		[CodecDescription("JPEG", ".jpg")]
		Jpeg,
		[CodecDescription("TIFF", ".tif")]
		Tiff,

		// Container formats
		[CodecDescription("MPEG-1 System", ".mpg")]
		Mpeg1System,
		[CodecDescription("MPEG-2 System", ".mpg")]
		Mpeg2System,
		[CodecDescription("MPEG Transport Stream", ".m2ts")]
		MpegTs,
		[CodecDescription("MPEG-4 System", ".mp4")]
		Mpeg4System,
		[CodecDescription("QuickTime", ".mov")]
		QuickTime,
		[CodecDescription("ITU 3GPP", ".3gp")]
		Itu3Gpp,
		// Note: AVI is the RIFF format that has been fully covered by the AVI Detector
		// The other RIFF format are found by the AVI detector but not fully implemented
		[CodecDescription("AVI", ".avi")]
		Avi,
		[CodecDescription("RIFF(WAVE)", ".wav")]
		Wave,
		[CodecDescription("RIFF(ACON)", ".ani")]
		Acon,
		[CodecDescription("RIFF(CDR6)", ".cdr")]
		Cdr6,

		[CodecDescription("Matroska", ".mkv")]
		Matroska,
		[CodecDescription("ASF/WMV", ".asf")]
		Asf,
	}

	// Used to display more verbose information about a codec 4CC
	public enum CodecInformation
	{
		// Video formats
		[CodecDescription("DEC H.263")]
		D263,
		[CodecDescription("LEAD Technologies H.263")]
		L263,
		[CodecDescription("Microsoft H.263")]
		M263,
		[CodecDescription("H.263")]
		S263,
		[CodecDescription("H.263")]
		T263,
		[CodecDescription("H.263")]
		U263,
		[CodecDescription("Xirlink H.263")]
		X263,
		[CodecDescription("MPEG-4 ASP video")]
		Xvid,

		//----------------------------------------
		[CodecDescription("MPEG4-based codec 3ivx")]
		ThreeIv0,	// 3IV0
		[CodecDescription("MPEG4-based codec 3ivx")]
		ThreeIv1,	// 3IV1
		[CodecDescription("MPEG4-based codec 3ivx")]
		ThreeIv2,	// 3IV2
		[CodecDescription("FFmpeg DivX ;-) (MS MPEG-4 v3)")]
		ThreeIvd,	// 3IVD
		[CodecDescription("MPEG4-based codec 3ivx")]
		ThreeIvx,	// 3IVX
		[CodecDescription("Autodesk Animator codec (RLE)")]
		Aas4,
		[CodecDescription("Autodesk Animator codec (RLE)")]
		Aasc,
		[CodecDescription("Kensington codec")]
		Abyr,
		[CodecDescription("Loronix WaveCodec (used in various CCTV products)")]
		Adv1,
		[CodecDescription("Avid M-JPEG Avid Technology (also known as AVRn)")]
		Advj,
		[CodecDescription("Array VideoONE MPEG1-I Capture")]
		Aemi,
		[CodecDescription("Autodesk Animator FLC (256 color)")]
		Aflc,
		[CodecDescription("Autodesk Animator FLI (256 color)")]
		Afli,
		[CodecDescription("Array VideoONE MPEG")]
		Ampg,
		[CodecDescription("Intel - RDX")]
		Anim,
		[CodecDescription("AngelPotion Definitive (hack MS MP43)")]
		Ap41,
		[CodecDescription("Asus Video V1")]
		Asv1,
		[CodecDescription("Asus Video V2")]
		Asv2,
		[CodecDescription("Asus Video 2.0")]
		Asvx,
		[CodecDescription("AuraVision - Aura 2 Codec - YUV 422")]
		Aur2,
		[CodecDescription("AuraVision - Aura 1 Codec - YUV 411")]
		Aura,
		[CodecDescription("Avid Motion JPEG")]
		Avdj,
		[CodecDescription("MainConcept Motion JPEG Codec")]
		Avi1,
		[CodecDescription("MainConcept Motion JPEG Codec")]
		Avi2,
		[CodecDescription("Avid Motion JPEG (also known as ADVJ)")]
		Avrn,
		[CodecDescription("Quicktime Apple Video")]
		Azpr,
		//[CodecDescription("Uncompressed BGR32 8:8:8:8")]
		//BGR
		//[CodecDescription("Uncompressed BGR15 5:5:5")]
		//BGR(15)
		//[CodecDescription("Uncompressed BGR16 5:6:5")]
		//BGR(16)
		//[CodecDescription("Uncompressed BGR24 8:8:8")]
		//BGR(24)
		[CodecDescription("Bink Video (RAD Game Tools) (256 color)")]
		Bink,
		[CodecDescription("Microsoft H.261")]
		Bitm,
		[CodecDescription("FFmpeg MPEG-4")]
		Blz0,
		[CodecDescription("Conexant (ex Brooktree) - MediaStream codec")]
		Bt20,
		[CodecDescription("Conexant (ex Brooktree) - Composite Video codec")]
		Btcv,
		[CodecDescription("Conexant (ex Brooktree) - Composite Video codec")]
		Btvc,
		[CodecDescription("Data Translation Broadway MPEG Capture/Compression")]
		Bw10,
		[CodecDescription("Intel - YUV12 codec")]
		CC12,
		[CodecDescription("Canopus - DV codec")]
		Cdvc,
		[CodecDescription("Conkrete DPS Perception Motion JPEG")]
		Cfcc,
		[CodecDescription("Camcorder Video (MS Office 97)")]
		Cgdi,
		[CodecDescription("Winnov, Inc. - MM_WINNOV_CAVIARA_CHAMPAGNE")]
		Cham,
		[CodecDescription("Creative Video Blaster Webcam Go JPEG")]
		Cjpg,
		[CodecDescription("Cirrus Logic YUV 4:1:1")]
		Cljr,
		[CodecDescription("Format similar to YV12 but including a level of indirection.")]
		Clpl,
		[CodecDescription("Common Data Format in Printing")]
		Cmyk,
		[CodecDescription("FFmpeg DivX ;-) (MS MPEG-4 v3)")]
		Col0,
		[CodecDescription("FFmpeg DivX ;-) (MS MPEG-4 v3)")]
		Col1,
		[CodecDescription("Weitek - 4:2:0 YUV Planar")]
		Cpla,
		[CodecDescription("Microsoft Video 1")]
		Cram,
		[CodecDescription("Cinepak")]
		Cvid,
		[CodecDescription("Microsoft Color WLT DIB codec ")]
		Cwlt,
		[CodecDescription("Creative Labs YUV 4:2:2")]
		Cyuv,
		[CodecDescription("ATI Technologies YUV")]
		Cyuy,
		[CodecDescription("Duck Corp. - TrueMotion 1.0")]
		Duck,
		[CodecDescription("InSoft - DVE-2 Videoconferencing codec")]
		Dve2,
		[CodecDescription("S3 Texture Compression")]
		Dxt1,
		[CodecDescription("S3 Texture Compression")]
		Dxt2,
		[CodecDescription("S3 Texture Compression")]
		Dxt3,
		[CodecDescription("S3 Texture Compression")]
		Dxt4,
		[CodecDescription("S3 Texture Compression")]
		Dxt5,
		[CodecDescription("DirectX Texture Compression")]
		Dxtc,
		[CodecDescription("D-Vision - Field Encoded Motion JPEG With LSI Bitstream Format")]
		Fljp,
		[CodecDescription("Microsoft Greyscale WLT DIB")]
		Gwlt,
		[CodecDescription("Intel - Conferencing codec")]
		H260,
		[CodecDescription("Intel - Conferencing codec")]
		H261,
		[CodecDescription("Intel - Conferencing codec")]
		H262,
		[CodecDescription("Intel - Conferencing codec")]
		H263,
		[CodecDescription("Intel - Conferencing codec")]
		H264,
		[CodecDescription("Intel - Conferencing codec")]
		H265,
		[CodecDescription("Intel - Conferencing codec")]
		H266,
		[CodecDescription("Intel - Conferencing codec")]
		H267,
		[CodecDescription("Intel - Conferencing codec")]
		H268,
		[CodecDescription("Intel - Conferencing codec")]
		H269,
		[CodecDescription("Intel - I263")]
		I263,
		[CodecDescription("Intel - Indeo 4 codec")]
		I420,
		//[CodecDescription("Intel - RDX")]
		//IAN
		[CodecDescription("InSoft - CellB Videoconferencing codec")]
		Iclb,
		[CodecDescription("Intel - Layered Video")]
		Ilvc,
		[CodecDescription("ITU-T - H.263+ compression standard")]
		Ilvr,
		[CodecDescription("Intel - YUV uncompressed")]
		Iraw,
		[CodecDescription("Intel Indeo 2.1")]
		Ir21,
		[CodecDescription("Intel - Indeo Video 3.0 codec")]
		Iv30,
		[CodecDescription("Intel - Indeo Video 3.1 codec")]
		Iv31,
		[CodecDescription("Intel - Indeo Video 3.2 codec")]
		Iv32,
		[CodecDescription("Intel - Indeo Video 3.3 codec")]
		Iv33,
		[CodecDescription("Intel - Indeo Video 3.4 codec")]
		Iv34,
		[CodecDescription("Intel - Indeo Video 3.5 codec")]
		Iv35,
		[CodecDescription("Intel - Indeo Video 3.6 codec")]
		Iv36,
		[CodecDescription("Intel - Indeo Video 3.7 codec")]
		Iv37,
		[CodecDescription("Intel - Indeo Video 3.8 codec")]
		Iv38,
		[CodecDescription("Intel - Indeo Video 3.9 codec")]
		Iv39,
		[CodecDescription("Intel - Indeo Video 4.0 codec")]
		Iv40,
		[CodecDescription("Intel - Indeo Video 4.1 codec")]
		Iv41,
		[CodecDescription("Intel - Indeo Video 4.2 codec")]
		Iv42,
		[CodecDescription("Intel - Indeo Video 4.3 codec")]
		Iv43,
		[CodecDescription("Intel - Indeo Video 4.4 codec")]
		Iv44,
		[CodecDescription("Intel - Indeo Video 4.5 codec")]
		Iv45,
		[CodecDescription("Intel - Indeo Video 4.6 codec")]
		Iv46,
		[CodecDescription("Intel - Indeo Video 4.7 codec")]
		Iv47,
		[CodecDescription("Intel - Indeo Video 4.8 codec")]
		Iv48,
		[CodecDescription("Intel - Indeo Video 4.9 codec")]
		Iv49,
		[CodecDescription("Intel - Indeo Video 5.0")]
		Iv50,
		[CodecDescription("Chromatic - MPEG 1 Video I Frame")]
		Mpeg,
		[CodecDescription("Incompatible Microsoft MPEG-4 codec version 1")]
		Mpg4,
		[CodecDescription("Incompatible Microsoft MPEG-4 codec version 2")]
		Mp42,
		[CodecDescription("Incompatible Microsoft MPEG-4 codec version 3")]
		Mp43,
		[CodecDescription("MPEG-4 version 1 August 1999")]
		Mp4S,
		[CodecDescription("Microsoft MPEG-4 codec")]
		Mp4V,
		[CodecDescription("MPEG-4 version 2 ISO/IEC-14496")]
		Mps2,
		[CodecDescription("FAST Multimedia - Mrcodec")]
		Mrca,
		[CodecDescription("Microsoft - Run Length Encoding")]
		Mrle,
		[CodecDescription("Microsoft - Video 1")]
		Msvc,
		[CodecDescription("Nogatech - Video Compression 1")]
		Ntn1,
		[CodecDescription("Q-Team - QPEG 1.1 Format video codec")]
		Qpeq,
		[CodecDescription("Computer Concepts - 32 bit support")]
		Rgbt,
		[CodecDescription("Intel - Indeo 2.1 codec")]
		Rt21,
		//[CodecDescription("Intel - RDX")]
		//RVX
		[CodecDescription("Sun Communications - Digital Camera Codec")]
		Sdcc,
		[CodecDescription("Crystal Net - SFM Codec")]
		Sfmc,
		[CodecDescription("Radius - proprietary")]
		Smsc,
		[CodecDescription("Radius - proprietary")]
		Smsd,
		[CodecDescription("Splash Studios - ACM audio codec")]
		Splc,
		[CodecDescription("Microsoft - VXtreme Video Codec V2")]
		Sqz2,
		[CodecDescription("Sorenson - Video R1")]
		Sv10,
		[CodecDescription("TeraLogic - Motion Intraframe Codec")]
		Tlms,
		[CodecDescription("TeraLogic - Motion Intraframe Codec")]
		Tlst,
		[CodecDescription("Duck Corp. - TrueMotion 2.0")]
		Tm20,
		[CodecDescription("TeraLogic - Motion Intraframe Codec")]
		Tmic,
		[CodecDescription("Horizons Technology - TrueMotion Video Compression Algorithm")]
		Tmot,
		[CodecDescription("Duck Corp. - TrueMotion RT 2.0")]
		Tr20,
		[CodecDescription("Vitec Multimedia - 24 bit YUV 4:2:2 format (CCIR 601). For this format, 2 consecutive pixels are represented by a 32 bit (4 byte) Y1UY2V color value.")]
		V422,
		[CodecDescription("Vitec Multimedia - 16 bit YUV 4:2:2 format.")]
		V655,
		[CodecDescription("ATI - VCR 1.0")]
		Vcr1,
		[CodecDescription("Vivo - H.263 Video Codec")]
		Vivo,
		[CodecDescription("Miro Computer Products AG - for use with the Miro line of capture cards.")]
		Vixl,
		[CodecDescription("Videologic - VLCAP.DRV")]
		Vlv1,
		[CodecDescription("Winbond Electronics - W9960")]
		Wbvc,
		[CodecDescription("NetXL, Inc. - XL Video Decoder")]
		Xlv0,
		[CodecDescription("Intel - YUV12 codec")]
		Yc12,
		[CodecDescription("Winnov, Inc. - MM_WINNOV_CAVIAR_YUV8")]
		Yuv8,
		[CodecDescription("Intel - YUV9")]
		Yuv9,
		[CodecDescription("Canopus - YUYV compressor")]
		Yuyv,
		[CodecDescription("Metheus - Video Zipper")]
		Zpeg,

		// Audio formats
		[CodecDescription("AMR Speech")]
		Samr,
		[CodecDescription("MPEG Audio")]
		Mp3,
	}

	/// <summary>
	/// Provides methods for extended codec information.
	/// These methods act as <em>extension methods</em> for <c>CodecID</c>.
	/// </summary>
	public static class Codecs
	{
		#region Inner classes
		/// <summary>
		/// Describes a codec.
		/// </summary>
		private struct Description
		{
			private readonly string _name;
			private readonly string _descriptiveName;
			private readonly string _outputFileExtension;

			#region Properties
			/// <summary>The name for the codec.</summary>
			public string Name { get { return _name; } }
			/// <summary>The descriptive name for the codec.</summary>
			public string DescriptiveName { get { return _descriptiveName; } }
			/// <summary>The preferred output file extension for the codec.</summary>
			public string OutputFileExtension { get { return _outputFileExtension; } }
			#endregion Properties

			/// <summary>
			/// Creates a new description.
			/// </summary>
			/// <param name="name">the enumeration name</param>
			/// <param name="descriptiveName">the descriptive name </param>
			/// <param name="outputFileExtension">the preferred output file extension</param>
			public Description(string name, string descriptiveName, string outputFileExtension)
			{
				_name = name;
				_descriptiveName = descriptiveName;
				_outputFileExtension = outputFileExtension;
			}
		}
		#endregion Inner classes

		private static readonly Dictionary<string, CodecID> _codecs;
		private static readonly Dictionary<CodecID, Description> _descriptions;
		private static readonly Dictionary<string, string> _fourCCDescription;

		/// <summary>Static data initialization.</summary>
		static Codecs()
		{
			Type codeIDEnumType = typeof(CodecID);

			_codecs = new Dictionary<string, CodecID>();
			_descriptions = new Dictionary<CodecID, Description>();

			// Use reflection to find the attributes describing the codec identifiers
			foreach (CodecID codecID in Enum.GetValues(codeIDEnumType))
			{
				string name = Enum.GetName(codeIDEnumType, codecID);

				_codecs.Add(name, codecID);

				FieldInfo fieldInfo = codeIDEnumType.GetField(name);
				CodecDescriptionAttribute[] attributes = (CodecDescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(CodecDescriptionAttribute), false);

				if (attributes != null && attributes.Length > 0)
				{
					_descriptions.Add(codecID, new Description(name, attributes[0].DescriptiveName, attributes[0].OutputFileExtension));
				}
				else
				{
					_descriptions.Add(codecID, new Description(name, name, ".bin"));
				}
			}

			Type codeInformationEnumType = typeof(CodecInformation);

			_fourCCDescription = new Dictionary<string, string>();

			// Use reflection to find the attributes describing the codec 4CC's
			foreach (CodecInformation codecID in Enum.GetValues(codeInformationEnumType))
			{
				string name = Enum.GetName(codeInformationEnumType, codecID);

				FieldInfo fieldInfo = codeInformationEnumType.GetField(name);
				CodecDescriptionAttribute[] attributes = (CodecDescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(CodecDescriptionAttribute), false);

				if (name == "Mp3")
				{
					name = ".mp3";
				}
				if(name.StartsWith("Three"))
				{
					name = name.Replace("Three", "3");
				}

				_fourCCDescription.Add(name.ToLower(), attributes[0].DescriptiveName);
			}
		}

		/// <summary>
		/// Parses a codec identifier from an enum <paramref name="name"/>.
		/// Invalid or unsupported codecs are indicated by <c>CodecID.Unknown</c>.
		/// </summary>
		/// <param name="name">the name of the codec</param>
		/// <returns>the codec identifier or <c>CodecID.Unknown</c></returns>
		/// <seealso cref="GetName(CodecID)"/>
		public static CodecID Parse(string name)
		{
			CodecID codecID;
			return _codecs.TryGetValue(name, out codecID) ? codecID : CodecID.Unknown;
		}

		/// <summary>
		/// Gets the enum name for the given <paramref name="codecId"/>.
		/// </summary>
		/// <param name="codecId">the codec identifier</param>
		/// <returns>the enum name</returns>
		public static string GetName(this CodecID codecId)
		{
			return codecId.GetDescription().Name;
		}

		/// <summary>
		/// Extention method to get a descriptive name for the <paramref name="codecId"/>,
		/// suitable for presentation in a user interface.
		/// </summary>
		/// <param name="codecId">the codec identifier</param>
		/// <returns>the descriptive name</returns>
		public static string GetDescriptiveName(this CodecID codecId)
		{
			return codecId.GetDescription().DescriptiveName;
		}

		/// <summary>
		/// Append a descriptive name for the <paramref name="codec4CC"/>,
		/// suitable for presentation in a user interface.
		/// </summary>
		/// <param name="codecStreamName">the string builder to append the descriptive name to</param>
		/// <param name="codec4CC">the 4CC</param>
		/// <returns>the descriptive name</returns>
		public static void AppendDescriptiveCodecName(this StringBuilder codecStreamName, string codec4CC)
		{
			PreConditions.Argument("codecStreamName").Value(codecStreamName).IsNotNull();
			PreConditions.Argument("fourCC").Value(codec4CC).IsNotNull().And.IsNotEmpty();
			if (codec4CC.Length != 4) throw new ArgumentException(string.Format("The specified 4CC code ({0}) is not four characters long.", codec4CC), "codec4CC");

			string descriptiveCodecName;
			_fourCCDescription.TryGetValue(codec4CC.ToLower(), out descriptiveCodecName);

			if (!string.IsNullOrEmpty(descriptiveCodecName))
			{
				codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", descriptiveCodecName);
			}
		}

		/// <summary>
		/// Gets the preferred output file extension for <paramref name="codecId"/>.
		/// </summary>
		/// <param name="codecId">the codec identifier</param>
		/// <returns>the output file extension, starts with a dot (.)</returns>
		public static string GetOutputFileExtension(this CodecID codecId)
		{
			return codecId.GetDescription().OutputFileExtension;
		}

		/// <summary>
		/// Gets the description for the given <paramref name="codecId"/>.
		/// </summary>
		/// <param name="codecId">the codec identifier</param>
		/// <returns>the description for the given codec identifier</returns>
		/// <exception cref="ArgumentException">if the codec identifier is invalid</exception>
		private static Description GetDescription(this CodecID codecId)
		{
			Description description;
			if (!_descriptions.TryGetValue(codecId, out description))
			{
				throw new ArgumentException("Invalid codec.", "codecId");
			}
			return description;
		}
	}
}
