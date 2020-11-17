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

using System.Diagnostics;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264.Cavlc
{
	internal sealed class CodedCoefficients
	{
		private const byte TotalCoeffUnavailable = 64;

		private static readonly int[] TotalCoeffLookup = CreateTotalCoeffLookup();

		#region Lookup table initialization
		private static int[] CreateTotalCoeffLookup()
		{
			var totalCoeffLookup = new int[128 + 1];

			// Both blkA and blkB are available: average nA and nB
			for (int i = 0; i <= 32; i++)
			{
				totalCoeffLookup[i] = (i + 1) >> 1;
			}

			// Either blkA or blkB is available, but not both
			for (int i = 0; i <= 16; i++)
			{
				totalCoeffLookup[i + TotalCoeffUnavailable] = i;
			}

			// blkA and blkB are both unavailable
			totalCoeffLookup[TotalCoeffUnavailable + TotalCoeffUnavailable] = 0;
			return totalCoeffLookup;
		}
		#endregion Lookup table initialization

		private readonly ISliceState _sliceState;
		private readonly SubBlockPartition _subBlockPartition;
		private readonly byte[,] _totalCoeffs;

		public CodedCoefficients(ISliceState sliceState, uint picHeightInMbs, SubBlockPartition subBlockPartition)
		{
			_sliceState = sliceState;
			_subBlockPartition = subBlockPartition;

			uint macroBlockCount = (picHeightInMbs * sliceState.PictureState.SequenceState.PicWidthInMbs);
			int subBlockCount = _subBlockPartition.SubBlockCount;
			_totalCoeffs = new byte[macroBlockCount, subBlockCount];

			// Clear the total (non-zero) coeffs array
			for (int i = 0; i < macroBlockCount; i++)
			{
				for (int j = 0; j < subBlockCount; j++)
				{
					_totalCoeffs[i, j] = TotalCoeffUnavailable;
				}
			}
		}

		internal void UpdateTotalCoeff(int mbIndex, int totalCoeff)
		{
			Debug.Assert((totalCoeff <= 16), "Number of coded coefficients cannot be greater than 16");
			for (uint subBlockIndex = 0; subBlockIndex < _subBlockPartition.SubBlockCount; subBlockIndex++)
			{
				_totalCoeffs[mbIndex, subBlockIndex] = (byte)totalCoeff;
			}
		}

		internal void UpdateTotalCoeff(int mbIndex, uint subBlockIndex, int totalCoeff)
		{
			Debug.Assert((totalCoeff <= 16), "Number of coded coefficients cannot be greater than 16");
			_totalCoeffs[mbIndex, subBlockIndex] = (byte)totalCoeff;
		}

		internal int PredictTotalCoeff(uint mbIndex, uint subBlockIndex)
		{
			int nTop = GetTopNumberCoeff(mbIndex, subBlockIndex);
			int nLeft = GetLeftNumberCoeff(mbIndex, subBlockIndex);
#if DEBUG
			H264Debug.WriteLine("   * (left={0}, top={1}, nzc={2})", nLeft, nTop, TotalCoeffLookup[nLeft + nTop]);
#endif
			return TotalCoeffLookup[nLeft + nTop];
		}

		/// <summary>
		/// Retrieves the TotalCoeff of the subblock left of the provided one (one column left)
		/// </summary>
		private int GetLeftNumberCoeff(uint mbIndex, uint subBlockIndex)
		{
			if (!_subBlockPartition.IsLeftMostSubBlock(subBlockIndex))
			{
				return _totalCoeffs[mbIndex, _subBlockPartition.GetSubBlockIndexLeft(subBlockIndex)];
			}

			uint mbAddrA = _sliceState.GetMbAddrA(mbIndex);
			if (mbAddrA == uint.MaxValue)
			{
				return TotalCoeffUnavailable; // 'TotalCoeff' information not available!
			}

			return _totalCoeffs[mbAddrA, _subBlockPartition.GetSubBlockIndexLeft(subBlockIndex)];
		}

		// Retrieves the TotalCoeff of the subblock above the provided one (one row up)
		private int GetTopNumberCoeff(uint mbIndex, uint subBlockIndex)
		{
			if (!_subBlockPartition.IsTopMostSubBlock(subBlockIndex))
			{
				return _totalCoeffs[mbIndex, _subBlockPartition.GetSubBlockIndexAbove(subBlockIndex)];
			}

			uint mbAddrB = _sliceState.GetMbAddrB(mbIndex);
			if (mbAddrB == uint.MaxValue)
			{
				return TotalCoeffUnavailable; // 'TotalCoeff' information not available!
			}

			return _totalCoeffs[mbAddrB, _subBlockPartition.GetSubBlockIndexAbove(subBlockIndex)];
		}
	}
}
