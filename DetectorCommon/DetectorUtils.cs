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

namespace Defraser.Detector.Common
{
	public static class DetectorUtils
	{
		private static readonly sbyte[] Log2Table;

		static DetectorUtils()
		{
			Log2Table = new sbyte[65536];
			Log2Table[0] = -1;

			for (sbyte log2 = 0; log2 < 16; log2++)
			{
				int minValue = (1 << log2);
				int maxValue = (2 << log2);
				for (int i = minValue; i < maxValue; i++)
				{
					Log2Table[i] = log2;
				}
			}
		}

		/// <summary>
		/// Returns the Log2 of <paramref name="value"/> or <code>-1</code> if
		/// <paramref name="value"/> is <code>0</code>.
		/// </summary>
		/// <param name="value">the value to compute the Log2 of</param>
		/// <returns>Floor(Log2(value))</returns>
		public static int Log2(uint value)
		{
			uint highWord = (value >> 16);
			if (highWord != 0)
			{
				return Log2Table[highWord] + 16;
			}

			// Note: upper 16 bits of 'value' are '0'!!
			return Log2Table[value];
		}
	}
}
