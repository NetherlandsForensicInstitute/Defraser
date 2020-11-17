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
using System.IO;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.GuiCe.Properties;

namespace Defraser.GuiCe
{
	/// <summary>
	/// The <see cref="Form"/> for specifying the file and metadata for creating
	/// a new project.
	/// This form creates the project using the <see cref="ProjectManager"/>.
	/// </summary>
	public partial class CreateNewProjectForm : Form
	{
		/// <summary>The file extension for Defraser projects.</summary>
		private const string DefraserProjectFileExtension = ".dpr";

		private readonly ProjectManager _projectManager;
		private readonly IFormFactory _formFactory;

		#region Properties
		private string ProjectFileName
		{
			get { return textBoxProjectFileName.Text; }
			set { textBoxProjectFileName.Text = value; }
		}
		private string ProjectDirectory
		{
			get { return textBoxProjectDirectory.Text; }
			set { textBoxProjectDirectory.Text = value; }
		}
		private string ProjectDescription
		{
			get { return textBoxProjectDescription.Text; }
			set { textBoxProjectDescription.Text = value; }
		}
		private string InvestigatorName
		{
			get { return textBoxInvestigatorName.Text; }
			set { textBoxInvestigatorName.Text = value; }
		}
		private DateTime DateCreated { get; set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="CreateNewProjectForm"/>.
		/// </summary>
		/// <param name="projectManager">The <see cref="ProjectManager"/> for creating the project</param>
		/// <param name="formFactory">The factory for creating an <see cref="AdvancedDetectorConfiguration"/></param>
		public CreateNewProjectForm(ProjectManager projectManager, IFormFactory formFactory)
		{
			_projectManager = projectManager;
			_formFactory = formFactory;

			InitializeComponent();

			DateCreated = DateTime.Now;
		}

		private void CreateProject(string path)
		{
			try
			{
				_projectManager.CreateProject(path, InvestigatorName, DateCreated, ProjectDescription);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Create Project Failed",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#region Event handlers
		private void CreateNewProjectForm_Load(object sender, EventArgs e)
		{
			// ### Set default values in the text boxes ###

			// The creation date/time
			textBoxCreationDate.Text = DateCreated.ToLongDateString() + " - " + DateCreated.ToLongTimeString();

			// The name of the investigator
			if (!string.IsNullOrEmpty(Settings.Default.InvestigatorName))
			{
				InvestigatorName = Settings.Default.InvestigatorName;
			}
			else
			{
				InvestigatorName = Environment.UserName;
			}

			// The location of the project file
			if (!string.IsNullOrEmpty(Settings.Default.ProjectPath) && Directory.Exists(Settings.Default.ProjectPath))
			{
				// Present the last used folder
				ProjectDirectory = Settings.Default.ProjectPath;
			}
			else
			{
				// Present the user the 'My Documents' folder as a default
				ProjectDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}

		private void buttonAdvancedPluginConfiguration_Click(object sender, EventArgs e)
		{
			_formFactory.Create<AdvancedDetectorConfiguration>().ShowDialog(this);
		}

		private void buttonBrowse_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(ProjectDirectory) && Directory.Exists(ProjectDirectory))
			{
				folderBrowserDialog.SelectedPath = ProjectDirectory;
			}
			else
			{
				// There is no or no correct path in textBoxProjectFileLocation.Text; use MyDocuments as default
				ProjectDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			if (DialogResult.OK == folderBrowserDialog.ShowDialog(this))
			{
				ProjectDirectory = folderBrowserDialog.SelectedPath;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			// Check the file name
			if (string.IsNullOrEmpty(textBoxProjectFileName.Text))
			{
				MessageBox.Show("You did not specify a file name. Please specify a valid file name",
								Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return; // Do not execute 'DialogResult = DialogResult.OK;' to keep dialog open.
			}
			if (textBoxProjectFileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				MessageBox.Show("The file name contains invalid characters. Please specify a valid file name",
								Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return; // Do not execute 'DialogResult = DialogResult.OK;' to keep dialog open.
			}
			// Check the file location
			if (string.IsNullOrEmpty(ProjectDirectory))
			{
				MessageBox.Show("You did not specify a file location. Please specify a file location",
								Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return; // Do not execute 'DialogResult = DialogResult.OK;' to keep dialog open.
			}
			// Check the investigator name
			if (string.IsNullOrEmpty(InvestigatorName))
			{
				MessageBox.Show("You did not specify the investigator name. Please specify the name of the investigator",
								Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return; // Do not execute 'DialogResult = DialogResult.OK;' to keep dialog open.
			}

			// Write the content of the text boxes to the settings file
			Settings.Default.ProjectPath = ProjectDirectory;
			Settings.Default.InvestigatorName = InvestigatorName;
			Settings.Default.Save();

			// Combine the project path (directory) and file name and check if it already exists
			string projectPath = Path.Combine(ProjectDirectory, ProjectFileName);
			if (!projectPath.EndsWith(DefraserProjectFileExtension))
			{
				projectPath += DefraserProjectFileExtension;
			}
			if (File.Exists(projectPath))
			{
				if (DialogResult.No ==
					MessageBox.Show(string.Format("Do you want to overwrite the existing file '{0}'?", projectPath),
									"File Already Exists.", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
									MessageBoxDefaultButton.Button2))
				{
					DialogResult = DialogResult.Cancel;
					return;
				}
			}

			CreateProject(projectPath);

			DialogResult = DialogResult.OK;
		}
		#endregion Event handlers
	}
}
