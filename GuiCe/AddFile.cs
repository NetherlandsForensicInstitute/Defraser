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
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	public partial class AddFile : Form
	{
		private readonly IProject _project;
		private readonly IReferenceHeaderDatabase _referenceHeaderDatabase;

		#region Properties
		/// <summary>The selected container detectors.</summary>
		public IList<IDetector> ContainerDetectorSelection { get { return containerDetectorSelector.SelectedDetectors; } }
		/// <summary>The selected codec detectors.</summary>
		public IList<IDetector> CodecDetectorSelection { get { return codecDetectorSelector.SelectedDetectors; } }
		/// <summary>The files selected to be scanned.</summary>
		public IEnumerable<string> SelectedInputFiles
		{
			get
			{
				if (listBoxFiles == null) return Enumerable.Empty<string>();

				// Check for and avoid duplicate files
				return listBoxFiles.Items.OfType<string>().Distinct();
			}
		}

		public bool StartKeyframeOverviewAfterFileScanner
		{
			get { return StartKeyframeOverviewCheckBox.Checked; }
		}
		#endregion Properties

		public AddFile(IProject project, IDetectorFactory detectorFactory, IReferenceHeaderDatabase referenceHeaderDatabase)
		{
			PreConditions.Argument("project").Value(project).IsNotNull();

			_project = project;
			_referenceHeaderDatabase = referenceHeaderDatabase;

			InitializeComponent();

			dataGridView1.IncludedColumnVisible = true;
			dataGridView1.IncludedHeadersChanged += dataGridView1_IncludedHeadersChanged;

			containerDetectorSelector.AvailableDetectors = detectorFactory.ContainerDetectors;
			codecDetectorSelector.AvailableDetectors = detectorFactory.CodecDetectors;

			// If the H.264 detector is missing, display text on how it can be licensed.
			label4.Visible = (MainForm.EditionType == MainForm.Edition.Free);
		}

		internal void AddInitialFiles(IEnumerable<string> fileNames)
		{
			listBoxFiles.Items.AddRange(fileNames.ToArray());
		}

		/// <summary>
		/// Runs the file scanner after closing this dialog.
		/// Start a scan on the selected file(s)
		/// </summary>
		private void Run()
		{
			if (ContainerDetectorSelection.Count == 0 && CodecDetectorSelection.Count == 0)
			{
				MessageBox.Show("There is no container and no codec detector selected to scan the files. Please select one or more container and codec detectors.",
					"Select a detector",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				this.DialogResult = DialogResult.None;	// Keep the dialog open
				return;
			}

			if ((SelectedInputFiles == null) || (SelectedInputFiles.Count() == 0))
			{
				MessageBox.Show("There are no files selected. Please select one ore more files to scan.",
					"Select a file",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				this.DialogResult = DialogResult.None;	// Keep the dialog open
				return;
			}

			// Check for files that are already scanned
			IEnumerable<IInputFile> filesAlreadyScanned = FilesAlreadyScanned();
			if (filesAlreadyScanned.Count() > 0)
			{
				string message = "These files are already scanned by some of the selected plug-ins:" + Environment.NewLine + Environment.NewLine;
				foreach (IInputFile inputFile in filesAlreadyScanned)
				{
					message += inputFile.Name + Environment.NewLine;
				}
				message += Environment.NewLine + "When you press OK, they will be deleted and rescanned.";
				message += Environment.NewLine + "Note that all open Workpads will be closed.";
				DialogResult dialogResult = MessageBox.Show(message,
					"Delete and rescan files",
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Exclamation);

				if (dialogResult == DialogResult.Cancel)
				{
					this.DialogResult = DialogResult.None;	// Keep the dialog open
					return;
				}

				CloseAllWorkpads();

				DeleteFiles(filesAlreadyScanned);
			}
			DialogResult = DialogResult.OK;
		}

		private static void CloseAllWorkpads()
		{
			for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
			{
				if (Application.OpenForms[i] is Workpad)
				{
					Application.OpenForms[i].Close();
				}
			}
		}

		private IEnumerable<IInputFile> FilesAlreadyScanned()
		{
			// return the intersection of _fileNames and inputFiles
			if (_project == null) return Enumerable.Empty<IInputFile>();

			return _project.GetInputFiles().Where(f => SelectedInputFiles.Contains(f.Name));
		}

		private void DeleteFiles(IEnumerable<IInputFile> inputFiles)
		{
			if (_project == null) return;

			// TODO: stop result scanner in FileTree

			// The ToArray() method is required when inputFiles is the result of a
			// LINQ query on the project's input files.
			foreach (IInputFile inputFile in inputFiles.ToArray())
			{
				_project.DeleteFile(inputFile);
			}
		}

		#region Event handlers
		private void buttonAdd_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				foreach (string fileName in openFileDialog.FileNames)
				{
					listBoxFiles.Items.Add(fileName);
				}
			}
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			ListBox.SelectedObjectCollection selectedObjectCollection = listBoxFiles.SelectedItems;
			while (selectedObjectCollection.Count > 0)
			{
				listBoxFiles.Items.Remove(selectedObjectCollection[0]);
			}
		}

		private void buttonRun_Click(object sender, EventArgs e)
		{
			Run();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			listBoxFiles.Items.Clear();

			DialogResult = DialogResult.Cancel;
		}

		private void codecDetectorSelector_SelectionChanged(object sender, EventArgs e)
		{
			var detector = (codecDetectorSelector.SelectedItem as ICodecDetector);
			dataGridView1.ReferenceHeaders = _referenceHeaderDatabase.ListHeaders(r => (detector != null) && detector.SupportedFormats.Contains(r.CodecParameters.Codec));
			dataGridView1.IncludedHeaders = ((detector == null) || (detector.ReferenceHeaders == null)) ? Enumerable.Empty<IReferenceHeader>() : detector.ReferenceHeaders;
		}

		private void dataGridView1_IncludedHeadersChanged(object sender, EventArgs e)
		{
			var detector = (codecDetectorSelector.SelectedItem as ICodecDetector);
			if (detector != null)
			{
				detector.ReferenceHeaders = dataGridView1.IncludedHeaders;
				codecDetectorSelector.UpdateRowData();
			}
		}
		#endregion Event handlers
	}
}
