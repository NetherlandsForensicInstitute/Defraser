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

using Defraser.Detector.Common.Carver;

namespace Defraser.Detector.H264
{
	internal sealed class Slice : INalUnitParser
	{
		internal enum Attribute
		{
			HeaderEndPosition
		}

		private readonly SliceHeader _sliceHeader;
		private readonly SliceData _sliceData;

		public Slice(SliceHeader sliceHeader, SliceData sliceData)
		{
			_sliceHeader = sliceHeader;
			_sliceData = sliceData;
		}

		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			_sliceHeader.Parse(reader, resultState);

			if (reader.Position < reader.Length)
			{
				reader.Result.AddAttribute(Attribute.HeaderEndPosition, GetAbsoluteOffset(reader));
			}

			_sliceData.Parse(reader, resultState);

			// Note: slice_trailing_bits() is handled by INalUnitParser
		}

		private static long GetAbsoluteOffset(INalUnitReader reader)
		{
			if (reader.Position == 0)
			{
				return reader.GetDataPacket(reader.Position, 1).StartOffset;
			}

			return reader.GetDataPacket(reader.Position - 1, 1).EndOffset;
		}
	}
}
