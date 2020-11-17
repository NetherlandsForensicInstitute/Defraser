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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal sealed class SequenceParameterSet : INalUnitPayloadParser
	{
		private readonly EnumResultFormatter<Profile> _profileValidator;

		internal enum Attribute
		{
			ProfileIdc,
			LevelIdc,
			Log2MaxFrameNumMinus4,
			SequenceParameterSetId,
			ChromaFormatIdc,
			BitDepthLumaArraySampleMinus8,
			BitDepthChromaArraySampleMinus8,
			/// <summary>
			/// Specifies the method to decode picture order count (as specified in subclause 8.2.1).
			/// </summary>
			PictureOrderCountType,
			MaxNumberReferenceFrames,
			Log2MaxPictureOrderCountLsbMinus4,
			NumRefFramesInPicOrderCountCycle,
			PictureWidthInMacroBlocksMinus1,
			PictureHeightInMapUnitsMinus1,
			FrameCropLeftOffset,
			FrameCropRightOffset,
			FrameCropTopOffset,
			FrameCropBottomOffset,
			FrameMbsOnlyFlag,
			SeparateColorPlaneFlag
		}

		internal const string Name = "SequenceParameterSet";

		#region Properties
		public NalUnitType UnitType { get { return NalUnitType.SequenceParameterSet; } }
		#endregion Properties

		public SequenceParameterSet(EnumResultFormatter<Profile> profileValidator)
		{
			_profileValidator = profileValidator;
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		// 7.3.2.1.1 SequenceState parameter set data syntax
		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			// Constraints from 7.4.2.11

			resultState.Name = Name;

			Profile profileIdc = reader.GetBits(8, Attribute.ProfileIdc, _profileValidator);
			reader.GetBits(6); // constraint_set{0-5}_flag

			if (reader.GetBits(2) != 0) // reserved_zero_2bits
			{
				resultState.Invalidate();
				return;
			}

			reader.GetByte(false, Attribute.LevelIdc, 59); // level_idc, Table A-1 - Level limits
			uint sequenceParameterSetId = reader.GetExpGolombCoded(Attribute.SequenceParameterSetId, 31);

			ISequenceStateBuilder builder = new SequenceStateBuilder(sequenceParameterSetId);
			builder.ByteStreamFormat = reader.State.ByteStreamFormat;
			builder.ProfileIdc = profileIdc;

			if (IsHighProfile(profileIdc))
			{
				uint chromaFormatIdc = reader.GetExpGolombCoded(Attribute.ChromaFormatIdc, 3); // Table 6-1
				builder.ChromaFormatIdc = chromaFormatIdc;

				if (chromaFormatIdc == 3)
				{
					builder.SeparateColourPlaneFlag = reader.GetBit(Attribute.SeparateColorPlaneFlag);
				}

				builder.BitDepthLumaMinus8 = reader.GetExpGolombCoded(Attribute.BitDepthLumaArraySampleMinus8, 6);
				builder.BitDepthChromaMinus8 = reader.GetExpGolombCoded(Attribute.BitDepthChromaArraySampleMinus8, 6);
				reader.GetBit(); // qpprime_y_zero_transform_bypass_flag

				if (reader.GetBit()) // seq_scaling_matrix_present_flag
				{
					for (int i = 0; i < ((chromaFormatIdc != 3) ? 8 : 12); i++)
					{
						if (reader.GetBit()) // seq_scaling_list_present_flag[i]
						{
							// FIXME: scaling_list() is not implemented!!

							resultState.Invalidate();
							return;
						}
					}
				}
			}

			builder.Log2MaxFrameNumMinus4 = reader.GetExpGolombCoded(Attribute.Log2MaxFrameNumMinus4, 28);

			DecodePictureOrderCount(reader, builder);

			uint maxDpbFramesUpperLimit = (profileIdc == Profile.MultiviewHigh) ? 160U : 16U; //could use the exact maxDpbFrames, but this is easier
			builder.MaxNumRefFrames = reader.GetExpGolombCoded(Attribute.MaxNumberReferenceFrames, maxDpbFramesUpperLimit);
			reader.GetBit(); // gaps_in_frame_num_value_allowed_flag
			builder.PicWidthInMbsMinus1 = reader.GetExpGolombCoded(Attribute.PictureWidthInMacroBlocksMinus1, 543);
			builder.PicHeightInMapUnitsMinus1 = reader.GetExpGolombCoded(Attribute.PictureHeightInMapUnitsMinus1, 543);

			var frameMbsOnlyFlag = reader.GetBit(Attribute.FrameMbsOnlyFlag);
			builder.FrameMbsOnlyFlag = frameMbsOnlyFlag;
			if (!frameMbsOnlyFlag)
			{
				builder.MbAdaptiveFrameFieldFlag = reader.GetBit();
			}

			bool direct8X8InferenceFlag = reader.GetBit();
			builder.Direct8X8InferenceFlag = direct8X8InferenceFlag;
			if (!frameMbsOnlyFlag && !direct8X8InferenceFlag)
			{
				resultState.Invalidate();
				return;
			}

			if (reader.GetBit()) // frame_cropping_flag
			{
				// TODO: Cropping should not exceed the size of a frame!
				reader.GetExpGolombCoded(Attribute.FrameCropLeftOffset, 8703);
				reader.GetExpGolombCoded(Attribute.FrameCropRightOffset, 8703);
				reader.GetExpGolombCoded(Attribute.FrameCropTopOffset, 8703);
				reader.GetExpGolombCoded(Attribute.FrameCropBottomOffset, 8703);
			}
			if (reader.GetBit()) // vui_parameters_present_flag
			{
				GetVuiParameters(reader, resultState);
			}
			if (reader.ShowBits(1) == 1)
			{
				reader.GetBit(); // rbsp_stop_one_bit (equal to 1)
				// trailing zero bits
			}

			if (resultState.Valid)
			{
				reader.State.SequenceStates[sequenceParameterSetId] = builder.Build();
			}
		}

		private static bool IsHighProfile(Profile profile)
		{
			return profile == Profile.High || profile == Profile.High10 ||
				   profile == Profile.High422 || profile == Profile.High444Old ||
				   profile == Profile.MultiviewHigh || profile == Profile.High444 ||
				   profile == Profile.CaVlc;
		}

		private static void DecodePictureOrderCount(INalUnitReader reader, ISequenceStateBuilder builder)
		{
			uint pictureOrderCountType = reader.GetExpGolombCoded(Attribute.PictureOrderCountType, 2); // pic_order_cnt_type
			builder.PictureOrderCountType = pictureOrderCountType;
			switch (pictureOrderCountType)
			{
				case 0:
					builder.Log2MaxPicOrderCntLsbMinus4 = reader.GetExpGolombCoded(Attribute.Log2MaxPictureOrderCountLsbMinus4, 12); // log2_max_pic_order_cnt_lsb_minus4us4
					break;
				case 1:
					builder.DeltaPicOrderAlwaysZeroFlag = reader.GetBit(); // delta_pic_order_always_zero_flag
					reader.GetSignedExpGolombCoded(); // offset_for_non_ref_pic
					reader.GetSignedExpGolombCoded(); // offset_for_top_to_bottom_field

					// num_ref_frames_in_pic_order_cnt_cycle
					uint numRefFramesInPicOrderCntCycle = reader.GetExpGolombCoded(Attribute.NumRefFramesInPicOrderCountCycle, 255);
					for (uint i = 0; i < numRefFramesInPicOrderCntCycle; i++)
					{
						reader.GetSignedExpGolombCoded(); // offset_for_ref_frame[i]
					}
					break;
				case 2:
					break;
			}
		}

		private static void GetVuiParameters(INalUnitReader reader, IResultNodeState resultState)
		{
			const byte extendedSar = 255;			// Extended_SAR

			bool aspectRatioInfoPresentFlag = reader.GetBit();
			if (aspectRatioInfoPresentFlag)			// aspect_ratio_info_present_flag
			{
				byte aspectRatioIdc = reader.GetByte(false);
				if (aspectRatioIdc == extendedSar)	// aspect_ratio_idc
				{
					reader.GetBits(16);				// sar_width
					reader.GetBits(16);				// sar_height
				}
			}

			bool overscanInfoPresentFlag = reader.GetBit();
			if (overscanInfoPresentFlag)			// overscan_info_present_flag
			{
				reader.GetBit();					// overscan_appropriate_flag
			}

			bool videoSignalTypePresentFlag = reader.GetBit();
			if (videoSignalTypePresentFlag)			// video_signal_type_present_flag
			{
				reader.GetBits(3);					// video_format
				reader.GetBit();					// video_full_range_flag

				bool colourDescriptionPresentFlag = reader.GetBit();
				if (colourDescriptionPresentFlag)	// colour_description_present_flag
				{

					reader.GetByte(false);				// colour_primaries
					reader.GetByte(false);				// transfer_characteristics
					reader.GetByte(false);				// matrix_coefficients
				}
			}

			bool chromaLocInfoPresentFlag = reader.GetBit();
			if (chromaLocInfoPresentFlag)			// chroma_loc_info_present_flag
			{
				reader.GetExpGolombCoded();			// chroma_sample_loc_type_top_field
				reader.GetExpGolombCoded();			// chroma_sample_loc_type_bottom_field
			}

			bool timingInfoPresentFlag = reader.GetBit();
			if (timingInfoPresentFlag)				// timing_info_present_flag
			{
				reader.GetBits(32);					// num_units_in_tick
				reader.GetBits(32);					// time_scale
				reader.GetBit();					// fixed_frame_rate_flag
			}

			bool nalHrdParametersPresentFlag = reader.GetBit();
			if (nalHrdParametersPresentFlag)		//nal_hrd_parameters_present_flag
			{
				GetHrdParameters(reader, resultState);			// hrd_parameters()
			}

			bool vclHrdParametersPresentFlag = reader.GetBit();
			if (vclHrdParametersPresentFlag)		// vcl_hrd_parameters_present_flag
			{
				GetHrdParameters(reader, resultState);			// hrd_parameters()
			}
			if (nalHrdParametersPresentFlag || vclHrdParametersPresentFlag)
			{
				reader.GetBit();					// low_delay_hrd_flag
			}

			reader.GetBit();						// pic_struct_present_flag
			bool bitstreamRestrictionFlag = reader.GetBit();
			if (bitstreamRestrictionFlag)			// bitstream_restriction_flag
			{
				reader.GetBit();					// motion_vectors_over_pic_boundaries_flag
				reader.GetExpGolombCoded();			// max_bytes_per_pic_denom
				reader.GetExpGolombCoded();			// max_bits_per_mb_denom
				reader.GetExpGolombCoded();			// log2_max_mv_length_horizontal
				reader.GetExpGolombCoded();			// log2_max_mv_length_vertical
				reader.GetExpGolombCoded();			// num_reorder_frames
				reader.GetExpGolombCoded();			// max_dec_frame_buffering
			}
		}

		private static void GetHrdParameters(INalUnitReader reader, IState resultState)
		{
			uint cpbCntMinus1 = reader.GetExpGolombCoded();				// cpb_cnt_minus1
			reader.GetBits(4);						// bit_rate_scale
			reader.GetBits(4);						// cpb_size_scale

			// for (SchedSelIdx = 0; SchedSelIdx <= cpb_cnt_minus1; SchedSelIdx++)
			for (int schedSelIdx = 0; schedSelIdx <= cpbCntMinus1; schedSelIdx++)
			{
				if (!reader.HasMoreRbspData())
				{
					resultState.Invalidate();
					return;
				}

				reader.GetExpGolombCoded();			// bit_rate_value_minus1[SchedSelIdx]
				reader.GetExpGolombCoded();			// cpb_size_value_minus1[SchedSelIdx]
				reader.GetBit();					// cbr_flag[SchedSelIdx]
			}
			reader.GetBits(5);						// initial_cpb_removal_delay_length_minus1
			reader.GetBits(5);						// cpb_removal_delay_length_minus1
			reader.GetBits(5);						// dpb_output_delay_length_minus1
			reader.GetBits(5);						// time_offset_length
		}

		// scaling_list( scalingList, sizeOfScalingList, useDefaultScalingMatrixFlag )
		private static void ScalingList(INalUnitReader reader, int[] scalingList, int sizeOfScalingList, bool useDefaultScalingMatrixFlag)
		{
			int lastScale = 8;
			int nextScale = 8;
			for (int j = 0; j < sizeOfScalingList; j++)
			{
				if (nextScale != 0)
				{
					uint deltaScale = reader.GetExpGolombCoded();	// delta_scale
					nextScale = (int)(lastScale + deltaScale + 256) % 256;
					useDefaultScalingMatrixFlag = (j == 0 && nextScale == 0);
				}
				scalingList[j] = (nextScale == 0) ? lastScale : nextScale;
				lastScale = scalingList[j];
			}
		}
	}
}
