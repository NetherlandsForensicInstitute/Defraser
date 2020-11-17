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
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.System.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	interface IMpeg2SystemReader : IReader
	{
		#region Properties
		/// <summary>
		/// Collected data about the videostream (width, height, ...)
		/// </summary>
		IMpeg2SystemState State { get; }
		bool Valid { get; }
		long BytesRemaining { get; }
		#endregion Properties

		void FlushBits(int numBits);

		uint GetReservedBits(int numBits);

		byte GetByte();

		uint GetBits(int numBits);

		uint GetBits<T>(int numBits, T attribute);

		uint GetBits<T>(int numBits, T attribute, Func<uint, bool> check);

		uint GetBits<T>(int numBits, T attribute, string format);

		bool GetFlag<T>(T attribute);

		void GetMarker();

		void GetData<T>(T attribute, int numBytes);

		uint GetZeroByteStuffing<T>(T attribute);

		uint ShowBits(int numBits);

		uint NextStartCode();

		void GetStuffingBytes(int numBytes);

		int SkipBytes(int bytesToSkip);

		void SetMpegFormat(CodecID format);

		void GetAttribute(IAttributeParser<IMpeg2SystemReader> parser);

		void BreakFragment();
	}
}
