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
	public class TestH264CabacInByteStream
	{
		private const string FileNameProject = "h264CabacInBytestream.dpr";
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
		public void TestH264Foreman()
		{
			/**
			 * More details available in test.264.txt
 SequenceState type                     : Hierarchy (QP: I 28, P 28, B 30)
 Entropy coding method             : CABAC
 ProfileIdc/Level IDC                 : (100,40)
-------------------------------------------------------------------------------
Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

-------------------------------------------------------------------------------
00000(NVB)     320
00000(IDR)   24152   27  38.894  42.244  43.945      1894       0    FRM    3
00002( P )   10472   28  38.381  41.961  43.907     76770   67918    FRM    2
00001( B )    2152   33  36.542  42.081  43.557    402704  391572    FRM    0
-------------------------------------------------------------------------------
			 * */
			var videoFragment = _tester.VerifyScanOf(TestFile("test.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip default encoding profile
			// Video stream
			videoFragment.StartOffset(Is.EqualTo(0));
			videoFragment.DataFormat(Is.EqualTo(CodecID.H264));

			var pictureSet = videoFragment.VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name);
			pictureSet.VerifyChild(0).NameEquals(PictureParameterSet.Name);
			pictureSet.VerifyChild(1).NameEquals(PictureParameterSet.Name);
			pictureSet.VerifyChild(2).NameEquals(PictureParameterSet.Name);
			var slices = pictureSet.VerifyChild(2);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			slice.IsNoKeyFrame(); //FIXME: should be slice.IsKeyFrame().WithDimension(176, 144);
			slices.HeaderCountEquals(3);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			videoFragment.Length(Is.EqualTo(4637/*disksize*/));
		}

		[Test]
		public void TestH264ForemanMain()
		{
			/*
			 * More details available in test_main.264.txt
 SequenceState type                     : I-B-P-B-P (QP: I 28, P 28, B 30)
 Entropy coding method             : CABAC
 ProfileIdc/Level IDC                 : (77,40)
-------------------------------------------------------------------------------
Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

-------------------------------------------------------------------------------
00000(NVB)     168
00000(IDR)   21096   28  37.526  41.289  42.851       285       0    FRM    3
00002( P )    8200   28  36.824  40.943  42.317      6518    6157    FRM    2
00001( B )    2696   30  36.222  41.126  42.620     22996   22619    FRM    0
-------------------------------------------------------------------------------
			 */
			var videoFragment = _tester.VerifyScanOf(TestFile("test_main.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip main encoding profile
			// Video stream
			videoFragment.StartOffset(Is.EqualTo(0));
			videoFragment.DataFormat(Is.EqualTo(CodecID.H264));
			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);

			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			slices.HeaderCountEquals(3);
			videoFragment.Length(Is.EqualTo(4020/*disksize*/));
		}

		[Test]
		public void TestH264ForemanFast()
		{
			/** 
			 * More details available in test_fast.264.txt
 SequenceState type                     : Hierarchy (QP: I 28, P 28, B 30)
 Entropy coding method             : CABAC
 ProfileIdc/Level IDC                 : (100,40)
-------------------------------------------------------------------------------
Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

-------------------------------------------------------------------------------
00000(NVB)     320
00000(IDR)   21528   28  37.701  41.571  43.231       446       0    FRM    3
read_one_frame: cannot read 176 bytes from input file, unexpected EOF!

Incorrect FramesToBeEncoded: actual number is      3 frames!
00002( B )    4968   31  35.610  40.705  42.303      1758    1029    FRM    1
00001( B )    1952   32  35.625  41.073  42.614      1772    1161    FRM    0
-------------------------------------------------------------------------------
			 * /
			 */
			var videoFragment = _tester.VerifyScanOf(TestFile("test_fast.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip 
			videoFragment.StartOffset(Is.EqualTo(0)); 
			videoFragment.DataFormat(Is.EqualTo(CodecID.H264));
			var seq=videoFragment.VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name);
			seq.VerifyChild(0).NameEquals(PictureParameterSet.Name);
			seq.VerifyChild(1).NameEquals(PictureParameterSet.Name);
			seq.VerifyChild(2).NameEquals(PictureParameterSet.Name);
			var slices=seq.VerifyChild(2);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			slice.IsNoKeyFrame(); //FIXME: should be slice.IsKeyFrame().WithDimension(176, 144);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			slices.HeaderCountEquals(3);
			videoFragment.Length(Is.EqualTo(3596/*disksize*/));
		}

		[Test]
		public void TestH264ForemanYuv422()
		{
			/** 
			 * More details available in test_yuv422.264.txt
  SequenceState type                     : I-B-P-B-P (QP: I 28, P 28, B 30)
 Entropy coding method             : CABAC
 ProfileIdc/Level IDC                 : (122,40)
-------------------------------------------------------------------------------
Frame     Bit/pic    QP   SnrY    SnrU    SnrV    Time(ms) MET(ms) Frm/Fld Ref

-------------------------------------------------------------------------------
00000(NVB)     176
00000(IDR)   22480   28  37.719  42.499  44.317       446       0    FRM    3
00002( P )    8768   28  37.104  41.957  43.653      6940    6340    FRM    2
00001( B )    2608   30  36.231  42.112  43.897     23675   22968    FRM    0
-------------------------------------------------------------------------------
			 * /
			 */
			var dataBlock = _tester.VerifyScanOf(TestFile("test_yuv422.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip 
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));
			dataBlock.Length(Is.EqualTo(4254));

			var slices = dataBlock.VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.HeaderCountEquals(3);
			slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
		}

		[Test]
		public void TestH264ForemanHigh444()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("test_yuv444.264")); //Generated using http://iphome.hhi.de/suehring/tml/download/jm18.1.zip
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));
			dataBlock.Length(Is.EqualTo(4348));

			//var pictureSet = videoFragment.VerifyResultNode()
			//    .VerifyOnlyChild(SequenceParameterSet.Name);
			//pictureSet.VerifyChild(0).NameEquals(PictureParameterSet.Name);
			//pictureSet.VerifyChild(1).NameEquals(PictureParameterSet.Name);
			//pictureSet.VerifyChild(2).NameEquals(PictureParameterSet.Name);
			//var slices = pictureSet.VerifyChild(2);
			//var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			//slice.VerifyAttribute("SliceType", (int)SliceType.OnlyI);
			//slice.IsNoKeyFrame(); //FIXME: should be slice.IsKeyFrame().WithDimension(176, 144);
			//slices.HeaderCountEquals(3);
			//slices.VerifyChild(1).VerifyAttribute("SliceType", (int)SliceType.OnlyP);
			//slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.OnlyB);
			//videoFragment.Length(Is.EqualTo(4637/*disksize*/));
		}

		[Test]
		public void TestH264In4Cif1500Kbits()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("nature_704x576_25Hz_1500kbits.h264"));
			// Video stream
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));
			var result=dataBlock.VerifyFragmentation().VerifyResultNode();
			result.VerifyChild(0).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(367 /*slices*/);
			result.VerifyChild(1).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(120 /*slices*/);
			result.VerifyChild(2).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(82 /*slices*/);
			result.VerifyChild(3).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(38 /*slices*/);
			result.VerifyChild(4).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(114 /*slices*/);
			result.VerifyChild(5).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(37 /*slices*/);
			var slice= result.VerifyChild(15).NameEquals(SequenceParameterSet.Name).VerifyOnlyChild(PictureParameterSet.Name).HeaderCountEquals(101/*slices*/).VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(704, 576);
			result.HeaderCountEquals(16);//no more sequences
			dataBlock.Length(Is.EqualTo(13339932));
		}

		/// <summary>
		/// sample from http://www.fastvdo.com/H.264.html
		/// </summary>
		[Test]
		public void TestH264In4Cif433Kbits()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("FVDO_Golf_4cif.264"));//Highprofile, 704X576@433 Kbps
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));

			var slices=dataBlock.VerifyFragmentation().VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(704, 576);
			slices.HeaderCountEquals(311);
			dataBlock.Length(Is.EqualTo(717322));
		}


		/// <summary>
		/// sample from http://www.fastvdo.com/H.264.html
		/// </summary>
		[Test]
		public void TestH264Hd()
		{
			//bytestream
			var dataBlock = _tester.VerifyScanOf(TestFile("FVDO_Plane_720p.264"));//Highprofile, 1280X720@1.53 Mbps
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));

			var slices=dataBlock.VerifyFragmentation().VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(1280, 720);
			slices.HeaderCountEquals(298);
			dataBlock.Length(Is.EqualTo(2374764));
		}

		/// <summary>
		/// sample from http://www.fastvdo.com/H.264.html
		/// </summary>
		[Test]
		public void TestH264Qcif()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("FVDO_Freeway_qcif.264"));//Highprofile, 176X144@119 Kbps
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));
			var slices = dataBlock.VerifyFragmentation().VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(176, 144);
			slices.HeaderCountEquals(232);
			dataBlock.Length(Is.EqualTo(143826));
		}

		/// <summary>
		/// sample from http://www.fastvdo.com/H.264.html
		/// </summary>
		[Test]
		public void TestH264Cif()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("FVDO_Girl_cif.264"));//Highprofile, 352X288@345 Kbps
			dataBlock.CodecStreamCount(Is.EqualTo(0));
			dataBlock.StartOffset(Is.EqualTo(0));
			dataBlock.DataFormat(Is.EqualTo(CodecID.H264));
			var slices=dataBlock.VerifyFragmentation().VerifyResultNode()
				.VerifyOnlyChild(SequenceParameterSet.Name)
				.VerifyOnlyChild(PictureParameterSet.Name);
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(352, 288);
			slices.HeaderCountEquals(174);
			dataBlock.Length(Is.EqualTo(313192));
		}

	}
}
