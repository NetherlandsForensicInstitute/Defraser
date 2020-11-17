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
using Defraser.Util;

namespace Defraser.Interface
{
	/// <summary>
	/// The interface for scanning a block of data for video data blocks.
	/// </summary>
	/// <remarks>
	/// Any data detected or discarded by <see cref="Scan(IDataReader)"/> is reported
	/// through the corresponding event.
	/// These events are produced in order of increasing offsets.
	/// </remarks>
	public interface IDataScanner
	{
		#region Events
		/// <summary>Occurs when a data block is detected.</summary>
		event EventHandler<DataBlockDetectedEventArgs> DataBlockDetected;

		/// <summary>Occurs when an overlapping data block is discared.</summary>
		event EventHandler<SingleValueEventArgs<IDataBlock>> DataBlockDiscarded;

		/// <summary>
		/// Occurs when unknown data is detected.
		/// Unknown means that none of the current <see cref="Detectors"/> has been
		/// able to detect a data block.
		/// </summary>
		event EventHandler<UnknownDataDetectedEventArgs> UnknownDataDetected;
		#endregion Events

		#region Properties
		/// <summary>The detectors to scan with.</summary>
		IEnumerable<IDetector> Detectors { get; set; }
		/// <summary>Indicates whether overlapping data blocks are allowed.</summary>
		bool AllowOverlap { get; set; }
		#endregion Properties

		/// <summary>
		/// Scans the given <paramref name="dataReader"/> for data blocks.
		/// </summary>
		/// <remarks>
		/// Detected data blocks and unknown data are reported as a contiguous stream
		/// through the corresponding events.
		/// </remarks>
		/// <param name="dataReader">The <see cref="IDataReader"/> that provides the data to scan</param>
		/// <param name="progressReporter">
		/// The <see cref="IProgressReporter"/> for reporting progress of the scan proces
		/// and testing for cancellation
		/// </param>
		void Scan(IDataReader dataReader, IProgressReporter progressReporter);
	}
}
