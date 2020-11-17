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
	internal class Slice : Mpeg4Header
	{
		public new enum Attribute
		{
			MacroBlockNumber
		}

		/// <summary>
		/// The length of the macro block number is between 1 and 14 bits
		/// </summary>
		private int MacroblockNumberLength
		{
			get
			{
				VopBase vop = Parent as VopBase;
				if (vop == null) return 0;

				uint macroBlockWidth = ((vop.Width + 15) / 16);
				uint macroBlockHeight = ((vop.Height + 15) / 16);

				uint macroBlockCount = macroBlockWidth * macroBlockHeight;

				return Mpeg4Math.Max2Log(macroBlockCount);
			}
		}

		public Slice(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.Slice)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser) || MacroblockNumberLength == 0) return false;

			parser.GetBits(MacroblockNumberLength, Attribute.MacroBlockNumber);

			return true;
		}
	}
}
