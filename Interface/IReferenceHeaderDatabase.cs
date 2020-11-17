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

namespace Defraser.Interface
{
	/// <summary>
	/// The database that maintains the reference headers for each detector.
	/// </summary>
	public interface IReferenceHeaderDatabase
	{
		/// <summary>
		/// Lists the headers that satisfy the given <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">The reference headers to return</param>
		/// <returns>The reference headers that satisfy <paramref name="predicate"/></returns>
		IEnumerable<IReferenceHeader> ListHeaders(Func<IReferenceHeader, bool> predicate);

		/// <summary>
		/// Adds a new reference header to this database.by scanning the file with
		/// given <paramref name="filename"/>.
		/// </summary>
		/// <param name="detector">The detector to add a reference header for</param>
		/// <param name="filename">The name of the file to scan for reference headers</param>
		/// <returns>The header that was added or <code>null</code> if no header was detected</returns>
		IReferenceHeader AddHeader(ICodecDetector detector, string filename);

		/// <summary>
		/// Remove an existing reference header from this database.
		/// </summary>
		/// <remarks>If the header does not exist, this method is silently ignored.</remarks>
		/// <param name="header">The reference header to remove</param>
		void RemoveHeader(IReferenceHeader header);

		/// <summary>
		/// Imports the database from an XML file with the given <paramref name="filename"/>.
		/// </summary>
		/// <param name="filename">The name of the file to import the database from</param>
		void Import(string filename);

		/// <summary>
		/// Exports the database to an XML file with given <paramref name="filename"/>.
		/// </summary>
		/// <param name="filename">The name of the file to export the database to</param>
		void Export(string filename);
	}
}
