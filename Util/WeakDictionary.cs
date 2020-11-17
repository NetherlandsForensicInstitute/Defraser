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

namespace Defraser.Util
{
	/// <summary>
	/// Dictionary of weak references to objects of type <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TKey">the type of keys</typeparam>
	/// <typeparam name="TValue">the type of objects</typeparam>
	public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class
	{
		#region Inner classes
		private class WeakEnumerator : IEnumerator<KeyValuePair<TKey,TValue>>
		{
			private readonly IEnumerator<KeyValuePair<TKey, WeakReference>> _enumerator;
			private KeyValuePair<TKey, TValue> _current;

			#region Properties
			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					if (_current.Value == null)
					{
						throw new InvalidOperationException("Enumerator is before or after the collection.");
					}
					return _current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					if (_current.Value == null)
					{
						throw new InvalidOperationException("Enumerator is before or after the collection.");
					}
					return _current;
				}
			}
			#endregion Properties


			public WeakEnumerator(IEnumerator<KeyValuePair<TKey,WeakReference>> enumerator)
			{
				_enumerator = enumerator;
				_current = default(KeyValuePair<TKey, TValue>);
			}

			public void Dispose()
			{
				_enumerator.Dispose();
				_current = default(KeyValuePair<TKey, TValue>);
			}

			public bool MoveNext()
			{
				while (_enumerator.MoveNext())
				{
					TValue value = _enumerator.Current.Value.Target as TValue;
					if (value != null)
					{
						_current = new KeyValuePair<TKey, TValue>(_enumerator.Current.Key, value);
						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				_enumerator.Reset();
				_current = default(KeyValuePair<TKey, TValue>);
			}
		}
		#endregion Inner classes

		private readonly Dictionary<TKey, WeakReference> _dictionary;

		#region Properties
		public ICollection<TKey> Keys
		{
			get
			{
				List<TKey> keys = new List<TKey>();
				foreach (KeyValuePair<TKey, WeakReference> entry in _dictionary)
				{
					if (entry.Value.Target != null)
					{
						keys.Add(entry.Key);
					}
				}
				return keys.ToArray();
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				List<TValue> values = new List<TValue>();
				foreach (WeakReference value in _dictionary.Values)
				{
					Object ob = value.Target;
					if (ob != null)
					{
						values.Add((TValue)ob);
					}
				}
				return values.ToArray();
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				WeakReference weakRef = _dictionary[key];
				Object ob = (weakRef == null) ? null : _dictionary[key].Target;
				return (ob == null) ? default(TValue) : (TValue)ob;
			}
			set
			{
				_dictionary[key] = new WeakReference(value);
			}
		}

		public int Count
		{
			get
			{
				int count = 0;
				foreach (WeakReference value in _dictionary.Values)
				{
					if (value.IsAlive)
					{
						count++;
					}
				}
				return count;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		protected Dictionary<TKey, WeakReference> Dictionary
		{
			get { return _dictionary; }
		}
		#endregion Properties


		public WeakDictionary()
		{
			_dictionary = new Dictionary<TKey, WeakReference>();
		}

		public void Add(TKey key, TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (Dictionary.ContainsKey(key))
			{
				Dictionary[key] = new WeakReference(value);
			}
			else
			{
				// TODO: opruimen!
				Dictionary.Add(key, new WeakReference(value));
			}
		}

		public bool ContainsKey(TKey key)
		{
			WeakReference weakRef;
			return Dictionary.TryGetValue(key, out weakRef) && weakRef.IsAlive;
		}

		public bool Remove(TKey key)
		{
			return Dictionary.Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			WeakReference weakRef;
			value = Dictionary.TryGetValue(key, out weakRef) ? (weakRef.Target as TValue) : null;
			return (value != null);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			Dictionary.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			return TryGetValue(item.Key, out value) && (value.Equals(item.Value));
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			(new List<KeyValuePair<TKey, TValue>>(this)).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Contains(item) && Dictionary.Remove(item.Key);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return new WeakEnumerator(Dictionary.GetEnumerator());
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new WeakEnumerator(Dictionary.GetEnumerator());
		}
	}
}
