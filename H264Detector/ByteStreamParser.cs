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
using Defraser.Detector.Common.Carver;

namespace Defraser.Detector.H264
{
	internal sealed class ByteStreamParser : IStreamParser
	{
		/// <summary>
		/// Per NAL unit type code the capable parser
		/// </summary>
		private readonly INalUnitParser _nalUnitParser;

		public ByteStreamParser(INalUnitParser nalUnitParser)
		{
			_nalUnitParser = nalUnitParser;
		}

		public void Parse(IH264Reader reader, IResultNodeState resultState)
		{
			if (reader.Position == reader.Length)
			{
				resultState.Invalidate();
				return;
			}

			long nalUnitEndPosition;

			// Remove 'zero byte' and 'start code prefix' for the 'byte stream' format
			if (reader.PeekUInt() == 0x00000001)
			{
				// TODO: hoort eigenlijk bij de StartCodePrefix - zie pg. 305 van ITU-T H.264 (03/2010)
				reader.GetFixedByte(0x00, NalUnitParser.Attribute.ZeroByte);
			}

			reader.GetFixedThreeBytes(0x000001, NalUnitParser.Attribute.StartCodePrefix);
			var windowStart = reader.Position;
			if (reader.NextNalUnit()) //Forward till after next nalunit startcode
			{
				nalUnitEndPosition = reader.Position; //Position of next nalunit start
			}
			else
			{
				nalUnitEndPosition = reader.Length;
			}

			reader.Position = windowStart;//rewind

			if (!resultState.Valid)
			{
				return;
			}

			//Parse remainder with specific nalunitparser (NOTE: Is not a sub-object)
			reader.ParseOneNalUnit(_nalUnitParser, resultState, nalUnitEndPosition);
		}
	}
}
