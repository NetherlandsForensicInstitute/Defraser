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

using System.Collections.Generic;
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Extends formatted attribute with child attributes.
	/// </summary>
	/// <typeparam name="TEnum">the enumeration type or <c>string</c></typeparam>
	/// <typeparam name="TValue">the type of <c>Value</c></typeparam>
	/// <typeparam name="TParser">the the type of parser</typeparam>
	public abstract class CompositeAttribute<TEnum, TValue, TParser> : FormattedAttribute<TEnum, TValue>
	{
		private readonly List<IResultAttribute> _attributes = new List<IResultAttribute>();

		#region Properties
		public override IList<IResultAttribute> Attributes { get { return _attributes; } }
		#endregion Properties


		public CompositeAttribute(TEnum attribute, TValue value)
			: base(attribute, value)
		{
		}

		public CompositeAttribute(TEnum attribute, TValue value, string format)
			: base(attribute, value, format)
		{
		}

		/// <summary>
		/// Parses the composite attribute from <paramref name="parser"/>.
		/// The <c>Offset</c> of this attribute is set to the current <c>Position</c>
		/// of <c>parser.DataReader</c>.
		/// </summary>
		/// <param name="parser">the parser</param>
		/// <returns>whether the attribute was parsed successfully</returns>
		public abstract bool Parse(TParser parser);
	}
}
