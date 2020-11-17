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
using Defraser.Detector.Common;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestFourCCExtensionMethods
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestTo4CCNullArgument()
		{
			string test = null;
			test.To4CC();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestTo4CCContainingNonAsciiCharacters()
		{
			string test = Char.ConvertFromUtf32(0x1234) + Char.ConvertFromUtf32(0x1235) + Char.ConvertFromUtf32(0x1236) + Char.ConvertFromUtf32(0x1237);
			Assert.That(test.Length, Is.EqualTo(4));
			test.To4CC();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestTo4CCStringToLong()
		{
			//StyleCop (4.3) gives Warning SA0102 on next line: A syntax error has been discovered in file ...
			//"test123".To4CC();
			const string test123 = "test123";
			test123.To4CC();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestTo4CCStringToShort()
		{
			//StyleCop (4.3) gives Warning SA0102 on next line: A syntax error has been discovered in file ...
			//"tes".To4CC();
			const string tes = "tes";
			tes.To4CC();
		}

		[Test]
		public void TestTo4CCHappyFlow()
		{
			Assert.That("test".To4CC(), Is.EqualTo(0x74657374));
		}

		[Test]
		public void TestToString4CC0Value()
		{
			Assert.That(0U.ToString4CC(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void TestToString4CCHappyFlow()
		{
			Assert.That(0x74657374U.ToString4CC(), Is.EqualTo("test"));
		}

		[Test]
		public void IsValid4CCWithSpaces()
		{
			uint fourCC = "    ".To4CC();
			Assert.That(fourCC.IsValid4CC(), Is.EqualTo(false));
		}

		[Test]
		public void IsValid4CCWithNonPrintableCharacters()
		{
			Assert.That(0x74657300U.IsValid4CC(), Is.EqualTo(false), "0x00 is not printable");
			Assert.That(0x7465731FU.IsValid4CC(), Is.EqualTo(false), "0x1F is not printable");
			Assert.That(0x74657320U.IsValid4CC(), Is.EqualTo(true), "0x20 is printable");
			Assert.That(0x7465737EU.IsValid4CC(), Is.EqualTo(true), "0x7E is printable");
			Assert.That(0x7465737FU.IsValid4CC(), Is.EqualTo(false), "0x7F is not printable");
			Assert.That(0x74657380U.IsValid4CC(), Is.EqualTo(true), "0x80 is printable");
			Assert.That(0x746573FFU.IsValid4CC(), Is.EqualTo(true), "0xFF is printable");
			Assert.That(0x00657380U.IsValid4CC(), Is.EqualTo(false), "0x80 is not printable");
		}

		[Test]
		public void IsHexDigit()
		{
			// positive corner cases
			Assert.That('0'.IsHexDigit() == true);
			Assert.That('9'.IsHexDigit() == true);
			Assert.That('a'.IsHexDigit() == true);
			Assert.That('f'.IsHexDigit() == true);
			Assert.That('A'.IsHexDigit() == true);
			Assert.That('F'.IsHexDigit() == true);

			// Negative corner cases
			Assert.That('\0'.IsHexDigit() == false);
			Assert.That(((char)(0xFF)).IsHexDigit() == false);
			Assert.That('/'.IsHexDigit() == false);
			Assert.That('@'.IsHexDigit() == false);
			Assert.That('G'.IsHexDigit() == false);
			Assert.That(((char)(0x60)).IsHexDigit() == false); // character before 'a'
			Assert.That('g'.IsHexDigit() == false);
		}
	}
}
