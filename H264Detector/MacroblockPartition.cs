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
	internal sealed class MacroblockPartition
	{
		public static readonly MacroblockPartition Mb16X16 = new MacroblockPartition(0, MbA(5), MbB(10), 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

		public static readonly MacroblockPartition Mb16X8SubBlk0 = new MacroblockPartition(1, MbA(5), MbB(10), 0, 1, 2, 3, 4, 5, 6, 7);
		public static readonly MacroblockPartition Mb16X8SubBlk1 = new MacroblockPartition(2, MbA(13), Cur(2), 8, 9, 10, 11, 12, 13, 14, 15);
	
		public static readonly MacroblockPartition Mb8X16SubBlk0 = new MacroblockPartition(3, MbA(5), MbB(10), 0, 1, 2, 3, 8, 9, 10, 11);
		public static readonly MacroblockPartition Mb8X16SubBlk1 = new MacroblockPartition(4, Cur(1), MbB(14), 4, 5, 6, 7, 12, 13, 14, 15);

		public static readonly MacroblockPartition Mb8X8SubBlk0 = new MacroblockPartition(5, MbA(5), MbB(10), 0, 1, 2, 3);
		public static readonly MacroblockPartition Mb8X8SubBlk1 = new MacroblockPartition(6, Cur(1), MbB(14), 4, 5, 6, 7);
		public static readonly MacroblockPartition Mb8X8SubBlk2 = new MacroblockPartition(7, MbA(13), Cur(2), 8, 9, 10, 11);
		public static readonly MacroblockPartition Mb8X8SubBlk3 = new MacroblockPartition(8, Cur(9), Cur(6), 12, 13, 14, 15);

		public static readonly MacroblockPartition Mb8X4SubBlk0 = new MacroblockPartition(9, MbA(5), MbB(10), 0, 1);
		public static readonly MacroblockPartition Mb8X4SubBlk1 = new MacroblockPartition(10, MbA(7), Cur(0),  2, 3);
		public static readonly MacroblockPartition Mb8X4SubBlk2 = new MacroblockPartition(11, Cur(1), MbB(14), 4, 5);
		public static readonly MacroblockPartition Mb8X4SubBlk3 = new MacroblockPartition(12, Cur(3), Cur(4), 6, 7);
		public static readonly MacroblockPartition Mb8X4SubBlk4 = new MacroblockPartition(13, MbA(13), Cur(2), 8, 9);
		public static readonly MacroblockPartition Mb8X4SubBlk5 = new MacroblockPartition(14, MbA(15), Cur(8), 10, 11);
		public static readonly MacroblockPartition Mb8X4SubBlk6 = new MacroblockPartition(15, Cur(9), Cur(6), 12, 13);
		public static readonly MacroblockPartition Mb8X4SubBlk7 = new MacroblockPartition(16, Cur(11), Cur(12), 14, 15);

		public static readonly MacroblockPartition Mb4X8SubBlk0 = new MacroblockPartition(17, MbA(5), MbB(10), 0, 2);
		public static readonly MacroblockPartition Mb4X8SubBlk1 = new MacroblockPartition(18, Cur(0), MbB(11), 1, 3);
		public static readonly MacroblockPartition Mb4X8SubBlk2 = new MacroblockPartition(19, Cur(1), MbB(14), 4, 6);
		public static readonly MacroblockPartition Mb4X8SubBlk3 = new MacroblockPartition(20, Cur(4), MbB(15), 5, 7);
		public static readonly MacroblockPartition Mb4X8SubBlk4 = new MacroblockPartition(21, MbA(13), Cur(2), 8, 10);
		public static readonly MacroblockPartition Mb4X8SubBlk5 = new MacroblockPartition(22, Cur(8), Cur(3), 9, 11);
		public static readonly MacroblockPartition Mb4X8SubBlk6 = new MacroblockPartition(23, Cur(9), Cur(6), 12, 14);
		public static readonly MacroblockPartition Mb4X8SubBlk7 = new MacroblockPartition(24, Cur(12), Cur(7), 13, 15);

		public static readonly MacroblockPartition Mb4X4SubBlk0 = new MacroblockPartition(25, MbA(5), MbB(10), 0);
		public static readonly MacroblockPartition Mb4X4SubBlk1 = new MacroblockPartition(26, Cur(0), MbB(11), 1);
		public static readonly MacroblockPartition Mb4X4SubBlk2 = new MacroblockPartition(27, MbA(7), Cur(0), 2);
		public static readonly MacroblockPartition Mb4X4SubBlk3 = new MacroblockPartition(28, Cur(2), Cur(1), 3);
		public static readonly MacroblockPartition Mb4X4SubBlk4 = new MacroblockPartition(29, Cur(1), MbB(14), 4);
		public static readonly MacroblockPartition Mb4X4SubBlk5 = new MacroblockPartition(30, Cur(4), MbB(15), 5);
		public static readonly MacroblockPartition Mb4X4SubBlk6 = new MacroblockPartition(31, Cur(3), Cur(4), 6);
		public static readonly MacroblockPartition Mb4X4SubBlk7 = new MacroblockPartition(32, Cur(6), Cur(5), 7);
		public static readonly MacroblockPartition Mb4X4SubBlk8 = new MacroblockPartition(33, MbA(13), Cur(2), 8);
		public static readonly MacroblockPartition Mb4X4SubBlk9 = new MacroblockPartition(34, Cur(8), Cur(3), 9);
		public static readonly MacroblockPartition Mb4X4SubBlk10 = new MacroblockPartition(35, MbA(15), Cur(8), 10);
		public static readonly MacroblockPartition Mb4X4SubBlk11 = new MacroblockPartition(36, Cur(10), Cur(9), 11);
		public static readonly MacroblockPartition Mb4X4SubBlk12 = new MacroblockPartition(37, Cur(9), Cur(6), 12);
		public static readonly MacroblockPartition Mb4X4SubBlk13 = new MacroblockPartition(38, Cur(12), Cur(7), 13);
		public static readonly MacroblockPartition Mb4X4SubBlk14 = new MacroblockPartition(39, Cur(11), Cur(12), 14);
		public static readonly MacroblockPartition Mb4X4SubBlk15 = new MacroblockPartition(40, Cur(14), Cur(13), 15);

		private static byte Cur(byte subBlkIdx)
		{
			return subBlkIdx;
		}

		private static byte MbA(byte subBlkIdx)
		{
			return (byte)(subBlkIdx + 16);
		}

		private static byte MbB(byte subBlkIdx)
		{
			return (byte)(subBlkIdx + 32);
		}

		#region Properties
		public uint Idx { get { return _idx; } }
		public uint SubMbPartIdxA { get { return _subMbPartIdxA; } }
		public uint SubMbPartIdxB { get { return _subMbPartIdxB; } }
		public byte[] MbPartSubBlks { get { return _mbPartSubBlks; } }
		#endregion Properties

		private readonly byte _idx;
		private readonly byte _subMbPartIdxA;
		private readonly byte _subMbPartIdxB;
		private readonly byte[] _mbPartSubBlks;

		private MacroblockPartition(byte idx, byte subMbPartIdxA, byte subMbPartIdxB, params byte[] bytes)
		{
			_idx = idx;
			_subMbPartIdxA = subMbPartIdxA;
			_subMbPartIdxB = subMbPartIdxB;
			_mbPartSubBlks = bytes;
		}

		//  0..15 = current
		// 16..31 = MbA
		// 32..47 = MbB
	}
}
