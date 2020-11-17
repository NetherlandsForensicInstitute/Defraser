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
	/// <typeparam name="TEnum">the enumeration type</typeparam>
	public sealed class OptionAttribute<TEnum> : Attribute<TEnum, int>	// where TEnum : enum
	{
		private readonly string[] _options;

		#region Properties
		public override string ValueAsString
		{
			get
			{
				if (TypedValue < 0 || TypedValue >= _options.Length)
				{
					return string.Format("{0}",TypedValue);
				}
				return string.Format("{0} ({1})", _options[TypedValue], TypedValue);
			}
		}
		/// <summary>The options indexed by <c>Value</c>.</summary>
		public string[] Options { get { return _options; } }
		#endregion Properties

		public OptionAttribute(TEnum attributeName, int value, string[] options, bool allowValuesOutsideOfRange)
			: base(attributeName, value)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			if ((allowValuesOutsideOfRange == false  ) &&
				(value < 0 || value >= options.Length)    )
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_options = options;
		}
	}
}
