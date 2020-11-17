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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Infralution.Controls;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Lists the attributes of a single <c>IResultNode</c> in a tree view.
	/// The result to display is controlled through the <c>Result</c> property.
	/// </summary>
	public sealed partial class HeaderDetailTree : Tree
	{
		public event EventHandler<AddColumnsEventArgs> AddColumns;

		private readonly IDictionary<IResult, IResult> _parentLinks = new Dictionary<IResult, IResult>();
		private static readonly StyleDelta GrayedOutStyleDelta = new StyleDelta { ForeColor = Color.Gray };
	    private IHeaderDetailSource _activeHeaderSource;

	    #region Properties
		/// <summary>
		/// The result of which the attributes are currently displayed.
		/// </summary>
		internal IResultNode Result
		{
			get { return DataSource as IResultNode; }
		}

		/// <summary>
		/// The column names for the selected attributes.
		/// </summary>
		private IList<string> SelectedColumnNames
		{
			get
			{
				List<string> columnNames = new List<string>();
				if (SelectedItems != null)
				{
					foreach (IResultAttribute resultAttribute in SelectedItems)
					{
						columnNames.Add(resultAttribute.Name);
					}
				}
				return columnNames;
			}
		}

	    internal IHeaderDetailSource ActiveHeaderSource
	    {
            get { return _activeHeaderSource; }
	        set
	        {
	            Control control;

                // remove old event handler
                if (_activeHeaderSource != null)
                {
                    control = _activeHeaderSource.GetSourceControl();
                    if (control != null) control.TextChanged -= new System.EventHandler(this.source_TextChanged);
                }

                // make new object the active source
	            _activeHeaderSource = value;

                // add new event handler
                control = _activeHeaderSource.GetSourceControl();
                if (control != null) control.TextChanged += new System.EventHandler(this.source_TextChanged);

                // update result (detail) tree
                DataSource = (_activeHeaderSource != null) ? _activeHeaderSource.GetSourceResultNode() : null;

                // update window
                HeaderDetailWindow window = Parent as HeaderDetailWindow;
                if (window != null)
                {
                    window.SetDefaultTitle();
                    if (_activeHeaderSource != null) window.Text += string.Format(" - {0}", _activeHeaderSource.GetSourceTitle());
                }

                // update context menuitem
                string menuItemText = (_activeHeaderSource != null) ? _activeHeaderSource.GetSourceTitle() : "Unknown";
                SetDefaultAddToMenuItemText(menuItemText);
	        }
	    }

	    private void source_TextChanged(object sender, EventArgs e)
	    {
            // Update data; Refresh.
	        ActiveHeaderSource = _activeHeaderSource;
	    }

	    #endregion Properties

		public HeaderDetailTree()
		{
			InitializeComponent();
		}

		public HeaderDetailTree(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		#region Programmatic data binding
		protected override void OnDataSourceChanged(EventArgs e)
		{
			// Update parent links on change in data source
			_parentLinks.Clear();

			if (Result != null)
			{
				AddParentLinks(Result);
			}
			base.OnDataSourceChanged(e);
		}

		protected override object GetParentForItem(object item)
		{
			IResult result = item as IResult;
			IResult parent = null;

			if (result != null)
			{
				_parentLinks.TryGetValue(result, out parent);
			}
			return parent;
		}

		protected override IList GetChildrenForRow(Row row)
		{
			IResult result = row.Item as IResult;

			if (result == null || result.Attributes == null) return null;

			return new ReadOnlyCollection<IResultAttribute>(result.Attributes);
		}

		/// <summary>
		/// Sets the icon (valid/invalid) for a certain <paramref name="row"/>.
		/// </summary>
		/// <param name="row">the row</param>
		/// <param name="rowData">the row data to set</param>
		protected override void OnGetRowData(Row row, RowData rowData)
		{
			base.OnGetRowData(row, rowData);

			IResult result = row.Item as IResult;

			if (result == null) return;

			rowData.Icon = result.Valid ? ValidIcon : InvalidIcon;
		}

		protected override void OnGetCellData(Row row, Column column, CellData cellData)
		{
			base.OnGetCellData(row, column, cellData);

			IResultAttribute attribute = row.Item as IResultAttribute;
			if (attribute != null && column.DataField != null)
			{
				if (column == columnName)
				{
					cellData.Value = attribute.Name;
				}
				else if (column == columnValue)
				{
					cellData.Value = attribute.ValueAsString;
				}

				// Display attributes that have no corresponding column as grayed out
				if (!Result.Detectors.First().ColumnInHeader(Result.Name, attribute.Name))
				{
					cellData.EvenStyle = new Style(cellData.EvenStyle, GrayedOutStyleDelta);
					cellData.OddStyle = new Style(cellData.OddStyle, GrayedOutStyleDelta);
				}
			}
		}
		#endregion Programmatic data binding

		/// <summary>
		/// Returns whether any of the <paramref name="selectedRows"/> has
		/// an accompanying column.
		/// </summary>
		/// <param name="selectedRows">the selected rows</param>
		/// <returns><c>true</c> if any row has an accompanying column, <c>false</c> otherwise</returns>
		private bool HasAccompanyingColumn(RowSelectionList selectedRows)
		{
			if (!HasSelectedRows) return false;

			foreach (Row row in selectedRows)
			{
				IResultAttribute resultAttribute = row.Item as IResultAttribute;
				if (resultAttribute != null && Result.Detectors.First().ColumnInHeader(Result.Name, resultAttribute.Name))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds parent links for the specified <paramref name="result"/>.
		/// This information is used to query the parent for an attribute.
		/// </summary>
		/// <param name="result">the result to add</param>
		private void AddParentLinks(IResult result)
		{
			if (result.Attributes != null)
			{
				foreach (IResultAttribute child in result.Attributes)
				{
					_parentLinks[child] = result;

					AddParentLinks(child);
				}
			}
		}

		#region Event handlers
		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			toolStripMenuItemAddAsColumnToHeaders.Enabled = HasAccompanyingColumn(SelectedRows);
		}

		private void toolStripMenuItemAddAsColumnToHeaders_Click(object sender, EventArgs e)
		{
			// Throw the 'AddColumnsClick' event when the user has selected
			// one or more header details rows. This event is used to add columns
			// to the Headers pane.
			if (AddColumns != null)
			{
				AddColumns(this, new AddColumnsEventArgs(SelectedColumnNames));
			}
		}
		#endregion Event handlers
	}
}
