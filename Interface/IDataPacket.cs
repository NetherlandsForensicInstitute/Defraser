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

namespace Defraser.Interface
{
	/// <summary>
	/// The <see cref="IDataPacket"/> interface describes a block of data by
	/// file, offset and length.
	/// </summary>
	/// <remarks>
	/// Instances of <see cref="IDataPacket"/> are immutable.
	/// </remarks>
	public interface IDataPacket
	{
		#region Properties
		/// <summary>The input file (of the first byte of data).</summary>
		IInputFile InputFile { get; }
		/// <summary>The total length of the data packet in bytes.</summary>
		long Length { get; }
		/// <summary>The (minimum) start offset in the input file.</summary>
		long StartOffset { get; }
		/// <summary>The (maximum) end offset in the input file.</summary>
		long EndOffset { get; }
		#endregion Properties

		/// <summary>
		/// Appends <paramref name="dataPacket"/> to the end of this data packet.
		/// </summary>
		/// <remarks>
		/// This creates a new data packet that contains the two data packets.
		/// </remarks>
		/// <param name="dataPacket">the data packet to append</param>
		/// <returns>the concatenated data packet</returns>
		IDataPacket Append(IDataPacket dataPacket);

		/// <summary>
		/// Creates a sub packet of this <see cref="IDataPacket"/>.
		/// </summary>
		/// <param name="offset">the relative offset in this data packet</param>
		/// <param name="length">the length of the sub packet</param>
		/// <returns>the existing or newly created sub packet</returns>
		IDataPacket GetSubPacket(long offset, long length);

		/// <summary>
		/// Gets the fragment (sub packet) starting at <paramref name="offset"/>.
		/// The data packet returned by this method describes a single contiguous
		/// block of data in an input file, i.e. an unfragmented data packet.
		/// </summary>
		/// <param name="offset">the offset in this data packet</param>
		/// <returns>the fragment</returns>
		/// <seealso cref="GetSubPacket(long, long)"/>
		IDataPacket GetFragment(long offset);
	}
}
