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
using System.Linq;
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestIssue2256
	{
		private const string FileNameTestProject1 = "test1.dpr";
		private const string FileNameTestProject2 = "test2.dpr";
		private readonly string FileName3GP = Util.TestdataPath + "3GP_mp4v.3gp";
		private IProject _testProject1;
		private IProject _testProject2;
		private IInputFile _testFile1;
		private IInputFile _testFile2;

		[SetUp]
		public void TestFixtureSetup()
		{
			File.Delete(FileNameTestProject1);
			File.Delete(FileNameTestProject2);

			_testProject1 = TestFramework.ProjectManager.CreateProject(FileNameTestProject1, "S. Holmes", DateTime.Now, "Scan file 1");
			_testProject2 = TestFramework.ProjectManager.CreateProject(FileNameTestProject2, "S. Holmes", DateTime.Now, "Scan file 2");
		}

		[TearDown]
		public void TestFixtureTeardown()
		{
			if (_testProject1 != null)
			{
				TestFramework.ProjectManager.CloseProject(_testProject1);
				_testProject1 = null;
			}
			if (_testProject2 != null)
			{
				TestFramework.ProjectManager.CloseProject(_testProject2);
				_testProject2 = null;
			}
			File.Delete(FileNameTestProject1);
			File.Delete(FileNameTestProject2);
		}

		[Test, Category("Regression")]
		public void TestCheckFileScannerUsingNewMpeg4Detector()
		{
			ScanFile("MPEG-4 Video/H.263");
			CompareResults();
		}

		private void ScanFile(string codecDetector)
		{
			// Locate detectors
			TestFramework.DetectorFactory.Initialize(".");
			IDetector[] containerDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems"),
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "3GPP/QT/MP4")
			};
			IDetector[] codecDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, codecDetector)
			};

			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);

			// Scan test file (1)
			_testFile1 = TestFramework.DetectData(containerDetectors, codecDetectors, _testProject1, FileName3GP);

			Array.Reverse(containerDetectors);

			// Scan test file (2)
			_testFile2 = TestFramework.DetectData(containerDetectors, codecDetectors, _testProject2, FileName3GP);
		}

		private void CompareResults()
		{
			IList<IDataBlock> dataBlocks1 = _testProject1.GetDataBlocks(_testFile1);
			IList<IDataBlock> dataBlocks2 = _testProject2.GetDataBlocks(_testFile2);

			Assert.AreEqual(dataBlocks1.Count, dataBlocks2.Count, "Number of data blocks detected");

			for (int i = 0; i < dataBlocks1.Count; i++)
			{
				Assert.AreEqual(dataBlocks1[i].DataFormat, dataBlocks2[i].DataFormat, "Detected data blocks differ");
				Assert.AreEqual(dataBlocks1[i].Detectors.Count(), dataBlocks2[i].Detectors.Count(), "Detected data blocks differ");
				for (int detectorIndex = 0; detectorIndex < dataBlocks1[i].Detectors.Count(); detectorIndex++)
				{
					Assert.AreEqual(dataBlocks1[i].Detectors.ElementAt(detectorIndex), dataBlocks2[i].Detectors.ElementAt(detectorIndex), "Detected data blocks differ");
				}
				Assert.AreEqual(dataBlocks1[i].InputFile.Name, dataBlocks2[i].InputFile.Name, "Detected data blocks differ");
				Assert.AreEqual(dataBlocks1[i].StartOffset, dataBlocks2[i].StartOffset, "Detected data blocks differ");
				Assert.AreEqual(dataBlocks1[i].EndOffset, dataBlocks2[i].EndOffset, "Detected data blocks differ");
				Assert.AreEqual(dataBlocks1[i].IsFullFile, dataBlocks2[i].IsFullFile, "Detected data blocks differ");
				// TODO Assert.AreEqual(dataBlocks1[i].CodecStreams, dataBlocks2[i].CodecStreams, "Detected data blocks differ");
			}
		}
	}
}
