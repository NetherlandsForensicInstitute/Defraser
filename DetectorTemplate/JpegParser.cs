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
using Defraser;
using DetectorCommon;

namespace DetectorTemplate
{
	internal class JpegParser : Parser<JpegHeader, JpegHeaderName, JpegParser, ByteStreamDataReader>
	{
		public JpegParser(IDataReader dataReader)
			: base(new ByteStreamDataReader(dataReader))
		{
		}

		/// <summary>
		/// Finds and shows the next marker.
		/// The stream pointer is positioned <em>before</em> the next marker,
		/// so that <code>GetUShort()</code> will return the marker again.
		/// </summary>
		/// <returns>the marker</returns>
		public ushort NextMarker()
		{
			ushort marker = (ushort)DataReader.GetShort();

			while (marker <= 0xFF00 && DataReader.State == DataReaderState.Ready)
			{
				marker = (ushort)((marker << 8) | DataReader.GetByte());
			}
			if (DataReader.State != DataReaderState.Cancelled && marker > 0xFF00)
			{
				DataReader.Position -= 2;
			}
			else
			{
				marker = 0;
			}
			return marker;
		}

		public byte GetByte<T>(T id)
		{
			byte value = DataReader.GetByte();
			AddAttribute(new FormattedAttribute<T, byte>(id, value));
			return value;
		}

		public ushort GetUShort<T>(T id)
		{
			ushort value = (ushort)DataReader.GetShort();
			AddAttribute(new FormattedAttribute<T, ushort>(id, value));
			return value;
		}

		public ushort GetUShort<T>(T id, string format)
		{
			ushort value = (ushort)DataReader.GetShort();
			AddAttribute(new FormattedAttribute<T, ushort>(id, value, format));
			return value;
		}

		public uint GetFourCC<T>(T id)
		{
			uint value = (uint)DataReader.GetInt();
			AddAttribute(new FourCCAttribute<T>(id, (uint)value));
			return value;
		}
	}
}
