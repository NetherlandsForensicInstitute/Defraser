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

namespace Defraser.Detector.Mpeg2.System.State
{
	interface IMpeg2SystemState
	{
		#region Properties
		IStreamMap Streams { get; }
		/// <summary>
		/// <see cref="CodecID.Mpeg2System"/> if the stream contains MPEG-2 data,
		/// <see cref="CodecID.Mpeg1System"/> if the stream contains MPEG-1 data or
		/// <see cref="CodecID.Unknown"/> if it has not yet been determined whether
		/// the stream contains MPEG-1 or MPEG-2 data.
		/// </summary>
		CodecID MpegFormat { get; set; }
		/// <summary></summary>
		uint StartCode { get; set; }
		/// <summary></summary>
		string FirstHeaderName { get; set; }
		/// <summary></summary>
		string LastHeaderName { get; set; }
		/// <summary>Number of zero stuffing bytes of the last (previous) header.</summary>
		uint LastHeaderZeroByteStuffing { get; set; }
		/// <summary>The last known System Clock Reference timestamp.</summary>
		ulong LastSystemClockReference { get; set; }
		/// <summary>The last timestamp that was successfully parserd.</summary>
		ulong LastTimestamp { get; set; }
		/// <summary>The total number of succesfully PARSED headers, not the number of found startcodes.</summary>
		uint ParsedHeaderCount { get; set; }

		/// <summary></summary>
		uint ProgramMuxRate { get; set; }
		/// <summary></summary>
		bool SeenPackHeader { get; set; }
		/// <summary></summary>
		bool IsFragmented { get; set; }
		#endregion Properties

		void Reset();

		/// <summary>
		/// Indicates whether the stream contains MPEG-2 data.
		/// </summary>
		/// <returns>true if MPEG-2, false if not</returns>
		bool IsMpeg2();
	}
}
