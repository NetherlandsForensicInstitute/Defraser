/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public partial class EditDefaultCodecHeader : Form
	{
		private readonly DefaultCodecHeaderManager _defaultCodecHeaderManager;
		private readonly ProjectManager _projectManager;
		private readonly Creator<SearchHeader> _searchHeaderCreator;

		#region PrivateClassDefines
		private class DefaultCodecHeaderItem
		{
			private readonly CodecID _codecName;
			private String _referenceName;

			public DefaultCodecHeaderItem(CodecID codecName, String referenceName)
			{
				_codecName = codecName;
				_referenceName = referenceName;
			}

			#region Properties
			public CodecID Codec
			{
				get { return _codecName; }
			}

			public String ReferenceName
			{
				set { _referenceName = value; }
				get
				{
					return _referenceName;
				}
			}
			#endregion Properties
		}

		private class FileListItem
		{
			private readonly IInputFile _inputFile;

			public FileListItem(IInputFile inputFile)
			{
				_inputFile = inputFile;
			}

			#region Properties
			public IInputFile InputFile
			{
				get { return _inputFile; }
			}
			#endregion Properties

			public override string ToString()
			{
				return new FileInfo(_inputFile.Name).Name;
			}
		}
		#endregion PrivateClassDefines

		public EditDefaultCodecHeader(DefaultCodecHeaderManager defaultCodecHeaderManager,
									  ProjectManager projectManager, Creator<SearchHeader> searchHeaderCreator)
        {
			_defaultCodecHeaderManager = defaultCodecHeaderManager;
			_projectManager = projectManager;
			_searchHeaderCreator = searchHeaderCreator;

            InitializeComponent();
        	UpdateCodecList();
        }

		private void UpdateCodecList()
		{
		    CodecID selectedCodecId = (selectProjectDefaultCodecHeaders.SelectedRows.Count > 0)
                            ? ((CodecID)selectProjectDefaultCodecHeaders.SelectedRows[0].Cells[0].Value)
		                    : CodecID.Unknown;
			selectProjectDefaultCodecHeaders.DataSource = _defaultCodecHeaderManager.GetCodecList();
            if(selectedCodecId != CodecID.Unknown)
            {
                SelectCodec(selectedCodecId);
            }
		}

		public void SelectCodec(CodecID codecId)
		{
			List<DefaultCodecHeaderManager.CodecListItem> codecList = selectProjectDefaultCodecHeaders.DataSource as List<DefaultCodecHeaderManager.CodecListItem>;
			if(codecList == null) return;

			int row = 0;
			foreach (DefaultCodecHeaderManager.CodecListItem item in codecList)
			{
				if(item.Codec == codecId)
				{
					selectProjectDefaultCodecHeaders.FirstDisplayedScrollingRowIndex = row;
					selectProjectDefaultCodecHeaders.Refresh();
					selectProjectDefaultCodecHeaders.CurrentCell = selectProjectDefaultCodecHeaders.Rows[row].Cells[0];
					selectProjectDefaultCodecHeaders.Rows[row].Selected = true;
					return;
				}
				row++;
			}
		}

		private void UpdateFileList()
		{
			listAvailableReferenceFiles.DataSource = null;
			if(_projectManager.Project == null) return;

			DataGridViewSelectedRowCollection rows = selectProjectDefaultCodecHeaders.SelectedRows;
			if (rows.Count == 1)
			{
				CodecID selectedCodec = (CodecID) rows[0].Cells[0].Value;
				List<FileListItem> possibleFileList = new List<FileListItem>();

                IEnumerable<IInputFile> detectedFiles = _projectManager.Project.GetInputFiles().Sort("Name", ListSortDirection.Ascending); ;
				foreach (InputFile detectedFile in detectedFiles)
				{
					if (CheckCodecDetectorResultForFile(detectedFile, selectedCodec))
					{
						possibleFileList.Add(new FileListItem(detectedFile));
					}
				}
				listAvailableReferenceFiles.DataSource = possibleFileList;
			}
		}

		private static bool CheckCodecDetectorResultForFile(IInputFile detectedFile, CodecID selectedCodec)
		{
			IList<IDataBlock> dataBlocks = detectedFile.Project.GetDataBlocks(detectedFile);

			foreach (IDataBlock fileDataBlock in dataBlocks)
			{
				if (fileDataBlock.CodecStreams.Count > 0)
				{
					foreach (ICodecStream codecStream in fileDataBlock.CodecStreams)
					{
						if (codecStream.DataFormat != CodecID.Unknown)
						{
							if(CheckDetectors(codecStream.Detectors, selectedCodec))
								return true;
						}
					}
				}
				else if (fileDataBlock.DataFormat != CodecID.Unknown)
				{
					if (CheckDetectors(fileDataBlock.Detectors, selectedCodec))
						return true;
				}
			}
			return false;
		}

		private static bool CheckDetectors(IEnumerable<IDetector> detectors, CodecID selectedCodec)
		{
			foreach (IDetector detector in detectors)
			{
				foreach (CodecID codec in detector.SupportedFormats)
				{
					if (codec == selectedCodec)
					{
						return true;
					}
				}
			}
			return false;
		}

		#region EventHandlers
		private void buttonResetDefaultCodecHeader_Click(object sender, EventArgs e)
		{
			DataGridViewSelectedRowCollection rows = selectProjectDefaultCodecHeaders.SelectedRows;
			if(rows.Count == 1)
			{
				CodecID codec = (CodecID)rows[0].Cells[0].Value;
				_defaultCodecHeaderManager.RemoveDefaultCodecHeader(codec);
			    UpdateCodecList();
			}
		}

		private void selectProjectDefaultCodecHeaders_SelectionChanged(object sender, EventArgs e)
		{
			UpdateFileList();
		}

		private void listAvailableReferenceFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			buttonSearchCodecHeader.Enabled = (listAvailableReferenceFiles.SelectedItem != null);
		}

		private void buttonSearchCodecHeader_Click(object sender, EventArgs e)
		{
			FileListItem fileListItem = listAvailableReferenceFiles.SelectedItem as FileListItem;
			if(fileListItem == null) return;
			DataGridViewSelectedRowCollection rows = selectProjectDefaultCodecHeaders.SelectedRows;
			if(rows.Count != 1) return;

			SearchHeader searchHeader = _searchHeaderCreator();
			searchHeader.Show(Owner);

			if (searchHeader.RunSearchHeader(fileListItem.InputFile, (CodecID)rows[0].Cells[0].Value))
			{
				Close();
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion EventHandlers
	}
}
