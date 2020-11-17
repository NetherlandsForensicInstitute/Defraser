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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.Interface;
using Defraser.Util;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	public partial class FileTree : Tree
	{
		private const string HexNumberFormat = "{0:X}";

		#region Events
		public event EventHandler<FileExportEventArgs<IInputFile>> SaveAsSingleFile;
		public event EventHandler<FileExportEventArgs<IEnumerable<IDataPacket>>> SaveAsContiguousFile;
		public event EventHandler<FileExportEventArgs<IEnumerable<object>>> SaveAsSeparateFiles;
		public event EventHandler<FileExportEventArgs<IEnumerable<IFragment>>> ExportToXml;
		public event EventHandler<EventArgs> GotoOffsetInHexWorkshop;
		public event EventHandler<EventArgs> GotoEndOffsetInHexWorkshop;
		#endregion Events

		#region Attributes
		// Icons for each level in the tree: Project (0), File (1), Data Block (2), Codec Stream (3)
		private readonly Icon[] _treeLevelIcons;
		private string _numericColumnFormat = HexNumberFormat;
		// Keep the names in the same sort direction after choosing an other sort column
		private ListSortDirection _nameSortDirection = ListSortDirection.Ascending;
		private IFragment _resultBeingRetrieved;
		#endregion Attributes

		#region Properties
		/// <summary>
		/// The file menu tool strip items.
		/// </summary>
		/// <remarks>
		/// Do not modify the contents of this array!
		/// </remarks>
		public ToolStripItem[] FileToolStripItems { get; private set; }
		public IInputFile FileBeingScanned { get; set; }
		public IFragment ResultBeingRetrieved
		{
			get
			{
				return _resultBeingRetrieved;
			}
			set
			{
				_resultBeingRetrieved = value;

				UpdateContextMenu();
			}
		}
		public int RetrieveResultProgressPercentage { get; set; }
		public bool DeleteEnabled { get; private set; }
		/// <summary>Set the display mode of numeric data types (hexadecimal or decimal).</summary>
		public DisplayMode DisplayMode
		{
			set
			{
				_numericColumnFormat = (value == DisplayMode.Hex) ? HexNumberFormat : "{0}";

				columnOffset.Caption = string.Format("Offset ({0})", value);
				columnLength.Caption = string.Format("Length ({0})", value);

				UpdateRows(false);
			}
		}
		public bool HexWorkshopAvailable
		{
			set
			{
				toolStripSeparator3.Visible = value;
				toolStripMenuItemGotoOffsetInHexWorkshop.Visible = value;
				toolStripMenuItemGotoEndOffsetInHexWorkshop.Visible = value;
			}
		}
		#endregion Properties

		public FileTree()
		{
			InitializeComponent();

			Init();

			FileToolStripItems = new ToolStripItem[contextMenuStrip.Items.Count];
			contextMenuStrip.Items.CopyTo(FileToolStripItems, 0);

			_treeLevelIcons = new Icon[imageList.Images.Count];

			for (int i = 0; i < imageList.Images.Count; i++)
			{
				_treeLevelIcons[i] = CreateIcon(imageList.Images[i]);
			}

			InitializeDataBindings();
		}

		private void InitializeDataBindings()
		{
			toolStripMenuItemDelete.DataBindings().Add("Enabled", this, "DeleteEnabled");
			toolStripMenuItemSaveAsSeparateFiles.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemGotoOffsetInHexWorkshop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemGotoEndOffsetInHexWorkshop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemSendSelectionTo.DataBindings().Add("Enabled", this, "HasSelectedRows");
		}

		private void Init()
		{
			UpdateContextMenu();

			// Remove the Column Chooser menu item from the Header context menu
			HeaderContextMenu.Items.RemoveAt(HeaderContextMenu.Items.Count - 1);
			// Remove the separator from the Header context menu
			HeaderContextMenu.Items.RemoveAt(HeaderContextMenu.Items.Count - 1);
			// Remove the 'Pinned' menu item from the Header context menu
			HeaderContextMenu.Items.RemoveAt(HeaderContextMenu.Items.Count - 1);
		}

		public void DeleteSelection()
		{
			if (!HasSelectedRows)
			{
				return; // Nothing to delete
			}

			// We want the keep the focus at the same row index after the row has been deleted.
			// Store the index of the row that has the focus for restoring it at the end of this method.
			int focusedRowIndex = SelectedRows[0].RowIndex;

			Row[] rows = SelectedRows.GetUniqueRowsInGuiOrder();

			// Clear the selection to avoid intermediate updates
			SelectedRows.Clear();

			var project = DataSource as IProject;
			foreach (Row row in rows)
			{
				if (row.Item is IInputFile)
				{
					DeleteInputFile(project, row.Item as IInputFile);
				}
				else if (row.Item is IDataBlock)
				{
					DeleteDataBlock(project, row.Item as IDataBlock);
				}
			}

			// Put the focus back at the same row index,
			// or at the row index above if the last row was removed
			FocusRow = GetRow(focusedRowIndex) ?? RootRow.ChildRowByIndex(RootRow.NumChildren - 1);
		}

		private void DeleteInputFile(IProject project, IInputFile inputFile)
		{
			if (inputFile == FileBeingScanned)
			{
				if (DialogResult.Yes != MessageBox.Show("The file '" + inputFile.Name + "' you selected to remove is currently being scanned. "
				                                        + "Do you want to stop the scan process and remove the partly scanned file?",
				                                        "File currently being scanned", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
				{
					return; // File delete was aborted
				}
				//if (!_backgroundFileScanner.Stop())
				//{
				//    return;
				//}

				//_backgroundDataBlockScanner.Stop();
			}

			project.DeleteFile(inputFile);
		}

		private void DeleteDataBlock(IProject project, IDataBlock dataBlock)
		{
			// TODO: What if 'ResultBeingRetrieved' is a codec stream of 'dataBlock' ???

			if (ResultBeingRetrieved == dataBlock)
			{
				//_backgroundDataBlockScanner.Stop();
			}

			project.DeleteDataBlock(dataBlock);
		}

		public virtual void CreateSendToSubmenu(ToolStripMenuItem toolStripMenuItem)
		{
			// TODO
		}

		protected void UpdateContextMenu()
		{
			// Rules:
			// - The 'Send To' and 'Save As' context menu items are disabled
			//   for the items the data is retrieved for.
			// - The Separate files can only be saved when one row containing an IInputFile object
			//   has the focus.
			// - Enable Export to Xml when there is a selection and no InputFile is selected
			// ...

			// Default (selection.count == 0)
			toolStripMenuItemSendSelectionTo.Enabled = false;
			toolStripMenuItemSaveAsContiguousFile.Enabled = false;
			toolStripMenuItemExportToXml.Enabled = false;

			if (HasSelectedRows)
			{
				// Enable Export to Xml when there is a selection and no InputFile is selected
				toolStripMenuItemExportToXml.Enabled = !AnySelectedIsFile(SelectedRows);

				if (ResultBeingRetrieved == null)
				{
					if (AllSelectedAreVideoBlocks(SelectedRows) || ((SelectedRows.Count == 1) && AnySelectedIsFile(SelectedRows)))
					{
						toolStripMenuItemSendSelectionTo.Enabled = true;
						toolStripMenuItemSaveAsContiguousFile.Enabled = true;
					}
				}
			}
		}

		private static bool AllSelectedAreVideoBlocks(RowSelectionList selection)
		{
			foreach (Row row in selection)
			{
				if (!IsVideoBlock(row.Item))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsVideoBlock(object item)
		{
			return (item is IDataBlock) || (item is ICodecStream);
		}

		private static bool AnySelectedIsFile(RowSelectionList selection)
		{
			foreach (Row row in selection)
			{
				if (row.Item is IInputFile)
				{
					return true;
				}
			}
			return false;
		}

		#region Programmatic data binding
		protected override object GetParentForItem(object item)
		{
			if (item is IInputFile)
			{
				return DataSource as IProject;
			}
			if (item is IDataBlock)
			{
				return (item as IDataBlock).InputFile;
			}
			if (item is ICodecStream)
			{
				return (item as ICodecStream).DataBlock;
			}

			return base.GetParentForItem(item);
		}

		protected override IList GetChildrenForRow(Row row)
		{
			// When the user chooses another column as the sort column,
			// keep the name column sorted the way it was last sorted
			if (SortColumn == columnName)
			{
				_nameSortDirection = SortColumn.SortDirection;
			}

			var project = DataSource as IProject;
			if (project != null)
			{
				if (row.Item == project)
				{
					return project.GetInputFiles().OrderBy(f => f.Name, _nameSortDirection).ToList().AsReadOnly();
				}
				if (row.Item is IInputFile)
				{
					return SortDataBlocks(project.GetDataBlocks(row.Item as IInputFile), SortColumn.DataField, SortColumn.SortDirection).ToList().AsReadOnly();
				}
				if (row.Item is IDataBlock)
				{
					var dataBlock = row.Item as IDataBlock;
					if (dataBlock.CodecStreams.Count > 0)
					{
						return dataBlock.CodecStreams.ToList().AsReadOnly();
					}
				}
			}
			return null;
		}

		private static IEnumerable<IDataBlock> SortDataBlocks(IEnumerable<IDataBlock> dataBlocks, string column, ListSortDirection sortDirection)
		{
			switch (column)
			{
				case "Name":
					return dataBlocks;
				case "Detector":
					return dataBlocks.OrderBy(d => d.Detectors.First().Name, sortDirection);
				case "DetectorVersion":
					return dataBlocks.OrderBy(d => GetDetectorVersion(d.Detectors.First()), sortDirection);
				case "Offset":
					return dataBlocks.OrderBy(d => d.StartOffset, sortDirection);
				case "Length":
					return dataBlocks.OrderBy(d => d.Length, sortDirection);
				case "EndOffset":
					return dataBlocks.OrderBy(d => d.EndOffset, sortDirection);
			}

			throw new ArgumentException("Invalid sort column", "column");
		}

		protected override void OnGetRowData(Row row, RowData rowData)
		{
			base.OnGetRowData(row, rowData);

			Debug.Assert(row.Level - 1 < _treeLevelIcons.Length);

			rowData.Icon = _treeLevelIcons[row.Level - 1];
		}

		protected override void OnGetCellData(Row row, Column column, CellData cellData)
		{
			if (row.Item is IInputFile)
			{
				GetCellDataFile(row.Item as IInputFile, column, cellData);
			}
			else if (row.Item is IFragment)
			{
				GetCellDataFragment(row.Item as IFragment, column, cellData);
			}
		}

		private void GetCellDataFile(IInputFile inputFile, Column column, CellData cellData)
		{
			if (column == columnName)
			{
				cellData.Value = (new FileInfo(inputFile.Name)).Name;
				cellData.ToolTip = inputFile.Name;
				cellData.AlwaysDisplayToolTip = true;
			}
		}

		private void GetCellDataFragment(IFragment fragment, Column column, CellData cellData)
		{
			if (column == columnName)
			{
				cellData.Value = (ResultBeingRetrieved == fragment)
				                 ? string.Format("Retrieving data {0}%", RetrieveResultProgressPercentage)
				                 : fragment.GetDescriptiveName();
			}
			else if (column == columnDetector)
			{
				IDetector detector = fragment.Detectors.First();
				cellData.Value = IsUnknownDetector(detector) ? "Not available" : detector.Name;
			}
			else if (column == columnDetectorVersion)
			{
				cellData.Value = GetDetectorVersion(fragment.Detectors.First());
			}
			else if (column == columnOffset)
			{
				if (fragment is IDataBlock)
				{
					cellData.Format = _numericColumnFormat;
					cellData.Value = fragment.StartOffset;
				}
			}
			else if (column == columnLength)
			{
				cellData.Format = _numericColumnFormat;
				cellData.Value = fragment.Length;
			}
		}

		private static string GetDetectorVersion(IDetector detector)
		{
			if (IsUnknownDetector(detector))
			{
				return string.Empty; // Unknown detector
			}

			Version version = detector.GetType().Assembly.GetName().Version;
			return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
		}

		private static bool IsUnknownDetector(IDetector detector)
		{
			return detector.SupportedFormats.Count() == 0;
		}
		#endregion Programmatic data binding

		#region Event handlers
		private void FileTree_DataSourceChanged(object sender, EventArgs e)
		{
			var project = DataSource as IProject;
			if (project != null)
			{
				project.ProjectChanged += Project_ProjectChanged;
			}
		}

		private void Project_ProjectChanged(object sender, ProjectChangedEventArgs e)
		{
			switch (e.Type)
			{
				case ProjectChangedType.FileDeleted:
				case ProjectChangedType.DataBlockDeleted:
					{
						Row row = FindRow(e.Item);
						if (row.HasFocus)
						{
							SetFocusRow(row.ParentRow, false);
						}
						row.ParentRow.UpdateChildren(true, false);
					}
					break;
				case ProjectChangedType.FileAdded:
					{
						RootRow.UpdateChildren(true, false);
					}
					break;
				case ProjectChangedType.DataBlockAdded:
					{
						Row row = FindRow(((IDataBlock)e.Item).InputFile);
						if (row != null)
						{
							row.UpdateChildren(true, false);
						}
						else
						{
							RootRow.UpdateChildren(true, false);
						}
					}
					break;
			}
		}

		private void FileTree_SelectionChanged(object sender, EventArgs e)
		{
			bool b = ContainsDataBlocksOrFiles(SelectedRows);
			if (DeleteEnabled != b)
			{
				DeleteEnabled = b;

				OnPropertyChanged(new PropertyChangedEventArgs("DeleteEnabled"));
			}

			UpdateContextMenu();
		}

		// Only rows != codec stream can be deleted.
		// If the selection consists of codec streams only,
		// do not enable the delete menu option.
		private static bool ContainsDataBlocksOrFiles(RowSelectionList rows)
		{
			foreach (Row row in rows)
			{
				if ((row != null) && ((row.Item is IInputFile) || (row.Item is IDataBlock)))
				{
					return true;
				}
			}
			return false;
		}

		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			contextMenuStrip.Items.Clear();

			toolStripMenuItemSendSelectionTo.DropDownItems.Clear();

			if (HasSelectedRows)
			{
				CreateSendToSubmenu(toolStripMenuItemSendSelectionTo);
			}

			contextMenuStrip.Items.AddRange(FileToolStripItems);
		}

		private void toolStripMenuItemSaveAsContiguousFile_Click(object sender, EventArgs e)
		{
			if (!HasSelectedRows)
			{
				MessageBox.Show(this, "No selection was made. Please make a selection in the file tree first.",
				                GetDisplayedText(toolStripMenuItemSaveAsContiguousFile),
				                MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				return;
			}

			var fileDialog = new SaveFilePresenter();
			fileDialog.FilterAllFiles();
			fileDialog.Title = "Save As Contiguous File";

			// Using the GUI, it is only possible to select one file or one or more container and codec streams
			Row firstSelectedRow = SelectedRows[0];
			if (firstSelectedRow.Item is IInputFile)
			{
				if (SaveAsSingleFile != null)
				{
					FileInfo contiguousFile;
					if (fileDialog.ShowDialog(this, out contiguousFile))
					{
						RunBackgroundTask(toolStripMenuItemSaveAsContiguousFile, "Save as " + contiguousFile.Name,
						                  p => SaveAsSingleFile(this, new FileExportEventArgs<IInputFile>(firstSelectedRow.Item as IInputFile, contiguousFile.FullName, p)));
					}
				}
			}
			else if (firstSelectedRow.Item is IFragment)
			{
				if (SaveAsContiguousFile != null)
				{
					// Fill the SaveFileDialog filter
					var fragment = firstSelectedRow.Item as IFragment;
					IDetector detector;
					if ((fragment != null) &&
						(fragment.Detectors != null) &&
						(fragment.Detectors.Count() > 0) &&
						((detector = fragment.Detectors.FirstOrDefault()) != null))
					{
						string extension = detector.OutputFileExtension;
						fileDialog.Filter = string.Format("{0} Files (*{1})|*{2}|All Files (*.*)|*.*", detector.Name, extension, extension);
					}

					FileInfo contiguousFile;
					if (fileDialog.ShowDialog(this, out contiguousFile))
					{
						RunBackgroundTask(toolStripMenuItemSaveAsContiguousFile, "Save as " + contiguousFile.Name,
						                  p => SaveAsContiguousFile(this, new FileExportEventArgs<IEnumerable<IDataPacket>>(SelectedRows.GetSelectedFragmentsInGuiOrder(), contiguousFile.FullName, p)));
					}
				}
			}
		}

		private void toolStripMenuItemSaveAsSeparateFiles_Click(object sender, EventArgs e)
		{
			string caption = GetDisplayedText(toolStripMenuItemSaveAsSeparateFiles);

			if (!HasSelectedRows)
			{
				MessageBox.Show(this, "No selection was made. Please make a selection in the file tree first.",
				                caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				return;
			}

			if ((SaveAsSeparateFiles != null) && SelectOutputFolder())
			{
				var outputFolder = new DirectoryInfo(folderBrowserDialog.SelectedPath);
				var items = SelectedRows.GetUniqueRowsInGuiOrder().ToList().ConvertAll(r => r.Item);

				RunBackgroundTask(toolStripMenuItemSaveAsSeparateFiles, "Save to directory " + outputFolder.FullName,
				                  p => SaveAsSeparateFiles(this, new FileExportEventArgs<IEnumerable<object>>(items, outputFolder.FullName, p)));
			}
		}

		private bool SelectOutputFolder()
		{
			if (DialogResult.OK != folderBrowserDialog.ShowDialog(this))
			{
				return false; // No directory selected
			}

			string directory = folderBrowserDialog.SelectedPath;
			if (!String.IsNullOrEmpty(directory) && Directory.Exists(directory))
			{
				return true; // Output folder selected!
			}
			if (DialogResult.OK != MessageBox.Show(String.IsNullOrEmpty(directory) ? "No folder has been provided. Please review your choice."
			                                                                       : "The selected folder does not exist. Please review your choice.",
			                                       "Invalid choice", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
			{
				return false; // Canceled
			}

			return SelectOutputFolder(); // Retry!
		}

		private void toolStripMenuItemExportToXml_Click(object sender, EventArgs e)
		{
			IFragment[] fragments = SelectedRows.GetSelectedFragmentsInGuiOrder();
			if ((fragments == null) || (fragments.Length == 0))
			{
				MessageBox.Show(this, "Nothing to be exported was selected. Please make a selection in the file tree first.",
				                GetDisplayedText(toolStripMenuItemSaveAsContiguousFile),
				                MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				return;
			}

			var saveFileDialog = new SaveFilePresenter();
			saveFileDialog.FilterXml();
			FileInfo xmlFile;
			if (saveFileDialog.ShowDialog(this, out xmlFile))
			{
				try
				{
					if (ExportToXml != null)
					{
						RunBackgroundTask(toolStripMenuItemExportToXml, "Export to " + xmlFile.Name,
						                  p => ExportToXml(this, new FileExportEventArgs<IEnumerable<IFragment>>(fragments, xmlFile.FullName, p)));
					}
				}
				catch (IOException ex)
				{
					MessageBox.Show(this, "An error occurred when trying to write to file '" + xmlFile.FullName + "'" + Environment.NewLine + ex.Message, "Error writing file");
				}
			}
		}

		private static void RunBackgroundTask(ToolStripItem toolStripMenuItem, string progressText, Action<IProgressReporter> task)
		{
			var workerProgressReporter = new FileExportProgressForm();
			workerProgressReporter.Text = GetDisplayedText(toolStripMenuItem);
			workerProgressReporter.ProgressLabelText = progressText;
			workerProgressReporter.DoWork += (sender, e) => task(workerProgressReporter);
			workerProgressReporter.Show();
		}

		/// <summary>
		/// Returns the <see cref="ToolStripItem.Text">Text</see> of <paramref name="toolStripItem"/>
		/// with the special characters '&' and trailing '...' removed.
		/// </summary>
		/// <param name="toolStripItem">The item to return the displayed text of</param>
		/// <returns>The displayed text</returns>
		private static string GetDisplayedText(ToolStripItem toolStripItem)
		{
			string s = toolStripItem.Text.Replace("&", "");
			return s.EndsWith("...") ? s.Substring(0, (s.Length - 3)) : s;
		}

		private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows)
			{
				DeleteSelection();
			}
		}

		private void toolStripMenuItemGotoOffsetInHexWorkshop_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows && (GotoOffsetInHexWorkshop != null))
			{
				GotoOffsetInHexWorkshop(this, EventArgs.Empty);
			}
		}

		private void toolStripMenuItemGotoEndOffsetInHexWorkshop_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows && (GotoEndOffsetInHexWorkshop != null))
			{
				GotoEndOffsetInHexWorkshop(this, EventArgs.Empty);
			}
		}
		#endregion Event handlers
	}
}
