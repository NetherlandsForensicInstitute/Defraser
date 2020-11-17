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
using System.Drawing;
using System.IO;
using System.Linq;
using Defraser.DataStructures;
using Defraser.Detector.H264;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Interface;
using NUnit.Framework;
using Defraser.FFmpegConverter;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	#region SetupClass
	[SetUpFixture]
	public class TestFFmpegSetup
	{
		private static FFmpegManager _ffmpegManager;

		[SetUp]
		public void SetUp()
		{
			_ffmpegManager = new FFmpegManager(TestFramework.CreateFrameConverter(), null);
		}

		[TearDown]
		public void TearDown()
		{
			_ffmpegManager.Dispose();
			_ffmpegManager = null;
		}

		#region Properties
		public static FFmpegManager FFmpegManager
		{
			get { return _ffmpegManager;  }
		}
		#endregion Properties
	}
	#endregion SetupClass

	/// <summary>
	/// Unit tests for the <see cref="FFmpegConverter"/> class.
	/// </summary>
	[TestFixture]
	public class TestFFmpeg
	{
		private const string FileNameTestProject = "FFmpegTestProject.dpr";
		private readonly string FileNameMpeg1 = Util.TestdataPath + "MPEG-1 met geluid - chimp.mpeg";
		private readonly string FileNameMpeg2 = Util.TestdataPath + "resolutionchange - MPEG2.mpg";
		private readonly string FileName3Gp = Util.TestdataPath + "3GP_K800i.3gp";
		private readonly string FileNameH264 = Util.TestdataPath + "cavlc-b.h264";

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullHeaderDecodeTest()
		{
			TestFFmpegSetup.FFmpegManager.FrameConvertor.FrameToBitmap(null);
		}

		[Test]
		public void EmptyHeaderInformationDecodeTest()
		{
			MockDetector detector = new MockDetector("TestDectector");
			MockHeader header = new MockHeader(Enumerable.Repeat<IDetector>(detector, 1), new MockHeaderName());

			Assert.IsNull(TestFFmpegSetup.FFmpegManager.FrameConvertor.FrameToBitmap(header));
		}

		[Test]
		public void Mpeg1FrameDecodeTest()
		{
			Bitmap bitmap = ScanFile(FileNameMpeg1, "MPEG-1/2 Systems", Mpeg2VideoDetector.DetectorName);
			CheckBitmapConstraints(bitmap, Is.EqualTo(160), Is.EqualTo(120));
		}

		[Test]
		public void Mpeg2FrameDecodeTest()
		{
			Bitmap bitmap = ScanFile(FileNameMpeg2, "MPEG-1/2 Systems", Mpeg2VideoDetector.DetectorName);
			CheckBitmapConstraints(bitmap, Is.EqualTo(1920), Is.EqualTo(1080));
		}

		[Test]
		public void H263FrameDecodeTest()
		{
			Bitmap bitmap = ScanFile(FileName3Gp, "3GPP/QT/MP4", "MPEG-4 Video/H.263");
			CheckBitmapConstraints(bitmap, Is.EqualTo(176), Is.EqualTo(144));
		}

		[Test]
		public void H264FrameDecodeTest()
		{
			Bitmap bitmap = ScanFile(FileNameH264, H264Detector.DetectorName);
			CheckBitmapConstraints(bitmap, Is.EqualTo(696), Is.EqualTo(604));
		}

		private static void CheckBitmapConstraints(Bitmap bitmap, Constraint width, Constraint height)
		{
			Assert.IsNotNull(bitmap, "Convertion failed, bitmap is null.");
			Assert.That(bitmap.Width, width, "Bitmap Width doesn't match.");
			Assert.That(bitmap.Height, height, "Bitmap Height doesn't match.");
		}

		private static Bitmap ScanFile(string filename, string codecDetectorName)
		{
			TestFramework.DetectorFactory.Initialize(".");

			IProject testProject = TestFramework.ProjectManager.CreateProject(FileNameTestProject, "S. Holmes", DateTime.Now, "File scan, used to test creating of thumbs.");

			IDetector codecDetector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, codecDetectorName);
			IInputFile testFile = TestFramework.DetectData(codecDetector, testProject, filename);
			IList<IDataBlock> dataBlocks = testProject.GetDataBlocks(testFile);
			IResultNode resultNode = TestFramework.GetResults(dataBlocks[0]);

			// Get the first I-frame from the video stream
			IResultNode keyFrameNode = FindKeyFrame(resultNode);
			Assert.IsNotNull(keyFrameNode, "Could not find any keyframe in "+Path.GetFileName(filename)+" using codec "+codecDetectorName+". Please check decoder's unittests.");
			Bitmap bitmap = TestFFmpegSetup.FFmpegManager.FrameConvertor.FrameToBitmap(keyFrameNode);

			TestFramework.ProjectManager.CloseProject(testProject);
			File.Delete(FileNameTestProject);

			return bitmap;
		}

		private static Bitmap ScanFile(string filename, string containerDetectorName, string codecDetectorName)
		{
			TestFramework.DetectorFactory.Initialize(".");

			IProject testProject = TestFramework.ProjectManager.CreateProject(FileNameTestProject, "S. Holmes", DateTime.Now, "File scan, used to test creating of thumbs.");

			IDetector codecDetector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, codecDetectorName);
			IDetector containerDetector = Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, containerDetectorName);
			IInputFile testFile = TestFramework.DetectData(new[] {containerDetector}, new[] {codecDetector}, testProject, filename);
			IList<IDataBlock> dataBlocks = testProject.GetDataBlocks(testFile);
			IResultNode resultNode = TestFramework.GetResults(dataBlocks[0].CodecStreams[0]);

			// Get the first I-frame from the video stream
			IResultNode keyFrameNode = FindKeyFrame(resultNode);
			Assert.IsNotNull(keyFrameNode, "Could not find any keyframe in "+Path.GetFileName(filename)+" using codec "+codecDetectorName+". Please check decoder's unittests.");
			Bitmap bitmap = TestFFmpegSetup.FFmpegManager.FrameConvertor.FrameToBitmap(keyFrameNode);

			TestFramework.ProjectManager.CloseProject(testProject);
			File.Delete(FileNameTestProject);

			return bitmap;
		}

		private static IResultNode FindKeyFrame(IResultNode node)
		{
			if (node.Detectors.Any(d => node.IsKeyframe()))
			{
				return node;
			}

			foreach (IResultNode childNode in node.Children)
			{
				IResultNode keyFrameNode = FindKeyFrame(childNode);
				if (keyFrameNode != null)
				{
					return keyFrameNode;
				}
			}
			return null;
		}
	}
}
