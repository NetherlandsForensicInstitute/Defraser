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
	internal sealed class PictureParameterSet : INalUnitPayloadParser
	{
		internal enum Attribute
		{
			PictureParameterSetId,
			SequenceParameterSetId,
			EntropyCodingMode,
			PictureOrderPresent,
			NumSliceGroupsMinus1,
			SliceGroupMapType,
			NumRefIdxl0DefaultActiveMinus1,
			NumRefIdxl1DefaultActiveMinus1,
			WeightedPrediction,
			WeightedBidirectionalPrediction,
			PictureInitialQPyMinus26,
			PictureInitialQSyMinus26,
			ChromaQpIndexOffset,
			DeblockingFilterControlPresent,
			ConstrainedIntraPrediction,
			RedundantPictureControlPresent,
		}

		internal const string Name = "PictureParameterSet";

		private readonly EnumResultFormatter<WeightedBidirectionalPredictionType> _weightBidirectionalPredictionFormatter;
		private readonly IValidityResultFormatter _sliceGroupMapTypeFormatter;

		#region Properties
		public NalUnitType UnitType { get { return NalUnitType.PictureParameterSet; } }
		#endregion Properties

		public PictureParameterSet()
		{
			_weightBidirectionalPredictionFormatter = new EnumResultFormatter<WeightedBidirectionalPredictionType>();
			_sliceGroupMapTypeFormatter = new EnumResultFormatter<SliceGroupMapType>();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		// 7.3.2.2 PictureState parameter set RBSP syntax
		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			uint pictureParameterSetId = reader.GetExpGolombCoded(Attribute.PictureParameterSetId, 255/*7.4.2.2*/);
			uint sequenceParameterSetId = reader.GetExpGolombCoded(Attribute.SequenceParameterSetId, 31/*7.4.2.2*/);
			var sequenceState = reader.State.SequenceStates[sequenceParameterSetId];
			if (sequenceState == null)
			{
				resultState.Invalidate();
				return;
			}

			IPictureStateBuilder builder = new PictureStateBuilder(pictureParameterSetId, sequenceState);
			builder.EntropyCodingMode = reader.GetBit(Attribute.EntropyCodingMode);
			builder.BottomFieldPicOrderInFramePresentFlag = reader.GetBit(Attribute.PictureOrderPresent);
			uint numSliceGroupsMinus1 = reader.GetExpGolombCoded(Attribute.NumSliceGroupsMinus1, 7/*A.2*/);
			builder.NumSliceGroupsMinus1 = numSliceGroupsMinus1;

			if (numSliceGroupsMinus1 > 0)
			{
				if (sequenceState.Profile == Profile.Main)
				{
					resultState.Invalidate();
					return;
				}

				ParseSliceGroupMap(reader, resultState, builder, (numSliceGroupsMinus1 + 1));

				// FIXME: this code contains too many bugs, so it is disabled for now!
				resultState.Invalidate();
				// (end of FIXME)
				if (!resultState.Valid)
				{
					return;
				}
			}

			builder.NumRefIdxL0DefaultActiveMinus1 = reader.GetExpGolombCoded(Attribute.NumRefIdxl0DefaultActiveMinus1, 31/*7.4.2.2*/);
			builder.NumRefIdxL1DefaultActiveMinus1 = reader.GetExpGolombCoded(Attribute.NumRefIdxl1DefaultActiveMinus1, 31/*7.4.2.2*/);
			builder.WeightedPredFlag = reader.GetBit(Attribute.WeightedPrediction);
			builder.WeightedBipredIdc = reader.GetBits(2, Attribute.WeightedBidirectionalPrediction, _weightBidirectionalPredictionFormatter);
			// SliceQPY = 26 + PicInitQpMinus26 + slice_qp_delta
			int qpBdOffset = (6 * ((int)sequenceState.BitDepthLuma - 8));
			builder.PicInitQpMinus26 = reader.GetSignedExpGolombCoded(Attribute.PictureInitialQPyMinus26, -(26 + qpBdOffset), 25);
			reader.GetSignedExpGolombCoded(Attribute.PictureInitialQSyMinus26, -26, 25); // pic_init_qs_minus26; relative to 26
			reader.GetSignedExpGolombCoded(Attribute.ChromaQpIndexOffset, -12, 12);	 // chroma_qp_index_offset
			builder.DeblockingFilterControlPresentFlag = reader.GetBit(Attribute.DeblockingFilterControlPresent);
			reader.GetBit(Attribute.ConstrainedIntraPrediction); // constrained_intra_pred_flag
			builder.RedundantPicCntPresentFlag = reader.GetBit(Attribute.RedundantPictureControlPresent);

			// Note: Total size upto this point is between 11 and 59 bits (+ 'slice group map')

			if (reader.HasMoreRbspData())				// more_rbsp_data()
			{
				builder.Transform8X8ModeFlag = reader.GetBit(); // transform_8x8_mode_flag
				bool picScalingMatrixPresentFlag = reader.GetBit();
				//if (picScalingMatrixPresentFlag)		// pic_scaling_matrix_present_flag
				//{
				//    for (int i = 0; i < 6 + 2 * pictureState.transform_8x8_mode_flag; i++)
				//    {
				//        bool picScalingListPresentFlag = reader.GetBit();
				//        if (picScalingListPresentFlag)	// pic_scaling_list_present_flag[i]
				//        {
				//            if (i < 6)
				//            {
				//                throw new NotImplementedException("scaling_list(ScalingList4x4[i], 16, UseDefaultScalingMatrix4x4Flag[i]);");	// TODO
				//            }
				//            else
				//            {
				//                throw new NotImplementedException("scaling_list(ScalingList8x8[i – 6], 64, UseDefaultScalingMatrix8x8Flag[i – 6]);");	// TODO
				//            }
				//        }
				//    }
				//}
				reader.GetSignedExpGolombCoded(); // second_chroma_qp_index_offset
			}

			// rbsp_trailing_bits()

			if (reader.ShowBits(1) == 1)
			{
				reader.GetBit(); // rbsp_stop_one_bit (equal to 1)
				// trailing zero bits
			}

			if (resultState.Valid)
			{
				reader.State.PictureStates[pictureParameterSetId] = builder.Build();
			}
		}

		private void ParseSliceGroupMap(INalUnitReader reader, IState resultState, IPictureStateBuilder builder, uint sliceGroupCount)
		{
			var sliceGroupMapType = (SliceGroupMapType)reader.GetExpGolombCoded(Attribute.SliceGroupMapType, _sliceGroupMapTypeFormatter); // slice_group_map_type
			builder.SliceGroupMapType = sliceGroupMapType;

			switch (sliceGroupMapType)
			{
				case SliceGroupMapType.InterleavedSliceGroups:
					uint[] runLengthMinus1 = new uint[sliceGroupCount];
					for (int iGroup = 0; iGroup < sliceGroupCount; iGroup++)
					{
						runLengthMinus1[iGroup] = reader.GetExpGolombCoded(); // run_length_minus1[iGroup]
					}
					builder.RunLengthMinus1 = runLengthMinus1;
					break;

				case SliceGroupMapType.DispersedSliceGroups:
					break;

				case SliceGroupMapType.ForegroundAndLeftoverSliceGroups:
					uint[] topLeft = new uint[sliceGroupCount - 1];
					uint[] bottomRight = new uint[sliceGroupCount - 1];
					for (int iGroup = 0; iGroup < (sliceGroupCount - 1); iGroup++)
					{
						topLeft[iGroup] = reader.GetExpGolombCoded();
						bottomRight[iGroup] = reader.GetExpGolombCoded();
					}
					builder.TopLeft = topLeft;
					builder.BottomRight = bottomRight;
					break;

				case SliceGroupMapType.ChangingSliceGroups3:
				case SliceGroupMapType.ChangingSliceGroups4:
				case SliceGroupMapType.ChangingSliceGroups5:
					builder.SliceGroupChangeDirectionFlag = reader.GetBit();
					builder.SliceGroupChangeRateMinus1 = reader.GetExpGolombCoded();
					break;

				case SliceGroupMapType.ExplicitSliceGroups:
					uint pictureSizeInMapUnits = 1 + reader.GetExpGolombCoded(); // pic_size_in_map_units_minus1
					int syntaxElementSize =  DetectorUtils.Log2(sliceGroupCount - 1) + 1;
					uint[] sliceGroupId = new uint[pictureSizeInMapUnits];
					for (int i = 0; i < pictureSizeInMapUnits; i++)
					{
						sliceGroupId[i] = reader.GetBits(syntaxElementSize);
					}
					builder.SliceGroupId = sliceGroupId;
					break;

				default:
					resultState.Invalidate();
					break;
			}
		}
	}
}
