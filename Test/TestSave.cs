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
using System.Reflection;
using Defraser.DataStructures;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestSave
	{
		private const string ProjectInvestigator = "Paulus Pietsma";
		private static readonly DateTime ProjectCreationDate = DateTime.Now;
		private const string ProjectDescription = "<description>";
		private const string TestFilesPath = "TestSaveFiles";
		private const string TestOutputFile = "output.dat";

		private IDetector _detector;
		private IProject _project1;
		private IProject _project2;
		private string _testDataFile1;
		private string _testDataFile2;
		private string _testDataFile3;
		private byte[] _fullFile1;
		private byte[] _fullFile2;
		private byte[] _fullFile3;
		private string _codebase;
		private string _testOutputFile;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_codebase = Assembly.GetExecutingAssembly().CodeBase;
			_codebase = _codebase.Substring(8);

			if (Directory.Exists(TestFilesPath))
			{
				Directory.Delete(TestFilesPath, true);
			}

			Directory.CreateDirectory(TestFilesPath);
			_testDataFile1 = TestFilesPath + @"/Data1.ejv";
			_testDataFile2 = TestFilesPath + @"/Data2.ejv";
			_testDataFile3 = TestFilesPath + @"/Data3.ejv";
			_testOutputFile = Path.Combine(TestFilesPath, TestOutputFile);

			const byte FileSize = 255;
			_fullFile1 = new byte[FileSize];
			_fullFile2 = new byte[FileSize];
			_fullFile3 = new byte[FileSize];

			for (byte count = 0, i = 1; count < FileSize; count++, i++)
			{
				_fullFile1[count] = count;
				_fullFile2[count] = (byte)(i + 3);
				_fullFile3[count] = (byte)(i + 6);
				if (i == 3) i = 0;
			}

			Util.WriteData(_testDataFile1, _fullFile1);
			Util.WriteData(_testDataFile2, _fullFile2);
			Util.WriteData(_testDataFile3, _fullFile3);

			TestFramework.DetectorFactory.Initialize(".");
			_detector = TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorSave));
			_project1 = TestFramework.ProjectManager.CreateProject(TestFilesPath + @"/TestSave1", ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			_project2 = TestFramework.ProjectManager.CreateProject(TestFilesPath + @"/TestSave2", ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.DetectData(_detector, _project1, _testDataFile1);
			TestFramework.DetectData(_detector, _project1, _testDataFile2);
			TestFramework.DetectData(_detector, _project2, _testDataFile3);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			TestFramework.ProjectManager.Dispose();
			Directory.Delete(TestFilesPath, true);
		}

		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(_testOutputFile))
			{
				File.Delete(_testOutputFile);
			}
		}

		[Test]
		public void TestWriteDataBlockFile1()
		{
			string outputFilePath = string.Format("{0}{1}", _testDataFile1, @"_TestDefraser.MockDetectorSave_0000000000000000-0000000000000033.dat");

			try
			{
				IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
				IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
				TestFramework.SaveAsSeparateFiles(new List<object> {dataBlocks[0] }, TestFilesPath);
				byte[] contents = new byte[51];
				Array.Copy(_fullFile1, contents, 51);
				foreach (FileInfo fi in new DirectoryInfo(TestFilesPath).GetFiles())
				{
					Console.WriteLine(fi);
				}
				Util.AssertArrayEqualsFile(contents, outputFilePath);
			}
			finally
			{
				if (File.Exists(outputFilePath))
				{
					File.Delete(outputFilePath);
				}
			}
		}

		[Test]
		public void TestWriteDataBlocksFile1()
		{
			long dummyHandledBytes = 0L;
			IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
			IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
			Assert.AreEqual(5, dataBlocks.Count);
			TestFramework.WriteDetectables(dataBlocks, _testOutputFile, MockRepository.GenerateStub<IProgressReporter>(), ref dummyHandledBytes, 0);
			byte[] contents = new byte[255];
			Array.Copy(_fullFile1, contents, 255);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestWriteResultFile1()
		{
			IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
			IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
			IResultNode results = TestFramework.GetResults(dataBlocks[1]);
			results = results.Children[0];
			IResultNode[] result = new IResultNode[1];
			result[0] = results.Children[0].ShallowCopy();
			TestFramework.WriteResults(result, _testOutputFile);
			byte[] contents = new byte[17];
			Array.Copy(_fullFile1, 4 * 17, contents, 0, 17);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestWriteContiguousResultsFile2()
		{
			IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile2);
			IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
			IResultNode[] results = (new List<IResultNode>(TestFramework.GetResults(dataBlocks[0]).Children)).ToArray();
			TestFramework.WriteResults(results, _testOutputFile);
			byte[] contents = new byte[255];
			Array.Copy(_fullFile2, contents, 255);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestWriteRandomResultsFile1()
		{
			IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
			IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
			IResultNode results0 = TestFramework.GetResults(dataBlocks[0]);
			IResultNode results1 = TestFramework.GetResults(dataBlocks[1]);
			IResultNode results2 = TestFramework.GetResults(dataBlocks[2]);
			IResultNode results3 = TestFramework.GetResults(dataBlocks[3]);
			IResultNode results4 = TestFramework.GetResults(dataBlocks[4]);
			IResultNode[] results = new IResultNode[5];
			results[0] = results0.Children[0].ShallowCopy();
			results[1] = results2.Children[0].ShallowCopy();
			results[2] = results4.Children[0].ShallowCopy();
			results[3] = results3.Children[0].ShallowCopy();
			results[4] = results1.Children[0].ShallowCopy();
			TestFramework.WriteResults(results, _testOutputFile);
			byte[] contents = new byte[85];
			Array.Copy(_fullFile1, 0, contents, 0, 17);
			Array.Copy(_fullFile1, 102, contents, 17, 17);
			Array.Copy(_fullFile1, 204, contents, 34, 17);
			Array.Copy(_fullFile1, 153, contents, 51, 17);
			Array.Copy(_fullFile1, 51, contents, 68, 17);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestWriteDataBlockFile2()
		{
			string outputFilePath = string.Format("{0}{1}", _testDataFile2, @"_TestDefraser.MockDetectorSave_0000000000000000-00000000000000FF.dat");

			try
			{
				IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile2);
				IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
				TestFramework.SaveAsSeparateFiles(new List<object> { dataBlocks[0] }, TestFilesPath);
				byte[] contents = new byte[255];
				Array.Copy(_fullFile2, contents, 255);
				Util.AssertArrayEqualsFile(contents, outputFilePath);
			}
			finally
			{
				if (File.Exists(outputFilePath))
				{
					File.Delete(outputFilePath);
				}
			}
		}

		[Test]
		public void TestWriteDataBlocksFile2()
		{
			long dummyHandledBytes = 0L;
			IInputFile inputFile = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile2);
			IList<IDataBlock> dataBlocks = _project1.GetDataBlocks(inputFile);
			Assert.AreEqual(1, dataBlocks.Count);
			TestFramework.WriteDetectables(dataBlocks, _testOutputFile, MockRepository.GenerateStub<IProgressReporter>(), ref dummyHandledBytes, 0L);
			byte[] contents = new byte[255];
			Array.Copy(_fullFile2, contents, 255);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestSaveRandomResultsMultiFile()
		{
			IInputFile inputFile1 = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
			IInputFile inputFile2 = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile2);
			IList<IDataBlock> dataBlocks1 = _project1.GetDataBlocks(inputFile1);
			IList<IDataBlock> dataBlocks2 = _project1.GetDataBlocks(inputFile2);
			IResultNode results11 = TestFramework.GetResults(dataBlocks1[1]);
			IResultNode results13 = TestFramework.GetResults(dataBlocks1[3]);
			IResultNode results20 = TestFramework.GetResults(dataBlocks2[0]);
			IResultNode[] results = new IResultNode[3];
			results[0] = results11.Children[0].ShallowCopy();
			results[1] = results20.Children[0].ShallowCopy();
			results[2] = results13.Children[0].ShallowCopy();
			TestFramework.WriteResults(results, _testOutputFile);
			byte[] contents = new byte[39];
			Array.Copy(_fullFile1, 51, contents, 0, 17);
			Array.Copy(_fullFile2, 0, contents, 17, 5);
			Array.Copy(_fullFile1, 153, contents, 22, 17);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}

		[Test]
		public void TestSaveRandomResultsMultiProject()
		{
			IInputFile inputFile1 = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile1);
			IInputFile inputFile2 = Util.FindInputFile(_project1.GetInputFiles(), _testDataFile2);
			IInputFile inputFile3 = Util.FindInputFile(_project2.GetInputFiles(), _testDataFile3);
			IList<IDataBlock> dataBlocks1 = _project1.GetDataBlocks(inputFile1);
			IList<IDataBlock> dataBlocks2 = _project1.GetDataBlocks(inputFile2);
			IList<IDataBlock> dataBlocks3 = _project2.GetDataBlocks(inputFile3);
			IResultNode results12 = TestFramework.GetResults(dataBlocks1[2]);
			IResultNode results20 = TestFramework.GetResults(dataBlocks2[0]);
			IResultNode results30 = TestFramework.GetResults(dataBlocks3[0]);
			IResultNode[] results = new IResultNode[5];
			results[0] = results30.Children[4].ShallowCopy();
			results[1] = results20.Children[0].ShallowCopy();
			results[2] = results12.Children[0].ShallowCopy();
			results[3] = results30.Children[1].ShallowCopy();
			results[4] = results20.Children[0].ShallowCopy();
			TestFramework.WriteResults(results, _testOutputFile);
			byte[] contents = new byte[57];
			Array.Copy(_fullFile3, 60, contents, 0, 15);
			Array.Copy(_fullFile2, 0, contents, 15, 5);
			Array.Copy(_fullFile1, 102, contents, 20, 17);
			Array.Copy(_fullFile3, 15, contents, 37, 15);
			Array.Copy(_fullFile2, 0, contents, 52, 5);
			Util.AssertArrayEqualsFile(contents, _testOutputFile);
		}
	}
}
