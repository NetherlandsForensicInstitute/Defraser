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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public partial class HeaderSource : Form
	{
		#region PrivateClassDefines
		public class HeaderSourceItem
		{
			private HeaderSourceItem _parent;
			private readonly ArrayList _children = new ArrayList();
			private readonly string _name;
			private readonly IInputFile _inputFile;
			private readonly IDetector _detector;
			private readonly bool _allowSelection;

			public HeaderSourceItem(HeaderSourceItem parent, string name, bool allowSelection)
				: this(parent, name, null, null, allowSelection)
			{
				
			}

			public HeaderSourceItem(HeaderSourceItem parent, string name, IInputFile inputFile, IDetector detector, bool allowSelection)
			{
				_parent = parent;
				_name = name;
				_inputFile = inputFile;
				_detector = detector;
				_allowSelection = allowSelection;
				if (_parent != null)
				{
					parent._children.Add(this);
				}
			}

			public HeaderSourceItem Parent
			{
				get { return _parent; }
				set { _parent = value; }
			}

			public IList Children
			{
				get { return _children; }
			}

			public string Name
			{
				get { return _name; }
			}

			public bool AllowSelection
			{
				get { return _allowSelection; }
			}

			public IInputFile InputFile
			{
				get { return _inputFile; }
			}

			public IDetector Detector
			{
				get { return _detector; }
			}
		}
		#endregion PrivateClassDefines

		#region Properties
		/// <summary>
		/// DialogOpen is NOT thread-safe
		/// </summary>
		public static bool DialogOpen
		{
			get { return _dialogOpen; }
		}
		#endregion Properties

		private static bool _dialogOpen;
		private readonly IFormFactory _formFactory;
		private readonly FileTree _mainFileTree;
		private readonly FFmpegManager _ffmpegManager;

		public HeaderSource(IFormFactory formFactory, FileTree mainFileTree, FFmpegManager ffmpegManager)
		{
			_dialogOpen = true;
			_formFactory = formFactory;
			_mainFileTree = mainFileTree;
			_ffmpegManager = ffmpegManager;

			InitializeComponent();

			UpdateHeaderSourceMessage();
			AddFileAndStreamSourceToList();
		}

		private void AddFileAndStreamSourceToList()
		{
			IProject project = _mainFileTree.DataSource as IProject;
			if (project == null) return;

			HeaderSourceItem rootNode = new HeaderSourceItem(null, "root", false);
			IList<IInputFile> files = project.GetInputFiles();
			
            foreach(IInputFile file in files)
            {
				HeaderSourceItem fileNode = new HeaderSourceItem(rootNode, (new FileInfo(file.Name)).Name, false);

				IList<IDetector> detectors = project.GetDetectors(file);
				foreach (IDetector detector in detectors)
				{
					new HeaderSourceItem(fileNode, detector.Name, file, detector, true);
				}
            }
			fileAndStreamSourceTree.DataSource = rootNode;
		}

		private void UpdateFormEnableState()
		{
			fileAndStreamSourceTree.Enabled = false;
			databaseSourceList.Enabled = false;
			databaseSourceList.BackColor = BackColor;

			if (fileAndStreamRadio.Checked)
			{
				fileAndStreamSourceTree.Enabled = true;
			}
			else if (databaseRadio.Checked)
			{
				databaseSourceList.Enabled = true;
				databaseSourceList.BackColor = Color.White;
			}
			UpdateUseSourceState();
		}

		private void UpdateUseSourceState()
		{
			buttonUseSource.Enabled = ((fileAndStreamSourceTree.Enabled && fileAndStreamSourceTree.SelectedItem != null)
										|| (databaseSourceList.Enabled && databaseSourceList.SelectedItem != null));
		}

		private void UpdateHeaderSourceMessage()
		{
			if(_ffmpegManager.HeaderSource == null)
			{
				textboxCurrentHeaderSource.Text = "No custom header source has been set. The headers from the active file will be used during frame decoding.";
			}
			else
			{
				IResultNode customSource = _ffmpegManager.HeaderSource;
				
				string detectorName = string.Empty;
				foreach (IDetector detector in customSource.Detectors)
				{
                    if (detectorName != string.Empty)
                        detectorName += ", ";
					detectorName += detector.Name;
				}
				textboxCurrentHeaderSource.Text = string.Format("A custom header source is being used during frame decoding. File: {0}, Detector: {1}.", new FileInfo(customSource.InputFile.Name).Name, detectorName);
			}
		}

		private void UseFileAsSource(IInputFile inputFile, IDetector detector)
		{
			// Use the owner of the current window, this Form will be closed soon.
			SearchHeader searchHeader = _formFactory.Create<SearchHeader>();
			if(searchHeader.RunSearchHeader(inputFile, detector))
				searchHeader.ShowDialog(Owner);
		}

		private void ResetHeaderSource()
		{
			_ffmpegManager.HeaderSource = null;
			UpdateHeaderSourceMessage();
		}

		#region EventHandlers
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateFormEnableState();
		}

		private void buttonUseSource_Click(object sender, EventArgs e)
		{
			if (fileAndStreamSourceTree.Enabled)
			{
				HeaderSourceItem item = fileAndStreamSourceTree.SelectedItem as HeaderSourceItem;
				if (item != null) UseFileAsSource(item.InputFile, item.Detector);
			}
			else if(databaseSourceList.Enabled)
			{
				// TODO: Use database as source.
			}
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void headerSource_CheckedChanged(object sender, EventArgs e)
		{
			UpdateFormEnableState();
		}

		private void list_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUseSourceState();
		}

		private void fileAndStreamSourceTree_SelectionChanged(object sender, EventArgs e)
		{
			HeaderSourceItem item = fileAndStreamSourceTree.SelectedItem as HeaderSourceItem;
			if (item != null && !item.AllowSelection) fileAndStreamSourceTree.SelectedItem = null;

			UpdateUseSourceState();
		}

		private void buttonResetHeaderSource_Click(object sender, EventArgs e)
		{
			ResetHeaderSource();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			_dialogOpen = false;
		}
		#endregion EventHandlers
	}
}
