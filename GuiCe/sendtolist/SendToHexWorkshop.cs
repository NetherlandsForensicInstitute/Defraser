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
using System.Diagnostics;
using Defraser.Interface;

namespace Defraser.GuiCe.sendtolist
{
	public class SendToHexWorkshop : ISendToPlayer
	{
		private string _path;
        private string _parameters;

		public SendToHexWorkshop(string path, string parameters)
		{
			_path = path;
            _parameters = parameters;
		}

		#region ISendToPlayer Members

		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

        public string Parameters 
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

		public void SendSelectionToApplication(ISelection selection)
		{
			GotoFocusItem(selection.FocusItem);
		}

		public bool IsHexWorkShop()
		{
			return true;
		}

		public ISendToPlayer DeepClone()
		{
			return new SendToHexWorkshop(_path, _parameters);
		}

		#endregion

		/// <summary>
		/// Goes to the object that is under focus in the <see cref="HeaderTree"/> or <see cref="FileTreeObject"/>
		/// </summary>
		/// <param name="focusItem"></param>
		protected void GotoFocusItem(object focusItem)
		{
			IInputFile inputFile;
			IDataBlock dataBlock;
			ICodecStream codecStream;
			IResultNode header;

			if ((inputFile = focusItem as IInputFile) != null)
			{
				GotoOffset(inputFile.Name, 0L);
			}
			if ((dataBlock = focusItem as IDataBlock) != null)
			{
				inputFile = dataBlock.InputFile;
				GotoOffset(inputFile.Name, dataBlock.StartOffset);
			}
			if ((codecStream = focusItem as ICodecStream) != null)
			{
				inputFile = codecStream.InputFile;
				GotoOffset(inputFile.Name, codecStream.AbsoluteStartOffset);
			}
			if ((header = focusItem as IResultNode) != null)
			{
				inputFile = header.InputFile;
				GotoOffset(inputFile.Name, header.StartOffset);
			}
		}

		/// <summary>
		/// Open the file <paramref name="fileName"/> readonly in HexWorkshop,
		/// and execute the command 'goto <paramref name="offset"/>'.
		/// </summary>
		/// <param name="fileName">the file to send to HexWorkshop.</param>
		/// <param name="offset">the offset to go to in HexWorkshop.</param>
		private void GotoOffset(string fileName, long offset)
		{
			Process.Start(_path, string.Format("\"{0}\" /readonly /goto:{1}", fileName, offset));
		}
	}
}
