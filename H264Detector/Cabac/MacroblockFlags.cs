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
	internal struct MacroblockFlags
	{
		private const int  CodedBlockBit           = 7;
		private const uint CodedBlockMask          = (1U << CodedBlockBit);
		private const uint MbUnavailableMask       = 0x40;
		private const uint ReservedFlag20          = 0x20;
		private const uint ReservedFlag10          = 0x10;
		private const int  FieldDecodingModeBit    = 3; // defaults to 0 (frame) for unavailable blocks
		private const uint FieldDecodingModeMask   = (1U << FieldDecodingModeBit);
		private const int  TransformSize8X8Bit     = 2;
		private const uint TransformSize8X8Mask    = (1U << TransformSize8X8Bit);
		private const int  IntraChromaPredModeBit  = 1;
		private const uint IntraChromaPredModeMask = (1U << IntraChromaPredModeBit);
		private const int  MbTypeNotZeroBit        = 0;
		private const uint MbTypeNotZeroMask       = (1U << MbTypeNotZeroBit);

		private uint _flags;

		#region Properties
		public bool IsUnavailable { get { return (_flags & MbUnavailableMask) != 0; } }
		public bool TransformSize8X8Flag { get { return (_flags & TransformSize8X8Mask) != 0; } }

		public byte Bits { get { return (byte)_flags; } }
		#endregion Properties

		public MacroblockFlags(byte flagsA, byte flagsB)
		{
			_flags = (uint)((flagsA << 8) | (flagsB << 16));
		}

		// 9.3.3.1.1.1 Derivation process of ctxIdxInc for the syntax element mb_skip_flag
		public uint GetMbSkipFlagCtxIdxInc()
		{
			return ((_flags >> ( 8 + CodedBlockBit)) & 1) + // condTermFlagA
				   ((_flags >> (16 + CodedBlockBit)) & 1);  // condTermFlagB
		}

		public void SetCodedBlockFlag()
		{
			_flags |= CodedBlockMask;
		}

		// 9.3.3.1.1.2 Derivation process of ctxIdxInc for the syntax element mb_field_decoding_flag
		public uint GetMbFieldDecodingFlagCtxIdxInc()
		{
			return ((_flags >> ( 8 + FieldDecodingModeBit)) & 1) + // condTermFlagA
				   ((_flags >> (16 + FieldDecodingModeBit)) & 1);  // condTermFlagB
		}

		public void SetFieldDecodingMode()
		{
			_flags |= FieldDecodingModeMask;
		}

		// 9.3.3.1.1.10 Derivation process of ctxIdxInc for the syntax element transform_size_8x8_flag
		public uint GetTransformSize8X8FlagCtxIdxInc()
		{
			return ((_flags >> ( 8 + TransformSize8X8Bit)) & 1) + // condTermFlagA
				   ((_flags >> (16 + TransformSize8X8Bit)) & 1);  // condTermFlagB
		}

		public void SetTransformSize8X8Flag()
		{
			_flags |= TransformSize8X8Mask;
		}

		// 9.3.3.1.1.8 Derivation process of ctxIdxInc for the syntax element intra_chroma_pred_mode
		public uint GetIntraChromaPredModeCtxIdxInc()
		{
			return ((_flags >> ( 8 + IntraChromaPredModeBit)) & 1) + // condTermFlagA
				   ((_flags >> (16 + IntraChromaPredModeBit)) & 1);  // condTermFlagB
		}

		public void SetIntraChromaPredModeNotZero()
		{
			_flags |= IntraChromaPredModeMask;
		}

		// 9.3.3.1.1.3 Derivation process of ctxIdxInc for the syntax element mb_type
		public uint GetMbTypeCtxIdxInc()
		{
			return ((_flags >> ( 8 + MbTypeNotZeroBit)) & 1) + // condTermFlagA
				   ((_flags >> (16 + MbTypeNotZeroBit)) & 1);  // condTermFlagB
		}

		public void SetMbTypeNotZero()
		{
			_flags |= MbTypeNotZeroMask;
		}
	}
}
