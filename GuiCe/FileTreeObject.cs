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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.GuiCe.Properties;
using Defraser.GuiCe.sendtolist;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// This tree lists the input files and data blocks for a single <c>IProject</c>.
	/// The project to view/edit is controlled through the <c>Project</c> property.
	/// </summary>
	public sealed class FileTreeObject : FileTree
	{
		/// <summary>
		/// Occurs when the focus row has changed and the results for the
		/// detectable are available.
		/// </summary>
		public event EventHandler<ResultsEventArgs> FocusRowResultsChanged;

		#region Inner Classes
		private enum RowSelectionType
		{
			Unknown,
			/// <summary>All selected rows in GUI order</summary>
			AllInGuiOrder,
			/// <summary>Only selected rows of which the parents are not selected</summary>
			UniqueInGuiOrder
		}

		/// <summary>
		/// Describes the selection in the <c>FileTree</c>.
		/// </summary>
		private class Selection : ISelection
		{
			private const string DefaultOutputFileExtension = ".bin";
			private readonly FileTreeObject _fileTreeObject;
			private readonly RowSelectionType _rowSelectionType;
			private IProgressReporter _progressReporter;

			#region Properties
			public bool IsEmpty { get { return !_fileTreeObject.HasSelectedRows; } }

			public IDataPacket DataPacket
			{
				get
				{
					Debug.Assert(_progressReporter != null);

					if (IsEmpty)
					{
						return null;
					}

					IDataPacket dataPacket = null;
					IInputFile inputFile;
					Row[] rows = _fileTreeObject.GetSelectedRows(_rowSelectionType);
					if (rows.Length == 1 && (inputFile = rows[0].Item as IInputFile) != null)
					{
						// Single file
						dataPacket = inputFile.CreateDataPacket();
					}
					else
					{
						// Find the number of codec streams in the selection.
						// This is done because handling the codes streams will
						// be reported to the GUI.
						int codecStreamCount = 0;
						foreach (Row row in rows)
						{
							if (row.Item is ICodecStream)
							{
								codecStreamCount++;
							}
						}
						_progressReporter.ReportProgress(0, new UserState(UserStateType.CodecStreamCount, codecStreamCount));

						foreach (Row row in rows)
						{
							IDataPacket dp = null;
							IDataBlock dataBlock;
							ICodecStream codecStream;
							if ((inputFile = row.Item as IInputFile) != null)
							{
								dp = inputFile.CreateDataPacket();
							}
							else if ((dataBlock = row.Item as IDataBlock) != null)
							{
								// It's a container stream
								dp = dataBlock;
							}
							else if ((codecStream = row.Item as ICodecStream) != null)
							{
								dp = _fileTreeObject.GetData(codecStream, _progressReporter);
							}

							if (dp != null)
							{
								dataPacket = (dataPacket == null) ? dp : dataPacket.Append(dp);
							}
						}
					}
					return dataPacket;
				}
			}

			public IResultNode[] Results
			{
				get { return _fileTreeObject.GetSelectedResults(_rowSelectionType); }
			}

			public string OutputFileExtension
			{
				get
				{
					if (IsEmpty) return DefaultOutputFileExtension;

					foreach (Row row in _fileTreeObject.GetSelectedRows(_rowSelectionType))
					{
						IInputFile inputFile = row.Item as IInputFile;
						if (inputFile != null)
						{
							return (new FileInfo(inputFile.Name)).Extension;
						}
					}

					foreach (Row row in _fileTreeObject.GetSelectedRows(_rowSelectionType))
					{
						IMetadata dataPacket = row.Item as IMetadata;
						if ((dataPacket != null) && (dataPacket.Detectors != null) && (dataPacket.Detectors.Count() > 0))
						{
							return dataPacket.Detectors.First().OutputFileExtension;
						}
					}

					return DefaultOutputFileExtension;
				}
			}

			public object FocusItem
			{
				get { return _fileTreeObject.FocusItem; }
			}

			#endregion Properties

			/// <summary>
			/// Creates a new selection.
			/// </summary>
			/// <param name="fileTreeObject">the tree for the selection</param>
			/// <param name="rowSelectionType">make the selection unique and in GUI order</param>
			public Selection(FileTreeObject fileTreeObject, RowSelectionType rowSelectionType)
			{
				_fileTreeObject = fileTreeObject;
				_rowSelectionType = rowSelectionType;
			}

			public IDataPacket GetDataPacket(IProgressReporter progressReporter)
			{
				_progressReporter = progressReporter;
				IDataPacket result = DataPacket;
				_progressReporter = null;
				return result;
			}
		}
		#endregion Inner Classes

		private readonly WorkpadManager _workpadManager;
		private readonly SendToList _sendToList;
		private readonly IFileExport _fileExport;
		private readonly BackgroundDataBlockScanner _backgroundDataBlockScanner;

		#region Properties
		public override IList SelectedItems
		{
			get
			{
				// After removing all items from the model and updating the tree view,
				// the SelectedItems incorrectly reports a value != 0 (issue 2825)
				if (RootRow == null || RootRow.NumChildren == 0) return new ArrayList();
				return base.SelectedItems;
			}
			set
			{
				base.SelectedItems = value;
			}
		}

		public IProject Project
		{
			get { return DataSource as IProject; }
			set { DataSource = value; }
		}

	    public BackgroundDataBlockScanner BackgroundDataBlockScanner
	    {
            get { return _backgroundDataBlockScanner; }
	    }

		private IDataPacket SelectionDataPacket
		{
			get
			{
				return GetSelection(RowSelectionType.UniqueInGuiOrder).GetDataPacket(new NullProgressReporter());
			}
		}
		#endregion Properties

		public FileTreeObject(WorkpadManager workpadManager, SendToList sendToList, IFileExport fileExport, BackgroundDataBlockScanner backgroundDataBlockScanner)
		{
			_workpadManager = workpadManager;
			_sendToList = sendToList;
			_fileExport = fileExport;
			_backgroundDataBlockScanner = backgroundDataBlockScanner;
			_backgroundDataBlockScanner.RunWorkerCompleted += BackgroundDataBlockScanner_RunWorkerCompleted;
			_backgroundDataBlockScanner.ProgressChanged += BackgroundDataBlockScanner_ProgressChanged;

			HexWorkshopAvailable = sendToList.HexWorkshopAvailable();

			this.DataSourceChanged += FileTree_DataSourceChanged;
			this.FocusRowChanged += FileTree_FocusRowChanged;
			this.SaveAsSingleFile += FileTree_SaveAsSingleFile;
			this.SaveAsContiguousFile += FileTree_SaveAsContiguousFile;
			this.SaveAsSeparateFiles += FileTree_SaveAsSeparateFiles;
			this.ExportToXml += FileTree_ExportToXml;
			this.GotoOffsetInHexWorkshop += FileTree_GotoOffsetInHexWorkshop;
			this.GotoEndOffsetInHexWorkshop += FileTree_GotoEndOffsetInHexWorkshop;
		}

		/// <summary>
		/// Gets the data or result selection in the tree.
		/// </summary>
		/// <returns>the selection</returns>
		private ISelection GetSelection(RowSelectionType rowSelectionType)
		{
			return new Selection(this, rowSelectionType);
		}

		public override void CreateSendToSubmenu(ToolStripMenuItem toolStripMenuItem)
		{
			ISelection selection = GetSelection(RowSelectionType.AllInGuiOrder);
			// Add 'Send Selection To New/Existing Workpad'
			// only if there is not a file in the selection
			if (!SelectedRows.ContainsInputFile())
			{
				_workpadManager.AddDropDownMenuItems(toolStripMenuItem, selection, null);
			}

			_sendToList.AddDropDownMenuItems(toolStripMenuItem, selection);
		}

		private Row[] GetSelectedRows(RowSelectionType rowSelectionType)
		{
			switch (rowSelectionType)
			{
				case RowSelectionType.UniqueInGuiOrder: return UniqueSelectedRowsInGuiOrder;
				case RowSelectionType.AllInGuiOrder: return SelectedRowsInGuiOrder;
				default:
					Debug.Fail("No implementation for value '{0}' of enum 'RowSelectionType'", Enum.GetName(typeof(RowSelectionType), rowSelectionType));
					return null;
			}
		}

		/// <summary>
		/// Gets the selected results.
		/// </summary>
		/// <seealso cref="VirtualTreeExtensions.GetUniqueRowsInGuiOrder(Row[])"/>
		/// <returns>the selected results</returns>
		private IResultNode[] GetSelectedResults(RowSelectionType rowSelectionType)
		{
			List<IResultNode> selectedResults = new List<IResultNode>();

			foreach (Row row in GetSelectedRows(rowSelectionType))
			{
				IFragment detectable;
				if ((detectable = row.Item as IFragment) != null)
				{
					try
					{
						selectedResults.AddRange(_backgroundDataBlockScanner.GetResults(detectable).Children);
					}
					catch (FileNotFoundException fileNotFoundException)
					{
						ExceptionHandling.HandleFileNotFoundException(fileNotFoundException);
					}
					catch(IOException ioException)
					{
						ExceptionHandling.HandleIOException(ioException, detectable.InputFile.Name);
					}
					catch (FrameworkException frameworkException)
					{
						ExceptionHandling.HandleFrameworkException(frameworkException);
					}
				}
			}
			return selectedResults.ToArray();
		}

		private IDataPacket GetData(ICodecStream codecStream, IProgressReporter progressReporter)
		{
			return _backgroundDataBlockScanner.GetData(codecStream, progressReporter);
		}

		#region Event handlers
		private void FileTree_DataSourceChanged(object sender, EventArgs e)
		{
			_backgroundDataBlockScanner.Stop();

			ResultBeingRetrieved = null;
		}

		private void FileTree_FocusRowChanged(object sender, EventArgs e)
		{
			_backgroundDataBlockScanner.Stop();

			RetrieveResultProgressPercentage = 0;
			ResultBeingRetrieved = (FocusRow == null) ? null : FocusRow.Item as IFragment;

			if (ResultBeingRetrieved != null)
			{
				_backgroundDataBlockScanner.Scan(ResultBeingRetrieved);

				// Show progress message in the tree
				UpdateRows(false);
			}
		}

		private void FileTree_SaveAsSingleFile(object sender, FileExportEventArgs<IInputFile> e)
		{
			_fileExport.SaveAsSingleFile(e.Items, e.OutputPath, Settings.Default.ExportAlsoForensicIntegrityLog, e.ProgressReporter);
		}

		private void FileTree_SaveAsContiguousFile(object sender, FileExportEventArgs<IEnumerable<IDataPacket>> e)
		{
			_fileExport.SaveAsContiguousFile(e.Items, GetDetectors(e.Items), e.OutputPath, Settings.Default.ExportAlsoForensicIntegrityLog, e.ProgressReporter);
		}

		private static IEnumerable<IDetector> GetDetectors(IEnumerable<IDataPacket> dataPackets)
		{
			var detectors = new HashSet<IDetector>();
			foreach (IDataPacket dataPacket in dataPackets)
			{
				var metadata = dataPacket as IMetadata;
				if (metadata != null)
				{
					foreach (IDetector detector in metadata.Detectors)
					{
						detectors.Add(detector);
					}
				}
			}
			return detectors;
		}

		private void FileTree_SaveAsSeparateFiles(object sender, FileExportEventArgs<IEnumerable<object>> e)
		{
			_fileExport.SaveAsSeparateFiles(e.Items, e.OutputPath, Settings.Default.ExportAlsoForensicIntegrityLog, e.ProgressReporter);
		}

		private void FileTree_ExportToXml(object sender, FileExportEventArgs<IEnumerable<IFragment>> e)
		{
			_fileExport.ExportToXml(e.Items, e.OutputPath, e.ProgressReporter);
		}

		private void FileTree_GotoOffsetInHexWorkshop(object sender, EventArgs e)
		{
			_sendToList.GotoOffsetInHexWorkshop(SelectionDataPacket.InputFile.Name, SelectionDataPacket.StartOffset);
		}

		private void FileTree_GotoEndOffsetInHexWorkshop(object sender, EventArgs e)
		{
			_sendToList.GotoOffsetInHexWorkshop(SelectionDataPacket.InputFile.Name, SelectionDataPacket.EndOffset);
		}

		private void BackgroundDataBlockScanner_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// Update progress message in the tree
			if (FocusRow != null) // issue 2824
			{
				RetrieveResultProgressPercentage = e.ProgressPercentage;
				FocusRow.UpdateChildren(false, false);
			}
		}

		private void BackgroundDataBlockScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (!e.Cancelled)
			{
				if (FocusRowResultsChanged != null)
				{
					FocusRowResultsChanged(this, new ResultsEventArgs(e.Result as IResultNode));
				}
			}

			ResultBeingRetrieved = null;

			// Update results and context menu for currently select row
			UpdateContextMenu();

			// Replace progress message in the tree with original data block
			UpdateRows(false);
		}
		#endregion Event handlers
	}
}
