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
using System.Windows.Forms;
using Defraser.GuiCe.Properties;

namespace Defraser.GuiCe
{
	public partial class Options : Form
	{
		private MainForm _mainForm;

		public Options(MainForm mainForm)
		{
			_mainForm = mainForm;
			InitializeComponent();

			InitControls();

			radioButtonSystemDefinedTempFileLocation.Text += string.Format(" ({0})", TempFile.SystemDefaultTempPath);
			radioButtonSystemDefinedTempFileLocation.Checked = !radioButtonUserDefinedTempFileLocation.Checked;

			// Every time I changed the GUI, these settings got lost
			AcceptButton = buttonOK;
			CancelButton = buttonCancel;
		}

		private void InitControls()
		{
			buttonBrowseTempDirectory.Enabled = radioButtonUserDefinedTempFileLocation.Checked;
			textBoxTempDirectory.Enabled = radioButtonUserDefinedTempFileLocation.Checked;
		}

		#region EventHandlers
		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Settings.Default.Reload();

			DialogResult = DialogResult.Cancel;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			Settings.Default.Save();

			DialogResult = DialogResult.OK;
		}

		private void buttonBrowseTempDirectory_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
				{
					textBoxTempDirectory.Text = folderBrowserDialog.SelectedPath;
				}
			}
		}

		private void buttonBrowseExternalLogViewer_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					textBoxExternalLogViewerApplication.Text = openFileDialog.FileName;
				}
			}
		}

		private void radioButtonUserDefinedTempFileLocation_CheckedChanged(object sender, EventArgs e)
		{
			InitControls();
		}

		private void buttonResetToDefaults_Click(object sender, EventArgs e)
		{
			_mainForm.ResetGuiToDefaults();
		}
		#endregion EventHandlers
	}
}
