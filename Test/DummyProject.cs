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
using Defraser.Interface;

namespace Defraser.Test
{
	class DummyProject : IProject
	{
#pragma warning disable 67
		// Note: pragma disables warning: "The event 'Defraser.Test.DummyProject.ProjectChanged' is never used"
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		public string FileName
		{
			get { throw new NotImplementedException("The method or operation is not implemented."); }
			set { throw new NotImplementedException("The method or operation is not implemented."); }
		}

		public bool Dirty
		{
			get { throw new NotImplementedException("The method or operation is not implemented."); }
			set { throw new NotImplementedException("The method or operation is not implemented."); }
		}

		public void SuspendProjectChangeNotification()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void ResumeProjectChangeNotification()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IDictionary<ProjectMetadataKey, string> GetMetadata()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void SetMetadata(IDictionary<ProjectMetadataKey, string> metadata)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IList<IInputFile> GetInputFiles()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IList<IDetector> GetDetectors(IInputFile inputFile)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IList<IDataBlock> GetDataBlocks(IInputFile inputFile)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public TimeSpan GetScanDuration(IInputFile inputFile)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void SetScanDuration(IInputFile inputFile, TimeSpan scanDuration)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void AddDataBlock(IDataBlock dataBlock)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void DeleteFile(IInputFile inputFile)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void DeleteDataBlock(IDataBlock dataBlock)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void Dispose()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IList<IColumnInfo> GetVisibleColumns(IDetector detector)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void UpdateColumnWidth(IDetector detector, string columnName, int columnWidth)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public void AddVisibleColumn(IDetector detector, IColumnInfo visibleColumn)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveVisibleColumn(IDetector detector, string columnName)
		{
			throw new System.NotImplementedException();
		}

		public bool IsVisibleColumn(IDetector detector, string columnName)
		{
			throw new System.NotImplementedException();
		}

		public void SetVisibleColumns(IList<IDetector> detectors, IList<IColumnInfo> visibleColumns)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IInputFile AddFile(string filePath, IEnumerable<IDetector> detectors)
		{
			throw new NotImplementedException("The method AddFile is not implemented.");
		}

		public void SetVisibleColumns(IDetector detector, IList<IColumnInfo> visibleColumns)
		{
			throw new NotImplementedException("The method SetVisibleColumns is not implemented.");
		}
	}
}
