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
using System.IO;

namespace Defraser
{
	public class FileScannerDataReaderPool : IDataReaderPool
	{
		/// <summary>The buffer size to use for reading input files.</summary>
		private const int BufferSize = 1024 * 1024;

		private readonly Dictionary<IInputFile, IDataReader> _dataReaders;
		private FileScanner _fileScanner;

		public FileScannerDataReaderPool(FileScanner fileScanner)
		{
			_dataReaders = new Dictionary<IInputFile, IDataReader>();
			_fileScanner = fileScanner;
		}

		public void Dispose()
		{
			foreach (IDataReader dataReader in _dataReaders.Values)
			{
				dataReader.Dispose();
			}

			_dataReaders.Clear();
		}

		public IDataReader GetDataReader(IInputFile inputFile)
		{
			if (inputFile == null)
			{
				throw new ArgumentNullException("inputFile");
			}

			IDataReader dataReader;
			if (_dataReaders.TryGetValue(inputFile, out dataReader))
			{
				return dataReader;
			}

			dataReader = new FileDataReader(new DataPacket(inputFile));
			// Add buffering decorator
			dataReader = new BufferedDataReader(dataReader, BufferSize);
			// Add progress reporting decorator
			IProgressReporter progressReporter = new FileScannerProgressReporter(_fileScanner, dataReader.Length);
			dataReader = new ProgressDataReader(dataReader, progressReporter);

			_dataReaders.Add(inputFile, dataReader);
			return dataReader;
		}

		public IDataReader GetDataReader(IDataPacket dataPacket)
		{
			return new FragmentedDataReader(dataPacket, this);
		}
	}
}
