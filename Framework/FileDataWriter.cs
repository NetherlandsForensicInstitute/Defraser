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
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="FileDataWriter"/> is used for writing result data to a file.
	/// </summary>
	public sealed class FileDataWriter : IDataWriter
	{
		/// <summary>The default I/O buffer size for read/write operations.</summary>
		private const int DefaultBufferSize = (16 * 1024);

		private readonly byte[] _buffer;
		private Stream _outputStream;

		/// <summary>
		/// Creates a <see cref="IDataWriter"/> for writing to <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath">The path of the file to write to</param>
		/// <exception cref="ArgumentNullException">If <paramref name="filePath"/> is <c>null</c></exception>
		public FileDataWriter(String filePath)
		{
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();

			_buffer = new byte[DefaultBufferSize];
			_outputStream = File.Create(filePath);
		}

		/// <summary>
		/// Closes the output stream.
		/// </summary>
		public void Dispose()
		{
			if (_outputStream != null)
			{
				_outputStream.Close();
				_outputStream = null;
			}
		}

		public void Write(IDataReader dataReader)
		{
			PreConditions.Object(this).IsDisposedIf(_outputStream == null);
			PreConditions.Argument("dataReader").Value(dataReader).IsNotNull();

			dataReader.Position = 0L;

			while (dataReader.State == DataReaderState.Ready)
			{
				long bytesRemaining = (dataReader.Length - dataReader.Position);
				int bytesRead = dataReader.Read(_buffer, 0, (int)Math.Min(_buffer.Length, bytesRemaining));
				_outputStream.Write(_buffer, 0, bytesRead);

				dataReader.Position += bytesRead;
			}
		}
	}
}
