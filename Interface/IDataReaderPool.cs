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
	/// The interface for a shared pool of file data readers.
	/// <para>
	/// The purpose of <see cref="IDataReaderPool"/> is to avoid unnecessary
	/// opening and closing of files within a single action.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Invoking the <see cref="IDisposable.Dispose"/> method disposes all open
	/// data readers. Subsequent invocation of <see cref="ReadInputFile"/> after
	/// disposal will create new data readers.
	/// </remarks>
	public interface IDataReaderPool : IDisposable
	{
		/// <summary>
		/// Reads upto <paramref name="count"/> byte(s) of data at the given
		/// <paramref name="position"/> in the <paramref name="inputFile"/> into
		/// the specified <paramref name="array"/>.
		/// <para>
		/// If this pool does not yet contain a data reader for the given file,
		/// it will be created and stored for future use.
		/// </para>
		/// </summary>
		/// <remarks>
		/// Subsequent calls to this method with the same input file will reuse
		/// the same data reader.
		/// </remarks>
		/// <param name="inputFile">the input file to read from</param>
		/// <param name="position">the location in <paramref name="inputFile"/></param>
		/// <param name="array">array to store the bytes in</param>
		/// <param name="arrayOffset">offset in the array where the bytes are stored</param>
		/// <param name="count">number of bytes to read</param>
		/// <returns>the actual number of bytes read</returns>
		/// <see cref="IDataReader.Read"/>
		int ReadInputFile(IInputFile inputFile, long position, byte[] array, int arrayOffset, int count);

		/// <summary>
		/// Creates a data reader for reading the given <paramref name="dataPacket"/>.
		/// </summary>
		/// <param name="dataPacket">The data packet to read</param>
		/// <returns>The data reader for the given <paramref name="dataPacket"/></returns>
		IDataReader CreateDataReader(IDataPacket dataPacket);

		/// <summary>
		/// Creates a data reader for reading the given <paramref name="dataPacket"/>
		/// and reporting progress to <paramref name="progressReporter"/>.
		/// </summary>
		/// <remarks>
		/// This reports progress of the <c>Position</c> of <see cref="IDataReader"/>
		/// to <paramref name="progressReporter"/>, ranging from <c>0</c> to <c>100</c>
		/// for <c>Position = 0L</c> to <c>Position = dataReader.Length</c> respectively.
		/// </remarks>
		/// <param name="dataPacket">The data packet to read</param>
		/// <param name="progressReporter">The <see cref="IProgressReporter"/> for reporting progress</param>
		/// <returns>The data reader for the given <paramref name="dataPacket"/></returns>
		IDataReader CreateDataReader(IDataPacket dataPacket, IProgressReporter progressReporter);
	}
}
