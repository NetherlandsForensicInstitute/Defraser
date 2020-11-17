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
using Defraser.DataStructures;
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Attribute with optional format string.
	/// </summary>
	/// <typeparam name="TEnum">the enumeration type or <c>string</c></typeparam>
	/// <typeparam name="TValue">the type of <c>Value</c></typeparam>
	public class FormattedAttribute<TEnum, TValue> : Attribute<TEnum, TValue>
	{
		private readonly IResultFormatter _formatter;

		#region Properties
		public override string ValueAsString { get { return _formatter.Format(TypedValue); } }
		#endregion Properties


		public FormattedAttribute(TEnum attributeName, TValue value)
			: this(attributeName, value, "{0}")
		{
		}

		/// <summary>
		/// Creates a new attribute with the given <paramref name="format"/> string.
		/// For example, use <c>"{0:X8}"</c> for hexadecimal number formatting.
		/// </summary>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="value">the initial value of the attribute</param>
		/// <param name="format">the format string</param>
		/// 
		public FormattedAttribute(TEnum attributeName, TValue value, string format)
			: base(attributeName, value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}

			_formatter = new StringResultFormatter(format);
		}

		public FormattedAttribute(TEnum attributeName, TValue value, IResultFormatter formatter)
			: base(attributeName, value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}

			_formatter = formatter;
		}

		
	}
}
