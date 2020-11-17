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
	/// The interface for building instances of <see cref="ICodecStream"/>.
	/// An instance is provided to <see cref="IDetector.DetectData"/> through the
	/// <see cref="IDataBlockBuilder"/> for reporting the codec streams of the
	/// data block found by the detector.
	/// </summary>
	/// <seealso cref="IDataBlockBuilder"/>
	public interface ICodecStreamBuilder : IBuilder<ICodecStream>
	{
		#region Properties
		/// <summary>The stream number of the codec stream.</summary>
		int StreamNumber { set; }
		/// <summary>The name of the codec stream.</summary>
		string Name { set; }
		/// <summary>The data format.</summary>
		CodecID DataFormat { set; }
		/// <summary>The detector for the codec stream.</summary>
		IDetector Detector { set; }
		/// <summary>The data described by the codec stream.</summary>
		IDataPacket Data { get; set; }
		/// <summary>The parent data block the codec stream belongs to.</summary>
		IDataBlock DataBlock { set; }
		/// <summary>TODO</summary>
		bool IsFragmented { set; }
		/// <summary>TODO</summary>
		IFragment PreviousFragment { set; }
		/// <summary>The absolute start offset in the file</summary>
		/// <remarks>TODO Remove when the StartOffset property correctly returns the absolute start offset</remarks>
		long AbsoluteStartOffset { set; }
		/// <summary>TODO</summary>
		long ReferenceHeaderOffset { set; }
		/// <summary>TODO</summary>
		IDataPacket ReferenceHeader { set; }
		#endregion Properties
	}
}
