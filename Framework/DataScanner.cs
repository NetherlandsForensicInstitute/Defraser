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
using System.Linq;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Scans a block of data for video data blocks.
	/// <para>
	/// The detectors are used alternately to maximize sequential reading.
	/// Data blocks and blocks of unknown data are produced in order of increasing
	/// offset. This improves caching of the data, reducing disk I/O.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Detected data blocks and unknown data are reported as a contiguous stream
	/// through the corresponding events.
	/// </remarks>
	public class DataScanner : IDataScanner
	{
		#region Inner classes
		/// <summary>
		/// The <see cref="IProgressReporter"/> for <see cref="IDataReader.Position"/>
		/// based progress reporting.
		/// </summary>
		private sealed class ProgressReporter : IProgressReporter
		{
			private readonly IProgressReporter _progressReporter;
			private readonly int _detectorCount;
			private DetectorState _currentDetector;
			/// <summary>
			/// The total sum of the progress percentage of all detectors,
			/// except for the current detector.
			/// </summary>
			private int _totalProgressPercentage;
			private int _currentProgressPercentage;

			#region Properties
			public bool CancellationPending { get { return _progressReporter.CancellationPending; } }

			internal DetectorState CurrentDetector
			{
				set
				{
					if (_currentDetector != null)
					{
						_totalProgressPercentage += _currentDetector.ProgressPercentage;

						ReportProgress(0);
					}

					_currentDetector = value;

					if (_currentDetector != null)
					{
						_totalProgressPercentage -= _currentDetector.ProgressPercentage;
					}
				}
			}
			#endregion Properties

			public ProgressReporter(IProgressReporter progressReporter, int detectorCount)
			{
				_progressReporter = progressReporter;
				_detectorCount = Math.Max(1, detectorCount);
			}

			public void ReportProgress(int percentProgress)
			{
				int progressPercentage = ((_totalProgressPercentage + percentProgress) / _detectorCount);
				if (progressPercentage > _currentProgressPercentage)
				{
					_currentProgressPercentage = progressPercentage;
					_progressReporter.ReportProgress(progressPercentage);
				}
			}

			public void ReportProgress(int percentProgress, object userState)
			{
				_currentProgressPercentage = ((_totalProgressPercentage + percentProgress) / _detectorCount);
				_progressReporter.ReportProgress(_currentProgressPercentage, userState);
			}

			internal void UpdateTotalProgressPercentage(int totalProgressPercentage)
			{
				_totalProgressPercentage = totalProgressPercentage;

				if (_currentDetector != null)
				{
					_totalProgressPercentage -= _currentDetector.ProgressPercentage;
				}
			}
		}

		/// <summary>
		/// The scan state of a single detector. 
		/// </summary>
		private sealed class DetectorState
		{
			private readonly DataScanner _dataScanner;
			private readonly Creator<IScanContext, IProject> _createScanContext;

			#region Properties
			public IDetector Detector { get; private set; }
			public long Position { get; private set; }
			public long Length { get; private set; }
			public int ProgressPercentage { get { return (int)((Position * 100) / Length); } }
			public IDataBlock LastDataBlock { get; set; }
			#endregion Properties

			public DetectorState(DataScanner dataScanner, IDetector detector, Creator<IScanContext, IProject> createScanContext)
			{
				_dataScanner = dataScanner;
				_createScanContext = createScanContext;
				Detector = detector;
			}

			public void Reset(long length)
			{
				Position = 0;
				Length = length;
				LastDataBlock = null;
			}

			public void RewindTo(long position)
			{
				Position = position;
				LastDataBlock = null;
			}

			public bool IsRunningBehind(DetectorState detectorState)
			{
				if (Position < detectorState.Position)
				{
					return true;
				}
				if (Position > detectorState.Position)
				{
					return false;
				}
				if ((LastDataBlock != null) && (detectorState.LastDataBlock == null))
				{
					return true;
				}

				return false;
			}

			/// <summary>Detects the next data block.</summary>
			/// <param name="dataReader">the data to scan</param>
			/// <param name="dataBlockBuilder">the builder for creating the data block</param>
			/// <returns>the data block that was detected; <c>null</c> for none</returns>
			public void DetectNextDataBlock(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder)
			{
				IInputFile inputFile = dataReader.GetDataPacket(Position, 1).InputFile;
				IScanContext scanContext = _createScanContext(inputFile.Project);
				scanContext.Detectors = new[] {/*?*/Detector };
				dataBlockBuilder.InputFile = inputFile;
				dataBlockBuilder.PreviousFragment = LastDataBlock;
				dataBlockBuilder.Detectors = scanContext.Detectors;
				LastDataBlock = Detector.DetectData(dataReader, dataBlockBuilder, scanContext);
				Position = ((LastDataBlock == null) || _dataScanner.AllowOverlap) ? dataReader.Position : LastDataBlock.EndOffset;

				if (!_dataScanner.AllowOverlap && (LastDataBlock != null))
				{
					if (OverlapsWithLargerDataBlock())
					{
						DiscardLastDataBlock();
					}
					else
					{
						DiscardOverlappingDataBlocks();
					}
				}
			}

			private bool OverlapsWithLargerDataBlock()
			{
				foreach (DetectorState detectorState in _dataScanner._detectorStates)
				{
					if (detectorState.HasLargerOverlappingDataBlock(LastDataBlock))
					{
						return true;
					}
				}
				return false;
			}

			private void DiscardOverlappingDataBlocks()
			{
				foreach (DetectorState detectorState in _dataScanner._detectorStates)
				{
					if ((detectorState != this) && detectorState.HasOverlappingDataBlock(LastDataBlock))
					{
						detectorState.DiscardLastDataBlock();
					}
				}
			}

			private void DiscardLastDataBlock()
			{
				if (LastDataBlock.FragmentContainer != null)
				{
					LastDataBlock.FragmentContainer.Remove(LastDataBlock);
				}

				IDataBlock discardedDataBlock = LastDataBlock;
				LastDataBlock = null;
				_dataScanner.OnDataBlockDiscarded(new SingleValueEventArgs<IDataBlock>(discardedDataBlock));
			}

			private bool HasLargerOverlappingDataBlock(IDataBlock dataBlock)
			{
				return HasOverlappingDataBlock(dataBlock) && (LastDataBlock.Length > dataBlock.Length);
			}

			/// <summary>
			/// Returns whether the <see cref="LastDataBlock"/> of this <see cref="DetectorState"/>
			/// is not <c>null</c> and overlaps with <param name="dataBlock"/>.
			/// </summary>
			/// <param name="dataBlock">the data block to test</param>
			/// <returns>whether the data blocks overlap</returns>
			private bool HasOverlappingDataBlock(IDataBlock dataBlock)
			{
				if ((dataBlock == null) || (LastDataBlock == null)) return false;

				// No overlap if LastDataBlock is either completely before or after dataBlock
				if (LastDataBlock.EndOffset <= dataBlock.StartOffset) return false;
				if (LastDataBlock.StartOffset >= dataBlock.EndOffset) return false;

				return true;
			}
		}
		#endregion Inner classes

		#region Events
		public event EventHandler<DataBlockDetectedEventArgs> DataBlockDetected;
		public event EventHandler<SingleValueEventArgs<IDataBlock>> DataBlockDiscarded;
		public event EventHandler<UnknownDataDetectedEventArgs> UnknownDataDetected;
		#endregion Events

		private readonly Creator<IDataBlockBuilder> _createDataBlockBuilder;
		private readonly Creator<IDataReader, IDataReader, IProgressReporter> _createProgressDataReader;
		private readonly Creator<IScanContext, IProject> _createScanContext;
		private readonly IList<DetectorState> _detectorStates;

		#region Properties
		public IEnumerable<IDetector> Detectors
		{
			get { return _detectorStates.Select(s => s.Detector); }
			set
			{
				PreConditions.Argument("value").Value(value).IsNotNull();
				PreConditions.Argument("value").Value(value).DoesNotContainNull();

				_detectorStates.Clear();

				foreach (IDetector detector in value)
				{
					_detectorStates.Add(new DetectorState(this, detector, _createScanContext));
				}
			}
		}
		public bool AllowOverlap { get; set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="DataScanner"/>.
		/// </summary>
		/// <param name="createDataBlockBuilder">The factory method for creating data blocks</param>
		/// <param name="createProgressDataReader">The factory method for decorating
		/// an <see cref="IDataReader"/> with progress reporting based on its <c>Position</c></param>
		public DataScanner(Creator<IDataBlockBuilder> createDataBlockBuilder, Creator<IDataReader, IDataReader, IProgressReporter> createProgressDataReader, Creator<IScanContext, IProject> createScanContext)
		{
			PreConditions.Argument("createDataBlockBuilder").Value(createDataBlockBuilder).IsNotNull();
			PreConditions.Argument("createProgressDataReader").Value(createProgressDataReader).IsNotNull();
			PreConditions.Argument("createScanContext").Value(createScanContext).IsNotNull();

			_createDataBlockBuilder = createDataBlockBuilder;
			_createProgressDataReader = createProgressDataReader;
			_createScanContext = createScanContext;
			_detectorStates = new List<DetectorState>();
		}

		public virtual void Scan(IDataReader dataReader, IProgressReporter aProgressReporter)
		{
			PreConditions.Argument("dataReader").Value(dataReader).IsNotNull();
			PreConditions.Argument("aProgressReporter").Value(aProgressReporter).IsNotNull();

			ProgressReporter progressReporter = new ProgressReporter(aProgressReporter, _detectorStates.Count);
			IDataReader progressDataReader = _createProgressDataReader(dataReader, progressReporter);

			ResetDetectorStates(progressDataReader.Length);
			aProgressReporter.ReportProgress(0);

			DetectorState detectorState = SelectNextDetector(progressReporter, progressDataReader);
			long lastReportedOffset = 0L;

			while ((progressDataReader.State == DataReaderState.Ready) && !aProgressReporter.CancellationPending)
			{
				detectorState.DetectNextDataBlock(progressDataReader, _createDataBlockBuilder());

				// Select next detector
				detectorState = SelectNextDetector(progressReporter, progressDataReader);

				// Report detected data block, if any
				if (!aProgressReporter.CancellationPending && (detectorState.LastDataBlock != null))
				{
					ReportUnknownData(lastReportedOffset, GetMinimumLatestDataBlockStartOffset());

					if (detectorState.Position > lastReportedOffset)
					{
						lastReportedOffset = detectorState.Position;
					}

					OnDataBlockDetected(new DataBlockDetectedEventArgs(detectorState.LastDataBlock));
				}
			}

			// Report unknown data at end of stream, if any
			if (!aProgressReporter.CancellationPending)
			{
				ReportUnknownData(lastReportedOffset, progressDataReader.Length);
			}
		}

		/// <summary>
		/// The minimum <see cref="IDataPacket.StartOffset">StartOffset</see> of the
		/// <see cref="DetectorState.LastDataBlock"/> property of all detectors that
		/// actually have a (non <c>null</c>) last data block.
		/// </summary>
		/// <returns>The minimum start offset</returns>
		private long GetMinimumLatestDataBlockStartOffset()
		{
			long startOffset = long.MaxValue;
			foreach (DetectorState detectorState in _detectorStates)
			{
				if ((detectorState.LastDataBlock != null) && (detectorState.LastDataBlock.StartOffset < startOffset))
				{
					startOffset = detectorState.LastDataBlock.StartOffset;
				}
			}
			return startOffset;
		}

		/// <summary>
		/// Raises the <see cref="DataBlockDetected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="DataBlockDetectedEventArgs"/> for the event</param>
		/// </param>
		protected virtual void OnDataBlockDetected(DataBlockDetectedEventArgs e)
		{
			if (DataBlockDetected != null)
			{
				DataBlockDetected(this, e);
			}
		}

		/// <summary>
		/// Raises the <see cref="DataBlockDiscarded"/> event.
		/// </summary>
		/// <param name="e">The <see cref="DataBlockEventArgs"/> for the event.</param>
		protected virtual void OnDataBlockDiscarded(SingleValueEventArgs<IDataBlock> e)
		{
			if (DataBlockDiscarded != null)
			{
				DataBlockDiscarded(this, e);
			}
		}

		/// <summary>
		/// Raises the <see cref="UnknownDataDetected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="UnknownDataDetectedEventArgs"/> for the event.</param>
		protected virtual void OnUnknownDataDetected(UnknownDataDetectedEventArgs e)
		{
			if (UnknownDataDetected != null)
			{
				UnknownDataDetected(this, e);
			}
		}

		private void ResetDetectorStates(long length)
		{
			foreach (DetectorState detectorState in _detectorStates)
			{
				detectorState.Reset(length);
			}
		}

		private DetectorState SelectNextDetector(ProgressReporter progressReporter, IDataReader dataReader)
		{
			DetectorState nextDetector = null;
			foreach (DetectorState detectorState in _detectorStates)
			{
				if ((nextDetector == null) || detectorState.IsRunningBehind(nextDetector))
				{
					nextDetector = detectorState;
				}
			}
			if (nextDetector == null)
			{
				dataReader.Position = dataReader.Length;
			}
			else
			{
				progressReporter.CurrentDetector = nextDetector;
				dataReader.Position = nextDetector.Position;
			}
			return nextDetector;
		}

		private void ReportUnknownData(long startOffset, long endOffset)
		{
			if (endOffset > startOffset)
			{
				OnUnknownDataDetected(new UnknownDataDetectedEventArgs(startOffset, (endOffset - startOffset)));
			}
		}
	}
}
