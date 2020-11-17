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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264.Cavlc
{
	internal partial class CavlcSliceData : ISliceData
	{
		/// <summary>
		/// Maps <em>chroma_array_type</em> to <see cref="SubBlockPartition"/>.
		/// </summary>
		/// <see cref="ISequenceState.ChromaFormat"/>
		private static readonly SubBlockPartition[] ChromaArrayTypeSubBlockPartitions = new[]
		{
			SubBlockPartition.Luma,
			SubBlockPartition.Chroma420,
			SubBlockPartition.Chroma422,
			SubBlockPartition.Luma
		};

		private const int Cb = 0; // chroma index for Cb
		private const int Cr = 1; // chroma index for Cr

		private readonly INalUnitReader _reader;
		private readonly IState _readerState;
		private readonly ISliceState _sliceState;
		private readonly IPictureState _pictureState;
		private readonly ISequenceState _sequenceState;
		private readonly MbToSliceGroupMapdecoder _mbToSliceGroup = new MbToSliceGroupMapdecoder();
		private readonly VlcTable<CoeffToken> _coeffTokenChromaDc;
		private readonly CodedCoefficients _lumaCodedCoefficients;
		private readonly CodedCoefficients[] _chromaCodedCoefficients; // Cb, Cr
		private readonly ISubMacroblockType[] _subMbTypes;
		private uint _currMbAddr;
		private uint _codedBlockPattern;

		#region Properties
#if DEBUG
		private long CurrentMbIndex { get { return ((_currMbAddr / PicWidthInMbs) * (PicWidthInMbs + 1)) + (_currMbAddr % PicWidthInMbs); } }
#endif
		private ChromaFormat ChromaFormat { get { return _sequenceState.ChromaFormat; } }
		private IMbToSliceGroupMap MbToSliceGroupMap { get; set; }
		private bool MbFieldDecodingFlag { get; set; }
		private bool MbAffFrameFlag { get { return _sliceState.MbAffFrameFlag; } }
		private uint PicWidthInMbs { get { return _sequenceState.PicWidthInMbs; } }
		private uint PicHeightInMbs { get { return _sequenceState.FrameHeightInMbs / (_sliceState.FieldPicFlag ? 2U : 1U); } }
		private uint PicSizeInMbs { get { return (PicWidthInMbs * PicHeightInMbs); } }
		#endregion Properties

		public CavlcSliceData(INalUnitReader reader, IState readerState)
		{
			_reader = reader;
			_readerState = readerState;
			_sliceState = reader.State.SliceState;
			_pictureState = _sliceState.PictureState;
			_sequenceState = _pictureState.SequenceState;
			_coeffTokenChromaDc = (ChromaFormat == ChromaFormat.YCbCr420) ? CoeffTokenChromaDc420 : CoeffTokenChromaDc422;
			_lumaCodedCoefficients = new CodedCoefficients(_sliceState, PicHeightInMbs, SubBlockPartition.Luma);
			_chromaCodedCoefficients = new CodedCoefficients[2];
			_subMbTypes = new ISubMacroblockType[4];

			SubBlockPartition chromaSubBlockPartition = ChromaArrayTypeSubBlockPartitions[_sequenceState.ChromaFormat.ChromaFormatIdc];
			for (int i = 0; i < _chromaCodedCoefficients.Length; i++)
			{
				_chromaCodedCoefficients[i] = new CodedCoefficients(_sliceState, PicHeightInMbs, chromaSubBlockPartition);
			}

			MbToSliceGroupMap = _mbToSliceGroup.CreateMacroBlockToSliceGroupMap(_sliceState);
		}

		public void Parse()
		{
			_currMbAddr = _sliceState.FirstMacroBlockInSlice * (MbAffFrameFlag ? 2U : 1U);
			MbFieldDecodingFlag = _sliceState.FieldPicFlag; // The default for non-MBAFF frames

			bool moreDataFlag = true;
			bool prevMbSkipped = false;
			while (_readerState.Valid && moreDataFlag)
			{
				if (!_sliceState.IntraCoded)
				{
					uint mbSkipRun = _reader.GetExpGolombCoded();
					prevMbSkipped = (mbSkipRun > 0);

					if (mbSkipRun > 0)
					{
						for (uint i = 0; i < (mbSkipRun - 1); i++)
						{
							UpdateSkippedMacroblockPrediction();
							NextMbAddress();

							if (!_readerState.Valid)
							{
								return; //Could not find expected macroblock
							}
						}

						moreDataFlag = HasMoreRbspData();
						if (moreDataFlag)
						{
							// Skip the last block
							UpdateSkippedMacroblockPrediction();
							NextMbAddress();

							if (!_readerState.Valid)
							{
								return; //Could not find expected macroblock
							}
						}
					}
				}
				if (moreDataFlag)
				{
					if (MbAffFrameFlag && (IsFirstMacroblockInPair() || prevMbSkipped))
					{
						MbFieldDecodingFlag = _reader.GetBit(); // is field macro block
					}

					MacroBlockLayer();

					moreDataFlag = HasMoreRbspData();
				}
				if (moreDataFlag)
				{
					NextMbAddress();
				}
			}
		}

		private bool IsFirstMacroblockInPair()
		{
			return (_currMbAddr % 2) == 0;
		}

		private void NextMbAddress()
		{
			_currMbAddr = MbToSliceGroupMap.NextMbAddr(_currMbAddr);

			if (_currMbAddr >= PicSizeInMbs)
			{
				_readerState.Invalidate(); // Unexpected end-of-slice
			}
		}

		private bool HasMoreRbspData()
		{
			if ((_currMbAddr == (PicSizeInMbs - 1)) && _reader.IsPossibleStopBit())
			{
				return false;
			}

			return _reader.HasMoreRbspData();
		}

		private void MacroBlockLayer()
		{
			//page 76
			IMacroblockType macroBlockType = GetMacroBlockType();
#if DEBUG
			H264Debug.WriteLine("{0}: mb_type={1}", CurrentMbIndex, macroBlockType);
			H264Debug.WriteLine("ShowBits(32) = {0:x8} at {1:x8}", _reader.ShowBits(32), _reader.Position);
#endif
			if (macroBlockType.IsIPcm)
			{
				_reader.GetPcmSamples();

				_lumaCodedCoefficients.UpdateTotalCoeff((int)_currMbAddr, 16);
				_chromaCodedCoefficients[Cb].UpdateTotalCoeff((int)_currMbAddr, 16);
				_chromaCodedCoefficients[Cr].UpdateTotalCoeff((int)_currMbAddr, 16);
			}
			else
			{
				bool transform8X8ModeFlagSet = false;
				if (macroBlockType.NumMbPart == 4)
				{
					// Note: Only inter-coded blocks can have mb parts, in particular,
					//       we are not currently decoding an intra-coded block!
					SubMacroBlockPrediction(macroBlockType);

					if (HasSubMbPartSizeLessThan8X8())
					{
						transform8X8ModeFlagSet = true; // Transform 8x8 mode is not possible!
					}
				}
				else
				{
					bool transformSize8X8Flag = false;
					if (_pictureState.Transform8X8ModeFlag && macroBlockType.IsI_NxN)
					{
						transformSize8X8Flag = _reader.GetBit();
						transform8X8ModeFlagSet = true;
					}

					MacroBlockPrediction(macroBlockType, transformSize8X8Flag);
				}

				if (macroBlockType.IsIntra16X16)
				{
					_codedBlockPattern = macroBlockType.CodedBlockPattern;
				}
				else
				{
					GetCodedBlockPattern(macroBlockType.IntraCoded);

					if (_pictureState.Transform8X8ModeFlag && !transform8X8ModeFlagSet &&
						((_codedBlockPattern & 15)/*luma*/ != 0) &&
						(!macroBlockType.IsDirect || _sequenceState.Direct8X8InferenceFlag))
					{
						_reader.GetBit(); // transform_size_8x8_flag
					}
				}

				if ((_codedBlockPattern != 0) || macroBlockType.IsIntra16X16)
				{
					_reader.GetSignedExpGolombCoded(_sequenceState.MinMbQpDelta, _sequenceState.MaxMbQpDelta); // mb_qp_delta
					Residual(macroBlockType, 0, 15);
				}
				else
				{
					UpdateSkippedMacroblockPrediction();
				}
			}
		}

		private bool HasSubMbPartSizeLessThan8X8()
		{
			for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
			{
				var subMbType = _subMbTypes[mbPartIdx];
				if (!subMbType.IsDirect)
				{
					if (subMbType.NumSubMbPart > 1)
					{
						return true;
					}
				}
				else if (!_sequenceState.Direct8X8InferenceFlag)
				{
					return true;
				}
			}
			return false;
		}

		private void Residual(IMacroblockType macroBlockType, int startIdx, int endIdx)
		{
			ResidualLuma(macroBlockType, startIdx, endIdx, _lumaCodedCoefficients);

			if (_sequenceState.IsChromaSubsampling) // Chroma format 4:2:0 or 4:2:2
			{
				uint numSubBlocks = (4 * ChromaFormat.NumC8X8); // in 4x4 blocks
				if (IsChromaDcResidualPresent() && (startIdx == 0))
				{
					for (uint iCbCr = 0; iCbCr < 2; iCbCr++)
					{
						var coeffToken = GetChromaCoeffToken();
						if (coeffToken.TotalCoeff != 0)
						{
							ResidualBlockCavlc(0, (int)(numSubBlocks - 1), (int)numSubBlocks, coeffToken);
						}
					}
				}
				if (IsChromaAcResidualPresent())
				{
					for (uint iCbCr = 0; iCbCr < 2; iCbCr++)
					{
						CodedCoefficients codedCoefficients = _chromaCodedCoefficients[iCbCr];
						for (uint i = 0; i < numSubBlocks; i++)
						{
							var coeffToken = GetCoeffToken(codedCoefficients, i);
							if (coeffToken.TotalCoeff != 0)
							{
								ResidualBlockCavlc(Math.Max(0, (startIdx - 1)), (endIdx - 1), 15, coeffToken);
							}
						}
					}
				}
				else
				{
					// Clear non-zero coefficient predictions for non-coded blocks
					_chromaCodedCoefficients[Cb].UpdateTotalCoeff((int)_currMbAddr, 0);
					_chromaCodedCoefficients[Cr].UpdateTotalCoeff((int)_currMbAddr, 0);
				}
			}
			else if (ChromaFormat == ChromaFormat.YCbCr444)
			{
				ResidualLuma(macroBlockType, startIdx, endIdx, _chromaCodedCoefficients[0]); // Cb
				ResidualLuma(macroBlockType, startIdx, endIdx, _chromaCodedCoefficients[1]); // Cr
			}
		}

		private bool IsChromaDcResidualPresent()
		{
			return (_codedBlockPattern & 0x30) != 0;
		}

		private bool IsChromaAcResidualPresent()
		{
			return (_codedBlockPattern & 0x20) != 0;
		}

		private bool IsLumaBitSet(uint i8X8)
		{
			return (_codedBlockPattern & (1U << (int)i8X8)) != 0;
		}

		private CoeffToken GetCoeffToken(CodedCoefficients codedCoefficients, uint subBlockIndex)
		{
			var nC = codedCoefficients.PredictTotalCoeff(_currMbAddr, subBlockIndex);
			var coeffToken = _reader.GetVlc(LookupCoeffTokenVlcTable(nC));
			if (coeffToken.TotalCoeff == 255/*invalid*/)
			{
				_readerState.Invalidate();
				return new CoeffToken(0, 0);
			}

			codedCoefficients.UpdateTotalCoeff((int)_currMbAddr, subBlockIndex, coeffToken.TotalCoeff);
			return coeffToken;
		}

		private CoeffToken GetChromaCoeffToken()
		{
			return _reader.GetVlc(_coeffTokenChromaDc);
		}

		private static VlcTable<CoeffToken> LookupCoeffTokenVlcTable(int nC)
		{
			if (nC < 2)
			{
				return CoeffTokenN0;
			}
			if (nC < 4)
			{
				return CoeffTokenN2;
			}
			if (nC < 8)
			{
				return CoeffTokenN4;
			}

			return CoeffTokenN8;
		}

		private void ResidualLuma(IMacroblockType macroBlockType, int startIdx, int endIdx, CodedCoefficients codedCoefficients)
		{
			if ((startIdx == 0) && macroBlockType.IsIntra16X16)
			{
				var coeffTokenDc = GetCoeffToken(codedCoefficients, 0);
#if DEBUG
				H264Debug.WriteLine("   coeff_token( luma dc )={0}", coeffTokenDc);
#endif
				if (coeffTokenDc.TotalCoeff != 0)
				{
					ResidualBlockCavlc(0, 15, 16, coeffTokenDc);
				}
			}
			for (uint i8X8 = 0; i8X8 < 4; i8X8++)
			{
				if (IsLumaBitSet(i8X8))
				{
					for (uint i4X4 = 0; i4X4 < 4; i4X4++)
					{
						uint subBlkIdx = ((i8X8 << 2) + i4X4);
						var coeffToken = GetCoeffToken(codedCoefficients, subBlkIdx);
#if DEBUG
						H264Debug.WriteLine("   coeff_token={0}", coeffToken);
#endif
						if (coeffToken.TotalCoeff != 0)
						{
							if (macroBlockType.IsIntra16X16)
							{
								ResidualBlockCavlc(Math.Max(0, (startIdx - 1)), (endIdx - 1), 15, coeffToken);
							}
							else
							{
								ResidualBlockCavlc(startIdx, endIdx, 16, coeffToken);
							}
						}
					}
				}
				else
				{
					// Clear non-zero coefficient predictions for non-coded blocks
					for (uint i4X4 = 0; i4X4 < 4; i4X4++)
					{
						codedCoefficients.UpdateTotalCoeff((int)_currMbAddr, (4 * i8X8) + i4X4, 0);
					}
				}
			}
		}

		private void ResidualBlockCavlc(int startIdx, int endIdx, int maxNumCoeff, CoeffToken coeffToken)
		{
			int suffixLength = ((coeffToken.TotalCoeff > 10) && (coeffToken.TrailingOnes < 3)) ? 1 : 0;
#if DEBUG
			H264Debug.Write("   - levels=");
			if (coeffToken.TrailingOnes > 0)
			{
				uint signs = _reader.GetBits(coeffToken.TrailingOnes); // (n) x trailing_ones_sign_flag
				for (int i = 0; i < coeffToken.TrailingOnes; i++)
				{
					H264Debug.Write((((signs & (1 << (coeffToken.TrailingOnes - 1 - i))) == 0) ? "1," : "-1,"));
				}
			}
#else
			_reader.GetBits(coeffToken.TrailingOnes); // (n) x trailing_ones_sign_flag
#endif

			for (int i = coeffToken.TrailingOnes; i < coeffToken.TotalCoeff; i++)
			{
				int levelCode = GetCoefficientLevelCode(suffixLength);

				// The first non-zero coefficient after 'trailing ones' must be larger than 1,
				// unless the maximum count of trailing ones, i.e. 3, was reached.
				// This will further reduce the amount of bits required to represent a
				// coefficient level.
				if ((i == coeffToken.TrailingOnes) && (i/*trailing ones*/ < 3))
				{
					levelCode += 2; // 2 instead of 1, because the lowest bit is the sign bit!
				}
#if DEBUG
				H264Debug.Write("{0},", ((levelCode % 2) == 0) ? ((levelCode + 2) >> 1) : ((-levelCode - 1) >> 1));
#endif

				// Automatically adjust the suffix length if high coefficients levels are
				// encountered, to reduce the number of bits required to represent subsequent
				// high coefficient levels.
				if (suffixLength == 0)
				{
					suffixLength = 1;
				}
				if (suffixLength < 6)
				{
					int absLevelMinusOne = (levelCode >> 1);
					if (absLevelMinusOne >= (3 << (suffixLength - 1)))
					{
						suffixLength++;
					}
				}
			}
#if DEBUG
			H264Debug.WriteLine();
#endif

			if (coeffToken.TotalCoeff < (endIdx - startIdx + 1))
			{
				int zerosLeft = GetTotalZeros(coeffToken.TotalCoeff, maxNumCoeff); // total_zeros
#if DEBUG
				H264Debug.WriteLine("   - total_zeros={0}", zerosLeft);
				H264Debug.WriteLine("ShowBits(32) = {0:x8} at {1:x8}", _reader.ShowBits(32), _reader.Position);
				H264Debug.Write("   - run_before=");
#endif
				for (int i = 0; i < (coeffToken.TotalCoeff - 1) && (zerosLeft > 0); i++)
				{
#if DEBUG
					int runBefore = _reader.GetVlc(RunBeforeTable[Math.Min(zerosLeft, 7)]);
					zerosLeft -= runBefore; // run_before
					H264Debug.Write("{0},", runBefore);
#else
					zerosLeft -= _reader.GetVlc(RunBeforeTable[Math.Min(zerosLeft, 7)]); // run_before
#endif
				}
#if DEBUG
				H264Debug.WriteLine();
#endif
			}
		}

		private int GetCoefficientLevelCode(int suffixLength)
		{
			int levelPrefix = _reader.GetLeadingZeroBits(); // level_prefix
			int levelCode = (levelPrefix << suffixLength);

			if ((suffixLength == 0) && (levelPrefix < 14))
			{
				return levelCode; // level_suffix is (0)
			}

			int levelSuffixSize;
			if (levelPrefix >= 15)
			{
				// Escape-coded (high) coefficient level
				levelSuffixSize = (levelPrefix - 3);

				levelCode = 15 << Math.Max(1, suffixLength);
				levelCode += (1 << levelSuffixSize) - 4096;
			}
			else if ((levelPrefix == 14) && (suffixLength == 0))
			{
				levelSuffixSize = 4;
			}
			else
			{
				levelSuffixSize = suffixLength;
			}

			var levelSuffix = (int)_reader.GetBits(levelSuffixSize); // level_suffix
			return (levelCode + levelSuffix);
		}

		private int GetTotalZeros(uint totalCoeff, int maxCoeff)
		{
			if (maxCoeff <= 4)
			{
				return _reader.GetVlc(TotalZerosTable2X2[totalCoeff + 4 - maxCoeff]);
			}
			if (maxCoeff <= 8)
			{
				return _reader.GetVlc(TotalZerosTable2X4[totalCoeff + 8 - maxCoeff]);
			}
			if (maxCoeff < 15)
			{
				return _reader.GetVlc(TotalZerosTable4X4[totalCoeff + 16 - maxCoeff]);
			}

			return _reader.GetVlc(TotalZerosTable4X4[totalCoeff]);
		}

		private IMacroblockType GetMacroBlockType()
		{
			//Table 7-10,7-11,..
			switch (_sliceState.SliceType)
			{
				case SliceType.I:
					uint mbTypeI = _reader.GetExpGolombCoded(25);
					return MacroblockType.GetMbTypeI(mbTypeI);

				case SliceType.Si:
					uint mbTypeSi = _reader.GetExpGolombCoded(26);
					return MacroblockType.GetMbTypeSi(mbTypeSi);

				case SliceType.P:
				case SliceType.Sp:
					uint mbTypeP = _reader.GetExpGolombCoded(30);
					return MacroblockType.GetMbTypeP(mbTypeP);

				case SliceType.B:
					uint mbTypeB = _reader.GetExpGolombCoded(48);
					return MacroblockType.GetMbTypeB(mbTypeB);
			}

			throw new InvalidOperationException("Current type not set");
		}

		private void GetCodedBlockPattern(bool intraCoded)
		{
			//section 9.1.2: me(v) : Mapped Exp Colomb Coded
			if (_sequenceState.IsChromaSubsampling)
			{
				uint codeNum42X = _reader.GetExpGolombCoded(47); //CodedBlockPattern
				_codedBlockPattern = intraCoded ? IntraCodedBlockPattern42X[codeNum42X] : InterCodedBlockPattern42X[codeNum42X];
			}
			else
			{
				uint codeNum444 = _reader.GetExpGolombCoded(15); //CodedBlockPattern
				_codedBlockPattern = intraCoded ? IntraCodedBlockPattern444[codeNum444] : InterCodedBlockPattern444[codeNum444];
			}
		}

		// 7.3.5.1 Macroblock prediction syntax
		private void MacroBlockPrediction(IMacroblockType macroBlockType, bool transformSize8X8Flag)
		{
			if (macroBlockType.IsIntra4X4 || macroBlockType.IsIntra16X16)
			{
				if (macroBlockType.IsIntra4X4)
				{
					uint lumaSubBlocks = transformSize8X8Flag ? 4U : 16U;
					for (uint i = 0; i < lumaSubBlocks; i++)
					{
						if (!_reader.GetBit()) // prev_intra4x4_pred_mode_flag[luma4x4BlkIdx])
						{
							_reader.GetBits(3); // rem_intra4x4_pred_mode[luma4x4BlkIdx]
						}
					}
				}
				if (_sequenceState.IsChromaSubsampling)
				{
					_reader.GetExpGolombCoded(); // intra_chroma_pred_mode
				}
			}
			else if (!macroBlockType.IsDirect)
			{
				uint numMbPart = macroBlockType.NumMbPart; // either 1 or 2

				uint referencePictureCount0 = _sliceState.ActiveReferencePictureCount0;//defaults to _pictureState.DefaultReferencePictureCount0;
				if (HasReferencePictureIndex(referencePictureCount0))
				{
					for (uint i = 0; i < numMbPart; i++)
					{
						if (macroBlockType.IsList0Predicted(i))
						{
							GetReferencePicture(referencePictureCount0); // ref_idx_l0[ mbPartIdx ]
						}
					}
				}

				uint referencePictureCount1 = _sliceState.ActiveReferencePictureCount1;//defaults to _pictureState.DefaultReferencePictureCount1;
				if (HasReferencePictureIndex(referencePictureCount1))
				{
					for (uint i = 0; i < numMbPart; i++)
					{
						if (macroBlockType.IsList1Predicted(i))
						{
							GetReferencePicture(referencePictureCount1); // ref_idx_l1[ mbPartIdx ]
						}
					}
				}

				for (uint i = 0; i < numMbPart; i++)
				{
					if (macroBlockType.IsList0Predicted(i))
					{
						GetMotionVector(); // mvd_l0[ i ][ 0 ][ compIdx ]
					}
				}
				for (uint i = 0; i < numMbPart; i++)
				{
					if (macroBlockType.IsList1Predicted(i))
					{
						GetMotionVector(); // mvd_l1[ i ][ 0 ][ compIdx ]
					}
				}
			}
		}

		private bool HasReferencePictureIndex(uint maximumListIndex)
		{
			return (maximumListIndex > 1) || (MbFieldDecodingFlag != _sliceState.FieldPicFlag);
		}

		private void GetReferencePicture(uint referencePictureCount)
		{
			if (MbAffFrameFlag && MbFieldDecodingFlag)
			{
				_reader.GetTruncatedExpGolombCoded((2 * referencePictureCount) - 1);
			}
			else
			{
				_reader.GetTruncatedExpGolombCoded(referencePictureCount - 1);
			}
		}

		private void GetMotionVector()
		{
#if DEBUG
			int dx = _reader.GetSignedExpGolombCoded(); // dx coordinate?
			int dy = _reader.GetSignedExpGolombCoded(); // dy coordinate?
			H264Debug.WriteLine("   dmv={0},{1}", dx, dy);
#else
			_reader.GetSignedExpGolombCoded(); // dmv(x)
			_reader.GetSignedExpGolombCoded(); // dmv(y)
#endif
		}

		private void SubMacroBlockPrediction(IMacroblockType macroBlockType)
		{
			if (_sliceState.SliceType == SliceType.B)
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					uint subMbType = _reader.GetExpGolombCoded(12); // sub_mb_type[mbPartIdx]
					_subMbTypes[mbPartIdx] = SubMacroblockType.GetSubMbTypeB(subMbType);
				}
			}
			else // P or Sp
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					uint subMbType = _reader.GetExpGolombCoded(3); // sub_mb_type[mbPartIdx]
					_subMbTypes[mbPartIdx] = SubMacroblockType.GetSubMbTypeP(subMbType);
				}
			}

			uint referencePictureCount0 = _sliceState.ActiveReferencePictureCount0;//defaults to _pictureState.DefaultReferencePictureCount0;
			if (HasReferencePictureIndex(referencePictureCount0) && !macroBlockType.IsP8X8Ref0)
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					if (_subMbTypes[mbPartIdx].List0Predicted)
					{
						GetReferencePicture(referencePictureCount0);
					}
				}
			}

			uint referencePictureCount1 = _sliceState.ActiveReferencePictureCount1;//defaults to _pictureState.DefaultReferencePictureCount1;
			if (HasReferencePictureIndex(referencePictureCount1))
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					if (_subMbTypes[mbPartIdx].List1Predicted)
					{
						GetReferencePicture(referencePictureCount1);
					}
				}
			}

			for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
			{
				ISubMacroblockType subMbType = _subMbTypes[mbPartIdx];
				if (subMbType.List0Predicted)
				{
					for (uint subMbPartIdx = 0; subMbPartIdx < subMbType.NumSubMbPart; subMbPartIdx++)
					{
						GetMotionVector(); // mvd_l0[ i ][ j ][ compIdx ]
					}
				}
			}
			for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
			{
				ISubMacroblockType subMbType = _subMbTypes[mbPartIdx];
				if (subMbType.List1Predicted)
				{
					for (uint subMbPartIdx = 0; subMbPartIdx < subMbType.NumSubMbPart; subMbPartIdx++)
					{
						GetMotionVector(); // mvd_l1[ i ][ j ][ compIdx ]
					}
				}
			}
		}

		private void UpdateSkippedMacroblockPrediction()
		{
			_lumaCodedCoefficients.UpdateTotalCoeff((int)_currMbAddr, 0);
			_chromaCodedCoefficients[Cb].UpdateTotalCoeff((int)_currMbAddr, 0);
			_chromaCodedCoefficients[Cr].UpdateTotalCoeff((int)_currMbAddr, 0);
		}
	}
}
