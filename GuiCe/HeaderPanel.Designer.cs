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

using WeifenLuo.WinFormsUI.Docking;

namespace Defraser.GuiCe
{
	partial class HeaderPanel
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
			if (disposing)
			{
				// Stop listening to actions from the headerDetailTree
				this.headerDetailTree.AddColumns -= new System.EventHandler<AddColumnsEventArgs>(this.headerDetailTree_AddColumns);

				if (components != null)
				{
					components.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeaderPanel));
			this.headerContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemRemoveColumn = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemColumnChooser = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.headerTree)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.headerDetailTree)).BeginInit();
			this.headerContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// headerTree
			// 
			this.headerTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.headerTree.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.headerTree.Location = new System.Drawing.Point(0, 20);
			this.headerTree.Name = "headerTree";
			this.headerTree.Size = new System.Drawing.Size(611, 343);
			this.headerTree.SortColumnDirection = System.ComponentModel.ListSortDirection.Ascending;
			this.headerTree.SortColumnName = null;
			this.headerTree.TabIndex = 0;
			this.headerTree.FocusRowChanged += new System.EventHandler(this.headerTree_FocusRowChanged);
			this.headerTree.DataSourceChanged += new System.EventHandler<System.EventArgs>(this.headerTree_DataSourceChanged);
			this.headerTree.SelectionChanged += new System.EventHandler(this.headerTree_SelectionChanged);
			this.headerTree.GotFocus += new System.EventHandler(headerTree_GotFocus);
            this.headerTree.RowDrop += new Infralution.Controls.VirtualTree.RowDropHandler(headerTree_RowDrop);
			// 
			// columnChooser
			// 
			this.columnChooser.Location = new System.Drawing.Point(44, 58);
			this.columnChooser.Name = "columnChooser";
			this.columnChooser.Visible = false;
			this.columnChooser.VisibleChanged += new System.EventHandler(this.columnChooser_VisibleChanged);
			// 
			// headerContextMenuStrip
			// 
			this.headerContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemoveColumn,
            this.toolStripMenuItemColumnChooser});
			this.headerContextMenuStrip.Name = "headerContextMenuStrip";
			this.headerContextMenuStrip.Size = new System.Drawing.Size(164, 48);
			// 
			// toolStripMenuItemRemoveColumn
			// 
			this.toolStripMenuItemRemoveColumn.Name = "toolStripMenuItemRemoveColumn";
			this.toolStripMenuItemRemoveColumn.Size = new System.Drawing.Size(163, 22);
			this.toolStripMenuItemRemoveColumn.Text = "Remove Column";
			this.toolStripMenuItemRemoveColumn.Click += new System.EventHandler(this.toolStripMenuItemRemoveColumn_Click);
			// 
			// toolStripMenuItemColumnChooser
			// 
			this.toolStripMenuItemColumnChooser.Name = "toolStripMenuItemColumnChooser";
			this.toolStripMenuItemColumnChooser.Size = new System.Drawing.Size(163, 22);
			this.toolStripMenuItemColumnChooser.Text = "Column Chooser";
			this.toolStripMenuItemColumnChooser.Click += new System.EventHandler(this.toolStripMenuItemColumnChooser_Click);
			//
			// HeaderTree
			//
			this.Controls.Add(this.headerTree);
			// 
			// HeaderPanel
			// 
			this.Name = "HeaderPanel";
			this.Size = new System.Drawing.Size(611, 530);
			((System.ComponentModel.ISupportInitialize)(this.headerTree)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.headerDetailTree)).EndInit();
			this.headerContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);
			//
			// Header Detail Tree
			//
			this.headerDetailTree.AddColumns += new System.EventHandler<AddColumnsEventArgs>(this.headerDetailTree_AddColumns);
		}

		#endregion

		private readonly HeaderTree headerTree;
		private HeaderDetailTree headerDetailTree;
		private ColumnChooser columnChooser;
		private System.Windows.Forms.ContextMenuStrip headerContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveColumn;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumnChooser;
	}
}
