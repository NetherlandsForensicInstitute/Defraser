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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Retrieves the results for a video data block in a background task.
	/// </summary>
	public sealed partial class BackgroundDataBlockScanner : BackgroundWorker, IProgressReporter
	{
		private readonly Creator<IDataReaderPool> _createDataReaderPool;
		private readonly DataBlockScanner _dataBlockScanner;

		#region Properties
		/// <summary>
		/// The detectable for which results are currently begin retrieved.
		/// </summary>
		public IFragment Fragment { get; private set; }
		/// <summary>
		/// The progress percentage of the background task.
		/// </summary>
		public int ProgressPercentage { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new background result scanner.
		/// </summary>
		public BackgroundDataBlockScanner(Creator<IDataReaderPool> createDataReaderPool,
										  DataBlockScanner dataBlockScanner)
		{
			_createDataReaderPool = createDataReaderPool;
			_dataBlockScanner = dataBlockScanner;

			InitializeComponent();
		}

		/// <summary>
		/// Scans the <paramref name="fragment"/> for results in a background task.
		/// </summary>
		/// <param name="fragment">the fragment to retrieve results for</param>
		public void Scan(IFragment fragment)
		{
			PreConditions.Argument("fragment").Value(fragment).IsNotNull();

			if (Fragment == null)
			{
				Fragment = fragment;

				RunWorkerAsync(fragment);
			}
		}

		/// <summary>
		/// Stops the result scanner if it is busy.
		/// </summary>
		public void Stop()
		{
			CancelAsync();

			while (IsBusy)
			{
				// Keep UI messages moving, so the application remains 
				// responsive during the asynchronous operation.
				Application.DoEvents();
			}
		}

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			IFragment fragment = e.Argument as IFragment;
			IResultNode results = null;
			try
			{
				results = GetResults(fragment, this);
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				ExceptionHandling.HandleFileNotFoundException(fileNotFoundException);
			}
			catch(IOException ioException)
			{
				ExceptionHandling.HandleIOException(ioException, fragment == null ? string.Empty : fragment.InputFile.Name);
			}
			catch (FrameworkException frameworkException)
			{
				ExceptionHandling.HandleFrameworkException(frameworkException);
			}

			// Return cancellation state
			if (CancellationPending)
			{
				e.Cancel = true;
			}
			else
			{
				e.Result = results;
			}

			base.OnDoWork(e);
		}

		protected override void OnProgressChanged(ProgressChangedEventArgs e)
		{
			if (CancellationPending) return;

			ProgressPercentage = e.ProgressPercentage;

			base.OnProgressChanged(e);
		}

		protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
		{
			Fragment = null;

			base.OnRunWorkerCompleted(e);
		}

		//internal IDataPacket GetData(ICodecStream codecStream)
		//{
		//    return GetData(codecStream, new NullProgressReporter());
		//}

		internal IDataPacket GetData(ICodecStream codecStream, IProgressReporter progressReporter)
		{
			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				return _dataBlockScanner.GetData(codecStream, progressReporter, dataReaderPool);
			}
		}

		internal IResultNode GetResults(IFragment fragment)
		{
			return GetResults(fragment, new NullProgressReporter());
		}

		private IResultNode GetResults(IFragment fragment, IProgressReporter progressReporter)
		{
			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				return _dataBlockScanner.GetResults(fragment, progressReporter, dataReaderPool);
			}
		}
	}
}
