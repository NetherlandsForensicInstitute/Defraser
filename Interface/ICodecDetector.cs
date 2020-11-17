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
	/// Detector interface for codec formats.
	/// </summary>
	public interface ICodecDetector :  IDetector
	{
		#region Properties
		/// <summary>The reference headers that will be used for scanning.</summary>
		IEnumerable<IReferenceHeader> ReferenceHeaders { get; set; }
		#endregion Properties

		/// <summary>
		/// Returns whether <paramref name="resultNode"/> contains a key frame.
		/// </summary>
		/// <remarks>
		/// If this detector is a container detector or if the result node does
		/// not represent a frame at all, this also returns <code>false</code>.
		/// </remarks>
		/// <param name="resultNode">The result node to test</param>
		/// <returns><code>true</code> if the result node is a key frame, <code>false</code> otherwise</returns>
		bool IsKeyFrame(IResultNode resultNode);

		/// <summary>
		/// Get the header structure/tree for a given packet. Normaly headerPacket is a keyframe.
		/// </summary>
		/// <param name="headerPacket">The header source packet, most of the times equal to the data packet. Only changed when it's set in the 'Default Codec Header' window.</param>
		/// <returns>the data packet containing the video headers</returns>
		IDataPacket GetVideoHeaders(IResultNode headerPacket);

		/// <summary>
		/// Get the display data structure/tree for a given packet. 
		/// This method selects the minimum number of reference packages to make itself visible.
		/// For the most codecs this means it only returns itself.
		/// </summary>
		/// <param name="resultNode">The result node that should be decoded.</param>
		/// <returns>the data packet containing the video data</returns>
		IDataPacket GetVideoData(IResultNode resultNode);

		/// <summary>
		/// Finds the reference header(s) for a given packet.
		/// </summary>
		/// <param name="dataReader">The data that should be scanned for reference headers</param>
		/// <param name="codecParameters">
		/// The strategy for reporting codec parameters of the reference header, such as the width and height of the video
		/// </param>
		/// <returns>The data packet that contains the actual header bytes, or <c>null</c> if none was detected</returns>
		IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec codecParameters);
	}
}
