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
using System.Linq;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class Slice : IVideoHeaderParser
	{
		//private static readonly ILog Log = LogManager.GetLogger("MPEG2Video");
		private static readonly int[] Escape = new [] { -1, -1 };
		private static readonly int[] EndOfBlock = new [] { -1, 0 };
		private static readonly int[] Invalid = new int[] { InvalidInteger, InvalidInteger };
		private const byte InvalidInteger = byte.MaxValue;
		private const int MacroblockEscape = -1;
		private const int MacroblockStuffing = -2;

		private enum Attribute
		{
			SliceVerticalPosition,
			SliceVerticalPositionExtension,
			MarcoblockRow,
			PriorityBreakpoint,
			QuantiserScaleCode,
			SliceExtensionFlag,
			IntraSlice,
			SlicePictureIdEnable,
			SlicePictureId,
			ExtraBitSlice,
			ExtraInformationSlice,
		}

		internal const string Name = "Slice";

		private enum MotionVectorFormat	// mv_format
		{
			Unknown,
			Field,
			Frame
		}

		#region VLC Tables
		[Flags]
		public enum MacroblockMode
		{
			NoFlags = 0,
			Quant = 1,
			MotionForward = 2,
			MotionBackward = 4,
			Pattern = 8,
			Intra = 16,
			SpatialTemporalWeightCodeFlag = 32,
			Invalid = 64
		}

			/// <summary>
		/// Table B.1 – Variable length codes for macroblock_address_increment
		/// </summary>
		private static readonly VlcTable<int> MacroblockAddressIncrementTable = new VlcTable<int>(new object[,]
		{
			{ "1",			 1 },	{ "0000010101",		18 },
			{ "011",		 2 },	{ "0000010100",		19 },
			{ "010",		 3 },	{ "0000010011",		20 },
			{ "0011",		 4 },	{ "0000010010",		21 },
			{ "0010",		 5 },	{ "00000100011",	22 },
			{ "00011",		 6 },	{ "00000100010",	23 },
			{ "00010",		 7 },	{ "00000100001",	24 },
			{ "0000111",	 8 },	{ "00000100000",	25 },
			{ "0000110",	 9 },	{ "00000011111",	26 },
			{ "00001011",	10 },	{ "00000011110",	27 },
			{ "00001010",	11 },	{ "00000011101",	28 },
			{ "00001001",	12 },	{ "00000011100",	29 },
			{ "00001000",	13 },	{ "00000011011",	30 },
			{ "00000111",	14 },	{ "00000011010",	31 },
			{ "00000110",	15 },	{ "00000011001",	32 },
			{ "0000010111",	16 },	{ "00000011000",	33 },
			{ "0000010110",	17 },	{ "00000001000",	MacroblockEscape },
									{ "00000001111",	MacroblockStuffing },
			// D.9.2 Macroblock stuffing
			// MPEG-1 – The VLC code '0000 0001 111' (macroblock_stuffing) can be inserted any number of times before each
			// macroblock_address_increment. This code must be discarded by the decoder. This is described in 2.4.2.7 of MPEG-1.

		}, InvalidInteger);

		/// <summary>
		/// Table B.2 – Variable length codes for macroblock_type in I-pictures
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB2IPictures = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "1",		MacroblockMode.Intra },
			{ "01",		MacroblockMode.Intra | MacroblockMode.Quant }
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.3 – Variable length codes for macroblock_type in P-pictures
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB3PPictures = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "1",		MacroblockMode.MotionForward | MacroblockMode.Pattern },
			{ "01",		MacroblockMode.Pattern },
			{ "001",	MacroblockMode.MotionForward },
			{ "00011",	MacroblockMode.Intra },
			{ "00010",	MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.Quant },
			{ "00001",	MacroblockMode.Pattern | MacroblockMode.Quant },
			{ "000001",	MacroblockMode.Intra | MacroblockMode.Quant }
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.4 – Variable length codes for macroblock_type in B-pictures
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB4BPictures = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "10",		MacroblockMode.MotionForward | MacroblockMode.MotionBackward },
			{ "11",		MacroblockMode.MotionForward | MacroblockMode.MotionBackward | MacroblockMode.Pattern },
			{ "010",	MacroblockMode.MotionBackward },
			{ "011",	MacroblockMode.MotionBackward | MacroblockMode.Pattern },
			{ "0010",	MacroblockMode.MotionForward },
			{ "0011",	MacroblockMode.MotionForward | MacroblockMode.Pattern },
			{ "00011",	MacroblockMode.Intra },
			{ "00010",	MacroblockMode.MotionForward | MacroblockMode.MotionBackward | MacroblockMode.Pattern | MacroblockMode.Quant },
			{ "000011",	MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.Quant },
			{ "000010",	MacroblockMode.MotionBackward | MacroblockMode.Pattern | MacroblockMode.Quant },
			{ "000001",	MacroblockMode.Intra | MacroblockMode.Quant }
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.5 – Variable length codes for macroblock_type in I-pictures with spatial scalability
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB5IPicturesWithSpatialScalability = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "1",		MacroblockMode.Pattern},
			{ "01",		MacroblockMode.Quant | MacroblockMode.Pattern},
			{ "0011",	MacroblockMode.Intra },
			{ "0010",	MacroblockMode.Quant| MacroblockMode.Intra },
			{ "0001",	MacroblockMode.NoFlags },
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.6 – Variable length codes for macroblock_type in P-pictures with spatial scalability
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB6PPicturesWithSpatialScalability = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "10",		MacroblockMode.MotionForward | MacroblockMode.Pattern },
			{ "011",	MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "0000100",MacroblockMode.Pattern },
			{ "000111",	MacroblockMode.Pattern| MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "0010",	MacroblockMode.MotionForward },
			{ "0000111",MacroblockMode.Intra },
			{ "0011",	MacroblockMode.MotionForward | MacroblockMode.SpatialTemporalWeightCodeFlag},
			{ "010",	MacroblockMode.Quant | MacroblockMode.MotionForward | MacroblockMode.Pattern },
			{ "000100",	MacroblockMode.Quant | MacroblockMode.Pattern },
			{ "0000110",MacroblockMode.Quant | MacroblockMode.Intra },
			{ "11",		MacroblockMode.Quant | MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000101",	MacroblockMode.Quant | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000110",	MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "0000101",MacroblockMode.Pattern },
			{ "0000010",MacroblockMode.Quant | MacroblockMode.Pattern },
			{ "0000011",MacroblockMode.NoFlags },
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.7 – Variable length codes for macroblock_type in B-pictures with spatial scalability
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB7BPicturesWithSpatialScalability = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "10",			MacroblockMode.MotionForward | MacroblockMode.MotionBackward },
			{ "11",			MacroblockMode.MotionForward | MacroblockMode.MotionBackward | MacroblockMode.Pattern },
			{ "010",		MacroblockMode.MotionBackward },
			{ "011",		MacroblockMode.MotionBackward | MacroblockMode.Intra },
			{ "0010",		MacroblockMode.MotionForward },
			{ "0011",		MacroblockMode.MotionForward | MacroblockMode.Pattern },
			{ "000110",		MacroblockMode.MotionBackward | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000111",		MacroblockMode.MotionBackward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000100",		MacroblockMode.MotionForward | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000101",		MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag},
			{ "0000110",	MacroblockMode.Intra },
			{ "0000111",	MacroblockMode.Quant | MacroblockMode.MotionForward | MacroblockMode.MotionBackward | MacroblockMode.Pattern },
			{ "0000100",	MacroblockMode.Quant | MacroblockMode.MotionForward | MacroblockMode.Pattern},
			{ "0000101",	MacroblockMode.Quant | MacroblockMode.MotionBackward | MacroblockMode.Pattern },
			{ "00000100",	MacroblockMode.Quant | MacroblockMode.Intra },
			{ "00000101",	MacroblockMode.Quant | MacroblockMode.MotionForward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000001100",	MacroblockMode.Quant | MacroblockMode.MotionBackward | MacroblockMode.Pattern | MacroblockMode.SpatialTemporalWeightCodeFlag },
			{ "000001110",	MacroblockMode.NoFlags },
			{ "000001101",	MacroblockMode.Quant | MacroblockMode.Pattern },
			{ "000001111",	MacroblockMode.Pattern },
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Table B.8 – Variable length codes for macroblock_type in I-pictures, P-pictures and
		/// B-pictures with SNR scalability
		/// </summary>
		private static readonly VlcTable<MacroblockMode> TableB8PicturesWithSnrScalability = new VlcTable<MacroblockMode>(new object[,]
		{
			{ "1",	MacroblockMode.Pattern },
			{ "01",	MacroblockMode.Quant | MacroblockMode.Pattern},
			{ "001",MacroblockMode.NoFlags },
		}, MacroblockMode.Invalid);

		/// <summary>
		/// Macroblock pattern
		/// Table B.9 – Variable length codes for coded_block_pattern
		/// </summary>
		private static readonly VlcTable<int> TableB9CodedBlockPatterns = new VlcTable<int>(new object[,]
		{
			{ "111" , 60 },		{ "00011100", 35 },
			{ "1101", 4 },		{ "00011011", 13 },
			{ "1100", 8 },		{ "00011010", 49 },
			{ "1011", 16 },		{ "00011001", 21 },
			{ "1010", 32 },		{ "00011000", 41 },
			{ "10011", 12 },	{ "00010111", 14 },
			{ "10010", 48 },	{ "00010110", 50 },
			{ "10001", 20 },	{ "00010101", 22 },
			{ "10000", 40 },	{ "00010100", 42 },
			{ "01111", 28 },	{ "00010011", 15 },
			{ "01110", 44 },	{ "00010010", 51 },
			{ "01101", 52 },	{ "00010001", 23 },
			{ "01100", 56 },	{ "00010000", 43 },
			{ "01011", 1 },		{ "00001111", 25 },
			{ "01010", 61 },	{ "00001110", 37 },
			{ "01001", 2 },		{ "00001101", 26 },
			{ "01000", 62 },	{ "00001100", 38 },
			{ "001111", 24 },	{ "00001011", 29 },
			{ "001110", 36 },	{ "00001010", 45 },
			{ "001101", 3 },	{ "00001001", 53 },
			{ "001100", 63 },	{ "00001000", 57 },
			{ "0010111", 5 },	{ "00000111", 30 },
			{ "0010110", 9 },	{ "00000110", 46 },
			{ "0010101", 17 },	{ "00000101", 54 },
			{ "0010100", 33 },	{ "00000100", 58 },
			{ "0010011", 6 },	{ "000000111", 31 },
			{ "0010010", 10 },	{ "000000110", 47 },
			{ "0010001", 18 },	{ "000000101", 55 },
			{ "0010000", 34 },	{ "000000100", 59 },
			{ "00011111", 7 },	{ "000000011", 27 },
			{ "00011110", 11 },	{ "000000010", 39 },
			{ "00011101", 19 },	{ "000000001", 0 }	// (Note)
			// NOTE – This entry shall not be used with 4:2:0 chrominance structure.
		}, InvalidInteger);

		/// <summary>
		/// Motion vectors
		/// Table B.10 – Variable length codes for motion_code
		/// </summary>
		private static readonly VlcTable<int> TableB10MotionCode = new VlcTable<int>(new object[,]
		{
			{ "00000011001", -16 },
			{ "00000011011", -15 },
			{ "00000011101", -14 },
			{ "00000011111", -13 },
			{ "00000100001", -12 },
			{ "00000100011", -11 },
			{ "0000010011", -10 },
			{ "0000010101", -9 },
			{ "0000010111", -8 },
			{ "00000111", -7 },
			{ "00001001", -6 },
			{ "00001011", -5 },
			{ "0000111", -4 },
			{ "00011", -3 },
			{ "0011", -2 },
			{ "011", -1 },
			{ "1", 0 },
			{ "010", 1 },
			{ "0010", 2 },
			{ "00010", 3 },
			{ "0000110", 4 },
			{ "00001010", 5 },
			{ "00001000", 6 },
			{ "00000110", 7 },
			{ "0000010110", 8 },
			{ "0000010100", 9 },
			{ "0000010010", 10 },
			{ "00000100010", 11 },
			{ "00000100000", 12 },
			{ "00000011110", 13 },
			{ "00000011100", 14 },
			{ "00000011010", 15 },
			{ "00000011000", 16 }
		}, InvalidInteger);

		/// <summary>
		/// Motion vectors
		/// Table B.11 – Variable length codes for dmvector[t]
		/// </summary>
		private static readonly VlcTable<int> TableB11DmVector = new VlcTable<int>(new object[,]
		{
			{ "11", -1 },
			{ "0", 0 },
			{ "10", 1 }
		}, InvalidInteger);

		/// <summary>
		/// DCT coefficients
		/// Table B.12 – Variable length codes for dct_dc_size_luminance
		/// </summary>
		private static readonly VlcTable<int> TableB12DctDcSizeLuminance = new VlcTable<int>(new object[,]
		{
			{ "100", 0 },
			{ "00", 1 },
			{ "01", 2 },
			{ "101", 3 },
			{ "110", 4 },
			{ "1110", 5 },
			{ "11110", 6 },
			{ "111110", 7 },
			{ "1111110", 8 },
			{ "11111110", 9 },
			{ "111111110", 10 },
			{ "111111111", 11 }
		}, InvalidInteger);

		/// <summary>
		/// DCT coefficients
		/// Table B.13 – Variable length codes for dct_dc_size_chrominance
		/// </summary>
		private static readonly VlcTable<int> TableB13DctDcSizeChrominance = new VlcTable<int>(new object[,]
		{
			{ "00", 0 },
			{ "01", 1 },
			{ "10", 2 },
			{ "110", 3 },
			{ "1110", 4 },
			{ "11110", 5 },
			{ "111110", 6 },
			{ "1111110", 7 },
			{ "11111110", 8 },
			{ "111111110", 9 },
			{ "1111111110", 10 },
			{ "1111111111", 11 }
		}, InvalidInteger);

		/// <summary>
		/// DCT coefficients
		/// Table B.14 (first) – DCT coefficients Table zero
		/// </summary>
		/// <remarks>
		/// NOTE 1 – The last bit 's' denotes the sign of the level: '0' for positive, '1' for negative.
		/// NOTE 2 – "End of Block" shall not be the only code of the block.
		/// NOTE 3 – This code shall be used for the first (DC) coefficient in the block.
		/// NOTE 4 – This code shall be used for all other coefficients.
		/// </remarks>
		private static readonly VlcTable<int[]> TableB14FirstDctCoefficientsTableZero = new VlcTable<int[]>(new object[,]
		{
			{ "1", new [] {0, 1}},		// (Note 3)
			{ "011", new []{1, 1}},
			{ "0100", new [] { 0, 2}},
			{ "0101", new [] { 2, 1}},
			{ "00101", new [] { 0, 3}},
			{ "00111", new [] { 3, 1}},
			{ "00110", new [] { 4, 1}},
			{ "000110", new [] { 1, 2}},
			{ "000111", new [] { 5, 1}},
			{ "000101", new [] { 6, 1}},
			{ "000100", new [] { 7, 1}},
			{ "0000110", new [] { 0, 4}},
			{ "0000100", new [] { 2, 2}},
			{ "0000111", new [] { 8, 1}},
			{ "0000101", new [] { 9, 1}},
			{ "000001", Escape},	// Escape (not followed by sign bit)
			{ "00100110", new [] { 0, 5}},
			{ "00100001", new [] { 0, 6}},
			{ "00100101", new [] { 1, 3}},
			{ "00100100", new [] { 3, 2}},
			{ "00100111", new [] { 10, 1}},
			{ "00100011", new [] { 11, 1}},
			{ "00100010", new [] { 12, 1}},
			{ "00100000", new [] { 13, 1}},
			{ "0000001010", new [] { 0, 7}},
			{ "0000001100", new [] { 1, 4}},
			{ "0000001011", new [] { 2, 3}},
			{ "0000001111", new [] { 4, 2}},
			{ "0000001001", new [] { 5, 2}},
			{ "0000001110", new [] { 14, 1}},
			{ "0000001101", new [] { 15, 1}},
			{ "0000001000", new [] { 16, 1}},

			{ "000000011101", new [] { 0, 8}},
			{ "000000011000", new [] { 0, 9}},
			{ "000000010011", new [] { 0, 10}},
			{ "000000010000", new [] { 0, 11}},
			{ "000000011011", new [] { 1, 5}},
			{ "000000010100", new [] { 2, 4}},
			{ "000000011100", new [] { 3, 3}},
			{ "000000010010", new [] { 4, 3}},
			{ "000000011110", new [] { 6, 2}},
			{ "000000010101", new [] { 7, 2}},
			{ "000000010001", new [] { 8, 2}},
			{ "000000011111", new [] { 17, 1}},
			{ "000000011010", new [] { 18, 1}},
			{ "000000011001", new [] { 19, 1}},
			{ "000000010111", new [] { 20, 1}},
			{ "000000010110", new [] { 21, 1}},
			{ "0000000011010", new [] { 0, 12}},
			{ "0000000011001", new [] { 0, 13}},
			{ "0000000011000", new [] { 0, 14}},
			{ "0000000010111", new [] { 0, 15}},
			{ "0000000010110", new [] { 1, 6}},
			{ "0000000010101", new [] { 1, 7}},
			{ "0000000010100", new [] { 2, 5}},
			{ "0000000010011", new [] { 3, 4}},
			{ "0000000010010", new [] { 5, 3}},
			{ "0000000010001", new [] { 9, 2}},
			{ "0000000010000", new [] { 10, 2}},
			{ "0000000011111", new [] { 22, 1}},
			{ "0000000011110", new [] { 23, 1}},
			{ "0000000011101", new [] { 24, 1}},
			{ "0000000011100", new [] { 25, 1}},
			{ "0000000011011", new [] { 26, 1}},

			{ "00000000011111", new[] { 0, 16}},
			{ "00000000011110", new[] { 0, 17}},
			{ "00000000011101", new[] { 0, 18}},
			{ "00000000011100", new[] { 0, 19}},
			{ "00000000011011", new[] { 0, 20}},
			{ "00000000011010", new[] { 0, 21}},
			{ "00000000011001", new[] { 0, 22}},
			{ "00000000011000", new[] { 0, 23}},
			{ "00000000010111", new[] { 0, 24}},
			{ "00000000010110", new[] { 0, 25}},
			{ "00000000010101", new[] { 0, 26}},
			{ "00000000010100", new[] { 0, 27}},
			{ "00000000010011", new[] { 0, 28}},
			{ "00000000010010", new[] { 0, 29}},
			{ "00000000010001", new[] { 0, 30}},
			{ "00000000010000", new[] { 0, 31}},
			{ "000000000011000", new[] { 0, 32}},
			{ "000000000010111", new[] { 0, 33}},
			{ "000000000010110", new[] { 0, 34}},
			{ "000000000010101", new[] { 0, 35}},
			{ "000000000010100", new[] { 0, 36}},
			{ "000000000010011", new[] { 0, 37}},
			{ "000000000010010", new[] { 0, 38}},
			{ "000000000010001", new[] { 0, 39}},
			{ "000000000010000", new[] { 0, 40}},
			{ "000000000011111", new[] { 1, 8}},
			{ "000000000011110", new[] { 1, 9}},
			{ "000000000011101", new[] { 1, 10}},
			{ "000000000011100", new[] { 1, 11}},
			{ "000000000011011", new[] { 1, 12}},
			{ "000000000011010", new[] { 1, 13}},
			{ "000000000011001", new[] { 1, 14}},

			{ "0000000000010011", new[] { 1, 15}},
			{ "0000000000010010", new[] { 1, 16}},
			{ "0000000000010001", new[] { 1, 17}},
			{ "0000000000010000", new[] { 1, 18}},
			{ "0000000000010100", new[] { 6, 3}},
			{ "0000000000011010", new[] { 11, 2}},
			{ "0000000000011001", new[] { 12, 2}},
			{ "0000000000011000", new[] { 13, 2}},
			{ "0000000000010111", new[] { 14, 2}},
			{ "0000000000010110", new[] { 15, 2}},
			{ "0000000000010101", new[] { 16, 2}},
			{ "0000000000011111", new[] { 27, 1}},
			{ "0000000000011110", new[] { 28, 1}},
			{ "0000000000011101", new[] { 29, 1}},
			{ "0000000000011100", new[] { 30, 1}},
			{ "0000000000011011", new[] { 31, 1}}
		}, Invalid);

		/// <summary>
		/// DCT coefficients
		/// Table B.14 (subsequent) – DCT coefficients Table zero
		/// </summary>
		/// <remarks>
		/// NOTE 1 – The last bit 's' denotes the sign of the level: '0' for positive, '1' for negative.
		/// NOTE 2 – "End of Block" shall not be the only code of the block.
		/// NOTE 3 – This code shall be used for the first (DC) coefficient in the block.
		/// NOTE 4 – This code shall be used for all other coefficients.
		/// </remarks>
		private static readonly VlcTable<int[]> TableB14DctCoefficientsTableZero = new VlcTable<int[]>(new object[,]
		{
			{ "10", EndOfBlock },	// (Note 2) End of Block (not followed by sign bit)
			{ "11", new [] {0, 1}},		// (Note 4)
			{ "011", new []{1, 1}},
			{ "0100", new [] { 0, 2}},
			{ "0101", new [] { 2, 1}},
			{ "00101", new [] { 0, 3}},
			{ "00111", new [] { 3, 1}},
			{ "00110", new [] { 4, 1}},
			{ "000110", new [] { 1, 2}},
			{ "000111", new [] { 5, 1}},
			{ "000101", new [] { 6, 1}},
			{ "000100", new [] { 7, 1}},
			{ "0000110", new [] { 0, 4}},
			{ "0000100", new [] { 2, 2}},
			{ "0000111", new [] { 8, 1}},
			{ "0000101", new [] { 9, 1}},
			{ "000001", Escape},	// Escape (not followed by sign bit)
			{ "00100110", new [] { 0, 5}},
			{ "00100001", new [] { 0, 6}},
			{ "00100101", new [] { 1, 3}},
			{ "00100100", new [] { 3, 2}},
			{ "00100111", new [] { 10, 1}},
			{ "00100011", new [] { 11, 1}},
			{ "00100010", new [] { 12, 1}},
			{ "00100000", new [] { 13, 1}},
			{ "0000001010", new [] { 0, 7}},
			{ "0000001100", new [] { 1, 4}},
			{ "0000001011", new [] { 2, 3}},
			{ "0000001111", new [] { 4, 2}},
			{ "0000001001", new [] { 5, 2}},
			{ "0000001110", new [] { 14, 1}},
			{ "0000001101", new [] { 15, 1}},
			{ "0000001000", new [] { 16, 1}},

			{ "000000011101", new [] { 0, 8}},
			{ "000000011000", new [] { 0, 9}},
			{ "000000010011", new [] { 0, 10}},
			{ "000000010000", new [] { 0, 11}},
			{ "000000011011", new [] { 1, 5}},
			{ "000000010100", new [] { 2, 4}},
			{ "000000011100", new [] { 3, 3}},
			{ "000000010010", new [] { 4, 3}},
			{ "000000011110", new [] { 6, 2}},
			{ "000000010101", new [] { 7, 2}},
			{ "000000010001", new [] { 8, 2}},
			{ "000000011111", new [] { 17, 1}},
			{ "000000011010", new [] { 18, 1}},
			{ "000000011001", new [] { 19, 1}},
			{ "000000010111", new [] { 20, 1}},
			{ "000000010110", new [] { 21, 1}},
			{ "0000000011010", new [] { 0, 12}},
			{ "0000000011001", new [] { 0, 13}},
			{ "0000000011000", new [] { 0, 14}},
			{ "0000000010111", new [] { 0, 15}},
			{ "0000000010110", new [] { 1, 6}},
			{ "0000000010101", new [] { 1, 7}},
			{ "0000000010100", new [] { 2, 5}},
			{ "0000000010011", new [] { 3, 4}},
			{ "0000000010010", new [] { 5, 3}},
			{ "0000000010001", new [] { 9, 2}},
			{ "0000000010000", new [] { 10, 2}},
			{ "0000000011111", new [] { 22, 1}},
			{ "0000000011110", new [] { 23, 1}},
			{ "0000000011101", new [] { 24, 1}},
			{ "0000000011100", new [] { 25, 1}},
			{ "0000000011011", new [] { 26, 1}},

			{ "00000000011111", new[] { 0, 16}},
			{ "00000000011110", new[] { 0, 17}},
			{ "00000000011101", new[] { 0, 18}},
			{ "00000000011100", new[] { 0, 19}},
			{ "00000000011011", new[] { 0, 20}},
			{ "00000000011010", new[] { 0, 21}},
			{ "00000000011001", new[] { 0, 22}},
			{ "00000000011000", new[] { 0, 23}},
			{ "00000000010111", new[] { 0, 24}},
			{ "00000000010110", new[] { 0, 25}},
			{ "00000000010101", new[] { 0, 26}},
			{ "00000000010100", new[] { 0, 27}},
			{ "00000000010011", new[] { 0, 28}},
			{ "00000000010010", new[] { 0, 29}},
			{ "00000000010001", new[] { 0, 30}},
			{ "00000000010000", new[] { 0, 31}},
			{ "000000000011000", new[] { 0, 32}},
			{ "000000000010111", new[] { 0, 33}},
			{ "000000000010110", new[] { 0, 34}},
			{ "000000000010101", new[] { 0, 35}},
			{ "000000000010100", new[] { 0, 36}},
			{ "000000000010011", new[] { 0, 37}},
			{ "000000000010010", new[] { 0, 38}},
			{ "000000000010001", new[] { 0, 39}},
			{ "000000000010000", new[] { 0, 40}},
			{ "000000000011111", new[] { 1, 8}},
			{ "000000000011110", new[] { 1, 9}},
			{ "000000000011101", new[] { 1, 10}},
			{ "000000000011100", new[] { 1, 11}},
			{ "000000000011011", new[] { 1, 12}},
			{ "000000000011010", new[] { 1, 13}},
			{ "000000000011001", new[] { 1, 14}},

			{ "0000000000010011", new[] { 1, 15}},
			{ "0000000000010010", new[] { 1, 16}},
			{ "0000000000010001", new[] { 1, 17}},
			{ "0000000000010000", new[] { 1, 18}},
			{ "0000000000010100", new[] { 6, 3}},
			{ "0000000000011010", new[] { 11, 2}},
			{ "0000000000011001", new[] { 12, 2}},
			{ "0000000000011000", new[] { 13, 2}},
			{ "0000000000010111", new[] { 14, 2}},
			{ "0000000000010110", new[] { 15, 2}},
			{ "0000000000010101", new[] { 16, 2}},
			{ "0000000000011111", new[] { 27, 1}},
			{ "0000000000011110", new[] { 28, 1}},
			{ "0000000000011101", new[] { 29, 1}},
			{ "0000000000011100", new[] { 30, 1}},
			{ "0000000000011011", new[] { 31, 1}}
		}, Invalid);

		/// <summary>
		/// DCT coefficients
		/// Table B.15 – DCT coefficients Table one
		/// </summary>
		/// <remarks>
		/// Note 1 –The last bit 's' denotes the sign of the level: '0' for positive, '1' for negative.
		/// Note 2 – "End of Block" shall not be the only code of the block.</remarks>
		private static readonly VlcTable<int[]> TableB15DctCoefficientsTableOne = new VlcTable<int[]>(new object[,]
		{
			{ "0110", EndOfBlock},	// (Note 2) End of Block (not followed by sign bit)
			{ "10", new [] {0, 1}},
			{ "010", new [] { 1, 1}},
			{ "110", new [] { 0, 2}},
			{ "00101", new [] { 2, 1}},
			{ "0111", new [] { 0, 3}},
			{ "00111", new [] { 3, 1}},
			{ "000110", new [] { 4, 1}},
			{ "00110", new [] { 1, 2}},
			{ "000111", new [] { 5, 1}},
			{ "0000110", new [] { 6, 1}},
			{ "0000100", new [] { 7, 1}},
			{ "11100", new [] { 0, 4}},
			{ "0000111", new [] { 2, 2}},
			{ "0000101", new [] { 8, 1}},
			{ "1111000", new [] { 9, 1}},
			{ "000001", Escape},	// Escape (not followed by sign bit)
			{ "11101", new [] { 0, 5}},
			{ "000101", new [] { 0, 6}},
			{ "1111001", new [] { 1, 3}},
			{ "00100110", new [] { 3, 2}},
			{ "1111010", new [] { 10, 1}},
			{ "00100001", new [] { 11, 1}},
			{ "00100101", new [] { 12, 1}},
			{ "00100100", new [] { 13, 1}},
			{ "000100", new [] { 0, 7}},
			{ "00100111", new [] { 1, 4}},
			{ "11111100", new [] { 2, 3}},
			{ "11111101", new [] { 4, 2}},
			{ "000000100", new [] { 5, 2}},
			{ "000000101", new [] { 14, 1}},
			{ "000000111", new [] { 15, 1}},
			{ "0000001101", new [] { 16, 1}},

			{ "1111011", new [] { 0, 8}},
			{ "1111100", new [] { 0, 9}},
			{ "00100011", new [] { 0, 10}},
			{ "00100010", new [] { 0, 11}},
			{ "00100000", new [] { 1, 5}},
			{ "0000001100", new [] { 2, 4}},
			{ "000000011100", new [] { 3, 3}},
			{ "000000010010", new [] { 4, 3}},
			{ "000000011110", new [] { 6, 2}},
			{ "000000010101", new [] { 7, 2}},
			{ "000000010001", new [] { 8, 2}},
			{ "000000011111", new [] { 17, 1}},
			{ "000000011010", new [] { 18, 1}},
			{ "000000011001", new [] { 19, 1}},
			{ "000000010111", new [] { 20, 1}},
			{ "000000010110", new [] { 21, 1}},
			{ "11111010", new [] { 0, 12}},
			{ "11111011", new [] { 0, 13}},
			{ "11111110", new [] { 0, 14}},
			{ "11111111", new [] { 0, 15}},
			{ "0000000010110", new [] { 1, 6}},
			{ "0000000010101", new [] { 1, 7}},
			{ "0000000010100", new [] { 2, 5}},
			{ "0000000010011", new [] { 3, 4}},
			{ "0000000010010", new [] { 5, 3}},
			{ "0000000010001", new [] { 9, 2}},
			{ "0000000010000", new [] { 10, 2}},
			{ "0000000011111", new [] { 22, 1}},
			{ "0000000011110", new [] { 23, 1}},
			{ "0000000011101", new [] { 24, 1}},
			{ "0000000011100", new [] { 25, 1}},
			{ "0000000011011", new [] { 26, 1}},

			{ "00000000011111", new [] { 0, 16}},
			{ "00000000011110", new [] { 0, 17}},
			{ "00000000011101", new [] { 0, 18}},
			{ "00000000011100", new [] { 0, 19}},
			{ "00000000011011", new [] { 0, 20}},
			{ "00000000011010", new [] { 0, 21}},
			{ "00000000011001", new [] { 0, 22}},
			{ "00000000011000", new [] { 0, 23}},
			{ "00000000010111", new [] { 0, 24}},
			{ "00000000010110", new [] { 0, 25}},
			{ "00000000010101", new [] { 0, 26}},
			{ "00000000010100", new [] { 0, 27}},
			{ "00000000010011", new [] { 0, 28}},
			{ "00000000010010", new [] { 0, 29}},
			{ "00000000010001", new [] { 0, 30}},
			{ "00000000010000", new [] { 0, 31}},
			{ "000000000011000", new [] { 0, 32}},
			{ "000000000010111", new [] { 0, 33}},
			{ "000000000010110", new [] { 0, 34}},
			{ "000000000010101", new [] { 0, 35}},
			{ "000000000010100", new [] { 0, 36}},
			{ "000000000010011", new [] { 0, 37}},
			{ "000000000010010", new [] { 0, 38}},
			{ "000000000010001", new [] { 0, 39}},
			{ "000000000010000", new [] { 0, 40}},
			{ "000000000011111", new [] { 1, 8}},
			{ "000000000011110", new [] { 1, 9}},
			{ "000000000011101", new [] { 1, 10}},
			{ "000000000011100", new [] { 1, 11}},
			{ "000000000011011", new [] { 1, 12}},
			{ "000000000011010", new [] { 1, 13}},
			{ "000000000011001", new [] { 1, 14}},

			{ "0000000000010011", new [] { 1, 15}},
			{ "0000000000010010", new [] { 1, 16}},
			{ "0000000000010001", new [] { 1, 17}},
			{ "0000000000010000", new [] { 1, 18}},
			{ "0000000000010100", new [] { 6, 3}},
			{ "0000000000011010", new [] { 11, 2}},
			{ "0000000000011001", new [] { 12, 2}},
			{ "0000000000011000", new [] { 13, 2}},
			{ "0000000000010111", new [] { 14, 2}},
			{ "0000000000010110", new [] { 15, 2}},
			{ "0000000000010101", new [] { 16, 2}},
			{ "0000000000011111", new [] { 27, 1}},
			{ "0000000000011110", new [] { 28, 1}},
			{ "0000000000011101", new [] { 29, 1}},
			{ "0000000000011100", new [] { 30, 1}},
			{ "0000000000011011", new [] { 31, 1}}
		}, Invalid);

		// DCT coefficients
		// Table B.16 – Encoding of run and level following an ESCAPE code
		#endregion VLC Tables

		#region Properties
		public uint StartCode { get { return 0x101/*... 0x1af*/; } }
		#endregion Properties

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			IMpeg2VideoState state = reader.State;

			// Slices can occur after other slices, picture headers, extensions or user data
			string lastHeaderName = state.LastHeaderName;
			if (!state.Picture.Initialized && (lastHeaderName != Name) && (lastHeaderName != null))
			{
				resultState.Invalidate();
				return;
			}
			if (!state.Sequence.Initialized && state.Picture.Initialized)
			{
				if (reader.TryDefaultHeaders(resultState, () => ParseSlice(reader, resultState)))
				{
					return; // Slice successfully decoded!
				}
			}

			ParseSlice(reader, resultState);
		}

		private static bool GetDmv(bool macroblockIntra, IMpeg2VideoState state)
		{
			// If frame_pred_frame_dct is equal to 1 then frame_motion_type
			// is omitted from the bitstream. In this case motion vector
			// decoding and prediction formation shall be performed as if
			// frame_motion_type had indicated "Frame-based prediction".
			// TODO: expression 'state.FramePredFrameDct || (macroblockIntra && state.ConcealmentMotionVectors)' is also used in 'GetMvFormat()' (extract to method)
			if (state.Picture.FramePredFrameDct || (macroblockIntra && state.Picture.ConcealmentMotionVectors))
			{
				return false;
			}

			return state.Slice.FrameMotionType == 3 || state.Slice.FieldMotionType == 3;
		}

		private static MotionVectorFormat GetMvFormat(IMpeg2VideoReader reader, IState resultState, bool macroblockIntra)
		{
			// If frame_pred_frame_dct is equal to 1 then frame_motion_type
			// is omitted from the bitstream. In this case motion vector
			// decoding and prediction formation shall be performed as if
			// frame_motion_type had indicated "Frame-based prediction".
			if (reader.State.Picture.FramePredFrameDct || (macroblockIntra && reader.State.Picture.ConcealmentMotionVectors))
			{
				return MotionVectorFormat.Frame;
			}

			// Frame
			switch (reader.State.Slice.FrameMotionType)
			{
				case 1: return MotionVectorFormat.Field;
				case 2: return MotionVectorFormat.Frame;
				case 3: return MotionVectorFormat.Field;
			}

			// Field
			if (reader.State.Slice.FieldMotionType != 0)
			{
				Debug.Assert(reader.State.Slice.FieldMotionType <= 3);
				return MotionVectorFormat.Field;
			}
			resultState.Invalidate();
			return MotionVectorFormat.Unknown;
		}

		/// <summary>
		/// motion_vector_count is derived from field_motion_type or
		/// frame_motion_type as indicated in Tables 6-17 and 6-18.
		/// </summary>
		/// <returns></returns>
		private static int GetMotionVectorCount(IMpeg2VideoState state, IState resultState, bool macroblockIntra)
		{
			// If frame_pred_frame_dct is equal to 1 then frame_motion_type
			// is omitted from the bitstream. In this case motion vector
			// decoding and prediction formation shall be performed as if
			// frame_motion_type had indicated "Frame-based prediction".
			if (state.Picture.FramePredFrameDct || (macroblockIntra && state.Picture.ConcealmentMotionVectors))
			{
				return 1;
			}

			// Frame
			switch (state.Slice.FrameMotionType)
			{
				case 1: return 2;
				case 2:
				case 3: return 1;
			}

			// Field
			switch (state.Slice.FieldMotionType)
			{
				case 1:
				case 3: return 1;
				case 2: return 2;
			}

			resultState.Invalidate();
			return 0;
		}

		private void ParseSlice(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			// TODO: support pictures that cannot be parsed (missing sequence header)

			// slice_vertical_position – This is given by the last eight bits
			// of the slice_start_code. It is an unsigned integer giving the
			// vertical position in macroblock units of the first macroblock in
			// the slice.
			ISliceState sliceState = reader.State.Slice;
			uint previousSliceVerticalPosition = sliceState.SliceVerticalPosition;
			sliceState.SliceVerticalPosition = (byte)(reader.State.StartCode & 0xFF);

			// slice_start_code – The slice_start_code is a string of 32-bits.
			// The first 24-bits have the value 000001 in hexadecimal and
			// the last 8-bits are the slice_vertical_position having a value
			// in the range 01 through AF hexadecimal inclusive.
			reader.AddDerivedAttribute(Attribute.SliceVerticalPosition, sliceState.SliceVerticalPosition);

			long length;

			// Note: because of the next statement nowhere is the rest of the code SequenceHeader and PictureHeader are checked against null.
			if (!reader.State.Sequence.Initialized || !reader.State.Picture.Initialized)
			{
				reader.NextStartCode();

				// Limit size of slice
				length = 0; // FIXME: resultState.ResultLength; // FIXME: 'ResultLength' doesn't work!!
				if (length > reader.State.Configuration.SliceMaxLength)
				{
					// Do not add this slice to the results.
					// The slice content could not be validated and its bigger
					// than sliceMaxLength which make it unlikely its
					// a valid slice.
					resultState.Invalidate();
					return;
				}
				if (reader.State.LastHeaderName == Name)
				{
					// Note-1: To reduce false hits, we accept that this may fail to detect slices from videos with height > 2800 !!
					// Note-2: This may also fail to detect (small fragments of) CDi movies that consist of 2 slices for the entire frame.
					// FIXME: this does not obey the configuration setting 'MaximumSliceNumberIncrement'
					if (sliceState.SliceVerticalPosition != (previousSliceVerticalPosition + 1))
					{
						resultState.Invalidate();
						return;
					}
				}

				// Add this slice to the results.
				// Although its content could not validated,
				// its smaller than sliceMaxLength.
				resultState.Invalidate();
				resultState.Recover();
				return;
			}

			GetSliceVerticalPositionExtension(reader, resultState, (byte)sliceState.SliceVerticalPosition);
			if (!resultState.Valid)
			{
				return;
			}

			GetPriorityBreakpoint(reader);

			reader.GetBits(5, Attribute.QuantiserScaleCode);

			// Optional slice extension
			if (reader.ShowBits(1) == 1)
			{
				reader.GetFlag(Attribute.SliceExtensionFlag);
				reader.GetFlag(Attribute.IntraSlice);
				reader.GetFlag(Attribute.SlicePictureIdEnable);
				reader.GetBits(6, Attribute.SlicePictureId);

				while (reader.ShowBits(1) == 1)
				{
					reader.GetFlag(Attribute.ExtraBitSlice);
					reader.GetBits(8, Attribute.ExtraInformationSlice);
				}
			}

			// Note: The previous block guarantees that the next bit is '0'
			reader.GetFlag(Attribute.ExtraBitSlice);

			do
			{
				GetMacroBlock(reader, resultState);
			}
			while ((reader.ShowBits(23) != 0) && resultState.Valid);

			reader.NextStartCode();

			// Limit size of slice
			//FIXME: if (resultState.ResultLength > _sliceMaxLength)
			{
				//    resultState.Invalidate();
			}
		}

		private void GetMacroBlock(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			IPictureState pictureState = reader.State.Picture;
			while (reader.ShowBits(11) == 0x08)// nextbits() == '0000 0001 000' )
			{
				// macroblock_escape (11 bits)
				reader.GetBits(11);
			}

			// macroblock_address_increment (1..11 bits)
			int macroblockAddressIncrement = GetMacroblockAddressIncrement(reader, resultState);
			if (macroblockAddressIncrement == InvalidInteger)
			{
				resultState.Invalidate();
				return;
			}

			// macroblock_modes()
			MacroblockMode macroblockType = GetMacroblockModes(reader, resultState, reader.State.Picture.CodingType.Value);

			// if ( macroblock_quant )
			if (IsFlagSet(macroblockType, MacroblockMode.Quant))
			{
				// quantiser_scale_code (5 bits)
				reader.GetBits(5);
			}
			//PictureCodingExtension pictureCodingExtension = pictureHeader.PictureCodingExtension;
			if (IsFlagSet(macroblockType, MacroblockMode.MotionForward) ||						// macroblock_motion_forward ||
			    (IsFlagSet(macroblockType, MacroblockMode.Intra) && reader.State.Picture.ConcealmentMotionVectors))	// (macroblock_intra && concealment_motion_vectors)
			{
				GetMotionVectors(reader, resultState, pictureState.ForwardHorizontalFCode, pictureState.ForwardVerticalFCode, IsFlagSet(macroblockType, MacroblockMode.Intra));	// motion_vectors( 0 )
			}
			if (IsFlagSet(macroblockType, MacroblockMode.MotionBackward))						// macroblock_motion_backward
			{
				GetMotionVectors(reader, resultState, pictureState.BackwardHorizontalFCode, pictureState.BackwardVerticalFCode, IsFlagSet(macroblockType, MacroblockMode.Intra));	//   motion_vectors( 1 )
			}
			if ((IsFlagSet(macroblockType, MacroblockMode.Intra)) && pictureState.ConcealmentMotionVectors) // macroblock_intra && concealment_motion_vectors
			{
				if (reader.GetBits(1) == 0)														// marker_bit
				{
					resultState.Invalidate();
					return;
				}
			}

			byte[] patternCode;
			if (IsFlagSet(macroblockType, MacroblockMode.Pattern))			// if ( macroblock_pattern )
			{
				// coded_block_pattern()
				patternCode = GetCodedBlockPattern(reader, resultState);
			}
			else
			{
				patternCode = new byte[12];
				for (int i = 0; i < 12; i++)
				{
					if (IsFlagSet(macroblockType, MacroblockMode.Intra))	// if(macroblock_intra)
					{
						patternCode[i] = 1;									// pattern_code[i] = 1;
					}
					else
					{
						patternCode[i] = 0;									// pattern_code[i] = 0;
					}
				}
			}

			int blockCount = GetBlockCount(reader.State.Sequence, resultState);
			for (int i = 0; i < blockCount; i++)							// for ( i = 0; i < block_count; i++ )
			{
				// block( i )
				GetBlock(reader, resultState, i, patternCode, IsFlagSet(macroblockType, MacroblockMode.Intra));
			}
		}

		private static int GetMacroblockAddressIncrement(IMpeg2VideoReader reader, IState resultState)
		{
			int macroblockAddressIncrement = reader.GetVlc(MacroblockAddressIncrementTable);
			if (macroblockAddressIncrement == InvalidInteger)
			{
				resultState.Invalidate();
				return InvalidInteger;
			}

			// FIXME: does not work with a sequence of stuffing!
			if (macroblockAddressIncrement == MacroblockStuffing)
			{
				if (reader.State.IsMpeg2())
				{
					resultState.Invalidate();
					return InvalidInteger;	// Macroblock stuffing is only valid for MPEG-1
				}

				macroblockAddressIncrement = reader.GetVlc(MacroblockAddressIncrementTable);
			}

			return macroblockAddressIncrement;
		}

		// TODO This subclause does not adequately document the block layer
		// syntax when data partitioning is used. See 7.10
		private static void GetBlock(IMpeg2VideoReader reader, IState resultState, int i, byte[] patternCode, bool macroblockIntra)
		{
			if (patternCode != null && patternCode[i] != 0) //if ( pattern_code[i] )
			{
				bool intraVlcFormat = reader.State.Picture.IntraVlcFormat;

				VlcTable<int[]> dctCoefficientTable = GetDctCoefficientTable(intraVlcFormat, macroblockIntra);

				if ( macroblockIntra )
				{
					if (i < 4)
					{
						int dctDcSizeLuminance = reader.GetVlc(TableB12DctDcSizeLuminance);	// dct_dc_size_luminance; 2..9 bits
						if (dctDcSizeLuminance == InvalidInteger)
						{
							resultState.Invalidate();
							return;
						}
						if (dctDcSizeLuminance != 0)												// if(dct_dc_size_luminance != 0)
						{
							reader.GetBits(dctDcSizeLuminance);							// dct_dc_differential; 1..11 bits
						}
					}
					else
					{
						int dctDcSizeChrominance = reader.GetVlc(TableB13DctDcSizeChrominance);	// dct_dc_size_chrominance; 2..10 bits
						if (dctDcSizeChrominance == InvalidInteger)
						{
							resultState.Invalidate();
							return;
						}
						if (dctDcSizeChrominance != 0)													// if(dct_dc_size_chrominance != 0)
						{
							reader.GetBits(dctDcSizeChrominance);										// dct_dc_differential; 1..11 bits
						}
					}
				}
				else
				{
					GetFirstDctCoefficient(reader, resultState);	// First DCT coefficient; 2..24 bits
				}

				bool endOfBlock = false;
				while (!endOfBlock && resultState.Valid)		// while (nextbits() != End of block)
				{
					endOfBlock = GetSubsequentDctCoefficient(reader, resultState, dctCoefficientTable);	// Subsequent DCT coefficients; 3..24 bits
				}
				// GetEndOfBlock(reader);			// End of block; 2 or 4 bits
			}
		}

		/// <summary>
		/// Selection of DCT coefficient VLC tables.
		/// Table 7-3 of the documentation (ISO/IEC 13818-2:2000) indicates
		/// which Table shall be used for decoding the DCT coefficients.
		/// </summary>
		/// <param name="intraVlcFormat"></param>
		/// <param name="macroblockIntra"></param>
		/// <returns>The DCT coefficient table</returns>
		private static VlcTable<int[]> GetDctCoefficientTable(bool intraVlcFormat, bool macroblockIntra)
		{
			return (intraVlcFormat && macroblockIntra) ? TableB15DctCoefficientsTableOne : TableB14DctCoefficientsTableZero;
		}

		private static void GetFirstDctCoefficient(IMpeg2VideoReader reader, IState resultState)
		{
			// Note: The first coefficient can never be (0), since this would contradict
			//		 that the DCT-block is coded (not skipped).
			int[] result = reader.GetVlc(TableB14FirstDctCoefficientsTableZero);
			if ((result == Invalid) || (result == EndOfBlock))
			{
				resultState.Invalidate();
				return;
			}

			if (result == Escape)
			{
				GetEscapeCodedDctCoefficient(reader);
			}
			else
			{
				reader.GetBits(1); // Read the sign bit.
			}
		}

		private static bool GetSubsequentDctCoefficient(IMpeg2VideoReader reader, IState resultState, VlcTable<int[]> dctCoefficientTable)
		{
			int[] result = reader.GetVlc(dctCoefficientTable);
			if (result == Invalid)
			{
				resultState.Invalidate();
				return false;
			}

			if (result == EndOfBlock)
			{
				return true;
			}

			if (result == Escape)
			{
				GetEscapeCodedDctCoefficient(reader);
			}
			else
			{
				reader.GetBits(1); // Read the sign bit.
			}
			return false;
		}

		private static void GetEscapeCodedDctCoefficient(IMpeg2VideoReader reader)
		{
			reader.GetBits(6);	// 6-bit run (number of DCT zero coefficients to skip)

			if (reader.State.IsMpeg2())
			{
				reader.GetBits(12);		// Signed 12-bit level (DCT coefficient)
			}
			else
			{
				uint signedLevel = reader.GetBits(8);
				if ((signedLevel & 0x7f) == 0)	// Level 0 or 128 indicates an escape
				{
					reader.GetBits(8);
				}
			}
		}

		/// <summary>
		/// The number "block_count" which determines the number of blocks in
		/// the macroblock is derived from the chrominance format as shown in Table 6-20.
		/// </summary>
		/// <returns></returns>
		private static int GetBlockCount(ISequenceState sequenceState, IState resultState)
		{
			// table 6-20
			// chroma_format	block_count
			// 4:2:0			6
			// 4:2:2			8
			// 4:4:4			12
			switch (sequenceState.ChromaFormat)
			{
				case ChromaFormat.CF_4_2_0: return 6;
				case ChromaFormat.CF_4_2_2: return 8;
				case ChromaFormat.CF_4_4_4: return 12;
				default: resultState.Invalidate(); return 0;
			}
		}

		private static byte[] GetCodedBlockPattern(IMpeg2VideoReader reader, IState resultState)
		{
			byte[] patternCode = new byte[12];	// pattern_code 

			// coded_block_pattern_420; 3..9 bits
			int codecBlockPattern = reader.GetVlc(TableB9CodedBlockPatterns);
			if (codecBlockPattern == InvalidInteger)
			{
				resultState.Invalidate();
				return null;
			}

			//if (sequenceExtension == null) return null;

			int codedBlockPattern1 = 0;
			int codedBlockPattern2 = 0;

			if (reader.State.Sequence.ChromaFormat == ChromaFormat.CF_4_2_2)// if ( chroma_format == 4:2:2 )
			{
				codedBlockPattern1 = (int)reader.GetBits(2);			// coded_block_pattern_1; 2 bits
			}
			if (reader.State.Sequence.ChromaFormat == ChromaFormat.CF_4_4_4)// if ( chroma_format == 4:4:4 )
			{
				codedBlockPattern2 = (int)reader.GetBits(6);			// coded_block_pattern_2; 6 bits
			}

			for (int i = 0; i < 6; i++)									// for (int i = 0; i < 6; i++)
			{
				if ((codecBlockPattern & (1 << (5 - i))) != 0)			//   if ( cbp & (1<<(5 – i)) )
				{
					patternCode[i] = 1;									//     pattern_code[i] = 1;
				}
			}

			if (reader.State.Sequence.ChromaFormat == ChromaFormat.CF_4_2_2)//if (chroma_format == "4:2:2")
			{
				for (int i = 6; i < 8; i++)								// for (i = 6; i < 8; i++)
				{
					if ((codedBlockPattern1 & (1 << (7 - i))) != 0)		//   if ( coded_block_pattern_1 & (1<<(7 – i)) )
					{
						patternCode[i] = 1;								//     pattern_code[i] = 1;
					}
				}
			}
			if (reader.State.Sequence.ChromaFormat == ChromaFormat.CF_4_4_4)// if (chroma_format == "4:4:4")
			{
				for (int i = 6; i < 12; i++)							// for (i = 6; i < 12; i++)
				{
					if ((codedBlockPattern2 & (1 << (11 - i))) != 0)	//   if ( coded_block_pattern_2 & (1<<(11 – i)) )
					{
						patternCode[i] = 1;								//     pattern_code[i] = 1;
					}
				}
			}
			return patternCode;
		}

		private void GetMotionVectors(IMpeg2VideoReader reader, IState resultState, int horizontalFCode, int verticalFCode, bool macroblockIntra)
		{
			if (GetMotionVectorCount(reader.State, resultState, macroblockIntra) == 1)	// motion_vector_count == 1
			{
				if (GetMvFormat(reader, resultState, macroblockIntra) == MotionVectorFormat.Field && !GetDmv(macroblockIntra, reader.State))	// ( mv_format == field ) && ( dmv != 1)
				{
					reader.GetBits(1);								// motion_vertical_field_select[0][s]
				}
				GetMotionVector(reader, resultState, horizontalFCode, verticalFCode, macroblockIntra);		// motion_vector( 0, s )
			}
			else
			{
				reader.GetBits(1);									// motion_vertical_field_select[0][s]
				GetMotionVector(reader, resultState, horizontalFCode, verticalFCode, macroblockIntra);		// motion_vector(0, s)
				reader.GetBits(1);									// motion_vertical_field_select[1][s]
				GetMotionVector(reader, resultState, horizontalFCode, verticalFCode, macroblockIntra);		// motion_vector(1, s)
			}
		}

		private void GetMotionVector(IMpeg2VideoReader reader, IState resultState, int horizontalFCode, int verticalFCode, bool macroblockIntra)
		{
			GetMotionVectorComponent(reader, resultState, horizontalFCode, macroblockIntra);

			if (!resultState.Valid) return;

			GetMotionVectorComponent(reader, resultState, verticalFCode, macroblockIntra);
		}

		private void GetMotionVectorComponent(IMpeg2VideoReader reader, IState resultState, int fCode, bool macroblockIntra)
		{
			int motionCode = reader.GetVlc(TableB10MotionCode);	// motion_code[r][s][t]; 1..11 bits
			if (motionCode == InvalidInteger)
			{
				resultState.Invalidate();
				return;
			}

			if ((fCode != 1) && (motionCode != 0))						// if ( ( f_code[s][t] != 1) && ( motion_code[r][s][t] != 0 ) )
			{
				int rSize = fCode - 1;									// r_size = f_code[s][t] –1
				reader.GetBits(rSize);									// motion_residual[r][s][t]; 1..8 bits
			}
			if (GetDmv(macroblockIntra, reader.State))			// if (dmv == 1)
			{
				int dmVector = reader.GetVlc(TableB11DmVector);	// dmvector[t]; 1..2 bits
				if (dmVector == InvalidInteger)
				{
					resultState.Invalidate();
					return;
				}
			}
		}

		/// <summary>
		/// macroblock_type (1..9 bits)
		/// Variable length coded indicator which indicates the method of
		/// coding and content of the macroblock according to the Tables B.2
		/// through B.8, selected by picture_coding_type and scalable_mode.
		/// </summary>
		private static MacroblockMode GetMacroblockType(IMpeg2VideoReader reader, IState resultState, PictureCodingType pictureCodingType)
		{
			MacroblockMode macroblockMode = MacroblockMode.Invalid;

			ISequenceState sequenceState = reader.State.Sequence;
			ScalableMode scalableMode = sequenceState.ScalableMode;
			if (!sequenceState.HasExtension(ExtensionId.SequenceScalableExtensionId) || (scalableMode == ScalableMode.DataPartitioning) || (scalableMode == ScalableMode.TemporalScalability))
			{
				// macroblock_type tables: B-2, B-3 and B-4
				switch (pictureCodingType)
				{
					case PictureCodingType.IType:
						macroblockMode = reader.GetVlc(TableB2IPictures);
						break;
					case PictureCodingType.PType:
						macroblockMode = reader.GetVlc(TableB3PPictures);
						break;
					case PictureCodingType.BType:
						macroblockMode = reader.GetVlc(TableB4BPictures);
						break;
				}
			}
			else if (scalableMode == ScalableMode.SpatialScalability)
			{
				if (reader.State.Picture.SpatialScalability)	// present
				{
					// macroblock_type tables when picture_spatial_scalable_extension present: B-5, B-6 and B-7
					switch (pictureCodingType)
					{
						case PictureCodingType.IType:
							macroblockMode = reader.GetVlc(TableB5IPicturesWithSpatialScalability);
							break;
						case PictureCodingType.PType:
							macroblockMode = reader.GetVlc(TableB6PPicturesWithSpatialScalability);
							break;
						case PictureCodingType.BType:
							macroblockMode = reader.GetVlc(TableB7BPicturesWithSpatialScalability);
							break;
					}
				}
				else
				{
					// macroblock_type tables when picture_spatial_scalable_extension not present: B-2, B-3 and B-4
					switch (pictureCodingType)
					{
						case PictureCodingType.IType:
							macroblockMode = reader.GetVlc(TableB2IPictures);
							break;
						case PictureCodingType.PType:
							macroblockMode = reader.GetVlc(TableB3PPictures);
							break;
						case PictureCodingType.BType:
							macroblockMode = reader.GetVlc(TableB4BPictures);
							break;
					}
				}
			}
			else if (scalableMode == ScalableMode.SnrScalability)
			{
				// macroblock_type tables: B-8
				macroblockMode = reader.GetVlc(TableB8PicturesWithSnrScalability);
			}

			if (IsFlagSet(macroblockMode, MacroblockMode.Invalid))
			{
				resultState.Invalidate();
			}
			return macroblockMode;
		}

		private static bool IsFlagSet(MacroblockMode macroblockType, MacroblockMode macroblockTypeFlag)
		{
			return (macroblockType & macroblockTypeFlag) == macroblockTypeFlag;
		}

		private MacroblockMode GetMacroblockModes(IMpeg2VideoReader reader, IResultNodeState resultState, PictureCodingType pictureCodingType)
		{
			MacroblockMode macroblockType = GetMacroblockType(reader, resultState, pictureCodingType);

			if( IsFlagSet(macroblockType, MacroblockMode.SpatialTemporalWeightCodeFlag) &&	// spatial_temporal_weight_code_flag == 1 
			    reader.State.Picture.SpatialTemporalWeightCodeTableIndex != 0)	// spatial_temporal_weight_code_table_index != '00'
			{
				// spatial_temporal_weight_code (2 bits)
				reader.GetBits(2);
			}

			if (IsFlagSet(macroblockType, MacroblockMode.MotionForward) ||	// macroblock_motion_forward ||
			    IsFlagSet(macroblockType, MacroblockMode.MotionBackward))	// macroblock_motion_backward
			{
				if (reader.State.Picture.Structure == PictureStructure.FramePicture)		// picture_structure == 'frame' TODO
				{
					if (reader.State.Picture.FramePredFrameDct == false)							// frame_pred_frame_dct == 0 
					{
						reader.State.Slice.FrameMotionType = (byte)reader.GetBits(2);			// frame_motion_type (2 bits)
					}
				}
				else
				{
					reader.State.Slice.FieldMotionType = (byte)reader.GetBits(2);				// field_motion_type (2 bits)
				}
			}
			if ((reader.State.Picture.Structure == PictureStructure.FramePicture) && (reader.State.Picture.FramePredFrameDct == false) &&
			    (IsFlagSet(macroblockType, MacroblockMode.Intra) || IsFlagSet(macroblockType, MacroblockMode.Pattern)))	// macroblock_intra || macoblock_pattern
			{
				reader.GetBits(1);											// dct_type (1 bit)
			}
			return macroblockType;
		}

		private static void GetPriorityBreakpoint(IMpeg2VideoReader reader)
		{
			ISequenceState sequenceState = reader.State.Sequence;
			if (sequenceState.HasExtension(ExtensionId.SequenceScalableExtensionId) && (sequenceState.ScalableMode == ScalableMode.DataPartitioning))
			{
				reader.GetBits(7, Attribute.PriorityBreakpoint);
			}
		}

		/// <summary>
		/// Get the slice_vertical_position_extension from the stream if vertical_size > 2800
		/// and calculate the macroblock row.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sliceVerticalPosition"></param>
		/// <returns>The calculated macroblock row</returns>
		private static ushort GetSliceVerticalPositionExtension(IMpeg2VideoReader reader, IState resultState, byte sliceVerticalPosition)
		{
			//TODO:check macroblock position of previous slice
			ushort macroblockRow;
			if (reader.State.Sequence.VerticalSize > 2800)
			{
				// If the SliceVerticalPositionExtension is set, the range of SliceVerticalPosition is 1:128
				if (sliceVerticalPosition < 1 || sliceVerticalPosition > 0x80) 
				{
					resultState.Invalidate();
				}
				macroblockRow = (ushort)((reader.GetBits(3, Attribute.SliceVerticalPositionExtension) << 7) + sliceVerticalPosition - 1);
			}
			else
			{
				// If the SliceVerticalPositionExtension is set, the range of SliceVerticalPosition is 1:175
				if (sliceVerticalPosition < 1 || sliceVerticalPosition > 0xAF)
				{
					resultState.Invalidate();
				}
				macroblockRow = (ushort)(sliceVerticalPosition - 1);
			}
			reader.AddDerivedAttribute(Attribute.MarcoblockRow, macroblockRow);
			return macroblockRow;
		}
	}
}
