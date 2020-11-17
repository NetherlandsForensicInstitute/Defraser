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
using System.Globalization;
using System.Linq;
using System.Text;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	internal sealed class CodecStream
	{
		private readonly IDataReader _dataReader;
		private readonly IDataBlockBuilder _dataBlockBuilder;
		private readonly QtAtom _root;
		private readonly QtAtom _moov;
		private readonly QtAtom _mdat;
		private readonly uint _mdatMaxUnparsedBytes;
		private readonly long _fileOffset;
		private Chunk _mdatChunk;
		private bool _truncated;

		internal CodecStream(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, QtAtom root)
		{
			if (dataReader == null) throw new ArgumentNullException("dataReader");
			if (dataBlockBuilder == null) throw new ArgumentNullException("dataBlockBuilder");
			if (root == null) throw new ArgumentNullException("root");

			_dataReader = dataReader;
			_dataBlockBuilder = dataBlockBuilder;
			_root = root;
			_moov = _root.FindChild(AtomName.Movie);
			_mdat = _root.FindChild(AtomName.MediaData);
			_fileOffset = _root.FirstChild.Offset;

			if (_mdat == null)
			{
				_mdatChunk = new Chunk(((QtAtom)_root.GetLastDescendant()).Offset - _fileOffset, 0);
			}
			else
			{
				_mdatChunk = new Chunk(_mdat.Offset - _fileOffset + 8, (_mdat.Length - 8));
			}

			_mdatMaxUnparsedBytes = (uint)QtDetector.Configurable[QtDetector.ConfigurationKey.MediaDateMaxUnparsedBytes];
		}

		internal long? CreateCodecStreams()
		{
			IList<TrackData> tracks = new List<TrackData>();
			bool containsChunks = false;
			bool containsIllegalChunks = false;
			bool containsCompleteTrack = false;
			long firstSampleOffset = _mdatChunk.EndOffset;
			long lastSampleEndOffset = _mdatChunk.Offset;
			long firstInvalidSampleOffset = _mdatChunk.Offset; // RIKKERT: Is dit logisch? Ook met 2 (of meer) video tracks?
			long lastValidSampleOffset = _mdatChunk.Offset;

			// TODO: This assumes there is only one 'mdat' block!!

			if (_moov != null)
			{
				// Process all track atoms in the file
				foreach (QtAtom trackAtom in _moov.Children.Where(r => ((QtAtom)r).HeaderName == AtomName.Track))
				{
					var track = new TrackData(trackAtom, _mdatChunk, _fileOffset);
					track.Read(_dataReader);

					if (track.ContainsIllegalChunks)
					{
						containsIllegalChunks = true;

						TruncateMDat(0); // Avoid chunk validation of remaining tracks
					}
					if (track.ContainsValidData)
					{
						tracks.Add(track);

						if (track.IsComplete)
						{
							containsCompleteTrack = true;
						}
						if (track.ContainsChunks)
						{
							firstSampleOffset = Math.Min(firstSampleOffset, track.FirstSampleOffset);
							lastSampleEndOffset = Math.Max(lastSampleEndOffset, track.LastSampleEndOffset);
							firstInvalidSampleOffset = Math.Max(firstInvalidSampleOffset, track.FirstInvalidSampleOffset);
							lastValidSampleOffset = Math.Max(lastValidSampleOffset, track.LastValidSampleOffset);
							containsChunks = true;
						}
					}
				}
			}

			// If the 'moov' atom is missing or invalid, truncate the 'mdat'
			if (!containsChunks || containsIllegalChunks || !IsValidMDat(firstSampleOffset, lastSampleEndOffset))
			{
				TruncateMDat(0);
				lastValidSampleOffset = _mdatChunk.Offset; // In this case, all samples are considered invalid!
			}
			// If the 'mdat' atom is partially overwritten, truncate it
			if (!_truncated && !containsCompleteTrack)
			{
				TruncateMDat(firstInvalidSampleOffset - _mdatChunk.Offset);
			}

			// Now write out the tracks that contain one or more chunks (after truncation) and/or an 'esds' or 'avcC' atom
			foreach (TrackData track in tracks)
			{
				if (_truncated)
				{
					track.Truncate(_mdatChunk.EndOffset);
				}
				if (track.ContainsValidData)
				{
					track.InitCodecStream(_dataBlockBuilder.AddCodecStream(), _dataReader);
				}
			}
			if (_moov == null)
			{
				CreateCodecStreamsForEsdsAndAvcCAtoms(_root);
			}

			return containsCompleteTrack ? (long?)null : (_fileOffset + lastValidSampleOffset);
		}

		private void CreateCodecStreamsForEsdsAndAvcCAtoms(IResultNode resultNode)
		{
			foreach (IResultNode child in resultNode.Children)
			{
				if (child is AvcConfigurationBox)
				{
					InitCodecStream(_dataBlockBuilder.AddCodecStream(), ((AvcConfigurationBox)child).ExtraData, child);
				}
				else if (child is ElementaryStreamDescriptor)
				{
					InitCodecStream(_dataBlockBuilder.AddCodecStream(), ((ElementaryStreamDescriptor) child).ExtraData, child);
				}

				CreateCodecStreamsForEsdsAndAvcCAtoms(child);
			}
		}

		private static void InitCodecStream(ICodecStreamBuilder builder, IDataPacket data, IResultNode headerDataAtom)
		{
			builder.Name = GetExtraDataCodecStreamName(headerDataAtom);
			// The DataFormat will be set by the codec detector (if one exists)
			builder.Data = data;
			builder.AbsoluteStartOffset = data.StartOffset;
		}

		private static string GetExtraDataCodecStreamName(IResultNode headerDataAtom)
		{
			QtAtom track = FindParentAtom(headerDataAtom, AtomName.Track);
			QtAtom sampleDescription = FindParentAtom(headerDataAtom, AtomName.SampleDescription);
			return GetCodecStreamName(track, sampleDescription, true);
		}

		private static QtAtom FindParentAtom(IResultNode atom, AtomName atomName)
		{
			for (var parent = atom.Parent as QtAtom; !parent.IsRoot; parent = parent.Parent as QtAtom)
			{
				if (parent.HeaderName == atomName)
				{
					return parent;
				}
			}
			return null;
		}

		private static string GetCodecStreamName(QtAtom track, QtAtom sampleDescription, bool isPartial)
		{
			var sb = new StringBuilder();

			if (track != null)
			{
				// Report the track ID
				var trackHeader = track.FindChild(AtomName.TrackHeader) as TrackHeader;
				if (trackHeader != null)
				{
					sb.AppendFormat(CultureInfo.CurrentCulture, "Track {0}", trackHeader.TrackID);
				}
				if (isPartial)
				{
					sb.Append(" [Partial]");
				}

				// Report the handler type (audio/video) and the handler component name
				var handlerReference = track.FindChild(AtomName.HandlerReference, true) as HandlerReference;
				if (handlerReference != null)
				{
					sb.AppendFormat(CultureInfo.CurrentCulture, ", {0}", handlerReference.ComponentSubType);
					if (!string.IsNullOrEmpty(handlerReference.ComponentName))
					{
						sb.AppendFormat(CultureInfo.CurrentCulture, ", {0}", handlerReference.ComponentName);
					}
				}
			}
			else if (isPartial)
			{
				sb.Append("[Partial]");
			}

			// Report the codec name + description
			if ((sampleDescription != null) && sampleDescription.HasChildren())
			{
				string typeName = Enum.GetName(typeof(QtAtom.Attribute), QtAtom.Attribute.Type);
				string dataFormat = sampleDescription.Children[0].FindAttributeByName(typeName).ValueAsString;

				if (!string.IsNullOrEmpty(dataFormat))
				{
					sb.AppendFormat(CultureInfo.CurrentCulture, ", {0}", dataFormat);
					sb.AppendDescriptiveCodecName(dataFormat);
				}
			}
			return sb.ToString();
		}

		private bool IsValidMDat(long firstSampleOffset, long lastSampleEndOffset)
		{
			var byteRange = (lastSampleEndOffset - firstSampleOffset);
			return byteRange >= (_mdatChunk.Length - _mdatMaxUnparsedBytes);
		}

		private void TruncateMDat(long length)
		{
			_truncated = true;
			_mdatChunk = new Chunk(_mdatChunk.Offset, length);
		}

		private sealed class TrackData
		{
			private delegate bool VideoSampleValidator(IDataReader dataReader, Chunk sample, byte[] headerBytes);

			private readonly QtAtom _track;
			private readonly QtAtom _sampleTable;
			private readonly SampleDescription _sampleDescription;
			private readonly HandlerReference _handlerReference;
			private readonly ChunkOffset _chunkOffset;
			private readonly SampleSize _sampleSize;
			private readonly SampleToChunk _sampleToChunk;
			private readonly IList<Chunk> _chunks;
			private readonly IList<VideoSampleValidator> _validators;
			private readonly IList<VideoSampleValidator> _validatorsToRemove;
			private readonly Chunk _mdatChunk;
			private readonly long _fileOffset;
			private readonly byte[] _buffer;
			private IDataPacket _extraData;
			private bool _requiredAtomsMissing;
			private bool _truncated;

			public long FirstSampleOffset { get; private set; }
			public long LastSampleEndOffset { get; private set; }
			public long LastValidSampleOffset { get; private set; }
			public long FirstInvalidSampleOffset { get; private set; }
			public bool ContainsValidData { get { return ContainsChunks || (_extraData != null); } }
			public bool IsComplete { get { return ContainsChunks && !ContainsIllegalChunks && (_validators.Count > 0); } }
			public bool ContainsChunks { get { return _chunks.Count > 0; } }
			public bool ContainsIllegalChunks { get; private set; }

			internal TrackData(QtAtom track, Chunk mdatChunk, long fileOffset)
			{
				_track = track;
				_mdatChunk = mdatChunk;
				_fileOffset = fileOffset;
				_buffer = new byte[8];
				_sampleTable = GetRequiredAtom<QtAtom>(track, AtomName.SampleTable, true);
				_handlerReference = GetRequiredAtom<HandlerReference>(track, AtomName.HandlerReference, true);
				_sampleDescription = GetRequiredAtom<SampleDescription>(_sampleTable, AtomName.SampleDescription);
				_chunkOffset = GetRequiredAtom<ChunkOffset>(_sampleTable, AtomName.ChunkOffset);
				_sampleSize = GetRequiredAtom<SampleSize>(_sampleTable, AtomName.SampleSize);
				_sampleToChunk = GetRequiredAtom<SampleToChunk>(_sampleTable, AtomName.SampleToChunk);
				_chunks = new List<Chunk>();

				_validators = new List<VideoSampleValidator> { IsValidH264Sample, IsValidMpeg4Sample, IsValidH263Sample };
				_validatorsToRemove = new List<VideoSampleValidator>();

				FirstSampleOffset = long.MaxValue;
				LastSampleEndOffset = long.MinValue;
			}

			private bool IsValidH264Sample(IDataReader dataReader, Chunk sample, byte[] headerBytes)
			{
				var size1 = (headerBytes[0] << 24) | (headerBytes[1] << 16) | (headerBytes[2] << 8) | headerBytes[3];
				if ((size1 + 4) == sample.Length)
				{
					return true;
				}

				// Some samples contain several slices!
				for (var totalSize = 0; totalSize < sample.Length; )
				{
					ReadChunkHeaderBytes(new Chunk(sample.Offset + totalSize, sample.Length - totalSize), dataReader);

					var size = (headerBytes[0] << 24) | (headerBytes[1] << 16) | (headerBytes[2] << 8) | headerBytes[3];
					if (size < 2)
					{
						return false;
					}

					totalSize += (size + 4);

					if (totalSize > sample.Length)
					{
						return false;
					}

					dataReader.Position += (size + 4);
				}
				return true; //totalSize == sample.Length;
			}

			private static bool IsValidMpeg4Sample(IDataReader dataReader, Chunk sample, byte[] headerBytes)
			{
				return (headerBytes[0] == 0) && (headerBytes[1] == 0) && (headerBytes[2] == 1) && IsValidMpeg4StartCodeValue(headerBytes[3]);
			}

			private static bool IsValidH263Sample(IDataReader dataReader, Chunk sample, byte[] headerBytes)
			{
				return (headerBytes[0] == 0) && (headerBytes[1] == 0) && ((headerBytes[2] & 0xfc) == 0x80) && ((headerBytes[3] & 0x03) == 0x02);
			}

			// See Table 6-3 of ISO/IEC 14496-2:2004, pg. 39
			private static bool IsValidMpeg4StartCodeValue(uint value)
			{
				return (value <= 0x5f) || ((value >= 0xb0) && (value <= 0xc3));
			}

			private T GetRequiredAtom<T>(QtAtom parent, AtomName atomName) where T : QtAtom
			{
				return GetRequiredAtom<T>(parent, atomName, false);
			}

			private T GetRequiredAtom<T>(QtAtom parent, AtomName atomName, bool recursive) where T : QtAtom
			{
				T atom = (parent == null) ? null : parent.FindChild(atomName, recursive) as T;
				if (atom == null)
				{
					_requiredAtomsMissing = true;
				}
				return atom;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="dataReader"></param>
			/// <returns>true if the track was completely validated</returns>
			public void Read(IDataReader dataReader)
			{
				_extraData = GetExtraData(dataReader);

				if (_requiredAtomsMissing)
				{
					// TODO: perhaps we have an 'esd' or 'avcC' atom!?
					return; // Required atoms missing for chunks, but 'mdat' may still be valid.
				}

				var byteStreamDataReader = new ByteStreamDataReader(dataReader);
				var chunkOffsetTable = _chunkOffset.GetOffsetTable(byteStreamDataReader);
				var sampleSizeTable = _sampleSize.GetSampleSizeTable(byteStreamDataReader);
				var sampleToChunkTable = _sampleToChunk.GetSampleToChunkTable(byteStreamDataReader);
				var sampleSizeValue = ((sampleSizeTable == null) || (sampleSizeTable.Count == 0)) ? _sampleSize.GetSampleSizeValue(byteStreamDataReader) : 0;

				// Add chunks that are contained within the 'mdat' atom
				int sampleToChunkTableIndex = 0;
				int sampleSizeTableIndex = 0;
				for (int chunkIndex = 0; chunkIndex < chunkOffsetTable.Count; chunkIndex++)
				{
					int chunkNumber = (chunkIndex + 1);
					sampleToChunkTableIndex = FindSampleToChunkTableIndex(sampleToChunkTable, sampleToChunkTableIndex, chunkNumber);

					long offset = chunkOffsetTable[chunkIndex];

					uint samplesPerChunk = sampleToChunkTable[sampleToChunkTableIndex].SamplesPerChunk;
					if ((sampleSizeTable == null) || (sampleSizeTable.Count == 0))
					{
						long size = (samplesPerChunk * sampleSizeValue);
						if (!CheckSampleOrChunk(dataReader, offset, size))
						{
							return;
						}
						_chunks.Add(new Chunk(offset, size));
					}
					else if ((sampleSizeTableIndex + samplesPerChunk) <= sampleSizeTable.Count)
					{
						long totalSize = 0;
						for (int j = 0; j < samplesPerChunk; j++)
						{
							uint size = sampleSizeTable[sampleSizeTableIndex++];
							if (!CheckSampleOrChunk(dataReader, (offset + totalSize), size))
							{
								if (totalSize > 0)
								{
									// Add partially valid chunk
									_chunks.Add(new Chunk(offset, totalSize));
								}
								return;
							}
							totalSize += size;
						}
						if (totalSize > 0)
						{
							_chunks.Add(new Chunk(offset, totalSize));
						}
					}
				}
			}

			private IDataPacket GetExtraData(IDataReader dataReader)
			{
				if (_sampleDescription != null)
				{
					var esds = _sampleTable.FindChild(AtomName.ElementaryStreamDescriptor, true) as ElementaryStreamDescriptor;
					if (esds != null)
					{
						return esds.ExtraData;
					}

					var avcC = _sampleTable.FindChild(AtomName.AvcConfigurationBox, true) as AvcConfigurationBox;
					if (avcC != null)
					{
						return avcC.ExtraData;
					}
				}
				return null;
			}

			private bool CheckSampleOrChunk(IDataReader dataReader, long offset, long size)
			{
				if (size == 0)
				{
					return true; // Skip empty chunks
				}

				// Update and validate sample offset range
				var sample = new Chunk(offset, size);
				if (sample.Offset < FirstSampleOffset)
				{
					FirstSampleOffset = sample.Offset;

					if (FirstSampleOffset < _mdatChunk.Offset)
					{
						// Sample is not within the 'mdat' block
						ContainsIllegalChunks = true;
						return false;
					}
				}
				if (sample.EndOffset > LastSampleEndOffset)
				{
					LastSampleEndOffset = sample.EndOffset;

					if (LastSampleEndOffset > _mdatChunk.EndOffset)
					{
						// Sample is not within the 'mdat' block
						ContainsIllegalChunks = true;
						return false;
					}
				}

				// Try to validate the sample/chunk with all known (video) format validators
				if (_validators.Count > 0)
				{
					if (sample.EndOffset > dataReader.Length)
					{
						_validators.Clear();

						FirstInvalidSampleOffset = (_chunks.Count == 0) ? 0 : Math.Min(sample.Offset, dataReader.Length);
						return true;
					}

					foreach (VideoSampleValidator validator in _validators)
					{
						ReadChunkHeaderBytes(sample, dataReader);

						if (validator(dataReader, sample, _buffer))
						{
							LastValidSampleOffset = sample.Offset;
						}
						else
						{
							FirstInvalidSampleOffset = (_chunks.Count == 0) ? 0 : sample.Offset;
							_validatorsToRemove.Add(validator);
						}
					}
					foreach (VideoSampleValidator validator in _validatorsToRemove)
					{
						_validators.Remove(validator);
					}

					_validatorsToRemove.Clear();
				}
				return true;
			}

			private void ReadChunkHeaderBytes(Chunk chunk, IDataReader dataReader)
			{
				dataReader.Position = _fileOffset + chunk.Offset;

				var bytesRemaining = (dataReader.Length - dataReader.Position);
				if (bytesRemaining > _buffer.Length)
				{
					dataReader.Read(_buffer, 0, _buffer.Length);
				}
				else
				{
					dataReader.Read(_buffer, 0, (int)bytesRemaining);
					Array.Clear(_buffer, (int)bytesRemaining, (_buffer.Length - (int)bytesRemaining));
				}
			}

			/// <summary>
			/// Determines the index in sampleToChunkTable for given chunkNumber.
			/// 
			/// Each entry in the sample-to-chunk table can be used for more
			/// than one chunk. This method locates the index of the entry in
			/// the sample-to-chunk table that should be used for a given chunk.
			/// 
			/// The startIndex is provided for faster lookup (O(N) instead of O(N^2)).
			/// </summary>
			/// <param name="sampleToChunkTable">the sample-to-chunk table</param>
			/// <param name="startIndex">the start index in the sampleToChunkTable</param>
			/// <param name="chunkNumber">the number of the chunk to find (1 for first chunk)</param>
			/// <returns>the index in sampleToChunkTable that corresponds with chunk</returns>
			private static int FindSampleToChunkTableIndex(IList<SampleToChunk.SampleToChunkTableEntry> sampleToChunkTable, int startIndex, int chunkNumber)
			{
				for (; startIndex < (sampleToChunkTable.Count - 1); startIndex++)
				{
					uint nextFirstChunkNumber = sampleToChunkTable[startIndex + 1].FirstChunk;

					if (chunkNumber < nextFirstChunkNumber)
					{
						// The chunk fits within the bounds of this entry
						return startIndex;
					}
				}

				// Use the last entry of the sample-to-chunk table
				return (sampleToChunkTable.Count - 1);
			}

			/// <summary>
			/// Keeps only the chunks that are (partially) contained in the (truncated) mdat.
			/// </summary>
			public void Truncate(long offset)
			{
				IList<Chunk> originalChunks = new List<Chunk>(_chunks);
				_chunks.Clear();

				foreach (Chunk chunk in originalChunks)
				{
					if (chunk.EndOffset > offset)
					{
						if (chunk.Offset < offset)
						{
							_chunks.Add(new Chunk(chunk.Offset, (offset - chunk.Offset)));
						}

						_truncated = true;
						break; // Skip remaining chunks!
					}

					_chunks.Add(chunk);
				}
			}

			public void InitCodecStream(ICodecStreamBuilder builder, IDataReader dataReader)
			{
				builder.Name = GetCodecStreamName();
				// The DataFormat will be set by the codec detector (if one exists)
				builder.Data = GetData(dataReader);
				builder.AbsoluteStartOffset = builder.Data.StartOffset;
			}

			private string GetCodecStreamName()
			{
				return CodecStream.GetCodecStreamName(_track, _sampleDescription, _truncated);
			}

			private IDataPacket GetData(IDataReader dataReader)
			{
				IDataPacket data = _extraData;
				foreach (Chunk chunk in _chunks)
				{
					IDataPacket dp = dataReader.GetDataPacket((_fileOffset + chunk.Offset), chunk.Length);
					data = (data == null) ? dp : data.Append(dp);
				}
				return data;
			}
		}
	}
}
