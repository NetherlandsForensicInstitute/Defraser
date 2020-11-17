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
	/// Acomponent detect atom specifies a QuickTime component, such as a
	/// particular video decompressor, required to play the movie. The
	/// component type, subtype, and other required attributes can be
	/// specified, as well as a minimum version.
	/// </summary>
	/// <remarks>Type = 'rmcd'</remarks>
	internal class ComponentDetect : QtAtom
	{
		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer that is currently always 0.
			/// </summary>
			Flags,
			/// <summary>
			/// A component description record.
			/// </summary>
			//ComponentDescription,
			/// <summary>
			/// A four-character code that identifies the type of component.
			/// </summary>
			ComponentType,
			/// <summary>
			/// A four-character code that identifies the subtype of the
			/// component. For example, the subtype of an image compressor
			/// component indicates the compression algorithm employed by the
			/// compressor. A value of 0 matches any subtype.
			/// </summary>
			ComponentSubType,
			/// <summary>
			/// A four-character code that identifies the manufacturer of the
			/// component. Components provided by Apple have a manufacturer
			/// value of 'appl'. A value of 0 matches any manufacturer.
			/// </summary>
			ComponentManufacturer,
			/// <summary>
			/// A 32-bit field that contains flags describing required
			/// component capabilities. The high-order 8 bits should be set to
			/// 0. The low-order 24 bits are specific to each component type.
			/// These flags can be used to indicate the presence of features or
			/// capabilities in a given component.
			/// </summary>
			ComponentFlags,
			/// <summary>
			/// A 32-bit field that indicates which flags in the componentFlags
			/// field are relevant to this operation. For each flag in the
			/// componentFlags field that is to be considered as a search
			/// criterion, set the corresponding bit in this field to 1. To
			/// ignore a flag, set the bit to 0.
			/// </summary>
			ComponentFlagsMask,
			/// <summary>
			/// An unsigned 32-bit integer containing the minimum required
			/// version of the specified component.
			/// </summary>
			MinimumVersion,
		}

		public ComponentDetect(QtAtom previousHeader)
			: base(previousHeader, AtomName.ComponentDetect)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			uint flags = parser.GetUInt(Attribute.Flags);
			parser.CheckAttribute(Attribute.Flags, flags == 0);
			parser.GetFourCC(Attribute.ComponentType);
			parser.GetFourCC(Attribute.ComponentSubType);
			parser.GetFourCC(Attribute.ComponentManufacturer);
			uint componentFlags = parser.GetUInt(Attribute.ComponentFlags);
			parser.CheckAttribute(Attribute.ComponentFlags, (componentFlags & 0xFF000000) == 0);
			parser.GetUInt(Attribute.ComponentFlagsMask);
			parser.GetUInt(Attribute.MinimumVersion);

			return this.Valid;
		}
	}
}
