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

namespace Defraser.Detector.H264.Cabac
{
	internal struct MacroblockState
	{
		#region Predefined macroblock states
		public static readonly MacroblockState UnavailableIntraCoded = new MacroblockState
		{
			Flags = 0x40,
			CodedBlockPattern = 0xf,
			LumaCodedBlockFlags = 0xff,
			CbCodedBlockFlags = 0xff,
			CrCodedBlockFlags = 0xff
		};
		public static readonly MacroblockState UnavailableInterCoded = new MacroblockState
		{
			Flags = 0x40,
			CodedBlockPattern = 0xf,
			LumaCodedBlockFlags = 0,
			CbCodedBlockFlags = 0,
			CrCodedBlockFlags = 0
		};
		public static readonly MacroblockState Pcm = new MacroblockState
		{
			Flags = 0x80,
			CodedBlockPattern = 0x2f,
			LumaCodedBlockFlags = 0xff,
			CbCodedBlockFlags = 0xff,
			CrCodedBlockFlags = 0xff
		};
		#endregion Predefined macroblock states

		private const uint MbUnavailableMask = 0x40;

		#region Properties
		public byte Flags { get; set; }
		public byte CodedBlockPattern { get; set; }
		public byte LumaCodedBlockFlags { get; set; }
		public byte CbCodedBlockFlags { get; set; }
		public byte CrCodedBlockFlags { get; set; }
		public byte RefIdxL0 { get; set; }
		public byte RefIdxL1 { get; set; }

		public bool IsUnavailable { get { return (Flags & MbUnavailableMask) != 0; } }
		#endregion Properties
	}
}
