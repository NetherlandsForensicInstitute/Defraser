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
using System.ComponentModel;
using System.IO;
using System.Linq;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="Project"/> class.
	/// </summary>
	[TestFixture]
	public class TestProject
	{
		#region Test data
		/// <summary>The <c>Name</c> of an invalid project file.</summary>
		private const string InvalidProjectFileName = "dummy.txt";
		/// <summary>The <c>Name</c> of an empty project file.</summary>
		private const string EmptyProjectFileName = "empty.txt";
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
		/// <summary>Updated project description.</summary>
		private const string UpdatedProjectDescription = "<description>";
		/// <summary>The <c>Name</c> of the first test file.</summary>
		private const string FileName1 = "dd200906151710_3830.bin";
		/// <summary>The <c>Name</c> of the second test file.</summary>
		private const string FileName2 = "dummy.mpg";
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
		private IDictionary<ProjectMetadataKey, string> _fullMetadata;
		private IDictionary<ProjectMetadataKey, string> _emptyMetadata;
		private EventHandler<ProjectChangedEventArgs> _projectChangedEventHandler;
		private PropertyChangedEventHandler _propertyChangedEventHandler;
		private Creator<IInputFile, IProject, string> _createInputFile;
		#endregion Mocks and stubs

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

			File.Delete(InvalidProjectFileName);
			File.Delete(EmptyProjectFileName);

			using (StreamWriter streamWriter = File.CreateText(InvalidProjectFileName))
			{
				streamWriter.WriteLine("dummy text file");
			}

			File.CreateText(EmptyProjectFileName).Close();

			_fullMetadata = new Dictionary<ProjectMetadataKey, string>();
			_fullMetadata[ProjectMetadataKey.ProjectDescription] = UpdatedProjectDescription;
			_fullMetadata[ProjectMetadataKey.InvestigatorName] = ProjectInvestigator;
			_fullMetadata[ProjectMetadataKey.DateCreated] = ProjectCreationDate;
			_fullMetadata[ProjectMetadataKey.DateLastModified] = ProjectModificationDate;

			_emptyMetadata = new Dictionary<ProjectMetadataKey, string>();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			File.Delete(InvalidProjectFileName);
			File.Delete(EmptyProjectFileName);

			_fullMetadata = null;
			_emptyMetadata = null;
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
			_projectChangedEventHandler = _mockRepository.StrictMock<EventHandler<ProjectChangedEventArgs>>();
			_propertyChangedEventHandler = _mockRepository.StrictMock<PropertyChangedEventHandler>();
			_createInputFile = MockRepository.GenerateStub<Creator<IInputFile, IProject, string>>();

			_inputFile1.Stub(x => x.Name).Return(FileName1);
			_inputFile2.Stub(x => x.Name).Return(FileName2);
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
			_projectChangedEventHandler = null;
			_propertyChangedEventHandler = null;
			_createInputFile = null;

			if (_project != null)
			{
				_project.Dispose();
				_project = null;
			}

			File.Delete(ProjectPath);
		}
		#endregion Test initialization and cleanup

		#region Tests for ProjectMetadataKey enum
		[Test]
		public void ProjectMetadataKeyValues()
		{
			Assert.AreEqual(5, Enum.GetValues(typeof(ProjectMetadataKey)).Length, "ProjectMetadataKey, number of enumeration constants");
		}
		#endregion Tests for ProjectMetadataKey enum

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorInputFileCreatorNull()
		{
			using (new Project(null, ProjectPath))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorFileNameNull()
		{
			using (new Project(_createInputFile, null))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ConstructorFileNameEmpty()
		{
			using (new Project(_createInputFile, string.Empty))
			{
			}
		}

		[Test]
		public void ConstructorFileExists()
		{
			using (new Project(_createInputFile, EmptyProjectFileName))
			{
				Assert.IsTrue(true);
			}
		}
		#endregion Tests for constructor arguments

		#region Tests for ProjectChanged event
		[Test]
		public void ProjectChangedSetMetadata()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.MetadataChanged);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.SetMetadata(_fullMetadata);
			});
		}

		[Test]
		public void ProjectChangedSetVisibleColumns()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.VisibleColumnsChanged, _detector1);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.AddFile(FileName1, OneDetector);
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.SetVisibleColumns(_detector1, TwoColumns);
			});
		}

		[Test]
		public void ProjectChangedSetFileName()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.FileNameChanged, EmptyProjectFileName);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.FileName = EmptyProjectFileName;
			});
		}

		[Test]
		public void ProjectChangedAddFile()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.FileAdded, _inputFile1);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.AddFile(FileName1, OneDetector);
			});
		}

		[Test]
		public void ProjectChangedDeleteFile()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.FileDeleted, _inputFile1);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				IInputFile inputFile = _project.AddFile(FileName1, OneDetector);
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.DeleteFile(inputFile);
			});
		}

		[Test]
		public void ProjectChangedAddDataBlock()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.DataBlockAdded, _dataBlock1);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.AddFile(FileName1, OneDetector);
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.AddDataBlock(_dataBlock1);
			});
		}

		[Test]
		public void ProjectChangedDeleteDataBlock()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				var e = new ProjectChangedEventArgs(ProjectChangedType.DataBlockDeleted, _dataBlock1);
				_projectChangedEventHandler.Expect(x => x(_project, e));
			}).Verify(delegate
			{
				_project.AddFile(FileName1, OneDetector);
				_project.AddDataBlock(_dataBlock1);
				_project.ProjectChanged += _projectChangedEventHandler;
				_project.DeleteDataBlock(_dataBlock1);
			});
		}
		#endregion Tests for ProjectChanged event

		#region Tests for PropertyChanged event
		[Test]
		public void PropertyChanged()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_propertyChangedEventHandler.Expect(x => x(
						Arg<IProject>.Is.Same(_project),
						Arg<PropertyChangedEventArgs>.Matches(y => y.PropertyName == Project.DirtyProperty)));
			}).Verify(delegate
			{
				_project.Dirty = false;
				_project.PropertyChanged += _propertyChangedEventHandler;
				_project.Dirty = true;
			});
		}
		#endregion Tests for PropertyChanged event

		#region Tests for FileName property
		[Test]
		public void FileNameInitial()
		{
			Assert.AreEqual(ProjectPath, _project.FileName);
		}

		[Test]
		public void FileNameSet()
		{
			_project.FileName = EmptyProjectFileName;
			Assert.AreEqual(EmptyProjectFileName, _project.FileName);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FileNameSetToNull()
		{
			_project.FileName = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void FileNameSetToEmptyString()
		{
			_project.FileName = string.Empty;
		}
		#endregion Tests for FileName property

		#region Tests for Dirty property
		[Test]
		public void NewProjectNotDirty()
		{
			Assert.IsFalse(_project.Dirty);
		}

		[Test]
		public void DirtySetToTrue()
		{
			_project.Dirty = true;
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void DirtySetToFalse()
		{
			_project.Dirty = true;
			_project.Dirty = false;
			Assert.IsFalse(_project.Dirty);
		}

		[Test]
		public void DirtyAfterSetMetadata()
		{
			_project.SetMetadata(_fullMetadata);
			Assert.IsTrue(_project.Dirty, "Dirty, upon SetMetadata() (true)");
		}

		[Test]
		public void DirtyAfterAddFile()
		{
			_project.AddFile(FileName1, OneDetector);
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void DirtyAfterSetVisibleColumns()
		{
			_project.SetVisibleColumns(_detector1, TwoColumns);
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void DirtyAfterUpdateColumnWidth()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, OneColumn);
			_columnInfo1.Stub(x => x.UpdateWidth(UpdatedColumnWidth)).Return(_columnInfo2);
			_project.Dirty = false;
			_project.UpdateColumnWidth(_detector1, ColumnName1, UpdatedColumnWidth);
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void NotDirtyAfterUpdateColumnWidthNoChange()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, OneColumn);
			_columnInfo1.Stub(x => x.UpdateWidth(ColumnWidth1)).Return(_columnInfo1);
			_project.Dirty = false;
			_project.UpdateColumnWidth(_detector1, ColumnName1, ColumnWidth1);
			Assert.IsFalse(_project.Dirty);
		}

		[Test]
		public void DirtyAfterAddDataBlock()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.Dirty = false;
			_project.AddDataBlock(_dataBlock1);
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void DirtyAfterDeleteDataBlock()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			_project.Dirty = false;
			_project.DeleteDataBlock(_dataBlock1);
			Assert.IsTrue(_project.Dirty);
		}

		[Test]
		public void DirtyAfterDeleteFile()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.Dirty = false;
			_project.DeleteFile(_inputFile1);
			Assert.IsTrue(_project.Dirty);
		}
		#endregion Tests for Dirty property

		#region Tests for GetInputFiles() method
		[Test]
		public void TestGetInputFiles()
		{
			Assert.IsEmpty(_project.GetInputFiles().ToArray(), "New projects contain no input files");
		}
		#endregion Tests for GetInputFiles() method

		#region Tests for GetMetadata() method
		[Test]
		public void GetMetadata()
		{
			IDictionary<ProjectMetadataKey, string> metadata = _project.GetMetadata();
			Assert.IsTrue(metadata.SequenceEqual(new Dictionary<ProjectMetadataKey, string> { { ProjectMetadataKey.FileVersion, ProjectFileVersion } }));
		}
		#endregion Tests for GetMetadata() method

		#region Tests for SetMetadata() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetMetadataNull()
		{
			_project.SetMetadata(null);
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void SetMetadataInvalidKey()
		{
			_project.SetMetadata(new Dictionary<ProjectMetadataKey, string> { { (ProjectMetadataKey)2008, "Value" } });
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SetMetadataNullValue()
		{
			_project.SetMetadata(new Dictionary<ProjectMetadataKey, string> { { ProjectMetadataKey.ProjectDescription, null } });
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SetMetadateWrongFileVersion()
		{
			_project.SetMetadata(new Dictionary<ProjectMetadataKey, string> { { ProjectMetadataKey.FileVersion, "2.0" } });
		}

		[Test(Description = "SetMetadata() with correct FileVersion should not throw an exception")]
		public void SetMetadateCorrectFileVersion()
		{
			_project.SetMetadata(new Dictionary<ProjectMetadataKey, string> { { ProjectMetadataKey.FileVersion, ProjectFileVersion } });
		}

		[Test]
		public void SetMetadata()
		{
			_project.SetMetadata(_fullMetadata);

			IDictionary<ProjectMetadataKey, string> metadata = _project.GetMetadata();
			Assert.AreEqual(5, metadata.Count, "SetMetadata(), all");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.FileVersion), "SetMetadata(), all, FileVersion");
			Assert.AreEqual(ProjectFileVersion, metadata[ProjectMetadataKey.FileVersion], "SetMetadata(), all, FileVersion");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.ProjectDescription), "SetMetadata(), all, UpdatedProjectDescription");
			Assert.AreEqual(UpdatedProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "SetMetadata(), all, UpdatedProjectDescription");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.InvestigatorName), "SetMetadata(), all, InvestigatorName");
			Assert.AreEqual(ProjectInvestigator, metadata[ProjectMetadataKey.InvestigatorName], "SetMetadata(), all, InvestigatorName");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateCreated), "SetMetadata(), all, DateCreated");
			Assert.AreEqual(ProjectCreationDate, metadata[ProjectMetadataKey.DateCreated], "SetMetadata(), all, DateCreated");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateLastModified), "SetMetadata(), all, DateLastModified");
			Assert.AreEqual(ProjectModificationDate, metadata[ProjectMetadataKey.DateLastModified], "SetMetadata(), all, DateLastModified");
		}

		[Test]
		public void SetMetadataOverwrite()
		{
			IDictionary<ProjectMetadataKey, string> otherMetadata = new Dictionary<ProjectMetadataKey, string>();
			otherMetadata[ProjectMetadataKey.ProjectDescription] = ProjectDescription;

			_project.SetMetadata(_fullMetadata);
			_project.SetMetadata(otherMetadata);

			IDictionary<ProjectMetadataKey, string> metadata = _project.GetMetadata();
			Assert.AreEqual(2, metadata.Count, "SetMetadata(), 1 entry");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.FileVersion), "SetMetadata(), 1 entry, FileVersion");
			Assert.AreEqual(ProjectFileVersion, metadata[ProjectMetadataKey.FileVersion], "SetMetadata(), 1 entry, FileVersion");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.ProjectDescription), "SetMetadata(), all, UpdatedProjectDescription");
			Assert.AreEqual(ProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "SetMetadata(), all, UpdatedProjectDescription");
		}

		[Test]
		public void SetMetadataClear()
		{
			_project.SetMetadata(_fullMetadata);
			_project.SetMetadata(_emptyMetadata);

			IDictionary<ProjectMetadataKey, string> metadata = _project.GetMetadata();
			Assert.AreEqual(1, metadata.Count, "SetMetadata(), clear");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.FileVersion), "SetMetadata(), clear, FileVersion");
			Assert.AreEqual(ProjectFileVersion, metadata[ProjectMetadataKey.FileVersion], "SetMetadata(), clear, FileVersion");
		}
		#endregion Tests for SetMetadata() method

		#region Tests for GetDetectors() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestGetDetectorsNull()
		{
			_project.GetDetectors(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetDetectorsNonExistingFile()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.GetDetectors(_inputFile2);
		}
		#endregion Tests for GetDetectors() method

		#region Tests for GetDataBlocks() method
		[Test]
		public void TestGetDataBlocksEmpty()
		{
			_project.AddFile(FileName1, OneDetector);
			Assert.AreEqual(0, _project.GetDataBlocks(_inputFile1).Count);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestGetDataBlocksNull()
		{
			_project.GetDataBlocks(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetDataBlocksNonExistingFile()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.GetDataBlocks(_inputFile2);
		}
		#endregion Tests for GetDataBlocks() method

		#region Tests for AddFile() method
		[Test]
		public void AddFile()
		{
			IInputFile inputFile = _project.AddFile(FileName1, OneDetector);
			Assert.AreSame(_inputFile1, inputFile);
		}

		[Test]
		public void AddFileOneDetector()
		{
			_project.AddFile(FileName1, OneDetector);
			IList<IDetector> detectors = _project.GetDetectors(_inputFile1);
			Assert.IsTrue(detectors.SequenceEqual(OneDetector));
		}

		[Test]
		public void AddFileTwoDetectors()
		{
			_project.AddFile(FileName1, TwoDetectors);
			IList<IDetector> detectors = _project.GetDetectors(_inputFile1);
			Assert.IsTrue(detectors.SequenceEqual(TwoDetectors));
		}

		[Test]
		public void AddFileGetInputFiles()
		{
			_project.AddFile(FileName1, OneDetector);
			IList<IInputFile> inputFiles = _project.GetInputFiles();
			Assert.IsTrue(inputFiles.SequenceEqual(Enumerable.Repeat(_inputFile1, 1)));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddFileInputFileNull()
		{
			_project.AddFile(null, OneDetector);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddFileDetectorsNull()
		{
			_project.AddFile(FileName1, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddFileNoDetectors()
		{
			_project.AddFile(FileName1, Enumerable.Empty<IDetector>());
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddFileNullDetector()
		{
			_project.AddFile(FileName1, Enumerable.Repeat<IDetector>(null, 1));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddFileDuplicateDetector()
		{
			_project.AddFile(FileName1, new[] { _detector1, _detector1 });
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddFileExists()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddFile(FileName1, OneDetector);
		}
		#endregion Tests for AddFile() method

		#region Tests for DeleteFile()
		[Test]
		public void DeleteFile()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.DeleteFile(_inputFile1);
			Assert.AreEqual(0, _project.GetInputFiles().Count, "No files after DeleteFile()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DeleteFileNull()
		{
			_project.DeleteFile(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DeleteFileNonExisting()
		{
			_project.DeleteFile(_inputFile1);
		}
		#endregion Tests for DeleteFile()

		#region Tests for AddDataBlock() method
		[Test]
		public void AddDataBlock()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			IList<IDataBlock> dataBlocks = _project.GetDataBlocks(_inputFile1);
			Assert.IsTrue(dataBlocks.SequenceEqual(Enumerable.Repeat(_dataBlock1, 1)));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddDataBlockNull()
		{
			_project.AddDataBlock(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddDataBlockNonExistingFile()
		{
			_project.AddDataBlock(_dataBlock1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddDataBlockDuplicate()
		{
			_createInputFile.Stub(x => x(_project, FileName1)).Return(_inputFile1);
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			_project.AddDataBlock(_dataBlock1);
		}
		#endregion Tests for AddDataBlock() method

		#region Tests for DeleteDataBlock() method
		[Test]
		public void DeleteDataBlock()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			_project.DeleteDataBlock(_dataBlock1);
			Assert.AreEqual(0, _project.GetDataBlocks(_inputFile1).Count, "No blocks after DeleteDataBlock()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DeleteDataBlockNull()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			_project.DeleteDataBlock(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DeleteDataBlockNonExistingFile()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddDataBlock(_dataBlock1);
			_project.DeleteDataBlock(_dataBlock2);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DeleteDataBlockNonExisting()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.DeleteDataBlock(_dataBlock1);
		}
		#endregion Tests for DeleteDataBlock() method

		#region Tests for GetVisibleColumns() method
		[Test]
		public void GetVisibleColumnsNotSet()
		{
			_project.AddFile(FileName1, OneDetector);
			Assert.IsEmpty(_project.GetVisibleColumns(_detector1).ToArray(), "Empty if no visible columns are set");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetVisibleColumnsNull()
		{
			_project.GetVisibleColumns(null);
		}
		#endregion Tests for GetVisibleColumns() method

		#region Tests for SetVisibleColumns() method
		[Test]
		public void SetVisibleColumns()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, TwoColumns);
			IList<IColumnInfo> visibleColumns = _project.GetVisibleColumns(_detector1);
			Assert.AreEqual(2, visibleColumns.Count, "2 visible columns");
			Assert.IsTrue(visibleColumns.Contains(_columnInfo1), "Contains column #1");
			Assert.IsTrue(visibleColumns.Contains(_columnInfo2), "Contains column #2");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetVisibleColumnsDetectorNull()
		{
			_project.SetVisibleColumns(null, Enumerable.Empty<IColumnInfo>().ToList());
		}

		[Test]
		[Ignore]
		[ExpectedException(typeof(ArgumentException))]
		public void SetVisibleColumnsInvalidDetector()
		{
			_project.SetVisibleColumns(_detector1, Enumerable.Empty<IColumnInfo>().ToList());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetVisibleColumnsVisibleColumnsNull()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, null);
		}
		#endregion Tests for SetVisibleColumns() method

		#region Tests for UpdateColumnWidth() method
		[Test]
		public void UpdateColumnWidth()
		{
			_columnInfo1.Stub(x => x.UpdateWidth(UpdatedColumnWidth)).Return(_updatedColumnInfo);
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, TwoColumns);
			_project.UpdateColumnWidth(_detector1, ColumnName1, UpdatedColumnWidth);
			IList<IColumnInfo> visibleColumns = _project.GetVisibleColumns(_detector1);
			Assert.AreEqual(2, visibleColumns.Count, "UpdateColumnWidth(), 2 columns");
			Assert.IsTrue(visibleColumns.Contains(_updatedColumnInfo), "SetVisibleColumns(), column 1 (column width updated)");
			Assert.IsTrue(visibleColumns.Contains(_columnInfo2), "UpdateColumnWidth(), column 2 (unmodified)");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void UpdateColumnWidthDetectorNull()
		{
			_project.UpdateColumnWidth(null, ColumnName1, ColumnWidth1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void UpdateColumnWidthColumnNameNull()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, TwoColumns);
			_project.UpdateColumnWidth(_detector1, null, ColumnWidth1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void UpdateColumnWidthNegativeWidth()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, TwoColumns);
			_project.UpdateColumnWidth(_detector1, ColumnName1, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void UpdateColumnWidthNonExistingColumn()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.SetVisibleColumns(_detector1, TwoColumns);
			_project.UpdateColumnWidth(_detector1, "BogusColumn", ColumnWidth1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void UpdateColumnWidthNotSet()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.UpdateColumnWidth(_detector1, ColumnName1, ColumnWidth1);
		}
		#endregion Tests for UpdateColumnWidth() method

		#region Tests for AddVisibleColumn() method
		[Test]
		public void AddVisibleColumn()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddVisibleColumn(_detector1, _columnInfo1);
			Assert.IsTrue(_project.IsVisibleColumn(_detector1, ColumnName1));
		}

		[Test]
		public void AddVisibleColumnToExistingColumn()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddVisibleColumn(_detector1, _columnInfo1);
			_project.AddVisibleColumn(_detector1, _columnInfo2);
			Assert.IsTrue(_project.IsVisibleColumn(_detector1, ColumnName2));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddVisibleColumnDetectorNull()
		{
			_project.AddVisibleColumn(null, _columnInfo1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddVisibleColumnColumnNameNull()
		{
			_project.AddVisibleColumn(_detector1, null);
		}
		#endregion Tests for AddVisibleColumn() method

		#region Tests for RemoveVisibleColumn() method
		[Test]
		public void RemoveVisibleColumn()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddVisibleColumn(_detector1, _columnInfo1);
			_project.RemoveVisibleColumn(_detector1, ColumnName1);
			Assert.IsFalse(_project.IsVisibleColumn(_detector1, ColumnName1));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveVisibleColumnDetectorNull()
		{
			_project.RemoveVisibleColumn(null, ColumnName1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveVisibleColumnColumnNameNull()
		{
			_project.RemoveVisibleColumn(_detector1, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RemoveVisibleColumnColumnNameEmpty()
		{
			_project.RemoveVisibleColumn(_detector1, string.Empty);
		}
		#endregion Tests for RemoveVisibleColumn() method

		#region Tests for IsVisibleColumn() method
		[Test]
		public void IsVisibleColumnFalse()
		{
			_project.AddFile(FileName1, OneDetector);
			Assert.IsFalse(_project.IsVisibleColumn(_detector1, ColumnName1));
		}

		[Test]
		public void IsVisibleColumnTrue()
		{
			_project.AddFile(FileName1, OneDetector);
			_project.AddVisibleColumn(_detector1, _columnInfo1);
			Assert.IsTrue(_project.IsVisibleColumn(_detector1, ColumnName1));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void IsVisibleColumnDetectorNull()
		{
			_project.IsVisibleColumn(null, ColumnName1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void IsVisibleColumnColumnNameNull()
		{
			_project.IsVisibleColumn(_detector1, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void IsVisibleColumnColumnNameEmpty()
		{
			_project.IsVisibleColumn(_detector1, string.Empty);
		}
		#endregion Tests for IsVisibleColumn() method
	}
}
