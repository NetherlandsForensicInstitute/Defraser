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
using System.Linq;
using Defraser.DataStructures;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="SaveAsContiguousFile"/> file export strategy saves input files,
	/// data blocks and/or codec streams as a single, contiguous file.
	/// </summary>
	public sealed class SaveAsContiguousFile : IExportStrategy<IDataPacket>
	{
		private readonly Creator<IDataWriter, string> _createDataWriter;
		private readonly IForensicIntegrityLog _forensicIntegrityLog;

		/// <summary>
		/// Creates a new <see cref="SaveAsContiguousFile"/> strategy.
		/// </summary>
		/// <param name="createDataWriter">The factory method for creating a file data writer</param>
		/// <param name="forensicIntegrityLog">The forensic integrity log</param>
		public SaveAsContiguousFile(Creator<IDataWriter, string> createDataWriter,
									IForensicIntegrityLog forensicIntegrityLog)
		{
			PreConditions.Argument("createDataWriter").Value(createDataWriter).IsNotNull();
			PreConditions.Argument("forensicIntegrityLog").Value(forensicIntegrityLog).IsNotNull();

			_createDataWriter = createDataWriter;
			_forensicIntegrityLog = forensicIntegrityLog;
		}

		/// <summary>
		/// Saves the <paramref name="dataPacket"/> sequentially to a single file with
		/// given <paramref name="filePath"/>.
		/// </summary>
		/// <param name="dataPacket">The data packet to save</param>
		/// <param name="detectors">The detectors used to create data packet</param>
		/// <param name="dataReaderPool">The shared pool of file data readers</param>
		/// <param name="filePath">The path name of the file to write to</param>
		/// <param name="progressReporter">For reporting progress and checking cancellation</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		/// <exception cref="IOException">On error writing the output file</exception>
		public void Save(IDataPacket dataPacket, IEnumerable<IDetector> detectors, IDataReaderPool dataReaderPool, string filePath, IProgressReporter progressReporter, bool createForensicIntegrityLog)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();
			PreConditions.Argument("detectors").Value(detectors).IsNotNull().And.DoesNotContainNull();
			PreConditions.Argument("dataReaderPool").Value(dataReaderPool).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			if (progressReporter.CancellationPending) return;

			using (IDataWriter dataWriter = _createDataWriter(filePath))
			{
				using (IDataReader dataReader = dataReaderPool.CreateDataReader(dataPacket, progressReporter))
				{
					dataWriter.Write(dataReader);
				}
			}
			if (createForensicIntegrityLog)
			{
				string logFileName = string.Format("{0}.csv", filePath);
				using (FileStream fs = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					_forensicIntegrityLog.Log(dataPacket, detectors, filePath, fs, ForensicLogType.CopiedData);
				}
			}
		}
	}
}
