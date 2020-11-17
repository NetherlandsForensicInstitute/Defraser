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
	partial class HeaderDetailTree
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
			this.columnName = new Infralution.Controls.VirtualTree.Column();
			this.columnValue = new Infralution.Controls.VirtualTree.Column();
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemAddAsColumnToHeaders = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// own style
			// 
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Location = new System.Drawing.Point(0, 20);
			this.Name = "headerDetailTree";
			this.Size = new System.Drawing.Size(611, 143);
			this.TabIndex = 1;
			// 
			// columnName
			// 
			this.columnName.Caption = "Name";
			this.columnName.DataField = "Name";
			this.columnName.Name = "columnName";
			this.columnName.Sortable = false;
			this.columnName.Width = 200;
			// 
			// columnValue
			// 
			this.columnValue.Caption = "Value";
			this.columnValue.DataField = "Value";
			this.columnValue.Name = "columnValue";
			this.columnValue.Sortable = false;
			this.columnValue.Width = 300;
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAddAsColumnToHeaders});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(213, 26);
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// toolStripMenuItemAddAsColumnToHeaders
			// 
			this.toolStripMenuItemAddAsColumnToHeaders.Name = "toolStripMenuItemAddAsColumnToHeaders";
			this.toolStripMenuItemAddAsColumnToHeaders.Size = new System.Drawing.Size(212, 22);
			this.SetDefaultAddToMenuItemText("Headers");
			this.toolStripMenuItemAddAsColumnToHeaders.Click += new System.EventHandler(this.toolStripMenuItemAddAsColumnToHeaders_Click);
			// 
			// HeaderDetailTree
			// 
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Columns.Add(this.columnName);
			this.Columns.Add(this.columnValue);
			this.contextMenuStrip.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}

		private void SetDefaultAddToMenuItemText(string targetName)
		{
			this.toolStripMenuItemAddAsColumnToHeaders.Text = string.Format("&Add as Column to {0}", targetName);
		}

		#endregion

		private Infralution.Controls.VirtualTree.Column columnName;
		private Infralution.Controls.VirtualTree.Column columnValue;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddAsColumnToHeaders;
	}
}
