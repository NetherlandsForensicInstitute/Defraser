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
using System.Diagnostics;
using System.IO;
using System.Management;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestIssue2821
	{
		private const string ProjectFile = @"./performancetest-project.dpr";
		private readonly string TestFileName = Util.TestdataPath + "dancing-skeleton.3gp";

		private IProject _project;

		[SetUp]
		public void SetUp()
		{
			DeleteProjectsFiles();

			TestFramework.DetectorFactory.Initialize(".");

			_project = TestFramework.ProjectManager.CreateProject(ProjectFile, "Kerst Klaas", DateTime.Now, "<description>");
		}

		[Test, Category("Regression")]
		public void TestPerformance()
		{
			IDetector[] containerDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "3GPP/QT/MP4")
			};
			IDetector[] codecDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, "MPEG-4 Video/H.263")
			};

			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);

			Assert.That(detectors, Is.Not.Null);
			Assert.That(detectors.Count, Is.EqualTo(2));

			const long CpuSpeedInMegaHerzMinimalRequired = 2900;
			long cpuSpeed = 3000;//GetCpuSpeedInMegaHerz();

			if (cpuSpeed > CpuSpeedInMegaHerzMinimalRequired)
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				TestFramework.DetectData(containerDetectors, codecDetectors, _project, TestFileName);
				stopwatch.Stop();
				Console.WriteLine("Scan time: {0}ms", stopwatch.ElapsedMilliseconds);
#if DEBUG
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(15000));
#else
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1500));
#endif
			}
			else
			{
				Assert.Ignore(string.Format("Performance test not tested for clock speeds below {0}MHz", CpuSpeedInMegaHerzMinimalRequired));
			}
		}

		private int GetCpuSpeedInMegaHerz()
		{
			ManagementClass managementClass = new ManagementClass("Win32_Processor");
			ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();
			string cpuSpeed = String.Empty;
			foreach (ManagementObject managementObject in managementObjectCollection)
			{
				if (cpuSpeed == String.Empty)
				{// only return cpuInfo from first CPU
					cpuSpeed = managementObject.Properties["CurrentClockSpeed"].Value.ToString();
				}
			}
			return int.Parse(cpuSpeed);
		}


		[TearDown]
		public void TestFixtureTeardown()
		{
			// Close the project files
			if (_project != null)
			{
				TestFramework.ProjectManager.CloseProject(_project);
			}

			DeleteProjectsFiles();
		}

		private void DeleteProjectsFiles()
		{
			if (File.Exists(ProjectFile)) File.Delete(ProjectFile);
		}
	}
}
