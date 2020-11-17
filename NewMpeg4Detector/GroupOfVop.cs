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

using System.Globalization;
using Defraser.Detector.Common;

namespace Defraser.Detector.Mpeg4
{
	internal class GroupOfVop : Mpeg4Header
	{
		public new enum Attribute
		{
			TimeCode,
			Time,
			MarkerBit,
			ClosedGov,
			BrokenLink
		}

		public GroupOfVop(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.GroupOfVop)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			uint hour = parser.GetBits(5);
			uint minute = parser.GetBits(6);

			bool markerBit = parser.GetMarkerBit();
			var markerBitAttribute = new FormattedAttribute<Attribute, bool>(Attribute.MarkerBit, markerBit);
			if(markerBit == false)
			{
				markerBitAttribute.Valid = false;
			}
			Attributes.Add(markerBitAttribute);

			uint seconds = parser.GetBits(6);

			long timeCode = seconds + minute * 60 + hour * 3600;
			Attributes.Add(new FormattedAttribute<Attribute, long>(Attribute.TimeCode, timeCode));
			Attributes.Add(new FormattedAttribute<Attribute, string>(Attribute.Time, string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", hour.ToString("D2", CultureInfo.InvariantCulture), minute.ToString("D2", CultureInfo.InvariantCulture), seconds.ToString("D2", CultureInfo.InvariantCulture))));

			/*bool closedGov = */parser.GetBit(Attribute.ClosedGov);
			/*bool brokenLink = */parser.GetBit(Attribute.BrokenLink);

			return true;
		}
	}
}
