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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.GuiCe.Properties;
using Defraser.Util;
using Microsoft.Win32;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// Lists the send-to target applications, should be a singleton! (else the multiple instances overwrite each others values when flushing to Settings.Default 
	/// </summary>
	public sealed class SendToList : ICollection<ISendToItem> //TODO: make it multiple-instance-safe (not per se a singleton)
	{
		private readonly SendToItemFactory _itemFactory;
		private readonly LinkedDictionary<string, ISendToItem> _items;

		#region Properties

		#region ICollection<ISendToItem> properties

		public int Count
		{
			get { return _items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion ICollection<ISendToItem> properties

		#endregion Properties

		/// <summary>
		/// Creates a new send-to list.
		/// </summary>
		public SendToList(SendToItemFactory itemFactory)
		{
			_itemFactory = itemFactory;
			_items = new LinkedDictionary<string, ISendToItem>();

			// Create list with only default items
			AddDefaultPrograms();

			Settings.Default.SettingsLoaded += Default_SettingsLoaded;
			Settings.Default.SettingsSaving += Default_SettingsSaving;
		}

		#region ICollection<ISendToItem> methods

		public IEnumerator<ISendToItem> GetEnumerator()
		{
			return new Enumerator(_items);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(_items);
		}

		public void Add(ISendToItem item)
		{
			if (!_items.ContainsKey(item.Name)) // Only add the item if the key does not already exists
			{
				if (item.SaveToDictionary(_items))
				{
					item.PathChanged += InvokeCollectionChangedEventHandler;
					InvokeCollectionChanged();
				}
			}
		}

		public bool Remove(ISendToItem item)
		{
			if (_items.Remove(new KeyValuePair<string, ISendToItem>(item.Name, item)))
			{
				item.PathChanged -= InvokeCollectionChangedEventHandler;
				InvokeCollectionChanged();
				return true;
			}
			return false;
		}

		public void Clear()
		{
			foreach (var item in _items)
			{
				item.Value.PathChanged -= InvokeCollectionChangedEventHandler;
			}
			_items.Clear();
			InvokeCollectionChanged();
		}

		public bool Contains(ISendToItem item)
		{
			return _items.Contains(new KeyValuePair<string, ISendToItem>(item.Name, item));
		}

		public void CopyTo(ISendToItem[] array, int arrayIndex)
		{
			int count = Count;

			if (array.Length < count)
			{
				throw new ArgumentException("Array too small to hold items.", "array");
			}
			if ((arrayIndex + count) > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			foreach (ISendToItem item in this)
			{
				array[arrayIndex++] = item;
			}
		}

		private void InvokeCollectionChangedEventHandler(Object sender, EventArgs args)
		{
			InvokeCollectionChanged();
		}


		public ISendToListMemento GetStateMemento()
		{
			return new Memento(this);
		}

		#endregion ICollection<ISendToItem> methods

		/// <summary>
		/// Occurs when the send-to item collection changes.
		/// </summary>
		public event EventHandler<EventArgs> CollectionChanged;

		/// <summary>
		/// Adds all items in the given <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">the items to add</param>
		public void AddRange(IEnumerable<ISendToItem> collection)
		{
			foreach (ISendToItem item in collection)
			{
				_items.Add(item.Name, item);
				item.PathChanged += InvokeCollectionChangedEventHandler;
			}
			InvokeCollectionChanged();
		}

		/// <summary>
		/// Gets the send-to item with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">the name of the application</param>
		/// <returns>the send-to item</returns>
		public ISendToItem GetItem(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			ISendToItem item;
			if (!_items.TryGetValue(name, out item))
			{
				throw new ArgumentException("Item does not exist.", "name");
			}
			return item;
		}

		/// <summary>
		/// Removes the send-to item with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">the name of the application</param>
		/// <returns>true if the item was removed, false otherwise</returns>
		public bool RemoveItem(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (_items.ContainsKey(name))
			{
				_items[name].PathChanged -= InvokeCollectionChangedEventHandler;
				_items.Remove(name);
				InvokeCollectionChanged();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether a send-to item with the given <paramref name="name"/> exists.
		/// </summary>
		/// <param name="name">the name of the application</param>
		/// <returns>true if the item exists, false otherwise</returns>
		public bool ContainsItem(string name)
		{
			return _items.ContainsKey(name);
		}

		/// <summary>
		/// Gets the path for the send-to item with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">the name of the application</param>
		/// <returns>
		/// the path of the application executable, <c>null</c> if it the item does not exist
		/// </returns>
		public string GetPath(string name)
		{
			ISendToItem item;
			if (!_items.TryGetValue(name, out item))
			{
				return null;
			}
			return item.Path;
		}

		/// <summary>
		/// Adds drop down menu items for all external applications to the
		/// given <paramref name="toolStripMenuItem"/>.
		/// </summary>
		/// <param name="toolStripMenuItem">the item to add the drop down items to</param>
		/// <param name="selection">the selection to send to external applications</param>
		public void AddDropDownMenuItems(ToolStripMenuItem toolStripMenuItem, ISelection selection)
		{
			// Add menu separator if necessary
			if (toolStripMenuItem.DropDownItems.Count > 0 && Count > 0)
			{
				toolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
			}

			// Create and add items for the external applications to the drop down menu
			foreach (ISendToItem item in this)
			{
				item.ShowIn(toolStripMenuItem.DropDownItems, selection);
			}
		}

		/// <summary>
		/// Open the file <paramref name="fileName"/> readonly in HexWorkshop,
		/// and execute the command 'goto <paramref name="offset"/>'.
		/// </summary>
		/// <param name="fileName">the file to send to HexWorkshop.</param>
		/// <param name="offset">the offset to go to in HexWorkshop.</param>
		public void GotoOffsetInHexWorkshop(string fileName, long offset)
		{
			string hexWorkshopPath = GetHexWorkshopPath();
			if (string.IsNullOrEmpty(hexWorkshopPath)) return;

			string cmd = string.Format("\"{0}\" /readonly /goto:{1}", fileName, offset);
			Process.Start(hexWorkshopPath, cmd);
		}

		private string GetHexWorkshopPath()
		{
			foreach (ISendToItem item in _items.Values)
			{
				if (item.IsHexWorkShop())
				{
					return item.Path;
				}
			}
			return string.Empty;
		}


		public void InvokeCollectionChanged()
		{
			var handler = CollectionChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		public void AddSendToItem(string prettyApplicationName, string applicationPath, string parameters)
		{
			Add(_itemFactory.Create(prettyApplicationName, applicationPath, parameters));
		}

		private void AddSendToItem(string registryKey, string registryValue, string applicationName, string prettyApplicationName, string parameters)
		{
			var path = (string) Registry.GetValue(registryKey, registryValue, null);

			if (string.IsNullOrEmpty(path)) return;

			// If an application name is specified, use Path.GetDirectory() to get the directory only
			if (!string.IsNullOrEmpty(applicationName) && File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
			}

			AddSendToItem(prettyApplicationName, Path.Combine(path, applicationName), parameters);
		}

		/// <summary>
		/// Adds send-to items for default programs that are installed.
		/// </summary>
		private void AddDefaultPrograms()
		{
            string defaultParameters = "[DAT]"; // the default is the selected headers
            string[] subNodes = {string.Empty, @"Wow6432Node\" };
            int rounds = Is64BitsOs() ? 2 : 1;
            for (int i = 0; i < rounds ; i++)
            {
                AddSendToItem(string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\{0}VideoLAN\VLC", subNodes[i]), string.Empty, string.Empty, "VLC Media Player", defaultParameters);
                AddSendToItem(string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\{0}Microsoft\MediaPlayer", subNodes[i]), "Installation Directory", "wmplayer.exe", "Windows Media Player", defaultParameters);
                AddSendToItem(string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\{0}Apple Computer, Inc.\QuickTime", subNodes[i]), "InstallDir", "QuickTimePlayer.exe", "QuickTime Player", defaultParameters);
                AddSendToItem(string.Format(@"HKEY_LOCAL_MACHINE\SOFTWARE\{0}GRETECH\GomPlayer", subNodes[i]), "ProgramPath", "", "GOM Player", defaultParameters);
            }
            
            AddSendToItem("FF Play", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ffplay\ffplay.exe"), defaultParameters);
			AddSendToItem("FF Play", Path.Combine(string.Format("{0} (x86)", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)), @"ffplay\ffplay.exe"), defaultParameters);
            AddSendToItem("NFI Player", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"NFIPlayer\NFIplayer2.exe"), defaultParameters);
            AddSendToItem("NFI Player", Path.Combine(string.Format("{0} (x86)", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)), @"NFIPlayer2\NFIplayer.exe"), defaultParameters);
            AddHexWorkshop();
		}

		private static bool Is64BitsOs()
		{
			try
			{
				return IntPtr.Size == 8;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void AddHexWorkshop()
		{
			_itemFactory.AddHexWorkshopTo(Add);
		}

		private void Default_SettingsSaving(object sender, CancelEventArgs e)
		{
			Settings.Default.SendToList = new StringCollection();
			foreach (ISendToItem sendToItem in this)
			{
				sendToItem.SaveToSettings(Settings.Default.SendToList);
			}
		}

		private void Default_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
		{
			if (Settings.Default.SendToList != null)
			{
				foreach (string serializedItem in Settings.Default.SendToList)
				{
					var settings = _itemFactory.LoadFromSettings(serializedItem);
					Add(settings);
				}
			}
		}

		public bool HexWorkshopAvailable()
		{
			foreach (ISendToItem entry in this)
			{
				if (entry.IsHexWorkShop())
					return true;
			}
			return false;
		}

		#region Inner classes

		/// <summary>
		/// Enumerates the send-to items in the list.
		/// </summary>
		private struct Enumerator : IEnumerator<ISendToItem>
		{
			private readonly IEnumerator<KeyValuePair<string, ISendToItem>> _enumerator;

			#region Properties

			public object Current
			{
				get { return _enumerator.Current.Value; }
			}

			ISendToItem IEnumerator<ISendToItem>.Current
			{
				get { return _enumerator.Current.Value; }
			}

			#endregion Properties

			/// <summary>
			/// Creates a new enumerator.
			/// </summary>
			/// <param name="dictionary">the dictionary to enumerate</param>
			public Enumerator(IEnumerable<KeyValuePair<string, ISendToItem>> dictionary)
			{
				_enumerator = dictionary.GetEnumerator();
			}

			#region IEnumerator<ISendToItem> Members

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

			#endregion
		}

		/// <summary>
		/// Remembers the collection contents, such that an 'undo' operation can be created.
		/// </summary>
		private class Memento : ISendToListMemento
		{
			private readonly IDictionary<string, ISendToItem> _originalItems= new LinkedDictionary<string,ISendToItem>();
			private readonly SendToList _mementoedList;

			public Memento(SendToList currentList)
			{
				_mementoedList = currentList;
				foreach (var currentItem in currentList._items)
				{
					_originalItems.Add(currentItem.Key,currentItem.Value.DeepClone());
				}
			}

			public void RevertToRememberedState()
			{
				_mementoedList.Clear();
				foreach (var item in _originalItems)
				{
					_mementoedList._items.Add(item);
				}
				_mementoedList.InvokeCollectionChanged();
			}
		}
		#endregion Inner classes
	}
}
