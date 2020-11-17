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
using System.Linq;
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public partial class ProjectKeyframeOverview : Form
	{
		#region Const
		private const int NumThumbs = 5;
		#endregion Const

		private static ProjectKeyframeOverview _activeInstance;

		private readonly FileTreeObject _fileTreeObject;
		private readonly ProjectManager _projectManager;
		private readonly FFmpegManager _ffmpegManager;
		private readonly DefaultCodecHeaderManager _defaultCodecHeaderManager;
		private readonly FullFileScanner _fullFileScanner;
        private readonly Creator<SelectingKeyframe> _selectingKeyframeCreator;
		
        private readonly List<ProjectKeyframeOverviewRow> _resultRows;

        #region FileScanAttributes
		private IList<IInputFile> _projectFiles;
		private int _activeScanFileIndex = -1;
		private int _currentFileScanPercentage;
        #endregion FileScanAttributes

		#region Properties
		public int NumKeyframeRows
		{
            get { return _resultRows.Count; }
		}

		public static bool WindowOpen
		{
			get { return (_activeInstance != null); }
		}

		public static ProjectKeyframeOverview ActiveInstance
		{
			get { return _activeInstance;  }
		}
		#endregion Properties

		public ProjectKeyframeOverview(FileTreeObject fileTreeObject, ProjectManager projectManager, 
									FFmpegManager ffmpegManager, DefaultCodecHeaderManager defaultCodecHeaderManager,
									FullFileScanner fullFileScanner, Creator<SelectingKeyframe> selectingKeyframeCreator)
		{
			if (_activeInstance != null)
			{
				throw new Exception("It's not allowed to create multiple ProjectKeyframeOverview windows.");
			}

			_activeInstance = this;
			_fileTreeObject = fileTreeObject;
			_projectManager = projectManager;
			_ffmpegManager = ffmpegManager;
			_defaultCodecHeaderManager = defaultCodecHeaderManager;
			_fullFileScanner = fullFileScanner;
		    _selectingKeyframeCreator = selectingKeyframeCreator;
            
			InitializeComponent();
			ReportProgressComplete();

            _resultRows = new List<ProjectKeyframeOverviewRow>();
            resultsTree.DataSource = _resultRows;

			projectManager.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(projectManager_ProjectChanged);
			defaultCodecHeaderManager.CodecHeaderChanged += new DefaultCodecHeaderManager.DefaultCodecHeaderChangeHandler(defaultCodecHeaderManager_CodecHeaderChanged);

		    fullFileScanner.ScanNextCodecStreamOnInvalidation = true;
		    fullFileScanner.ScanMoreThanOneFragment = false;
			fullFileScanner.ScanProgressChanged += new ProgressChangedEventHandler(fullFileScanner_ScanProgressChanged);
			fullFileScanner.ResultDetected += new FullFileScanner.ResultDetectedHandler(fullFileScanner_ResultDetected);
			fullFileScanner.ScanCompleted += new FullFileScanner.ScanCompleteHandler(fullFileScanner_ScanCompleted);
			fullFileScanner.ScanNumCodecStreams = 1; // Limit to only 1 codecstream, otherwise the scan will take to long
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			StartKeyframeScan();
		}

		#region GUIMethods
		public ProjectKeyframeOverviewRow GetKeyframeRowAt(int index)
		{
            return _resultRows[index];
		}

		private ProjectKeyframeOverviewRow CreateAndAddKeyframeRow(IInputFile inputFile)
		{
            ProjectKeyframeOverviewRow row = new ProjectKeyframeOverviewRow(resultsTree);
			row.InputFile = inputFile;
            _resultRows.Add(row);
            resultsTree.UpdateRows();
		    return row;
		}
		#endregion GUIMethods

		#region ScanReportProgressMethods
		private void ReportCurrentFile()
		{
			toolStripStatusLabel.Text = string.Format("Scanning for keyframes in file '{0}'...", (_projectFiles != null) ? _projectFiles[_activeScanFileIndex].Name : string.Empty);
			toolStripProgressBar.Visible = true;
			toolStripMenuItemStopScan.Enabled = true;
		}

		private void ReportProgressStop()
		{
			toolStripStatusLabel.Text = "Stopped";
			toolStripProgressBar.Visible = false;
			toolStripMenuItemStopScan.Enabled = false;
		}

		private void ReportProgressComplete()
		{
			toolStripStatusLabel.Text = "Ready";
			toolStripProgressBar.Visible = false;
			toolStripMenuItemStopScan.Enabled = false;
		}

		private void UpdateScanProgressbar()
		{
			if(_activeScanFileIndex == -1)
			{
				toolStripProgressBar.Value = 0;
			}
			else
			{
				double totalPercentageBeforeCurrentFile = _activeScanFileIndex * 100.0;
				double totalPercentage = totalPercentageBeforeCurrentFile + _currentFileScanPercentage;
				double normalizedPercentage = totalPercentage/_projectFiles.Count;
				//Console.WriteLine(string.Format("{0} + {1} = {2}%", totalPercentageBeforeCurrentFile, _currentFileScanPercentage, normalizedPercentage));
				toolStripProgressBar.Value = Math.Max(0, Math.Min(100, (int)normalizedPercentage));
			}
		}
		#endregion ScanReportProgressMethods

		public void StartKeyframeScan()
		{
			if (_projectManager.Project != null && !_fullFileScanner.IsBusy)
			{
				RemoveAllFiles();
			    UpdateInputFiles();
				ScanNextFile();
			}
		}

        private void UpdateInputFiles()
        {
            _projectFiles = _projectManager.Project.GetInputFiles().Sort("Name", ListSortDirection.Ascending).ToList();
        }

		public void StopKeyframeScan()
		{
			if (_fullFileScanner.IsBusy)
			{
				_fullFileScanner.StopScan();
				_ffmpegManager.ClearQueue();
			}
			_activeScanFileIndex = -1;
		}

		public void StopKeyframeScanByUser()
		{
			StopKeyframeScan();
			ReportProgressStop();
		}

		public void RestartKeyframeScan()
		{
			StopKeyframeScan();
			StartKeyframeScan();
		}

		private void ScanNextFile()
		{
			if (_projectManager.Project == null)
				return;

			_activeScanFileIndex++;
			_currentFileScanPercentage = 0;

			if (_activeScanFileIndex < _projectFiles.Count)
			{
				ReportCurrentFile();
				UpdateScanProgressbar();

				_fullFileScanner.StartScan(_projectFiles[_activeScanFileIndex]);
			}
			else
			{
				StopKeyframeScan();
				ReportProgressComplete();
			}
		}

		private void SelectPacketInTree(IFragment rowSourceFragment, IResultNode sourcePacket)
		{
            SelectingKeyframe selectingKeyframe = _selectingKeyframeCreator();
            _fileTreeObject.SelectedItem = rowSourceFragment;
            selectingKeyframe.Show(this);

            selectingKeyframe.SelectKeyframeAfterFileScannerComplete(_fileTreeObject.BackgroundDataBlockScanner, sourcePacket);
		}

		private void AskChangeDefaultCodecHeader(FFmpegResult ffmpegResult)
		{
			// Execute the messagebox in Thread context of the Form.
			// Otherwise the messagebox is not a child of the program.
			this.Invoke((MethodInvoker)delegate
			{
				_defaultCodecHeaderManager.AskChangeDefaultCodecHeader(ffmpegResult, this);
			});
		}

		private bool AskSureCloseProjectKeyframeOverview()
		{
			DialogResult result = MessageBox.Show(this,
                            "The Project Keyframe Overview will be cleared. Are you sure you wish to continue?",
							MainForm.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return false;
			return true;
		}

		private void RemoveRowByInputFile(IInputFile inputFile)
		{
		    UpdateInputFiles();
			List<ProjectKeyframeOverviewRow> toRemove = new List<ProjectKeyframeOverviewRow>();
            foreach (ProjectKeyframeOverviewRow row in _resultRows)
			{
				if(row.InputFile == inputFile)
				{
					toRemove.Add(row);
				}
			}
			foreach (ProjectKeyframeOverviewRow row in toRemove)
			{
                _resultRows.Remove(row);
			}
            resultsTree.UpdateRows();
		}

		private void RemoveAllFiles()
		{
			_activeScanFileIndex = -1;
            _resultRows.Clear();
			_projectFiles = null;
		}

		#region EventHandlers
		private void fullFileScanner_ScanProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_currentFileScanPercentage = e.ProgressPercentage;
			UpdateScanProgressbar();
		}

		private void fullFileScanner_ResultDetected(object sender, FullFileScanner.FileScanResult scanresult)
		{
            IResultNode result = scanresult.Result;
            if (scanresult.Result == null) return;

            // First make sure the result is valid.
            List<IResultNode> allKeyframes = ThumbUtil.GetAllKeyFrames(scanresult.Result.Children.ToArray());
            if (allKeyframes.Count > 0)
            {
			    ProjectKeyframeOverviewRow row = CreateAndAddKeyframeRow(_projectFiles[_activeScanFileIndex]);
		        row.KeyframesSourceFragment = scanresult.SourceFragment;

			    if (_fullFileScanner.ScanNumCodecStreams > 0)
			    {
				    int codecCountNotScanned = scanresult.AvailableCodecStreams - _fullFileScanner.ScanNumCodecStreams;
				    if (scanresult.AvailableCodecStreams > 1)
				    {
					    // Warn that only 1 codecstream in the container is scanned for this row
					    row.WarnMoreCodecStreams(codecCountNotScanned);
				    }
			    }
            
                List<IResultNode> keyFrames = ThumbUtil.CheckMaxThumbCount(allKeyframes, NumThumbs);
                foreach (IResultNode frame in keyFrames)
                {
                    _ffmpegManager.AddToConvertQueue(frame, row);
                }
            }
            else
            {
                // Invalidate the result, the scanner will scan the next codecstream (when available).
                scanresult.IsValid = false;
            }
            
		}

		private void fullFileScanner_ScanCompleted(object sender, EventArgs e)
		{
			ScanNextFile();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if(NumKeyframeRows > 0 && !AskSureCloseProjectKeyframeOverview())
			{
				e.Cancel = true;
				return;
			}
			StopKeyframeScan();
			_activeInstance = null;
		}

		private void defaultCodecHeaderManager_CodecHeaderChanged(CodecID changedcodecid, EventArgs e)
		{
			RestartKeyframeScan();
		}

		private void projectManager_ProjectChanged(object sender, ProjectChangedEventArgs e)
		{
            if (IsDisposed || Disposing) return;
			bool scanState = _fullFileScanner.IsBusy;

			switch (e.Type)
			{
				case ProjectChangedType.FileDeleted:
					if (scanState) StopKeyframeScanByUser();
					RemoveRowByInputFile((IInputFile)e.Item);
					if (scanState) StartKeyframeScan();
					break;
				case ProjectChangedType.Closed:
					if(scanState) StopKeyframeScanByUser();
					RemoveAllFiles();
					break;
			}
		}

		private void toolStripMenuItemStopScan_Click(object sender, EventArgs e)
		{
			StopKeyframeScanByUser();
		}

		private void toolStripMenuItemRestartScan_Click(object sender, EventArgs e)
		{
			RestartKeyframeScan();
		}

        private void resultsTree_CellClick(object sender, EventArgs e)
        {
            ThumbCellWidget cell = sender as ThumbCellWidget;
            if(cell != null)
            {
                ProjectKeyframeOverviewRow row = cell.Row.Item as ProjectKeyframeOverviewRow;
                if (row != null)
                {
                	FFmpegResult result = cell.CellFFmpegResult;
                    if (result != null)
                    {
                        if (result.Bitmap != null)
                        {
                            SelectPacketInTree(row.KeyframesSourceFragment, result.SourcePacket);
                        }
                        else
                        {
                            AskChangeDefaultCodecHeader(result);
                        }
                    }
                }
            }
        }
		#endregion EventHandlers
	}
}
