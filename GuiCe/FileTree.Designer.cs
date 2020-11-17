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
	partial class FileTree
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileTree));
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.columnName = new Infralution.Controls.VirtualTree.Column();
			this.columnDetector = new Infralution.Controls.VirtualTree.Column();
			this.columnDetectorVersion = new Infralution.Controls.VirtualTree.Column();
			this.columnOffset = new Infralution.Controls.VirtualTree.Column();
			this.columnLength = new Infralution.Controls.VirtualTree.Column();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemSendSelectionTo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemSaveAsContiguousFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportToXml = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemGotoOffsetInHexWorkshop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveAsSeparateFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Save As Separate Files";
			// 
			// columnName
			// 
			this.columnName.Caption = "Name";
			this.columnName.DataField = "Name";
			this.columnName.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Near;
			this.columnName.Movable = false;
			this.columnName.Name = "columnName";
			this.columnName.Width = 250;
			// 
			// columnDetector
			// 
			this.columnDetector.Caption = "Detector";
			this.columnDetector.DataField = "Detector";
			this.columnDetector.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Near;
			this.columnDetector.Movable = false;
			this.columnDetector.Name = "columnDetector";
			this.columnDetector.Width = 110;
			// 
			// columnDetectorVersion
			// 
			this.columnDetectorVersion.Caption = "Detector Version";
			this.columnDetectorVersion.CellStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnDetectorVersion.DataField = "DetectorVersion";
			this.columnDetectorVersion.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnDetectorVersion.Movable = false;
			this.columnDetectorVersion.Name = "columnDetectorVersion";
			this.columnDetectorVersion.Width = 40;
			// 
			// columnOffset
			// 
			this.columnOffset.Caption = "Offset";
			this.columnOffset.CellStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnOffset.DataField = "Offset";
			this.columnOffset.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnOffset.Movable = false;
			this.columnOffset.Name = "columnOffset";
			// 
			// columnLength
			// 
			this.columnLength.Caption = "Length";
			this.columnLength.CellStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnLength.DataField = "Length";
			this.columnLength.HeaderStyle.HorzAlignment = System.Drawing.StringAlignment.Far;
			this.columnLength.Movable = false;
			this.columnLength.Name = "columnLength";
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "page_white_quest.png");
			this.imageList.Images.SetKeyName(1, "plugin.png");
			this.imageList.Images.SetKeyName(2, "");
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSendSelectionTo,
            this.toolStripSeparator1,
            this.toolStripMenuItemSaveAsContiguousFile,
            this.toolStripMenuItemSaveAsSeparateFiles,
            this.toolStripMenuItemExportToXml,
            this.toolStripSeparator2,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator3,
            this.toolStripMenuItemGotoOffsetInHexWorkshop,
            this.toolStripMenuItemGotoEndOffsetInHexWorkshop});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(364, 176);
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// toolStripMenuItemSendSelectionTo
			// 
			this.toolStripMenuItemSendSelectionTo.Name = "toolStripMenuItemSendSelectionTo";
			this.toolStripMenuItemSendSelectionTo.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemSendSelectionTo.Text = "Send Selection &To";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(360, 6);
			// 
			// toolStripMenuItemSaveAsContiguousFile
			// 
			this.toolStripMenuItemSaveAsContiguousFile.Name = "toolStripMenuItemSaveAsContiguousFile";
			this.toolStripMenuItemSaveAsContiguousFile.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemSaveAsContiguousFile.Text = "Save Selection as &Contiguous File...";
			this.toolStripMenuItemSaveAsContiguousFile.Click += new System.EventHandler(this.toolStripMenuItemSaveAsContiguousFile_Click);
			// 
			// toolStripMenuItemSaveAsSeparateFiles
			// 
			this.toolStripMenuItemSaveAsSeparateFiles.Name = "toolStripMenuItemSaveAsSeparateFiles";
			this.toolStripMenuItemSaveAsSeparateFiles.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemSaveAsSeparateFiles.Text = "Save Selection and children as &Separate Files...";
			this.toolStripMenuItemSaveAsSeparateFiles.Click += new System.EventHandler(toolStripMenuItemSaveAsSeparateFiles_Click);
			// 
			// toolStripMenuItemExportToXml
			// 
			this.toolStripMenuItemExportToXml.Name = "toolStripMenuItemExportToXml";
			this.toolStripMenuItemExportToXml.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemExportToXml.Text = "Export selected Container and Codec Streams to XML...";
			this.toolStripMenuItemExportToXml.Click += new System.EventHandler(this.toolStripMenuItemExportToXml_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(360, 6);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemDelete.Image")));
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemDelete.Text = "&Delete Selection";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(360, 6);
			// 
			// toolStripMenuItemGotoOffsetInHexWorkshop
			// 
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Name = "toolStripMenuItemGotoOffsetInHexWorkshop";
			this.toolStripMenuItemGotoOffsetInHexWorkshop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Text = "Goto Offset in Hex &Workshop";
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Click += new System.EventHandler(this.toolStripMenuItemGotoOffsetInHexWorkshop_Click);
			// 
			// toolStripMenuItemGotoEndOffsetInHexWorkshop
			// 
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Name = "toolStripMenuItemGotoEndOffsetInHexWorkshop";
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
						| System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Size = new System.Drawing.Size(363, 22);
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Text = "Goto End Offset in Hex &Workshop";
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Click += new System.EventHandler(this.toolStripMenuItemGotoEndOffsetInHexWorkshop_Click);
			// 
			// FileTree
			// 
			this.Columns.Add(this.columnName);
			this.Columns.Add(this.columnDetector);
			this.Columns.Add(this.columnDetectorVersion);
			this.Columns.Add(this.columnOffset);
			this.Columns.Add(this.columnLength);
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RowEvenStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
			this.RowSelectedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(197)))), ((int)(((byte)(205)))));
			this.RowSelectedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.RowSelectedStyle.BorderWidth = 1;
			this.RowSelectedStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.RowSelectedUnfocusedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(219)))), ((int)(((byte)(226)))));
			this.RowSelectedUnfocusedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.RowSelectedUnfocusedStyle.BorderWidth = 1;
			this.RowSelectedUnfocusedStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.RowStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
			this.RowStyle.BorderSide = ((System.Windows.Forms.Border3DSide)(((((System.Windows.Forms.Border3DSide.Left | System.Windows.Forms.Border3DSide.Top)
						| System.Windows.Forms.Border3DSide.Right)
						| System.Windows.Forms.Border3DSide.Bottom)
						| System.Windows.Forms.Border3DSide.Middle)));
			this.RowStyle.BorderWidth = 1;
			this.Size = new System.Drawing.Size(273, 313);
			this.SortColumn = this.columnOffset;
			this.DataSourceChanged += new System.EventHandler<System.EventArgs>(this.FileTree_DataSourceChanged);
			this.SelectionChanged += new System.EventHandler(this.FileTree_SelectionChanged);
			this.contextMenuStrip.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private Infralution.Controls.VirtualTree.Column columnName;
		private Infralution.Controls.VirtualTree.Column columnDetector;
		private Infralution.Controls.VirtualTree.Column columnDetectorVersion;
		private Infralution.Controls.VirtualTree.Column columnOffset;
		private Infralution.Controls.VirtualTree.Column columnLength;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSendSelectionTo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAsContiguousFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAsSeparateFiles;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportToXml;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGotoOffsetInHexWorkshop;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGotoEndOffsetInHexWorkshop;
	}
}
