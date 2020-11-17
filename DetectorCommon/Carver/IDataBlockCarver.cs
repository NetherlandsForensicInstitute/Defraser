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

namespace Defraser.Detector.Common.Carver
{
	/// <summary>
	/// Strategy used by detectors for carving and parsing data blocks.
	/// </summary>
	/// <remarks>
	/// Instances of <see cref="IDataBlockCarver"/> are stateful objects that
	/// are generally NOT thread-safe!
	/// Therefore, each invocation of <see cref="IDetector.DetectData"/> should
	/// create a new instance of <see cref="IDataBlockCarver"/>, suitable for
	/// the type of format(s) supported by that particular detector.
	/// </remarks>
	public interface IDataBlockCarver
	{
		/// <summary>
		/// Carves for the start of a valid block of data, i.e. find the first
		/// valid header or startcode in the underlying stream, and advances
		/// the <see cref="IDataReader.Position">Position</see> of the data reader 
		/// </summary>
		/// <param name="offsetLimit">the maximum position of the header to find</param>
		/// <returns>true if-and-only-if a header was found</returns>
		bool Carve(long offsetLimit);

		/// <summary>
		/// Parses the header located at the current <see cref="IDataReader.Position"/>
		/// and reports the metadata to the <paramref name="readerState"/> object.
		/// </summary>
		/// <remarks>
		/// Only useful after a call to <see cref="Carve"/>.
		/// In case of a parse failure, the <see cref="IDataReader.Position">Position</see>
		/// is NOT rewinded, but remains at the point where the failure was detected.
		/// </remarks>
		/// <param name="readerState">the reader state for reporting metadata</param>
		void ParseHeader(IReaderState readerState);

		/// <summary>
		/// Validates the currently built result, starting at the last call to <see cref="Carve"/>
		/// and extending upto the last successful <see cref="ParseHeader"/> call.
		/// The <paramref name="dataBlockBuilder"/> passed to this method is initialized,
		/// including the <see cref="IDataBlockBuilder.StartOffset">StartOffset</see>
		/// and <see cref="IDataBlockBuilder.EndOffset">EndOffset</see> properties.
		/// The resposibility of this method is to validate the result and adjust properties
		/// of the data block builder when necessary.
		/// </summary>
		/// <remarks>
		/// In order for this method to return a valid data block, it must specify
		/// at least the <see cref="IDataBlockBuilder.DataFormat">DataFormat</see>.
		/// </remarks>
		/// <param name="dataBlockBuilder">the data block builder used for building the result</param>
		/// <param name="startOffset">the start offset of the result</param>
		/// <param name="endOffset">the end offset of the result</param>
		/// <returns><code>true</code> if the built block is valid, <code>null</code> otherwise</returns>
		bool ValidateDataBlock(IDataBlockBuilder dataBlockBuilder, long startOffset, long endOffset);
	}
}
