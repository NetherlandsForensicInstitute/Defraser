/*
 * Copyright (c) 2008-2020, Netherlands Forensic Institute
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
using System.Linq;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class PictureHeader : IVideoHeaderParser
	{
		internal enum Attribute
		{
			TemporalReference,
			PictureCodingType,
			VbvDelay,
			FullPelForwardVector,
			ForwardFCode,
			FullPelBackwardVector,
			BackwardFCode
		}

		internal const string Name = "PictureHeader";
		internal const uint PictureStartCode = 0x100;

		private readonly EnumResultFormatter<PictureCodingType> _pictureCodingTypeResultFormatter;

		#region Properties
		public uint StartCode { get { return PictureStartCode; } }
		#endregion Properties

		public PictureHeader(EnumResultFormatter<PictureCodingType> pictureCodingTypeResultFormatter)
		{
			_pictureCodingTypeResultFormatter = pictureCodingTypeResultFormatter;
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			IPictureState pictureState = reader.State.Picture;

			resultState.Name = Name;

			// It can occur after the last slice of the previous picture or after a sequence header
			// or group of pictures header, possible followed by user data and/or extensions.
			if ((reader.State.LastHeaderName != Slice.Name) && pictureState.Initialized)
			{
				resultState.Invalidate();
				return;
			}
			if (!reader.State.Sequence.Initialized && !reader.State.SeenGop)
			{
				reader.InsertReferenceHeaderBeforeStartCode();
			}

			pictureState.Reset();
			reader.State.Slice.Reset();

			reader.GetBits(10, Attribute.TemporalReference);
			var pictureCodingType = (PictureCodingType)reader.GetBits(3, Attribute.PictureCodingType, _pictureCodingTypeResultFormatter);
			pictureState.CodingType = pictureCodingType;
			reader.GetBits(16, Attribute.VbvDelay);

			if ((pictureCodingType == PictureCodingType.PType) || (pictureCodingType == PictureCodingType.BType))
			{
				reader.GetFlag(Attribute.FullPelForwardVector);

				byte forwardFCode = (byte)reader.GetBits(3, Attribute.ForwardFCode);
				pictureState.ForwardHorizontalFCode = forwardFCode;
				pictureState.ForwardVerticalFCode = forwardFCode;
			}
			if (pictureCodingType == PictureCodingType.BType)
			{
				reader.GetFlag(Attribute.FullPelBackwardVector);

				byte backwardFCode = (byte)reader.GetBits(3, Attribute.BackwardFCode);
				pictureState.BackwardHorizontalFCode = backwardFCode;
				pictureState.BackwardVerticalFCode = backwardFCode;
			}

			int count = 0;	// Sanity check
			uint maxExtraInformationLength = reader.State.Configuration.PictureHeaderMaxLengthOfExtraInformation;
			while (reader.ShowBits(1) == 1)
			{
				reader.GetBits(1);		// ExtraBitPicture
				reader.GetBits(8);		// ExtraInformationPicture

				if (count++ > maxExtraInformationLength)
				{
					resultState.Invalidate();
					return;
				}
			}
		}
	}
}
