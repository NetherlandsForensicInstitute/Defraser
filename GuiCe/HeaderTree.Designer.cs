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
	partial class HeaderTree
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
			//this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemConvertToH264ByteStream = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveSelectionInH264ByteStreamAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSendSelectionInH264ByteStreamTo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveSelectionAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemGotoOffsetInHexWorkshop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemSendSelectionTo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDeleteSelection = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteSelectedHeaderTypes = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemMoveToTop = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemConvertToH264ByteStream,
            this.toolStripSeparator5,
            this.toolStripMenuItemSaveSelectionAs,
            this.toolStripSeparator1,
            this.toolStripMenuItemSendSelectionTo,
            this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself,
            this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren,
            this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren,
            this.toolStripSeparator2,
            this.toolStripMenuItemGotoOffsetInHexWorkshop,
            this.toolStripMenuItemGotoEndOffsetInHexWorkshop,
            this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop,
            this.toolStripSeparator4,
            this.toolStripMenuItemDeleteSelection,
            this.toolStripMenuItemDeleteSelectedHeaderTypes,
            this.toolStripMenuItemDeleteAll,
            this.toolStripSeparator3,
            this.toolStripMenuItemMoveToTop});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(425, 220);
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// toolStripMenuItemConvertToH264ByteStream
			// 
			this.toolStripMenuItemConvertToH264ByteStream.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSendSelectionInH264ByteStreamTo,
            this.toolStripMenuItemSaveSelectionInH264ByteStreamAs});
			this.toolStripMenuItemConvertToH264ByteStream.Name = "toolStripMenuItemConvertToH264ByteStream";
			this.toolStripMenuItemConvertToH264ByteStream.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemConvertToH264ByteStream.Text = "Convert to H.264 Byte Stream...";
			// 
			// toolStripMenuItemSendSelectionInH264ByteStreamTo
			// 
			this.toolStripMenuItemSendSelectionInH264ByteStreamTo.Name = "toolStripMenuItemSendSelectionInH264ByteStreamTo";
			this.toolStripMenuItemSendSelectionInH264ByteStreamTo.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemSendSelectionInH264ByteStreamTo.Text = "Send Selection &To";
			// 
			// toolStripMenuItemSaveSelectionInH264ByteStreamAs
			// 
			this.toolStripMenuItemSaveSelectionInH264ByteStreamAs.Name = "toolStripMenuItemSaveSelectionInH264ByteStreamAs";
			this.toolStripMenuItemSaveSelectionInH264ByteStreamAs.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemSaveSelectionInH264ByteStreamAs.Text = "Save Selection &As...";
			this.toolStripMenuItemSaveSelectionInH264ByteStreamAs.Click += new System.EventHandler(toolStripMenuItemSaveSelectionInH264ByteStreamAs_Click);
			// 
			// toolStripMenuItemSaveSelectionAs
			// 
			this.toolStripMenuItemSaveSelectionAs.Name = "toolStripMenuItemSaveSelectionAs";
			this.toolStripMenuItemSaveSelectionAs.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemSaveSelectionAs.Text = "Save Selection &As...";
			this.toolStripMenuItemSaveSelectionAs.Click += new System.EventHandler(this.toolStripMenuItemSaveSelectionAs_Click);
			// 
			// toolStripMenuItemGotoOffsetInHexWorkshop
			// 
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Name = "toolStripMenuItemGotoOffsetInHexWorkshop";
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Text = "Goto Offset in Hex &Workshop";
			this.toolStripMenuItemGotoOffsetInHexWorkshop.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
			this.toolStripMenuItemGotoOffsetInHexWorkshop.Click += new System.EventHandler(this.toolStripMenuItemGotoOffsetInHexWorkshop_Click);
			// 
			// toolStripMenuItemGotoEndOffsetInHexWorkshop
			// 
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Name = "toolStripMenuItemGotoEndOffsetInHexWorkshop";
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Text = "Goto End Offset in Hex &Workshop";
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H | System.Windows.Forms.Keys.Shift;
			this.toolStripMenuItemGotoEndOffsetInHexWorkshop.Click += new System.EventHandler(toolStripMenuItemGotoEndOffsetInHexWorkshop_Click);
			// 
			// toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop
			// 
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.Name = "toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop";
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.Size = new System.Drawing.Size(298, 22);
			// Note: The text intentionally displays 'Last Child' instead of 'Last Descendant'. Do *NOT* change this!!
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.Text = "Goto End Offset of Last Child in Hex &Workshop";
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H;
			this.toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.Click += new System.EventHandler(toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(421, 6);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(421, 6);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(421, 6);
			// 
			// toolStripMenuItemSendSelectionTo
			// 
			this.toolStripMenuItemSendSelectionTo.Name = "toolStripMenuItemSendSelectionTo";
			this.toolStripMenuItemSendSelectionTo.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemSendSelectionTo.Text = "Send Selection &To";
			// 
			// toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself
			// 
			this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself.Name = "toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself";
			this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself.Text = "Send All Children of Selected Header Type, Excluding &Header Itself, To";
			// 
			// toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren
			// 
			this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren.Name = "toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren";
			this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren.Text = "Send All Headers of Selected Type, &Including Children, To";
			// 
			// toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren
			// 
			this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren.Name = "toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren";
			this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren.Text = "Send All Headers of Selected Type, &Excluding Children, To";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(421, 6);
			// 
			// toolStripMenuItemDeleteSelection
			// 
			this.toolStripMenuItemDeleteSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemDeleteSelection.Name = "toolStripMenuItemDeleteSelection";
			this.toolStripMenuItemDeleteSelection.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDeleteSelection.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemDeleteSelection.Text = "D&elete Selection";
			this.toolStripMenuItemDeleteSelection.Click += new System.EventHandler(this.toolStripMenuItemDeleteSelection_Click);
			// 
			// toolStripMenuItemDeleteSelectedHeaderTypes
			// 
			this.toolStripMenuItemDeleteSelectedHeaderTypes.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemDeleteSelectedHeaderTypes.Name = "toolStripMenuItemDeleteSelectedHeaderTypes";
			this.toolStripMenuItemDeleteSelectedHeaderTypes.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
			this.toolStripMenuItemDeleteSelectedHeaderTypes.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemDeleteSelectedHeaderTypes.Text = "Delete Selected Header &Types";
			this.toolStripMenuItemDeleteSelectedHeaderTypes.Click += new System.EventHandler(this.toolStripMenuItemDeleteHeaderType_Click);
			// 
			// toolStripMenuItemDeleteAll
			// 
			this.toolStripMenuItemDeleteAll.Name = "toolStripMenuItemDeleteAll";
			this.toolStripMenuItemDeleteAll.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemDeleteAll.Text = "Delete &All";
			this.toolStripMenuItemDeleteAll.Click += new System.EventHandler(toolStripMenuItemDeleteAll_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(421, 6);
			// 
			// toolStripMenuItemMoveToTop
			// 
			this.toolStripMenuItemMoveToTop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemMoveToTop.Name = "toolStripMenuItemMoveToTop";
			this.toolStripMenuItemMoveToTop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this.toolStripMenuItemMoveToTop.Size = new System.Drawing.Size(424, 22);
			this.toolStripMenuItemMoveToTop.Text = "&Move to Top";
			this.toolStripMenuItemMoveToTop.Click += new System.EventHandler(this.toolStripMenuItemMoveToTop_Click);
			// 
			// HeaderTree
			// 
			this.ContextMenuStrip = this.contextMenuStrip;
			this.contextMenuStrip.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		//private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemConvertToH264ByteStream;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSelectionInH264ByteStreamAs;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSendSelectionInH264ByteStreamTo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSendSelectionTo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSelectionAs;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGotoOffsetInHexWorkshop;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGotoEndOffsetInHexWorkshop;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteSelection;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteSelectedHeaderTypes;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteAll;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveToTop;
	}
}
