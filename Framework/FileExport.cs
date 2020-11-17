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

using System.Collections.Generic;
using System.Linq;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The default implementation of <see cref="IFileExport"/>.
	/// </summary>
	public sealed class FileExport : IFileExport
	{
		private readonly Creator<IDataReaderPool> _createDataReaderPool;
		private readonly Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
		private readonly DataBlockScanner _dataBlockScanner;
		private readonly IExportStrategy<IInputFile> _saveAsSingleFile;
		private readonly IExportStrategy<IDataPacket> _saveAsContiguousFile;
		private readonly IExportStrategy<IEnumerable<object>> _saveAsSeparateFiles;
		private readonly ExportToXml _exportToXml;

		/// <summary>
		/// Creates a new <see cref="FileExport"/>.
		/// </summary>
		/// <param name="createDataReaderPool">
		/// The factory method for creating an <see cref="IDataReaderPool"/> to
		/// pass to the export strategies
		/// </param>
		/// <param name="createSubProgressReporter">The factory method for creating a sub-progress reporter</param>
		/// <param name="dataBlockScanner">The data block scanner</param>
		/// <param name="saveAsSingleFile">The strategy for saving a single input file</param>
		/// <param name="saveAsContiguousFile">The strategy for saving multiple results as a single file</param>
		/// <param name="saveAsSeparateFiles">The strategy for saving multiple results as separate files</param>
		/// <param name="exportToXml">The strategy for exporting results to an XML document</param>
		public FileExport(Creator<IDataReaderPool> createDataReaderPool,
							Creator<IProgressReporter, IProgressReporter, long, long, long> createSubProgressReporter,
							DataBlockScanner dataBlockScanner,
							IExportStrategy<IInputFile> saveAsSingleFile,
							IExportStrategy<IDataPacket> saveAsContiguousFile,
							IExportStrategy<IEnumerable<object>> saveAsSeparateFiles,
							ExportToXml exportToXml)
		{
			PreConditions.Argument("createDataReaderPool").Value(createDataReaderPool).IsNotNull();
			PreConditions.Argument("createSubProgressReporter").Value(createSubProgressReporter).IsNotNull();
			PreConditions.Argument("dataBlockScanner").Value(dataBlockScanner).IsNotNull();
			PreConditions.Argument("saveAsSingleFile").Value(saveAsSingleFile).IsNotNull();
			PreConditions.Argument("saveAsContiguousFile").Value(saveAsContiguousFile).IsNotNull();
			PreConditions.Argument("saveAsSeparateFiles").Value(saveAsSeparateFiles).IsNotNull();
			PreConditions.Argument("exportToXml").Value(exportToXml).IsNotNull();

			_createDataReaderPool = createDataReaderPool;
			_createSubProgressReporter = createSubProgressReporter;
			_dataBlockScanner = dataBlockScanner;
			_saveAsSingleFile = saveAsSingleFile;
			_saveAsContiguousFile = saveAsContiguousFile;
			_saveAsSeparateFiles = saveAsSeparateFiles;
			_exportToXml = exportToXml;
		}

		public void SaveAsSingleFile(IInputFile inputFile, string filePath, bool createForensicIntegrityLog, IProgressReporter progressReporter)
		{
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				_saveAsSingleFile.Save(inputFile, Enumerable.Empty<IDetector>(), dataReaderPool, filePath, progressReporter, createForensicIntegrityLog);
			}
		}

		public void SaveAsContiguousFile(IEnumerable<IDataPacket> dataPackets, IEnumerable<IDetector> detectors, string filePath, bool createForensicIntegrityLog)
		{
			SaveAsContiguousFile(dataPackets, detectors, filePath, createForensicIntegrityLog, new NullProgressReporter());
		}

		public void SaveAsContiguousFile(IEnumerable<IDataPacket> dataPackets, IEnumerable<IDetector> detectors, string filePath, bool createForensicIntegrityLog, IProgressReporter progressReporter)
		{
			PreConditions.Argument("dataPackets").Value(dataPackets).IsNotNull();
			PreConditions.Argument("detectors").Value(dataPackets).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				IDataPacket rescannedDataPacket = RescanDataPackets(dataPackets, dataReaderPool, progressReporter);
				if (rescannedDataPacket != null)
				{
					_saveAsContiguousFile.Save(rescannedDataPacket, detectors, dataReaderPool, filePath, progressReporter, createForensicIntegrityLog);
				}
			}
		}

		public void SaveAsSeparateFiles(IEnumerable<object> items, string directory, bool createForensicIntegrityLog)
		{
			SaveAsSeparateFiles(items, directory, createForensicIntegrityLog, new NullProgressReporter());
		}

		public void SaveAsSeparateFiles(IEnumerable<object> items, string directory, bool createForensicIntegrityLog, IProgressReporter progressReporter)
		{
			PreConditions.Argument("items").Value(items).IsNotNull();
			PreConditions.Argument("directory").Value(directory).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				_saveAsSeparateFiles.Save(items, Enumerable.Empty<IDetector>(), dataReaderPool, directory, progressReporter, createForensicIntegrityLog);
			}
		}

		public void ExportToXml(IEnumerable<IFragment> fragments, string filePath)
		{
			ExportToXml(fragments, filePath, new NullProgressReporter());
		}

		public void ExportToXml(IEnumerable<IFragment> fragments, string filePath, IProgressReporter progressReporter)
		{
			PreConditions.Argument("fragments").Value(fragments).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			using (IDataReaderPool dataReaderPool = _createDataReaderPool())
			{
				_exportToXml.Export(fragments, dataReaderPool, filePath, progressReporter);
			}
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
					// FIXME: This check should not be necessary, but the user interface is not
					//        consistent in rescanning data packets: either in the user interface
					//        itself, or in the framework!
					if (dataPacket is RescannedCodecStream)
					{
						// Note: data packet is already rescanned, so just append it ...
						rescannedDataPacket = dataPacket;
					}
					else
					{
						IProgressReporter rescanProgressReporter = _createSubProgressReporter(progressReporter, bytesSaved, dataPacket.Length, totalByteCount);
						rescannedDataPacket = _dataBlockScanner.GetData(dataPacket as ICodecStream, rescanProgressReporter, dataReaderPool);
					}
				}

				concatenatedDataPacket = (concatenatedDataPacket == null) ? rescannedDataPacket : concatenatedDataPacket.Append(rescannedDataPacket);

				bytesSaved += dataPacket.Length;
			}
			return concatenatedDataPacket;
		}
	}
}
