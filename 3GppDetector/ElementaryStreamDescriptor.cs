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
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Extension to the sample descriptions for MPEG-4 audio and video.
	/// </summary>
	/// <remarks>defined in ISO/IEC 14496-1</remarks>
	internal class ElementaryStreamDescriptor : QtAtom
	{
		public new enum Attribute
		{
			Version,
			ObjectTypeIndication,
			StreamType,
			BufferSizeDB,
			MaxBitrate,
			AvgBitrate,
			DecSpecificInfoSize
		}

		#region Inner classes
		private enum DescriptorClassTag
		{
			Forbidden0x00 = 0x00,
			ObjectDescrTag = 0x01,
			InitialObjectDescrTag = 0x02,
			ES_DescrTag = 0x03,
			DecoderConfigDescrTag = 0x04,
			DecSpecificInfoTag = 0x05,
			// TODO handle next values
			SLConfigDescrTag = 0x06,
			ContentIdentDescrTag = 0x07,
			SupplContentIdentDescrTag = 0x08,
			IPI_DescrPointerTag = 0x09,
			IPMP_DescrPointerTag = 0x0A,
			IPMP_DescrTag = 0x0B,
			QoS_DescrTag = 0x0C,
			RegistrationDescrTag = 0x0D,
			ES_ID_IncTag = 0x0E,
			ES_ID_RefTag = 0x0F,
			MP4_IOD_Tag = 0x10,
			MP4_OD_Tag = 0x11,
			IPL_DescrPointerRefTag = 0x12,
			ExtendedProfileLevelDescrTag = 0x13,
			profileLevelIndicationIndexDescrTag = 0x14,
			//Reserved for ISO use = 0x15-0x3F
			ContentClassificationDescrTag = 0x40,
			KeyWordDescrTag = 0x41,
			RatingDescrTag = 0x42,
			LanguageDescrTag = 0x43,
			ShortTextualDescrTag = 0x44,
			ExpandedTextualDescrTag = 0x45,
			ContentCreatorNameDescrTag = 0x46,
			 ContentCreationDateDescrTag = 0x47,
			OCICreatorNameDescrTag = 0x48,
			OCICreationDateDescrTag = 0x49,
			SmpteCameraPositionDescrTag = 0x4A,
			//Reserved for ISO use (OCI extensions) = 0x4B-0x5F
			//Reserved for ISO use = 0x60-0xBF
			//User private = 0xC0-0xFE
			Forbidden0xFF = 0xFF
		}

		private struct BaseDescriptor
		{
			private readonly DescriptorClassTag _tag;
			private readonly long _length;

			#region Properties
			public DescriptorClassTag Tag { get { return _tag; } }
			public long Length { get { return _length; } }
			#endregion Properties


			public BaseDescriptor(DescriptorClassTag tag, long length)
			{
				_tag = tag;
				_length = length;
			}
		}
		#endregion Inner classes

		#region Properties
		public IDataPacket ExtraData { get; set; }
		#endregion Properties


		public ElementaryStreamDescriptor(QtAtom previousHeader)
			: base(previousHeader, AtomName.ElementaryStreamDescriptor)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetUInt(Attribute.Version, "{0:X8}");

			// TODO: the sequence of descriptors is taken (mostly) from ffmpeg
			// TODO: verify that this is the actual sequence seen in (all) 3GPP files
			BaseDescriptor descriptor = ReadDescriptor(parser);
			if (descriptor.Tag == DescriptorClassTag.ES_DescrTag)
			{
				parser.GetUShort();		// ID
				parser.GetByte();		// priority
			}
			else
			{
				parser.GetUShort();		// ID
			}

			descriptor = ReadDescriptor(parser);
			if (descriptor.Tag == DescriptorClassTag.DecoderConfigDescrTag)
			{
				parser.GetByte(Attribute.ObjectTypeIndication);		// 0x20 Visual ISO/IEC 14496-2
				parser.GetByte(Attribute.StreamType);
				parser.GetThreeBytes(Attribute.BufferSizeDB);				// 3 bytes
				parser.GetUInt(Attribute.MaxBitrate);
				parser.GetUInt(Attribute.AvgBitrate);

				descriptor = ReadDescriptor(parser);
				if ((parser.Position + descriptor.Length) > parser.Length)
				{
					return false;
				}
				if (descriptor.Tag == DescriptorClassTag.DecSpecificInfoTag)
				{
					// Extra data can be empty (0 bytes)
					if (descriptor.Length > 0)
					{
						ExtraData = parser.GetDataPacket(parser.Position, descriptor.Length);
					}

					Attributes.Add(new FormattedAttribute<Attribute, long>(Attribute.DecSpecificInfoSize, descriptor.Length));
				}
				parser.Position += Math.Min(descriptor.Length, parser.Length - parser.Position);
			}
			return this.Valid;
		}

		/// <summary>
		/// Reads a MPEG-4 descriptor.
		/// </summary>
		/// <remarks>defined in ISO/IEC 14496-1 §7.2.2</remarks>
		private static BaseDescriptor ReadDescriptor(QtParser parser)
		{
			DescriptorClassTag tag = (DescriptorClassTag)parser.GetByte();

			// Expandable length [see ISO/IEC 14496-1 §8.3.3]
			int count = 4;
			uint nextByte = parser.GetByte();
			uint sizeOfInstance = (nextByte & 0x7F);
			while ((nextByte & 0x80) != 0 && --count > 0)
			{
				nextByte = parser.GetByte();
				sizeOfInstance = (sizeOfInstance << 7) | (nextByte & 0x7F);
			}
			return new BaseDescriptor(tag, sizeOfInstance);
		}
	}
}
