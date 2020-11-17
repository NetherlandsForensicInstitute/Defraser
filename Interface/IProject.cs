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

namespace Defraser.Interface
{
	/// <summary>
	/// Specifies the project metadata key.
	/// </summary>
	public enum ProjectMetadataKey
	{
		/// <summary>The version of the project file (read-only).</summary>
		FileVersion,
		/// <summary>The description of the project.</summary>
		ProjectDescription,
		/// <summary>The name of the project investigator.</summary>
		InvestigatorName,
		/// <summary>The date the project was created.</summary>
		DateCreated,
		/// <summary>The date the project was last modified.</summary>
		DateLastModified
	}

	/// <summary>
	/// Stores project information.
	/// 
	/// The input files in a project are the image files that were scanned
	/// for video data blocks. For each input file, a list of detectors is
	/// stored that were used to scan the file and a list of data blocks
	/// detected in the file.
	/// The project metadata contains the project's properties
	/// (see <see cref="ProjectMetadataKey"/>). The 'visible columns' describe
	/// the properties of data blocks and attributes of data block results
	/// that are visible as columns in the user interface.
	/// </summary>
	/// <remarks>
	/// The list of input files includes files for which no results were found.
	/// </remarks>
	public interface IProject : IDisposable
	{
		#region Events
		/// <summary>Occurs when the project is changed.</summary>
		event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		/// <summary>Occurs when the <see cref="Dirty"/> property changes.</summary>
		event PropertyChangedEventHandler PropertyChanged;
		#endregion Events

		/// <summary>The name of the project file.</summary>
		string FileName { get; set; }
		/// <summary>Indicates whether the project has changed since it was last saved.</summary>
		bool Dirty { get; set; }

		/// <summary>Stop the creation of ProjectChanged events.</summary>
		void SuspendProjectChangeNotification();
		/// <summary>Continue the creation of ProjectChanged events. Throws the last unthrown event.</summary>
		void ResumeProjectChangeNotification();

		/// <summary>
		/// Retrieves the project metadata.
		/// </summary>
		/// <returns>the metadata</returns>
		IDictionary<ProjectMetadataKey, string> GetMetadata();

		/// <summary>
		/// Sets or updates the project metadata.
		/// </summary>
		/// <param name="metadata">the new metadata</returns>
		void SetMetadata(IDictionary<ProjectMetadataKey, string> metadata);

		/// <summary>
		/// Lists the input files in this project.
		/// </summary>
		/// <remarks>
		/// This includes files for which no results were found.
		/// </remarks>
		/// <returns>the input files</returns>
		IList<IInputFile> GetInputFiles();

		/// <summary>
		/// Lists the detectors used for the given <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">the input file</param>
		/// <returns>the detectors</returns>
		IList<IDetector> GetDetectors(IInputFile inputFile);

		/// <summary>
		/// Returns the time taken to scan the given <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">the input file</param>
		/// <returns>the scan duration of the given file</returns>
		TimeSpan GetScanDuration(IInputFile inputFile);

		/// <summary>
		/// Sets the time taken to scan the given <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">the input file</param>
		/// <param name="scanDuration">the scan duration of <paramref name="inputFile"/></param>
		void SetScanDuration(IInputFile inputFile, TimeSpan scanDuration);

		/// <summary>
		/// Lists the data blocks in this project that were detected for
		/// the given <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">the input file</param>
		/// <returns>the data blocks for the file</returns>
		IList<IDataBlock> GetDataBlocks(IInputFile inputFile);

		/// <summary>
		/// Adds a file to this project and sets the <paramref name="detectors"/>
		/// used to scan the file.
		/// </summary>
		/// <param name="filePath">The full path of the file to add</param>
		/// <param name="detectors">the detectors used to scan the file</param>
		/// <returns>
		/// The <see cref="IInputFile"/> created for <paramref name="filePath"/> that
		/// was added to this <see cref="IProject"/>
		/// </returns>
		IInputFile AddFile(string filePath, IEnumerable<IDetector> detectors);

		/// <summary>
		/// Adds a detected <paramref name="dataBlock"/> to this project.
		/// </summary>
		/// <param name="dataBlock">the data block to add</param>
		void AddDataBlock(IDataBlock dataBlock);

		/// <summary>
		/// Deletes the file from this project and any data blocks that
		/// belong to it. Deletes detector from this project that are no
		/// longer referenced by any data block.
		/// </summary>
		/// <param name="inputFile">the file to delete</param>
		void DeleteFile(IInputFile inputFile);

		/// <summary>
		/// Deletes the data block from this project and any detectors and
		/// input files that are no longer referenced by any data block.
		/// </summary>
		/// <remarks>
		/// If this is the last data block for the corresponding input file,
		/// this method will delete the file itself.
		/// </remarks>
		/// <param name="dataBlock">the data block to delete</param>
		void DeleteDataBlock(IDataBlock dataBlock);

		/// <summary>
		/// Lists the visible columns for the given <paramref name="detector"/>
		/// in order of appearance.
		/// </summary>
		/// <param name="detector">the detector to retrieve the columns for</param>
		/// <returns>the visibile column for the detector</returns>
		IList<IColumnInfo> GetVisibleColumns(IDetector detector);

		/// <summary>
		/// Sets the visible columns for the given <paramref name="detector"/>
		/// in this project to <paramref name="visibleColumns"/>.
		/// </summary>
		/// <remarks>
		/// Column information is saved per project and per detector.
		/// </remarks>
		/// <param name="detector">the detector to set the columns for</param>
		/// <param name="visibleColumns">the visible columns</param>
		void SetVisibleColumns(IDetector detector, IList<IColumnInfo> visibleColumns);

		/// <summary>
		/// Updates the width of column <paramref name="columnName"/>
		/// for <paramref name="detector"/> in the project database.
		/// </summary>
		/// <param name="detector">the detector to change the column for</param>
		/// <param name="columnName">the column to change</param>
		/// <param name="columnWidth">the new column width</param>
		void UpdateColumnWidth(IDetector detector, string columnName, int columnWidth);

		/// <summary>
		/// Add one visible column
		/// </summary>
		/// <param name="detector">the detector to add the column for</param>
		/// <param name="visibleColumn">the column to add</param>
		void AddVisibleColumn(IDetector detector, IColumnInfo visibleColumn);

		/// <summary>
		/// Remove one visible column.
		/// </summary>
		/// <param name="detector">the detector to remove to column for</param>
		/// <param name="columnName">the column to remove</param>
		void RemoveVisibleColumn(IDetector detector, string columnName);

		/// <summary>
		/// Check if for <paramref name="detector"/> <paramref name="columnName"/> is a visible column.
		/// </summary>
		/// <param name="detector">the detector to check the column for</param>
		/// <param name="columnName">the column to check</param>
		/// <returns>true if the column is visible, else false</returns>
		bool IsVisibleColumn(IDetector detector, string columnName);
	}
}
