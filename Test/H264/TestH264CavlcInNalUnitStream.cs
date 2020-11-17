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

using System.IO;
using System.Linq;
using Defraser.Detector.H264;
using Defraser.Detector.H264.State;
using Defraser.Detector.QT;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.H264
{
	/// <summary>
	/// NalUnitStream
	/// </summary>
	[TestFixture]
	public class TestH264CavlcInNalUnitStream
	{
		private const string FileNameProject = "h264CavlcInNalunitStream.dpr";
		private DetectorTester _tester;

		[TestFixtureSetUp]
		public void CreateProject()
		{
			_tester = new DetectorTester().WithProjectFile(FileNameProject).WithContainerDetector(new QtDetector()).WithDetector(new H264Detector());

			// Note: These movies contains some very small slices!!
			foreach (IConfigurationItem configurationItem in H264Detector.Configurable.Configuration)
			{
				if (configurationItem.Description == "Min Slice Nal Unit Length")
				{
					configurationItem.SetUserValue("4");
				}
			}

			// Note: The 'SupplementalEnhancementInformation' NAL unit is currently not supported (and therefore not reported)!
		}

		[TestFixtureTearDown]
		public void CloseProject()
		{
			_tester.Dispose();
		}

		private static string TestFile(string fileName)
		{
			var file = Util.TestdataPath + fileName;
			Assert.That(File.Exists(file), "Expected " + file + " to exist");
			return file;
		}
		/// <summary>
		/// and http://support.apple.com/kb/ht1425
		/// </summary>
		[Test]
		public void TestH264NalUnitStreamCavlcM4V()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("sample_iPod-h264.m4v"));
			var videoFragment = dataBlock.CodecStreamCountEquals(2).VerifyCodecStream(1).VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(320, 240);
			slices.HeaderCountEquals(1710);
			slices.VerifyChild(0).VerifyAttribute("SliceType", (int)SliceType.I);
			slices.VerifyChild(2).VerifyAttribute("SliceType", (int)SliceType.P);
			dataBlock.Length(Is.EqualTo(2236480));//audio+video
			videoFragment.Length(Is.EqualTo(970405));//video
		}

		[Test]
		public void TestH264CavlcStreamWithBFrames()
		{
			// This movie contains some very small slices!!
			foreach (IConfigurationItem configurationItem in H264Detector.Configurable.Configuration)
			{
				if (configurationItem.Description == "Min Slice Nal Unit Length")
				{
					configurationItem.SetUserValue("4");
				}
			}

			var dataBlock = _tester.VerifyScanOf(TestFile("cavlc-b.h264"));

			var videoFragment = dataBlock.CodecStreamCountEquals(0).VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(696, 604);
			slices.HeaderCountEquals(2000);
			slices.VerifyChild(0).VerifyAttribute("SliceType").Value(Is.EqualTo((int)SliceType.I));
			slices.VerifyChild(5).NameEquals("CodedSliceOfANonIdrPicture").VerifyAttribute("SliceType", (int)SliceType.P);
			slices.VerifyChild(11).NameEquals("CodedSliceOfANonIdrPicture").VerifyAttribute("SliceType", (int)SliceType.B);
			videoFragment.Length(Is.EqualTo(750817));
		}

		[Test]
		public void TestH264CavlcStreamWithoutBFrames()
		{
			var dataBlock = _tester.VerifyScanOf(TestFile("cavlc.h264"));

			var videoFragment = dataBlock.CodecStreamCountEquals(0).VerifyFragmentation();
			// Video stream
			videoFragment.VerifyDataPacket().StartOffset(Is.EqualTo(0));
			videoFragment.VerifyMetaData().DataFormat(Is.EqualTo(CodecID.H264));
			videoFragment.VerifyDataPacket().Length(Is.EqualTo(0x7D2C7));

			var slices = videoFragment.VerifyResultNode()
				.VerifyOnlyChild("SequenceParameterSet")
				.VerifyOnlyChild("PictureParameterSet");
			var slice = slices.VerifyChild(0).NameEquals("CodedSliceOfAnIdrPicture");
			slice.IsKeyFrame().WithDimension(414, 360);
			slices.HeaderCountEquals(2000);
			slices.VerifyChild(0).VerifyAttribute("SliceType", (int)SliceType.I);
			slices.VerifyChild(5).VerifyAttribute("SliceType", (int)SliceType.P);
			videoFragment.Length(Is.EqualTo(512711));
		}
	}
}
