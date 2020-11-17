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
using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Mpeg4
{
	internal class VideoObjectLayer : Mpeg4Header
	{
		public new enum Attribute
		{
			ID,
			ShortVideoHeader,
			Shape,
			ShapeExtension,
			TypeIndication,
			Scalability,
			DataPartioned,
			ObmcDisable,
			ResyncMarkerDisable,
			ReversibleVlc,
			NewPredEnable,
			SADctDisable,
			VopTimeIncrementResolution,
			FixedVopRate,
			FixedVopTimeIncrement,
			Width,
			Height,
			SpriteEnable,
			SpriteWidth,
			SpriteHeight,
			EnhancementType,

			RandomAccessibleVol,
			VerID,
			Priority,
			AspectRatioInfo,
			AspectRatioInfoParWidth,
			AspectRatioInfoParHeight,
			VolControlParameters,

			ChromaFormat,
			LowDelay,
			VbvParameters,
			BitRate,
			VbvBufferSize,
			Interlaced,
			SpriteBrightnessChange,
			SpriteWarpingAccuracy,
			SpriteWarpingPoints,
			LowLatencySpriteEnable,
			Not8bit,
			QuantPrecision,
			BitsPerPixel,
			NoGrayQuantUpdate,
			CompositionMethod,
			LinearComposition,
			QuantType,
			LoadIntraQuantMat,
			LoadIntraQuantMatGrayscale,
			LoadNonintraQuantMatGrayscale,
			QuarterSample,

			// Complexity Estimation 
			EstimationMethod,

			Opaque,
			Transparent,
			IntraCae,
			InterCae,
			NoUpdate,
			Upsampling,

			TextureComplexityEstimationSet1Disable,

			IntraBlocks,
			InterBlocks,
			Inter4vBlocks,
			NotCodedBlocks,

			TextureComplexityEstimationSet2Disable,

			DctCoefs,
			DctLines,
			VlcSymbols,
			VlcBits,

			MotionCompensationComplexityDisable,

			Apm,
			Npm,
			InterpolateMcQ,
			ForwBackMcQ,
			Halfpel2,
			Halfpel4,

			Version2ComplexityEstimationDisable,

			Sadct,
			Quarterpel,
			DataPartitioned,
			RequestedUpstreamMessageType,
			NewpredSegmentType,
			ReducedResolutionVopEnable,
		}

		public const int MP4SpriteStatic = 1;
		public const int MP4SpriteGMC = 2;

		public bool IsShortVideoHeader { get; private set; }
		public Shape VideoShape { get; private set; }
		public uint VopTimeIncrementResolution { get; private set; }
		public short VopTimeIncrementResolutionBits { get; private set; }
		public bool ReducedResolutionVopEnable { get; private set; }
		public uint SpriteEnable { get; private set; }
		public bool ComplexityEstimationDisable { get; private set; }
		public bool Interlaced { get; private set; }
		public bool Scalability { get; private set; }
		public uint SpriteWarpingPoints { get; private set; }
		public bool SpriteBrightnessChange { get; private set; }
		public short QuantPrecision { get; private set; }
		public uint ShapeExtension { get; private set; }
		public bool NewPredEnable { get; private set; }
		public bool EnhancementType { get; private set; }
		public bool DataPartitioned { get; private set; }
		public bool LowLatencySpriteEnabled { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }
		public int AuxCompCount { get { return mp4_aux_comp_count[ShapeExtension]; } }
		public bool ResyncMarkerDisable { get; private set; }
		public bool UseReverseDCT { get { return _reversible_vlc; } }
		/// <summary>Whether this header occured inside a VOP</summary>
		internal bool InVop { get; private set; }

		private readonly byte[][] _intra_quant_mat_grayscale = new byte[3][] { new byte[64], new byte[64], new byte[64] };
		private readonly byte [][]  _nonintra_quant_mat_grayscale = new byte[3][] { new byte[64], new byte[64], new byte[64] };
		private mp4_ComplexityEstimation _complexityEstimation;
		private bool _reversible_vlc;

		public static readonly int [] mp4_aux_comp_count = {1, 1, 2, 2, 3, 1, 2, 1, 1, 2, 3, 2, 3, 0};

		private static readonly byte [] mp4_ClassicalZigzag  = { 0,   1,  8, 16,  9,  2,  3, 10, 17, 24, 32, 25, 18, 11,  4,  5,
														12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13,  6,  7, 14, 21, 28,
														35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
														58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63 };
		private static readonly byte[] mp4_DefaultNonIntraQuantMatrix = { 16, 17, 18, 19, 20, 21, 22, 23, 17, 18, 19, 20, 21, 22, 23, 24,
																	18, 19, 20, 21, 22, 23, 24, 25, 19, 20, 21, 22, 23, 24, 26, 27,
																	20, 21, 22, 23, 25, 26, 27, 28, 21, 22, 23, 24, 26, 27, 28, 30,
																	22, 23, 24, 26, 27, 28, 30, 31, 23, 24, 25, 27, 28, 30, 31, 33};
		private static readonly byte[] mp4_DefaultIntraQuantMatrix = { 8, 17, 18, 19, 21, 23, 25, 27, 17, 18, 19, 21, 23, 25, 27, 28,
																20, 21, 22, 23, 24, 26, 28, 30, 21, 22, 23, 24, 26, 28, 30, 32,
																22, 23, 24, 26, 28, 30, 32, 35, 23, 24, 26, 28, 30, 32, 35, 38,
																25, 26, 28, 30, 32, 35, 38, 41, 27, 28, 30, 32, 35, 38, 41, 45};

		public enum Shape : byte
		{
			Rectangular = 0,
			Binary = 1,
			BinaryOnly = 2,
			Grayscale = 3
		}

		private enum PixelAspectRatio : byte
		{
			Forbidden = 0,
			Square1_1 = 1,
			Type625For4_3Picture_12_11 = 2,
			Type525For4_3Picture_10_11 = 3,
			Type625StretchedFor16_9Picture_16_11 = 4,
			Type525StretchedFot16_9Picture_40_33 = 5,
			// 6..14 reserved
			ExtentedPar = 15
		}

		public struct mp4_ComplexityEstimation
		{
			public uint estimation_method;
			public bool shape_complexity_estimation_disable;
			public bool opaque;
			public bool transparent;
			public bool intra_cae;
			public bool inter_cae;
			public bool no_update;
			public bool upsampling;
			public bool texture_complexity_estimation_set_1_disable;
			public bool intra_blocks;
			public bool inter_blocks;
			public bool inter4v_blocks;
			public bool not_coded_blocks;
			public bool texture_complexity_estimation_set_2_disable;
			public bool dct_coefs;
			public bool dct_lines;
			public bool vlc_symbols;
			public bool vlc_bits;
			public bool motion_compensation_complexity_disable;
			public bool apm;
			public bool npm;
			public bool interpolate_mc_q;
			public bool forw_back_mc_q;
			public bool halfpel2;
			public bool halfpel4;
			public bool version2_complexity_estimation_disable;     // verid != 1
			public bool sadct;                                      // verid != 1
			public bool quarterpel;                                 // verid != 1
		}

		public VideoObjectLayer(Mpeg4Header previousHeader, bool inVop)
			: base(previousHeader, Mpeg4HeaderName.VideoObjectLayer)
		{
			this.InVop = inVop;
		}

		public VideoObjectLayer(Mpeg4Header previousHeader)
			: this(previousHeader, false)
		{
		}

		//private static bool HeaderIsShortVideo(ByteStreamDataReader dataReader, long offset)
		//{
		//    dataReader.Position = offset;
		//    byte[] bytes = new byte[3];
		//    dataReader.Read(bytes, 0, 3);
		//    return ((bytes[0] == 0x00) && (bytes[1] == 0x00) && ((bytes[2] & 0xFC) == 0x80));
		//}

		public override bool Parse(Mpeg4Parser parser)
		{
			if(InVop)
			{
				IsShortVideoHeader = true;
				QuantPrecision = 5;
				VideoShape = Shape.Rectangular;
				ResyncMarkerDisable = true;
				DataPartitioned = false;
				_reversible_vlc = false;
				Interlaced = false;
				ComplexityEstimationDisable = true;
				Scalability = false;
				VopTimeIncrementResolution = 30000;

				return true;
			}

			if (!base.Parse(parser)) return false;

			if (StartCode < 0x120 || StartCode > 0x12F)
			{
				//mp4_Error("Error: Bad start code for VideoObjectLayerLayer");
				return false;
			}

			// Found video_object_start_code
			parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.ID, StartCode & 15));

			IsShortVideoHeader = false;
			parser.GetBit(Attribute.RandomAccessibleVol);

			/*byte typeIndication = (byte)*/parser.GetBits(8, Attribute.TypeIndication);
			//if (typeIndication == FineGranularityScalable)
			//{
			//	TODO add branch for type indication value
			//}
			//else
			//{

			bool isIdentifier = parser.GetBit();

			uint verid = 0;
			if (isIdentifier == true)
			{
				verid = parser.GetBits(4);

				if ((verid != 1) && (verid != 2) && (verid != 4) && (verid != 5))
				{
					var verIDAttribute = new FormattedAttribute<Attribute, string>(Attribute.VerID, string.Format(CultureInfo.CurrentCulture ,"Invalid value found: {0}. It should be 1, 2, 4 or 5. Assuming '1'.", verid));
					verIDAttribute.Valid = false;

					//mp4_Error("Warning: invalid version number in VOL");
					verid = 1;
				}
				else
				{
					parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.VerID, verid));
				}
				parser.GetBits(3, Attribute.Priority);
			}
			else
			{
				verid = 1;
			}

			byte aspectRatioInfo = (byte)parser.GetBits(4);
			string aspectRatioName = Enum.IsDefined(typeof(PixelAspectRatio), aspectRatioInfo) ? Enum.GetName(typeof(PixelAspectRatio), aspectRatioInfo) : "Reserved";
			parser.AddAttribute(new FormattedAttribute<Attribute, string>(Attribute.AspectRatioInfo, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", aspectRatioInfo.ToString(CultureInfo.CurrentCulture), aspectRatioName)));

			if (aspectRatioInfo == (uint)PixelAspectRatio.ExtentedPar)
			{
				parser.GetBits(8, Attribute.AspectRatioInfoParWidth);
				parser.GetBits(8, Attribute.AspectRatioInfoParHeight);
			}
			bool isVolControlParameters = parser.GetBit(Attribute.VolControlParameters);
			if (isVolControlParameters  == true)
			{
				const int ChromaFormat420 = 1;

				uint chromaFormat = parser.GetBits(2, Attribute.ChromaFormat);

				if (chromaFormat != ChromaFormat420)
				{
					//mp4_Error("Error: vol_control_parameters.chroma_format != 4:2:0");
					return false;
				}

				parser.GetBit(Attribute.LowDelay);

				bool vbvParameters = parser.GetBit(Attribute.VbvParameters);
				if (vbvParameters)
				{
					uint bitRate = parser.GetBits(15);
					bitRate <<= 15;

					if(!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					//_VOLControlParameters.bit_rate += Utils.GetBits(15);
					bitRate += parser.GetBits(15);
					parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.BitRate, bitRate));

					if(!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					if (bitRate == 0) {
						//mp4_Error("Error: vbv_parameters bit_rate == 0");
						return false;
					}
					uint vbvBufferSize = parser.GetBits(15);
					vbvBufferSize <<= 3;
					
					if(!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					vbvBufferSize += parser.GetBits(3);
					if (vbvBufferSize == 0) {
						//mp4_Error("Error: vbv_parameters vbv_buffer_size == 0");
						return false;
					}
					parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.VbvBufferSize, vbvBufferSize));

					uint vbvOccupancy = parser.GetBits(11);
					vbvOccupancy <<= 15;

					if (!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					vbvOccupancy += parser.GetBits(15);

					if (!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
				}
			}
			const int ShapeExtNum = 13;

			this.VideoShape = (Shape)parser.GetBits(2);
			parser.AddAttribute(new FormattedAttribute<Attribute, string>(Attribute.Shape, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", (int)VideoShape, Enum.GetName(typeof(Shape), (byte)this.VideoShape))));

			if(verid != 1 && VideoShape == Shape.Grayscale)
			{
				ShapeExtension = parser.GetBits(4, Attribute.ShapeExtension);
				if (ShapeExtension >= ShapeExtNum)
				{
					//mp4_Error("Error: wrong value for video_object_layer_shape_extension");
					return false;
				}
			}
			else
			{
				ShapeExtension = ShapeExtNum;
				parser.AddAttribute(new FormattedAttribute<Attribute, uint>(Attribute.ShapeExtension, ShapeExtension));
			}
			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOL header
				return false;
			}
			VopTimeIncrementResolution = parser.GetBits(16, Attribute.VopTimeIncrementResolution);
			if (VopTimeIncrementResolution == 0)
			{
				//mp4_Error("Error: wrong value for vop_time_increment_resolution");
				return false;
			}
			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOL header
				return false;
			}
			// define number bits in vop_time_increment_resolution
			uint numBits = VopTimeIncrementResolution - 1;
			uint i = 0;
			do
			{
				numBits >>= 1;
				i++;
			} while (numBits > 0);
			VopTimeIncrementResolutionBits = (short)i;
			bool fixedVopRate = parser.GetBit(Attribute.FixedVopRate);
			if (fixedVopRate == true)
			{
				parser.GetBits((int)VopTimeIncrementResolutionBits, Attribute.FixedVopTimeIncrement);
			}
			if (VideoShape != Shape.BinaryOnly)
			{
				if (VideoShape == Shape.Rectangular)
				{
					if (!parser.GetMarkerBit())
					{		
						// Does not appear to be a mp4 VOL header
						return false;
					}
					Width = parser.GetBits(13, Attribute.Width);
					if (!parser.GetMarkerBit() || Width == 0)
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					Height = parser.GetBits(13, Attribute.Height);
					if (!parser.GetMarkerBit() || Height == 0)
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
				}
				Interlaced = parser.GetBit(Attribute.Interlaced);
				parser.GetBit(Attribute.ObmcDisable);
				SpriteEnable = parser.GetBits((short)( verid != 1 ? 2 : 1), Attribute.SpriteEnable);
				const int SpriteStatic = 1;
				const int SpriteGmc = 2;
				if (SpriteEnable == SpriteStatic || SpriteEnable == SpriteGmc)
				{
					if (SpriteEnable == SpriteStatic)
					{
						parser.GetBits(13, Attribute.SpriteWidth);
						if (!parser.GetMarkerBit())
						{
							// Does not appear to be a mp4 VOL header
							return false;
						}
						parser.GetBits(13, Attribute.SpriteHeight);
						if (!parser.GetMarkerBit())
						{
							// Does not appear to be a mp4 VOL header
							return false;
						}
						uint sprite_left_coordinate = parser.GetBits(13);
						sprite_left_coordinate <<= (32 - 13);
						sprite_left_coordinate >>= (32 - 13);
						if ((sprite_left_coordinate & 1) > 0) {
							//mp4_Error("Error: sprite_left_coordinate must be divisible by 2");
							return false;
						}
						if (!parser.GetMarkerBit())
						{
							// Does not appear to be a mp4 VOL header
							return false;
						}
						uint sprite_top_coordinate = parser.GetBits(13);
						sprite_top_coordinate <<= (32 - 13);
						sprite_top_coordinate >>= (32 - 13);
						if ((sprite_top_coordinate & 1) > 0)
						{
							//mp4_Error("Error: sprite_top_coordinate must be divisible by 2");
							return false;
						}
						if (!parser.GetMarkerBit())
						{
							// Does not appear to be a mp4 VOL header
							return false;
						}
					}
					SpriteWarpingPoints = parser.GetBits(6, Attribute.SpriteWarpingPoints);
					if (SpriteWarpingPoints > 4 ||
						(SpriteWarpingPoints == 4 &&
						 SpriteEnable == SpriteGmc))
					{
						//mp4_Error("Error: bad no_of_sprite_warping_points");
						return false;
					}
					parser.GetBits(2, Attribute.SpriteWarpingAccuracy);
					SpriteBrightnessChange = parser.GetBit(Attribute.SpriteBrightnessChange);
					if (SpriteEnable == SpriteGmc)
					{
						if (SpriteBrightnessChange == true)
						{
							//mp4_Error("Error: sprite_brightness_change should be 0 for GMC sprites");
							return false;
						}
					}
					if (SpriteEnable != SpriteGmc)
					{
						LowLatencySpriteEnabled = parser.GetBit(Attribute.LowLatencySpriteEnable);
					}
				}
				if (verid != 1 && VideoShape != Shape.Rectangular)
				{
					parser.GetBit(Attribute.SADctDisable);
				}
				bool not8Bit = parser.GetBit(Attribute.Not8bit);
				if (not8Bit == true)
				{
					QuantPrecision = (short)parser.GetBits(4, Attribute.QuantPrecision);
					if (QuantPrecision < 3 || QuantPrecision > 9)
					{
						//mp4_Error("Error: quant_precision must be in range [3; 9]");
						return false;
					}
					uint bits_per_pixel = parser.GetBits(4, Attribute.BitsPerPixel);
					if (bits_per_pixel < 4 || bits_per_pixel > 12)
					{
						//mp4_Error("Error: bits_per_pixel must be in range [4; 12]");
						return false;
					}
				}
				else
				{
					QuantPrecision = 5;
					parser.AddAttribute(new FormattedAttribute<Attribute, short>(Attribute.QuantPrecision, QuantPrecision));
					parser.AddAttribute(new FormattedAttribute<Attribute, short>(Attribute.BitsPerPixel, 8));
				}
				if (VideoShape == Shape.Grayscale)
				{
					parser.GetBit(Attribute.NoGrayQuantUpdate);
					parser.GetBit(Attribute.CompositionMethod);
					parser.GetBit(Attribute.LinearComposition);
				}
				bool quantType = parser.GetBit(Attribute.QuantType);
				if (quantType == true)
				{
					bool loadIntraQuantMat = parser.GetBit(Attribute.LoadIntraQuantMat);
					byte[] intraQuantMat = new byte[64];
					if (loadIntraQuantMat == true) 
					{
						if (ParseQuantMatrix(parser, ref intraQuantMat) != 0)
						{
							return false;
						}
					}
					else
					{
						mp4_DefaultIntraQuantMatrix.CopyTo(intraQuantMat, 0);
					}

					bool loadNonintraQuantMat = parser.GetBit();
					byte[] nonintraQuantMat = new byte[64];
					if (loadNonintraQuantMat == true)
					{
						if (ParseQuantMatrix(parser, ref nonintraQuantMat) != 0) 
						{
							return false;
						}
					}
					else
					{
						mp4_DefaultNonIntraQuantMatrix.CopyTo(nonintraQuantMat, 0);
					}
					if (VideoShape == Shape.Grayscale)
					{
						int ac;
						int index = 0;

						ac = mp4_aux_comp_count[ShapeExtension];
						bool[] loadIntraQuantMatGrayscale = new bool[3];
						bool[] loadNonintraQuantMatGrayscale = new bool[3];
						for (index = 0; index < ac; index++)
						{
							loadIntraQuantMatGrayscale[index] = parser.GetBit(Attribute.LoadIntraQuantMatGrayscale);
							if (loadIntraQuantMatGrayscale[index] == true)
							{
								if (ParseQuantMatrix(parser, ref _intra_quant_mat_grayscale[index]) != 1)
									return false;
							}
							else
							{
								mp4_DefaultIntraQuantMatrix.CopyTo(_intra_quant_mat_grayscale[index], 0);
							}
							loadNonintraQuantMatGrayscale[index] = parser.GetBit(Attribute.LoadNonintraQuantMatGrayscale);
							if (loadNonintraQuantMatGrayscale[index] == true)
							{
								if (ParseQuantMatrix(parser, ref _nonintra_quant_mat_grayscale[index]) != 1)
								{
									return false;
								}
							}
							else
							{
								mp4_DefaultNonIntraQuantMatrix.CopyTo(_nonintra_quant_mat_grayscale[index], 0);
							}
						}
					}
				}
				if (verid != 1)
				{
					parser.GetBit(Attribute.QuarterSample);
				}
				ComplexityEstimationDisable = parser.GetBit();
				if (ComplexityEstimationDisable == false)
				{
					_complexityEstimation.estimation_method = parser.GetBits(2, Attribute.EstimationMethod);
					if (_complexityEstimation.estimation_method <= 1)
					{
						_complexityEstimation.shape_complexity_estimation_disable = parser.GetBit();
						if (_complexityEstimation.shape_complexity_estimation_disable == false)
						{
							_complexityEstimation.opaque = parser.GetBit(Attribute.Opaque);
							_complexityEstimation.transparent = parser.GetBit(Attribute.Transparent);
							_complexityEstimation.intra_cae = parser.GetBit(Attribute.IntraCae);
							_complexityEstimation.inter_cae = parser.GetBit(Attribute.InterCae);
							_complexityEstimation.no_update = parser.GetBit(Attribute.NoUpdate);
							_complexityEstimation.upsampling = parser.GetBit(Attribute.Upsampling);
						}
						_complexityEstimation.texture_complexity_estimation_set_1_disable = parser.GetBit(Attribute.TextureComplexityEstimationSet1Disable);
						if (_complexityEstimation.texture_complexity_estimation_set_1_disable == false)
						{
							_complexityEstimation.intra_blocks = parser.GetBit(Attribute.IntraBlocks);
							_complexityEstimation.inter_blocks = parser.GetBit(Attribute.InterBlocks);
							_complexityEstimation.inter4v_blocks = parser.GetBit(Attribute.Inter4vBlocks);
							_complexityEstimation.not_coded_blocks = parser.GetBit(Attribute.NotCodedBlocks);
						}
						if (!parser.GetMarkerBit())
						{
							// Does not appear to be a mp4 VOL header
							return false;
						}
						_complexityEstimation.texture_complexity_estimation_set_2_disable = parser.GetBit(Attribute.TextureComplexityEstimationSet2Disable);
						if (_complexityEstimation.texture_complexity_estimation_set_2_disable == false)
						{
							_complexityEstimation.dct_coefs = parser.GetBit(Attribute.DctCoefs);
							_complexityEstimation.dct_lines = parser.GetBit(Attribute.DctLines);
							_complexityEstimation.vlc_symbols = parser.GetBit(Attribute.VlcSymbols);
							_complexityEstimation.vlc_bits = parser.GetBit(Attribute.VlcBits);
						}
						_complexityEstimation.motion_compensation_complexity_disable = parser.GetBit(Attribute.MotionCompensationComplexityDisable);
						if (_complexityEstimation.motion_compensation_complexity_disable == false)
						{
							_complexityEstimation.apm = parser.GetBit(Attribute.Apm);
							_complexityEstimation.npm = parser.GetBit(Attribute.Npm);
							_complexityEstimation.interpolate_mc_q = parser.GetBit(Attribute.InterpolateMcQ);
							_complexityEstimation.forw_back_mc_q = parser.GetBit(Attribute.ForwBackMcQ);
							_complexityEstimation.halfpel2 = parser.GetBit(Attribute.Halfpel2);
							_complexityEstimation.halfpel4 = parser.GetBit(Attribute.Halfpel4);
						}
					}
					if (!parser.GetMarkerBit())
					{
						// Does not appear to be a mp4 VOL header
						return false;
					}
					if (_complexityEstimation.estimation_method == 1)
					{
						// verid != 1
						_complexityEstimation.version2_complexity_estimation_disable = parser.GetBit(Attribute.Version2ComplexityEstimationDisable);
						if (_complexityEstimation.version2_complexity_estimation_disable == false)
						{
							_complexityEstimation.sadct = parser.GetBit(Attribute.Sadct);
							_complexityEstimation.quarterpel = parser.GetBit(Attribute.Quarterpel);
						}
					}
				}
				ResyncMarkerDisable = parser.GetBit(Attribute.ResyncMarkerDisable);
				DataPartitioned = parser.GetBit(Attribute.DataPartitioned);
				//f GrayScale Shapes does not support data_part
				if (DataPartitioned == true)
				{
					_reversible_vlc = parser.GetBit(Attribute.ReversibleVlc);
				}
				if (verid != 1) {
					NewPredEnable = parser.GetBit(Attribute.NewPredEnable);
					if (NewPredEnable == true)
					{
						parser.GetBits(2, Attribute.RequestedUpstreamMessageType);
						parser.GetBit(Attribute.NewpredSegmentType);
					}
					ReducedResolutionVopEnable = parser.GetBit(Attribute.ReducedResolutionVopEnable);
				}
				bool scalability = parser.GetBit(Attribute.Scalability);
				if (scalability == true)
				{
					parser.FlushBits(26);
					EnhancementType = parser.GetBit(Attribute.EnhancementType);
				}
			}
			else
			{
				if (verid != 1)
				{
					Scalability = parser.GetBit(Attribute.Scalability);
					if (Scalability == true)
					{
						parser.FlushBits(24);
					}
				}
				this.ResyncMarkerDisable = parser.GetBit();
			}

			//_VideoObjectLayerPlane.sprite_transmit_mode = Sprite.MP4_SPRITE_TRANSMIT_MODE_PIECE;
			return true;
		}

		public static int ParseQuantMatrix(Mpeg4Parser parser, ref byte [] quantMatrix)
		{
			byte code = 0;
			int i;

			for (i = 0; i < 64; i++) {
				code = (byte)parser.GetBits(8);
				if (code == 0) 
					break;
				quantMatrix[mp4_ClassicalZigzag[i]] = code;
			}
			if (i >= 1)
			{
				code = quantMatrix[mp4_ClassicalZigzag[i - 1]];
			}
			for (; i < 64; i ++) {
				quantMatrix[mp4_ClassicalZigzag[i]] = code;
			}
			return 1;
		}

		public void ComplexityEstimation(out mp4_ComplexityEstimation complexityEstimation)
		{
			complexityEstimation = this._complexityEstimation; 
		}
	}
}
