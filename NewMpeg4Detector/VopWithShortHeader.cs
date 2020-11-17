/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All rights reserved.
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
using System.Diagnostics;
using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Mpeg4
{
	internal class VopWithShortHeader : VopBase
	{
		private static readonly int[] MP4H263Width = { 128, 176, 352, 704, 1408 };
		private static readonly int[] MP4H263Height = { 96, 144, 288, 576, 1152 };
		private static readonly int[] MP4H263MbGob = { 8, 11, 22, 88, 352 };
		private static readonly int[] MP4H263GobVop = { 6, 9, 18, 18, 18 };
		private static readonly int[] MP4H263RowGob = { 1, 1, 1, 2, 4 };

		private enum H263PictureTypeCode : byte	// (3 bits) H263PictureTypeCode
		{
			I_Picture = 0x00,
			P_Picture = 0x01,
			ImprovedPBFrame = 0x02,
			B_Picture = 0x03,
			EI_Picture = 0x04,
			EP_Picture = 0x05,
			Reserved = 0x06,
			//Reserved = 0x07,
			Invalid = 0xFF	// Value not set
		}

		private enum PTypeSourceFormat : byte // (3 bits used)
		{
			Forbidden = 0x00,
			SubQCif = 0x01,
			QCif = 0x02,
			Cif = 0x03,
			FourCif = 0x04,
			SixteenCif = 0x05,
			Reserved = 0x06,
			ExtendedPType = 0x07,
			Invalid = 0xFF
		}

		private enum PlusPTypeSourceFormat : byte // (3 bits used)
		{
			Reserved = 0x00,
			SubQCif = 0x01,
			QCif = 0x02,
			Cif = 0x03,
			FourCif = 0x04,
			SixteenCif = 0x05,
			CustomSourceFormat = 0x06,
			//Reserved = 0x07,
			Invalid = 0xFF
		}

		private enum FillModeH263 : byte
		{
			Color = 0,
			Black = 1,
			Gray = 2,
			Clip = 3,
			Invalid = 0xFF
		}

		private uint _numMacroblocksInGob;

		/// <summary>
		/// gob_resync_marker: This is a fixed length code of 17 bits having the
		/// value ‘0000 0000 0000 0000 1’ which may optionally be inserted at
		/// the beginning of each gob_layer(). Its purpose is to serve as a type
		/// of resynchronization marker for error recovery in the bitstream.
		/// The gob_resync_marker codes may (and should) be byte aligned by
		/// inserting zero to seven zero-valued bits in the bitstream just prior
		/// to the gob_resync_marker in order to obtain byte alignment.
		/// The gob_resync_marker shall not be present for the first GOB
		/// (for which gob_number = 0).
		/// </summary>
		private const int GobResyncMarker = 1;

		/// <summary>
		/// short_video_end_marker: This is a 22-bit end of sequence marker
		/// containing the value ‘0000 0000 0000 0000 1111 11’. It is used to
		/// mark the end of a sequence of video_plane_with_short_header().
		/// short_video_end_marker may (and should) be byte aligned by the
		/// insertion of zero to seven zero-valued bits to achieve byte
		/// alignment prior to short_video_end_marker.
		private const int ShortVideoEndMarker = 0x3F;

		public byte TemporalReference { get; private set; }

		public VopWithShortHeader(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.VopWithShortHeader)
		{
		}

		public override bool IsSuitableParent(Mpeg4Header parent)
		{
			if (base.IsSuitableParent(parent) == false) return false;

			// A result can only contain H263 short video VOP's or VOP's and VOL's.
			// Search the result for the type of headers in it.
			Mpeg4Header header = Root.FindChildren(new Mpeg4HeaderName[] { Mpeg4HeaderName.Vop, Mpeg4HeaderName.VideoObjectLayer, Mpeg4HeaderName.VopWithShortHeader }, true);

			if (header == null) return true;

			return (header.HeaderName == Mpeg4HeaderName.VopWithShortHeader);
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			VideoObjectLayer VOL = new VideoObjectLayer(Parent as Mpeg4Header, true);
			VOL.Parse(parser);

			_fcodeForward = 1;

			Attributes.Add(new FormattedAttribute<Attribute, bool>(Attribute.ShortVideoHeader, true));

			// Get the Temporal Reference (TR)
			this.TemporalReference = (byte)parser.GetBits(8, Attribute.TemporalReference);

			// Get the Type Information (PTYPE) (Variable Length)
			if (!parser.GetMarkerBit())	// PTYPE, bit 1. First bit must always be '1',
			{										// in order to avoid start code emulation.
#if DEBUG
				//long offset = parser.GetDataPacket(parser._dataReader.BytePosition, 1).StartOffset;
				//string message = string.Format(CultureInfo.CurrentCulture, "Offset: 0x{0}; First bit of PTYPE is '0', it must always be '1'.", offset.ToString("X", CultureInfo.CurrentCulture));
				//Debug.WriteLineIf(_mpeg4H263DebugInfo, message, Mpeg4H263);
#endif // DEBUG
				return false;
			}
			// The second bit must always be '0', for distinction with ITU-T Rec. H.261.
			if (!parser.GetZeroBit()) // PTYPE, bit 2
			{
#if DEBUG
				Debug.WriteLineIf(_mpeg4H263DebugInfo, "Second bit of PTYPE is '1', it must always be '0'.", Mpeg4H263);
#endif // DEBUG
				return false;
			}
			/*bool splitScreenIndicator = */parser.GetBit();	// PTYPE, bit 3
			/*bool documentCameraIndicator = */parser.GetBit();	// PTYPE, bit 4
			/*bool fullPictureFreezeRelease = */parser.GetBit();	// PTYPE, bit 5
			byte sourceFormat = (byte)parser.GetBits(3);	// PTYPE, bit 6-8
			Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.PTypeSourceFormat, Enum.GetName(typeof(PTypeSourceFormat), sourceFormat)));

			bool extendedPType = (sourceFormat == (byte)PTypeSourceFormat.ExtendedPType);
			if (sourceFormat == (byte)PTypeSourceFormat.Forbidden)
			{
#if DEBUG
				Debug.WriteLineIf(_mpeg4H263DebugInfo, "The source format has the forbidden value of '0'.", Mpeg4H263);
#endif // DEBUG
				return false;
			}
			H263PictureTypeCode pictureCodingType = H263PictureTypeCode.Invalid;
			bool optionalPBFramesMode = false;
			bool optionalCustomPictureClockFrequency = false;
			bool cpm = false;

			CustomPictureFormat customPictureFormat = null;

			if (!extendedPType)
			{
				pictureCodingType = parser.GetBit() ? H263PictureTypeCode.P_Picture : H263PictureTypeCode.I_Picture;	// PTYPE, bit 9
				_codingType = (byte)pictureCodingType;
				Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.CodingType, Enum.GetName(typeof(H263PictureTypeCode), _codingType)));

				/*bool optionalUnrestrictedMotionVectorMode = */parser.GetBit();	// PTYPE, bit 10
				/*bool optionalSyntaxBasedArithmeticCodingMode = */parser.GetBit();	// PTYPE, bit 11
				/*bool optionalAdvancedPredictionMode = */parser.GetBit();	// PTYPE, bit 12
				optionalPBFramesMode = parser.GetBit();	// PTYPE, bit 13

				// If bit 9 is set to 0, bit 13 shall be set to 0 as well
				if (pictureCodingType == H263PictureTypeCode.I_Picture && optionalPBFramesMode)
				{
#if DEBUG
					Debug.WriteLineIf(_mpeg4H263DebugInfo, "Bit 9 of PTYPE is '0', and bit 13 is '1', while bit 13 should be '0'.", Mpeg4H263);
#endif // DEBUG
					return false;
				}
			}
			else// SourceFormat == SourceFormat.ExtendedPType
			{	// Plus PTYPE (PLUSPTYPE) (Variable Length; 12 or 30 bits)

				// Update Full Extended PTYPE (UFEP) (3 bits)
				// Values of UFEP other than "000" and "001" are reserved
				byte updateFullExtendedPType = (byte)parser.GetBits(3);

				//PlusPTypeSourceFormat plusPTypeSourceFormat = PlusPTypeSourceFormat.Invalid;
				bool optionalUnrestrictedMotionVector = false;
				bool optionalSliceStructured = false;
				bool optionalReferencePictureSelection = false;
				if (updateFullExtendedPType == 1)
				{	// The Optional Part of PLUSPTYPE (OPPTYPE) (18 bits)
					// overwrite sourceFormat variable
					sourceFormat = (byte)parser.GetBits(3);	// bit 1-3
					Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.PlusPTypeSourceFormat, Enum.GetName(typeof(PlusPTypeSourceFormat), (sourceFormat == 7) ? (byte)0 /* enum values 0 and 7 are both 'Reserved' */: sourceFormat)));

					optionalCustomPictureClockFrequency = parser.GetBit();	// bit 4 (PCF)
					optionalUnrestrictedMotionVector = parser.GetBit();	// bit 5 (UMV)
					/*bool optionalSyntaxBasedArithmeticCoding = */parser.GetBit();	// bit 6 (SAC)
					/*bool optionalAdvancedPrediction = */parser.GetBit();	// bit 7 (AP)
					/*bool optionalAdvancedIntraCoding = */parser.GetBit();	// bit 8 (AIC)
					/*bool optionalDeblockingFilter = */parser.GetBit();	// bit 9 (DF)
					optionalSliceStructured = parser.GetBit();	// bit 10 (SS)
					optionalReferencePictureSelection = parser.GetBit();	// bit 11 (RPS)
					/*bool optionalIndependentSegmentDecoding = */parser.GetBit();	// bit 12 (ISD)
					/*bool optionalAlternativeInterVariableLengthCode = */parser.GetBit();	// bit 13 (AIV)
					_optionalModifiedQuantization = parser.GetBit();	// bit 14 (MQ)

					if (!parser.GetOneBit())	// bit 15 should be "1" to prevent start code emulation
					{
#if DEBUG
						Debug.WriteLineIf(_mpeg4H263DebugInfo, "Bit 15 of PLUSPTYPE is '0' but should be '1'.", Mpeg4H263);
#endif // DEBUG
						return false;
					}

					if (parser.GetBits(3) != 0)	// bit 16-18 shall be equal to "0"
					{
#if DEBUG
						Debug.WriteLineIf(_mpeg4H263DebugInfo, "Bit 16-18 of PLUSPTYPE are != '0' but shall be equal to '0'.", Mpeg4H263);
#endif // DEBUG
						return false;
					}
				}

				// The mandatory part of PLUSPTYPE (MPPTYPE) (9 bits)
				pictureCodingType = (H263PictureTypeCode)parser.GetBits(3); // bits 1-3
				_codingType = (byte)pictureCodingType;
				Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.CodingType, Enum.GetName(typeof(H263PictureTypeCode), (_codingType == 7) ? (byte)6 /* enum values 6 and 7 are both 'Reserved' */: _codingType)));

				bool optionalReferencePictureResamplingMode = parser.GetBit();	// bit 4 (RPR)
				bool optionalReducedResolutionUpdateMode = parser.GetBit();	// bit 5 (RRU)

				bool roundingType = parser.GetBit();	// bit 6 (RTYPE)

				// bit 6 can be set to "1" only when bits 1-3 indicate P-picture, Improved PB-frame or EP-frame
				if (roundingType &&
					!(pictureCodingType == H263PictureTypeCode.P_Picture ||
					pictureCodingType == H263PictureTypeCode.ImprovedPBFrame ||
					pictureCodingType == H263PictureTypeCode.EP_Picture))
				{
#if DEBUG
					Debug.WriteLineIf(_mpeg4H263DebugInfo, "Bit 6 of PLUSPTYPE can only be set to '1' when bits 1-3 indicate P-picture, Improved PB-frame or EP-frame.", Mpeg4H263);
#endif // DEBUG
					return false;
				}

				// Read two reserved bits
				// Shall be equal to "0"
				if (parser.GetBits(2) != 0)	// bit 7 and 8
				{
#if DEBUG
					Debug.WriteLineIf(_mpeg4H263DebugInfo, "The reserved bits 7 and 8 of PLUSPTYPE are != '0', but should be '0'.", Mpeg4H263);
#endif // DEBUG
					return false;
				}

				// Bit 9 must be equal to "1" to prevent start code emulation
				if (!parser.GetOneBit())	// bit 9
				{
#if DEBUG
					Debug.WriteLineIf(_mpeg4H263DebugInfo, "Bit 9 of PLUSPTYPE is '0' but must be '1'.", Mpeg4H263);
#endif // DEBUG
					return false;
				}

				// If the picture type is INTRA or EI, updateFullExtendedPType shall be set to "001".
				if ((pictureCodingType == H263PictureTypeCode.I_Picture || pictureCodingType == H263PictureTypeCode.EI_Picture) &&
					updateFullExtendedPType != 1)
				{
#if DEBUG
					Debug.WriteLineIf(_mpeg4H263DebugInfo, "If the picture type is INTRA or EI, updateFullExtendedPType shall be set to '001'", Mpeg4H263);
#endif // DEBUG
					return false;
				}

				// Continuous Presence Multipoint and Video Multiplex (CPM) (1 bit)
				// CPM follows immediately after PLUSPTYPE if PLUSPTYPE is present,
				// but follows PQUANT in the picture header if PLUSPTYPE is not present.
				// - This is the case where PLUSPTYPE is present
				cpm = ReadCpmAndPsbi(parser);

				// Custom Picture Format (CPFMT) (23 bits)
				// A fixed length codeword of 23 bits that is present only if the use of a
				// custom picture format is signalled in PLUSPTYPE and UFEP is '001'
				if (sourceFormat == (int)PlusPTypeSourceFormat.CustomSourceFormat && updateFullExtendedPType == 1)
				{
					customPictureFormat = new CustomPictureFormat(optionalReducedResolutionUpdateMode);
					if(!customPictureFormat.Parse(parser)) return false;

					// Extended Pixel Aspect Ratio (EPAR) (16 bits)
					if (customPictureFormat.PixelAspectRatioCode == 15/*Extended PAR*/)
					{
						// The natural binary representation of the PAR width
						byte parWidth = (byte)parser.GetBits(8);	// bit 1-8
						// PAR Width: "0" is forbidden.
						if (parWidth == 0) return false;
						// The natural binary representation of the PAR height.
						byte parHeight = (byte)parser.GetBits(8);	// bit 9-16
						// PAR Height: "0" is forbidden.
						if (parHeight == 0) return false;
					}
				}

				// Custom Picture Clock Frequency Code (CPCFC) (8 bits)
				// A fixed length codeword of 8 bits that is present only if PLUSPTYPE is present
				// and UFEP is 001 and a custom picture clock frequency is signalled in PLUSPTYPE.
				if (updateFullExtendedPType == 1 && optionalCustomPictureClockFrequency)
				{
					// Clock Conversion Code: "0" indicates a clock conversion factor of 1000 and
					// "1" indicates 1001
					/*bool clockConversionCode = */parser.GetBit();	// bit 1

					// Bits 2-8
					// Clock Divisor: "0" is forbidden. The natural binary representation of the value
					// of the clock divisor.
					byte clockDivisor = (byte)parser.GetBits(7);	// bit 2-8
					if (clockDivisor == 0) return false;
				}

				// Extended Temporal Reference (ETR) (2 bits)
				// A fixed length codeword of 2 bits which is present only if a custom picture clock frequency is in
				// use (regardless of the value of UFEP). It is the two MSBs of the 10-bit Temporal Reference (TR).
				if (optionalCustomPictureClockFrequency)
				{
					/*byte extendedTemporalReference = (byte)*/parser.GetBits(2);	// bit 1-2
				}

				// Unlimited Unrestricted Motion Vectors Indicator (UUI) (Variable length)
				// A variable length codeword of 1 or 2 bits that is present only if the
				// optional Unrestricted Motion Vector mode is indicated in PLUSPTYPE and UFEP is 001.
				if (updateFullExtendedPType == 1 && optionalUnrestrictedMotionVector)
				{
					bool unlimitedUnrestrictedMotionVectorsIndicator = parser.GetBit();

					if (!unlimitedUnrestrictedMotionVectorsIndicator)
					{
						// UUI = "01" The motion vector range is not limited except by the picture size.
						unlimitedUnrestrictedMotionVectorsIndicator = parser.GetBit();
						if (!unlimitedUnrestrictedMotionVectorsIndicator) return false;
					}
					else
					{
						// UUI = "1" The motion vector range is limited according to Tables D.1 and D.2.
					}
				}

				// Slice Structured Submode bits (SSS) (2 bits)
				// A fixed length codeword of 2 bits which is present only if the optional Slice Structured mode
				// (see Annex K) is indicated in PLUSPTYPE and UFEP is 001. If the Slice Structured mode is in use
				// but UFEP is not 001, the last values sent for SSS shall remain in effect.
				if (updateFullExtendedPType == 1 && optionalSliceStructured)
				{
					// Bit 1; "0" indicates free-running slices, "1" indicates rectangular slices
					/*bool rectangularSlices = */parser.GetBit();
					// Bit 2 "0" indicates sequential order, "1" indicates arbitrary order
					/*bool arbitrarySliceOrdering = */parser.GetBit();
				}

				if (pictureCodingType == H263PictureTypeCode.B_Picture ||
					pictureCodingType == H263PictureTypeCode.EI_Picture ||
					pictureCodingType == H263PictureTypeCode.EP_Picture)
				{
					// Enhancement Layer Number (ELNUM) (4 bits)
					// A fixed length codeword of 4 bits which is present only if the optional Temporal, SNR,
					// and Spatial Scalability mode is in use (regardless of the value of UFEP)
					/*byte enhancementLayerNumber = (byte)*/parser.GetBits(4);

					// Reference Layer Number (RLNUM) (4 bits)
					// A fixed length codeword of 4 bits which is present only if the optional Temporal, SNR,
					// and Spatial Scalability mode is in use (see Annex O) and UFEP is 001.
					if (updateFullExtendedPType == 1)
					{
						/*byte referenceLayerNumber = (byte)*/parser.GetBits(4);

						// Note that for B-pictures in an enhancement layer having temporally surrounding EI- or EP-pictures
						// which are present in the same enhancement layer, RLNUM shall be equal to ELNUM
						//if (pictureCodingType == H263VopType.BPicture && updateFullExtendedPType != referenceLayerNumber) return false;
					}
				}

				// Reference Picture Selection Mode Flags (RPSMF) (3 bits)
				if (updateFullExtendedPType == 1 && optionalReferencePictureSelection)
				{
					/*byte referencePictureSelectionModeFlags = (byte)*/parser.GetBits(3);
				}

				if (optionalReferencePictureSelection)
				{
					// Temporal Reference for Prediction Indication (TRPI) (1 bit)
					// A fixed length codeword of 1 bit that is present only if the optional Reference Picture Selection
					// mode is in use (regardless of the value of UFEP).
					bool temporalReferenceForPredictionIndication = parser.GetBit();

					if (temporalReferenceForPredictionIndication)
					{
						if (pictureCodingType == H263PictureTypeCode.I_Picture || pictureCodingType == H263PictureTypeCode.EI_Picture)
						{
							// TRPI shall be 0 whenever the picture header indicates an I- or EI-picture.
							return false;
						}

						// Temporal Reference for Prediction (TRP) (10 bits)
						parser.FlushBits(10);
					}

					// Back-Channel message Indication (BCI) (Variable length)
					while (parser.GetBit() == true)
					{
						// Back-Channel Message (BCM) (Variable length)
						// Structure: BT URF TR ELNUMI ELNUM BCPM BSBI BEPB1 GN/MBA BEPB2 RTR BSTUF

						// Back-channel message Type (BT) (2 bits)
						parser.FlushBits(2);

						// Unreliable Flag (URF) (1 bit)
						// 0: Reliable; 1: Unreliable.
						parser.FlushBits(1);

						// Temporal Reference (TR) (10 bits)
						parser.FlushBits(10);

						// Enhancement Layer Number Indication (ELNUMI) (1 bit)
						bool enhancementLayerNumberIndication = parser.GetBit();

						// Enhancement Layer Number (ELNUM) (4 bits)
						if (enhancementLayerNumberIndication)
						{
							parser.FlushBits(4);
						}

						// BCPM (1 bit)
						bool bcpm = parser.GetBit();

						// Back-channel Sub-Bitstream Indicator (BSBI) (2 bits)
						if (bcpm)
						{
							parser.FlushBits(2);
						}

						// Back-channel Emulation Prevention Bit 1 (BEPB1) (1 bit)
						// A field which is present if and only if the videomux mode is in use.
						//if(videomux mode is in use) TODO
						//{
						// This field is always set to "1" to prevent a start code emulation.
						if (!parser.GetOneBit()) return false;
						//}
					}
					bool bit2 = parser.GetBit();
					if (bit2 == false) return false; // If bit1 == 0, bit2 must be 1.
				}

				// Reference Picture Resampling Parameters (RPRP) (Variable length)
				if (optionalReferencePictureResamplingMode)
				{
					ParseReferencePictureResampling(parser, pictureCodingType);
				}
			}// SourceFormat == SourceFormat.ExtendedPType

			// Quantizer Information (PQUANT) (5 bits)
			_pQuant = parser.GetBits(5);

			// Continuous Presence Multipoint and Video Multiplex (CPM) (1 bit)
			// CPM follows immediately after PLUSPTYPE if PLUSPTYPE is present,
			// but follows PQUANT in the picture header if PLUSPTYPE is not present.
			// - This is the case where PLUSPTYPE is not present.
			if (!extendedPType)
			{
				cpm = ReadCpmAndPsbi(parser);
			}

			if (optionalPBFramesMode ||	// PTYPE indicates "PB-frame"
				pictureCodingType == H263PictureTypeCode.ImprovedPBFrame)	//PLUSPTYPE indicates "Improved PB-frame"
			{
				// Temporal Reference for B-pictures in PB-frames (TRB) (3/5 bits)
				// TRB is present if PTYPE or PLUSPTYPE indicates "PB-frame" or "Improved PB-frame"
				//
				// The codeword is the natural binary representation of the number of nontransmitted
				// pictures plus one. It is 3 bits long for standard CIF picture clock frequency and is
				// extended to 5 bits when a custom picture clock frequency is in use. The maximum number
				// of nontransmitted pictures is 6 for the standard CIF picture clock frequency and 30 when
				// a custom picture clock frequency is used.
				byte trb = (byte)parser.GetBits((short)(optionalCustomPictureClockFrequency ? 5 : 3));
				if (trb == 0) return false;

				// Quantization information for B-pictures in PB-frames (DBQUANT) (2 bits)
				// DBQUANT is present if PTYPE or PLUSPTYPE indicates "PB-frame" or "Improved PB-frame"
				/*byte dbQuant = (byte)*/parser.GetBits(2);
			}

			// PEI (1 bit) and PSUPP (0/8/16 ... bits)
			for (; ; )
			{
				// Extra Insertion Information (PEI) (1 bit)
				// A bit which when set to "1" signals the presence of the following optional data field.
				bool extraInsertionInformation = parser.GetBit(); // 1 bit (PEI)
				// If PEI is set to "1", then 9 bits follow consisting of 8 bits of data (PSUPP)
				// and then another PEI bit to indicate if a further 9 bits follow and so on.
				if (!extraInsertionInformation)
					break;
				/*byte supplementalEnhancementInformation = (byte)*/parser.GetBits(8); // 8 bit (PSUPP)
			}

			// Group of Blocks Layer (GOB)
			// Structure: GSTUF GBSC GN GSBI GFID GQUANT Macroblock Data

			uint numGobsInVop;
			uint numRowsInGob;

			if (customPictureFormat != null)
			{
				_vopWidth = customPictureFormat.PixelsPerLine;
				_vopHeight = customPictureFormat.NumberOfLines;
				numGobsInVop = customPictureFormat.GobsInVopCount;
				_numMacroblocksInGob = customPictureFormat.MacroblocksInGobCount;
				numRowsInGob = customPictureFormat.RowsInGobCount;
			}
			else
			{
				int sourceFormatIndex = sourceFormat - 1;

				if (sourceFormatIndex < 0 || sourceFormatIndex >= MP4H263Width.Length)
					return false;

				_vopWidth = (uint)MP4H263Width[sourceFormatIndex];
				_vopHeight = (uint)MP4H263Height[sourceFormatIndex];
				numGobsInVop = (uint)MP4H263GobVop[sourceFormatIndex];
				_numMacroblocksInGob = (uint)MP4H263MbGob[sourceFormatIndex];
				numRowsInGob = (uint)MP4H263RowGob[sourceFormatIndex];
			}

			int gob_number = 0;
			uint rowNum = 0;
			uint colNum = 0;

			for (int i = 0; i < numGobsInVop; i++)
			{
				if (!GobLayer(parser, VOL, ref gob_number, ref rowNum, ref colNum, cpm, numRowsInGob, numGobsInVop)) return false;
			}

			// Stuffing (ESTUF) (Variable length)
			// A codeword of variable length consisting of less than 8 zero-bits.
			// TODO 5.1.26 

			// End Of Sequence (EOS) (22 bits)
			if (parser.ShowBits(22) == ShortVideoEndMarker)
			{
				parser.FlushBits(22);
			}

			// Stuffing (PSTUF) (Variable length)
			// A codeword of variable length consisting of less than 8 zero-bits.
			while (!parser.ByteAligned)
			{
				if (!parser.GetZeroBit()) return false;
			}

			return true;
		}

		private class CustomPictureFormat
		{
			private readonly bool _optionalReducedResolutionUpdateMode;

			internal uint PixelsPerLine { get; private set; }
			internal uint NumberOfLines { get; private set; }
			internal byte PixelAspectRatioCode { get; private set; }
			internal uint GobsInVopCount { get; private set; }
			internal uint MacroblocksInGobCount { get; private set; }
			internal uint RowsInGobCount { get; private set; }

			private uint K
			{
				get
				{
					// If the number of lines is more than 800, then k = 4
					if (NumberOfLines > 800)
					{
						return 4;
					}
					// If the number of lines is less than or equal to 400 and the optional
					// Reduced-Resolution Update mode is not in use, then k = 1.
					if (NumberOfLines <= 400 && !_optionalReducedResolutionUpdateMode)
					{
						return 1;
					}
					// If the number of lines is less than or equal to 800 and the optional
					// Reduced-Resolution Update mode is in use or the number of lines is more
					// than 400, then k = 2.
					return 2;
				}
			}

			internal CustomPictureFormat(bool optionalReducedResolutionUpdateMode)
			{
				_optionalReducedResolutionUpdateMode = optionalReducedResolutionUpdateMode;
			}

			internal bool Parse(Mpeg4Parser parser)
			{
				PixelAspectRatioCode = (byte)parser.GetBits(4);	// bit 1-4
				if (PixelAspectRatioCode == 0/*Forbidden*/) return false;

				ushort pictureWidthIndication = (ushort)parser.GetBits(9);	// bit 5-13
				PixelsPerLine= (pictureWidthIndication + 1U)*4U;

				if (!parser.GetOneBit()) return false;	// Bit 14 equal to "1" to prevent start code emulation;

				ushort pictureHeightIndication = (ushort)parser.GetBits(9);	// bit 15-23
				NumberOfLines = pictureHeightIndication * 4U;

				uint k = K;
				const int macroblockLength = 16 * 16;
				GobsInVopCount = NumberOfLines / (16U * k);
				MacroblocksInGobCount = (PixelsPerLine * NumberOfLines) / (GobsInVopCount * macroblockLength);
				RowsInGobCount = k;

				return true;
			}
		}

		/// <summary>
		/// This is a layer containing a fixed number of macroblocks in the VOP.
		/// Which macroblocks which belong to each gob can be determined by
		/// gob_number and num_macroblocks_in_gob.
		/// Remark: only used by short video
		/// </summary>
		private bool GobLayer(Mpeg4Parser parser, VideoObjectLayer VOL, ref int gob_number, ref uint rowNum, ref uint colNum, bool cpm, uint numRowsInGob, uint numGobsInVop)
		{
			//bool gob_header_empty = true;
			if (gob_number != 0)
			{
				if(parser.ShowBits(17) == GobResyncMarker)
				{
					//gob_header_empty = false;

					parser.FlushBits(17);

					// Group Number (GN) (5 bits)
					gob_number = (int)parser.GetBits(5);

					// check gob_number is valid
					if (gob_number > numGobsInVop)
					{
#if DEBUG
						Debug.WriteLineIf(_mpeg4H263DebugInfo, "H263 GOB number is invalid.", Mpeg4H263);
#endif // DEBUG
						return false;
					}

					// GOB Sub-Bitstream Indicator (GSBI) (2 bits)
					// A fixed length codeword of 2 bits that is only present if CPM is "1" in the picture header.
					if (cpm)
					{
						/*byte gobSubBitstreamIndicator = (byte)*/parser.GetBits(2);
					}

					// GOB Frame ID (GFID) (2 bits)
					// GFID shall have the same value in every GOB (or slice) header
					// of a given picture.
					parser.GetBits(2);

					// Quantizer Information (GQUANT) (5 bits)
					_quantScale = parser.GetBits(5);

					rowNum = (uint)(gob_number * numRowsInGob);
					colNum = 0;
				}
			}

			for (int i = 0; i < _numMacroblocksInGob; i++)
			{
				if (!HandleMacroBlock(parser, VOL, ref colNum, ref rowNum))
				{
					if (gob_number == 0)
					{
#if DEBUG
						Debug.WriteLineIf(_mpeg4H263DebugInfo, "H263 GOB number is 0.", Mpeg4H263);
#endif // DEBUG
						return false;
					}
					else
					{
						//break;
						return true;
					}
				}
			}
			if (parser.ShowBits(17) != GobResyncMarker &&
				parser.ShowNextBitsByteAligned(17) == GobResyncMarker)
			{
				while (!parser.ByteAligned)
				{
					if (!parser.GetZeroBit()) return false;
				}
			}
			gob_number++;
			return true;
		}

		static private bool ParseReferencePictureResampling(Mpeg4Parser parser, H263PictureTypeCode pictureCodingType)
		{
			// Warping Displacement Accuracy (WDA) (2 bits)
			/*byte warpingDisplacementAccuracy = (byte)*/parser.GetBits(2);

			FillModeH263 fillMode = FillModeH263.Invalid;
			// Warping parameters (Variable length)
			if (pictureCodingType == H263PictureTypeCode.P_Picture ||
				pictureCodingType == H263PictureTypeCode.B_Picture ||
				pictureCodingType == H263PictureTypeCode.ImprovedPBFrame)
			{
				// VLC-coded warping parameters
				// TODO


				// Fill Mode (FILL_MODE) (2 bits)
				fillMode = (FillModeH263)parser.GetBits(2);



			}
			else if (pictureCodingType == H263PictureTypeCode.EP_Picture)
			{


				// For an EP-picture, the fill-mode action is the same as that for
				// the reference layer, and the two fill-mode bits are not sent.
				// TODO

			}
			//TODO else if(pictureCodingType == H263VopType.

			// Fill Color Specification (Y_FILL, CB_EPB, CB_FILL, CR_EPB, CR_FILL) (26 bits)
			if (pictureCodingType != H263PictureTypeCode.EP_Picture && fillMode == FillModeH263.Color)
			{
				// Y_FILL
				parser.FlushBits(8);
				// CB_EPB
				bool cbepb = parser.GetBit();
				if (cbepb == false) return false;
				// CB_FILL
				parser.FlushBits(8);
				// CR_EPB
				bool crepb = parser.GetBit();
				if (crepb == false) return false;
				// CR_FILL
				parser.FlushBits(8);
			}

			//Resampling algorithm
			// TODO

			return true;
		}

		/// <summary>
		/// Read Continuous Presence Multipoint and Video Multiplex (CPM) and
		/// Picture Sub-Bitstream Indicator (PSBI) (when CPM is 1)
		/// </summary>
		/// <param name="dataInfo"></param>
		/// <returns>The value of CMP</returns>
		static private bool ReadCpmAndPsbi(Mpeg4Parser parser)
		{
			// Continuous Presence Multipoint and Video Multiplex (CPM) (1 bit)
			bool cpm = parser.GetBit();
			if (cpm)
			{
				// Picture Sub-Bitstream Indicator (PSBI) (2 bits)
				/*byte psbi = (byte)*/parser.GetBits(2);
			}
			return cpm;
		}

	}
}
