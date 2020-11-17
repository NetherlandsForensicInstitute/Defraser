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
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Base class for detector result attributes.
	/// Subclasses must provide a formatter method <c>ValueAsString</c>.
	/// </summary>
	/// <remarks>
	/// The type parameter <typeparamref name="TEnum"/> should be set to
	/// the <c>Attribute</c> enumeration type of a header class or <c>string</c>
	/// to specify attributes without a corresponding column.
	/// </remarks>
	/// <typeparam name="TEnum">the enumeration type or <c>string</c></typeparam>
	/// <typeparam name="TValue">the type of <c>Value</c></typeparam>
	/// <seealso cref="Detector"/>
	public abstract class Attribute<TEnum, TValue> : IResultAttribute	// where TEnum : enum | string
	{
		private bool _valid;
		private TValue _value;

		#region Properties
		protected TEnum AttributeName { get; set; }
		public string Name
		{
			get
			{
				if (typeof(TEnum).IsEnum)
				{
					return Enum.GetName(typeof(TEnum), AttributeName);
				}
				else
				{
					return AttributeName.ToString();
				}
			}
		}

		public object Value { get { return _value; } }
		public virtual IList<IResultAttribute> Attributes { get { return null; } }
		public abstract string ValueAsString { get; }

		/// <summary>The typed <c>Value</c>.</summary>
		public TValue TypedValue
		{
			get { return _value; }
			set { _value = value; }
		}

		public bool Valid
		{
			get { return _valid; }
			set { _valid = value; }
		}
		#endregion Properties


		/// <summary>
		/// Creates a new attribute with the given name and <paramref name="value"/>.
		/// </summary>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="value">the initial value of the attribute</param>
		public Attribute(TEnum attributeName, TValue value)
		{
			if (attributeName == null)
			{
				throw new ArgumentNullException("attributeName");
			}

			AttributeName = attributeName;
			_value = value;
			_valid = true;
		}

		public IResultAttribute FindAttributeByName(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (Attributes != null)
			{
				foreach (IResultAttribute attribute in Attributes)
				{
					if (attribute.Name == name)
					{
						return attribute;
					}
				}
			}
			return null;
		}
	}
}
