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
using System.Collections.Generic;
using System.IO;
using Defraser.Detector.Mpeg2.System;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Mpeg2Detector
{
	/// <summary>
	/// Issue 2332 - Limit the maximum block size to allow scanning of very large video files.
	/// Large block will cause memory problems. When the maximum block size is reached
	/// a new contiguous block is started. It is clearly indicated in the name of that
	/// the blocks that they belong together by adding (part x) to the name.
	/// </summary>
	[TestFixture]
	public class MpegFragmentationTest
	{
		private const string FileNameProjectIssue2332 = "issue2332.dpr";
		private readonly string FileNameIssue2332 = Util.TestdataPath + "Back_Up.mpg";
		private const string MaxSystemHeaderCount = "Max System Header Count";
		private const string MaxVideoHeaderCount = "Max Video Header Count";

		private DetectorTester _tester;
		private Mpeg2SystemDetector _systemDetector;
		private Mpeg2VideoDetector _detector;

		[SetUp]
		public void TestFixtureSetup()
		{
			_systemDetector = new Mpeg2SystemDetector();
			_detector = new Mpeg2VideoDetector();
			_tester = new DetectorTester().WithProjectFile(FileNameProjectIssue2332).WithContainerDetector(_systemDetector).WithDetector(_detector);
		}

		[TearDown]
		public void ResetSettings()
		{
			_systemDetector.ResetConfigurationItem(MaxSystemHeaderCount);
			_detector.ResetConfigurationItem(MaxVideoHeaderCount);
			_tester.Dispose();
		}

		[Test]
		public void TestUnfragmented()
		{
			var scannedBlock = _tester.VerifyScanOf(FileNameIssue2332);
			scannedBlock.DataFormat(Is.EqualTo(CodecID.Mpeg1System));

			// Check the data block + video and audio steam
			scannedBlock.CodecStreamCount(Is.EqualTo(2));
			scannedBlock.StartOffset(Is.EqualTo(0)).Length(Is.EqualTo(0x3A280C));
			var fragment = scannedBlock.VerifyFragmentation();
			fragment.Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			fragment.VerifyResultNode().HeaderCount(Is.EqualTo(1642));
			// Video stream
			var videoFragment = scannedBlock.VerifyCodecStream(0).VerifyFragmentation();
			videoFragment.Length(Is.EqualTo(0x2F6171)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));

			var videoResultNode = videoFragment.VerifyResultNode();
			videoResultNode.HeaderCount(Is.EqualTo(31));
			videoResultNode.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			videoResultNode.Last().Name(Is.EqualTo("SequenceEndCode"));

			// Audio stream
			var audioFragment = scannedBlock.VerifyCodecStream(1).VerifyFragmentation();
			audioFragment.Length(Is.EqualTo(0x93ADB)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
		}

		[Ignore("Can not get it to work without breaking other code"), Test]
		public void TestFragmentedContainerStream()
		{
			_systemDetector.SetConfigurationItem(MaxSystemHeaderCount, "2000"); ;
			var dataBlocks = _tester.VerifyScanOfFragmented(FileNameIssue2332);

			Assert.That(dataBlocks.Count, Is.EqualTo(2));

			// Check the first data block + video and audio stream
			var block1 = dataBlocks[0];
			block1.StartOffset(Is.EqualTo(0)).Length(Is.EqualTo(0x237620));
			var fragment = block1.VerifyFragmentation();
			fragment.IsFragmented().Container(Is.Not.Null).Index(Is.EqualTo(0));
			fragment.VerifyResultNode().HeaderCount(Is.EqualTo(1000));

			block1.CodecStreamCountEquals(2);
			// Video stream
			var fragment1 = block1.VerifyCodecStream(1).VerifyFragmentation();
			fragment1.Length(Is.EqualTo(0x1d40e6)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			var resultNode = fragment1.VerifyResultNode();
			resultNode.HeaderCount(Is.EqualTo(19));
			resultNode.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode.Last().Name(Is.EqualTo("Current"));
			// Audio stream
			var audioFragment = block1.VerifyCodecStream(1).VerifyFragmentation();
			audioFragment.Length(Is.EqualTo(0x59947)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));

			// Check the second data block + video and audio stream
			var block2 = dataBlocks[1];
			block2.Length(Is.EqualTo(0x16B1EC)).StartOffset(Is.EqualTo(0x237620));
			var fragment3 = block2.VerifyFragmentation();
			fragment3.Fragmented(Is.False).Container(Is.Not.Null).Index(Is.EqualTo(1));
			fragment3.VerifyResultNode().HeaderCount(Is.EqualTo(642));

			block2.CodecStreamCount(Is.EqualTo(2));
			// Video stream
			var fragment4 = block2.VerifyCodecStream(0).VerifyFragmentation();
			fragment4.Length(Is.EqualTo(0x12208b)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			var resultNode4 = fragment4.VerifyResultNode();
			resultNode4.HeaderCount(Is.EqualTo(20));
			resultNode4.VerifyChild(0).Name(Is.EqualTo("PictureHeader"));
			resultNode4.Last().Name(Is.EqualTo("SequenceEndCode"));

			// Audio stream
			var fragment5 = block2.VerifyCodecStream(1).VerifyFragmentation();
			fragment5.Length(Is.EqualTo(0x3a194)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
		}

		[Test]
		public void TestFragmentedCodecStream()
		{
			_detector.SetConfigurationItem(MaxVideoHeaderCount, "400");
			var dataBlock = _tester.VerifyScanOf(FileNameIssue2332);

			// Check the first data block + video and audio stream
			dataBlock.StartOffset(Is.EqualTo(0)).Length(Is.EqualTo(0x3a280c));
			var fragment = dataBlock.VerifyFragmentation().Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			fragment.VerifyResultNode().HeaderCount(Is.EqualTo(1642));

			dataBlock.CodecStreamCount(Is.EqualTo(4));
			// Video stream fragment 1
			var fragment1 = dataBlock.VerifyCodecStream(0).VerifyFragmentation().Length(Is.EqualTo(0x11317d)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(0));
			var resultNode1 = fragment1.VerifyResultNode().HeaderCount(Is.EqualTo(11));
			resultNode1.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode1.Last().Name(Is.EqualTo("Slice"));
			// Video stream fragment 2
			var fragment2 = dataBlock.VerifyCodecStream(1).VerifyFragmentation().Length(Is.EqualTo(0x115ee1)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(1));
			var resultNode2 = fragment2.VerifyResultNode().HeaderCount(Is.EqualTo(11));
			resultNode2.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode2.Last().Name(Is.EqualTo("Slice"));
			// Video stream fragment 3
			var fragment3 = dataBlock.VerifyCodecStream(2).VerifyFragmentation().Length(Is.EqualTo(0xcd113)).Fragmented(Is.False).Container(Is.Not.Null).Index(Is.EqualTo(2));
			var resultNode3 = fragment3.VerifyResultNode().HeaderCount(Is.EqualTo(9));
			resultNode3.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode3.Last().Name(Is.EqualTo("SequenceEndCode"));
			// Audio stream
			var fragment4 = dataBlock.VerifyCodecStream(3).VerifyFragmentation().Length(Is.EqualTo(0x93adb)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			fragment4.VerifyResultNode().HeaderCountEquals(1).VerifyChild(0).Name(Is.EqualTo("Data"));
		}

		[Ignore("Can not get it to work without breaking other code"), Test]
		public void TestFragmentedContainerAndFragmentedCodecStream()
		{
			_systemDetector.SetConfigurationItem(MaxSystemHeaderCount, "2000");
			_detector.SetConfigurationItem(MaxVideoHeaderCount, "400");
			var dataBlocks = _tester.VerifyScanOfFragmented(FileNameIssue2332);
			Assert.That(dataBlocks.Count, Is.EqualTo(2));

			// Check the first data block + video and audio stream
			var block = dataBlocks[0];
			block.StartOffset(Is.EqualTo(0)).Length(Is.EqualTo(0x237620));
			var fragment = block.VerifyFragmentation().Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(0));
			fragment.VerifyResultNode().HeaderCount(Is.EqualTo(1000));

			block.CodecStreamCount(Is.EqualTo(3));
			// Video stream fragment 1
			var stream = block.CodecStreams[0];
			var fragment1 =  stream.VerifyFragmentation().Length(Is.EqualTo(0x11317d)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(0));
			var resultNode = fragment1.VerifyResultNode().HeaderCount(Is.EqualTo(11));
			resultNode.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode.Last().Name(Is.EqualTo("Current"));
			// Video stream fragment 2
			stream = block.CodecStreams[1];
			var fragment2 =  stream.VerifyFragmentation().Length(Is.EqualTo(0xc0f69)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(1));
			var resultNode1 = fragment2.VerifyResultNode().HeaderCount(Is.EqualTo(8));
			resultNode1.VerifyChild(0).Name(Is.EqualTo("SequenceHeader"));
			resultNode1.Last().Name(Is.EqualTo("Current"));
			// Audio stream
			stream = block.CodecStreams[2];
			var fragment3 =  stream.VerifyFragmentation().Length(Is.EqualTo(0x59947)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			fragment3.VerifyResultNode().HeaderCountEquals(1).VerifyChild(0).Name(Is.EqualTo("Data"));

			// Check the second data block + video and audio stream
			block = dataBlocks[1];
			block.StartOffset(Is.EqualTo(0x237620)).Length(Is.EqualTo(0x16b1ec));
			var fragment4 = block.VerifyFragmentation().Fragmented(Is.False).Container(Is.Not.Null).Index(Is.EqualTo(1));
			fragment4.VerifyResultNode().HeaderCount(Is.EqualTo(642));

			block.CodecStreamCount(Is.EqualTo(3));
			// Video stream fragment 1
			stream = block.CodecStreams[0];
			var fragment5 =  stream.VerifyFragmentation().Length(Is.EqualTo(0x122087)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(0));

			var resultNode3 = fragment5.VerifyResultNode();
			resultNode3.HeaderCount(Is.EqualTo(19));
			resultNode3.VerifyChild(0).Name(Is.EqualTo("PictureHeader"));
			resultNode3.Last().Name(Is.EqualTo("Current"));
			// Video stream fragment 2
			stream = block.CodecStreams[1];
			var fragment6 =  stream.VerifyFragmentation().Length(Is.EqualTo(0x4)).Fragmented(Is.True).Container(Is.Not.Null).Index(Is.EqualTo(1));

			fragment6.VerifyResultNode().HeaderCount(Is.EqualTo(1)).VerifyChild(0).Name(Is.EqualTo("SequenceEndCode"));
			// Audio stream
			stream = block.CodecStreams[2];
			var fragment7 = stream.VerifyFragmentation().Length(Is.EqualTo(0x3a194)).Fragmented(Is.False).Container(Is.Null).Index(Is.EqualTo(0));
			fragment7.VerifyResultNode().HeaderCountEquals(1).VerifyChild(0).Name(Is.EqualTo("Data"));
		}
	}
}
