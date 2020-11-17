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
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Avi
{
	/// <summary>
	/// ExampleDetector is a template class for detector development for Defraser. The ExampleDetector
	/// searches for an ExampleHeader and when it finds one it will process it.
	/// </summary>
	[DataContract]
	public sealed class AviDetector : Detector<AviChunk, AviChunkName, AviParser>, IConfigurable
	{
		internal enum ConfigurationKey
		{
			AttributeMaxStringLength,
			AviStreamIndexMaxTableEntryCount,
			AviStreamNameMaxNameLength,
			HeaderListMaxDescriptionLength,
			HeaderListMaxPrmiLength,
			DigitizingDateMaxDateStringLength,
			AviChunkMaxUnparsedBytes
		}

		#region Properties
		override public string Name { get { return "AVI/RIFF"; } }
		override public string Description { get { return "AVI detector"; } }
		override public string OutputFileExtension { get { return ".avi"; } }
		override public Type DetectorType { get { return GetType(); } }

		override public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Properties

		static AviDetector()
		{
			_supportedFormats = new[] { CodecID.Avi };

			Configurable = new Configurable<ConfigurationKey>();

			Configurable.Add(ConfigurationKey.AviStreamIndexMaxTableEntryCount, (uint)65536);
			Configurable.Add(ConfigurationKey.AviStreamNameMaxNameLength, (uint)1000);
			Configurable.Add(ConfigurationKey.HeaderListMaxDescriptionLength, (uint)1000);
			Configurable.Add(ConfigurationKey.HeaderListMaxPrmiLength, (uint)1000);
			Configurable.Add(ConfigurationKey.AttributeMaxStringLength, (uint)1000);
			Configurable.Add(ConfigurationKey.DigitizingDateMaxDateStringLength, (uint)100);
			Configurable.Add(ConfigurationKey.AviChunkMaxUnparsedBytes, (uint)(65536 * 20));
		}

		/// This method looks for the beginning of an AviHeader and if one is found it will parse it
		/// by calling Parser.Parse().
		/// </summary>
		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			AviChunk root = new AviChunk(context.Detectors);

			// Try to parse a contiguous block of headers
			using (AviParser parser = new AviParser(new ByteStreamDataReader(dataReader, Endianness.Little)))
			{
				if (!parser.ParseRoot(root, dataReader.Length))
				{
					dataReader.Position = dataReader.Length;
					return null;
				}

				// Synchronize the position of the data reader
				dataReader.Position = parser.Position;
			}

			// The content of a Movie Entry can not be parsed by the avi detector.
			// When a Movie Entry is the last header of a data block,
			// we can't be sure that its correct (not overwritten).
			// Because of this, the content of this last Movie Entry
			// will be fed to other detectors to check its content.
			long? removedLastMovieEntryOffset = null;
			AviChunk lastHeader = root.LastHeader;
			if (lastHeader.HeaderName == AviChunkName.MovieEntry &&
				lastHeader.RelativeEndOffset != (ulong)dataReader.Length)
			{
				removedLastMovieEntryOffset = lastHeader.Offset;
				lastHeader.Parent.RemoveChild(lastHeader);
			}

			// Trim trailing unknown, padding byte or large unparsable chunks
			lastHeader = root.LastHeader;
			while ((lastHeader.IsUnknown || lastHeader.HeaderName == AviChunkName.PaddingByte) && lastHeader.Parent != null)
			{
				// Note: ALWAYS strip padding bytes, to avoid a 'retrieved detectable is different...' error on rescan!
				if (lastHeader.HeaderName != AviChunkName.PaddingByte)
				{
					if (lastHeader.RelativeEndOffset == ((AviChunk) lastHeader.Parent).RelativeEndOffset)
					{
						break; // Last child perfectly aligned with parent chunk
					}
				}

				removedLastMovieEntryOffset = null;
				lastHeader.Parent.RemoveChild(lastHeader);
				lastHeader = root.LastHeader;
			}

			// Discard data blocks of only 1 chunk
			if ((dataReader.State == DataReaderState.Cancelled) ||
				(root.Children.Count == 0) ||
				((root.Children.Count == 1) && !root.FirstChild.HasChildren()))
			{
				// Rewind data reader
				if(removedLastMovieEntryOffset != null)
				{
					// If the last header was a movie entry;
					// proceed just after the 4CC and size bytes of that header.
					dataReader.Position = removedLastMovieEntryOffset.Value + 1;
				}
				else
				{
					dataReader.Position = lastHeader.Offset + lastHeader.Length; // != lastHeader.RelativeEndOffset; !!!
				}
				return null;
			}

			// Set properties and return result
			context.Results = root;

			dataBlockBuilder.DataFormat = root.DataFormat;
			var firstChild = ((AviChunk)root.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((AviChunk)root.GetLastDescendant());
			var endOffset = (lastChild.Offset + lastChild.Length);
			dataBlockBuilder.EndOffset = endOffset;

			// TODO properties.IsFullFile = (root.FirstChild.HeaderName == AviChunkName.FileType) && (movie != null) && (mediaData != null);
			CodecStream codecStream = new CodecStream(root);
			codecStream.CreateCodecStreams(dataBlockBuilder);

			// TODO: check for (complete) mdat block (for IsFullFile)

			dataReader.Position = endOffset;
			return dataBlockBuilder.Build();
		}
	}
}
