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

using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	partial class AddFile
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddFile));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.listBoxFiles = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonRun = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.columnReferenceHeaders = new Infralution.Controls.VirtualTree.Column();
			this.StartKeyframeOverviewCheckBox = new System.Windows.Forms.CheckBox();
			this.containerDetectorSelector = new DetectorSelector();
			this.codecDetectorSelector = new DetectorSelector();			this.label5 = new System.Windows.Forms.Label();
			this.dataGridView1 = new Defraser.GuiCe.ReferenceHeaderGridView();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containerDetectorSelector)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.codecDetectorSelector)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.listBoxFiles, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label4, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.buttonRun, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 6);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.containerDetectorSelector, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.codecDetectorSelector, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.StartKeyframeOverviewCheckBox, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 1, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(601, 571);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// listBoxFiles
			// 
			this.listBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxFiles.FormattingEnabled = true;
			this.listBoxFiles.Location = new System.Drawing.Point(112, 3);
			this.listBoxFiles.Name = "listBoxFiles";
			this.listBoxFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBoxFiles.Size = new System.Drawing.Size(403, 82);
			this.listBoxFiles.TabIndex = 1;
			this.listBoxFiles.IntegralHeight = true;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(49, 3);
			this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&File Name:";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 103);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(99, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Container &Detector:";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(21, 209);
			this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "&Codec Detector:";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(109, 453);
			this.label4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(399, 78);
			this.label4.TabIndex = 4;
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.Visible = false;
			// 
			// buttonRun
			// 
			this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRun.Location = new System.Drawing.Point(440, 537);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(75, 23);
			this.buttonRun.TabIndex = 7;
			this.buttonRun.Text = "Run";
			this.buttonRun.UseVisualStyleBackColor = true;
			this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(521, 537);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 8;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonRemove);
			this.panel1.Controls.Add(this.buttonAdd);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(518, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(83, 100);
			this.panel1.TabIndex = 6;
			// 
			// buttonRemove
			// 
			this.buttonRemove.Location = new System.Drawing.Point(3, 32);
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonRemove.TabIndex = 1;
			this.buttonRemove.Text = "&Remove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// buttonAdd
			// 
			this.buttonAdd.Location = new System.Drawing.Point(3, 3);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(75, 23);
			this.buttonAdd.TabIndex = 0;
			this.buttonAdd.Text = "&Add...";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// containerDetectorSelector
			// 
			this.containerDetectorSelector.AllowDrop = false;
			this.containerDetectorSelector.AutoFitColumns = true;
			this.containerDetectorSelector.Cursor = System.Windows.Forms.Cursors.Default;
			this.containerDetectorSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerDetectorSelector.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.containerDetectorSelector.Location = new System.Drawing.Point(112, 103);
			this.containerDetectorSelector.Name = "containerDetectorSelector";
			this.containerDetectorSelector.RowEvenStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
			this.containerDetectorSelector.RowSelectedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(197)))), ((int)(((byte)(205)))));
			this.containerDetectorSelector.RowSelectedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.containerDetectorSelector.RowSelectedStyle.BorderWidth = 1;
			this.containerDetectorSelector.RowSelectedStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.containerDetectorSelector.RowSelectedUnfocusedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(219)))), ((int)(((byte)(226)))));
			this.containerDetectorSelector.RowSelectedUnfocusedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.containerDetectorSelector.RowSelectedUnfocusedStyle.BorderWidth = 1;
			this.containerDetectorSelector.RowSelectedUnfocusedStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.containerDetectorSelector.RowStyle.BackColor = System.Drawing.SystemColors.Window;
			this.containerDetectorSelector.RowStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.containerDetectorSelector.RowStyle.BorderSide = ((System.Windows.Forms.Border3DSide)(((((System.Windows.Forms.Border3DSide.Left | System.Windows.Forms.Border3DSide.Top)
						| System.Windows.Forms.Border3DSide.Right)
						| System.Windows.Forms.Border3DSide.Bottom)
						| System.Windows.Forms.Border3DSide.Middle)));
			this.containerDetectorSelector.RowStyle.BorderWidth = 1;
			this.containerDetectorSelector.ShowRootRow = false;
			this.containerDetectorSelector.Size = new System.Drawing.Size(403, 100);
			this.containerDetectorSelector.TabIndex = 3;
			// 
			// codecDetectorSelector
			// 
			this.codecDetectorSelector.AllowDrop = false;
			this.codecDetectorSelector.AllowMultiSelect = false;
			this.codecDetectorSelector.AutoFitColumns = true;
			this.codecDetectorSelector.Columns.Add(this.columnReferenceHeaders);
			this.codecDetectorSelector.Cursor = System.Windows.Forms.Cursors.Default;
			this.codecDetectorSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codecDetectorSelector.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.codecDetectorSelector.Location = new System.Drawing.Point(112, 209);
			this.codecDetectorSelector.Name = "codecDetectorSelector";
			this.codecDetectorSelector.RowEvenStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
			this.codecDetectorSelector.RowSelectedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(197)))), ((int)(((byte)(205)))));
			this.codecDetectorSelector.RowSelectedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.codecDetectorSelector.RowSelectedStyle.BorderWidth = 1;
			this.codecDetectorSelector.RowSelectedStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.codecDetectorSelector.RowSelectedUnfocusedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(219)))), ((int)(((byte)(226)))));
			this.codecDetectorSelector.RowSelectedUnfocusedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.codecDetectorSelector.RowSelectedUnfocusedStyle.BorderWidth = 1;
			this.codecDetectorSelector.RowSelectedUnfocusedStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.codecDetectorSelector.RowStyle.BackColor = System.Drawing.SystemColors.Window;
			this.codecDetectorSelector.RowStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.codecDetectorSelector.RowStyle.BorderSide = ((System.Windows.Forms.Border3DSide)(((((System.Windows.Forms.Border3DSide.Left | System.Windows.Forms.Border3DSide.Top)
						| System.Windows.Forms.Border3DSide.Right)
						| System.Windows.Forms.Border3DSide.Bottom)
						| System.Windows.Forms.Border3DSide.Middle)));
			this.codecDetectorSelector.RowStyle.BorderWidth = 1;
			this.codecDetectorSelector.ShowRootRow = false;
			this.codecDetectorSelector.Size = new System.Drawing.Size(403, 100);
			this.codecDetectorSelector.TabIndex = 9;
			this.codecDetectorSelector.SelectionChanged += new System.EventHandler(this.codecDetectorSelector_SelectionChanged);
			// 
			// columnReferenceHeaders
			// 
			this.columnReferenceHeaders.Caption = "Reference Headers";
			this.columnReferenceHeaders.MinWidth = 95;
			this.columnReferenceHeaders.Name = "columnReferenceHeaders";
			this.columnReferenceHeaders.Width = 97;
			this.columnReferenceHeaders.AutoSizePolicy = ColumnAutoSizePolicy.AutoIncrease;
			// 
			// StartKeyframeOverviewCheckBox
			// 
			this.StartKeyframeOverviewCheckBox.AutoSize = true;
			this.StartKeyframeOverviewCheckBox.Location = new System.Drawing.Point(112, 430);
			this.StartKeyframeOverviewCheckBox.Name = "StartKeyframeOverviewCheckBox";
			this.StartKeyframeOverviewCheckBox.Size = new System.Drawing.Size(288, 17);
			this.StartKeyframeOverviewCheckBox.TabIndex = 15;
			this.StartKeyframeOverviewCheckBox.Text = "Generate Keyframe Overview after all files are scanned.";
			this.StartKeyframeOverviewCheckBox.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 315);
			this.label5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(103, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Reference &Headers:";
			// 
			// dataGridView1
			// 
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(112, 315);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(403, 109);
			this.dataGridView1.TabIndex = 17;
			// 
			// openFileDialog
			// 
			this.openFileDialog.AddExtension = false;
			this.openFileDialog.Filter = "All files|*.*";
			this.openFileDialog.Multiselect = true;
			this.openFileDialog.Title = "Browse";
			// 
			// AddFile
			// 
			this.AcceptButton = this.buttonRun;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(750, 580);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "AddFile";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add File";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containerDetectorSelector)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.codecDetectorSelector)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ListBox listBoxFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button buttonRun;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonRemove;
		private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.CheckBox StartKeyframeOverviewCheckBox;

        private DetectorSelector containerDetectorSelector;
        private DetectorSelector codecDetectorSelector;
		private Infralution.Controls.VirtualTree.Column columnReferenceHeaders;
		private System.Windows.Forms.Label label5;
		private ReferenceHeaderGridView dataGridView1;
	}
}
