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

namespace Defraser.Test
{
	[TestFixture]
	public class TestVlc
	{
		public const uint BitCode = 0x2B;
		public const int BitLength = 7;
		public const string BitString = "0101011";
		
		private Vlc _vlc;


		[SetUp]
		public void SetUp()
		{
			_vlc = new Vlc(BitCode, BitLength);
		}

		[TearDown]
		public void TearDown()
		{
			_vlc = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorInvalidCode()
		{
			new Vlc(0xDD30BB, 2);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorInvalidLength()
		{
			new Vlc(BitCode, 33);
		}

		[Test]
		public void TestLength32()
		{
			Vlc vlc = new Vlc(0xFF3FFFFF, 32);
			Assert.AreEqual(0xFF3FFFFFU, vlc.Code, "Code for 32 bits VLC");
			Assert.AreEqual(32, vlc.Length, "Length for 32 bits VLC");
			Assert.AreEqual("11111111001111111111111111111111", vlc.ToString(), "ToString() for 32 bits VLC");
			Assert.AreEqual(0xFF3FFFFFU, (uint)vlc.GetHashCode(), "GetHasCode() for 32 bits VLC");
		}

		[Test]
		public void TestCode()
		{
			Assert.AreEqual(BitCode, _vlc.Code, "Code");
		}

		[Test]
		public void TestLength()
		{
			Assert.AreEqual(BitLength, _vlc.Length, "Length");
		}

		[Test]
		public void TestToString()
		{
			Assert.AreEqual(BitString, _vlc.ToString(), "ToString()");
		}

		[Test]
		public void TestEquals()
		{
			Vlc vlc1 = new Vlc(BitCode, BitLength);
			Vlc vlc2 = new Vlc(0x7, 4);
			Assert.AreEqual(_vlc, vlc1, "Equals()");
			Assert.AreNotEqual(_vlc, vlc2, "Equals()");
			Assert.AreNotEqual(_vlc, null, "Equals()");
		}

		[Test]
		public void TestGetHashCode()
		{
			Assert.AreEqual(0xAB, _vlc.GetHashCode(), "GetHashCode()");
		}
	}
}
