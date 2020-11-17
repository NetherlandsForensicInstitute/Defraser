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

namespace Defraser.Detector.QT
{
	/// <summary>
	/// The base media info atom, contained in the base media information atom, defines the media’s control
	/// information, including graphics mode and balance information.
	/// </summary>
	/// <remarks>
	/// Type: 'gmin'
	/// Container: 'minf'
	/// </remarks>
	internal class BaseMediaInfo : QtAtom
	{
		public new enum Attribute
		{
			/// <summary>
			/// A 16-bit integer that specifies the transfer mode.
			/// </summary>
			GraphicsMode,
			/// <summary>
			/// Three 16-bit values that specify the red, green, and blue colors for the transfer mode operation
			/// indicated in the graphics mode field.
			/// </summary>
			Opcolor,
			/// <summary>
			/// Balance values are represented as 16-bit, fixed-point numbers that range from -1.0 to +1.0. The
			/// high-order 8 bits contain the integer portion of the value; the low-order 8 bits contain the fractional
			/// part. Negative values weight the balance toward the left speaker; positive values emphasize the right
			/// channel. Setting the balance to 0 corresponds to a neutral setting.
			/// </summary>
			Balance,
			/// <summary>
			/// Reserved for use by Apple. Set this field to 0.
			/// </summary>
			Reserved,
		}

		public BaseMediaInfo(QtAtom previousHeader)
			: base(previousHeader, AtomName.BaseMediaInfo)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetShort(Attribute.GraphicsMode);
			parser.GetOpcolor(Attribute.Opcolor);
			parser.GetFixed8_8(Attribute.Balance);
			parser.GetUShort(Attribute.Reserved);

			return this.Valid;
		}
	}
}
