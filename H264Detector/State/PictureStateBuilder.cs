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

namespace Defraser.Detector.H264.State
{
	/// <summary>
	/// The implementation of <see cref="IPictureStateBuilder"/>.
	/// </summary>
	internal sealed class PictureStateBuilder : IPictureStateBuilder
	{
		#region Inner classes
		private sealed class PictureState : IPictureState
		{
			#region Properties
			public uint Id { get; private set; }
			public ISequenceState SequenceState { get; private set; }
			public bool EntropyCodingMode { get; private set; }
			public bool PictureOrderPresent { get; private set; }
			public uint SliceGroupCount { get; private set; }
			public SliceGroupMapType SliceGroupMapType { get; private set; }
			public uint[] RunLengthMinus1 { get; private set; }
			public uint[] TopLeft { get; private set; }
			public uint[] BottomRight { get; private set; }
			public bool SliceGroupChangeDirectionFlag { get; private set; }
			public uint SliceGroupChangeRate { get; private set; }
			public uint[] SliceGroupId { get; private set; }
			public uint DefaultReferencePictureCount0 { get; private set; }
			public uint DefaultReferencePictureCount1 { get; private set; }
			public bool WeightedPrediction { get; private set; }
			public WeightedBidirectionalPredictionType WeightedBidirectionalPrediction { get; private set; }
			public int PicInitQpMinus26 { get; private set; }
			public bool DeblockingFilterControlPresent { get; private set; }
			public bool RedundantPictureControlPresent { get; private set; }
			public bool Transform8X8ModeFlag { get; private set; }
			public SliceType? PictureSliceType { get; set; }
			#endregion Properties

			public PictureState(PictureStateBuilder builder)
			{
				Id = builder._id;
				SequenceState = builder._sequenceState;
				EntropyCodingMode = builder.EntropyCodingMode;
				PictureOrderPresent = builder.BottomFieldPicOrderInFramePresentFlag;
				SliceGroupCount = builder.NumSliceGroupsMinus1 + 1;
				SliceGroupMapType = builder.SliceGroupMapType;
				RunLengthMinus1 = builder.RunLengthMinus1;
				TopLeft = builder.TopLeft;
				BottomRight = builder.BottomRight;
				SliceGroupChangeDirectionFlag = builder.SliceGroupChangeDirectionFlag;
				SliceGroupChangeRate = builder.SliceGroupChangeRateMinus1 + 1;
				SliceGroupId = builder.SliceGroupId;
				DefaultReferencePictureCount0 = builder.NumRefIdxL0DefaultActiveMinus1 + 1;
				DefaultReferencePictureCount1 = builder.NumRefIdxL1DefaultActiveMinus1 + 1;
				WeightedPrediction = builder.WeightedPredFlag;
				WeightedBidirectionalPrediction = builder.WeightedBipredIdc;
				PicInitQpMinus26 = builder.PicInitQpMinus26;
				DeblockingFilterControlPresent = builder.DeblockingFilterControlPresentFlag;
				RedundantPictureControlPresent = builder.RedundantPicCntPresentFlag;
				Transform8X8ModeFlag = builder.Transform8X8ModeFlag;
			}

			public void CopyTo(IH264State state)
			{
				state.PictureStates[Id] = this;
				state.SequenceStates[SequenceState.Id] = SequenceState;
			}
		}
		#endregion Inner classes

		private readonly uint _id;
		private readonly ISequenceState _sequenceState;

		#region Properties
		public bool EntropyCodingMode { private get; set; }
		public bool BottomFieldPicOrderInFramePresentFlag { private get; set; }
		public uint NumSliceGroupsMinus1 { private get; set; }
		public SliceGroupMapType SliceGroupMapType { private get; set; }
		public uint[] RunLengthMinus1 { private get; set; }
		public uint[] TopLeft { private get; set; }
		public uint[] BottomRight { private get; set; }
		public bool SliceGroupChangeDirectionFlag { private get; set; }
		public uint SliceGroupChangeRateMinus1 { private get; set; }
		public uint[] SliceGroupId { private get; set; }
		public uint NumRefIdxL0DefaultActiveMinus1 { private get; set; }
		public uint NumRefIdxL1DefaultActiveMinus1 { private get; set; }
		public bool WeightedPredFlag { private get; set; }
		public WeightedBidirectionalPredictionType WeightedBipredIdc { private get; set; }
		public int PicInitQpMinus26 { private get; set; }
		public bool DeblockingFilterControlPresentFlag { private get; set; }
		public bool RedundantPicCntPresentFlag { private get; set; }
		public bool Transform8X8ModeFlag { private get; set; }
		#endregion Properties

		public PictureStateBuilder(uint id, ISequenceState sequenceState)
		{
			_id = id;
			_sequenceState = sequenceState;

			// Defaults
			WeightedBipredIdc = WeightedBidirectionalPredictionType.DefaultWeightedPrediction;
			SliceGroupMapType = SliceGroupMapType.InterleavedSliceGroups;
		}

		public IPictureState Build()
		{
			return new PictureState(this);
		}
	}
}
