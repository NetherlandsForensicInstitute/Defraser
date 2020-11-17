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

using System;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;

namespace Defraser.Detector.H264
{
	internal class SupplementalEnhancementInformation : INalUnitPayloadParser
	{
		private enum Attribute
		{
			PayloadType,
			PayloadSize,
		}

		internal const string Name = "SupplementalEnhancementInformation";

		#region Properties
		public NalUnitType UnitType { get { return NalUnitType.SupplementalEnhancementInformation; } }
		#endregion Properties

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(INalUnitReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			uint payloadType = ReadPayloadValue(reader);
			resultState.AddAttribute(Attribute.PayloadType, payloadType);

			uint payloadSize = ReadPayloadValue(reader);
			resultState.AddAttribute(Attribute.PayloadSize, payloadSize);

			//skip payload
			for (int i = 0; i < payloadSize; i++)
			{
				reader.GetByte(true);
			}

			// payload type = (FF)* E5
			// payload size = (FF)* 01
			// payload = 9C
		}

		private static uint ReadPayloadValue(INalUnitReader reader)
		{
			uint value = 0;
			uint b = reader.GetByte(true);
			while (b == 0xff)
			{
				value += 255;
				b = reader.GetByte(true);
			}
			return value + b;
		}
	}
}
