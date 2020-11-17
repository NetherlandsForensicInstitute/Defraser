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
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestIssue2498
	{
		private const string FileNameProjectIssue2498 = "issue2498.dpr";
		private readonly string FileNameIssue2498 = Util.TestdataPath + "issue2498";
		private IProject _projectIssue2498;
		private IInputFile _fileIssue2498;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			File.Delete(FileNameProjectIssue2498);

			_projectIssue2498 = TestFramework.ProjectManager.CreateProject(FileNameProjectIssue2498, "S. Holmes", DateTime.Now, "Scan file 1");

			// Locate detectors
			TestFramework.DetectorFactory.Initialize(".");
			IDetector[] containerDetectors = new[] {
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems")
			};
			IDetector[] codecDetectors = new[] {
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, "MPEG-1/2 Video")
			};

			// Scan test file (1)
			_fileIssue2498 = TestFramework.DetectData(containerDetectors, codecDetectors, _projectIssue2498, FileNameIssue2498);
		}
		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			if (_projectIssue2498 != null)
			{
				TestFramework.ProjectManager.CloseProject(_projectIssue2498);
				_projectIssue2498 = null;
			}
			File.Delete(FileNameProjectIssue2498);
		}

		[Test, Category("Regression")]
		public void TestCheckForMpeg1()
		{
			IList<IDataBlock> dataBlocks = _projectIssue2498.GetDataBlocks(_fileIssue2498);

			Assert.That(dataBlocks.Count, Is.EqualTo(1));
			Assert.That(dataBlocks[0].DataFormat, Is.EqualTo(CodecID.Mpeg1System));

			// Note: The MPEG-1 Video result is too small to be considered a valid result. This is the desired behavior.
			Assert.That(dataBlocks[0].CodecStreams.Count, Is.EqualTo(1));
			Assert.That(dataBlocks[0].CodecStreams[0].DataFormat, Is.EqualTo(CodecID.Unknown));
		}
	}
}
