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
using System.IO;
using System.Windows.Forms;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// Form for adding a new send-to item to the <see cref="SendToList"/>.
	/// </summary>
	public partial class AddSendToItemForm : Form
	{
        private ParametersChecker _parametersChecker;

		private readonly SendToList _sendToList;

		/// <summary>
		/// Creates a new <see cref="AddSendToItemForm"/>.
		/// </summary>
		/// <param name="sendToList">The <see cref="SendToList"/> that receives the new item</param>
		public AddSendToItemForm(SendToList sendToList, ParametersChecker parametersChecker)
		{
			_sendToList = sendToList;
            _parametersChecker = parametersChecker;

			InitializeComponent();
		}

		#region Event handlers

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			string extension = Path.GetExtension(textBoxPath.Text);
			string path = textBoxPath.Text;
			string name = textBoxName.Text;
            string parameters = textBoxParameters.Text;
			if (name.Length <= 0)
			{
				MessageBox.Show("No name is given. Please, enter a name.",
				                "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			if (!File.Exists(path) || !openFileDialog.Filter.Contains(extension))
			{
				MessageBox.Show(string.Format("This path for the {0}-player is not valid. Please, select the executable.", name),
				                "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Information,
				                MessageBoxDefaultButton.Button1);
				return;
			}

			if (_sendToList.ContainsItem(name))
			{
				MessageBox.Show(string.Format("The name already exist for player: {0}. Please, enter a new name.", _sendToList.GetPath(name)),
				                "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

            if (!_parametersChecker.IsValidParameterString(parameters))
            {
                MessageBox.Show(string.Format("The parameter string is invalid: {0}", parameters), 
                                "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

			_sendToList.AddSendToItem(name, path, parameters);

			DialogResult = DialogResult.OK;
		}

		private void buttonBrowseVideoPlayer_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				textBoxPath.Text = openFileDialog.FileName;
			}
		}

		#endregion Event handlers
        
	}
}
