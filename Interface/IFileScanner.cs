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
	/// The interface for scanning a single file for video data blocks.
	/// 
	/// This implementation first tries the container detectors (if any),
	/// then the codec detectors are tried on any undetected space.
	/// </summary>
	public interface IFileScanner : IDataScanner
	{
		#region Properties
		/// <summary>The container detectors to scan with.</summary>
		IEnumerable<IDetector> ContainerDetectors { get; set; }
		/// <summary>
		/// The codec detectors for scanning container streams and data not detected
		/// by the container detectors.
		/// </summary>
		IEnumerable<IDetector> CodecDetectors { get; set; }
		#endregion Properties

		/// <summary>
		/// Scans the given <paramref name="inputFile"/> for data blocks.
		/// </summary>
		/// <remarks>
		/// Detected data blocks are reported through the <see cref="DataBlockDetected"/>
		/// event in order of increasing offset.
		/// </remarks>
		/// <param name="inputFile">The file to scan</param>
		/// <param name="progressReporter">TODO</param>
		void Scan(IInputFile inputFile, IProgressReporter progressReporter);
	}
}
