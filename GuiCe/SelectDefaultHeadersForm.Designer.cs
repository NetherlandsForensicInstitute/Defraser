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

namespace Defraser.GuiCe
{
	partial class SelectDefaultHeadersForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.listBoxDefaultHeaders = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.openHeaderFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.tableLayoutPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 3;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
			this.tableLayoutPanel.Controls.Add(this.listBoxDefaultHeaders, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.buttonPanel, 2, 0);
			this.tableLayoutPanel.Controls.Add(this.buttonCancel, 2, 1);
			this.tableLayoutPanel.Controls.Add(this.buttonOK, 1, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel.RowCount = 2;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(479, 308);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// listBoxDefaultHeaders
			// 
			this.listBoxDefaultHeaders.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxDefaultHeaders.FormattingEnabled = true;
			this.listBoxDefaultHeaders.Location = new System.Drawing.Point(58, 13);
			this.listBoxDefaultHeaders.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
			this.listBoxDefaultHeaders.Name = "listBoxDefaultHeaders";
			this.listBoxDefaultHeaders.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBoxDefaultHeaders.Size = new System.Drawing.Size(327, 251);
			this.listBoxDefaultHeaders.TabIndex = 2;
			this.listBoxDefaultHeaders.SelectedIndexChanged += new System.EventHandler(this.listBoxDefaultHeaders_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "&Files:";
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.buttonRemove);
			this.buttonPanel.Controls.Add(this.buttonAdd);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonPanel.Location = new System.Drawing.Point(391, 13);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(75, 253);
			this.buttonPanel.TabIndex = 2;
			// 
			// buttonRemove
			// 
			this.buttonRemove.Enabled = false;
			this.buttonRemove.Location = new System.Drawing.Point(0, 29);
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonRemove.TabIndex = 4;
			this.buttonRemove.Text = "Remove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// buttonAdd
			// 
			this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonAdd.Location = new System.Drawing.Point(0, 0);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(75, 23);
			this.buttonAdd.TabIndex = 3;
			this.buttonAdd.Text = "Add...";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonCancel.Location = new System.Drawing.Point(391, 272);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonOK.Location = new System.Drawing.Point(310, 272);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// openHeaderFileDialog
			// 
			this.openHeaderFileDialog.Filter = "All files|*.*";
			// 
			// SelectDefaultHeadersForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(479, 308);
			this.Controls.Add(this.tableLayoutPanel);
			this.Name = "SelectDefaultHeadersForm";
			this.Text = "Select Default Headers";
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.ListBox listBoxDefaultHeaders;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button buttonAdd;
		private System.Windows.Forms.Button buttonRemove;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.OpenFileDialog openHeaderFileDialog;
	}
}
