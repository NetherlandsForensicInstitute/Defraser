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
using Defraser.Util;

namespace Defraser.Detector.Mpeg4
{
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	public class Mpeg4Detector : Detector<Mpeg4Header, Mpeg4HeaderName, Mpeg4Parser>, ICodecDetector
	{
		internal enum ConfigurationKey
		{
			MaxUserDataLength,
			ParserMaxZeroByteStuffingLength,
			VopHeaderMaxUnparsedLength,
			VopHeaderMaxOtherStreamLength,
			MinVopShortHeaderCount,
			TemporalReferenceValueDeltaMax,
			MaxOffsetBetweenHeaders,
		}

		private static readonly string VideoObjectName = Enum.GetName(typeof(Mpeg4HeaderName), Mpeg4HeaderName.VideoObject);
		private static readonly string VideoObjectLayerName = Enum.GetName(typeof(Mpeg4HeaderName), Mpeg4HeaderName.VideoObjectLayer);

		#region Properties
		override public string Name { get { return "MPEG-4 Video/H.263"; } }
		override public string Description { get { return "MPEG-4 Video/H.263 Detector"; } }
		override public string OutputFileExtension { get { return ".m4v"; } }
		override public Type DetectorType { get { return GetType(); } }

		override public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Properties

		static Mpeg4Detector()
		{
			_supportedFormats = new[] { CodecID.Mpeg4Video, CodecID.H263 };

			Configurable = new Configurable<ConfigurationKey>();
			Configurable.Add(ConfigurationKey.MaxUserDataLength, (uint)1000);
			Configurable.Add(ConfigurationKey.ParserMaxZeroByteStuffingLength, (byte)32);
			Configurable.Add(ConfigurationKey.VopHeaderMaxUnparsedLength, (uint)0x20000);
			Configurable.Add(ConfigurationKey.VopHeaderMaxOtherStreamLength, (uint)0x10000);
			Configurable.Add(ConfigurationKey.MinVopShortHeaderCount, 2);
			Configurable.Add(ConfigurationKey.TemporalReferenceValueDeltaMax, (byte)5);
			Configurable.Add(ConfigurationKey.MaxOffsetBetweenHeaders, 0x4000);
		}

		/// <summary>
		/// This method looks for the beginning of an MPEG-4 header and if one is found it will parse it
		/// by calling Parser.Parse().
		/// </summary>
		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			if (ReferenceHeaders != null)
			{
				var referenceHeaders = new Dictionary<IDataPacket, VideoObjectLayer>();
				foreach (IReferenceHeader header in ReferenceHeaders)
				{
					IInputFile headerFile = context.CreateReferenceHeaderFile(header);
					using (var parser = new Mpeg4Parser(new BitStreamDataReader(headerFile.CreateDataReader())))
					{
						Mpeg4Header root;
						while (parser.ParseRoot(root = new Mpeg4Header(context.Detectors), parser.Length))
						{
							VideoObjectLayer vol = root.FindChild(Mpeg4HeaderName.VideoObjectLayer, true) as VideoObjectLayer;
							if (vol != null)
							{
								referenceHeaders[headerFile.CreateDataPacket()] = vol;
							}
						}
					}
				}

				context.ReferenceHeader = referenceHeaders;
			}

			return Carve(dataReader, dataBlockBuilder, context);
		}

		private static IDataBlock Carve(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			// Try to parse a contiguous block of headers
			using (var parser = new Mpeg4Parser(new BitStreamDataReader(dataReader)))
			{
				if (context.ReferenceHeader is Dictionary<IDataPacket, VideoObjectLayer>)
				{
					parser.ReferenceHeaders = context.ReferenceHeader as Dictionary<IDataPacket, VideoObjectLayer>;
				}

				Mpeg4Header root;
				while (parser.ParseRoot(root = new Mpeg4Header(context.Detectors), dataReader.Length))
				{
					Mpeg4Header lastHeader = root.LastHeader;
					if (lastHeader.ZeroByteStuffing > 0)
					{
						lastHeader.TrimZeroByteStuffing(parser);
					}

					// Discard results that do not contain a VOP, short video VOP or VOL.
					// Discard data blocks of only one header that is not a H.263 Short Header VOP.
					// The Short Header VOP counts for two headers because it is hard to correctly
					// parse a Short Header VOP from random bits.
					var requiredMpeg4Headers = new[] { Mpeg4HeaderName.VideoObjectLayer, Mpeg4HeaderName.Vop, Mpeg4HeaderName.VopWithShortHeader };
					if (RequiredHeaderCount(root) && root.ContainsChildren(requiredMpeg4Headers, true) && RequiredShortHeaderCount(root))
					{
						context.Results = root;

						dataBlockBuilder.DataFormat = root.DataFormat;
						var firstChild = ((Mpeg4Header)root.Children[0]);
						dataBlockBuilder.StartOffset = firstChild.Offset;
						var lastChild = ((Mpeg4Header)root.GetLastDescendant());
						var endOffset = (lastChild.Offset + lastChild.Length);
						dataBlockBuilder.EndOffset = endOffset;
						dataReader.Position = endOffset;
						if (parser.ReferenceHeader != null)
						{
							dataBlockBuilder.ReferenceHeaderOffset = parser.ReferenceHeaderPosition - firstChild.Offset;
							dataBlockBuilder.ReferenceHeader = parser.ReferenceHeader;
						}
						return dataBlockBuilder.Build();
					}

					RewindParser(parser, lastHeader);
				}
			}

			dataReader.Position = dataReader.Length;
			return null; // Nothing detected!
		}

		public override bool IsKeyFrame(IResultNode resultNode)
		{
			VopBase vop = resultNode as VopBase;
			return (vop != null) && vop.IsKeyframe();
		}

		public override IDataPacket GetVideoHeaders(IResultNode headerPacket)
		{
			IDataPacket data = null;

			IResultNode parentNode = headerPacket.Parent;
			if (parentNode is GroupOfVop)
			{
				parentNode = parentNode.Parent;
			}
			if (parentNode != null)
			{
				// Add parent parent node (required WHEN available)
				IResultNode parentParentNode = parentNode.Parent;
				if ((parentParentNode != null) && (parentParentNode.Length > 0))
				{
					data = parentParentNode;
				}

				// Add parent node (required)
				if (parentNode.Length > 0)
				{
					data = (data == null) ? parentNode : data.Append(parentNode);
				}
			}
			return data;
		}

		public override IDataPacket GetVideoData(IResultNode resultNode)
		{
			// Mpeg4 doesn't require other data (frames) to make itself visible.
			IDataPacket dataPacket = resultNode;
			// Duplicate the frame data, because otherwise, FFmpeg won't decode it.
			return dataPacket.Append(dataPacket);
		}

		private static bool RequiredHeaderCount(Mpeg4Header root)
		{
			if (root.Children.Count == 0)
			{
				return false; // No headers!
			}
			if ((root.Children.Count > 1) || root.FirstChild.HasChildren())
			{
				return true; // At least 2 headers
			}

			return (root.FirstChild is VopWithShortHeader);
		}

		/// <summary>
		/// If the number of headers is more than MinVopShortHeaderCount
		/// return true. If the number of headers is less and one of the
		/// headers is a short header VOP, return false, else true.
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		private static bool RequiredShortHeaderCount(IResultNode header)
		{
			int minVopShortHeaderCount = (int)Configurable[ConfigurationKey.MinVopShortHeaderCount];

			if (header.Children.Count >= minVopShortHeaderCount) return true;

			foreach (Mpeg4Header childHeader in header.Children)
			{
				if (childHeader.HeaderName == Mpeg4HeaderName.VopWithShortHeader)
					return false;
			}
			return true;
		}

		private static void RewindParser(IDataReader parser, Mpeg4Header lastHeader)
		{
			parser.Position = lastHeader.Offset + lastHeader.Length;
		}

		public override IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec codecParameters)
		{
			// Note: Loop to detect reference headers preceeded by a block of video frames
			//       (e.g. for 3GP files with 'mdat' before the 'moov' atom, so that the slices are located before the SPS/PPS pair)
			while (dataReader.State == DataReaderState.Ready)
			{
				IScanContext scanContext = new ReferenceHeaderScanContext {Detectors = new[] {this}};

				var dataBlock = Carve(dataReader, new ReferenceHeaderDataBlockBuilder(), scanContext);
				if (dataBlock == null)
				{
					break; // No (more) data blocks detected
				}

				IResultNode videoObject = scanContext.Results.FindChild(VideoObjectName);
				if ((videoObject != null) && videoObject.HasChildren())
				{
					IResultNode videoObjectLayer = videoObject.GetFirstChild();
					if (videoObjectLayer.Name == VideoObjectLayerName)
					{
						return CreateReferenceHeader(dataBlock.DataFormat, videoObject, videoObjectLayer, codecParameters);
					}
				}

				// VideoObject or VideoObjectLayer is missing: Continue scanning after current data block ...
				dataReader.Position = dataBlock.EndOffset;
			}

			return null; // No reference header detected
		}

		private static IDataPacket CreateReferenceHeader(CodecID codec, IDataPacket videoObject, IResultNode videoObjectLayer, ICodecParametersSpec codecParameters)
		{
			// Report codec parameters for reference header
			codecParameters.Codec = codec;
			codecParameters.Width = GetAttribute<uint>(videoObjectLayer, VideoObjectLayer.Attribute.Width);
			codecParameters.Height = GetAttribute<uint>(videoObjectLayer, VideoObjectLayer.Attribute.Height);
			codecParameters["VTIRate"] = GetAttributeStringValue(videoObjectLayer, VideoObjectLayer.Attribute.VopTimeIncrementResolution);

			return videoObject.Append(videoObjectLayer);
		}

		private static T GetAttribute<T>(IResult result, object attributeName)
		{
			return (T)result.FindAttributeByName(Enum.GetName(attributeName.GetType(), attributeName)).Value;
		}

		private static string GetAttributeStringValue(IResult result, object attributeName)
		{
			return result.FindAttributeByName(Enum.GetName(attributeName.GetType(), attributeName)).ValueAsString;
		}
	}
}
