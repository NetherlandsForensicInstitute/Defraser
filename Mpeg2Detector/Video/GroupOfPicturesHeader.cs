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
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	// ISO/IEC 13818-2, §6.2.2.6
	internal sealed class GroupOfPicturesHeader : IVideoHeaderParser
	{
		#region Inner classes
		/// <summary>
		/// Describes the time code information.
		/// </summary>
		private sealed class TimeCodeAttribute : IAttributeParser<IMpeg2VideoReader>
		{
			private enum Attribute
			{
				DropFrameFlag,
				TimeCodeHours,
				TimeCodeMinutes,
				TimeCodeSeconds,
				TimeCodePictures
			}

			private readonly IResultFormatter _timeCodeResultFormatter;

			internal TimeCodeAttribute()
			{
				_timeCodeResultFormatter = new TimeCodeResultFormatter();
			}

			public void Parse(IMpeg2VideoReader reader, IAttributeState resultState)
			{
				resultState.Name = GroupOfPicturesHeader.Attribute.TimeCode;
				reader.GetBits(1, Attribute.DropFrameFlag);
				uint hours = reader.GetBits(5, Attribute.TimeCodeHours, h => h <= 23);
				uint minutes = reader.GetBits(6, Attribute.TimeCodeMinutes, m => m <= 59);
				reader.GetMarker();
				uint seconds = reader.GetBits(6, Attribute.TimeCodeSeconds, s => s <= 59);
				uint pictures = reader.GetBits(6, Attribute.TimeCodePictures);
				resultState.Value = 64 * (3600 * hours + 60 * minutes + seconds) + pictures;
				resultState.Formatter = _timeCodeResultFormatter;
			}
		}

		private sealed class TimeCodeResultFormatter : IResultFormatter
		{
			public string Format(object value)
			{
				uint totalSeconds = (uint)value / 64;

				uint hours = totalSeconds / 3600;
				uint minutes = (totalSeconds / 60) % 60;
				uint seconds = totalSeconds % 60;
				uint pictures = (uint)value % 64;

				return string.Format("{0:D2}:{1:D2}:{2:D2}-{3:D2}", hours, minutes, seconds, pictures);
			}
		}
		#endregion Inner classes

		private enum Attribute
		{
			TimeCode,
			ClosedGop,
			BrokenLink
		}

		internal const string Name = "GroupOfPicturesHeader";
		internal const uint GopStartCode = 0x1b8;

		private readonly IAttributeParser<IMpeg2VideoReader> _timeCodeAttribute;

		#region Properties
		public uint StartCode { get { return GopStartCode; } }
		#endregion Properties

		public GroupOfPicturesHeader()
		{
			_timeCodeAttribute = new TimeCodeAttribute();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			IMpeg2VideoState state = reader.State;

			// It can occur after the last slice of a picture or after a sequence header, possible followed by user data and/or extensions
			if ((state.LastHeaderName != Slice.Name) && (state.LastHeaderName != null) && (!state.Sequence.Initialized || state.SeenGop || state.Picture.Initialized))
			{
				resultState.Invalidate();
				return;
			}
			if (!reader.State.Sequence.Initialized)
			{
				reader.InsertReferenceHeaderBeforeStartCode();
			}

			state.SeenGop = true;
			state.Picture.Reset();

			reader.GetAttribute(_timeCodeAttribute);
			reader.GetFlag(Attribute.ClosedGop);
			reader.GetFlag(Attribute.BrokenLink);
		}
	}
}
