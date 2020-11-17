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
using System.ComponentModel;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	partial class ProjectKeyframeOverview
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
				if(_projectManager != null) _projectManager.ProjectChanged -= new EventHandler<ProjectChangedEventArgs>(projectManager_ProjectChanged);
				if (_defaultCodecHeaderManager != null) _defaultCodecHeaderManager.CodecHeaderChanged -= new DefaultCodecHeaderManager.DefaultCodecHeaderChangeHandler(defaultCodecHeaderManager_CodecHeaderChanged);

				if (_fullFileScanner != null)
				{
					_fullFileScanner.ScanProgressChanged -= new ProgressChangedEventHandler(fullFileScanner_ScanProgressChanged);
					_fullFileScanner.ResultDetected -= new FullFileScanner.ResultDetectedHandler(fullFileScanner_ResultDetected);
					_fullFileScanner.ScanCompleted -= new FullFileScanner.ScanCompleteHandler(fullFileScanner_ScanCompleted);
				}

				if(components != null)
				{
					components.Dispose();
				}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectKeyframeOverview));
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding1 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding2 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding3 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding4 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding5 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            Infralution.Controls.VirtualTree.ObjectCellBinding objectCellBinding6 = new Infralution.Controls.VirtualTree.ObjectCellBinding();
            this.sourceInfo = new Infralution.Controls.VirtualTree.Column();
            this.thumb1 = new Infralution.Controls.VirtualTree.Column();
            this.thumb2 = new Infralution.Controls.VirtualTree.Column();
            this.thumb3 = new Infralution.Controls.VirtualTree.Column();
            this.thumb4 = new Infralution.Controls.VirtualTree.Column();
            this.thumb5 = new Infralution.Controls.VirtualTree.Column();
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripDropDownUserAction = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItemStopScan = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRestartScan = new System.Windows.Forms.ToolStripMenuItem();
            this.resultsTree = new Defraser.GuiCe.ProjectKeyframeOverviewTree(this.components);
            this.objectRowBinding1 = new Infralution.Controls.VirtualTree.ObjectRowBinding();
            this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultsTree)).BeginInit();
            this.SuspendLayout();
            // 
            // sourceInfo
            // 
            this.sourceInfo.AutoSizePolicy = Infralution.Controls.VirtualTree.ColumnAutoSizePolicy.AutoIncrease;
            this.sourceInfo.Caption = null;
            this.sourceInfo.CellStyle.CompatibleTextRendering = false;
            this.sourceInfo.Hidable = false;
            this.sourceInfo.MinWidth = 250;
            this.sourceInfo.Movable = false;
            this.sourceInfo.Name = "sourceInfo";
            this.sourceInfo.Selectable = false;
            this.sourceInfo.Sortable = false;
            this.sourceInfo.Width = 440;
            // 
            // thumb1
            // 
            this.thumb1.Caption = null;
            this.thumb1.CellStyle.CompatibleTextRendering = false;
            this.thumb1.Hidable = false;
            this.thumb1.Movable = false;
            this.thumb1.Name = "thumb1";
            this.thumb1.Resizable = false;
            this.thumb1.Selectable = false;
            this.thumb1.Sortable = false;
            this.thumb1.Width = 110;
            // 
            // thumb2
            // 
            this.thumb2.Caption = null;
            this.thumb2.CellStyle.CompatibleTextRendering = false;
            this.thumb2.Hidable = false;
            this.thumb2.Movable = false;
            this.thumb2.Name = "thumb2";
            this.thumb2.Resizable = false;
            this.thumb2.Selectable = false;
            this.thumb2.Sortable = false;
            this.thumb2.Width = 110;
            // 
            // thumb3
            // 
            this.thumb3.Caption = null;
            this.thumb3.CellStyle.CompatibleTextRendering = false;
            this.thumb3.Hidable = false;
            this.thumb3.Movable = false;
            this.thumb3.Name = "thumb3";
            this.thumb3.Resizable = false;
            this.thumb3.Selectable = false;
            this.thumb3.Sortable = false;
            this.thumb3.Width = 110;
            // 
            // thumb4
            // 
            this.thumb4.Caption = null;
            this.thumb4.CellStyle.CompatibleTextRendering = false;
            this.thumb4.Hidable = false;
            this.thumb4.Movable = false;
            this.thumb4.Name = "thumb4";
            this.thumb4.Resizable = false;
            this.thumb4.Selectable = false;
            this.thumb4.Sortable = false;
            this.thumb4.Width = 110;
            // 
            // thumb5
            // 
            this.thumb5.Caption = null;
            this.thumb5.CellStyle.CompatibleTextRendering = false;
            this.thumb5.Hidable = false;
            this.thumb5.Movable = false;
            this.thumb5.Name = "thumb5";
            this.thumb5.Resizable = false;
            this.thumb5.Selectable = false;
            this.thumb5.Sortable = false;
            this.thumb5.Width = 110;
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
            this.toolStripContainer.ContentPanel.AutoScroll = true;
            this.toolStripContainer.ContentPanel.Controls.Add(this.resultsTree);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(992, 651);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.Size = new System.Drawing.Size(992, 673);
            this.toolStripContainer.TabIndex = 0;
            this.toolStripContainer.Text = "toolStripContainer";
            this.toolStripContainer.TopToolStripPanelVisible = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar,
            this.toolStripDropDownUserAction});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(992, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(948, 17);
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(200, 16);
            this.toolStripProgressBar.Visible = false;
            // 
            // toolStripDropDownUserAction
            // 
            this.toolStripDropDownUserAction.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownUserAction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStopScan,
            this.toolStripMenuItemRestartScan});
            this.toolStripDropDownUserAction.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownUserAction.Image")));
            this.toolStripDropDownUserAction.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownUserAction.Name = "toolStripDropDownUserAction";
            this.toolStripDropDownUserAction.Size = new System.Drawing.Size(29, 20);
            // 
            // toolStripMenuItemStopScan
            // 
            this.toolStripMenuItemStopScan.Enabled = false;
            this.toolStripMenuItemStopScan.Image = global::Defraser.GuiCe.Properties.Resources.stop;
            this.toolStripMenuItemStopScan.Name = "toolStripMenuItemStopScan";
            this.toolStripMenuItemStopScan.Size = new System.Drawing.Size(196, 22);
            this.toolStripMenuItemStopScan.Text = "Stop Keyframe Scan";
            this.toolStripMenuItemStopScan.Click += new System.EventHandler(this.toolStripMenuItemStopScan_Click);
            // 
            // toolStripMenuItemRestartScan
            // 
            this.toolStripMenuItemRestartScan.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemRestartScan.Image")));
            this.toolStripMenuItemRestartScan.Name = "toolStripMenuItemRestartScan";
            this.toolStripMenuItemRestartScan.Size = new System.Drawing.Size(196, 22);
            this.toolStripMenuItemRestartScan.Text = "Restart Keyframe Scan";
            this.toolStripMenuItemRestartScan.Click += new System.EventHandler(this.toolStripMenuItemRestartScan_Click);
            // 
            // resultsTree
            // 
            this.resultsTree.AllowDrop = false;
            this.resultsTree.AllowIndividualRowResize = false;
            this.resultsTree.AllowMultiSelect = false;
            this.resultsTree.AllowRowResize = false;
            this.resultsTree.AllowUserPinnedColumns = false;
            this.resultsTree.AutoFitColumns = true;
            this.resultsTree.Columns.Add(this.sourceInfo);
            this.resultsTree.Columns.Add(this.thumb1);
            this.resultsTree.Columns.Add(this.thumb2);
            this.resultsTree.Columns.Add(this.thumb3);
            this.resultsTree.Columns.Add(this.thumb4);
            this.resultsTree.Columns.Add(this.thumb5);
            this.resultsTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsTree.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultsTree.Location = new System.Drawing.Point(0, 0);
            this.resultsTree.Name = "resultsTree";
            this.resultsTree.RowBindings.Add(this.objectRowBinding1);
            this.resultsTree.RowEvenStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.resultsTree.RowHeight = 80;
            this.resultsTree.RowSelectedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(197)))), ((int)(((byte)(205)))));
            this.resultsTree.RowSelectedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
            this.resultsTree.RowSelectedStyle.BorderWidth = 1;
            this.resultsTree.RowSelectedStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.resultsTree.RowSelectedUnfocusedStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(219)))), ((int)(((byte)(226)))));
            this.resultsTree.RowSelectedUnfocusedStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
            this.resultsTree.RowSelectedUnfocusedStyle.BorderWidth = 1;
            this.resultsTree.RowSelectedUnfocusedStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
            this.resultsTree.RowStyle.BackColor = System.Drawing.SystemColors.Window;
            this.resultsTree.RowStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(164)))), ((int)(((byte)(188)))));
            this.resultsTree.ShowColumnHeaders = false;
            this.resultsTree.ShowRootRow = false;
            this.resultsTree.Size = new System.Drawing.Size(992, 651);
            this.resultsTree.TabIndex = 0;
            this.resultsTree.CellClick += new System.EventHandler(this.resultsTree_CellClick);
            // 
            // objectRowBinding1
            // 
            objectCellBinding1.Column = this.sourceInfo;
            objectCellBinding1.Field = "FileSourceInfo";
            objectCellBinding1.Format = null;
            objectCellBinding2.Column = this.thumb1;
            objectCellBinding2.Field = "Result1";
            objectCellBinding2.ShowText = false;
            objectCellBinding3.Column = this.thumb2;
            objectCellBinding3.Field = "Result2";
            objectCellBinding3.ShowText = false;
            objectCellBinding4.Column = this.thumb3;
            objectCellBinding4.Field = "Result3";
            objectCellBinding4.ShowText = false;
            objectCellBinding5.Column = this.thumb4;
            objectCellBinding5.Field = "Result4";
            objectCellBinding5.ShowText = false;
            objectCellBinding6.Column = this.thumb5;
            objectCellBinding6.Field = "Result5";
            objectCellBinding6.ShowText = false;
            this.objectRowBinding1.CellBindings.Add(objectCellBinding1);
            this.objectRowBinding1.CellBindings.Add(objectCellBinding2);
            this.objectRowBinding1.CellBindings.Add(objectCellBinding3);
            this.objectRowBinding1.CellBindings.Add(objectCellBinding4);
            this.objectRowBinding1.CellBindings.Add(objectCellBinding5);
            this.objectRowBinding1.CellBindings.Add(objectCellBinding6);
            this.objectRowBinding1.Name = "objectRowBinding1";
            this.objectRowBinding1.TypeName = "Defraser.GuiCe.ProjectKeyframeOverviewRow";
            // 
            // ProjectKeyframeOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(992, 673);
            this.Controls.Add(this.toolStripContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectKeyframeOverview";
            this.Text = "Project Keyframe Overview";
            this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultsTree)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
		private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownUserAction;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStopScan;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRestartScan;
        private ProjectKeyframeOverviewTree resultsTree;
        private Infralution.Controls.VirtualTree.Column sourceInfo;
        private Infralution.Controls.VirtualTree.Column thumb1;
        private Infralution.Controls.VirtualTree.Column thumb2;
        private Infralution.Controls.VirtualTree.Column thumb3;
        private Infralution.Controls.VirtualTree.Column thumb4;
        private Infralution.Controls.VirtualTree.Column thumb5;
        private Infralution.Controls.VirtualTree.ObjectRowBinding objectRowBinding1;

	}
}
