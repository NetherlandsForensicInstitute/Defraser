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
using System.Linq;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.System.State;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class Mpeg2SystemHeaderParser : IResultParser<IMpeg2SystemReader>, IDetectorColumnsInitializer
	{
		internal enum Attribute
		{
			StartCode,
			ZeroByteStuffing,
			Reserved,
			Marker,
			StuffingBytes
		}

		private readonly uint _maxHeaderCount;
		private readonly IDictionary<uint, ISystemHeaderParser> _headerParsers;

		public Mpeg2SystemHeaderParser(IEnumerable<ISystemHeaderParser> systemHeaderParsers, PesPacket pesPacket)
		{
			_maxHeaderCount = (uint)Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.MaxSystemHeaderCount];
			_headerParsers = new Dictionary<uint, ISystemHeaderParser>();

			foreach (ISystemHeaderParser systemHeaderParser in systemHeaderParsers)
			{
				_headerParsers.Add(systemHeaderParser.StartCode, systemHeaderParser);
			}
			for (uint startCode = 0x1bd; startCode <= 0x1fe; startCode++)
			{
				_headerParsers.Add(startCode, pesPacket);
			}
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			// Add default columns to include for all headers
			IDetectorColumnsBuilder defaultDetectorColumnsBuilder = builder.WithDefaultColumns(Enum.GetNames(typeof(Attribute)));

			// Add the columns for all headers and include the default columns (for each header)
			foreach (var headerParser in _headerParsers.Values.Distinct())
			{
				headerParser.AddColumnsTo(defaultDetectorColumnsBuilder);
			}
		}

		public void Parse(IMpeg2SystemReader reader, IResultNodeState resultState)
		{
			IMpeg2SystemState state = reader.State;

			// Check start code and determine header-specific parsing strategy
			state.StartCode = reader.GetBits(32, Attribute.StartCode, "{0:X8}");

			ISystemHeaderParser headerParser;
			if (!resultState.Valid || !_headerParsers.TryGetValue(state.StartCode, out headerParser))
			{
				resultState.Invalidate();
				return;
			}

			// Invoke the header-specific parsing strategy
			headerParser.Parse(reader, resultState);

			if (!reader.Valid) return;

			// Handle stuffing (leading 00's before the next start code)
			uint zeroByteStuffing = reader.GetZeroByteStuffing(Attribute.ZeroByteStuffing);

			if (IsFragmentBreakPoint(state))
			{
				reader.BreakFragment();
				return;
			}

			string headerName = resultState.Name as string;

			// Record the header and (possible) the extension
			if (state.LastHeaderName == null) state.FirstHeaderName = headerName;
			state.LastHeaderName = headerName;
			state.LastHeaderZeroByteStuffing = zeroByteStuffing;
			state.ParsedHeaderCount++;
		}

		private bool IsFragmentBreakPoint(IMpeg2SystemState state)
		{
			return (state.ParsedHeaderCount > _maxHeaderCount);
		}
	}
}
