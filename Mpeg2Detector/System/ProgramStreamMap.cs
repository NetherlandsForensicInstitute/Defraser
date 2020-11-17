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

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class ProgramStreamMap : ISystemHeaderParser
	{
		private enum Attribute
		{
			ProgramStreamMapLength,
			CurrentNextIndicator,
			ProgramStreamMapVersion,
			ProgramStreamInfoLength,
			ElementaryStreamMapLength,
			Crc32
		}

		private const string Name = "ProgramStreamMap";

		#region Properties
		public uint StartCode { get { return 0x1bc; } }
		#endregion Properties

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2SystemReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;
			resultState.ParentName = PackHeader.Name; // if no pack headers have been encountered, it ends up in the root.

			int bytesRemaining = (int)reader.GetBits(16, Attribute.ProgramStreamMapLength, n => (n >= 10) && (n <= Math.Min(1018, reader.BytesRemaining)));
			if (!resultState.Valid) return;

			reader.GetBits(1, Attribute.CurrentNextIndicator);
			reader.GetReservedBits(2);
			reader.GetBits(5, Attribute.ProgramStreamMapVersion);
			reader.GetReservedBits(7);
			reader.GetMarker();

			bytesRemaining -= 2;

			// TODO: issue 2282: MPEG-2 system detector does not implement full specification

			int maxProgramStreamInfoLength = (bytesRemaining - 2);
			uint programStreamInfoLength = reader.GetBits(16, Attribute.ProgramStreamInfoLength, n => n <= maxProgramStreamInfoLength);
			if (!resultState.Valid) return;

			bytesRemaining -= 2 + reader.SkipBytes((int)programStreamInfoLength);

			int maxElementaryStreamInfoLength = (bytesRemaining - 2);
			uint elementaryStreamInfoLength = reader.GetBits(16, Attribute.ElementaryStreamMapLength, n => n <= maxElementaryStreamInfoLength);
			if (!resultState.Valid) return;

			bytesRemaining -= 2 + reader.SkipBytes((int)elementaryStreamInfoLength);

			if (bytesRemaining < 4)
			{
				resultState.Invalidate();
				return;
			}

			reader.GetBits(32, Attribute.Crc32);
			reader.SkipBytes(bytesRemaining - 4);
		}
	}
}
