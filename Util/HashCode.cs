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

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="HashCode"/> class provides extension methods for implementing
	/// the <see cref="object.GetHashCode"/> method.
	/// </summary>
	/// <remarks>
	/// Described in Effective Java, 2nd edition, page 47-48 (item 9).
	/// </remarks>
	public static class HashCode
	{
		/// <summary>The hash code initialization value.</summary>
		private const int InitializationValue = 17;
		/// <summary>The multiplier for combining hash codes.</summary>
		private const int CombineMultiplier = 31;

		/// <summary>
		/// Returns the hash code for empty objects.
		/// </summary>
		/// <returns>The hash code for empty (non-null) objects</returns>
		public static int EmptyHashCode
		{
			get { return InitializationValue; }
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>bool</c> field with value <paramref name="b"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="b">The value of the <c>bool</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, bool b)
		{
			return CombineHashCodes(hashCode, (b ? 1 : 0));
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an <c>sbyte</c> field with value <paramref name="s"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="s">The value of the <c>sbyte</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, sbyte s)
		{
			return CombineHashCodes(hashCode, s);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an <c>byte</c> field with value <paramref name="b"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="b">The value of the <c>byte</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, byte b)
		{
			return CombineHashCodes(hashCode, b);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a character field with value <paramref name="c"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="c">The value of the character field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, char c)
		{
			return CombineHashCodes(hashCode, c);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>short</c> field with value <paramref name="s"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="s">The value of the <c>short</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, short s)
		{
			return CombineHashCodes(hashCode, s);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an <c>ushort</c> field with value <paramref name="u"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="u">The value of the <c>ushort</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, ushort u)
		{
			return CombineHashCodes(hashCode, u);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an integer field with value <paramref name="i"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="i">The value of the integer field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, int i)
		{
			return CombineHashCodes(hashCode, i);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an unsigned integer field with value <paramref name="u"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="u">The value of the unsigned integer field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, uint u)
		{
			return CombineHashCodes(hashCode, (int)u);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>long</c> field with value <paramref name="l"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="l">The value of the <c>long</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, long l)
		{
			return CombineHashCodes(hashCode,  (int) (l ^ (l >> 32)));
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>ulong</c> field with value <paramref name="l"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="u">The value of the <c>ulong</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, ulong u)
		{
			return CombineHashCodes(hashCode, (int)(u ^ (u >> 32)));
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>float</c> field with value <paramref name="f"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="f">The value of the <c>float</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, float f)
		{
			return CombineHashCodes(hashCode, f.GetHashCode());
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>double</c> field with value <paramref name="d"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="d">The value of the <c>double</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, double d)
		{
			return CombineHashCodes(hashCode, d.GetHashCode());
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with a <c>decimal</c> field with value <paramref name="d"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="d">The value of the <c>decimal</c> field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, decimal d)
		{
			return CombineHashCodes(hashCode, d.GetHashCode());
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an enumerable field with value <paramref name="e"/>.
		/// Enumerables include arrays, lists, dictionaries, etc.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="e">The value of the enumerable field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode<T>(this int hashCode, IEnumerable<T> e)
		{
			if (e == null)
			{
				return CombineHashCodes(hashCode, 0);
			}

			int eHashCode = InitializationValue;
			foreach (T element in e)
			{
				eHashCode = eHashCode.CombineHashCode(element);
			}
			return CombineHashCodes(hashCode, eHashCode);
		}

		/// <summary>
		/// Combines the hash codes of an object with given <paramref name="hashCode"/>
		/// that is extended with an object field with value <paramref name="o"/>.
		/// </summary>
		/// <param name="hashCode">The initial hash code of the object</param>
		/// <param name="o">The value of the object field</param>
		/// <returns>The combined hash code</returns>
		public static int CombineHashCode(this int hashCode, object o)
		{
			return CombineHashCodes(hashCode, ((o == null) ? 0 : o.GetHashCode()));
		}

		private static int CombineHashCodes(int hashCode, int additionalHashCode)
		{
			return (CombineMultiplier * hashCode) + additionalHashCode;
		}
	}
}
