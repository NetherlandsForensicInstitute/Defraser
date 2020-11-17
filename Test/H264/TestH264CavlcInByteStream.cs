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

using System;
using System.IO;
using System.Linq;
using Defraser.Detector.H264;
using Defraser.Detector.H264.State;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.H264
{
	[TestFixture]
	public class TestH264CavlcInByteStream
	{
		private const string FileNameProject = "h264CavlcInBytestream.dpr";
		private DetectorTester _tester;

		[TestFixtureSetUp]
		public void CreateProject()
		{
			_tester = new DetectorTester().WithProjectFile(FileNameProject).WithDetector(new H264Detector());
		}

		[TestFixtureTearDown]
		public void CloseProject()
		{
			_tester.Dispose();
		}

		private static string TestFile(string fileName)
		{
			var file = Util.TestdataPath + fileName;
			Assert.That(File.Exists(file), "Expected " + file + " to exist.");
			return file;
		}


		[Test]
		public void TestH264Baseline()
		{
			// More details available in test_baseline.txt
			//From the testfile.txt file:
			//SequenceState type                     : IPPP (QP: I 28, P 28)
			//Entropy coding method             : CAVLC
			//ProfileIdc/Level IDC                 : (66,40)
			//-------------------------------------------------------------------------------
			//Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

			//-------------------------------------------------------------------------------
			//00000(NVB)     168
			//00000(IDR)   21984   28  37.427  41.282  42.818       251       0    FRM    3
			//00001( P )    5272   28  36.879  41.048  42.706      6466    6154    FRM    2
			//00002( P )    6808   28  36.722  40.717  42.501     12642   12318    FRM    2
			//-------------------------------------------------------------------------------
			// Total Frames:  3
			var videoFragment = _tester.VerifyScanOf(TestFile("test_baseline.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip baseline encoding profile
			// Video stream
			videoFragment.StartOffset(Is.EqualTo(0));
			videoFragment.DataFormat(Is.EqualTo(CodecID.H264));
			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			slices.VerifyChild(0).VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.HeaderCountEquals(3);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			videoFragment.Length(Is.EqualTo(4279/*disksize*/));
		}

		[Test]
		public void TestH264ForemanExtended()
		{
			// More details available in test_extended.txt
			//From the testfile.txt file:
			// SequenceState type                     : I-B-P-B-P (QP: I 28, P 28, B 30)
			// Entropy coding method             : CAVLC
			// ProfileIdc/Level IDC                 : (88,40)
			//-------------------------------------------------------------------------------
			//Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

			//-------------------------------------------------------------------------------
			//00000(NVB)     168
			//00000(IDR)   22088   28  37.489  41.289  42.851       250       0    FRM    3
			//00002( P )    8600   28  36.795  40.953  42.329      6516    6192    FRM    2
			//00001( B )    2768   30  36.220  41.091  42.678     22975   22641    FRM    0
			//-------------------------------------------------------------------------------
			// Total Frames:  3
			var dataBlocks = _tester.VerifyScanOf(TestFile("test_extended.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip extended encoding profile
			var videoFragment = dataBlocks.VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			slices.VerifyChild(0).VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			slices.HeaderCountEquals(3);
			videoFragment.Length(Is.EqualTo(4203/*disksize*/));
		}

		[Test]
		public void TestH264ForemanExtendedSp()
		{
			// More details available in test_extended_sp.txt
			//From the testfile.txt file:
			// SequenceState type                     : I-B-P-B-P (QP: I 38, P 38, B 38)
			// Entropy coding method             : CAVLC
			// ProfileIdc/Level IDC                 : (88,30)
			//-------------------------------------------------------------------------------
			//Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

			//-------------------------------------------------------------------------------
			//00000(NVB)     168
			//00000(IDR)    8560   38  30.815  38.388  38.832       225       0    FRM    3
			//read_one_frame: cannot read 176 bytes from input file, unexpected EOF!

			//Incorrect FramesToBeEncoded: actual number is      3 frames!
			//00002( B )    1792   38  29.475  37.882  38.803     17584   17286    FRM    0
			//-------------------------------------------------------------------------------
			//total frames:  2
			var dataBlocks = _tester.VerifyScanOf(TestFile("test_extended_sp.264"));
			var videoFragment = dataBlocks.VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			slices.VerifyChild(0).VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			slices.HeaderCountEquals(2);
			videoFragment.Length(Is.EqualTo(1315/*disksize*/));
		}

		[Test]
		public void TestH264ForemanSpSi()
		{
			//From the testfile.txt file:
			// More details available in test_sp_si.txt
			// SequenceState type                     : I-B-P-B-P (QP: I 38, P 38, B 38)
			// Entropy coding method             : CAVLC
			// ProfileIdc/Level IDC                 : (88,30)
			//-------------------------------------------------------------------------------
			//Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

			//-------------------------------------------------------------------------------
			//00000(NVB)     168
			//00000(IDR)    8632   38  30.853  38.122  38.762       424       0    FRM    3
			//read_one_frame: cannot read 176 bytes from input file, unexpected EOF!

			//Incorrect FramesToBeEncoded: actual number is      3 frames!
			//00002( B )    1688   38  29.030  37.646  38.173      4378    3839    FRM    0
			//-------------------------------------------------------------------------------
			// Total Frames:  2
			var dataBlocks = _tester.VerifyScanOf(TestFile("test_sp_si.264"));
			var videoFragment = dataBlocks.VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int) SliceType.OnlyB);
			slices.HeaderCountEquals(2);
			videoFragment.Length(Is.EqualTo(1311/*disksize*/));
		}
	}
}
