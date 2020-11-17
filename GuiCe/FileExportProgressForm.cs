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
using System.ComponentModel;
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	internal sealed partial class FileExportProgressForm : Form, IProgressReporter
	{
		private const int ProgressBarLastUpdate = 333; // in milliseconds

		#region Events
		/// <summary>
		/// These events are forwarded from the <see cref="BackgroundWorker"/>.
		/// </summary>
		public event EventHandler<DoWorkEventArgs> DoWork;
		#endregion Events

		#region Attributes
		private DateTime _progressBarLastUpdate;
		#endregion Attributes

		#region Properties
		/// <summary>
		/// Set the text above the progress bar.
		/// </summary>
		public string ProgressLabelText
		{
			set { labelProgress.Text = value; }
		}

		public bool CancellationPending
		{
			get { return backgroundWorker.CancellationPending; }
		}
		#endregion Properties

		public FileExportProgressForm()
		{
			InitializeComponent();
		}

		public void ReportProgress(int percentProgress)
		{
			backgroundWorker.ReportProgress(percentProgress);
		}

		public void ReportProgress(int percentProgress, object userState)
		{
			backgroundWorker.ReportProgress(percentProgress, userState);
		}

		#region Event handlers
		private void FileExportProgressForm_Shown(object sender, EventArgs e)
		{
			backgroundWorker.RunWorkerAsync();
		}

		private void FileExportProgressForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (backgroundWorker.IsBusy)
			{
				e.Cancel = true;
				backgroundWorker.CancelAsync();
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			// Stops the BackGroundWorker.
			// Once stopped, BackGroundWorker.RunWorkerCompleted event will close the dialog.
			backgroundWorker.CancelAsync();
		}

		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (DoWork != null)
			{
				DoWork(sender, e);
			}
		}

		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var userState = e.UserState as string;
			if (userState != null)
			{
				labelProgress.Text = userState;
			}

			if (DateTime.Now.Subtract(_progressBarLastUpdate).Milliseconds > ProgressBarLastUpdate)
			{
				if (e.ProgressPercentage < progressBar.Maximum)
				{
					progressBar.Value = e.ProgressPercentage;
				}

				_progressBarLastUpdate = DateTime.Now;
			}
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				ProgressLabelText = "Error encountered: " + e.Error.Message;
			}
			else
			{
				Close();
			}
		}
		#endregion Event handlers
	}
}
