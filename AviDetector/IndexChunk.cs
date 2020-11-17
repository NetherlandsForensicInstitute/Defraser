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

using Defraser.Detector.Common;

namespace Defraser.Detector.Avi
{
	internal class IndexChunk : AviChunk
	{
		#region Inner Classes
		private class IndexChunkEntry : CompositeAttribute<Attribute, string, AviParser>
		{
			public enum LAttribute
			{
				/// <summary>
				/// Specifies a FOURCC that identifies a stream in the AVI file.
				/// The FOURCC must have the form 'xxyy' where xx is the stream
				/// number and yy is a two-character code that identifies the
				/// contents of the stream
				/// </summary>
				ChunkID,
				/// <see cref="Flags"/>
				Flags,
				/// <summary>
				/// Specifies the location of the data chunk in the file. The value
				/// should be specified as an offset, in bytes, from the start of
				/// the 'movi' list; however, in some AVI files it is given as an
				/// offset from the start of the file.
				/// </summary>
				Offset,
				/// <summary>
				/// Specifies the size of the data chunk, in bytes.
				/// </summary>
				ChunkSize
			}

			public IndexChunkEntry()
				: base(Attribute.IndexChunkEntry, string.Empty, "{0}")
			{
			}

			public override bool Parse(AviParser parser)
			{
				uint chunkID = parser.GetFourCC(LAttribute.ChunkID);
				int flags = parser.GetInt(LAttribute.Flags);
				int offset = parser.GetInt(LAttribute.Offset);
				int chunkSize = parser.GetInt(LAttribute.ChunkSize);

				TypedValue = string.Format("({0}, {1}, {2}, {3})", chunkID.ToString4CC(), flags, offset, chunkSize);

				return Valid;
			}
		}
		#endregion Inner Classes

		public enum Flags
		{
			/// <summary>
			/// The data chunk is a 'rec ' list.
			/// </summary>
			List = 1,
			/// <summary>
			/// The data chunk is a key frame.
			/// </summary>
			Keyframe = 2,
			/// <summary>
			/// The data chunk does not affect the timing of the stream.
			/// For example, this flag should be set for palette changes.
			/// </summary>
			NoTime = 4
		}

		public new enum Attribute
		{
			IndexChunkTable,
			IndexChunkEntry
		}

		public IndexChunk(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.IndexChunk)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			const uint EntrySize = 16;

			uint numberOfEntries = Size/EntrySize;

			parser.GetTable(Attribute.IndexChunkTable, numberOfEntries, EntrySize, () => new IndexChunkEntry());

			return Valid;
		}
	}
}
