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
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal class MbToSliceGroupMapdecoder
	{
		public IMbToSliceGroupMap CreateMacroBlockToSliceGroupMap(ISliceState sliceState)
		{
			IPictureState pictureState = sliceState.PictureState;
			ISequenceState sequence = sliceState.PictureState.SequenceState;

			uint picSizeInMapUnits = sequence.PicSizeInMapUnits;
			uint sliceGroupChangeRate = pictureState.SliceGroupChangeRate;
			uint mapUnitsInSliceGroup0 = Math.Min(sliceState.SliceGroupChangeCycle * sliceGroupChangeRate, picSizeInMapUnits);
			int[] mapUnitToSliceGroupMap;
			if (pictureState.SliceGroupCount == 1)
			{
				return new SingleSliceGroupMap();
			}
			// num_slice_groups_minus1 is not equal to 0
			switch (pictureState.SliceGroupMapType)
			{
				case SliceGroupMapType.InterleavedSliceGroups:
					mapUnitToSliceGroupMap = InterleavedSliceGroupMapType(pictureState);
					break;
				case SliceGroupMapType.DispersedSliceGroups:
					mapUnitToSliceGroupMap = DispersedSliceGroupMapType(pictureState);
					break;
				case SliceGroupMapType.ForegroundAndLeftoverSliceGroups:
					mapUnitToSliceGroupMap = ForegroundWithLeftOverSliceGroupMapType(pictureState);
					break;
				case SliceGroupMapType.ChangingSliceGroups3:
					mapUnitToSliceGroupMap = BoxOutSliceGroupMapTypes(pictureState, mapUnitsInSliceGroup0);
					break;
				case SliceGroupMapType.ChangingSliceGroups4:
					mapUnitToSliceGroupMap = RasterScanSliceGroupMapTypes(pictureState, mapUnitsInSliceGroup0);
					break;
				case SliceGroupMapType.ChangingSliceGroups5:
					mapUnitToSliceGroupMap = WipeSliceGroupMapTypes(pictureState, mapUnitsInSliceGroup0);
					break;
				case SliceGroupMapType.ExplicitSliceGroups:
					mapUnitToSliceGroupMap = ExplicitSliceGroupMapType(pictureState);
					break;
				default:
					throw new InvalidOperationException();
			}
			if (sequence.FrameMbsOnlyFlag || sliceState.FieldPicFlag)
			{
				return new OneMacroblockPerSliceGroupMap(mapUnitToSliceGroupMap);
			}

			if (sequence.MbAdaptiveFrameFieldFlag) // mb_adaptive_frame_field_flag && !FieldPicFlag
			{
				return new TwoMacroblocksPerSliceGroupMap(mapUnitToSliceGroupMap);
			}

			// if (!sequence.FrameMbsOnlyFlag && !sequence.MbAdaptiveFrameFieldFlag && !sliceState.FieldPicFlag)
			return new OverlappingMbToSliceGroupMap(mapUnitToSliceGroupMap, sequence.PicWidthInMbs);
		}

		private int[] InterleavedSliceGroupMapType(IPictureState pictureState)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			var sliceGroupCount = pictureState.SliceGroupCount;
			var runLengthMinus1 = pictureState.RunLengthMinus1;
			int i = 0;
			do
			{
				for (int iGroup = 0; iGroup < sliceGroupCount && i < picSizeInMapUnits; i += (int)runLengthMinus1[iGroup++] + 1)
				{
					for (int j = 0; j <= runLengthMinus1[iGroup] && i + j < picSizeInMapUnits; j++)
					{
						mapUnitToSliceGroupMap[i + j] = iGroup;
					}
				}
			} while (i < picSizeInMapUnits);
			return mapUnitToSliceGroupMap;
		}

		private int[] DispersedSliceGroupMapType(IPictureState pictureState)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			var sliceGroupCount = pictureState.SliceGroupCount;
			var picWidthInMbs = pictureState.SequenceState.PicWidthInMbs;
			for (int i = 0; i < picSizeInMapUnits; i++)
			{
				mapUnitToSliceGroupMap[i] = (int)((int)((i % picWidthInMbs) +
														   (((i / picWidthInMbs) * sliceGroupCount) / 2))
													% (sliceGroupCount));
			}
			return mapUnitToSliceGroupMap;
		}

		private int[] ForegroundWithLeftOverSliceGroupMapType(IPictureState pictureState)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			var sliceGroupCount = pictureState.SliceGroupCount;
			var picWidthInMbs = pictureState.SequenceState.PicWidthInMbs;
			var topLeft = pictureState.TopLeft;
			var bottomRight = pictureState.BottomRight;
			for (int i = 0; i < picSizeInMapUnits; i++)
			{
				mapUnitToSliceGroupMap[i] = (int)sliceGroupCount - 1;
			}
			for (int iGroup = (int)sliceGroupCount; iGroup >= 0; iGroup--)
			{
				uint yTopLeft = topLeft[iGroup] / picWidthInMbs;
				uint xTopLeft = topLeft[iGroup] % picWidthInMbs;
				uint yBottomRight = bottomRight[iGroup] / picWidthInMbs;
				uint xBottomRight = bottomRight[iGroup] % picWidthInMbs;
				for (uint y = yTopLeft; y <= yBottomRight; y++)
				{
					for (uint x = xTopLeft; x <= xBottomRight; x++)
					{
						mapUnitToSliceGroupMap[y * picWidthInMbs + x] = iGroup;
					}
				}
			}
			return mapUnitToSliceGroupMap;
		}

		private int[] BoxOutSliceGroupMapTypes(IPictureState pictureState, uint mapUnitsInSliceGroup0)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			var picWidthInMbs = pictureState.SequenceState.PicWidthInMbs;
			var picHeightInMapUnits = pictureState.SequenceState.PicHeightInMapUnits;
			for (int i = 0; i < picSizeInMapUnits; i++)
			{
				mapUnitToSliceGroupMap[i] = 1;
			}
			var sliceGroupChangeDirectionFlag = pictureState.SliceGroupChangeDirectionFlag ? 1 : 0; //1=counter-clockwise, 0=clockwise
			int x = (int)(picWidthInMbs - sliceGroupChangeDirectionFlag) / 2;
			int y = (int)(picHeightInMapUnits - sliceGroupChangeDirectionFlag) / 2;
			// ( leftBound, topBound ) = ( x, y );
			int leftBound = x;
			int topBound = y;
			// ( rightBound, bottomBound ) = ( x, y );
			int rightBound = x;
			int bottomBound = y;
			//( xDir, yDir ) = ( SliceGroupChangeDirectionFlag - 1, SliceGroupChangeDirectionFlag );
			int xDir = sliceGroupChangeDirectionFlag - 1;
			int yDir = sliceGroupChangeDirectionFlag;
			bool mapUnitVacant;
			for (int k = 0; k < mapUnitsInSliceGroup0; k += mapUnitVacant ? 1 : 0)
			{
				mapUnitVacant = (mapUnitToSliceGroupMap[y * picWidthInMbs + x] == 1);
				if (mapUnitVacant)
				{
					mapUnitToSliceGroupMap[y * picWidthInMbs + x] = 0;
				}
				if (xDir == -1 && x == leftBound)
				{
					leftBound = Math.Max(leftBound - 1, 0);
					x = leftBound;
					//( xDir, yDir ) = ( 0, 2 * SliceGroupChangeDirectionFlag – 1 )
					xDir = 0;
					yDir = 2 * sliceGroupChangeDirectionFlag - 1;
				}
				else if (xDir == 1 && x == rightBound)
				{
					rightBound = (int)Math.Min(rightBound + 1, picWidthInMbs - 1);
					x = rightBound;
					//( xDir, yDir ) = ( 0, 1 – 2 * SliceGroupChangeDirectionFlag )
					xDir = 0;
					yDir = 1 - 2 * sliceGroupChangeDirectionFlag;
				}
				else if (yDir == -1 && y == topBound)
				{
					topBound = Math.Max(topBound - 1, 0);
					y = topBound;
					//( xDir, yDir ) = ( 1 – 2 * SliceGroupChangeDirectionFlag, 0 );
					xDir = 1 - 2 * sliceGroupChangeDirectionFlag;
					yDir = 0;
				}
				else if (yDir == 1 && y == bottomBound)
				{
					bottomBound = (int)Math.Min(bottomBound + 1, picHeightInMapUnits - 1);
					y = bottomBound;
					//( xDir, yDir ) = ( 2 * SliceGroupChangeDirectionFlag – 1, 0 );
					xDir = 2 * sliceGroupChangeDirectionFlag - 1;
					yDir = 0;
				}
				else
				{
					//( x, y ) = ( x + xDir, y + yDir );
					x = x + xDir;
					y = y + yDir;
				}
			}
			return mapUnitToSliceGroupMap;
		}

		private int[] RasterScanSliceGroupMapTypes(IPictureState pictureState, uint mapUnitsInSliceGroup0)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			long sizeOfUpperLeftGroup = pictureState.SliceGroupChangeDirectionFlag
										? (pictureState.SequenceState.PicSizeInMapUnits - mapUnitsInSliceGroup0) : mapUnitsInSliceGroup0;
			var sliceGroupChangeDirectionFlag = pictureState.SliceGroupChangeDirectionFlag ? 1 : 0; //0=normal, 1= reverse
			for (int i = 0; i < picSizeInMapUnits; i++)
			{
				if (i < sizeOfUpperLeftGroup)
					mapUnitToSliceGroupMap[i] = sliceGroupChangeDirectionFlag;
				else
					mapUnitToSliceGroupMap[i] = 1 - sliceGroupChangeDirectionFlag;
			}
			return mapUnitToSliceGroupMap;
		}

		private int[] WipeSliceGroupMapTypes(IPictureState pictureState, uint mapUnitsInSliceGroup0)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			var picWidthInMbs = pictureState.SequenceState.PicWidthInMbs;
			var picHeightInMapUnits = pictureState.SequenceState.PicHeightInMapUnits;
			long sizeOfUpperLeftGroup = pictureState.SliceGroupChangeDirectionFlag
										? (pictureState.SequenceState.PicSizeInMapUnits - mapUnitsInSliceGroup0) : mapUnitsInSliceGroup0;
			var sliceGroupChangeDirectionFlag = pictureState.SliceGroupChangeDirectionFlag ? 1 : 0; //0=right, 1=left
			int k = 0;
			for (int j = 0; j < picWidthInMbs; j++)
			{
				for (int i = 0; i < picHeightInMapUnits; i++)
				{
					if (k++ < sizeOfUpperLeftGroup)
						mapUnitToSliceGroupMap[i * picWidthInMbs + j] = sliceGroupChangeDirectionFlag;
					else
						mapUnitToSliceGroupMap[i * picWidthInMbs + j] = 1 - sliceGroupChangeDirectionFlag;
				}
			}
			return mapUnitToSliceGroupMap;
		}

		private int[] ExplicitSliceGroupMapType(IPictureState pictureState)
		{
			var picSizeInMapUnits = pictureState.SequenceState.PicSizeInMapUnits;
			var mapUnitToSliceGroupMap = new int[picSizeInMapUnits];
			uint[] sliceGroupId = pictureState.SliceGroupId;
			for (int i = 0; i < picSizeInMapUnits; i++)
			{
				mapUnitToSliceGroupMap[i] = (int)sliceGroupId[i % sliceGroupId.Length];
			}
			return mapUnitToSliceGroupMap;
		}
	}
}
