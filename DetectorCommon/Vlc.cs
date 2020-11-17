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
using System.Text;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Describes a Variable-Length Code (VLC).
	/// </summary>
	public class Vlc
	{
		private readonly uint _code;
		private readonly int _length;

		#region Properties
		/// <summary>The right-aligned binary code.</summary>
		public uint Code { get { return _code; } }
		/// <summary>The length of the code in bits.</summary>
		public int Length { get { return _length; } }
		#endregion Properties

		/// <summary>
		/// Creates a Variable-Length Code.
		/// </summary>
		/// <param name="code">the right-aligned bit code</param>
		/// <param name="length">the length in bits</param>
		public Vlc(uint code, int length)
		{
			if (length < 1 || length > 32)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			if (length < 32 && code >= (1U << length))
			{
				throw new ArgumentOutOfRangeException("code");
			}

			_code = code;
			_length = length;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(_length);
			for (uint mask = 1U << (_length - 1); mask != 0; mask >>= 1)
			{
				sb.Append(((_code & mask) == 0) ? '0' : '1');
			}
			return sb.ToString();
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return (obj is Vlc) && this.Equals(obj as Vlc);
		}

		public bool Equals(Vlc vlc)
		{
			return this._code == vlc._code && this._length == vlc._length;
		}

		public override int GetHashCode()
		{
			return (int)(_code | (1U << _length));
		}
		#endregion Equals method
	}
}
