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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Wraps a <see cref="SaveFileDialog"/> (or other implementation) to export a file to some path
	/// </summary>
	internal class SaveFilePresenter
	{
		private string _filter;
        private int _filterIndex;
		private string _initialDirectory;
		private string _title;

		#region Properties
		public string Title
		{
			set { _title = value; }
		}

		public string Filter
		{
			set { _filter = value; }
		}

        public int FilterIndex
        {
            set { _filterIndex = value; }
        }

		public static bool ExportForensicIntegrityLog
		{
			get
			{
				return Properties.Settings.Default.ExportAlsoForensicIntegrityLog;
			}
		}

		public string InitialDirectory
		{
			set { _initialDirectory = value; }
		}
		#endregion Properties

		public void FilterAllFiles()
		{
			_filter = "All Files (*.*)|*.*";
		}

		/// <summary>
		/// Shows a dialog to the user, in which a file needs to be selected.
		/// </summary>
		/// <param name="owner">The owning window</param>
		/// <param name="selectedExistingWritableFile">The valid path, if <c>true</c> is returned</param>
		/// <returns>If the user provided a valid path, and choose to continue.</returns>
		public bool ShowDialog(IWin32Window owner, out FileInfo selectedExistingWritableFile)
		{
			using (var dialog = new SaveFileDialog())
			{
				if(!String.IsNullOrEmpty(_title))
					dialog.Title = _title;
				if(!String.IsNullOrEmpty(_filter))
					dialog.Filter = _filter;
                if (_filterIndex != null)
                    dialog.FilterIndex = _filterIndex;
				if (!String.IsNullOrEmpty(_initialDirectory))
					dialog.InitialDirectory = _initialDirectory;
				return DialogResult.OK == PresentOnDialog(dialog, owner, out selectedExistingWritableFile);
			}
		}

		private static DialogResult PresentOnDialog(FileDialog dialog, IWin32Window owner, out FileInfo result)
		{
			var answer = dialog.ShowDialog(owner);
			if (answer != DialogResult.OK)
			{
				result = null;
				return answer;
			}
			if (String.IsNullOrEmpty(dialog.FileName))
			{
				if (AskForRetry("No filename has been provided. Please review your choice."))
				{
					return PresentOnDialog(dialog, owner, out result);
				}
				result = null;
				return answer;
			}
			if (File.Exists(dialog.FileName) && new FileInfo(dialog.FileName).IsReadOnly)
			{
				if (AskForRetry("The provided file is readonly. Please review your choice."))
				{
					return PresentOnDialog(dialog, owner, out result);
				}
				result = null;
				return answer;
			}
			result = new FileInfo(dialog.FileName);
			return answer;
		}

		/// <summary>
		/// Asks the user whether it wants to correct its provided path, after telling him/her it is invalid because of <see cref="text"/>
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private static bool AskForRetry(string text)
		{
			return (DialogResult.OK == MessageBox.Show(text, "Invalid choice", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1));
		}

		public void FilterImages()
		{
			_filter = "JPEG-Image (*.jpg)|*.jpg|PNG-Image (*.png)|*.png|Windows BMP-Image (*.bmp)|*.bmp";
		}

        public void FilterImagesIndex()
        {
            _filterIndex = 2;
        }
        
		public void FilterXml()
		{
			_filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
		}

		public void FilterProjectFiles()
		{
			_filter = "Project Files (*.dpr)|*.dpr|All Files (*.*)|*.*";
		}
	}
}
