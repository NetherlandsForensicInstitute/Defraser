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
using Defraser.Detector.Mpeg2.Video.State;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class SequenceScalableExtension : IExtensionParser
	{
		private enum Attribute
		{
			ScalableMode,
			LayerID,
			LowerLayerPredictionHorizontalSize,
			LowerLayerPredictionVerticalSize,
			HorizontalSubsamplingFactorM,
			HorizontalSubsamplingFactorN,
			VerticalSubsamplingFactorM,
			VerticalSubsamplingFactorN,
			PictureMuxEnable,
			MuxToProgressiveSequence,
			PictureMuxOrder,
			PictureMuxFactor
		}

		internal const string Name = "SequenceScalableExtension";

		private readonly EnumResultFormatter<ScalableMode> _scalableModeResultFormatter;

		#region Properties
		public ExtensionId ExtensionId { get { return ExtensionId.SequenceScalableExtensionId; } }
		#endregion Properties

		public SequenceScalableExtension(EnumResultFormatter<ScalableMode> scalableModeResultFormatter)
		{
			_scalableModeResultFormatter = scalableModeResultFormatter;
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			ScalableMode scalableMode = (ScalableMode)reader.GetBits(2, Attribute.ScalableMode, _scalableModeResultFormatter);
			reader.State.Sequence.ScalableMode = scalableMode;
			reader.GetBits(4, Attribute.LayerID);

			if (scalableMode == ScalableMode.SpatialScalability)
			{
				reader.GetBits(14, Attribute.LowerLayerPredictionHorizontalSize);
				reader.GetMarker();
				reader.GetBits(14, Attribute.LowerLayerPredictionVerticalSize);
				reader.GetBits(5, Attribute.HorizontalSubsamplingFactorM);
				reader.GetBits(5, Attribute.HorizontalSubsamplingFactorN);
				reader.GetBits(5, Attribute.VerticalSubsamplingFactorM);
				reader.GetBits(5, Attribute.VerticalSubsamplingFactorN);
			}
			if (scalableMode == ScalableMode.TemporalScalability)
			{
				if (reader.GetFlag(Attribute.PictureMuxEnable))
				{
					reader.GetFlag(Attribute.MuxToProgressiveSequence);
				}

				reader.GetBits(3, Attribute.PictureMuxOrder);
				reader.GetBits(3, Attribute.PictureMuxFactor);
			}
		}
	}
}
