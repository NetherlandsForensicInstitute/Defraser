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

namespace Defraser.Detector.H264
{
	internal struct ReferencePictureIndex
	{
		// 0    : 16x16
		// 1..2 : 16x8
		// 3..4 : 8x16
		// 5..8 : 8x8
		private static readonly int[] CondTermFlagsA =
		{
			10, 10, 12, 10, 1, 10, 1, 12, 3
		};
		private static readonly int[] CondTermFlagsB =
		{
			6, 6, 0, 6, 7, 6, 7, 0, 1
		};
		private static readonly uint[] RefIdxNonZeroMasks =
		{
			0x1e, 0x6, 0x18, 0xa, 0x14, 0x2, 0x4, 0x8, 0x10
		};

		private uint _refIdxNonZeroFlags;

		#region Properties
		public byte Bits { get { return (byte)((_refIdxNonZeroFlags >> 1) & 0xf); } }
		#endregion Properties

		public ReferencePictureIndex(byte refIdxNonZeroA, byte refIdxNonZeroB)
		{
			_refIdxNonZeroFlags = ((uint)refIdxNonZeroA << 9) | ((uint)refIdxNonZeroB << 5);
		}

		// 9.3.3.1.1.6 Derivation process of ctxIdxInc for the syntax elements ref_idx_l0 and ref_idx_l1
		public uint GetCtxIdxInc(uint mbPartIdx)
		{
			return ((_refIdxNonZeroFlags >> CondTermFlagsA[mbPartIdx]) & 1) + // condTermFlagA
			       ((_refIdxNonZeroFlags >> CondTermFlagsB[mbPartIdx]) & 2);  // 2*condTermFlagB
		}

		public void SetNonZeroRefIdx(uint mbPartIdx)
		{
			_refIdxNonZeroFlags |= RefIdxNonZeroMasks[mbPartIdx];
		}
	}
}
