/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
	/// Maps values to <see cref="Enum"/> names, assuming the value is the underlying type's value.
	/// </summary>
	public sealed class EnumResultFormatter<TEnum> : IValidityResultFormatter
	{
		private readonly IDictionary<int,string> _options;

		/// <summary>
		/// Uses the names of given <typeparamref name="TEnum"/> as options array.
		/// An int value will be formatted as the name of the enumeration value with the same underlying integer value.
		/// </summary>
		/// <remarks>
		/// The underlying type of the enumeration must be able to be casted to int, just as the formatted value.
		/// </remarks>
		/// <typeparam name="TEnum">the enumeration type</typeparam>
		/// <see cref="Enum.GetValues(Type)" />
		/// <see cref="Enum.GetName(Type, object)" />
		public EnumResultFormatter()
		{
			Array optionValues = Enum.GetValues(typeof(TEnum));
			_options = new Dictionary<int, string>(optionValues.Length);
			foreach (int value in optionValues)
			{
				_options[value] = Enum.GetName(typeof(TEnum), value);
			}
		}

		public string Format(object value)
		{
			int intValue = -1;
			if (value is int)
			{
				intValue = (int) value;
			}
			else if (value is uint)
			{
				intValue = (int)(uint)value;
			}
			string optionValue;
			return String.Format((_options.TryGetValue(intValue, out optionValue)? optionValue : "<invalid>")+"({0})",value);
		}

		public bool IsValid(object value)
		{
			return _options.ContainsKey((int) value);
		}
	}
}
