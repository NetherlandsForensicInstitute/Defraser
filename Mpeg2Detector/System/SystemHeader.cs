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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class SystemHeader : ISystemHeaderParser
	{
		#region Inner classes
		/// <summary>
		/// Describes elementary stream information.
		/// </summary>
		private class StreamInfoAttributeParser : IAttributeParser<IMpeg2SystemReader>
		{
			private enum Attribute
			{
				StreamId,
				PStdBufferBoundScale,
				PStdBufferSizeBound
			}

			private readonly IResultFormatter _streamInfoResultFormatter;

			public StreamInfoAttributeParser()
			{
				_streamInfoResultFormatter = new StringResultFormatter("{0:X2}");
			}

			public void Parse(IMpeg2SystemReader reader, IAttributeState resultState)
			{
				resultState.Name = SystemHeader.Attribute.StreamInfo;

				// TODO: use formatter "{0:X2}"
				uint streamId = reader.GetBits(8, Attribute.StreamId, id => id == 0xb8 || id == 0xb9 || id >= 0xbc);
				resultState.Value = streamId;
				resultState.Formatter = _streamInfoResultFormatter;

				reader.GetBits(2, 0x3);
				reader.GetBits(1, Attribute.PStdBufferBoundScale, n => (!IsAudioStream(streamId) || n == 0) && (!IsVideoStream(streamId) || n == 1));
				reader.GetBits(13, Attribute.PStdBufferSizeBound);
			}

			private static bool IsAudioStream(uint streamId)
			{
				return (streamId == 0xB8 || streamId >= 0xC0 && streamId <= 0xDF);
			}

			private static bool IsVideoStream(uint streamId)
			{
				return (streamId == 0xB9 || streamId >= 0xE0 && streamId <= 0xEF);
			}
		}
		#endregion Inner classes

		private enum Attribute
		{
			HeaderLength,
			RateBound,
			AudioBound,
			FixedFlag,
			CspsFlag,
			SystemAudioLockFlag,
			SystemVideoLockFlag,
			VideoBound,
			PacketRateRestrictionFlag,
			StreamInfo
		}

		private const string Name = "SystemHeader";

		private readonly IAttributeParser<IMpeg2SystemReader> _streamInfoAttributeParser;

		#region Properties
		public uint StartCode { get { return 0x1bb; } }
		#endregion Properties

		public SystemHeader()
		{
			_streamInfoAttributeParser = new StreamInfoAttributeParser();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2SystemReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;
			resultState.ParentName = PackHeader.Name;

			// This header should be preceeded by a pack header, but which may have been overwritten
			if ((reader.State.LastHeaderName != PackHeader.Name) && reader.State.SeenPackHeader/*|| SeenPesPacket)*/)
			{
				resultState.Invalidate();
				return;
			}

			reader.GetBits(16, Attribute.HeaderLength);
			reader.GetMarker();

			uint rateBound = reader.GetBits(22, Attribute.RateBound);
			if (reader.State.SeenPackHeader && (rateBound < reader.State.ProgramMuxRate))
			{
				resultState.Invalidate();
				return;
			}

			reader.GetMarker();
			reader.GetBits(6, Attribute.AudioBound, ab => ab <= 32);
			reader.GetFlag(Attribute.FixedFlag);
			reader.GetFlag(Attribute.CspsFlag);
			reader.GetFlag(Attribute.SystemAudioLockFlag);
			reader.GetFlag(Attribute.SystemVideoLockFlag);
			reader.GetMarker();
			reader.GetBits(5, Attribute.VideoBound, vb => vb <= 16);
			reader.GetFlag(Attribute.PacketRateRestrictionFlag);
			reader.GetReservedBits(7);	// 0x7F

			// Stream information (MPEG-2 only)
			while (resultState.Valid && (reader.ShowBits(1) == 1))
			{
				resultState.Parse(_streamInfoAttributeParser, reader);
			}
		}
	}
}
