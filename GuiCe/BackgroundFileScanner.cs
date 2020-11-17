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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Scans files for video data blocks in a background task.
	/// </summary>
	public partial class BackgroundFileScanner : BackgroundWorker, INotifyPropertyChanged, IProgressReporter
	{
		/// <value>The name of the <see cref="CurrentFile"/> property.</value>
		public const string CurrentFileProperty = "CurrentFile";

		#region Events
		/// <summary>Occurs when the <c>CurrentFile</c> property changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>Occurs when the background task is about to be cancelled.</summary>
		public event EventHandler<EventArgs> Cancel;
		/// <summary>Queries whether to continue waiting for cancellation.</summary>
		public event CancelEventHandler WaitForCancel;
		#endregion Events

		private readonly Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
		private readonly IFileScanner _fileScanner;
		private readonly DataBlockScanner _dataBlockScanner;
		private readonly ISynchronizeInvoke _synchronizeInvoke;
		private IInputFile _currentFile;

		#region Properties
		/// <summary>The number of seconds to wait for cancel.</summary>
		public int WaitForCancelTime { get; private set; }
		/// <summary>The file currently being scanned, <c>null</c> if not scanning.</summary>
		public IInputFile CurrentFile
		{
			get { return _currentFile; }
			private set
			{
				if (value != _currentFile)
				{
					_currentFile = value;
					OnPropertyChanged(new PropertyChangedEventArgs(CurrentFileProperty));
				}
			}
		}
		public IEnumerable<IDetector> Detectors { get { return _fileScanner.Detectors; } }
		#endregion Properties

		/// <summary>
		/// Creates a new background file scanner.
		/// </summary>
		/// <param name="createSubProgressReporter">
		/// The factory method for creating progress reporters for a sub-task,
		/// e.g. scanning a single file
		/// </param>
		/// <param name="fileScanner">The <see cref="IFileScanner"/> used for scanning input files</param>
		/// <param name="dataBlockScanner"></param>
		/// <param name="synchronizeInvoke">
		/// The <see cref="ISynchronizeInvoke"/> for executing code on the GUI-thread
		/// </param>
		public BackgroundFileScanner(Creator<IProgressReporter, IProgressReporter, long, long, long> createSubProgressReporter,
									 IFileScanner fileScanner, DataBlockScanner dataBlockScanner, ISynchronizeInvoke synchronizeInvoke)
		{
			PreConditions.Argument("createSubProgressReporter").Value(createSubProgressReporter).IsNotNull();
			PreConditions.Argument("fileScanner").Value(fileScanner).IsNotNull();
			PreConditions.Argument("synchronizeInvoke").Value(synchronizeInvoke).IsNotNull();

			_createSubProgressReporter = createSubProgressReporter;
			_fileScanner = fileScanner;
			_dataBlockScanner = dataBlockScanner;
			_synchronizeInvoke = synchronizeInvoke;

			_fileScanner.DataBlockDetected += FileScanner_DataBlockDetected;

			InitializeComponent();
		}

		/// <summary>
		/// Scans data consisting of one or more files for video data blocks in a background task.
		/// </summary>
		/// <param name="containerDetectors">the container detectors</param>
		/// <param name="codecDetectors">the codec detectors</param>
		/// <param name="inputFiles">the files to be scanned</param>
		/// <param name="project">The <see cref="IProject"/> to add the files to</param>
		public void ScanFiles(IEnumerable<IDetector> containerDetectors, IEnumerable<IDetector> codecDetectors,
							  IEnumerable<string> inputFiles, IProject project)
		{
			WaitForCancelTime = 3;

			_fileScanner.ContainerDetectors = containerDetectors;
			_fileScanner.CodecDetectors = codecDetectors;

			RunWorkerAsync(new Action(() => ScanFiles(inputFiles, project)));
		}

		private void ScanFiles(IEnumerable<string> inputFiles, IProject project)
		{
			long totalBytesToScan = inputFiles.Sum(x => new FileInfo(x).Length);
			long bytesScanned = 0L;

			foreach (string filePath in inputFiles)
			{
				if (CancellationPending) break;

				try
				{
					DateTime scanStartTime = DateTime.Now;

					AddFileToProject(project, filePath, Detectors);

					_dataBlockScanner.ClearCache();
					_fileScanner.Scan(CurrentFile, _createSubProgressReporter(this, bytesScanned, CurrentFile.Length, totalBytesToScan));

					bytesScanned += CurrentFile.Length;

					project.SetScanDuration(CurrentFile, DateTime.Now - scanStartTime);
				}
				catch (FileNotFoundException)
				{
				}
			}
		}

		/// <summary>
		/// Stops the file scanner if it is busy.
		/// Sends a <c>Cancel</c> event before cancelling the worker thread.
		/// </summary>
		/// <returns>whether the file scanner was successfully stopped</returns>
		public bool Stop()
		{
			DateTime cancelStartTime = DateTime.Now;

			if (IsBusy)
			{
				CancelAsync();

				// Signal cancellation
				if (Cancel != null)
				{
					Cancel(this, EventArgs.Empty);
				}
			}

			while (IsBusy)
			{
				// Wait WaitForCancelTime seconds for the termination of the
				// background worker thread. If the background worker thread
				// is not terminated after that time, inform the user and aks
				// the user the retry or cancel.
				if (DateTime.Now.Subtract(cancelStartTime).Seconds > WaitForCancelTime)
				{
					if (WaitForCancel != null)
					{
						var cancelEventArgs = new CancelEventArgs();

						WaitForCancel(this, cancelEventArgs);

						if (cancelEventArgs.Cancel)
						{
							break;
						}
					}

					// Wait another period ...
					cancelStartTime = DateTime.Now;
					WaitForCancelTime *= 2;
				}

				// Keep UI messages moving, so the application remains 
				// responsive during the asynchronous operation.
				Application.DoEvents();
			}
			return !IsBusy;
		}

		/// <summary>
		/// Adds the given <paramref name="filePath"/> to the <see cref="project"/>
		/// on the GUI thread.
		/// This will set the <see cref="CurrentFile"/> property to the <see cref="IInputFile"/>
		/// that was created by and added to the <paramref name="project"/>.
		/// </summary>
		/// <param name="project">The <see cref="IProject"/> to add the file to</param>
		/// <param name="filePath">The path of the file to add</param>
		/// <param name="detectors">The detectors used for scanning <see cref="filePath"/></param>
		private void AddFileToProject(IProject project, string filePath, IEnumerable<IDetector> detectors)
		{
			_synchronizeInvoke.Invoke(new Action(() => CurrentFile = project.AddFile(filePath, detectors)), null);
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			base.OnDoWork(e);

			// Run the ScanFiles() action on the (this) background thread
			Action scanFilesAction = e.Argument as Action;
			scanFilesAction();

			// Return cancellation state
			if (CancellationPending)
			{
				e.Cancel = true;
			}
		}

		protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			CurrentFile = null;

			base.OnRunWorkerCompleted(e);
		}

		#region Event handlers
		private void FileScanner_DataBlockDetected(object sender, DataBlockDetectedEventArgs e)
		{
			IDataBlock dataBlock = e.DataBlock;
			_synchronizeInvoke.Invoke(new Action(() => dataBlock.InputFile.Project.AddDataBlock(dataBlock)), null);
		}
		#endregion Event handlers
	}
}
