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

using System.Collections.Generic;

namespace Defraser.Interface
{
	/// <summary>
	/// The interface for building instances of <see cref="IDataBlock"/>.
	/// An instance is provided to <see cref="IDetector.DetectData"/> for building
	/// and the data block found by the detector.
	/// </summary>
	public interface IDataBlockBuilder : IBuilder<IDataBlock>
	{
		#region Properties
		/// <summary>The data format.</summary>
		CodecID DataFormat { set; }
		/// <summary>The detector for the data block.</summary>
		IEnumerable<IDetector> Detectors { set; }
		/// <summary>TODO</summary>
		IInputFile InputFile { set; }
		/// <summary>TODO</summary>
		long StartOffset { set; }
		/// <summary>TODO</summary>
		long EndOffset { set; }
		/// <summary>The <em>full-file</em> property.</summary>
		bool IsFullFile { set; }
		/// <summary>TODO</summary>
		bool IsFragmented { set; }
		/// <summary>TODO</summary>
		IFragment PreviousFragment { set; }
		/// <summary>TODO</summary>
		long ReferenceHeaderOffset { set; }
		/// <summary>TODO</summary>
		IDataPacket ReferenceHeader { set; }
		#endregion Properties

		/// <summary>
		/// Adds a codec stream to the data block being built.
		/// </summary>
		/// <returns>an <see cref="ICodecStreamBuilder"/> for building the codec stream</returns>
		ICodecStreamBuilder AddCodecStream();
	}
}
