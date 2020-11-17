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

using System;

namespace Defraser.Detector.Common
{
	public static class FourCCExtensionMethods
	{
		/// <summary>
		/// Gets the 4-character-code for the given string.
		/// </summary>
		/// <param name="s">the string to convert to a 4CC</param>
		/// <returns>the 4CC</returns>
		public static uint To4CC(this string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (s.Length != 4)
			{
				throw new ArgumentException("String should consists of 4 characters.", "s");
			}

			uint fourCC = 0;
			foreach (char c in s)
			{
				if (c > 255)
				{
					throw new ArgumentException("String contains non-ASCII characters.", "s");
				}
				fourCC = (fourCC << 8) | c;
			}
			return fourCC;
		}

		/// <summary>
		/// Gets the string representation of the given <paramref name="fourCC"/>.
		/// Control characters are replaced by spaces.
		/// </summary>
		/// <paramref name="fourCC"/>the 4-character-code</param>
		/// <returns>the 4-character-code as string</returns>
		public static string ToString4CC(this uint fourCC)
		{
			if (fourCC == 0)
			{
				return string.Empty;
			}

			char[] chars =
			{
				Char.IsControl((char)(fourCC >> 24)) ? ' ' : (char)(fourCC >> 24),
				Char.IsControl((char)((fourCC >> 16) & 0xFF)) ? ' ' : (char)((fourCC >> 16) & 0xFF),
				Char.IsControl((char)((fourCC >> 8) & 0xFF)) ? ' ' : (char)((fourCC >> 8) & 0xFF),
				Char.IsControl((char)(fourCC & 0xFF)) ? ' ' : (char)(fourCC & 0xFF)
			};
			return new string(chars);
		}

		/// <summary>
		/// Returns whether the given <paramref name="fourCC"/> might be valid.
		/// </summary>
		/// <param name="fourCC">the 4-character-code</param>
		/// <returns>false if the 4CC is invalid, true otherwise</returns>
		public static bool IsValid4CC(this uint fourCC)
		{
			const int ASCII_US = 0x1F;	// Last control character code
			const int ASCII_DEL = 0x7F;

			if ((fourCC & ~0xff) == 0x6D730000) return true;	// "ms\u0000\u00??"

			// 4 spaces is not a valid 4CC
			if (fourCC == 0x20202020)
			{
				return false;
			}

			// Check for illegal (non-printable) characters
			uint code = fourCC;
			for (int i = 0; i < sizeof(uint); i++)
			{
				byte ch = (byte)(code & 0xFF);
				if (ch <= ASCII_US || ch == ASCII_DEL)
				{
					return false;
				}
				code >>= 8;
			}
			return true;
		}

		/// <summary>
		/// Returns whether the given <paramref name="c"/> is a hexadecimal digit.
		/// </summary>
		/// <param name="c">the character</param>
		/// <returns>true when <paramref name="c"/> is hexadecimal, else false</returns>
		public static bool IsHexDigit(this char c)
		{
			return ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
		}
	}
}
