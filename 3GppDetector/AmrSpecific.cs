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

namespace Defraser.Detector.QT
{
	internal class AmrSpecific : QtAtom
	{
		public new enum Attribute
		{
			/// <summary>
			/// Four character code of the manufacturer of the codec.
			/// </summary>
			Vendor,
			/// <summary>
			/// Version of the vendor"s decoder which can decode the encoded stream in the best (i.e. optimal) way.
			/// The value is set to 0 if decoder version has no importance for the vendor. It can be safely ignored.
			/// </summary>
			DecoderVersion,
			/// <summary>
			/// The active codec modes.
			/// </summary>
			ModeSet,
			/// <summary>
			/// defines a number N, which restricts the mode changes only at a multiple of N frames.
			/// If no restriction is applied, this value should be set to 0.
			/// </summary>
			ModeChangedPeriod,
			/// <summary>
			/// defines the number of frames to be considered as 'one sample' inside the 3GP file.
			/// This number shall be greater than 0 and less than 16. A value of 1 means each frame
			/// is treated as one sample. A value of 10 means that 10 frames (of duration 20 msec each)
			/// are put together and treated as one sample.
			/// </summary>
			FramesPerSample
		}

		public AmrSpecific(QtAtom previousHeader)
			: base(previousHeader, AtomName.AmrSpecific)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetFourCC(Attribute.Vendor);
			parser.GetByte(Attribute.DecoderVersion);
			parser.GetUShort(Attribute.ModeSet);
			parser.GetByte(Attribute.ModeChangedPeriod);
			parser.GetByte(Attribute.FramesPerSample);

			return this.Valid;
		}
	}
}
