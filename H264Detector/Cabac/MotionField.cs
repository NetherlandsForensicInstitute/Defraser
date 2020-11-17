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
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264.Cabac
{
	internal sealed class MotionField
	{
		private readonly ISliceState  _sliceState;
		private readonly uint _picWidthInMbs;
		private readonly uint _picHeightInMbs;
		private readonly byte[,,] _motionVectors;

		public MotionField(ISliceState sliceState)
		{
			_sliceState = sliceState;

			ISequenceState sequenceState = sliceState.PictureState.SequenceState;
			_picWidthInMbs = sequenceState.PicWidthInMbs;
			_picHeightInMbs = sequenceState.FrameHeightInMbs / (sliceState.FieldPicFlag ? 2U : 1);

			_motionVectors = new byte[(_picWidthInMbs * _picHeightInMbs), 16, 2];
		}

		// 9.3.3.1.1.7 Derivation process of ctxIdxInc for the syntax elements mvd_l0 and mvd_l1
		public uint GetCtxIdxInc(uint mbAddr, MacroblockPartition mbPart, uint compIdx)
		{
			uint absMvdCompA;
			if (mbPart.SubMbPartIdxA < 16)
			{
				absMvdCompA = _motionVectors[mbAddr, mbPart.SubMbPartIdxA, compIdx];
			}
			else
			{
				uint mbAddrA = _sliceState.GetMbAddrA(mbAddr);
				absMvdCompA = (mbAddrA == uint.MaxValue) ? 0U : _motionVectors[mbAddrA, (mbPart.SubMbPartIdxA & 15), compIdx];
			}

			uint absMvdCompB;
			if (mbPart.SubMbPartIdxB < 16)
			{
				absMvdCompB = _motionVectors[mbAddr, mbPart.SubMbPartIdxB, compIdx];
			}
			else
			{
				uint mbAddrB = _sliceState.GetMbAddrB(mbAddr);
				absMvdCompB = (mbAddrB == uint.MaxValue) ? 0U : _motionVectors[mbAddrB, (mbPart.SubMbPartIdxB & 15), compIdx];
			}

			uint absMvdComp = (absMvdCompA + absMvdCompB);
			return (absMvdComp < 3) ? 0U : ((absMvdComp > 32) ? 2U : 1U);
		}

		public void UpdateComponent(uint mbAddr, MacroblockPartition mbPart, uint compIdx, uint absValue)
		{
			var byteValue = (byte)Math.Min(absValue, 127);
			foreach (uint subBlkIdx in mbPart.MbPartSubBlks)
			{
				_motionVectors[mbAddr, subBlkIdx, compIdx] = byteValue;
			}
		}
	}
}
