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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The default implementation of <see cref="IDataReaderPool"/>.
	/// </summary>
	public sealed class DataReaderPool : IDataReaderPool
	{
		private readonly Creator<IDataReader, IDataPacket, IDataReaderPool> _createDataReader;
		private readonly Creator<IDataReader, IDataReader, IProgressReporter> _createProgressDataReader;
		private readonly Dictionary<IInputFile, IDataReader> _dataReaders;
		private readonly DataBlockScanner _dataBlockScanner;

		/// <summary>
		/// Creates a new <see cref="DataReaderPool"/>.
		/// </summary>
		/// <param name="createDataReader">The factory method for creating fragmented data readers</param>
		/// <param name="createProgressDataReader">The factory method for creating a progress reporting data reader</param>
		/// <param name="dataBlockScanner">TODO</param>
		public DataReaderPool(Creator<IDataReader, IDataPacket, IDataReaderPool> createDataReader,
		                      Creator<IDataReader, IDataReader, IProgressReporter> createProgressDataReader,
		                      DataBlockScanner dataBlockScanner)
		{
			PreConditions.Argument("createDataReader").Value(createDataReader).IsNotNull();
			PreConditions.Argument("createProgressDataReader").Value(createProgressDataReader).IsNotNull();
			PreConditions.Argument("dataBlockScanner").Value(dataBlockScanner).IsNotNull();

			_createDataReader = createDataReader;
			_createProgressDataReader = createProgressDataReader;
			_dataBlockScanner = dataBlockScanner;
			_dataReaders = new Dictionary<IInputFile, IDataReader>();
		}

		public void Dispose()
		{
			if (_dataReaders.Count > 0)
			{
				foreach (IDataReader dataReader in _dataReaders.Values)
				{
					dataReader.Dispose();
				}

				_dataReaders.Clear();
			}
		}

		public int ReadInputFile(IInputFile inputFile, long position, byte[] array, int arrayOffset, int count)
		{
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();

			IDataReader dataReader;
			if (!_dataReaders.TryGetValue(inputFile, out dataReader))
			{
				dataReader = inputFile.CreateDataReader();
				_dataReaders.Add(inputFile, dataReader);
			}

			dataReader.Position = position;
			return dataReader.Read(array, arrayOffset, count);
		}

		public IDataReader CreateDataReader(IDataPacket dataPacket)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();

			/*#region fix voor exporteren van codec streams
			if ((dataPacket is ICodecStream) && (dataPacket.GetFragment(0).Length == dataPacket.Length))
			{
				dataPacket = _dataBlockScanner.GetData(dataPacket as ICodecStream, new NullProgressReporter(), this);
			}
			#endregion fix voor exporteren van codec streams*/
			return _createDataReader(dataPacket, this);
		}

		public IDataReader CreateDataReader(IDataPacket dataPacket, IProgressReporter progressReporter)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			/*#region fix voor exporteren van codec streams
			if ((dataPacket is ICodecStream) && (dataPacket.GetFragment(0).Length == dataPacket.Length))
			{
				dataPacket = _dataBlockScanner.GetData(dataPacket as ICodecStream, new NullProgressReporter(), this);
			}
			#endregion fix voor exporteren van codec streams*/
			return _createProgressDataReader(_createDataReader(dataPacket, this), progressReporter);
		}
	}
}
