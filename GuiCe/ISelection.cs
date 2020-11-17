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

using Defraser.Interface;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Describes a selection in the FileTree or HeaderTree.
	/// </summary>
	public interface ISelection
	{
		/// <summary>
		/// Returns whether the selection is empty (no results).
		/// </summary>
		bool IsEmpty { get; }
		/// <summary>
		/// The data corresponding to the selection.
		/// </summary>
		IDataPacket DataPacket { get; }
		/// <summary>
		/// Get data corresponding to the selection and report progress
		/// to the progress reporter.
		/// </summary>
		/// <returns>The data corresponding to the selection.</returns>
		IDataPacket GetDataPacket(IProgressReporter progressReporter);
		/// <summary>
		/// The selected results. Null if the selected items are not results.
		/// </summary>
		/// <remarks>
		/// These are the actual results in the tree, <b>not</b> a copy of the results!!
		/// </remarks>
		IResultNode[] Results { get; }
		/// <summary>
		/// The output file extension.
		/// </summary>
		string OutputFileExtension { get; }
		/// <summary>
		/// The item under focus
		/// </summary>
		object FocusItem { get; }
	}
}
