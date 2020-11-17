/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

namespace Defraser.Detector.Avi
{
	/// <summary>
	/// ExampleHeader is a template class for detector development for Defraser. It searches for
	/// blocks of 1 or more consecutive bytes with the value 42, including the next byte. These
	/// blocks are called a Header. Each detected Header will have two parameters, an integer
	/// holding the length of the header and a byte containing the last value of the header.
	/// </summary>
	internal class DigitizingDate : AviChunk
	{
		/// <summary>The possible Attributes of a detected header.</summary>
		public new enum Attribute
		{
			DateTime
		}

		public DigitizingDate(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.DigitizingDate)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			uint maxDateStringLength = (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.DigitizingDateMaxDateStringLength];
			if (parser.BytesRemaining > maxDateStringLength) return false;

			parser.GetString(Attribute.DateTime, parser.BytesRemaining);

			return Valid;
		}
	}
}
