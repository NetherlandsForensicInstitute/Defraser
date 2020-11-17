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
// ReSharper disable RedundantUsingDirective
using System.Text;
// ReSharper restore RedundantUsingDirective
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264.Cabac
{
	internal sealed partial class CabacSliceData : ISliceData
	{
		private readonly INalUnitReader _reader;
		private readonly IState _readerState;
		private readonly ISliceState _sliceState;
		private readonly IPictureState _pictureState;
		private readonly ISequenceState _sequenceState;
		private readonly MacroblockState[] _macroblockStates;
		private readonly MotionField _motionFieldL0;
		private readonly MotionField _motionFieldL1;
		private readonly ISubMacroblockType[] _subMbTypes;
		private readonly ArithmeticDecoder _arithmeticDecoder;
		private readonly MbToSliceGroupMapdecoder _mbToSliceGroup = new MbToSliceGroupMapdecoder();
		private readonly MacroblockState _unavailableMacroblockState;
		private uint _currMbAddr;
		private MacroblockState _mbStateA; // state of macroblock above the current macroblock
		private MacroblockState _mbStateB; // state of macroblock to the left of the current macroblock
		private MacroblockState _currMbState;
		private CodedBlockPattern _codedBlockPattern;
		private MacroblockFlags _mbFlags;
		private byte _mbQpDeltaNotZero;
		private byte _prevMbQpDeltaNotZero;

		#region Properties
#if DEBUG
		private long CurrentMbIndex { get { return ((_currMbAddr / PicWidthInMbs) * (PicWidthInMbs + 1)) + (_currMbAddr % PicWidthInMbs); } }
#endif
		private ChromaFormat ChromaFormat { get { return _sequenceState.ChromaFormat; } }
		private IMbToSliceGroupMap MbToSliceGroupMap { get; set; }
		private bool MbFieldDecodingFlag { get; set; }
		private bool MbAffFrameFlag { get { return _sliceState.MbAffFrameFlag; } }
		private int PicWidthInMbs { get { return (int) _sequenceState.PicWidthInMbs; } }
		private int PicHeightInMbs { get { return (int)(_sequenceState.FrameHeightInMbs / (_sliceState.FieldPicFlag ? 2 : 1)); } }
		private int PicSizeInMbs { get { return (PicWidthInMbs * PicHeightInMbs); } }
		private bool IsList0Predicted { get { return !_sliceState.IntraCoded; } }
		private bool IsList1Predicted { get { return (_sliceState.SliceType == SliceType.B); } }
		#endregion Properties

		public CabacSliceData(INalUnitReader reader, IState readerState)
		{
			_reader = reader;
			_readerState = readerState;
			_sliceState = reader.State.SliceState;
			_pictureState = _sliceState.PictureState;
			_sequenceState = _pictureState.SequenceState;
			_unavailableMacroblockState = _sliceState.IntraCoded
			                              	? MacroblockState.UnavailableIntraCoded
			                              	: MacroblockState.UnavailableInterCoded;
			_macroblockStates = new MacroblockState[PicSizeInMbs];
			_motionFieldL0 = IsList0Predicted ? new MotionField(_sliceState) : null;
			_motionFieldL1 = IsList1Predicted ? new MotionField(_sliceState) : null;
			_subMbTypes = new ISubMacroblockType[4];
			_arithmeticDecoder = new ArithmeticDecoder(reader);

			for (int i = 0; i < _macroblockStates.Length; i++)
			{
				_macroblockStates[i] = _unavailableMacroblockState;
			}

			MbToSliceGroupMap = _mbToSliceGroup.CreateMacroBlockToSliceGroupMap(_sliceState);
		}

		public void Parse()
		{
			InitializeCabac();

			_currMbAddr = _sliceState.FirstMacroBlockInSlice * (MbAffFrameFlag ? 2U : 1U);
			MbFieldDecodingFlag = _sliceState.FieldPicFlag; // The default for non-MBAFF frames

			bool moreDataFlag = true;
			bool prevMbSkipped = false;
			while (_readerState.Valid && moreDataFlag)
			{
				_currMbState = new MacroblockState();
				_mbQpDeltaNotZero = 0;

				// Load macroblock states for block A (left) and B (top)
				_mbStateA = GetMacroblockState(_sliceState.GetMbAddrA(_currMbAddr));
				_mbStateB = GetMacroblockState(_sliceState.GetMbAddrB(_currMbAddr));
				_mbFlags = new MacroblockFlags(_mbStateA.Flags, _mbStateB.Flags);

				bool mbSkipFlag = false;
				if (!_sliceState.IntraCoded)
				{
					mbSkipFlag = GetMbSkipFlag(); // mb_skip_flag
				}
				if (!mbSkipFlag)
				{
					_mbFlags.SetCodedBlockFlag();

					if (MbAffFrameFlag && (IsFirstMacroblockInPair() || prevMbSkipped))
					{
						MbFieldDecodingFlag = GetMbFieldDecodingFlag(); // is field macro block
					}

					MacroBlockLayer();
				}
				if (MbFieldDecodingFlag && (!MbAffFrameFlag || IsLastMacroblockInPair()))
				{
					_mbFlags.SetFieldDecodingMode();
				}

				_currMbState.Flags = _mbFlags.Bits;
				_macroblockStates[_currMbAddr] = _currMbState;
				_prevMbQpDeltaNotZero = _mbQpDeltaNotZero;

				if (!_sliceState.IntraCoded)
				{
					prevMbSkipped = mbSkipFlag;
				}
				if (!MbAffFrameFlag || IsLastMacroblockInPair())
				{
					moreDataFlag = !GetEndOfSliceFlag();
				}
				if (moreDataFlag)
				{
					NextMbAddress();
				}
			}
#if DEBUG
			H264Debug.WriteLine(" - Terminated ({4}) at {0} of {1} ({2}), nextBits(16)={3}",
								_reader.Position, _reader.Length, _currMbAddr, ToBitString(_reader.ShowBits(16), 16),
								_sliceState.SliceType.ToString());
#endif
		}

		private bool IsFirstMacroblockInPair()
		{
			return (_currMbAddr%2) == 0;
		}

		private bool IsLastMacroblockInPair()
		{
			return (_currMbAddr%2) == 1;
		}

		private void NextMbAddress()
		{
			_currMbAddr = MbToSliceGroupMap.NextMbAddr(_currMbAddr);

			if (_currMbAddr >= PicSizeInMbs)
			{
				_readerState.Invalidate(); // Unexpected end-of-slice
			}
		}

		private void InitializeCabac()
		{
			_reader.GetCabacAlignmentOneBits();

			if (_readerState.Valid)
			{
				if (_reader.ShowBits(8) == 0xff)
				{
					// Note: IOffset was 510 or 511, which is forbidden!
					_readerState.Invalidate();
				}
				else
				{
					_arithmeticDecoder.Initialize();
				}
			}
		}

		private MacroblockState GetMacroblockState(uint mbAddr)
		{
			return (mbAddr == uint.MaxValue) ? _unavailableMacroblockState : _macroblockStates[mbAddr];
		}

#if DEBUG
		private static string ToBitString(uint value, uint numBits)
		{
			var sb = new StringBuilder();
			for (uint i=0; i < numBits; i++)
			{
				uint bit = (value >> (int)(numBits - i - 1)) & 1;
				sb.Append(bit);
			}
			return sb.ToString();
		}
#endif

		private bool GetMbSkipFlag()
		{
			// ctxIdxOffset is 11 for P/SP slices, 24 for B slices
			uint ctxIdxOffset = (_sliceState.SliceType == SliceType.B) ? 24U : 11U;
			return _arithmeticDecoder.DecodeDecision(ctxIdxOffset + _mbFlags.GetMbSkipFlagCtxIdxInc()) == 1;
		}

		private bool GetMbFieldDecodingFlag()
		{
			return (_arithmeticDecoder.DecodeDecision(70 + _mbFlags.GetMbFieldDecodingFlagCtxIdxInc()) == 1);
		}

		private bool GetEndOfSliceFlag()
		{
			return (_arithmeticDecoder.DecodeTerminate() == 1);
		}

		private void MacroBlockLayer()
		{
			IMacroblockType macroBlockType = GetMacroblockType();
#if DEBUG
			H264Debug.WriteLine("{0}: mb_type={1}", CurrentMbIndex, macroBlockType);
#endif
			if (macroBlockType.IsIPcm)
			{
				_reader.GetPcmSamples();

				InitializeCabac();

				_currMbState = MacroblockState.Pcm;
			}
			else
			{
				bool transform8X8ModeFlagSet = false;
				if (macroBlockType.NumMbPart == 4)
				{
					// Note: Only inter-coded blocks can have mb parts, in particular,
					//       we are not currently decoding an intra-coded block!
					SubMacroBlockPrediction();

					if (HasSubMbPartSizeLessThan8X8())
					{
						transform8X8ModeFlagSet = true; // Transform 8x8 mode is not possible!
					}
				}
				else
				{
					if (_pictureState.Transform8X8ModeFlag && macroBlockType.IsI_NxN)
					{
						GetTransformSize8X8Flag();

						transform8X8ModeFlagSet = true;
					}

					MacroBlockPrediction(macroBlockType);
				}

				if (macroBlockType.IsIntra16X16)
				{
					_codedBlockPattern = new CodedBlockPattern(macroBlockType.CodedBlockPattern);
				}
				else
				{
					GetCodedBlockPattern();

					if (_pictureState.Transform8X8ModeFlag && !transform8X8ModeFlagSet &&
					    _codedBlockPattern.IsLumaResidualPresent &&
					    (!macroBlockType.IsDirect || _sequenceState.Direct8X8InferenceFlag))
					{
						GetTransformSize8X8Flag();
					}
				}

				_currMbState.CodedBlockPattern = _codedBlockPattern.Bits;

				if (_codedBlockPattern.IsResidualPresent || macroBlockType.IsIntra16X16)
				{
					GetMbQpDelta(); // mb_qp_delta
					Residual(macroBlockType, 0, 15);
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

		private IMacroblockType GetMacroblockType()
		{
			switch (_sliceState.SliceType)
			{
				case SliceType.I:
					return GetMbTypeI();
				case SliceType.Si:
					return GetMbTypeSi();
				case SliceType.P:
				case SliceType.Sp:
					return GetMbTypeP();
				case SliceType.B:
					return GetMbTypeB();
			}

			throw new InvalidOperationException();
		}

		private IMacroblockType GetMbTypeI()
		{
			const uint ctxOffset = 3;
			if (_arithmeticDecoder.DecodeDecision(ctxOffset + _mbFlags.GetMbTypeCtxIdxInc()) == 0)
			{
				return MacroblockType.GetMbTypeI(0);
			}

			_mbFlags.SetMbTypeNotZero();

			return GetMbTypeIntra(ctxOffset, MbTypeIntraCtxIdxIncIntraSlice);
		}

		private IMacroblockType GetMbTypeSi()
		{
			const uint ctxOffsetPrefix = 0;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + _mbFlags.GetMbTypeCtxIdxInc()) == 0)
			{
				return MacroblockType.GetMbTypeSi(0);
			}

			_mbFlags.SetMbTypeNotZero();

			const uint ctxOffsetSuffix = 3;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetSuffix) == 0)
			{
				return MacroblockType.GetMbTypeI(0);
			}

			return GetMbTypeIntra(ctxOffsetSuffix, MbTypeIntraCtxIdxIncIntraSlice);
		}

		private IMacroblockType GetMbTypeP()
		{
			// Note: The 'MbTypeNotZero' flag is not used in P-slices!

			// Table 9-37 – Binarization for macroblock types in P, SP, and B slices
			const uint ctxOffsetPrefix = 14;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix) == 0)
			{
				// 9.3.3.1.2 Assignment process of ctxIdxInc using prior decoded bin values
				uint b1 = _arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 1);
				uint b2 = _arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + (b1 + 2));
				return MacroblockType.GetMbTypeP((b1 << 1) ^ (3*b2));
			}

			const uint ctxOffsetSuffix = 17;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetSuffix) == 0)
			{
				return MacroblockType.GetMbTypeI(0);
			}

			return GetMbTypeIntra(ctxOffsetSuffix, MbTypeIntraCtxIdxIncInterSlice);
		}

		private IMacroblockType GetMbTypeB()
		{
			const uint ctxOffsetPrefix = 27;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + _mbFlags.GetMbTypeCtxIdxInc()) == 0)
			{
				return MacroblockType.GetMbTypeB(0);
			}

			_mbFlags.SetMbTypeNotZero();

			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 3) == 0)
			{
				return MacroblockType.GetMbTypeB(1U + _arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5));
			}
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 4) == 0)
			{
				return MacroblockType.GetMbTypeB(3U + GetFixedLength((ctxOffsetPrefix + 5), 3));
			}
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5) == 0)
			{
				return MacroblockType.GetMbTypeB(12U + GetFixedLength((ctxOffsetPrefix + 5), 3));
			}
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5) == 1)
			{
				return MacroblockType.GetMbTypeB(11U + (11U*_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5)));
			}
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5) == 0)
			{
				return MacroblockType.GetMbTypeB(20U + _arithmeticDecoder.DecodeDecision(ctxOffsetPrefix + 5));
			}

			const uint ctxOffsetSuffix = 32;
			if (_arithmeticDecoder.DecodeDecision(ctxOffsetSuffix) == 0)
			{
				return MacroblockType.GetMbTypeI(0);
			}

			return GetMbTypeIntra(ctxOffsetSuffix, MbTypeIntraCtxIdxIncInterSlice);
		}

		private IMacroblockType GetMbTypeIntra(uint ctxOffset, uint[] binIdxCtxIdxInc)
		{
			if (_arithmeticDecoder.DecodeTerminate() == 1)
			{
				return MacroblockType.GetMbTypeI(25); // I_PCM
			}

			// First decision determines 'CodedBlockPatternLuma' (0 for '0', 1 for '15')
			uint mbType = (12U * _arithmeticDecoder.DecodeDecision(ctxOffset + binIdxCtxIdxInc[2]));

			// Next 1-2 bits determine 'CodedBlockPatternChroma' (0, 1 or 2)
			if (_arithmeticDecoder.DecodeDecision(ctxOffset + binIdxCtxIdxInc[3]) == 1)
			{
				mbType += (1 + (uint)_arithmeticDecoder.DecodeDecision(ctxOffset + binIdxCtxIdxInc[4])) << 2;
			}

			// Final 2 bits determine the 'intra_chroma_pred_mode'
			mbType |= (uint)_arithmeticDecoder.DecodeDecision(ctxOffset + binIdxCtxIdxInc[5]) << 1;
			mbType |= _arithmeticDecoder.DecodeDecision(ctxOffset + binIdxCtxIdxInc[6]);
			return MacroblockType.GetMbTypeI(mbType + 1);
		}

		private void GetTransformSize8X8Flag()
		{
			if (_arithmeticDecoder.DecodeDecision(399 + _mbFlags.GetTransformSize8X8FlagCtxIdxInc()) == 1)
			{
				_mbFlags.SetTransformSize8X8Flag();
			}
		}

		// 7.3.5.1 Macroblock prediction syntax
		private void MacroBlockPrediction(IMacroblockType macroBlockType)
		{
			if (macroBlockType.IsIntra4X4 || macroBlockType.IsIntra16X16)
			{
				if (macroBlockType.IsIntra4X4)
				{
					int lumaSubBlocks = _mbFlags.TransformSize8X8Flag ? 4 : 16;
					for (int i = 0; i < lumaSubBlocks; i++)
					{
						if (GetPrevIntra4X4PredModeFlag() == 0) // prev_intra4x4_pred_mode_flag[luma4x4BlkIdx])
						{
							GetRemIntra4X4PredMode(); // rem_intra4x4_pred_mode[luma4x4BlkIdx]
						}
					}
				}
				if (_sequenceState.IsChromaSubsampling)
				{
					GetIntraChromaPredMode(); // intra_chroma_pred_mode
				}
			}
			else if (!macroBlockType.IsDirect)
			{
				uint numMbPart = macroBlockType.NumMbPart; // either 1 or 2
				MacroblockPartitioning macroblockPartitioning = macroBlockType.MacroblockPartitioning;

				uint referencePictureCount0 = _sliceState.ActiveReferencePictureCount0;//defaults to _pictureState.DefaultReferencePictureCount0;
				if (HasReferencePictureIndex(referencePictureCount0))
				{
					var referencePictureIndex = new ReferencePictureIndex(_mbStateA.RefIdxL0, _mbStateB.RefIdxL0);
					for (uint i = 0; i < numMbPart; i++)
					{
						if (macroBlockType.IsList0Predicted(i))
						{
							uint idx = macroblockPartitioning[i].Idx;
							if (GetReferencePicture(referencePictureCount0, referencePictureIndex.GetCtxIdxInc(idx))) // ref_idx_l0[ mbPartIdx ]
							{
								referencePictureIndex.SetNonZeroRefIdx(idx);
							}
						}
					}
					_currMbState.RefIdxL0 = referencePictureIndex.Bits;
				}

				uint referencePictureCount1 = _sliceState.ActiveReferencePictureCount1;//defaults to _pictureState.DefaultReferencePictureCount1;
				if (HasReferencePictureIndex(referencePictureCount1))
				{
					var referencePictureIndex = new ReferencePictureIndex(_mbStateA.RefIdxL1, _mbStateB.RefIdxL1);
					for (uint i = 0; i < numMbPart; i++)
					{
						if (macroBlockType.IsList1Predicted(i))
						{
							uint idx = macroblockPartitioning[i].Idx;
							if (GetReferencePicture(referencePictureCount1, referencePictureIndex.GetCtxIdxInc(idx))) // ref_idx_l1[ mbPartIdx ]
							{
								referencePictureIndex.SetNonZeroRefIdx(idx);
							}
						}
					}
					_currMbState.RefIdxL1 = referencePictureIndex.Bits;
				}

				for (uint i = 0; i < numMbPart; i++)
				{
					if (macroBlockType.IsList0Predicted(i))
					{
						GetMotionVector(_motionFieldL0, macroblockPartitioning[i]); // mvd_l0[ i ][ 0 ][ compIdx ]
					}
				}
				for (uint i = 0; i < numMbPart; i++)
				{
					if (macroBlockType.IsList1Predicted(i))
					{
						GetMotionVector(_motionFieldL1, macroblockPartitioning[i]); // mvd_l1[ i ][ 0 ][ compIdx ]
					}
				}
			}
		}

		private void SubMacroBlockPrediction()
		{
			if (_sliceState.SliceType == SliceType.B)
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					uint subMbType = GetSubMbTypeB();
					_subMbTypes[mbPartIdx] = SubMacroblockType.GetSubMbTypeB(subMbType);
				}
			}
			else // P or Sp
			{
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					uint subMbType = GetSubMbTypeP();
					_subMbTypes[mbPartIdx] = SubMacroblockType.GetSubMbTypeP(subMbType);
				}
			}

			uint referencePictureCount0 = _sliceState.ActiveReferencePictureCount0;//defaults to _pictureState.DefaultReferencePictureCount0;
			if (HasReferencePictureIndex(referencePictureCount0))
			{
				var referencePictureIndex = new ReferencePictureIndex(_mbStateA.RefIdxL0, _mbStateB.RefIdxL0);
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					if (_subMbTypes[mbPartIdx].List0Predicted)
					{
						uint idx = MacroblockPartitioning.M8X8[mbPartIdx].Idx;
						if (GetReferencePicture(referencePictureCount0, referencePictureIndex.GetCtxIdxInc(idx))) // ref_idx_l0[ mbPartIdx ]
						{
							referencePictureIndex.SetNonZeroRefIdx(idx);
						}
					}
				}
				_currMbState.RefIdxL0 = referencePictureIndex.Bits;
			}

			uint referencePictureCount1 = _sliceState.ActiveReferencePictureCount1;//defaults to _pictureState.DefaultReferencePictureCount1;
			if (HasReferencePictureIndex(referencePictureCount1))
			{
				var referencePictureIndex = new ReferencePictureIndex(_mbStateA.RefIdxL1, _mbStateB.RefIdxL1);
				for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
				{
					if (_subMbTypes[mbPartIdx].List1Predicted)
					{
						uint idx = MacroblockPartitioning.M8X8[mbPartIdx].Idx;
						if (GetReferencePicture(referencePictureCount1, referencePictureIndex.GetCtxIdxInc(idx))) // ref_idx_l0[ mbPartIdx ]
						{
							referencePictureIndex.SetNonZeroRefIdx(idx);
						}
					}
				}
				_currMbState.RefIdxL1 = referencePictureIndex.Bits;
			}

			for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
			{
				ISubMacroblockType subMbType = _subMbTypes[mbPartIdx];
				if (subMbType.List0Predicted)
				{
					MacroblockPartitioning macroblockPartitioning = subMbType.MacroblockPartitioning;
					uint subMbPartOffset = (mbPartIdx * subMbType.NumSubMbPart);
					for (uint subMbPartIdx = 0; subMbPartIdx < subMbType.NumSubMbPart; subMbPartIdx++)
					{
						GetMotionVector(_motionFieldL0, macroblockPartitioning[subMbPartOffset + subMbPartIdx]); // mvd_l0[ i ][ j ][ compIdx ]
					}
				}
			}
			for (uint mbPartIdx = 0; mbPartIdx < 4; mbPartIdx++)
			{
				ISubMacroblockType subMbType = _subMbTypes[mbPartIdx];
				if (subMbType.List1Predicted)
				{
					MacroblockPartitioning macroblockPartitioning = subMbType.MacroblockPartitioning;
					uint subMbPartOffset = (mbPartIdx * subMbType.NumSubMbPart);
					for (uint subMbPartIdx = 0; subMbPartIdx < subMbType.NumSubMbPart; subMbPartIdx++)
					{
						GetMotionVector(_motionFieldL1, macroblockPartitioning[subMbPartOffset + subMbPartIdx]); // mvd_l1[ i ][ j ][ compIdx ]
					}
				}
			}
		}

		private uint GetSubMbTypeP()
		{
			const int ctxIdxOffset = 21;
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset) == 1)
			{
				return 0; // P_L0_8x8
			}

			// 9.3.3.1.2 Assignment process of ctxIdxInc using prior decoded bin values
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + 1) == 0)
			{
				return 1; // P_L0_8x4
			}

			return 3U ^ _arithmeticDecoder.DecodeDecision(ctxIdxOffset + 2); // P_L0_4x8 (1) or P_L0_4x4 (0)
		}

		private uint GetSubMbTypeB()
		{
			const int ctxIdxOffset = 36; // upto 3 (0, 1, 2|3, 3, 3, 3)
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset) == 0)
			{
				return 0; // B_Direct_8x8
			}

			// 9.3.3.1.2 Assignment process of ctxIdxInc using prior decoded bin values
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + 1) == 0)
			{
				return 1U + _arithmeticDecoder.DecodeDecision(ctxIdxOffset + 3); // B_L0_8x8 (1) or B_L1_8x8 (2)
			}
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + 2) == 0)
			{
				return 3U + GetFixedLength((ctxIdxOffset + 3), 2);
			}
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + 3) == 0)
			{
				return 7U + GetFixedLength((ctxIdxOffset + 3), 2);
			}

			return 11U + _arithmeticDecoder.DecodeDecision(ctxIdxOffset + 3); // B_L1_4x4 (11) or B_Bi_4x4 (12)
		}

		// 9.3.2.2 Truncated unary (TU) binarization process
		private uint GetTruncatedUnary(uint ctxIdx, uint cMax)
		{
			for (uint synElVal = 0; synElVal < cMax; synElVal++)
			{
				if (_arithmeticDecoder.DecodeDecision(ctxIdx) == 0)
				{
					return synElVal;
				}
			}
			return cMax;
		}

		// 9.3.2.4 Fixed-length (FL) binarization process
		private uint GetFixedLength(uint ctxIdx, uint fixedLength)
		{
			uint value = 0;
			for (uint i = 0; i < fixedLength; i++)
			{
				value = (value << 1) | _arithmeticDecoder.DecodeDecision(ctxIdx);
			}
			return value;
		}

		private bool HasReferencePictureIndex(uint maximumListIndex)
		{
			return (maximumListIndex > 1) || (MbFieldDecodingFlag != _sliceState.FieldPicFlag);
		}

		private bool GetReferencePicture(uint numRefPic, uint binIdx0CtxIdxInc)
		{
			const uint ctxIdxOffset = 54;
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + binIdx0CtxIdxInc) == 0)
			{
				return false;
			}

			uint cMax = (MbAffFrameFlag && MbFieldDecodingFlag) ? ((2 * numRefPic) - 1) : (numRefPic - 1);
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + 4) == 0)
			{
				if (cMax == 0)
				{
					_readerState.Invalidate();
				}
				return true;
			}

			// Note: cMax is larger than the maximum allowed value to detect errors
			if ((GetTruncatedUnary(ctxIdxOffset + 5, cMax) + 2) > cMax)
			{
				_readerState.Invalidate();
			}

			return true;
		}

		private void GetMotionVector(MotionField motionField, MacroblockPartition mbPart)
		{
#if DEBUG
			int dmvx = 0, dmvy = 0;
#endif
			if (_arithmeticDecoder.DecodeDecision(40 + motionField.GetCtxIdxInc(_currMbAddr, mbPart, 0)) == 1)
			{
				uint dx = GetAbsMotionVectorComponent(40); // dx coordinate?
				motionField.UpdateComponent(_currMbAddr, mbPart, 0, dx);
#if DEBUG
				dmvx = (_arithmeticDecoder.DecodeBypass() == 1) ? -(int)dx : (int)dx; // sign?
#else
				_arithmeticDecoder.DecodeBypass(); // sign
#endif
			}
			if (_arithmeticDecoder.DecodeDecision(47 + motionField.GetCtxIdxInc(_currMbAddr, mbPart, 1)) == 1)
			{
				uint dy = GetAbsMotionVectorComponent(47); // dy coordinate?
				motionField.UpdateComponent(_currMbAddr, mbPart, 1, dy);
#if DEBUG
				dmvy = (_arithmeticDecoder.DecodeBypass() == 1) ? -(int)dy : (int)dy; // sign?
#else
				_arithmeticDecoder.DecodeBypass(); // sign
#endif
			}
#if DEBUG
			H264Debug.WriteLine("   dmv={0},{1}", dmvx, dmvy);
#endif
		}

		private uint GetAbsMotionVectorComponent(uint ctxIdxOffset)
		{
			for (uint prefix = 1; prefix < 9; prefix++)
			{
				uint ctxIdxInc = Math.Min(6, (prefix + 2));
				if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + ctxIdxInc) == 0)
				{
					return prefix;
				}
			}

			uint value = GetExpGolomb();

			for (int i = 0; i < 3; i++)
			{
				value = (value << 1) | _arithmeticDecoder.DecodeBypass();
			}
			return (value + 9);
		}

		private uint GetPrevIntra4X4PredModeFlag()
		{
			return _arithmeticDecoder.DecodeDecision(68);
		}

		private void GetRemIntra4X4PredMode()
		{
			for (int binIdx = 0; binIdx < 3; binIdx++)
			{
				_arithmeticDecoder.DecodeDecision(69);
			}
		}

		private void GetIntraChromaPredMode()
		{
			if (_arithmeticDecoder.DecodeDecision(64 + _mbFlags.GetIntraChromaPredModeCtxIdxInc()) != 0)
			{
				_mbFlags.SetIntraChromaPredModeNotZero();

				if (_arithmeticDecoder.DecodeDecision(67) != 0)
				{
					_arithmeticDecoder.DecodeDecision(67);
				}
			}
		}

		private void GetCodedBlockPattern()
		{
			_codedBlockPattern = new CodedBlockPattern(_mbStateA.CodedBlockPattern, _mbStateB.CodedBlockPattern);

			for (uint binIdx = 0; binIdx < 4; binIdx++)
			{
				uint ctxIdx = 73 + (_codedBlockPattern.GetCtxIdxInc(binIdx) ^ 3);
				if (_arithmeticDecoder.DecodeDecision(ctxIdx) == 1)
				{
					_codedBlockPattern.SetCodedBlockPatternBit(binIdx);
				}
			}
			if (_sequenceState.IsChromaSubsampling)
			{
				uint ctxIdxBinIdx4 = 77 + _codedBlockPattern.GetCtxIdxInc(4);
				if (_arithmeticDecoder.DecodeDecision(ctxIdxBinIdx4) == 1)
				{
					uint ctxIdxBinIdx5 = 77 + (4 + _codedBlockPattern.GetCtxIdxInc(5));
					_codedBlockPattern.SetCodedBlockPatternBit(4U + _arithmeticDecoder.DecodeDecision(ctxIdxBinIdx5));
				}
			}
		}

		private void GetMbQpDelta()
		{
			// 9.3.3.1.1.5 Derivation process of ctxIdxInc for the syntax element mb_qp_delta
			uint ctxIdxBinIdx0 = 60U + _prevMbQpDeltaNotZero;
			_mbQpDeltaNotZero = _arithmeticDecoder.DecodeDecision(ctxIdxBinIdx0);

			if (_mbQpDeltaNotZero != 0)
			{
				if (_arithmeticDecoder.DecodeDecision(62) != 0)
				{
					// Parsing mb_qp_delta '> 1' or  '<= -1'; cMax is larger than the maximum allowed value (86)
					int codeNum = (int)GetTruncatedUnary(63, 87) + 2;
					// Table 9-3 – Assignment of syntax element to codeNum for signed Exp-Golomb coded syntax elements se(v)
					int mbQpDelta = -((codeNum >> 1) ^ -(codeNum & 1));

					// 7.4.5 Macroblock layer semantics (note on 'mb_qp_delta')
					if ((mbQpDelta < _sequenceState.MinMbQpDelta) || (mbQpDelta > _sequenceState.MaxMbQpDelta))
					{
						_readerState.Invalidate();
					}
				}
			}
		}

		private void Residual(IMacroblockType macroBlockType, int startIdx, int endIdx)
		{
			// Special case for unavailable macroblock states of intra coded blocks in inter-coded slices
			bool b1 = macroBlockType.IntraCoded && !_sliceState.IntraCoded;
			bool b2 = _mbFlags.TransformSize8X8Flag && (ChromaFormat == ChromaFormat.YCbCr444);
			if (b1 || b2)
			{
				byte unavailableCodedBlockFlags = (byte)(macroBlockType.IntraCoded ? 0xff : 0);
				if ((b1 && ((_mbStateA.Flags & 0x40) == 0x40)) || (b2 && ((_mbStateA.Flags & 0x4) == 0)))
				{
					_mbStateA.LumaCodedBlockFlags = unavailableCodedBlockFlags;
					_mbStateA.CrCodedBlockFlags = unavailableCodedBlockFlags;
					_mbStateA.CbCodedBlockFlags = unavailableCodedBlockFlags;
				}
				if ((b1 && ((_mbStateB.Flags & 0x40) == 0x40)) || (b2 && ((_mbStateB.Flags & 0x4) == 0)))
				{
					_mbStateB.LumaCodedBlockFlags = unavailableCodedBlockFlags;
					_mbStateB.CrCodedBlockFlags = unavailableCodedBlockFlags;
					_mbStateB.CbCodedBlockFlags = unavailableCodedBlockFlags;
				}
			}

			var codedBlockFlags = new CodedBlockFlags(_mbStateA.LumaCodedBlockFlags, _mbStateB.LumaCodedBlockFlags);
			ResidualLuma(macroBlockType, startIdx, endIdx, ref codedBlockFlags, 0);

			_currMbState.LumaCodedBlockFlags = codedBlockFlags.Flags;

			if (_sequenceState.IsChromaSubsampling) // Chroma format 4:2:0 or 4:2:2
			{
				if (_codedBlockPattern.IsChromaDcResidualPresent)
				{
// ReSharper disable SuggestUseVarKeywordEvident
					int numSubBlocks = (int)(4 * ChromaFormat.NumC8X8); // in 4x4 blocks
// ReSharper restore SuggestUseVarKeywordEvident
					var cbCodedBlockFlags = new CodedBlockFlags(_mbStateA.CbCodedBlockFlags, _mbStateB.CbCodedBlockFlags);
					var crCodedBlockFlags = new CodedBlockFlags(_mbStateA.CrCodedBlockFlags, _mbStateB.CrCodedBlockFlags);

					if (GetCodedBlockFlag(3, ref cbCodedBlockFlags, 32))
					{
						cbCodedBlockFlags.SetCodedBlockFlag(32);

						ResidualBlockCabac(0, (numSubBlocks - 1), numSubBlocks, 3);
					}
					if (GetCodedBlockFlag(3, ref crCodedBlockFlags, 32))
					{
						crCodedBlockFlags.SetCodedBlockFlag(32);

						ResidualBlockCabac(0, (numSubBlocks - 1), numSubBlocks, 3);
					}
					if (_codedBlockPattern.IsChromaAcResidualPresent)
					{
						uint cbfIndex = 16U + (uint) numSubBlocks;
						for (uint i = 0; i < numSubBlocks; i++)
						{
							if (GetCodedBlockFlag(4, ref cbCodedBlockFlags, cbfIndex + i))
							{
								cbCodedBlockFlags.SetCodedBlockFlag(cbfIndex + i);

								ResidualBlockCabac(Math.Max(0, (startIdx - 1)), (endIdx - 1), 15, 4);
							}
						}
						for (uint i = 0; i < numSubBlocks; i++)
						{
							if (GetCodedBlockFlag(4, ref crCodedBlockFlags, cbfIndex + i))
							{
								crCodedBlockFlags.SetCodedBlockFlag(cbfIndex + i);

								ResidualBlockCabac(Math.Max(0, (startIdx - 1)), (endIdx - 1), 15, 4);
							}
						}
					}

					_currMbState.CbCodedBlockFlags = cbCodedBlockFlags.Flags;
					_currMbState.CrCodedBlockFlags = crCodedBlockFlags.Flags;
				}
			}
			else if (ChromaFormat == ChromaFormat.YCbCr444)
			{
#if DEBUG
				H264Debug.WriteLine("  [Cb]");
#endif
				var cbCodedBlockFlags = new CodedBlockFlags(_mbStateA.CbCodedBlockFlags, _mbStateB.CbCodedBlockFlags);
				ResidualLuma(macroBlockType, startIdx, endIdx, ref cbCodedBlockFlags, 1);

				_currMbState.CbCodedBlockFlags = cbCodedBlockFlags.Flags;

#if DEBUG
				H264Debug.WriteLine("  [Cr]");
#endif
				var crCodedBlockFlags = new CodedBlockFlags(_mbStateA.CrCodedBlockFlags, _mbStateB.CrCodedBlockFlags);
				ResidualLuma(macroBlockType, startIdx, endIdx, ref crCodedBlockFlags, 2); // Cr

				_currMbState.CrCodedBlockFlags = crCodedBlockFlags.Flags;
			}
		}

		private void ResidualLuma(IMacroblockType macroBlockType, int startIdx, int endIdx, ref CodedBlockFlags codedBlockFlags, int comp)
		{
			if ((startIdx == 0) && macroBlockType.IsIntra16X16)
			{
				if (GetCodedBlockFlag(CtxBlockCatTabLuma[comp, 0], ref codedBlockFlags, 32))
				{
					codedBlockFlags.SetCodedBlockFlag(32);

					ResidualBlockCabac(0, 15, 16, CtxBlockCatTabLuma[comp, 0]);
				}
			}
			for (uint i8X8 = 0; i8X8 < 4; i8X8++)
			{
				if (_codedBlockPattern.IsLumaBitSet(i8X8))
				{
					if (_mbFlags.TransformSize8X8Flag)
					{
						if ((ChromaFormat != ChromaFormat.YCbCr444) || GetCodedBlockFlag(CtxBlockCatTabLuma[comp, 3], ref codedBlockFlags, i8X8 + 16))
						{
							ResidualBlockCabac((4*startIdx), (4*endIdx) + 3, 64, CtxBlockCatTabLuma[comp, 3]);

							// Update coded_block_flag prediction
							codedBlockFlags.SetCodedBlockFlag(i8X8 + 16);
						}
					}
					else
					{
						for (uint i4X4 = 0; i4X4 < 4; i4X4++)
						{
							uint ctxBlockCat = macroBlockType.IsIntra16X16 ? CtxBlockCatTabLuma[comp, 1] : CtxBlockCatTabLuma[comp, 2];
							uint subBlkIdx = ((i8X8 << 2) + i4X4);
							if (GetCodedBlockFlag(ctxBlockCat, ref codedBlockFlags, subBlkIdx))
							{
								codedBlockFlags.SetCodedBlockFlag(subBlkIdx);

								if (macroBlockType.IsIntra16X16)
								{
									ResidualBlockCabac(Math.Max(0, (startIdx - 1)), (endIdx - 1), 15, ctxBlockCat);
								}
								else
								{
									ResidualBlockCabac(startIdx, endIdx, 16, ctxBlockCat);
								}
							}
						}
					}
				}
			}
		}

		private bool GetCodedBlockFlag(uint ctxBlockCat, ref CodedBlockFlags codedBlockFlags, uint subBlkIdx)
		{
			uint ctxIdx = CodedBlockFlagCtxBlockCatOffset[ctxBlockCat] + codedBlockFlags.GetCtxIdxInc(subBlkIdx);
			return (_arithmeticDecoder.DecodeDecision(ctxIdx) == 1);
		}

// ReSharper disable UnusedParameter.Local
		private void ResidualBlockCabac(int startIdx, int endIdx, int maxNumCoeff, uint ctxBlockCat)
// ReSharper restore UnusedParameter.Local
		{
			int numCoeff = (endIdx + 1);

			ulong significantCoeffFlags = 0U;
			for (int i = startIdx; i < (numCoeff - 1); i++)
			{
				if (GetSignificantCoeffFlag(ctxBlockCat, (uint)i))
				{
					significantCoeffFlags |= (1UL << i);

					if (GetLastSignificantCoeffFlag(ctxBlockCat, (uint)i))
					{
						numCoeff = (i + 1);
					}
				}
			}

			// Last significant_coeff_flag is inferred to be 1
			significantCoeffFlags |= 1UL << (numCoeff - 1);

			int numDecodAbsLevelGt1 = 0;
			int numDecodAbsLevelEq1 = 0;
			for (int i = (numCoeff - 1); i >= startIdx; i--)
			{
				if (((int)(significantCoeffFlags >> i) & 1) != 0)
				{
					uint absLevelMinus1 = GetCoeffAbsLevelMinus1(ctxBlockCat, numDecodAbsLevelGt1, numDecodAbsLevelEq1);
					if (absLevelMinus1 == 0)
					{
						numDecodAbsLevelEq1++;
					}
					else
					{
						numDecodAbsLevelGt1++;
					}

					_arithmeticDecoder.DecodeBypass(); // coeff_sign_flag
				}
			}
		}

		private bool GetSignificantCoeffFlag(uint ctxBlockCat, uint levelListIdx)
		{
			uint ctxIdx = SignificantCoeffFlagFrameCodedCtxBlockCatOffset[ctxBlockCat];
			// 9.3.3.1.3 Assignment process of ctxIdxInc for syntax elements significant_coeff_flag, last_significant_coeff_flag, and coeff_abs_level_minus1
			if (ctxBlockCat == 3)
			{
				ctxIdx += Math.Min((levelListIdx / ChromaFormat.NumC8X8), 2U);
			}
			else if (ctxBlockCat == 5 || ctxBlockCat == 9 || ctxBlockCat == 13)
			{
				ctxIdx += SignificantCoeffFlagCtxIdcInc0[levelListIdx];
			}
			else
			{
				ctxIdx += levelListIdx;
			}
			return _arithmeticDecoder.DecodeDecision(ctxIdx) == 1;
		}

		private bool GetLastSignificantCoeffFlag(uint ctxBlockCat, uint levelListIdx)
		{
			uint ctxIdx = LastSignificantCoeffFlagFrameCodedCtxBlockCatOffset[ctxBlockCat];
			// 9.3.3.1.3 Assignment process of ctxIdxInc for syntax elements significant_coeff_flag, last_significant_coeff_flag, and coeff_abs_level_minus1
			if (ctxBlockCat == 3)
			{
				ctxIdx += Math.Min((levelListIdx / ChromaFormat.NumC8X8), 2);
			}
			else if (ctxBlockCat == 5 || ctxBlockCat == 9 || ctxBlockCat == 13)
			{
				ctxIdx += LastSignificantCoeffFlagCtxIdcInc[levelListIdx];
			}
			else
			{
				ctxIdx += levelListIdx;
			}
			return _arithmeticDecoder.DecodeDecision(ctxIdx) == 1;
		}

		private uint GetCoeffAbsLevelMinus1(uint ctxBlockCat, int numDecodAbsLevelGt1, int numDecodAbsLevelEq1)
		{
			uint ctxIdxOffset = CoeffAbsLevelMinus1CtxBlockCatOffset[ctxBlockCat];
			// 9.3.3.1.3 Assignment process of ctxIdxInc for syntax elements significant_coeff_flag, last_significant_coeff_flag, and coeff_abs_level_minus1
			int ctxIdxInc0 = (numDecodAbsLevelGt1 != 0) ? 0 : Math.Min(4, 1 + numDecodAbsLevelEq1);
			if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + (uint)ctxIdxInc0) == 0)
			{
				return 0;
			}
			for (uint prefix = 1; prefix < 14; prefix++)
			{
				int ctxIdxInc = 5 + Math.Min(4 - ((ctxBlockCat == 3) ? 1 : 0), numDecodAbsLevelGt1);
				if (_arithmeticDecoder.DecodeDecision(ctxIdxOffset + (uint)ctxIdxInc) == 0)
				{
					return prefix;
				}
			}

			return 14 + GetExpGolomb();
		}

		private uint GetExpGolomb()
		{
			uint numBits = 0;
			while (_arithmeticDecoder.DecodeBypass() == 1)
			{
				if (++numBits == 31)
				{
					_readerState.Invalidate();
					return 0; // The exp-golomb code is invalid!
				}
			}

			uint value = 1;
			for (int i = 0; i < numBits; i++)
			{
				value = (value << 1) | _arithmeticDecoder.DecodeBypass();
			}
			return (value - 1);
		}


	}
}
