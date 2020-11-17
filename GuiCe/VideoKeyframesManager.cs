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
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
    public class VideoKeyframesManager : IFFmpegCallback
    {
    	private const int DefaultMaxThumbs = 50;
        
        private readonly FFmpegManager _ffmpegManager;
        private readonly Creator<VideoKeyframesWindow> _videoThumbsWindowCreator;
        private HeaderTree _headerTree;
        private VideoKeyframesWindow _videoKeyframesWindow;
    	private int _maxThumbs = DefaultMaxThumbs;
    	private IResultNode[] _currentThumbSource;

        public VideoKeyframesManager(FFmpegManager ffmpegManager, ProjectManager projectManager,
                                  DefaultCodecHeaderManager defaultCodecHeaderManager, Creator<VideoKeyframesWindow> videoThumbsWindowCreator)
        {
            _ffmpegManager = ffmpegManager;
            _videoThumbsWindowCreator = videoThumbsWindowCreator;

			projectManager.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(ProjectChanged);
			defaultCodecHeaderManager.CodecHeaderChanged += new DefaultCodecHeaderManager.DefaultCodecHeaderChangeHandler(defaultCodecHeaderManager_CodecHeaderChanged);
		}

    	#region Properties
		public int MaxThumbs
    	{
			get { return _maxThumbs; }
    		set
    		{
    			_maxThumbs = value;
				ReloadAllThumbs();
    		}
		}

    	public HeaderTree KeyframeSourceTree
    	{
    		set
    		{
    			_headerTree = value;
				if(_videoKeyframesWindow != null)
				{
					_videoKeyframesWindow.KeyframeSourceTree = _headerTree;
				}
    		}
			get { return _headerTree; }
    	}
		#endregion Properties

		public VideoKeyframesWindow GetThumbsWindow()
        {
            if (_videoKeyframesWindow == null)
            {
                _videoKeyframesWindow = _videoThumbsWindowCreator();
                _videoKeyframesWindow.FormClosed += new FormClosedEventHandler(ThumbsFormClosed);
                _videoKeyframesWindow.KeyframeSourceTree = _headerTree;
            }
            return _videoKeyframesWindow;
        }

        public void ClearWindow()
        {
            if(_videoKeyframesWindow != null)
            {
                _videoKeyframesWindow.RemoveAllThumbs();
            }
        }

        public void CloseWindow()
        {
			if (_videoKeyframesWindow != null)
			{
				_videoKeyframesWindow.Close();
			}
        }

        public bool IsOpen()
        {
            return (_videoKeyframesWindow != null);
        }

		public void ReloadAllThumbs()
		{
			if(_currentThumbSource == null) return;
			DisplayIFramesAsThumbs(_currentThumbSource);
		}

        public void DisplayIFramesAsThumbs(IResultNode[] resultNodes)
        {
			_currentThumbSource = resultNodes;
            if (_videoKeyframesWindow == null) return;
            _ffmpegManager.NewQueue();

        	List<IResultNode> allKeyframes = ThumbUtil.GetAllKeyFrames(resultNodes);
        	int allKeyframesCount = allKeyframes.Count;
			List<IResultNode> keyFrames = ThumbUtil.CheckMaxThumbCount(allKeyframes, _maxThumbs);
            if(keyFrames.Count > 0)
            {
				_videoKeyframesWindow.NewConvertBatch(keyFrames.Count, allKeyframesCount);

                foreach (IResultNode frame in keyFrames)
                {
                    _ffmpegManager.AddToConvertQueue(frame, this);
                }
            }
        }

        public void FFmpegDataConverted(FFmpegResult ffmpegResult)
        {
            if(_videoKeyframesWindow == null) return;
			_videoKeyframesWindow.AddThumb(ffmpegResult);
        }

        #region Event handlers
        private void ProjectChanged(object sender, ProjectChangedEventArgs e)
        {
            switch (e.Type)
            {
                case ProjectChangedType.FileDeleted:
				case ProjectChangedType.Closed:
                    ClearWindow();
                    break;
            }
        }

        private void ThumbsFormClosed(object sender, FormClosedEventArgs e)
        {
            _videoKeyframesWindow.FormClosed -= new FormClosedEventHandler(ThumbsFormClosed);
            _videoKeyframesWindow = null;
        }

		private void defaultCodecHeaderManager_CodecHeaderChanged(CodecID changedCodecId, EventArgs e)
		{
			if (_currentThumbSource != null && _currentThumbSource.Length > 0 && _currentThumbSource[0].DataFormat == changedCodecId)
			{
				ReloadAllThumbs();
			}
		}
        #endregion Event handlers
    }
}
