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
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestResultReaderWriter
	{
		private const string PluginName = "TestDefraser.TestPlugin";
		private const string PluginDescription = "The dummy plug-in to test the framework.";
		private const string BogusPath = "No such path!";
		private const string TestFilesPath = "./TestResultReaderWriterFiles";
		private const int StartOffset = 0;
		private const string ProjectInvestigator = "Paulus Pietsma";
		private readonly DateTime ProjectCreationDate = DateTime.Now;
		private const string ProjectDescription = "<description>";

		private IDetector _detector;
		private IProject _project;
		private string _testInvalidPluginFile = null;
		private string _testDataFile = null;
		private byte[] _fullFile = null;
		private string _codebase;

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
			_testInvalidPluginFile = TestFilesPath + @"/InvalidPlugin.dll";
			_testDataFile = TestFilesPath + @"/Data.ejv";

			_fullFile = new byte[518];
			for (int count = 3; count < 259; count++)
			{
				_fullFile[count] = (byte)count;
				_fullFile[259 + count] = (byte)count;
			}
			_fullFile[StartOffset] = System.Text.Encoding.ASCII.GetBytes("E")[0];
			_fullFile[StartOffset+1] = System.Text.Encoding.ASCII.GetBytes("J")[0];
			_fullFile[StartOffset+2] = System.Text.Encoding.ASCII.GetBytes("V")[0];
			_fullFile[259] = System.Text.Encoding.ASCII.GetBytes("E")[0];
			_fullFile[260] = System.Text.Encoding.ASCII.GetBytes("N")[0];
			_fullFile[261] = System.Text.Encoding.ASCII.GetBytes("D")[0];
			Util.WriteData(_testInvalidPluginFile, _fullFile);
			Util.WriteData(_testDataFile, _fullFile);

			File.Create(TestFilesPath + @"/emptyproject").Close();

			TestFramework.DetectorFactory.Initialize(".");
			_detector = TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorReaderWriter));
			_project = TestFramework.ProjectManager.CreateProject(TestFilesPath + @"/TestReaderWriterProject", ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.DetectData(_detector, _project, _testDataFile);
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
		}

		[Test]
		public void TestReaderGetFileNamesVerifyContents()
		{
			Assert.AreEqual(_testDataFile, _project.GetInputFiles()[0].Name, "First file name must be '" + _testDataFile + "'.");
		}

		[Test]
		public void TestReaderGetFileNamesVerifyCount()
		{
			IList<IInputFile> inputFiles = _project.GetInputFiles();
			Assert.AreEqual(1, inputFiles.Count, "Number of files must be 1.");
		}

		[Test]
		public void TestReaderGetDetectorNamesVerifyContents()
		{
			IList<IDetector> detectors = _project.GetDetectors(_project.GetInputFiles()[0]);
			Assert.AreEqual(TestFramework.DetectorFactory.GetDetector(typeof(MockDetectorReaderWriter)), detectors[0], "First detector must be '" + TestFramework.DetectorFactory.Detectors[0].Name + "'.");
		}

		[Test]
		public void TestReaderGetDetectorNamesVerifyCount()
		{
			IList<IDetector> detectors = _project.GetDetectors(_project.GetInputFiles()[0]);
			Assert.AreEqual(1, detectors.Count, "Number of detectors must be 1.");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestReaderGetDetectorsInvalid()
		{
			IList<IDetector> detectors = _project.GetDetectors(null);
		}

		// TODO: The framework currently does not test whether the input file exists in the database
		//[Test]
		//[ExpectedException(typeof(ArgumentException))]
		//public void TestReaderGetDetectorsUnknown()
		//{
		//    IDetector[] detectors = _project.GetDetectors(new DummyInputFile(BogusPath));
		//}

		[Test]
		public void TestReaderGetDataBlocksVerifyCount()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_project.GetInputFiles()[0]);
			Assert.AreEqual(2, dataBlocks.Count, "Number of data blocks must be 2.");
		}

		[Test]
		public void TestReaderGetDataBlocksVerifyContents()
		{
			IInputFile inputFile = _project.GetInputFiles()[0];
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(inputFile);

			Assert.AreEqual(inputFile, dataBlocks[0].InputFile, "DataBlock.InputFile");
			Assert.AreEqual(0, dataBlocks[0].StartOffset, "DataBlock.StartOffset");
			Assert.AreEqual(268, dataBlocks[0].Length, "DataBlock.Length");
			Assert.AreEqual(false, dataBlocks[0].IsFullFile, "DataBlock.IsFullFile");
			IResultNode results = TestFramework.GetResults(dataBlocks[0]);
			Assert.AreEqual(1, results.Children[0].Length, "DataBlock.StartResult.Length");
			Assert.AreEqual("correct type 1", results.Children[0].Name, "DataBlock.StartResult.Name");
			Assert.AreEqual(true, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "DataBlock.StartResult.HasChildren");
			Assert.AreEqual(2, results.Children[0].Attributes.Count, "DataBlock.StartResult.Attributes.Count");

			Assert.AreEqual(inputFile, dataBlocks[1].InputFile, "DataBlock.InputFile");
			Assert.AreEqual(517, dataBlocks[1].StartOffset, "DataBlock.StartOffset");
			Assert.AreEqual(1, dataBlocks[1].Length, "DataBlock.Length");
			Assert.AreEqual(true, dataBlocks[1].IsFullFile, "DataBlock.IsFullFile");
			results = TestFramework.GetResults(dataBlocks[1]);
			Assert.AreEqual(1, results.Children[0].Length, "DataBlock.StartResult.Length");
			Assert.AreEqual("correct type 1", results.Children[0].Name, "DataBlock.StartResult.Name");
			Assert.AreEqual(false, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "DataBlock.StartResult.HasChildren");
			Assert.AreEqual(2, results.Children[0].Attributes.Count, "DataBlock.StartResult.Attributes.Count");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestReaderGetDataBlocksInputFileInvalid()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(null);
		}

		// TODO: The framework currently does not test whether the input file exists in the database
		//[Test]
		//[ExpectedException(typeof(ArgumentException))]
		//public void TestReaderGetDataBlocksFileNameUnscanned()
		//{
		//    IDataBlock[] dataBlocks = _project.GetDataBlocks(new DummyInputFile(_testInvalidPluginFile));
		//}

		[Test]
		public void TestReaderGetResultsDataBlockVerifyCount()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_project.GetInputFiles()[0]);
			IResultNode results = TestFramework.GetResults(dataBlocks[0]);
			Assert.AreEqual(1, results.Children.Count, "Data block should contain a single top-level result.");
			results = TestFramework.GetResults(dataBlocks[1]);
			Assert.AreEqual(1, results.Children.Count, "Data block should contain a single top-level result.");
		}

		[Test]
		public void TestReaderGetResultsDataBlockVerifyContents()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_project.GetInputFiles()[0]);

			IResultNode results = TestFramework.GetResults(dataBlocks[0]);
			Assert.AreEqual(0, results.Children[0].StartOffset, "Result.Offset");
			Assert.AreEqual(1, results.Children[0].Length, "Result.Length");
			Assert.AreEqual("correct type 1", results.Children[0].Name, "Result.Name");
			Assert.AreEqual(true, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "Result.HasChildren");
			Assert.AreEqual(2, results.Children[0].Attributes.Count, "Result.Attributes.Count");
			Assert.AreEqual("type1 first", results.Children[0].Attributes[0].Value, "Result.Attributes[0]");

			results = TestFramework.GetResults(dataBlocks[1]);
			Assert.AreEqual(517, results.Children[0].StartOffset, "Result.Offset");
			Assert.AreEqual(1, results.Children[0].Length, "Result.Length");
			Assert.AreEqual("correct type 1", results.Children[0].Name, "Result.Name");
			Assert.AreEqual(false, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "Result.HasChildren");
			Assert.AreEqual(2, results.Children[0].Attributes.Count, "Result.Attributes.Count");
			Assert.AreEqual("type1 unique second", results.Children[0].Attributes[1].Value, "Result.Attributes[0]");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestReaderGetResultsDataBlockInvalid()
		{
			IResultNode results = TestFramework.GetResults((IDataBlock)null);
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void TestReaderGetResultsDataBlockUnknown()
		{
			IDataPacket data = TestFramework.CreateDataPacket(TestFramework.CreateInputFile("ja"), 31337, 1);
			IDataBlock dataBlock = TestFramework.CreateDataBlock(TestFramework.DetectorFactory.Detectors[0], data, false, null);
			IResultNode results = TestFramework.GetResults(dataBlock);	// TODO: _project??
		}

		[Test]
		public void TestReaderGetResultsResultVerifyCount()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_project.GetInputFiles()[0]);
			IResultNode results = TestFramework.GetResults(dataBlocks[0]);

			results = results.Children[0];
			Assert.AreEqual(1, results.Children.Count, "First result should have a single child result.");

			results = results.Children[0];
			Assert.AreEqual(2, results.Children.Count, "Second result should have two child results.");

			Assert.AreEqual(0, results.Children[0].Children.Count, "Third result should have no child results.");
			Assert.AreEqual(0, results.Children[1].Children.Count, "Fourth result should have no child results.");
		}

		[Test]
		public void TestReaderGetResultsResultVerifyContents()
		{
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_project.GetInputFiles()[0]);
			IResultNode results = TestFramework.GetResults(dataBlocks[0]);

			results = results.Children[0];
			Assert.AreEqual(255, results.Children[0].StartOffset, "Result.Offset");
			Assert.AreEqual(2, results.Children[0].Length, "Result.Length");
			Assert.AreEqual("correct type 2", results.Children[0].Name, "Result.Name");
			Assert.AreEqual(true, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "Result.HasChildren");
			Assert.AreEqual(4, results.Children[0].Attributes.Count, "Result.Attributes.Count");
			Assert.AreEqual("type2 first", results.Children[0].Attributes[0].Value, "Result.Attributes[0]");

			results = results.Children[0];
			Assert.AreEqual(260, results.Children[0].StartOffset, "Result.Offset");
			Assert.AreEqual(3, results.Children[0].Length, "Result.Length");
			Assert.AreEqual("correct type 1", results.Children[0].Name, "Result.Name");
			Assert.AreEqual(false, results.Children[0].Children != null && results.Children[0].Children.Count > 0, "Result.HasChildren");
			Assert.AreEqual(2, results.Children[0].Attributes.Count, "Result.Attributes.Count");
			Assert.AreEqual("type1 first", results.Children[0].Attributes[0].Value, "Result.Attributes[0]");

			Assert.AreEqual(264, results.Children[1].StartOffset, "Result.Offset");
			Assert.AreEqual(4, results.Children[1].Length, "Result.Length");
			Assert.AreEqual("correct type 2", results.Children[1].Name, "Result.Name");
			Assert.AreEqual(false, results.Children[1].Children != null && results.Children[1].Children.Count > 0, "Result.HasChildren");
			Assert.AreEqual(4, results.Children[1].Attributes.Count, "Result.Attributes.Count");
			Assert.AreEqual("type2 unique second", results.Children[1].Attributes[1].Value, "Result.Attributes[1]");
		}
	}
}
