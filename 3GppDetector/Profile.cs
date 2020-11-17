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

using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// The parent of the profile atom can be a movie and track atom.
	/// 
	/// The profile atom contains a list of features. In a movie profile atom,
	/// these features summarize the movie as a whole.
	/// In a track profile atom, these features describe a particular track.
	/// </summary>
	/// <remarks>
	/// Type: 'prfl'
	/// container: Movie atom ('moov') or track atom ('trak')
	/// Mandatory: no
	/// Quantity: zero or one
	/// TODO At the movie level, the profile atom must occur within the movie atom before the movie header atom.
	/// TODO A track-level profile atom must occur within the track atom before the track header atom ('tkhd')
	/// </remarks>
	internal class Profile : QtAtom
	{
		#region Inner classes
		private class FeatureEntity : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum Feature : uint
			{
				// TODO QUESTION create separate atoms?
				mvbr = 0x6D766272, // “Maximum Video Bitrate” (page 282) Movie or Video Track
				avvb = 0x61767662, // “Average Video Bitrate” (page 283) Movie or Video Track
				mabr = 0x6D616272, // “Maximum Audio Bitrate” (page 284) Movie or Track
				avab = 0x61766162, // “Average Audio Bitrate” (page 285) Movie or Audio Track
				vfmt = 0x76666D74, // “QuickTime Video Codec Type” (page 286) Movie or Video Track
				afmt = 0x61666D74, // “QuickTime Audio Codec Type” (page 287) Movie or Video Track
				m4vp = 0x6D347670, // “MPEG-4 Video Profile” (page 288) Movie or Video Track
				mp4v = 0x6D703476, // “MPEG-4 Video Codec” (page 289) Movie or Video Track
				m4vo = 0x6D34766F, // “MPEG-4 Video Object Type” (page 290) Movie or Video Track
				mp4a = 0x6D703461, // “MPEG-4 Audio Codec” (page 291) Movie or Audio Track
				mvsz = 0x6D76737A, // “Maximum Video Size in a Movie” (page 292) Movie
				tvsz = 0x7476737A, // “Maximum Video Size in a Track” (page 294) Movie or Video Track
				vfps = 0x76667073, // “MaximumVideo Frame Rate in a Single Track” (page Movie or Video Track 295)
				tafr = 0x74616672, // “Average Video Frame Rate in a Single Track” (page Movie or Video Track 296)
				vvfp = 0x76766670, // “Video Variable Frame Rate Indication” (page 297) Movie or Video Track
				ausr = 0x61757372, // “Audio Sample Rate for a Sample Entry” (page 298) Movie or Audio Track
				avbr = 0x61766272, // “Audio Variable Bitrate Indication” (page 299) Movie or Audio Track
				achc = 0x61636863, // “Audio Channel Count” (page 300) Movie or Audio Track
			}

			public enum LAttribute
			{
				/// <summary>
				/// A 32-bit field that must be set to zero.
				/// </summary>
				Reserved,
				/// <summary>
				/// Either a brand identifier that occurs in the file-type
				/// atom of the same file, indicating a feature that is
				/// specific to this brand, or the value 0x20202020 (four
				/// ASCII spaces) indicating a universal feature that can
				/// be found in any file type that allows the profile atom.
				/// The value 0 is reserved for an empty slot.
				/// </summary>
				PartID,
				/// <summary>
				/// A four-character code either documented here (universal
				/// features), or in the specification identified by the
				/// brand. The value of 0 is reserved for an empty slot
				/// with no meaningful feature-value.
				/// </summary>
				FeatureCode,
				/// <summary>
				/// Either a value from an enumerated set (for example,
				/// 1 or 0 for true or false, or an MPEG-4 profile-level
				/// ID) or a value that can compared (for example, bitrate
				/// as an integer or dimensions as a 32-bit packed structure).
				/// </summary>
				FeatureValue,
				MaximumVideoBitrate,
				AverageVideoBitrate,
				MaximumAudioBitrate,
				AverageAudioBitrate,
				QuickTimeVideoCodecType,
				QuickTimeAudioCodecType,
				MPEG4VideoProfile,
				MPEG4VideoCodec,
				MPEG4VideoObjectType,
				MPEG4AudioCodec,
				MaximumVideoSizeInAMovieWidth,
				MaximumVideoSizeInAMovieHeight,
				MaximumVideoSizeInATrackWidth,
				MaximumVideoSizeInATrackHeight,
				MaximumVideoFrameRateinaSingleTrack,
				AverageVideoFrameRateinaSingleTrack,
				VideoVariableFrameRateIndication,
				AudioSampleRateforaSampleEntry,
				AudioVariableBitrateIndication,
				AudioChannelCount,
			}

			public FeatureEntity()
				: base(Attribute.FeatureEntity, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				uint reserved = parser.GetUInt(LAttribute.Reserved);
				parser.CheckAttribute(LAttribute.Reserved, reserved == 0);
				uint partID = parser.GetUInt(LAttribute.PartID);
				uint featureCode = parser.GetFourCC(LAttribute.FeatureCode);

				switch (featureCode)
				{
					case (uint)Feature.mvbr:
						uint maximumVideoBitrate = parser.GetUInt(LAttribute.MaximumVideoBitrate);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, maximumVideoBitrate);
						break;
					case (uint)Feature.avvb:
						uint averageVideoBitrate = parser.GetUInt(LAttribute.AverageVideoBitrate);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, averageVideoBitrate);
						break;
					case (uint)Feature.mabr:
						uint maximumAudioBitrate = parser.GetUInt(LAttribute.MaximumAudioBitrate);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, maximumAudioBitrate);
						break;
					case (uint)Feature.avab:
						uint averageAudioBitrate = parser.GetUInt(LAttribute.AverageAudioBitrate);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, averageAudioBitrate);
						break;
					case (uint)Feature.vfmt:
						uint quickTimeVideoCodecType = parser.GetFourCC(LAttribute.QuickTimeVideoCodecType);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, quickTimeVideoCodecType.ToString4CC());
						break;
					case (uint)Feature.afmt:
						uint quickTimeAudioCodecType = parser.GetFourCC(LAttribute.QuickTimeAudioCodecType);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, quickTimeAudioCodecType.ToString4CC());
						break;
					case (uint)Feature.m4vp:
						uint mpeg4VideoProfile = parser.GetUInt(LAttribute.MPEG4VideoProfile);
						parser.CheckAttribute(LAttribute.MPEG4VideoProfile, (mpeg4VideoProfile & 0xFFFFFF00) == 0);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, mpeg4VideoProfile);
						break;
					case (uint)Feature.mp4v:
						uint mpeg4VideoCodec = parser.GetUInt(LAttribute.MPEG4VideoCodec);
						parser.CheckAttribute(LAttribute.MPEG4VideoCodec, (mpeg4VideoCodec & 0xFFFFFFF0) == 0);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, mpeg4VideoCodec);
						break;
					case (uint)Feature.m4vo:
						uint mpeg4VideoObjectType = parser.GetUInt(LAttribute.MPEG4VideoObjectType);
						parser.CheckAttribute(LAttribute.MPEG4VideoObjectType, (mpeg4VideoObjectType & 0xFFFFFF00) == 0);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, mpeg4VideoObjectType);
						break;
					case (uint)Feature.mp4a:
						uint mpeg4AudioCodec = parser.GetUInt(LAttribute.MPEG4AudioCodec);
						parser.CheckAttribute(LAttribute.MPEG4AudioCodec, (mpeg4AudioCodec & 0xFFFFFFE0) == 0);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, mpeg4AudioCodec);
						break;
					case (uint)Feature.mvsz:
						ushort movieWidth = parser.GetUShort(LAttribute.MaximumVideoSizeInAMovieWidth);
						ushort movieHeight = parser.GetUShort(LAttribute.MaximumVideoSizeInAMovieHeight);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3}, {4})", reserved, partID, featureCode, movieWidth, movieHeight);
						break;
					case (uint)Feature.tvsz:
						ushort trackWidth = parser.GetUShort(LAttribute.MaximumVideoSizeInATrackWidth);
						ushort trackHeight = parser.GetUShort(LAttribute.MaximumVideoSizeInATrackHeight);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3}, {4})", reserved, partID, featureCode, trackWidth, trackHeight);
						break;
					case (uint)Feature.vfps:
						double maximumVideoFrameRateinaSingleTrack = parser.GetUInt(LAttribute.MaximumVideoFrameRateinaSingleTrack);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, maximumVideoFrameRateinaSingleTrack);
						break;
					case (uint)Feature.tafr:
						uint averageVideoFrameRateinaSingleTrack = parser.GetUInt(LAttribute.AverageVideoFrameRateinaSingleTrack);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, averageVideoFrameRateinaSingleTrack);
						break;
					case (uint)Feature.vvfp:
						uint videoVariableFrameRateIndication = parser.GetUInt(LAttribute.VideoVariableFrameRateIndication);
						parser.CheckAttribute(LAttribute.VideoVariableFrameRateIndication, videoVariableFrameRateIndication == 0 || videoVariableFrameRateIndication == 1);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, videoVariableFrameRateIndication);
						break;
					case (uint)Feature.ausr:
						uint audioSampleRateforaSampleEntry = parser.GetUInt(LAttribute.AudioSampleRateforaSampleEntry);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, audioSampleRateforaSampleEntry);
						break;
					case (uint)Feature.avbr:
						uint audioVariableBitrateIndication = parser.GetUInt(LAttribute.AudioVariableBitrateIndication);
						parser.CheckAttribute(LAttribute.AudioVariableBitrateIndication, audioVariableBitrateIndication == 0 || audioVariableBitrateIndication == 1);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, audioVariableBitrateIndication);
						break;
					case (uint)Feature.achc:
						uint audioChannelCount = parser.GetUInt(LAttribute.AudioChannelCount);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, audioChannelCount);
						break;
					default:
						uint featureValue = parser.GetUInt(LAttribute.FeatureValue);
						this.TypedValue = string.Format("({0}, {1}, {2}, {3})", reserved, partID, featureCode, featureValue);
						break;
				}
				return this.Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			NumberOfFeatureEntities,
			FeatureEntities,
			FeatureEntity,
		}

		public Profile(QtAtom previousHeader)
			: base(previousHeader, AtomName.Profile)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetTable(Attribute.FeatureEntities, Attribute.NumberOfFeatureEntities, NumberOfEntriesType.UInt, 16, () => new FeatureEntity(), parser.BytesRemaining);

			return Valid;
		}
	}
}
