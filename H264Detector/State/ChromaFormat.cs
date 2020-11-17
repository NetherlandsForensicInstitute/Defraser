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

namespace Defraser.Detector.H264.State
{
	internal sealed class ChromaFormat
	{
		#region Table 6-1 – SubWidthC, and SubHeightC values derived from ChromaFormatIdc and separate_colour_plane_flag
		public static readonly ChromaFormat Monochrome = new ChromaFormat(0, 0);
		public static readonly ChromaFormat YCbCr420 = new ChromaFormat(1, 1);
		public static readonly ChromaFormat YCbCr422 = new ChromaFormat(2, 2);
		public static readonly ChromaFormat YCbCr444 = new ChromaFormat(3, 4);
		public static readonly ChromaFormat SeparateColorPlane = new ChromaFormat(3, 0);
		#endregion Table 6-1 – SubWidthC, and SubHeightC values derived from ChromaFormatIdc and separate_colour_plane_flag

		private readonly byte _chromaFormatIdc;
		private readonly byte _numC8X8;

		#region Properties
		public uint ChromaFormatIdc { get { return _chromaFormatIdc; } }
		public uint NumC8X8 { get { return _numC8X8; } }
		#endregion Properties

		private ChromaFormat(byte chromaFormatIdc, byte numC8X8)
		{
			_chromaFormatIdc = chromaFormatIdc;
			_numC8X8 = numC8X8;
		}
	}
}
