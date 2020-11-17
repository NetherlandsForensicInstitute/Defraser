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
using Defraser.Detector.Mpeg2.Video;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestIssue2496
	{
		private const string FileNameProjectIssue2496 = "issue2496.dpr";
		private readonly string FileNameIssue2496 = Util.TestdataPath + "issue2496";
		private IProject _projectIssue2496;
		private IInputFile _fileIssue2496;
		private const string MaxSystemHeaderCount = "Max System Header Count";

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			File.Delete(FileNameProjectIssue2496);

			_projectIssue2496 = TestFramework.ProjectManager.CreateProject(FileNameProjectIssue2496, "S. Holmes", DateTime.Now, "Scan file 1");

			// Locate detectors (and change configuration)
			TestFramework.DetectorFactory.Initialize(".");

			IDetector systemDetector = Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems");
			systemDetector.SetConfigurationItem(MaxSystemHeaderCount, "3");

			IDetector[] containerDetectors = new IDetector[] { systemDetector };
			IDetector[] codecDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, Mpeg2VideoDetector.DetectorName)
			};

			// Scan test file (1)
			_fileIssue2496 = TestFramework.DetectData(containerDetectors, codecDetectors, _projectIssue2496, FileNameIssue2496);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			if (_projectIssue2496 != null)
			{
				TestFramework.ProjectManager.CloseProject(_projectIssue2496);
				_projectIssue2496 = null;
			}
			File.Delete(FileNameProjectIssue2496);
		}

		[Test, Category("Regression")]
		public void TestCheckForSpuriousVideoResult()
		{
			IList<IDataBlock> dataBlocks = _projectIssue2496.GetDataBlocks(_fileIssue2496);

			// Check data blocks
			Assert.That(dataBlocks.Count, Is.EqualTo(2));
			Assert.That(dataBlocks[0].DataFormat, Is.EqualTo(CodecID.Mpeg1System));
			Assert.That(dataBlocks[1].DataFormat, Is.EqualTo(CodecID.Mpeg1System));
			Assert.That(dataBlocks[0].CodecStreams.Count, Is.EqualTo(0));
			Assert.That(dataBlocks[1].CodecStreams.Count, Is.EqualTo(1));
			Assert.That(dataBlocks[1].CodecStreams[0].DataFormat, Is.EqualTo(CodecID.Unknown));
		}
	}
}
