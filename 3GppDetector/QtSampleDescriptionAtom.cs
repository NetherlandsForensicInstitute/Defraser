﻿/*
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

using System.Diagnostics;

namespace Defraser.Detector.QT
{
	internal class QtSampleDescriptionAtom : QtAtom
	{
		public new enum Attribute
		{
			Reserved,
			DataReferenceIndex
		}

		public QtSampleDescriptionAtom(QtAtom previousHeader, AtomName atomName)
			: base(previousHeader, atomName)
		{
			Debug.Assert(atomName.IsFlagSet(AtomFlags.SizeAndType) && !atomName.IsFlagSet(AtomFlags.VersionAndFlags));
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetInt(Attribute.Reserved);
			parser.GetShort(Attribute.Reserved);
			parser.GetShort(Attribute.DataReferenceIndex);

			return this.Valid;
		}

		/// <summary>
		/// Locates the handler reference and verifies that the given
		/// <paramref name="componentSubType"/> is correct for this
		/// sample description.
		/// </summary>
		/// <param name="componentSubType">the component sub-type</param>
		/// <returns>true if the component sub-type is valid, false otherwise</returns>
		protected bool CheckComponentSubType(ComponentSubType componentSubType)
		{
			QtAtom media = FindParent(AtomName.Media);
			if (media != null)
			{
				HandlerReference handlerReference = media.FindChild(AtomName.HandlerReference) as HandlerReference;
				if (handlerReference != null && handlerReference.ComponentSubType != componentSubType)
				{
					return false;
				}
			}
			return true;
		}
	}
}
