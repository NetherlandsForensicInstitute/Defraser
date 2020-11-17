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
using Defraser.Detector.Mpeg2.System.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class PackHeader : ISystemHeaderParser
	{
		#region Inner classes
		private sealed class SystemClockReferenceAttribute : IAttributeParser<IMpeg2SystemReader>
		{
			private const ulong FirstSystemClockReference = 0;

			private readonly IAttributeParser<IMpeg2SystemReader> _timeStampAttribute;
			private readonly bool _useSystemClockReferenceValidation;
			private readonly double _maxTimeGapSeconds;

			public SystemClockReferenceAttribute()
			{
				_timeStampAttribute = new TimeStampAttribute<Attribute>(Attribute.SystemClockReference, true);
				_useSystemClockReferenceValidation = (bool)Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.UseSystemClockReferenceValidation];
				_maxTimeGapSeconds = (double)Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.MaxTimeGapSeconds];
			}

			public void Parse(IMpeg2SystemReader reader, IAttributeState resultState)
			{
				_timeStampAttribute.Parse(reader, resultState);

				if (_useSystemClockReferenceValidation)
				{
					ulong systemClockReference = reader.State.LastTimestamp + 1;
					if (!IsContinuousStream(reader.State, systemClockReference))
					{
						resultState.Invalidate();
						return;
					}

					reader.State.LastSystemClockReference = systemClockReference;
				}
			}

			private bool IsContinuousStream(IMpeg2SystemState state, ulong systemClockReference)
			{
				ulong lastSystemClockReference = state.LastSystemClockReference;
				if (lastSystemClockReference == FirstSystemClockReference)
				{
					return true;
				}

				// Compare system clock reference to last system clock reference.
				// It should be higher and have a limited time gap.

				if (systemClockReference < lastSystemClockReference)
				{
					return false; // Indicates a discontinuity because of a backwards jump in time
				}

				ulong timeGap = (systemClockReference - lastSystemClockReference);
				return (timeGap < ((state.IsMpeg2() ? 27E6 : 90E3) * _maxTimeGapSeconds));
			}
		}
		#endregion Inner classes

		private enum Attribute
		{
			Mpeg2Flag,
			SystemClockReference,
			SystemClockReferenceExtension,
			ProgramMuxRate,
			PackStuffingLength,
			StuffingBytes
		}

		internal const string Name = "PackHeader";

		private readonly IAttributeParser<IMpeg2SystemReader> _systemClockReferenceAttribute;

		#region Properties
		public uint StartCode { get { return 0x1ba; } }
		#endregion Properties

		public PackHeader()
		{
			_systemClockReferenceAttribute = new SystemClockReferenceAttribute();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(typeof(PackHeader).Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2SystemReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			if (reader.ShowBits(2) == 1)
			{
				reader.GetBits(2, Attribute.Mpeg2Flag);
				reader.SetMpegFormat(CodecID.Mpeg2System);
			}
			else if (reader.ShowBits(4) == 2)
			{
				reader.GetBits(4, Attribute.Mpeg2Flag);
				reader.SetMpegFormat(CodecID.Mpeg1System);
			}
			else
			{
				// Unknown whether it is MPEG-1 or 2, cannot parse header!
				reader.GetBits(2, Attribute.Mpeg2Flag, mpeg2Flag => false);
				resultState.Invalidate();
				return;
			}

			reader.GetAttribute(_systemClockReferenceAttribute);
			reader.GetMarker();
			reader.State.ProgramMuxRate = reader.GetBits(22, Attribute.ProgramMuxRate, muxRate => muxRate != 0);
			reader.GetMarker();

			if (reader.State.IsMpeg2())
			{
				reader.GetMarker();
				reader.GetReservedBits(5);
				int packStuffingLength = (int)reader.GetBits(3, Attribute.PackStuffingLength);
				if (packStuffingLength > 0)
				{
					reader.GetStuffingBytes(packStuffingLength);
				}
			}
		}
	}
}
