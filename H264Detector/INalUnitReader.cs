/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal interface INalUnitReader : IReader
	{
		#region Properties
		IResultState Result { get; set; }
		IH264State State { get; }
		long Position { get; set; }
		long Length { get; }
		#endregion Properties

		uint ShowBits(int numBits);
		bool GetBit();
		uint GetBits(int numbits);
		byte GetByte(bool align);
		uint GetExpGolombCoded();
		uint GetExpGolombCoded(uint maxCodeNum);
		int GetSignedExpGolombCoded();
		int GetSignedExpGolombCoded(int minValue, int maxValue);
		uint GetTruncatedExpGolombCoded(uint maxCodeNum);
		T GetVlc<T>(VlcTable<T> vlcTable);
		int GetLeadingZeroBits();
		void ReadZeroAlignmentBits();
		void GetCabacAlignmentOneBits();

		bool HasMoreRbspData();
		bool IsPossibleStopBit();

		bool GetBit<T>(T attributeName);
		uint GetBits<T>(int numBits, T attribute);
		/// <summary>
		/// Prevents a cast exception when casting the read bit to the enumtype
		/// </summary>
		TEnum GetBits<TAttribute, TEnum>(int numBits, TAttribute attribute, EnumResultFormatter<TEnum> enumResultFormatter);
		uint GetFixedBits<T>(int numBits, uint bitCode, T attributeName);
		byte GetByte<T>(bool align, T attributeName, byte maxValue);
		uint GetExpGolombCoded<T>(T attributeName, uint maxCodeNum);
		uint GetExpGolombCoded<T>(T attributeName, IValidityResultFormatter formatter);
		int GetSignedExpGolombCoded<T>(T attributeName, int minValue, int maxValue);
		void GetPcmSamples();

		void InsertReferenceHeaderBeforeNalUnit();
		bool TryDefaultHeaders(IResultNodeState resultState, Action evaluateHeader);
	}
}
