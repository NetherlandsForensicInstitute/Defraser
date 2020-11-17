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
using System.Collections.Generic;

namespace Defraser.Detector.H264
{
	internal sealed class MacroblockPartitioning
	{
		public static readonly MacroblockPartitioning M16X16 = new MacroblockPartitioning(16, 16,
			MacroblockPartition.Mb16X16
		);
		public static readonly MacroblockPartitioning M16X8 = new MacroblockPartitioning(16, 8,
			MacroblockPartition.Mb16X8SubBlk0, MacroblockPartition.Mb16X8SubBlk1
		);
		public static readonly MacroblockPartitioning M8X16 = new MacroblockPartitioning(8, 16,
			MacroblockPartition.Mb8X16SubBlk0, MacroblockPartition.Mb8X16SubBlk1
		);
		public static readonly MacroblockPartitioning M8X8 = new MacroblockPartitioning(8, 8,
			MacroblockPartition.Mb8X8SubBlk0, MacroblockPartition.Mb8X8SubBlk1,
			MacroblockPartition.Mb8X8SubBlk2, MacroblockPartition.Mb8X8SubBlk3
		);
		public static readonly MacroblockPartitioning M8X4 = new MacroblockPartitioning(8, 4,
			MacroblockPartition.Mb8X4SubBlk0, MacroblockPartition.Mb8X4SubBlk1,
			MacroblockPartition.Mb8X4SubBlk2, MacroblockPartition.Mb8X4SubBlk3,
			MacroblockPartition.Mb8X4SubBlk4, MacroblockPartition.Mb8X4SubBlk5,
			MacroblockPartition.Mb8X4SubBlk6, MacroblockPartition.Mb8X4SubBlk7
		);
		public static readonly MacroblockPartitioning M4X8 = new MacroblockPartitioning(4, 8,
			MacroblockPartition.Mb4X8SubBlk0, MacroblockPartition.Mb4X8SubBlk1,
			MacroblockPartition.Mb4X8SubBlk2, MacroblockPartition.Mb4X8SubBlk3,
			MacroblockPartition.Mb4X8SubBlk4, MacroblockPartition.Mb4X8SubBlk5,
			MacroblockPartition.Mb4X8SubBlk6, MacroblockPartition.Mb4X8SubBlk7
		);
		public static readonly MacroblockPartitioning M4X4 = new MacroblockPartitioning(4, 4,
			MacroblockPartition.Mb4X4SubBlk0, MacroblockPartition.Mb4X4SubBlk1,
			MacroblockPartition.Mb4X4SubBlk2, MacroblockPartition.Mb4X4SubBlk3,
			MacroblockPartition.Mb4X4SubBlk4, MacroblockPartition.Mb4X4SubBlk5,
			MacroblockPartition.Mb4X4SubBlk6, MacroblockPartition.Mb4X4SubBlk7,
			MacroblockPartition.Mb4X4SubBlk8, MacroblockPartition.Mb4X4SubBlk9,
			MacroblockPartition.Mb4X4SubBlk10, MacroblockPartition.Mb4X4SubBlk11,
			MacroblockPartition.Mb4X4SubBlk12, MacroblockPartition.Mb4X4SubBlk13,
			MacroblockPartition.Mb4X4SubBlk14, MacroblockPartition.Mb4X4SubBlk15
		);

		private static readonly ICollection<MacroblockPartitioning> Partitionings;

		static MacroblockPartitioning()
		{
			Partitionings = new List<MacroblockPartitioning> { M16X16, M16X8, M8X16, M8X8, M8X4, M4X8, M4X4 };
		}

		#region Properties
		public MacroblockPartition this[uint mbPartIdx] { get { return _macroblockPartitions[mbPartIdx]; } }
		public uint MbPartWidth { get { return _mbPartPredWidth; } }
		public uint MbPartHeight { get { return _mbPartPredHeight; } }
		#endregion Properties

		private readonly byte _mbPartPredWidth;
		private readonly byte _mbPartPredHeight;
		private readonly MacroblockPartition[] _macroblockPartitions;

		private MacroblockPartitioning(byte mbPartPredWidth, byte mbPartPredHeight, params MacroblockPartition[] partitions)
		{
			_mbPartPredWidth = mbPartPredWidth;
			_mbPartPredHeight = mbPartPredHeight;
			_macroblockPartitions = partitions;
		}

		public static MacroblockPartitioning GetMacroblockPartitioning(uint mbPartWidth, uint mbPartHeight)
		{
			foreach (MacroblockPartitioning macroblockPartitioning in Partitionings)
			{
				if ((macroblockPartitioning.MbPartWidth == mbPartWidth) && (macroblockPartitioning.MbPartHeight == mbPartHeight))
				{
					return macroblockPartitioning;
				}
			}

			throw new ArgumentException(String.Format("No partitioning defined for {0} x {1}", mbPartWidth, mbPartHeight), "mbPartWidth/mbPartHeight");
		}
	}
}
