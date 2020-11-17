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
using System.Reflection;
using Defraser.Detector.Common;

namespace Defraser.Detector.Avi
{
	/// <summary>
	/// The chunk type flags.
	/// </summary>
	[Flags]
	internal enum ChunkFlags
	{
		/// <summary>The chunk contains size and type fields.</summary>
		SizeAndType = 1,
		/// <summary>The chunk is a container chunk.</summary>
		Container = 4,
		/// <summary>The flags for container chunks.</summary>
		ContainerChunk = SizeAndType | Container,
	}

	// Headers the AviDetector will find
	public enum AviChunkName
	{
		Root,
		[Avi(null, new AviChunkName[0]/* Allowed in all containers */, ChunkFlags.SizeAndType)]
		Unknown,
		[Avi(null, new AviChunkName[0]/* Allowed in all containers */, 0)]
		PaddingByte,
		[Avi("JUNK", new AviChunkName[0]/* Allowed in all containers */, ChunkFlags.SizeAndType)]
		Junk,
		[Avi("RIFF", new AviChunkName[] { AviChunkName.Root }, ChunkFlags.ContainerChunk)]
		Riff,
		[Avi("LIST", new AviChunkName[] { AviChunkName.Riff, AviChunkName.HeaderList }, ChunkFlags.ContainerChunk)]
		HeaderList,
		[Avi("avih", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviMainHeader,
		[Avi("strf", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviStreamFormat,
		[Avi("strn", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviStreamName,
		[Avi("strh", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviStreamHeader,
		[Avi("idx1", new AviChunkName[] { AviChunkName.Riff }, ChunkFlags.SizeAndType)]
		IndexChunk,
		[Avi("vedt", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviMsVidEditChunk,
		[Avi("dmlh", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		ExtendedAviHeader,
		[Avi("IDIT", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		DigitizingDate,
		[Avi("vprp", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		VideoPropertiesHeader,
		[Avi("indx", new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		AviStreamIndex,
		[Avi(null/*"##db|##dc|##wb|##pc"*/, new AviChunkName[] { AviChunkName.HeaderList }, ChunkFlags.SizeAndType)]
		MovieEntry
		// TODO add ix## / ##ix
	}

		/// <summary>
	/// Provides extension methods to the <c>ChunkName</c> enumeration.
	/// </summary>
	internal static class ChunkNameExtensions
	{
		#region Inner class
		/// <summary>
		/// Stores attributes for the ChunkName enum type extension methods.
		/// </summary>
		private class ChunkAttributes
		{
			public ICollection<uint> ChunkTypes { get; private set; }
			public ICollection<AviChunkName> SuitableParents { get; private set; }
			public ChunkFlags ChunkFlags { get; private set; }

			public ChunkAttributes(ICollection<uint> chunkTypes, ChunkFlags chunkFlags, ICollection<AviChunkName> suitableParents)
			{
				ChunkTypes = chunkTypes;
				SuitableParents = suitableParents;
				ChunkFlags = chunkFlags;
			}
		}
		#endregion Inner class

		private static readonly Dictionary<uint, AviChunkName> ChunkNameForChunkType;
		private static readonly Dictionary<AviChunkName, ChunkAttributes> ChunkAttributesForChunkName;

		/// <summary>Static data initialization.</summary>
		static ChunkNameExtensions()
		{
			Type aviChunkNameType = typeof(AviChunkName);

			ICollection<AviChunkName> allContainerChunks = new HashSet<AviChunkName>();
			ChunkNameForChunkType = new Dictionary<uint, AviChunkName>();
			ChunkAttributesForChunkName = new Dictionary<AviChunkName, ChunkAttributes>();

			// Create the list to feed to the chunks that can be in all container chunks
			allContainerChunks.Add(AviChunkName.Root);

			// Use reflection to find the attributes describing the codec identifiers
			foreach (AviChunkName chunkName in Enum.GetValues(aviChunkNameType))
			{
				string name = Enum.GetName(aviChunkNameType, chunkName);

				FieldInfo fieldInfo = aviChunkNameType.GetField(name);
				AviAttribute[] attributes = (AviAttribute[])fieldInfo.GetCustomAttributes(typeof(AviAttribute), false);

				if (chunkName == AviChunkName.Root)
				{
					// The root container has no metadata and does not occur in any other chunk
					ChunkAttributesForChunkName.Add(chunkName, new ChunkAttributes(new uint[0], ChunkFlags.ContainerChunk, new AviChunkName[0]));
				}
				else if (attributes != null && attributes.Length == 1)
				{
					ChunkAttributes chunkAttributes = GetChunkAttributes(chunkName, attributes[0], allContainerChunks);
					ChunkAttributesForChunkName.Add(chunkName, chunkAttributes);

					// List all 4CC for this chunk
					foreach (uint chunkType in chunkAttributes.ChunkTypes)
					{
						Debug.Assert(!ChunkNameForChunkType.ContainsKey(chunkType), string.Format("Duplicate 4CC '{0}'", chunkType.ToString4CC()));
						ChunkNameForChunkType.Add(chunkType, chunkName);
					}
				}
				else
				{
					Debug.Fail(string.Format("No attributes for {0}. Please add attributes to the ChunkName enumeration.", chunkName));
				}
			}

			CheckSuitableParents();
		}

		/// <summary>
		/// Gets the chunk attributes for the given <paramref name="chunkName"/>
		/// using the information from the given <paramref name="attribute"/>.
		/// This adds the chunk to the list of container chunks if-and-only-if the
		/// chunk is a container chunk.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <param name="attribute">the attribute for the given chunk name</param>
		/// <param name="allContainerChunks">the list of all container chunks</param>
		/// <returns>the chunk attributes</returns>
		private static ChunkAttributes GetChunkAttributes(AviChunkName chunkName, AviAttribute attribute, ICollection<AviChunkName> allContainerChunks)
		{
			// Retrieve list of chunk types (4CC)
			List<uint> chunkTypes = new List<uint>();
			if (attribute.ChunkType != null)
			{
				foreach (string chunkType in attribute.ChunkType.Split("|".ToCharArray()))
				{
					chunkTypes.Add(chunkType.To4CC());
				}
			}

			// Retrieve suitable parents, all container chunks if empty list
			ICollection<AviChunkName> suitableParents = attribute.SuitableParents;
			if (suitableParents.Count == 0)
			{
				suitableParents = allContainerChunks;
			}

			//// Update list of all container chunks
			if ((attribute.ChunkFlags & ChunkFlags.ContainerChunk) == ChunkFlags.ContainerChunk)
			{
				allContainerChunks.Add(chunkName);
			}
			return new ChunkAttributes(chunkTypes.AsReadOnly(), attribute.ChunkFlags, suitableParents);
		}

		/// <summary>
		/// Checks that the suitable parents of all chunk names are container chunks.
		/// </summary>
		private static void CheckSuitableParents()
		{
			foreach (AviChunkName chunkName in Enum.GetValues(typeof(AviChunkName)))
			{
				foreach (AviChunkName suitableParent in chunkName.GetSuitableParents())
				{
					Debug.Assert(suitableParent.IsFlagSet(ChunkFlags.Container),
							string.Format("Suitable parent {0} of chunk {1} must be a container chunk.",
									suitableParent, chunkName));
				}
			}
		}

		/// <summary>
		/// Returns whether the given <paramref name="chunkName"/> is a top-level chunk type.
		/// Top-level chunks are only allowed in the root. For example, the Movie chunk.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <returns>true if it is a top-level chunk type, false otherwise</returns>
		public static bool IsTopLevel(this AviChunkName chunkName)
		{
			ICollection<AviChunkName> suitableParents = chunkName.GetSuitableParents();
			return suitableParents.Count == 1 && suitableParents.Contains(AviChunkName.Root);
		}

		/// <summary>
		/// Gets the suitable parents for the given <paramref name="chunkName"/>.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <returns>the suitable parents</returns>
		public static ICollection<AviChunkName> GetSuitableParents(this AviChunkName chunkName)
		{
			return ChunkAttributesForChunkName[chunkName].SuitableParents;
		}

		/// <summary>
		/// Gets the chunk name from the given <paramref name="fourCC"/>.
		/// This returns <code>ChunkName.Unknown</code> if the FourCC is unknown.
		/// </summary>
		/// <param name="fourCC">the 4-character-code for the chunk type field</param>
		/// <returns>the chunk name</returns>
		public static AviChunkName GetChunkName(this uint fourCC)
		{
			AviChunkName chunkName;
			if (!ChunkNameForChunkType.TryGetValue(fourCC, out chunkName))
			{
				chunkName = AviChunkName.Unknown;
			}
			return chunkName;
		}

		/// <summary>
		/// Returns whether the given <paramref name="chunkFlag"/> is set for
		/// the given <paramref name="chunkName"/>.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <param name="chunkFlag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		public static bool IsFlagSet(this AviChunkName chunkName, ChunkFlags chunkFlag)
		{
			return (chunkName.GetChunkFlags() & chunkFlag) == chunkFlag;
		}

		/// <summary>
		/// Gets the chunk type flags for the given <paramref name="chunkName"/>.
		/// </summary>
		/// <param name="chunkName">the chunk name</param>
		/// <returns>the chunk flags</returns>
		public static ChunkFlags GetChunkFlags(this AviChunkName chunkName)
		{
			return ChunkAttributesForChunkName[chunkName].ChunkFlags;
		}

		/// <summary>
		/// Returns whether the given <paramref name="chunkType"/> is a known chunk type.
		/// </summary>
		/// <param name="chunkType">the chunk type field (4-character-code)</param>
		/// <returns>true if it is a known chunk type, false otherwise</returns>
		public static bool IsKnownChunkType(this uint chunkType)
		{
			return chunkType.IsMovieEntry() || ChunkNameForChunkType.ContainsKey(chunkType);
		}
	}

	/// <summary>
	/// Specifies the 4-character-code, type flags and suitable parent(s) for a chunk.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class AviAttribute : Attribute
	{
		#region Properties
		/// <summary>The chunk type field 4CC as string.</summary>
		public string ChunkType { get; private set; }
		/// <summary>The suitable parent for the chunk.</summary>
		public AviChunkName[] SuitableParents { get; private set; }
		/// <summary>The chunk type flags.</summary>
		public ChunkFlags ChunkFlags { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new chunk attribute.
		/// </summary>
		/// <remarks>
		/// Use an empty list for <param name="suitableParents"/> to specify
		/// all container chunks.
		/// </remarks>
		/// <param name="chunkType">the chunk type field 4CC as string</param>
		/// <param name="suitableParents">the suitable parent for the chunk</param>
		/// <param name="flags">the chunk type flags</param>
		public AviAttribute(string chunkType, AviChunkName[] suitableParents, ChunkFlags flags)
		{
			ChunkType = chunkType;
			SuitableParents = suitableParents;
			ChunkFlags = flags;
		}

		//public AviAttribute(string chunkType, AviChunkName[] suitableParents)
		//{
		//    ChunkType = chunkType;
		//    SuitableParents = suitableParents;
		//    ChunkFlags = ChunkFlags.Unknown;
		//}
	}
}
