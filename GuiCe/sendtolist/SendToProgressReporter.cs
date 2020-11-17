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
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe.sendtolist
{
	public partial class SendToProgressReporter : Form, IProgressReporter
	{
		#region Inner class

		private class SendToBackgroundWorkerArgument
		{
			internal SendToBackgroundWorkerArgument(ISelection selection, string dataPath)
			{
				Selection = selection;
				DataPath = dataPath;
			}

			internal string DataPath { get; private set; }
			internal ISelection Selection { get; private set; }
		}

		#endregion Inner class

		/// <summary>During a task progress is reported <code>ProgressSubTaskCount</code> many times.</summary>
		private const int ProgressSubTaskPerCodecStreamCount = 2;

		/// <summary>The background worker that executes the send to task.</summary>
		private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();

		private readonly IFileExport _fileExport;

		/// <summary>The path to the application the data will be send to</summary>
		private string _applicationPath;

		/// <summary>The parameters to the application the data will be send to</summary>
		private string _applicationParameters;

		/// <summary>Number of codec streams for 'Send To'.</summary>
		private int _codecStreamCount;

		/// <summary>The data file created from the selection</summary>
		private string _dataPath;

		/// <summary>Help variable to increment <code>_progressSubTask</code>.</summary>
		private bool _progressHighValueReached;

		/// <summary>The current sub task.</summary>
		private int _progressSubTask;

		private ParametersChecker _parametersChecker;

		public SendToProgressReporter(IFileExport fileExport, ParametersChecker parametersChecker)
		{
			_fileExport = fileExport;
			_parametersChecker = parametersChecker;

			InitializeComponent();

			_backgroundWorker.WorkerReportsProgress = true;
			_backgroundWorker.WorkerSupportsCancellation = true;

			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
			_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
			_backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
		}

		#region IProgressReporter Members

		public bool CancellationPending
		{
			get { return _backgroundWorker.CancellationPending; }
		}

		public void ReportProgress(int percentProgress)
		{
			_backgroundWorker.ReportProgress(percentProgress);
		}

		public void ReportProgress(int percentProgress, object userState)
		{
			var state = userState as UserState;

			if (state != null && state.UserStateType == UserStateType.CodecStreamCount)
			{
				_codecStreamCount = state.CodecStreamCount;
			}

			_backgroundWorker.ReportProgress(percentProgress, userState);
		}

		#endregion

		/// <summary>
		/// Transform the given selection to a file and start the application specified
		/// with the file as the argument.
		/// </summary>
		/// <param name="selection">The selection the generate the data from</param>
		/// <param name="applicationPath">Application to run</param>
		/// <param name="applicationParameters">Parameters to the application</param>
		/// <param name="dataPath">Data as a file to be fed to the application</param>
		public void SendToAsync(ISelection selection, string applicationPath, string applicationParameters, string dataPath)
		{
			_applicationPath = applicationPath;
			_applicationParameters = applicationParameters;
			_dataPath = dataPath;
			_backgroundWorker.RunWorkerAsync(new SendToBackgroundWorkerArgument(selection, dataPath));

			Show();
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var sendToBackgroundWorkerArgument = e.Argument as SendToBackgroundWorkerArgument;
			var backgroundWorker = sender as BackgroundWorker;

			ISelection selection = sendToBackgroundWorkerArgument.Selection;
			string dataPath = sendToBackgroundWorkerArgument.DataPath;

			IDataPacket dataPacket = selection.GetDataPacket(this);
			if (backgroundWorker.CancellationPending || dataPacket == null)
			{
				return;
			}
			// Saves the current selection to disk as one contiguous file
			IEnumerable<IDataPacket> dataPackets = Enumerable.Repeat(dataPacket, 1);
			_fileExport.SaveAsContiguousFile(dataPackets, Enumerable.Empty<IDetector>()/*don't care*/, dataPath, false /* no Forensic Integrity Log File */);

			string parameters = _parametersChecker.Substitute(_applicationParameters, selection, dataPacket, dataPath);

			if (!backgroundWorker.CancellationPending)
			{
				Process.Start(_applicationPath, parameters);
			}
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage > 90) _progressHighValueReached = true;

			if (e.ProgressPercentage < 10 && _progressHighValueReached)
			{
				_progressHighValueReached = false;
				_progressSubTask++;
			}

			int codecStreamCount = (_codecStreamCount == 0) ? 1 : _codecStreamCount;

			int progressPercentage = e.ProgressPercentage / (ProgressSubTaskPerCodecStreamCount * codecStreamCount) + (int)(((float)_progressSubTask / ((float)ProgressSubTaskPerCodecStreamCount * (float)codecStreamCount)) * 100.0);
			Debug.Assert(progressPercentage <= 100);

			if (progressBar.Value != progressPercentage)
			{
				progressBar.Value = progressPercentage <= 100 ? progressPercentage : 100;
			}
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			_backgroundWorker.CancelAsync();
		}
	}

	internal enum UserStateType
	{
		CodecStreamCount
	}

	internal class UserState
	{
		internal UserState(UserStateType userStateType, int codecStreamCount)
		{
			UserStateType = userStateType;
			CodecStreamCount = codecStreamCount;
		}

		internal UserStateType UserStateType { get; private set; }
		internal int CodecStreamCount { get; private set; }
	}
}
