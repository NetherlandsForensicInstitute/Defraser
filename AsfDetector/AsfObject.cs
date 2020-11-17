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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Asf
{
	class AsfObject : Header<AsfObject, AsfObjectName, AsfParser>
	{
		/// <summary>The possible Attributes of a detected object.</summary>
		public enum Attribute
		{
			Type,
			Size,
			AdditionalData
		}

		#region Properties
		public override CodecID DataFormat { get { return CodecID.Asf; } }

		public ulong Size { get; protected set; }
		virtual internal ulong RelativeEndOffset { get { return (ulong)Offset + Size; } }
		internal ulong UnparsedBytes { get; private set; }
		/// <summary>
		/// The object type or large part of its data is unkown.
		/// </summary>
		virtual internal bool IsUnknown
		{
			get { return HeaderName == AsfObjectName.Unknown || UnparsedBytes > MaxUnparsedBytes; }
		}

		// The maximum number of unparsed bytes allowed for trailing objects.
		private static uint MaxUnparsedBytes
		{
			get { return (uint)AsfDetector.Configurable[AsfDetector.ConfigurationKey.AsfObjectMaxUnparsedByteCount]; }
		}
		#endregion Properties

		#region Constructors
		/// <summary>Constructs a new top level object.</summary>
		public AsfObject(IEnumerable<IDetector> detectors)
			: base(detectors, AsfObjectName.Root)
		{
		}

		public AsfObject(AsfObject previousObject, AsfObjectName asfObjectName)
			: base(previousObject, asfObjectName)
		{
		}
		#endregion Constructors

		/// <summary>Parse the ASF object</summary>
		public override bool Parse(AsfParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (HeaderName.IsFlagSet(ObjectFlags.SizeAndType))
			{
				// Parse the type and size of the object.
				parser.GetGuid(Attribute.Type);
				Size = (ulong)parser.GetLong(Attribute.Size);
			}
			if (!HeaderName.IsFlagSet(ObjectFlags.Container) && Size > MaxUnparsedBytes) return false;

			return Valid;
		}

		/// <summary>
		/// This method is called from Parser.Parse() and shall contain the code to
		/// detect adjacent objects.
		/// </summary>
		public override bool IsBackToBack(AsfObject asfObject)
		{
			return true;
		}

		public override bool IsSuitableParent(AsfObject parent)
		{
			if (!parent.IsRoot)
			{
				// Should fit in parent object
				if (Offset < parent.Offset || RelativeEndOffset > parent.RelativeEndOffset)
				{
					return false;
				}
			}

			// First object should not be unknown
			if (IsUnknown && parent.IsRoot && !parent.HasChildren())
			{
				return false;
			}

			// ASF object should have no duplicates in its parent if DuplicatesAllowed is false
			if (!HeaderName.IsFlagSet(ObjectFlags.DuplicatesAllowed) && parent.HasChild(HeaderName))
			{
				// ASF object of partial files may end up in the root
				// Therefore, duplicates are allowed, except for top-level atoms
				if (!parent.IsRoot || HeaderName.IsTopLevel())
				{
					return false;
				}
			}

			// Header object is always the first object
			if (HeaderName == AsfObjectName.HeaderObject && parent.HasChildren())
			{
				return false;
			}

			// Object is allowed in its suitable parent (correct files)
			if (HeaderName.GetSuitableParents().Contains(parent.HeaderName))
			{
				return true;
			}

			// Object ends up in the root if no suitable parent was found (partial files)
			if (!parent.IsRoot)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns whether this object contains a child of the given type.
		/// </summary>
		/// <param name="objectName">the object name type</param>
		/// <returns>true if this object contains such as child, false otherwise</returns>
		internal bool HasChild(AsfObjectName objectName)
		{
			return FindChild(objectName) != null;
		}

		public override bool ParseEnd(AsfParser parser)
		{
			return ParseEnd(parser, false);
		}

		public bool ParseEnd(AsfParser parser, bool ignoreReadOverflow)
		{
			if (!ignoreReadOverflow && parser.ReadOverflow)
			{
				Valid = false;
				return false;
			}

			if (!HeaderName.IsFlagSet(ObjectFlags.ContainerObject))
			{
				// Objects extending beyond the end of the file are considered invalid
				if (RelativeEndOffset > (ulong)parser.Length)
				{
					Valid = false;
					return false;
				}

				// Record unparsed bytes statistic
				ulong endOffset = Math.Min(RelativeEndOffset, (ulong)parser.Length);
				if ((ulong)parser.Position < endOffset)
				{
					UnparsedBytes = endOffset - (ulong)parser.Position;
					parser.GetHexDump(Attribute.AdditionalData, (int)UnparsedBytes);
				}
			}
			if (!ignoreReadOverflow && (!Valid || (ulong)parser.Position > RelativeEndOffset))
			{
				return false;
			}
			return base.ParseEnd(parser);
		}

		protected static IDictionary<Guid, T> CreateNameForValueCollection<T>()
		{
			IDictionary<Guid, T> nameForValue = new Dictionary<Guid, T>();

			// Use reflection to find the attributes describing the codec identifiers
			Type enumType = typeof(T);
			foreach (T enumValue in Enum.GetValues(enumType))
			{
				string name = Enum.GetName(enumType, enumValue);

				if (name == "Unknown") continue;

				FieldInfo fieldInfo = enumType.GetField(name);
				GuidAttribute[] attributes = (GuidAttribute[])fieldInfo.GetCustomAttributes(typeof(GuidAttribute), false);

				if (attributes != null && attributes.Length == 1)
				{
					nameForValue.Add(attributes[0].Guid, enumValue);
				}
				else
				{
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "No attributes for enumeration value. Please add attributes to '{0}' of enumeration '{1}'.", enumValue, enumType));
				}
			}
			return nameForValue;
		}
	}
}
