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
using System.Diagnostics;
using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Describes the flags for full atoms.
	/// </summary>
	/// <typeparam name="TEnum">the type of attribute</typeparam>
	/// <typeparam name="TFlag">the flags enumeration</typeparam>
	internal sealed class FlagsAttribute<TEnum, TFlag> : CompositeAttribute<TEnum, uint, QtParser>
	{
		private readonly int _size;

		/// <summary>
		/// Creates a new flags attribute.
		/// </summary>
		/// <param name="attributeName">the attribute name</param>
		/// <param name="size">the size of the flags field in byte(s)</param>
		public FlagsAttribute(TEnum attributeName, int size)
			: base(attributeName, 0U, "{0:X" + (2 * size) + "}")
		{
			Debug.Assert(size >= 1 && size <= 4);
			_size = size;
		}

		public override bool Parse(QtParser parser)
		{
			uint flags = 0;
			for (int i = 0; i < _size; i++)
			{
				flags = (flags << 8) | parser.GetByte();
			}
			foreach (TFlag flag in Enum.GetValues(typeof(TFlag)))
			{
				parser.AddAttribute(new FormattedAttribute<TFlag, bool>(flag, IsFlagSet(flags, flag)));
			}

			this.TypedValue = flags;

			return this.Valid;
		}

		/// <summary>
		/// Returns whether the given <paramref name="flag"/> is set in <paramref name="flags"/>.
		/// </summary>
		/// <param name="flags">the flags</param>
		/// <param name="flag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		private static bool IsFlagSet(uint flags, object flag)
		{
			return (flags & (int)flag) != 0;
		}
	}
}
