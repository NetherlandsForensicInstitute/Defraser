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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	public partial class ColumnChooser : Form
	{
		private readonly ProjectManager _projectManager;
		private readonly List<IDetector> _detectors = new List<IDetector>();

		#region Properties
		internal HeaderPanel HeaderPanel { private get;  set; }

		private ColumnList Columns { get { return HeaderPanel.Columns; } }
		private DisplayMode DisplayMode { get { return HeaderPanel.DisplayMode; } }
		private IResultNode RootResult { get { return HeaderPanel.RootResult; } }
		private IProject Project { get { return _projectManager.Project; } }
		private bool IsWorkpad { get { return !HeaderPanel.ReadOnly; } }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="ColumnChooser"/>.
		/// </summary>
		/// <param name="projectManager">The <see cref="ProjectManager"/></param>
		public ColumnChooser(ProjectManager projectManager)
		{
			_projectManager = projectManager;

			InitializeComponent();

			buttonUp.Enabled = false;
			buttonDown.Enabled = false;
			buttonAdd.Enabled = false;
			buttonRemove.Enabled = false;
		}

		/// <summary>
		/// Fill the GUI with all columns and highlight.
		/// Yellow highlighting for columns that belong to the focused header.
		/// </list>
		/// </summary>
		internal void CreateColumns()
		{
			SuspendLayout();
			try
			{
				listViewHiddenColumns.Items.Clear();
				listViewVisibleColumns.Items.Clear();

				foreach (Column column in Columns)
				{
					ListViewItem listViewItem;

					if (column.Active)
						listViewItem = listViewVisibleColumns.Items.Add(column.Name, column.Caption, 0);
					else
						listViewItem = listViewHiddenColumns.Items.Add(column.Name, column.Caption, 0);

					HighlightListViewItem(listViewItem, HeaderPanel.FocusRowResult);

					if (!IsWorkpad)
					{
						column.PropertyChanged += ColumnPropertyChanged;
					}
				}
			}
			finally
			{
				ResumeLayout();
				buttonApply.Enabled = false;
			}
		}

		/// <summary>
		/// Create a list containing all visible columns,
		/// using the GUI as input.
		/// </summary>
		/// <returns>A list containing all visible columns</returns>
		internal IList<string> GetVisibleColumnNames()
		{
			List<string> visibleColumnNames = new List<string>();

			foreach (ListViewItem listViewItem in listViewVisibleColumns.Items)
			{
				visibleColumnNames.Add(listViewItem.Name);
			}
			return visibleColumnNames;
		}

		internal void HighlightItemsForHeader(IResultNode result)
		{
			foreach (ListViewItem listViewItem in listViewVisibleColumns.Items)
			{
				HighlightListViewItem(listViewItem, result);
			}
			foreach (ListViewItem listViewItem in listViewHiddenColumns.Items)
			{
				HighlightListViewItem(listViewItem, result);
			}
		}

		private void HighlightListViewItem(ListViewItem listViewItem, IResultNode result)
		{
			if (listViewItem == null) return;

			if (result != null && result.Detectors.First().ColumnInHeader(result.Name, listViewItem.Name))
			{
				listViewItem.BackColor = Color.Yellow;
			}
			else
			{
				listViewItem.BackColor = listViewVisibleColumns.BackColor;
			}
		}

		private void ColumnChooser_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			ApplyChanges();

			Hide();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Hide();
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			ApplyChanges();
		}

		/// <summary>
		/// Applies changes to the columns and updates the visible columns
		/// in the current project.
		/// </summary>
		private void ApplyChanges()
		{
			buttonApply.Enabled = false;

			IList<string> visibleColumnNames = GetVisibleColumnNames();

			// Update visibility
			foreach (Column column in Columns)
			{
				column.Active = visibleColumnNames.Contains(column.Name);
			}

			// Update order and create list of visible columns
			List<IColumnInfo> visibleColumns = new List<IColumnInfo>();
			for (int index = 0; index < visibleColumnNames.Count; index++)
			{
				Column column = Columns[visibleColumnNames[index]];

				if (column != null)
				{
					Columns.SetIndexOf(column, index);
					visibleColumns.Add(new ColumnInfo(column.Caption, column.Name, column.HeaderStyle.HorzAlignment, column.Width));
				}
			}

			if ((Project != null) && (RootResult != null) && !IsWorkpad)
			{
				Project.SetVisibleColumns(RootResult.Detectors.First(), visibleColumns);
			}
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			MoveSelectedItemsToVisibleColumn();
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			MoveSelectedItemsToHiddenColumn();
		}

		private void buttonUp_Click(object sender, EventArgs e)
		{
			buttonApply.Enabled = true;

			ListView.SelectedListViewItemCollection selectedListViewItemCollection = listViewVisibleColumns.SelectedItems;
			if (selectedListViewItemCollection.Count == 1 && selectedListViewItemCollection[0].Index > 0)
			{
				ListViewItem listViewItem = selectedListViewItemCollection[0];
				int index = listViewItem.Index;
				listViewVisibleColumns.Items.RemoveAt(index);
				listViewVisibleColumns.Items.Insert(--index, listViewItem);
				listViewItem.Focused = true;
			}
		}

		private void buttonDown_Click(object sender, EventArgs e)
		{
			buttonApply.Enabled = true;

			ListView.SelectedListViewItemCollection selectedListViewItemCollection = listViewVisibleColumns.SelectedItems;
			if (selectedListViewItemCollection.Count == 1 &&
				selectedListViewItemCollection[0].Index < listViewVisibleColumns.Items.Count - 1)
			{
				ListViewItem listViewItem = selectedListViewItemCollection[0];
				int index = listViewItem.Index;
				listViewVisibleColumns.Items.RemoveAt(index);
				listViewVisibleColumns.Items.Insert(++index, listViewItem);
				listViewItem.Focused = true;
			}
		}

		private void listViewVisibleColumns_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (listViewVisibleColumns.SelectedItems.Count == 1)
			{
				buttonDown.Enabled = true;
				buttonUp.Enabled = true;
			}
			else
			{
				buttonDown.Enabled = false;
				buttonUp.Enabled = false;
			}

			buttonRemove.Enabled = listViewVisibleColumns.SelectedItems.Count > 0;
		}

		private void listViewHiddenColumns_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			buttonAdd.Enabled = listViewHiddenColumns.SelectedItems.Count > 0;
		}

		private void listViewHiddenColumns_DoubleClick(object sender, EventArgs e)
		{
			MoveSelectedItemsToVisibleColumn();
		}

		private void listViewVisibleColumns_DoubleClick(object sender, EventArgs e)
		{
			MoveSelectedItemsToHiddenColumn();
		}

		private void MoveSelectedItemsToVisibleColumn()
		{
			buttonApply.Enabled = true;

			ListView.SelectedListViewItemCollection selectedHiddenColumns = listViewHiddenColumns.SelectedItems;

			foreach (ListViewItem selectedHiddenColumn in selectedHiddenColumns)
			{
				listViewHiddenColumns.Items.Remove(selectedHiddenColumn);
				listViewVisibleColumns.Items.Add(selectedHiddenColumn);
			}
		}

		private void MoveSelectedItemsToHiddenColumn()
		{
			buttonApply.Enabled = true;

			ListView.SelectedListViewItemCollection selectedVisibleColumns = listViewVisibleColumns.SelectedItems;

			foreach (ListViewItem selectedVisibleColumn in selectedVisibleColumns)
			{
				listViewVisibleColumns.Items.Remove(selectedVisibleColumn);
				listViewHiddenColumns.Items.Add(selectedVisibleColumn);
			}
		}

		void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Column column = sender as Column;
			if (column == null) return;
			if ((Project == null) || (RootResult == null)) return;

			if (column.Active)
			{
				if (Project.IsVisibleColumn(RootResult.Detectors.First(), column.Name))
				{
					Project.UpdateColumnWidth(RootResult.Detectors.First(), column.Name, column.Width);
				}
				else // Column not yet visible
				{
					Project.AddVisibleColumn(RootResult.Detectors.First(), new ColumnInfo(column.Caption, column.Name, column.HeaderStyle.HorzAlignment, column.Width));
				}
			}
			else if (!column.Active)
			{
				if (Project.IsVisibleColumn(RootResult.Detectors.First(), column.Name))
				{
					Project.RemoveVisibleColumn(RootResult.Detectors.First(), column.Name);
				}
			}
		}

		/// <summary>
		/// Sets the columns for this tree to the column names of the
		/// given <paramref name="detector"/>.
		/// </summary>
		/// <param name="detector">the detector</param>
		/// <param name="isWorkpad">The column chooser can be called from a workpad or the main dailog</param>
		public void SetColumns(IDetector detector, bool isWorkpad)
		{
			_detectors.Clear();
			Columns.Clear();
			listViewHiddenColumns.Items.Clear();
			listViewVisibleColumns.Items.Clear();

			if (!isWorkpad && detector == null) return;
			//if(detector==null && isWorkpad) -> default columns only

			if (detector != null)
			{
				_detectors.Add(detector);
			}

			IList<IColumnInfo> visibleColumns = null;
			IProject project = _projectManager.Project;

			if (project != null && detector != null)
			{
				visibleColumns = project.GetVisibleColumns(detector);
			}
			if (visibleColumns == null || visibleColumns.Count == 0)
			{
				// Do not add column 'Detector' / 'Detector Version' and 'File' to the default visible columns
				visibleColumns = new List<IColumnInfo>();
				foreach (IColumnInfo columnInfo in DefaultColumnExtensions.GetHeaderTreeDefaultColumns(DisplayMode))
				{
					if (columnInfo.Name != Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.Detector) &&
						columnInfo.Name != Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.DetectorVersion) &&
						columnInfo.Name != Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.File))
					{
						visibleColumns.Add(columnInfo);
					}
				}

				if (project != null && detector != null)
				{
					project.SetVisibleColumns(detector, visibleColumns);
				}
			}
			Columns.SuspendChangeNotification();

			// Add the visible columns
			foreach (ColumnInfo columnInfo in visibleColumns)
			{
				AddColumn(detector, GetCorrectCaptionForOffsetAndLength(columnInfo), true);
			}

			// Make the Offset column the default sort column; sort ascending
			Column offsetColumn = Columns[DefaultColumnIndex.Offset.GetName()];
			if (offsetColumn != null)
			{
				offsetColumn.SortDirection = ListSortDirection.Ascending;
				HeaderPanel.SortColumn = offsetColumn;
			}

			// Add the invisible default columns
			foreach (ColumnInfo defaultColumnInfo in DefaultColumnExtensions.GetHeaderTreeDefaultColumns(DisplayMode))
			{
				if (!IsVisibleColumn(defaultColumnInfo.Name, visibleColumns))
				{
					AddColumn(detector, defaultColumnInfo, false);
				}
			}

			// Add the invisible custom columns
			foreach (IColumnInfo customColumnInfo in detector.GetColumns())
			{
				if (!IsVisibleColumn(customColumnInfo.Name, visibleColumns))
				{
					AddColumn(detector, customColumnInfo, false);
				}
			}

			Columns.ResumeChangeNotification();

			CreateColumns();
		}

		/// <summary>
		/// This method makes sure the caption of Length and Offset have the
		/// correct value for Hex or Dec, depending on the mode.
		/// 
		/// ColumnInfo can be stored using Project.SetVisibleColumns() and later
		/// retrieved using Project.GetVisibleColumns().
		///
		/// This method is needed because the Hex/Dec mode during retrival can
		/// be different from the mode during storage.
		/// </summary>
		/// <param name="columnInfo">the column info object to correct</param>
		/// <returns>When the <paramref name="columnInfo"/> is of type Length or Offset
		/// set the Hex/Dec to the current mode.</returns>
		private IColumnInfo GetCorrectCaptionForOffsetAndLength(IColumnInfo columnInfo)
		{
			if (columnInfo.Name == Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.Length))
			{
				return DefaultColumnIndex.Length.GetColumnInfo(DisplayMode);
			}
			if (columnInfo.Name == Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.Offset))
			{
				return DefaultColumnIndex.Offset.GetColumnInfo(DisplayMode);
			}
			if (columnInfo.Name == Enum.GetName(typeof(DefaultColumnIndex), DefaultColumnIndex.EndOffset))
			{
				return DefaultColumnIndex.EndOffset.GetColumnInfo(DisplayMode);
			}
			return columnInfo;
		}

		/// <summary>
		/// Adds columns for the given <paramref name="detector"/>.
		/// </summary>
		/// <param name="detector">the detector to add columns for</param>
		public void AddColumns(IDetector detector)
		{
			Columns.SuspendChangeNotification();

			// Add columns from the given detector that are not yet in this tree
			if (!_detectors.Contains(detector))
			{
				_detectors.Add(detector);

				foreach (IColumnInfo columnInfo in detector.GetColumns())
				{
					if (Columns[columnInfo.Name] == null)
					{
						AddColumn(detector, columnInfo, false);
					}
				}
			}

			// Check the database for visible columns for the given dectector
			IProject project = _projectManager.Project;
			if (project != null)
			{
				IList<IColumnInfo> visibleColumns = project.GetVisibleColumns(detector);

				foreach (ColumnInfo columnInfo in visibleColumns)
				{
					Column column = Columns[columnInfo.Name];

					if (column != null)
					{
						column.Width = columnInfo.Width;
						column.Active = true;
					}
				}
			}

			Columns.ResumeChangeNotification();
		}

		private void AddColumn(IDetector detector, IColumnInfo columnInfo, bool visible)
		{
			if (!ContainsColumn(columnInfo.Name))
			{
				Column column = new Column();
				column.Caption = columnInfo.Caption;
				column.DataField = (detector == null) ? string.Empty : detector.Name;
				column.Name = columnInfo.Name;	// Now you can use Columns[Column.Name] to get a column
				column.Active = visible;
				column.HeaderStyle.HorzAlignment = columnInfo.HorizontalAlignment;
				column.CellStyle.HorzAlignment = columnInfo.HorizontalAlignment;
				column.Width = columnInfo.Width;

				Columns.Add(column);

				if (column.Name == HeaderPanel.SortColumnName)
				{
					column.Active = true;
					HeaderPanel.SortColumn = column;
				}
			}
			else
			{
				Column column = Columns[columnInfo.Name];
				if (column != null)
				{
					column.Active = visible;
					column.Width = columnInfo.Width;
				}
			}
		}

		private static bool IsVisibleColumn(string columnName, IEnumerable<IColumnInfo> visibleColumns)
		{
			foreach (IColumnInfo columnInfo in visibleColumns)
			{
				if (columnInfo.Name == columnName)
				{
					return true;
				}
			}
			return false;
		}

		private bool ContainsColumn(string name)
		{
			foreach (Column column in Columns)
			{
				if (name == column.Name)
				{
					return true;
				}
			}
			return false;
		}

		// TODO: split into GetDetectorsForResult in HeaderPanel and AddColumnsTo(IList<IDetector) in ColumnChooser
		internal void AddResults(IList<IResultNode> results)
		{
			if (results == null) return;

			foreach (IResultNode result in results)
			{
				// Add custom columns for new detectors
				IDetector detector = result.Detectors.First();
				if (detector != null && !_detectors.Contains(detector))
				{
					AddColumns(detector);
				}
			}
		}
	}
}
