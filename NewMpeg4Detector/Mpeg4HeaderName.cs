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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Defraser.Detector.Mpeg4
{
	/// <summary>
	/// The header flags.
	/// </summary>
	[Flags]
	internal enum HeaderFlags
	{
		/// <summary>The header contains start code and Size fields.</summary>
		StartCode = 1,
		/// <summary>The header is a master header.</summary>
		Master = 4,
		/// <summary>The flags for master headers.</summary>
		MasterHeader = StartCode | Master,
	}

	// Headers the MPEG-4 Detector will find
	public enum Mpeg4HeaderName
	{
		Root,
		Unknown,

		[Mpeg4("00-1F", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObject }, new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObjectLayer }, HeaderFlags.MasterHeader)]
		VideoObject,		// 0x00 - 0x1F

		[Mpeg4("20-2F", new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObject, Mpeg4HeaderName.VisualObject }, new Mpeg4HeaderName[] { Mpeg4HeaderName.Vop, Mpeg4HeaderName.UserData, Mpeg4HeaderName.GroupOfVop, Mpeg4HeaderName.FgsVop, Mpeg4HeaderName.FbaObject, Mpeg4HeaderName.MeshObject }, HeaderFlags.MasterHeader)]
		VideoObjectLayer,	// 0x20 - 0x2F

		// 30 - 3F: reserved

		[Mpeg4("40-5F", new Mpeg4HeaderName[] { Mpeg4HeaderName.FgsVop }, new Mpeg4HeaderName[] {Mpeg4HeaderName.FgsVop}, HeaderFlags.StartCode)]
		FgsBp, // 0x40 - 0x5F

		// 60 - AF: reserved

		// short_video_start_marker: This is a 22-bit start marker containing
		// the value ‘0000 0000 0000 0000 1000 00’. It is used to mark the
		// location of a video plane having the short header format.
		// short_video_start_marker shall be byte aligned by the insertion of
		// zero to seven zero-valued bits as necessary to achieve byte
		// alignment prior to short_video_start_marker.
		[Mpeg4("80-83", new Mpeg4HeaderName[] { Mpeg4HeaderName.Root }, new Mpeg4HeaderName[] { Mpeg4HeaderName.VopWithShortHeader }, HeaderFlags.StartCode)]
		VopWithShortHeader, // H263 (0x81, 0x82, 0x83 are changed to startcode with value 0x80)

		[Mpeg4("B0", new Mpeg4HeaderName[] { Mpeg4HeaderName.Root }, new Mpeg4HeaderName[] {Mpeg4HeaderName.UserData, Mpeg4HeaderName.VisualObject, Mpeg4HeaderName.Vop, Mpeg4HeaderName.Extension, Mpeg4HeaderName.FbaObject}, HeaderFlags.MasterHeader)]
		VisualObjectSequenceStart,

		[Mpeg4("B1", new Mpeg4HeaderName[] { Mpeg4HeaderName.Root }, HeaderFlags.StartCode)]
		VisualObjectSequenceEnd,

		[Mpeg4("B2", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObjectSequenceStart, Mpeg4HeaderName.VisualObject, Mpeg4HeaderName.VideoObjectLayer }, HeaderFlags.StartCode)]
		UserData,

		[Mpeg4("B3", new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObjectLayer }, HeaderFlags.MasterHeader)]
		GroupOfVop,

		[Mpeg4("B4", new Mpeg4HeaderName[] { }, HeaderFlags.StartCode)]
		VideoSessionError,	//Not interesting

		[Mpeg4("B5", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObjectSequenceStart }, new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObject, Mpeg4HeaderName.VideoObjectLayer, Mpeg4HeaderName.UserData, Mpeg4HeaderName.FbaObject, Mpeg4HeaderName.MeshObject, Mpeg4HeaderName.StillTextureObject, Mpeg4HeaderName.TextureSpatialLayer}, HeaderFlags.MasterHeader)]
		VisualObject,

		[Mpeg4("B6", new Mpeg4HeaderName[] { Mpeg4HeaderName.GroupOfVop, Mpeg4HeaderName.VideoObjectLayer, Mpeg4HeaderName.Root /* in root when no parents found */ }, new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObject, Mpeg4HeaderName.Vop, Mpeg4HeaderName.VideoObjectLayer, Mpeg4HeaderName.GroupOfVop, Mpeg4HeaderName.Slice, Mpeg4HeaderName.Stuffing, Mpeg4HeaderName.VisualObjectSequenceStart }, HeaderFlags.MasterHeader)]
		Vop, // MPEG-4

		[Mpeg4("B7", new Mpeg4HeaderName[] { Mpeg4HeaderName.Vop }, HeaderFlags.StartCode)]
		Slice,

		[Mpeg4("B8", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObjectSequenceStart }, HeaderFlags.StartCode)]
		Extension,

		[Mpeg4("B9", new Mpeg4HeaderName[] { Mpeg4HeaderName.VideoObjectLayer }, HeaderFlags.MasterHeader)]
		FgsVop,

		[Mpeg4("BA", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObjectSequenceStart, Mpeg4HeaderName.VisualObject, Mpeg4HeaderName.VideoObjectLayer }, new Mpeg4HeaderName[] { Mpeg4HeaderName.FbaObjectPlane }, HeaderFlags.MasterHeader)]
		FbaObject,

		[Mpeg4("BB", new Mpeg4HeaderName[] { Mpeg4HeaderName.FbaObjectPlane, Mpeg4HeaderName.FbaObject }, new Mpeg4HeaderName[] {Mpeg4HeaderName.FbaObjectPlane}, HeaderFlags.MasterHeader)]
		FbaObjectPlane,

		[Mpeg4("BC", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObjectSequenceStart, Mpeg4HeaderName.VisualObject, Mpeg4HeaderName.VideoObjectLayer }, new Mpeg4HeaderName[] { Mpeg4HeaderName.MeshObjectPlane }, HeaderFlags.MasterHeader)]
		MeshObject,

		[Mpeg4("BD", new Mpeg4HeaderName[] { Mpeg4HeaderName.MeshObjectPlane, Mpeg4HeaderName.MeshObject }, HeaderFlags.MasterHeader)]
		MeshObjectPlane,

		[Mpeg4("BE", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObject }, HeaderFlags.MasterHeader)]
		StillTextureObject,

		[Mpeg4("BF", new Mpeg4HeaderName[] { Mpeg4HeaderName.VisualObject, Mpeg4HeaderName.StillTextureObject }, HeaderFlags.StartCode)]
		TextureSpatialLayer,

		[Mpeg4("C0", new Mpeg4HeaderName[] { Mpeg4HeaderName.TextureSnrLayer, Mpeg4HeaderName.StillTextureObject }, HeaderFlags.MasterHeader)]
		TextureSnrLayer,

		[Mpeg4("C1", new Mpeg4HeaderName[] { Mpeg4HeaderName.TextureTile, Mpeg4HeaderName.StillTextureObject }, HeaderFlags.MasterHeader)]
		TextureTile,

		[Mpeg4("C2", new Mpeg4HeaderName[] { Mpeg4HeaderName.TextureShapeLayer, Mpeg4HeaderName.StillTextureObject }, HeaderFlags.MasterHeader)]
		TextureShapeLayer,

		[Mpeg4("C3", new Mpeg4HeaderName[] { Mpeg4HeaderName.Vop }, HeaderFlags.StartCode)]
		Stuffing,

		// C4 - C5: reserved
		// C6 - FF: system start codes
	}

	/// <summary>
	/// Provides extension methods to the <c>HeaderName</c> enumeration.
	/// </summary>
	internal static class HeaderNameExtensions
	{
		#region Inner class
		/// <summary>
		/// Stores attributes for the HeaderName enum extension methods.
		/// </summary>
		private class HeaderAttributes
		{
			public ICollection<uint> HeaderStartCodes { get; set; }
			public ICollection<Mpeg4HeaderName> SuitableParents { get; set; }
			public HeaderFlags HeaderFlags { get; set; }

			public HeaderAttributes(ICollection<uint> headerStartCodes, HeaderFlags headerFlags, ICollection<Mpeg4HeaderName> suitableParents)
			{
				HeaderStartCodes = headerStartCodes;
				SuitableParents = suitableParents;
				HeaderFlags = headerFlags;
			}
		}
		#endregion Inner class

		private static readonly Dictionary<uint, Mpeg4HeaderName> _headerNameForHeaderStartCode;
		private static readonly Dictionary<Mpeg4HeaderName, HeaderAttributes> _headerAttributes;
		private static readonly Dictionary<Mpeg4HeaderName, HashSet<uint> > _allowedNextHeaders;

		/// <summary>Static data initialization.</summary>
		static HeaderNameExtensions()
		{
			Type mpeg4HeaderNameStartCode = typeof(Mpeg4HeaderName);

			ICollection<Mpeg4HeaderName> allContainerHeaders = new HashSet<Mpeg4HeaderName>();
			_headerNameForHeaderStartCode = new Dictionary<uint, Mpeg4HeaderName>();
			_headerAttributes = new Dictionary<Mpeg4HeaderName, HeaderAttributes>();
			_allowedNextHeaders = new Dictionary<Mpeg4HeaderName, HashSet<uint>>();

			// Create the list to feed to the headers that can be in all container headers
			allContainerHeaders.Add(Mpeg4HeaderName.Root);

			// Use reflection to find the attributes describing the codec identifiers
			foreach (Mpeg4HeaderName headerName in Enum.GetValues(mpeg4HeaderNameStartCode))
			{
				if (headerName == Mpeg4HeaderName.Unknown) continue;

				string name = Enum.GetName(mpeg4HeaderNameStartCode, headerName);

				FieldInfo fieldInfo = mpeg4HeaderNameStartCode.GetField(name);
				Mpeg4Attribute[] attributes = (Mpeg4Attribute[])fieldInfo.GetCustomAttributes(typeof(Mpeg4Attribute), false);

				if (headerName == Mpeg4HeaderName.Root)
				{
					// The root container has no metadata and does not occur in any other header
					_headerAttributes.Add(headerName, new HeaderAttributes(new uint[0], HeaderFlags.MasterHeader, new Mpeg4HeaderName[0]));
				}
				else if (attributes != null && attributes.Length == 1)
				{
					HeaderAttributes headerAttributes = GetHeaderAttributes(headerName, attributes[0], allContainerHeaders);
					_headerAttributes.Add(headerName, headerAttributes);

					// Fill the _headerNameForHeaderStartCode collection
					if (headerName != Mpeg4HeaderName.VopWithShortHeader)	// Special handling for short video headers
					{
						foreach (uint headerStartCode in headerAttributes.HeaderStartCodes)
						{
							_headerNameForHeaderStartCode.Add(headerStartCode, headerName);
						}
					}
				}
				else
				{
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "No attributes for {0}. Please add attributes to the HeaderName enumeration.", headerName));
				}
			}

			// Use reflection to fill the _allowedNextHeaders collection
			// This can not be done in the foreach loop above because the we need the start codes for all
			// Mpeg4HeaderNames. This list is complete when the foreach loop above is done.
			foreach (Mpeg4HeaderName headerName in Enum.GetValues(mpeg4HeaderNameStartCode))
			{
				string name = Enum.GetName(mpeg4HeaderNameStartCode, headerName);

				FieldInfo fieldInfo = mpeg4HeaderNameStartCode.GetField(name);
				Mpeg4Attribute[] attributes = (Mpeg4Attribute[])fieldInfo.GetCustomAttributes(typeof(Mpeg4Attribute), false);

				if(attributes == null || attributes.Length == 0 || attributes[0].AllowedNextHeaders == null) continue;

				HashSet<uint> allowedHeaders = new HashSet<uint>();

				foreach (Mpeg4HeaderName allowedNextHeader in attributes[0].AllowedNextHeaders)
				{
					ICollection<uint> allowedHeaderStartCodes = _headerAttributes[allowedNextHeader].HeaderStartCodes;

					foreach(uint allowedHeaderStartCode in allowedHeaderStartCodes)
					{
						allowedHeaders.Add(allowedHeaderStartCode);
					}
				}
				_allowedNextHeaders.Add(headerName, allowedHeaders );
			}

			CheckSuitableParents();
		}

		/// <summary>
		/// Checks that the suitable parents of all header names are container headers.
		/// </summary>
		private static void CheckSuitableParents()
		{
			foreach (Mpeg4HeaderName headerName in Enum.GetValues(typeof(Mpeg4HeaderName)))
			{
				if (headerName == Mpeg4HeaderName.Unknown) continue;

				foreach (Mpeg4HeaderName suitableParent in headerName.GetSuitableParents())
				{
					Debug.Assert(suitableParent.IsFlagSet(HeaderFlags.Master),
							string.Format(CultureInfo.CurrentCulture, "Suitable parent {0} of header {1} must be a container header.",
									suitableParent, headerName));
				}
			}
		}

		/// <summary>
		/// Gets the header attributes for the given <paramref name="headerName"/>
		/// using the information from the given <paramref name="attribute"/>.
		/// This adds the header to the list of container headers if-and-only-if the
		/// header is a container header.
		/// </summary>
		/// <param name="headerName">the header name</param>
		/// <param name="attribute">the attribute for the given header name</param>
		/// <param name="allContainerHeaders">the list of all container headers</param>
		/// <returns>the header attributes</returns>
		private static HeaderAttributes GetHeaderAttributes(Mpeg4HeaderName headerName, Mpeg4Attribute attribute, ICollection<Mpeg4HeaderName> allContainerHeaders)
		{
			// Retrieve list of header start codes
			List<uint> headerStartCodes = new List<uint>();
			if (attribute.HeaderStartCode != null)
			{
				const string SeparatorCharacter = "-";
				if (attribute.HeaderStartCode.Contains(SeparatorCharacter))
				{
					Debug.Assert(attribute.HeaderStartCode.Length == 5); // Example: "40-5F"

					string[] startCodes = attribute.HeaderStartCode.Split(SeparatorCharacter.ToCharArray());

					uint firstStartCode = uint.Parse(startCodes[0], System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					uint lastStartCode = uint.Parse(startCodes[1], System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);

					Debug.Assert(firstStartCode < lastStartCode);

					for (uint startCode = firstStartCode; startCode <= lastStartCode; startCode++)
					{
						headerStartCodes.Add(0x100 + startCode);
					}
				}
				else
				{
					headerStartCodes.Add(0x100 + uint.Parse(attribute.HeaderStartCode, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
				}
			}

			// Retrieve suitable parents, all container headers if empty list
			ICollection<Mpeg4HeaderName> suitableParents = attribute.SuitableParents;
			if (suitableParents.Count == 0)
			{
				suitableParents = allContainerHeaders;
			}

			// Update list of all container headers
			if ((attribute.HeaderFlags & HeaderFlags.MasterHeader) == HeaderFlags.MasterHeader)
			{
				allContainerHeaders.Add(headerName);
			}
			return new HeaderAttributes(headerStartCodes.AsReadOnly(), attribute.HeaderFlags, suitableParents);
		}

		/// <summary>
		/// Returns whether the given <paramref name="headerFlag"/> is set for
		/// the given <paramref name="headerName"/>.
		/// </summary>
		/// <param name="headerName">the header name</param>
		/// <param name="headerFlag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		public static bool IsFlagSet(this Mpeg4HeaderName headerName, HeaderFlags headerFlag)
		{
			return (headerName.GetHeaderFlags() & headerFlag) == headerFlag;
		}

		/// <summary>
		/// Gets the header flags for the given <paramref name="headerName"/>.
		/// </summary>
		/// <param name="headerName">the header name</param>
		/// <returns>the header flags</returns>
		public static HeaderFlags GetHeaderFlags(this Mpeg4HeaderName headerName)
		{
			return _headerAttributes[headerName].HeaderFlags;
		}

		/// <summary>
		/// Returns whether the given <paramref name="chunkName"/> is a top-level chunk type.
		/// Top-level chunks are only allowed in the root. For example, the Movie chunk.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <returns>true if it is a top-level chunk type, false otherwise</returns>
		public static bool IsTopLevel(this Mpeg4HeaderName chunkName)
		{
			ICollection<Mpeg4HeaderName> suitableParents = chunkName.GetSuitableParents();
			return suitableParents.Count == 1 && suitableParents.Contains(Mpeg4HeaderName.Root);
		}

		/// <summary>
		/// Gets the suitable parents for the given <paramref name="headerName"/>.
		/// </summary>
		/// <param name="headerName">the header name</param>
		/// <returns>the suitable parents</returns>
		public static ICollection<Mpeg4HeaderName> GetSuitableParents(this Mpeg4HeaderName headerName)
		{
			return _headerAttributes[headerName].SuitableParents;
		}

		/// <summary>
		/// Gets the header name from the given <paramref name="startCode"/>.
		/// This returns <code>headerName.Unknown</code> if the start code is unknown.
		/// </summary>
		/// <param name="startCode">the start code for the header start code field</param>
		/// <returns>the header name</returns>
		public static Mpeg4HeaderName GetHeaderName(this uint startCode)
		{
			Mpeg4HeaderName headerName;
			if (!_headerNameForHeaderStartCode.TryGetValue(startCode, out headerName))
			{
				headerName = ((startCode & 0xFFFFFC00) == 0x8000) ? Mpeg4HeaderName.VopWithShortHeader : Mpeg4HeaderName.Unknown;
			}
			return headerName;
		}

		/// <summary>
		/// Returns whether the given <paramref name="headerStartCode"/> is a known header start code.
		/// </summary>
		/// <param name="headerStartCode">the header start code field</param>
		/// <returns>true if it is a known header start code, false otherwise</returns>
		public static bool IsKnownHeaderStartCode(this uint headerStartCode)
		{
			bool shortVideoHeader = ((headerStartCode & 0xFC00) == 0x8000);
			if (shortVideoHeader) return true;

			return _headerNameForHeaderStartCode.ContainsKey(headerStartCode);
		}

		/// <summary>
		/// Returns whether the given <paramref name="headerStartCode"/> is known and allowed to
		/// follow the <paramref name="mpeg4HeaderName"/> header.
		/// </summary>
		/// <param name="headerStartCode">the header start code field</param>
		/// <param name="previousHeader">the header that lies before the <paramref name="headerStartCode"/></param>
		/// <returns>true if it is a known header start code and allowed to follow <paramref name="headerStartCode"/>,
		/// false otherwise</returns>
		public static bool IsKnownHeaderStartCode(this uint headerStartCode, Mpeg4Header previousHeader)
		{
			bool shortVideoHeader = ((headerStartCode & 0xFC00) == 0x8000);
			if (shortVideoHeader) return true;

			HashSet<uint> allowedNextHeaders;
			if (previousHeader != null && _allowedNextHeaders.TryGetValue(previousHeader.HeaderName, out allowedNextHeaders))
			{
				return allowedNextHeaders.Contains(headerStartCode);
			}

			return headerStartCode.IsKnownHeaderStartCode();
		}
	}

	/// <summary>
	/// Specifies the start code, header flags and suitable parent(s) for a header.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class Mpeg4Attribute : Attribute
	{
		#region Properties
		/// <summary>The header start code as string.</summary>
		public string HeaderStartCode { get; private set; }
		/// <summary>The suitable parent for the header.</summary>
		public Mpeg4HeaderName[] SuitableParents { get; private set; }
		/// <summary>The header flags.</summary>
		public HeaderFlags HeaderFlags { get; private set; }
		/// <summary>The headers permitted to succeed the current header</summary>
		public Mpeg4HeaderName[] AllowedNextHeaders { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new header attribute.
		/// </summary>
		/// <remarks>
		/// Use an empty list for <param name="suitableParents"/> to specify
		/// all container headers.
		/// </remarks>
		/// <param name="headerStartCode">the header start code as string</param>
		/// <param name="suitableParents">the suitable parent for the header</param>
		/// <param name="flags">the header start code flags</param>
		public Mpeg4Attribute(string headerStartCode, Mpeg4HeaderName[] suitableParents, HeaderFlags flags)
		{
			HeaderStartCode = headerStartCode;
			SuitableParents = suitableParents;
			HeaderFlags = flags;
		}

		public Mpeg4Attribute(string headerStartCode, Mpeg4HeaderName[] suitableParents, Mpeg4HeaderName[] allowedNextHeaders, HeaderFlags flags)
		{
			HeaderStartCode = headerStartCode;
			SuitableParents = suitableParents;
			AllowedNextHeaders = allowedNextHeaders;
			HeaderFlags = flags;
		}
	}
}
