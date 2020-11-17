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

using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.FFmpegConverter;
using Defraser.Interface;
using Defraser.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Defraser.Framework
{
    public class DefRazor
    {
        private readonly IContainer _container;

        IDictionary<string, IDetector> _codecDetectorDictionary = new Dictionary<string, IDetector>();
        IDictionary<string, IDetector> _containerDetectorDictionary = new Dictionary<string, IDetector>();

        public DefRazor(string backEndFolder)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FrameworkModule());

            // TODO: added this when trying to get exporter to work
            builder.RegisterModule(new DataContractSerializerModule());
            builder.RegisterCreator<IDataReaderPool>();

            _container = builder.Build();

            InitCodecDetectors(backEndFolder);
            InitContainerDetectors(backEndFolder);
        }

        public IProject CreateProject(string filename)
        {
            Creator<IInputFile, IProject, string> inputFileCreator = _container.Resolve<Creator<IInputFile, IProject, string>>();
            return new Project(inputFileCreator, filename);
        }

        public IInputFile CreateInputFile(IProject project, string filename)
        {
            return _container.Resolve<IInputFile>(new TypedParameter(typeof(string), filename), new TypedParameter(typeof(IProject), project));
        }

        public IFileScanner CreateFileScanner()
        {
            return _container.Resolve<IFileScanner>();
        }

        public DataBlockScanner CreateDataBlockScanner()
        {
            return _container.Resolve<DataBlockScanner>();
        }

        public IDetectorFactory CreateDetectorFactory()
        {
            return _container.Resolve<IDetectorFactory>();
        }

        public Creator<IDataReaderPool> CreateDataReaderPoolCreator()
        {
            return _container.Resolve<Creator<IDataReaderPool>>();
        }

        public IDataReaderPool CreateDataReaderPool()
        {
            return _container.Resolve<IDataReaderPool>();
        }

        public StringList GetCodecDetectors()
        {
            return new StringList(_codecDetectorDictionary.Keys);
        }

        public StringList GetContainerDetectors()
        {
            return new StringList(_containerDetectorDictionary.Keys);
        }

        private void InitContainerDetectors(string backEndFolder)
        {
            IDetectorFactory detectorFactory = CreateDetectorFactory();
            detectorFactory.Initialize(backEndFolder);

            foreach (var detector in detectorFactory.ContainerDetectors)
            {
                _containerDetectorDictionary.Add(detector.Name, detector);
            }
        }

        private void InitCodecDetectors(string backEndFolder)
        {
            IDetectorFactory detectorFactory = CreateDetectorFactory();
            detectorFactory.Initialize(backEndFolder);
            foreach (var detector in detectorFactory.CodecDetectors)
            {
                _codecDetectorDictionary.Add(detector.Name, detector);
            }
        }

        public string GetVersion(string detectorName)
        {
            IDetector detector;

            if (_codecDetectorDictionary.TryGetValue(detectorName, out detector))
            {
                return detector.VersionString();
            }

            if (_containerDetectorDictionary.TryGetValue(detectorName, out detector))
            {
                return detector.VersionString();
            }

            return null;
        }

        public void Carve(string path, long position, long length, IDataBlockCallback callback)
        {
            IProject project = CreateProject("C:/folderDieNietBestaat");
            IInputFile inputFile = CreateInputFile(project, path);
            IFileScanner fileScanner = CreateFileScanner();
            IDetectorFactory detectorFactory = CreateDetectorFactory();

            fileScanner.CodecDetectors = detectorFactory.CodecDetectors;
            fileScanner.ContainerDetectors = detectorFactory.ContainerDetectors;
            fileScanner.DataBlockDetected += (o, e) => { callback.DataBlockDetectedWithCodecs(e.DataBlock.StartOffset, e.DataBlock.EndOffset, GetCodecStreamsAndKeyFrames(e.DataBlock)); };

            (fileScanner as FileScanner).Scan(inputFile, CreateDataReaderPool().CreateDataReader(inputFile.CreateDataPacket().GetSubPacket(position, length)), new NullProgressReporter());
        }

        public void Carve(string path, long position, long length, StringList codecDetectors, StringList containerDetectors, IDataBlockCallback callback, IBaseProgressListener baseProgressListener)
        {
            IProject project = CreateProject("C:/nonExistingFolder");
            IInputFile inputFile = CreateInputFile(project, path);
            IFileScanner fileScanner = CreateFileScanner();
            IDetectorFactory detectorFactory = CreateDetectorFactory();
            CancelableProgressAdapter progressReporter = new CancelableProgressAdapter(baseProgressListener);

            fileScanner.CodecDetectors = detectorFactory.CodecDetectors.Where(detector => codecDetectors.Contains(detector.Name));
            fileScanner.ContainerDetectors = detectorFactory.ContainerDetectors.Where(detector => containerDetectors.Contains(detector.Name));
            fileScanner.DataBlockDetected += (o, e) => { callback.DataBlockDetectedWithCodecs(e.DataBlock.StartOffset, e.DataBlock.EndOffset, GetCodecStreamsAndKeyFrames(e.DataBlock)); };

            try
            {
                StartPolling(progressReporter);
                (fileScanner as FileScanner).Scan(inputFile, CreateDataReaderPool().CreateDataReader(inputFile.CreateDataPacket().GetSubPacket(position, length)), progressReporter);
            }
            finally
            {
                _polling = false;
            }
        }

        private volatile bool _polling = false;

        private void StartPolling(CancelableProgressAdapter progressReporter)
        {
            _polling = true;
            new Thread(() =>
            {
                while (_polling)
                {
                    bool isCancelled = progressReporter.Cancelling();
                    if (isCancelled)
                    {
                        progressReporter.CancellationPending = true;
                        _polling = false;
                        break;
                    }
                    Thread.Sleep(500);
                }
            }).Start();
        }

        private ResultNode GetCodecStreamsAndKeyFrames(IDataBlock dataBlock)
        {
            DataBlockScanner scanner = CreateDataBlockScanner();

            if (dataBlock.Detectors.Where(d => d is ICodecDetector).Count() >= 1)
            {
                // Codec stream; rescan and return
                using (IDataReaderPool dataReaderPool = CreateDataReaderPool())
                {
                    IResultNode results = scanner.GetResults(dataBlock, new NullProgressReporter(), dataReaderPool);
                    KeyFrameList keyFrames = new KeyFrameList();
                    AddKeyFramesToList(results, keyFrames);

                    if (results.DataFormat == Interface.CodecID.H264)
                    {
                        // TODO: will this always work?
                        return new CodecStreamNode(dataBlock, results.DataFormat, IsH264NalUnitResultFoThaRealzWhenAlreadyH264(results), keyFrames, CreateRanges(results));
                    }

                    IDataPacket rescannedDataPacket = RescanDataPackets(new IDataPacket[] { dataBlock }, dataReaderPool, new NullProgressReporter());
                    if (rescannedDataPacket != null)
                    {
                        if (rescannedDataPacket is ICodecStream)
                        {
                            var codecStream = rescannedDataPacket as ICodecStream;
                            return new CodecStreamNode(codecStream, codecStream.DataFormat, keyFrames, BuildRangeList(codecStream));
                        }
                        else if (rescannedDataPacket is IDataBlock)
                        {
                            var dataPacket = rescannedDataPacket as IDataBlock;
                            return new CodecStreamNode(dataPacket, dataPacket.DataFormat, keyFrames, BuildRangeList(rescannedDataPacket));
                        }
                        // TODO: is this possible?
                        throw new Exception("not a codecstream, nor a datablock: " + rescannedDataPacket);
                    }
                    return new CodecStreamNode(dataBlock, dataBlock.DataFormat, keyFrames, BuildRangeList(dataBlock));
                }
            }
            else
            {
                // Container stream; iterate over children streams, rescan and return
                IList<CodecStreamNode> streams = new List<CodecStreamNode>();
                RangeList containerDataRanges = new RangeList();

                using (IDataReaderPool dataReaderPool = CreateDataReaderPool())
                {
                    containerDataRanges = BuildRangeList(RescanDataPackets(new IDataPacket[] { dataBlock }, dataReaderPool, new NullProgressReporter()));
                }

                foreach (var stream in dataBlock.CodecStreams)
                {
                    using (IDataReaderPool dataReaderPool = CreateDataReaderPool())
                    {
                        IResultNode results = scanner.GetResults(stream, new NullProgressReporter(), dataReaderPool);
                        KeyFrameList keyFrames = new KeyFrameList();
                        AddKeyFramesToList(results, keyFrames);

                        if (results.DataFormat == Interface.CodecID.H264)
                        {
                            // TODO: will this always work?
                            var node = new CodecStreamNode(stream, stream.DataFormat, IsH264NalUnitResultFoThaRealzWhenAlreadyH264(results), keyFrames, CreateRanges(results));
                            streams.Add(node);
                        }
                        else
                        {
                            IDataPacket rescannedDataPacket = RescanDataPackets(new IDataPacket[] { stream }, dataReaderPool, new NullProgressReporter());
                            if (rescannedDataPacket != null)
                            {
                                if (!(rescannedDataPacket is ICodecStream))
                                {
                                    // TODO: is this possible?
                                    throw new Exception("not a codecstream " + rescannedDataPacket);
                                }
                                var codecStream = rescannedDataPacket as ICodecStream;
                                streams.Add(new CodecStreamNode(codecStream, codecStream.DataFormat, keyFrames, BuildRangeList(codecStream)));
                            }
                            else
                            {
                                streams.Add(new CodecStreamNode(stream, stream.DataFormat, keyFrames, BuildRangeList(stream)));
                            }
                        }
                    }
                }

                return new ContainerNode(dataBlock, streams, containerDataRanges);
            }
        }

        private void AddKeyFramesToList(IResultNode node, KeyFrameList keyFrames)
        {
            if (node.IsKeyframe())
            {
                ICodecDetector detector = node.Detectors.Where(d => d is ICodecDetector).SingleOrDefault() as ICodecDetector;
                keyFrames.Add(new KeyFrame(BuildRangeList(detector.GetVideoHeaders(node)), BuildRangeList(detector.GetVideoData(node))));
            }
            else
            {
                foreach (var childNode in node.Children)
                {
                    AddKeyFramesToList(childNode, keyFrames);
                }
            }
        }

        private RangeList CreateRanges(IResultNode node)
        {
            return CreateRanges(node, new RangeList());
        }

        private RangeList CreateRanges(IResultNode node, RangeList rangeList)
        {
            if (node.Name != "Root")
            {
                rangeList.Add(new Range(node.StartOffset, node.Length));
            }
            foreach (var child in node.Children)
            {
                CreateRanges(child, rangeList);
            }
            return rangeList;
        }

        private static bool IsH264NalUnitResult(IResultNode result)
        {
            return (result.DataFormat == Interface.CodecID.H264) && (result.FindAttributeByName("NalUnitLength") != null);
        }

        private static bool IsH264NalUnitResultFoThaRealzWhenAlreadyH264(IResultNode result)
        {
            if (result.FindAttributeByName("NalUnitLength") != null)
            {
                return true;
            }
            foreach (var child in result.Children)
            {
                if (IsH264NalUnitResultFoThaRealzWhenAlreadyH264(child))
                {
                    return true;
                }
            }
            return false;
        }

        private RangeList BuildRangeList(IDataPacket dataPacket)
        {
            RangeList ranges = new RangeList();
            if (dataPacket == null)
            {
                return ranges;
            }

            long bytesRead = 0;
            while (bytesRead < dataPacket.Length)
            {
                IDataPacket fragment = dataPacket.GetFragment(bytesRead);
                ranges.Add(new Range(fragment.StartOffset, fragment.Length));
                bytesRead += fragment.Length;

            }
            return ranges;
        }

        private IDataPacket RescanDataPackets(IEnumerable<IDataPacket> dataPackets, IDataReaderPool dataReaderPool, IProgressReporter progressReporter)
        {
            IDataPacket concatenatedDataPacket = null;

            long totalByteCount = dataPackets.Sum(x => x.Length);
            long bytesSaved = 0L;

            foreach (IDataPacket dataPacket in dataPackets)
            {
                if (progressReporter.CancellationPending) return null;

                IDataPacket rescannedDataPacket = dataPacket;
                if (dataPacket is ICodecStream)
                {
                    if (dataPacket is RescannedCodecStream)
                    {
                        rescannedDataPacket = dataPacket;
                    }
                    else
                    {
                        IProgressReporter rescanProgressReporter = new NullProgressReporter();
                        rescannedDataPacket = CreateDataBlockScanner().GetData(dataPacket as ICodecStream, rescanProgressReporter, dataReaderPool);
                    }
                }

                concatenatedDataPacket = (concatenatedDataPacket == null) ? rescannedDataPacket : concatenatedDataPacket.Append(rescannedDataPacket);
                bytesSaved += dataPacket.Length;
            }
            return concatenatedDataPacket;
        }
    }
}
