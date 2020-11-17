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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="SaveAsSingleFile"/> file export strategy saves a single
	/// input file to an output file.
	/// </summary>
	public sealed class SaveAsSingleFile : IExportStrategy<IInputFile>
	{
		private readonly Creator<IDataWriter, string> _createDataWriter;

		/// <summary>
		/// Creates a new <see cref="SaveAsSingleFile"/> strategy.
		/// </summary>
		/// <param name="createDataWriter">The factory method for creating a file data writer</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		public SaveAsSingleFile(Creator<IDataWriter, string> createDataWriter)
		{
			PreConditions.Argument("createDataWriter").Value(createDataWriter).IsNotNull();

			_createDataWriter = createDataWriter;
		}

		/// <summary>
		/// Saves the <paramref name="inputFile"/> to the file with given
		/// <paramref name="filePath"/>.
		/// </summary>
		/// <param name="inputFile">The <see cref="IInputFile"/> to save</param>
		/// <param name="detectors">The detectors (empty/ignored)</param>
		/// <param name="dataReaderPool">Ignored</param>
		/// <param name="filePath">The path name of the file to write to</param>
		/// <param name="progressReporter">For reporting progress and checking cancellation</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		/// <exception cref="IOException">On error writing the output file</exception>
		public void Save(IInputFile inputFile, IEnumerable<IDetector> detectors, IDataReaderPool dataReaderPool, string filePath, IProgressReporter progressReporter, bool createForensicIntegrityLog)
		{
			PreConditions.Argument("inputFile").Value(inputFile).IsNotNull();
			PreConditions.Argument("detectors").Value(detectors).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			if (progressReporter.CancellationPending) return;

			using (IDataWriter dataWriter = _createDataWriter(filePath))
			using (IDataReader dataReader = dataReaderPool.CreateDataReader(inputFile.CreateDataPacket(), progressReporter))
			{
				dataWriter.Write(dataReader);
			}
		}
	}
}
