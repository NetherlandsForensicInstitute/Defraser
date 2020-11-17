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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.GuiCe.Properties;
using Defraser.GuiCe.sendtolist;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;
using log4net.Config;
using WeifenLuo.WinFormsUI.Docking;
using System.Linq;

namespace Defraser.GuiCe
{
	public partial class MainForm : Form
	{

		/// <summary>The regular expression for locating the product name.</summary>
		private const string ProductNameRegex = @"^(\w+)\..*$";
		/// <summary>The regular expression for locating the 3-digit product version.</summary>
		private const string ProductVersionRegex = @"^(\d+.\d+.\d+)(.\d)$";

		private readonly IFormFactory _formFactory;
		private readonly ProjectManager _projectManager;
		private readonly WorkpadManager _workpadManager;
		private readonly FramePreviewManager _framePreviewManager;
	    private readonly VideoKeyframesManager _videoKeyframesManager;
		private readonly DefaultCodecHeaderManager _defaultCodecHeaderManager;
		private readonly IDetectorFactory _detectorFactory;
		private bool _startKeyframeOverviewAfterFileScanner;
		private readonly SendToList _sendToList;
		private readonly IReferenceHeaderDatabase _referenceHeaderDatabase;

		// TODO issue 1722 Support multiple open projects at once.
		private const string DisplayModeProperty = "DisplayMode";

		internal enum Edition
		{
			Free,
			Full
		}

		#region Properties
		/// <summary>The active open project.</summary>
		public IProject Project
		{
			get { return fileTreeObject.Project; }
			set { fileTreeObject.Project = value; }
		}

		public HeaderPanel HeaderPanel
		{
			get { return headerPanel; }
		}

		/// <summary>The detector plugin directory.</summary>
		internal static string DetectorPath
		{
			get { return ConfigurationManager.AppSettings.Get("Defraser.DetectorPath") ?? Application.StartupPath; }
		}

		/// <summary>The application name, i.e. "Defraser".</summary>
		internal static string ApplicationName
		{
			get
			{
				Match match = Regex.Match(Application.ProductName, ProductNameRegex);
				return match.Success ? match.Result("$1") : Application.ProductName;
			}
		}

		private static string ApplicationFullName
		{
			get { return string.Format("{0} ({1}{2})", ApplicationName, EditionName, string.IsNullOrEmpty(TrialName) ? "" : " ~ " + TrialName); } 
		}

		/// <summary>The application version, e.g. "0.5.1".</summary>
		internal static string ApplicationVersion
		{
			get
			{
				var sb = new StringBuilder();
				Match match = Regex.Match(Application.ProductVersion, ProductVersionRegex);
				//return match.Success ? match.Result("$1") : Application.ProductVersion;
				sb.Append(match.Success ? match.Result("$1") : Application.ProductVersion);
				sb.Append(" (");

				if (IntPtr.Size == 8)
				{
					sb.Append("64-bit");
				}
				else
				{
					sb.Append("32-bit");
				}
				sb.Append(' ');
				sb.Append(EditionName);
				sb.Append(")");
				return sb.ToString();
			}
		}

		internal static Edition EditionType
		{
			get
			{
				// Check for the existance of the H.264 detector, which is only available in the full edition
				FileInfo[] files = new DirectoryInfo(DetectorPath).GetFiles("H264Detector.dll");
				return (files.Length == 0) ? Edition.Free : Edition.Full;
			}
		}

		private static string EditionName
		{
			get { return Enum.GetName(typeof(Edition), EditionType) + " Edition"; }
		}

		internal static string TrialName { get; set; }

		/// <summary>The (version-specific) Defraser application data directory.</summary>
		internal static string ApplicationDataPath
		{
			get
			{
				string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + ".0";
				string applicationDataPath = ApplicationDataBasePath + Path.DirectorySeparatorChar + version;

				// If a version of Defraser is started for the very first time, and closed immediately
				// after the start, the application path does not yet exist. So create it.
				if (!Directory.Exists(applicationDataPath))
				{
					Directory.CreateDirectory(applicationDataPath);
				}

				return applicationDataPath;
			}
		}

		private static string ApplicationDataBasePath
		{
			get
			{
				string applicationDataBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Defraser";

				// If Defraser is started for the very first time, and closed immediately after the start,
				// the application path does not yet exist. So create it.
				if (!Directory.Exists(applicationDataBasePath))
				{
					Directory.CreateDirectory(applicationDataBasePath);
				}

				return applicationDataBasePath;
			}
		}

		private static string ReferenceHeaderDatabasePath
		{
			get { return Path.Combine(ApplicationDataBasePath, "ReferenceHeaderDatabase.xml"); }
		}

		private static int CopyrightEndYear { get { return DateTime.Now.Year; } }
		#endregion Properties

		public MainForm(DockPanel dockPanel, FileTreeObject fileTreeObject, HeaderPanel headerPanel, HeaderDetailTree headerDetailTree, IFormFactory formFactory,
						ProjectManager projectManager, WorkpadManager workpadManager, FramePreviewManager framePreviewManager,
						VideoKeyframesManager videoKeyframesManager, DefaultCodecHeaderManager defaultCodecHeaderManager, BackgroundFileScanner backgroundFileScanner, 
						IDetectorFactory detectorFactory, SendToList sendToList, IReferenceHeaderDatabase referenceHeaderDatabase)
		{
			this.dockPanel = dockPanel;
			this.fileTreeObject = fileTreeObject;
			this.headerPanel = headerPanel;
			this.headerDetailTree = headerDetailTree;
			_formFactory = formFactory;
			_projectManager = projectManager;
			_workpadManager = workpadManager;
			_framePreviewManager = framePreviewManager;
		    _videoKeyframesManager = videoKeyframesManager;
			_defaultCodecHeaderManager = defaultCodecHeaderManager;
			_detectorFactory = detectorFactory;
			this.backgroundFileScanner = backgroundFileScanner;
			_sendToList = sendToList;
			_referenceHeaderDatabase = referenceHeaderDatabase;

			// Set (main) HeaderTree as source for VideoKeyframesWindow.
			_videoKeyframesManager.KeyframeSourceTree = headerPanel.HeaderTree;

			InitializeComponent();

			Settings.Default.PropertyChanged += Settings_PropertyChanged;

			fileTreeObject.FocusRowChanged += fileTree_FocusRowChanged;
			// Note: the method used to handle the FocusRowChanged event is also used to handle the
			//       SelectionChanged event. This is because the FocusRowChanged event does not get
			//       thrown when there is only one item in the tree.
			fileTreeObject.SelectionChanged += fileTree_FocusRowChanged;

			// Bind Enabled state of delete item in FileTree with delete button in MainForm
			toolStripButtonDelete.DataBindings().Add("Enabled", fileTreeObject, "DeleteEnabled");
			toolStripMenuItemDeleteFile.DataBindings().Add("Enabled", fileTreeObject, "DeleteEnabled");

			// Initialize logging
			XmlConfigurator.Configure();

			// Initialize Defraser framework
			_detectorFactory.Initialize(DetectorPath);

			// Initialize Codec Header Source manager (requires that the detectFactory has been initialized)
			_defaultCodecHeaderManager.Initialize();

			// Reload the recent projects from the user.config file.
			// The MruToolStripMenuItem should do this on its own,
			// but I can't get it to work with two MruToolStripMenuItems in one application.
			if (Settings.Default.RecentProjects != null)
			{
				toolStripMenuItemRecentProjects.ClearEntries();
				foreach (string recentProject in Settings.Default.RecentProjects)
				{
					toolStripMenuItemRecentProjects.AddEntry(recentProject);
				}
			}
			if (Settings.Default.RecentFiles != null)
			{
				toolStripMenuItemRecentFiles.ClearEntries();
				foreach (string recentFile in Settings.Default.RecentFiles)
				{
					toolStripMenuItemRecentFiles.AddEntry(recentFile);
				}
			}

			if (toolStripMenuItemRecentProjects.Entries.Length > 0)
			{
				toolStripMenuItemRecentProjects.Enabled = true;
			}

			fileTreeObject.Project = null;
			headerPanel.RootResult = null;

			Text = ApplicationFullName;
		}

		/// <summary>
		/// Inserts <paramref name="items"/> into a <paramref name="collection"/>
		/// of tool strip items after the item <paramref name="insertAfterItem"/>.
		/// </summary>
		/// <param name="collection">the collection to add the items to</param>
		/// <param name="items">the tool strip items to insert</param>
		/// <param name="insertAfterItem">the item after which to insert</param>
		public static void InsertToolStripItems(ToolStripItemCollection collection, ToolStripItem[] items, ToolStripItem insertAfterItem)
		{
			int index = collection.IndexOf(insertAfterItem);

			foreach (ToolStripItem toolStripItem in items)
			{
				collection.Insert(++index, toolStripItem);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (_detectorFactory.Detectors.Count == 0)
			{
				MessageBox.Show(string.Format("No detectors found in '{0}'.\nMenu option File/Add will be disabled.\nIs the detector path correctly configured in the config file?", DetectorPath),
						"No detectors", MessageBoxButtons.OK, MessageBoxIcon.Error);
				toolStripMenuItemAdd.Enabled = false;
				toolStripButtonAdd.Enabled = false;
			}
			if (File.Exists(ReferenceHeaderDatabasePath))
			{
				try
				{
					_referenceHeaderDatabase.Import(ReferenceHeaderDatabasePath);
				}
				catch (Exception)
				{
					// Ignored, new database will created upon exit!
				}
			}

			// When this bool stays false when have to create the default GUI.
			bool guiSuccessfullyCreated = false;

			// Reload Window Settings from configuration files
			// When both Control and Shift are down during startup this will be skipped. 
			// The application will start in its default settings, on application close the resetted settings will be saved.
			if (!((ModifierKeys & Keys.Control) == Keys.Control && (ModifierKeys & Keys.Shift) == Keys.Shift))
			{
				// Window state
				if (Settings.Default.IsMaximized)
				{
					WindowState = FormWindowState.Maximized;
				}

				// Check if there where any hardware/resolution etc. changes.
				// When there is a change detected, the GUI should be resetted.
				// Otherwise the window can be outside the monitors visible area.
				if (IsWindowDataValid())
				{
					// Restore the last size and position of the MainDialog.
					Location = new Point(Settings.Default.RestoreBounds.X, Settings.Default.RestoreBounds.Y);
					Size = new Size(Settings.Default.RestoreBounds.Width, Settings.Default.RestoreBounds.Height);

					//
					// Create GUI Windows
					//
					// Try to load settings and apply.
					string path = GetLayoutStorageFile(GetLayoutStorageDirectory());
					if (File.Exists(path))
					{
						CreateDefaultGui(false);

						try
						{
							dockPanel.LoadFromXml(path, new DeserializeDockContent(GetWindowContentFromPersistString));
							guiSuccessfullyCreated = true;
						}
						catch (Exception)
						{
							
						}
					}
				}
			}

			if (!guiSuccessfullyCreated)
			{
				// Add the windows an error occurred while reading from the XML file.
				// The windows were created but not added, that should have been done by the LoadFromXML.
				CreateDefaultGui(true);
			}

			headerPanel.SetColumns(null);

			if (Settings.Default.DeleteTempFilesAtApplicationStart)
			{
				TempFile.RemoveTempFiles();
			}

#if DEBUG
			// Make sure the names used to set the GUI are not changed
			// These names are used here and in the call back method for
			// the property changed event.
			List<string> names = new List<string>();
			foreach (SettingsProperty settingsProperty in Settings.Default.Properties)
			{
				names.Add(settingsProperty.Name);
			}
			names.Contains(DisplayModeProperty);
#endif // DEBUG

			// Update GUI to initial property values
			SetGuiUsingSettingsProperty(DisplayModeProperty);

			_projectManager.ProjectChanged += ProjectManager_ProjectChanged;
			_projectManager.PropertyChanged += ProjectManager_PropertyChanged;
		}

		private static bool IsWindowDataValid()
		{
			Screen screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName.IndexOf(Settings.Default.ActiveScreenName) >= 0);

			if (screen != null
			    && Screen.AllScreens.Length == Settings.Default.ScreenCount
			    && screen.WorkingArea.IntersectsWith(Settings.Default.RestoreBounds)
			    && Settings.Default.RestoreBounds.Width > 0
			    && Settings.Default.RestoreBounds.Height > 0)
			{
				return true;
			}
			return false;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// Note close project can store the project
			if (!CloseProject())
			{
				e.Cancel = true;
				return;
			}

			// Save screen count/name, prevent form from being created at an non-existing screen when screencount changes on next start.
			Settings.Default.ScreenCount = (uint)Screen.AllScreens.Length;
			Settings.Default.ActiveScreenName = Regex.Match(Screen.FromControl(this).DeviceName, "[A-Za-z]{7}[0-9]{1,}").Value;

			// Write the currect size and position of the main dialog
			if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
			{
				if (WindowState == FormWindowState.Maximized)
					Settings.Default.RestoreBounds = RestoreBounds;
				else
					Settings.Default.RestoreBounds = new Rectangle(Location, Size);

				Settings.Default.IsMaximized = (WindowState == FormWindowState.Maximized);
			}

			// First close all workpads, we don't want to save them in the layout
			_workpadManager.CloseAllWorkpads();

			// Write the layout settings to a file
			try
			{
				string directory = GetLayoutStorageDirectory();
				if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
				dockPanel.SaveAsXml(GetLayoutStorageFile(directory));
			}
			catch (Exception)
			{
				// Layout save error
			}

			// Save the recent files to the user.config file.
			// The MruToolStripMenuItem should do this by its own,
			// but I can't get it to work.
			Settings.Default.RecentProjects = new StringCollection();
			Settings.Default.RecentProjects.AddRange(toolStripMenuItemRecentProjects.Entries);

			Settings.Default.RecentFiles = new StringCollection();
			Settings.Default.RecentFiles.AddRange(toolStripMenuItemRecentFiles.Entries);

			Settings.Default.Save();

			_referenceHeaderDatabase.Export(ReferenceHeaderDatabasePath);
		}

		private String PathVar(string pathVar)
		{
			pathVar = pathVar.Replace(' ', '_');
			return pathVar.Substring(0, (pathVar.Length < 25) ? pathVar.Length : 25);
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);

			// Show a copy icon next to the cursor when a file is being dragged.
			if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
				!DirectoriesOnly((string[])e.Data.GetData(DataFormats.FileDrop)))
				e.Effect = DragDropEffects.Copy;
			else if (e.Data.GetDataPresent(typeof(Column)))
				e.Effect = DragDropEffects.Move;
			else
				e.Effect = DragDropEffects.None;
		}

		private static bool DirectoriesOnly(IEnumerable<string> directories)
		{
			foreach (string directory in directories)
			{
				if (!Directory.Exists(directory)) return false;
			}
			return true;
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);

			// Handle dragged and dropped files
			if (_detectorFactory.Detectors.Count == 0 || !e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				return;
			}

			// Get the names of the dragged and dropped files
			string[] dragAndDropFileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

			// Open the detector dialog to let the user select a detector for
			// the dragged file (simulate the File/Add menu selection)
			if (dragAndDropFileNames.Length > 0)
			{
				List<string> dragAndDropInputFiles = new List<string>();
				foreach (string dragAndDropFileName in dragAndDropFileNames)
				{
					if (File.Exists(dragAndDropFileName)) // Add files only, ignore directories
					{
						dragAndDropInputFiles.Add(dragAndDropFileName);
					}
				}

				AddFilesUsingDialog(dragAndDropInputFiles.ToArray());
			}
		}

		private static string GetAppenderName(string appenderName)
		{
			log4net.Repository.ILoggerRepository loggerRepository = log4net.LogManager.GetRepository();

			foreach (log4net.Appender.IAppender appender in loggerRepository.GetAppenders())
			{
				log4net.Appender.FileAppender fileAppender = appender as log4net.Appender.FileAppender;

				if ((appender.Name == appenderName) && (fileAppender != null))
				{
					return fileAppender.File;
				}
			}
			return string.Empty;
		}

		private void SetApplicationTitle(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				Text = ApplicationFullName;
			}
			else
			{
				Text = string.Format("{0} - {1}", Path.GetFileName(fileName), ApplicationFullName);

				toolStripMenuItemRecentProjects.AddEntry(fileName);
			}
		}

		private void SetModifiedFlagForProjectNameInTitleBar(bool modified)
		{
			if (modified)
			{
				if (!Text.Contains("*"))	// Not already set as modified
				{
					Text = Text.Replace(" - ", "* - ");
				}
			}
			else
			{
				Text = Text.Replace("* - ", " - ");
			}
		}

		/// <summary>
		/// Use the InputForm dialog to let the user select one or more files.
		/// </summary>
		/// <param name="initialFileNames">This file will be shown as default selection. Use null to keep te selection empty.</param>
		private void AddFilesUsingDialog(string[] initialFileNames)
		{
			// When there is no project, create or open a project.
			if (Project == null)
			{
				var openOrCreateProject = _formFactory.Create<OpenOrCreateProject>();
				// Note: providing the this pointer in the call to ShowDialog makes sure the
				// dialog is displayed on top of the main form and not on a second screen
				if (DialogResult.OK == openOrCreateProject.ShowDialog(this))
				{
					if (openOrCreateProject.Result == OpenOrCreateProjectResult.CreateNew)
					{
						CreateNewProject();
					}
					if (openOrCreateProject.Result == OpenOrCreateProjectResult.OpenExisting)
					{
						OpenProject();
					}
				}
			}
			if (Project == null || !StopFileScanner("The program is already scanning a file. Do you want to stop the current scan and start a new scan?"))
			{
				return;
			}

			AddFile addFile = _formFactory.Create<AddFile>();
			if (initialFileNames != null)
			{
				addFile.AddInitialFiles(initialFileNames);
			}
			if ((DialogResult.OK == addFile.ShowDialog(this)) && CheckFilesExist(addFile.SelectedInputFiles))
			{
				try
				{
					backgroundFileScanner.ScanFiles(addFile.ContainerDetectorSelection, addFile.CodecDetectorSelection,
					                                addFile.SelectedInputFiles, _projectManager.Project);
					_startKeyframeOverviewAfterFileScanner = addFile.StartKeyframeOverviewAfterFileScanner;
				}
				catch (FileNotFoundException e)
				{
					MessageBox.Show(this, string.Format("The file '{0}' does not exist. Please, select an existing file.", e.Message),
							"Invalid File Name",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private bool CheckFilesExist(IEnumerable<string> inputFiles)
		{
			if (inputFiles == null) return false;

			foreach (string fileName in inputFiles)
			{
				if (!File.Exists(fileName))
				{
					MessageBox.Show(this, string.Format("The file '{0}' does not exist. Please, select an existing file.",
							fileName), "Invalid File Name",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;
				}
			}
			return true;
		}

		private void ViewLogFile()
		{
			string logFileName = GetAppenderName("RollingLogFileAppender");

			if (!File.Exists(logFileName))
			{
				MessageBox.Show(this, string.Format("Log file '{0}' not found.", logFileName),
						"View log file", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				Process.Start(Settings.Default.ExternalLogViewerApplication, "\"" + logFileName + "\"");
			}
			catch (Exception)
			{
				MessageBox.Show(this, string.Format("Failed to view log file '{0}' using '{1}'",
							logFileName, Settings.Default.ExternalLogViewerApplication));
			}
		}

		/// <summary>
		/// Stops the file scanner if it is busy.
		/// </summary>
		/// <param name="message">the message/question to display if busy</param>
		/// <returns>whether the file scanner was stopped</returns>
		private bool StopFileScanner(string message)
		{
			if (!backgroundFileScanner.IsBusy)
			{
				return true;
			}

			DialogResult result = MessageBox.Show(this, message,
					"Scan in progress",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

			return (result == DialogResult.Yes) && backgroundFileScanner.Stop();
		}

		private void CreateNewProject()
		{
			if (!CloseProject())
			{
				return;
			}

			if (DialogResult.OK == _formFactory.Create<CreateNewProjectForm>().ShowDialog(this))
			{
				Project = _projectManager.Project;
			}
		}

		private void OpenProject()
		{
			if (DialogResult.OK == openProjectDialog.ShowDialog(this))
			{
				try
				{
					OpenProject(openProjectDialog.FileName);
				}
				catch (Exception exception)
				{
					MessageBox.Show(this, exception.Message,
							"Failed to open an existing project",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void OpenProject(string projectName)
		{
			if (!File.Exists(projectName))
			{
				MessageBox.Show(this, string.Format("The project file '{0}' does not exist.", projectName),
						ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				toolStripMenuItemRecentProjects.RemoveEntry(projectName);
				return;
			}
			if (!CloseProject())
			{
				return;
			}

			try
			{
				Project = _projectManager.OpenProject(projectName);
			}
			catch (Exception exception)
			{
				string message = exception.Message;
				if (exception.InnerException != null)
				{
					message += Environment.NewLine;
					message += exception.InnerException.Message;
				}
				MessageBox.Show(this, message,
						string.Format("Failed to open project '{0}'", projectName),
						MessageBoxButtons.OK, MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1);

				toolStripMenuItemRecentProjects.RemoveEntry(projectName);
			}
		}

		/// <summary>Closes the current project if open.</summary>
		/// <returns>true if closed or not open, false if the project remains open</returns>
		private bool CloseProject()
		{
			if (Project == null)
			{
				return true;
			}
			try
			{
				if (Project.Dirty)
				{
					string warning = backgroundFileScanner.IsBusy ? "Warning: there is a scan in progress that will be terminated.\n\r" : string.Empty;

					DialogResult result = MessageBox.Show(this,
							string.Format("{0}Do you want to save the changes to {1}?", warning, Project.FileName),
							ApplicationName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

					if (result == DialogResult.Cancel)
					{
						return false;
					}
					if (backgroundFileScanner.IsBusy)
					{
						backgroundFileScanner.Stop();
					}
					if (result == DialogResult.Yes)
					{
						_projectManager.SaveProject(Project);
					}
				}

				_projectManager.CloseProject(Project);
				Project = null;
				return true;
			}
			catch (Exception exception)
			{
				MessageBox.Show(this, exception.Message,
						"Failed to close the project",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		private void SaveProject()
		{
			if (Project == null) return;
			_projectManager.SaveProject(Project);
		}

		private void SaveProjectAs()
		{
			if (Project == null) return;

			var saveFileDialog = new SaveFilePresenter();
			saveFileDialog.FilterProjectFiles();
			saveFileDialog.InitialDirectory = Path.GetDirectoryName(Project.FileName);

			FileInfo projectFile;
			if (saveFileDialog.ShowDialog(this, out projectFile))
			{
				Project.FileName = projectFile.FullName;
				_projectManager.SaveProject(Project);
			}
		}

		private void ShowProjectProperties()
		{
			try
			{
				_formFactory.Create<ProjectProperties>().ShowDialog(this);
			}
			catch (Exception exception)
			{
				string message = exception.Message;

				if (exception.InnerException != null)
				{
					message += Environment.NewLine + exception.InnerException.Message;
				}

				MessageBox.Show(this, message, "Failed to read Project Properties",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateSelectedFileTreeRowCountInTitleBar()
		{
			if (_fileTreeWindow == null) return;
			_fileTreeWindow.Text = "Files and Streams";

			int selectionCount = fileTreeObject.SelectedItems.Count;
			if (selectionCount > 0)
			{
				_fileTreeWindow.Text += string.Format(" ({0} in selection)", selectionCount);
			}
		}

		private void SetGuiUsingSettingsProperty(string propertyName)
		{
			switch (propertyName)
			{
				case DisplayModeProperty:
					DisplayMode displayMode = Settings.Default.DisplayMode;
					headerPanel.DisplayMode = displayMode;
					fileTreeObject.DisplayMode = displayMode;
					toolStripMenuItemHex.Checked = (Settings.Default.DisplayMode == DisplayMode.Hex);
					break;
			}
		}

		private string GetLayoutStorageDirectory()
		{
			return string.Format(@"{0}/{1}/{2}/{3}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PathVar(Application.CompanyName), PathVar(Application.ProductName), Application.ProductVersion);
		}

		private string GetLayoutStorageFile(string directory)
		{
			return string.Format("{0}/layout.xml", directory);
		}

		private IDockContent GetWindowContentFromPersistString(string persistString)
		{
			if (persistString == typeof (HeaderDetailWindow).ToString())
				return _headerDetailWindow;
			if (persistString == typeof (FileTreeWindow).ToString())
				return _fileTreeWindow;
			if (persistString == typeof (FileAndStreamDetailsWindow).ToString())
				return _fileAndStreamDetailsWindow;
			if (persistString == typeof (FramePreviewWindow).ToString())
				return _framePreviewManager.GetPreviewWindow();
			if (persistString == typeof (HeaderPanelWindow).ToString())
				return _headerPanelWindow;
			if (persistString == typeof (VideoKeyframesWindow).ToString())
				return _videoKeyframesManager.GetThumbsWindow();
			return null;
		}

		#region GUIOperations
		private void CreateDefaultGui(bool add)
		{
			CreateHeaderDetailTreeWindow(add);
			CreateFileTreeWindow(add);
			CreateFileDetailWindow(add);
			CreateFramePreviewWindow(add);
			CreateHeaderPanelWindow(add);
			CreateThumbsPreviewWindow(add);
		}

		public void ResetGuiToDefaults()
		{
			CloseGuiWindows();
			CreateDefaultGui(true);
		}

		private void CreateFileTreeWindow(bool add)
		{
			if (_fileTreeWindow == null)
			{
				_fileTreeWindow = new FileTreeWindow();
				_fileTreeWindow.FormClosed += new FormClosedEventHandler(FileTreeWindowClosed);
				UpdateSelectedFileTreeRowCountInTitleBar();
				_fileTreeWindow.Controls.Add(fileTreeObject);
			}

			if (add)
			{
				if (_fileAndStreamDetailsWindow != null
					&& _fileAndStreamDetailsWindow.Pane != null
					&& _fileAndStreamDetailsWindow.DockState != DockState.DockBottomAutoHide
					&& _fileAndStreamDetailsWindow.DockState != DockState.DockLeftAutoHide
					&& _fileAndStreamDetailsWindow.DockState != DockState.DockRightAutoHide
					&& _fileAndStreamDetailsWindow.DockState != DockState.DockTopAutoHide)
				{
					_fileTreeWindow.Show(_fileAndStreamDetailsWindow.Pane, DockAlignment.Top, 0.5);
				}
				else
				{
					_fileTreeWindow.Show(dockPanel, DockState.DockLeft);
				}
			}
		}

		private void CreateFileDetailWindow(bool add)
		{
			if (_fileAndStreamDetailsWindow == null)
			{
				_fileAndStreamDetailsWindow = new FileAndStreamDetailsWindow();
				_fileAndStreamDetailsWindow.FormClosed += new FormClosedEventHandler(FileDetailWindowClosed);
				_fileAndStreamDetailsWindow.Controls.Add(fileDetailTree);
			}

			if (add)
			{
				if (_fileTreeWindow != null
					&& _fileTreeWindow.Pane != null
					&& _fileTreeWindow.DockState != DockState.DockBottomAutoHide
					&& _fileTreeWindow.DockState != DockState.DockLeftAutoHide
					&& _fileTreeWindow.DockState != DockState.DockRightAutoHide
					&& _fileTreeWindow.DockState != DockState.DockTopAutoHide)
				{
					_fileAndStreamDetailsWindow.Show(_fileTreeWindow.Pane, DockAlignment.Bottom, 0.5);
				}
				else
				{
					_fileAndStreamDetailsWindow.Show(dockPanel, DockState.DockLeft);
				}
			}
		}

		/// <summary>
		/// Window creation is managed by FramePreviewManager.
		/// </summary>
		private void CreateFramePreviewWindow(bool add)
		{
			if (!_framePreviewManager.IsOpen() && add)
			{
				FramePreviewWindow framePreviewWindow = _framePreviewManager.GetPreviewWindow();

				if (_fileAndStreamDetailsWindow != null
					&& _fileAndStreamDetailsWindow.Pane != null
                    && _fileAndStreamDetailsWindow.DockState != DockState.DockBottomAutoHide
                    && _fileAndStreamDetailsWindow.DockState != DockState.DockLeftAutoHide
                    && _fileAndStreamDetailsWindow.DockState != DockState.DockRightAutoHide
                    && _fileAndStreamDetailsWindow.DockState != DockState.DockTopAutoHide)
				{
					framePreviewWindow.Show(_fileAndStreamDetailsWindow.Pane, _fileAndStreamDetailsWindow);
				}
				else
				{
					framePreviewWindow.Show(dockPanel);
				}
			}
		}

		public void CreateHeaderDetailTreeWindow(bool add)
		{
			if (_headerDetailWindow == null)
			{
				_headerDetailWindow = new HeaderDetailWindow();
				_headerDetailWindow.FormClosed += new FormClosedEventHandler(HeaderDetailTreeWindowClosed);
				_headerDetailWindow.Controls.Add(headerDetailTree);
			}

			if (add)
			{
				_headerDetailWindow.Show(dockPanel, DockState.DockBottom);
			}
		}

		private void CreateHeaderPanelWindow(bool add)
		{
			if (_headerPanelWindow == null)
			{
                _headerPanelWindow = new HeaderPanelWindow(headerPanel);
				_headerPanelWindow.FormClosed += new FormClosedEventHandler(HeaderPanelWindowClosed);
				_headerPanelWindow.Controls.Add(headerPanel);
			}

			if (add)
			{
				_headerPanelWindow.Show(dockPanel, DockState.Document);
			}
		}

		/// <summary>
		/// Window creation is managed by VideoKeyframesManager.
		/// </summary>
		private void CreateThumbsPreviewWindow(bool add)
		{
			if (!_videoKeyframesManager.IsOpen() && add)
			{
				_videoKeyframesManager.GetThumbsWindow().Show(dockPanel);
			}
		}

		private void CreateNewWorkpadWindow()
		{
			Workpad newWorkpad = _workpadManager.CreateWorkpad();
			if (_headerPanelWindow != null)
			{
				newWorkpad.Show(_headerPanelWindow.Pane, null);
			}
			else
			{
				newWorkpad.Show(dockPanel, DockState.Document);
			}
		}

		private void CloseGuiWindows()
		{
			_videoKeyframesManager.CloseWindow();
			if(_headerPanelWindow != null) _headerPanelWindow.Close();
			if (_headerDetailWindow != null) _headerDetailWindow.Close();
			_framePreviewManager.CloseWindow();
			if (_fileAndStreamDetailsWindow != null) _fileAndStreamDetailsWindow.Close();
			if (_fileTreeWindow != null) _fileTreeWindow.Close();
		}

		// Note: Info about this window is not stored (in GUI configuration file).
		private void CreateProjectKeyframeOverview()
		{
			if (!ProjectKeyframeOverview.WindowOpen)
			{
				_formFactory.Create<ProjectKeyframeOverview>().Show();
			}
			else
			{
				ProjectKeyframeOverview.ActiveInstance.Focus();
				ProjectKeyframeOverview.ActiveInstance.RestartKeyframeScan();
			}
		}
		#endregion GUIOperations

		#region Event handlers
		private void ProjectManager_ProjectChanged(object sender, ProjectChangedEventArgs e)
		{
			switch (e.Type)
			{
				case ProjectChangedType.Closed:
					SetApplicationTitle(string.Empty);
					break;
				case ProjectChangedType.Created:
					SetApplicationTitle(((IProject)e.Item).FileName);
					break;
				case ProjectChangedType.DataBlockAdded:
					break;
				case ProjectChangedType.DataBlockDeleted:
					break;
				case ProjectChangedType.FileAdded:
					break;
				case ProjectChangedType.FileDeleted:
					// Handle the situation that all selected items got removed.
					// This is not handled by the code that updates the enabled
					// property when file tree items get (de)selected.
					if(fileTreeObject.RootRow.NumChildren == 0)
					{
						toolStripMenuItemDeleteFile.Enabled = false;
						toolStripButtonDelete.Enabled = false;
					}
					break;
				case ProjectChangedType.FileNameChanged:
					SetApplicationTitle(e.Item as string);
					break;
				case ProjectChangedType.MetadataChanged:
					break;
				case ProjectChangedType.Opened:
					SetApplicationTitle(((IProject)e.Item).FileName);
					break;
				case ProjectChangedType.VisibleColumnsChanged:
					break;
			}
		}

		void ProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IProject project = sender as IProject;
			if(project == null) return;

			if (e.PropertyName == Framework.Project.DirtyProperty)
			{
				SetModifiedFlagForProjectNameInTitleBar(((IProject)sender).Dirty);
			}
		}

		private void FocusWorkpad(object sender, EventArgs e)
		{
			if (sender == null || sender.GetType() != typeof(ToolStripMenuItem))
			{
				return;
			}

			Workpad workpad = _workpadManager.GetWorkpad(((ToolStripMenuItem)sender).Text);
			if (workpad != null)
			{
				workpad.Focus();
			}
		}

		private void headerPanel_ColumnChooserVisibleChanged(object sender, EventArgs e)
		{
			toolStripButtonShowColumnChooser.Checked = headerPanel.ColumnChooserVisible;
		}

		private void fileTree_DataSourceChanged(object sender, EventArgs e)
		{
			UpdateSelectedFileTreeRowCountInTitleBar();

			toolStripMenuItemCloseProject.Enabled = (Project != null);
			toolStripMenuItemSaveProject.Enabled = (Project != null);
			toolStripMenuItemSaveProjectAs.Enabled = (Project != null);
		}

		private void fileTree_FocusRowResultsChanged(object sender, ResultsEventArgs e)
		{
			// Update the content of the header tree list (right side of the window)
			headerPanel.RootResult = e.Results;
		}

		private void fileTree_FocusRowChanged(object sender, EventArgs e)
		{
			UpdateSelectedFileTreeRowCountInTitleBar();

			Row focusRow = fileTreeObject.FocusRow;
			fileDetailTree.DataSource = (focusRow == null) ? null : focusRow.Item;
			if(_fileAndStreamDetailsWindow != null) _fileAndStreamDetailsWindow.Text = fileDetailTree.Title;
			headerPanel.RootResult = null;
		}

		private void toolStripMenuItemFile_DropDownOpening(object sender, EventArgs e)
		{
			// Add the items from the FileTree to the Files menu item
			toolStripMenuItemFiles.DropDownItems.AddRange(fileTreeObject.FileToolStripItems);
			// Add the items from the Header Panel to the Headers menu item
			toolStripMenuItemHeaders.DropDownItems.AddRange(headerPanel.FileToolStripItems);
		}

		private void toolStripMenuItemNewProject_Click(object sender, EventArgs e)
		{
			CreateNewProject();
		}

		private void toolStripMenuItemOpenProject_Click(object sender, EventArgs e)
		{
			OpenProject();
		}

		private void toolStripMenuItemCloseProject_Click(object sender, EventArgs e)
		{
			CloseProject();
		}

		private void toolStripMenuItemSaveProject_Click(object sender, EventArgs e)
		{
			SaveProject();
		}

		private void toolStripMenuItemSaveProjectAs_Click(object sender, EventArgs e)
		{
			SaveProjectAs();
		}

		private void toolStripMenuItemRecentFiles_MruMenuItemClicked(object sender, Infralution.Controls.MruMenuItemClickedEventArgs e)
		{
			if (File.Exists(e.Entry))
			{
				AddFilesUsingDialog(new[] { e.Entry });
			}
			else
			{
				string message = string.Format("Could not open the file '{0}'", e.Entry);
				MessageBox.Show(this, message, "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				toolStripMenuItemRecentFiles.RemoveEntry(e.Entry);
			}
		}

		private void toolStripMenuItemRecentProjects_MruMenuItemClicked(object sender, Infralution.Controls.MruMenuItemClickedEventArgs e)
		{
			OpenProject(e.Entry);
		}

		private void toolStripMenuItemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripMenuItemAddWorkpad_Click(object sender, EventArgs e)
		{
			CreateNewWorkpadWindow();
		}

        private void toolStripButtonDefaultCodecHeader_Click(object sender, EventArgs e)
        {
            _defaultCodecHeaderManager.CreateEditDefaultCodecHeaderWindow().ShowDialog(this);
        }

        private void toolStripButtonProjectKeyframeOverview_Click(object sender, EventArgs e)
        {
            CreateProjectKeyframeOverview();
        }

        private void toolStripMenuItemViewWindow_Click(object sender, EventArgs e)
        {
        	ToolStripMenuItem item = sender as ToolStripMenuItem;

			if (item == toolStripMenuItemFileDetails)
			{
				CreateFileDetailWindow(true);
			}
			else if (item == toolStripMenuItemFilesAndStreams)
			{
				CreateFileTreeWindow(true);
			}
			else if (item == toolStripMenuItemFramePreview)
			{
				CreateFramePreviewWindow(true);
			}
			else if (item == toolStripMenuItemHeaderDetail)
			{
				CreateHeaderDetailTreeWindow(true);
			}
			else if (item == toolStripMenuItemHeaders2)
			{
				CreateHeaderPanelWindow(true);	
			}
			else if (item == toolStripMenuItemVideoKeyframes)
			{
				CreateThumbsPreviewWindow(true);
			}
        }

		private void toolStripMenuItemHex_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.DisplayMode = toolStripMenuItemHex.Checked ? DisplayMode.Hex : DisplayMode.Dec;
		}

		private void toolStripMenuItemLogFile_Click(object sender, EventArgs e)
		{
			ViewLogFile();
		}

		private void toolStripMenuItemColumnChooser_Click(object sender, EventArgs e)
		{
			headerPanel.ToggleColumnChooser();
		}

		private void toolStripMenuItemProject_DropDownOpening(object sender, EventArgs e)
		{
			toolStripMenuItemProperties.Enabled = (Project != null);
		}

		private void toolStripMenuItemAdd_Click(object sender, EventArgs e)
		{
			AddFilesUsingDialog(null);
		}

		private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
		{
			fileTreeObject.DeleteSelection();
		}

		private void toolStripMenuItemStop_Click(object sender, EventArgs e)
		{
			backgroundFileScanner.Stop();
		}

		private void toolStripMenuItemProperties_Click(object sender, EventArgs e)
		{
			ShowProjectProperties();
		}

		private void toolStripMenuItemEditSendTo_Click(object sender, EventArgs e)
		{
			_formFactory.Create<SendToForm>().ShowDialog(this);
		}

		private void toolStripMenuItemOptions_Click(object sender, EventArgs e)
		{
			_formFactory.Create<Options>().ShowDialog(this);
		}

		private void toolStripMenuItemDefaultCodecHeader_Click(object sender, EventArgs e)
		{
			_defaultCodecHeaderManager.CreateEditDefaultCodecHeaderWindow().ShowDialog(this);
		}

		private void toolStripMenuItemReferenceHeaderDatabase_Click(object sender, EventArgs e)
		{
			ShowReferenceHeaderDatabase();
		}

		private void toolStripMenuItemProjectKeyframeOverview_Click(object sender, EventArgs e)
		{
			CreateProjectKeyframeOverview();
		}

		private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
		{
			using (AboutBox aboutbox = _formFactory.Create<AboutBox>())
			{
				aboutbox.StartPosition = FormStartPosition.CenterParent;
				aboutbox.ShowDialog(this);
			}
		}

		private void toolStripButtonAdd_Click(object sender, EventArgs e)
		{
			AddFilesUsingDialog(null);
		}

		private void toolStripButtonDelete_Click(object sender, EventArgs e)
		{
			fileTreeObject.DeleteSelection();
		}

		private void toolStripButtonStop_Click(object sender, EventArgs e)
		{
			backgroundFileScanner.Stop();
		}

		private void toolStripButtonNewWorkpad_Click(object sender, EventArgs e)
		{
			CreateNewWorkpadWindow();
		}

		private void toolStripButtonShowColumnChooser_Click(object sender, EventArgs e)
		{
			headerPanel.ToggleColumnChooser();
		}

		private void toolStripButtonReferenceHeaderDatabase_Click(object sender, EventArgs e)
		{
			ShowReferenceHeaderDatabase();
		}

		private void ShowReferenceHeaderDatabase()
		{
			if (!backgroundFileScanner.IsBusy)
			{
				using (var form = new ReferenceHeaderDatabaseForm())
				{
					form.Database = _referenceHeaderDatabase;
					form.CodecDetectors = _detectorFactory.CodecDetectors;
					form.ShowDialog(this);
				}
			}
		}

		private void BackgroundFileScanner_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			toolStripProgressBar.Value = Math.Max(0, Math.Min(100, e.ProgressPercentage));
		}

		private void BackgroundFileScanner_DoWork(object sender, DoWorkEventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke((Action)(() => BackgroundFileScanner_DoWork(sender, e)));
				return;
			}

			toolStripMenuItemSaveProject.Enabled = false;
			toolStripMenuItemSaveProjectAs.Enabled = false;
			toolStripMenuItemDefaultCodecHeader.Enabled = false;
		    toolStripButtonDefaultCodecHeader.Enabled = false;
			toolStripMenuItemProjectKeyframeOverview.Enabled = false;
			toolStripButtonStop.Enabled = true;
			toolStripMenuItemStop.Enabled = true;
			toolStripProgressBar.Visible = true;
		}

		private void BackgroundFileScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			fileTreeObject.UpdateRows(false);

			toolStripMenuItemSaveProject.Enabled = true;
			toolStripMenuItemSaveProjectAs.Enabled = true;
			toolStripMenuItemDefaultCodecHeader.Enabled = true;
            toolStripButtonDefaultCodecHeader.Enabled = true;
			toolStripMenuItemProjectKeyframeOverview.Enabled = true;
			toolStripButtonStop.Enabled = false;
			toolStripMenuItemStop.Enabled = false;
			toolStripProgressBar.Visible = false;
			toolStripStatusLabel.Text = "Ready";

			if (e.Error == null && !e.Cancelled && _startKeyframeOverviewAfterFileScanner)
			{
                CreateProjectKeyframeOverview();
			}
			_startKeyframeOverviewAfterFileScanner = false;

			if (e.Error != null)
			{
				MessageBox.Show(this, e.Error.Message,
						"Failed to complete scan",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void BackgroundFileScanner_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IInputFile inputFile = backgroundFileScanner.CurrentFile;
			if (inputFile == null) return;

			toolStripMenuItemRecentFiles.AddEntry(inputFile.Name);
			toolStripStatusLabel.Text = string.Format("Running detectors on file '{0}'...", inputFile.Name);
		}

		private void BackgroundFileScanner_Cancel(object sender, EventArgs e)
		{
			toolStripStatusLabel.Text = "Cancelling...";
		}

		private void BackgroundFileScanner_WaitForCancel(object sender, CancelEventArgs e)
		{
			if (Disposing) return;

			int seconds = backgroundFileScanner.WaitForCancelTime;

			if (DialogResult.Cancel == MessageBox.Show(this,
					string.Format("The program failed to abort the scan task in {0} seconds. Do you want to wait another {1} seconds?", seconds, seconds * 2),
					"Failed to abort the scan task", MessageBoxButtons.RetryCancel,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
				e.Cancel = true;
			}
		}

		private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SetGuiUsingSettingsProperty(e.PropertyName);
		}

		private void FileTreeWindowClosed(object sender, FormClosedEventArgs e)
		{
            // Remove the fileTree before it's being disposed by the framework
			_fileTreeWindow.Controls.Remove(fileTreeObject);

			_fileTreeWindow.FormClosed -= FileTreeWindowClosed;
			_fileTreeWindow = null;
		}

		private void FileDetailWindowClosed(object sender, FormClosedEventArgs e)
		{
            // Remove the fileDetailTree before it's being disposed by the framework
            _fileAndStreamDetailsWindow.Controls.Remove(fileDetailTree);

			_fileAndStreamDetailsWindow.FormClosed -= FileDetailWindowClosed;
			_fileAndStreamDetailsWindow = null;
		}

		private void HeaderPanelWindowClosed(object sender, FormClosedEventArgs e)
		{
			// Remove the HeaderPanel before it's being disposed by the framework
			_headerPanelWindow.Controls.Remove(headerPanel);

			_headerPanelWindow.FormClosed -= HeaderPanelWindowClosed;
			_headerPanelWindow = null;
		}

		private void HeaderDetailTreeWindowClosed(object sender, FormClosedEventArgs e)
		{
			// Remove the headerDetailTree before it's being disposed by the framework
			_headerDetailWindow.Controls.Remove(headerDetailTree);

			_headerDetailWindow.FormClosed -= HeaderDetailTreeWindowClosed;
			_headerDetailWindow = null;
		}
		#endregion Event handlers
	}
}
