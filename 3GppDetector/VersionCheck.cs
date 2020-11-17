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
	/// A version check atom specifies a software package, such as QuickTime
	/// or QuickTime VR, and the version of that package needed to display a
	/// movie.
	/// </summary>
	/// <remarks>Type = 'rmvc'</remarks>
	internal class VersionCheck : QtAtom
	{
		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer that is currently always 0.
			/// </summary>
			Flags,
			/// <summary>
			/// A 32-bit Gestalt type, such as 'qtim', specifying the software
			/// package to check for.
			/// </summary>
			SoftwarePackage,
			/// <summary>
			/// An unsigned 32-bit integer containing either the minimum
			/// required version or the required value after a binary AND
			/// operation.
			/// </summary>
			Version,
			/// <summary>
			/// The mask for a binary AND operation on the Gestalt bitfield.
			/// </summary>
			Mask,
			/// <summary>
			/// The type of check to perform, expressed as 16-bit integer. Set
			/// to 0 for a minimum version check, set to 1 for a required value
			/// after a binary AND of the Gestalt bitfield and the mask.
			/// </summary>
			CheckType,
		}

		public VersionCheck(QtAtom previousHeader)
			: base(previousHeader, AtomName.VersionCheck)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			int flags = parser.GetInt(Attribute.Flags);
			parser.CheckAttribute(Attribute.Flags, flags == 0);
			parser.GetFourCC(Attribute.SoftwarePackage);
			parser.GetUInt(Attribute.Version);
			parser.GetUInt(Attribute.Mask);
			parser.GetUShort(Attribute.CheckType);

			return this.Valid;
		}
	}
}
