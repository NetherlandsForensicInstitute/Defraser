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
using System.Reflection;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	[Flags]
	internal enum ObjectFlags
	{
		/// <summary>The object contains size and type fields.</summary>
		SizeAndType = 1,
		/// <summary>The object is a container object.</summary>
		Container = 4,
		/// <summary>The object is mandatory in its suitable parent.</summary>
		Mandatory = 8,
		/// <summary>The object is allowed more than once in its suitable parent.</summary>
		DuplicatesAllowed = 16,

		/// <summary>The flags for container objects.</summary>
		ContainerObject = SizeAndType | Container,
	}

	// Objects the ASF-Detector will find
	internal enum AsfObjectName
	{
		Root,
		[Asf(null, new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject | ObjectFlags.Mandatory)]
		DataObjectWithoutStart,
		[Asf(null, new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		Unknown,
		[Asf(null, new [] { AsfObjectName.DataObject }, ObjectFlags.SizeAndType)]
		DataPacket,

		// Top-level ASF object GUIDS
		[Asf("75B22630-668E-11CF-A6D9-00AA0062CE6C", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject|ObjectFlags.Mandatory)]
		HeaderObject,
		[Asf("75B22636-668E-11CF-A6D9-00AA0062CE6C", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject|ObjectFlags.Mandatory)]
		DataObject,
		[Asf("33000890-E5B1-11CF-89F4-00A0C90349CB", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject | ObjectFlags.DuplicatesAllowed/*1 for each non-hidden video stream*/)]
		SimpleIndexObject,
		[Asf("D6E229D3-35DA-11D1-9034-00A0C90349BE", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject)]
		IndexObject,
		[Asf("FEB103F8-12AD-4C64-840F-2A1D2F7AD48C", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject)]
		MediaObjectIndexObject,
		//[Asf("3CB73FD0-0C4A-4803-953D-EDF7B6228F0C", new[] { AsfObjectName.Root }, ObjectFlags.ContainerObject)]
		//TimecodeIndexObject,

		// Header Object GUIDs
		[Asf("8CABDCA1-A947-11CF-8EE4-00C00C205365", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType | ObjectFlags.Mandatory)]
		FilePropertiesObject,
		[Asf("B7DC0791-A9B7-11CF-8EE6-00C00C205365", new[] { AsfObjectName.HeaderObject }, ObjectFlags.ContainerObject | ObjectFlags.Mandatory | ObjectFlags.DuplicatesAllowed/*one per stream*/)]
		StreamPropertiesObject,
		[Asf("5FBF03B5-A92E-11CF-8EE3-00C00C205365", new[] { AsfObjectName.HeaderObject }, ObjectFlags.ContainerObject | ObjectFlags.Mandatory)]
		HeaderExtensionObject,
		[Asf("86D15240-311D-11D0-A3A4-00A0C90348F6", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		CodecListObject,
		[Asf("1EFB1A30-0B62-11D0-A39B-00A0C90348F6", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ScriptCommandObject,
		[Asf("F487CD01-A951-11CF-8EE6-00C00C205365", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		MarkerObject,
		[Asf("D6E229DC-35DA-11D1-9034-00A0C90349BE", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		BitrateMutualExclusionObject,
		[Asf("75B22635-668E-11CF-A6D9-00AA0062CE6C", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ErrorCorrectionObject,
		[Asf("75B22633-668E-11CF-A6D9-00AA0062CE6C", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ContentDescriptionObject,
		[Asf("D2D0A440-E307-11D2-97F0-00A0C95EA850", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ExtendedContentDescriptionObject,
		[Asf("2211B3FA-BD23-11D2-B4B7-00A0C955FC6E", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ContentBrandingObject,
		[Asf("7BF875CE-468D-11D1-8D82-006097C9A2B2", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		StreamBitratePropertiesObject,
		[Asf("2211B3FB-BD23-11D2-B4B7-00A0C955FC6E", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ContentEncryptionObject,
		[Asf("298AE614-2622-4C17-B935-DAE07EE9289C", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		ExtendedContentEncryptionObject,
		[Asf("2211B3FC-BD23-11D2-B4B7-00A0C955FC6E", new[] { AsfObjectName.HeaderObject }, ObjectFlags.SizeAndType)]
		DigitalSignatureObject,

		[Asf("1806D474-CADF-4509-A4BA-9AABCB96AAE8", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.DuplicatesAllowed)]
		PaddingObject,
		// Found in ASF files but not in documentation
		[Asf("D9AADE20-7C17-4F9C-BC28-8555DD98E2A2", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		IndexParametersPlaceholderObject,

		// Objects in the ASF header extension object
		//HeaderExtensionObject
		[Asf("14E6A5CB-C672-4332-8399-A96952065B5A", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.DuplicatesAllowed/*one per media stream*/)]
		ExtendedStreamPropertiesObject,
		[Asf("A08649CF-4775-4670-8A16-6E35357566CD", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.DuplicatesAllowed)]
		AdvancedMutualExclusionObject,
		[Asf("D1465A40-5A79-4338-B71B-E36B8FD6C249", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.DuplicatesAllowed)]
		GroupMutualExclusionObject,
		[Asf("D4FED15B-88D3-454F-81F0-ED5C45999E24", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		StreamPrioritizationObject,
		[Asf("A69609E6-517B-11D2-B6AF-00C04FD908E9", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.DuplicatesAllowed)]
		BandwidthSharingObject,
		[Asf("7C4346A9-EFE0-4BFC-B229-393EDE415C85", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		LanguageListObject,
		[Asf("C5F8CBEA-5BAF-4877-8467-AA8C44FA4CCA", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		MetadataObject,
		[Asf("44231C94-9498-49D1-A141-1D134E457054", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		MetadataLibraryObject,
		[Asf("D6E229DF-35DA-11D1-9034-00A0C90349BE", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.Mandatory)]
		IndexParametersObject,
		[Asf("6B203BAD-3F11-48E4-ACA8-D7613DE2CFA7", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType | ObjectFlags.Mandatory)]
		MediaObjectIndexParametersObject,
		[Asf("F55E496D-9797-4B5D-8C8B-604DFE9BFB24", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		TimecodeIndexParametersObject,
		[Asf("26F18B5D-4584-47EC-9F5F-0E651F0452C9", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		CompatibilityObject,
		[Asf("43058533-6981-49E6-9B74-AD12CB86D58C", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		AdvancedContentEncryptionObject,

		//ExtendedStreamPropertiesObject
		//[Asf("399595EC-8667-4E2D-8FDB-98814CE76C1E", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemTimecode,
		//[Asf("E165EC0E-19ED-45D7-B4A7-25CBD1E28E9B", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemFileName,
		//[Asf("D590DC20-07BC-436C-9CF7-F3BBFBF1A4DC", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemContentType,
		//[Asf("1B1EE554-F9EA-4BC8-821A-376B74E4C4B8", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemPixelAspectRatio,
		//[Asf("C6BD9450-867F-4907-83A3-C77921B733AD", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemSampleDuration,
		//[Asf("6698B84E-0AFA-4330-AEB2-1C0A98D7A44D", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//PayloadExtensionSystemEncryptionSampleID,

		//AdvancedMutualExclusionObject
		//[Asf("D6E22A00-35DA-11D1-9034-00A0C90349BE", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//MutexLanguage,
		//[Asf("D6E22A01-35DA-11D1-9034-00A0C90349BE", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//MutexBitrate,
		//[Asf("D6E22A02-35DA-11D1-9034-00A0C90349BE", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//MutexUnknown,

		//StreamPropertiesObject
		//[Asf("ABD3D211-A9BA-11cf-8EE6-00C00C205365", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//Reserved1,
		//[Asf("F8699E40-5B4D-11CF-A8FD-00805F5C442B", new[] { AsfObjectName.HeaderExtensionObject }, ObjectFlags.SizeAndType)]
		//AudioMedia,
		//[Asf("F8699E40-5B4D-11CF-A8FD-00805F5C442B", new AsfHeaderName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//Audio_Media,
		//[Asf("BC19EFC0-5B4D-11CF-A8FD-00805F5C442B", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//VideoMedia,
		//[Asf("59DACFC0-59E6-11D0-A3AC-00A0C90348F6", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//CommandMedia,
		//[Asf("B61BE100-5B4E-11CF-A8FD-00805F5C442B", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//JfifMedia,
		//[Asf("35907DE0-E415-11CF-A917-00805F5C442B", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//DegradableJpegMedia,
		//[Asf("91BD222C-F21C-497A-8B6D-5AA86BFC0185", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//FileTransferMedia,
		//[Asf("3AFB65E2-47EF-40F2-AC2C-70A90D71D343", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//BinaryMedia,
		//[Asf("20FB5700-5B55-11CF-A8FD-00805F5C442B", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//NoErrorCorrection,
		//[Asf("BFC3CD50-618F-11CF-8BB2-00AA00B4E220", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//AudioSpread,
		//[Asf("86D15241-311D-11D0-A3A4-00A0C90348F6", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//Reserved2,

		//ScriptCommandObject
		//[Asf("4B1ACBE3-100B-11D0-A39B-00A0C90348F6", new AsfObjectName[0]/* Allowed in all containers */, ObjectFlags.SizeAndType)]
		//Reserved3,
	}

	internal static class ObjectNameExtensions
	{
		#region Inner class
		/// <summary>
		/// Stores attributes for the objectName enum type extension methods.
		/// </summary>
		private class ObjectAttributes
		{
			public ICollection<Guid> ObjectTypes { get; private set; }
			public ICollection<AsfObjectName> SuitableParents { get; private set; }
			public ObjectFlags ObjectFlags { get; private set; }

			public ObjectAttributes(ICollection<Guid> objectTypes, ObjectFlags objectFlags, ICollection<AsfObjectName> suitableParents)
			{
				ObjectTypes = objectTypes;
				SuitableParents = suitableParents;
				ObjectFlags = objectFlags;
			}
		}
		#endregion Inner class

		private static readonly Dictionary<Guid, AsfObjectName> ObjectNameForObjectType;
		private static readonly Dictionary<AsfObjectName, ObjectAttributes> ObjectAttributesForObjectName;

		public static readonly Guid DataObjectWithoutStartGuid = new Guid("4AA62787-1920-470d-BA88-2F2AAD4717EE");

		static ObjectNameExtensions()
		{
			Type asfObjectNameType = typeof(AsfObjectName);

			ICollection<AsfObjectName> allContainerObjects = new HashSet<AsfObjectName>();
			ObjectNameForObjectType = new Dictionary<Guid, AsfObjectName>();
			ObjectAttributesForObjectName = new Dictionary<AsfObjectName, ObjectAttributes>();

			// Create the list to feed to the objects that can be in all container object
			allContainerObjects.Add(AsfObjectName.Root);

			// Use reflection to find the attributes describing the codec identifiers
			foreach (AsfObjectName objectName in Enum.GetValues(asfObjectNameType))
			{
				string name = Enum.GetName(asfObjectNameType, objectName);

				FieldInfo fieldInfo = asfObjectNameType.GetField(name);
				AsfAttribute[] attributes = (AsfAttribute[])fieldInfo.GetCustomAttributes(typeof(AsfAttribute), false);

				if (objectName == AsfObjectName.Root)
				{
					// The root container has no metadata and does not occur in any other object
					ObjectAttributesForObjectName.Add(objectName, new ObjectAttributes(new Guid[0], ObjectFlags.ContainerObject, new AsfObjectName[0]));
				}
				else if (attributes != null && attributes.Length == 1)
				{
					ObjectAttributes objectAttributes = GetObjectAttributes(objectName, attributes[0], allContainerObjects);
					ObjectAttributesForObjectName.Add(objectName, objectAttributes);

					// List all GUID's for this object
					foreach (Guid objectType in objectAttributes.ObjectTypes)
					{
						//Debug.Assert(!_objectNameForObjectType.ContainsKey(objectType), string.Format("Duplicate GUID '{0}'"));
						ObjectNameForObjectType.Add(objectType, objectName);
					}
				}
				else
				{
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "No attributes for {0}. Please add attributes to the ObjectName enumeration.", objectName));
				}
			}

			CheckSuitableParents();
		}

		/// <summary>
		/// Gets the object attributes for the given <paramref name="objectName"/>
		/// using the information from the given <paramref name="attribute"/>.
		/// This adds the object to the list of container objects if-and-only-if the
		/// object is a container object.
		/// </summary>
		/// <param name="objectName">the object name</param>
		/// <param name="attribute">the attribute for the given object name</param>
		/// <param name="allContainerObjects">the list of all container objects</param>
		/// <returns>the object attributes</returns>
		private static ObjectAttributes GetObjectAttributes(AsfObjectName objectName, AsfAttribute attribute, ICollection<AsfObjectName> allContainerObjects)
		{
			// Retrieve list of object types (GUID)
			List<Guid> objectTypes = new List<Guid>();
			if (attribute.ObjectType != null)
			{
				foreach (string objectType in attribute.ObjectType.Split("|".ToCharArray()))
				{
					objectTypes.Add(new Guid(objectType));
				}
			}

			// Retrieve suitable parents, all container objects if empty list
			ICollection<AsfObjectName> suitableParents = attribute.SuitableParents;
			if (suitableParents.Count == 0)
			{
				suitableParents = allContainerObjects;
			}

			// Update list of all container objects
			if ((attribute.ObjectFlags & ObjectFlags.ContainerObject) == ObjectFlags.ContainerObject)
			{
				allContainerObjects.Add(objectName);
			}
			return new ObjectAttributes(objectTypes.AsReadOnly(), attribute.ObjectFlags, suitableParents);
		}

		public static bool IsTopLevel(this AsfObjectName objectName)
		{
			ICollection<AsfObjectName> suitableParents = objectName.GetSuitableParents();
			return suitableParents.Count == 1 && suitableParents.Contains(AsfObjectName.Root);
		}

		/// <summary>
		/// Checks that the suitable parents of all object names are container objects.
		/// </summary>
		private static void CheckSuitableParents()
		{
			foreach (AsfObjectName objectName in Enum.GetValues(typeof(AsfObjectName)))
			{
				foreach (AsfObjectName suitableParent in objectName.GetSuitableParents())
				{
					Debug.Assert(suitableParent.IsFlagSet(ObjectFlags.Container),
							string.Format(CultureInfo.CurrentCulture, "Suitable parent {0} of object {1} must be a container object.",
									suitableParent, objectName));
				}
			}
		}

		/// <summary>
		/// Gets the suitable parents for the given <paramref name="objectName"/>.
		/// </summary>
		/// <param name="objectName">the object name</param>
		/// <returns>the suitable parents</returns>
		public static ICollection<AsfObjectName> GetSuitableParents(this AsfObjectName objectName)
		{
			return ObjectAttributesForObjectName[objectName].SuitableParents;
		}

		/// <summary>
		/// Gets the object name from the given <paramref name="guid"/>.
		/// This returns <code>objectName.Unknown</code> if the GUID is unknown.
		/// </summary>
		/// <param name="guid">the GUID for the object type field</param>
		/// <returns>the object name</returns>
		public static AsfObjectName GetObjectName(this Guid guid)
		{
			AsfObjectName objectName;
			if (!ObjectNameForObjectType.TryGetValue(guid, out objectName))
			{
				if(guid == DataObjectWithoutStartGuid)
				{
					objectName = AsfObjectName.DataObjectWithoutStart;
				}
				else
				{
					objectName = AsfObjectName.Unknown;
				}
			}
			return objectName;
		}

		/// <summary>
		/// Returns whether the given <paramref name="objectFlag"/> is set for
		/// the given <paramref name="objectName"/>.
		/// </summary>
		/// <param name="objectName">the object name</param>
		/// <param name="objectFlag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		public static bool IsFlagSet(this AsfObjectName objectName, ObjectFlags objectFlag)
		{
			return (objectName.GetObjectFlags() & objectFlag) == objectFlag;
		}

		/// <summary>
		/// Gets the object type flags for the given <paramref name="objectName"/>.
		/// </summary>
		/// <param name="objectName">the object name</param>
		/// <returns>the object flags</returns>
		public static ObjectFlags GetObjectFlags(this AsfObjectName objectName)
		{
			return ObjectAttributesForObjectName[objectName].ObjectFlags;
		}

		public static bool IsDataPacket(this byte[] guidArray, ByteStreamDataReader dataReader)
		{
			if(guidArray[0] == 0x11 && guidArray[1] == 0x5D &&
				(guidArray[10] & 0xc0) == 0x80 &&
				((guidArray[11] & 0x7F) == 1 || (guidArray[11] & 0x7F) == 2))
			{
				if(dataReader.Position + 2 < dataReader.Length)
				{
					dataReader.GetByte();	// Ignore byte
					byte replicatedDataLength = dataReader.GetByte();
					dataReader.Position -= 2;

					if(replicatedDataLength == 8)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// First value of the enum should should be 'Unknown' and have value '0'.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="guid"></param>
		/// <param name="nameForGuid"></param>
		/// <returns></returns>
		public static T ToEnum<T>(this Guid guid, IDictionary<Guid, T> nameForGuid)
		{
			T value;
			if (nameForGuid.TryGetValue(guid, out value))
			{
				return value;
			}
			return default(T);
		}

		/// <summary>
		/// Returns whether the given <paramref name="guid"/> is a known guid.
		/// </summary>
		/// <param name="guid">a Guid value</param>
		/// <returns>true if it is a known Guid, false otherwise</returns>
		public static bool IsKnownObjectType(this Guid guid)
		{
			return ObjectNameForObjectType.ContainsKey(guid);
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class AsfAttribute : Attribute
	{
		#region Properties
		/// <summary>The object type field GUI as string.</summary>
		public string ObjectType { get; private set; }
		/// <summary>The suitable parent for the object.</summary>
		public AsfObjectName[] SuitableParents { get; private set; }
		/// <summary>The object type flags.</summary>
		public ObjectFlags ObjectFlags { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new object attribute.
		/// </summary>
		/// <remarks>
		/// Use an empty list for <param name="suitableParents"/> to specify
		/// all container objects.
		/// </remarks>
		/// <param name="objectType">the object type field GUID as string</param>
		/// <param name="suitableParents">the suitable parent for the object</param>
		/// <param name="flags">the object type flags</param>
		public AsfAttribute(string objectType, AsfObjectName[] suitableParents, ObjectFlags flags)
		{
			ObjectType = objectType;
			SuitableParents = suitableParents;
			ObjectFlags = flags;
		}
	}
}
