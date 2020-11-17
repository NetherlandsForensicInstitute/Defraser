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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Avi
{
	/// <summary>
	/// Generic AVI header.
	/// </summary>
	public class AviChunk : Header<AviChunk, AviChunkName, AviParser>
	{
		/// <summary>The possible Attributes of a detected header.</summary>
		public enum Attribute
		{
			Type,
			Size,
			AdditionalData
		}

		private const int FourCCAndSizeLength = 8;

		#region Properties
		/// <summary>
		/// When the file type is not found it will be set to AVI
		/// </summary>
		public override CodecID DataFormat
		{
			get
			{
				if (!Root.HasChildren()) return CodecID.Avi;

				foreach (AviChunk child in Root.Children)
				{
					if (child.HeaderName == AviChunkName.Riff)
					{
						Riff riff = child as Riff;
						if (riff == null) return CodecID.Avi;

						if (riff.FileType == "AVI ".To4CC()) return CodecID.Avi;
						if (riff.FileType == "AVIX".To4CC()) return CodecID.Avi;
						if (riff.FileType == "WAVE".To4CC()) return CodecID.Wave;
						if (riff.FileType == "ACON".To4CC()) return CodecID.Acon;
						if (riff.FileType == "CDR6".To4CC()) return CodecID.Cdr6;

						// An unknown file type was found
						return CodecID.Unknown;
					}
				}
				return CodecID.Avi;
			}
		}
		public uint Size { get; protected set; }
		internal ulong RelativeEndOffset
		{
			get
			{
				ulong endOffset = (ulong)this.Offset + this.Size;
				if (HeaderName.IsFlagSet(ChunkFlags.SizeAndType))
				{
					endOffset += FourCCAndSizeLength; 
				}
				return endOffset;
			}
		}
		internal ulong UnparsedBytes { get; private set; }
		/// <summary>
		/// The chunk type or large part of its data is unkown.
		/// </summary>
		internal bool IsUnknown
		{
			get
			{
				return HeaderName == AviChunkName.Unknown || UnparsedBytes > MaxUnparsedBytes;
			}
		}

		// The maximum number of unparsed bytes allowed for trailing chunks.
		protected uint MaxUnparsedBytes
		{
			get { return (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.AviChunkMaxUnparsedBytes]; }
		}
		#endregion Properties

		#region Constructors
		/// <summary>Constructs a new top level header.</summary>
		public AviChunk(IEnumerable<IDetector> detectors)
			: base(detectors, AviChunkName.Root)
		{
		}

		public AviChunk(AviChunk previousHeader, AviChunkName aviChunkName)
			: base(previousHeader, aviChunkName)
		{
		}
		#endregion Constructors

		/// <summary>
		/// Parse the AVIHeader.
		/// </summary>
		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (HeaderName.IsFlagSet(ChunkFlags.SizeAndType))
			{
				// Parse the type and size of the header.
				parser.GetFourCC(Attribute.Type);
				Size = (uint)parser.GetInt(Attribute.Size);

				// 3GPP only
				//if (_size == 1)
				//{
				//    // 64-bit size chunk
				//    parser.GetULong(Attribute.Size);
				//}
			}

			// 3GPP only
			//if (HeaderName.IsFlagSet(ChunkFlags.VersionAndFlags))
			//{
			//    parser.GetByte(Attribute.Version);
			//    ParseFlags(parser);
			//}
			return this.Valid;
		}

		/// <summary>
		/// This method is called from Parser.Parse() and shall contain the code to
		/// detect adjacent headers.
		/// </summary>
		public override bool IsBackToBack(AviChunk header)
		{
			return true;
		}

		public override bool IsSuitableParent(AviChunk parent)
		{
			if (!parent.IsRoot)
			{
				// Should fit in parent chunk (box)
				if (this.Offset < parent.Offset || this.RelativeEndOffset > parent.RelativeEndOffset)
				{
					return false;
				}
			}

			// First chunk should not be unknown
			if (IsUnknown && parent.IsRoot && !parent.HasChildren())
			{
				return false;
			}

			//// Chunk should have no duplicates in its parent if DuplicatesAllowed is false
			//if (!HeaderName.IsFlagSet(ChunkFlags.DuplicatesAllowed) && parent.HasChild(HeaderName))
			//{
			//    // Chunks of partial files may end up in the root
			//    // Therefore, duplicates are allowed, except for top-level chunks
			//    if (!parent.IsRoot || HeaderName.IsTopLevel())
			//    {
			//        return false;
			//    }
			//}

			// Chunk is allowed in its suitable parent (correct files)
			if (HeaderName.GetSuitableParents().Contains(parent.HeaderName))
			{
				return true;
			}

			// Chunk ends up in the root if no suitable parent was found (partial files)
			if (!parent.IsRoot)
			{
				return false;
			}

			// Chunks of partial files are allowed in the root if empty
			if (!parent.HasChildren())
			{
				return true;
			}

			// Root should not already contain top-level chunks (FileType, MediaData or Movie)
			// Otherwise, the top-level parent of this chunk should have been in the root as well
			foreach (AviChunk chunk in parent.Children)
			{
				if (chunk.HeaderName.IsTopLevel())
				{
					return false;
				}
			}

			// Depth of this chunk should be equal or lower than last child of the root
			//if (!IsChunkTypeAllowedInRootAtThisPoint(parent, HeaderName))
			//{
			//    return false;
			//}

			// Root should not contain only suitable ancestor of the chunk
			//if (false)
			//{
			//    return false;
			//}

			// No 'higher chunks' than its suitable parent (siblings of an ancestor)
			//if (false)
			//{
			//    return false;
			//}

			return true;
		}

		/// <summary>
		/// Returns whether this chunk contains a child of the given type.
		/// </summary>
		/// <param name="chunkName">the chunk name type</param>
		/// <returns>true if this chunk contains such as child, false otherwise</returns>
		internal bool HasChild(AviChunkName chunkName)
		{
			return FindChild(chunkName) != null;
		}

		public override bool ParseEnd(AviParser parser)
		{
			if (parser.ReadOverflow)
			{
				this.Valid = false;
				return false;
			}

			if (!HeaderName.IsFlagSet(ChunkFlags.ContainerChunk))
			{
				// Chunks extending beyond the end of the file are considered invalid
				if (RelativeEndOffset > (ulong)parser.Length)
				{
					this.Valid = false;
					return false;
				}

				// Record unparsed bytes statistic
				ulong endOffset = Math.Min(RelativeEndOffset, (ulong)parser.Length);
				if ((ulong)parser.Position < endOffset)
				{
					UnparsedBytes = endOffset - (ulong)parser.Position;

					if (!HeaderName.IsFlagSet(ChunkFlags.Container) && (UnparsedBytes > MaxUnparsedBytes)) return false;

					parser.GetHexDump(Attribute.AdditionalData, (int)UnparsedBytes);
				}
			}
			if (!this.Valid || (ulong)parser.Position > RelativeEndOffset)
			{
				return false;
			}
			return base.ParseEnd(parser);
		}
	}
}
