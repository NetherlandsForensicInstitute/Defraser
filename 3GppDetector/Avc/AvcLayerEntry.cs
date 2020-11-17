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

using Defraser.Interface;

namespace Defraser.Detector.QT
{
	class AvcLayerEntry : VisualSampleGroupEntry
	{
		public new enum Attribute
		{
			// Gives the number of this layer with the base layer being
			// numbered as zero and all enhancement layers being numbered
			// as one or higher with consecutive numbers.
			LayerNumber,			// uint8
			// Indicates how reliable the values of avgBitRate and avgFrameRate
			// are. accurateStatisticsFlag equal to 1 indicates that avgBitRate
			// and avgFrameRate are rounded from statistically correct values.
			// accurateStatisticsFlag equal to 0 indicates that avgBitRate and
			// avgFrameRate are estimates and may deviate somewhat from the
			// correct values.
			AccurateStatisticsFlag,	// uint8
			// Gives the average bit rate in units of 1000 bits per second
			AvgBitRate,				// uint16
			// Gives the average frame rate in units of frames/(256 seconds)
			AvgFrameRate,			// uint16
		}

		public AvcLayerEntry(QtAtom previousHeader)
			: base(previousHeader, AtomName.AvcLayerEntry)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetByte(Attribute.LayerNumber);
			parser.GetByte(Attribute.AccurateStatisticsFlag);
			parser.GetUShort(Attribute.AvgBitRate);
			parser.GetUShort(Attribute.AvgFrameRate);

			return Valid;
		}
	}
}
