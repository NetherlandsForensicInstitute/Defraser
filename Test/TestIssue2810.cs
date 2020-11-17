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
	public class TestIssue2810
	{
		private const string FileNameProject = "issue2810.dpr";
		private IProject _project;

		[Test, Category("Regression")]
		public void Test()
		{
			string TestFilePath = Util.TestdataPath + "issue2810";

			File.Delete(FileNameProject);

			_project = TestFramework.ProjectManager.CreateProject(FileNameProject, "S. Holmes", DateTime.Now, "Scan file 1");

			// Locate detectors
			TestFramework.DetectorFactory.Initialize(".");
			IDetector containerDetector = Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems");
			containerDetector.ResetConfiguration();
			IDetector[] containerDetectors = new IDetector[] { containerDetector };

			IDetector codecDetector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, Mpeg2VideoDetector.DetectorName);
			codecDetector.ResetConfiguration();
			IDetector[] codecDetectors = new IDetector[] { codecDetector };

			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);

			// Scan test file
			IInputFile inputFile = TestFramework.DetectData(containerDetectors, codecDetectors, _project, TestFilePath);

			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(inputFile);

			Assert.That(dataBlocks.Count, Is.EqualTo(1));
			Assert.That(dataBlocks[0].DataFormat, Is.EqualTo(CodecID.Mpeg2System));

			Assert.That(dataBlocks[0].CodecStreams.Count, Is.EqualTo(1));
			Assert.That(dataBlocks[0].CodecStreams[0].DataFormat, Is.EqualTo(CodecID.Mpeg2Video));
		}

		[TearDown]
		public void TearDown()
		{
			if (_project != null)
			{
				TestFramework.ProjectManager.CloseProject(_project);
				_project = null;
			}
			File.Delete(FileNameProject);
		}
	}
}
