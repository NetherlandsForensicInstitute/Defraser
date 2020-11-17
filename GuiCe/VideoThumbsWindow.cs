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
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
    public partial class VideoThumbsWindow : ToolWindow
    {
        private const int OffsetX = 8;
        private const int OffsetY = 7;

    	private int _numberOfThumbsComplete;
    	private int _totalNumberOfThumbs;

		public VideoThumbsWindow()
        {
            InitializeComponent();
        }

		private void UpdateTitle()
		{
			Text = "Video Keyframes";

			if (_totalNumberOfThumbs > 0 && _numberOfThumbsComplete < _totalNumberOfThumbs)
			{
				int percent = (int)(((float)_numberOfThumbsComplete / (float)_totalNumberOfThumbs) * 100.0);
				Text += string.Format(" {0}%", percent);
			}
		}

		public void AddThumb(Bitmap bitmap, HeaderTree headerTree, IDataPacket sourceNode)
        {
            // TODO: why is this property not true when the is disposed?
            if (this.IsDisposed || thumbsContainer.IsDisposed) return;

            // Execute Add method in Thread context of the Form.
            this.Invoke((MethodInvoker) delegate
            {
				VideoThumbControl thumbControl = new VideoThumbControl(bitmap, headerTree, sourceNode);
				thumbControl.Selected += new VideoThumbControl.SelectEventHandler(thumbControl_Selected);
                thumbsContainer.Controls.Add(thumbControl);
            	UpdateProgress();
            });
        }
        
    	public void RemoveAllThumbs()
        {
    		IEnumerator enumerator = thumbsContainer.Controls.GetEnumerator();
			while(enumerator.MoveNext())
			{
				VideoThumbControl thumbControl = enumerator.Current as VideoThumbControl;
				if(thumbControl != null) thumbControl.Selected -= new VideoThumbControl.SelectEventHandler(thumbControl_Selected);
			}
            thumbsContainer.Controls.Clear();
            thumbsContainer.ActiveThumbControlIndex = -1;
		}

		public void NewConvertBatch(int count)
		{
			RemoveAllThumbs();

			_numberOfThumbsComplete = 0;
			_totalNumberOfThumbs = count;
			UpdateTitle();
		}

		private void UpdateProgress()
		{
			_numberOfThumbsComplete++;
			UpdateTitle();
		}

		#region EventHandlers
		private void windowResizeHandler(object sender, EventArgs e)
		{
			thumbsContainer.Width = Width - OffsetX;
			thumbsContainer.Height = Height - OffsetY;
		}

		private void thumbControl_Selected(object sender, EventArgs e)
		{
            thumbsContainer.ActiveThumbControlIndex = thumbsContainer.Controls.GetChildIndex((VideoThumbControl)sender);
		}
        #endregion EventHandlers
    }
}
