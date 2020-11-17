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
using System.Runtime.Serialization;

namespace Defraser.Util
{
	/// <summary>
	/// Provides a generic pair of two values.
	/// From the book: 'C# in Depth' by Jon Skeet
	/// </summary>
	/// <typeparam name="TFirst">the type of the first value</typeparam>
	/// <typeparam name="TSecond">the type of the second value</typeparam>
	[DataContract]
	public sealed class Pair<TFirst, TSecond>
		: IEquatable<Pair<TFirst, TSecond>>
	{
		private TFirst _first;
		private TSecond _second;

		[DataMember]
		public TFirst First { get { return _first; } private set { _first = value; } }
		[DataMember]
		public TSecond Second { get { return _second; } private set { _second = value; } }

		public Pair(TFirst first, TSecond second)
		{
			_first = first;
			_second = second;
		}

		public bool Equals(Pair<TFirst, TSecond> other)
		{
			if (other == null)
			{
				return false;
			}
			return EqualityComparer<TFirst>.Default.Equals(First, other.First) &&
			       EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
		}

		public override bool Equals(object o)
		{
			return Equals(o as Pair<TFirst, TSecond>);
		}

		public override int GetHashCode()
		{
			return EqualityComparer<TFirst>.Default.GetHashCode(_first)
				.CombineHashCode(EqualityComparer<TSecond>.Default.GetHashCode(_second));
		}
	}
}
