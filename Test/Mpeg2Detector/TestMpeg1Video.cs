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

using Defraser.Detector.Mpeg2.Video;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Mpeg2Detector
{
	[TestFixture]
	public class TestMpeg1Video
	{
		private const string FileNameProject = "mpeg2video.dpr";
		private readonly string _fileNameMpeg1Video = Util.TestdataPath + "mpeg1video.m1v";

		private DetectorTester _tester;

		[TestFixtureSetUp]
		public void CreateProject()
		{
			_tester = new DetectorTester().WithProjectFile(FileNameProject).WithDetector(new Mpeg2VideoDetector());
		}

		[TestFixtureTearDown]
		public void CloseProject()
		{
			_tester.Dispose();
		}

		[Test, Category("Regression")]
		public void TestMpeg1VideoDeepSliceParsing()
		{
			var dataBlock = _tester.VerifyScanOf(_fileNameMpeg1Video);

			// Video stream
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			dataBlock.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.Mpeg1Video));
			dataBlock.VerifyDataPacket().Length(Is.EqualTo(0x9588));

			var rootNode = dataBlock.VerifyFragmentation().VerifyResultNode();
			rootNode.HeaderCount(Is.EqualTo(1));

			var firstHeader = rootNode.VerifyChild(0).Name(Is.EqualTo("SequenceHeader")).StartOffset(Is.EqualTo(0x0)).Length(Is.EqualTo(0x4c));

			firstHeader.VerifyChild(0).Name(Is.EqualTo("UserData")).StartOffset(Is.EqualTo(0x4c)).Length(Is.EqualTo(0x30));
			var groupOfPictureHeaders = firstHeader.VerifyChild(1).Name(Is.EqualTo("GroupOfPicturesHeader")).StartOffset(Is.EqualTo(0x7c)).Length(Is.EqualTo(0x8));

			var pictureHeader = groupOfPictureHeaders.VerifyChild(0).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0x84)).Length(Is.EqualTo(0x8));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x8c)).Length(Is.EqualTo(0x5df1));

			pictureHeader = groupOfPictureHeaders.VerifyChild(1).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0x5e7d)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x5e86)).Length(Is.EqualTo(0x2e30));

			pictureHeader = groupOfPictureHeaders.VerifyChild(2).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0x8cb6)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x8cbf)).Length(Is.EqualTo(0x8c9));
		}
	}
}
