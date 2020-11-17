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
using System.Windows.Forms;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// Provides a form for viewing and editing the list of send-to applications.
	/// </summary>
	public partial class SendToForm : Form
	{
		private readonly IFormFactory _formFactory;
		private readonly ISendToListMemento _itemBackup;
		private readonly SendToList _sendToList;
        private ParametersChecker _parametersChecker;

		public SendToForm(SendToList sendToList, IFormFactory formFactory, ParametersChecker parametersChecker)
		{
			_sendToList = sendToList;
			_formFactory = formFactory;
            _parametersChecker = parametersChecker;
			_itemBackup = _sendToList.GetStateMemento();// Create copy of list in case 'Cancel' is pressed

			InitializeComponent();

			_sendToList.CollectionChanged += SendToList_CollectionChanged;
			SendToList_CollectionChanged(_sendToList, EventArgs.Empty);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            if(table.SelectedRows.Count == 0)
            {
                buttonChange.Enabled = false;
                buttonDelete.Enabled = false;
            }
		}

        private void table_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e) {
            if(table.SelectedRows.Count > 0)
            {
                buttonDelete.Enabled = true;
            } else
            {
                buttonDelete.Enabled = false;
            }

            if(table.SelectedRows.Count == 1)
            {
                buttonChange.Enabled = true;
            } else
            {
                buttonChange.Enabled = false;
            }
        }

		#region Event handlers

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			_itemBackup.RevertToRememberedState();

			DialogResult = DialogResult.Cancel;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
            if(table.SelectedRows.Count == 0)
			{
				MessageBox.Show("No item is selected. Please, select the player(s) that must be deleted.", "No player selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

            // First gather the to be deleted items because deleting them direct would mess up the table (events)
            var programsToDelete = new List<string>(table.SelectedRows.Count);

            foreach(DataGridViewRow row in table.SelectedRows)
            {
                string name = row.Cells[0].Value.ToString();
                programsToDelete.Add(name);
            }
            foreach(string name in programsToDelete)
            {
                _sendToList.RemoveItem(name);
            }
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			(new AddSendToItemForm(_sendToList, _parametersChecker)).ShowDialog(this);
		}

		private void buttonChange_Click(object sender, EventArgs e)
		{
            if(table.SelectedRows.Count != 1)
			{
				MessageBox.Show("Please, select (only) one item", "item not correctly selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

            string name = table.SelectedRows[0].Cells[0].Value.ToString();
			var editForm = _formFactory.Create<EditSendToItemForm>(); // TODO: we used to be able to edit the name too!!
			editForm.Item = _sendToList.GetItem(name);
			editForm.ShowDialog(this);
		}

		private void SendToList_CollectionChanged(object sender, EventArgs e)
		{
            table.Rows.Clear();
            table.Refresh();

            foreach(ISendToItem item in _sendToList)
            {
                table.Rows.Add(item.Name, item.Path, item.Parameters);
            }
		}

        #endregion Event handlers

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        
    }
}
