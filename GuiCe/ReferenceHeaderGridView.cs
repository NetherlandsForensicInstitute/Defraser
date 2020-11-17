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
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public partial class ReferenceHeaderGridView : DataGridView
	{
		#region Events
		public event EventHandler<EventArgs> IncludedHeadersChanged;
		#endregion Events

		private readonly IList<DataGridViewColumn> _codecParameterColumns;

		#region Properties
		public bool IncludedColumnVisible
		{
			get { return columnIncluded.Visible; }
			set { columnIncluded.Visible = value; }
		}

		public IEnumerable<IReferenceHeader> IncludedHeaders
		{
			get
			{
				ICollection<IReferenceHeader> includedHeaders = new List<IReferenceHeader>();
				foreach (DataGridViewRow row in Rows)
				{
					if ((bool)row.Cells[columnIncluded.Index].Value)
					{
						includedHeaders.Add(GetReferenceHeaderForRow(row));
					}
				}
				return includedHeaders;
			}
			set
			{
				ICollection<IReferenceHeader> includedHeaders = new List<IReferenceHeader>(value);
				foreach (DataGridViewRow row in Rows)
				{
					row.Cells[columnIncluded.Index].Value = includedHeaders.Contains(GetReferenceHeaderForRow(row));
				}
			}
		}

		private IReferenceHeader GetReferenceHeaderForRow(DataGridViewRow row)
		{
			return (IReferenceHeader) row.Cells[columnId.Index].Value;
		}

		public IEnumerable<IReferenceHeader> ReferenceHeaders
		{
			set
			{
				// Remove codec parameter columns for previous results
				foreach (DataGridViewColumn column in _codecParameterColumns)
				{
					Columns.Remove(column);
				}

				_codecParameterColumns.Clear();

				// Create and add columns for codec parameters
				int index = 9;
				IList<string> codecParameterNames = GetUniqueCodecParameterNames(value);
				foreach (string parameterName in codecParameterNames)
				{
					DataGridViewColumn column = CreateCodecParameterColumn(parameterName, index++);
					_codecParameterColumns.Add(column);
					Columns.Add(column);
				}

				columnData.DisplayIndex = index;

				IList<DataGridViewRow> rowsToRemove = new List<DataGridViewRow>();
				IList<IReferenceHeader> headersToAdd = new List<IReferenceHeader>(value);

				// Update existing rows and determine the rows to add/remove
				foreach (DataGridViewRow row in Rows)
				{
					IReferenceHeader header = GetHeaderForRow(row);
					if (headersToAdd.Contains(header))
					{
						// Update existing row
						row.SetValues(CreateRow(codecParameterNames, header));
						// Row exists: Do not add another row for this header!
						headersToAdd.Remove(header);
					}
					else
					{
						rowsToRemove.Add(row);
					}
				}

				// Add rows for headers that were added
				foreach (IReferenceHeader header in headersToAdd)
				{
					Rows.Add(CreateRow(codecParameterNames, header));
				}

				// Remove rows of deleted headers
				foreach (DataGridViewRow row in rowsToRemove)
				{
					Rows.Remove(row);
				}
			}
		}
		#endregion Properties

		public ReferenceHeaderGridView()
		{
			_codecParameterColumns = new List<DataGridViewColumn>();

			InitializeComponent();
		}

		private static IList<string> GetUniqueCodecParameterNames(IEnumerable<IReferenceHeader> headers)
		{
			var uniqueCodecParameterNames = new List<string>();
			foreach (IReferenceHeader header in headers)
			{
				foreach (string name in header.CodecParameters)
				{
					if (!uniqueCodecParameterNames.Contains(name))
					{
						uniqueCodecParameterNames.Add(name);
					}
				}
			}
			return uniqueCodecParameterNames;
		}

		private static DataGridViewColumn CreateCodecParameterColumn(string parameterName, int index)
		{
			return new DataGridViewTextBoxColumn
			{
				HeaderText = parameterName,
				Name = parameterName,
				ReadOnly = true,
				DisplayIndex = index,
				AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
			};
		}

		private static object[] CreateRow(IList<string> codecParameterNames, IReferenceHeader header)
		{
			var row = new object[10 + codecParameterNames.Count];
			row[0] = header;
			row[1] = false;
			row[2] = header.Brand;
			row[3] = header.Model;
			row[4] = header.Setting;
			row[5] = header.CodecParameters.Codec;
			row[6] = header.CodecParameters.Width;
			row[7] = header.CodecParameters.Height;
			row[8] = header.CodecParameters.FrameRate;
			row[9] = BitConverter.ToString(header.Data).Replace("-", string.Empty);

			foreach (string parameterName in header.CodecParameters)
			{
				row[10 + codecParameterNames.IndexOf(parameterName)] = header.CodecParameters[parameterName];
			}
			return row;
		}

		public IReferenceHeader GetHeaderForRow(int rowIndex)
		{
			if ((rowIndex < 0) || (rowIndex >= Rows.Count))
			{
				return null; // Header row or no row (selected)
			}

			return GetHeaderForRow(Rows[rowIndex]);
		}

		private IReferenceHeader GetHeaderForRow(DataGridViewRow row)
		{
			return row.Cells[columnId.Index].Value as IReferenceHeader;
		}

		#region Event handlers
		private void ReferenceHeaderGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			DataGridViewColumn column = Columns[e.ColumnIndex];
			if ((column == columnBrand) || (column == columnModel) || (column == columnSetting))
			{
				if ((e.Value == null) || "".Equals(e.Value))
				{
					e.Value = "<unknown>";
				}
			}
		}

		private void ReferenceHeaderGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if ((Columns[e.ColumnIndex] == columnIncluded) && (IncludedHeadersChanged != null))
			{
				IncludedHeadersChanged(this, EventArgs.Empty);
			}
		}
		#endregion Event handlers
	}
}
