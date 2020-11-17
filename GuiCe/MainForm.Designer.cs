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

using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Defraser.GuiCe
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.fileDetailTree = new FileDetailTree(this.components);
			this.columnDetail = new Infralution.Controls.VirtualTree.Column();
			this.columnDescription = new Infralution.Controls.VirtualTree.Column();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNewProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOpenProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemCloseProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveProjectAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHeaders = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemRecentFiles = new Infralution.Controls.MruToolStripMenuItem();
			this.toolStripMenuItemRecentProjects = new Infralution.Controls.MruToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddWorkpad = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemFileDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemFilesAndStreams = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemFramePreview = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHeaderDetail = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHeaders2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemVideoKeyframes = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemHex = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemLogFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemColumnChooser = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemStop = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemTools = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEditSendTo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDefaultCodecHeader = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemReferenceHeaderDatabase = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemProjectKeyframeOverview = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonNewWorkpad = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonDefaultCodecHeader = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonProjectKeyframeOverview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonShowColumnChooser = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReferenceHeaderDatabase = new System.Windows.Forms.ToolStripButton();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
			this.toolStripMenuItemDeleteFile = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.fileTreeObject)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fileDetailTree)).BeginInit();
			this.menuStrip.SuspendLayout();
			this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer.ContentPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileTree
			// 
			this.fileTreeObject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileTreeObject.Location = new System.Drawing.Point(0, 20);
			this.fileTreeObject.Name = "fileTreeObject";
			this.fileTreeObject.Size = new System.Drawing.Size(383, 457);
			this.fileTreeObject.TabIndex = 0;
			this.fileTreeObject.FocusRowResultsChanged += new System.EventHandler<ResultsEventArgs>(this.fileTree_FocusRowResultsChanged);
			this.fileTreeObject.DataSourceChanged += new System.EventHandler<System.EventArgs>(this.fileTree_DataSourceChanged);
			// 
			// fileDetailTree
			// 
			this.fileDetailTree.Columns.Add(this.columnDetail);
			this.fileDetailTree.Columns.Add(this.columnDescription);
			this.fileDetailTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileDetailTree.Location = new System.Drawing.Point(0, 20);
			this.fileDetailTree.Name = "fileDetailTree";
			this.fileDetailTree.Size = new System.Drawing.Size(383, 196);
			this.fileDetailTree.TabIndex = 3;
			// 
			// columnDetail
			// 
			this.columnDetail.Caption = "Detail";
			this.columnDetail.Name = "columnDetail";
			// 
			// columnDescription
			// 
			this.columnDescription.Caption = "Description";
			this.columnDescription.Name = "columnDescription";
			//
			// dockPanel
			//
			this.dockPanel.ActiveAutoHideContent = null;
			this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dockPanel.DockBackColor = System.Drawing.SystemColors.AppWorkspace;
			this.dockPanel.DockBottomPortion = 150;
			this.dockPanel.DockLeftPortion = 200;
			this.dockPanel.DockRightPortion = 200;
			this.dockPanel.DockTopPortion = 150;
			this.dockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
			this.dockPanel.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
			this.dockPanel.Location = new System.Drawing.Point(3, 28);
			this.dockPanel.Name = "dockPanel";
			this.dockPanel.RightToLeftLayout = true;
			this.dockPanel.Size = new System.Drawing.Size(573, 332);
			// 
			// headerPanel
			//
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.ReadOnly = true;
			this.headerPanel.RootResult = null;
			this.headerPanel.Size = new System.Drawing.Size(649, 697);
			this.headerPanel.SortColumnDirection = System.ComponentModel.ListSortDirection.Ascending;
			this.headerPanel.SortColumnName = "Offset";
			this.headerPanel.TabIndex = 0;
			this.headerPanel.ColumnChooserVisibleChanged += new System.EventHandler<System.EventArgs>(this.headerPanel_ColumnChooserVisibleChanged);
			// 
			// backgroundFileScanner
			// 
			this.backgroundFileScanner.WorkerReportsProgress = true;
			this.backgroundFileScanner.WorkerSupportsCancellation = true;
			this.backgroundFileScanner.WaitForCancel += new System.ComponentModel.CancelEventHandler(this.BackgroundFileScanner_WaitForCancel);
			this.backgroundFileScanner.Cancel += new System.EventHandler<System.EventArgs>(this.BackgroundFileScanner_Cancel);
			this.backgroundFileScanner.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundFileScanner_DoWork);
			this.backgroundFileScanner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundFileScanner_RunWorkerCompleted);
			this.backgroundFileScanner.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundFileScanner_ProgressChanged);
			this.backgroundFileScanner.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.BackgroundFileScanner_PropertyChanged);
			// 
			// menuStrip
			// 
			this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile,
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemView,
            this.toolStripMenuItemProject,
            this.toolStripMenuItemTools,
            this.toolStripMenuItemHelp});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(1036, 24);
			this.menuStrip.TabIndex = 1;
			this.menuStrip.Text = "menuStrip";
			// 
			// toolStripMenuItemFile
			// 
			this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewProject,
            this.toolStripMenuItemOpenProject,
            this.toolStripMenuItemCloseProject,
            this.toolStripMenuItemSaveProject,
            this.toolStripMenuItemSaveProjectAs,
            this.toolStripSeparator1,
            this.toolStripMenuItemFiles,
            this.toolStripMenuItemHeaders,
            this.toolStripSeparator3,
            this.toolStripMenuItemRecentFiles,
            this.toolStripMenuItemRecentProjects,
            this.toolStripSeparator4,
            this.toolStripMenuItemExit});
			this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
			this.toolStripMenuItemFile.ShortcutKeyDisplayString = "";
			this.toolStripMenuItemFile.Size = new System.Drawing.Size(94, 20);
			this.toolStripMenuItemFile.Text = "&File";
			this.toolStripMenuItemFile.DropDownOpening += new System.EventHandler(this.toolStripMenuItemFile_DropDownOpening);
			// 
			// toolStripMenuItemNewProject
			// 
			this.toolStripMenuItemNewProject.Name = "toolStripMenuItemNewProject";
			this.toolStripMenuItemNewProject.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemNewProject.Text = "&New Project...";
			this.toolStripMenuItemNewProject.Click += new System.EventHandler(this.toolStripMenuItemNewProject_Click);
			// 
			// toolStripMenuItemOpenProject
			// 
			this.toolStripMenuItemOpenProject.Name = "toolStripMenuItemOpenProject";
			this.toolStripMenuItemOpenProject.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemOpenProject.Text = "&Open Project...";
			this.toolStripMenuItemOpenProject.Click += new System.EventHandler(this.toolStripMenuItemOpenProject_Click);
			// 
			// toolStripMenuItemCloseProject
			// 
			this.toolStripMenuItemCloseProject.Enabled = false;
			this.toolStripMenuItemCloseProject.Name = "toolStripMenuItemCloseProject";
			this.toolStripMenuItemCloseProject.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemCloseProject.Text = "&Close Project";
			this.toolStripMenuItemCloseProject.Click += new System.EventHandler(this.toolStripMenuItemCloseProject_Click);
			// 
			// toolStripMenuItemSaveProject
			// 
			this.toolStripMenuItemSaveProject.Enabled = false;
			this.toolStripMenuItemSaveProject.Name = "toolStripMenuItemSaveProject";
			this.toolStripMenuItemSaveProject.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemSaveProject.Text = "&Save Project";
			this.toolStripMenuItemSaveProject.ShortcutKeys = Keys.Control | Keys.S;
			this.toolStripMenuItemSaveProject.Click += new System.EventHandler(this.toolStripMenuItemSaveProject_Click);
			// 
			// toolStripMenuItemSaveProjectAs
			// 
			this.toolStripMenuItemSaveProjectAs.Enabled = false;
			this.toolStripMenuItemSaveProjectAs.Name = "toolStripMenuItemSaveProjectAs";
			this.toolStripMenuItemSaveProjectAs.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemSaveProjectAs.Text = "Save Project &As...";
			this.toolStripMenuItemSaveProjectAs.Click += new System.EventHandler(this.toolStripMenuItemSaveProjectAs_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(170, 6);
			// 
			// toolStripMenuItemFiles
			// 
			this.toolStripMenuItemFiles.Name = "toolStripMenuItemFiles";
			this.toolStripMenuItemFiles.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemFiles.Text = "&Files";
			// 
			// toolStripMenuItemHeaders
			// 
			this.toolStripMenuItemHeaders.Name = "toolStripMenuItemHeaders";
			this.toolStripMenuItemHeaders.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemHeaders.Text = "&Headers";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(170, 6);
			// 
			// toolStripMenuItemRecentFiles
			// 
			this.toolStripMenuItemRecentFiles.Enabled = false;
			this.toolStripMenuItemRecentFiles.Name = "toolStripMenuItemRecentFiles";
			this.toolStripMenuItemRecentFiles.SettingsKey = "recentFilesToolStripMenuItem";
			this.toolStripMenuItemRecentFiles.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemRecentFiles.Text = "R&ecent Files";
			this.toolStripMenuItemRecentFiles.MruMenuItemClicked += new Infralution.Controls.MruMenuItemClickedHandler(this.toolStripMenuItemRecentFiles_MruMenuItemClicked);
			// 
			// toolStripMenuItemRecentProjects
			// 
			this.toolStripMenuItemRecentProjects.Enabled = false;
			this.toolStripMenuItemRecentProjects.Name = "toolStripMenuItemRecentProjects";
			this.toolStripMenuItemRecentProjects.SettingsKey = "recentProjectsToolStripMenuItem";
			this.toolStripMenuItemRecentProjects.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemRecentProjects.Text = "&Recent Projects";
			this.toolStripMenuItemRecentProjects.MruMenuItemClicked += new Infralution.Controls.MruMenuItemClickedHandler(this.toolStripMenuItemRecentProjects_MruMenuItemClicked);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(170, 6);
			// 
			// toolStripMenuItemExit
			// 
			this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
			this.toolStripMenuItemExit.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemExit.Text = "E&xit";
			this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
			// 
			// toolStripMenuItemEdit
			// 
			this.toolStripMenuItemEdit.Enabled = false;
			this.toolStripMenuItemEdit.Name = "toolStripMenuItemEdit";
			this.toolStripMenuItemEdit.Size = new System.Drawing.Size(37, 20);
			this.toolStripMenuItemEdit.Text = "&Edit";
			this.toolStripMenuItemEdit.Visible = false;
			// 
			// toolStripMenuItemView
			// 
			this.toolStripMenuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAddWorkpad,
            this.toolStripSeparator5,
			this.toolStripMenuItemFileDetails,
			this.toolStripMenuItemFilesAndStreams,
            this.toolStripMenuItemFramePreview,
			this.toolStripMenuItemHeaderDetail,
			this.toolStripMenuItemHeaders2,
			this.toolStripMenuItemVideoKeyframes,
			this.toolStripSeparator10,
            this.toolStripMenuItemHex,
            this.toolStripSeparator6,
            this.toolStripMenuItemLogFile,
            this.toolStripSeparator7,
            this.toolStripMenuItemColumnChooser});
			this.toolStripMenuItemView.Name = "toolStripMenuItemView";
			this.toolStripMenuItemView.Size = new System.Drawing.Size(41, 20);
			this.toolStripMenuItemView.Text = "&View";
			// 
			// toolStripMenuItemAddWorkpad
			// 
			this.toolStripMenuItemAddWorkpad.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemAddWorkpad.Image")));
			this.toolStripMenuItemAddWorkpad.Name = "toolStripMenuItemAddWorkpad";
			this.toolStripMenuItemAddWorkpad.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemAddWorkpad.Text = "New &Workpad";
			this.toolStripMenuItemAddWorkpad.Click += new System.EventHandler(this.toolStripMenuItemAddWorkpad_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(215, 6);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(215, 6);
            // 
			// toolStripMenuItemFileDetails
            // 
            this.toolStripMenuItemFileDetails.Name = "toolStripMenuItemFileDetails";
            this.toolStripMenuItemFileDetails.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemFileDetails.Text = "&File Details";
			this.toolStripMenuItemFileDetails.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(FileAndStreamDetailsWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemFileDetails.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			//
			// toolStripMenuItemFilesAndStreams
			// 
			this.toolStripMenuItemFilesAndStreams.Name = "toolStripMenuItemFilesAndStreams";
			this.toolStripMenuItemFilesAndStreams.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemFilesAndStreams.Text = "&Files and Streams";
			this.toolStripMenuItemFilesAndStreams.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(FileTreeWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemFilesAndStreams.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			//
			// toolStripMenuItemFramePreview
			// 
			this.toolStripMenuItemFramePreview.Name = "toolStripMenuItemFramePreview";
			this.toolStripMenuItemFramePreview.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemFramePreview.Text = "&Frame Preview";
			this.toolStripMenuItemFramePreview.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(FramePreviewWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemFramePreview.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			//
			// toolStripMenuItemHeaderDetail
			// 
			this.toolStripMenuItemHeaderDetail.Name = "toolStripMenuItemHeaderDetail";
			this.toolStripMenuItemHeaderDetail.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemHeaderDetail.Text = "&Header Detail";
			this.toolStripMenuItemHeaderDetail.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(HeaderDetailWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemHeaderDetail.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			//
			// toolStripMenuItemHeaders2
			// 
			this.toolStripMenuItemHeaders2.Name = "toolStripMenuItemHeaders2";
			this.toolStripMenuItemHeaders2.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemHeaders2.Text = "&Headers";
			this.toolStripMenuItemHeaders2.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(HeaderPanelWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemHeaders2.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			//
			// toolStripMenuItemVideoKeyframes
			// 
			this.toolStripMenuItemVideoKeyframes.Name = "toolStripMenuItemVideoKeyframes";
			this.toolStripMenuItemVideoKeyframes.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemVideoKeyframes.Text = "&Video keyframes";
			this.toolStripMenuItemVideoKeyframes.Image = ((System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(VideoKeyframesWindow)).GetObject("$this.Icon")).ToBitmap();
			this.toolStripMenuItemVideoKeyframes.Click += new System.EventHandler(this.toolStripMenuItemViewWindow_Click);
			// 
			// toolStripMenuItemHex
			// 
			this.toolStripMenuItemHex.Checked = true;
			this.toolStripMenuItemHex.CheckOnClick = true;
			this.toolStripMenuItemHex.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemHex.Name = "toolStripMenuItemHex";
			this.toolStripMenuItemHex.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemHex.Text = "&Hex Display";
			this.toolStripMenuItemHex.CheckedChanged += new System.EventHandler(this.toolStripMenuItemHex_CheckedChanged);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(215, 6);
			// 
			// toolStripMenuItemLogFile
			// 
			this.toolStripMenuItemLogFile.Name = "toolStripMenuItemLogFile";
			this.toolStripMenuItemLogFile.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemLogFile.Text = "Log &File";
			this.toolStripMenuItemLogFile.Image = global::Defraser.GuiCe.Properties.Resources.log_text;
			this.toolStripMenuItemLogFile.Click += new System.EventHandler(this.toolStripMenuItemLogFile_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(215, 6);
			// 
			// toolStripMenuItemColumnChooser
			// 
			this.toolStripMenuItemColumnChooser.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemColumnChooser.Image")));
			this.toolStripMenuItemColumnChooser.Name = "toolStripMenuItemColumnChooser";
			this.toolStripMenuItemColumnChooser.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemColumnChooser.Text = "Headers &Column Chooser...";
			this.toolStripMenuItemColumnChooser.Click += new System.EventHandler(this.toolStripMenuItemColumnChooser_Click);
			// 
			// toolStripMenuItemProject
			// 
			this.toolStripMenuItemProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAdd,
            this.toolStripMenuItemDeleteFile,
            this.toolStripMenuItemStop,
            this.toolStripMenuItemProperties});
			this.toolStripMenuItemProject.Name = "toolStripMenuItemProject";
			this.toolStripMenuItemProject.Size = new System.Drawing.Size(53, 20);
			this.toolStripMenuItemProject.Text = "&Project";
			this.toolStripMenuItemProject.DropDownOpening += new System.EventHandler(this.toolStripMenuItemProject_DropDownOpening);
			// 
			// toolStripMenuItemAdd
			// 
			this.toolStripMenuItemAdd.Image = global::Defraser.GuiCe.Properties.Resources.page_white_quest_add;
			this.toolStripMenuItemAdd.Name = "toolStripMenuItemAdd";
			this.toolStripMenuItemAdd.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemAdd.Text = "&Add File...";
			this.toolStripMenuItemAdd.Click += new System.EventHandler(this.toolStripMenuItemAdd_Click);
			// 
			// toolStripMenuItemDeleteFile
			// 
			this.toolStripMenuItemDeleteFile.Enabled = false;
			this.toolStripMenuItemDeleteFile.Image = global::Defraser.GuiCe.Properties.Resources.page_white_quest_del;
			this.toolStripMenuItemDeleteFile.Name = "toolStripMenuItemDeleteFile";
			this.toolStripMenuItemDeleteFile.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemDeleteFile.Text = "&Delete File";
			this.toolStripMenuItemDeleteFile.Click += new System.EventHandler(toolStripMenuItemDelete_Click);
			// 
			// toolStripMenuItemStop
			// 
			this.toolStripMenuItemStop.Enabled = false;
			this.toolStripMenuItemStop.Image = global::Defraser.GuiCe.Properties.Resources.stop;
			this.toolStripMenuItemStop.Name = "toolStripMenuItemStop";
			this.toolStripMenuItemStop.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemStop.Text = "&Stop";
			this.toolStripMenuItemStop.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
			// 
			// toolStripMenuItemProperties
			// 
			this.toolStripMenuItemProperties.Name = "toolStripMenuItemProperties";
			this.toolStripMenuItemProperties.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemProperties.Text = "&Properties";
			this.toolStripMenuItemProperties.Click += new System.EventHandler(this.toolStripMenuItemProperties_Click);
			// 
			// toolStripMenuItemTools
			// 
			this.toolStripMenuItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemEditSendTo,
            this.toolStripMenuItemOptions,
			this.toolStripMenuItemDefaultCodecHeader,
			this.toolStripMenuItemReferenceHeaderDatabase,
			this.toolStripMenuItemProjectKeyframeOverview});
			this.toolStripMenuItemTools.Name = "toolStripMenuItemTools";
			this.toolStripMenuItemTools.Size = new System.Drawing.Size(44, 20);
			this.toolStripMenuItemTools.Text = "&Tools";
			// 
			// toolStripMenuItemEditSendTo
			// 
			this.toolStripMenuItemEditSendTo.Name = "toolStripMenuItemEditSendTo";
			this.toolStripMenuItemEditSendTo.Size = new System.Drawing.Size(176, 22);
			this.toolStripMenuItemEditSendTo.Text = "&Edit Send To List...";
			this.toolStripMenuItemEditSendTo.Click += new System.EventHandler(this.toolStripMenuItemEditSendTo_Click);
			// 
			// toolStripMenuItemOptions
			// 
			this.toolStripMenuItemOptions.Name = "toolStripMenuItemOptions";
			this.toolStripMenuItemOptions.Size = new System.Drawing.Size(176, 22);
			this.toolStripMenuItemOptions.Text = "&Options...";
			this.toolStripMenuItemOptions.Click += new System.EventHandler(this.toolStripMenuItemOptions_Click);
			// 
			// toolStripMenuItemDefaultCodecHeader
			// 
			this.toolStripMenuItemDefaultCodecHeader.Name = "toolStripMenuItemDefaultCodecHeader";
			this.toolStripMenuItemDefaultCodecHeader.Size = new System.Drawing.Size(176, 22);
            this.toolStripMenuItemDefaultCodecHeader.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDefaultCodecHeader.Image")));
			this.toolStripMenuItemDefaultCodecHeader.Text = "&Edit Default Codec Header...";
			this.toolStripMenuItemDefaultCodecHeader.Click += new System.EventHandler(this.toolStripMenuItemDefaultCodecHeader_Click);
			// 
			// toolStripMenuItemReferenceHeaderDatabase
			// 
			this.toolStripMenuItemReferenceHeaderDatabase.Name = "toolStripMenuItemReferenceHeaderDatabase";
			this.toolStripMenuItemReferenceHeaderDatabase.Size = new System.Drawing.Size(176, 22);
			this.toolStripMenuItemReferenceHeaderDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonReferenceHeaderDatabase.Image")));
			this.toolStripMenuItemReferenceHeaderDatabase.Text = "&Reference Header Database...";
			this.toolStripMenuItemReferenceHeaderDatabase.Click += new System.EventHandler(toolStripMenuItemReferenceHeaderDatabase_Click);
			// 
            // toolStripMenuItemProjectKeyframeOverview
			// 
			this.toolStripMenuItemProjectKeyframeOverview.Name = "toolStripMenuItemProjectKeyframeOverview";
			this.toolStripMenuItemProjectKeyframeOverview.Size = new System.Drawing.Size(176, 22);
            this.toolStripMenuItemProjectKeyframeOverview.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonProjectKeyframeOverview.Image")));
			this.toolStripMenuItemProjectKeyframeOverview.Text = "&Project Keyframe Overview";
			this.toolStripMenuItemProjectKeyframeOverview.Click += new System.EventHandler(this.toolStripMenuItemProjectKeyframeOverview_Click);
			// 
			// toolStripMenuItemHelp
			// 
			this.toolStripMenuItemHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout});
			this.toolStripMenuItemHelp.Name = "toolStripMenuItemHelp";
			this.toolStripMenuItemHelp.Size = new System.Drawing.Size(40, 20);
			this.toolStripMenuItemHelp.Text = "&Help";
			// 
			// toolStripMenuItemAbout
			// 
			this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
			this.toolStripMenuItemAbout.Size = new System.Drawing.Size(159, 22);
			this.toolStripMenuItemAbout.Text = "&About Defraser";
			this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
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
			this.toolStripContainer.ContentPanel.Controls.Add(this.dockPanel);
			this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(1036, 697);
			this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer.LeftToolStripPanelVisible = false;
			this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer.Name = "toolStripContainer";
			this.toolStripContainer.RightToolStripPanelVisible = false;
			this.toolStripContainer.Size = new System.Drawing.Size(1036, 768);
			this.toolStripContainer.TabIndex = 2;
			this.toolStripContainer.Text = "toolStripContainer";
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip1);
			// 
			// statusStrip
			// 
			this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar});
			this.statusStrip.Location = new System.Drawing.Point(0, 0);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1036, 22);
			this.statusStrip.TabIndex = 1;
			this.statusStrip.Text = "statusStrip";
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(1021, 17);
			this.toolStripStatusLabel.Spring = true;
			this.toolStripStatusLabel.Text = "Ready";
			this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripProgressBar
			// 
			this.toolStripProgressBar.Name = "toolStripProgressBar";
			this.toolStripProgressBar.Size = new System.Drawing.Size(200, 16);
			this.toolStripProgressBar.Visible = false;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAdd,
            this.toolStripButtonDelete,
            this.toolStripButtonStop,
            this.toolStripSeparator8,
            this.toolStripButtonNewWorkpad,
            this.toolStripSeparator9,
            this.toolStripButtonDefaultCodecHeader,
            this.toolStripButtonReferenceHeaderDatabase,
            this.toolStripButtonProjectKeyframeOverview,
            this.toolStripSeparator11,
            this.toolStripButtonShowColumnChooser});
			this.toolStrip1.Location = new System.Drawing.Point(3, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(222, 25);
			this.toolStrip1.TabIndex = 2;
			// 
			// toolStripButtonAdd
			// 
			this.toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonAdd.Image = global::Defraser.GuiCe.Properties.Resources.page_white_quest_add;
			this.toolStripButtonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAdd.Name = "toolStripButtonAdd";
			this.toolStripButtonAdd.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonAdd.Text = "Add";
			this.toolStripButtonAdd.ToolTipText = "Add File";
			this.toolStripButtonAdd.Click += new System.EventHandler(this.toolStripButtonAdd_Click);
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDelete.Enabled = false;
			this.toolStripButtonDelete.Image = global::Defraser.GuiCe.Properties.Resources.page_white_quest_del;
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonDelete.Text = "Delete File";
			this.toolStripButtonDelete.ToolTipText = "Delete File";
			this.toolStripButtonDelete.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
			// 
			// toolStripButtonStop
			// 
			this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonStop.Enabled = false;
			this.toolStripButtonStop.Image = global::Defraser.GuiCe.Properties.Resources.stop;
			this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonStop.Name = "toolStripButtonStop";
			this.toolStripButtonStop.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonStop.Text = "Stop";
			this.toolStripButtonStop.ToolTipText = "Stop";
			this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonNewWorkpad
			// 
			this.toolStripButtonNewWorkpad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonNewWorkpad.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNewWorkpad.Image")));
			this.toolStripButtonNewWorkpad.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNewWorkpad.Name = "toolStripButtonNewWorkpad";
			this.toolStripButtonNewWorkpad.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonNewWorkpad.Text = "toolStripButton1";
			this.toolStripButtonNewWorkpad.ToolTipText = "New Workpad";
			this.toolStripButtonNewWorkpad.Click += new System.EventHandler(this.toolStripButtonNewWorkpad_Click);
            // 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonDefaultCodecHeader
            // 
            this.toolStripButtonDefaultCodecHeader.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDefaultCodecHeader.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDefaultCodecHeader.Image")));
            this.toolStripButtonDefaultCodecHeader.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDefaultCodecHeader.Name = "toolStripButtonDefaultCodecHeader";
            this.toolStripButtonDefaultCodecHeader.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonDefaultCodecHeader.Text = "toolStripButton1";
            this.toolStripButtonDefaultCodecHeader.ToolTipText = "Edit Default Codec Header...";
            this.toolStripButtonDefaultCodecHeader.Click += new System.EventHandler(this.toolStripButtonDefaultCodecHeader_Click);
            // 
            // toolStripButtonProjectKeyframeOverview
            // 
            this.toolStripButtonProjectKeyframeOverview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonProjectKeyframeOverview.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonProjectKeyframeOverview.Image")));
            this.toolStripButtonProjectKeyframeOverview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonProjectKeyframeOverview.Name = "toolStripButtonProjectKeyframeOverview";
            this.toolStripButtonProjectKeyframeOverview.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonProjectKeyframeOverview.Text = "toolStripButton1";
            this.toolStripButtonProjectKeyframeOverview.ToolTipText = "Project Keyframe Overview";
            this.toolStripButtonProjectKeyframeOverview.Click += new System.EventHandler(this.toolStripButtonProjectKeyframeOverview_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonShowColumnChooser
			// 
			this.toolStripButtonShowColumnChooser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonShowColumnChooser.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonShowColumnChooser.Image")));
			this.toolStripButtonShowColumnChooser.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowColumnChooser.Name = "toolStripButtonShowColumnChooser";
			this.toolStripButtonShowColumnChooser.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonShowColumnChooser.Text = "Headers column chooser";
			this.toolStripButtonShowColumnChooser.ToolTipText = "Headers column chooser";
			this.toolStripButtonShowColumnChooser.Click += new System.EventHandler(this.toolStripButtonShowColumnChooser_Click);
			//
			// toolStripButtonReferenceHeaderDatabase
			//
			this.toolStripButtonReferenceHeaderDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonReferenceHeaderDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonReferenceHeaderDatabase.Image")));
			this.toolStripButtonReferenceHeaderDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonReferenceHeaderDatabase.Name = "toolStripButtonReferenceHeaderDatabase";
			this.toolStripButtonReferenceHeaderDatabase.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonReferenceHeaderDatabase.ToolTipText = "Reference Header Database";
			this.toolStripButtonReferenceHeaderDatabase.Click += new System.EventHandler(this.toolStripButtonReferenceHeaderDatabase_Click);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select the folder in which the separate files must be saved.";
			// 
			// openProjectDialog
			// 
			this.openProjectDialog.Filter = "project files (*.dpr)|*.dpr|All files (*.*)|*.*";
			this.openProjectDialog.Title = "Open an Existing Project";
			// 
			// MainForm
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1036, 768);
			this.Controls.Add(this.toolStripContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Defraser";
			((System.ComponentModel.ISupportInitialize)(this.fileTreeObject)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fileDetailTree)).EndInit();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer.ContentPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private readonly BackgroundFileScanner backgroundFileScanner;
		private readonly DockPanel dockPanel;
		private FileTreeWindow _fileTreeWindow;
		private readonly FileTreeObject fileTreeObject;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHelp;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddWorkpad;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTools;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEditSendTo;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.OpenFileDialog openProjectDialog;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFileDetails;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilesAndStreams;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFramePreview;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHeaderDetail;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHeaders2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVideoKeyframes;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHex;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOptions;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDefaultCodecHeader;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemReferenceHeaderDatabase;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProjectKeyframeOverview;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLogFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdit;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
		private System.Windows.Forms.ToolStripButton toolStripButtonStop;
		private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewProject;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenProject;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCloseProject;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProject;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAdd;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProperties;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStop;
		private HeaderPanelWindow _headerPanelWindow;
        private readonly HeaderPanel headerPanel;
		private HeaderDetailWindow _headerDetailWindow;
		private HeaderDetailTree headerDetailTree;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton toolStripButtonProjectKeyframeOverview;
        private System.Windows.Forms.ToolStripButton toolStripButtonDefaultCodecHeader;
	    private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumnChooser;
		private System.Windows.Forms.ToolStripButton toolStripButtonShowColumnChooser;
		private System.Windows.Forms.ToolStripButton toolStripButtonNewWorkpad;
		private System.Windows.Forms.ToolStripButton toolStripButtonReferenceHeaderDatabase;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private Infralution.Controls.MruToolStripMenuItem toolStripMenuItemRecentFiles;
		private Infralution.Controls.MruToolStripMenuItem toolStripMenuItemRecentProjects;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private FileAndStreamDetailsWindow _fileAndStreamDetailsWindow;
		private FileDetailTree fileDetailTree;
		private Infralution.Controls.VirtualTree.Column columnDetail;
		private Infralution.Controls.VirtualTree.Column columnDescription;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFiles;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHeaders;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveProject;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveProjectAs;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteFile;
	}
}
