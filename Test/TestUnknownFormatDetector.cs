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

using System.Collections;
using System.Linq;
using Defraser.DataStructures;
using Defraser.Detector.UnknownFormat;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestUnknownFormatDetector
	{
		/// <summary><c>Name</c> of the file used for testing.</summary>
		private const string DataFileName = "defrasersong.mp3";
		/// <summary><c>Length</c> of the file used for testing.</summary>
		private const long DataFileLength = 1398;

		private MockRepository _mockRepository;
		private IDetector _detector;
		private IInputFile _inputFile;
		private IProject _project;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile = _mockRepository.StrictMock<IInputFile>();
			_project = _mockRepository.DynamicMock<IProject>();
			_detector = new UnknownFormatDetector();

			With.Mocks(_mockRepository).Expecting(delegate
			{
				SetupResult.For(_inputFile.Name).Return(DataFileName);
				SetupResult.For(_inputFile.Length).Return(DataFileLength);
			});
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_mockRepository = null;
			_inputFile = null;
			_detector = null;
		}

		[Test]
		public void TestConstructorDefaultFormat()
		{
			using (IDataReader dataReader = new MockDataReader(new byte[DataFileLength], _inputFile))
			{
				IDetector detector = new UnknownFormatDetector();
				IScanContext scanContext = TestFramework.CreateScanContext(_project);
				scanContext.Detectors = new[] { detector };
				IDataBlockBuilder builder = TestFramework.CreateDataBlockBuilder();
				builder.Detectors = scanContext.Detectors;
				builder.InputFile = _inputFile;
				IDataBlock dataBlock = detector.DetectData(dataReader, builder, scanContext);
				Assert.IsNotNull(dataBlock);
				IResultNode results = scanContext.Results;
				Assert.IsNotNull(results);
				Assert.GreaterOrEqual(results.Children.Count, 1);
				Assert.AreEqual(0, results.Children[0].Attributes.Count);
			}
		}

		[Test]
		public void TestName()
		{
			Assert.AreEqual("Unknown", _detector.Name, "Name");
		}

		[Test]
		public void TestDescription()
		{
			Assert.AreEqual("Unknown format or codec", _detector.Description, "Description");
		}

		[Test]
		public void TestOutputFileExtension()
		{
			Assert.AreEqual(".bin", _detector.OutputFileExtension, "Output file extension");
		}

		[Test]
		public void TestColumns()
		{
			Assert.IsEmpty(_detector.Columns as ICollection, "Columns (none)");
		}

		[Test]
		public void TestDetectData()
		{
			using (IDataReader dataReader = new MockDataReader(new byte[DataFileLength], _inputFile))
			{
				IScanContext scanContext = TestFramework.CreateScanContext(_project);
				scanContext.Detectors = new[] { _detector };
				IDataBlockBuilder builder = TestFramework.CreateDataBlockBuilder();
				builder.Detectors = scanContext.Detectors;
				builder.InputFile = _inputFile;
				IDataBlock dataBlock = _detector.DetectData(dataReader, builder, scanContext);
				Assert.IsNotNull(dataBlock, "Unknown format detector (DetectData not null)");
				IResultNode results = scanContext.Results;
				Assert.IsNotNull(results, "Unknown format detector (Results not null)");
				Assert.AreEqual(_detector, dataBlock.Detectors.First(), "Detector of data block");
				Assert.AreEqual(1, results.Children.Count, "One child 'Data'");
				IResultNode data = results.Children[0];
				Assert.IsEmpty((ICollection)data.Children, "Data has no children");
				Assert.AreEqual(0, data.StartOffset, "");
				Assert.AreEqual(DataFileLength, data.Length, "");
				Assert.IsFalse(data.IsFragmented(), "Single fragment");
				Assert.AreEqual(_inputFile, data.InputFile, "Input file");
				Assert.IsFalse(dataBlock.IsFullFile, "Does not produce full files");
			}
		}
	}
}
