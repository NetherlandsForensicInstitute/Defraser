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
using System.Collections.Specialized;
using System.Windows.Forms;

namespace Defraser.GuiCe.sendtolist
{
	/// <summary>
	/// A null-instance, representing a failed creation/import.
	/// </summary>
	public class InvalidSendToItem : ISendToItem
	{
		private readonly string _name;
		private string _path;
        private string _parameters;

		public InvalidSendToItem(string name, string path, string parameters)
		{
			_name = name;
			_path = path;
            _parameters = parameters;
		}

		#region ISendToItem Members

		public string Name
		{
			get { return _name; }
		}

		public string Path
		{
			get { return _path; }
			set { throw new NotSupportedException("This ISendToItem is used to mark a failed creation/import. This should not be used. Please create a new one."); }
		}

        public string Parameters 
        {
            get { return _parameters; }
			set { throw new NotSupportedException("This ISendToItem is used to mark a failed creation/import. This should not be used. Please create a new one."); }
        }

		public event EventHandler<EventArgs> PathChanged;

        public event EventHandler<EventArgs> ParametersChanged;

		public void SaveToSettings(StringCollection collection)
		{
		}

		public void ShowIn(ToolStripItemCollection collection, ISelection selection)
		{
		}

		public bool IsHexWorkShop()
		{
			return false;
		}

		public bool SaveToDictionary(IDictionary<string, ISendToItem> dictionary)
		{
			return false;
		}

		public ISendToItem DeepClone()
		{
			return new InvalidSendToItem(_name, _path, _parameters);
		}

		#endregion
	}
}
