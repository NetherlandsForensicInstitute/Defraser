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
using System.Linq;
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Provides a serializable implementation of <c>IProject</c>.
	/// </summary>
	[DataContract]
	public sealed class Project : IProject
	{
		/// <value>The name of the <see cref="Dirty"/> property.</value>
		public const string DirtyProperty = "Dirty";
		/// <summary>The current project file version.</summary>
		private const string FileVersion = "1.0.0.4";

		#region Inner classes
		public sealed class SerializationContext : ISerializationContext
		{
			private readonly Creator<IInputFile, IProject, string> _createInputFile;

			#region Properties
			public Type Type { get { return typeof(Project); } }
			#endregion Properties

			public SerializationContext(Creator<IInputFile, IProject, string> createInputFile)
			{
				PreConditions.Argument("createInputFile").Value(createInputFile).IsNotNull();

				_createInputFile = createInputFile;
			}

			public void CompleteDeserialization(Object obj)
			{
				((Project)obj).CompleteDeserialization(_createInputFile);
			}
		}

		/// <summary>
		/// Describes the project data for an input file.
		/// </summary>
		[DataContract]
		private class InputFileData
		{
			[DataMember]
			private readonly List<IDetector> _detectors;
			[DataMember]
			private readonly List<IDataBlock> _dataBlocks;
			[DataMember]
			private TimeSpan _scanDuration;

			#region Properties
			/// <summary>The data blocks detected for the input file.</summary>
			public List<IDataBlock> DataBlocks { get { return _dataBlocks; } }
			/// <summary>The detectors used to scan the input file (immutable).</summary>
			public IList<IDetector> Detectors { get { return _detectors.AsReadOnly(); } }
			/// <summary>The time used to scan the input file. </summary>
			public TimeSpan ScanDuration { get { return _scanDuration; } set { _scanDuration = value; } }
			#endregion Properties

			/// <summary>
			/// Creates a new input file data.
			/// </summary>
			/// <param name="detectors">the detectors used to scan the input file</param>
			internal InputFileData(IEnumerable<IDetector> detectors)
			{
				_dataBlocks = new List<IDataBlock>();
				_detectors = new List<IDetector>();

				foreach (IDetector detector in detectors)
				{
					if (_detectors.Contains(detector))
					{
						throw new ArgumentException("Detectors has duplicates.", "detectors");
					}

					_detectors.Add(detector);
				}
			}
		}
		#endregion Inner classes

		#region Events
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion Events

		private Creator<IInputFile, IProject, string> _createInputFile;
		private bool _dirty;

		[DataMember]
		private string _fileName;
		[DataMember]
		private readonly LinkedDictionary<ProjectMetadataKey, string> _metadata;
		[DataMember]
		private readonly LinkedDictionary<IInputFile, InputFileData> _inputFiles;
		[DataMember]
		private readonly LinkedDictionary<IDetector, List<IColumnInfo>> _visibleColumns;

		#region Properties
		public string FileName
		{
			get { return _fileName; }
			set
			{
				PreConditions.Argument("value").Value(value).IsNotNull().And.IsNotEmpty();

				_fileName = value;
				UpdateProject(ProjectChangedType.FileNameChanged, _fileName);
			}
		}

		public bool Dirty
		{
			get { return _dirty; }
			set
			{
				if (value != _dirty)
				{
					_dirty = value;
					OnPropertyChanged(new PropertyChangedEventArgs(DirtyProperty));
				}
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new project.
		/// </summary>
		/// <param name="createInputFile">The factory method for creating input files</param>
		/// <param name="fileName">the name of the project file</param>
		public Project(Creator<IInputFile, IProject, string> createInputFile, string fileName)
		{
			PreConditions.Argument("createInputFile").Value(createInputFile).IsNotNull();
			PreConditions.Argument("fileName").Value(fileName).IsNotNull().And.IsNotEmpty();

			_createInputFile = createInputFile;
			_fileName = fileName;
			_metadata = new LinkedDictionary<ProjectMetadataKey, string>();
			_metadata[ProjectMetadataKey.FileVersion] = FileVersion;
			_inputFiles = new LinkedDictionary<IInputFile, InputFileData>();
			_visibleColumns = new LinkedDictionary<IDetector, List<IColumnInfo>>();
		}

		private void CompleteDeserialization(Creator<IInputFile, IProject, string> createInputFile)
		{
			_createInputFile = createInputFile;
		}

		public void Dispose()
		{
		}

		public IDictionary<ProjectMetadataKey, string> GetMetadata()
		{
			return new ReadOnlyDictionary<ProjectMetadataKey, string>(_metadata);
		}

		public void SetMetadata(IDictionary<ProjectMetadataKey, string> metadata)
		{
			PreConditions.Argument("metadata").Value(metadata).IsNotNull();

			foreach (KeyValuePair<ProjectMetadataKey, string> item in metadata)
			{
				if (!Enum.IsDefined(typeof(ProjectMetadataKey), item.Key))
				{
					throw new InvalidEnumArgumentException("Invalid metadata key.");
				}
				if (item.Key == ProjectMetadataKey.FileVersion && item.Value != FileVersion)
				{
					throw new ArgumentException("Incorrect file version.", "metadata");
				}
				if (item.Value == null)
				{
					throw new ArgumentException("Null value.", "metadata");
				}
			}

			if(_metadata.IsEqual(metadata)) return;

			// Copy metadata
			_metadata.Clear();
			_metadata.Add(ProjectMetadataKey.FileVersion, FileVersion);

			foreach (KeyValuePair<ProjectMetadataKey, string> item in metadata)
			{
				if (item.Key != ProjectMetadataKey.FileVersion)
				{
					_metadata.Add(item.Key, item.Value);
				}
			}

			UpdateProject(ProjectChangedType.MetadataChanged);
		}

		public IList<IInputFile> GetInputFiles()
		{
			return new ReadOnlyList<IInputFile>(_inputFiles.Keys);
		}

		public IList<IDetector> GetDetectors(IInputFile inputFile)
		{
			InputFileData inputFileData = GetInputFileData(inputFile);
			return inputFileData.Detectors;
		}

		public IList<IDataBlock> GetDataBlocks(IInputFile inputFile)
		{
			InputFileData inputFileData = GetInputFileData(inputFile);
			return inputFileData.DataBlocks.AsReadOnly();
		}

		public TimeSpan GetScanDuration(IInputFile inputFile)
		{
			InputFileData inputFileData = GetInputFileData(inputFile);
			return inputFileData.ScanDuration;
		}

		public void SetScanDuration(IInputFile inputFile, TimeSpan scanDuration)
		{
			InputFileData inputFileData = GetInputFileData(inputFile);
			inputFileData.ScanDuration = scanDuration;
		}

		private InputFileData GetInputFileData(IInputFile inputFile)
		{
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();

			InputFileData inputFileData;
			if (!_inputFiles.TryGetValue(inputFile, out inputFileData))
			{
				throw new ArgumentException("Input file does not exist.", "inputFile");
			}
			return inputFileData;
		}

		public IInputFile AddFile(string filePath, IEnumerable<IDetector> detectors)
		{
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			//if (_inputFiles.Keys.Contains(...))
			//{
			//    throw new ArgumentException("File already exists.", "filePath");
			//}
			PreConditions.Argument("detectors").Value(detectors).IsNotNull().And.IsNotEmpty().And.DoesNotContainNull();

			IInputFile inputFile = _createInputFile(this, filePath);
			_inputFiles.Add(inputFile, new InputFileData(detectors));

			UpdateProject(ProjectChangedType.FileAdded, inputFile);
			return inputFile;
		}

		public void AddDataBlock(IDataBlock dataBlock)
		{
			PreConditions.Argument("dataBlock").Value(dataBlock).IsNotNull();

			InputFileData inputFileData;
			if (!_inputFiles.TryGetValue(dataBlock.InputFile, out inputFileData))
			{
				throw new ArgumentException("Input file not in project.", "dataBlock");
			}

			// Add data block if it does not already exist
			if (inputFileData.DataBlocks.Contains(dataBlock))
			{
				throw new ArgumentException("Data block already exists.", "dataBlock");
			}

			inputFileData.DataBlocks.Add(dataBlock);

			UpdateProject(ProjectChangedType.DataBlockAdded, dataBlock);
		}

		public void DeleteFile(IInputFile inputFile)
		{
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();

			// Remove input file if it exists
			if (!_inputFiles.Remove(inputFile))
			{
				throw new ArgumentException("Input file does not exist.", "inputFile");
			}

			UpdateProject(ProjectChangedType.FileDeleted, inputFile);
		}

		public void DeleteDataBlock(IDataBlock dataBlock)
		{
			PreConditions.Argument("dataBlock").Value(dataBlock).IsNotNull();

			if (!_inputFiles.ContainsKey(dataBlock.InputFile))
			{
				throw new ArgumentException("Input file not in project.", "dataBlock");
			}

			// Remove data block if it exists
			if (!_inputFiles[dataBlock.InputFile].DataBlocks.Remove(dataBlock))
			{
				throw new ArgumentException("Data block does not exist.", "dataBlock");
			}

			UpdateProject(ProjectChangedType.DataBlockDeleted, dataBlock);
		}

		public IList<IColumnInfo> GetVisibleColumns(IDetector detector)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			// TODO: detector should be in project

			List<IColumnInfo> visibleColumns;
			if (!_visibleColumns.TryGetValue(detector, out visibleColumns))
			{
				visibleColumns = new List<IColumnInfo>();
			}
			return visibleColumns.AsReadOnly();
		}

		public void SetVisibleColumns(IDetector detector, IList<IColumnInfo> visibleColumns)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			// TODO: detector should be in project
			// TODO: visibleColumns should not contain null values
			PreConditions.Argument("visibleColumns").Value(visibleColumns).IsNotNull();

			// TODO: check that visible column names are unique and part of the detector's columns
			_visibleColumns[detector] = new List<IColumnInfo>(visibleColumns);

			// TODO: only send if visibleColumns actually change
			UpdateProject(ProjectChangedType.VisibleColumnsChanged, detector);
		}

		public void UpdateColumnWidth(IDetector detector, string columnName, int columnWidth)
		{
			// TODO: detector should be in project; create unit test
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull().And.IsNotEmpty();
			if (columnWidth < 0)
			{
				throw new ArgumentOutOfRangeException("columnWidth");
			}

			List<IColumnInfo> columns;
			if (_visibleColumns.TryGetValue(detector, out columns))
			{
				for (int i = 0; i < columns.Count; i++)
				{
					IColumnInfo columnInfo = columns[i];
					if (columnInfo.Name == columnName)
					{
						IColumnInfo updatedColumnInfo = columnInfo.UpdateWidth(columnWidth);
						if (updatedColumnInfo != columnInfo)
						{
							columns[i] = updatedColumnInfo;
							UpdateProject(ProjectChangedType.VisibleColumnsChanged, detector);
						}
						return;
					}
				}
			}
			throw new ArgumentException(string.Format("'{0}' is not a visible column.", columnName), "columnName");
		}

		public void AddVisibleColumn(IDetector detector, IColumnInfo visibleColumn)
		{
			if (detector == null) throw new ArgumentNullException("detector");
			if (visibleColumn == null) throw new ArgumentNullException("visibleColumn");

			List<IColumnInfo> visibleColumns;
			if (_visibleColumns.TryGetValue(detector, out visibleColumns))
			{
				visibleColumns.Add(visibleColumn);
			}
			else
			{
				visibleColumns = new List<IColumnInfo>();
				visibleColumns.Add(visibleColumn);
				_visibleColumns.Add(detector, visibleColumns);
			}
			UpdateProject(ProjectChangedType.VisibleColumnsChanged, detector);
		}

		public void RemoveVisibleColumn(IDetector detector, string columnName)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull().And.IsNotEmpty();

			List<IColumnInfo> columns;
			if (_visibleColumns.TryGetValue(detector, out columns))
			{
				if (columns.Select(x => x.Name).Contains(columnName))
				{
					columns.Remove(columns.Single(x => x.Name == columnName));
					UpdateProject(ProjectChangedType.VisibleColumnsChanged, detector);
				}
			}
		}

		public bool IsVisibleColumn(IDetector detector, string columnName)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull().And.IsNotEmpty();

			List<IColumnInfo> visibleColumns;
			if (_visibleColumns.TryGetValue(detector, out visibleColumns))
			{
				foreach (IColumnInfo visibleColumn in visibleColumns)
				{
					if (visibleColumn.Name == columnName)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void UpdateProject(ProjectChangedType type)
		{
			OnProjectChanged(new ProjectChangedEventArgs(type));

			Dirty = true;
		}

		private void UpdateProject(ProjectChangedType type, object item)
		{
			OnProjectChanged(new ProjectChangedEventArgs(type, item));

			Dirty = true;
		}

		private ProjectChangedEventArgs _lastProjectChangedEventArgs;
		private bool _isSuspended;

		public void SuspendProjectChangeNotification()
		{
			_isSuspended = true;
			_lastProjectChangedEventArgs = null;
		}

		public void ResumeProjectChangeNotification()
		{
			_isSuspended = false;

			if (_lastProjectChangedEventArgs != null)
			{
				OnProjectChanged(_lastProjectChangedEventArgs);
			}
		}

		/// <summary>
		/// Raises the <c>ProjectChanged</c> event
		/// </summary>
		/// <param name="e">the event args for the <c>ProjectChanged</c> event</param>
		private void OnProjectChanged(ProjectChangedEventArgs e)
		{
			if (_isSuspended)
			{
				_lastProjectChangedEventArgs = e;
			}
			else if (ProjectChanged != null)
			{
				ProjectChanged(this, e);
			}
		}

		private void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}
	}
}
