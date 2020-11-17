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
using Defraser.Detector.Common;
using Defraser.Util;

namespace Defraser.Detector.Mpeg4
{
	/// <summary>
	/// VOP - Video Object Plane header
	/// </summary>
	internal class Vop : VopBase
	{
		/* MacroBlock Info for Motion */
		private struct MP4MacroBlock
		{
			public int type;          // for OBMC, BVOP
			public bool not_coded;     // for OBMC, BVOP
			public bool ac_pred_flag;
			public bool mcsel;
			public int cbpy;
			public int quant;
		}

		public Vop(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.Vop)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.InsertReferenceHeaderBeforeStartCode();
			parser.SeenVop = true;

			// In MPEG-4 every VOP is a child of a VOL header
			VideoObjectLayer VOL = Vol;
			uint code = 0;

			Attributes.Add(new FormattedAttribute<Attribute, bool>(Attribute.ShortVideoHeader, false));

			_codingType = (byte)parser.GetBits(2);
			Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.CodingType, Enum.GetName(typeof(Mpeg4VopType), _codingType)));
			int moduloTimeBase = 0;
			do
			{
				code = (parser.GetBit() == true) ? 1U : 0U;
				moduloTimeBase += (int)code;
			} while (code > 0);

			Attributes.Add(new FormattedAttribute<Attribute, int>(Attribute.ModuloTimeBase, moduloTimeBase));

			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOP header
				return false;
			}

			// No VOL (Video Object Layer) header was found.
			// Parsing this VOP header beyond the marker bit above is not possible
			// without the VOL.
			// There will be a VOP created anyway. Its size will be determined by the
			// location of the next header start code. When the resulting VOP is to big,
			// it will be rejected. The downside of this solution is that other streams
			// will be taken in the resulting VOP. The good news is that there is a result
			// reported without a VOL. When the detector finds a VOL anywhere else in the
			// data, the user of Defraser can use this VOL to try to repair these VOP's.
			if (VOL == null)
			{
				if (parser.TryDefaultHeaders(vol => ParseVop(parser, vol)))
				{
					return true; // VOP successfully decoded using reference header!
				}

				// Search for the next start location

				// The max distance from the current position to the next start code
				uint maxDistanceToNextStartCode = (uint)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.VopHeaderMaxUnparsedLength];
				maxDistanceToNextStartCode += (uint)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.VopHeaderMaxOtherStreamLength];

				this.Valid = false;

				return IsNextHeaderWithinMaximumDistance(parser, maxDistanceToNextStartCode);
			}

			return ParseVop(parser, VOL);
		}

		private bool ParseVop(Mpeg4Parser parser, VideoObjectLayer vol)
		{
			_vopWidth = 0;
			_vopHeight = 0;
			_intraDcVlcThr = 0;
			_pQuant = 0;
			_fcodeForward = 0;
			_fcodeBackward = 0;
			_refSelectCode = 0;
			_quantScale = 0;
			_macroblockNum = 0;
			_numBitsMacroBlock = 0;

			_rvlc = vol.UseReverseDCT;

			if (vol.VopTimeIncrementResolutionBits != 0)
			{
				/*_timeIncrement = */parser.GetBits(vol.VopTimeIncrementResolutionBits, Attribute.TimeIncrement);
			}
			else
			{
				//AddAttributes();
				return true;
			}

			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOP header
				return false;
			}
			bool coded = parser.GetBit();
			if (coded == false)
			{
				//AddAttributes();
				return true;
			}
			if (vol.NewPredEnable)
			{
				//vop_id
				//vop_id_prediction
				// if (vop_id_prediction > 0) 
				// vop_id_for_prediction;
				if (!parser.GetMarkerBit())
				{
					// Does not appear to be a mp4 VOP header
					return false;
				}
			}
			if (vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly && (_codingType == (int)Mpeg4VopType.P_VOP ||
				(_codingType == (int)Mpeg4VopType.S_VOP && vol.SpriteEnable == VideoObjectLayer.MP4SpriteGMC)))
			{
				parser.GetBit(Attribute.RoundingType);
			}
			if (vol.ReducedResolutionVopEnable && vol.VideoShape == VideoObjectLayer.Shape.Rectangular &&
				(_codingType == (int)Mpeg4VopType.I_VOP || _codingType == (int)Mpeg4VopType.P_VOP))
			{
				parser.GetBit(Attribute.ReducedResolution);
			}

			if (vol.VideoShape != VideoObjectLayer.Shape.Rectangular)
			{
				if (!(vol.SpriteEnable == VideoObjectLayer.MP4SpriteStatic && _codingType == (int)Mpeg4VopType.I_VOP))
				{
					_vopWidth = parser.GetBits(13, Attribute.Width);
					if (!parser.GetMarkerBit())
					{
						return false;
					}

					_vopHeight = parser.GetBits(13, Attribute.Height);
					if (!parser.GetMarkerBit())
					{
						return false;
					}

					uint vopHorizontalMcSpatialRef = parser.GetBits(13);
					vopHorizontalMcSpatialRef <<= (32 - 13);
					vopHorizontalMcSpatialRef >>= (32 - 13);
					Attributes.Add(new FormattedAttribute<Attribute, uint>(Attribute.HorizontalMcSpatialRef, vopHorizontalMcSpatialRef));
					if (!parser.GetMarkerBit())
					{
						return false;
					}
					uint vopVerticalMcSpatialRef = parser.GetBits(13);
					vopVerticalMcSpatialRef <<= (32 - 13);
					vopVerticalMcSpatialRef >>= (32 - 13);
					Attributes.Add(new FormattedAttribute<Attribute, uint>(Attribute.VerticalMcSpatialRef, vopVerticalMcSpatialRef));
					if (!parser.GetMarkerBit())
					{
						return false;
					}
				}
				if (vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly && vol.Scalability == true && vol.EnhancementType)
				{
					/*bool backgroundComposition = */parser.GetBit(Attribute.BackgroundComposition);
				}
				parser.GetBit(Attribute.ChangeConvRatioDisable);
				bool vopConstantAlpha = parser.GetBit(Attribute.ConstantAlpha);
				byte vopConstantAlphaValue;
				if (vopConstantAlpha == true)
				{
					vopConstantAlphaValue = (byte)parser.GetBits(8);
				}
				else
				{
					vopConstantAlphaValue = 255;
				}
				Attributes.Add(new FormattedAttribute<Attribute, byte>(Attribute.ConstantAlphaValue, vopConstantAlphaValue));
			}
			if (vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly)
			{
				if (!vol.ComplexityEstimationDisable)
				{
					VideoObjectLayer.mp4_ComplexityEstimation complex;
					vol.ComplexityEstimation(out complex);
					if (complex.estimation_method == 0)
					{
						if (_codingType == (int)Mpeg4VopType.I_VOP)
						{
							if (complex.opaque) /*complex.dcecs_opaque =*/ parser.GetBits(8, Attribute.DcecsOpaque);
							if (complex.transparent) /*complex.dcecs_transparent =*/ parser.GetBits(8, Attribute.DcecsTransparent);
							if (complex.intra_cae) /*complex.dcecs_intra_cae =*/ parser.GetBits(8, Attribute.DcecsIntraCae);
							if (complex.inter_cae) /*complex.dcecs_inter_cae =*/ parser.GetBits(8, Attribute.DcecsInterCae);
							if (complex.no_update) /*complex.dcecs_no_update =*/ parser.GetBits(8, Attribute.DcecsNoUpdate);
							if (complex.upsampling) /*complex.dcecs_upsampling =*/ parser.GetBits(8, Attribute.DcecsUpsampling);
							if (complex.intra_blocks) /*complex.dcecs_intra_blocks =*/ parser.GetBits(8, Attribute.DcecsIntraBlocks);
							if (complex.not_coded_blocks) /*complex.dcecs_not_coded_blocks =*/ parser.GetBits(8, Attribute.DcecsNotCodedBlocks);
							if (complex.dct_coefs) /*complex.dcecs_dct_coefs =*/ parser.GetBits(8, Attribute.DcecsDctCoefs);
							if (complex.dct_lines) /*complex.dcecs_dct_lines =*/ parser.GetBits(8, Attribute.DcecsDctLines);
							if (complex.vlc_symbols) /*complex.dcecs_vlc_symbols =*/ parser.GetBits(8, Attribute.DcecsVlcSymbols);
							if (complex.vlc_bits) /*complex.dcecs_vlc_bits =*/ parser.GetBits(4, Attribute.DcecsVlcBits);
							if (complex.sadct) /*complex.dcecs_sadct =*/ parser.GetBits(8, Attribute.DcecsSadct);
						}
						if (_codingType == (int)Mpeg4VopType.P_VOP)
						{
							if (complex.opaque) /*complex.dcecs_opaque =*/ parser.GetBits(8, Attribute.DcecsOpaque);
							if (complex.transparent) /*complex.dcecs_transparent =*/ parser.GetBits(8, Attribute.DcecsTransparent);
							if (complex.intra_cae) /*complex.dcecs_intra_cae =*/ parser.GetBits(8, Attribute.DcecsIntraCae);
							if (complex.inter_cae) /*complex.dcecs_inter_cae =*/ parser.GetBits(8, Attribute.DcecsInterCae);
							if (complex.no_update) /*complex.dcecs_no_update =*/ parser.GetBits(8, Attribute.DcecsNoUpdate);
							if (complex.upsampling) /*complex.dcecs_upsampling =*/ parser.GetBits(8, Attribute.DcecsUpsampling);
							if (complex.intra_blocks) /*complex.dcecs_intra_blocks =*/ parser.GetBits(8, Attribute.DcecsIntraBlocks);
							if (complex.not_coded_blocks) /*complex.dcecs_not_coded_blocks =*/ parser.GetBits(8, Attribute.DcecsNotCodedBlocks);
							if (complex.dct_coefs) /*complex.dcecs_dct_coefs =*/ parser.GetBits(8, Attribute.DcecsDctCoefs);
							if (complex.dct_lines) /*complex.dcecs_dct_lines =*/ parser.GetBits(8, Attribute.DcecsDctLines);
							if (complex.vlc_symbols) /*complex.dcecs_vlc_symbols =*/ parser.GetBits(8, Attribute.DcecsVlcSymbols);
							if (complex.vlc_bits) /*complex.dcecs_vlc_bits =*/ parser.GetBits(4, Attribute.DcecsVlcBits);
							if (complex.inter_blocks) /*complex.dcecs_inter_blocks =*/ parser.GetBits(8, Attribute.DcecsInterBlocks);
							if (complex.inter4v_blocks) /*complex.dcecs_inter4v_blocks =*/ parser.GetBits(8, Attribute.DcecsInter4vBlocks);
							if (complex.apm) /*complex.dcecs_apm =*/ parser.GetBits(8, Attribute.DcecsApm);
							if (complex.npm) /*complex.dcecs_npm =*/ parser.GetBits(8, Attribute.DcecsNpm);
							if (complex.forw_back_mc_q) /*complex.dcecs_forw_back_mc_q =*/ parser.GetBits(8, Attribute.DcecsForwBackMcQ);
							if (complex.halfpel2) /*complex.dcecs_halfpel2 =*/ parser.GetBits(8, Attribute.DcecsHalfpel2);
							if (complex.halfpel4) /*complex.dcecs_halfpel4 =*/ parser.GetBits(8, Attribute.DcecsHalfpel4);
							if (complex.sadct) /*complex.dcecs_sadct =*/ parser.GetBits(8, Attribute.DcecsSadct);
							if (complex.quarterpel) /*complex.dcecs_quarterpel =*/ parser.GetBits(8, Attribute.DcecsQuarterpel);
						}
						if (_codingType == (int)Mpeg4VopType.B_VOP)
						{
							if (complex.opaque) /*complex.dcecs_opaque =*/ parser.GetBits(8, Attribute.DcecsOpaque);
							if (complex.transparent) /*complex.dcecs_transparent =*/ parser.GetBits(8, Attribute.DcecsTransparent);
							if (complex.intra_cae) /*complex.dcecs_intra_cae =*/ parser.GetBits(8, Attribute.DcecsIntraCae);
							if (complex.inter_cae) /*complex.dcecs_inter_cae =*/ parser.GetBits(8, Attribute.DcecsInterCae);
							if (complex.no_update) /*complex.dcecs_no_update =*/ parser.GetBits(8, Attribute.DcecsNoUpdate);
							if (complex.upsampling) /*complex.dcecs_upsampling =*/ parser.GetBits(8, Attribute.DcecsUpsampling);
							if (complex.intra_blocks) /*complex.dcecs_intra_blocks =*/ parser.GetBits(8, Attribute.DcecsIntraBlocks);
							if (complex.not_coded_blocks) /*complex.dcecs_not_coded_blocks =*/ parser.GetBits(8, Attribute.DcecsNotCodedBlocks);
							if (complex.dct_coefs) /*complex.dcecs_dct_coefs =*/ parser.GetBits(8, Attribute.DcecsDctCoefs);
							if (complex.dct_lines) /*complex.dcecs_dct_lines =*/ parser.GetBits(8, Attribute.DcecsDctLines);
							if (complex.vlc_symbols) /*complex.dcecs_vlc_symbols =*/ parser.GetBits(8, Attribute.DcecsVlcSymbols);
							if (complex.vlc_bits) /*complex.dcecs_vlc_bits =*/ parser.GetBits(4, Attribute.DcecsVlcBits);
							if (complex.inter_blocks) /*complex.dcecs_inter_blocks =*/ parser.GetBits(8, Attribute.DcecsInterBlocks);
							if (complex.inter4v_blocks) /*complex.dcecs_inter4v_blocks =*/ parser.GetBits(8, Attribute.DcecsInter4vBlocks);
							if (complex.apm) /*complex.dcecs_apm =*/ parser.GetBits(8, Attribute.DcecsApm);
							if (complex.npm) /*complex.dcecs_npm =*/ parser.GetBits(8, Attribute.DcecsNpm);
							if (complex.forw_back_mc_q) /*complex.dcecs_forw_back_mc_q =*/ parser.GetBits(8, Attribute.DcecsForwBackMcQ);
							if (complex.halfpel2) /*complex.dcecs_halfpel2 =*/ parser.GetBits(8, Attribute.DcecsHalfpel2);
							if (complex.halfpel4) /*complex.dcecs_halfpel4 =*/ parser.GetBits(8, Attribute.DcecsHalfpel4);
							if (complex.interpolate_mc_q) /*complex.dcecs_interpolate_mc_q =*/ parser.GetBits(8, Attribute.DcecsInterpolateMcQ);
							if (complex.sadct) /*complex.dcecs_sadct =*/ parser.GetBits(8, Attribute.DcecsSadct);
							if (complex.quarterpel) /*complex.dcecs_quarterpel =*/ parser.GetBits(8, Attribute.DcecsQuarterpel);
						}
						if (_codingType == (int)Mpeg4VopType.S_VOP && vol.SpriteEnable == VideoObjectLayer.MP4SpriteStatic)
						{
							if (complex.intra_blocks) /*complex.dcecs_intra_blocks =*/ parser.GetBits(8, Attribute.DcecsIntraBlocks);
							if (complex.not_coded_blocks) /*complex.dcecs_not_coded_blocks =*/ parser.GetBits(8, Attribute.DcecsNotCodedBlocks);
							if (complex.dct_coefs) /*complex.dcecs_dct_coefs =*/ parser.GetBits(8, Attribute.DcecsDctCoefs);
							if (complex.dct_lines) /*complex.dcecs_dct_lines =*/ parser.GetBits(8, Attribute.DcecsDctLines);
							if (complex.vlc_symbols) /*complex.dcecs_vlc_symbols =*/ parser.GetBits(8, Attribute.DcecsVlcSymbols);
							if (complex.vlc_bits) /*complex.dcecs_vlc_bits =*/ parser.GetBits(4, Attribute.DcecsVlcBits);
							if (complex.inter_blocks) /*complex.dcecs_inter_blocks =*/ parser.GetBits(8, Attribute.DcecsInterBlocks);
							if (complex.inter4v_blocks) /*complex.dcecs_inter4v_blocks =*/ parser.GetBits(8, Attribute.DcecsInter4vBlocks);
							if (complex.apm) /*complex.dcecs_apm =*/ parser.GetBits(8, Attribute.DcecsApm);
							if (complex.npm) /*complex.dcecs_npm =*/ parser.GetBits(8, Attribute.DcecsNpm);
							if (complex.forw_back_mc_q) /*complex.dcecs_forw_back_mc_q =*/ parser.GetBits(8, Attribute.DcecsForwBackMcQ);
							if (complex.halfpel2) /*complex.dcecs_halfpel2 =*/ parser.GetBits(8, Attribute.DcecsHalfpel2);
							if (complex.halfpel4) /*complex.dcecs_halfpel4 =*/ parser.GetBits(8, Attribute.DcecsHalfpel4);
							if (complex.interpolate_mc_q) /*complex.dcecs_interpolate_mc_q =*/ parser.GetBits(8, Attribute.DcecsInterpolateMcQ);
						}
					}
				}
				_intraDcVlcThr = parser.GetBits(3, Attribute.IntraDcVlcThr);
				if (vol.Interlaced)
				{
					/*bool topFieldFirst = */parser.GetBit(Attribute.TopFieldFirst);
					/*bool alternateVerticalScanFlag = */parser.GetBit(Attribute.AlternateVerticalScanFlag);
				}
			}
			if (vol != null && (vol.SpriteEnable == VideoObjectLayer.MP4SpriteStatic || vol.SpriteEnable == VideoObjectLayer.MP4SpriteGMC) && _codingType == (int)Mpeg4VopType.S_VOP)
			{
				if (vol.SpriteWarpingPoints > 0)
				{
					if (mp4_Sprite_Trajectory(parser, vol) != 1)
					{
						return false;
					}
				}

				uint _brightnessChangeFactor;
				if (vol.SpriteBrightnessChange)
				{
					uint code = code = parser.ShowBits(4);
					if (code == 15)
					{
						parser.FlushBits(4);
						_brightnessChangeFactor = 625 + parser.GetBits(10);
					}
					else if (code == 14)
					{
						parser.FlushBits(4);
						_brightnessChangeFactor = 113 + parser.GetBits(9);
					}
					else if (code >= 12)
					{
						parser.FlushBits(3);
						code = parser.GetBits(7);
						_brightnessChangeFactor = (code < 64) ? code - 112 : code - 15;
					}
					else if (code >= 8)
					{
						parser.FlushBits(2);
						code = parser.GetBits(6);
						_brightnessChangeFactor = (code < 32) ? code - 48 : code - 15;
					}
					else
					{
						parser.FlushBits(1);
						code = parser.GetBits(5);
						_brightnessChangeFactor = (code < 16) ? code - 16 : code - 15;
					}
					Attributes.Add(new FormattedAttribute<Attribute, uint>(Attribute.BrightnessChangeFactor, _brightnessChangeFactor));
				}
				else
				{
					_brightnessChangeFactor = 0;
				}
				if (vol.SpriteEnable == VideoObjectLayer.MP4SpriteStatic)
				{
					// Handle sprite mode
					//AddAttributes();
					return true;
				}
			}
			if (vol != null && vol.VideoShape != VideoObjectLayer.Shape.BinaryOnly)
			{
				_pQuant = parser.GetBits(vol.QuantPrecision);
				if (vol.VideoShape == VideoObjectLayer.Shape.Grayscale)
				{
					int ac;
					int i;

					ac = vol.AuxCompCount;
					int[] alphaQuant = new int[3];
					for (i = 0; i < ac; i++)
					{
						alphaQuant[i] = (int)parser.GetBits(6);
					}
				}
				if (_codingType != (int)Mpeg4VopType.I_VOP)
				{
					_fcodeForward = parser.GetBits(3, Attribute.FCodeForward);
					if (_fcodeForward == 0)
					{
						//mp4_Error("Error: vop_fcode_forward == 0");
						return false;
					}
				}
				if (_codingType == (int)Mpeg4VopType.B_VOP)
				{
					_fcodeBackward = parser.GetBits(3, Attribute.FCodeBackward);
					if (_fcodeBackward == 0)
					{
						//mp4_Error("Error: vop_fcode_backward == 0");
						return false;
					}
				}
				if (vol.Scalability == false)
				{
					if (vol.VideoShape != VideoObjectLayer.Shape.Rectangular && _codingType != (int)Mpeg4VopType.I_VOP)
					{
						/*_shapeCodingType = */parser.GetBit(Attribute.ShapeCodingType);
					}
					// handle motion shape texture
					if (vol.DataPartitioned && _codingType != (int)Mpeg4VopType.B_VOP)
					{
						DataPartitioned(parser, vol);
					}
					else
					{
						if (HandleCombinedMotionShapeTexture(parser, vol) == false) return false;
					}
				}
				else
				{
					if (vol.EnhancementType)
					{
						if (parser.GetBit() == true)
						{
							parser.FlushBits(45);
							if (parser.GetBit() == true)
							{
								parser.FlushBits(45);
							}
						}
					}
					_refSelectCode = parser.GetBits(2, Attribute.RefSelectCode);

					if (HandleCombinedMotionShapeTexture(parser, vol) == false) return false;
				}
			}
			else
			{
				if (HandleCombinedMotionShapeTexture(parser, vol) == false) return false;
			}

			parser.AlignBits7F();

			return true;
		}

		static private bool IsNextHeaderWithinMaximumDistance(Mpeg4Parser parser, uint maxDistanceToNextStartCode)
		{
			const int StartCodeLength = 4;

			if ((parser.Position + StartCodeLength) >= parser.Length) return false;

			uint value = parser.GetBits(32);

			if ((value & 0x000001FF) == value)
			{
				parser.Position -= StartCodeLength;
				return value == 0/* value for second scan */ || value.IsKnownHeaderStartCode();
			}

			long maxAbsolutePositionOfNextHeader = Math.Min(parser.Position + maxDistanceToNextStartCode, parser.Length);

			while (parser.Position < maxAbsolutePositionOfNextHeader)
			{
				value = (value <<= 8) | parser.GetByte();

				if ((value & 0x000001FF) == value)
				{
					if (value == 0 /* value for second scan */ || value.IsKnownHeaderStartCode())
					{
						parser.Position = parser.Position - StartCodeLength;
						return true;
					}
				}
			}
			// return true when at the end of the datapacket or file else return false.
			if (parser.Position == maxAbsolutePositionOfNextHeader)
			{
				return true;
			}
			return false;
		}

		private void DataPartitioned(Mpeg4Parser parser, VideoObjectLayer VOL)
		{
			uint mbPerRow = (VOL.Width + 15) >> 4;
			uint mbPerCol = (VOL.Height + 15) >> 4;
			int i;
			int cbpc = 0;
			int cbpy = 0;
			int quant = (int)_pQuant;
			int macroblockType = 0;
			int macroblockInVideoPacket = 0;
			uint columnNumber = 0;
			uint rowNumber = 0;
			int macroblockCurrent = 0;
			MP4MacroBlock[] DCTInfo = new MP4MacroBlock[mbPerCol * mbPerRow];
			_numBitsMacroBlock = mp4_GetMacroBlockNumberSize((int)(mbPerCol * mbPerRow));

			if (_codingType == (int)Mpeg4VopType.I_VOP)
			{
				for (; ; )
				{
					macroblockInVideoPacket = 0;
					const int MP4_DC_MARKER = 0x6B001;
					do
					{
						if (VOL.VideoShape != VideoObjectLayer.Shape.Rectangular)
						{
							//Handle BAB TYPE

							// Handle MCBPC
							do
							{
								if (!DeriveMacroblockType(parser, out macroblockType, out cbpc))
								{
									return;
								}
							}
							while (macroblockType == (int)MacroblockType.Stuffing);
						}
						else
						{
							if (!DeriveMacroblockType(parser, out macroblockType, out cbpc))
							{
								return;
							}
						}
						if (macroblockType != (int)MacroblockType.Stuffing)
						{
							if (macroblockType == (int)MacroblockType.IntraQ)
							{
								quant += GetDbquant(parser);
								quant = MP4Clip(quant, 1, 1 << (VOL.QuantPrecision - 1));
							}

							if (quant < mp4_DC_vlc_Threshold[_intraDcVlcThr])
							{
								for (i = 0; i < 6; i++)
								{
									parser.GetLuminanceChrominance(i);
								}
							}
							DCTInfo[macroblockInVideoPacket].type = macroblockType;
							DCTInfo[macroblockInVideoPacket].quant = quant;
							DCTInfo[macroblockInVideoPacket].cbpy = cbpc;
							macroblockInVideoPacket++;
						}
					}
					while (parser.ShowBits(19) != MP4_DC_MARKER);
					parser.GetBits(19);

					for (i = 0; i < macroblockInVideoPacket; i++)
					{
						DCTInfo[i].ac_pred_flag = parser.GetBit();
						if (!DecodeCBPY(parser, out cbpy, 1))
						{
							return;
						}
						DCTInfo[i].cbpy = (cbpy << 2) + DCTInfo[i].cbpy;
					}

					for (i = 0; i < macroblockInVideoPacket; i++)
					{
						DecodeBlock(parser, VOL, DCTInfo[i].type, DCTInfo[i].cbpy, DCTInfo[i].quant < mp4_DC_vlc_Threshold[_intraDcVlcThr]);
						columnNumber++;
						if (columnNumber == mbPerRow)
						{
							columnNumber = 0;
							rowNumber++;
							if (rowNumber == mbPerCol)
							{
								return;
							}
						}
					}
					macroblockCurrent += macroblockInVideoPacket;
					if (!VOL.ResyncMarkerDisable)
					{
						// nextbits_bytealigned
						//if (NextBitsByteAligned(parser) == GetResyncMarker(parser, VOL))
						//{
						if (mp4_CheckDecodeVideoPacket(parser, VOL))
						{
							quant = (int)_quantScale;
							rowNumber = _macroblockNum / mbPerRow;
							columnNumber = _macroblockNum % mbPerRow;
						}
						//}
					}
				}
			}
			else if (_codingType == (int)Mpeg4VopType.P_VOP || (_codingType == (int)Mpeg4VopType.S_VOP && VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC))
			{
				bool mb_not_coded;
				bool mcsel = false;
				for (; ; )
				{
					macroblockInVideoPacket = 0;
					const int MP4_MV_MARKER = 0x1F001;
					do
					{
						mb_not_coded = true;
						if (VOL.VideoShape != VideoObjectLayer.Shape.Rectangular)
						{
							//Handle BAB TYPE

							// Handle MCBPC
							do
							{
								mb_not_coded = parser.GetBit();
								if (mb_not_coded == false)
								{
									if (!DeriveMacroblockType(parser, out macroblockType, out cbpc))
									{
										return;
									}
								}
							}
							while (!(mb_not_coded == true || macroblockType != (int)MacroblockType.Stuffing));
						}
						else
						{
							mb_not_coded = parser.GetBit();
							if (mb_not_coded != false)
							{
								macroblockType = (int)MacroblockType.Inter;
							}
							else
							{
								if (!DeriveMacroblockType(parser, out macroblockType, out cbpc))
								{
									return;
								}
							}
						}
						if (macroblockType != (int)MacroblockType.Stuffing && mb_not_coded == false)
						{
							mcsel = false;
							if (VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC && _codingType == (int)Mpeg4VopType.S_VOP && macroblockType < 2)
							{
								mcsel = parser.GetBit();
							}
							if (!(VOL.SpriteEnable == VideoObjectLayer.MP4SpriteGMC && _codingType == (int)Mpeg4VopType.S_VOP && mcsel != false) && macroblockType < 2 || macroblockType == 2)
							{
								parser.GetMotionVector(_fcodeForward);
								if (macroblockType == 2)
								{
									for (i = 0; i < 3; i++)
									{
										parser.GetMotionVector(_fcodeForward);
									}
								}
							}
						}
						DCTInfo[macroblockInVideoPacket].not_coded = mb_not_coded;
						DCTInfo[macroblockInVideoPacket].mcsel = mcsel;
						DCTInfo[macroblockInVideoPacket].type = macroblockType;
						DCTInfo[macroblockInVideoPacket].quant = quant;
						DCTInfo[macroblockInVideoPacket].cbpy = cbpc;
						macroblockInVideoPacket++;
					}
					while (parser.ShowBits(17) != MP4_MV_MARKER);
					parser.GetBits(17);
					for (i = 0; i < macroblockInVideoPacket; i++)
					{
						if (DCTInfo[i].not_coded == false)
						{
							if (DCTInfo[i].type >= (int)MacroblockType.Intra)
							{
								DCTInfo[i].ac_pred_flag = parser.GetBit();
							}

							if (!DecodeCBPY(parser, out cbpy, DCTInfo[i].type))
							{
								return;
							}
							DCTInfo[i].cbpy = (cbpy << 2) + DCTInfo[i].cbpy;
							if (macroblockType == 1 || macroblockType == 4)
							{
								quant += GetDbquant(parser);
								quant = MP4Clip(quant, 1, 1 << (VOL.QuantPrecision - 1));
							}
							DCTInfo[i].quant = quant;
							// decode DC coefficient of Intra blocks
							if ((DCTInfo[i].type >= (int)MacroblockType.Intra) && quant < mp4_DC_vlc_Threshold[_intraDcVlcThr])
							{
								for (int j = 0; j < 6; j++)
								{
									parser.GetLuminanceChrominance(j);
								}
							}
						}
					}

					for (i = 0; i < macroblockInVideoPacket; i++)
					{
						if (DCTInfo[i].not_coded == false)
						{
							DecodeBlock(parser, VOL, DCTInfo[i].type, DCTInfo[i].cbpy, DCTInfo[i].quant < mp4_DC_vlc_Threshold[_intraDcVlcThr]);
						}
						columnNumber++;
						if (columnNumber == mbPerRow)
						{
							columnNumber = 0;
							rowNumber++;
							if (rowNumber == mbPerCol)
							{
								return;
							}
						}
					}
					macroblockCurrent += macroblockInVideoPacket;
					if (!VOL.ResyncMarkerDisable)
					{
						if (mp4_CheckDecodeVideoPacket(parser, VOL))
						{
							quant = (int)_quantScale;
							rowNumber = _macroblockNum / mbPerRow;
							columnNumber = _macroblockNum % mbPerRow;
						}
					}
				}
			}
		}

		private bool HandleCombinedMotionShapeTexture(Mpeg4Parser parser, VideoObjectLayer vol)
		{
			uint nextBitsByteAligned = 0U;
			Pair<byte, long> bitPosition = new Pair<byte, long>(0, 0L);

			uint macroBlocksPerRow = (vol.Width + 15) >> 4;

			if (_vopHeight != 0 && _vopWidth != 0)
			{
				macroBlocksPerRow = (_vopWidth + 15) >> 4;
			}

			uint column = 0U;
			uint row = 0U;
			do
			{
				long saveMacroBlockStartPosition = parser.Position;

				if (HandleMacroBlock(parser, vol, ref column, ref row) == false)
				{
					parser.Position = saveMacroBlockStartPosition;
					this.Valid = false;
					return (row > 0);	// At least one row processed
				}

				// All rows processed
				if (row == macroBlocksPerRow && column == 0)
				{
					return true;
				}

				nextBitsByteAligned = parser.ShowNextBitsByteAligned(32);

				// Break out of the loop when there is no progress
				if (parser.BitPosition == bitPosition)
				{
					return false;
				}
				bitPosition = parser.BitPosition;
			}
			while ((!IsResyncMarker(nextBitsByteAligned, vol.VideoShape) && !IsStartCode(nextBitsByteAligned)) ||
				!parser.ValidStuffing());

			return true;
		}

		static private bool IsStartCode(uint value)
		{
			return ((value & 0xFFFFFE00) == 0x00000000);
		}

		private bool IsResyncMarker(uint value, VideoObjectLayer.Shape videoShape)
		{
			return (value >>= (32 - ResyncMarkerLength(videoShape))) == 1;
		}

		/// <summary>
		/// The resync marker length is between 17..23 bits. All zero's followed by a one: ‘0 0000 0000 0000 0001’.
		/// For an IVOP or a VOP where video_object_layer_shape has the value “binary_only”, the resync marker is 16 zeros
		/// followed by a one. The length of this resync marker is dependent on the value of vop_fcode_forward, for a P-VOP
		/// or a S(GMC)-VOP, and the larger value of either vop_fcode_forward and vop_fcode_backward for a B-VOP. For a
		/// P-VOP and a S(GMC)-VOP, the resync_marker is (15+fcode) zeros followed by a one; for a B-VOP, the
		/// resync_marker is max(15+fcode,17) zeros followed by a one.
		/// </summary>
		private int ResyncMarkerLength(VideoObjectLayer.Shape videoShape)
		{
			int resyncMarkerLength = 0;

			// For an IVOP or a VOP where video_object_layer_shape has the
			// value “binary_only”, the resync marker is 16 zeros followed by a one.
			if (_codingType == (int)Mpeg4VopType.I_VOP || videoShape == VideoObjectLayer.Shape.BinaryOnly)
			{
				return 16 + 1; // 16 zeros followed by a one
			}
			// For a P-VOP and a S(GMC)-VOP, the resync_marker is (15+fcode) zeros followed by a one
			else if (_codingType == (int)Mpeg4VopType.P_VOP || _codingType == (int)Mpeg4VopType.S_VOP)
			{
				return (int)(15 + _fcodeForward + 1);
			}
			// For a B-VOP, the resync_marker is max(15+fcode,17) zeros followed by a one.
			else if (_codingType == (int)Mpeg4VopType.B_VOP)
			{
				return (int)Math.Max(15 + Math.Max(_fcodeForward, _fcodeBackward), 17) + 1;
			}
			return resyncMarkerLength;
		}
	}
}
