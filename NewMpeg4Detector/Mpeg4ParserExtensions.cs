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
	public static class Mpeg4ParserExtensions
	{
		public static byte LeadingZeroCount(this byte value)
		{
			byte integerCount = 0;

			for (integerCount = 0; integerCount < 8; integerCount++)
			{
				if ((0x80 & value) != 0) break;
				value <<= 1;
			}
			return integerCount;
		}

		public static byte ResetFirstOneBit(this byte value)
		{
			byte mask = 0x80;
			while (mask > 0)
			{
				if ((value & mask) > 0) break; // found first bit set to one
				mask >>= 1;
			}
			value &= (byte) ~mask;
			return value;
		}

		public static ulong ResetFirstOneBit(this ulong value)
		{
			ulong mask = 0x8000000000000000L;
			while (mask > 0)
			{
				if ((value & mask) > 0) break; // found first bit set to one
				mask >>= 1;
			}
			return (value &= ~mask);
		}

		public static bool IsValidID(this uint id)
		{
			// Check of the length of the ID is one to four bytes
			if (id > 0x1F) return true;
			return false;
		}
	}
}
