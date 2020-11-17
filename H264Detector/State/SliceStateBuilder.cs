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
	internal sealed class SliceStateBuilder : ISliceStateBuilder
	{
		#region Inner classes
		private sealed class SliceState : ISliceState
		{
			#region Properties
			public IPictureState PictureState { get; private set; }
			public uint FirstMacroBlockInSlice { get; set; }
			public SliceType SliceType { get; set; }
			public bool IntraCoded { get; private set; }
			public bool FieldPicFlag { get; private set; }
			public bool MbAffFrameFlag { get; private set; }
			public uint SliceGroupChangeCycle { get; private set; }
			private ISequenceState Sequence { get { return PictureState.SequenceState; } }
			public uint CabacInitIdc { get; private set; }
			public int SliceQPy { get; private set; }
			public uint ActiveReferencePictureCount0 { get; set; }
			public uint ActiveReferencePictureCount1 { get; set; }
			#endregion Properties

			private readonly uint _picWidthInMbs;

			public SliceState(SliceStateBuilder builder)
			{
				PictureState = builder.PictureState;
				FirstMacroBlockInSlice = builder.FirstMacroBlockInSlice;
				SliceType = (SliceType)((int)builder.SliceType % 5);
				IntraCoded = (SliceType == SliceType.I) || (SliceType == SliceType.Si);
				FieldPicFlag = builder.FieldPicFlag;
				MbAffFrameFlag = PictureState.SequenceState.MbAdaptiveFrameFieldFlag && !FieldPicFlag;
				ActiveReferencePictureCount0 = builder.ActiveReferencePictureCount0;
				ActiveReferencePictureCount1 = builder.ActiveReferencePictureCount1;
				CabacInitIdc = builder.CabacInitIdc;
				SliceQPy = 26 + PictureState.PicInitQpMinus26 + builder.SliceQpDelta;
				SliceGroupChangeCycle = builder.SliceGroupChangeCycle;

				_picWidthInMbs = PictureState.SequenceState.PicWidthInMbs;
			}

			public uint GetMbAddrA(uint currMbAddr)
			{
				if (MbAffFrameFlag)
				{
					uint currMbPairAddr = (currMbAddr >> 1);
					return (IsLeftMostMacrobBlock(currMbPairAddr) ? uint.MaxValue : (currMbPairAddr - 1)) << 1;
				}

				return IsLeftMostMacrobBlock(currMbAddr) ? uint.MaxValue : (currMbAddr - 1);
			}

			private bool IsLeftMostMacrobBlock(uint mbAddr)
			{
				return (mbAddr % _picWidthInMbs) == 0;
			}

			public uint GetMbAddrB(uint currMbAddr)
			{
				if (MbAffFrameFlag)
				{
					uint currMbPairAddr = (currMbAddr >> 1);
					return (IsTopMostMacroBlock(currMbPairAddr) ? uint.MaxValue : (currMbPairAddr - _picWidthInMbs)) << 1;
				}

				return IsTopMostMacroBlock(currMbAddr) ? uint.MaxValue : (currMbAddr - _picWidthInMbs);
			}

			private bool IsTopMostMacroBlock(uint mbAddr)
			{
				return (mbAddr < _picWidthInMbs);
			}
		}
		#endregion Inner classes

		#region Properties
		private IPictureState PictureState { get; set; }
		public uint FirstMacroBlockInSlice { private get; set; }
		public SliceType SliceType { private get; set; }
		public bool FieldPicFlag { private get; set; }
		public uint ActiveReferencePictureCount0 { private get; set; }
		public uint ActiveReferencePictureCount1 { private get; set; }
		public uint CabacInitIdc { private get; set; }
		public int SliceQpDelta { private get; set; }
		public uint SliceGroupChangeCycle { private get; set; }
		#endregion Properties

		public SliceStateBuilder(IPictureState pictureState)
		{
			PictureState = pictureState;
		}

		public ISliceState Build()
		{
			return new SliceState(this);
		}
	}
}
