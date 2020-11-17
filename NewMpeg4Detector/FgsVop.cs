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

namespace Defraser.Detector.Mpeg4
{
	internal class FgsVOP : Mpeg4Header
	{
		public new enum Attribute
		{
			CodingType
		}

		public FgsVOP(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.FgsVop)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			// In MPEG-4 every VOP is a child of a VOL header
			VideoObjectLayer VOL = Vol;

			/*int codingType = (int)*/parser.GetBits(2, Attribute.CodingType);
			int moduloTimeBase = 0;

			bool code;
			while ((code = parser.GetBit()) == true)
			{
				moduloTimeBase += code ? 1 : 0;
			}

			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOP header
				return false;
			}
			if (VOL != null && VOL.VopTimeIncrementResolutionBits != 0)
			{
				/*uint timeIncrement = */parser.GetBits(VOL.VopTimeIncrementResolutionBits);
			}
			else
			{
			}

			if (!parser.GetMarkerBit())
			{
				// Does not appear to be a mp4 VOP header
				return false;
			}

			return true;
		}
	}
}
