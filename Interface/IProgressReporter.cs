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

using System.ComponentModel;

namespace Defraser.Interface
{
	/// <summary>
	/// Reports progress and cancellation state of a background operation.
	/// </summary>
	/// <remarks>
	/// <b>WARNING:</b> Do not try to replace this interface by <see cref="BackgroundWorker"/>.
	/// <see cref="BackgroundWorker"/> only works correct in places where it can
	/// work in the background of the GUI.
	/// This interface is also used in places where the progress is not reported to
	/// the GUI. <see cref="BackgroundWorker"/> fails in that scenario.
	/// </remarks>
	public interface IProgressReporter
	{
		/// <value>
		/// The cancellation state of the background process.
		/// </value>
		bool CancellationPending { get; }

		/// <summary>
		/// Reports progress of the background process.
		/// </summary>
		/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
		void ReportProgress(int percentProgress);

		/// <summary>
		/// Reports progress of the background process.
		/// </summary>
		/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
		/// <param name="userState">The state object passed to RunWorkerAsync.</param>
		void ReportProgress(int percentProgress, object userState);
	}
}
