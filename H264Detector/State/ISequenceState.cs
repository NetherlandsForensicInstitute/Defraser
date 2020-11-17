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
	internal interface ISequenceState
	{
		#region Properties
		uint Id { get; }
		bool ByteStreamFormat { get; }
		Profile Profile { get; }
		ChromaFormat ChromaFormat { get; }
		/// <summary>Bit depth of the samples of the luma array.</summary>
		uint BitDepthLuma { get; }
		/// <summary>Bit depth of the samples of the chroma array.</summary>
		uint BitDepthChroma { get; }
		uint Log2MaxFrameNum { get; }
		uint PictureOrderCountType { get; }
		uint Log2MaxPicOrderCntLsb { get; }
		bool DeltaPicOrderAlwaysZeroFlag { get; }
		uint MaxNumRefFrames { get; }
		uint PicWidthInMbs { get; }
		uint PicHeightInMapUnits { get; }
		bool FrameMbsOnlyFlag { get; }
		bool MbAdaptiveFrameFieldFlag { get; }
		bool Direct8X8InferenceFlag { get; }

		int MinMbQpDelta { get; }
		int MaxMbQpDelta { get; }

		/// <summary>
		/// Returns <code>true</code> if-and-only-if <see cref="ChromaFormat"/>
		/// indicates that the chrominance components are subsampled, i.e. type is
		/// either <code>1</code> (4:2:0) or <code>2</code> (4:2:2).
		/// </summary>
		bool IsChromaSubsampling { get; }
		uint FrameHeightInMbs { get; }
		uint PicSizeInMapUnits { get; }
		#endregion Properties
	}
}
