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
using System.ComponentModel;
using Defraser.Util;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Show detail of file or data block
	/// </summary>
	internal partial class FileDetailTree : Tree
	{
		private DetailsBase _details;

		public FileDetailTree()
		{
			InitializeComponent();

			RemoveColumnChooserFromMenu();
		}

		public FileDetailTree(IContainer container)
		{
			container.Add(this);

			InitializeComponent();

			RemoveColumnChooserFromMenu();
		}

		private void RemoveColumnChooserFromMenu()
		{
			HeaderContextMenu.Items.RemoveAt(HeaderContextMenu.Items.Count - 1);
		}

		public override object DataSource
		{
			get { return base.DataSource; }
			set
			{
				_details = DetailsBase.CreateDetailInstance(value);

				if (Columns != null && Columns.Count >= 2)
				{
					Columns[0].Caption = _details.Column0Caption;
					Columns[1].Caption = _details.Column1Caption;
				}

				base.DataSource = value;
			}
		}

		internal string Title
		{
			get
			{
				if (_details == null)
				{
					throw new InvalidOperationException("First set the DataSource");
				}
				return _details.Title;
			}
		}

		#region Programmatic data binding
		protected override IList GetChildrenForRow(Row row)
		{
			return _details.GetChildrenForRow(row);
		}

		protected override void OnGetCellData(Row row, Column column, CellData cellData)
		{
			base.OnGetCellData(row, column, cellData);

			cellData.Value = _details.GetCellData(row.Item as Pair<string, string>, column.Caption);
		}
		#endregion Programmatic data binding
	}
}
