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
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Mpeg2Detector
{
	[TestFixture]
	public class TestMpeg2Video
	{
		private const string FileNameProject = "mpeg2video.dpr";
		private readonly string _fileNameMpeg2Video = Util.TestdataPath + "mpeg2video.m2v";

		private DetectorTester _tester;

		[SetUp] 
		public void CreateProject()
		{
			_tester = new DetectorTester().WithProjectFile(FileNameProject).WithDetector(new Mpeg2VideoDetector());
		}

		[TearDown]
		public void CloseProject()
		{
			_tester.Dispose();
		}

		[Test, Category("Regression")]
		public void TestMpeg2VideoDeepSliceParsing()
		{
			var dataBlock = _tester.VerifyScanOf(_fileNameMpeg2Video);

			// Video stream
			var codecStream=dataBlock.CodecStreamCount(Is.EqualTo(0));
			codecStream.VerifyDataPacket().StartOffset(Is.EqualTo(0)).Length(Is.EqualTo(0x16B96));
			codecStream.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.Mpeg2Video));

			dataBlock.VerifyFragmentation().VerifyResultNode().HeaderCount(Is.EqualTo(1));

			var results = dataBlock.VerifyFragmentation().VerifyResultNode();

			var sequenceHeader = results.VerifyChild(0).Name(Is.EqualTo("SequenceHeader")).StartOffset(Is.EqualTo(0x0)).Length(Is.EqualTo(0x4c));

			sequenceHeader.VerifyChild(0).Name(Is.EqualTo("SequenceExtension")).StartOffset(Is.EqualTo(0x4c)).Length(Is.EqualTo(0xA));
			sequenceHeader.VerifyChild(1).Name(Is.EqualTo("SequenceDisplayExtension")).StartOffset(Is.EqualTo(0x56)).Length(Is.EqualTo(0xC));
			var groupOfPictureHeaders = sequenceHeader.VerifyChild(2).Name(Is.EqualTo("GroupOfPicturesHeader")).StartOffset(Is.EqualTo(0x62)).Length(Is.EqualTo(0x8)).HeaderCount(Is.EqualTo(4));

			var pictureHeader = (groupOfPictureHeaders).VerifyChild(0).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0x6A)).Length(Is.EqualTo(0x8)).HeaderCount(Is.EqualTo(31));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("PictureCodingExtension")).StartOffset(Is.EqualTo(0x72)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(1).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x7B)).Length(Is.EqualTo(0x394));
			pictureHeader.VerifyChild(30).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x9638)).Length(Is.EqualTo(0x234));

			pictureHeader = groupOfPictureHeaders.VerifyChild(1).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0x986C)).Length(Is.EqualTo(0x9)).HeaderCount(Is.EqualTo(31));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("PictureCodingExtension")).StartOffset(Is.EqualTo(0x9875)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(1).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x987E)).Length(Is.EqualTo(0x236));
			pictureHeader.VerifyChild(30).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0xC0A5)).Length(Is.EqualTo(0xE7));

			pictureHeader = groupOfPictureHeaders.VerifyChild(2).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0xC18C)).Length(Is.EqualTo(0x9)).HeaderCount(Is.EqualTo(31));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("PictureCodingExtension")).StartOffset(Is.EqualTo(0xC195)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(1).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0xC19E)).Length(Is.EqualTo(0xC6));
			pictureHeader.VerifyChild(30).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0xE837)).Length(Is.EqualTo(0xC2));

			pictureHeader = groupOfPictureHeaders.VerifyChild(3).Name(Is.EqualTo("PictureHeader")).StartOffset(Is.EqualTo(0xE8F9)).Length(Is.EqualTo(0x9)).HeaderCount(Is.EqualTo(31));
			pictureHeader.VerifyChild(0).Name(Is.EqualTo("PictureCodingExtension")).StartOffset(Is.EqualTo(0xE902)).Length(Is.EqualTo(0x9));
			pictureHeader.VerifyChild(1).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0xE90B)).Length(Is.EqualTo(0x37E));
			pictureHeader.VerifyChild(30).Name(Is.EqualTo("Slice")).StartOffset(Is.EqualTo(0x168B6)).Length(Is.EqualTo(0x2E0));
		}
	}
}
