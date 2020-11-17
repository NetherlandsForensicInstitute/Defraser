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

using Defraser.Detector.Common;

namespace Defraser.Detector.Mpeg4
{
	// It is used in conjunction with possibly following stuffing_byte(s) for the
	// purpose of stuffing bits to guaranty the VBV buffer regulation.
	internal class Stuffing : Mpeg4Header
	{
		public new enum Attribute
		{
			StuffingByteNumber
		}

		// stuffing_byte: This is the 8-bit string in which the value is ’11111111’
		private const int StuffingByteValue = 0xFF;

		public Stuffing(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.Stuffing)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			int stuffingByteNumber = 0;
			while (parser.ShowBits(8) == StuffingByteValue)
			{
				stuffingByteNumber++;
				parser.GetByte();
			}

			Attributes.Add(new FormattedAttribute<Attribute, int>(Attribute.StuffingByteNumber, stuffingByteNumber));

			return true;
		}
	}
}
