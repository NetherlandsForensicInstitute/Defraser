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
	internal class SliceHeader : IResultParser<INalUnitReader>
	{
		internal enum Attribute
		{
			FirstMacroblockInSlice,
			SliceType,
			PictureParameterSetId,
			FrameNumber,
			ColorPlane,
			IdrPictureId,
			RedundantPictureCounter
		}

		private readonly EnumResultFormatter<SliceType> _sliceTypeResultFormatter;
		private readonly EnumResultFormatter<ColorPlane> _colorPlaneResultFormatter;

		public SliceHeader(EnumResultFormatter<SliceType> sliceTypeResultFormatter, EnumResultFormatter<ColorPlane> colorPlaneResultFormatter)
		{
			_sliceTypeResultFormatter = sliceTypeResultFormatter;
			_colorPlaneResultFormatter = colorPlaneResultFormatter;
		}

		// 7.3.3 Slice header syntax
		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			reader.State.SliceState = null; // Invalid, unless the slice header is valid!

			uint firstMacroblockInSlice = reader.GetExpGolombCoded(Attribute.FirstMacroblockInSlice, uint.MaxValue); // first_mb_in_slice
#if DEBUG
			H264Debug.WriteLine("+ first_mb_in_slice={0}", firstMacroblockInSlice);
#endif
			SliceType pictureSliceType = (SliceType)reader.GetExpGolombCoded(Attribute.SliceType, _sliceTypeResultFormatter);
			SliceType sliceType = (SliceType)((int)pictureSliceType % 5);
#if DEBUG
			H264Debug.WriteLine("+ slice_type={0}", (int)sliceType);
#endif
			byte pictureParamaterSetId = (byte)reader.GetExpGolombCoded(Attribute.PictureParameterSetId, 255);
#if DEBUG
			H264Debug.WriteLine("+ pic_parameter_set_id={0}", pictureParamaterSetId);
#endif
			var pictureState = reader.State.PictureStates[pictureParamaterSetId];
			if (pictureState == null)
			{
				if (resultState.Valid)
				{
					if (IsFalseHit(firstMacroblockInSlice, sliceType, pictureParamaterSetId) || (pictureParamaterSetId > 10))
					{
						resultState.Invalidate();
					}
				}
				return; // cannot validate slice header!!
			}

			ISliceStateBuilder builder = new SliceStateBuilder(pictureState);
			builder.FirstMacroBlockInSlice = firstMacroblockInSlice;
			builder.SliceType = sliceType;

			if (pictureState.PictureSliceType.HasValue && (sliceType != pictureState.PictureSliceType))
			{
				// FIXME: This rule only applies to slices within a single picture.
				//        Since we currently don't detect whether this slice starts a
				//        new picture, we have to disable this rule for now!
				//				oldIReaderState.Invalidate();
				//				return;
			}
			if (/*!pictureState.PictureSliceType.HasValue && */sliceType != pictureSliceType)
			{
				pictureState.PictureSliceType = pictureSliceType;
			}
			if (IsNonIntraSliceInIntraCodedPicture(reader, sliceType, pictureState))
			{
				resultState.Invalidate();
				return;
			}

			var sequenceState = pictureState.SequenceState;
			if (sequenceState.ChromaFormat == ChromaFormat.SeparateColorPlane)
			{
				reader.GetBits(2, Attribute.ColorPlane, _colorPlaneResultFormatter); // colour_plane_id
			}

			ParseFrameNum(reader, sequenceState);

			bool fieldPicFlag = false;
			if (!sequenceState.FrameMbsOnlyFlag)
			{
				fieldPicFlag = reader.GetBit(); // field_pic_flag
				builder.FieldPicFlag = fieldPicFlag;
				if (fieldPicFlag)
				{
					reader.GetBit(); // bottom_field_flag
				}
			}
			if (reader.State.IdrPicFlag)
			{
				reader.GetExpGolombCoded(Attribute.IdrPictureId, 65535); // idr_pic_id
			}
			if (sequenceState.PictureOrderCountType == 0)
			{
				reader.GetBits((int) sequenceState.Log2MaxPicOrderCntLsb); // pic_order_cnt_lsb

				if (pictureState.PictureOrderPresent && !fieldPicFlag)
				{
					reader.GetSignedExpGolombCoded(); // delta_pic_order_cnt_bottom
				}
			}
			else if ((sequenceState.PictureOrderCountType == 1) && !sequenceState.DeltaPicOrderAlwaysZeroFlag)
			{
				reader.GetSignedExpGolombCoded(); // delta_pic_order_cnt_0

				if (pictureState.PictureOrderPresent && !fieldPicFlag)
				{
					reader.GetSignedExpGolombCoded(); // delta_pic_order_cnt_1
				}
			}
			if (pictureState.RedundantPictureControlPresent)
			{
				reader.GetExpGolombCoded(Attribute.RedundantPictureCounter, 127); // redundant_pic_cnt
			}
			if (sliceType == SliceType.B)
			{
				reader.GetBit(); // direct_spatial_mv_pred_flag
			}

			uint activeReferencePictureCount0 = pictureState.DefaultReferencePictureCount0;
			uint activeReferencePictureCount1 = pictureState.DefaultReferencePictureCount1;
			if (!IsIntraCoded(sliceType) && reader.GetBit()) // num_ref_idx_active_override_flag
			{
				uint maxRefIdxActiveMinus1 = fieldPicFlag ? 31U : 15U;
				activeReferencePictureCount0 = reader.GetExpGolombCoded(maxRefIdxActiveMinus1) + 1; // num_ref_idx_l0_active_minus1

				if (sliceType == SliceType.B)
				{
					activeReferencePictureCount1 = reader.GetExpGolombCoded(maxRefIdxActiveMinus1) + 1; // num_ref_idx_l1_active_minus1
				}
			}

			builder.ActiveReferencePictureCount0 = activeReferencePictureCount0;
			builder.ActiveReferencePictureCount1 = activeReferencePictureCount1;

			RefPicListModification(reader, resultState, sliceType);

			if ((pictureState.WeightedPrediction && (sliceType == SliceType.P || sliceType == SliceType.Sp)) ||
				(pictureState.WeightedBidirectionalPrediction == WeightedBidirectionalPredictionType.ExplicitWeightedPrediction && sliceType == SliceType.B))
			{
				PredWeightTable(reader, sequenceState, sliceType,activeReferencePictureCount0, activeReferencePictureCount1);
			}
			if (reader.State.NalRefIdc != 0)
			{
				DecRefPicMarking(reader, resultState);
			}
			if (pictureState.EntropyCodingMode && !IsIntraCoded(sliceType))
			{
				builder.CabacInitIdc = reader.GetExpGolombCoded(2U);
			}

			builder.SliceQpDelta = reader.GetSignedExpGolombCoded();

			if ((sliceType == SliceType.Sp) || (sliceType == SliceType.Si))
			{
				if (sliceType == SliceType.Sp)
				{
					reader.GetBit();// sp_for_switch_flag
				}

				reader.GetSignedExpGolombCoded();// slice_qs_delta
			}
			if (pictureState.DeblockingFilterControlPresent)
			{
				uint disableDeblockingFilterIdc = reader.GetExpGolombCoded();	// disable_deblocking_filter_idc
				if (disableDeblockingFilterIdc != 1)
				{
					reader.GetSignedExpGolombCoded();	// slice_alpha_c0_offset_div2
					reader.GetSignedExpGolombCoded();	// slice_beta_offset_div2
				}
			}
			if (pictureState.SliceGroupCount > 1 &&
				pictureState.SliceGroupMapType >= SliceGroupMapType.ChangingSliceGroups3 && pictureState.SliceGroupMapType <= SliceGroupMapType.ChangingSliceGroups5)
			{
				uint value = (sequenceState.PicSizeInMapUnits / pictureState.SliceGroupChangeRate);
				int requiredBitsToRepresentValue = DetectorUtils.Log2(value) + 1;
				builder.SliceGroupChangeCycle = reader.GetBits(requiredBitsToRepresentValue); // slice_group_change_cycle;
			}

			// Check 'firstMacroblockInSlice' field
			uint picHeightInMbs = sequenceState.FrameHeightInMbs / (fieldPicFlag ? 2U : 1U);
			uint picSizeInMbs = sequenceState.PicWidthInMbs * picHeightInMbs;
			bool mbAffFrameFlag = sequenceState.MbAdaptiveFrameFieldFlag && !fieldPicFlag;
			if ((firstMacroblockInSlice * (mbAffFrameFlag ? 2U : 1U)) >= picSizeInMbs)
			{
				resultState.Invalidate();
			}

			if (resultState.Valid)
			{
				// Since the 'pictureParameterSetId' is probably correct at this point,
				// we prevent the check for 'pictureParameterSetId > 10'.
				if (IsFalseHit(firstMacroblockInSlice, sliceType, Math.Min((byte)1, pictureParamaterSetId)))
				{
					resultState.Invalidate();
				}
				else
				{
					reader.State.SliceState = builder.Build();
				}
			}
		}

		private static bool IsFalseHit(uint firstMacroblockInSlice, SliceType sliceType, uint pictureParamaterSetId)
		{
			int falseHitLikelihood = H264Utils.ComputeFalseHitLikelihoodScore(firstMacroblockInSlice, pictureParamaterSetId, sliceType);
			return (falseHitLikelihood > 1);
		}

		private static bool IsIntraCoded(SliceType sliceType)
		{
			return (sliceType == SliceType.I) || (sliceType == SliceType.Si);
		}

		private static void ParseFrameNum(INalUnitReader reader, ISequenceState sequenceState)// TODO:checks uitbreiden volgens 7.4.3(Current Header Semantics).frame_num specs
		{
			uint frameNum = reader.GetBits((int)sequenceState.Log2MaxFrameNum, Attribute.FrameNumber);
#if DEBUG
			H264Debug.WriteLine("+ frame_num={0} ({1} bits)", frameNum, sequenceState.Log2MaxFrameNum);
#endif
			if (reader.State.IdrPicFlag && (frameNum != 0))
			{
				//FIXME: Some H.264 files seem to use frame number != 0 for IDR pictures!?
				//readerState.Invalidate();
			}
		}

		private static bool IsNonIntraSliceInIntraCodedPicture(INalUnitReader reader, SliceType sliceType, IPictureState pictureState)
		{
			if ((reader.State.NalUnitType != NalUnitType.CodedSliceOfAnIdrPicture) && (pictureState.SequenceState.MaxNumRefFrames > 0))
			{
				return false; // Not an intra coded picture
			}

			// Check that the slice type indicates an inter coded slice
			return !IsIntraCoded(sliceType);
		}

		// 7.3.3.1 Reference picture list modification syntax
		private static void RefPicListModification(INalUnitReader reader, IResultNodeState resultState, SliceType sliceType)
		{
			if (!IsIntraCoded(sliceType))
			{
				RefPicListModification(reader, resultState);
			}
			if (sliceType == SliceType.B)
			{
				RefPicListModification(reader, resultState);
			}
		}

// ReSharper disable UnusedParameter.Local
		private static void RefPicListModification(INalUnitReader reader, IResultNodeState resultState)
// ReSharper restore UnusedParameter.Local
		{
			bool refPicListReorderingFlag = reader.GetBit();
			if (refPicListReorderingFlag) // ref_pic_list_reordering_flag_lX
			{
				uint reorderingOfPicNumsIdc; // reordering_of_pic_nums_idc
				do
				{
					if (!reader.HasMoreRbspData())
					{
						resultState.Invalidate();
						return;
					}

					reorderingOfPicNumsIdc = reader.GetExpGolombCoded(); // reordering_of_pic_nums_idc

					if ((reorderingOfPicNumsIdc == 0) || (reorderingOfPicNumsIdc == 1))
					{
						reader.GetExpGolombCoded(); // abs_diff_pic_num_minus1
					}
					else if (reorderingOfPicNumsIdc == 2)
					{
						reader.GetExpGolombCoded(); // long_term_pic_num
					}
				} while (reorderingOfPicNumsIdc != 3);
			}
		}

		// 7.3.3.2 Prediction weight table syntax
		private static void PredWeightTable(INalUnitReader reader, ISequenceState sequenceState, SliceType sliceType, uint activeReferencePictureCount0, uint activeReferencePictureCount1)
		{
			reader.GetExpGolombCoded(); // luma_log2_weight_denom

			bool monochrome = (sequenceState.ChromaFormat.NumC8X8 == 0);
			if (!monochrome)
			{
				reader.GetExpGolombCoded(); // chroma_log2_weight_denom
			}
			for (int i = 0; i < activeReferencePictureCount0; i++)
			{
				if (reader.GetBit()) // luma_weight_l0_flag
				{
					reader.GetSignedExpGolombCoded(); // luma_weight_l0[ i ]
					reader.GetSignedExpGolombCoded(); // luma_offset_l0[ i ]
				}
				if (!monochrome && reader.GetBit()) // chroma_weight_l0_flag
				{
					for (int j = 0; j < 2; j++)
					{
						reader.GetSignedExpGolombCoded(); // chroma_weight_l0[i][j]
						reader.GetSignedExpGolombCoded(); // chroma_offset_l0[i][j]
					}
				}
			}
			if (sliceType == SliceType.B)
			{
				for (int i = 0; i < activeReferencePictureCount1; i++)
				{
					if (reader.GetBit()) // luma_weight_l1_flag
					{
						reader.GetSignedExpGolombCoded(); // luma_weight_l1[i]
						reader.GetSignedExpGolombCoded(); // luma_offset_l1[i]
					}
					if (!monochrome && reader.GetBit()) // chroma_weight_l1_flag
					{
						for (int j = 0; j < 2; j++)
						{
							reader.GetSignedExpGolombCoded(); // chroma_weight_l1[i][j]
							reader.GetSignedExpGolombCoded(); // chroma_offset_l1[i][j]
						}
					}
				}
			}
		}

		// 7.3.3.3 Decoded reference picture marking syntax
		private static void DecRefPicMarking(INalUnitReader reader, IResultNodeState resultState)
		{
			if (reader.State.NalUnitType == NalUnitType.CodedSliceOfAnIdrPicture)
			{
				reader.GetBit(); // no_output_of_prior_pics_flag
				reader.GetBit(); // long_term_reference_flag
			}
			else if (reader.GetBit()) // adaptive_ref_pic_marking_mode_flag
			{
				uint memoryManagementControlOperation;
				do
				{
					if (!reader.HasMoreRbspData())
					{
						resultState.Invalidate();
						return;
					}

					memoryManagementControlOperation = reader.GetExpGolombCoded(); // memory_management_control_operation

					if ((memoryManagementControlOperation == 1) || (memoryManagementControlOperation == 3))
					{
						reader.GetExpGolombCoded(); // difference_of_pic_nums_minus1
					}
					if (memoryManagementControlOperation == 2)
					{
						reader.GetExpGolombCoded(); // long_term_pic_num
					}
					if ((memoryManagementControlOperation == 3) || (memoryManagementControlOperation == 6))
					{
						reader.GetExpGolombCoded(); // long_term_frame_idx
					}
					if (memoryManagementControlOperation == 4)
					{
						reader.GetExpGolombCoded(); // max_long_term_frame_idx_plus1
					}
				} while (memoryManagementControlOperation != 0);
			}
		}
	}
}
