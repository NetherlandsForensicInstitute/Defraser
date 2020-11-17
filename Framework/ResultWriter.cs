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
using System.IO;
using Defraser.Interface;

namespace Defraser.Framework
{
	/// <summary>
	/// Writes file data indicated by results to an output stream or file.
	/// </summary>
	public class ResultWriter : IDisposable
	{
		private readonly byte[] _buffer = new byte[16 * 1024];	// 16KB IO buffer
		private Stream _outputStream;
		private long _bytesWritten = 0;
		private IDataReaderPool _dataReaderPool;


		/// <summary>
		/// Constructs a writer for the specified <paramref name="outStream"/>.
		/// </summary>
		/// <param name="outStream">the stream to write the results to</param>
		public ResultWriter(Stream outStream, IDataReaderPool dataReaderPool)
		{
			_outputStream = outStream;
			_dataReaderPool = dataReaderPool;
		}

		/// <summary>
		/// Closes all associated output and input streams.
		/// </summary>
		public void Dispose()
		{
			if (_outputStream != null)
			{
				_outputStream.Close();
				_outputStream = null;
			}
			if (_dataReaderPool != null)
			{
				// DO NOT DISPOSE THE DATA READER POOL !!!!
				_dataReaderPool = null;
			}
		}

		/// <summary>
		/// Writes the given <paramref name="result"/> and all of its children
		/// to the output file.
		/// </summary>
		/// <param name="result">the result to write</param>
		public void WriteResult(IResultNode result, IProgressReporter progressReporter, ref long handledBytes, long totalBytes)
		{
			WriteDataPacket(result, progressReporter, ref handledBytes, totalBytes);

			// Write child results
			if (result.Children.Count > 0)
			{
				foreach (IResultNode child in result.Children)
				{
					WriteResult(child, progressReporter, ref handledBytes, totalBytes);
				}
			}
		}

		public void WriteDataPacket(IDataPacket dataPacket, IProgressReporter progressReporter, ref long handledBytes, long totalBytes)
		{
			// TODO: refactor to use ProgressDataReader
			using (IDataReader dataReader = new FragmentedDataReader(dataPacket, _dataReaderPool))
			{
				// Write data fragment
				while (dataReader.State == DataReaderState.Ready)
				{
					long bytesRemaining = (dataReader.Length - dataReader.Position);
					int bytesRead = dataReader.Read(_buffer, 0, (int)Math.Min(_buffer.Length, bytesRemaining));
					dataReader.Position += bytesRead;

					_outputStream.Write(_buffer, 0, bytesRead);
					_bytesWritten += bytesRead;

					if (progressReporter != null)
					{
						handledBytes += bytesRead;

						if (progressReporter.CancellationPending) return;
						progressReporter.ReportProgress(totalBytes == 0 ? 0 : (int)((handledBytes * 100) / totalBytes));
					}
				}
			}
		}
	}
}
