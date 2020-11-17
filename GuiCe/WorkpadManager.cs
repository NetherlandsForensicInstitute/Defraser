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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Defraser.Interface;
using Defraser.Util;
using WeifenLuo.WinFormsUI.Docking;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Manages the workpads for the application.
	/// </summary>	
	public sealed class WorkpadManager
	{
		/// <summary>Occurs when a workpad is added or removed.</summary>
		public event PropertyChangedEventHandler WorkpadsChanged;

		private readonly Creator<Workpad> _createWorkPad;
	    private readonly DockPanel _dockPanel;
		private readonly List<Workpad> _workpads;
		private readonly IList<Workpad> _readOnlyWorkpads;
		private readonly PropertyChangedEventArgs _workpadsChangedEventArgs;

		#region Properties
		/// <summary>The list of workpads.</summary>
		public ICollection<Workpad> Workpads { get { return _readOnlyWorkpads; } }
		#endregion Properties


		/// <summary>
		/// Creates a new workpad manager.
		/// </summary>
		public WorkpadManager(Creator<Workpad> createWorkPad, DockPanel dockPanel)
		{
			_createWorkPad = createWorkPad;
		    _dockPanel = dockPanel;
			_workpads = new List<Workpad>();
			_readOnlyWorkpads = _workpads.AsReadOnly();
			_workpadsChangedEventArgs = new PropertyChangedEventArgs("Workpads");
		}

		/// <summary>
		/// Creates a new workpad.
		/// </summary>
		/// <return>the newly created workpad</return>
		public Workpad CreateWorkpad()
		{
			Workpad workpad = _createWorkPad();
			workpad.FormClosed += new FormClosedEventHandler(Workpad_FormClosed);
			workpad.NameChanged += Workpad_NameChanged;

			// Add the workpad and send event
			_workpads.Add(workpad);

			if (WorkpadsChanged != null)
			{
				WorkpadsChanged(this, _workpadsChangedEventArgs);
			}
			return workpad;
		}

		/// <summary>
		/// Gets the workpad with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">the name of the workpad</param>
		/// <returns>the workpad or null if no such workpad exists</returns>
		public Workpad GetWorkpad(string name)
		{
			return Workpads.SingleOrDefault(workpad => workpad.WorkpadName == name);
		}

		/// <summary>
		/// Adds drop down menu items for existing and new workpads to the
		/// given <paramref name="toolStripMenuItem"/>.
		/// </summary>
		/// <param name="toolStripMenuItem">the item to add the drop down items to</param>
		/// <param name="selection">the selection to send to workpads</param>
		/// <param name="excludeWorkpad">the workpad to exclude</param>
		public void AddDropDownMenuItems(ToolStripMenuItem toolStripMenuItem, ISelection selection, Workpad excludeWorkpad)
		{
			// Add the 'new workpad' item to the drop down menu
			toolStripMenuItem.DropDownItems.Add("New Workpad", null, (sender, e) => CopyToNewWorkpad(selection));

			// Create and add items for existing workpads to the drop down menu
			ICollection<Workpad> existingWorkpads = Workpads;
			if (existingWorkpads.Count > 0 && (excludeWorkpad == null || existingWorkpads.Count >= 2))
			{
				var existingWorkpadMenuItem = new ToolStripMenuItem("Existing Workpad");
				toolStripMenuItem.DropDownItems.Add(existingWorkpadMenuItem);

				// Add all workpad that are not in the exclude list
				// We do not want the workpad itself appear in the list of existing workpads
				foreach (Workpad existingWorkpad in existingWorkpads.Where(workpad => workpad != excludeWorkpad))
				{
					existingWorkpadMenuItem.DropDownItems.Add(existingWorkpad.WorkpadName, null, (sender, e) => CopyToExistingWorkpad(((ToolStripMenuItem)sender).Text, selection));
				}
			}
		}

		/// <summary>
		/// Copies the given <paramref name="selection"/> to a new workpad.
		/// </summary>
		/// <param name="selection">the selection to copy</param>
		private void CopyToNewWorkpad(ISelection selection)
		{
			IResultNode[] results = selection.Results;
			if (results != null)
			{
				Workpad workpad = CreateWorkpad();
				if (workpad != null)
				{
                    workpad.Show(_dockPanel, DockState.Document);
					workpad.AddResults(results);
				}
			}
		}

		/// <summary>
		/// Copies the given <paramref name="selection"/> to an existing workpad
		/// with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">the name of the workpad</param>
		/// <param name="selection">the selection to copy</param>
		private void CopyToExistingWorkpad(string name, ISelection selection)
		{
			IResultNode[] results = selection.Results;
			if (results != null)
			{
				Workpad workpad = GetWorkpad(name);
				if (workpad != null)
				{
					workpad.AddResults(results);
				}
			}
		}

		public void CloseAllWorkpads()
		{
			Workpad[] workpads = _workpads.ToArray();
			foreach (Workpad workpad in workpads)
			{
				workpad.Close();
			}
		}

		#region Event handlers
		private void Workpad_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_workpads.Remove(sender as Workpad))
			{
				if (WorkpadsChanged != null)
				{
					WorkpadsChanged(this, _workpadsChangedEventArgs);
				}
			}
		}

		private void Workpad_NameChanged(object sender, EventArgs e)
		{
			if (WorkpadsChanged != null)
			{
				WorkpadsChanged(this, _workpadsChangedEventArgs);
			}
		}
		#endregion Event handlers
	}
}
