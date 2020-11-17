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

namespace Defraser.Detector.H264.State
{
	internal interface IPictureState
	{
		#region Properties
		uint Id { get; }
		ISequenceState SequenceState { get; }
		bool EntropyCodingMode { get; }
		bool PictureOrderPresent { get; }
		uint SliceGroupCount { get; }
		SliceGroupMapType SliceGroupMapType { get; }
		uint[] RunLengthMinus1 { get; }
		uint[] TopLeft { get; }
		uint[] BottomRight { get; }
		bool SliceGroupChangeDirectionFlag { get; }
		uint SliceGroupChangeRate { get; }
		uint[] SliceGroupId { get; }
		/// <summary>1 + num_ref_idx_l0_default_active_minus1</summary>
		uint DefaultReferencePictureCount0 { get; }
		/// <summary>1 + num_ref_idx_l1_default_active_minus1</summary>
		uint DefaultReferencePictureCount1 { get; }
		bool WeightedPrediction { get; }
		WeightedBidirectionalPredictionType WeightedBidirectionalPrediction { get; }
		int PicInitQpMinus26 { get; }
		bool DeblockingFilterControlPresent { get; }
		bool RedundantPictureControlPresent { get; }
		/// <summary>transform_8x8_mode_flag</summary>
		bool Transform8X8ModeFlag { get; }

		/// <summary>The slice type for ALL slices in this picture.</summary>
		SliceType? PictureSliceType { get; set; }
		#endregion Properties

		void CopyTo(IH264State state);
	}
}
