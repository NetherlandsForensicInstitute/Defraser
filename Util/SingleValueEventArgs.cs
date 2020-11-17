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

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="EventArgs"/> class adds a single generic value to the
	/// default <see cref="System.EventArgs"/>.
	/// <para>
	/// This class can be used to implement most simple events without the need
	/// for creating a subclass of <see cref="System.EventArgs"/>.
	/// </para>
	/// </summary>
	/// <typeparam name="T">The type of value</typeparam>
	public class SingleValueEventArgs<T> : EventArgs, IEquatable<SingleValueEventArgs<T>>
	{
		private readonly T _value;

		#region Properties
		/// <summary>The value.</summary>
		public T Value { get { return _value; } }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="EventArgs"/> with a single <param name="value"/>
		/// of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="value">The value</param>
		public SingleValueEventArgs(T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			_value = value;
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}]", GetType().Name, Value);
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return Equals(obj as SingleValueEventArgs<T>);
		}

		public bool Equals(SingleValueEventArgs<T> other)
		{
			if (other == null) return false;
			if (other == this) return true;

			return other.Value.Equals(Value);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
		#endregion Equals method
	}
}
