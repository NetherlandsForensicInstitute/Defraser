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
using System.Runtime.Serialization;
using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Mpeg2.Video
{
	[DataContract]
	public sealed class Mpeg2VideoDetector : ICodecDetector
	{
		public const string DetectorName = "MPEG-1/2 Video";

		#region Configuration
		internal enum ConfigurationKey
		{
			/// <summary>
			/// The maximum number of headers in one result.
			/// This value is introduced to protect the application from an
			/// out-of-memory exception by having too many headers at once in memory.
			/// </summary>
			MaxVideoHeaderCount,
			/// <summary>
			/// The minimum number of headers required to be considered a valid result.
			/// </summary>
			MinVideoHeaderCount,
			UserDataMaxLength,
			SliceMaxLength,
			ParserMaxZeroByteStuffingLength,
			MaximumSliceNumberIncrement,
			PictureHeaderMaxLengthOfExtraInformation
		}

		static Mpeg2VideoDetector()
		{
			Configurable = new Configurable<ConfigurationKey>();
			Configurable.Add(ConfigurationKey.MaxVideoHeaderCount, (uint)100000);
			Configurable.Add(ConfigurationKey.MinVideoHeaderCount, (uint)2);
			Configurable.Add(ConfigurationKey.UserDataMaxLength, (uint)4096);
			Configurable.Add(ConfigurationKey.SliceMaxLength, (uint)0x20000);
			Configurable.Add(ConfigurationKey.ParserMaxZeroByteStuffingLength, (uint)32);
			Configurable.Add(ConfigurationKey.MaximumSliceNumberIncrement, (uint)2);
			Configurable.Add(ConfigurationKey.PictureHeaderMaxLengthOfExtraInformation, (uint)1000);
		}

		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Configuration

		#region Properties
		public string Name { get { return DetectorName; } }
		public string Description { get { return "MPEG-1 and 2 Video detector"; } }
		public string OutputFileExtension { get { return ".m2v"; } }
		public IDictionary<string, string[]> Columns { get { return _detectorColumns.Columns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return new[] { CodecID.Mpeg1Video, CodecID.Mpeg2Video }; } }
		public Type DetectorType { get { return typeof(Mpeg2VideoDetector); } }
		public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		public IEnumerable<IReferenceHeader> ReferenceHeaders { get; set; }
		#endregion Properties

		private readonly IDetectorColumns _detectorColumns;
		private readonly ICarver _carver;

		public Mpeg2VideoDetector()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new DetectorCommonModule());
			builder.RegisterModule(new Mpeg2VideoModule());
			IContainer container = builder.Build();

			_detectorColumns = container.Resolve<IDetectorColumns>();
			_carver = container.Resolve<ICarver>();
		}

		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			bool isFragmented = context.IsFragmented;

			if (ReferenceHeaders != null)
			{
				var referenceHeaders = new Dictionary<IDataPacket, ISequenceState>();
				foreach (IReferenceHeader header in ReferenceHeaders)
				{
					IInputFile headerFile = context.CreateReferenceHeaderFile(header);
					using (var reader = headerFile.CreateDataReader())
					{
						var headerDataBlock = _carver.Carve(new BitStreamDataReader(reader), dataBlockBuilder, context);
						if (headerDataBlock != null)
						{
							referenceHeaders[headerFile.CreateDataPacket()] = context.ReferenceHeader as ISequenceState;
						}
					}
				}

				context.ReferenceHeader = referenceHeaders;
			}

			// Restore scan context
			context.Results = null;
			context.IsFragmented = isFragmented;
			// context.ReferenceHeader

			return _carver.Carve(new BitStreamDataReader(dataReader), dataBlockBuilder, context);
		}

		public bool IsKeyFrame(IResultNode resultNode)
		{
			if (resultNode.Name != PictureHeader.Name)
			{
				return false;
			}

			var pictureCodingTypeAttribute = resultNode.FindAttributeByName(Enum.GetName(typeof(PictureHeader.Attribute), PictureHeader.Attribute.PictureCodingType));
			if (pictureCodingTypeAttribute == null)
			{
				return false;
			}

			return (int)pictureCodingTypeAttribute.Value == (int)PictureCodingType.IType;
		}

		public IDataPacket GetVideoHeaders(IResultNode headerPacket)
		{
			IResultNode pictureNode = GetPictureNode(headerPacket);

			IDataPacket data = null;
			for (IResultNode node = pictureNode.Parent; node != null; node = node.Parent)
			{
				if (node.Name == SequenceHeader.Name)
				{
					// Check for 'SequenceExtension', required for MPEG2
					foreach (IResultNode childNode in node.Children)
					{
						if (childNode.Name == SequenceExtension.Name)
						{
							data = data.Prepend(childNode);
						}
					}
				}
				if (node.Length > 0)
				{
					// Add parent node
					data = data.Prepend(node);
				}
			}
			return data;
		}

		public IDataPacket GetVideoData(IResultNode resultNode)
		{
			IResultNode pictureNode = GetPictureNode(resultNode);

			// Add the frame data
			IDataPacket data = pictureNode;
			foreach (IResultNode childResultNode in pictureNode.Children)
			{
				data = data.Append(childResultNode);

				if (childResultNode == resultNode)
				{
					break; // The slice that was selected has been reached!
				}
			}

			// Duplicate the frame data, because otherwise, FFmpeg won't decode it.
			return data.Append(data);
		}

		private static IResultNode GetPictureNode(IResultNode resultNode)
		{
			if (resultNode.Name == Slice.Name)
			{
				return resultNode.Parent;
			}

			return resultNode; // Note: This is assumed to be the picture header!
		}

		public IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec codecParameters)
		{
			while (dataReader.State == DataReaderState.Ready)
			{
				IScanContext scanContext = new ReferenceHeaderScanContext {Detectors = new[] {this}};

				var dataBlock = _carver.Carve(new BitStreamDataReader(dataReader), new ReferenceHeaderDataBlockBuilder(), scanContext);
				if (dataBlock == null)
				{
					break; // No (more) data blocks detected
				}

				IResultNode sequenceHeader = scanContext.Results.FindChild(SequenceHeader.Name);
				if (sequenceHeader != null)
				{
					return CreateReferenceHeader(dataBlock.DataFormat, sequenceHeader, codecParameters);
				}

				// Sequence header is missing: Continue scanning after current data block ...
				dataReader.Position = dataBlock.EndOffset;
			}

			return null; // No reference header detected
		}

		private static IDataPacket CreateReferenceHeader(CodecID codec, IResultNode sequenceHeader, ICodecParametersSpec codecParameters)
		{
			IDataPacket data = sequenceHeader;

			// Include sequence extensions
			foreach (IResultNode childResultNode in sequenceHeader.Children)
			{
				if (!IsSequenceExtension(childResultNode.Name))
				{
					break;
				}

				data = data.Append(childResultNode);
			}

			// Report codec parameters for reference header
			var width = GetAttribute<uint>(sequenceHeader, SequenceHeader.Attribute.HorizontalSizeValue);
			var height = GetAttribute<uint>(sequenceHeader, SequenceHeader.Attribute.VerticalSizeValue);

			if (sequenceHeader.HasChildren())
			{
				IResult sequenceExtension = sequenceHeader.GetFirstChild();
				if (sequenceExtension.Name == SequenceExtension.Name)
				{
					width |= GetAttribute<uint>(sequenceExtension, SequenceExtension.Attribute.HorizontalSizeExtension) << 12;
					height |= GetAttribute<uint>(sequenceExtension, SequenceExtension.Attribute.VerticalSizeExtension) << 12;
				}
			}

			codecParameters.Codec = codec;
			codecParameters.Width = width;
			codecParameters.Height = height;
			codecParameters.FrameRate = GetAttributeStringValue(sequenceHeader, SequenceHeader.Attribute.FrameRateCode);

			return data;
		}

		private static bool IsSequenceExtension(string name)
		{
			return (name == SequenceExtension.Name) || (name == SequenceDisplayExtension.Name) || (name == SequenceScalableExtension.Name);
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
