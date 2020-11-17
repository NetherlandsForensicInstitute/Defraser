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

namespace Defraser.Util
{
	/// <summary>
	/// Provides a read-only view on a generic list or collection.
	/// </summary>
	/// <typeparam name="T">the type of elements in the collection</typeparam>
	public sealed class ReadOnlyList<T> : IList<T>
	{
		private const string ReadOnlyMessage = "List is read-only.";

		private readonly ICollection<T> _collection;

		#region Properties
		public int Count { get { return _collection.Count; } }
		public bool IsReadOnly { get { return _collection.IsReadOnly; } }

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				// Use indexer if collection is a generic list
				IList<T> list = _collection as IList<T>;
				if (list != null)
				{
					return list[index];
				}

				// Get the item by iterating over the collection, an O(n) operation
				foreach (T item in _collection)
				{
					if (index-- == 0)
					{
						return item;
					}
				}

				// This should never happen!
				throw new ArgumentOutOfRangeException("index");
			}
			set
			{
				throw new NotSupportedException(ReadOnlyMessage);
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new read-only list.
		/// </summary>
		/// <param name="list">the list to wrap</param>
		public ReadOnlyList(IList<T> list)
		{
			_collection = list;
		}

		/// <summary>
		/// Creates a new read-only list for the given <paramref name="collection"/>.
		/// Particularly useful for wrapping dictionary key or value collections.
		/// </summary>
		/// <param name="collection">the collection to wrap</param>
		public ReadOnlyList(ICollection<T> collection)
		{
			_collection = collection;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		public void Add(T item)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public void Clear()
		{
			throw new NotSupportedException(ReadOnlyMessage);
		}

		public bool Contains(T item)
		{
			return _collection.Contains(item);
		}

		public int IndexOf(T item)
		{
			// Use indexer if collection is a generic list
			IList<T> list = _collection as IList<T>;
			if (list != null)
			{
				return list.IndexOf(item);
			}

			// Find the item by iterating over the collection
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			int index = 0;

			foreach (T element in _collection)
			{
				if (comparer.Equals(element, item))
				{
					return index;
				}

				index++;
			}
			return -1;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_collection.CopyTo(array, arrayIndex);
		}
	}
}
