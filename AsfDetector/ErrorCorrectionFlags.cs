﻿/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using Defraser.Detector.Common;

namespace Defraser.Detector.Asf
{
	internal class ErrorCorrectionFlags : CompositeAttribute<DataPacket.LAttribute, string, AsfParser>
	{
		public enum LAttribute
		{
			ErrorCorrectionDataLength,
			OpaqueDataPresent,
			ErrorCorrectionLengthType,
			ErrorCorrectionPresent,
		}

		private byte _errorCorrectionFlags;

		internal byte ErrorCorrectionDataLength { get; private set; }

		public ErrorCorrectionFlags(byte flags)
			: base(DataPacket.LAttribute.PropertyFlags, string.Empty, "{0}")
		{
			_errorCorrectionFlags = flags;
		}

		public override bool Parse(AsfParser parser)
		{
			ErrorCorrectionDataLength = (byte)(_errorCorrectionFlags & 0x0F);
			Attributes.Add(new FormattedAttribute<LAttribute, byte>(LAttribute.ErrorCorrectionDataLength, ErrorCorrectionDataLength));

			bool opaqueDataPresent = ((_errorCorrectionFlags >>= 4) & 1) != 0;
			Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.OpaqueDataPresent, opaqueDataPresent));
			if(opaqueDataPresent) Valid = false;

			LengthType errorCorrectionLengthType = (LengthType)((_errorCorrectionFlags >>= 1) & 3);
			Attributes.Add(new FormattedAttribute<LAttribute, string>(LAttribute.ErrorCorrectionLengthType, errorCorrectionLengthType.PrettyPrint()));
			if(errorCorrectionLengthType != 0) Valid = false;

			bool errorCorrectionPresent = ((_errorCorrectionFlags >> 2) & 1) != 0;
			Attributes.Add(new FormattedAttribute<LAttribute, bool>(LAttribute.ErrorCorrectionPresent, errorCorrectionPresent));

			return Valid;
		}
	}
}
