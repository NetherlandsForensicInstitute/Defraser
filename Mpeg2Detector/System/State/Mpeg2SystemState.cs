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
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System.State
{
	internal sealed class Mpeg2SystemState : IMpeg2SystemState
	{
		private readonly IStreamMap _streamMap;

		#region Properties
		public IStreamMap Streams { get { return _streamMap; } }
		public CodecID MpegFormat { get; set; }
		public uint StartCode { get; set; }
		public string FirstHeaderName { get; set; }
		public string LastHeaderName { get; set; }
		public uint LastHeaderZeroByteStuffing { get; set; }
		public ulong LastSystemClockReference { get; set; }
		public ulong LastTimestamp { get; set; }
		public uint ParsedHeaderCount { get; set; }
		public uint ProgramMuxRate { get; set; }
		public bool SeenPackHeader { get; set; }
		public bool IsFragmented { get; set; }
		#endregion

		public Mpeg2SystemState(IStreamMap streamMap)
		{
			_streamMap = streamMap;

			Reset();
		}

		public void Reset()
		{
			_streamMap.Clear();

			MpegFormat = CodecID.Unknown;
			StartCode = 0;
			FirstHeaderName = null;
			LastHeaderName = null;
			LastHeaderZeroByteStuffing = 0;
			LastSystemClockReference = 0;
			LastTimestamp = 0;
			ParsedHeaderCount = 0;
			ProgramMuxRate = 0;
			SeenPackHeader = false;
			IsFragmented = false;
		}

		public bool IsMpeg2()
		{
			return MpegFormat == CodecID.Mpeg2System; // Note: Returns 'false' if uninitialized (unknown)!
		}
	}
}
