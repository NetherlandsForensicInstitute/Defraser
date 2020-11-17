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
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public partial class VideoKeyframesWindow : ToolWindow, IThumbsControlContainer
    {
		private readonly MainForm _mainForm;
		private VideoKeyframesManager _videoKeyframesManager;
	    private readonly DefaultCodecHeaderManager _defaultCodecHeaderManager;

    	private HeaderTree _keyframeSourceTree;
    	private int _numberOfThumbsComplete;
    	private int _totalNumberOfThumbs;
		private int _allTotalNumberOfThumbs;
        
        #region Properties
        public HeaderTree KeyframeSourceTree
        { 
            set
            {
                if (_keyframeSourceTree != null)
                {
                    _keyframeSourceTree.SelectionChanged -= new EventHandler(headerTree_SelectionChanged);
                }
                _keyframeSourceTree = value;

                if (_keyframeSourceTree != null)
                {
                    ReportNewSelectionToAllThumbs();
                    _keyframeSourceTree.SelectionChanged += new EventHandler(headerTree_SelectionChanged);
                }
            }
        }
        #endregion Properties
        
        public VideoKeyframesWindow(MainForm mainForm, VideoKeyframesManager videoKeyframesManager, 
									DefaultCodecHeaderManager defaultCodecHeaderManager)
		{
        	_mainForm = mainForm;
			_videoKeyframesManager = videoKeyframesManager;
		    _defaultCodecHeaderManager = defaultCodecHeaderManager;
			InitializeComponent();
			UpdateTitle();

			textBoxMaxNumThumbs.Text = _videoKeyframesManager.MaxThumbs.ToString();
        }

        private void ResetValuesToDefault()
        {
            _totalNumberOfThumbs = 0;
            _numberOfThumbsComplete = 0;
            _totalNumberOfThumbs = 0;
            _allTotalNumberOfThumbs = 0;
        }
        
        private void UpdateTitle()
		{
			Text = "Video Keyframes";

			if (_totalNumberOfThumbs > 0 && _numberOfThumbsComplete < _totalNumberOfThumbs)
			{
				int percent = (int)(((float)_numberOfThumbsComplete / (float)_totalNumberOfThumbs) * 100.0);
				Text += string.Format(" {0}%", percent);
			}
			else
			{
				Text += string.Format(" - Displaying {0} of {1} keyframe", _totalNumberOfThumbs, _allTotalNumberOfThumbs);
				if (_totalNumberOfThumbs != 1) Text += "s";
			}
		}

		public void AddThumb(FFmpegResult ffmpegResult)
        {
            if (IsDisposed || thumbsContainer.IsDisposed) return;

            // Execute Add method in Thread context of the Form.
            this.Invoke((MethodInvoker) delegate
            {
                ThumbControl thumbControl = new ThumbControl(this, ffmpegResult);
                thumbsContainer.Controls.Add(thumbControl);
            	UpdateProgress();
            });
        }
        
    	public void RemoveAllThumbs()
        {
            thumbsContainer.Controls.Clear();
            thumbsContainer.ActiveThumbControlIndex = -1;
    	    ResetValuesToDefault();
    	    UpdateTitle();
		}

		public void NewConvertBatch(int limitedCount, int unlimitedCount)
		{
			RemoveAllThumbs();

			_numberOfThumbsComplete = 0;
			_totalNumberOfThumbs = limitedCount;
			_allTotalNumberOfThumbs = unlimitedCount;
			UpdateTitle();
		}

		private void UpdateProgress()
		{
			_numberOfThumbsComplete++;
			UpdateTitle();
		}

		public void SelectPacketInTree(IResultNode resultNode, Control parentControl)
		{
			if(_keyframeSourceTree != null)
			{
				_keyframeSourceTree.SelectedItem = resultNode;
			}
		}

		public void ThumbSelected(ThumbControl videoThumbControl)
		{
			thumbsContainer.ScrollControlIntoView(videoThumbControl);
			thumbsContainer.ActiveThumbControlIndex = thumbsContainer.Controls.GetChildIndex(videoThumbControl);
		}

        public void AskChangeDefaultCodecHeader(FFmpegResult ffmpegResult)
		{
			// Execute the messagebox in Thread context of the Form.
			// Otherwise the messagebox is not a child of the program.
        	this.Invoke((MethodInvoker) delegate
        	{
        		_defaultCodecHeaderManager.AskChangeDefaultCodecHeader(ffmpegResult, _mainForm);
        	});
		}

		private void ApplyMaxThumbsCount()
		{
			if (_videoKeyframesManager == null || textBoxMaxNumThumbs.Text == string.Empty) return;

			int thumbCount = Int32.Parse(textBoxMaxNumThumbs.Text);
			if (thumbCount > 0)
			{
				buttonApplyMaxThumbs.Focus();
				_videoKeyframesManager.MaxThumbs = thumbCount;
			}
		}

        private void ReportNewSelectionToAllThumbs()
        {
            if (_keyframeSourceTree == null) return;
            foreach (ThumbControl thumbControl in thumbsContainer.Controls)
            {
                thumbControl.UpdateSelectedState(_keyframeSourceTree.SelectedItem);
            }
        }

		#region EventHandlers
		private void headerTree_SelectionChanged(object sender, EventArgs e)
		{
		    ReportNewSelectionToAllThumbs();
		}

	    private void textBoxMaxNumThumbs_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == 8) return; // allow backspace
			
			if((Keys)e.KeyChar == Keys.Enter)
			{
				ApplyMaxThumbsCount();
			}
			else
			{
				// Only allow numeric input
				int isNumber;
				e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
			}
		}

		private void buttonApplyMaxThumbs_Click(object sender, EventArgs e)
		{
			ApplyMaxThumbsCount();
		}
    	#endregion EventHandlers
    }
}
