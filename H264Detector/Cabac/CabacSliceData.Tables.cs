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
	internal partial class CabacSliceData
	{
		#region Table 9-34 – Syntax elements and associated types of binarization, maxBinIdxCtx, and ctxIdxOffset
		// Table 9-40 – Assignment of ctxIdxBlockCatOffset to ctxBlockCat for syntax elements coded_block_flag,
		//              significant_coeff_flag, last_significant_coeff_flag, and coeff_abs_level_minus1
		//
		// (tables are combined to form a single table)
		private static readonly ushort[] CodedBlockFlagCtxBlockCatOffset =
		{
			85+0, 85+4, 85+8, 85+12, 85+16, 1012+0, 460+0, 460+4, 460+8, 1012+4, 472+0, 472+4, 472+8, 1012+8
		};
		private static readonly ushort[] SignificantCoeffFlagFrameCodedCtxBlockCatOffset =
		{
			105+0, 105+15, 105+29, 105+44, 105+47, 402+0, 484+0, 484+15, 484+29, 660+0, 528+0, 528+15, 528+29, 718+0
		};
		private static readonly ushort[] LastSignificantCoeffFlagFrameCodedCtxBlockCatOffset =
		{
			166+0, 166+15, 166+29, 166+44, 166+47, 417+0, 572+0, 572+15, 572+29, 690+0, 616+0, 616+15, 616+29, 748+0
		};
		private static readonly ushort[] CoeffAbsLevelMinus1CtxBlockCatOffset =
		{
			227+0, 227+10, 227+20, 227+30, 227+39, 426+0, 952+0, 952+10, 952+20, 708+0, 982+0, 982+10, 982+20, 766+0
		};
		#endregion Table 9-34 – Syntax elements and associated types of binarization, maxBinIdxCtx, and ctxIdxOffset

		#region Table 9-39 – Assignment of ctxIdxInc to binIdx for all ctxIdxOffset values except ...
		private static readonly uint[] MbTypeIntraCtxIdxIncIntraSlice = { 0/*0,1,2*/, 0/*ctxIdx=276*/, 3, 4, 5, 6, 7 };

		private static readonly uint[] MbTypeIntraCtxIdxIncInterSlice = { 0, 0/*ctxIdx=276*/, 1, 2, 2, 3, 3 };
		#endregion Table 9-39 – Assignment of ctxIdxInc to binIdx for all ctxIdxOffset values except ...

		#region Table 9-43 – Mapping of scanning position to ctxIdxInc for ctxBlockCat = = 5, 9, or 13
		// Frame-coded macroblocks
		private static readonly byte[] SignificantCoeffFlagCtxIdcInc0 =
		{
			 0,  1,  2,  3,  4,  5,  5,  4,
			 4,  3,  3,  4,  4,  4,  5,  5,
			 4,  4,  4,  4,  3,  3,  6,  7,
			 7,  7,  8,  9, 10,  9,  8,  7,
			 7,  6, 11, 12, 13, 11,  6,  7,
			 8,  9, 14, 10,  9,  8,  6, 11,
			12, 13, 11,  6,  9, 14, 10,  9,
			11, 12, 13, 11, 14, 10, 12
		};
		private static readonly byte[] LastSignificantCoeffFlagCtxIdcInc =
		{
			0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
			5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8
		};
		#endregion Table 9-43 – Mapping of scanning position to ctxIdxInc for ctxBlockCat = = 5, 9, or 13

		#region Table 9-42 – Specification of ctxBlockCat for the different blocks
		private static readonly byte[,] CtxBlockCatTabLuma =
		{
			{ 0, 1, 2, 5 }, { 6, 7, 8, 9 }, { 10, 11, 12, 13 }
		};
		#endregion Table 9-42 – Specification of ctxBlockCat for the different blocks
	}
}
