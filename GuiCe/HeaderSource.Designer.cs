/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	partial class HeaderSource
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
			Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding1 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeaderSource));
			this.columnName = new Infralution.Controls.VirtualTree.Column();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.fileAndStreamSourceTree = new Infralution.Controls.VirtualTree.VirtualTree();
			this.rowBindingFileAndStreamItem = new Infralution.Controls.VirtualTree.ObjectRowBinding();
			this.databaseRadio = new System.Windows.Forms.RadioButton();
			this.fileAndStreamRadio = new System.Windows.Forms.RadioButton();
			this.databaseSourceList = new System.Windows.Forms.ListBox();
			this.buttonManageDatabase = new System.Windows.Forms.Button();
			this.buttonUseSource = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.informationLabel2 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.informationLabel1 = new System.Windows.Forms.Label();
			this.labelCurrentHeaderSource = new System.Windows.Forms.Label();
			this.buttonResetHeaderSource = new System.Windows.Forms.Button();
			this.textboxCurrentHeaderSource = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fileAndStreamSourceTree)).BeginInit();
			this.SuspendLayout();
			// 
			// columnName
			// 
			this.columnName.AutoSizePolicy = Infralution.Controls.VirtualTree.ColumnAutoSizePolicy.AutoSize;
			this.columnName.Caption = "Item";
			this.columnName.DataField = "__MAIN__";
			this.columnName.Movable = false;
			this.columnName.Name = "columnName";
			this.columnName.Width = 30;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 83F));
			this.tableLayoutPanel1.Controls.Add(this.fileAndStreamSourceTree, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.databaseRadio, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.fileAndStreamRadio, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.databaseSourceList, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.buttonManageDatabase, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.buttonUseSource, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.informationLabel2, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.textBox2, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.informationLabel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelCurrentHeaderSource, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonResetHeaderSource, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.textboxCurrentHeaderSource, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(512, 471);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// fileAndStreamSourceTree
			// 
			this.fileAndStreamSourceTree.AllowDrop = false;
			this.fileAndStreamSourceTree.Columns.Add(this.columnName);
			this.fileAndStreamSourceTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileAndStreamSourceTree.Enabled = false;
			this.fileAndStreamSourceTree.Location = new System.Drawing.Point(133, 55);
			this.fileAndStreamSourceTree.Name = "fileAndStreamSourceTree";
			this.fileAndStreamSourceTree.RowBindings.Add(this.rowBindingFileAndStreamItem);
			this.fileAndStreamSourceTree.ShowColumnHeaders = false;
			this.fileAndStreamSourceTree.ShowRootRow = false;
			this.fileAndStreamSourceTree.Size = new System.Drawing.Size(293, 139);
			this.fileAndStreamSourceTree.TabIndex = 0;
			this.fileAndStreamSourceTree.SelectionChanged += new System.EventHandler(this.fileAndStreamSourceTree_SelectionChanged);
			// 
			// rowBindingFileAndStreamItem
			// 
			objectCellBinding1.Column = this.columnName;
			objectCellBinding1.Field = "Name";
			this.rowBindingFileAndStreamItem.CellBindings.Add(objectCellBinding1);
			this.rowBindingFileAndStreamItem.ChildProperty = "Children";
			this.rowBindingFileAndStreamItem.Name = "rowBindingFileAndStreamItem";
			this.rowBindingFileAndStreamItem.ParentProperty = "Parent";
			this.rowBindingFileAndStreamItem.TypeName = typeof(HeaderSourceItem).FullName;
			// 
			// databaseRadio
			// 
			this.databaseRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.databaseRadio.AutoSize = true;
			this.databaseRadio.Location = new System.Drawing.Point(53, 252);
			this.databaseRadio.Name = "databaseRadio";
			this.databaseRadio.Size = new System.Drawing.Size(74, 17);
			this.databaseRadio.TabIndex = 3;
			this.databaseRadio.Text = "&Database:";
			this.databaseRadio.UseVisualStyleBackColor = true;
			// 
			// fileAndStreamRadio
			// 
			this.fileAndStreamRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileAndStreamRadio.AutoSize = true;
			this.fileAndStreamRadio.Checked = true;
			this.fileAndStreamRadio.Location = new System.Drawing.Point(35, 55);
			this.fileAndStreamRadio.Name = "fileAndStreamRadio";
			this.fileAndStreamRadio.Size = new System.Drawing.Size(92, 17);
			this.fileAndStreamRadio.TabIndex = 4;
			this.fileAndStreamRadio.TabStop = true;
			this.fileAndStreamRadio.Text = "&File or Stream:";
			this.fileAndStreamRadio.UseVisualStyleBackColor = true;
			this.fileAndStreamRadio.CheckedChanged += new System.EventHandler(this.headerSource_CheckedChanged);
			// 
			// databaseSourceList
			// 
			this.databaseSourceList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.databaseSourceList.Enabled = false;
			this.databaseSourceList.FormattingEnabled = true;
			this.databaseSourceList.Location = new System.Drawing.Point(133, 252);
			this.databaseSourceList.Name = "databaseSourceList";
			this.databaseSourceList.Size = new System.Drawing.Size(293, 134);
			this.databaseSourceList.TabIndex = 6;
			this.databaseSourceList.SelectedIndexChanged += new System.EventHandler(this.list_SelectedIndexChanged);
			// 
			// buttonManageDatabase
			// 
			this.buttonManageDatabase.Enabled = false;
			this.buttonManageDatabase.Location = new System.Drawing.Point(432, 252);
			this.buttonManageDatabase.Name = "buttonManageDatabase";
			this.buttonManageDatabase.Size = new System.Drawing.Size(75, 34);
			this.buttonManageDatabase.TabIndex = 7;
			this.buttonManageDatabase.Text = "&Manage Database";
			this.buttonManageDatabase.UseVisualStyleBackColor = true;
			// 
			// buttonUseSource
			// 
			this.buttonUseSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUseSource.Enabled = false;
			this.buttonUseSource.Location = new System.Drawing.Point(351, 436);
			this.buttonUseSource.Name = "buttonUseSource";
			this.buttonUseSource.Size = new System.Drawing.Size(75, 23);
			this.buttonUseSource.TabIndex = 2;
			this.buttonUseSource.Text = "Use Source";
			this.buttonUseSource.UseVisualStyleBackColor = true;
			this.buttonUseSource.Click += new System.EventHandler(this.buttonUseSource_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(432, 436);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new System.Drawing.Point(133, 397);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(293, 33);
			this.textBox1.TabIndex = 9;
			this.textBox1.Text = "The header database is not available in the current release. It will be implement" +
				"ed in a future release.";
			// 
			// informationLabel2
			// 
			this.informationLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.informationLabel2.AutoSize = true;
			this.informationLabel2.Location = new System.Drawing.Point(65, 394);
			this.informationLabel2.Name = "informationLabel2";
			this.informationLabel2.Size = new System.Drawing.Size(62, 13);
			this.informationLabel2.TabIndex = 8;
			this.informationLabel2.Text = "Information:";
			// 
			// textBox2
			// 
			this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox2.Location = new System.Drawing.Point(133, 200);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = new System.Drawing.Size(293, 46);
			this.textBox2.TabIndex = 10;
			this.textBox2.Text = "Select a source detector in a file of the same type as the one you are trying to mak" +
				"e visible. Defraser will use the headers of this source file to decode the curre" +
				"nt video frames.";
			// 
			// informationLabel1
			// 
			this.informationLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.informationLabel1.AutoSize = true;
			this.informationLabel1.Location = new System.Drawing.Point(65, 197);
			this.informationLabel1.Name = "informationLabel1";
			this.informationLabel1.Size = new System.Drawing.Size(62, 13);
			this.informationLabel1.TabIndex = 11;
			this.informationLabel1.Text = "Information:";
			// 
			// labelCurrentHeaderSource
			// 
			this.labelCurrentHeaderSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCurrentHeaderSource.AutoSize = true;
			this.labelCurrentHeaderSource.Location = new System.Drawing.Point(8, 0);
			this.labelCurrentHeaderSource.Name = "labelCurrentHeaderSource";
			this.labelCurrentHeaderSource.Size = new System.Drawing.Size(119, 13);
			this.labelCurrentHeaderSource.TabIndex = 12;
			this.labelCurrentHeaderSource.Text = "Current Header Source:";
			// 
			// buttonResetHeaderSource
			// 
			this.buttonResetHeaderSource.Location = new System.Drawing.Point(432, 3);
			this.buttonResetHeaderSource.Name = "buttonResetHeaderSource";
			this.buttonResetHeaderSource.Size = new System.Drawing.Size(75, 38);
			this.buttonResetHeaderSource.TabIndex = 13;
			this.buttonResetHeaderSource.Text = "&Reset Source";
			this.buttonResetHeaderSource.UseVisualStyleBackColor = true;
			this.buttonResetHeaderSource.Click += new System.EventHandler(this.buttonResetHeaderSource_Click);
			// 
			// textboxCurrentHeaderSource
			// 
			this.textboxCurrentHeaderSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textboxCurrentHeaderSource.Location = new System.Drawing.Point(133, 3);
			this.textboxCurrentHeaderSource.Multiline = true;
			this.textboxCurrentHeaderSource.Name = "textboxCurrentHeaderSource";
			this.textboxCurrentHeaderSource.ReadOnly = true;
			this.textboxCurrentHeaderSource.Size = new System.Drawing.Size(293, 46);
			this.textboxCurrentHeaderSource.TabIndex = 14;
			// 
			// HeaderSource
			// 
			this.AcceptButton = this.buttonUseSource;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(512, 471);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "HeaderSource";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Header Source";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fileAndStreamSourceTree)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button buttonUseSource;
		private System.Windows.Forms.RadioButton fileAndStreamRadio;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.RadioButton databaseRadio;
		private System.Windows.Forms.ListBox databaseSourceList;
		private System.Windows.Forms.Button buttonManageDatabase;
		private System.Windows.Forms.Label informationLabel2;
		private System.Windows.Forms.TextBox textBox1;
		private Column columnName;
		private ObjectRowBinding rowBindingFileAndStreamItem;
		private VirtualTree fileAndStreamSourceTree;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label informationLabel1;
		private System.Windows.Forms.Label labelCurrentHeaderSource;
		private System.Windows.Forms.Button buttonResetHeaderSource;
		private System.Windows.Forms.TextBox textboxCurrentHeaderSource;
	}
}
