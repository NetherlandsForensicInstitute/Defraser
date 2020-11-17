/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	/// <summary>
	/// Defraser detector for the H.264 / MPEG-4 AVC format
	/// </summary>
	/// <remarks>
	/// Paragraph references are references to the specification document:
	/// 'T-REC-H.264-200503-I-Advanced video coding'
	/// </remarks>
	[DataContract]
	public sealed class H264Detector : ICodecDetector
	{
		public const string DetectorName = "H.264";

		#region Configuration
		internal enum ConfigurationKey
		{
			/// <summary>
			/// The maximum number of bytes between a Picture Parameter Set (PPS)
			/// and the following slice NAL unit.
			/// </summary>
			MaxGapBetweenPpsAndSlice,
			/// <summary>
			/// The maximum number of bytes between a Picture Parameter Set (PPS)
			/// and a consecutive Sequence Parameter Set (SPS).
			/// </summary>
			MaxGapBetweenPpsAndSps,
			/// <summary>
			/// The maximum number of bytes between other consecutive NAL units.
			/// </summary>
			MaxGapBetweenNalUnits,
			/// <summary>The minimum size of a slice in bytes.</summary>
			MinSliceNalUnitLength,
			/// <summary>The maximum size of a slice in bytes.</summary>
			MaxSliceNalUnitLength,
			/// <summary>Enable/disable false hit reduction algorithm.</summary>
			FalseHitReduction,
			//HasTrailingZeroBytes,
			MaxTrailingZeroByteLength,
			//LimitNalUnitTypesToOneFiveSixSevenEight,
			//InvalidContentRejectionLimitPercentage
			MaxReferenceHeaderRetriesPerFragment
		}

		static H264Detector()
		{
			Configurable = new Configurable<ConfigurationKey>();
			Configurable.Add(ConfigurationKey.MaxGapBetweenPpsAndSlice, 0x100000U);
			Configurable.Add(ConfigurationKey.MaxGapBetweenPpsAndSps, 16U);
			Configurable.Add(ConfigurationKey.MaxGapBetweenNalUnits, 0x6000U);
			Configurable.Add(ConfigurationKey.MinSliceNalUnitLength, 8U);
			Configurable.Add(ConfigurationKey.MaxSliceNalUnitLength, 0x100000U);
			Configurable.Add(ConfigurationKey.FalseHitReduction, true);
			//Configurable.Add(ConfigurationKey.HasTrailingZeroBytes, false);
			Configurable.Add(ConfigurationKey.MaxTrailingZeroByteLength, 20U);
			//Configurable.Add(ConfigurationKey.LimitNalUnitTypesToOneFiveSixSevenEight, true);
			//Configurable.Add(ConfigurationKey.InvalidContentRejectionLimitPercentage, 20);
			Configurable.Add(ConfigurationKey.MaxReferenceHeaderRetriesPerFragment, 8U);
		}

		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Configuration

		private static readonly string FirstMacroblockInSliceName = Enum.GetName(typeof(SliceHeader.Attribute), SliceHeader.Attribute.FirstMacroblockInSlice);
		private static readonly string SliceTypeName = Enum.GetName(typeof (SliceHeader.Attribute), SliceHeader.Attribute.SliceType);

		private readonly IDetectorColumns _detectorColumns;
		private readonly ICarver _carver;

		#region Properties
		public string Name { get { return DetectorName; } }
		public string Description { get { return "H.264 Detector"; } }
		public string OutputFileExtension { get { return ".h264"; } }
		public IDictionary<string, string[]> Columns { get { return _detectorColumns.Columns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return Enumerable.Repeat(CodecID.H264, 1); } }
		public Type DetectorType { get { return typeof(H264Detector); } }
		public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		public IEnumerable<IReferenceHeader> ReferenceHeaders { get; set; }
		#endregion Properties

		public H264Detector()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new DetectorCommonModule());
			builder.RegisterModule(new H264Module());
			IContainer container = builder.Build();

			_detectorColumns = container.Resolve<IDetectorColumns>();
			_carver = container.Resolve<ICarver>();
		}

		/// <summary>
		/// This method looks for the beginning of an NAL unit and if one is found it will parse it
		/// by calling Parser.Parse().
		/// </summary>
		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			if (ReferenceHeaders != null)
			{
				BuildReferenceHeaders(context, dataBlockBuilder);
			}
			return _carver.Carve(new BitStreamDataReader(dataReader), dataBlockBuilder, context);
		}

		private void BuildReferenceHeaders(IScanContext context, IDataBlockBuilder dataBlockBuilder)
		{
			var referenceHeaders = new Dictionary<IDataPacket, IPictureState>();
			foreach (IReferenceHeader header in ReferenceHeaders)
			{
				IInputFile headerFile = context.CreateReferenceHeaderFile(header);
				using (var reader = headerFile.CreateDataReader())
				{
					var headerDataBlock = _carver.Carve(new BitStreamDataReader(reader), dataBlockBuilder, context);
					if (headerDataBlock != null)
					{
						referenceHeaders[headerFile.CreateDataPacket()] = context.ReferenceHeader as IPictureState;
					}
				}
			}

			// Set reference header and reset other state
			context.ReferenceHeader = referenceHeaders;
			context.Results = null;
			context.IsFragmented = false;
		}

		public bool IsKeyFrame(IResultNode resultNode)
		{
			return IsIntraSlice(resultNode) && (GetFirstMacroblockInSlice(resultNode) == 0);
		}

		private static bool IsIntraSlice(IResultNode resultNode)
		{
			if (!IsSlice(resultNode) || (resultNode.Name != IdrPictureSlice.Name))
			{
				return false;
			}

			var sliceTypeAttribute = resultNode.FindAttributeByName(SliceTypeName);
			if (sliceTypeAttribute == null)
			{
				return false;
			}

			if ((int)sliceTypeAttribute.Value == (int)SliceType.I)
			{
				return true;
			}
			if ((int)sliceTypeAttribute.Value == (int)SliceType.OnlyI)
			{
				return true;
			}

			return false;
		}

		private static uint? GetFirstMacroblockInSlice(IResultNode resultNode)
		{
			var firstMacroblockInSlice = resultNode.FindAttributeByName(FirstMacroblockInSliceName);
			if (firstMacroblockInSlice == null)
			{
				return null;
			}

			return (uint) firstMacroblockInSlice.Value;
		}

		public IDataPacket GetVideoHeaders(IResultNode resultNode)
		{
			if (!IsSlice(resultNode))
			{
				return null;
			}

			IDataPacket data = null;
			for (IResultNode node = resultNode.Parent; node != null; node = node.Parent)
			{
				if (node.Length > 0)
				{
					// Add (prepend) parent node
					data = data.Prepend(node);
				}
			}
			return data;
		}

		public IDataPacket GetVideoData(IResultNode resultNode)
		{
			if (!IsIntraSlice(resultNode))
			{
				return null;
			}
			if (resultNode.Parent == null)
			{
				// Slice without headers (won't decode!)
				return resultNode.Append(resultNode);
			}

			uint? firstMacroblockInSlice = GetFirstMacroblockInSlice(resultNode);
			if (!firstMacroblockInSlice.HasValue)
			{
				return resultNode.Append(resultNode);
			}

			int index = GetParentIndex(resultNode);
			IList<IResultNode> siblings = resultNode.Parent.Children;
			IDataPacket dataPacket = resultNode;
			for (int i = (index + 1); i < siblings.Count; i++)
			{
				IResultNode sibling = siblings[i];
				if (!IsIntraSlice(sibling))
				{
					break; // Not an intra-coded slice, so not part of the same IDR picture
				}

				uint? nextFirstMacroblockInSlice = GetFirstMacroblockInSlice(sibling);
				if (!nextFirstMacroblockInSlice.HasValue || (nextFirstMacroblockInSlice <= firstMacroblockInSlice))
				{
					break; // Slice is not part of the same picture
				}

				firstMacroblockInSlice = nextFirstMacroblockInSlice.Value;
				dataPacket = dataPacket.Append(sibling);
			}

			// Duplicate the frame data, because otherwise, FFmpeg won't decode it.
			return dataPacket.Append(dataPacket);
		}

		private static int GetParentIndex(IResultNode resultNode)
		{
			IList<IResultNode> siblings = resultNode.Parent.Children;
			for (int i = 0; i < siblings.Count; i++)
			{
				if (siblings[i] == resultNode)
				{
					return i;
				}
			}

			throw new ArgumentException("resultNode is not a child of its parent!");
		}

		private static bool IsSlice(IResult resultNode)
		{
			return (resultNode.Name == IdrPictureSlice.Name) || (resultNode.Name == NonIdrPictureSlice.Name);
		}

		public IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec codecParameters)
		{
			// Note: Loop to detect reference headers preceeded by a block of video frames
			//       (e.g. for 3GP files with 'mdat' before the 'moov' atom, so that the slices are located before the SPS/PPS pair)
			while (dataReader.State == DataReaderState.Ready)
			{
				IScanContext scanContext = new ReferenceHeaderScanContext {Detectors = new[] {this}};

				var dataBlock = _carver.Carve(new BitStreamDataReader(dataReader), new ReferenceHeaderDataBlockBuilder(), scanContext);
				if (dataBlock == null)
				{
					break; // No (more) data blocks detected
				}

				IResultNode sequenceParameterSet = scanContext.Results.FindChild(SequenceParameterSet.Name);
				if (sequenceParameterSet != null)
				{
					IResultNode pictureParameterSet = sequenceParameterSet.FindChild(PictureParameterSet.Name);
					if (pictureParameterSet != null)
					{
						// TODO: perhaps we need to include the headers between the SPS and PPS !?
						IDataPacket data = sequenceParameterSet.Append(pictureParameterSet);

						// Report codec parameters for reference header
						codecParameters.Codec = CodecID.H264;
						codecParameters.Width = GetWidth(sequenceParameterSet);
						codecParameters.Height = GetHeight(sequenceParameterSet);
						var entropyCodingMode = GetAttributeValue<bool>(pictureParameterSet, PictureParameterSet.Attribute.EntropyCodingMode);
						codecParameters["EntropyCodingMode"] = entropyCodingMode ? "CABAC" : "CAVLC";
						// TODO: codecParameters["FrameRate"] = GetAttributeStringValue(sequenceParameterSet, ???);

						return data;
					}
				}

				// Sequence- or Picture Parameter Set is missing: Continue scanning after current data block ...
				dataReader.Position = dataBlock.EndOffset;
			}

			return null; // No reference header detected
		}

		private static uint GetWidth(IResult sequenceParameterSet)
		{
			var picWidthInMbsMinus1 = GetAttributeValue<uint>(sequenceParameterSet, SequenceParameterSet.Attribute.PictureWidthInMacroBlocksMinus1);
			var frameCropLeftOffset = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.FrameCropLeftOffset);
			var frameCropRightOffset = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.FrameCropRightOffset);

			var width = (picWidthInMbsMinus1 + 1) << 4;
			if ((frameCropLeftOffset == null) || (frameCropRightOffset == null))
			{
				return width; // No cropping
			}

			// Decode cropping information, as described in "T-REC-H.264-201003 Advanced video coding for generic audiovisual", page 77
			var chromaFormatIdc = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.ChromaFormatIdc);

			uint cropUnitX = 1U;
			if ((chromaFormatIdc == null) || ((uint) chromaFormatIdc.Value == 1) || ((uint) chromaFormatIdc.Value == 2))
			{
				cropUnitX *= 2;
			}

			// Adjust width for frame cropping
			return width - (cropUnitX * ((uint)frameCropRightOffset.Value + (uint)frameCropLeftOffset.Value));
		}

		private static uint GetHeight(IResult sequenceParameterSet)
		{
			var picHeightInMapUnits = 1 + GetAttributeValue<uint>(sequenceParameterSet, SequenceParameterSet.Attribute.PictureHeightInMapUnitsMinus1);
			var frameCropTopOffset = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.FrameCropTopOffset);
			var frameCropBottomOffset = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.FrameCropBottomOffset);
			var frameMbsOnlyFlag = GetAttributeValue<bool>(sequenceParameterSet, SequenceParameterSet.Attribute.FrameMbsOnlyFlag);

			var height = ((2 - (frameMbsOnlyFlag ? 1U : 0U)) * picHeightInMapUnits) << 4;
			if ((frameCropTopOffset == null) || (frameCropBottomOffset == null))
			{
				return height; // No cropping
			}

			// Decode cropping information, as described in "T-REC-H.264-201003 Advanced video coding for generic audiovisual", page 77
			var chromaFormatIdc = GetAttribute(sequenceParameterSet, SequenceParameterSet.Attribute.ChromaFormatIdc);

			uint cropUnitY = frameMbsOnlyFlag ? 1U : 2U;
			if ((chromaFormatIdc == null) || ((uint)chromaFormatIdc.Value == 1))
			{
				cropUnitY *= 2;
			}

			// Adjust height for frame cropping
			return height - (cropUnitY * ((uint)frameCropTopOffset.Value + (uint)frameCropBottomOffset.Value));
		}

		private static T GetAttributeValue<T>(IResult result, object attributeName)
		{
			return (T)GetAttribute(result, attributeName).Value;
		}

		private static IResultAttribute GetAttribute(IResult result, object attributeName)
		{
			return result.FindAttributeByName(Enum.GetName(attributeName.GetType(), attributeName));
		}
	}
}
