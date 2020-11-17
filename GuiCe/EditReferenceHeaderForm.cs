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

namespace Defraser.GuiCe
{
	public partial class EditReferenceHeaderForm : Form
	{
		#region Properties
		public string Brand
		{
			get { return textBoxBrand.Text; }
			set { textBoxBrand.Text = value; }
		}

		public string Model
		{
			get { return textBoxModel.Text; }
			set { textBoxModel.Text = value; }
		}

		public string Setting
		{
			get { return textBoxSetting.Text; }
			set { textBoxSetting.Text = value; }
		}
		#endregion Properties

		public EditReferenceHeaderForm()
		{
			InitializeComponent();
		}

		#region Event handlers
		private void EditReferenceHeaderForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if ((DialogResult == DialogResult.OK) && (Brand == "") && (Model == "") && (Setting == ""))
			{
				MessageBox.Show("Please enter at least the brand, model or info/setting of the reference header.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Cancel = true;
			}
		}
		#endregion Event handlers
	}
}
