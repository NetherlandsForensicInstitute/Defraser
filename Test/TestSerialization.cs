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
using Defraser.Detector.Mpeg2.Video;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestSerialization
	{
		#region Inner classes
		private sealed class ProjectChangedEventTest
		{
			private ProjectChangedEventArgs _eventArgs;

			#region Properties
			public ProjectChangedEventArgs EventArgs { get { return _eventArgs; } }
			#endregion Properties

			public void Clear()
			{
				_eventArgs = null;
			}

			public void ProjectChanged(object sender, ProjectChangedEventArgs e)
			{
				if (_eventArgs != null)
				{
					throw new InvalidOperationException("Unexpected ProjectChanged event.");
				}

				_eventArgs = e;
			}
		}
		#endregion Inner classes

		#region Test data
		/// <summary>Project path.</summary>
		private const string ProjectPath = "test.dpr";
		/// <summary>Project investigator.</summary>
		private const string ProjectInvestigator = "Paulus Pietsma";
		/// <summary>Project creation date.</summary>
		private static readonly string ProjectCreationDate = DateTime.Now.AddSeconds(-2).ToString("yyyy/MM/dd HH:mm:ss");
		/// <summary>Project modification date.</summary>
		private static readonly string ProjectModificationDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
		/// <summary>Project description.</summary>
		private const string ProjectDescription = "<description that will be overwritten>";
		/// <summary>The <c>Name</c> of the first test file.</summary>
		private const string FileName1 = "TestSerialization__dd200906151710_3830.bin";
		/// <summary>The <c>Name</c> of the first test file.</summary>
		private const long FileLength1 = 1690289L;
		/// <summary>The <c>Name</c> of the second test file.</summary>
		private const string FileName2 = "TestSerialization__dummy.mpg";
		/// <summary>The <c>Name</c> of the second test file.</summary>
		private const long FileLength2 = 9182L;
		/// <summary>The <c>Name</c> of an existing file.</summary>
		private const string ExistingFileName = "empty.txt";
		/// <summary>The current <c>FileVersion</c> of project.</summary>
		private const string ProjectFileVersion = "1.0.0.4";
		/// <summary>The <c>Name</c> of the first (visible) column.</summary>
		private const string ColumnName1 = "Name";
		/// <summary>The <c>Width</c> of the first (visible) column.</summary>
		private const int ColumnWidth1 = 100;
		/// <summary>The <c>Name</c> of the second (visible) column.</summary>
		private const string ColumnName2 = "Length";
		/// <summary>The <c>Width</c> of the second (visible) column.</summary>
		private const int ColumnWidth2 = 87;
		/// <summary>The <c>Width</c> for updating a (visible) column.</summary>
		private const int UpdatedColumnWidth = 3000;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile1;
		private IInputFile _inputFile2;
		private IDetector _detector1;
		private IDetector _detector2;
		private IDataBlock _dataBlock1;
		private IDataBlock _dataBlock2;
		private IDataBlock _dataBlock3;
		private IColumnInfo _columnInfo1;
		private IColumnInfo _columnInfo2;
		private IColumnInfo _updatedColumnInfo;
		private Creator<IInputFile, IProject, string> _createInputFile;
		#endregion Mocks and stubs

		#region Stuff ...
		private const string DummyFileName = "dummy.txt";
		private const long DummyFileLength = 128;
		private const string EmptyFileName = "empty.txt";
		private const long EmptyFileLength = 4096;

		private Dictionary<ProjectMetadataKey, string> _fullMetadata;
		#endregion Stuff ...

		#region Objects under test
		private IProject _project;
		#endregion Objects under test

		#region Properties
		private IEnumerable<IDetector> OneDetector { get { return Enumerable.Repeat<IDetector>(_detector1, 1); } }
		private IEnumerable<IDetector> TwoDetectors { get { return new List<IDetector> { _detector1, _detector2 }; } }
		private IList<IColumnInfo> OneColumn { get { return Enumerable.Repeat<IColumnInfo>(_columnInfo1, 1).ToList(); } }
		private IList<IColumnInfo> TwoColumns { get { return new[] { _columnInfo1, _columnInfo2 }; } }
		#endregion Properties

		#region Test initialization and cleanup
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			TestFramework.DetectorFactory.Initialize(".");

			File.WriteAllBytes(FileName1, Enumerable.Repeat((byte)'*', (int)FileLength1).ToArray());
			File.WriteAllBytes(FileName2, Enumerable.Repeat((byte)'*', (int)FileLength2).ToArray());

			File.Delete(DummyFileName);
			File.Delete(EmptyFileName);

			using (StreamWriter streamWriter = File.CreateText(DummyFileName))
			{
				streamWriter.WriteLine("dummy text file");
			}

			File.CreateText(EmptyFileName).Close();

			_fullMetadata = new Dictionary<ProjectMetadataKey, string>();
			_fullMetadata[ProjectMetadataKey.ProjectDescription] = ProjectDescription;
			_fullMetadata[ProjectMetadataKey.InvestigatorName] = ProjectInvestigator;
			_fullMetadata[ProjectMetadataKey.DateCreated] = ProjectCreationDate;
			_fullMetadata[ProjectMetadataKey.DateLastModified] = ProjectModificationDate;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			File.Delete(FileName1);
			File.Delete(FileName2);
			File.Delete(DummyFileName);
			File.Delete(EmptyFileName);

			_fullMetadata = null;
		}

		[SetUp]
		public void SetUp()
		{
			File.Delete(ProjectPath);

			_mockRepository = new MockRepository();
			_inputFile1 = MockRepository.GenerateStub<IInputFile>();
			_inputFile2 = MockRepository.GenerateStub<IInputFile>();
			_detector1 = MockRepository.GenerateStub<IDetector>();
			_detector2 = MockRepository.GenerateStub<IDetector>();
			_dataBlock1 = MockRepository.GenerateStub<IDataBlock>();
			_dataBlock2 = MockRepository.GenerateStub<IDataBlock>();
			_dataBlock3 = MockRepository.GenerateStub<IDataBlock>();
			_columnInfo1 = MockRepository.GenerateStub<IColumnInfo>();
			_columnInfo2 = MockRepository.GenerateStub<IColumnInfo>();
			_updatedColumnInfo = MockRepository.GenerateStub<IColumnInfo>();
			_createInputFile = MockRepository.GenerateStub<Creator<IInputFile, IProject, string>>();

			_inputFile1.Stub(x => x.Name).Return(DummyFileName);
			_inputFile1.Stub(x => x.Length).Return(DummyFileLength);
			_inputFile2.Stub(x => x.Name).Return(EmptyFileName);
			_inputFile2.Stub(x => x.Length).Return(EmptyFileLength);
			_dataBlock1.Stub(x => x.InputFile).Return(_inputFile1);
			_dataBlock2.Stub(x => x.InputFile).Return(_inputFile2);
			_dataBlock3.Stub(x => x.InputFile).Return(_inputFile2);
			_columnInfo1.Stub(x => x.Name).Return(ColumnName1);
			_columnInfo1.Stub(x => x.Width).Return(ColumnWidth1);
			_columnInfo2.Stub(x => x.Name).Return(ColumnName2);
			_columnInfo2.Stub(x => x.Width).Return(ColumnWidth2);

			_project = new Project(_createInputFile, ProjectPath);

			_createInputFile.Stub(x => x(_project, FileName1)).Return(_inputFile1);
			_createInputFile.Stub(x => x(_project, FileName2)).Return(_inputFile2);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile1 = null;
			_inputFile2 = null;
			_detector1 = null;
			_detector2 = null;
			_dataBlock1 = null;
			_dataBlock2 = null;
			_columnInfo1 = null;
			_columnInfo2 = null;
			_updatedColumnInfo = null;
			_createInputFile = null;

			if (_project != null)
			{
				_project.Dispose();
				_project = null;
			}

			File.Delete(ProjectPath);
		}
		#endregion Test initialization and cleanup

		#region Tests for Project serialization
		[Test]
		[Ignore]
		[ExpectedException(typeof(InvalidProjectException))]
		public void TestDeserializationInvalidFileVersion()
		{
		}

		[Test]
		public void SerializeProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, DateTime.Now, ProjectDescription);

			IDetector systemDetector = TestFramework.DetectorFactory.ContainerDetectors.Single(x => x.Name == "MPEG-1/2 Systems");
			IDetector videoDetector = TestFramework.DetectorFactory.CodecDetectors.Single(x => x.Name == Mpeg2VideoDetector.DetectorName);
			IInputFile inputFile1 = project.AddFile(FileName1, new[] { systemDetector, videoDetector });
			IInputFile inputFile2 = project.AddFile(FileName2, new[] { videoDetector });

			IDataBlockBuilder dataBlockBuilder1 = TestFramework.CreateDataBlockBuilder();
			dataBlockBuilder1.DataFormat = CodecID.Mpeg2System;
			dataBlockBuilder1.Detectors = new[] { systemDetector };
			dataBlockBuilder1.InputFile = inputFile1;
			dataBlockBuilder1.StartOffset = 0L;
			dataBlockBuilder1.EndOffset = 20480L;
			dataBlockBuilder1.IsFullFile = true;
			ICodecStreamBuilder codecStreamBuilder = dataBlockBuilder1.AddCodecStream();
			codecStreamBuilder.Name = "Video Stream #1";
			codecStreamBuilder.DataFormat = CodecID.Mpeg2Video;
			codecStreamBuilder.Detector = videoDetector;
			codecStreamBuilder.Data = TestFramework.CreateDataPacket(inputFile1, 0L, 3033L);
			IDataBlock dataBlock1 = dataBlockBuilder1.Build();

			IDataBlockBuilder dataBlockBuilder2 = TestFramework.CreateDataBlockBuilder();
			dataBlockBuilder2.DataFormat = CodecID.Mpeg2Video;
			dataBlockBuilder2.Detectors = new[] { videoDetector };
			dataBlockBuilder2.InputFile = inputFile1;
			dataBlockBuilder2.StartOffset = 130303L;
			dataBlockBuilder2.EndOffset = 130327L;
			dataBlockBuilder2.IsFullFile = false;
			IDataBlock dataBlock2 = dataBlockBuilder2.Build();

			IDataBlockBuilder dataBlockBuilder3 = TestFramework.CreateDataBlockBuilder();
			dataBlockBuilder3.DataFormat = CodecID.Mpeg2Video;
			dataBlockBuilder3.Detectors = new[] { videoDetector };
			dataBlockBuilder3.InputFile = inputFile2;
			dataBlockBuilder3.StartOffset = 0L;
			dataBlockBuilder3.EndOffset = FileLength2;
			dataBlockBuilder3.IsFullFile = true;
			IDataBlock dataBlock3 = dataBlockBuilder3.Build();

			project.AddDataBlock(dataBlock1);
			project.AddDataBlock(dataBlock2);
			project.AddDataBlock(dataBlock3);
			project.SetMetadata(_fullMetadata);
//			project.SetVisibleColumns(systemDetector, TwoColumns);

			IProject deserializedProject;
			using (FileStream fileStream = new FileStream(_project.FileName, FileMode.Create))
			{
				TestFramework.CreateXmlObjectSerializer().WriteObject(fileStream, project);
			}
			using (FileStream fileStream = new FileStream(ProjectPath, FileMode.Open))
			{
				deserializedProject = TestFramework.CreateXmlObjectSerializer().ReadObject(fileStream) as IProject;
			}

			IList<IInputFile> inputFiles = deserializedProject.GetInputFiles();
			Assert.AreEqual(2, inputFiles.Count, "Serialization, input files (2 files)");
			AreEqual(inputFile1, inputFiles[0], "1");
			AreEqual(inputFile2, inputFiles[1], "2");

			IList<IDataBlock> dataBlocks = deserializedProject.GetDataBlocks(inputFiles[0]);
			Assert.AreEqual(2, dataBlocks.Count, "Serialization, data blocks (file 1, 2 blocks)");
			AreEqual(dataBlock1, dataBlocks[0], "1", "1");
			AreEqual(dataBlock2, dataBlocks[1], "1", "2");
			dataBlocks = deserializedProject.GetDataBlocks(inputFiles[1]);
			Assert.AreEqual(1, dataBlocks.Count, "Serialization, data blocks (file 2, 1 block)");
			AreEqual(dataBlock3, dataBlocks[0], "2", "1");

			IDictionary<ProjectMetadataKey, string> metadata = deserializedProject.GetMetadata();
			Assert.AreEqual(5, metadata.Count, "Serialization, metadata (5 entries)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.FileVersion), "Serialization, metadata (FileVersion)");
			Assert.AreEqual("1.0.0.4", metadata[ProjectMetadataKey.FileVersion], "Serialization, metadata (FileVersion)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.ProjectDescription), "Serialization, metadata (ProjectDescription)");
			Assert.AreEqual(ProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "Serialization, metadata (ProjectDescription)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.InvestigatorName), "Serialization, metadata (InvestigatorName)");
			Assert.AreEqual(ProjectInvestigator, metadata[ProjectMetadataKey.InvestigatorName], "Serialization, metadata (InvestigatorName)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateCreated), "Serialization, metadata (DateCreated)");
			Assert.AreEqual(ProjectCreationDate, metadata[ProjectMetadataKey.DateCreated], "Serialization, metadata (DateCreated)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateLastModified), "Serialization, metadata (DateLastModified)");
			Assert.AreEqual(ProjectModificationDate, metadata[ProjectMetadataKey.DateLastModified], "Serialization, metadata (DateLastModified)");

			//IList<IColumnInfo> visibleColumns = deserializedProject.GetVisibleColumns(systemDetector);
			//Assert.AreEqual(2, visibleColumns.Count, "Serialization, visible columns (2 columns)");
			//Assert.IsTrue(visibleColumns.Contains(_columnInfo1), "Serialization, visible columns (column 1)");
			//Assert.IsTrue(visibleColumns.Contains(_columnInfo2), "Serialization, visible columns (column 2)");

			TestFramework.ProjectManager.CloseProject(project);
		}

		private void AreEqual(IInputFile inputFile1, IInputFile inputFile2, string fileNumber)
		{
			Assert.AreEqual(inputFile1.Name, inputFile2.Name, string.Format("Serialization, input file name (file {0})", fileNumber));
			Assert.AreEqual(inputFile1.Length, inputFile2.Length, string.Format("Serialization, input file length (file {0})", fileNumber));
		}

		private void AreEqual(IDataBlock dataBlock1, IDataBlock dataBlock2, string fileNumber, string dataBlockNumber)
		{
			Assert.AreEqual(dataBlock1.Length, dataBlock2.Length, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.EndOffset, dataBlock2.EndOffset, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.StartOffset, dataBlock2.StartOffset, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.InputFile.Name, dataBlock2.InputFile.Name, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.IsFragmented, dataBlock2.IsFragmented, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.IsFullFile, dataBlock2.IsFullFile, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.Detectors.First().Name, dataBlock2.Detectors.First().Name, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.DataFormat, dataBlock2.DataFormat, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
			Assert.AreEqual(dataBlock1.CodecStreams.Count, dataBlock2.CodecStreams.Count, string.Format("Serialization, data blocks (file {0}, data block {1})", fileNumber, dataBlockNumber));
		}

		[Test]
		[Ignore("broken")]
		public void SerializeProjectDetectorConfiguration()
		{
			// Open a project
			IProject project1 = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, DateTime.Now, ProjectDescription);

			// Change detector configuration
			IDetector mockConfigurableDetector = TestFramework.DetectorFactory.Detectors.Single(detector => detector.Name == "MockConfigurableDetector");
			IConfigurationItem configurationItem = (mockConfigurableDetector as IConfigurable).Configuration.ElementAt(0);
			Assert.That((int)configurationItem.Value, Is.EqualTo(123));
			configurationItem.SetUserValue("456");
			Assert.That((int)configurationItem.Value, Is.EqualTo(456));

			// Save the project containing the detector configuration
			TestFramework.ProjectManager.SaveProject(project1);
			TestFramework.ProjectManager.CloseProject(project1);

			// Change the detector configuration
			configurationItem.SetUserValue("123");
			// Check the change made
			Assert.That((int)configurationItem.Value, Is.EqualTo(123));

			// Open project
			IProject project2 = TestFramework.ProjectManager.OpenProject(ProjectPath);

			// Check the detector configuration
			Assert.That((int)configurationItem.Value, Is.EqualTo(456));
			TestFramework.ProjectManager.CloseProject(project2);
		}

		[Test]
		public void SerializeProjectMetadata()
		{
			_project.SetMetadata(_fullMetadata);

			IProject deserializedProject;
			using (FileStream fileStream = new FileStream(_project.FileName, FileMode.Create))
			{
				TestFramework.CreateXmlObjectSerializer().WriteObject(fileStream, _project);
			}
			using (FileStream fileStream = new FileStream(ProjectPath, FileMode.Open))
			{
				deserializedProject = TestFramework.CreateXmlObjectSerializer().ReadObject(fileStream) as IProject;
			}

			IDictionary<ProjectMetadataKey, string> metadata = deserializedProject.GetMetadata();
			Assert.AreEqual(5, metadata.Count, "Serialization, metadata (5 entries)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.FileVersion), "Serialization, metadata (FileVersion)");
			Assert.AreEqual("1.0.0.4", metadata[ProjectMetadataKey.FileVersion], "Serialization, metadata (FileVersion)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.ProjectDescription), "Serialization, metadata (ProjectDescription)");
			Assert.AreEqual(ProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "Serialization, metadata (ProjectDescription)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.InvestigatorName), "Serialization, metadata (InvestigatorName)");
			Assert.AreEqual(ProjectInvestigator, metadata[ProjectMetadataKey.InvestigatorName], "Serialization, metadata (InvestigatorName)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateCreated), "Serialization, metadata (DateCreated)");
			Assert.AreEqual(ProjectCreationDate, metadata[ProjectMetadataKey.DateCreated], "Serialization, metadata (DateCreated)");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateLastModified), "Serialization, metadata (DateLastModified)");
			Assert.AreEqual(ProjectModificationDate, metadata[ProjectMetadataKey.DateLastModified], "Serialization, metadata (DateLastModified)");

			// This will fail if the Creator delegate was not set!!
			deserializedProject.AddFile(ExistingFileName, OneDetector);
		}
		#endregion Tests for Project serialization
	}
}
