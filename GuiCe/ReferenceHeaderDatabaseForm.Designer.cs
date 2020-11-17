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

using System.Windows.Forms;

namespace Defraser.GuiCe
{
	partial class ReferenceHeaderDatabaseForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReferenceHeaderDatabaseForm));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.buttonClose = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonExport = new System.Windows.Forms.Button();
			this.buttonImport = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonEdit = new System.Windows.Forms.Button();
			this.buttonDelete = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.referenceHeaderGridView = new Defraser.GuiCe.ReferenceHeaderGridView();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBoxOptions = new System.Windows.Forms.GroupBox();
			this.checkBoxHideDuplicateHeaders = new System.Windows.Forms.CheckBox();
			this.groupBoxFilters = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.labelDetector = new System.Windows.Forms.Label();
			this.listBoxCodecs = new System.Windows.Forms.CheckedListBox();
			this.labelCameraBrand = new System.Windows.Forms.Label();
			this.labelCameraModel = new System.Windows.Forms.Label();
			this.labelInfo = new System.Windows.Forms.Label();
			this.comboBoxCameraBrand = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraModel = new System.Windows.Forms.ComboBox();
			this.comboBoxInfo = new System.Windows.Forms.ComboBox();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.comboBoxWidth = new System.Windows.Forms.ComboBox();
			this.comboBoxHeight = new System.Windows.Forms.ComboBox();
			this.buttonClearFilters = new System.Windows.Forms.Button();
			this.openVideoFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveDatabaseFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openDatabaseFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.referenceHeaderGridView)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			this.groupBoxOptions.SuspendLayout();
			this.groupBoxFilters.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.referenceHeaderGridView, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(984, 390);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.buttonClose);
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.buttonExport);
			this.flowLayoutPanel1.Controls.Add(this.buttonImport);
			this.flowLayoutPanel1.Controls.Add(this.label2);
			this.flowLayoutPanel1.Controls.Add(this.buttonEdit);
			this.flowLayoutPanel1.Controls.Add(this.buttonDelete);
			this.flowLayoutPanel1.Controls.Add(this.buttonAdd);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 351);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(964, 29);
			this.flowLayoutPanel1.TabIndex = 3;
			// 
			// buttonClose
			// 
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonClose.Location = new System.Drawing.Point(879, 3);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(82, 24);
			this.buttonClose.TabIndex = 20;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(863, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(10, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = " ";
			// 
			// buttonExport
			// 
			this.buttonExport.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonExport.Location = new System.Drawing.Point(775, 3);
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.Size = new System.Drawing.Size(82, 24);
			this.buttonExport.TabIndex = 19;
			this.buttonExport.Tag = "";
			this.buttonExport.Text = "Export...";
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
			// 
			// buttonImport
			// 
			this.buttonImport.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonImport.Location = new System.Drawing.Point(687, 3);
			this.buttonImport.Name = "buttonImport";
			this.buttonImport.Size = new System.Drawing.Size(82, 24);
			this.buttonImport.TabIndex = 18;
			this.buttonImport.Text = "Import...";
			this.buttonImport.UseVisualStyleBackColor = true;
			this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(671, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(10, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = " ";
			// 
			// buttonEdit
			// 
			this.buttonEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonEdit.Location = new System.Drawing.Point(590, 3);
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.Size = new System.Drawing.Size(75, 24);
			this.buttonEdit.TabIndex = 17;
			this.buttonEdit.Text = "Edit";
			this.buttonEdit.UseVisualStyleBackColor = true;
			this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
			// 
			// buttonDelete
			// 
			this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonDelete.Location = new System.Drawing.Point(502, 3);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(82, 24);
			this.buttonDelete.TabIndex = 16;
			this.buttonDelete.Text = "Delete";
			this.buttonDelete.UseVisualStyleBackColor = true;
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// buttonAdd
			// 
			this.buttonAdd.Location = new System.Drawing.Point(414, 3);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.buttonAdd.Size = new System.Drawing.Size(82, 24);
			this.buttonAdd.TabIndex = 15;
			this.buttonAdd.Text = "Add...";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// referenceHeaderGridView
			// 
			this.referenceHeaderGridView.AllowUserToAddRows = false;
			this.referenceHeaderGridView.AllowUserToDeleteRows = false;
			this.referenceHeaderGridView.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
			this.referenceHeaderGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.referenceHeaderGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.referenceHeaderGridView.BackgroundColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.referenceHeaderGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.referenceHeaderGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.referenceHeaderGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.referenceHeaderGridView.IncludedColumnVisible = true;
			this.referenceHeaderGridView.IncludedHeaders = ((System.Collections.Generic.IEnumerable<Defraser.Interface.IReferenceHeader>)(resources.GetObject("referenceHeaderGridView.IncludedHeaders")));
			this.referenceHeaderGridView.Location = new System.Drawing.Point(13, 125);
			this.referenceHeaderGridView.Name = "referenceHeaderGridView";
			this.referenceHeaderGridView.RowHeadersVisible = false;
			this.referenceHeaderGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.referenceHeaderGridView.Size = new System.Drawing.Size(958, 223);
			this.referenceHeaderGridView.TabIndex = 14;
			this.referenceHeaderGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.headerDataGridView_CellDoubleClick);
			this.referenceHeaderGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.referenceHeaderGridView_UserDeletedRow);
			this.referenceHeaderGridView.SelectionChanged += new System.EventHandler(this.headerDataGridView_SelectionChanged);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.groupBoxOptions, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.groupBoxFilters, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(13, 13);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(958, 106);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// groupBoxOptions
			// 
			this.groupBoxOptions.Controls.Add(this.checkBoxHideDuplicateHeaders);
			this.groupBoxOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxOptions.Location = new System.Drawing.Point(614, 3);
			this.groupBoxOptions.Name = "groupBoxOptions";
			this.groupBoxOptions.Size = new System.Drawing.Size(341, 100);
			this.groupBoxOptions.TabIndex = 5;
			this.groupBoxOptions.TabStop = false;
			this.groupBoxOptions.Text = "Options";
			// 
			// checkBoxHideDuplicateHeaders
			// 
			this.checkBoxHideDuplicateHeaders.AutoSize = true;
			this.checkBoxHideDuplicateHeaders.Dock = System.Windows.Forms.DockStyle.Top;
			this.checkBoxHideDuplicateHeaders.Location = new System.Drawing.Point(3, 16);
			this.checkBoxHideDuplicateHeaders.Name = "checkBoxHideDuplicateHeaders";
			this.checkBoxHideDuplicateHeaders.Size = new System.Drawing.Size(335, 17);
			this.checkBoxHideDuplicateHeaders.TabIndex = 13;
			this.checkBoxHideDuplicateHeaders.Text = "Hide D&uplicate Headers";
			this.checkBoxHideDuplicateHeaders.UseVisualStyleBackColor = true;
			this.checkBoxHideDuplicateHeaders.CheckedChanged += new System.EventHandler(checkBoxHideDuplicateHeaders_CheckedChanged);
			// 
			// groupBoxFilters
			// 
			this.groupBoxFilters.Controls.Add(this.tableLayoutPanel2);
			this.groupBoxFilters.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxFilters.Location = new System.Drawing.Point(3, 3);
			this.groupBoxFilters.Name = "groupBoxFilters";
			this.groupBoxFilters.Size = new System.Drawing.Size(605, 100);
			this.groupBoxFilters.TabIndex = 4;
			this.groupBoxFilters.TabStop = false;
			this.groupBoxFilters.Text = "Filters";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanel2.ColumnCount = 6;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.labelDetector, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.listBoxCodecs, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelCameraBrand, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelCameraModel, 2, 1);
			this.tableLayoutPanel2.Controls.Add(this.labelInfo, 2, 2);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxCameraBrand, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxCameraModel, 3, 1);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxInfo, 3, 2);
			this.tableLayoutPanel2.Controls.Add(this.labelWidth, 4, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelHeight, 4, 1);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxWidth, 5, 0);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxHeight, 5, 1);
			this.tableLayoutPanel2.Controls.Add(this.buttonClearFilters, 5, 2);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(599, 81);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// labelDetector
			// 
			this.labelDetector.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDetector.AutoSize = true;
			this.labelDetector.Location = new System.Drawing.Point(3, 7);
			this.labelDetector.Name = "labelDetector";
			this.labelDetector.Size = new System.Drawing.Size(51, 13);
			this.labelDetector.TabIndex = 0;
			this.labelDetector.Text = "&Detector:";
			// 
			// listBoxCodecs
			// 
			this.listBoxCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxCodecs.FormattingEnabled = true;
			this.listBoxCodecs.IntegralHeight = false;
			this.listBoxCodecs.Location = new System.Drawing.Point(60, 3);
			this.listBoxCodecs.Name = "listBoxCodecs";
			this.tableLayoutPanel2.SetRowSpan(this.listBoxCodecs, 3);
			this.listBoxCodecs.Size = new System.Drawing.Size(120, 77);
			this.listBoxCodecs.TabIndex = 1;
			this.listBoxCodecs.CheckOnClick = true;
			this.listBoxCodecs.SelectionMode = SelectionMode.One;
			this.listBoxCodecs.DisplayMember = "GetDescriptiveName";
			this.listBoxCodecs.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(listBoxCodec_ItemCheck);
			// 
			// labelCameraBrand
			// 
			this.labelCameraBrand.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelCameraBrand.AutoSize = true;
			this.labelCameraBrand.Location = new System.Drawing.Point(186, 7);
			this.labelCameraBrand.Name = "labelCameraBrand";
			this.labelCameraBrand.Size = new System.Drawing.Size(77, 13);
			this.labelCameraBrand.TabIndex = 2;
			this.labelCameraBrand.Text = "Camera &Brand:";
			// 
			// labelCameraModel
			// 
			this.labelCameraModel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelCameraModel.AutoSize = true;
			this.labelCameraModel.Location = new System.Drawing.Point(186, 34);
			this.labelCameraModel.Name = "labelCameraModel";
			this.labelCameraModel.Size = new System.Drawing.Size(78, 13);
			this.labelCameraModel.TabIndex = 4;
			this.labelCameraModel.Text = "Camera &Model:";
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelInfo.AutoSize = true;
			this.labelInfo.Location = new System.Drawing.Point(186, 62);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(111, 13);
			this.labelInfo.TabIndex = 6;
			this.labelInfo.Text = "&Info / Camera Setting:";
			// 
			// comboBoxCameraBrand
			// 
			this.comboBoxCameraBrand.FormattingEnabled = true;
			this.comboBoxCameraBrand.Location = new System.Drawing.Point(303, 3);
			this.comboBoxCameraBrand.Name = "comboBoxCameraBrand";
			this.comboBoxCameraBrand.Size = new System.Drawing.Size(121, 21);
			this.comboBoxCameraBrand.TabIndex = 3;
			this.comboBoxCameraBrand.SelectionChangeCommitted += new System.EventHandler(comboBoxCameraBrand_SelectionChangeCommitted);
			// 
			// comboBoxCameraModel
			// 
			this.comboBoxCameraModel.FormattingEnabled = true;
			this.comboBoxCameraModel.Location = new System.Drawing.Point(303, 30);
			this.comboBoxCameraModel.Name = "comboBoxCameraModel";
			this.comboBoxCameraModel.Size = new System.Drawing.Size(121, 21);
			this.comboBoxCameraModel.TabIndex = 5;
			this.comboBoxCameraModel.SelectionChangeCommitted += new System.EventHandler(comboBoxCameraModel_SelectionChangeCommitted);
			// 
			// comboBoxInfo
			// 
			this.comboBoxInfo.FormattingEnabled = true;
			this.comboBoxInfo.Location = new System.Drawing.Point(303, 57);
			this.comboBoxInfo.Name = "comboBoxInfo";
			this.comboBoxInfo.Size = new System.Drawing.Size(121, 21);
			this.comboBoxInfo.TabIndex = 7;
			this.comboBoxInfo.SelectionChangeCommitted += new System.EventHandler(comboBoxInfo_SelectionChangeCommitted);
			// 
			// labelWidth
			// 
			this.labelWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelWidth.AutoSize = true;
			this.labelWidth.Location = new System.Drawing.Point(430, 7);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(38, 13);
			this.labelWidth.TabIndex = 8;
			this.labelWidth.Text = "&Width:";
			// 
			// labelHeight
			// 
			this.labelHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeight.AutoSize = true;
			this.labelHeight.Location = new System.Drawing.Point(430, 34);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(41, 13);
			this.labelHeight.TabIndex = 10;
			this.labelHeight.Text = "&Height:";
			// 
			// comboBoxWidth
			// 
			this.comboBoxWidth.FormattingEnabled = true;
			this.comboBoxWidth.Location = new System.Drawing.Point(477, 3);
			this.comboBoxWidth.Name = "comboBoxWidth";
			this.comboBoxWidth.Size = new System.Drawing.Size(121, 21);
			this.comboBoxWidth.TabIndex = 9;
			this.comboBoxWidth.SelectionChangeCommitted += new System.EventHandler(comboBoxWidth_SelectionChangeCommitted);
			// 
			// comboBoxHeight
			// 
			this.comboBoxHeight.FormattingEnabled = true;
			this.comboBoxHeight.Location = new System.Drawing.Point(477, 30);
			this.comboBoxHeight.Name = "comboBoxHeight";
			this.comboBoxHeight.Size = new System.Drawing.Size(121, 21);
			this.comboBoxHeight.TabIndex = 11;
			this.comboBoxHeight.SelectionChangeCommitted += new System.EventHandler(comboBoxHeight_SelectionChangeCommitted);
			// 
			// buttonClearFilters
			// 
			this.buttonClearFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClearFilters.Location = new System.Drawing.Point(523, 57);
			this.buttonClearFilters.Name = "buttonClearFilters";
			this.buttonClearFilters.Size = new System.Drawing.Size(75, 23);
			this.buttonClearFilters.TabIndex = 12;
			this.buttonClearFilters.Text = "Clear Filters";
			this.buttonClearFilters.UseVisualStyleBackColor = true;
			this.buttonClearFilters.Click += new System.EventHandler(buttonClearFilters_Click);
			// 
			// openVideoFileDialog
			// 
			this.openVideoFileDialog.Title = "Select Reference File";
			// 
			// saveDatabaseFileDialog
			// 
			this.saveDatabaseFileDialog.DefaultExt = "xml";
			this.saveDatabaseFileDialog.Filter = "XML Files|*.xml";
			// 
			// ReferenceHeaderDatabaseForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CancelButton = this.buttonClose;
			this.ClientSize = new System.Drawing.Size(1200, 720);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ReferenceHeaderDatabaseForm";
			this.Text = "Reference Header Database";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.referenceHeaderGridView)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.groupBoxOptions.ResumeLayout(false);
			this.groupBoxOptions.PerformLayout();
			this.groupBoxFilters.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.Button buttonExport;
		private System.Windows.Forms.Button buttonImport;
		private System.Windows.Forms.Button buttonDelete;
		private System.Windows.Forms.Button buttonAdd;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.OpenFileDialog openVideoFileDialog;
		private ReferenceHeaderGridView referenceHeaderGridView;
		private System.Windows.Forms.SaveFileDialog saveDatabaseFileDialog;
		private System.Windows.Forms.OpenFileDialog openDatabaseFileDialog;
		private System.Windows.Forms.Button buttonEdit;
		private System.Windows.Forms.GroupBox groupBoxFilters;
		private System.Windows.Forms.GroupBox groupBoxOptions;
		private System.Windows.Forms.CheckBox checkBoxHideDuplicateHeaders;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label labelDetector;
		private System.Windows.Forms.CheckedListBox listBoxCodecs;
		private System.Windows.Forms.Label labelCameraBrand;
		private System.Windows.Forms.Label labelCameraModel;
		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.ComboBox comboBoxCameraBrand;
		private System.Windows.Forms.ComboBox comboBoxCameraModel;
		private System.Windows.Forms.ComboBox comboBoxInfo;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.ComboBox comboBoxWidth;
		private System.Windows.Forms.ComboBox comboBoxHeight;
		private System.Windows.Forms.Button buttonClearFilters;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
	}
}
