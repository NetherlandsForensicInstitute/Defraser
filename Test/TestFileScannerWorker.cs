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
using System.Reflection;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[Ignore]
	[TestFixture]
	public class TestFileScannerWorker
	{
		private const string TestlFileDirectoryName = "../../../../ROOT/testdata";
		private readonly string[,] TestFileNames = new string[,] {
			{ "MPEG-1/2 Systems",	"Back_Up.mpg" },
			{ "MPEG-1/2 Systems",	"cancan_0x046FFE_ParialTwoByteHeader.mpg" },
			{ "MPEG-1/2 Systems",	"cancan_0x019800_PartialFourByteHeader.mpg" },
			{ null,/*partial*/		"cancan_0xEC1800_FalseDetectionOfDataBlock.mpg" },
			{ "3GPP/QT/MP4",		"dancing-skeleton.3gp" },
			{ null,					"dancing-skeleton.xml" },
			{ null,					"Data.ejv" },
			{ null,					"database.dpr" },
			{ "MPEG-1/2 Systems",	"FalseDetectionOfDataBlock_MPEG-2_met_geluid_NFI_part_0-1FFF_1E12800-1E13800.mpg" },
			{ "MPEG-1/2 Systems",	"issue1939.mpg" },
			{ "3GPP/QT/MP4",		"skating-dog.3gp" },
			{ null,					"skating-dog.xml" },
			{ Mpeg2VideoDetector.DetectorName,		"twocans.mpg" }
		};
		private List<IDataBlock> DesiredDataBlocks;

		private DirectoryInfo _testFilesDirectory;
		private IProject _testProject;
		private IInputFile _testFile;


		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			// Create temporary directory
			Uri codeBaseUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
			DirectoryInfo debugDirectory = (new FileInfo(codeBaseUri.LocalPath)).Directory;

			const string TestFrameworkFiles = "TestFrameworkFiles";
			if (Directory.Exists(debugDirectory + "\\" + TestFrameworkFiles))
			{
				Directory.Delete(debugDirectory + "\\" + TestFrameworkFiles, true);
			}

			_testFilesDirectory = debugDirectory.CreateSubdirectory(TestFrameworkFiles);
			FileInfo testFile = new FileInfo(_testFilesDirectory.FullName + "/allfiles.dat");
			FileInfo testProjectFile = new FileInfo(_testFilesDirectory.FullName + "/allfiles.dpr");

			// Create input file and project
			_testProject = TestFramework.ProjectManager.CreateProject(testProjectFile.FullName, "S. Holmes", DateTime.Now, "Scan all files");

			// Locate detectors
			TestFramework.DetectorFactory.Initialize(".");
			IDetector[] containerDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems"),
				Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "3GPP/QT/MP4")
			};
			IDetector[] codecDetectors = new IDetector[] {
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, Mpeg2VideoDetector.DetectorName),
				Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, "MPEG-4")
			};

			// Append test files, create desired data blocks
			DirectoryInfo testDataDirectory = new DirectoryInfo(Util.TestdataPath);
			using (FileStream testFileStream = testFile.OpenWrite())
			{
				long offset = 0;
				DesiredDataBlocks = new List<IDataBlock>();
				for (int i = 0; i < TestFileNames.GetLength(0); i++)
				{
					// Copy file
					byte[] b = File.ReadAllBytes(testDataDirectory.FullName + "/" + TestFileNames[i,1]);
					testFileStream.Write(b, 0, b.Length);
					if (TestFileNames[i, 0] != null)
					{
						IDetector detector = Util.FindDetector(TestFramework.DetectorFactory.Detectors, TestFileNames[i, 0]);
						IDataBlock dataBlock = TestFramework.CreateDataBlock(detector, TestFramework.CreateDataPacket(_testFile, offset, b.Length), true, null);
						DesiredDataBlocks.Add(dataBlock);
					}
					else if (i == 3)
					{
						// Partial detection: File is split in 2 blocks
						IDetector detector = Util.FindDetector(TestFramework.DetectorFactory.Detectors, "MPEG-1/2 Systems");
						DesiredDataBlocks.Add(TestFramework.CreateDataBlock(detector, TestFramework.CreateDataPacket(_testFile, offset, 30), false, null));
						DesiredDataBlocks.Add(TestFramework.CreateDataBlock(detector, TestFramework.CreateDataPacket(_testFile, offset + b.Length - 16, 16), false, null));
					}
					offset += b.Length;

					// Add zero padding
					testFileStream.Write(new byte[256], 0, 256);
					offset += 256;
				}
			}
			
			bool isFullFile = DesiredDataBlocks[0].IsFullFile;
			DesiredDataBlocks[0] = TestFramework.CreateDataBlock(DesiredDataBlocks[0].Detectors.First(), TestFramework.CreateDataPacket(DesiredDataBlocks[0].InputFile, DesiredDataBlocks[0].StartOffset, DesiredDataBlocks[0].Length - 20), isFullFile, null);

			_testFile = TestFramework.DetectData(containerDetectors, codecDetectors, _testProject, testFile.FullName);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			if (_testProject != null)
			{
				TestFramework.ProjectManager.CloseProject(_testProject);
				_testProject = null;
			}
			if (_testFilesDirectory != null)
			{
				_testFilesDirectory.Delete(true);
				_testFilesDirectory = null;
			}
		}

		[Test]
		public void TestResults()
		{
			List<IDataBlock> dataBlocks = new List<IDataBlock>(_testProject.GetDataBlocks(_testFile));
			foreach (IDataBlock dataBlock in DesiredDataBlocks)
			{
				Assert.IsTrue(RemoveDataBlock(dataBlocks, dataBlock), "Data block not detected");
			}
			foreach (IDataBlock dataBlock in dataBlocks)
			{
				foreach (IDataBlock dataBlock2 in DesiredDataBlocks)
				{
					Assert.IsFalse(IsOverlapping(dataBlock, dataBlock2), "Data block overlaps with desired block");
				}
				foreach (IDataBlock dataBlock2 in DesiredDataBlocks)
				{
					Assert.IsFalse(IsOverlapping(dataBlock, dataBlock2), "Data block overlaps with other data block");
				}
			}
			Assert.IsEmpty(dataBlocks);
		}

		private static bool RemoveDataBlock(IList<IDataBlock> dataBlocks, IDataBlock dataBlock)
		{
			foreach (IDataBlock db in dataBlocks)
			{
				// TODO: 'db.Equals(dataBlock)' should only check InputFile, StartOffset and Length
				if (db != null && db.InputFile.Equals(dataBlock.InputFile) &&
					db.StartOffset.Equals(dataBlock.StartOffset) && db.Length.Equals(dataBlock.Length) &&
					db.Detectors.First().Name == dataBlock.Detectors.First().Name)
				{
					dataBlocks.Remove(db);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns whether data blocks <paramref name="a"/> and <paramref name="b"/>
		/// overlap.
		/// </summary>
		/// <param name="a">the first data block</param>
		/// <param name="b">the second data block</param>
		/// <returns>whether the data blocks overlap</returns>
		private static bool IsOverlapping(IDataBlock a, IDataBlock b)
		{
			return ((a != null) && (b != null) &&
					(a.StartOffset < b.EndOffset) && (b.StartOffset < a.EndOffset));
		}
	}
}
