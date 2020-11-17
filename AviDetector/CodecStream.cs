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
using System.Text;
using Defraser.Detector.Common;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Avi
{
	class CodecStream : ICodecStreamExtractor
	{
		private readonly AviChunk _root;

		public CodecStream(AviChunk root)
		{
			if (root == null) throw new ArgumentNullException("root");

			_root = root;
		}

		public void CreateCodecStreams(IDataBlockBuilder dataBlockBuilder)
		{
			if (dataBlockBuilder == null) throw new ArgumentNullException("dataBlockBuilder");

			IList<HeaderList> headerLists = GetAviStrlHeaderLists(_root);

			IDictionary<byte, ICodecStreamBuilder> codecStreamBuilders = new Dictionary<byte, ICodecStreamBuilder>();

			AddMovieEntries(dataBlockBuilder, codecStreamBuilders, _root, headerLists);

			// Prefix codec stream with extra data (when available) containing SPS and PPS for H.264 videos
			for (byte channel = 0; channel < headerLists.Count; channel++)
			{
				var aviStreamFormat = headerLists[channel].FindChild(AviChunkName.AviStreamFormat) as AviStreamFormat;
				if ((aviStreamFormat != null) && (aviStreamFormat.ExtraData != null) && codecStreamBuilders.ContainsKey(channel))
				{
					ICodecStreamBuilder codecStreamBuilder = codecStreamBuilders[channel];
					codecStreamBuilder.Data = aviStreamFormat.ExtraData.Append(codecStreamBuilder.Data);
				}
			}
		}

		private static IList<HeaderList> GetAviStrlHeaderLists(IResultNode root)
		{
			List<HeaderList> headerLists = new List<HeaderList>();

			GetAviStrlHeaderLists(root, headerLists);

			return headerLists;
		}

		private static void GetAviStrlHeaderLists(IResultNode parent, ICollection<HeaderList> headerLists)
		{
			HeaderList asfObjectToFind = parent as HeaderList;
			if (asfObjectToFind != null && asfObjectToFind.LstType == (uint)ListType.AviStreamHeader)
			{
				headerLists.Add(asfObjectToFind);
				return;
			}

			foreach (IResultNode child in parent.Children)
			{
				GetAviStrlHeaderLists(child, headerLists);
			}
		}

		private static void AddMovieEntries(IDataBlockBuilder dataBlockBuilder, IDictionary<byte, ICodecStreamBuilder> codecStreamBuilders, IResultNode chunk, IEnumerable<HeaderList> headerLists)
		{
			MovieEntry movieEntry = chunk as MovieEntry;
			if (movieEntry != null)
			{
				AddMovieEntry(dataBlockBuilder, codecStreamBuilders, movieEntry, headerLists);
			}

			foreach (AviChunk child in chunk.Children)
			{
				AddMovieEntries(dataBlockBuilder, codecStreamBuilders, child, headerLists);
			}
		}

		private static void AddMovieEntry(IDataBlockBuilder dataBlockBuilder, IDictionary<byte, ICodecStreamBuilder> codecStreamBuilders, MovieEntry movieEntry, IEnumerable<HeaderList> headerLists)
		{
			if (movieEntry.StreamType != InformationType.AudioData &&
				movieEntry.StreamType != InformationType.UncompressedVideoFrame &&
				movieEntry.StreamType != InformationType.CompressedVideoFrame)
			{
				//if (movieEntry.StreamType == InformationType.PaletteChanged) { TODO ask Rikkert }
				return;
			}

			IDataPacket streamData = movieEntry.StreamData;
			if (streamData == null) return;

			byte channel = movieEntry.Channel;
			if (codecStreamBuilders.ContainsKey(channel))
			{
				ICodecStreamBuilder codecStreamBuilder = codecStreamBuilders[channel];
				codecStreamBuilder.Data = codecStreamBuilder.Data.Append(streamData);
			}
			else
			{
				ICodecStreamBuilder codecStreamBuilder = dataBlockBuilder.AddCodecStream();
				codecStreamBuilder.Name = GetCodecStreamName(channel, movieEntry, headerLists);
				codecStreamBuilder.AbsoluteStartOffset = streamData.StartOffset;
				codecStreamBuilder.Data = streamData;

				// The DataFormat will be set by the codec detector (if one exists)
				//codecStreamBuilder.DataFormat = GetDataFormat(headerLists);
				codecStreamBuilders.Add(channel, codecStreamBuilder);
			}
		}

		private static string GetCodecStreamName(byte streamNumber, MovieEntry movieEntry, IEnumerable<HeaderList> headerLists)
		{
			StringBuilder codecStreamName = new StringBuilder();

			codecStreamName.AppendFormat(CultureInfo.CurrentCulture, "Stream {0}", streamNumber);

			if (movieEntry != null)
			{
				// The Stream Type
				codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", Enum.GetName(typeof(InformationType), movieEntry.StreamType));

				if (movieEntry.StreamType == InformationType.AudioData)
				{
					Pair<AviStreamFormat, AviStreamHeader> aviStreamFormatAndHeader = GetAviStreamFormatAndHeader(headerLists, StreamType.Audio);

					if (aviStreamFormatAndHeader != null)
					{
						AviStreamFormat aviStreamFormat = aviStreamFormatAndHeader.First;

						if (Enum.IsDefined(typeof(AviStreamFormat.FormatTag), (int)aviStreamFormat.FormatTagValue))
						{
							codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", Enum.GetName(typeof(AviStreamFormat.FormatTag), (int)aviStreamFormat.FormatTagValue).ToLower().Replace('_', ' '));
						}
					}
				}
				else if (movieEntry.StreamType == InformationType.CompressedVideoFrame ||
							movieEntry.StreamType == InformationType.UncompressedVideoFrame)
				{
					Pair<AviStreamFormat, AviStreamHeader> aviStreamFormatAndHeader = GetAviStreamFormatAndHeader(headerLists, StreamType.Video);
					if (aviStreamFormatAndHeader != null)
					{
						AviStreamFormat aviStreamFormat = aviStreamFormatAndHeader.First;
						AviStreamHeader aviStreamHeader = aviStreamFormatAndHeader.Second;

						string handler = aviStreamHeader.Handler.ToString4CC().ToLower(CultureInfo.CurrentCulture);
						string compression = aviStreamFormat.Compression.ToString4CC().ToLower(CultureInfo.CurrentCulture);

						if (!string.IsNullOrEmpty(handler))
						{
							codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", handler);
							codecStreamName.AppendDescriptiveCodecName(handler);
						}
						if (compression != handler && !string.IsNullOrEmpty(compression))
						{
							codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", compression);
							codecStreamName.AppendDescriptiveCodecName(compression);
						}
					}
				}
			}

			return codecStreamName.ToString();
		}

		private static Pair<AviStreamFormat, AviStreamHeader> GetAviStreamFormatAndHeader(IEnumerable<HeaderList> headerLists, StreamType streamType)
		{
			foreach (IResultNode headerList in headerLists)
			{
				AviStreamFormat aviStreamFormat = null;
				AviStreamHeader aviStreamHeader = null;

				foreach (AviChunk child in headerList.Children)
				{
					if (child is AviStreamFormat)
					{
						aviStreamFormat = (AviStreamFormat)child;
					}
					if (child is AviStreamHeader && ((AviStreamHeader)child).StreamType == (uint)streamType)
					{
						aviStreamHeader = (AviStreamHeader)child;
					}
					if (aviStreamFormat != null && aviStreamHeader != null)
					{
						return new Pair<AviStreamFormat, AviStreamHeader>(aviStreamFormat, aviStreamHeader);
					}
				}
			}
			return null;
		}
	}
}
