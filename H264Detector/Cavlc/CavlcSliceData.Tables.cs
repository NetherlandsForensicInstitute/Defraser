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

namespace Defraser.Detector.H264.Cavlc
{
	internal partial class CavlcSliceData
	{
		private const byte InvalidInteger = byte.MaxValue;
		private static readonly CoeffToken InvalidCoeffToken = new CoeffToken(byte.MaxValue, byte.MaxValue);

		private static readonly int[] TotalChromaSubBlocksPerMb = { 0, 128, 256, 512 };

		#region table 7-11 - Macroblock types for I slices
		private static readonly uint[] Intra16X16CodedBlockPattern = { 0, 16, 32, 15, 31, 47 };
		#endregion table 7-11 - Macroblock types for I slices

		#region table 9-4 (a) - coded_block_pattern for macroblock prediction modes (4:2:2 or 4:2:0 chroma sampling)
		private static readonly uint[] IntraCodedBlockPattern42X =
		{
				47, 31, 15,  0, 23, 27, 29, 30,  7, 11, 13, 14, 39, 43, 45, 46,
				16,  3,  5, 10, 12, 19, 21, 26, 28, 35, 37, 42, 44,  1,  2,  4,
				 8, 17, 18, 20, 24,  6,  9, 22, 25, 32, 33, 34, 36, 40, 38, 41
		};

		private static readonly uint[] InterCodedBlockPattern42X =
		{
				 0, 16,  1,  2,  4,  8, 32,  3,  5, 10, 12, 15, 47,  7, 11, 13,
				14,  6,  9, 31, 35, 37, 42, 44, 33, 34, 36, 40, 39, 43, 45, 46,
				17, 18, 20, 24, 19, 21, 26, 28, 23, 27, 29, 30, 22, 25, 38, 41
		};
		#endregion table 9-4 (a) - coded_block_pattern for macroblock prediction modes (ChromaFormat is equal to 1 or 2)

		#region table 9-4 (b) - coded_block_pattern for macroblock prediction modes (4:4:4 chroma sampling)
		private static readonly uint[] IntraCodedBlockPattern444 =
		{
				15,  0,  7, 11, 13, 14,  3,  5, 10, 12,  1,  2,  4,  8,  6,  9
		};

		private static readonly uint[] InterCodedBlockPattern444 =
		{
				 0,  1,  2,  4,  8,  3,  5, 10, 12, 15,  7, 11, 13, 14,  6,  9
		};
		#endregion table 9-4 (b) - coded_block_pattern for macroblock prediction modes

		#region table 9-5 - coeff_token mapping to TotalCoeff( coeff_token ) and TrailingOnes( coeff_token )
		private static readonly VlcTable<CoeffToken> CoeffTokenN0 = new VlcTable<CoeffToken>(new object[,]
		{
			{"1",new CoeffToken(0,0)},
			{"000101",new CoeffToken(0,1)},
			{"01",new CoeffToken(1,1)},
			{"00000111",new CoeffToken(0,2)},
			{"000100",new CoeffToken(1,2)},
			{"001",new CoeffToken(2,2)},
			{"000000111",new CoeffToken(0,3)},
			{"00000110",new CoeffToken(1,3)},
			{"0000101",new CoeffToken(2,3)},
			{"00011",new CoeffToken(3,3)},
			{"0000000111",new CoeffToken(0,4)},
			{"000000110",new CoeffToken(1,4)},
			{"00000101",new CoeffToken(2,4)},
			{"000011",new CoeffToken(3,4)},
			{"00000000111",new CoeffToken(0,5)},
			{"0000000110",new CoeffToken(1,5)},
			{"000000101",new CoeffToken(2,5)},
			{"0000100",new CoeffToken(3,5)},
			{"0000000001111",new CoeffToken(0,6)},
			{"00000000110",new CoeffToken(1,6)},
			{"0000000101",new CoeffToken(2,6)},
			{"00000100",new CoeffToken(3,6)},
			{"0000000001011",new CoeffToken(0,7)},
			{"0000000001110",new CoeffToken(1,7)},
			{"00000000101",new CoeffToken(2,7)},
			{"000000100",new CoeffToken(3,7)},
			{"0000000001000",new CoeffToken(0,8)},
			{"0000000001010",new CoeffToken(1,8)},
			{"0000000001101",new CoeffToken(2,8)},
			{"0000000100",new CoeffToken(3,8)},
			{"00000000001111",new CoeffToken(0,9)},
			{"00000000001110",new CoeffToken(1,9)},
			{"0000000001001",new CoeffToken(2,9)},
			{"00000000100",new CoeffToken(3,9)},
			{"00000000001011",new CoeffToken(0,10)},
			{"00000000001010",new CoeffToken(1,10)},
			{"00000000001101",new CoeffToken(2,10)},
			{"0000000001100",new CoeffToken(3,10)},
			{"000000000001111",new CoeffToken(0,11)},
			{"000000000001110",new CoeffToken(1,11)},
			{"00000000001001",new CoeffToken(2,11)},
			{"00000000001100",new CoeffToken(3,11)},
			{"000000000001011",new CoeffToken(0,12)},
			{"000000000001010",new CoeffToken(1,12)},
			{"000000000001101",new CoeffToken(2,12)},
			{"00000000001000",new CoeffToken(3,12)},
			{"0000000000001111",new CoeffToken(0,13)},
			{"000000000000001",new CoeffToken(1,13)},
			{"000000000001001",new CoeffToken(2,13)},
			{"000000000001100",new CoeffToken(3,13)},
			{"0000000000001011",new CoeffToken(0,14)},
			{"0000000000001110",new CoeffToken(1,14)},
			{"0000000000001101",new CoeffToken(2,14)},
			{"000000000001000",new CoeffToken(3,14)},
			{"0000000000000111",new CoeffToken(0,15)},
			{"0000000000001010",new CoeffToken(1,15)},
			{"0000000000001001",new CoeffToken(2,15)},
			{"0000000000001100",new CoeffToken(3,15)},
			{"0000000000000100",new CoeffToken(0,16)},
			{"0000000000000110",new CoeffToken(1,16)},
			{"0000000000000101",new CoeffToken(2,16)},
			{"0000000000001000",new CoeffToken(3,16)}
		}, InvalidCoeffToken);

		private static readonly VlcTable<CoeffToken> CoeffTokenN2 = new VlcTable<CoeffToken>(new object[,]
		{
			{"11",				new CoeffToken(0,0)},
			{"001011",			new CoeffToken(0,1)},
			{"10",				new CoeffToken(1,1)},
			{"000111",			new CoeffToken(0,2)},
			{"00111",			new CoeffToken(1,2)},
			{"011",				new CoeffToken(2,2)},
			{"0000111",			new CoeffToken(0,3)},
			{"001010",			new CoeffToken(1,3)},
			{"001001",			new CoeffToken(2,3)},
			{"0101",			new CoeffToken(3,3)},
			{"00000111",		new CoeffToken(0,4)},
			{"000110",			new CoeffToken(1,4)},
			{"000101",			new CoeffToken(2,4)},
			{"0100",			new CoeffToken(3,4)},
			{"00000100",		new CoeffToken(0,5)},
			{"0000110",			new CoeffToken(1,5)},
			{"0000101",			new CoeffToken(2,5)},
			{"00110",			new CoeffToken(3,5)},
			{"000000111",		new CoeffToken(0,6)},
			{"00000110",		new CoeffToken(1,6)},
			{"00000101",		new CoeffToken(2,6)},
			{"001000",			new CoeffToken(3,6)},
			{"00000001111",		new CoeffToken(0,7)},
			{"000000110",		new CoeffToken(1,7)},
			{"000000101",		new CoeffToken(2,7)},
			{"000100",			new CoeffToken(3,7)},

			{"00000001011",new CoeffToken(0,8)},
			{"00000001110",new CoeffToken(1,8)},
			{"00000001101",new CoeffToken(2,8)},
			{"0000100",new CoeffToken(3,8)},
			{"000000001111",new CoeffToken(0,9)},
			{"00000001010",new CoeffToken(1,9)},
			{"00000001001",new CoeffToken(2,9)},
			{"000000100",new CoeffToken(3,9)},
			{"000000001011",new CoeffToken(0,10)},
			{"000000001110",new CoeffToken(1,10)},
			{"000000001101",new CoeffToken(2,10)},
			{"00000001100",new CoeffToken(3,10)},
			{"000000001000",new CoeffToken(0,11)},
			{"000000001010",new CoeffToken(1,11)},
			{"000000001001",new CoeffToken(2,11)},
			{"00000001000",new CoeffToken(3,11)},
			{"0000000001111",new CoeffToken(0,12)},
			{"0000000001110",new CoeffToken(1,12)},
			{"0000000001101",new CoeffToken(2,12)},
			{"000000001100",new CoeffToken(3,12)},
			{"0000000001011",new CoeffToken(0,13)},
			{"0000000001010",new CoeffToken(1,13)},
			{"0000000001001",new CoeffToken(2,13)},
			{"0000000001100",new CoeffToken(3,13)},
			{"0000000000111",new CoeffToken(0,14)},
			{"00000000001011",new CoeffToken(1,14)},
			{"0000000000110",new CoeffToken(2,14)},
			{"0000000001000",new CoeffToken(3,14)},
			{"00000000001001",new CoeffToken(0,15)},
			{"00000000001000",new CoeffToken(1,15)},
			{"00000000001010",new CoeffToken(2,15)},
			{"0000000000001",new CoeffToken(3,15)},
			{"00000000000111",new CoeffToken(0,16)},
			{"00000000000110",new CoeffToken(1,16)},
			{"00000000000101",new CoeffToken(2,16)},
			{"00000000000100",new CoeffToken(3,16)},
		}, InvalidCoeffToken);

		private static readonly VlcTable<CoeffToken> CoeffTokenN4 = new VlcTable<CoeffToken>(new object[,]
		{
			{"1111",new CoeffToken(0,0)},
			{"001111",new CoeffToken(0,1)},
			{"1110",new CoeffToken(1,1)},
			{"001011",new CoeffToken(0,2)},
			{"01111",new CoeffToken(1,2)},
			{"1101",new CoeffToken(2,2)},
			{"001000",new CoeffToken(0,3)},
			{"01100",new CoeffToken(1,3)},
			{"01110",new CoeffToken(2,3)},
			{"1100",new CoeffToken(3,3)},
			{"0001111",new CoeffToken(0,4)},
			{"01010",new CoeffToken(1,4)},
			{"01011",new CoeffToken(2,4)},
			{"1011",new CoeffToken(3,4)},
			{"0001011",new CoeffToken(0,5)},
			{"01000",new CoeffToken(1,5)},
			{"01001",new CoeffToken(2,5)},
			{"1010",new CoeffToken(3,5)},
			{"0001001",new CoeffToken(0,6)},
			{"001110",new CoeffToken(1,6)},
			{"001101",new CoeffToken(2,6)},
			{"1001",new CoeffToken(3,6)},
			{"0001000",new CoeffToken(0,7)},
			{"001010",new CoeffToken(1,7)},
			{"001001",new CoeffToken(2,7)},
			{"1000",new CoeffToken(3,7)},
			{"00001111",new CoeffToken(0,8)},
			{"0001110",new CoeffToken(1,8)},
			{"0001101",new CoeffToken(2,8)},
			{"01101",new CoeffToken(3,8)},
			{"00001011",new CoeffToken(0,9)},
			{"00001110",new CoeffToken(1,9)},
			{"0001010",new CoeffToken(2,9)},
			{"001100",new CoeffToken(3,9)},
			{"000001111",new CoeffToken(0,10)},
			{"00001010",new CoeffToken(1,10)},
			{"00001101",new CoeffToken(2,10)},
			{"0001100",new CoeffToken(3,10)},
			{"000001011",new CoeffToken(0,11)},
			{"000001110",new CoeffToken(1,11)},
			{"00001001",new CoeffToken(2,11)},
			{"00001100",new CoeffToken(3,11)},
			{"000001000",new CoeffToken(0,12)},
			{"000001010",new CoeffToken(1,12)},
			{"000001101",new CoeffToken(2,12)},
			{"00001000",new CoeffToken(3,12)},
			{"0000001101",new CoeffToken(0,13)},
			{"000000111",new CoeffToken(1,13)},
			{"000001001",new CoeffToken(2,13)},
			{"000001100",new CoeffToken(3,13)},
			{"0000001001",new CoeffToken(0,14)},
			{"0000001100",new CoeffToken(1,14)},
			{"0000001011",new CoeffToken(2,14)},
			{"0000001010",new CoeffToken(3,14)},
			{"0000000101",new CoeffToken(0,15)},
			{"0000001000",new CoeffToken(1,15)},
			{"0000000111",new CoeffToken(2,15)},
			{"0000000110",new CoeffToken(3,15)},
			{"0000000001",new CoeffToken(0,16)},
			{"0000000100",new CoeffToken(1,16)},
			{"0000000011",new CoeffToken(2,16)},
			{"0000000010",new CoeffToken(3,16)},
		}, InvalidCoeffToken);

		private static readonly VlcTable<CoeffToken> CoeffTokenN8 = new VlcTable<CoeffToken>(new object[,]
		{
			{"000011",		new CoeffToken(0,0)},
			{"000000",		new CoeffToken(0,1)},
			{"000001",		new CoeffToken(1,1)},
			{"000100",		new CoeffToken(0,2)},
			{"000101",		new CoeffToken(1,2)},
			{"000110",		new CoeffToken(2,2)},
			{"001000",		new CoeffToken(0,3)},
			{"001001",		new CoeffToken(1,3)},
			{"001010",		new CoeffToken(2,3)},
			{"001011",		new CoeffToken(3,3)},
			{"001100",		new CoeffToken(0,4)},
			{"001101",		new CoeffToken(1,4)},
			{"001110",		new CoeffToken(2,4)},
			{"001111",		new CoeffToken(3,4)},
			{"010000",		new CoeffToken(0,5)},
			{"010001",		new CoeffToken(1,5)},
			{"010010",		new CoeffToken(2,5)},
			{"010011",		new CoeffToken(3,5)},
			{"010100",		new CoeffToken(0,6)},
			{"010101",		new CoeffToken(1,6)},
			{"010110",		new CoeffToken(2,6)},
			{"010111",		new CoeffToken(3,6)},
			{"011000",		new CoeffToken(0,7)},
			{"011001",		new CoeffToken(1,7)},
			{"011010",		new CoeffToken(2,7)},
			{"011011",		new CoeffToken(3,7)},
			{"011100",		new CoeffToken(0,8)},
			{"011101",		new CoeffToken(1,8)},
			{"011110",		new CoeffToken(2,8)},
			{"011111",		new CoeffToken(3,8)},
			{"100000",		new CoeffToken(0,9)},
			{"100001",		new CoeffToken(1,9)},
			{"100010",		new CoeffToken(2,9)},
			{"100011",		new CoeffToken(3,9)},
			{"100100",		new CoeffToken(0,10)},
			{"100101",		new CoeffToken(1,10)},
			{"100110",		new CoeffToken(2,10)},
			{"100111",		new CoeffToken(3,10)},
			{"101000",		new CoeffToken(0,11)},
			{"101001",		new CoeffToken(1,11)},
			{"101010",		new CoeffToken(2,11)},
			{"101011",		new CoeffToken(3,11)},
			{"101100",		new CoeffToken(0,12)},
			{"101101",		new CoeffToken(1,12)},
			{"101110",		new CoeffToken(2,12)},
			{"101111",		new CoeffToken(3,12)},
			{"110000",		new CoeffToken(0,13)},
			{"110001",		new CoeffToken(1,13)},
			{"110010",		new CoeffToken(2,13)},
			{"110011",		new CoeffToken(3,13)},
			{"110100",		new CoeffToken(0,14)},
			{"110101",		new CoeffToken(1,14)},
			{"110110",		new CoeffToken(2,14)},
			{"110111",		new CoeffToken(3,14)},
			{"111000",		new CoeffToken(0,15)},
			{"111001",		new CoeffToken(1,15)},
			{"111010",		new CoeffToken(2,15)},
			{"111011",		new CoeffToken(3,15)},
			{"111100",		new CoeffToken(0,16)},
			{"111101",		new CoeffToken(1,16)},
			{"111110",		new CoeffToken(2,16)},
			{"111111",		new CoeffToken(3,16)},
		}, InvalidCoeffToken);

		private static readonly VlcTable<CoeffToken> CoeffTokenChromaDc420 = new VlcTable<CoeffToken>(new object[,]
		{
			{"01",			new CoeffToken(0,0)},
			{"000111",		new CoeffToken(0,1)},
			{"1",			new CoeffToken(1,1)},
			{"000100",		new CoeffToken(0,2)},
			{"000110",		new CoeffToken(1,2)},
			{"001",			new CoeffToken(2,2)},
			{"000011",		new CoeffToken(0,3)},
			{"0000011",		new CoeffToken(1,3)},
			{"0000010",		new CoeffToken(2,3)},
			{"000101",		new CoeffToken(3,3)},
			{"000010",		new CoeffToken(0,4)},
			{"00000011",	new CoeffToken(1,4)},
			{"00000010",	new CoeffToken(2,4)},
			{"0000000",		new CoeffToken(3,4)},
		}, InvalidCoeffToken);

		private static readonly VlcTable<CoeffToken> CoeffTokenChromaDc422 = new VlcTable<CoeffToken>(new object[,]
		{
			{"1",			new CoeffToken(0,0)},
			{"0001111",		new CoeffToken(0,1)},
			{"01",			new CoeffToken(1,1)},
			{"0001110",		new CoeffToken(0,2)},
			{"0001101",		new CoeffToken(1,2)},
			{"001",			new CoeffToken(2,2)},
			{"000000111",	new CoeffToken(0,3)},
			{"0001100",		new CoeffToken(1,3)},
			{"0001011",		new CoeffToken(2,3)},
			{"00001",		new CoeffToken(3,3)},
			{"000000110",	new CoeffToken(0,4)},
			{"000000101",	new CoeffToken(1,4)},
			{"0001010",		new CoeffToken(2,4)},
			{"000001",		new CoeffToken(3,4)},
			{"0000000111",	new CoeffToken(0,5)},
			{"0000000110",	new CoeffToken(1,5)},
			{"000000100",	new CoeffToken(2,5)},
			{"0001001",		new CoeffToken(3,5)},
			{"00000000111",	new CoeffToken(0,6)},
			{"00000000110",	new CoeffToken(1,6)},
			{"0000000101",	new CoeffToken(2,6)},
			{"0001000",		new CoeffToken(3,6)},
			{"000000000111",new CoeffToken(0,7)},
			{"000000000110",new CoeffToken(1,7)},
			{"00000000101",	new CoeffToken(2,7)},
			{"0000000100",	new CoeffToken(3,7)},
			{"0000000000111",new CoeffToken(0,8)},
			{"000000000101",new CoeffToken(1,8)},
			{"000000000100",new CoeffToken(2,8)},
			{"00000000100",	new CoeffToken(3,8)},
		}, InvalidCoeffToken);
		#endregion table 9-5 - coeff_token mapping to TotalCoeff( coeff_token ) and TrailingOnes( coeff_token )

		#region table 9-7 and 9-8 - total zeros tables for 4x4 blocks
		private static readonly VlcTable<int>[] TotalZerosTable4X4 = new VlcTable<int>[]
		{
			null, // tzVlcIndex (0) - unused

			// tzVlcIndex (1)
			new VlcTable<int>(new object[,]
			{
				{ "1",			 0 },
				{ "011",		 1 },
				{ "010",		 2 },
				{ "0011",		 3 },
				{ "0010",		 4 },
				{ "00011",		 5 },
				{ "00010",		 6 },
				{ "000011",		 7 },
				{ "000010",		 8 },
				{ "0000011",	 9 },
				{ "0000010",	10 },
				{ "00000011",	11 },
				{ "00000010",	12 },
				{ "000000011",	13 },
				{ "000000010",	14 },
				{ "000000001",	15 }
			}, InvalidInteger),

			// tzVlcIndex (2)
			new VlcTable<int>(new object[,]
			{
				{ "111",		 0 },
				{ "110",		 1 },
				{ "101",		 2 },
				{ "100",		 3 },
				{ "011",		 4 },
				{ "0101",		 5 },
				{ "0100",		 6 },
				{ "0011",		 7 },
				{ "0010",		 8 },
				{ "00011",		 9 },
				{ "00010",		10 },
				{ "000011",		11 },
				{ "000010",		12 },
				{ "000001",		13 },
				{ "000000",		14 }
			}, InvalidInteger),

			// tzVlcIndex (3)
			new VlcTable<int>(new object[,]
			{
				{ "0101",		 0 },
				{ "111",		 1 },
				{ "110",		 2 },
				{ "101",		 3 },
				{ "0100",		 4 },
				{ "0011",		 5 },
				{ "100",		 6 },
				{ "011",		 7 },
				{ "0010",		 8 },
				{ "00011",		 9 },
				{ "00010",		10 },
				{ "000001",		11 },
				{ "00001",		12 },
				{ "000000",		13 }
			}, InvalidInteger),

			// tzVlcIndex (4)
			new VlcTable<int>(new object[,]
			{
				{ "00011",		 0 },
				{ "111",		 1 },
				{ "0101",		 2 },
				{ "0100",		 3 },
				{ "110",		 4 },
				{ "101",		 5 },
				{ "100",		 6 },
				{ "0011",		 7 },
				{ "011",		 8 },
				{ "0010",		 9 },
				{ "00010",		10 },
				{ "00001",		11 },
				{ "00000",		12 }
			}, InvalidInteger),

			// tzVlcIndex (5)
			new VlcTable<int>(new object[,]
			{
				{ "0101",		 0 },
				{ "0100",		 1 },
				{ "0011",		 2 },
				{ "111",		 3 },
				{ "110",		 4 },
				{ "101",		 5 },
				{ "100",		 6 },
				{ "011",		 7 },
				{ "0010",		 8 },
				{ "00001",		 9 },
				{ "0001",		10 },
				{ "00000",		11 }
			}, InvalidInteger),

			// tzVlcIndex (6)
			new VlcTable<int>(new object[,]
			{
				{ "000001",		 0 },
				{ "00001",		 1 },
				{ "111",		 2 },
				{ "110",		 3 },
				{ "101",		 4 },
				{ "100",		 5 },
				{ "011",		 6 },
				{ "010",		 7 },
				{ "0001",		 8 },
				{ "001",		 9 },
				{ "000000",		10 }
			}, InvalidInteger),

			// tzVlcIndex (7)
			new VlcTable<int>(new object[,]
			{
				{ "000001",		 0 },
				{ "00001",		 1 },
				{ "101",		 2 },
				{ "100",		 3 },
				{ "011",		 4 },
				{ "11",			 5 },
				{ "010",		 6 },
				{ "0001",		 7 },
				{ "001",		 8 },
				{ "000000",		 9 }
			}, InvalidInteger),

			// tzVlcIndex (8) - start of table 9-8
			new VlcTable<int>(new object[,]
			{
				{ "000001",		 0 },
				{ "0001",		 1 },
				{ "00001",		 2 },
				{ "011",		 3 },
				{ "11",			 4 },
				{ "10",			 5 },
				{ "010",		 6 },
				{ "001",		 7 },
				{ "000000",		 8 }
			}, InvalidInteger),

			// tzVlcIndex (9)
			new VlcTable<int>(new object[,]
			{
				{ "000001",		 0 },
				{ "000000",		 1 },
				{ "0001",		 2 },
				{ "11",			 3 },
				{ "10",			 4 },
				{ "001",		 5 },
				{ "01",			 6 },
				{ "00001",		 7 }
			}, InvalidInteger),

			// tzVlcIndex (10)
			new VlcTable<int>(new object[,]
			{
				{ "00001",		 0 },
				{ "00000",		 1 },
				{ "001",		 2 },
				{ "11",			 3 },
				{ "10",			 4 },
				{ "01",			 5 },
				{ "0001",		 6 }
			}, InvalidInteger),

			// tzVlcIndex (11)
			new VlcTable<int>(new object[,]
			{
				{ "0000",		 0 },
				{ "0001",		 1 },
				{ "001",		 2 },
				{ "010",		 3 },
				{ "1",			 4 },
				{ "011",		 5 }
			}, InvalidInteger),

			// tzVlcIndex (12)
			new VlcTable<int>(new object[,]
			{
				{ "0000",		 0 },
				{ "0001",		 1 },
				{ "01",			 2 },
				{ "1",			 3 },
				{ "001",		 4 }
			}, InvalidInteger),

			// tzVlcIndex (13)
			new VlcTable<int>(new object[,]
			{
				{ "000",		 0 },
				{ "001",		 1 },
				{ "1",			 2 },
				{ "01",			 3 }
			}, InvalidInteger),

			// tzVlcIndex (14)
			new VlcTable<int>(new object[,]
			{
				{ "00",			 0 },
				{ "01",			 1 },
				{ "1",			 2 }
			}, InvalidInteger),

			// tzVlcIndex (15)
			new VlcTable<int>(new object[,]
			{
				{ "0",			 0 },
				{ "1",			 1 }
			}, InvalidInteger),
		};
		#endregion table 9-7 and 9-8 - total zeros tables for 4x4 blocks

		#region table 9-9 (a) - total zeros tables for chroma DC 2x2 blocks (4:2:0 chroma sampling)
		private static readonly VlcTable<int>[] TotalZerosTable2X2 = new VlcTable<int>[]
		{
			null, // tzVlcIndex (0) - unused

			// tzVlcIndex (1)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "01",		 1 },
				{ "001",	 2 },
				{ "000",	 3 }
			}, InvalidInteger),

			// tzVlcIndex (2)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "01",		 1 },
				{ "00",		 2 }
			}, InvalidInteger),

			// tzVlcIndex (3)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "0",		 1 }
			}, InvalidInteger)
		};
		#endregion table 9-9 (a) - total zeros tables for chroma DC 2x2 blocks (4:2:0 chroma sampling)

		#region table 9-9 (b) - total zeros tables for chroma DC 2x4 blocks (4:2:2 chroma sampling)
		private static readonly VlcTable<int>[] TotalZerosTable2X4 = new VlcTable<int>[]
		{
			null, // tzVlcIndex (0) - unused

			// tzVlcIndex (1)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "010",	 1 },
				{ "011",	 2 },
				{ "0010",	 3 },
				{ "0011",	 4 },
				{ "0001",	 5 },
				{ "00001",	 6 },
				{ "00000",	 7 }
			}, InvalidInteger),

			// tzVlcIndex (2)
			new VlcTable<int>(new object[,]
			{
				{ "000",	 0 },
				{ "01",		 1 },
				{ "001",	 2 },
				{ "100",	 3 },
				{ "101",	 4 },
				{ "110",	 5 },
				{ "111",	 6 }
			}, InvalidInteger),

			// tzVlcIndex (3)
			new VlcTable<int>(new object[,]
			{
				{ "000",	 0 },
				{ "001",	 1 },
				{ "01",		 2 },
				{ "10",		 3 },
				{ "110",	 4 },
				{ "111",	 5 }
			}, InvalidInteger),

			// tzVlcIndex (4)
			new VlcTable<int>(new object[,]
			{
				{ "110",	 0 },
				{ "00",		 1 },
				{ "01",		 2 },
				{ "10",		 3 },
				{ "111",	 4 }
			}, InvalidInteger),

			// tzVlcIndex (5)
			new VlcTable<int>(new object[,]
			{
				{ "00",		 0 },
				{ "01",		 1 },
				{ "10",		 2 },
				{ "11",		 3 }
			}, InvalidInteger),

			// tzVlcIndex (6)
			new VlcTable<int>(new object[,]
			{
				{ "00",		 0 },
				{ "01",		 1 },
				{ "1",		 2 }
			}, InvalidInteger),

			// tzVlcIndex (7)
			new VlcTable<int>(new object[,]
			{
				{ "0",		 0 },
				{ "1",		 1 }
			}, InvalidInteger),
		};
		#endregion table 9-9 (b) - total zeros tables for chroma DC 2x4 blocks (4:2:2 chroma sampling)

		#region table 9-10 - Tables for run_before
		private static readonly VlcTable<int>[] RunBeforeTable = new VlcTable<int>[]
		{
			null, // zerosLeft (0) - unused

			// zerosLeft (1)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "0",		 1 }
			}, InvalidInteger),

			// zerosLeft (2)
			new VlcTable<int>(new object[,]
			{
				{ "1",		 0 },
				{ "01",		 1 },
				{ "00",		 2 }
			}, InvalidInteger),

			// zerosLeft (3)
			new VlcTable<int>(new object[,]
			{
				{ "11",		 0 },
				{ "10",		 1 },
				{ "01",		 2 },
				{ "00",		 3 }
			}, InvalidInteger),

			// zerosLeft (4)
			new VlcTable<int>(new object[,]
			{
				{ "11",		 0 },
				{ "10",		 1 },
				{ "01",		 2 },
				{ "001",	 3 },
				{ "000",	 4 }
			}, InvalidInteger),

			// zerosLeft (5)
			new VlcTable<int>(new object[,]
			{
				{ "11",		 0 },
				{ "10",		 1 },
				{ "011",	 2 },
				{ "010",	 3 },
				{ "001",	 4 },
				{ "000",	 5 }
			}, InvalidInteger),

			// zerosLeft (6)
			new VlcTable<int>(new object[,]
			{
				{ "11",		 0 },
				{ "000",	 1 },
				{ "001",	 2 },
				{ "011",	 3 },
				{ "010",	 4 },
				{ "101",	 5 },
				{ "100",	 6 }
			}, InvalidInteger),

			// zerosLeft (>6)
			new VlcTable<int>(new object[,]
			{
				{ "111",	 0 },
				{ "110",	 1 },
				{ "101",	 2 },
				{ "100",	 3 },
				{ "011",	 4 },
				{ "010",	 5 },
				{ "001",	 6 },
				{ "0001",	 7 },
				{ "00001",	 8 },
				{ "000001",	 9 },
				{ "0000001", 10 },
				{ "00000001", 11 },
				{ "000000001", 12 },
				{ "0000000001",	13 },
				{ "00000000001", 14 }
			}, InvalidInteger)
		};
		#endregion table 9-10 - Tables for run_before

		/// <summary>
		/// coeff_token
		/// </summary>
		private struct CoeffToken
		{
			private readonly byte _trailingOnes;
			private readonly byte _totalCoeff;

			#region Properties
			public byte TrailingOnes { get { return _trailingOnes; } }
			public byte TotalCoeff { get { return _totalCoeff; } }
			#endregion Properties

			public CoeffToken(byte trailingOnes, byte totalCoeff)
			{
				_trailingOnes = trailingOnes;
				_totalCoeff = totalCoeff;
			}

			public override String ToString()
			{
				return String.Format("({0},{1})", _trailingOnes, _totalCoeff);
			}
		}
	}
}
