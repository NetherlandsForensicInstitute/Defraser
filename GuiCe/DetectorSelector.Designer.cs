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

using System.ComponentModel;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	partial class DetectorSelector
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.columnCheckBox = new Infralution.Controls.VirtualTree.Column();
			this.cellEditor = new Infralution.Controls.VirtualTree.CellEditor();
			this.checkBox = new System.Windows.Forms.CheckBox();
			this.columnName = new Infralution.Controls.VirtualTree.Column();
			this.columnVersion = new Infralution.Controls.VirtualTree.Column();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// columnCheckBox
			// 
			this.columnCheckBox.Caption = null;
			this.columnCheckBox.CellEditor = this.cellEditor;
			this.columnCheckBox.Movable = false;
			this.columnCheckBox.Name = "columnCheckBox";
			this.columnCheckBox.Resizable = false;
			this.columnCheckBox.Sortable = false;
			this.columnCheckBox.Width = 20;
			this.columnCheckBox.AutoSizePolicy = ColumnAutoSizePolicy.AutoSize;
			// 
			// cellEditor
			// 
			this.cellEditor.Control = this.checkBox;
			this.cellEditor.DisplayMode = Infralution.Controls.VirtualTree.CellEditorDisplayMode.Always;
			this.cellEditor.UseCellHeight = false;
			this.cellEditor.UseCellWidth = false;
			// 
			// checkBox
			// 
			this.checkBox.Location = new System.Drawing.Point(0, 0);
			this.checkBox.Name = "checkBox";
			this.checkBox.Size = new System.Drawing.Size(13, 13);
			this.checkBox.TabIndex = 0;
			// 
			// columnName
			// 
			this.columnName.AutoFitWeight = 100F;
			this.columnName.Caption = "Name";
			this.columnName.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Near;
			this.columnName.Movable = false;
			this.columnName.Name = "columnName";
			this.columnName.SortDirection = ListSortDirection.Ascending;
			this.columnName.AutoSizePolicy = ColumnAutoSizePolicy.AutoSize;
			// 
			// columnVersion
			// 
			this.columnVersion.Caption = "Version";
			this.columnVersion.CellStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnVersion.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnVersion.Movable = false;
			this.columnVersion.Name = "columnVersion";
			this.columnVersion.Resizable = false;
			this.columnVersion.Width = 50;
			this.columnVersion.AutoSizePolicy = ColumnAutoSizePolicy.Manual;
			// 
			// DetectorSelector
			// 
			this.SortColumn = columnName;
			this.AutoFitColumns = true;
			this.Columns.Add(this.columnCheckBox);
			this.Columns.Add(this.columnName);
			this.Columns.Add(this.columnVersion);
			this.Editors.Add(this.cellEditor);
			this.MainColumn = this.columnName;
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Infralution.Controls.VirtualTree.Column columnCheckBox;
		private Infralution.Controls.VirtualTree.Column columnName;
		private Infralution.Controls.VirtualTree.Column columnVersion;
		private Infralution.Controls.VirtualTree.CellEditor cellEditor;
		private System.Windows.Forms.CheckBox checkBox;
	}
}
