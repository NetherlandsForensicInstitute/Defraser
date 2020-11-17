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
using Defraser.Detector.Common;
using Defraser.Util;

namespace Defraser.Detector.Mpeg4
{
	public struct MP4Vlc1
	{
		public MP4Vlc1(uint code, ushort len)
		{
			this.code = code;
			this.len = len;
		}
		public uint code;
		public ushort len;
	}

	internal class VopBase : Mpeg4Header
	{
		public new enum Attribute
		{
			ID,
			CodingType,
			TimeIncrement,
			ModuloTimeBase,
			ShortVideoHeader,
			TemporalReference,
			RoundingType,
			ReducedResolution,
			Width,
			Height,
			HorizontalMcSpatialRef,
			VerticalMcSpatialRef,
			BackgroundComposition,
			ChangeConvRatioDisable,
			ConstantAlpha,
			ConstantAlphaValue,

			DcecsOpaque,
			DcecsTransparent,
			DcecsIntraCae,
			DcecsInterCae,
			DcecsNoUpdate,
			DcecsUpsampling,
			DcecsIntraBlocks,
			DcecsNotCodedBlocks,
			DcecsDctCoefs,
			DcecsDctLines,
			DcecsVlcSymbols,
			DcecsVlcBits,
			DcecsInterBlocks,
			DcecsInter4vBlocks,
			DcecsApm,
			DcecsNpm,
			DcecsForwBackMcQ,
			DcecsHalfpel2,
			DcecsHalfpel4,
			DcecsInterpolateMcQ,
			DcecsSadct,
			DcecsQuarterpel,

			IntraDcVlcThr,
			TopFieldFirst,
			AlternateVerticalScanFlag,

			BrightnessChangeFactor,
			FCodeForward,
			FCodeBackward,
			ShapeCodingType,
			RefSelectCode,
			MarcoBlockNumber,
			QuantScale,
			HeaderExtensionCode,
			ResyncMarker,
			PTypeSourceFormat,
			PlusPTypeSourceFormat,
		}

#if DEBUG
		static protected readonly string Mpeg4H263 = "MPEG4_H263";
		static protected readonly bool _mpeg4H263DebugInfo = false;
#endif // DEBUG

		static private ReverseEvent[][] DCT3D;
		static private MP4Vlc1[][][][] CoefficientVlc;
		static private readonly VlcTable[][] VlcLookUpTable = new VlcTable[2][];

		protected enum Mpeg4VopType : byte
		{
			I_VOP = 0,
			P_VOP = 1,
			B_VOP = 2,
			S_VOP = 3
		}

		protected enum MacroblockType
		{
			Direct = 1,
			Inter = 0,
			Interpolate = 2,
			InterQ = 1,
			Backward = 3,
			Forward = 4,
			Intra = 3,
			IntraQ = 4,
			Stuffing = 5
		}

		protected byte _codingType = 0xFF;
		protected uint _vopWidth;
		protected uint _vopHeight;
		protected uint _intraDcVlcThr;
		protected uint _pQuant;
		protected uint _fcodeForward;
		protected uint _fcodeBackward;
		protected uint _refSelectCode;
		protected uint _quantScale;
		protected uint _macroblockNum;
		protected bool _rvlc;
		protected uint _numBitsMacroBlock;

		static protected bool _optionalModifiedQuantization;

		/* tables */
		static protected uint[] mp4_DC_vlc_Threshold = { 512, 13, 15, 17, 19, 21, 23, 0 };
		static private readonly byte[] mp4_PVOPmb_type = { 255, 255, 4, 4, 4, 1, 3, 3, 3, 3, 2, 2, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
		static private readonly byte[] mp4_PVOPmb_cbpc = { 0, 0, 3, 2, 1, 3, 2, 2, 1, 1, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		static private readonly byte[] mp4_PVOPmb_bits = { 9, 9, 9, 9, 9, 9, 8, 8, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
		static private readonly MP4Vlc1[] mp4_BVOPmb_type = new MP4Vlc1[] { new MP4Vlc1(255, 255), new MP4Vlc1((uint)MacroblockType.Forward, 4), new MP4Vlc1((uint)MacroblockType.Backward, 3), new MP4Vlc1((uint)MacroblockType.Backward, 3), new MP4Vlc1((uint)MacroblockType.Interpolate, 2), new MP4Vlc1((uint)MacroblockType.Interpolate, 2), new MP4Vlc1((uint)MacroblockType.Interpolate, 2), new MP4Vlc1((uint)MacroblockType.Interpolate, 2), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1), new MP4Vlc1((uint)MacroblockType.Direct, 1) };
		static private readonly MP4Vlc1[] mp4_cbpy4 = new MP4Vlc1[] { new MP4Vlc1(16, 255), new MP4Vlc1(16, 255), new MP4Vlc1(6, 6), new MP4Vlc1(9, 6), new MP4Vlc1(8, 5), new MP4Vlc1(8, 5), new MP4Vlc1(4, 5), new MP4Vlc1(4, 5), new MP4Vlc1(2, 5), new MP4Vlc1(2, 5), new MP4Vlc1(1, 5), new MP4Vlc1(1, 5), new MP4Vlc1(0, 4), new MP4Vlc1(0, 4), new MP4Vlc1(0, 4), new MP4Vlc1(0, 4), new MP4Vlc1(12, 4), new MP4Vlc1(12, 4), new MP4Vlc1(12, 4), new MP4Vlc1(12, 4), new MP4Vlc1(10, 4), new MP4Vlc1(10, 4), new MP4Vlc1(10, 4), new MP4Vlc1(10, 4), new MP4Vlc1(14, 4), new MP4Vlc1(14, 4), new MP4Vlc1(14, 4), new MP4Vlc1(14, 4), new MP4Vlc1(5, 4), new MP4Vlc1(5, 4), new MP4Vlc1(5, 4), new MP4Vlc1(5, 4), new MP4Vlc1(13, 4), new MP4Vlc1(13, 4), new MP4Vlc1(13, 4), new MP4Vlc1(13, 4), new MP4Vlc1(3, 4), new MP4Vlc1(3, 4), new MP4Vlc1(3, 4), new MP4Vlc1(3, 4), new MP4Vlc1(11, 4), new MP4Vlc1(11, 4), new MP4Vlc1(11, 4), new MP4Vlc1(11, 4), new MP4Vlc1(7, 4), new MP4Vlc1(7, 4), new MP4Vlc1(7, 4), new MP4Vlc1(7, 4), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2), new MP4Vlc1(15, 2) };
		static private readonly int[] mp4_dquant = { -1, -2, 1, 2 };

		private static readonly VlcTable[][] CoefficientTable = new VlcTable[][]
		{
			/* intra = 0 */
			new VlcTable [] {
				new VlcTable( 2,  2,0, 0, 1),
				new VlcTable(15,  4,0, 0, 2),
				new VlcTable(21,  6,0, 0, 3),
				new VlcTable(23,  7,0, 0, 4),
				new VlcTable(31,  8,0, 0, 5),
				new VlcTable(37,  9,0, 0, 6),
				new VlcTable(36,  9,0, 0, 7),
				new VlcTable(33, 10,0, 0, 8),
				new VlcTable(32, 10,0, 0, 9),
				new VlcTable( 7, 11,0, 0, 10),
				new VlcTable( 6, 11,0, 0, 11),
				new VlcTable(32, 11,0, 0, 12),
				new VlcTable( 6,  3,0, 1, 1),
				new VlcTable(20,  6,0, 1, 2),
				new VlcTable(30,  8,0, 1, 3),
				new VlcTable(15, 10,0, 1, 4),
				new VlcTable(33, 11,0, 1, 5),
				new VlcTable(80, 12,0, 1, 6),
				new VlcTable(14,  4,0, 2, 1),
				new VlcTable(29,  8,0, 2, 2),
				new VlcTable(14, 10,0, 2, 3),
				new VlcTable(81, 12,0, 2, 4),
				new VlcTable(13,  5,0, 3, 1),
				new VlcTable(35,  9,0, 3, 2),
				new VlcTable(13, 10,0, 3, 3),
				new VlcTable(12,  5,0, 4, 1),
				new VlcTable(34,  9,0, 4, 2),
				new VlcTable(82, 12,0, 4, 3),
				new VlcTable(11,  5,0, 5, 1),
				new VlcTable(12, 10,0, 5, 2),
				new VlcTable(83, 12,0, 5, 3),
				new VlcTable(19,  6,0, 6, 1),
				new VlcTable(11, 10,0, 6, 2),
				new VlcTable(84, 12,0, 6, 3),
				new VlcTable(18,  6,0, 7, 1),
				new VlcTable(10, 10,0, 7, 2),
				new VlcTable(17,  6,0, 8, 1),
				new VlcTable( 9, 10,0, 8, 2),
				new VlcTable(16,  6,0, 9, 1),
				new VlcTable( 8, 10,0, 9, 2),
				new VlcTable(22,  7,0, 10, 1),
				new VlcTable(85, 12,0, 10, 2),
				new VlcTable(21,  7,0, 11, 1),
				new VlcTable(20,  7,0, 12, 1),
				new VlcTable(28,  8,0, 13, 1),
				new VlcTable(27,  8,0, 14, 1),
				new VlcTable(33,  9,0, 15, 1),
				new VlcTable(32,  9,0, 16, 1),
				new VlcTable(31,  9,0, 17, 1),
				new VlcTable(30,  9,0, 18, 1),
				new VlcTable(29,  9,0, 19, 1),
				new VlcTable(28,  9,0, 20, 1),
				new VlcTable(27,  9,0, 21, 1),
				new VlcTable(26,  9,0, 22, 1),
				new VlcTable(34, 11,0, 23, 1),
				new VlcTable(35, 11,0, 24, 1),
				new VlcTable(86, 12,0, 25, 1),
				new VlcTable(87, 12,0, 26, 1),
				new VlcTable( 7,  4,1, 0, 1),
				new VlcTable(25,  9,1, 0, 2),
				new VlcTable( 5, 11,1, 0, 3),
				new VlcTable(15,  6,1, 1, 1),
				new VlcTable( 4, 11,1, 1, 2),
				new VlcTable(14,  6,1, 2, 1),
				new VlcTable(13,  6,1, 3, 1),
				new VlcTable(12,  6,1, 4, 1),
				new VlcTable(19,  7,1, 5, 1),
				new VlcTable(18,  7,1, 6, 1),
				new VlcTable(17,  7,1, 7, 1),
				new VlcTable(16,  7,1, 8, 1),
				new VlcTable(26,  8,1, 9, 1),
				new VlcTable(25,  8,1, 10, 1),
				new VlcTable(24,  8,1, 11, 1),
				new VlcTable(23,  8,1, 12, 1),
				new VlcTable(22,  8,1, 13, 1),
				new VlcTable(21,  8,1, 14, 1),
				new VlcTable(20,  8,1, 15, 1),
				new VlcTable(19,  8,1, 16, 1),
				new VlcTable(24,  9,1, 17, 1),
				new VlcTable(23,  9,1, 18, 1),
				new VlcTable(22,  9,1, 19, 1),
				new VlcTable(21,  9,1, 20, 1),
				new VlcTable(20,  9,1, 21, 1),
				new VlcTable(19,  9,1, 22, 1),
				new VlcTable(18,  9,1, 23, 1),
				new VlcTable(17,  9,1, 24, 1),
				new VlcTable( 7, 10,1, 25, 1),
				new VlcTable( 6, 10,1, 26, 1),
				new VlcTable( 5, 10,1, 27, 1),
				new VlcTable( 4, 10,1, 28, 1),
				new VlcTable(36, 11,1, 29, 1),
				new VlcTable(37, 11,1, 30, 1),
				new VlcTable(38, 11,1, 31, 1),
				new VlcTable(39, 11,1, 32, 1),
				new VlcTable(88, 12,1, 33, 1),
				new VlcTable(89, 12,1, 34, 1),
				new VlcTable(90, 12,1, 35, 1),
				new VlcTable(91, 12,1, 36, 1),
				new VlcTable(92, 12,1, 37, 1),
				new VlcTable(93, 12,1, 38, 1),
				new VlcTable(94, 12,1, 39, 1),
				new VlcTable(95, 12,1, 40, 1)
			},
			/* intra = 1 */
			new VlcTable [] {
				new VlcTable( 2,  2,0, 0, 1),
				new VlcTable(15,  4,0, 0, 3),
				new VlcTable(21,  6,0, 0, 6),
				new VlcTable(23,  7,0, 0, 9),
				new VlcTable(31,  8,0, 0, 10),
				new VlcTable(37,  9,0, 0, 13),
				new VlcTable(36,  9,0, 0, 14),
				new VlcTable(33, 10,0, 0, 17),
				new VlcTable(32, 10,0, 0, 18),
				new VlcTable( 7, 11,0, 0, 21),
				new VlcTable( 6, 11,0, 0, 22),
				new VlcTable(32, 11,0, 0, 23),
				new VlcTable( 6,  3,0, 0, 2),
				new VlcTable(20,  6,0, 1, 2),
				new VlcTable(30,  8,0, 0, 11),
				new VlcTable(15, 10,0, 0, 19),
				new VlcTable(33, 11,0, 0, 24),
				new VlcTable(80, 12,0, 0, 25),
				new VlcTable(14,  4,0, 1, 1),
				new VlcTable(29,  8,0, 0, 12),
				new VlcTable(14, 10,0, 0, 20),
				new VlcTable(81, 12,0, 0, 26),
				new VlcTable(13,  5,0, 0, 4),
				new VlcTable(35,  9,0, 0, 15),
				new VlcTable(13, 10,0, 1, 7),
				new VlcTable(12,  5,0, 0, 5),
				new VlcTable(34,  9,0, 4, 2),
				new VlcTable(82, 12,0, 0, 27),
				new VlcTable(11,  5,0, 2, 1),
				new VlcTable(12, 10,0, 2, 4),
				new VlcTable(83, 12,0, 1, 9),
				new VlcTable(19,  6,0, 0, 7),
				new VlcTable(11, 10,0, 3, 4),
				new VlcTable(84, 12,0, 6, 3),
				new VlcTable(18,  6,0, 0, 8),
				new VlcTable(10, 10,0, 4, 3),
				new VlcTable(17,  6,0, 3, 1),
				new VlcTable( 9, 10,0, 8, 2),
				new VlcTable(16,  6,0, 4, 1),
				new VlcTable( 8, 10,0, 5, 3),
				new VlcTable(22,  7,0, 1, 3),
				new VlcTable(85, 12,0, 1, 10),
				new VlcTable(21,  7,0, 2, 2),
				new VlcTable(20,  7,0, 7, 1),
				new VlcTable(28,  8,0, 1, 4),
				new VlcTable(27,  8,0, 3, 2),
				new VlcTable(33,  9,0, 0, 16),
				new VlcTable(32,  9,0, 1, 5),
				new VlcTable(31,  9,0, 1, 6),
				new VlcTable(30,  9,0, 2, 3),
				new VlcTable(29,  9,0, 3, 3),
				new VlcTable(28,  9,0, 5, 2),
				new VlcTable(27,  9,0, 6, 2),
				new VlcTable(26,  9,0, 7, 2),
				new VlcTable(34, 11,0, 1, 8),
				new VlcTable(35, 11,0, 9, 2),
				new VlcTable(86, 12,0, 2, 5),
				new VlcTable(87, 12,0, 7, 3),
				new VlcTable( 7,  4,1, 0, 1),
				new VlcTable(25,  9,0, 11, 1),
				new VlcTable( 5, 11,1, 0, 6),
				new VlcTable(15,  6,1, 1, 1),
				new VlcTable( 4, 11,1, 0, 7),
				new VlcTable(14,  6,1, 2, 1),
				new VlcTable(13,  6,0, 5, 1),
				new VlcTable(12,  6,1, 0, 2),
				new VlcTable(19,  7,1, 5, 1),
				new VlcTable(18,  7,0, 6, 1),
				new VlcTable(17,  7,1, 3, 1),
				new VlcTable(16,  7,1, 4, 1),
				new VlcTable(26,  8,1, 9, 1),
				new VlcTable(25,  8,0, 8, 1),
				new VlcTable(24,  8,0, 9, 1),
				new VlcTable(23,  8,0, 10, 1),
				new VlcTable(22,  8,1, 0, 3),
				new VlcTable(21,  8,1, 6, 1),
				new VlcTable(20,  8,1, 7, 1),
				new VlcTable(19,  8,1, 8, 1),
				new VlcTable(24,  9,0, 12, 1),
				new VlcTable(23,  9,1,  0, 4),
				new VlcTable(22,  9,1,  1, 2),
				new VlcTable(21,  9,1, 10, 1),
				new VlcTable(20,  9,1, 11, 1),
				new VlcTable(19,  9,1, 12, 1),
				new VlcTable(18,  9,1, 13, 1),
				new VlcTable(17,  9,1, 14, 1),
				new VlcTable( 7, 10,0, 13, 1),
				new VlcTable( 6, 10,1,  0, 5),
				new VlcTable( 5, 10,1,  1, 3),
				new VlcTable( 4, 10,1,  2, 2),
				new VlcTable(36, 11,1,  3, 2),
				new VlcTable(37, 11,1,  4, 2),
				new VlcTable(38, 11,1, 15, 1),
				new VlcTable(39, 11,1, 16, 1),
				new VlcTable(88, 12,0, 14, 1),
				new VlcTable(89, 12,1,  0, 8),
				new VlcTable(90, 12,1,  5, 2),
				new VlcTable(91, 12,1,  6, 2),
				new VlcTable(92, 12,1, 17, 1),
				new VlcTable(93, 12,1, 18, 1),
				new VlcTable(94, 12,1, 19, 1),
				new VlcTable(95, 12,1, 20, 1)
			}
		};

		/* constants taken from momusys/vm_common/inlcude/max_level.h */
		private static readonly ushort[][][] MaxLevel = new ushort[][][]{
			new ushort [][] {
				/* intra = 0, last = 0 */
				new ushort [] {
					12, 6, 4, 3, 3, 3, 3, 2,
					2, 2, 2, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				},
				/* intra = 0, last = 1 */
				new ushort [] {
					3, 2, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				}
			},
			new ushort [][] {
				/* intra = 1, last = 0 */
				new ushort [] {
					27, 10, 5, 4, 3, 3, 3, 3,
					2, 2, 1, 1, 1, 1, 1, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				},
				/* intra = 1, last = 1 */
				new ushort [] {
					8, 3, 2, 2, 2, 2, 2, 1,
					1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				}
			}
		};

		private static readonly ushort[][][] MaxRun = new ushort[][][]{
			new ushort [][] {
				/* intra = 0, last = 0 */
				new ushort [] {
					0, 26, 10, 6, 2, 1, 1, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
				},
				/* intra = 0, last = 1 */
				new ushort [] {
					0, 40, 1, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
				}
			},
			new ushort [][] {
				/* intra = 1, last = 0 */
				new ushort [] {
					0, 14, 9, 7, 3, 2, 1, 1,
					1, 1, 1, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
				},
				/* intra = 1, last = 1 */
				new ushort [] {
					0, 20, 6, 1, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0,
				}
			}
		};

		/// <summary>Static data initialization.</summary>
		static VopBase()
		{
			InitVLCTables();
		}


		protected struct Event
		{
			//public Event(ushort l, ushort r, ushort le)
			//{
			//    last = l;
			//    run = r;
			//    level = le;
			//}
			public ushort last;
			public ushort run;
			public ushort level;
		}


		protected struct ReverseEvent
		{
			public ushort len;
			public Event events;
		}

		protected struct VlcTable
		{
			public VlcTable(uint c, ushort len, ushort l, ushort r, ushort le)
			{
				vlc.code = c;
				vlc.len = len;
				events.last = l;
				events.run = r;
				events.level = le;
			}
			public MP4Vlc1 vlc;
			public Event events;
		}

		public VopBase(Mpeg4Header previousHeader, Mpeg4HeaderName mpeg4HeaderName)
			: base(previousHeader, mpeg4HeaderName)
		{
		}

		internal uint Width
		{
			get { return _vopWidth; }
		}

		internal uint Height
		{
			get { return _vopHeight; }
		}

		private static void InitVLCTables()
		{
			uint i, j, intra, last, run, run_esc, level, level_esc, escape, escape_len, offset;

			DCT3D = new ReverseEvent[2][];
			for (intra = 0; intra < 2; intra++)
			{
				VlcLookUpTable[intra] = new VlcTable[4096];
				DCT3D[intra] = new ReverseEvent[4096];
				for (i = 0; i < 4096; i++)
				{
					DCT3D[intra][i].events.level = 0;
				}
			}

			for (intra = 0; intra < 2; intra++)
			{
				for (i = 0; i < 102; i++)
				{
					uint index = CoefficientTable[intra][i].vlc.code << (12 - CoefficientTable[intra][i].vlc.len);
					uint end_offset = 0;
					for (j = 0; j < 12 - CoefficientTable[intra][i].vlc.len; j++)
					{
						end_offset <<= 1;
						end_offset += 1;
					}
					uint lastIndex = index + end_offset;
					while (index <= lastIndex)
					{
						VlcLookUpTable[intra][index] = new VlcTable(CoefficientTable[intra][i].vlc.code, (ushort)(CoefficientTable[intra][i].vlc.len + 1), CoefficientTable[intra][i].events.last, CoefficientTable[intra][i].events.run, CoefficientTable[intra][i].events.level);
						index++;
					}
				}
			}

			CoefficientVlc = new MP4Vlc1[2][][][];
			for (intra = 0; intra < 2; intra++)
			{
				CoefficientVlc[intra] = new MP4Vlc1[2][][];
				for (last = 0; last < 2; last++)
				{
					CoefficientVlc[intra][last] = new MP4Vlc1[64][];
					for (run = 0; run < 64; run++)
					{
						CoefficientVlc[intra][last][run] = new MP4Vlc1[64];
					}
				}
			}

			for (intra = 0; intra < 2; intra++)
			{
				for (last = 0; last < 2; last++)
				{
					for (run = 0; run < 63 + last; run++)
					{
						for (level = 0; level < (uint)(32 << (int)intra); level++)
						{
							if (intra == 0)
							{
								offset = 32;
							}
							else
							{
								offset = 0;
							}
							CoefficientVlc[intra][last][level + offset][run].len = 128;
						}
					}
				}
			}

			for (intra = 0; intra < 2; intra++)
			{
				for (i = 0; i < 102; i++)
				{

					if (intra == 0)
					{
						offset = 32;
					}
					else
					{
						offset = 0;
					}
					for (j = 0; j < (uint)(1 << (12 - CoefficientTable[intra][i].vlc.len)); j++)
					{
						DCT3D[intra][(int)((CoefficientTable[intra][i].vlc.code << (12 - CoefficientTable[intra][i].vlc.len)) | (uint)j)].len = CoefficientTable[intra][i].vlc.len;
						DCT3D[intra][(int)((CoefficientTable[intra][i].vlc.code << (12 - CoefficientTable[intra][i].vlc.len)) | (uint)j)].events = CoefficientTable[intra][i].events;
					}

					CoefficientVlc[intra][CoefficientTable[intra][i].events.last][CoefficientTable[intra][i].events.level + offset][CoefficientTable[intra][i].events.run].code
						= CoefficientTable[intra][i].vlc.code << 1;
					CoefficientVlc[intra][CoefficientTable[intra][i].events.last][CoefficientTable[intra][i].events.level + offset][CoefficientTable[intra][i].events.run].len
						= (ushort)(CoefficientTable[intra][i].vlc.len + 1);

					if (intra == 0)
					{
						CoefficientVlc[intra][CoefficientTable[intra][i].events.last][offset - CoefficientTable[intra][i].events.level][CoefficientTable[intra][i].events.run].code
							= (CoefficientTable[intra][i].vlc.code << 1) | 1;
						CoefficientVlc[intra][CoefficientTable[intra][i].events.last][offset - CoefficientTable[intra][i].events.level][CoefficientTable[intra][i].events.run].len
							= (ushort)(CoefficientTable[intra][i].vlc.len + 1);
					}
				}
			}

			for (intra = 0; intra < 2; intra++)
			{
				for (last = 0; last < 2; last++)
				{
					for (run = 0; run < 63 + last; run++)
					{
						const int ESCAPE3 = 15;

						for (level = 1; level < (uint)(32 << (int)intra); level++)
						{
							if (level <= MaxLevel[intra][last][run] && run <= MaxRun[intra][last][level])
								continue;

							if (intra == 0)
							{
								offset = 32;
							}
							else
							{
								offset = 0;
							}
							level_esc = level - MaxLevel[intra][last][run];
							run_esc = run - 1 - MaxRun[intra][last][level];

							if (level_esc <= MaxLevel[intra][last][run] && run <= MaxRun[intra][last][level_esc])
							{
								const int Escape1 = 6;
								escape = Escape1;
								escape_len = 7 + 1;
								run_esc = run;
							}
							else
							{
								if (run_esc <= MaxRun[intra][last][level] && level <= MaxLevel[intra][last][run_esc])
								{
									const int Escape2 = 14;
									escape = Escape2;
									escape_len = 7 + 2;
									level_esc = level;
								}
								else
								{
									if (intra == 0)
									{
										CoefficientVlc[intra][last][level + offset][run].code
											= (uint)((ESCAPE3 << 21) | (last << 20) | (run << 14) | (1 << 13) | ((level & 0xfff) << 1) | 1);
										CoefficientVlc[intra][last][level + offset][run].len = 30;
										CoefficientVlc[intra][last][offset - level][run].code
										= (uint)((ESCAPE3 << 21) | ((int)last << 20) | ((int)run << 14) | (1 << 13) | ((-(int)level & 0xfff) << 1) | 1);
										CoefficientVlc[intra][last][offset - level][run].len = 30;
									}
									continue;
								}
							}

							CoefficientVlc[intra][last][level + offset][run].code
								= (ushort)(((ushort)escape << CoefficientVlc[intra][last][level_esc + offset][run_esc].len) | (ushort)CoefficientVlc[intra][last][level_esc + offset][run_esc].code);
							CoefficientVlc[intra][last][level + offset][run].len
								= (ushort)(CoefficientVlc[intra][last][level_esc + offset][run_esc].len + escape_len);

							if (intra == 0)
							{
								CoefficientVlc[intra][last][offset - level][run].code
									= (ushort)(((ushort)escape << CoefficientVlc[intra][last][level_esc + offset][run_esc].len) | (ushort)CoefficientVlc[intra][last][level_esc + offset][run_esc].code | 1);
								CoefficientVlc[intra][last][offset - level][run].len
									= (ushort)(CoefficientVlc[intra][last][level_esc + offset][run_esc].len + escape_len);
							}
						}

						if (intra == 0)
						{
							CoefficientVlc[intra][last][0][run].code
								= (uint)((ESCAPE3 << 21) | (last << 20) | (run << 14) | (1 << 13) | ((-32 & 0xfff) << 1) | 1);
							CoefficientVlc[intra][last][0][run].len = 30;
						}
					}
				}
			}
		}

		static public int mp4_Sprite_Trajectory(Mpeg4Parser parser, VideoObjectLayer VOL)
		{
			short i, dmv_code, dmv_length, fb;
			uint code;
			int[] warpingMvCodeDu = new int[4];
			int[] warpinpMvCodeDv = new int[4];

			for (i = 0; i < VOL.SpriteWarpingPoints; i++)
			{
				code = parser.ShowBits(3);
				if (code == 7)
				{
					parser.FlushBits(3);
					code = parser.ShowBits(9);
					fb = 1;
					while ((code & 256) > 0)
					{
						code <<= 1;
						fb++;
					}
					if (fb > 9)
					{
						return 0;
					}
					dmv_length = (short)(fb + 5);
				}
				else
				{
					fb = (short)((code <= 1) ? 2 : 3);
					dmv_length = (short)(code - 1);
				}
				parser.FlushBits(fb);
				if (dmv_length <= 0)
				{
					dmv_code = 0;
				}
				else
				{
					dmv_code = (short)parser.GetBits(dmv_length);
					if ((dmv_code & (1 << (dmv_length - 1))) == 0)
					{
						dmv_code -= (short)((1 << dmv_length) - 1);
					}
				}
				if (!parser.GetMarkerBit())
				{
					return 0;
				}

				warpingMvCodeDu[i] = dmv_code;
				code = parser.ShowBits(3);
				if (code == 7)
				{
					parser.FlushBits(3);
					code = parser.ShowBits(9);
					fb = 1;
					while ((code & 256) > 0)
					{
						code <<= 1;
						fb++;
					}
					if (fb > 9)
					{
						return 0;
					}
					dmv_length = (short)(fb + 5);
				}
				else
				{
					fb = (short)((code <= 1) ? 2 : 3);
					dmv_length = (short)(code - 1);
				}
				parser.FlushBits(fb);
				if (dmv_length <= 0)
				{
					dmv_code = 0;
				}
				else
				{
					dmv_code = (short)parser.GetBits(dmv_length);
					if ((dmv_code & (1 << (dmv_length - 1))) == 0)
					{
						dmv_code -= (short)((1 << dmv_length) - 1);
					}
				}
				if (!parser.GetMarkerBit()) return 0;
				warpinpMvCodeDv[i] = dmv_code;
			}
			return 1;
		}

		protected bool HandleMacroBlock(Mpeg4Parser parser, VideoObjectLayer VOL, ref uint column, ref uint row)
		{
			// Structure of macroblock layer: COD MCBPC MODB CBPB CBPY DQUANT MVD MVD2 MVD3 MVD4 MVDB Block Data

			if (VOL == null)
			{
				// Cannot decode de VOP if no VOL is present
				return false;
			}

			uint mbPerRow = (VOL.Width + 15) >> 4;
			uint mbPerCol = (VOL.Height + 15) >> 4;
			int cbpc = 0;
			int cbpy = 0;
			int quant = (int) _pQuant;
			int mb_type = 0;
			bool mb_not_coded = true;
			bool mcsel = false;
			//bool ac_pred_flag = false;
			//bool field_dct = false;
			bool field_prediction = false;

			if (_vopHeight != 0 && _vopWidth != 0)
			{
				mbPerRow = (_vopWidth + 15) >> 4;
				mbPerCol = (_vopHeight + 15) >> 4;
			}
			_numBitsMacroBlock = mp4_GetMacroBlockNumberSize((int) (mbPerCol * mbPerRow));

			if (_codingType != (int)Mpeg4VopType.B_VOP)
			{
				if (VOL.VideoShape != VideoObjectLayer.Shape.Rectangular &&
					VOL.SpriteEnable == VideoObjectLayer.MP4SpriteStatic)
				{
					//mb_binary shape_coding()
				}
				if (VOL.VideoShape != VideoObjectLayer.Shape.BinaryOnly)
				{
					if (VOL.VideoShape != VideoObjectLayer.Shape.Rectangular &&
						!(VOL.SpriteEnable == VideoObjectLayer.MP4SpriteStatic && VOL.LowLatencySpriteEnabled))
					{
						do
						{
							mb_not_coded = false;
							if (_codingType != (int)Mpeg4VopType.I_VOP)
							{
								mb_not_coded = parser.GetBit();
							}

							if (mb_not_coded != false)
							{
								mb_type = (int)MacroblockType.Inter;
							}

							if (mb_not_coded == false || _codingType == (int)Mpeg4VopType.I_VOP ||
								(_codingType == (int)Mpeg4VopType.S_VOP && VOL.LowLatencySpriteEnabled))
							{
								if (!DeriveMacroblockType(parser, out mb_type, out cbpc))
								{
									return false;
								}
							}
						}
						while (!(mb_not_coded == true || mb_type != (int)MacroblockType.Stuffing));
					}
					else
					{
						if (_codingType != (int)Mpeg4VopType.I_VOP && !(VOL.SpriteEnable == VideoObjectLayer.MP4SpriteStatic))
						{
							mb_not_coded = parser.GetBit();
						}

						if (mb_not_coded == false || _codingType == (int)Mpeg4VopType.I_VOP ||
								(_codingType == (int)Mpeg4VopType.S_VOP && VOL.LowLatencySpriteEnabled))
						{
							if (!DeriveMacroblockType(parser, out mb_type, out cbpc))
							{
								return false;
							}
						}
					}

					if (mb_type != (int) MacroblockType.Stuffing && (mb_not_coded == false || _codingType == (int)Mpeg4VopType.I_VOP ||
								(_codingType == (int)Mpeg4VopType.S_VOP && VOL.LowLatencySpriteEnabled)))
					{
						if (_codingType == (int)Mpeg4VopType.S_VOP && VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC && mb_type <= 1)
						{
							mcsel = parser.GetBit();
						}
						if (!VOL.IsShortVideoHeader && mb_type >= (int)MacroblockType.Intra)
						{
							/*ac_pred_flag = */parser.GetBit();
						}

						//Decode CBPY 
						if (!DecodeCBPY(parser, out cbpy, mb_type))
						{
							return false;
						}

						if (mb_type == (int)MacroblockType.IntraQ || mb_type == (int)MacroblockType.InterQ)
						{
							quant += GetDbquant(parser);
							quant = MP4Clip(quant, 1, 1 << (VOL.QuantPrecision - 1));
						}
						if (VOL.Interlaced == true)
						{
							/*field_dct = */parser.GetBit();
							if (mb_type != (int)MacroblockType.Direct)
							{
								field_prediction = parser.GetBit();
								if (field_prediction)
								{
									if (mb_type != (int)MacroblockType.Backward)
									{
										parser.GetBit();
										parser.GetBit();
									}
									if (mb_type != (int)MacroblockType.Forward)
									{
										parser.GetBit();
										parser.GetBit();
									}
								}
							}

						}
						if (!(_refSelectCode == 3 && VOL.Scalability && VOL.SpriteEnable != VideoObjectLayer.MP4SpriteStatic))
						{
							if (mb_type <= 1 && (_codingType == (int)Mpeg4VopType.P_VOP || _codingType == (int)Mpeg4VopType.S_VOP && mcsel == false))
							{
								parser.GetMotionVector(_fcodeForward);
								if (field_prediction && VOL.Interlaced)
								{
									parser.GetMotionVector(_fcodeForward);
								}
							}
							if (mb_type == 2)
							{
								for (int i = 0; i < 4; i++)
								{
									parser.GetMotionVector(_fcodeForward);
								}
							}
						}
						// block decoding
						DecodeBlock(parser, VOL, mb_type, (cbpy << 2) + cbpc, quant < mp4_DC_vlc_Threshold[_intraDcVlcThr]);
					}
				}
			}
			else
			{
				if (VOL.VideoShape != VideoObjectLayer.Shape.Rectangular)
				{
					//binary shape_coding
				}
				byte codedBlockPatternForBBlocks = 0; // (CBPB)
				if (parser.GetBit() == false)
				{ /* modb=='0' */
					bool modb2 = parser.GetBit();

					uint code = parser.ShowBits(4);
					if (code != 0)
					{
						mb_type = (int) mp4_BVOPmb_type[code].code;
						parser.FlushBits((short) mp4_BVOPmb_type[code].len);
					}
					else
					{
						return false;
					}

					if (modb2 == false)
					{   /* modb=='00' */
						// Coded Block Pattern for B-blocks (CBPB) (6 bits)
						codedBlockPatternForBBlocks = (byte)parser.GetBits(6);
					}
					
					if (_refSelectCode != 0 || (VOL != null && VOL.Scalability == false))
					{
						if (mb_type != (int) MacroblockType.Direct && codedBlockPatternForBBlocks != 0)
						{
							quant += GetDbquant(parser);
							if (quant > 31)
								quant = 31;
							else if (quant < 1)
								quant = 1;
						}

						if (VOL != null && VOL.Interlaced)
						{
							if (codedBlockPatternForBBlocks != 0)
							{
								/*field_dct = */parser.GetBit();
							}

							if (mb_type != (int)MacroblockType.Direct)
							{
								field_prediction = parser.GetBit();
								if (field_prediction)
								{
									if (mb_type != (int)MacroblockType.Backward)
									{
										parser.GetBit();
										parser.GetBit();
									}
									if (mb_type != (int)MacroblockType.Forward)
									{
										parser.GetBit();
										parser.GetBit();
									}
								}
							}
						}

						switch (mb_type)
						{
							case (int)MacroblockType.Direct:
								parser.GetMotionVector(1);
								break;
							case (int)MacroblockType.Interpolate:
								parser.GetMotionVector(_fcodeForward);
								if (field_prediction && VOL.Interlaced)
								{
									parser.GetMotionVector(_fcodeForward);
								}
								parser.GetMotionVector(_fcodeBackward);
								if (field_prediction && VOL.Interlaced)
								{
									parser.GetMotionVector(_fcodeBackward);
								}
								break;

							case (int)MacroblockType.Backward:
								parser.GetMotionVector(_fcodeBackward);
								if (field_prediction && VOL.Interlaced)
								{
									parser.GetMotionVector(_fcodeBackward);
								}
								break;

							case (int)MacroblockType.Forward:
								parser.GetMotionVector(_fcodeForward);
								if (field_prediction && VOL.Interlaced)
								{
									parser.GetMotionVector(_fcodeForward);
								}
								break;

							default:
								break;
						}
					}
					if (_refSelectCode == 0 && VOL.Scalability && codedBlockPatternForBBlocks != 0)
					{
						quant += GetDbquant(parser);
						quant = MP4Clip(quant, 1, 1 << (VOL.QuantPrecision - 1));
						if (mb_type == (int)MacroblockType.Direct || mb_type == (int)MacroblockType.Interpolate)
						{
							parser.GetMotionVector(_fcodeForward);
						}
					}
					// block decoding
					DecodeBlock(parser, VOL, mb_type, (int) codedBlockPatternForBBlocks, quant < mp4_DC_vlc_Threshold[_intraDcVlcThr]);
				}
				else
				{
					//modb = 2;
					mb_type = (int)MacroblockType.Direct;
					codedBlockPatternForBBlocks = 0;
				}
			}

			if (VOL.VideoShape == VideoObjectLayer.Shape.Grayscale)
			{
				int cbpa = 0;
				for (int k = 0; k < VOL.AuxCompCount; k++)
				{
					bool alphaBlock = false;
					if (_codingType == (int)Mpeg4VopType.I_VOP
						|| ((_codingType == (int)Mpeg4VopType.P_VOP || (_codingType == (int)Mpeg4VopType.S_VOP && VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC)) && mb_not_coded == false && (mb_type >= 3)))
					{
						if (parser.GetBit() == false)
						{
							alphaBlock = true;
							/*ac_pred_flag = */parser.GetBit();
							if (!DecodeCBPY(parser, out cbpa, mb_type))
							{
								return false;
							}
						}
					}
					else if (_codingType == (int)Mpeg4VopType.P_VOP
						|| (VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC && (_codingType == (int)Mpeg4VopType.S_VOP)) || mb_not_coded == false)
					{
						if (parser.GetBit() == false)
						{
							if (parser.GetBit() == false)
							{
								if (!DecodeCBPY(parser, out cbpa, mb_type))
								{
									return false;
								}
								alphaBlock = true;
							}
						}
					}

					if (alphaBlock)
					{
						DecodeAlphaBlock(parser, VOL, mb_type, mb_not_coded, cbpa);
					}
				}
			}
			column++;
			if (column == mbPerCol)
			{
				column = 0;
				row++;
			}
			else if (!VOL.ResyncMarkerDisable)
			{
				if (mp4_CheckDecodeVideoPacket(parser, VOL))
				{
					quant = (int)_quantScale;
					row = _macroblockNum / mbPerRow;
					column = _macroblockNum % mbPerRow;
				}
			}
			return true;
		}

		protected bool DecodeCBPY(Mpeg4Parser parser, out int cbpy, int mb_type)
		{
			uint code = parser.ShowBits(6);

			cbpy = 0;
			if (_codingType == (int)Mpeg4VopType.I_VOP)
			{
				cbpy = (int)mp4_cbpy4[code].code;
				if (mp4_cbpy4[code].len == 255)
				{
					return false;
				}
				parser.FlushBits((short)mp4_cbpy4[code].len);
			}
			else if (_codingType != (int)Mpeg4VopType.B_VOP)
			{
				if (mb_type < (int)MacroblockType.Intra)
					cbpy = (int)(15 - mp4_cbpy4[code].code);
				else
				{
					cbpy = (int)mp4_cbpy4[code].code;
				}
				if (mp4_cbpy4[code].len == 255)
				{
					return false;
				}
				parser.FlushBits((short)mp4_cbpy4[code].len);
			}

			return true;
		}

		protected void DecodeBlock(Mpeg4Parser parser, VideoObjectLayer VOL, int mb_type, int cbp, bool dcVlc)
		{
			int start_coeff = 0;
			int pm = 32;
			for (int i = 0; i < 6; i++)
			{
				if (!VOL.DataPartitioned && mb_type >= (int)MacroblockType.Intra && _codingType != (int)Mpeg4VopType.B_VOP)
				{
					if (VOL.IsShortVideoHeader)
					{
						// Intra dc coeeff.
						parser.GetBits(8);
					}
					else if (dcVlc)
					{
						start_coeff = 1;
						parser.GetLuminanceChrominance(i);
					}
				}
				if ((cbp & pm) > 0)
				{
					if (mb_type >= (int)MacroblockType.Intra && _codingType != (int)Mpeg4VopType.B_VOP)
					{
						GetIntraBlock(parser, start_coeff, VOL.IsShortVideoHeader);
					}
					else
					{
						GetInterBlock(parser, VOL.IsShortVideoHeader);
					}
				}
				pm >>= 1;
			}
		}

		static protected uint mp4_GetMacroBlockNumberSize(int nmb)
		{
			uint nb = 0;
			nmb--;
			do
			{
				nmb >>= 1;
				nb++;
			} while (nmb > 0);
			return nb;
		}

		private void GetInterBlock(Mpeg4Parser parser, bool shortVideo)
		{

			int p = 0;
			//int level = 0;
			int run = 0;
			int last = 0;
			do
			{
				/*level = */GetCoeff(parser, ref run, ref last, 0, shortVideo, _rvlc);
				p += run;
				if (p >= 64)
				{
					break;
				}
				p++;
			} while (last == 0);
		}

		private static int GetCoeff(Mpeg4Parser parser, ref int run, ref int last, int intra, bool short_video_header, bool rvlc)
		{
			const int ESCAPE = 3;

			if (rvlc)
			{
				uint mode;
				int level;
				ReverseEvent reverse_event;

				uint cache = parser.ShowBits(32);

				if (short_video_header)		/* inter-VLCs will be used for both intra and inter blocks */
					intra = 0;

				if (GET_BITS(cache, 7) != ESCAPE)
				{
					reverse_event = DCT3D[intra][GET_BITS(cache, 12)];

					if ((level = reverse_event.events.level) == 0)
					{
						run = 64;
						return 0;
					}

					last = reverse_event.events.last;
					run = reverse_event.events.run;

					/* Don't forget to update the bitstream position */
					parser.FlushBits((short)(reverse_event.len + 1));

					return ((GET_BITS(cache, (short)(reverse_event.len + 1)) & 0x01) != 0) ? -level : level;
				}

				/* flush 7bits of cache */
				cache <<= 7;

				if (short_video_header)
				{
					/* escape mode 4 - H.263 type, only used if short_video_header = 1  */
					last = (int)GET_BITS(cache, 1);
					run = (int)(GET_BITS(cache, 7) & 0x3f);
					level = (int)(GET_BITS(cache, 15) & 0xff);

					/* We've "eaten" 22 bits */
					parser.FlushBits(22);

					return (level << 24) >> 24;
				}

				if ((mode = GET_BITS(cache, 2)) < 3)
				{
					int[] skip = new int[] { 1, 1, 2 };
					cache <<= skip[mode];

					reverse_event = DCT3D[intra][GET_BITS(cache, 12)];

					if ((level = reverse_event.events.level) == 0)
					{
						run = 64;
						return 0;
					}

					last = reverse_event.events.last;
					run = reverse_event.events.run;

					if (mode < 2)
					{
						/* first escape mode, level is offset */
						level += MaxLevel[intra][last][run];
					}
					else
					{
						/* second escape mode, run is offset */
						run += MaxRun[intra][last][level] + 1;
					}

					/* Update bitstream position */
					parser.FlushBits((short)(7 + skip[mode] + reverse_event.len + 1));

					return ((GET_BITS(cache, (short)(reverse_event.len + 1)) & 0x01) != 0) ? -level : level;
				}

				/* third escape mode - fixed length codes */
				cache <<= 2;
				last = (int)GET_BITS(cache, 1);
				run = (int)(GET_BITS(cache, 7) & 0x3f);
				level = (int)(GET_BITS(cache, 20) & 0xfff);

				/* Update bitstream position */
				parser.FlushBits(30);

				return (level << 20) >> 20;
			}
			else
			{
				if (short_video_header)
				{
					intra = 0;
				}
				uint cache = parser.ShowBits(12);
				if (VlcLookUpTable[intra][cache].vlc.code != 0)
				{
					parser.FlushBits((short)(VlcLookUpTable[intra][cache].vlc.len));
					run = VlcLookUpTable[intra][cache].events.run;
					last = VlcLookUpTable[intra][cache].events.last;
					return VlcLookUpTable[intra][cache].events.level;
				}

				// Escape sequence
				parser.FlushBits(7);
				if (short_video_header)
				{
					const int EXTENDED_ESCAPE_CODE = 0x80;
					last = (parser.GetBit() == true) ? 1 : 0;
					run = (int)parser.GetBits(6);
					cache = parser.GetBits(8);
					if (cache == EXTENDED_ESCAPE_CODE)
					{
						if (_optionalModifiedQuantization == false) // Modified Quantization should be '1' when 
						{
#if DEBUG
							Debug.WriteLineIf(_mpeg4H263DebugInfo, "LEVEL is 0x80 and Modified Quantization mode is '0', Modified Quantization mode should be '1' when LEVEL has the value of 0x80", Mpeg4H263);
#endif // DEBUG
						}
						// Flush EXTENDED-LEVEL (11 bits)
						parser.FlushBits(11);
					}
					return (int)cache;
				}

				if (parser.GetBit() == false)
				{
					cache = parser.ShowBits(12);
					parser.FlushBits((short)(VlcLookUpTable[intra][cache].vlc.len));
					last = VlcLookUpTable[intra][cache].events.last;
					run = VlcLookUpTable[intra][cache].events.run;
					return (VlcLookUpTable[intra][cache].events.level + MaxLevel[intra][last][run]);
				}
				else if (parser.GetBit() == false)
				{
					cache = parser.ShowBits(12);
					parser.FlushBits((short)(VlcLookUpTable[intra][cache].vlc.len));
					last = VlcLookUpTable[intra][cache].events.last;
					run = VlcLookUpTable[intra][cache].events.run + MaxRun[intra][last][VlcLookUpTable[intra][cache].events.level] + 1;
					return VlcLookUpTable[intra][cache].events.level;
				}

				last = (parser.GetBit() == true) ? 1 : 0;
				run = (int)parser.GetBits(6);
				parser.GetMarkerBit();
				cache = parser.GetBits(12);
				parser.GetMarkerBit();
				return (int)cache;
			}
		}

		protected bool DeriveMacroblockType(Mpeg4Parser parser, out int mb_type, out int cbpc)
		{
			short fb = 0;
			int code = (int)parser.ShowBits(9);
			if (_codingType == (int)Mpeg4VopType.P_VOP || _codingType == (int)Mpeg4VopType.S_VOP)
			{
				if (code >= 256)
				{
					mb_type = (int)MacroblockType.Inter;
					cbpc = 0;
					parser.FlushBits(1);
				}
				else
				{
					mb_type = mp4_PVOPmb_type[code];
					cbpc = mp4_PVOPmb_cbpc[code];
					parser.FlushBits(mp4_PVOPmb_bits[code]);
				}
				if (code == 0)
				{
					return false;// error
				}
			}
			else
			{
				if (code == 1)
				{
					mb_type = (int)MacroblockType.Stuffing;
					cbpc = 0;
					fb = 9;
				}
				else if (code >= 64)
				{
					mb_type = (int)MacroblockType.Intra;
					cbpc = code >> 6;
					if (cbpc >= 4)
					{
						cbpc = 0;
						fb = 1;
					}
					else
						fb = 3;
				}
				else
				{
					mb_type = (int)MacroblockType.IntraQ;
					cbpc = code >> 3;
					if (cbpc >= 4)
					{
						cbpc = 0;
						fb = 4;
					}
					else if (code >= 8)
					{
						fb = 6;
					}
					else
					{
						return false; // error	
					}
				}
				parser.FlushBits(fb);
			}
			return true;
		}

		/* for decode B-frame dbquant */
		protected int GetDbquant(Mpeg4Parser parser)
		{
			if (_codingType == (int)Mpeg4VopType.B_VOP)
			{
				if (parser.GetBit() == false)		/*  '0' */
					return (0);
				else if (parser.GetBit() == false)	/* '10' */
					return (-2);
				else								/* '11' */
					return (2);
			}
			else
			{
				return mp4_dquant[parser.GetBits(2)];
			}
		}

		static protected int MP4Clip(int x, int min, int max)
		{
			if (x < min)
			{
				x = min;
			}
			else if (x > max)
			{
				x = max;
			}
			return x;
		}

		private void DecodeAlphaBlock(Mpeg4Parser parser, VideoObjectLayer VOL, int mb_type, bool mb_not_coded, int cbp)
		{
			int start_coeff = 0;
			for (int i = 0; i < 4; i++)
			{
				if (!VOL.DataPartitioned
					&& (_codingType == (int)Mpeg4VopType.I_VOP || (_codingType == (int)Mpeg4VopType.P_VOP || (_codingType == (int)Mpeg4VopType.S_VOP && VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC))
					&& mb_not_coded == false && mb_type >= (int)MacroblockType.Intra))
				{
					start_coeff = 1;
					int alpha = parser.GetDCSizeLum();
					if (alpha != 0)
					{
						parser.GetBits((short)alpha);
						if (alpha > 8)
						{
							parser.GetMarkerBit();
						}
					}
				}
				if ((cbp & (1 << (5 - i))) > 0)
				{
					if (mb_type >= (int)MacroblockType.Intra)
					{
						GetIntraBlock(parser, start_coeff, VOL.IsShortVideoHeader);
					}
					else
					{
						GetInterBlock(parser, VOL.IsShortVideoHeader);
					}
				}
			}
		}

		private void GetIntraBlock(Mpeg4Parser parser, int coeff, bool shortVideo)
		{
			//int level = 0;
			int run = 0;
			int last = 0;

			do
			{
				/*level = */GetCoeff(parser, ref run, ref last, 1, shortVideo, _rvlc);
				coeff += run;
				if (coeff >= 64)
				{
					break;
				}
				coeff++;
			} while (last == 0);
		}

		protected bool mp4_CheckDecodeVideoPacket(Mpeg4Parser parser, VideoObjectLayer vol)
		{
			// Note see page 56 of ISO-IEC 14496-2-2004-MPEG-4-Video
			// for the definition of function video_packet_header()

			// Store the current bt position
			Pair<byte, long> bitPosition = parser.BitPosition;

			// next_resync_marker()
			if (parser.NextResyncMarker() == false)
			{
				parser.BitPosition = bitPosition;
				return false;
			}

			// resync_marker; 17..23 bits
			if (GetResyncMarker(parser, vol) != 1)
			{
				parser.BitPosition = bitPosition;
				return false;
			}

			//parser.FlushBits((short)(ResyncMarkerLength + 8 - dataInfo.bitOffset));
			bool header_extension_code = false;
			if (vol.VideoShape != VideoObjectLayer.Shape.Rectangular)
			{
				header_extension_code = parser.GetBit(Attribute.HeaderExtensionCode);
				if (header_extension_code && !(vol.SpriteEnable == VideoObjectLayer.MP4SpriteStatic && _codingType == (int)Mpeg4VopType.I_VOP))
				{
					_vopWidth = parser.GetBits(13, Attribute.Width);
					if (!parser.GetMarkerBit()) return false;

					_vopHeight = parser.GetBits(13, Attribute.Height);
					if (!parser.GetMarkerBit()) return false;

					/*_vopHorizontalMcSpatialRef = */
					parser.GetBits(13, Attribute.HorizontalMcSpatialRef);
					if (!parser.GetMarkerBit()) return false;

					/*_vopVerticalMcSpatialRef = */
					parser.GetBits(13, Attribute.VerticalMcSpatialRef);
					if (!parser.GetMarkerBit()) return false;
				}
			}

			_macroblockNum = parser.GetBits((short)_numBitsMacroBlock, Attribute.MarcoBlockNumber);
			if (vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly)
			{
				_quantScale = parser.GetBits(vol.QuantPrecision /*3..9*/, Attribute.QuantScale); // quant_scale
			}
			if (vol.VideoShape == VideoObjectLayer.Shape.Rectangular)
			{
				header_extension_code = parser.GetBit(Attribute.HeaderExtensionCode);
			}
			if (header_extension_code)
			{
				//f ignore modulo_time_base
				bool moduloTimeBase;
				do
				{
					moduloTimeBase = parser.GetBit();
				} while (moduloTimeBase);
				if (!parser.GetMarkerBit()) return false;

				//f ignore vop_time_increment
				if (vol.VopTimeIncrementResolutionBits != 0)
				{
					parser.GetBits(vol.VopTimeIncrementResolutionBits/*1-16*/);
				}
				if (!parser.GetMarkerBit())
				{
					return false;
				}
				parser.GetBits(2);
				if (vol.VideoShape != VideoObjectLayer.Shape.Rectangular)
				{
					/*_changeConvRatioDisable = */
					parser.GetBit();
					if (_codingType != (int)Mpeg4VopType.I_VOP)
						/*_shapeCodingType = */parser.GetBit();
				}

				if (vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly)
				{
					//f ignore intra_dc_vlc_thr
					parser.GetBits(3);
					if (vol.SpriteEnable == VideoObjectLayer.MP4SpriteGMC && _codingType == (int)Mpeg4VopType.S_VOP && vol.SpriteWarpingPoints > 0)
					{
						if (mp4_Sprite_Trajectory(parser, vol) != 0)
						{
							return false;
						}
					}
					//f ignore vop_reduced_resolution
					if (vol.ReducedResolutionVopEnable && vol.VideoShape == VideoObjectLayer.Shape.Rectangular &&
						(_codingType == (int)Mpeg4VopType.I_VOP || _codingType == (int)Mpeg4VopType.P_VOP))
					{
						/*code = */
						parser.GetBit();
					}
					if (_codingType != (int)Mpeg4VopType.I_VOP)
					{
						_fcodeForward = parser.GetBits(3);
					}
					if (_codingType == (int)Mpeg4VopType.B_VOP)
					{
						_fcodeBackward = parser.GetBits(3);
					}
				}
			}
			if (vol.NewPredEnable)
			{
				/*
				 * TODO
				 * vop_id						// 4..15
				 * vop_prediction_indication	// 1
				 * if(vop_prediction_indication)
				 *     vop_id_for_prediction	// 4..15
				 * marker_bit					// 1
				 */
			}
			return true;
		}

		protected static uint GET_BITS(uint cache, short n)
		{
			return ((cache) >> (32 - (n)));
		}

		/// <summary>
		/// resync_marker
		/// see page 157 of ISO-IEC 14496-2-2004-MPEG-4-Video
		///
		/// This is a binary string of at least 16 zero’s followed by a one
		/// ‘0 0000 0000 0000 0001’. For an IVOP or a VOP where
		/// video_object_layer_shape has the value “binary_only”, the resync
		/// marker is 16 zeros followed by a one. The length of this resync
		/// marker is dependent on the value of vop_fcode_forward, for a P-VOP
		/// or a S(GMC)-VOP, and the larger value of either vop_fcode_forward
		/// and vop_fcode_backward for a B-VOP. For a P-VOP and a S(GMC)-VOP,
		/// the resync_marker is (15+fcode) zeros followed by a one; for a
		/// B-VOP, the resync_marker is max(15+fcode,17) zeros followed by a
		/// one. It is only present when resync_marker_disable flag is set to
		/// ‘0’. A resync marker shall only be located immediately before a
		/// macroblock and aligned with a byte.
		/// </summary>
		private uint GetResyncMarker(Mpeg4Parser parser, VideoObjectLayer vol)
		{
			int resyncMarkerLength = 0;

			// It is only present when resync_marker_disable flag is set to ‘0’.
			if (!vol.ResyncMarkerDisable)
			{
				// For an IVOP or a VOP where video_object_layer_shape has the
				// value “binary_only”, the resync marker is 16 zeros followed by a one.
				if (_codingType == (int)Mpeg4VopType.I_VOP || vol.VideoShape == VideoObjectLayer.Shape.BinaryOnly)
				{
					resyncMarkerLength = 16 + 1; // 16 zeros followed by a one
				}
				// For a P-VOP and a S(GMC)-VOP, the resync_marker is (15+fcode) zeros followed by a one
				else if (_codingType == (int)Mpeg4VopType.P_VOP || _codingType == (int)Mpeg4VopType.S_VOP)
				{
					resyncMarkerLength = (int)(15 + _fcodeForward + 1);
				}
				// For a B-VOP, the resync_marker is max(15+fcode,17) zeros followed by a one.
				else if (_codingType == (int)Mpeg4VopType.B_VOP)
				{
					resyncMarkerLength = (int)Math.Max(15 + Math.Max(_fcodeForward, _fcodeBackward), 17) + 1;
				}
				uint resyncMarker = parser.GetBits(resyncMarkerLength);
				if (resyncMarker == 1)
				{
					Attributes.Add(new FormattedAttribute<Attribute, uint>(Attribute.ResyncMarker, resyncMarker));
				}
				return resyncMarker;
			}
			return 0;
		}

		internal bool IsKeyframe()
		{
			return (_codingType == (int)Mpeg4VopType.I_VOP);
		}
	}
}
