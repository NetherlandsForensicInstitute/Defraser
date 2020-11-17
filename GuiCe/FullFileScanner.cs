/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Linq;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public class FullFileScanner
	{
		#region InnerClassDefines
		public class FileScanResult
		{
		    private readonly IFragment _sourceFragment;
			private readonly IResultNode _result;
			private readonly int _availableCodecStreams;
            private readonly IDataBlock _resultDataBlock;
            private readonly ICodecStream _resultCodecStream;

		    public FileScanResult(IFragment sourceFragment, IResultNode result,
                IDataBlock resultDataBlock, ICodecStream resultCodecStream,
                int availableCodecStreams)
			{
                IsValid = true;
			    _sourceFragment = sourceFragment;
				_result = result;
			    _resultDataBlock = resultDataBlock;
                _resultCodecStream = resultCodecStream;
				_availableCodecStreams = availableCodecStreams;
			}

		    #region Properties
            public bool IsValid
            {
                set; get;
            }

			public IResultNode Result
			{
				get { return _result; }
			}

            public IDataBlock DataBlock
            {
                get { return _resultDataBlock; }
            }

            public ICodecStream CodecStream
            {
                get { return _resultCodecStream; }
            }

			public int AvailableCodecStreams
			{
				get { return _availableCodecStreams; }
			}

			public long StartOffset
			{
                get { return _sourceFragment.StartOffset; }
			}

			public long EndOffset
			{
                get { return _sourceFragment.EndOffset; }
			}

			public long Length
			{
                get { return (_sourceFragment.StartOffset - _sourceFragment.EndOffset); }
			}

		    public IFragment SourceFragment
		    {
                get { return _sourceFragment; }
		    }
		    #endregion Properties
		}
		#endregion InnerClassDefines

		#region CustomEvents
		public delegate void ResultDetectedHandler(object sender, FileScanResult scanResult);
		public delegate void ScanCompleteHandler(object sender, EventArgs e);

		public event ResultDetectedHandler ResultDetected;
		public event ScanCompleteHandler ScanCompleted;
		public event ProgressChangedEventHandler ScanProgressChanged;
		#endregion CustomEvents

		#region Properties
		/// <summary>
		/// The maximum number of codecs streams to scan.
		/// When it's set to -1 there is no limit.
		/// </summary>
		public int ScanNumCodecStreams
		{
			set; get;
		}

        public bool ScanNextCodecStreamOnInvalidation
        {
            set; get;
        }

        public bool ScanMoreThanOneFragment
        {
            set; get;
        }

		public bool IsBusy
		{
			get { return (_activeScanFile != null); }
		}
		#endregion Properties

		private readonly BackgroundDataBlockScanner _backgroundDataBlockScanner;

		#region ScanAttributes
		private IInputFile _activeScanFile;
		private IList<IDataBlock> _currentDataBlocks;
		private int _currentDataBlockIndex;
		private IList<ICodecStream> _currentValidCodecStreams;
		private int _currentCodecStreamIndex;
		#endregion ScanAttributes

		public FullFileScanner(BackgroundDataBlockScanner backgroundDataBlockScanner)
		{
			_backgroundDataBlockScanner = backgroundDataBlockScanner;
			ScanNumCodecStreams = -1;
		    ScanMoreThanOneFragment = true;

			_backgroundDataBlockScanner.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundDataBlockScanner_RunWorkerCompleted);
			_backgroundDataBlockScanner.ProgressChanged +=  new ProgressChangedEventHandler(backgroundDataBlockScanner_ProgressChanged);
		}

		private void ResetAllScanAttributes()
		{
			_activeScanFile = null;
			_currentDataBlocks = null;
			_currentDataBlockIndex = -1;
			_currentValidCodecStreams = null;
			_currentCodecStreamIndex = -1;
		}

		/// <summary>
		/// Scans a input file for packages in the (container/codec)streams.
		/// </summary>
		/// <param name="inputFile"></param>
		/// <returns>True when the scan is succesfully started, false when an error occurred.</returns>
		public bool StartScan(IInputFile inputFile)
		{
			if(IsBusy || inputFile.Project == null) return false;
			ResetAllScanAttributes();
			_activeScanFile = inputFile;

			_currentDataBlocks = inputFile.Project.GetDataBlocks(inputFile);
			ScanNextDataBlock();
			return true;
		}

		private void ScanNextDataBlock()
		{
			_currentDataBlockIndex++;
			_currentCodecStreamIndex = -1;

			if (_currentDataBlockIndex < _currentDataBlocks.Count)
			{
				IDataBlock dataBlock = _currentDataBlocks[_currentDataBlockIndex];
				_currentValidCodecStreams = dataBlock.CodecStreams.Where(d => (d.DataFormat != CodecID.Unknown)).ToList();

                // Check if we scan more than 1 fragment of a fragmentsequence or first fragment.
                if (ScanMoreThanOneFragment || dataBlock.FragmentIndex == 0)
                {
                    // Check if there are any codecstreams.
                    // If there are 0 codecstreams, the frames (results) are probably in the dataBlock (if so, the datablock will be scanned).
                    if (_currentValidCodecStreams.Count > 0)
                    {
                        ScanNextCodecStream();
                    }
                    else if (dataBlock.CodecStreams.Count == 0 && dataBlock.DataFormat != CodecID.Unknown)
                    {
                        DetectResultsInStream(dataBlock);
                    }
                    else
                    {
                        // No valid datablock was found, skip this one and scan next block.
                        ScanNextDataBlock();
                    }
                }
                else
                {
                    ScanNextDataBlock();
                }
			}
			else
			{
				// All datablocks has been scanned, report complete to users of this scanner
				ResetAllScanAttributes();
				ScanCompleted(this, EventArgs.Empty);
			}
		}

        private void ScanNextCodecStream()
        {
            ScanNextCodecStream(false);
        }

		private void ScanNextCodecStream(bool forceNextCodecStream)
		{
			_currentCodecStreamIndex++;

            if (_currentCodecStreamIndex < _currentValidCodecStreams.Count && (ScanNumCodecStreams == -1 || forceNextCodecStream || _currentCodecStreamIndex < ScanNumCodecStreams))
			{
				DetectResultsInStream(_currentValidCodecStreams[_currentCodecStreamIndex]);
			}
			else
			{
				// All Codecstream are scanned in this DataBlock, go to the next.
				ScanNextDataBlock();
			}
		}

		public void StopScan()
		{
			if(!IsBusy) return;

			_backgroundDataBlockScanner.Stop();
			ResetAllScanAttributes();
		}

		private void DetectResultsInStream(IFragment fragment)
		{
			// Stop scan, only when a scan is still active.
			_backgroundDataBlockScanner.Stop();
			_backgroundDataBlockScanner.Scan(fragment);
		}

		#region EventHandlers
		private void backgroundDataBlockScanner_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
            double totalPercentageBeforeCurrentDataBlock = _currentDataBlockIndex * 100.0;
            double totalPercentage = totalPercentageBeforeCurrentDataBlock + e.ProgressPercentage;
            double normalizedPercentage = totalPercentage / _currentDataBlocks.Count;

		    ScanProgressChanged(sender, new ProgressChangedEventArgs((int) normalizedPercentage, e.UserState));
		}

		private void backgroundDataBlockScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(e.Cancelled)
			{
				ResetAllScanAttributes();
				return;
			}

		    bool resultInvalidated = false;
			if (e.Error == null)
			{
				// Get used sourcedata
				IFragment sourceFragment = (_currentCodecStreamIndex == -1) ? ((IFragment)_currentDataBlocks[_currentDataBlockIndex]) : ((IFragment)_currentValidCodecStreams[_currentCodecStreamIndex]);
                
				// Report result to users of this scanner
			    FileScanResult result = new FileScanResult(sourceFragment, e.Result as IResultNode,
                                                           _currentDataBlocks[_currentDataBlockIndex],
                                                           (_currentCodecStreamIndex >= 0) ? _currentValidCodecStreams[_currentCodecStreamIndex] : null,
			                                               _currentValidCodecStreams.Count);
                ResultDetected(this, result);

                // Check if the result is invalidated by one of the event listeners.
			    resultInvalidated = ScanNextCodecStreamOnInvalidation && !result.IsValid;
			}

			// Continue scan operation
			if(_currentCodecStreamIndex == -1)
			{
				// We where scanning a Datablock
                ScanNextDataBlock();
			}
			else
			{
				// We where scanning a CodecStream
                ScanNextCodecStream(resultInvalidated);
			}
		}
		#endregion EventHandlers
	}
}
