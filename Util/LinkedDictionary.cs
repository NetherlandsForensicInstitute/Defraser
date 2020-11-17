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
using System.Runtime.Serialization;

namespace Defraser.Util
{
	/// <summary>
	/// Provides a dictionary that retains order of insertion.
	/// 
	/// The actual dictionary maps keys to <c>LinkedListNode</c>, which in
	/// turn contain a <c>KeyValuePair</c> of the given key and value.
	/// The nodes are part of a <c>LinkedList</c> that is used to enumerate
	/// through the items in the dictionary.
	/// </summary>
	/// <remarks>
	/// The dictionary and its key and value collections are all serializable
	/// using the binary serializer.
	/// </remarks>
	/// <typeparam name="TKey">the type of keys in the dictionary</typeparam>
	/// <typeparam name="TValue">the type of values in the dictionary</typeparam>
	[DataContract]
	public sealed class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private const string ReadOnlyMessage = "Collection is read-only.";

		#region Inner classes
		/// <summary>
		/// Provides a base class for key and value collections.
		/// These collections are always read-only and provide a view of
		/// the keys or values in the dictionary.
		/// </summary>
		/// <typeparam name="T">the type of items in this collection</typeparam>
		[DataContract] private abstract class SubCollection<T> : ICollection<T>
		{
			#region Inner classes
			/// <summary>
			/// Enumerates the keys or values in the dictionary.
			/// </summary>
			private struct Enumerator : IEnumerator<T>
			{
				private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
				private readonly SubCollection<T> _collection;

				#region Properties
				public object Current { get { return _collection.GetSubItem(_enumerator.Current); } }
				T IEnumerator<T>.Current { get { return _collection.GetSubItem(_enumerator.Current); } }
				#endregion Properties


				/// <summary>
				/// Creates a new enumerator.
				/// </summary>
				/// <param name="collection">the collection to enumerate</param>
				public Enumerator(SubCollection<T> collection)
				{
					_enumerator = collection.Dictionary.GetEnumerator();
					_collection = collection;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					return _enumerator.MoveNext();
				}

				public void Reset()
				{
					_enumerator.Reset();
				}
			}
			#endregion Inner classes

			private readonly LinkedDictionary<TKey, TValue> _dictionary;

			#region Properties
			public int Count { get { return _dictionary.Count; } }
			public bool IsReadOnly { get { return true; } }
			protected LinkedDictionary<TKey, TValue> Dictionary { get { return _dictionary; } }
			#endregion Properties


			/// <summary>
			/// Creates a new collection.
			/// </summary>
			/// <param name="dictionary">the underlying dictionary</param>
			protected SubCollection(LinkedDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new Enumerator(this);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(this);
			}

			public void Add(T item)
			{
				throw new NotSupportedException(ReadOnlyMessage);
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException(ReadOnlyMessage);
			}

			public void Clear()
			{
				throw new NotSupportedException(ReadOnlyMessage);
			}

			public virtual bool Contains(T item)
			{
				EqualityComparer<T> comparer = EqualityComparer<T>.Default;

				foreach (KeyValuePair<TKey, TValue> dictionaryItem in _dictionary)
				{
					if (comparer.Equals(item, GetSubItem(dictionaryItem)))
					{
						return true;
					}
				}
				return false;
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}
				if (arrayIndex < 0)
				{
					throw new ArgumentOutOfRangeException("arrayIndex");
				}
				if ((array.Length - arrayIndex) < Count)
				{
					throw new ArgumentException("Insufficient space in array.", "arrayIndex");
				}

				// Copy the items into the array
				foreach (T item in this)
				{
					array[arrayIndex++] = item;
				}
			}

			/// <summary>
			/// Gets the key or value for the given <paramref name="item"/>.
			/// </summary>
			/// <param name="item">the item in the dictionary</param>
			protected abstract T GetSubItem(KeyValuePair<TKey, TValue> item);
		}

		/// <summary>
		/// Provides a read-only view of the keys in the dictionary.
		/// </summary>
		[DataContract] private sealed class KeyCollection : SubCollection<TKey>
		{
			public KeyCollection(LinkedDictionary<TKey, TValue> dictionary)
				: base(dictionary)
			{
			}

			public override bool Contains(TKey item)
			{
				return Dictionary.ContainsKey(item);
			}

			protected override TKey GetSubItem(KeyValuePair<TKey, TValue> item)
			{
				return item.Key;
			}
		}

		/// <summary>
		/// Provides a read-only view of the values in the dictionary.
		/// </summary>
		[DataContract] private sealed class ValueCollection : SubCollection<TValue>
		{
			public ValueCollection(LinkedDictionary<TKey, TValue> dictionary)
				: base(dictionary)
			{
			}

			protected override TValue GetSubItem(KeyValuePair<TKey, TValue> item)
			{
				return item.Value;
			}
		}
		#endregion Inner classes

		[DataMember]
		private readonly LinkedList<KeyValuePair<TKey, TValue>> _linkedList;
		private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dictionary;

		#region Properties
		public int Count { get { return _dictionary.Count; } }
		public bool IsReadOnly { get { return false; } }
		public ICollection<TKey> Keys { get { return new KeyCollection(this); } }
		public ICollection<TValue> Values { get { return new ValueCollection(this); } }

		public TValue this[TKey key]
		{
			get { return _dictionary[key].Value.Value; }
			set
			{
				LinkedListNode<KeyValuePair<TKey, TValue>> node;	
				if (_dictionary.TryGetValue(key, out node))
				{
					node.Value = new KeyValuePair<TKey, TValue>(key, value);
				}
				else
				{
					Add(key, value);
				}
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new linked dictionary.
		/// </summary>
		public LinkedDictionary()
		{
			_linkedList = new LinkedList<KeyValuePair<TKey, TValue>>();
			_dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
		}

		/// <summary>
		/// Recreates the dictionary on deserialization.
		/// </summary>
		[OnDeserialized]
		private void CompleteDeserialization(StreamingContext sc)
		{
			//_linkedList.OnDeserialization(sender);
			if (_dictionary == null) _dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
			_dictionary.Clear();

			// Populate the dictionary with the entries in the linked list
			for (LinkedListNode<KeyValuePair<TKey, TValue>> node = _linkedList.First; node != null; node = node.Next)
			{
				_dictionary.Add(node.Value.Key, node);
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _linkedList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _linkedList.GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Add(TKey key, TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (_dictionary.ContainsKey(key))
			{
				throw new ArgumentException("Another item for key exists.", "key");
			}

			_dictionary.Add(key, _linkedList.AddLast(new KeyValuePair<TKey, TValue>(key, value)));
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key == null)
			{
				return false;
			}

			// Check for existence of item
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			if (!_dictionary.TryGetValue(item.Key, out node) || !node.Value.Equals(item))
			{
				return false;
			}

			// Remove the item from the dictionary and linked list
			_dictionary.Remove(item.Key);
			_linkedList.Remove(node);
			return true;
		}

		public bool Remove(TKey key)
		{
			// Check for existence of item
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			if (key == null || !_dictionary.TryGetValue(key, out node))
			{
				return false;
			}

			// Remove the item from the dictionary and linked list
			_dictionary.Remove(key);
			_linkedList.Remove(node);
			return true;
		}

		public void Clear()
		{
			_dictionary.Clear();
			_linkedList.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			return _dictionary.TryGetValue(item.Key, out node) && node.Value.Equals(item);
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			// Check for existence of item
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			if (!_dictionary.TryGetValue(key, out node))
			{
				value = default(TValue);
				return false;
			}

			// Return value
			value = node.Value.Value;

			return true;
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_linkedList.CopyTo(array, arrayIndex);
		}

		public bool IsEqual(IDictionary<TKey, TValue> otherDictionary)
		{
			// When the new metadata is the same as the current metadata,
			// do not copy and do not send an UpdateProject event
			if (Count != otherDictionary.Count) return false;	// Both collection must be the same length
			foreach (var key in Keys)
			{
				TValue otherValue;
				if (!otherDictionary.TryGetValue(key, out otherValue)) return false;	// Both collections must contain the same key
				if (!this[key].Equals(otherValue)) return false;						// Both collections must contain the same value for the same key
			}
			return true;
		}
	}
}
