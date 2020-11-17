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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Defraser.DataStructures;
using Defraser.Framework;
using Defraser.GuiCe.sendtolist;
using Defraser.Interface;
using Defraser.Util;
using Infralution.Controls;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// The type of headers to select.
	/// </summary>
	internal enum HeaderSelectionType
	{
		/// <summary>
		/// The selected headers.
		/// </summary>
		SelectedHeaders,
		/// <summary>
		/// The selected headers, which will be converted to H.264 byte stream
		/// format upon file export.
		/// </summary>
		SelectedHeadersInH264ByteStream,
		/// <summary>
		/// All headers of the selected types will be copied.
		/// Children will also be copied.
		/// </summary>
		AllHeadersOfSelectedTypeIncludingChildren,
		/// <summary>
		/// All headers of the selected header types will be copied.
		/// Children will not be copied.
		/// </summary>
		AllHeadersOfSelectedTypeExcludingChildren,
		/// <summary>
		/// All children of the selected header types will be copied.
		/// The selected header types themself in will be left alone.
		/// </summary>
		AllChildrenOfSelectedHeaderTypeExcludingHeaderItself
	}

	/// <summary>
	/// Displays the header tree.
	/// The <c>DataSource</c> is the root node of the headers to display.
	/// </summary>
	public sealed partial class HeaderTree : Tree
	{
		#region Inner classes
		/// <summary>
		/// Handles drag and drop events on the empty area.
		/// </summary>
		private class DragDropPanelWidget : PanelWidget
		{
			#region Properties
			internal HeaderTree OuterClass { get { return Tree as HeaderTree; } }
			#endregion Properties


			public DragDropPanelWidget(VirtualTree tree)
				: base(tree)
			{
			}

			public override void OnDragEnter(DragEventArgs e)
			{
				e.Effect = OuterClass.RowDropEffect(Tree.RootRow, RowDropLocation.OnRow, e.Data);
			}

			public override void OnDragOver(DragEventArgs e)
			{
				e.Effect = OuterClass.RowDropEffect(Tree.RootRow, RowDropLocation.OnRow, e.Data);
			}

			public override void OnDragDrop(DragEventArgs e)
			{
				e.Effect = OuterClass.RowDropEffect(Tree.RootRow, RowDropLocation.OnRow, e.Data);

				OuterClass.OnRowDrop(Tree.RootRow, RowDropLocation.OnRow, e.Data, e.Effect);
			}
		}

		/// <summary>
		/// Describes a selection in the <c>HeaderTree</c>.
		/// </summary>
		private class Selection : ISelection
		{
			private readonly HeaderTree _headerTree;
			private readonly HeaderSelectionType _selectionType;

			#region Properties
			public bool IsEmpty
			{
				get
				{
					// TODO: optimize, stop on first result found
					return Results.Length == 0;
				}
			}

			public IDataPacket DataPacket
			{
				get
				{
					IDataPacket dataPacket = null;
					if (_selectionType == HeaderSelectionType.SelectedHeadersInH264ByteStream)
					{
						IDataPacket h264StartCodePrefixDataPacket = _headerTree.CreateH264StartCodePrefixDataPacket(Results.First().InputFile.Project);
						foreach (IResultNode result in Results)
						{
							dataPacket = Append(dataPacket, CreateDataPacketAsH264ByteStream(result, h264StartCodePrefixDataPacket));
						}
					}
					else
					{
						foreach (IResultNode result in Results)
						{
							dataPacket = Append(dataPacket, CreateDataPacket(result));
						}
					}
					return dataPacket;
				}
			}

			private static IDataPacket Append(IDataPacket dataPacket1, IDataPacket dataPacket2)
			{
				return (dataPacket1 == null) ? dataPacket2 : dataPacket1.Append(dataPacket2);
			}

			public IResultNode[] Results
			{
				get
				{
					IResultNode[] results = _headerTree.GetSelectedResults();
					if (results.Length == 0 || _selectionType == HeaderSelectionType.SelectedHeaders || _selectionType == HeaderSelectionType.SelectedHeadersInH264ByteStream)
					{
						return results;
					}

					// Create a flat list containing the unique header type names.
					List<string> headerTypes = new List<string>();
					foreach (IResultNode result in results)
					{
						if (result != null && !headerTypes.Contains(result.Name))
						{
							headerTypes.Add(result.Name);
						}
					}

					// Get the result nodes
					IResultNode root = _headerTree.RootItem as IResultNode;
					List<IResultNode> resultNodes = new List<IResultNode>();
					switch (_selectionType)
					{
						case HeaderSelectionType.AllHeadersOfSelectedTypeIncludingChildren:
							GetHeadersOfType(root, resultNodes, headerTypes, true, true);
							break;
						case HeaderSelectionType.AllHeadersOfSelectedTypeExcludingChildren:
							GetHeadersOfType(root, resultNodes, headerTypes, false, true);
							break;
						case HeaderSelectionType.AllChildrenOfSelectedHeaderTypeExcludingHeaderItself:
							GetHeadersOfType(root, resultNodes, headerTypes, true, false);
							break;
						default:
							throw new InvalidOperationException("Invalid selection type.");
					}
					return resultNodes.ToArray();
				}
			}

			public string OutputFileExtension
			{
				get
				{
					if (_headerTree.HasSelectedRows)
					{
						IResultNode result = _headerTree.SelectedRows[0].Item as IResultNode;
						if (result != null && result.Detectors != null && result.Detectors.Count() > 0)
						{
							return result.Detectors.First().OutputFileExtension;
						}
					}
					return ".bin";
				}
			}

			public object FocusItem
			{
				get { return _headerTree.FocusItem; }
			}

			#endregion Properties

			/// <summary>
			/// Creates a new header selection.
			/// </summary>
			/// <param name="headerTree">the tree for the selection</param>
			/// <param name="selectionType">the type of headers to select</param>
			public Selection(HeaderTree headerTree, HeaderSelectionType selectionType)
			{
				_headerTree = headerTree;
				_selectionType = selectionType;
			}

			/// <summary>
			/// Gets the data packet for the given <paramref name="result"/>
			/// and its descendents.
			/// </summary>
			/// <param name="result">the result</param>
			/// <returns>the data packet</returns>
			private static IDataPacket CreateDataPacket(IResultNode result)
			{
				IDataPacket dataPacket = result;
				foreach (IResultNode childResult in result.Children)
				{
					dataPacket = dataPacket.Append(CreateDataPacket(childResult));
				}
				return dataPacket;
			}

			private static IDataPacket CreateDataPacketAsH264ByteStream(IResultNode result, IDataPacket h264StartCodePrefixDataPacket)
			{
				IDataPacket dataPacket = ProducePlayableExport(result, h264StartCodePrefixDataPacket);
				foreach (IResultNode childResult in result.Children)
				{
					dataPacket = dataPacket.Append(CreateDataPacketAsH264ByteStream(childResult, h264StartCodePrefixDataPacket));
				}
				return dataPacket;
			}

			private static IDataPacket ProducePlayableExport(IResultNode result, IDataPacket h264StartCodePrefixDataPacket)
			{
				if (result.DataFormat != CodecID.H264)
				{
					return result;
				}
				if (result.FindAttributeByName("StartCodePrefix") != null)
				{
					return result;
				}

				string nalUnitType = result.FindAttributeByName("NalUnitType").ValueAsString;
				int lengthPrefixBytes = ((nalUnitType == "7") || (nalUnitType == "8")) ? 2 : 4;
				return h264StartCodePrefixDataPacket.Append(result.GetSubPacket(lengthPrefixBytes, (result.Length - lengthPrefixBytes)));
			}

			/// <summary>
			/// Gets all headers of the given <paramref name="headerTypes"/>.
			/// </summary>
			/// <param name="root">the root node</param>
			/// <param name="headers">the list that receives the headers</param>
			/// <param name="headerTypes">the types of headers to include</param>
			/// <param name="includeChildren">whether to include the children</param>
			/// <param name="includeHeader">whether to include the header itself</param>
			private static void GetHeadersOfType(IResultNode root, List<IResultNode> headers, ICollection<string> headerTypes, bool includeChildren, bool includeHeader)
			{
				if (headerTypes.Contains(root.Name))
				{
					if (includeChildren)
					{
						// Include the header as-is, including its children
						if (includeHeader)
						{
							headers.Add(root);
						}
						else
						{
							headers.AddRange(root.Children);
						}
					}
					else
					{
						// Make shallow copies (exclude children)
						if (includeHeader) // Copy the header
						{
							headers.Add(root.ShallowCopy());

							foreach (IResultNode child in root.Children)
							{
								GetHeadersOfType(child, headers, headerTypes, includeChildren, includeHeader);
							}
						}
						else
						{
							foreach (IResultNode child in root.Children)
							{
								headers.Add(child.ShallowCopy());

								foreach (IResultNode grandChild in child.Children)
								{
									GetHeadersOfType(grandChild, headers, headerTypes, includeChildren, includeHeader);
								}
							}
						}
					}
				}
				else
				{
					// Header not in listed types, check children
					foreach (IResultNode childResultNode in root.Children)
					{
						GetHeadersOfType(childResultNode, headers, headerTypes, includeChildren, includeHeader);
					}
				}
			}

			public IDataPacket GetDataPacket(IProgressReporter progressReporter)
			{
				return DataPacket;
			}
		}
		#endregion Inner classes

		private readonly WorkpadManager _workpadManager;
		private readonly SendToList _sendToList;
		private readonly IFileExport _fileExport;
		private readonly IDetectorFormatter _detectorFormatter;
		private readonly Creator<IInputFile, IProject, string> _createInputFile;
		private readonly IDictionary<HeaderSelectionType, ISelection> _selections;
		private DisplayMode _displayMode;
		private StyleDelta _horzAlignmentNearStyleDelta;
		private StyleDelta _horzAlignmentFarStyleDelta;
		private List<ToolStripItem> _fileToolStripItems;
		private List<ToolStripItem> _editToolStripItems;

		#region Properties
		public override object DataSource
		{
			get { return base.DataSource; }
			set
			{
				try
				{
					SuspendSelectionChangedEvents();
					base.DataSource = value;
				}
				finally
				{
					ResumeSelectionChangedEvents();
				}
			}
		}

		/// <summary>The file menu tool strip items.</summary>
		public ToolStripItem[] FileToolStripItems { get { return _fileToolStripItems.ToArray(); } }
		/// <summary>The edit menu tool strip items.</summary>
		public ToolStripItem[] EditToolStripItems
		{
			get
			{
				EnableDeleteAllMenu();
				return _editToolStripItems.ToArray();
			}
		}

		public DisplayMode DisplayMode
		{
			get { return _displayMode; }
			set
			{
				_displayMode = value;

				if (Columns == null || Columns.Count == 0) return;

				IColumnInfo offsetColumnInfo = DefaultColumnIndex.Offset.GetColumnInfo(_displayMode);
				IColumnInfo lengthColumnInfo = DefaultColumnIndex.Length.GetColumnInfo(_displayMode);
				IColumnInfo endOffsetColumnInfo = DefaultColumnIndex.EndOffset.GetColumnInfo(_displayMode);

				Columns[offsetColumnInfo.Name].Caption = offsetColumnInfo.Caption;
				Columns[lengthColumnInfo.Name].Caption = lengthColumnInfo.Caption;
				Columns[endOffsetColumnInfo.Name].Caption = endOffsetColumnInfo.Caption;

				UpdateRows(false);
			}
		}

		/// <summary>
		/// Returns whether the header tree belongs to a workpad.
		/// </summary>
		public bool IsWorkpad { get { return AllowDrop; } }

		public string SortColumnName
		{
			get { return (SortColumn == null) ? null : SortColumn.Name; }
			set { if (SortColumn != null) { SortColumn.Name = value; } }
		}

		public ListSortDirection SortColumnDirection
		{
			get { return (SortColumn == null) ? ListSortDirection.Ascending : SortColumn.SortDirection; }
			set { if (SortColumn != null) { SortColumn.SortDirection = value; } }
		}

		/// <summary>
		/// The <c>Workpad</c> ancestor of this panel.
		/// If there is one; it could also be the MainForm
		/// </summary>
		internal Workpad ParentWorkpad
		{
			get
			{
				Control control;
				Workpad workpad = null;

				for (control = Parent; control != null; control = control.Parent)
				{
					workpad = control as Workpad;
					if (workpad != null) break;
				}
				return workpad;
			}
		}

		/// <summary>
		/// When the tree is on a workpad; this method returns the name of that workpad.
		/// </summary>
		public String WorkpadName
		{
			get
			{
				Workpad workpad = ParentWorkpad;
				if(workpad != null) return workpad.WorkpadName;
				return String.Empty;
			}
		}

		public bool SelectionStateForToolStripItems
		{
			get { return false; }
			set
			{
				// Enable/disable selection based tool strip menu items
				toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself.Enabled = value && !Selections[HeaderSelectionType.AllChildrenOfSelectedHeaderTypeExcludingHeaderItself].IsEmpty;
			}
		}

		internal IDictionary<HeaderSelectionType, ISelection> Selections
		{
			get { return _selections; }
		}

		internal bool IsH264NalUnitStreamResultSelected
		{
			get
			{
				if (!HasSelectedRows)
				{
					return false; // no selection
				}

				foreach (Row row in SelectedRows)
				{
					var resultNode = row.Item as IResultNode;
					if ((resultNode != null) && IsH264NalUnitStreamResult(resultNode))
					{
						return true;
					}
				}
				return false;
			}
		}

		private IResultNode[] SelectedResults
		{
			get { return Selections[HeaderSelectionType.SelectedHeaders].Results; }
		}
		#endregion Properties

		public HeaderTree(WorkpadManager workpadManager, SendToList sendToList, IFileExport fileExport, IDetectorFormatter detectorFormatter, Creator<IInputFile, IProject, string> createInputFile)
		{
			_workpadManager = workpadManager;
			_sendToList = sendToList;
			_fileExport = fileExport;
			_detectorFormatter = detectorFormatter;
			_createInputFile = createInputFile;
			_selections = new Dictionary<HeaderSelectionType, ISelection>();

			foreach (HeaderSelectionType selectionType in Enum.GetValues(typeof(HeaderSelectionType)))
			{
				_selections[selectionType] = new Selection(this, selectionType);
			}

			InitializeComponent();

			Init();

			InitializeDataBindings();
		}

		private void InitializeDataBindings()
		{
			toolStripMenuItemSendSelectionTo.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemSaveSelectionAs.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemDeleteSelection.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemDeleteSelectedHeaderTypes.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemMoveToTop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemGotoOffsetInHexWorkshop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemGotoEndOffsetInHexWorkshop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren.DataBindings().Add("Enabled", this, "HasSelectedRows");
			toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren.DataBindings().Add("Enabled", this, "HasSelectedRows");

			this.DataBindings.Add("SelectionStateForToolStripItems", this, "HasSelectedRows");

			// Delete all should be enabled when the root row contains children, else it should be disabled
			// It is enabled/disabled in the EditToolStripItems property
			//toolStripMenuItemDeleteAll.Enabled = this.RootRow != null && this.RootRow.NumChildren > 0;
		}

		private void Init()
		{
			DisplayMode = DisplayMode.Hex;

			_workpadManager.WorkpadsChanged += WorkpadManager_WorkpadsChanged;
			_sendToList.CollectionChanged += SendToList_CollectionChanged;

			WorkpadManager_WorkpadsChanged(this, EventArgs.Empty);

			// Initialize attribute value alignment styles
			_horzAlignmentNearStyleDelta = new StyleDelta {HorzAlignment = StringAlignment.Near};

			_horzAlignmentFarStyleDelta = new StyleDelta {HorzAlignment = StringAlignment.Far};

			// Create the tool strip item lists
			_fileToolStripItems = new List<ToolStripItem>
			                      	{
			                      		toolStripMenuItemSendSelectionTo,
			                      		toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself,
			                      		toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren,
			                      		toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren,
			                      		toolStripSeparator1,
			                      		toolStripMenuItemSaveSelectionAs
			                      	};
			if (_sendToList.HexWorkshopAvailable())
			{
				_fileToolStripItems.Add(toolStripSeparator4);
				_fileToolStripItems.Add(toolStripMenuItemGotoOffsetInHexWorkshop);
				_fileToolStripItems.Add(toolStripMenuItemGotoEndOffsetInHexWorkshop);
				_fileToolStripItems.Add(toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop);
			}

			_editToolStripItems = new List<ToolStripItem>
			                      	{
			                      		toolStripMenuItemDeleteSelection,
			                      		toolStripMenuItemDeleteSelectedHeaderTypes,
			                      		toolStripMenuItemDeleteAll,
			                      		toolStripSeparator3,
			                      		toolStripMenuItemMoveToTop
			                      	};
		}

		#region Programmatic data binding
		protected override object GetParentForItem(object item)
		{
			IResultNode result = item as IResultNode;

			return (result == null) ? null : result.Parent;
		}

		protected override IList GetChildrenForRow(Row row)
		{
			IResultNode result = row.Item as IResultNode;

			if (result == null) return null;

			if (SortColumnName == null)
			{
				return result.Children.ToList().AsReadOnly();
			}

			// TODO make SendToWorkpad work using the second line
			return new ReadOnlyCollection<IResultNode>(SortResults.Sort(result.Children, SortColumnName, SortColumnDirection));
			//result.Children.Sort(SortColumnName, SortColumnDirection).ToList().AsReadOnly();
		}

		protected override void OnGetRowData(Row row, RowData rowData)
		{
			base.OnGetRowData(row, rowData);

			IResult result = row.Item as IResult;

			if (result == null) return;

			rowData.Icon = result.Valid ? ValidIcon : InvalidIcon;

			if ((result is IResultNode) && (result as IResultNode).InputFile is IReferenceHeaderFile)
			{
				rowData.Icon = GeneratedIcon;
			}
		}

		protected override void OnGetCellData(Row row, Column column, CellData cellData)
		{
			base.OnGetCellData(row, column, cellData);

			if (column.Name == null) return;

			IResultNode result = row.Item as IResultNode;
			if (result == null)
			{
				return;
			}

			DefaultColumnIndex index;
			if (DefaultColumnExtensions.TryParse(column.Name, out index))
			{
				switch (index)
				{
					case DefaultColumnIndex.Name:
						cellData.Value = result.Name;

						if (result.InputFile is IReferenceHeaderFile)
						{
							cellData.EvenStyle = new Style(cellData.EvenStyle) { ForeColor = Color.RoyalBlue, Font = new Font(cellData.EvenStyle.Font.FontFamily, cellData.EvenStyle.Font.SizeInPoints, FontStyle.Bold) };
							cellData.OddStyle = new Style(cellData.OddStyle) { ForeColor = Color.RoyalBlue, Font = new Font(cellData.OddStyle.Font.FontFamily, cellData.OddStyle.Font.SizeInPoints, FontStyle.Bold) };
						}
						break;
					case DefaultColumnIndex.Detector:
						cellData.Value = result.Detectors.First().Name;
						break;
					case DefaultColumnIndex.DetectorVersion:
						cellData.Value = _detectorFormatter.FormatVersion(result.Detectors.First());
						break;
					case DefaultColumnIndex.Offset:
						cellData.Value = result.StartOffset;

						if (DisplayMode == DisplayMode.Hex)
						{
							cellData.Format = "{0:X}";
						}
						break;
					case DefaultColumnIndex.Length:
						cellData.Value = result.Length;

						if (DisplayMode == DisplayMode.Hex)
						{
							cellData.Format = "{0:X}";
						}
						break;
					case DefaultColumnIndex.EndOffset:
						cellData.Value = result.EndOffset;

						if (DisplayMode == DisplayMode.Hex)
						{
							cellData.Format = "{0:X}";
						}
						break;
					case DefaultColumnIndex.File:
						IInputFile inputFile = result.InputFile;
						if (inputFile != null)
						{
							cellData.Value = (new FileInfo(inputFile.Name)).Name;
							cellData.ToolTip = inputFile.Name;
							cellData.AlwaysDisplayToolTip = true;
						}
						break;
				}
			}
			else
			{
				IResultAttribute attribute = result.FindAttributeByName(column.Name);
				if (attribute == null)
				{
					cellData.Value = string.Empty;
				}
				else
				{
					cellData.Value = attribute.ValueAsString;

					// Align the cell content depending its value.
					if ((attribute.Value.GetType() == typeof(string)) ||
						(attribute.Value.GetType() == typeof(bool)))
					{
						cellData.EvenStyle = new Style(cellData.EvenStyle, _horzAlignmentNearStyleDelta);
						cellData.OddStyle = new Style(cellData.OddStyle, _horzAlignmentNearStyleDelta);
					}
					else
					{
						cellData.EvenStyle = new Style(cellData.EvenStyle, _horzAlignmentFarStyleDelta);
						cellData.OddStyle = new Style(cellData.OddStyle, _horzAlignmentFarStyleDelta);
					}
				}
			}
		}
		#endregion Programmatic data binding

		#region Drag and Drop
		protected override PanelWidget CreatePanelWidget()
		{
			return new DragDropPanelWidget(this);
		}

		protected override DragDropEffects RowDropEffect(Row row, RowDropLocation dropLocation, IDataObject data)
		{
			if (!IsWorkpad)
			{
				return base.RowDropEffect(row, dropLocation, data);
			}
			VirtualTree tree = GetTree(data);

			if (tree == null) return DragDropEffects.None;

			return (tree == this) ? DragDropEffects.Move : DragDropEffects.Copy;
		}

		protected override void OnRowDrop(Row row, RowDropLocation dropLocation, IDataObject data, DragDropEffects dropEffect)
		{
            base.OnRowDrop(row, dropLocation, data, dropEffect);

			if (IsWorkpad)
			{
				Row[] rows = GetRows(data);

				if (rows == null) return;

				// Delete rows from original location if dropped on the same tree
				List<IResultNode> results = new List<IResultNode>();

				foreach (Row r in rows.GetUniqueRowsInGuiOrder())
				{
					IResultNode result = (IResultNode)r.Item;

					if (r.Tree == this)
					{
						result.Parent.RemoveChild(result);

						SelectedRows.SuspendChangeNotification();
						r.ParentRow.UpdateChildren(false, true);
						SelectedRows.ResumeChangeNotification();
					}

					results.Add(result);
				}

				// Adds the results in the given location
				AddResults(row, dropLocation, results.ToArray());
			}
		}

		protected override bool AllowRowDrag(Row row)
		{
			return true;
		}

		protected override RowDropLocation AllowedRowDropLocations(Row row, IDataObject data)
		{
			if (!IsWorkpad)
			{
				return RowDropLocation.None;
			}
			// Make sure a row is not dropped on itself or one of its descendants
			Row[] rows = GetRows(data);
			if (rows == null) return RowDropLocation.None;

			foreach (Row draggedRow in rows)
			{
				if (draggedRow == row || row.IsDescendant(draggedRow))
				{
					return RowDropLocation.None;
				}
			}
			return RowDropLocation.AboveRow | RowDropLocation.BelowRow | RowDropLocation.OnRow;
		}
		#endregion Drag and Drop

		private bool _ignoreOnSelectionChangedEvents;
		private void SuspendSelectionChangedEvents()
		{

			_ignoreOnSelectionChangedEvents = true;
		}

		private void ResumeSelectionChangedEvents()
		{
			_ignoreOnSelectionChangedEvents = false;
			OnSelectionChanged();
		}

		private void EnableDeleteAllMenu()
		{
			IResultNode root = RootItem as IResultNode;
			toolStripMenuItemDeleteAll.Enabled = root != null && root.Children.Count > 0;
		}

		/// <summary>
		/// Deletes the current selection.
		/// </summary>
		public void DeleteSelection()
		{
			if (IsWorkpad && RootRow != null)
			{
				foreach (IResultNode resultNode in SelectedItems)
				{
					if (resultNode.Parent != null)
					{
						resultNode.Parent.RemoveChild(resultNode);
					}
				}

				SelectedRows.SuspendChangeNotification();
				RootRow.UpdateChildren(false, true);
				SelectedRows.ResumeChangeNotification();
			}
		}

		/// <summary>
		/// Deletes all headers of the selected types.
		/// </summary>
		public void DeleteSelectedHeaderTypes()
		{
			if (IsWorkpad && HasSelectedRows)
			{
				foreach (IResultNode resultNode in Selections[HeaderSelectionType.AllHeadersOfSelectedTypeIncludingChildren].Results)
				{
					if (resultNode.Parent != null)
					{
						resultNode.Parent.RemoveChild(resultNode);
					}
				}

				SelectedRows.SuspendChangeNotification();
				RootRow.UpdateChildren(false, true);
				SelectedRows.ResumeChangeNotification();
			}
		}

		/// <summary>
		/// Saves the current selection to a file.
		/// </summary>
		public void SaveSelectionAs(ISelection selection)
		{
			if (!HasSelectedRows)
			{
				return;
			}

			var saveFileDialog = new SaveFilePresenter();
			saveFileDialog.FilterAllFiles();
			saveFileDialog.Title = "Save Selection As";

			IResultNode result = SelectedRows[0].Item as IResultNode;
			if (result != null && result.Detectors != null && result.Detectors.Count() > 0)
			{
				saveFileDialog.Filter = string.Format("{0} Files (*{1})|*{2}|All Files (*.*)|*.*",
						result.Detectors.First().Name, result.Detectors.First().OutputFileExtension,
						result.Detectors.First().OutputFileExtension);
			}

			FileInfo contiguousFile;
			if ((saveFileDialog.ShowDialog(this, out contiguousFile)))
			{
				try
				{
					// Saves the current selection to disk as one contiguous file
					IEnumerable<IDataPacket> dataPackets = Enumerable.Repeat(selection.DataPacket, 1);
					IList<IDetector> detectors = new List<IDetector>();
					foreach (IResultNode resultNode in selection.Results)
					{
						AddDetectors(resultNode, detectors);
					}

					_fileExport.SaveAsContiguousFile(dataPackets, detectors, contiguousFile.FullName, SaveFilePresenter.ExportForensicIntegrityLog);
				}
				catch (IOException ex)
				{
					MessageBox.Show(this, string.Format("An error occurred when trying to write to file '{0}'\n{1}", contiguousFile.FullName, ex.Message),
							"Error writing file");
				}
			}
		}

		private static void AddDetectors(IResultNode node, ICollection<IDetector> uniqueDetectors)
		{
			foreach (IDetector detector in node.Detectors)
			{
				if (!uniqueDetectors.Contains(detector))
				{
					uniqueDetectors.Add(detector);
				}
			}
			foreach (IResultNode resultNode in node.Children)
			{
				AddDetectors(resultNode, uniqueDetectors);
			}
		}

		private static bool IsH264NalUnitStreamResult(IResultNode resultNode)
		{
			if ((resultNode.DataFormat == CodecID.H264) && (resultNode.FindAttributeByName("StartCodePrefix") == null))
			{
				return true; // NAL unit stream format
			}

			foreach (IResultNode childResult in resultNode.Children)
			{
				if (IsH264NalUnitStreamResult(childResult))
				{
					return true;
				}
			}
			return false;
		}

		private IDataPacket CreateH264StartCodePrefixDataPacket(IProject project)
		{
			string defraserDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			IInputFile inputFile = _createInputFile(project, Path.Combine(defraserDirectory, "StartCodePrefix.h264"));
			return inputFile.CreateDataPacket();
		}

		/// <summary>
		/// Moves the <paramref name="rows"/> to the top of the tree.
		/// </summary>
		/// <param name="rows">the rows to be moved</param>
		public void MoveToTop(Row[] rows)
		{
			if (rows == null)
			{
				throw new ArgumentNullException("rows");
			}
			if (IsWorkpad && RootRow != null && RootRow.NumChildren > 0)
			{
				IResultNode root = RootItem as IResultNode;
				int index = 0;

				foreach (Row row in UniqueSelectedRowsInGuiOrder)
				{
					IResultNode resultNode = (IResultNode)row.Item;
					resultNode.Parent.RemoveChild(resultNode);
					root.InsertChild(index++, resultNode);
				}

				SelectedRows.SuspendChangeNotification();
				RootRow.UpdateChildren(false, true);
				SelectedRows.ResumeChangeNotification();
			}
		}

		/// <summary>
		/// Removes all rows from the tree.
		/// </summary>
		public void Clear()
		{
			if (IsWorkpad && RootRow != null)
			{
				IResultNode root = RootItem as IResultNode;

				if (root != null)
				{
					root.Children.Clear();

					SelectedRows.SuspendChangeNotification();
					RootRow.UpdateChildren(false, true);
					SelectedRows.ResumeChangeNotification();
				}
			}
		}

		/// <summary>
		/// Adds one or more <paramref name="results"/> in the location
		/// specified by <paramref name="row"/> and <paramref name="dropLocation"/>
		/// of this tree.
		/// </summary>
		/// <param name="row">the target row</param>
		/// <param name="dropLocation">the relative drop location</param>
		/// <param name="results">the dropped results</param>
		public void AddResults(Row row, RowDropLocation dropLocation, IResultNode[] results)
		{
			if (!IsWorkpad || row == null || results == null || results.Length == 0) return;

			int childIndex;

			// Determine child index (where to Add or Insert)
			if (dropLocation == RowDropLocation.AboveRow || dropLocation == RowDropLocation.BelowRow)
			{
				childIndex = row.ChildIndex;

				if (dropLocation == RowDropLocation.BelowRow) childIndex++;

				row = row.ParentRow;
			}
			else
			{
				childIndex = row.NumChildren;
			}

			IResultNode rowResult = row.Item as IResultNode;
			if (rowResult != null)
			{
				// Copy/Move drop results to their new location
				foreach (IResultNode r in results)
				{
					IResultNode result = r;

					// Copy from old location
					if (result.Parent != null)
					{
						result = result.DeepCopy();
					}

					// Add/Insert at new location
					if (childIndex >= rowResult.Children.Count)
					{
						rowResult.AddChild(result);
					}
					else
					{
						rowResult.InsertChild(childIndex, result);
					}

					childIndex++;
				}
			}

			row.UpdateChildren(false, true);
		}

		/// <summary>
		/// Gets the selected results.
		/// </summary>
		/// <seealso cref="VirtualTreeExtensions.GetUniqueRowsInGuiOrder(Row[])"/>
		/// <returns>the selected results</returns>
		private IResultNode[] GetSelectedResults()
		{
			List<IResultNode> selectedResults = new List<IResultNode>();
			foreach (Row row in UniqueSelectedRowsInGuiOrder)
			{
				selectedResults.Add(row.Item as IResultNode);
			}
			return selectedResults.ToArray();
		}

		/// <summary>
		/// Creates the send-to sub menu for the given <paramref name="toolStripMenuItem"/>.
		/// </summary>
		/// <param name="toolStripMenuItem">the tool strip menu item</param>
		/// <param name="selection">the selection</param>
		private void CreateSendToSubmenu(ToolStripMenuItem toolStripMenuItem, ISelection selection)
		{
			toolStripMenuItem.DropDownItems.Clear();

			_workpadManager.AddDropDownMenuItems(toolStripMenuItem, selection, ParentWorkpad);
			_sendToList.AddDropDownMenuItems(toolStripMenuItem, selection);
		}

		/// <summary>
		/// Creates the send-to sub menus.
		/// </summary>
		private void CreateSendToSubMenus()
		{
			CreateSendToSubmenu(toolStripMenuItemSendSelectionTo,
					Selections[HeaderSelectionType.SelectedHeaders]);
			CreateSendToSubmenu(toolStripMenuItemAllChildrenOfSelectedHeaderTypeExcludingHeaderItself,
					Selections[HeaderSelectionType.AllChildrenOfSelectedHeaderTypeExcludingHeaderItself]);
			CreateSendToSubmenu(toolStripMenuItemAllHeadersOfSelectedTypeIncludingChildren,
					Selections[HeaderSelectionType.AllHeadersOfSelectedTypeIncludingChildren]);
			CreateSendToSubmenu(toolStripMenuItemAllHeadersOfSelectedTypeExcludingChildren,
					Selections[HeaderSelectionType.AllHeadersOfSelectedTypeExcludingChildren]);

			// This 'Send To' sub menu should *NOT* contain the work pad items!
			toolStripMenuItemSendSelectionInH264ByteStreamTo.DropDownItems.Clear();

			_sendToList.AddDropDownMenuItems(toolStripMenuItemSendSelectionInH264ByteStreamTo, Selections[HeaderSelectionType.SelectedHeadersInH264ByteStream]);
		}

		/// <summary>
		/// Returns the rows of <paramref name="data"/> if these rows contain
		/// IResultNodes or <code>null</code> otherwise.
		/// </summary>
		/// <param name="data">the data object</param>
		/// <returns>the rows (or null)</returns>
		private static Row[] GetRows(IDataObject data)
		{
			if (data.GetDataPresent(typeof(Row[])))
			{
				Row[] rows = data.GetData(typeof(Row[])) as Row[];

				if (rows != null && rows.Length > 0 && rows[0].Item is IResultNode)
				{
					return rows;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the tree of <paramref name="dataObject"/>.
		/// </summary>
		/// <param name="dataObject">the data object</param>
		/// <returns>the tree (or null)</returns>
		/// <seealso cref="GetRows(IDataObject)"/>
		private static VirtualTree GetTree(IDataObject dataObject)
		{
			Row[] rows = GetRows(dataObject);

			return (rows == null) ? null : rows[0].Tree;
		}

		#region Event handlers
		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			Console.WriteLine("contextMenuStrip_Opening()");
			Console.WriteLine("toolStripMenuItemMoveToTop.Enabled: " + toolStripMenuItemMoveToTop.Enabled);
			contextMenuStrip.Items.Clear();

			if (IsH264NalUnitStreamResultSelected)
			{
				contextMenuStrip.Items.Add(this.toolStripMenuItemConvertToH264ByteStream);
				contextMenuStrip.Items.Add(this.toolStripSeparator5);
			}

			contextMenuStrip.Items.AddRange(FileToolStripItems);

			if (IsWorkpad)
			{
				contextMenuStrip.Items.Add(this.toolStripSeparator2);
				contextMenuStrip.Items.AddRange(EditToolStripItems);
			}

			Console.WriteLine("toolStripMenuItemMoveToTop.Enabled: " + toolStripMenuItemMoveToTop.Enabled);
			//OnSelectionChanged();
			//Console.WriteLine("toolStripMenuItemMoveToTop.Enabled: " + toolStripMenuItemMoveToTop.Enabled);
		}

		private void toolStripMenuItemSaveSelectionAs_Click(object sender, EventArgs e)
		{
			SaveSelectionAs(Selections[HeaderSelectionType.SelectedHeaders]);
		}

		private void toolStripMenuItemSaveSelectionInH264ByteStreamAs_Click(object sender, System.EventArgs e)
		{
			SaveSelectionAs(Selections[HeaderSelectionType.SelectedHeadersInH264ByteStream]);
		}

		private void toolStripMenuItemMoveToTop_Click(object sender, EventArgs e)
		{
			MoveToTop(SelectedRows.GetRows());
		}

		private void toolStripMenuItemDeleteSelection_Click(object sender, EventArgs e)
		{
			DeleteSelection();
		}

		private void toolStripMenuItemDeleteHeaderType_Click(object sender, EventArgs e)
		{
			DeleteSelectedHeaderTypes();
		}

		private void toolStripMenuItemDeleteAll_Click(object sender, EventArgs e)
		{
			Clear();
		}

		private void toolStripMenuItemGotoOffsetInHexWorkshop_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows)
			{
				var firstSelectedResult = SelectedResults.First();
				_sendToList.GotoOffsetInHexWorkshop(firstSelectedResult.InputFile.Name, firstSelectedResult.StartOffset);
			}
		}

		private void toolStripMenuItemGotoEndOffsetInHexWorkshop_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows)
			{
				var lastSelectedResult = SelectedResults.Last();
				_sendToList.GotoOffsetInHexWorkshop(lastSelectedResult.InputFile.Name, lastSelectedResult.EndOffset);
			}
		}

		private void toolStripMenuItemGotoEndOffsetOfLastDescendantInHexWorkshop_Click(object sender, EventArgs e)
		{
			if (HasSelectedRows)
			{
				var lastSelectedDescendant = SelectedResults.Last().GetLastDescendant();
				_sendToList.GotoOffsetInHexWorkshop(lastSelectedDescendant.InputFile.Name, lastSelectedDescendant.EndOffset);
			}
		}

		private void WorkpadManager_WorkpadsChanged(object sender, EventArgs e)
		{
			CreateSendToSubMenus();
		}

		private void SendToList_CollectionChanged(object sender, EventArgs e)
		{
			CreateSendToSubMenus();
		}
		#endregion Event handlers
	}
}
