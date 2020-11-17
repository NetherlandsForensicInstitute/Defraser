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

namespace Defraser.Interface
{
	/// <summary>
	/// The <see cref="IFileExport"/> interface provides several file export strategies
	/// for saving detected results as binary or XML.
	/// </summary>
	public interface IFileExport
	{
		/// <summary>
		/// Saves the <paramref name="inputFile"/> to the file with given
		/// <paramref name="filePath"/>.
		/// </summary>
		/// <param name="inputFile">The <see cref="IInputFile"/> to save</param>
		/// <param name="filePath">The path name of the file to write to</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <param name="progressReporter">For progress reporting and cancellation checking</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		/// <exception cref="IOException">On error writing the output file</exception>
		void SaveAsSingleFile(IInputFile inputFile, string filePath, bool createForensicIntegrityLog, IProgressReporter progressReporter);

		/// <summary>
		/// Saves the <paramref name="dataPackets"/> sequentially to a single file with
		/// given <paramref name="filePath"/>.
		/// </summary>
		/// <param name="dataPackets">The data packets to save</param>
		/// <param name="detectors">The detectors that were used to generate the results</param>
		/// <param name="filePath">The path name of the file to write to</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		/// <exception cref="IOException">On error writing the output file</exception>
		void SaveAsContiguousFile(IEnumerable<IDataPacket> dataPackets, IEnumerable<IDetector> detectors, string filePath, bool createForensicIntegrityLog);
		/// <summary>
		/// Saves the <paramref name="dataPackets"/> sequentially to a single file with
		/// given <paramref name="filePath"/>.
		/// </summary>
		/// <param name="dataPackets">The data packets to save</param>
		/// <param name="detectors">The detectors that were used to generate the results</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <param name="filePath">The path name of the file to write to</param>
		/// <param name="progressReporter">For progress reporting and cancellation checking</param>
		/// <exception cref="ArgumentNullException">If any argument is <c>null</c></exception>
		/// <exception cref="IOException">On error writing the output file</exception>
		void SaveAsContiguousFile(IEnumerable<IDataPacket> dataPackets, IEnumerable<IDetector> detectors, string filePath, bool createForensicIntegrityLog, IProgressReporter progressReporter);

		/// <summary>TODO</summary>
		/// <param name="items">TODO</param>
		/// <param name="directory">TODO</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		void SaveAsSeparateFiles(IEnumerable<object> items, string directory, bool createForensicIntegrityLog);
		/// <summary>TODO</summary>
		/// <param name="items">TODO</param>
		/// <param name="directory">TODO</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <param name="progressReporter">For progress reporting and cancellation checking</param>
		void SaveAsSeparateFiles(IEnumerable<object> items, string directory, bool createForensicIntegrityLog, IProgressReporter progressReporter);

		/// <summary>TODO</summary>
		/// <param name="fragments">The fragments to export</param>
		/// <param name="filePath">The filename to export <paramref name="fragments"/> to</param>
		/// <param name="createForensicIntegrityLog">Create a forensic integrity log file along with the normal output</param>
		/// <param name="progressReporter">For progress reporting and cancellation checking</param>
		void ExportToXml(IEnumerable<IFragment> fragments, string filePath/*, bool createForensicIntegrityLog*/, IProgressReporter progressReporter);
	}
}
