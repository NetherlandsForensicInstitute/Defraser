﻿/*
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
	/// The codec parameters, such as the width and height of the video, that are
	/// defined for a <see cref="IReferenceHeader"/>.
	/// </summary>
	/// <remarks>
	/// Codec parameters are automatically detected from the reference header and
	/// are immutable (read-only).
	/// </remarks>
	public interface ICodecParameters : IEnumerable<string>
	{
		#region Properties
		/// <summary>
		/// The video codec.
		/// </summary>
		CodecID Codec { get; }
		/// <summary>
		/// The video frame width in pixels.
		/// </summary>
		uint Width { get; }
		/// <summary>
		/// The video frame height in pixels.
		/// </summary>
		uint Height { get; }
		/// <summary>
		/// The frame rate in frames per second.
		/// </summary>
		string FrameRate { get; }

		/// <summary>
		/// Returns the value of a custom codec parameter with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the codec parameter to return</param>
		/// <returns>the codec parameter value</returns>
		object this[string name] { get; }
		#endregion Properties
	}
}
