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
using Defraser.Detector.Mpeg2.Video;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Mpeg2Detector
{
	[TestFixture]
	public class TestMpeg2Detector
	{
		private const string ZeroByteStuffing = "ZeroByteStuffing";
		// Modified data snipped from file 'twocans.mpg' with issue 2310 at the end
		private static readonly byte[] _zeroByteStuffingAtEndData =
			{
				0x00, 0x00, 0x01, 0xB3, 0x0A, 0x00, 0x70, 0x15,
				0xFF, 0xFF, 0xE0, 0x80, 0x00, 0x00, 0x01, 0xB8,
				0x00, 0x08, 0x00, 0x40, 0x00, 0x00, 0x01, 0x00,
				0x00, 0x0F, 0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x01, 0xFF
			};

		// Modified data snipped from file 'twocans.mpg' with issue 2310 in the middle
		private static readonly byte[] _zeroByteStuffingInMiddleData =
			{
				0x00, 0x00, 0x01, 0xB3, 0x0A, 0x00, 0x70, 0x15,
				0xFF, 0xFF, 0xE0, 0x80, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x01, 0xB8, 0x00, 0x08, 0x00, 0x40, 0x00,
				0x00, 0x01, 0x00, 0x00, 0x0F, 0xFF, 0xF8
			};

		private IDetector _mpeg2Detector;
		private DetectorTester _tester;

		[SetUp]
		public void SetUp()
		{
			_mpeg2Detector = new Mpeg2VideoDetector();
			_tester = new DetectorTester().WithDetector(_mpeg2Detector);
		}

		[TearDown]
		public void TearDown()
		{
			_mpeg2Detector = null;
		}

		/// <summary>
		/// Functional/regression test that exposes issue 2310
		/// in the middle of a snipped from the file twocans.mpg.
		/// </summary>
		[Test, Category("Regression")]
		public void TestIssue2310ZeroByteStuffingInMiddle()
		{
			var rootNode= _tester.Scan(_zeroByteStuffingInMiddleData).VerifyFragmentation().VerifyResultNode();
			rootNode.IsRoot().HeaderCount(Is.EqualTo(1));
			
			var sequenceHeader = rootNode.VerifyChild(0);
			sequenceHeader.HeaderCount(Is.EqualTo(1))//
				.NameEquals("SequenceHeader")//
				.StartOffsetEquals(0)//
				.LengthEquals(15)//
				.VerifyAttribute(ZeroByteStuffing).Value(Is.EqualTo(3));

			var groupOfPictureHeader = sequenceHeader.VerifyChild(0);
			groupOfPictureHeader.HeaderCount(Is.EqualTo(1)) //
				.Name(Is.EqualTo("GroupOfPicturesHeader")) //
				.StartOffset(Is.EqualTo(15)) //
				.Length(Is.EqualTo(8))//
				.VerifyAttribute(ZeroByteStuffing).HasNoValue();

			var pictureHeader = groupOfPictureHeader.VerifyChild(0);
			pictureHeader.HeaderCount(Is.EqualTo(0))//
				.NameEquals("PictureHeader")//
				.StartOffsetEquals(23)//
				.LengthEquals(8)//
				.VerifyAttribute(ZeroByteStuffing).HasNoValue();
		}

		/// <summary>
		/// Functional/regression test that exposes issue 2310
		/// at the end of a snipped from the file twocans.mpg.
		/// </summary>
		[Test, Category("Regression")]
		public void TestIssue2310ZeroByteStuffingAtEnd()
		{
			var rootNode = _tester.Scan(_zeroByteStuffingAtEndData).VerifyFragmentation().VerifyResultNode();
			rootNode.IsRoot().HeaderCount(Is.EqualTo(1));

			var sequenceHeader = rootNode.VerifyChild(0);
			sequenceHeader.NameEquals("SequenceHeader") //
				.StartOffsetEquals(0) //
				.LengthEquals(12) //
				.HeaderCountEquals(1) //
				.VerifyAttribute(ZeroByteStuffing).HasNoValue();

			var groupOfPicturesHeader = sequenceHeader.VerifyChild(0);
			groupOfPicturesHeader.NameEquals("GroupOfPicturesHeader")//
				.StartOffsetEquals(12) //
				.LengthEquals(8) //
				.HeaderCountEquals(1) //
				.VerifyAttribute(ZeroByteStuffing).HasNoValue();

			var pictureHeader = groupOfPicturesHeader.VerifyChild(0);
			pictureHeader.NameEquals("PictureHeader") //
				.StartOffsetEquals(20) //
				.LengthEquals(8) //
				.HeaderCountEquals(0) //
				.VerifyAttribute(ZeroByteStuffing).HasNoValue();
		}
	}
}
