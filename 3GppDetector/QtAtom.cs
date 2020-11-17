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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// The atom type flags.
	/// </summary>
	[Flags]
	internal enum AtomFlags
	{
		/// <summary>The atom contains size and type fields.</summary>
		SizeAndType = 1,
		Box = 1,
		/// <summary>The atom contains version and flags fields.</summary>
		VersionAndFlags = 2,
		/// <summary>The atom is a container atom.</summary>
		Container = 4,
		/// <summary>The atom is mandatory in its suitable parent.</summary>
		Mandatory = 8,
		/// <summary>The atom is allowed more than once in its suitable parent.</summary>
		DuplicatesAllowed = 16,
		/// <summary>The flags for container atoms.</summary>
		ContainerAtom = SizeAndType | Container,
		/// <summary>The flags for full atoms.</summary>
		FullAtom = SizeAndType | VersionAndFlags,
		FullBox = FullAtom,
		//
		UserDataAtom = 1024 | SizeAndType,
		DataReferenceAtom = 4096 | FullAtom,
	}

	/// <summary>
	/// Basic data unit (header) in QuickTime files. Functionally identical to box in MPEG-4.
	/// Header of datablock, referred to as 'atom' in QT-specs.
	/// </summary>
	public class QtAtom : Header<QtAtom, AtomName, QtParser>
	{
		public enum Attribute
		{
			Size,
			Type,
			Version,
			Flags,
			AdditionalData
		}

		private long _unparsedBytes;

		#region Properties

		protected CodecID _dataFormat = CodecID.Unknown;
		public override CodecID DataFormat
		{
			get { return _dataFormat; }
		}
		internal long Size { get; set; }
		internal virtual long RelativeEndOffset { get { return Offset + Size; } }	// For mdat
		internal long UnparsedBytes { get { return _unparsedBytes; } }

		/// <summary>
		/// The atom type or large part of its data is unkown.
		/// </summary>
		internal bool IsUnknown
		{
			get
			{
				// The maximum number of unparsed bytes allowed for trailing atoms.
				uint maxUnparsedBytes = (uint)QtDetector.Configurable[QtDetector.ConfigurationKey.QtAtomMaxUnparsedBytes];

				// Media data atoms are often located at the end of a file
				return HeaderName == AtomName.Unknown ||
					   (UnparsedBytes > maxUnparsedBytes && HeaderName != AtomName.MediaData);
			}
		}
		#endregion Properties

		public QtAtom(IEnumerable<IDetector> detectors)
			: base(detectors, AtomName.Root)
		{
		}

		public QtAtom(QtAtom previousHeader, AtomName atomName)
			: base(previousHeader, atomName)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			if (HeaderName.IsFlagSet(AtomFlags.SizeAndType))
			{
				Size = parser.GetUInt(Attribute.Size);
				uint type = parser.GetFourCC(Attribute.Type);

				if (Size == 1)
				{
					// 64-bit size atom
					Size = (long) parser.GetULong(Attribute.Size);
				}
			}
			if (HeaderName.IsFlagSet(AtomFlags.VersionAndFlags))
			{
				parser.GetByte(Attribute.Version);
				ParseFlags(parser);
			}
			return Valid;
		}

		public override bool ParseEnd(QtParser parser)
		{
			if (parser.ReadOverflow)
			{
				Valid = false;
				return false;
			}

			if (!HeaderName.IsFlagSet(AtomFlags.Container))
			{
				// Atoms extending beyond the end of the file are considered invalid
				if (RelativeEndOffset > parser.Length)
				{
					Valid = false;

					if (HeaderName == AtomName.MediaData)
					{
						// 'mdat' block may be truncated in the initial scan!
						parser.Position = parser.Length;
						return base.ParseEnd(parser);
					}

					return false;
				}

				// Record unparsed bytes statistic
				long endOffset = Math.Min(RelativeEndOffset, parser.Length);
				if (parser.Position < endOffset)
				{
					_unparsedBytes = endOffset - parser.Position;
					parser.GetHexDump(Attribute.AdditionalData, (int)_unparsedBytes);
				}
			}

			if (!Valid || (parser.Position > RelativeEndOffset))
			{
				return false;
			}
			return base.ParseEnd(parser);
		}

		public override bool IsSuitableParent(QtAtom parent)
		{
			if (!parent.IsRoot)
			{
				// Should fit in parent atom (box)
				if ((Offset < parent.Offset) || (RelativeEndOffset > parent.RelativeEndOffset))
				{
					return false;
				}
			}

			// First atom should not be unknown
			if (IsUnknown && parent.IsRoot && !parent.HasChildren())
			{
				return false;
			}

			// Atom should have no duplicates in its parent if DuplicatesAllowed is false
			if (!HeaderName.IsFlagSet(AtomFlags.DuplicatesAllowed) && parent.HasChild(HeaderName))
			{
				// Atoms of partial files may end up in the root
				// Therefore, duplicates are allowed, except for top-level atoms
				if (!parent.IsRoot || HeaderName.IsTopLevel())
				{
					return false;
				}
			}

			// Atom is allowed in its suitable parent (correct files)
			if (HeaderName.GetSuitableParents().Contains(parent.HeaderName))
			{
				return true;
			}

			// Atom ends up in the root if no suitable parent was found (partial files)
			if (!parent.IsRoot)
			{
				return false;
			}

			// Atoms of partial files are allowed in the root if empty
			if (!parent.HasChildren())
			{
				return true;
			}

			// Root should not already contain top-level atoms (FileType, MediaData or Movie)
			// Otherwise, the top-level parent of this atom should have been in the root as well
			foreach (QtAtom atom in parent.Children)
			{
				if (atom.HeaderName.IsTopLevel())
				{
					return false;
				}
			}

			// Depth of this atom should be equal or lower than last child of the root
			//if (!IsAtomTypeAllowedInRootAtThisPoint(parent, HeaderName))
			//{
			//    return false;
			//}

			// Root should not contain only suitable ancestor of the atom
			//if (false)
			//{
			//    return false;
			//}

			// No 'higher atoms' than its suitable parent (siblings of an ancestor)
			//if (false)
			//{
			//    return false;
			//}

			return true;
		}

		public override sealed bool IsBackToBack(QtAtom header)
		{
			return true;
		}

		/// <summary>
		/// Parses the 3-byte flags attribute.
		/// </summary>
		/// <param name="parser">the parser</param>
		public virtual void ParseFlags(QtParser parser)
		{
			parser.GetThreeBytes(Attribute.Flags);
		}

		/// <summary>
		/// Returns whether this atom contains a child of the given type.
		/// </summary>
		/// <param name="atomName">the atom name type</param>
		/// <returns>true if this atom contains such as child, false otherwise</returns>
		internal bool HasChild(AtomName atomName)
		{
			return FindChild(atomName) != null;
		}
	}
}
