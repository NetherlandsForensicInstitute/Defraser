/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// Wraps other <see cref="SendToPlayer"/> implementations, by adding checks on the <see cref="File.Exists"/> of <see cref="Path"/>.
	/// </summary>
	public class PathValidatingSendToItem : ISendToItem
	{
		private readonly IFormFactory _formFactory;
		private readonly string _name;

		/// <summary>
		/// This item implements <see cref="ISendToItem"/> behavior assuming the <see cref="Path"/> does exist.
		/// All properties and events are always directed to this instance, functionality/behaviour only if <see cref="Path"/> is an existing valid executable.
		/// </summary>
		private readonly ISendToPlayer _player;

		public PathValidatingSendToItem(string name, ISendToPlayer player)
		{
			_player = player;
			_name = name;
		}

		#region ISendToItem Members

		/// <summary>The name of the application.</summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>The path of the application executable.</summary>
		public string Path
		{
			get { return _player.Path; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Path");
				_player.Path = value;
				InvokePathChanged(EventArgs.Empty);
			}
		}

        /// <summary>The parameters send to the application executable.</summary>
        public string Parameters 
        {
            get { return _player.Parameters; }
            set 
            {
                _player.Parameters = (value == null) ? "" : value;
                InvokeParametersChanged(EventArgs.Empty);
            }
        }

		public event EventHandler<EventArgs> PathChanged;
        public event EventHandler<EventArgs> ParametersChanged;

		public void SaveToSettings(StringCollection collection)
		{
			if (PathExists())
			{
				collection.Add(string.Format("{0};{1};{2}", _name, _player.Path, _player.Parameters));
			}
		}

		public void ShowIn(ToolStripItemCollection collection, ISelection selection)
		{
			if (PathExists())
			{
				EventHandler eventHandler = (sendingToolStripMenuItem, e) => SendSelectionToApplication(selection);
				collection.Add(Name, null, eventHandler);
			}
		}

		public bool IsHexWorkShop()
		{
			return _player.IsHexWorkShop();
		}

		public bool SaveToDictionary(IDictionary<string, ISendToItem> dictionary)
		{
			dictionary.Add(_name, this);
			return true;
		}

		public ISendToItem DeepClone()
		{
			var clone = new PathValidatingSendToItem(_name, _player.DeepClone());
			var pathChangedInvocations = PathChanged.GetInvocationList();
			foreach (var invocation in pathChangedInvocations)
			{
				clone.PathChanged+=(EventHandler<EventArgs>)invocation;
			}
            if (ParametersChanged != null)
            {
                var parametersChangedInvocations = ParametersChanged.GetInvocationList();
                foreach (var invocation in parametersChangedInvocations)
                {
                    clone.ParametersChanged += (EventHandler<EventArgs>)invocation;
                }
            }
			return clone;
		}

		#endregion

		private bool PathExists()
		{
			return File.Exists(_player.Path);
		}

		private void InvokePathChanged(EventArgs e)
		{
			EventHandler<EventArgs> handler = PathChanged;
			if (handler != null) handler(this, e);
		}

        private void InvokeParametersChanged(EventArgs e) 
        {
            EventHandler<EventArgs> handler = ParametersChanged;
            if (handler != null) handler(this, e);
        }

		private void SendSelectionToApplication(ISelection selection)
		{
			if (!PathExists())
			{
				MessageBox.Show("The path of the destination is not valid. Please, select an other executable.",
				                "Invalid destination", MessageBoxButtons.OK, MessageBoxIcon.Information);
				var editForm = _formFactory.Create<EditSendToItemForm>();
				editForm.Item = this;
				if (editForm.ShowDialog(Application.OpenForms[0]) != DialogResult.OK)
				{
					return;
				}
			}
			if (PathExists())
				_player.SendSelectionToApplication(selection);
		}
	}
}
