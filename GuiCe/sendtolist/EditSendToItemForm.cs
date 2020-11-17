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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Defraser.Util;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// Form for changing the path of an existing send-to item.
	/// </summary>
	public partial class EditSendToItemForm : Form
	{
		private readonly SendToList _sendToList;
		private ISendToItem _item;
        private ParametersChecker _checker;

		#region Properties

		/// <summary>
		/// The <see cref="SendToPlayer"/> to edit.
		/// </summary>
		internal ISendToItem Item
		{
			set
			{
				PreConditions.Argument("value").Value(value).IsNotNull();
				textBoxPath.Text = value.Path;
				textBoxName.Text = value.Name;
                textBoxParameters.Text = value.Parameters;
				_item = value;
			}
		}

		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="EditSendToItemForm"/>.
		/// </summary>
		/// <param name="sendToList">The <see cref="SendToList"/> containing the item to edit</param>
		public EditSendToItemForm(SendToList sendToList, ParametersChecker parametersChecker)
		{
            _checker = parametersChecker;
            _sendToList = sendToList;

			InitializeComponent();
		}

		#region Event handlers

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			Debug.Assert(!string.IsNullOrEmpty(textBoxName.Text));

			string extension = Path.GetExtension(textBoxPath.Text);
			string name = textBoxName.Text;
			string path = textBoxPath.Text;
            string parameters = textBoxParameters.Text;
			if (!File.Exists(path) || !openFileDialog.Filter.Contains(extension))
			{
				MessageBox.Show(string.Format("This path for the {0}-player is not valid. Please, select the executable.", name),
				                "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Information,
				                MessageBoxDefaultButton.Button1);
				return;
			}
			_item.Path = path;
            if (!_checker.IsValidParameterString(parameters))
            {
                MessageBox.Show(string.Format("The parameter string is invalid: {0}", parameters), 
                                "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _item.Parameters = parameters;

            _sendToList.InvokeCollectionChanged();
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
