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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;

namespace Defraser.Detector.Mpeg2.Video
{
	internal enum ExtensionId
	{
		SequenceExtensionId = 1,
		SequenceDisplayExtensionId = 2,
		QuantMatrixExtensionId = 3,
		CopyrightExtensionId = 4,
		SequenceScalableExtensionId = 5,
		PictureDisplayExtensionId = 7,
		PictureCodingExtensionId = 8,
		PictureSpatialScalableExtensionId = 9,
		PictureTemporalScalableExtensionId = 10,
		CameraParametersExtensionId = 11,
		ItuTExtensionId = 12
	}

	internal sealed class ExtensionParser : IVideoHeaderParser
	{
		private enum Attribute
		{
			ExtensionStartCodeIdentifier
		}

		internal const uint ExtensionStartCode = 0x1b5;

		private static readonly List<string> PictureExtensions = new List<string> {
			PictureCodingExtension.Name,
			CopyrightExtension.Name,
			ItuTExtension.Name,
			PictureCodingExtension.Name,
			PictureDisplayExtension.Name,
			PictureSpatialScalableExtension.Name,
			PictureTemporalScalableExtension.Name,
			QuantMatrixExtension.Name,
		};
		private static readonly List<string> SequenceExtensions = new List<string> {
			SequenceDisplayExtension.Name,
			SequenceExtension.Name,
			SequenceScalableExtension.Name,
		};

		private readonly IDictionary<ExtensionId, IExtensionParser> _extensionParsers;
		private readonly EnumResultFormatter<ExtensionId> _extensionTypeResultFormatter;

		#region Properties
		public uint StartCode { get { return ExtensionStartCode; } }
		#endregion Properties

		public ExtensionParser(IEnumerable<IExtensionParser> extensionParsers, EnumResultFormatter<ExtensionId> extensionTypeResultFormatter)
		{
			_extensionTypeResultFormatter = extensionTypeResultFormatter;
			_extensionParsers = new Dictionary<ExtensionId, IExtensionParser>();

			foreach (IExtensionParser extensionParser in extensionParsers)
			{
				_extensionParsers.Add(extensionParser.ExtensionId, extensionParser);
			}

			// Reserved extension IDs: 0, 6, 13, 14, 15
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			// Add default columns to include for all extensions
			IDetectorColumnsBuilder defaultDetectorColumnsBuilder = builder.WithDefaultColumns(Enum.GetNames(typeof(Attribute)));

			// Add the columns for all extensions and include the default columns (for each extension)
			foreach (var extensionParser in _extensionParsers.Values)
			{
				extensionParser.AddColumnsTo(defaultDetectorColumnsBuilder);
			}
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			if (reader.State.LastHeaderName == null)
			{
				// MPEG data cannot start with an extension!
				resultState.Invalidate();
				return;
			}

			var extensionId = (ExtensionId)reader.GetBits(4, Attribute.ExtensionStartCodeIdentifier, _extensionTypeResultFormatter);

			IExtensionParser extensionParser;
			if (!_extensionParsers.TryGetValue(extensionId, out extensionParser) || !CheckExtensionOccurance(reader.State, extensionId))
			{
				resultState.Invalidate();
				return;
			}

			// Note: Do not invoke the reader state here, it is *NOT* recursive!!
			extensionParser.Parse(reader, resultState);

			if (resultState.Valid)
			{
				if (reader.State.Picture.Initialized)
				{
					reader.State.Picture.AddExtension(extensionId);
				}
				else
				{
					reader.State.Sequence.AddExtension(extensionId);
				}
			}
		}

		private static bool CheckExtensionOccurance(IMpeg2VideoState state, ExtensionId extensionId)
		{
			if (extensionId == ExtensionId.SequenceExtensionId) return true;
			if (extensionId == ExtensionId.PictureCodingExtensionId) return true;

			// Extensions other than 'SequenceExtension' and 'PictureCodingExtension',
			// which are the first extension to follow a sequence or picture header
			// respectively in a valid MPEG-2 stream, can only occur after extension of
			// the same type (sequence or picture). Duplicates are not allowed!
			if (state.Picture.Initialized)
			{
				// No duplicates allowed!
				if (state.Picture.HasExtension(extensionId)) return false;
				// FIXME: 'extensionId' must be a picture extension!!
				// Last header must be a picture extension
				return IsPictureExtension(state.LastHeaderName);
			}
			else
			{
				// No duplicates allowed!
				if (state.Sequence.HasExtension(extensionId)) return false;
				// FIXME: 'extensionId' must be a sequence extension!!
				// Last header must be a sequence extension
				return IsSequenceExtension(state.LastHeaderName);
			}
		}

		private static bool IsSequenceExtension(string name)
		{
			return SequenceExtensions.Contains(name);
		}

		private static bool IsPictureExtension(string name)
		{
			return PictureExtensions.Contains(name);
		}
	}
}
