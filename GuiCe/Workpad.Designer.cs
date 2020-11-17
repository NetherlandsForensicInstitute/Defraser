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
	partial class Workpad
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Workpad));
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemClose = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRenameWorkpad = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHeaderColumnChooser = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHexDisplay = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.statusStrip.SuspendLayout();
			this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer.ContentPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip
			// 
			this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 0);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(611, 22);
			this.statusStrip.TabIndex = 0;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(596, 17);
			this.toolStripStatusLabel.Spring = true;
			this.toolStripStatusLabel.Text = "Ready";
			this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripContainer
			// 
			// 
			// toolStripContainer.BottomToolStripPanel
			// 
			this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip);
			// 
			// toolStripContainer.ContentPanel
			// 
			this.toolStripContainer.ContentPanel.Controls.Add(this.headerPanel);
			this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(611, 530);
			this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer.Name = "toolStripContainer";
			this.toolStripContainer.Size = new System.Drawing.Size(611, 576);
			this.toolStripContainer.TabIndex = 1;
			this.toolStripContainer.Text = "toolStripContainer1";
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
			// 
			// headerPanel
			// 
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.ReadOnly = false;
			this.headerPanel.Size = new System.Drawing.Size(611, 530);
			this.headerPanel.TabIndex = 0;
			// 
			// menuStrip
			// 
			this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile,
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemView});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(611, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip1";
			// 
			// toolStripMenuItemFile
			// 
			this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.toolStripMenuItemClose});
			this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
			this.toolStripMenuItemFile.Size = new System.Drawing.Size(35, 20);
			this.toolStripMenuItemFile.Text = "&File";
			this.toolStripMenuItemFile.DropDownOpening += new System.EventHandler(this.toolStripMenuItemFile_DropDownOpening);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
			// 
			// toolStripMenuItemClose
			// 
			this.toolStripMenuItemClose.Name = "toolStripMenuItemClose";
			this.toolStripMenuItemClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
			this.toolStripMenuItemClose.Size = new System.Drawing.Size(155, 22);
			this.toolStripMenuItemClose.Text = "&Close";
			this.toolStripMenuItemClose.Click += new System.EventHandler(this.toolStripMenuItemClose_Click);
			// 
			// toolStripMenuItemEdit
			// 
			this.toolStripMenuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator2});
			this.toolStripMenuItemEdit.Name = "toolStripMenuItemEdit";
			this.toolStripMenuItemEdit.Size = new System.Drawing.Size(37, 20);
			this.toolStripMenuItemEdit.Text = "&Edit";
			this.toolStripMenuItemEdit.DropDownOpening += new System.EventHandler(this.toolStripMenuItemEdit_DropDownOpening);
			this.toolStripMenuItemEdit.DropDownClosed += new System.EventHandler(this.toolStripMenuItemEdit_DropDownClosed);
			// 
			// toolStripMenuItemView
			// 
			this.toolStripMenuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRenameWorkpad,
            this.toolStripMenuItemHeaderColumnChooser,
            this.toolStripMenuItemHexDisplay});
			this.toolStripMenuItemView.Name = "toolStripMenuItemView";
			this.toolStripMenuItemView.Size = new System.Drawing.Size(41, 20);
			this.toolStripMenuItemView.Text = "&View";
			// 
			// toolStripMenuItemRenameWorkpad
			// 
			this.toolStripMenuItemRenameWorkpad.Name = "toolStripMenuItemRenameWorkpad";
			this.toolStripMenuItemRenameWorkpad.Size = new System.Drawing.Size(206, 22);
			this.toolStripMenuItemRenameWorkpad.Text = "&Rename Workpad...";
			this.toolStripMenuItemRenameWorkpad.Click += new System.EventHandler(this.toolStripMenuItemRenameWorkpad_Click);
			// 
			// toolStripMenuItemHeaderColumnChooser
			// 
			this.toolStripMenuItemHeaderColumnChooser.Image = global::Defraser.GuiCe.Properties.Resources.column_edit;
			this.toolStripMenuItemHeaderColumnChooser.Name = "toolStripMenuItemHeaderColumnChooser";
			this.toolStripMenuItemHeaderColumnChooser.Size = new System.Drawing.Size(206, 22);
			this.toolStripMenuItemHeaderColumnChooser.Text = "Headers &Column Chooser";
			this.toolStripMenuItemHeaderColumnChooser.Click += new System.EventHandler(this.toolStripMenuItemHeaderColumnChooser_Click);
			// 
			// toolStripMenuItemHexDisplay
			// 
			this.toolStripMenuItemHexDisplay.Checked = true;
			this.toolStripMenuItemHexDisplay.CheckOnClick = true;
			this.toolStripMenuItemHexDisplay.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemHexDisplay.Name = "toolStripMenuItemHexDisplay";
			this.toolStripMenuItemHexDisplay.Size = new System.Drawing.Size(206, 22);
			this.toolStripMenuItemHexDisplay.Text = "&Hex Display";
			this.toolStripMenuItemHexDisplay.CheckStateChanged += new System.EventHandler(this.toolStripMenuItemHexDisplay_CheckStateChanged);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
			// 
			// Workpad
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(611, 576);
			this.Controls.Add(this.toolStripContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Workpad";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer.ContentPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClose;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRenameWorkpad;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHeaderColumnChooser;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHexDisplay;
		private readonly HeaderPanel headerPanel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
	}
}
