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
using System.Collections;
using System.Collections.Generic;

namespace Defraser.DataStructures
{
	/// <summary>
	/// Provides a read-only view on a generic dictionary.
	/// </summary>
	/// <typeparam name="TKey">the type of keys in the dictionary</typeparam>
	/// <typeparam name="TValue">the type of values in the dictionary</typeparam>
	public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private const string ReadOnlyMessage = "Dictionary is read-only.";

		private readonly IDictionary<TKey, TValue> _dictionary;

		#region Properties
		public int Count { get { return _dictionary.Count; } }
		public bool IsReadOnly { get { return true; } }
		public ICollection<TKey> Keys { get { return _dictionary.Keys; } }
		public ICollection<TValue> Values { get { return _dictionary.Values; } }

		public TValue this[TKey key]
		{
			get { return _dictionary[key]; }
			set
			{
				throw new NotSupportedException(ReadOnlyMessage);
			}
		}
		#endregion Properties


		/// <summary>
		/// Creates a new read-only dictionary.
		/// </summary>
		/// <param name="dictionary">the dictionary to wrap</param>
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}

			_dictionary = dictionary;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public void Add(TKey key, TValue value)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public bool Remove(TKey key)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public void Clear()
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _dictionary.Contains(item);
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}
	}
}

