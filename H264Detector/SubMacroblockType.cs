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

namespace Defraser.Detector.H264
{
	internal sealed class SubMacroblockType : ISubMacroblockType
	{
		private sealed class SubMbPredMode
		{
			public static readonly SubMbPredMode Direct = new SubMbPredMode(false, false);
			public static readonly SubMbPredMode PredL0 = new SubMbPredMode(true, false);
			public static readonly SubMbPredMode PredL1 = new SubMbPredMode(false, true);
			public static readonly SubMbPredMode BiPred = new SubMbPredMode(true, true);

			private readonly bool _list0Predicted;
			private readonly bool _list1Predicted;

			#region Properties
			public bool List0Predicted { get { return _list0Predicted; } }
			public bool List1Predicted { get { return _list1Predicted; } }
			#endregion Properties

			private SubMbPredMode(bool list0Predicted, bool list1Predicted)
			{
				_list0Predicted = list0Predicted;
				_list1Predicted = list1Predicted;
			}
		}

		public static ISubMacroblockType GetSubMbTypeP(uint mbType)
		{
			return SubMbTypeP[mbType];
		}

		public static ISubMacroblockType GetSubMbTypeB(uint mbType)
		{
			return SubMbTypeB[mbType];
		}

		#region Table 7-17 – Sub-macroblock types in P macroblocks
		private static readonly SubMacroblockType[] SubMbTypeP =
		{
			//new SubMacroBlockTypeB(inferred, "na", na, na, na, na),
			new SubMacroblockType( 0, "P_L0_8x8", 1, SubMbPredMode.PredL0, 8, 8),
			new SubMacroblockType( 1, "P_L0_8x4", 2, SubMbPredMode.PredL0, 8, 4),
			new SubMacroblockType( 2, "P_L0_4x8", 2, SubMbPredMode.PredL0, 4, 8),
			new SubMacroblockType( 3, "P_L0_4x4", 4, SubMbPredMode.PredL0, 4, 4)
		};
		#endregion Table 7-17 – Sub-macroblock types in P macroblocks

		#region Table 7-18 – Sub-macroblock types in B macroblocks
		private static readonly SubMacroblockType[] SubMbTypeB =
		{
			//new SubMacroBlockTypeB(inferred, "mb_type", 4, SubMbPredMode.Direct, 4, 4),
			new SubMacroblockType( 0, "B_Direct_8x8", 4, SubMbPredMode.Direct, 4, 4),
			new SubMacroblockType( 1, "B_L0_8x8",     1, SubMbPredMode.PredL0, 8, 8),
			new SubMacroblockType( 2, "B_L1_8x8",     1, SubMbPredMode.PredL1, 8, 8),
			new SubMacroblockType( 3, "B_Bi_8x8",     1, SubMbPredMode.BiPred, 8, 8),
			new SubMacroblockType( 4, "B_L0_8x4",     2, SubMbPredMode.PredL0, 8, 4),
			new SubMacroblockType( 5, "B_L0_4x8",     2, SubMbPredMode.PredL0, 4, 8),
			new SubMacroblockType( 6, "B_L1_8x4",     2, SubMbPredMode.PredL1, 8, 4),
			new SubMacroblockType( 7, "B_L1_4x8",     2, SubMbPredMode.PredL1, 4, 8),
			new SubMacroblockType( 8, "B_Bi_8x4",     2, SubMbPredMode.BiPred, 8, 4),
			new SubMacroblockType( 9, "B_Bi_4x8",     2, SubMbPredMode.BiPred, 4, 8),
			new SubMacroblockType(10, "B_L0_4x4",     4, SubMbPredMode.PredL0, 4, 4),
			new SubMacroblockType(11, "B_L1_4x4",     4, SubMbPredMode.PredL1, 4, 4),
			new SubMacroblockType(12, "B_Bi_4x4",     4, SubMbPredMode.BiPred, 4, 4)
		};
		#endregion Table 7-18 – Sub-macroblock types inB macroblocks

		// ReSharper disable UnaccessedField.Local
		private readonly byte _mbType;
		// ReSharper restore UnaccessedField.Local
		private readonly string _name;
		private readonly byte _numSubMbPart;
		private readonly SubMbPredMode _subMbPredMode;
		private readonly MacroblockPartitioning _macroblockPartitioning;

		#region Properties
		public uint NumSubMbPart { get { return _numSubMbPart; } }
		public bool IsDirect { get { return (_subMbPredMode == SubMbPredMode.Direct); } }
		public bool List0Predicted { get { return _subMbPredMode.List0Predicted; } }
		public bool List1Predicted { get { return _subMbPredMode.List1Predicted; } }
		public MacroblockPartitioning MacroblockPartitioning { get { return _macroblockPartitioning; } }
		#endregion Properties

		private SubMacroblockType(byte mbType, string name, byte numSubMbPart, SubMbPredMode direct, byte subMbPartPredWidth, byte subMbPartPredHeight)
		{
			_mbType = mbType;
			_name = name;
			_numSubMbPart = numSubMbPart;
			_subMbPredMode = direct;
			_macroblockPartitioning = MacroblockPartitioning.GetMacroblockPartitioning(subMbPartPredWidth, subMbPartPredHeight);
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
