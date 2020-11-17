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
	/// Detectors are responsible for detecting and describing video data.
	/// The video data is described as a tree of results, returned by
	/// <see cref="DetectData(IDataReader, IDataBlockBuilder)"/>.
	/// Detector plugins usually handle a single video format or a group of
	/// closely related formats, such as MPEG-4 and DivX.
	/// </summary>
	public interface IDetector :  IConfigurable
	{
		#region Properties
		/// <summary>Name of this detector.</summary>
		string Name { get; }
		/// <summary>Description of this detector.</summary>
		string Description { get; }
		/// <summary>The output file extension, starts with a (.) dot.</summary>
		string OutputFileExtension { get; }
		/// <summary>The column names for all possible headers.</summary>
		IDictionary<string, string[]> Columns { get; }
		/// <summary>The data formats supported by this detector.</summary>
		IEnumerable<CodecID> SupportedFormats { get; }
		/// <summary>Return the detector type</summary>
		Type DetectorType { get; }
		#endregion Properties

		/// <summary>
		/// Detects video data reading from <paramref name="dataReader"/>.
		/// The <param name="dataBlockBuilder"/> can be used to exchange information
		/// from and to the detector.
		/// </summary>
		/// <remarks>
		/// If no results are detected, this method returns <c>null</c> and the
		/// <code>Position</code> of the <paramref name="dataReader"/> is set to the
		///	last scanned position.
		/// This position is used to resume scanning by this detector.
		/// </remarks>
		/// <param name="dataReader">the data to read</param>
		/// <param name="dataBlockBuilder">the builder for creating the data block</param>
		/// <param name="context">the context for passing information from/to the detector</param>
		/// <returns>the <see cref="IDataBlock"/> of the detected results, <c>null</c> for none</returns>
		IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context);
	}
}
