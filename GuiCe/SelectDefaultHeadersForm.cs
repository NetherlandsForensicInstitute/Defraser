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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Defraser.GuiCe
{
	public partial class SelectDefaultHeadersForm : Form
	{
		#region Properties
		public IEnumerable<string> DefaultHeaders
		{
			get
			{
				IList<string> defaultHeaders = new List<string>();
				foreach (var defaultHeader in listBoxDefaultHeaders.Items)
				{
					defaultHeaders.Add((string)defaultHeader);
				}
				return defaultHeaders;
			}
			set
			{
				listBoxDefaultHeaders.Items.Clear();

				foreach (string defaultHeader in value)
				{
					listBoxDefaultHeaders.Items.Add(defaultHeader);
				}
			}
		}
		#endregion Properties

		public SelectDefaultHeadersForm()
		{
			InitializeComponent();
		}

		#region Event handlers
		private void listBoxDefaultHeaders_SelectedIndexChanged(object sender, EventArgs e)
		{
			buttonRemove.Enabled = (listBoxDefaultHeaders.SelectedItems.Count > 0);
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			if (openHeaderFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				foreach (string headerFilename in openHeaderFileDialog.FileNames)
				{
					listBoxDefaultHeaders.Items.Add(headerFilename);
				}
			}
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			var selectedItems = new ArrayList(listBoxDefaultHeaders.SelectedItems);
			foreach (object selectedItem in selectedItems)
			{
				listBoxDefaultHeaders.Items.Remove(selectedItem);
			}
		}
		#endregion Event handlers
	}
}
