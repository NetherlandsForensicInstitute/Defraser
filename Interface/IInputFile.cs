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
	/// Describes the input file used to generate a result.
	/// </summary>
	public interface IInputFile
	{
		#region Properties
		/// <summary>The <see cref="IProject"/> containing this <see cref="IInputFile"/>.</summary>
		IProject Project { get; }
		/// <summary>The full path name of the file.</summary>
		string Name { get; }
		/// <summary>The length of the file in bytes.</summary>
		long Length { get; }
		/// <summary>A checksum of the file</summary>
		string Checksum { get; }
		#endregion Properties

		/// <summary>
		/// Creates an <see cref="IDataPacket"/> that represents this <see cref="IInputFile"/>.
		/// </summary>
		/// <returns>The data packet</returns>
		IDataPacket CreateDataPacket();

		/// <summary>
		/// Creates a new <see cref="IDataReader"/> for reading this <see cref="IInputFile"/>.
		/// </summary>
		/// <returns>The <see cref="IDataReader"/></returns>
		IDataReader CreateDataReader();
	}
}
