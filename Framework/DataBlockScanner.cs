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
using System.Diagnostics;
using System.Linq;
using Defraser.Detector.UnknownFormat;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Scans a data block or codec stream for results.
	/// </summary>
	/// <remarks>
	/// The results are cached using weak references. Therefore, subsequent
	/// calls to <see cref="GetResults"/> may return a cached
	/// copy, depending on that amount of available memory.
	/// </remarks>
	public class DataBlockScanner
	{
		private readonly Creator<IDataBlockBuilder> _createDataBlockBuilder;
		private readonly Creator<IScanContext, IProject> _createScanContext;
		private readonly IDictionary<IDataPacket, IResultNode> _resultsCache;
		private readonly IDictionary<IDataPacket, IDataPacket> _dataCache;

		/// <summary>
		/// Creates a new data block scanner.
		/// </summary>
		public DataBlockScanner(Creator<IDataBlockBuilder> createDataBlockBuilder, Creator<IScanContext, IProject> createScanContext)
		{
			PreConditions.Argument("createDataBlockBuilder").Value(createDataBlockBuilder).IsNotNull();
			PreConditions.Argument("createScanContext").Value(createScanContext).IsNotNull();

			_createDataBlockBuilder = createDataBlockBuilder;
			_createScanContext = createScanContext;
			_resultsCache = new WeakDictionary<IDataPacket, IResultNode>();
			_dataCache = new WeakDictionary<IDataPacket, IDataPacket>();
		}

		/// <summary>
		/// Gets the results for the given <paramref name="fragment"/> and
		/// reports progress to the given <paramref name="progressReporter"/>.
		/// </summary>
		/// <param name="fragment">the data block or codec stream to scan</param>
		/// <param name="progressReporter">the progress reporter</param>
		/// <param name="dataReaderPool">The shared pool of file data readers</param>
		/// <returns>the root node of the results</returns>
		public IResultNode GetResults(IFragment fragment, IProgressReporter progressReporter, IDataReaderPool dataReaderPool)
		{
			PreConditions.Argument("fragment").Value(fragment).IsNotNull();
			// TODO: fragment.Detector can be null !!
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			// Check if there exists a cached copy for the fragment
			IResultNode results;
			if (_resultsCache.TryGetValue(fragment, out results))
			{
				return results;
			}

			// During the rescan, set the IsFragmented property as it was during the first scan.
			// to make sure the result will be the same
			results = RescanDetectable(fragment, progressReporter, dataReaderPool);

			if (!progressReporter.CancellationPending)
			{
				CacheResults(fragment, results);
			}

			return results;
		}

		/// <summary>
		/// Caches <paramref name="results"/> for the given <paramref name="detectable"/>.
		/// </summary>
		/// <param name="detectable">the data block or codec stream</param>
		/// <param name="results">the root node of the results</param>
		internal void CacheResults(IDataPacket detectable, IResultNode results)
		{
			_resultsCache[detectable] = results;
		}

		/// <summary>
		/// Gets the data for the given <paramref name="codecStream"/> and
		/// reports progress to the given <paramref name="progressReporter"/>.
		/// This rescans the parent (container) of the codecStream to determine
		/// the data of codecStream.
		/// </summary>
		/// <param name="codecStream">the codec stream to retrieve the data for</param>
		/// <param name="progressReporter">the progress reporter</param>
		/// <param name="dataReaderPool">The shared pool of file data readers</param>
		/// <returns>the root node of the results</returns>
		public IDataPacket GetData(ICodecStream codecStream, IProgressReporter progressReporter, IDataReaderPool dataReaderPool)
		{
			IDataPacket data;
			if (_dataCache.TryGetValue(codecStream, out data))
			{
				return new RescannedCodecStream(codecStream, data);
			}

			IDataBlock containerStreamDataBlock = codecStream.DataBlock;
			using (IDataReader dataReader = new ProgressDataReader(new FragmentedDataReader(containerStreamDataBlock, dataReaderPool), progressReporter))
			{
				IDetector containerDetector = containerStreamDataBlock.Detectors.FirstOrDefault();
				IScanContext scanContext = _createScanContext(containerStreamDataBlock.InputFile.Project);
				scanContext.Detectors = new[] { containerDetector };
				IDataBlockBuilder containerStreamDataBlockBuilder = _createDataBlockBuilder();
				containerStreamDataBlockBuilder.Detectors = scanContext.Detectors;
				containerStreamDataBlockBuilder.InputFile = containerStreamDataBlock.InputFile;
				IDataBlock newContainerStream = containerDetector.DetectData(dataReader, containerStreamDataBlockBuilder, scanContext);

				// Check cancellation request
				if (progressReporter.CancellationPending) return null;

				// Check results / container stream
				if (newContainerStream == null)
				{
					RetrievedDetectableIsDifferentThanTheOneRetrievedDuringTheInitialScan();
					return null;
				}

				data = newContainerStream.CodecStreams.Where(c => c.StreamNumber == codecStream.StreamNumber).FirstOrDefault();

				if ((data == null) || (data.Length < codecStream.EndOffset))
				{
					RetrievedDetectableIsDifferentThanTheOneRetrievedDuringTheInitialScan();
					return null;
				}

				data = data.GetSubPacket(codecStream.StartOffset, codecStream.Length);
			}

			CacheData(codecStream, data);

			return new RescannedCodecStream(codecStream, data);
		}

		private static void RetrievedDetectableIsDifferentThanTheOneRetrievedDuringTheInitialScan()
		{
			// When you develop your own detector and get the message thrown by this exception:
#if DEBUG
			Debug.Fail("Retrieved detectable is different from the one retrieved during the initial scan.");
#else
			throw new FrameworkException("Retrieved detectable is different from the one retrieved during the initial scan.");
#endif
			// Than carefully check your detector code.
		}

		public void ClearCache()
		{
			_dataCache.Clear();
			_resultsCache.Clear();
		}

		private void CacheData(IDataPacket detectable, IDataPacket data)
		{
			_dataCache[detectable] = data;
		}

		private IResultNode RescanDetectable(IFragment detectable, IProgressReporter progressReporter, IDataReaderPool dataReaderPool)
		{
			IDataPacket data = detectable;
			if (detectable is ICodecStream)
			{
				// Data is specified relative to the codec stream (see IDetectable.Data)
				data = GetData(detectable as ICodecStream, progressReporter, dataReaderPool);

				if (progressReporter.CancellationPending) return null; // The rescan is cancelled

				var codecStream = detectable as ICodecStream;
				if (codecStream.ReferenceHeader != null)
				{
					long off = codecStream.ReferenceHeaderOffset;
					if (off == 0L)
					{
						data = codecStream.ReferenceHeader.Append(data);
					}
					else
					{
						data = data.GetSubPacket(0, off).Append(codecStream.ReferenceHeader).Append(data.GetSubPacket(off, data.Length - off));
					}
				}
			}
			if (detectable is IDataBlock)
			{
				IDataBlock dataBlock = detectable as IDataBlock;
				if (dataBlock.ReferenceHeader != null)
				{
					long off = dataBlock.ReferenceHeaderOffset;
					if (off == 0L)
					{
						data = dataBlock.ReferenceHeader.Append(detectable);
					}
					else
					{
						data = detectable.GetSubPacket(0, off).Append(dataBlock.ReferenceHeader).Append(detectable.GetSubPacket(off, detectable.Length - off));
					}
				}
			}

			IResultNode results;
			using (IDataReader dataReader = new ProgressDataReader(new FragmentedDataReader(data, dataReaderPool), progressReporter))
			{
				IScanContext scanContext = _createScanContext(detectable.InputFile.Project);
				scanContext.Detectors = detectable.Detectors;

				IDataBlockBuilder dataBlockBuilder = _createDataBlockBuilder();
				dataBlockBuilder.Detectors = scanContext.Detectors;
				dataBlockBuilder.InputFile = detectable.InputFile;

				IDetector detector = GetDetector(detectable);
				IDataBlock newDataBlock;

				var codecDetector = (detector as ICodecDetector);
				if (codecDetector == null)
				{
					newDataBlock = detector.DetectData(dataReader, dataBlockBuilder, scanContext);
				}
				else
				{
					var referenceHeaders = codecDetector.ReferenceHeaders;
					try
					{
						// Disable reference headers during rescan
						codecDetector.ReferenceHeaders = null;
						newDataBlock = detector.DetectData(dataReader, dataBlockBuilder, scanContext);
					}
					finally
					{
						// Restore reference headers
						codecDetector.ReferenceHeaders = referenceHeaders;
					}
				}

				if (!progressReporter.CancellationPending && newDataBlock == null)
				{
					RetrievedDetectableIsDifferentThanTheOneRetrievedDuringTheInitialScan();
					return null;
				}

				results = scanContext.Results;

				// Check cancellation request and results / data block
				if (!progressReporter.CancellationPending && (detectable is IDataBlock) && !IsDataBlockCorrect(detectable as IDataBlock, newDataBlock))
				{
					//RetrievedDetectableIsDifferentThanTheOneRetrievedDuringTheInitialScan();
					//return null;
				}
			}
			return results;
		}

		private static IDetector GetDetector(IFragment detectable)
		{
			var codecStream = detectable as ICodecStream;
			if (codecStream != null)
			{
				// Return the detector of the codec stream, i.e. not (one of) the detector(s)
				// of the container of the codec stream.
				return detectable.Detectors.Where(d => !codecStream.DataBlock.Detectors.Contains(d)).First();
			}
			if (detectable is IDataBlock)
			{
				// Note: Data blocks have only one detector!
				return detectable.Detectors.First();
			}

			throw new InvalidOperationException("Detectable is not a codec stream or data block: " + detectable);
		}

		/// <summary>
		/// Checks whether the <paramref name="newDataBlock"/> are not different from
		/// the initially detected <paramref name="initialDataBlock"/>.
		/// </summary>
		/// <param name="initialDataBlock">the initially detected data block</param>
		/// <param name="newDataBlock">the new data block to compare</param>
		/// <returns>whether the data block is correct</returns>
		private static bool IsDataBlockCorrect(IDataBlock initialDataBlock, IDataBlock newDataBlock)
		{
			if (newDataBlock == null)
			{
				return false;
			}
			if (newDataBlock.InputFile != initialDataBlock.InputFile)
			{
				return false;
			}
			if (newDataBlock.StartOffset != 0L)
			{
				return false;
			}
			if (newDataBlock.Length != initialDataBlock.Length)
			{
				return false;	// Size of data block differs
			}
			if (newDataBlock.IsFullFile != initialDataBlock.IsFullFile)
			{
				return false;	// IsFullFile property differs
			}
			if (newDataBlock.CodecStreams == null)
			{
				return initialDataBlock.CodecStreams.Count == 0;
			}

			// Codec streams can be divided into fragments when a maximum header count is reached
			// The data block object contains the fragmented codec streams,
			// the newDataBlock object contains the unfragmented codec streams.
			// This is handled by the 'Where' clause.

			// Also, codec streams that are split into multiple parts in the data block
			// are represented by a single codec stream in the 'newDataBlock' object.
			// This is handled by the 'GroupBy' clause.
			int initialCodecStreamCount = initialDataBlock.CodecStreams.Where(c => (c.FragmentIndex == 0)).GroupBy(c => c.StreamNumber).Count();
			if (initialCodecStreamCount != newDataBlock.CodecStreams.Count)
			{
				return false; // Codec stream count differs
			}

			// Check the codec and container stream detectors used
			foreach (ICodecStream initialCodecStream in initialDataBlock.CodecStreams)
			{
				if (initialCodecStream.FragmentIndex == 0)
				{
					int streamNumber = initialCodecStream.StreamNumber;
					ICodecStream newCodecStream = newDataBlock.CodecStreams.Where(c => c.StreamNumber == streamNumber).FirstOrDefault();
					if (newCodecStream == null)
					{
						return false; // Codec stream not found in rescanned data block
					}

					IDetector initialDetector = initialCodecStream.Detectors.FirstOrDefault();
					if (!IsCompatibleCodec(initialDetector, newCodecStream.DataFormat))
					{
						//It is possible that the container reports an incorrect format of the datastream
						//Therefore disabled:
						//return false; 
					}
				}
			}
			return true;
		}

		/// <summary>
		/// If the current detector is compatible with the data format
		/// </summary>
		/// <returns></returns>
		private static bool IsCompatibleCodec(IDetector detector, CodecID dataFormat)
		{
			if (detector == null)
				return true;
			if (dataFormat == CodecID.Unknown)
				return true;
			if (detector.DetectorType == typeof(UnknownFormatDetector))
				return true;
			if (detector.SupportedFormats.Contains(dataFormat))
				return true;
			return false;
		}
	}
}
