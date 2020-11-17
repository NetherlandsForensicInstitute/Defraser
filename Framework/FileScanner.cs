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
using Defraser.Detector.UnknownFormat;

namespace Defraser.Framework
{
    /// <summary>
    /// Scans one or more files for video data blocks.
    /// <para>
    /// This implementation first tries the container detectors (if any),
    /// then the codec detectors are tried on any undetected space.
    /// </para>
    /// <para>
    /// The detectors are used alternately to maximize creating data blocks
    /// in order of increasing offsets. This also improves caching of the
    /// data to be detected, reducing disk I/O.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Detected data blocks and progress are reported through events.
    /// </remarks>
    public sealed class FileScanner : IFileScanner
    {
        #region Events
        public event EventHandler<DataBlockDetectedEventArgs> DataBlockDetected;
#pragma warning disable 67
        public event EventHandler<SingleValueEventArgs<IDataBlock>> DataBlockDiscarded;
        public event EventHandler<UnknownDataDetectedEventArgs> UnknownDataDetected;
#pragma warning restore 67
        #endregion Events

        private const uint MaxUnparsableBytes = 16;
        private const long MaxPercentageUnkownDataForCompleteStream = 5;
        private const long MaxPercentageUnkownDataForDetect = 25;
        private const long MaxBytesForDetect = 0x200000; // 2MB

        private readonly IDataScanner _containerDataScanner;
        private readonly IDataScanner _codecDataScanner;
        private readonly IDataScanner _codecStreamDataScanner;
        private readonly IDataReaderPool _dataReaderPool;
        private readonly Creator<IDataBlockBuilder> _createDataBlockBuilder;
        private readonly Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
        private IInputFile _inputFile;
        private IProgressReporter _codecProgressReporter;
        private IDataBlock _fixedDataBlock;
        private long _fixedDataBlockStartOffset;

        #region Properties
        public bool AllowOverlap
        {
            get { return _containerDataScanner.AllowOverlap; }
            set { _containerDataScanner.AllowOverlap = value; }
        }

        public IEnumerable<IDetector> ContainerDetectors
        {
            get { return _containerDataScanner.Detectors; }
            set { _containerDataScanner.Detectors = value; }
        }

        public IEnumerable<IDetector> CodecDetectors
        {
            get { return _codecDataScanner.Detectors; }
            set { _codecDataScanner.Detectors = value; }
        }

        public IEnumerable<IDetector> Detectors
        {
            get
            {
                List<IDetector> detectors = new List<IDetector>();
                detectors.AddRange(_codecDataScanner.Detectors);
                detectors.AddRange(_containerDataScanner.Detectors);
                return detectors.AsReadOnly();
            }
            set { throw new NotImplementedException("Setter on Detectors property of FileScanner class is not implemented."); }
        }

        private IDataBlock LastDataBlock { get; set; }
        #endregion Properties

        /// <summary>
        /// Creates a new file scanner for scanning (multiple) files.
        /// </summary>
        /// <param name="containerDataScanner">The <see cref="IDataScanner"/> for container formats</param>
        /// <param name="codecDataScanner">The <see cref="IDataScanner"/> for codec formats</param>
        /// <param name="dataReaderPool">The shared pool of file data readers</param>
        /// <param name="createDataBlockBuilder">The factory method for creating data blocks</param>
        /// <param name="createSubProgressReporter">The factory method for creating a sub progress reporter</param>
        /// data scanner fors canning codec streams (of a detected data block)</param>
        /// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
        public FileScanner(IDataScanner containerDataScanner, IDataScanner codecDataScanner, IDataScanner codecStreamDataScanner,
                           IDataReaderPool dataReaderPool, Creator<IDataBlockBuilder> createDataBlockBuilder,
                           Creator<IProgressReporter, IProgressReporter, long, long, long> createSubProgressReporter)
        {
            PreConditions.Argument("containerDataScanner").Value(containerDataScanner).IsNotNull();
            PreConditions.Argument("codecDataScanner").Value(codecDataScanner).IsNotNull();
            PreConditions.Argument("codecStreamDataScanner").Value(codecStreamDataScanner).IsNotNull();
            PreConditions.Argument("dataReaderPool").Value(dataReaderPool).IsNotNull();
            PreConditions.Argument("createDataBlockBuilder").Value(createDataBlockBuilder).IsNotNull();
            PreConditions.Argument("createSubProgressReporter").Value(createSubProgressReporter).IsNotNull();

            _containerDataScanner = containerDataScanner;
            _codecDataScanner = codecDataScanner;
            _codecStreamDataScanner = codecStreamDataScanner;
            _dataReaderPool = dataReaderPool;
            _createDataBlockBuilder = createDataBlockBuilder;
            _createSubProgressReporter = createSubProgressReporter;

            _codecDataScanner.DataBlockDetected += (sender, e) => FixAndReportDataBlock(e.DataBlock);
            _containerDataScanner.DataBlockDetected += (sender, e) => ScanCodecStreams(e, e.DataBlock);
            _containerDataScanner.UnknownDataDetected += (sender, e) => ScanForCodecFormats(_inputFile.CreateDataPacket().GetSubPacket(e.Offset, e.Length));

            // The default is that container blocks are allowed to overlap. See also issue DEFR-867.
            AllowOverlap = true;
        }

        private void ResetFixAndReportDataBlock(long startOffset)
        {
            _fixedDataBlockStartOffset = startOffset;
            _fixedDataBlock = null;
        }

        private void FixAndReportDataBlock(IDataBlock dataBlock)
        {
            IDataBlockBuilder dataBlockBuilder = _createDataBlockBuilder();
            dataBlockBuilder.DataFormat = dataBlock.DataFormat;
            dataBlockBuilder.Detectors = dataBlock.Detectors;
            dataBlockBuilder.StartOffset = dataBlock.StartOffset + _fixedDataBlockStartOffset;
            dataBlockBuilder.EndOffset = dataBlock.EndOffset + _fixedDataBlockStartOffset;
            dataBlockBuilder.InputFile = dataBlock.InputFile;
            dataBlockBuilder.IsFullFile = dataBlock.IsFullFile;
            dataBlockBuilder.PreviousFragment = _fixedDataBlock;
            dataBlockBuilder.ReferenceHeaderOffset = dataBlock.ReferenceHeaderOffset;
            dataBlockBuilder.ReferenceHeader = dataBlock.ReferenceHeader;
            _fixedDataBlock = dataBlockBuilder.Build();

            ReportDataBlock(_fixedDataBlock);
        }

        public void Scan(IDataReader dataReader, IProgressReporter progressReporter)
        {
            ProgressReporterSummer progressReporterSummer = new ProgressReporterSummer(progressReporter);
            IProgressReporter containerProgressReporter = progressReporterSummer.AddProgressReporter(ContainerDetectors.Count());
            _codecProgressReporter = progressReporterSummer.AddProgressReporter(CodecDetectors.Count());

            LastDataBlock = null;

            try
            {
                using (_dataReaderPool)
                {
                    _containerDataScanner.Scan(dataReader, containerProgressReporter);
                }
            }
            finally
            {
                LastDataBlock = null;
            }
        }

        public void Scan(IInputFile inputFile, IDataReader dataReader, IProgressReporter progressReporter)
        {
            if (inputFile.Length == 0L) return;
            _inputFile = inputFile;

            Scan(dataReader, progressReporter);
        }

        #region Inner classes
        /// <summary>
        /// The <see cref="ProgressReporterSummer"/> reports the weighted sum of the
        /// progress reporters it contains to an existing <see cref="IProgressReporter"/>.
        /// </summary>
        private sealed class ProgressReporterSummer
        {
            private readonly IProgressReporter _targetProgressReporter;
            private readonly IDictionary<PartialProgressReporter, int> _partialProgressReporters;
            private int _currentPercentProgress;
            private int _totalWeight;

            #region Properties
            private bool CancellationPending { get { return _targetProgressReporter.CancellationPending; } }
            #endregion Properties

            /// <summary>
            /// Creates a new <see cref="ProgressReporterSummer"/>.
            /// </summary>
            /// <param name="progressReporter">The progress reporter to report to</param>
            internal ProgressReporterSummer(IProgressReporter progressReporter)
            {
                _targetProgressReporter = progressReporter;
                _partialProgressReporters = new Dictionary<PartialProgressReporter, int>();
                _currentPercentProgress = -1;
            }

            /// <summary>
            /// Creates and adds a new <see cref="IProgressReporter"/> that will
            /// contribute with given <paramref name="weight"/> to the overall progress.
            /// </summary>
            /// <param name="weight">The weight factor to apply to progress</param>
            /// <returns>The newly created progress reporter</returns>
            internal IProgressReporter AddProgressReporter(int weight)
            {
                PartialProgressReporter partialProgressReporter = new PartialProgressReporter(this);
                _partialProgressReporters.Add(partialProgressReporter, weight);
                _totalWeight += weight;
                return partialProgressReporter;
            }

            private void UpdateOverallProgress()
            {
                int percentProgress = GetOverallProgressPercentage();
                if (percentProgress != _currentPercentProgress)
                {
                    _currentPercentProgress = percentProgress;
                    _targetProgressReporter.ReportProgress(_currentPercentProgress);
                }
            }

            private void ReportUserState(object userState)
            {
                _currentPercentProgress = GetOverallProgressPercentage();
                _targetProgressReporter.ReportProgress(_currentPercentProgress, userState);
            }

            /// <summary>
            /// Gets the overall progress percentage.
            /// </summary>
            /// <returns>The weighted sum of all child progress reporters</returns>
            private int GetOverallProgressPercentage()
            {
                return _partialProgressReporters.Sum(x => (x.Key.ProgressPercentage * x.Value)) / _totalWeight;
            }

            /// <summary>
            /// The <see cref="PartialProgressReporter"/> contributes to the progress of a
            /// <see cref="ProgressReporterSummer"/> when the <see cref="ReportProgress()"/>
            /// method of this class is invoked.
            /// </summary>
            private sealed class PartialProgressReporter : IProgressReporter
            {
                private readonly ProgressReporterSummer _progressReporterSummer;
                private int _currentPercentProgress;

                #region Properties
                public bool CancellationPending { get { return _progressReporterSummer.CancellationPending; } }
                internal int ProgressPercentage { get { return _currentPercentProgress; } }
                #endregion Properties

                /// <summary>
                /// Creates a new <see cref="PartialProgressReporter"/> that reports
                /// to the given <paramref name="progressReporterSummer"/>
                /// </summary>
                /// <param name="progressReporterSummer">The summer to report to</param>
                internal PartialProgressReporter(ProgressReporterSummer progressReporterSummer)
                {
                    _progressReporterSummer = progressReporterSummer;
                    _currentPercentProgress = -1;
                }

                public void ReportProgress(int percentProgress)
                {
                    if (percentProgress != _currentPercentProgress)
                    {
                        _currentPercentProgress = percentProgress;
                        _progressReporterSummer.UpdateOverallProgress();
                    }
                }

                public void ReportProgress(int percentProgress, object userState)
                {
                    _currentPercentProgress = percentProgress;
                    _progressReporterSummer.ReportUserState(userState);
                }
            }
        }

        private sealed class CompleteCodecStreamScanner : IProgressReporter
        {
            private readonly IDataScanner _dataScanner;
            private readonly IDataBlockBuilder _dataBlockBuilder;
            private readonly ICodecStream _completeCodecStream;
            private readonly IProgressReporter _progressReporter;
            private readonly IList<IFragment> _codecStreamFragments;
            private readonly long _maxUnknownDataForCompleteStream;
            private readonly long _maxUnknownDataForDetect;
            private bool _cancelled;
            private bool _fragmented;
            private long _unknownData;

            #region Properties
            public bool CancellationPending
            {
                get { return _progressReporter.CancellationPending || _cancelled; }
            }

            internal bool HasCompleteCodecStream
            {
                get
                {
                    if (_cancelled)
                    {
                        return false;
                    }
                    if (_fragmented || (_codecStreamFragments.Count == 0))
                    {
                        return false;
                    }
                    if (_unknownData > _maxUnknownDataForCompleteStream)
                    {
                        return false;
                    }

                    return true;
                }
            }
            #endregion Properties

            internal CompleteCodecStreamScanner(IDataScanner dataScanner, IDataBlockBuilder dataBlockBuilder, ICodecStream completeCodecStream, IProgressReporter progressReporter)
            {
                _dataScanner = dataScanner;
                _dataBlockBuilder = dataBlockBuilder;
                _completeCodecStream = completeCodecStream;
                _progressReporter = progressReporter;
                _codecStreamFragments = new List<IFragment>();

                long length = _completeCodecStream.Length;
                _maxUnknownDataForCompleteStream = Math.Max((MaxPercentageUnkownDataForCompleteStream * length) / 100, Math.Min((2 * MaxUnparsableBytes), length));

                long detectBytes = Math.Min(length, MaxBytesForDetect);
                _maxUnknownDataForDetect = Math.Max((MaxPercentageUnkownDataForDetect * detectBytes) / 100, Math.Min((2 * MaxUnparsableBytes), detectBytes));
            }

            internal bool ScanForNearlyCompleteCodecStream(IDataReader dataReader)
            {
                dataReader.Position = 0L;

                Reset();

                _dataScanner.DataBlockDetected += DataBlockDetected;
                _dataScanner.UnknownDataDetected += UnknownDataDetected;
                try
                {
                    _dataScanner.Scan(dataReader, this);
                }
                finally
                {
                    _dataScanner.DataBlockDetected -= DataBlockDetected;
                    _dataScanner.UnknownDataDetected -= UnknownDataDetected;
                }

                if (_cancelled || (_codecStreamFragments.Count == 0))
                {
                    return false;
                }
                foreach (var fragment in _codecStreamFragments)
                {
                    AddRelativeCodecStream(_dataBlockBuilder, fragment);
                }
                return true;
            }

            internal IDetector ScanDetectForNearlyCompleteCodecStream(IDataReader dataReader, IEnumerable<IDetector> codecDetectors)
            {
                // First try to construct a complete codec stream using the preferred detectors,
                // i.e. the detectors that can handle the codec stream format.
                foreach (IDetector codecDetector in codecDetectors.Where(d => d.SupportedFormats.Contains(_completeCodecStream.DataFormat)))
                {
                    _dataScanner.Detectors = new[] { codecDetector };

                    if (ScanForNearlyCompleteCodecStream(dataReader))
                    {
                        return codecDetector;
                    }
                }

                // The preferred detectors did not detect a valid result, so try the other detectors.
                foreach (IDetector codecDetector in codecDetectors.Where(d => !d.SupportedFormats.Contains(_completeCodecStream.DataFormat)))
                {
                    _dataScanner.Detectors = new[] { codecDetector };

                    if (ScanForNearlyCompleteCodecStream(dataReader))
                    {
                        return codecDetector;
                    }
                }

                return null;
            }

            private void Reset()
            {
                _cancelled = false;
                _fragmented = false;
                _unknownData = 0L;

                _codecStreamFragments.Clear();
            }

            private void AddRelativeCodecStream(IDataBlockBuilder dataBlockBuilder, IFragment codecStreamFragment)
            {
                ICodecStreamBuilder relativeCodecStreamBuilder = dataBlockBuilder.AddCodecStream();
                relativeCodecStreamBuilder.StreamNumber = _completeCodecStream.StreamNumber;
                relativeCodecStreamBuilder.Name = _completeCodecStream.Name;
                relativeCodecStreamBuilder.DataFormat = codecStreamFragment.DataFormat;
                relativeCodecStreamBuilder.Detector = codecStreamFragment.Detectors.FirstOrDefault();
                relativeCodecStreamBuilder.Data = _completeCodecStream.InputFile.CreateDataPacket().GetSubPacket(codecStreamFragment.StartOffset, codecStreamFragment.Length);
                relativeCodecStreamBuilder.IsFragmented = codecStreamFragment.IsFragmented;
                relativeCodecStreamBuilder.AbsoluteStartOffset = _completeCodecStream.AbsoluteStartOffset + codecStreamFragment.StartOffset;
                relativeCodecStreamBuilder.ReferenceHeaderOffset = codecStreamFragment.ReferenceHeaderOffset;
                relativeCodecStreamBuilder.ReferenceHeader = codecStreamFragment.ReferenceHeader;
            }

            private void DataBlockDetected(object sender, DataBlockDetectedEventArgs e)
            {
                if (!_fragmented && !IsFragmentOfCompleteCodecStream(e.DataBlock))
                {
                    // Incomplete codec stream
                    _fragmented = true;
                }

                _codecStreamFragments.Add(e.DataBlock);
            }

            private bool IsFragmentOfCompleteCodecStream(IFragment fragment)
            {
                if (fragment.FragmentIndex > 0)
                {
                    return true; // Fragment is part of one (possibly complete) codec stream
                }
                if (_codecStreamFragments.Count > 0)
                {
                    return false; // Fragments belonging to another stream detected
                }

                return true;
            }

            private void UnknownDataDetected(object sender, UnknownDataDetectedEventArgs e)
            {
                _unknownData += e.Length;

                if (e.Offset < MaxBytesForDetect)
                {
                    long dataAfterLimit = Math.Max(0, (e.Offset + e.Length) - MaxBytesForDetect);
                    if ((_unknownData - dataAfterLimit) > _maxUnknownDataForDetect)
                    {
                        // Too much unknown data detected
                        _codecStreamFragments.Clear();
                        _cancelled = true;
                    }
                }
            }

            public void ReportProgress(int percentProgress)
            {
                _progressReporter.ReportProgress(percentProgress);
            }

            public void ReportProgress(int percentProgress, object userState)
            {
                _progressReporter.ReportProgress(percentProgress, userState);
            }
        }
        #endregion Inner classes

        public void Scan(IInputFile inputFile, IProgressReporter progressReporter)
        {
            PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();
            PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

            if (inputFile.Length == 0L) return;

            _inputFile = inputFile;

            using (IDataReader dataReader = _dataReaderPool.CreateDataReader(inputFile.CreateDataPacket()))
            {
                Scan(dataReader, progressReporter);
            }
        }

        private void ScanForCodecFormats(IDataPacket dataPacket)
        {
            ResetFixAndReportDataBlock(dataPacket.StartOffset);

            using (IDataReader dataReader = _dataReaderPool.CreateDataReader(dataPacket))
            {
                IProgressReporter subProgressReporter = _createSubProgressReporter(_codecProgressReporter, dataPacket.StartOffset, dataPacket.Length, dataPacket.InputFile.Length);
                _codecDataScanner.Scan(dataReader, subProgressReporter);
            }
        }

        private void ScanCodecStreams(DataBlockDetectedEventArgs e, IDataBlock dataBlock)
        {
            IProgressReporter dataBlockProgressReporter = _createSubProgressReporter(_codecProgressReporter, dataBlock.StartOffset, dataBlock.Length, dataBlock.InputFile.Length);
            long totalStreamBytes = dataBlock.CodecStreams.Sum(x => x.Length);
            long streamBytesScanned = 0L;
            bool hasCompleteCodecStream = false;
            IDetector detector = null;

            IDataBlockBuilder dataBlockBuilder = _createDataBlockBuilder();
            dataBlockBuilder.DataFormat = dataBlock.DataFormat;
            dataBlockBuilder.Detectors = dataBlock.Detectors;
            dataBlockBuilder.InputFile = dataBlock.InputFile;
            dataBlockBuilder.IsFullFile = dataBlock.IsFullFile;
            dataBlockBuilder.IsFragmented = dataBlock.IsFragmented;
            dataBlockBuilder.PreviousFragment = LastDataBlock;
            dataBlockBuilder.StartOffset = dataBlock.StartOffset;
            dataBlockBuilder.EndOffset = dataBlock.EndOffset;
            dataBlockBuilder.ReferenceHeaderOffset = dataBlock.ReferenceHeaderOffset;
            dataBlockBuilder.ReferenceHeader = dataBlock.ReferenceHeader;

            foreach (ICodecStream completeCodecStream in dataBlock.CodecStreams)
            {
                IProgressReporter codecStreamProgressReporter = _createSubProgressReporter(dataBlockProgressReporter, streamBytesScanned, completeCodecStream.Length, totalStreamBytes);
                var completeCodecStreamScanner = new CompleteCodecStreamScanner(_codecStreamDataScanner, dataBlockBuilder, completeCodecStream, codecStreamProgressReporter);

                // Note: The container detector that was used is not available to the codec detector!

                // NOTE: We assume that a file never uses multiple codec formats that are supported by Defraser.
                //       So, if at any time for example audio codecs will be supported, this code should be revised!
                using (IDataReader dataReader = _dataReaderPool.CreateDataReader(completeCodecStream, completeCodecStreamScanner))
                {
                    if (detector != null)
                    {
                        // Only scan other streams with the detector that already produced a valid and complete result
                        if (!completeCodecStreamScanner.ScanForNearlyCompleteCodecStream(dataReader))
                        {
                            AddUnknownCodecStream(dataBlockBuilder, completeCodecStream);
                        }
                    }
                    else
                    {
                        detector = completeCodecStreamScanner.ScanDetectForNearlyCompleteCodecStream(dataReader, CodecDetectors);

                        if (detector == null)
                        {
                            AddUnknownCodecStream(dataBlockBuilder, completeCodecStream);
                        }
                    }

                    hasCompleteCodecStream |= completeCodecStreamScanner.HasCompleteCodecStream;
                }

                streamBytesScanned += completeCodecStream.Length;
            }

            if (!hasCompleteCodecStream)
            {
                e.DoubleScanPayload = true;
            }

            LastDataBlock = dataBlockBuilder.Build();

            ReportDataBlock(LastDataBlock);
        }

        private static void AddUnknownCodecStream(IDataBlockBuilder dataBlockBuilder, ICodecStream completeCodecStream)
        {
            ICodecStreamBuilder relativeCodecStreamBuilder = dataBlockBuilder.AddCodecStream();
            relativeCodecStreamBuilder.StreamNumber = completeCodecStream.StreamNumber;
            relativeCodecStreamBuilder.Name = completeCodecStream.Name;
            relativeCodecStreamBuilder.DataFormat = CodecID.Unknown;
            relativeCodecStreamBuilder.Detector = new UnknownFormatDetector();
            relativeCodecStreamBuilder.Data = completeCodecStream.InputFile.CreateDataPacket().GetSubPacket(0L, completeCodecStream.Length);
            relativeCodecStreamBuilder.IsFragmented = false;
            relativeCodecStreamBuilder.AbsoluteStartOffset = completeCodecStream.AbsoluteStartOffset;
        }

        private void ReportDataBlock(IDataBlock dataBlock)
        {
            if (DataBlockDetected != null)
            {
                DataBlockDetected(this, new DataBlockDetectedEventArgs(dataBlock));
            }
        }
    }
}
