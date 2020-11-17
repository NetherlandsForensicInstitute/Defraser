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
	/// Color table atoms define a list of preferred colors for displaying the
	/// movie on devices that support only 256 colors. The list may contain up
	/// to 256 colors.
	/// </summary>
	/// <remarks>Type = 'ctab'</remarks>
	internal class ColorTable : QtAtom
	{
		#region Inner classes
		private class Color : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				Zero,
				Red,
				Green,
				Blue,
			}

			public Color()
				: base(Attribute.Color, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				ushort zero = parser.GetUShort(LAttribute.Zero);
				parser.CheckAttribute(LAttribute.Zero, zero == 0);
				ushort red = parser.GetUShort(LAttribute.Red);
				ushort green = parser.GetUShort(LAttribute.Green);
				ushort blue = parser.GetUShort(LAttribute.Blue);

				TypedValue = string.Format("({0}, {1}, {2}, {3})", zero, red, green, blue);

				return Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer that must be set to 0.
			/// </summary>
			ColorTableSeed,
			/// <summary>
			/// A 16-bit integer that must be set to 0x8000.
			/// </summary>
			ColorTableFlags,
			/// <summary>
			/// A 16-bit integer that indicates the number of colors in the
			/// following color array. This is a zero-relative value; setting
			/// this field to 0 means that there is one color in the array.
			/// </summary>
			ColorTableSize,
			/// <summary>
			/// An array of colors. Each color is made of four unsigned 16-bit
			/// integers. The first integer must be set to 0, the second is the
			/// red value, the third is the green value, and the fourth is the
			/// blue value.
			/// </summary>
			ColorArray,
			Color,
		}

		public ColorTable(QtAtom previousHeader)
			: base(previousHeader, AtomName.ColorTable)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			int colorTableSeed = parser.GetInt(Attribute.ColorTableSeed);
			parser.CheckAttribute(Attribute.ColorTableSeed, colorTableSeed == 0);
			ushort colorTableFlags = parser.GetUShort(Attribute.ColorTableFlags);
			parser.CheckAttribute(Attribute.ColorTableFlags, colorTableFlags == 0x8000);

			uint colorTableSize = parser.GetUShort(Attribute.ColorTableSize);
			colorTableSize++;	// The color table size is zero-relative; 0 means there is one color in the array
			parser.GetTable(Attribute.ColorArray, Attribute.ColorTableSize, colorTableSize, 8, () => new Color(), parser.BytesRemaining);

			return Valid;
		}
	}
}
