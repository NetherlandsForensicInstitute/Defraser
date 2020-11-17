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
using System.IO;
using System.Linq;
using System.Text;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="SaveAsSeparateFiles"/> file export strategy saves multiple
	/// results (files, data blocks and/or codec streams) as separate files.
	/// </summary>
	/// <remarks>
	/// Fragments belonging to one data block or codec stream are combined into
	/// a single result.
	/// </remarks>
	public sealed class SaveAsSeparateFiles : IExportStrategy<IEnumerable<object>>
	{
		private readonly Creator<IDataWriter, string> _createDataWriter;
		private readonly Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
		private readonly DataBlockScanner _dataBlockScanner;
		private readonly IForensicIntegrityLog _forensicIntegrityLog;

		#region Inner classes
		private class HandledContainers
		{
			private readonly HashSet<IFragmentContainer> _handledFragmentContainers = new HashSet<IFragmentContainer>();
			private readonly HashSet<Pair<IFragmentContainer, string>> _handledFragmentContainerStreams = new HashSet<Pair<IFragmentContainer, string>>();

			public HandledContainers()
			{
			}

			public void ClearHandledFragmentedContainers()
			{
				_handledFragmentContainers.Clear();
				_handledFragmentContainerStreams.Clear();
			}

			public bool FragmentedStreamAlreadySaved(IFragment fragment)
			{
				if (fragment.FragmentContainer == null) return false; // This is not a fragmented stream

				if (fragment is IDataBlock)
				{
					if (_handledFragmentContainers.Contains(fragment.FragmentContainer))
					{
						return true;
					}
					_handledFragmentContainers.Add(fragment.FragmentContainer);
				}
				else if (fragment is ICodecStream)
				{
					ICodecStream codecStream = fragment as ICodecStream;
					var pair = new Pair<IFragmentContainer, string>(codecStream.DataBlock.FragmentContainer, codecStream.Name);
					if (_handledFragmentContainerStreams.Contains(pair))
					{
						return true;
					}
					_handledFragmentContainerStreams.Add(pair);
				}

				return false;
			}
		}

		/// <summary>
		/// Wrapper class for the progress reporter.
		/// During the save as separate files task,
		/// many methods report a part of the overall progress
		/// this class combines all these separate progress reports
		/// into one progress report.
		/// </summary>
		private class OverallProgressReporter : IProgressReporter
		{
			/// <summary>The wrapped progress reporter</summary>
			private readonly IProgressReporter _progressReporter;
			/// <summary>
			/// Number of parts that this overall progress reporter consists of.
			/// Each part report from 0 to 100%.
			/// </summary>
			private int _partCount;
			/// <summary>The part that currenty reports</summary>
			private int _currentPart;
			/// <summary>Help member used to increment the currentPart member</summary>
			private bool _endOfPartReached;
			/// <summary>The value (it's a percentage) of this constant is used to switch  <code "_endOfPartReached"/> to true</summary>
			private const int EndOfPart = 80;

			/// <summary>Constructor that wraps a progress reporter</summary>
			/// <param name="progressReporter">The progress reporter to wrap</param>
			public OverallProgressReporter(IProgressReporter progressReporter)
			{
				if (progressReporter == null)
				{
					throw new ArgumentNullException("progressReporter");
				}
				_progressReporter = progressReporter;
			}

			public bool CancellationPending
			{
				get { return _progressReporter.CancellationPending; }
			}

			public void ReportProgress(int percentProgress)
			{
				ReportProgress(percentProgress, null);
			}

			public void ReportProgress(int percentProgress, object userState)
			{
				Debug.Assert(_partCount != 0);

				if (percentProgress > EndOfPart)
				{
					_endOfPartReached = true;
				}
				if (percentProgress == 0 && _endOfPartReached)
				{
					_endOfPartReached = false;
					_currentPart++;
				}

				_progressReporter.ReportProgress(_currentPart * (100 / _partCount) + percentProgress / _partCount, userState);
			}

			public void CountNumberOfParts(IEnumerable<object> items)
			{
				int codecStreamCount = 0;
				int containerStreamCount = 0;
				int uniqueStreamCount = 0;
				HashSet<IFragmentContainer> handledFragmentContainers = new HashSet<IFragmentContainer>();
				HashSet<Pair<IFragmentContainer, string>> handledFragmentContainerStreams = new HashSet<Pair<IFragmentContainer, string>>();

				foreach (object item in items)
				{
					IInputFile inputFile = item as IInputFile;
					IDataBlock containerStream = item as IDataBlock;
					ICodecStream codecStream = item as ICodecStream;

					if (inputFile != null)
					{
						IList<IDataBlock> dataBlocks = inputFile.Project.GetDataBlocks(inputFile);
						containerStreamCount += dataBlocks.Count;

						foreach (IDataBlock dataBlock in dataBlocks)
						{
							codecStreamCount += dataBlock.CodecStreams.Count;
							uniqueStreamCount += CountUniqueStreams(handledFragmentContainers, handledFragmentContainerStreams, dataBlock);
						}
					}
					else if (containerStream != null)
					{
						containerStreamCount++;
						codecStreamCount += containerStream.CodecStreams.Count;
						uniqueStreamCount += CountUniqueStreams(handledFragmentContainers, handledFragmentContainerStreams, containerStream);
					}
					else if (codecStream != null)
					{
						// Count the number of fragments for this codecStream.
						//  The codec stream can be part of a fragmented codec stream
						//  which on its turn can be part of a fragmented container stream
						if (codecStream.DataBlock.FragmentContainer != null)
						{
							foreach (IDataBlock containerFragment in codecStream.DataBlock.FragmentContainer)
							{
								codecStreamCount += CountCodecStreamFragments(containerFragment, codecStream.Name);
							}
						}
						else
						{
							codecStreamCount += CountCodecStreamFragments(codecStream.DataBlock, codecStream.Name);
						}
						if (FragmentedStreamAlreadySaved(handledFragmentContainers, handledFragmentContainerStreams, codecStream) == false)
						{
							uniqueStreamCount++;
						}
					}
				}

				// Progress is reported for each:
				// - codec stream (twice, once during DetectData and once during GetDataPacket)
				// - container stream and codec stream that is saved
				_partCount = codecStreamCount * 2 + containerStreamCount + uniqueStreamCount;
			}

			private static bool FragmentedStreamAlreadySaved(HashSet<IFragmentContainer> handledFragmentContainers, HashSet<Pair<IFragmentContainer, string>> handledFragmentContainerStreams, IFragment fragment)
			{
				if (fragment.FragmentContainer == null) return false; // This is not a fragmented stream

				if (fragment is IDataBlock)
				{
					if (handledFragmentContainers.Contains(fragment.FragmentContainer))
					{
						return true;
					}
					handledFragmentContainers.Add(fragment.FragmentContainer);
				}
				else if (fragment is ICodecStream)
				{
					ICodecStream codecStream = fragment as ICodecStream;
					var pair = new Pair<IFragmentContainer, string>(codecStream.DataBlock.FragmentContainer, codecStream.Name);
					if (handledFragmentContainerStreams.Contains(pair))
					{
						return true;
					}
					handledFragmentContainerStreams.Add(pair);
				}

				return false;
			}

			private static int CountCodecStreamFragments(IDataBlock dataBlock, string codecStreamName)
			{
				return dataBlock.CodecStreams.Where(c => (c.Name == codecStreamName)).Count();
			}

			private int CountUniqueStreams(HashSet<IFragmentContainer> handledFragmentContainers, HashSet<Pair<IFragmentContainer, string>> handledFragmentContainerStreams, IDataBlock containerStream)
			{
				if (containerStream == null) return 0;

				int uniqueStreamCount = 0;
				if (FragmentedStreamAlreadySaved(handledFragmentContainers, handledFragmentContainerStreams, containerStream) == false)
				{
					uniqueStreamCount++;
				}

				if (containerStream.CodecStreams == null) return uniqueStreamCount;

				foreach (ICodecStream codecStream in containerStream.CodecStreams)
				{
					if (FragmentedStreamAlreadySaved(handledFragmentContainers, handledFragmentContainerStreams, codecStream) == false)
					{
						uniqueStreamCount++;
					}
				}
				return uniqueStreamCount;
			}
		}
		#endregion Inner classes

		/// <summary>
		/// Creates a new <see cref="SaveAsSeparateFiles"/> strategy.
		/// </summary>
		/// <param name="createDataWriter">The factory method for creating a file data writer</param>
		/// <param name="createSubProgressReporter">The factory method for creating a sub-progress reporter</param>
		public SaveAsSeparateFiles(Creator<IDataWriter, string> createDataWriter,
									Creator<IProgressReporter, IProgressReporter, long, long, long> createSubProgressReporter,
									DataBlockScanner dataBlockScanner,
									IForensicIntegrityLog forensicIntegrityLog)
		{
			PreConditions.Argument("createDataWriter").Value(createDataWriter).IsNotNull();
			PreConditions.Argument("createSubProgressReporter").Value(createSubProgressReporter).IsNotNull();
			PreConditions.Argument("dataBlockScanner").Value(dataBlockScanner).IsNotNull();
			PreConditions.Argument("forensicIntegrityLog").Value(forensicIntegrityLog).IsNotNull();

			_createDataWriter = createDataWriter;
			_createSubProgressReporter = createSubProgressReporter;
			_dataBlockScanner = dataBlockScanner;
			_forensicIntegrityLog = forensicIntegrityLog;
		}

		public void Save(IEnumerable<object> items, IEnumerable<IDetector> detectors, IDataReaderPool dataReaderPool, string directory, IProgressReporter progressReporter, bool createForensicIntegrityLog)
		{
			PreConditions.Argument("items").Value(items).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("detectors").Value(detectors).IsNotNull().And.DoesNotContainNull();
			PreConditions.Argument("dataReaderPool").Value(dataReaderPool).IsNotNull();
			PreConditions.Argument("directory").Value(directory).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			if (progressReporter.CancellationPending) return;

			var overallProgressReporter = new OverallProgressReporter(progressReporter);
			overallProgressReporter.CountNumberOfParts(items);
			var handledContainers = new HandledContainers();

			int numSavedFiles = 0;

			handledContainers.ClearHandledFragmentedContainers();

			foreach (object item in items)
			{
				string path = Path.Combine(directory, ReplaceIllegalPathCharactersByUnderscore(GetFileName(item)));

				numSavedFiles += SaveItem(item, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog);

				if (overallProgressReporter.CancellationPending) break;
			}
		}

		private static string GetFileName(object item)
		{
			IInputFile inputFile = item as IInputFile;
			if (inputFile != null) return Path.GetFileName(inputFile.Name);

			IDataBlock containerStream = item as IDataBlock;
			if (containerStream != null) return Path.GetFileName(containerStream.InputFile.Name);

			ICodecStream codecStream = item as ICodecStream;
			if (codecStream != null) return Path.GetFileName(codecStream.InputFile.Name);

			Debug.Fail("item should be of type IInputFile, IDataBlock or ICodecStream");
			return string.Empty;
		}

		private int SaveItem(object item, IDataReaderPool dataReaderPool, string path, OverallProgressReporter overallProgressReporter, HandledContainers handledContainers, bool createForensicIntegrityLog)
		{
			IInputFile inputFile = item as IInputFile;
			if (inputFile != null)
			{
				return SaveInputFile(inputFile, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog);
			}

			IDataBlock dataBlock = item as IDataBlock;
			if (dataBlock != null)
			{
				return SaveDataBlock(dataBlock, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog);
			}

			ICodecStream codecStream = item as ICodecStream;
			if (codecStream != null)
			{
				path = string.Format("{0}__{1}", path, ReplaceIllegalPathCharactersByUnderscore(codecStream.Name));
				return SaveCodecStream(codecStream, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog);
			}

			return 0;	// TODO: is this an error?
		}

		/// <summary>
		/// Saves all data blocks (and codec streams) for one <paramref name="inputFile"/>.
		/// </summary>
		/// <param name="inputFile">The input file to save</param>
		/// <param name="path">The directory in which to store the files</param>
		/// <returns>The number of files saved</returns>
		private int SaveInputFile(IInputFile inputFile, IDataReaderPool dataReaderPool, string path, OverallProgressReporter overallProgressReporter, HandledContainers handledContainers, bool createForensicIntegrityLog)
		{
			int numFullFiles = 0;

			foreach (IDataBlock dataBlock in inputFile.Project.GetDataBlocks(inputFile))
			{
				numFullFiles += SaveDataBlock(dataBlock, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog);
			}
			return numFullFiles;
		}

		/// <summary>
		/// Saves a <paramref name="dataBlock"/> and its codec streams.
		/// </summary>
		/// <param name="dataBlock">The data block to save</param>
		/// <param name="dataReaderPool">The shared pool of file data readers</param>
		/// <param name="path">The directory in which to store the files</param>
		/// <param name="overallProgressReporter"></param>
		/// <param name="handledContainers"></param>
		/// <param name="forensicIntegrityLogFile"></param>
		/// <returns>The number of files saved</returns>
		private int SaveDataBlock(IDataBlock dataBlock, IDataReaderPool dataReaderPool, string path, OverallProgressReporter overallProgressReporter, HandledContainers handledContainers, bool createForensicIntegrityLog)
		{
			if (string.IsNullOrEmpty(path) || dataBlock == null) return 0;

			int numFullFiles = 0;

			path += string.Format("_{0}", ReplaceIllegalPathCharactersByUnderscore(dataBlock.Detectors.First().Name));

			if (Write(dataBlock, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog))
			{
				numFullFiles++;

				// Save all detectables
				foreach (ICodecStream codecStream in dataBlock.CodecStreams)
				{
					string fileName = string.Format("{0}_extracted_{1}_stream", path,
					                                ReplaceIllegalPathCharactersByUnderscore(codecStream.DataFormat.ToString()));
					numFullFiles += SaveCodecStream(codecStream, dataReaderPool, fileName, overallProgressReporter, handledContainers,
					                                createForensicIntegrityLog);
				}
			}
			return numFullFiles;
		}

		/// <summary>
		/// Saves the <paramref name="codecStream"/> in the given <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The directory to save the file to</param>
		/// <param name="codecStream">The <see cref="ICodecStream"/> to save</param>
		/// <returns>The number of files saved</returns>
		private int SaveCodecStream(ICodecStream codecStream, IDataReaderPool dataReaderPool, string path, OverallProgressReporter overallProgressReporter, HandledContainers handledContainers, bool createForensicIntegrityLog)
		{
			return Write(codecStream, dataReaderPool, path, overallProgressReporter, handledContainers, createForensicIntegrityLog) == true ? 1 : 0;
		}

		private static string ReplaceIllegalPathCharactersByUnderscore(string path)
		{
			StringBuilder sb = new StringBuilder();
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			foreach (char c in path)
			{
				sb.Append(invalidFileNameChars.Contains(c) ? '_' : c);
			}
			return sb.ToString().Replace(", ", "_").Replace(" ", "_");
		}

		/// <summary>
		/// Writes <paramref name="detectable"/> to file with name <paramref name="fileName"/>.
		/// </summary>
		/// <remarks>
		/// WARNING call SaveExtensions.ClearHandledFragmentedContainers(); before you call this method
		/// to clear the handled fragment containers
		/// </remarks>
		/// <param name="detectable">the detectable to write</param>
		/// <param name="fileName">the name of the file to write to/param>
		/// <returns>true when the file is written, else false</returns>
		private bool Write(IFragment detectable, IDataReaderPool dataReaderPool, string directory, IProgressReporter progressReporter, HandledContainers handledContainers, bool createForensicIntegrityLog)
		{
			if (detectable == null)
			{
				throw new ArgumentNullException("detectable");
			}
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException("directory");
			}

			string path = directory;

			if (handledContainers.FragmentedStreamAlreadySaved(detectable as IFragment))
			{
				return false;
			}

			if (progressReporter != null) progressReporter.ReportProgress(0, "Gathering data");

			IDataPacket data = GetCompleteFragmentData(detectable, progressReporter, dataReaderPool);
			if (progressReporter.CancellationPending) return false;
			long totalBytes = data.Length;

			path += string.Format("_{0}", data.StartOffset.ToString("X16"));

			if (detectable is IDataBlock)
			{
				long length = ((detectable as IFragment).FragmentContainer == null)
								? detectable.Length
								: (detectable as IFragment).FragmentContainer.Length;
				path += string.Format("-{0}", (data.StartOffset + length).ToString("X16"));
			}

			path += (detectable.Detectors != null && detectable.Detectors.Count() > 0) ? detectable.Detectors.First().OutputFileExtension : ".bin";

			bool detectableWritten = false;

			try
			{
				long handledBytes = 0L;
				if (progressReporter != null)
				{
					string message = string.Format("Saving file: {0}", Path.GetFileName(path));
					progressReporter.ReportProgress((totalBytes == 0) ? 0 : (int) ((handledBytes*100)/totalBytes), message);
				}
				using (ResultWriter writer = new ResultWriter(File.Create(path), dataReaderPool))
				{
					writer.WriteDataPacket(data, progressReporter, ref handledBytes, totalBytes);
					detectableWritten = true;
				}
			}
			catch (Exception e)
			{
				string message = string.Format("Failed to write {0} to {1}", ((detectable is ICodecStream) ? ReplaceIllegalPathCharactersByUnderscore((detectable as ICodecStream).Name) : string.Empty), path);
				throw new FrameworkException(message, e);
			}

			try
			{
				if (createForensicIntegrityLog)
				{
					string logFileName = string.Format("{0}.csv", path);
					using (FileStream fs = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						_forensicIntegrityLog.Log(data, detectable.Detectors, path, fs, ForensicLogType.CopiedData);
					}
				}
			}
			catch (Exception e)
			{
				string message = string.Format("Failed to write log file {0}.cvs ({1})", path, e.Message);
				throw new FrameworkException(message, e);
			}
			return detectableWritten;
		}

		/// <summary>
		/// Gets the data of the entire detectable using all related fragments.
		/// </summary>
		/// <param name="detectable">the detectable</param>
		/// <returns>the complete data of the detectable</returns>
		private IDataPacket GetCompleteFragmentData(IFragment detectable, IProgressReporter progressReporter, IDataReaderPool dataReaderPool)
		{
			if ((detectable is IDataBlock) && (detectable.FragmentContainer == null))
			{
				// Unfragmented data block
				return detectable;
			}

			IDataPacket data = null;
			if (detectable is IDataBlock)
			{
				// Fragmented data block
				foreach (IFragment fragment in detectable.FragmentContainer)
				{
					data = (data == null) ? fragment : data.Append(fragment);
				}
				return data;
			}

			if (!(detectable is ICodecStream))
			{
				throw new ArgumentException("Not an IDataBlock or ICodecStream", "detectable");
			}

			ICodecStream codecStream = (detectable as ICodecStream);
			IDataBlock parent = codecStream.DataBlock;
			if (parent.FragmentContainer == null)
			{
				if (detectable.FragmentContainer == null)
				{
					// Unfragmented codec stream
					return _dataBlockScanner.GetData(detectable as ICodecStream, progressReporter, dataReaderPool);
				}

				// Fragmented codec stream
				foreach (IFragment fragment in detectable.FragmentContainer)
				{
					IDataPacket fragmentData = _dataBlockScanner.GetData(fragment as ICodecStream, progressReporter, dataReaderPool);
					if (progressReporter.CancellationPending) return null;

					data = (data == null) ? fragmentData : data.Append(fragmentData);
				}
				return data;
			}

			int streamIndex = codecStream.StreamNumber;

			// Fragmented codec stream in a fragmented data block
			foreach (IFragment dbFragment in parent.FragmentContainer)
			{
				foreach (ICodecStream fragment in ((IDataBlock)dbFragment).CodecStreams)
				{
					if (fragment.StreamNumber == streamIndex)
					{
						IDataPacket fragmentData = _dataBlockScanner.GetData(fragment, progressReporter, dataReaderPool);
						if (progressReporter.CancellationPending) return null;

						data = (data == null) ? fragmentData : data.Append(fragmentData);
					}
				}
			}
			return data;
		}
	}
}
