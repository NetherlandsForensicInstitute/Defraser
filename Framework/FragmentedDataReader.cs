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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Provides a consecutive view of (possibly) fragmented data.
	/// The data is read directly from the underlying data reader(s).
	/// </summary>
	/// <remarks>
	/// The <see cref="Position"/> refers to the relative offset in the data.
	/// The <see cref="Length"/> refers to the total length of the fragmented data.
	/// </remarks>
	public sealed class FragmentedDataReader : IDataReader
	{
		private readonly IDataPacket _dataPacket;
		private IDataReaderPool _dataReaderPool;
		private long _position;

		#region Properties
		public long Position
		{
			get { return _position; }
			set
			{
				PreConditions.Argument("value").Value(value).InRange(0L, Length);

				_position = value;
			}
		}

		public long Length { get { return _dataPacket.Length; } }

		public DataReaderState State
		{
			get
			{
				if (_dataReaderPool == null)
				{
					return DataReaderState.Cancelled;
				}

				return (Position < Length) ? DataReaderState.Ready : DataReaderState.EndOfInput;
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="FragmentedDataReader"/> for reading the
		/// (possibly) fragmented <paramref name="dataPacket"/> using the given
		/// <paramref name="dataReaderPool"/>.
		/// </summary>
		/// <remarks>
		/// The fragments of the data packet can refer to multiple files, which will
		/// be opened using the <paramref name="dataReaderPool"/>. The <see cref="Dispose"/>
		/// method will close all open files by disposing the <see cref="IDataReaderPool"/>.
		/// </remarks>
		/// <param name="dataPacket">the data packet</param>
		/// <param name="dataReaderPool">the pool of data readers to use</param>
		public FragmentedDataReader(IDataPacket dataPacket, IDataReaderPool dataReaderPool)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();
			PreConditions.Argument("dataReaderPool").Value(dataReaderPool).IsNotNull();

			_dataPacket = dataPacket;
			_dataReaderPool = dataReaderPool;
			_position = 0;
		}

		public void Dispose()
		{
			if (_dataReaderPool != null)
			{
				// It should probably not dispose the pool, since that would defeat
				// the purpose of a _shared_ pool of data readers!
				//_dataReaderPool.Dispose();
				_dataReaderPool = null;
			}
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataPacket.GetSubPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			PreConditions.Object(this).IsDisposedIf(_dataReaderPool == null);
			PreConditions.Argument("array").Value(array).IsNotNull();
			PreConditions.Argument("arrayOffset").Value(arrayOffset).InRange(0, array.Length);
			PreConditions.Argument("count").Value(count).InRange(0, (array.Length - arrayOffset));

			int totalBytesToRead = (int)Math.Min(count, (Length - Position));
			int bytesRead = 0;

			while (bytesRead < totalBytesToRead)
			{
				IDataPacket fragment = _dataPacket.GetFragment(_position + bytesRead);

				int fragmentBytes = (int)Math.Min((totalBytesToRead - bytesRead), fragment.Length);
				int fragmentBytesRead = _dataReaderPool.ReadInputFile(fragment.InputFile, fragment.StartOffset, array, (arrayOffset + bytesRead), fragmentBytes);
				bytesRead += fragmentBytesRead;

				// Completed or cancelled if not read the _entire_ fragment
				if (fragmentBytesRead != fragmentBytes)
				{
					break;
				}
			}
			return bytesRead;
		}
	}
}
