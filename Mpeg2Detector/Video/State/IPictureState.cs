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

namespace Defraser.Detector.Mpeg2.Video.State
{
	internal enum PictureCodingType
	{
		IType = 1,
		PType = 2,
		BType = 3,
		DType = 4
	}

	internal enum PictureStructure
	{
		Unknown = 0,
		TopField = 1,
		BottomField = 2,
		FramePicture = 3
	}

	internal interface IPictureState
	{
		#region Properties
		bool Initialized { get;}
		PictureCodingType? CodingType { get; set; }
		PictureStructure Structure { get; set; }
		byte ForwardHorizontalFCode { get; set; }
		byte ForwardVerticalFCode { get; set; }
		byte BackwardHorizontalFCode { get; set; }
		byte BackwardVerticalFCode { get; set; }
		bool SpatialScalability { get; set; }
		byte SpatialTemporalWeightCodeTableIndex { get; set; }
		bool TopFieldFirst { get; set; }
		bool RepeatFirstField { get; set; }
		bool FramePredFrameDct { get; set; }
		bool ConcealmentMotionVectors { get; set; }
		bool IntraVlcFormat { get; set; }
		#endregion Properties

		void Reset();
		void AddExtension(ExtensionId extensionId);
		bool HasExtension(ExtensionId extensionId);
	}
}
