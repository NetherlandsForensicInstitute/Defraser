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
using System.Runtime.Serialization;
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public partial class ReferenceHeaderDatabaseForm : Form, INotifyPropertyChanged
	{
		private const string HeaderSelectedPropertyName = "HeaderSelected";
		private const string EmptyPropertyOption = "<unknown>";

		#region Events
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion Events

		private bool _headerSelected;
		private IList<IDetector> _codecDetectors;
		private IReferenceHeaderDatabase _database;

		#region Properties
		public IReferenceHeaderDatabase Database
		{
			private get { return _database; }
			set
			{
				Console.WriteLine("setting database: " + value);
				_database = value;

				UpdateFilterOptions();
				UpdateReferenceHeaderList();
			}
		}
		public IList<IDetector> CodecDetectors
		{
			private get
			{
				return _codecDetectors;
			}
			set
			{
				_codecDetectors = value;

				var codecIds = new List<CodecID>();
				foreach (IDetector codecDetector in value)
				{
					foreach (CodecID supportedFormat in codecDetector.SupportedFormats)
					{
						if (!codecIds.Contains(supportedFormat))
						{
							codecIds.Add(supportedFormat);
						}
					}
				}

				// This codec (H.263) does not require any headers.
				codecIds.Remove(CodecID.H263);
				codecIds.Sort();

				listBoxCodecs.Items.Clear();

				for (int i = 0; i < codecIds.Count; i++)
				{
					listBoxCodecs.Items.Add(codecIds[i].GetDescriptiveName());
					listBoxCodecs.SetItemChecked(i, true);
				}

				UpdateReferenceHeaderList();
			}
		}

		public bool HeaderSelected
		{
			get { return _headerSelected; }
			set
			{
				if (value != _headerSelected)
				{
					_headerSelected = value;

					OnPropertyChanged(new PropertyChangedEventArgs(HeaderSelectedPropertyName));
				}
			}
		}
		#endregion Properties

		public ReferenceHeaderDatabaseForm()
		{
			InitializeComponent();

			InitializeDataBindings();

			referenceHeaderGridView.IncludedColumnVisible = false;
		}

		private void InitializeDataBindings()
		{
			buttonDelete.DataBindings.Add("Enabled", this, HeaderSelectedPropertyName);
			buttonEdit.DataBindings.Add("Enabled", this, HeaderSelectedPropertyName);
		}

		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		private void InitializeFilterOptions<TValue>(ComboBox comboBox, string noFilterOption, Func<IReferenceHeader, TValue> headerPropertySelector)
		{
			// Determine the options for the combo box
			var possibleValues = new List<TValue>();
			bool containsDefaultOption = false;
			foreach (TValue value in Database.ListHeaders(r => true).Select(headerPropertySelector))
			{
				if (Equals(value, null) || "".Equals(value))
				{
					containsDefaultOption = true;
				}
				else if (!possibleValues.Contains(value))
				{
					possibleValues.Add(value);
				}
			}

			// Save the current selection
			object selectedItem = comboBox.SelectedItem;

			// Sort the items and add them to the combo box, including the no filter and empty property option, if applicable
			possibleValues.Sort();

			comboBox.Items.Clear();
			comboBox.Items.Add(noFilterOption);

			if (containsDefaultOption)
			{
				comboBox.Items.Add(EmptyPropertyOption);
			}
			foreach (TValue value in possibleValues)
			{
				comboBox.Items.Add(value);
			}

			// Restore the selection if the item still exists or select the 'no filter' option otherwise
			if (((selectedItem is TValue) && possibleValues.Contains((TValue) selectedItem)) || EmptyPropertyOption.Equals(selectedItem))
			{
				comboBox.SelectedItem = selectedItem;
			}
			else
			{
				comboBox.SelectedItem = noFilterOption;
			}
		}

		private void UpdateFilterOptions()
		{
			InitializeFilterOptions(comboBoxCameraBrand, "<any brand>", r => r.Brand);
			InitializeFilterOptions(comboBoxCameraModel, "<any model>", r => r.Model);
			InitializeFilterOptions(comboBoxInfo, "<any info/setting>", r => r.Setting);
			InitializeFilterOptions(comboBoxWidth, "<any width>", r => r.CodecParameters.Width);
			InitializeFilterOptions(comboBoxHeight, "<any height>", r => r.CodecParameters.Height);
		}

		private void UpdateReferenceHeaderList()
		{
			UpdateReferenceHeaderList(GetSelectedCodecs());
		}

		private IList<CodecID> GetSelectedCodecs()
		{
			IList<CodecID> selectedCodecs = new List<CodecID>();
			foreach (string codecName in listBoxCodecs.CheckedItems)
			{
				selectedCodecs.Add(CodecForDescriptiveName(codecName));
			}
			return selectedCodecs;
		}

		private void UpdateReferenceHeaderList(ICollection<CodecID> selectedCodecs)
		{
			IEnumerable<IReferenceHeader> referenceHeaders = Database.ListHeaders(r =>
					   selectedCodecs.Contains(r.CodecParameters.Codec)
					&& CheckReferenceHeaderInclusion(comboBoxCameraBrand, r.Brand)
					&& CheckReferenceHeaderInclusion(comboBoxCameraModel, r.Model)
					&& CheckReferenceHeaderInclusion(comboBoxInfo, r.Setting)
					&& CheckReferenceHeaderInclusion(comboBoxWidth, r.CodecParameters.Width)
					&& CheckReferenceHeaderInclusion(comboBoxHeight, r.CodecParameters.Height));

			if (checkBoxHideDuplicateHeaders.Checked)
			{
				IList<IReferenceHeader> uniqueReferenceHeaders = GetUniqueReferenceHeaders(referenceHeaders);
				referenceHeaders = uniqueReferenceHeaders;
			}

			referenceHeaderGridView.ReferenceHeaders = referenceHeaders;

		}

		private static CodecID CodecForDescriptiveName(string codecName)
		{
			foreach (CodecID codecId in Enum.GetValues(typeof(CodecID)))
			{
				if (codecName == codecId.GetDescriptiveName())
				{
					return codecId;
				}
			}
			return CodecID.Unknown;
		}

		private static bool CheckReferenceHeaderInclusion(ComboBox comboBox, object headerPropertyValue)
		{
			if (comboBox.SelectedIndex == 0)
			{
				return true; // No filtering applied
			}
			if (Equals(comboBox.SelectedItem, headerPropertyValue))
			{
				return true; // Property headerPropertyValue matches filtered headerPropertyValue
			}
			if ((headerPropertyValue == null) || "".Equals(headerPropertyValue))
			{
				return EmptyPropertyOption.Equals(comboBox.SelectedItem);
			}

			return false; // This header should be filtered (hidden)
		}

		private static IList<IReferenceHeader> GetUniqueReferenceHeaders(IEnumerable<IReferenceHeader> referenceHeaders)
		{
			IList<string> uniqueHeaderData = new List<string>();
			IList<IReferenceHeader> uniqueReferenceHeaders = new List<IReferenceHeader>();
			foreach (IReferenceHeader referenceHeader in referenceHeaders)
			{
				string headerData = BitConverter.ToString(referenceHeader.Data);
				if (!uniqueHeaderData.Contains(headerData))
				{
					uniqueHeaderData.Add(headerData);
					uniqueReferenceHeaders.Add(referenceHeader);
				}
			}
			return uniqueReferenceHeaders;
		}

		private void EditReferenceHeader(IReferenceHeader header)
		{
			using (var form = new EditReferenceHeaderForm())
			{
				form.Brand = header.Brand;
				form.Model = header.Model;
				form.Setting = header.Setting;

				if (DialogResult.OK == form.ShowDialog(this))
				{
					header.Brand = form.Brand;
					header.Model = form.Model;
					header.Setting = form.Setting;

					UpdateFilterOptions();
					UpdateReferenceHeaderList();
				}
			}
		}

		#region Event handlers
		private void comboBoxCameraBrand_SelectionChangeCommitted(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void comboBoxCameraModel_SelectionChangeCommitted(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void comboBoxInfo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void comboBoxWidth_SelectionChangeCommitted(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void comboBoxHeight_SelectionChangeCommitted(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void buttonClearFilters_Click(object sender, EventArgs e)
		{
			comboBoxCameraBrand.SelectedItem = comboBoxCameraBrand.Items[0];
			comboBoxCameraModel.SelectedItem = comboBoxCameraModel.Items[0];
			comboBoxInfo.SelectedItem = comboBoxInfo.Items[0];
			comboBoxWidth.SelectedItem = comboBoxWidth.Items[0];
			comboBoxHeight.SelectedItem = comboBoxHeight.Items[0];

			UpdateReferenceHeaderList();
		}

		private void checkBoxHideDuplicateHeaders_CheckedChanged(object sender, EventArgs e)
		{
			UpdateReferenceHeaderList();
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK == openVideoFileDialog.ShowDialog(this))
			{
				// TODO: Run this in a background thread ...
				string filename = openVideoFileDialog.FileName;
				IReferenceHeader addedReferenceHeader = ScanForReferenceHeader(filename);
				if (addedReferenceHeader == null)
				{
					MessageBox.Show("No headers of the supported video encoding types were found in file " + filename + ".",
									"No Reference Header Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					UpdateFilterOptions();
					UpdateReferenceHeaderList();

					HighlightAndScrollToReferenceHeader(addedReferenceHeader);
				}
			}
		}

		private IReferenceHeader ScanForReferenceHeader(string videoFilename)
		{
			// TODO: also scan the file using container detectors
			foreach (ICodecDetector codecDetector in CodecDetectors)
			{
				IReferenceHeader referenceHeader = Database.AddHeader(codecDetector, videoFilename);
				if (referenceHeader != null)
				{
					return referenceHeader;
				}
			}
			return null;
		}

		private void HighlightAndScrollToReferenceHeader(IReferenceHeader referenceHeader)
		{
			int referenceHeaderRowIndex = FindRowIndexForHeader(referenceHeader);
			referenceHeaderGridView.ClearSelection();
			referenceHeaderGridView.FirstDisplayedScrollingRowIndex = referenceHeaderRowIndex;
			referenceHeaderGridView.Rows[referenceHeaderRowIndex].Selected = true;
		}

		private int FindRowIndexForHeader(IReferenceHeader referenceHeader)
		{
			int rowCount = referenceHeaderGridView.Rows.Count;
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				if (referenceHeaderGridView.GetHeaderForRow(rowIndex) == referenceHeader)
				{
					return rowIndex;
				}
			}

			throw new ArgumentException("No such header in the database: " + referenceHeader);
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewCell cell in referenceHeaderGridView.SelectedCells)
			{
				if (cell.Value is IReferenceHeader)
				{
					Database.RemoveHeader(cell.Value as IReferenceHeader);
				}
			}

			UpdateFilterOptions();
			UpdateReferenceHeaderList();

			referenceHeaderGridView.ClearSelection();
		}

		private void buttonEdit_Click(object sender, EventArgs e)
		{
			var header = referenceHeaderGridView.GetHeaderForRow(referenceHeaderGridView.SelectedCells[0].RowIndex);
			if (header != null)
			{
				EditReferenceHeader(header);
			}
		}

		private void buttonImport_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK == openDatabaseFileDialog.ShowDialog(this))
			{
				try
				{
					Database.Import(openDatabaseFileDialog.FileName);

					UpdateFilterOptions();
					UpdateReferenceHeaderList();
				}
				catch (SerializationException exception)
				{
					MessageBox.Show(exception.Message, "Database is invalid: " + openDatabaseFileDialog.FileName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void buttonExport_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK == saveDatabaseFileDialog.ShowDialog(this))
			{
				try
				{
					Database.Export(saveDatabaseFileDialog.FileName);
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message, "Failed to export the database", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void listBoxCodec_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			IList<CodecID> selectedCodecs = GetSelectedCodecs();

			// Update the list of selected codecs according to the new state of the check box
			CodecID codecId = CodecForDescriptiveName(listBoxCodecs.Items[e.Index] as string);
			if (e.NewValue == CheckState.Unchecked)
			{
				selectedCodecs.Remove(codecId);
			}
			else if ((e.NewValue == CheckState.Checked) && !selectedCodecs.Contains(codecId))
			{
				selectedCodecs.Add(codecId);
			}

			UpdateReferenceHeaderList(selectedCodecs);
		}

		private void headerDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			HeaderSelected = (referenceHeaderGridView.SelectedCells.Count >= 1);
		}

		private void headerDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			var header = referenceHeaderGridView.GetHeaderForRow(e.RowIndex);
			if (header != null)
			{
				EditReferenceHeader(header);
			}
		}

		private void referenceHeaderGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
		{
			foreach (DataGridViewCell cell in e.Row.Cells)
			{
				if (cell.Value is IReferenceHeader)
				{
					Database.RemoveHeader(cell.Value as IReferenceHeader);
					break;
				}
			}

			UpdateReferenceHeaderList();

			referenceHeaderGridView.ClearSelection();
		}
		#endregion Event handlers
	}
}
