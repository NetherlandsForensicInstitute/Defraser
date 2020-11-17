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
using System.Text;
using Defraser.Interface;
using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	class CodecStream : ICodecStreamExtractor
	{
		private readonly AsfObject _root;

		internal CodecStream(AsfObject root)
		{
			_root = root;
		}

		public void CreateCodecStreams(IDataBlockBuilder dataBlockBuilder)
		{
			IDataObject dataObject = GetObject<DataObject>(_root) ?? GetObject<DataObjectWithoutStart>(_root) ?? default(IDataObject);
			if (dataObject == null) return;

			CodecListObject codecList = GetObject<CodecListObject>(_root);	// the value null is okay for this codecList

			IDictionary<byte, ICodecStreamBuilder> codecStreamBuilders = new Dictionary<byte, ICodecStreamBuilder>();

			foreach (DataPacket dataPacket in dataObject.DataPackets)
			{
				foreach (Payload payload in dataPacket.Payloads)
				{
					AddPayload(dataBlockBuilder, codecStreamBuilders, payload, codecList, _root);
				}
			}
		}

		private TObjectToFind GetObject<TObjectToFind>(IResultNode parent)
			where TObjectToFind : AsfObject
		{
			TObjectToFind asfObjectToFind = parent as TObjectToFind;
			if (asfObjectToFind != null)
			{
				return asfObjectToFind;
			}

			foreach (AsfObject child in parent.Children)
			{
				asfObjectToFind = GetObject<TObjectToFind>(child);

				if (asfObjectToFind != null) return asfObjectToFind;
			}
			return asfObjectToFind;

		}

		private StreamPropertiesObject GetStreamProperty(AsfObject asfObject, int streamID)
		{
			StreamPropertiesObject streamProperty = asfObject as StreamPropertiesObject;

			if (streamProperty != null && streamProperty.StreamNumber == streamID)
			{
				return streamProperty;
			}

			foreach (AsfObject child in asfObject.Children)
			{
				streamProperty = GetStreamProperty(child, streamID);

				if (streamProperty != null && streamProperty.StreamNumber == streamID) return streamProperty;
			}
			return streamProperty;
		}

		private void AddPayload(IDataBlockBuilder dataBlockBuilder, IDictionary<byte, ICodecStreamBuilder> codecStreamBuilders, Payload payload, CodecListObject codecList, AsfObject asfObject)
		{
			if (payload.Streams == null || payload.Streams.Count == 0) return;

			byte streamNumber = payload.StreamNumber;
			if (codecStreamBuilders.ContainsKey(streamNumber))
			{
				ICodecStreamBuilder codecStreamBuilder = codecStreamBuilders[streamNumber];

				foreach (IDataPacket streamData in payload.Streams)
				{
					codecStreamBuilder.Data = codecStreamBuilder.Data.Append(streamData);
				}
			}
			else
			{
				StreamPropertiesObject streamProperty = GetStreamProperty(asfObject, streamNumber);

				ICodecStreamBuilder codecStreamBuilder = dataBlockBuilder.AddCodecStream();
				codecStreamBuilder.Name = GetCodecStreamName(streamNumber, streamProperty, codecList);
				// The DataFormat will be set by the codec detector (if one exists)
				//codecStreamBuilder.DataFormat = GetDataFormat(streamProperty);
				codecStreamBuilder.Data = GetStreamData(streamProperty, payload.Streams[0]);
				for (int streamIndex = 1; streamIndex < payload.Streams.Count; streamIndex++)
				{
					codecStreamBuilder.Data = codecStreamBuilder.Data.Append(payload.Streams[streamIndex]);
				}
				codecStreamBuilder.AbsoluteStartOffset = codecStreamBuilder.Data.StartOffset;

				codecStreamBuilders.Add(streamNumber, codecStreamBuilder);
			}
		}

		private static IDataPacket GetStreamData(StreamPropertiesObject streamProperties, IDataPacket streamData)
		{
			// If stream properties contains a data packet with codec specific data,
			// insert that data packet before the stream data
			if (streamProperties != null &&
				streamProperties.CodecSpecificData != null)
			{
				return streamProperties.CodecSpecificData.Append(streamData);
			}
			return streamData;
		}

		private string GetCodecStreamName(byte streamNumber, StreamPropertiesObject streamProperty, CodecListObject codecList)
		{
			StringBuilder codecStreamName = new StringBuilder();

			codecStreamName.AppendFormat(CultureInfo.CurrentCulture, "Stream {0}", streamNumber);

			if (streamProperty != null)	// No stream property ASF-object found
			{
				// The Stream Type
				codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", Enum.GetName(typeof(ObjectStreamTypeGuid), streamProperty.StreamType));

				// The codec 4CC
				if (!string.IsNullOrEmpty(streamProperty.CompressionId))
				{
					codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", streamProperty.CompressionId);
				}

				// The codec name
				string codecName = null;
				if (codecList != null)
				{
					if (codecList.CodecNames.TryGetValue(GetCodecType(streamProperty.StreamType), out codecName))
					{
						codecStreamName.AppendFormat(CultureInfo.CurrentCulture, ", {0}", codecName);
					}
				}
				if (string.IsNullOrEmpty(codecName) && !string.IsNullOrEmpty(streamProperty.CompressionId))
				{
					codecStreamName.AppendDescriptiveCodecName(streamProperty.CompressionId);
				}
			}
			return codecStreamName.ToString();
		}

		private static CodecType GetCodecType(ObjectStreamTypeGuid objectStreamTypeGuid)
		{
			if (objectStreamTypeGuid == ObjectStreamTypeGuid.AudioMedia) return CodecType.AudioCodec;
			if (objectStreamTypeGuid == ObjectStreamTypeGuid.VideoMedia) return CodecType.VideoCodec;
			return CodecType.UnknownCodec;
		}
	}
}
