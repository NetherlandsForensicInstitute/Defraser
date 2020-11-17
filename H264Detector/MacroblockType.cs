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

namespace Defraser.Detector.H264
{
	internal sealed class MacroblockType : IMacroblockType
	{
		private enum Table : byte
		{
			I,
			P,
			B,
			Si
		}

		private sealed class MbPartPredMode
		{
			/// <summary>Used for both <em>Intra4X4</em> and <em>Intra_8x8</em>.</summary>
			public static readonly MbPartPredMode Intra4X4 = new MbPartPredMode(false, false);
			public static readonly MbPartPredMode Intra16X16 = new MbPartPredMode(false, false);
			public static readonly MbPartPredMode PredL0 = new MbPartPredMode(true, false);
			public static readonly MbPartPredMode PredL1 = new MbPartPredMode(false, true);
			public static readonly MbPartPredMode Direct = new MbPartPredMode(false, false);
			public static readonly MbPartPredMode BiPred = new MbPartPredMode(true, true);
			// ReSharper disable InconsistentNaming
			public static readonly MbPartPredMode na = new MbPartPredMode(false, false);
			// ReSharper restore InconsistentNaming

			private readonly bool _list0Predicted;
			private readonly bool _list1Predicted;

			#region Properties
			public bool List0Predicted { get { return _list0Predicted; } }
			public bool List1Predicted { get { return _list1Predicted; } }
			#endregion Properties

			private MbPartPredMode(bool list0Predicted, bool list1Predicted)
			{
				_list0Predicted = list0Predicted;
				_list1Predicted = list1Predicted;
			}
		}

		public static IMacroblockType GetMbTypeI(uint mbType)
		{
			return MbTypeI[mbType];
		}

		public static IMacroblockType GetMbTypeSi(uint mbType)
		{
			return (mbType == 0) ? MbTypeSi[mbType] : MbTypeI[mbType - 1];

		}

		public static IMacroblockType GetMbTypeP(uint mbType)
		{
			return (mbType <= 4) ? MbTypeP[mbType] : MbTypeI[mbType - 5];
		}

		public static IMacroblockType GetMbTypeB(uint mbType)
		{
			return (mbType <= 22) ? MbTypeB[mbType] : MbTypeI[mbType - 23];
		}

		#region Table 7-11 - Macroblock types for I slices
		private static readonly MacroblockType[] MbTypeI = new[]
		{
			new MacroblockType( 0, "I_NxN",         MbPartPredMode.Intra4X4,   0, 0, 0), // see Equation 7-35
			new MacroblockType( 1, "I_16x16_0_0_0", MbPartPredMode.Intra16X16, 0, 0, 0),
			new MacroblockType( 2, "I_16x16_1_0_0", MbPartPredMode.Intra16X16, 1, 0, 0),
			new MacroblockType( 3, "I_16x16_2_0_0", MbPartPredMode.Intra16X16, 2, 0, 0),
			new MacroblockType( 4, "I_16x16_3_0_0", MbPartPredMode.Intra16X16, 3, 0, 0),
			new MacroblockType( 5, "I_16x16_0_1_0", MbPartPredMode.Intra16X16, 0, 1, 0),
			new MacroblockType( 6, "I_16x16_1_1_0", MbPartPredMode.Intra16X16, 1, 1, 0),
			new MacroblockType( 7, "I_16x16_2_1_0", MbPartPredMode.Intra16X16, 2, 1, 0),
			new MacroblockType( 8, "I_16x16_3_1_0", MbPartPredMode.Intra16X16, 3, 1, 0),
			new MacroblockType( 9, "I_16x16_0_2_0", MbPartPredMode.Intra16X16, 0, 2, 0),
			new MacroblockType(10, "I_16x16_1_2_0", MbPartPredMode.Intra16X16, 1, 2, 0),
			new MacroblockType(11, "I_16x16_2_2_0", MbPartPredMode.Intra16X16, 2, 2, 0),
			new MacroblockType(12, "I_16x16_3_2_0", MbPartPredMode.Intra16X16, 3, 2, 0),
			new MacroblockType(13, "I_16x16_0_0_1", MbPartPredMode.Intra16X16, 0, 0, 15),
			new MacroblockType(14, "I_16x16_1_0_1", MbPartPredMode.Intra16X16, 1, 0, 15),
			new MacroblockType(15, "I_16x16_2_0_1", MbPartPredMode.Intra16X16, 2, 0, 15),
			new MacroblockType(16, "I_16x16_3_0_1", MbPartPredMode.Intra16X16, 3, 0, 15),
			new MacroblockType(17, "I_16x16_0_1_1", MbPartPredMode.Intra16X16, 0, 1, 15),
			new MacroblockType(18, "I_16x16_1_1_1", MbPartPredMode.Intra16X16, 1, 1, 15),
			new MacroblockType(19, "I_16x16_2_1_1", MbPartPredMode.Intra16X16, 2, 1, 15),
			new MacroblockType(20, "I_16x16_3_1_1", MbPartPredMode.Intra16X16, 3, 1, 15),
			new MacroblockType(21, "I_16x16_0_2_1", MbPartPredMode.Intra16X16, 0, 2, 15),
			new MacroblockType(22, "I_16x16_1_2_1", MbPartPredMode.Intra16X16, 1, 2, 15),
			new MacroblockType(23, "I_16x16_2_2_1", MbPartPredMode.Intra16X16, 2, 2, 15),
			new MacroblockType(24, "I_16x16_3_2_1", MbPartPredMode.Intra16X16, 3, 2, 15),
			new MacroblockType(25, "I_PCM",         MbPartPredMode.na,         0, 0, 0)
		};
		#endregion Table 7-11 - Macroblock types for I slices

		#region Table 7-12 - Macroblock type with value 0 for SI slices
		private static readonly MacroblockType[] MbTypeSi = new[]
		{
			new MacroblockType( 0, "SI", MbPartPredMode.Intra4X4, 0, 0, 0), // see Equation 7-35
		};
		#endregion Table 7-12 - Macroblock type with value 0 for SI slices

		#region Table 7-13 - Macroblock types for P and SP slices
		private static readonly MacroblockType[] MbTypeP = new[]
		{
			new MacroblockType(0, "P_L0_16x16",   1, MbPartPredMode.PredL0, MbPartPredMode.na,     16, 16),
			new MacroblockType(1, "P_L0_L0_16x8", 2, MbPartPredMode.PredL0, MbPartPredMode.PredL0, 16,  8),
			new MacroblockType(2, "P_L0_L0_8x16", 2, MbPartPredMode.PredL0, MbPartPredMode.PredL0,  8, 16),
			new MacroblockType(3, "P_8x8",        4, MbPartPredMode.na,     MbPartPredMode.na,      8,  8),
			new MacroblockType(4, "P_8x8ref0",    4, MbPartPredMode.na,     MbPartPredMode.na,      8,  8)
			//new MacroblockType(inferred, "P_Skip",1, MbPartPredMode.PredL0, MbPartPredMode.na,     16, 16), 
		};
		#endregion Table 7-13 - Macroblock types for P and SP slices

		#region Table 7-14 - Macroblock types for B slices
		private static readonly MacroblockType[] MbTypeB = new[]
		{
			new MacroblockType( 0, "B_Direct_16x16", 0, MbPartPredMode.Direct, MbPartPredMode.na,      8,  8),
			new MacroblockType( 1, "B_L0_16x16",     1, MbPartPredMode.PredL0, MbPartPredMode.na,     16, 16),
			new MacroblockType( 2, "B_L1_16x16",     1, MbPartPredMode.PredL1, MbPartPredMode.na,     16, 16),
			new MacroblockType( 3, "B_Bi_16x16",     1, MbPartPredMode.BiPred, MbPartPredMode.na,     16, 16),
			new MacroblockType( 4, "B_L0_L0_16x8",   2, MbPartPredMode.PredL0, MbPartPredMode.PredL0, 16,  8),
			new MacroblockType( 5, "B_L0_L0_8x16",   2, MbPartPredMode.PredL0, MbPartPredMode.PredL0,  8, 16),
			new MacroblockType( 6, "B_L1_L1_16x8",   2, MbPartPredMode.PredL1, MbPartPredMode.PredL1, 16,  8),
			new MacroblockType( 7, "B_L1_L1_8x16",   2, MbPartPredMode.PredL1, MbPartPredMode.PredL1,  8, 16),
			new MacroblockType( 8, "B_L0_L1_16x8",   2, MbPartPredMode.PredL0, MbPartPredMode.PredL1, 16,  8),
			new MacroblockType( 9, "B_L0_L1_8x16",   2, MbPartPredMode.PredL0, MbPartPredMode.PredL1,  8, 16),
			new MacroblockType(10, "B_L1_L0_16x8",   2, MbPartPredMode.PredL1, MbPartPredMode.PredL0, 16,  8),
			new MacroblockType(11, "B_L1_L0_8x16",   2, MbPartPredMode.PredL1, MbPartPredMode.PredL0,  8, 16),
			new MacroblockType(12, "B_L0_Bi_16x8",   2, MbPartPredMode.PredL0, MbPartPredMode.BiPred, 16,  8),
			new MacroblockType(13, "B_L0_Bi_8x16",   2, MbPartPredMode.PredL0, MbPartPredMode.BiPred,  8, 16),
			new MacroblockType(14, "B_L1_Bi_16x8",   2, MbPartPredMode.PredL1, MbPartPredMode.BiPred, 16,  8),
			new MacroblockType(15, "B_L1_Bi_8x16",   2, MbPartPredMode.PredL1, MbPartPredMode.BiPred,  8, 16),
			new MacroblockType(16, "B_Bi_L0_16x8",   2, MbPartPredMode.BiPred, MbPartPredMode.PredL0, 16,  8),
			new MacroblockType(17, "B_Bi_L0_8x16",   2, MbPartPredMode.BiPred, MbPartPredMode.PredL0,  8, 16),
			new MacroblockType(18, "B_Bi_L1_16x8",   2, MbPartPredMode.BiPred, MbPartPredMode.PredL1, 16,  8),
			new MacroblockType(19, "B_Bi_L1_8x16",   2, MbPartPredMode.BiPred, MbPartPredMode.PredL1,  8, 16),
			new MacroblockType(20, "B_Bi_Bi_16x8",   2, MbPartPredMode.BiPred, MbPartPredMode.BiPred, 16,  8),
			new MacroblockType(21, "B_Bi_Bi_8x16",   2, MbPartPredMode.BiPred, MbPartPredMode.BiPred,  8, 16),
			new MacroblockType(22, "B_8x8",          4, MbPartPredMode.na,     MbPartPredMode.na,      8,  8)
			//new MacroblockType(inferred, "B_Skip",  na, MbPartPredMode.Direct, MbPartPredMode.na,       8, 8),
		};
		#endregion Table 7-14 - Macroblock types for B slices

		private readonly Table _table;
		private readonly byte _mbType;
		private readonly string _name;
		private readonly MbPartPredMode[] _mbPartPredModes;
// ReSharper disable UnaccessedField.Local
		private readonly byte _intra16X16PredMode;
// ReSharper restore UnaccessedField.Local
		private readonly byte _codedBlockPattern;
		private readonly byte _numMbPart;
		private readonly MacroblockPartitioning _macroblockPartitioning;

		#region Properties
		public uint NumMbPart { get { return _numMbPart; } }
		public bool IntraCoded { get { return (_table == Table.I) || (_table == Table.Si); } }
		public bool IsDirect { get { return (_mbPartPredModes[0] == MbPartPredMode.Direct); } }
		public bool IsIntra4X4 { get { return (_mbPartPredModes[0] == MbPartPredMode.Intra4X4); } }
		public bool IsIntra16X16 { get { return (_mbPartPredModes[0] == MbPartPredMode.Intra16X16); } }
		public bool IsIPcm { get { return (_table == Table.I) && (_mbType == 25/*I_PCM*/); } }

		// ReSharper disable InconsistentNaming
		public bool IsI_NxN { get { return (_table == Table.I) && (_mbType == 0/*I_NxN*/); } }
		public bool IsP8X8Ref0 { get { return (_table == Table.P) && (_mbType == 4); } }
		// ReSharper restore InconsistentNaming
		public byte CodedBlockPattern { get { return _codedBlockPattern; } }
		public MacroblockPartitioning MacroblockPartitioning { get { return _macroblockPartitioning; } }
		#endregion Properties

		private MacroblockType(byte mbType, string name, MbPartPredMode mbPartPredMode, byte intra16X16PredMode, byte codedBlockPatternChroma, byte codedBlockPatternLuma)
		{
			_mbType = mbType;
			_name = name;
			_mbPartPredModes = new MbPartPredMode[2];
			_mbPartPredModes[0] = mbPartPredMode;
			_mbPartPredModes[1] = MbPartPredMode.na;
			_intra16X16PredMode = intra16X16PredMode;
			_codedBlockPattern = (byte)((codedBlockPatternChroma << 4) | codedBlockPatternLuma);

			// Note: Intra-coded blocks do not have parts (na)
			_numMbPart = 0;
			_macroblockPartitioning = MacroblockPartitioning.M16X16;

			_table = name.StartsWith("I") ? Table.I : Table.Si;
		}

		private MacroblockType(byte mbType, string name, byte numMbPart, MbPartPredMode mbPartPredMode0, MbPartPredMode mbPartPredMode1, byte mbPartPredWidth, byte mbPartPredHeight)
		{
			_mbType = mbType;
			_name = name;
			_numMbPart = numMbPart;
			_mbPartPredModes = new MbPartPredMode[2];
			_mbPartPredModes[0] = mbPartPredMode0;
			_mbPartPredModes[1] = mbPartPredMode1;
			_macroblockPartitioning = MacroblockPartitioning.GetMacroblockPartitioning(mbPartPredWidth, mbPartPredHeight);

			_intra16X16PredMode = 0;
			_codedBlockPattern = 0; // encoded in bitstream

			_table = name.StartsWith("P") ? Table.P : Table.B;
		}

		public bool IsList0Predicted(uint mbPartIdx)
		{
			return _mbPartPredModes[mbPartIdx].List0Predicted;
		}

		public bool IsList1Predicted(uint mbPartIdx)
		{
			return _mbPartPredModes[mbPartIdx].List1Predicted;
		}

		public override String ToString()
		{
			return _name;
		}
	}
}
