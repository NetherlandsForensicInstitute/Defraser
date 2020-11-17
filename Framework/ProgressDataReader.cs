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
	/// Decorates a data reader with progress reporting and cancellation
	/// state checking using <see cref="IProgressReporter"/>.
	/// </summary>
	/// <remarks>
	/// Progress is automatically reported based on the <c>Position</c> of
	/// the underlying data reader.
	/// <c>State</c> is set to <see cref="DataReaderState#Cancelled"/> if
	/// <see cref="IProjectReporter#CancellationPending"> is <c>true</c>.
	/// </remarks>
	public sealed class ProgressDataReader : IDataReader
	{
		private readonly IDataReader _dataReader;
		private readonly IProgressReporter _progressReporter;
		private long _nextProgressPosition;

		#region Properties
		public long Position
		{
			get { return _dataReader.Position; }
			set
			{
				_dataReader.Position = value;

				if (value >= _nextProgressPosition)
				{
					ReportProgress(value);
				}
			}
		}

		public long Length { get { return _dataReader.Length; } }

		public DataReaderState State
		{
			get { return _progressReporter.CancellationPending ? DataReaderState.Cancelled : _dataReader.State; }
		}
		#endregion Properties


		/// <summary>
		/// Creates a new progress data reader.
		/// </summary>
		/// <param name="dataReader">the underlying data reader</param>
		/// <param name="progressReporter">the progress reporter</param>
		public ProgressDataReader(IDataReader dataReader, IProgressReporter progressReporter)
		{
			PreConditions.Argument("dataReader").Value(dataReader).IsNotNull();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			_dataReader = dataReader;
			_progressReporter = progressReporter;
			_nextProgressPosition = 0L;

			ReportProgress(0L);
		}

		public void Dispose()
		{
			_dataReader.Dispose();
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			if (_progressReporter.CancellationPending) return 0;

			return _dataReader.Read(array, arrayOffset, count);
		}

		/// <summary>
		/// Reports progress of a new <paramref name="position"/>.
		/// </summary>
		/// <param name="position">the new position</param>
		private void ReportProgress(long position)
		{
			long length = this.Length;
			int percentProgress = (int)((100L * position) / length);

			_progressReporter.ReportProgress(percentProgress);
			_nextProgressPosition = Math.Max(position + 1, ((percentProgress + 1) * length) / 100L);
		}
	}
}
