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

namespace Defraser.Detector.Common
{
	/// <summary>
	/// This attribute's integer <c>Value</c> is the index into a list of options.
	/// 
	/// For example, the <em>picture coding type</em> field present in most
	/// modern video compression formats is often stored as an integer value,
	/// where distinct values represent different picture coding types.
	/// The options offer a textual description of the picture coding types.
	/// </summary>
	/// <remarks>
	/// The option value <c>null</c> can be used to mark invalid options.
	/// </remarks>
	/// <typeparam name="TEnum1">the enumeration type for the attribute name</typeparam>
	/// <typeparam name="TEnum2">the enumeration type for the option pair value</typeparam>
	public sealed class FourCCWithOptionalDescriptionAttribute<TEnum1, TEnum2> : Attribute<TEnum1, uint>	// where TEnum : enum
	{
		private readonly Dictionary<uint, TEnum2> _optionalDescription;

		#region Properties
		public override string ValueAsString
		{
			get
			{
				if(_optionalDescription.ContainsKey(TypedValue))
				{
					return string.Format("{0} ({1})", _optionalDescription[TypedValue], TypedValue.ToString4CC());
				}
				return string.Format("{0}", TypedValue.ToString4CC());
			}
		}
		/// <summary>The options indexed by <c>Value</c>.</summary>
		public Dictionary<uint, TEnum2> Options { get { return _optionalDescription; } }
		#endregion Properties

		/// <summary>
		/// OptionPairAttribute constuctor
		/// </summary>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="value">the initial value of the attribute</param>
		/// <param name="optionalDescription">the options dec</param>
		/// <param name="allowValuesOutsideRange">when set the value may lay outside the range of the <paramref name="optionalDescription"/></param>
		public FourCCWithOptionalDescriptionAttribute(TEnum1 attributeName, uint value, Dictionary<uint, TEnum2> optionalDescription)
			: base(attributeName, value)
		{
			if (optionalDescription == null)
			{
				throw new ArgumentNullException("optionalDescription");
			}
			_optionalDescription = optionalDescription;
		}
	}
}
