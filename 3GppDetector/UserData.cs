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
using Defraser;
using DetectorCommon;

namespace QtDetector
{
	internal abstract class UserDataListEntry : QtAtom
	{
		public new enum Attribute { Data }

		public UserDataListEntry(QtAtom previousHeader, AtomName atomName)
			: base(previousHeader, atomName) {}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetPascalString(Attribute.Data);

			return this.Valid;
		}
	}

	/// <summary>Name of arranger</summary>
	/// <remarks>Type = '©arg'</remarks>
	internal class NameOfArranger : UserDataListEntry
	{
		public NameOfArranger(QtAtom previousHeader)
			: base(previousHeader, AtomName.NameOfArranger) {}
	}

	/// <summary>Keyword(s) for arranger X</summary>
	/// <remarks>'©ark'</remarks>
	internal class KeywordsForArrangerX : UserDataListEntry
	{
		public KeywordsForArrangerX(QtAtom previousHeader)
			: base(previousHeader, AtomName.KeywordsForArrangerX) { }
	}

	/// <summary>Keyword(s) for composer X</summary>
	/// <remarks>'©cok'</remarks>
	internal class KeywordsForComposerX : UserDataListEntry
	{
		public KeywordsForComposerX(QtAtom previousHeader)
			: base(previousHeader, AtomName.KeywordsForComposerX) { }
	}

	/// <summary>Name of composer</summary>
	/// <remarks>'©com'</remarks>
	internal class NameOfComposer : UserDataListEntry
	{
		public NameOfComposer(QtAtom previousHeader)
			: base(previousHeader, AtomName.NameOfComposer) { }
	}

	/// <summary>Copyright statement</summary>
	/// <remarks>'©cpy'</remarks>
	internal class  CopyrightStatement : UserDataListEntry
	{
		public CopyrightStatement(QtAtom previousHeader)
			: base(previousHeader, AtomName.CopyrightStatement) { }
	}

#if  null
	/// <summary>
	/// Date the movie content was created
	/// </summary>
	/// <remarks>Type = '©day'</remarks>
	internal class cday : QtAtom
	{
	}
	/// <summary>
	/// Name of movie’s director
	/// </summary>
	/// <remarks>Type = '©dir'</remarks>
	internal class cdir : QtAtom
	{
	}
#endif

	/*
	/// <summary>
	/// User data atoms allow you to define and store data associated with a
	/// QuickTime object, such as a movie, track, or media.
	/// </summary>
	internal class UserData : QtAtom
	{
		#region Inner classes
		private class UserDataList : CompositeAttribute<Attribute, string, QtParser>
		{
			public enum LAttribute
			{
				Size,
				Type,
				Data,
			}

			public UserDataList()
				: base(Attribute.UserDataList, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				//parser.GetUInt(LAttribute.Size);
				//parser.GetFourCC(LAttribute.Type);
				//parser.GetString(LAttribute.Data);

				return this.Valid;
			}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			UserDataList,
		}

		public UserData(QtAtom previousHeader)
			: base(previousHeader, AtomName.UserData)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.Parse(new UserDataList());

			return this.Valid;
		}
	}*/
}
