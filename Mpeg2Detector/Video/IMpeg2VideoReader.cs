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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	internal interface IMpeg2VideoReader : IReader
	{
		#region Properties
		/// <summary>
		/// Collected data about the videostream (width, height, ...)
		/// </summary>
		IMpeg2VideoState State { get; }
		bool Valid { get; }
		IProject Project { get; }
		IDictionary<IDataPacket, ISequenceState> ReferenceHeaders { set; }
		#endregion Properties

		void FlushBits(int numBits);

		uint GetReservedBits(int numBits);

		uint GetBits(int numBits);

		uint GetBits<T>(int numBits, T attribute);

		uint GetBits<T>(int numBits, T attribute, Func<uint, bool> check);

		uint GetBits<T>(int numBits, T attribute, string format);

		int GetBits<T>(int numBits, T attribute, IValidityResultFormatter optionResultFormatter);

		bool GetFlag<T>(T attribute);

		void GetMarker();

		void GetData<T>(T attribute, int numBytes);

		uint GetZeroByteStuffing<T>(T attribute);

		void AddDerivedAttribute<T>(T attribute, uint value);

		uint ShowBits(int numBits);

		T GetVlc<T>(VlcTable<T> vlcTable);

		uint NextStartCode();

		bool HasBytes(int requiredBytes);

		void GetAttribute(IAttributeParser<IMpeg2VideoReader> parser);

		void BreakFragment();

		void InsertReferenceHeaderBeforeStartCode();

		bool TryDefaultHeaders(IResultState resultState, Action evaluateHeader);
	}
}
