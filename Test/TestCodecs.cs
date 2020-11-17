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
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestCodecs
	{
		public const CodecID CodecIdentifier = CodecID.Mpeg1Video;
		public const string CodecName = "Mpeg1Video";
		public const string CodecDescriptiveName = "MPEG-1 Video";
		public const string CodecOutputFileExtension = ".mpv";

		public const CodecID InvalidIdentifier = (CodecID)999999999;
		public const string InvalidName = ":-)";


		[Test]
		public void TestParse()
		{
			foreach (string name in Enum.GetNames(typeof(CodecID)))
			{
				Assert.IsTrue(Enum.IsDefined(typeof(CodecID), Codecs.Parse(name)), "Codecs.Parse()");
			}
			Assert.AreEqual(CodecID.Unknown, Codecs.Parse(InvalidName), "Codecs.Parse() invalid name");
		}

		[Test]
		public void TestGetName()
		{
			foreach (CodecID codecID in Enum.GetValues(typeof(CodecID)))
			{
				Assert.IsNotNull(codecID.GetName());
			}
			Assert.AreEqual(CodecName, CodecIdentifier.GetName(), "Codecs.GetName()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetNameInvalidCodecID()
		{
			InvalidIdentifier.GetName();
		}

		[Test]
		public void TestGetDescriptiveName()
		{
			foreach (CodecID codecID in Enum.GetValues(typeof(CodecID)))
			{
				Assert.IsNotNull(codecID.GetDescriptiveName());
			}
			Assert.AreEqual(CodecDescriptiveName, CodecIdentifier.GetDescriptiveName(), "Codecs.GetDescriptiveName()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetDescriptiveNameInvalidCodecID()
		{
			InvalidIdentifier.GetDescriptiveName();
		}

		[Test]
		public void TestGetOutputFileExtension()
		{
			foreach (CodecID codecID in Enum.GetValues(typeof(CodecID)))
			{
				Assert.IsNotNull(Codecs.GetOutputFileExtension(codecID));
			}
			Assert.AreEqual(CodecOutputFileExtension, Codecs.GetOutputFileExtension(CodecIdentifier), "Codecs.GetOutputFileExtension()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetOutputFileExtensionInvalidCodecID()
		{
			Codecs.GetOutputFileExtension(InvalidIdentifier);
		}
	}
}
