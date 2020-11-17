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
using System.Globalization;
using System.Text;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Asf
{
	internal enum TimeUnit
	{
		HundredNanoSeconds,
		Milliseconds
	}

	class AsfParser : ByteStreamParser<AsfObject, AsfObjectName, AsfParser>
	{
		private const int GuidLength = 16;

		#region Properties
		// TODO Get value from FileProperties object
		public int DataPacketLength {get;set;}

		public ulong BytesRemaining
		{
			get
			{
				AsfObject asfObject = Result as AsfObject;
				Debug.Assert(asfObject != null);
				return Math.Min((ulong)Length, asfObject.RelativeEndOffset) - (ulong)Position;
			}
		}
		#endregion

		public AsfParser(ByteStreamDataReader dataReader)
			: base(dataReader)
		{
			//DataReader = dataReader;
		}

		public override AsfObject FindFirstHeader(AsfObject root, long offsetLimit)
		{
			long asfObjectOffsetLimit = Math.Min(offsetLimit, Length);
			Guid? objectType;

			while ((objectType = NextAsfObjectType(asfObjectOffsetLimit, true)) != null)
			{
				AsfObject asfObject = CreateAsfObject(root, objectType.Value);

				if ((asfObject != null) &&
					(asfObject.HeaderName != AsfObjectName.Unknown))
				{
					return asfObject;
				}
			}
			return null;
		}

		public override AsfObject GetNextHeader(AsfObject previousObject, long offsetLimit)
		{
			if (Position > (Length - GuidLength))
			{
				return null;
			}

			// Try to read the directly succeeding ASF object
			Guid? nextAsfObjectType = NextAsfObjectType(Position);
			if (nextAsfObjectType == null)
			{
				return null;
			}

			return CreateAsfObject(previousObject, nextAsfObjectType.Value);
		}

		/// <summary>
		/// Reads a GUID from the data stream and returns the value.
		/// </summary>
		/// <returns>the GUID value</returns>
		public Guid? GetGuid()
		{
			if (!CheckRead(16)) return null;

			return DataReader.GetGuid();
		}

		/// <summary>
		/// Reads a GUID from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the GUID value</returns>
		public Guid? GetGuid<T>(T attributeName)
		{
			Guid? guid = GetGuid();
			if (guid == null) return null;

			AsfObjectName asfObjectName = AsfObjectName.Unknown;
			if (guid.Value.IsKnownObjectType())
			{
				asfObjectName = guid.Value.GetObjectName();
			}
			AddAttribute(new GuidAttribute<T>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", asfObjectName, guid)));
			return guid;
		}

		/// <summary>
		/// Reads a GUID from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="TAttributeEnum">the attribute name enumeration type</typeparam>
		/// <typeparam name="TGuidEnum">the GUID name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="nameForType">a collection containing all known GUIDs and the enum value</param>
		/// <returns>The enum value representing the GUID</returns>
		public TGuidEnum GetGuid<TAttributeEnum, TGuidEnum>(TAttributeEnum attributeName, IDictionary<Guid, TGuidEnum> nameForType)
		{
			Guid? guid = GetGuid();
			if (guid == null) return default(TGuidEnum);

			TGuidEnum value = guid.Value.ToEnum(nameForType);
			AddAttribute(new FormattedAttribute<TAttributeEnum, string>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Enum.GetName(typeof(TGuidEnum), value), guid)));
			return value;
		}

		/// <summary>
		/// Read a little endian UTF-16 unicode string.
		/// A Byte-Order Marker (BOM) character is not present.
		/// </summary>
		/// <typeparam name="T">The type of the attributeName</typeparam>
		/// <param name="attributeName">The name of the attribute</param>
		/// <param name="charsToRead">The number of characters to read</param>
		/// <returns>The string read</returns>
		public string GetUnicodeString<T>(T attributeName, int charsToRead)
		{
			return GetString(attributeName, charsToRead, StringType.Unicode);
		}

		/// <summary>
		/// Read an ASCII string
		/// </summary>
		/// <typeparam name="T">The type of the attributeName</typeparam>
		/// <param name="attributeName">The name of the attribute</param>
		/// <param name="bytesToRead">the number of characters to read</param>
		/// <returns>the string read</returns>
		public string GetString<T>(T attributeName, int bytesToRead)
		{
			return GetString(attributeName, bytesToRead, StringType.Ascii);
		}

		private enum StringType
		{
			Ascii,
			Unicode
		}

		private string GetString<T>(T attributeName, int charToReadLength, StringType stringType)
		{
			if (!CheckRead(charToReadLength) || charToReadLength == 0) return string.Empty;

			uint maxAttributeStringLength = (uint)AsfDetector.Configurable[AsfDetector.ConfigurationKey.AttributeMaxStringLength];

			// TODO use more than one line to display the string. Use /r/n formating from the string or break on line length.
			StringBuilder stringBuilder = new StringBuilder();
			ulong count = 0;
			while ((count++) < (ulong)charToReadLength && count < maxAttributeStringLength)
			{
				char c;
				if(stringType == StringType.Ascii)
					c = (char)DataReader.GetByte();
				else
					c = BitConverter.ToChar(new[] { DataReader.GetByte(), DataReader.GetByte() }, 0);

				if (!char.IsControl(c))
					stringBuilder.Append(c);
			}

			string value = stringBuilder.ToString();
			AddAttribute(new FormattedAttribute<T, string>(attributeName, value));
			return value;
		}

		public void GetLongDateTime<T>(T attributeName)
		{
			if (!CheckRead(8)) return;

			long hundredNanoSecondUnits = DataReader.GetLong();
			DateTime dateTime = new DateTime(1601, 1, 1, 0, 0, 0, 0);
			dateTime = dateTime.Add(new TimeSpan(hundredNanoSecondUnits));
			AddAttribute(new FormattedAttribute<T, DateTime>(attributeName, dateTime));
		}

		public void GetLongTime<T>(T attributeName, TimeUnit timeUnit)
		{
			if (!CheckRead(8)) return;

			long value = DataReader.GetLong();

			TimeSpan timeSpan = (timeUnit == TimeUnit.HundredNanoSeconds) ? new TimeSpan(value) : new TimeSpan(value * 10000);

			AddAttribute(new FormattedAttribute<T, TimeSpan>(attributeName, timeSpan));
		}

		/// <summary>
		/// Read a 32 bits value and parse it as time in milliseconds
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attributeName"></param>
		public void GetTime<T>(T attributeName)
		{
			if (!CheckRead(4)) return;

			int value = DataReader.GetInt();
			AddAttribute(new FormattedAttribute<T, TimeSpan>(attributeName, new TimeSpan(value * 10000)));
		}

		public int GetLengthValue(LengthType lengthType)
		{
			if (!CheckRead(lengthType.GetLengthInBytes())) return 0;

			switch (lengthType)
			{
				case LengthType.ValueNotPresent: return 0;
				case LengthType.Byte: return GetByte();
				case LengthType.Word: return GetShort();
				case LengthType.DWord: return GetInt();
				default:
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum LengthType not handled.", Enum.GetName(typeof(LengthType), lengthType)));
					return 0;
			}
		}

		public int GetLengthValue<T>(T attributeName, LengthType lengthType)
		{
			if (!CheckRead(lengthType.GetLengthInBytes())) return 0;

			switch (lengthType)
			{
				case LengthType.ValueNotPresent: return 0;
				case LengthType.Byte: return GetByte(attributeName);
				case LengthType.Word: return GetShort(attributeName);
				case LengthType.DWord: return GetInt(attributeName);
				default:
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum LengthType not handled.", Enum.GetName(typeof(LengthType), lengthType)));
					return 0;
			}
		}

		private Guid? NextAsfObjectType(long asfObjectOffsetLimit)
		{
			return NextAsfObjectType(asfObjectOffsetLimit, false);
		}

		/// <summary>
		/// Finds and returns the (known) type of the next object.
		/// The next object type is the (known) type of the next object.
		/// </summary>
		/// <param name="asfObjectOffsetLimit">the maximum offset for the next object</param>
		/// <param name="firstHeader">true when this method is used to find the first headers of a data block</param>
		/// <returns>the type (guid) of the next object, <code>0</code> for none</returns>
		private Guid? NextAsfObjectType(long asfObjectOffsetLimit, bool firstHeader)
		{
			if ((State != DataReaderState.Ready) || (Position > Math.Min(asfObjectOffsetLimit, Length)))
			{
				return null;
			}

			Queue<byte> guid = new Queue<byte>(DataReader.GetGuid().ToByteArray());

			// Try to read the next ASF object
			while (State != DataReaderState.Cancelled)
			{
				long offset = (Position - GuidLength);

				byte[] guidArray = guid.ToArray();
				Guid guidObject = new Guid(guidArray);

				// Validate the guid
				if (guidObject.IsKnownObjectType())
				{
					Position = offset;
					return guidObject;
				}
				// Check for data packet with data object missing
				if (firstHeader && guidArray.IsDataPacket(DataReader))
				{
					Position = offset;
					return ObjectNameExtensions.DataObjectWithoutStartGuid;
				}
				if (Position >= asfObjectOffsetLimit)
				{
					break;
				}

				// Shift buffer, read next byte
				guid.Dequeue();
				guid.Enqueue(DataReader.GetByte());
			}
			return null;
		}

		private static AsfObject CreateAsfObject(AsfObject previousObject, Guid startCode)
		{
			AsfObjectName asfObjectName = startCode.GetObjectName();

			switch (asfObjectName)
			{
				case AsfObjectName.HeaderObject: return new HeaderObject(previousObject);
				case AsfObjectName.FilePropertiesObject: return new FilePropertiesObject(previousObject);
				case AsfObjectName.StreamPropertiesObject: return new StreamPropertiesObject(previousObject);
				case AsfObjectName.CodecListObject: return new CodecListObject(previousObject);
				case AsfObjectName.HeaderExtensionObject: return new HeaderExtensionObject(previousObject);
				case AsfObjectName.ContentDescriptionObject: return new ContentDescriptionObject(previousObject);
				case AsfObjectName.DataObject: return new DataObject(previousObject);
				case AsfObjectName.DataObjectWithoutStart: return new DataObjectWithoutStart(previousObject);
				case AsfObjectName.ExtendedContentDescriptionObject: return new ExtendedContentDescriptionObject(previousObject);
				case AsfObjectName.StreamBitratePropertiesObject: return new StreamBitratePropertiesObject(previousObject);
				case AsfObjectName.PaddingObject: return new PaddingObject(previousObject);
				case AsfObjectName.ScriptCommandObject: return new ScriptCommandObject(previousObject);
				case AsfObjectName.ExtendedStreamPropertiesObject: return new ExtendedStreamPropertiesObject(previousObject);
				case AsfObjectName.CompatibilityObject: return new CompatibilityObject(previousObject);
				case AsfObjectName.LanguageListObject: return new LanguageListObject(previousObject);
				case AsfObjectName.MetadataObject: return new MetadataObject(previousObject);
				case AsfObjectName.AdvancedMutualExclusionObject: return new AdvancedMutualExclusionObject(previousObject);
				case AsfObjectName.GroupMutualExclusionObject: return new GroupMutualExclusionObject(previousObject);
				case AsfObjectName.StreamPrioritizationObject: return new StreamPrioritizationObject(previousObject);
				case AsfObjectName.BandwidthSharingObject: return new BandwidthSharingObject(previousObject);
				case AsfObjectName.MetadataLibraryObject: return new MetadataLibraryObject(previousObject);
				case AsfObjectName.IndexParametersObject: return new IndexParametersObject(previousObject);
				case AsfObjectName.MediaObjectIndexParametersObject: return new MediaObjectIndexParametersObject(previousObject);
				case AsfObjectName.TimecodeIndexParametersObject: return new TimecodeIndexParametersObject(previousObject);
				case AsfObjectName.AdvancedContentEncryptionObject: return new AdvancedContentEncryptionObject(previousObject);
				case AsfObjectName.BitrateMutualExclusionObject: return new BitrateMutualExclusionObject(previousObject);
				case AsfObjectName.MarkerObject: return new MarkerObject(previousObject);
				case AsfObjectName.ContentEncryptionObject: return new ContentEncryptionObject(previousObject);
				case AsfObjectName.ExtendedContentEncryptionObject: return new ExtendedContentEncryptionObject(previousObject);
				case AsfObjectName.ErrorCorrectionObject: return new ErrorCorrectionObject(previousObject);
				case AsfObjectName.ContentBrandingObject: return new ContentBrandingObject(previousObject);
				case AsfObjectName.DigitalSignatureObject: return new DigitalSignatureObject(previousObject);
				case AsfObjectName.SimpleIndexObject: return new SimpleIndexObject(previousObject);
				case AsfObjectName.IndexObject: return new IndexObject(previousObject);
				case AsfObjectName.MediaObjectIndexObject: return new MediaObjectIndexObject(previousObject);
			}
			return new AsfObject(previousObject, asfObjectName);
		}
	}
}
