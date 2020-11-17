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
	public class TestDetectorFactory
	{
		private const string BogusPath = "No such path!";
		private const string TestFilesPath = "./TestFrameworkFiles";

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Create directory with invalid plugin DLL
			Directory.CreateDirectory(TestFilesPath);

			using (StreamWriter streamWriter = File.CreateText(TestFilesPath + @"/InvalidPlugin.dll"))
			{
				streamWriter.WriteLine("dummy text file");
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Directory.Delete(TestFilesPath, true);
		}

		[SetUp]
		public void SetUp()
		{
			TestFramework.DetectorFactory.Initialize(".");
		}

		[Test]
		public void TestDetectorCount()
		{
			Assert.AreEqual(14, TestFramework.DetectorFactory.Detectors.Count);
		}

		[Test]
		public void TestContainerDetectorCount()
		{
			List<string> detectorNames = new List<string>();
			foreach(IDetector detector in TestFramework.DetectorFactory.ContainerDetectors)
			{
				detectorNames.Add(detector.Name);
			}
			Assert.That(detectorNames, Has.Member("3GPP/QT/MP4"));
			Assert.That(detectorNames, Has.Member("ASF/WMV"));
			Assert.That(detectorNames, Has.Member("AVI/RIFF"));
			Assert.That(detectorNames, Has.Member("MPEG-1/2 Systems"));
			Assert.That(detectorNames, Has.No.Member("Mock"));

			// Check for extra container detectors
			Assert.AreEqual(5, TestFramework.DetectorFactory.ContainerDetectors.Count);
		}

		[Test]
		public void TestCodecDetectorCount()
		{
			List<string> detectorNames = new List<string>();
			foreach (IDetector detector in TestFramework.DetectorFactory.CodecDetectors)
			{
				detectorNames.Add(detector.Name);
			}

			Assert.That(detectorNames, Has.Member("JPEG (work in progress)"));
			Assert.That(detectorNames, Has.Member(Mpeg2VideoDetector.DetectorName));
			Assert.That(detectorNames, Has.Member("MPEG-4 Video/H.263"));
			Assert.That(detectorNames, Has.Member("H.264"));
			Assert.That(detectorNames, Has.Member("TIFF (work in progress)"));

			// The four other detectors are:
			Assert.That(detectorNames, Has.Member("Mock"));
			Assert.That(detectorNames, Has.Member("TestDefraser.MockDetectorReaderWriter"));
			Assert.That(detectorNames, Has.Member("TestDefraser.MockDetectorSave"));
			Assert.That(detectorNames, Has.Member("MockConfigurableDetector"));

			// Check for extra codec detectors
			Assert.AreEqual(9, TestFramework.DetectorFactory.CodecDetectors.Count);
		}

		[Test]
		public void TestGetDetectorsForCodec()
		{
			// TODO
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestGetDetectorNull()
		{
			TestFramework.DetectorFactory.GetDetector(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetDetectorInvalid()
		{
			TestFramework.DetectorFactory.GetDetector(typeof(string));
		}

		[Test]
		public void TestGetDetector()
		{
			IDetector detector1 = TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorSave));
			Assert.That(detector1.DetectorType, Is.EqualTo(typeof(MockDetectorSave)), "GetDetector(), type");
			IDetector detector2 = TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorSave));
			Assert.AreSame(detector1.DetectorType, detector2.DetectorType, "GetDetector(), single shared instance");
		}

		[Test]
		public void TestGetDetectorVersion()
		{
			IDetector detector = new MockDetectorReaderWriter();
			string version = new DefaultDetectorFormatter().FormatVersion(detector);
			Assert.AreEqual("1.0.3", version, "GetDetectorVersion(), for MockDetectorReaderWriter");
		}

		[Test]
		public void TestInitialize()
		{
			TestFramework.DetectorFactory.Initialize(Environment.CurrentDirectory);
		}

		[Test]
		[ExpectedException(typeof(DirectoryNotFoundException))]
		public void TestInitializeInvalidPath()
		{
			TestFramework.DetectorFactory.Initialize(BogusPath);
		}

		[Test]
		public void TestInitializePlugins()
		{
			TestFramework.DetectorFactory.Initialize(".");
			IDetector detector = TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorReaderWriter));
			Assert.AreEqual("TestDefraser.MockDetectorReaderWriter", detector.Name, "IDetector.Name");
			Assert.AreEqual("Wrapped detector: TestDefraser.MockDetectorReaderWriter", detector.ToString(), "IDetector.ToString()");
			Assert.AreEqual("The dummy plug-in to test the framework.", detector.Description, "IDetector.Description");
			//string[] columns = Framework.GetCustomColumnNames(detector);
			//Assert.AreEqual(5, columns.Length, "DetectorInfo.GetColumns().Length");
			//Assert.AreEqual("General first", columns[0], "DetectorInfo.GetColumns()[0]");
			//Assert.AreEqual("Type1 specific second", columns[3], "DetectorInfo.GetColumns()[3]");
			//Assert.AreEqual("Type2 specific second", columns[4], "DetectorInfo.GetColumns()[4]");
			//Assert.AreEqual("General third", columns[2], "DetectorInfo.GetColumns()[2]");
			//Assert.AreEqual("General fourth", columns[1], "DetectorInfo.GetColumns()[1]");
			Assert.AreEqual(".dat", detector.OutputFileExtension, "DetectorInfo.OutputFileExtension");
			Assert.IsTrue(detector.ColumnInHeader("correct type 1", "General first"), "DetectorInfo.ColumnInHeader()");
			Assert.IsTrue(detector.ColumnInHeader("correct type 1", "Type1 specific second"), "DetectorInfo.ColumnInHeader()");
			Assert.IsFalse(detector.ColumnInHeader("correct type 1", "Type2 specific second"), "DetectorInfo.ColumnInHeader()");
			Assert.IsTrue(detector.ColumnInHeader("correct type 1", "General third"), "DetectorInfo.ColumnInHeader()");
			Assert.IsTrue(detector.ColumnInHeader("correct type 1", "General fourth"), "DetectorInfo.ColumnInHeader()");
			Assert.IsTrue(detector.ColumnInHeader("correct type 2", "Type2 specific second"), "DetectorInfo.ColumnInHeader()");
			Assert.IsFalse(detector.ColumnInHeader("correct type 1", "Something"), "DetectorInfo.ColumnInHeader()");
			detector = Util.FindDetector(TestFramework.DetectorFactory.Detectors, "TestDefraser.MockDetectorSave");
			Assert.AreEqual("TestDefraser.MockDetectorSave", detector.Name, "IDetector.Name");
			Assert.AreEqual("Wrapped detector: Defraser.Test.MockDetectorSave", detector.ToString(), "IDetector.ToString()");
			Assert.AreEqual("The mock detector to test data block and result saving.", detector.Description, "IDetector.Description");
			//columns = Framework.GetCustomColumnNames(detector);
			//Assert.AreEqual(0, columns.Length, "DetectorInfo.GetColumns().Length");
			//Assert.AreEqual(".dat", detector.OutputFileExtension, "DetectorInfo.OutputFileExtension");
		}

		[Test]
		[Ignore]
		public void TestInitializeNoPlugins()
		{
			TestFramework.DetectorFactory.Initialize(TestFilesPath);
			Assert.AreEqual(0, TestFramework.DetectorFactory.Detectors.Count, "No plug-ins must be installed.");
		}
	}
}
