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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;

namespace Defraser.Detector.Mpeg2.Video
{
	internal enum VideoFormat
	{
		Component = 0,
		Pal = 1,
		Ntsc = 2,
		Secam = 3,
		Mac = 4,
		UnspecifiedVideoFormat = 5
	}

	internal sealed class SequenceDisplayExtension : IExtensionParser
	{
		private enum Attribute
		{
			VideoFormat,
			ColourDescription,
			ColourPrimaries,
			TransferCharacteristics,
			MatrixCoefficients,
			DisplayHorizontalSize,
			DisplayVerticalSize
		}

		internal const string Name = "SequenceDisplayExtension";

		private readonly EnumResultFormatter<VideoFormat> _videoFormatResultFormatter;

		#region Properties
		public ExtensionId ExtensionId { get { return ExtensionId.SequenceDisplayExtensionId; } }
		#endregion Properties

		public SequenceDisplayExtension(EnumResultFormatter<VideoFormat> videoFormatResultFormatter)
		{
			_videoFormatResultFormatter = videoFormatResultFormatter;
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;
			reader.GetBits(3, Attribute.VideoFormat, _videoFormatResultFormatter);

			if (reader.GetFlag(Attribute.ColourDescription))
			{
				reader.GetBits(8, Attribute.ColourPrimaries);
				reader.GetBits(8, Attribute.TransferCharacteristics);
				reader.GetBits(8, Attribute.MatrixCoefficients);
			}

			reader.GetBits(14, Attribute.DisplayHorizontalSize);
			reader.GetMarker();
			reader.GetBits(14, Attribute.DisplayVerticalSize);
		}
	}
}
