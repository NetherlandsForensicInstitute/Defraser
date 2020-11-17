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
using System.Collections.Generic;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Detector.Mpeg2.Video.State;

namespace Defraser.Detector.Mpeg2
{
	internal sealed class PictureState : IPictureState
	{
		private readonly ICollection<ExtensionId> _extensions = new List<ExtensionId>();

		#region Properties
		public bool Initialized { get { return CodingType != null; } }
		public PictureCodingType? CodingType { get; set; }
		public PictureStructure Structure { get; set; }
		public byte ForwardHorizontalFCode { get; set; }
		public byte ForwardVerticalFCode { get; set; }
		public byte BackwardHorizontalFCode { get; set; }
		public byte BackwardVerticalFCode { get; set; }
		public byte SpatialTemporalWeightCodeTableIndex { get; set; }
		public bool SpatialScalability { get; set; }
		public bool TopFieldFirst { get; set; }
		public bool RepeatFirstField { get; set; }
		public bool FramePredFrameDct { get; set; }
		public bool ConcealmentMotionVectors { get; set; }
		public bool IntraVlcFormat { get; set; }
		#endregion Properties

		public PictureState()
		{
			Reset();
		}

		public void Reset()
		{
			CodingType = null;
			Structure = PictureStructure.FramePicture;
			ForwardHorizontalFCode = 0;
			ForwardVerticalFCode = 0;
			BackwardHorizontalFCode = 0;
			BackwardVerticalFCode = 0;
			SpatialScalability = false;
			SpatialTemporalWeightCodeTableIndex = 0; // Note: Does not transmit additional bits!
			TopFieldFirst = false;
			RepeatFirstField = false;
			FramePredFrameDct = true;
			ConcealmentMotionVectors = false;
			IntraVlcFormat = false;

			_extensions.Clear();
		}

		public void AddExtension(ExtensionId extensionId)
		{
			if (!_extensions.Contains(extensionId))
				_extensions.Add(extensionId);
		}

		public bool HasExtension(ExtensionId extensionId)
		{
			return _extensions.Contains(extensionId);
		}
	}
}
