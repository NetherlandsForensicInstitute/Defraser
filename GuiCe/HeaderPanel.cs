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
using Defraser.Interface;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Provides viewing and editing of headers.
	/// 
	/// The header tree displays the headers of a container stream or codec stream
	/// in a tree structure.
	/// 
	/// The header detail tree lists details (the attributes of an <c>IResultNode</c>)
	/// for the header corresponding to the focused row of the header tree.
	/// </summary>
	public partial class HeaderPanel : UserControl, IHeaderDetailSource
	{
		/// <summary>
		/// Occurs when the visibility of the column chooser has changed.
		/// </summary>
		public event EventHandler<EventArgs> ColumnChooserVisibleChanged;
        public event EventHandler<EventArgs> HeaderSelectionChanged;

		private readonly FramePreviewManager _framePreviewManager;
		private readonly VideoKeyframesManager _videoKeyframesManager;

		#region Properties
		public HeaderTree HeaderTree
		{
			get { return headerTree; }
		}

		public DisplayMode DisplayMode
		{
			get { return headerTree.DisplayMode; }
			set { headerTree.DisplayMode = value; }
		}
		public Column SortColumn
		{
			get
			{
				return headerTree.SortColumn;
			}
			set
			{
				headerTree.SortColumn = value;
				headerTree.SortColumn.SortDirection = SortColumnDirection;
			}
		}
		public string SortColumnName
		{
			get { return headerTree.SortColumnName; }
			set { headerTree.SortColumnName = value; }
		}
		public ListSortDirection SortColumnDirection
		{
			get { return headerTree.SortColumnDirection; }
			set { headerTree.SortColumnDirection = value; }
		}

	    public int SelectionCount
	    {
	        get
	        {
	            return headerTree.SelectedItems.Count;
	        }
	    }

		/// <summary>
		/// The read-only state of this header panel.
		/// Read-only header panels can not edit the headers.
		/// </summary>
		public bool ReadOnly
		{
			get { return !headerTree.AllowDrop; }
			set { headerTree.AllowDrop = !value; }
		}

		/// <summary>The file menu tool strip items.</summary>
		public ToolStripItem[] FileToolStripItems { get { return headerTree.FileToolStripItems; } }
		/// <summary>The edit menu tool strip items.</summary>
		public ToolStripItem[] EditToolStripItems { get { return headerTree.EditToolStripItems; } }
		/// <summary>Visibility of the column chooser.</summary>
		public bool ColumnChooserVisible { get { return columnChooser.Visible; } }

		/// <summary>
		/// The root node of the results to display.
		/// </summary>
		public IResultNode RootResult
		{
			get { return headerTree.DataSource as IResultNode; }
			set { headerTree.DataSource = value; }
		}

		public IResultNode FocusRowResult
		{
			get
			{
				if (headerTree.FocusRow == null) return null;

				return headerTree.FocusRow.Item as IResultNode;
			}
		}

		public ColumnList Columns { get { return headerTree.Columns; } }

		#endregion Properties

		public HeaderPanel(FramePreviewManager framePreviewManager, VideoKeyframesManager videoKeyframesManager,
			HeaderTree headerTree, HeaderDetailTree headerDetailTree, ColumnChooser columnChooser)
		{
			_framePreviewManager = framePreviewManager;
		    _videoKeyframesManager = videoKeyframesManager;
			this.headerTree = headerTree;
			this.headerDetailTree = headerDetailTree;
			this.columnChooser = columnChooser;

			columnChooser.HeaderPanel = this;

			InitializeComponent();

			// Add/replace custom menu items in original header context menu
			headerTree.HeaderContextMenu.Items.RemoveAt(headerTree.HeaderContextMenu.Items.Count - 1);
			headerTree.HeaderContextMenu.Items.Add(toolStripMenuItemColumnChooser);
			headerTree.HeaderContextMenu.Items.Add(toolStripMenuItemRemoveColumn);
			headerTree.HeaderContextMenu.Opening += new CancelEventHandler(headerTree_HeaderContextMenu_Opening);

			headerTree_SelectionChanged(this, EventArgs.Empty);
		}

	    /// <summary>
		/// Toggles visibility of the column chooser.
		/// Shows the column chooser when invisible, hides it when visible.
		/// </summary>
		public void ToggleColumnChooser()
		{
			if (!ColumnChooserVisible)
			{
				ShowColumnChooser();
			}
			else
			{
				HideColumnChooser();
			}
		}

		/// <summary>
		/// Shows the column chooser
		/// </summary>
		public void ShowColumnChooser()
		{
			if (!ColumnChooserVisible)
			{
				columnChooser.CreateColumns();
				columnChooser.Show(this); // By supplying the parent, it stays on top.
			}
		}

		/// <summary>
		/// Hides the column chooser
		/// </summary>
		public void HideColumnChooser()
		{
			if (ColumnChooserVisible)
			{
				columnChooser.Hide();
			}
		}

		public void SetColumns(IDetector detector)
		{
			columnChooser.SetColumns(detector, headerTree.IsWorkpad);
		    _videoKeyframesManager.ClearWindow();

		    IResultNode resultNode = headerTree.DataSource as IResultNode;
            if(resultNode != null) _videoKeyframesManager.DisplayIFramesAsThumbs(resultNode.Children.ToArray());
		}

		public void AddColumns(IDetector detector)
		{
			columnChooser.AddColumns(detector);
		}

		public void AddResults(IList<IResultNode> results)
		{
			headerTree.AddResults(headerTree.RootRow, RowDropLocation.OnRow, results.ToArray());

			if (headerTree.IsWorkpad)
			{
				columnChooser.AddResults(results);
			}
		}

		/// <summary>
		/// Updates the header detail tree to the header corresponding
		/// to the focused (and selected) row in the header tree.
		/// </summary>
		private void UpdateHeaderDetailTree()
		{
            if (HeaderSelectionChanged != null) 
                HeaderSelectionChanged(this, EventArgs.Empty);

            headerDetailTree.ActiveHeaderSource = this;
            _framePreviewManager.ClearWindow();
			if (headerTree.FocusRow != null && headerTree.FocusRow.Selected)
            {
                IResultNode node = headerTree.FocusRow.Item as IResultNode;
                if(node != null)
                {
                	_framePreviewManager.AddNewFFmpegConvertItem(node);
                }
			}
		}

		#region Event handlers
		private void columnChooser_VisibleChanged(object sender, EventArgs e)
		{
			if (ColumnChooserVisibleChanged != null)
			{
				ColumnChooserVisibleChanged(this, e);
			}
		}

		private void headerTree_DataSourceChanged(object sender, EventArgs e)
		{
			UpdateHeaderDetailTree();

			// Update columns
			SetColumns(((RootResult == null) || (RootResult.Detectors.Count() == 0)) ? null : RootResult.Detectors.First());
			// TODO: headerTree.UpdateRows();
		}

		private void headerTree_SelectionChanged(object sender, EventArgs e)
		{
			UpdateHeaderDetailTree();

			//CountSelectedRows();
		}

		private void headerTree_FocusRowChanged(object sender, EventArgs e)
		{
			UpdateHeaderDetailTree();

			if (headerTree.FocusRow != null)
			{
				IResultNode result = headerTree.FocusRow.Item as IResultNode;
				columnChooser.HighlightItemsForHeader(result);
			}
		}

		private void headerTree_HeaderContextMenu_Opening(object sender, CancelEventArgs e)
		{
			// Enable/disable the 'Remove Column' menu option of the header context menu.
			// Enable when menu is opened on a column, otherwise disable.
			toolStripMenuItemRemoveColumn.Enabled = (headerTree.ContextMenuColumn == null) ? false : true;
		}

		private void toolStripMenuItemRemoveColumn_Click(object sender, EventArgs e)
		{
			if (headerTree.ContextMenuColumn != null)
			{
				// Hide the clicked column by making it invisible
				headerTree.ContextMenuColumn.Active = false;
			}
		}

		private void toolStripMenuItemColumnChooser_Click(object sender, EventArgs e)
		{
			if (!columnChooser.Visible)
			{
				columnChooser.CreateColumns();
				columnChooser.Show(this); // By supplying the parent, it stays on top.
			}
			else
			{
				columnChooser.Hide();
			}
		}

        // show the selected item in detailtree when a tab becomes visible
        protected override void OnVisibleChanged(EventArgs e)
        {
            if(Visible) UpdateHeaderDetailTree();
            base.OnVisibleChanged(e);
        }

		private void headerDetailTree_AddColumns(object sender, AddColumnsEventArgs e)
		{
			if(!Visible) return;

			foreach (string columnName in e.ColumnNames)
			{
				Column column = headerTree.Columns[columnName];
				if (column != null)
				{
					column.Active = true;
				}
			}
		}
		#endregion Event handlers

        public String GetSourceTitle()
        {
            return (headerTree.IsWorkpad) ? headerTree.WorkpadName : "Headers";
        }

        public Control GetSourceControl()
        {
            return (headerTree.IsWorkpad) ? (Control)headerTree.ParentWorkpad : this;
        }

        public IResultNode GetSourceResultNode()
        {
			if (headerTree == null || headerTree.FocusRow == null || !headerTree.FocusRow.Selected)
                return null;
            return headerTree.FocusRow.Item as IResultNode;
        }

        /// <summary>
        /// Adds columns for the dropped detectors.
        /// Normaly this is done by Workpad.AddFragments, but the drop doesn't use this method.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Drop event row data</param>
	    private void headerTree_RowDrop(object sender, RowDropEventArgs e)
	    {
            if (!headerTree.IsWorkpad) return;
            Row[] rows = (Row[])e.Data.GetData(typeof(Row[]));

            // For each detector in the data blocks,
            // add the column names to the column chooser
            List<IDetector> detectors = new List<IDetector>(); // List to keep track of the handled detectors
            foreach (Row row in rows)
            {
                IResultNode resultNode = row.Item as IResultNode;

                if (resultNode != null)
                {
                    foreach (IDetector detector in resultNode.Detectors.Where(d => (!detectors.Contains(d))))
                    {
                        detectors.Add(detector);
                        AddColumns(detector);
                    }
                }
            }
	    }

		private void headerTree_GotFocus(object sender, System.EventArgs e)
		{
			UpdateHeaderDetailTree();
		}
	}
}
