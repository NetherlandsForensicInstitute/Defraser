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

namespace Defraser.Interface
{
	/// <summary>
	/// States for the data reader.
	/// </summary>
	/// <see cref="IDataReader.State"/>
	public enum DataReaderState
	{
		/// <summary>Ready to read more data.</summary>
		Ready,
		/// <summary>Positioned at the end of the data.</summary>
		EndOfInput,
		/// <summary>Data reader has been cancelled or disposed.</summary>
		Cancelled
	}

	/// <summary>
	/// Provides an abstract view of the input data for detectors.
	/// Data is read in blocks of arbitrary size.
	/// <para>
	/// The data is logically treated as a consecutive stream of bytes,
	/// where the first byte is at <c>Position = 0</c> and the last byte is
	/// is at <c>Position = Length - 1</c>.
	/// </para>
	/// </summary>
	/// <remarks>
	/// For implementations that provide a consecutive view on fragmented
	/// data, the method <see cref="GetDataPacket(long, long)"/> can be
	/// used to determine the mapping to actual input file positions.
	/// </remarks>
	public interface IDataReader : IDisposable
	{
		/// <summary>The current location to read from, in bytes.</summary>
		long Position { get; set; }
		/// <summary>The length of the underlying data in bytes.</summary>
		/// <remarks>Note: this can be a window on the actual data!</remarks>
		long Length { get; }
		/// <summary>The state for reading from this data reader.</summary>
		DataReaderState State { get; }

		/// <summary>
		/// Gets the data (sub) packet corresponding with the given relative
		/// <paramref name="offset"/> and <paramref name="length"/> in this stream.
		/// </summary>
		/// <param name="offset">the relative offset in <em>this</em> stream</param>
		/// <param name="length">the length in bytes</param>
		/// <returns>the data packet</returns>
		/// <see cref="IDataPacket.GetSubPacket(long, long)"/>
		IDataPacket GetDataPacket(long offset, long length);

		/// <summary>
		/// Read bytes from the data reader starting at <c>Position</c>.
		/// </summary>
		/// <remarks>This function does not advance the position!</remarks>
		/// <param name="array">array to store the bytes in</param>
		/// <param name="arrayOffset">offset in the array where the bytes are stored</param>
		/// <param name="count">number of bytes to read</param>
		/// <returns>the actual number of bytes read</returns>
		/// <exception cref="ObjectDisposedException">
		/// Thrown if the data reader has been disposed.
		/// </exception>
		int Read(byte[] array, int arrayOffset, int count);
	}
}
