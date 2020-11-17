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

namespace Defraser.Interface
{
	/// <summary>
	/// TODO
	/// </summary>
	/// <typeparam name="T">The type of item(s) to export</typeparam>
	public interface IExportStrategy<T>
	{
		/// <summary>
		/// Saves the <paramref name="items"/> to the given <paramref name="outputPath"/>.
		/// </summary>
		/// <param name="items">The items to save</param>
		/// <param name="detectors">The detectors used to create the data items</param>
		/// <param name="dataReaderPool">The <see cref="IDataReaderPool"/> for reading the <paramref name="items"/></param>
		/// <param name="outputPath">The path name of the file or directory to write to</param>
		/// <param name="progressReporter">For progress reporting and cancellation checking</param>
		/// <param name="createForensicIntegrityLog">If <code>true</code>, create a forensic integrity log</param>
		void Save(T items, IEnumerable<IDetector> detectors, IDataReaderPool dataReaderPool, string outputPath, IProgressReporter progressReporter, bool createForensicIntegrityLog);
	}
}
