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
	/// AVI is a derivative of the Resource Interchange File Format (RIFF),
	/// which divides a file's data into blocks, or "chunks." Each "chunk" is
	/// identified by a FourCC tag. An AVI file takes the form of a single
	/// chunk in a RIFF formatted file, which is then subdivided into two
	/// mandatory "chunks" and one optional "chunk".
	/// </summary>
	internal class Riff : AviChunk
	{
		/// <summary>The possible Attributes of a detected header.</summary>
		public new enum Attribute
		{
			FileType
		}

		public uint FileType { get; private set; }

		public Riff(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.Riff)
		{
		}

		public override bool Parse(AviParser parser)
		{
			// Call the Parse method of the parent to initialise the header and
			// allow parsing the containing header if there is one.
			if (!base.Parse(parser)) return false;

			FileType = parser.GetFourCC(Attribute.FileType);	// 'AVI ' or 'AVIX'

			return Valid;
		}
	}
}
