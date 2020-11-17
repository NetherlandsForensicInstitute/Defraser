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
	/// <summary>
	/// The implementation of <see cref="ISequenceStateBuilder"/>.
	/// </summary>
	internal sealed class SequenceStateBuilder : ISequenceStateBuilder
	{
		#region Inner classes
		private sealed class SequenceState : ISequenceState
		{
			private static readonly ChromaFormat[] ChromaFormats = new[] { ChromaFormat.Monochrome, ChromaFormat.YCbCr420, ChromaFormat.YCbCr422, ChromaFormat.YCbCr444 };

			#region Properties
			public uint Id { get; private set; }
			public bool ByteStreamFormat { get; private set; }
			public Profile Profile { get; private set; }
			public ChromaFormat ChromaFormat { get; private set; }
			public uint BitDepthLuma { get; private set; }
			public uint BitDepthChroma { get; private set; }
			public uint Log2MaxFrameNum { get; private set; }
			public uint PictureOrderCountType { get; private set; }
			public uint Log2MaxPicOrderCntLsb { get; private set; }
			public bool DeltaPicOrderAlwaysZeroFlag { get; private set; }
			public uint MaxNumRefFrames { get; private set; }
			public uint PicWidthInMbs { get; private set; }
			public uint PicHeightInMapUnits { get; private set; }
			public bool FrameMbsOnlyFlag { get; private set; }
			public bool MbAdaptiveFrameFieldFlag { get; private set; }
			public bool Direct8X8InferenceFlag { get; private set; }

			public int MinMbQpDelta { get; private set; }
			public int MaxMbQpDelta { get; private set; }

			public bool IsChromaSubsampling { get { return (ChromaFormat == ChromaFormat.YCbCr420) || (ChromaFormat == ChromaFormat.YCbCr422); } }
			public uint FrameHeightInMbs { get { return (PicHeightInMapUnits * (FrameMbsOnlyFlag ? 1U : 2U)); } }
			public uint PicSizeInMapUnits { get { return (PicWidthInMbs * PicHeightInMapUnits); } }
			#endregion Properties

			public SequenceState(SequenceStateBuilder builder)
			{
				Id = builder._id;
				ByteStreamFormat = builder.ByteStreamFormat;
				Profile = builder.ProfileIdc;
				ChromaFormat = builder.SeparateColourPlaneFlag ? ChromaFormat.SeparateColorPlane : ChromaFormats[builder.ChromaFormatIdc];
				BitDepthLuma = builder.BitDepthLumaMinus8 + 8;
				BitDepthChroma = builder.BitDepthChromaMinus8 + 8;
				Log2MaxFrameNum = builder.Log2MaxFrameNumMinus4 + 4;
				PictureOrderCountType = builder.PictureOrderCountType;
				Log2MaxPicOrderCntLsb = builder.Log2MaxPicOrderCntLsbMinus4 + 4;
				DeltaPicOrderAlwaysZeroFlag = builder.DeltaPicOrderAlwaysZeroFlag;
				MaxNumRefFrames = builder.MaxNumRefFrames;
				PicWidthInMbs = builder.PicWidthInMbsMinus1 + 1;
				PicHeightInMapUnits = builder.PicHeightInMapUnitsMinus1 + 1;
				FrameMbsOnlyFlag = builder.FrameMbsOnlyFlag;
				MbAdaptiveFrameFieldFlag = builder.MbAdaptiveFrameFieldFlag;
				Direct8X8InferenceFlag = builder.Direct8X8InferenceFlag;

				int qbBdOffsetY = 6 * ((int)BitDepthLuma - 8);
				MinMbQpDelta = -(26 + qbBdOffsetY / 2);
				MaxMbQpDelta = (25 + qbBdOffsetY / 2);
			}
		}
		#endregion Inner classes

		private readonly uint _id;

		#region Properties
		public bool ByteStreamFormat { private get; set; }
		public Profile ProfileIdc { private get; set; }
		public uint ChromaFormatIdc { private get; set; }
		public bool SeparateColourPlaneFlag { private get; set; }
		public uint BitDepthLumaMinus8 { private get; set; }
		public uint BitDepthChromaMinus8 { private get; set; }
		public uint Log2MaxFrameNumMinus4 { private get; set; }
		public uint PictureOrderCountType { private get; set; }
		public uint Log2MaxPicOrderCntLsbMinus4 { private get; set; }
		public bool DeltaPicOrderAlwaysZeroFlag { private get; set; }
		public uint MaxNumRefFrames { private get; set; }
		public uint PicWidthInMbsMinus1 { private get; set; }
		public uint PicHeightInMapUnitsMinus1 { private get; set; }
		public bool FrameMbsOnlyFlag { private get; set; }
		public bool MbAdaptiveFrameFieldFlag { private get; set; }
		public bool Direct8X8InferenceFlag { private get; set; }
		#endregion Properties

		public SequenceStateBuilder(uint id)
		{
			_id = id;

			ChromaFormatIdc = 1;
		}

		public ISequenceState Build()
		{
			return new SequenceState(this);
		}
	}
}
