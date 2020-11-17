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
using System.Diagnostics;
using System.Reflection;
using Defraser.Detector.Common;

namespace Defraser.Detector.Avi
{
	internal enum ListType
	{
		AviHeaderList = 0x68646C72,	// hdlr
		AviStreamHeader = 0x7374726C,	// strl
		Movie = 0x6D6F7669,	// movi
		Info = 0x494E464F,	// INFO
		Prmi = 0x50524D49	// PRMI
	}

	internal class HeaderList : AviChunk
	{
		#region Inner class
		internal sealed class FourCCAttribute : System.Attribute
		{
			public uint FourCC { get; private set; }

			public FourCCAttribute(string fourCC)
			{
				FourCC = fourCC.To4CC();
			}
		}

		private class InfoAttribute : CompositeAttribute<Attribute, string, AviParser>
		{
			private readonly ulong _bytesRemaining;
			private const int SizeOfTypeAndSize = 8;

			public enum LAttribute
			{
				Type,
				Size,
				Description
			}

			public InfoAttribute(ulong bytesRemaining)
				: base(Attribute.Info, string.Empty, "{0}")
			{
				_bytesRemaining = bytesRemaining;
			}

			public override bool Parse(AviParser parser)
			{
				uint type = parser.GetFourCCWithOptionalDescriptionAttribute(LAttribute.Type, _infoDescriptionNameForInfoDescriptionType);
				uint size = (uint)parser.GetInt(LAttribute.Size);

				uint maxDescriptionLength = (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.HeaderListMaxDescriptionLength];
				if (size > maxDescriptionLength) return false;	// Sanity check
				string description = parser.GetString(LAttribute.Description, size);

				// Sometimes there is an extra '0' at the end of a string.
				// This extra '0' is not taken into account in the info string length.
				if (_bytesRemaining - (SizeOfTypeAndSize + size) > 0)
				{
					if (parser.GetByte() != 0)
					{
						parser.Position--;
					}
				}

				if (_infoDescriptionNameForInfoDescriptionType.ContainsKey(type))
				{
					TypedValue = string.Format("{0}: {1}", _infoDescriptionNameForInfoDescriptionType[type], description);
				}
				else
				{
					if(type == 0)
						TypedValue = string.Format("{0}", description);
					else
						TypedValue = string.Format("{0}: {1}", type.ToString4CC(), description);
				}

				return Valid;
			}
		}
		#endregion Inner class

		public new enum Attribute
		{
			ListType,
			Unknown,
			Info,
			AviAdobePremiereInfoList,
		}

		enum InfoDescription
		{
			/// <summary>
			/// Indicates where the subject of the file is archived.
			/// </summary>
			[FourCC("IARL")]
			ArchivalLocation,
			/// <summary>
			/// Lists the artist of the original subject of the file; for example,
			/// “Michaelangelo.”
			/// </summary>
			[FourCC("IART")]
			Artist,
			/// <summary>
			/// Lists the name of the person or organization that
			/// commissioned the subject of the file; for example, “Pope Julian II.”
			/// </summary>
			[FourCC("ICMS")]
			Commissioned,
			/// <summary>
			/// Provides general comments about the file or the subject of
			/// the file. If the comment is several sentences long, end each sentence with
			/// a period. Do not include new-line characters.
			/// </summary>
			[FourCC("ICMT")]
			Comments,
			/// <summary>
			/// Records the copyright information for the file; for example,
			/// “Copyright Encyclopedia International 1991.” If there are multiple
			/// copyrights, separate them by a semicolon followed by a space.
			/// </summary>
			[FourCC("ICOP")]
			Copyright,
			/// <summary>
			/// Specifies the date the subject of the file was created. List
			/// dates in year-month-day format, padding one-digit months and days with
			/// a zero on the left; for example, “1553-05-03” for May 3, 1553.
			/// </summary>
			[FourCC("ICRD")]
			CreationDate,
			/// <summary>
			/// Describes whether an image has been cropped and, if so, how
			/// it was cropped; for example, “lower-right corner.”
			/// </summary>
			[FourCC("ICRP")]
			Cropped,
			/// <summary>
			/// Specifies the size of the original subject of the file; for
			/// example, “8.5 in h, 11 in w.”
			/// </summary>
			[FourCC("IDIM")]
			Dimensions,
			/// <summary>
			/// Stores dots per inch setting of the digitizer used to
			/// produce the file, such as “300.”
			/// </summary>
			[FourCC("IDPI")]
			DotsPerInch,
			/// <summary>
			/// Stores the name of the engineer who worked on the file. If
			/// there are multiple engineers, separate the names by a semicolon and a
			/// blank; for example, “Smith, John; Adams, Joe.”
			/// </summary>
			[FourCC("IENG")]
			Engineer,
			/// <summary>
			/// Describes the original work, such as “landscape,” “portrait,”
			/// “still life,” etc.
			/// </summary>
			[FourCC("IGNR")]
			Genre,
			/// <summary>
			/// Provides a list of keywords that refer to the file or subject of
			/// the file. Separate multiple keywords with a semicolon and a blank; for
			/// example, “Seattle; aerial view; scenery.”
			/// </summary>
			[FourCC("IKEY")]
			Keywords,
			/// <summary>
			/// Describes the changes in lightness settings on the digitizer
			/// required to produce the file. Note that the format of this information
			/// depends on hardware used.
			/// </summary>
			[FourCC("ILGT")]
			Lightness,
			/// <summary>
			/// Describes the original subject of the file, such as “computer
			/// image,” “drawing,” “lithograph,” and so on.
			/// </summary>
			[FourCC("IMED")]
			Medium,
			/// <summary>
			/// Stores the title of the subject of the file, such as “Seattle From
			/// Above.”
			/// </summary>
			[FourCC("INAM")]
			Name,
			/// <summary>
			/// Specifies the number of colors requested when digitizing
			/// an image, such as “256.”
			/// </summary>
			[FourCC("IPLT")]
			PaletteSetting,
			/// <summary>
			/// Specifies the name of the title the file was originally intended
			/// for, such as “Encyclopedia of Pacific Northwest Geography.”
			/// </summary>
			[FourCC("IPRD")]
			Product,
			/// <summary>
			/// Describes the contents of the file, such as “Aerial view of
			/// Seattle.”
			/// </summary>
			[FourCC("ISBJ")]
			Subject,
			/// <summary>
			/// Identifies the name of the software package used to create the
			/// file, such as “Microsoft WaveEdit.”
			/// </summary>
			[FourCC("ISFT")]
			Software,
			/// <summary>
			/// Identifies the changes in sharpness for the digitizer required
			/// to produce the file (the format depends on the hardware used).
			/// </summary>
			[FourCC("ISHP")]
			Sharpness,
			/// <summary>
			/// Identifies the name of the person or organization who supplied
			/// the original subject of the file; for example, “Trey Research.”
			/// </summary>
			[FourCC("ISRC")]
			Source,
			/// <summary>
			/// Identifies the original form of the material that was
			/// digitized, such as “slide,” “paper,” “map,” and so on. This is not
			/// necessarily the same as IMED.
			/// </summary>
			[FourCC("ISRF")]
			SourceForm,
			/// <summary>
			/// Identifies the technician who digitized the subject file; for
			/// example, “Smith, John.”
			/// </summary>
			[FourCC("ITCH")]
			Technician
		}

		public uint LstType { get; private set; }
		private static readonly Dictionary<uint, InfoDescription> _infoDescriptionNameForInfoDescriptionType;

		public HeaderList(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.HeaderList)
		{
		}

		static HeaderList()
		{
			Type infoDescriptionType = typeof(InfoDescription);

			_infoDescriptionNameForInfoDescriptionType = new Dictionary<uint, InfoDescription>();

			// Use reflection to find the attributes describing the codec identifiers
			foreach (InfoDescription infoDescription in Enum.GetValues(infoDescriptionType))
			{
				string infoDescriptionName = Enum.GetName(infoDescriptionType, infoDescription);

				FieldInfo fieldInfo = infoDescriptionType.GetField(infoDescriptionName);
				FourCCAttribute[] attributes = (FourCCAttribute[])fieldInfo.GetCustomAttributes(typeof(FourCCAttribute), false);

				if(attributes != null && attributes.Length == 1)
				{
					uint infoType = attributes[0].FourCC;
					Debug.Assert(!_infoDescriptionNameForInfoDescriptionType.ContainsKey(infoType), string.Format("Duplicate 4CC '{0}'", infoType.ToString4CC()));
					_infoDescriptionNameForInfoDescriptionType.Add(infoType, infoDescription);
				}
				else
				{
					Debug.Fail(string.Format("No attributes for {0}. Please add attributes to the ChunkName enumeration.", infoDescription));
				}
			}
		}

		public override bool Parse(AviParser parser)
		{
			// Call the Parse method of the parent to initialise the header and
			// allow parsing the containing header if there is one.
			if (!base.Parse(parser)) return false;

			// Now parse the header.
			LstType = parser.GetFourCC(Attribute.ListType);

			if(LstType == (uint)ListType.Info)
			{
				do
				{
					Valid = parser.Parse(new InfoAttribute(parser.BytesRemaining));
				}
				while (parser.BytesRemaining > 0 && Valid);
			}
			else if(LstType == (uint)ListType.Prmi)
			{
				uint maxPrmiLength = (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.HeaderListMaxPrmiLength];
				parser.GetInt(Attribute.Unknown);
				parser.GetInt(Attribute.Unknown);
				parser.GetInt(Attribute.Unknown);
				if (parser.BytesRemaining > maxPrmiLength) return false;	// Sanity check
				parser.GetString(Attribute.AviAdobePremiereInfoList, parser.BytesRemaining);
			}
			return Valid;
		}
	}
}
