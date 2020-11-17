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
	/// The <see cref="FileDataReader"/> reads data from (part of) a local file.
	/// </summary>
	/// <remarks>
	/// The <see cref="Length"/> of the file is checked upon opening the file.
	/// Changes in a file's length after creating a <see cref="FileDataReader"/>
	/// are therefore not reflected in the <see cref="Length"/> property.
	/// </remarks>
	public sealed class FileDataReader : IDataReader
	{
		private readonly IDataPacket _dataPacket;
		private long _position;
		private FileStream _fileStream;
		private DataReaderState _state;

		#region Properties
		public long Position
		{
			get { return _position; }
			set
			{
				PreConditions.Argument("value").Value(value).InRange(0, Length);

				_position = value;
				if (_fileStream != null)
				{
					_state = (value == Length) ? DataReaderState.EndOfInput : DataReaderState.Ready;
				}
			}
		}

		public long Length { get { return _dataPacket.Length; } }
		public DataReaderState State { get { return _state; } }
		#endregion Properties


		/// <summary>
		/// Creates a new <see cref="FileDataReader"/>.
		/// </summary>
		/// <param name="dataPacket">the data packet to read</param>
		/// <exception cref="ArgumentNullException">if <paramref name="dataPacket"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">if <paramref name="dataPacket"/> contains more than one <em>fragment</em></exception>
		/// <exception cref="ArgumentException">if <paramref name="dataPacket"/> extends beyond the end of the file</exception>
		/// <exception cref="FileNotFoundException">if <c>dataPacket.InputFile</c> does not exist</exception>
		public FileDataReader(IDataPacket dataPacket)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();
			PreConditions.Argument("dataPacket").IsInvalidIf((dataPacket.GetFragment(0L) != dataPacket), "Data packet contains more than one fragment");

			_dataPacket = dataPacket;
			_fileStream = new FileStream(dataPacket.InputFile.Name, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (dataPacket.EndOffset > _fileStream.Length)
			{
				throw new ArgumentException("Data packet extends beyond end-of-file.", "dataPacket");
			}

			Position = 0;	// This will also set the state
		}

		public void Dispose()
		{
			if (_fileStream != null)
			{
				_fileStream.Close();
				_fileStream = null;
				_state = DataReaderState.Cancelled;
			}
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataPacket.GetSubPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			PreConditions.Object(this).IsDisposedIf(_fileStream == null);
			PreConditions.Argument("array").Value(array).IsNotNull();
			PreConditions.Argument("arrayOffset").Value(arrayOffset).InRange(0, array.Length);
			PreConditions.Argument("count").Value(count).InRange(0, (array.Length - arrayOffset));

			int bytesToRead = (int)Math.Min(count, (Length - Position));
			_fileStream.Position = (_dataPacket.StartOffset + Position);
			return _fileStream.Read(array, arrayOffset, bytesToRead);
		}
	}
}
