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
using System.Diagnostics;
using System.IO;
using Defraser.Interface;

namespace Defraser.GuiCe.sendtolist
{
	public class SendToPlayer : ISendToPlayer
	{
		private readonly IFormFactory _formFactory;
		private string _path;
        private string _parameters;

		#region Properties

		/// <summary>The path of the application executable.</summary>
		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

        /// <summary>The parameters sent to the application executable.</summary>
        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

		#endregion Properties

		/// <summary>
		/// Creates a new send-to item.
		/// </summary>
		public SendToPlayer(string applicationExecutablePath, string applicationParameters, IFormFactory formFactory)
		{
			_path = applicationExecutablePath;
            _parameters = applicationParameters;
			_formFactory = formFactory;
		}

		#region ISendToPlayer Members

		public void SendSelectionToApplication(ISelection selection)
		{
			if (string.IsNullOrEmpty(_path) || !File.Exists(_path) || selection == null || selection.IsEmpty)
			{
				return;
			}
			IInputFile inputFile;
			if (selection.Results != null && selection.Results.Length == 1 && (inputFile = selection.Results[0] as IInputFile) != null)
			{
				Process.Start(_path, string.Format("\"{0}\"", inputFile.Name));
				return;
			}

			string tmpfile = TempFile.GetTempFileName();
			// Use the detector info of the first result to determine the file extension
			if (selection.OutputFileExtension != null)
			{
				tmpfile = System.IO.Path.ChangeExtension(tmpfile, selection.OutputFileExtension);
			}

			var sendToProgressReporter = _formFactory.Create<SendToProgressReporter>();
			sendToProgressReporter.SendToAsync(selection, _path, _parameters, tmpfile);
		}

		public virtual bool IsHexWorkShop()
		{
			return false;
		}

		public ISendToPlayer DeepClone()
		{
			return new SendToPlayer(_path,_parameters, _formFactory);
		}

		#endregion
	}
}
