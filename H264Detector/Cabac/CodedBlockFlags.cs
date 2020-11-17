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

namespace Defraser.Detector.H264.Cabac
{
	internal struct CodedBlockFlags
	{
		//  0..15 : 4x4
		// 16..19 : 8x8
		// 20..23 : Cb/Cr 4:2:0
		// 24..31 : Cb/Cr 4:2:2
		//     32 : DC
		private static readonly int[] CondTermFlagsA =
		{
			22, 29, 21, 28, 26, 12, 25, 31, 20, 27, 19, 0, 24, 30, 1, 2,
			22, 26, 20, 24, 20, 29, 19, 2, 22, 29, 21, 28, 20, 27, 19, 2,
			23
		};
		private static readonly int[] CondTermFlagsB =
		{
			7, 8, 28, 25, 9, 10, 11, 5, 27, 24, 26, 23, 30, 4, 29, 3,
			7, 9, 27, 30, 9, 10, 28, 3, 9, 10, 28, 5, 27, 4, 26, 3,
			14
		};
		private static readonly uint[] CodedBlockFlagMasks =
		{
			0x20000000, 0x4000000, 0x10000000, 0x2000000, 0x1000, 0x40, 0x80000000, 0x20,
			0x8000000, 0x1000000, 0x1, 0x2, 0x40000000, 0x10, 0x4, 0x8,
			0x36000000, 0x80001060, 0x9000003, 0x4000001c, 0x20000000, 0x10, 0x4, 0x8,
			0x20000000, 0x40, 0x10000000, 0x20, 0x8000000, 0x10, 0x4, 0x8,
			0x80,
		};

		// private const uint NonDcMask = 0x7f;
		// private const uint DcMask = 0x80;

		private uint _codedBlockFlags;

		#region Properties
		public byte Flags { get { return (byte)_codedBlockFlags; } }
		#endregion Properties

		public CodedBlockFlags(byte codedBlockFlagsA, byte codedBlockFlagsB)
		{
			_codedBlockFlags = ((codedBlockFlagsA & 0xf8U) << 16) | ((codedBlockFlagsB & 0x8fU) << 8);
		}

		// 9.3.3.1.1.9 Derivation process of ctxIdxInc for the syntax element coded_block_flag
		public uint GetCtxIdxInc(uint subBlkIdx)
		{
			return ((_codedBlockFlags >> CondTermFlagsA[subBlkIdx]) & 1) + // condTermFlagA
			       ((_codedBlockFlags >> CondTermFlagsB[subBlkIdx]) & 2);  // 2*condTermFlagB
		}

		public void SetCodedBlockFlag(uint subBlkIdx)
		{
			_codedBlockFlags |= CodedBlockFlagMasks[subBlkIdx];
		}
	}
}
