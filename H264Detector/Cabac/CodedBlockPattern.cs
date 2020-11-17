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
	internal struct CodedBlockPattern
	{
		private static readonly int[] CondTermFlagsA = { 1, 12, 3, 14, 4, 5 };
		private static readonly int[] CondTermFlagsB = { 7, 8, 11, 12, 9, 10 };
		private static readonly uint[] CodedBlockPatternMasks = { 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000 };

		// The bit position of the least significant bit of the coded block pattern for the current macroblock
		private const int BitPosition = 12;

		private const uint ChromaCodedBlockPatternMask = 0x30000;
		private const uint ChromaAcCodedBlockPattern   = 0x20000;
		private const uint LumaCodedBlockPatternMask   = 0x0f000;
		private const uint CodedBlockPatternMask       = ChromaCodedBlockPatternMask | LumaCodedBlockPatternMask;

		private uint _codedBlockPattern;

		#region Properties
		public bool IsResidualPresent { get { return (_codedBlockPattern & CodedBlockPatternMask) != 0; } }
		public bool IsLumaResidualPresent { get { return (_codedBlockPattern & LumaCodedBlockPatternMask) != 0; } }
		public bool IsChromaDcResidualPresent { get { return (_codedBlockPattern & ChromaCodedBlockPatternMask) != 0; } }
		public bool IsChromaAcResidualPresent { get { return (_codedBlockPattern & ChromaCodedBlockPatternMask) == ChromaAcCodedBlockPattern; } }

		public byte Bits { get { return (byte)((_codedBlockPattern & CodedBlockPatternMask) >> BitPosition); } }
		#endregion Properties

		public CodedBlockPattern(byte codedBlockPattern)
		{
			_codedBlockPattern = (uint)codedBlockPattern << BitPosition;
		}

		public CodedBlockPattern(byte codedBlockPatternA, byte codedBlockPatternB)
		{
			_codedBlockPattern = (uint)(codedBlockPatternA | (codedBlockPatternB << 6));
			_codedBlockPattern |= (_codedBlockPattern & 0x820) >> 1; // Copy the chroma AC residual present bit
		}

		// 9.3.3.1.1.9 Derivation process of ctxIdxInc for the syntax element coded_block_flag
		public  uint GetCtxIdxInc(uint binIdx)
		{
			return ((_codedBlockPattern >> CondTermFlagsA[binIdx]) & 1) + // condTermFlagA
			       ((_codedBlockPattern >> CondTermFlagsB[binIdx]) & 2);  // 2*condTermFlagB
		}

		public bool IsLumaBitSet(uint i8X8)
		{
			return (_codedBlockPattern & CodedBlockPatternMasks[i8X8]) != 0;
		}

		public void SetCodedBlockPatternBit(uint binIdx)
		{
			_codedBlockPattern |= CodedBlockPatternMasks[binIdx];
		}
	}
}
