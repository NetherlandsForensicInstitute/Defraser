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

using System;
using System.Diagnostics;
using System.Globalization;

namespace Defraser.Detector.Asf
{
	internal enum LengthType
	{
		ValueNotPresent,
		Byte,
		Word,
		DWord
	}

	internal static class LengthTypeExtensions
	{
		internal static int GetLengthInBytes(this LengthType lengthType)
		{
			switch (lengthType)
			{
				case LengthType.ValueNotPresent: return 0;
				case LengthType.Byte: return 1;
				case LengthType.Word: return 2;
				case LengthType.DWord: return 4;
				default:
					Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Value {0} of enum LengthType not handled.", Enum.GetName(typeof(LengthType), lengthType)));
					return 0;
			}
		}

		internal static string PrettyPrint(this LengthType lengthType)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Enum.GetName(typeof(LengthType), lengthType), (int)lengthType);
		}
	}
}
