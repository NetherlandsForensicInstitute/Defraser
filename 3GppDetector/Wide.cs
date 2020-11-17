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

using Defraser.DataStructures;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Prefix for large atoms to allow resizing the atoms to a 64-bit size field.
	/// </summary>
	internal class Wide : QtAtom
	{
		/// <summary>
		/// Creates a new wide atom.
		/// </summary>
		/// <param name="previousHeader">the header that directly precedes the wide atom</param>
		public Wide(QtAtom previousHeader)
			: base(previousHeader, AtomName.Wide)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.CheckAttribute(Attribute.Size, Size == 8);

			return this.Valid;
		}

		public override bool IsSuitableParent(QtAtom parent)
		{
			return !parent.HasChildren() || parent.LastChild.HeaderName != AtomName.Wide;
		}
	}
}
