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

namespace Defraser.Detector.H264.Cavlc
{
	internal sealed class SubBlockPartition
	{
		private static readonly uint[] SubBlockIndexAbove = new uint[] { 10, 11, 0, 1, 14, 15, 4, 5, 2, 3, 8, 9, 6, 7, 12, 13 };
		private static readonly uint[] SubBlockIndexLeft = new uint[] { 5, 0, 7, 2, 1, 4, 3, 6, 13, 8, 15, 10, 9, 12, 11, 14 };

		public static readonly SubBlockPartition Luma = new SubBlockPartition(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
		public static readonly SubBlockPartition Chroma422 = new SubBlockPartition(new uint[] { 0, 1, 2, 3, 8, 9, 10, 11 });
		public static readonly SubBlockPartition Chroma420 = new SubBlockPartition(new uint[] { 0, 1, 2, 3 });

		private const uint SubBlockUnavailable = uint.MaxValue;

		private readonly int _subBlockCount;
		private readonly uint[] _subBlockIndex;
		private readonly uint[] _subBlockIndexAbove;
		private readonly uint[] _subBlockIndexLeft;

		#region Properties
		public int SubBlockCount { get { return _subBlockCount; } }
		#endregion Properties

		private SubBlockPartition(params uint[] subBlockIndex)
		{
			_subBlockCount = subBlockIndex.Length;
			_subBlockIndex = new uint[subBlockIndex.Length];
			Array.Copy(subBlockIndex, _subBlockIndex, SubBlockCount);

			_subBlockIndexAbove = new uint[SubBlockCount];
			_subBlockIndexLeft = new uint[SubBlockCount];

			// Initialize inverse raster scan lookup table
			uint[] inverseRasterScan = new uint[16];
			for (int i = 0; i < 16; i++)
			{
				inverseRasterScan[i] = SubBlockUnavailable;
			}
			for (uint i = 0; i < SubBlockCount; i++)
			{
				inverseRasterScan[subBlockIndex[i]] = i;
			}

			// Initialize sub-block index above/left lookup tables
			for (uint i = 0; i < SubBlockCount; i++)
			{
				_subBlockIndexAbove[i] = FindSubBlockIndexAbove(inverseRasterScan, subBlockIndex[i]);
				_subBlockIndexLeft[i] = FindSubBlockIndexLeft(inverseRasterScan, subBlockIndex[i]);
			}
		}

		private static uint FindSubBlockIndexAbove(uint[] inverseRasterScan, uint subBlockIndex)
		{
			uint idx = SubBlockIndexAbove[subBlockIndex];
			while (inverseRasterScan[idx] == SubBlockUnavailable)
			{
				idx = SubBlockIndexAbove[idx];
			}
			return inverseRasterScan[idx];
		}

		private static uint FindSubBlockIndexLeft(uint[] inverseRasterScan, uint subBlockIndex)
		{
			uint idx = SubBlockIndexLeft[subBlockIndex];
			while (inverseRasterScan[idx] == SubBlockUnavailable)
			{
				idx = SubBlockIndexLeft[idx];
			}
			return inverseRasterScan[idx];
		}

		/// <summary>
		/// Returns the index of the subblock above the subblock at <paramref name="subBlockIndex"/>.
		/// </summary>
		/// <param name="subBlockIndex">the index of the reference subblock</param>
		/// <returns>the sub-block index</returns>
		public uint GetSubBlockIndexAbove(uint subBlockIndex)
		{
			return _subBlockIndexAbove[subBlockIndex];
		}

		public uint GetSubBlockIndexLeft(uint subBlockIndex)
		{
			return _subBlockIndexLeft[subBlockIndex];
		}

		/// <summary>
		/// Returns whether 'subBlockIndex' indicates a subblock in the leftmost
		/// column of the macroblock.
		/// </summary>
		/// <param name="subBlockIndex">the index of the reference subblock</param>
		/// <returns>whether <paramref name="subBlockIndex"/> is the left-most subblock</returns>
		public bool IsLeftMostSubBlock(uint subBlockIndex)
		{
			return (_subBlockIndex[subBlockIndex] & 5) == 0;
		}

		/// <summary>
		/// Returns whether 'subBlockIndex' indicates a subblock in the top row
		/// of the macroblock.
		/// </summary>
		/// <param name="subBlockIndex">the index of the reference subblock</param>
		/// <returns>whether <paramref name="subBlockIndex"/> is the top-most subblock</returns>
		public bool IsTopMostSubBlock(uint subBlockIndex)
		{
			return (_subBlockIndex[subBlockIndex] & ~5) == 0;
		}
	}
}
