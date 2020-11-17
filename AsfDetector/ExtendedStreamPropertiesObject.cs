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
using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class ExtendedStreamPropertiesObject : AsfObject
	{
		#region Inner classes
		private class StreamName : CompositeAttribute<Attribute, string, AsfParser>
		{
			private enum LAttribute
			{
				LanguageIDIndex,
				StreamNameLength,
				StreamName,
			}

			internal StreamName()
				: base(Attribute.StreamName, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				parser.GetShort(LAttribute.LanguageIDIndex);
				short streamLength = parser.GetShort(LAttribute.StreamNameLength);
				TypedValue = parser.GetUnicodeString(LAttribute.StreamName, streamLength / 2);

				return Valid;
			}
		}

		private class Payload : CompositeAttribute<Attribute, string, AsfParser>
		{
			internal enum StandardPayloadExtensionSystemGuids
			{
				Unknown,
				[Guid("399595EC-8667-4E2D-8FDB-98814CE76C1E")]
				Timecode,
				[Guid("E165EC0E-19ED-45D7-B4A7-25CBD1E28E9B")]
				FileName,
				[Guid("D590DC20-07BC-436C-9CF7-F3BBFBF1A4DC")]
				ContentType,
				[Guid("1B1EE554-F9EA-4BC8-821A-376B74E4C4B8")]
				PixelAspectRatio,
				[Guid("C6BD9450-867F-4907-83A3-C77921B733AD")]
				SampleDuration,
				[Guid("6698B84E-0AFA-4330-AEB2-1C0A98D7A44D")]
				EncryptionSampleID,
			}

			private enum LAttribute
			{
				/// <summary>
				/// Specifies a unique identifier for the extension system.
				/// Five standard GUIDs are defined in section 10.13, although
				/// custom extension system types can be defined as well.
				/// </summary>
				ExtensionSystemID,
				/// <summary>
				/// Specifies the fixed size of the extension data for this system that will
				/// appear in the replicated data alongside every payload for this stream.
				/// If this extension system uses variable-size data, then this should be
				/// set to 0xffff. Note, however, that replicated data length is limited
				/// to 255 bytes, which limits the total size of all extension systems for
				/// a particular stream.
				/// </summary>
				ExtensionDataSize,
				/// <summary>
				/// Specifies the length of the Extension System Info field.
				/// This field shall be set to 0 if there is no value in the
				/// Extension System Info field.
				/// </summary>
				ExtensionSystemInfoLength,
				/// <summary>
				/// Specifies additional information to describe this extension
				/// system (optional).
				/// </summary>
				ExtensionSystemInfo,
			}

			private static readonly IDictionary<Guid, StandardPayloadExtensionSystemGuids> NameForGuid = CreateNameForValueCollection<StandardPayloadExtensionSystemGuids>();

			internal Payload()
				: base(Attribute.Payload, string.Empty, "{0}")
			{
			}

			public override bool Parse(AsfParser parser)
			{
				StandardPayloadExtensionSystemGuids payloadGuid = parser.GetGuid(LAttribute.ExtensionSystemID, NameForGuid);
				short extensionDataSize = parser.GetShort(LAttribute.ExtensionDataSize);
				int systemInfoLength = parser.GetInt(LAttribute.ExtensionSystemInfoLength);
				string extensionSystemInfo = parser.GetString(LAttribute.ExtensionSystemInfo, (short)systemInfoLength);

				if (string.IsNullOrEmpty(extensionSystemInfo))
				{
					TypedValue = string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", payloadGuid, extensionDataSize, systemInfoLength);
				}
				else
				{
					TypedValue = string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}, {3}", payloadGuid, extensionDataSize, systemInfoLength, extensionSystemInfo);
				}

				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			StartTime,
			EndTime,
			DataBitrate,
			BufferSize,
			InitialBufferFullness,
			AlternateDataBitrate,
			AlternateBufferSize,
			AlternateInitialBufferFullness,
			MaximumObjectSize,
			Flags,
			StreamNumber,
			StreamLanguageIDIndex,
			AverageTimePerFrame,
			StreamNameCount,
			StreamName,
			PayloadExtensionSystemCount,
			Payload
		}

		public ExtendedStreamPropertiesObject(AsfObject previousObject)
			: base(previousObject, AsfObjectName.ExtendedStreamPropertiesObject)
		{
		}

		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetLongTime(Attribute.StartTime, TimeUnit.Milliseconds);
			parser.GetLongTime(Attribute.EndTime, TimeUnit.Milliseconds);
			parser.GetInt(Attribute.DataBitrate);
			parser.GetTime(Attribute.BufferSize);
			parser.GetTime(Attribute.InitialBufferFullness);
			parser.GetInt(Attribute.AlternateDataBitrate);
			parser.GetTime(Attribute.AlternateBufferSize);
			parser.GetTime(Attribute.AlternateInitialBufferFullness);
			parser.GetInt(Attribute.MaximumObjectSize);
			parser.GetInt(Attribute.Flags);
			parser.GetShort(Attribute.StreamNumber);	// TODO Check range 1 - 127
			parser.GetShort(Attribute.StreamLanguageIDIndex);
			parser.GetLongTime(Attribute.AverageTimePerFrame, TimeUnit.HundredNanoSeconds);
			short streamNameCount = parser.GetShort(Attribute.StreamNameCount);
			short payLoadCount = parser.GetShort(Attribute.PayloadExtensionSystemCount);

			for (int streamName = 0; streamName < streamNameCount; streamName++)
			{
				parser.Parse(new StreamName());
			}

			for (int payLoad = 0; payLoad < payLoadCount; payLoad++)
			{
				parser.Parse(new Payload());
			}

			// TODO Read the Stream Properties Object

			return Valid;
		}
	}
}
