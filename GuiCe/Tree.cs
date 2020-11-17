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
using System.Drawing;
using System.Windows.Forms;
using Infralution.Controls.VirtualTree;
using System.Collections.Generic;

namespace Defraser.GuiCe
{
	/// <summary>
	/// The numeric formats for data offset and length display.
	/// The default value is decimal.
	/// </summary>
	[DefaultValue(Dec)]
	public enum DisplayMode
	{
		/// <summary>Hexadecimal format.</summary>
		Hex,
		/// <summary>Decimal format.</summary>
		Dec
	}

	/// <summary>
	/// Defines the look and feel of all Defraser tree components.
	/// Provides common methods, GUI code and event handlers used in derivations
	/// of this class.
	/// </summary>
	public partial class Tree : VirtualTree, INotifyPropertyChanged
	{
		#region Events
		/// <summary>
		/// Occurs when the data source (root row) has changed.
		/// </summary>
		public event EventHandler<EventArgs> DataSourceChanged;
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion Events

		private readonly IList<Icon> _icons;

		#region Properties
		public override object DataSource
		{
			get { return base.DataSource; }
			set
			{
				base.DataSource = value;

				OnDataSourceChanged(EventArgs.Empty);
			}
		}

		/// <summary>Indicates whether any rows are selected in the tree.</summary>
		public bool HasSelectedRows { get; private set; }

		/// <summary>The icon for valid results.</summary>
		protected Icon ValidIcon { get; private set; }
		/// <summary>The icon for invalid results.</summary>
		protected Icon InvalidIcon { get; private set; }
		/// <summary>The icon for generated results, e.g. originating from a default header.</summary>
		protected Icon GeneratedIcon { get; private set; }
		/// <summary>A unique subset of the selected rows in the order as they appear on the GUI.</summary>
		/// <seealso cref="VirtualTreeExtensions.GetUniqueRowsInGuiOrder(Row[])"/>
		protected Row[] UniqueSelectedRowsInGuiOrder
		{
			get
			{
				if (!HasSelectedRows)
				{
					return new Row[0];
				}
				Row[] rows = (Row[])ArrayList.Adapter(SelectedRows).ToArray(typeof(Row));
				return rows.GetUniqueRowsInGuiOrder();
			}
		}
		/// <summary>The selected rows in the order as they appear on the GUI.</summary>
		protected Row[] SelectedRowsInGuiOrder
		{
			get
			{
				if (!HasSelectedRows)
				{
					return new Row[0];
				}
				Row[] rows = (Row[])ArrayList.Adapter(SelectedRows).ToArray(typeof(Row));
				return rows.GetRowsInGuiOrder();
			}
		}
		#endregion Properties

		public Tree()
		{
			InitializeComponent();

			_icons = new List<Icon>();

			ValidIcon = CreateIcon(imageList.Images["Valid"]);
			InvalidIcon = CreateIcon(imageList.Images["Invalid"]);
			GeneratedIcon = CreateIcon(imageList.Images["Generated"]);
		}

		protected Icon CreateIcon(Image image)
		{
			using (Bitmap bitmap = image as Bitmap ?? new Bitmap(image))
			{
				Icon icon = Icon.FromHandle(bitmap.GetHicon());
				_icons.Add(icon);
				return icon;
			}
		}
		
		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		#region Programmatic data binding
		/// <summary>
		/// Invoked when the data source has changed.
		/// </summary>
		/// <param name="e">the event arguments</param>
		protected virtual void OnDataSourceChanged(EventArgs e)
		{
			if (DataSourceChanged != null)
			{
				DataSourceChanged(this, e);
			}
		}
		#endregion Programmatic data binding

		#region Event handlers
		private void Tree_KeyDown(object sender, KeyEventArgs e)
		{
			// Select all rows in the tree when the user presses 'Ctrl A'
			if (e.Control && (e.KeyCode == Keys.A))
			{
				SelectAllRows();
			}
		}

		private void Tree_SelectionChanged(object sender, EventArgs e)
		{
			// TODO: move this code to the 'HasSelectedRows' property setter

			bool b = (SelectedRows != null) && (SelectedRows.Count > 0);
			if (HasSelectedRows != b)
			{
				HasSelectedRows = b;

				OnPropertyChanged(new PropertyChangedEventArgs("HasSelectedRows"));
			}
		}
		#endregion Event handlers
	}
}
