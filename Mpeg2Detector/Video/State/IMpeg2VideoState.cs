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
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video.State
{
	internal interface IMpeg2VideoState
	{
		#region Properties
		IMpeg2VideoConfiguration Configuration { get; }
		/// <summary>
		/// <see cref="CodecID.Mpeg2Video"/> if the stream contains MPEG-2 extensions,
		/// <see cref="CodecID.Mpeg1Video"/> if the stream contains MPEG-1 data or
		/// <see cref="CodecID.Unknown"/> if it has not yet been determined whether
		/// the stream contains MPEG-1 or MPEG-2 data.
		/// </summary>
		CodecID MpegFormat { get; set; }
		/// <summary>Sequence-level decoding state.</summary>
		ISequenceState Sequence { get; }
		/// <summary>Picture-level decoding state.</summary>
		IPictureState Picture { get; }
		/// <summary>Slice-level decoding state.</summary>
		ISliceState Slice { get; }
		/// <summary>
		/// Indicates whether a <i>Group-of-Pictures</i> header has been encountered
		/// since the last (active) sequence header.
		/// </summary>
		bool SeenGop { get; set; }
		/// <summary>The start code of the header currently being parsed.</summary>
		uint StartCode { get; set; }
		/// <summary>The name of the first header of the block being parsed.</summary>
		string FirstHeaderName { get; set; }
		/// <summary>The name of the last (previous) header of the block being parsed.</summary>
		string LastHeaderName { get; set; }
		/// <summary>Number of zero stuffing bytes of the last (previous) header.</summary>
		uint LastHeaderZeroByteStuffing { get; set; }
		/// <summary>The total number of succesfully PARSED headers, not the number of found startcodes.</summary>
		uint ParsedHeaderCount { get; set; }
		/// <summary>The number of valid (i.e. parsable) slices.</summary>
		uint ValidSliceCount { get; set; }
		/// <summary>The number of invalid (unparsable) slices.</summary>
		uint InvalidSliceCount { get; set; }
		/// <summary></summary>
		bool IsFragmented { get; set; }
		/// <summary></summary>
		long ReferenceHeaderPosition { get; set; }
		/// <summary></summary>
		IDataPacket ReferenceHeader { get; set; }
		/// <summary></summary>
		bool ReferenceHeadersTested { get; set; }
		#endregion Properties

		/// <summary>
		/// Resets the video decoding state.
		/// </summary>
		void Reset();

		/// <summary>
		/// Indicates whether the stream contains MPEG-2 extensions.
		/// </summary>
		/// <returns>true if MPEG-2, false if not</returns>
		bool IsMpeg2();
	}
}
