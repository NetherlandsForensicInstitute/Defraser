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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Defraser.DataStructures;
using Defraser.GuiCe.Properties;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	/// <summary>
	/// TODO: comment
	/// </summary>
	public sealed partial class Workpad : DocumentWindow
	{
		#region Events
		internal event EventHandler<EventArgs> NameChanged;
		#endregion Events

		/// <summary>The default sort column name for new workpads.</summary>
		private const string DefaultSortColumnName = "Offset";
		/// <summary>The default <see cref="ListSortDirection"/> for new workpads.</summary>
		private const ListSortDirection DefaultSortDirection = ListSortDirection.Ascending;

		private static int _workpadInstanceCount;

		private readonly BackgroundDataBlockScanner _backgroundDataBlockScanner;
		private readonly Creator<RenameForm,Workpad> _renameformCreator;
		private string _workpadName;

		#region Properties
        public string WorkpadName
        {
            get
            {
            	return _workpadName;
            }
			set
			{
				_workpadName = value;

				UpdateTitle();

				if (NameChanged != null)
				{
					NameChanged(this, EventArgs.Empty);
				}
			}
        }
        #endregion Properties

        /// <summary>
		/// Creates a new workpad.
		/// </summary>
        public Workpad(HeaderPanel headerPanel, BackgroundDataBlockScanner backgroundDataBlockScanner, Creator<RenameForm, Workpad> renameformCreator)
		{
			this.headerPanel = headerPanel;
			_backgroundDataBlockScanner = backgroundDataBlockScanner;
			_renameformCreator = renameformCreator;

			InitializeComponent();

			WorkpadName = "Workpad " + (++_workpadInstanceCount);

            headerPanel.HeaderSelectionChanged += new EventHandler<EventArgs>(headerPanel_HeaderSelectionChanged);

			toolStripMenuItemHexDisplay.CheckState = (Settings.Default.DisplayMode == DisplayMode.Hex) ? CheckState.Checked : CheckState.Unchecked;

        	headerPanel.RootResult = CreateRootResult();
			headerPanel.SetColumns(null);
			headerPanel.SortColumnName = DefaultSortColumnName;
			headerPanel.SortColumnDirection = DefaultSortDirection;
		}

		private static IResultNode CreateRootResult()
		{
			var unknownMetadata = new Metadata(CodecID.Unknown, Enumerable.Empty<IDetector>());
			return new RootResult(unknownMetadata, null);
		}

		private void headerPanel_HeaderSelectionChanged(object sender, EventArgs e)
	    {
	        UpdateTitle();
	    }

	    private void UpdateTitle()
        {
            Text = WorkpadName;

            if (headerPanel.SelectionCount > 0)
            {
                Text += string.Format(" ({0} in selection)", headerPanel.SelectionCount);
            }
        }

		/// <summary>
		/// Adds the given <paramref name="results"/>, including its children,
		/// to this Workpad.
		/// </summary>
		/// <param name="results">the results to add/copy</param>
		public void AddResults(IList<IResultNode> results)
		{
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}

			headerPanel.AddResults(results);
		}

		#region Event handlers
		private void toolStripMenuItemFile_DropDownOpening(object sender, EventArgs e)
		{
			int index = 0;
			foreach (ToolStripItem toolStripItem in headerPanel.FileToolStripItems)
			{
				toolStripMenuItemFile.DropDownItems.Insert(index++, toolStripItem);
			}
		}

		private void toolStripMenuItemClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripMenuItemEdit_DropDownOpening(object sender, EventArgs e)
		{
			toolStripMenuItemEdit.DropDownItems.Remove(toolStripSeparator2);
			toolStripMenuItemEdit.DropDownItems.AddRange(headerPanel.EditToolStripItems);
		}

		private void toolStripMenuItemEdit_DropDownClosed(object sender, EventArgs e)
		{
			// The 'DropDownOpening' event is not invoked for an empty menu when
			// using the keyboard combination [Alt]+[E].
			// Therefore, add a placeholder separator so that the edit menu can be
			// opened using the keyboard.
			toolStripMenuItemEdit.DropDownItems.Add(toolStripSeparator2);
		}

		private void toolStripMenuItemRenameWorkpad_Click(object sender, EventArgs e)
		{
		    var renameForm = _renameformCreator(this);
			renameForm.ShowDialog(this);
		}

		private void toolStripMenuItemHeaderColumnChooser_Click(object sender, EventArgs e)
		{
			headerPanel.ShowColumnChooser();
		}

		private void toolStripMenuItemHexDisplay_CheckStateChanged(object sender, EventArgs e)
		{
			headerPanel.DisplayMode = toolStripMenuItemHexDisplay.CheckState == CheckState.Checked ? DisplayMode.Hex : DisplayMode.Dec;
		}
		#endregion Event handlers
	}
}
