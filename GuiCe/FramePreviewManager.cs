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
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public class FramePreviewManager : IFFmpegCallback
	{
		private FramePreviewWindow _previewWindow;
		private readonly FFmpegManager _ffmpegManager;
		private readonly IForensicIntegrityLog _forensicIntegrityLog;

		private IResultNode _activeFrameSourceNode;

		public FramePreviewManager(ProjectManager projectManager, FFmpegManager ffmpegManager,
								   DefaultCodecHeaderManager defaultCodecHeaderManager,
								   IForensicIntegrityLog forensicIntegrityLog)
        {
			_ffmpegManager = ffmpegManager;
			_forensicIntegrityLog = forensicIntegrityLog;

            projectManager.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(ProjectChanged);
			defaultCodecHeaderManager.CodecHeaderChanged += new DefaultCodecHeaderManager.DefaultCodecHeaderChangeHandler(defaultCodecHeaderManager_CodecHeaderChanged);
        }

		public void AddNewFFmpegConvertItem(IResultNode resultNode)
		{
			if (_previewWindow == null) return;
			_ffmpegManager.AddToConvertQueue(resultNode, this, true);
		}

		public FramePreviewWindow GetPreviewWindow()
		{
			if (_previewWindow == null)
			{
				_previewWindow = new FramePreviewWindow(_forensicIntegrityLog);
				_previewWindow.FormClosed += new FormClosedEventHandler(PreviewFormClosed);
			}
			return _previewWindow;
		}

		public void ReloadFramePreview()
		{
			if (_previewWindow != null && _activeFrameSourceNode != null)
			{
				AddNewFFmpegConvertItem(_activeFrameSourceNode);
			}
		}

        public void ClearWindow()
        {
            if (_previewWindow != null)
            {
                _previewWindow.UpdateResult(null);
                _activeFrameSourceNode = null;
            }
        }

		public void CloseWindow()
		{
			if (_previewWindow != null)
			{
				_previewWindow.Close();
			}
		}

        public bool IsOpen()
		{
			return (_previewWindow != null);
		}

		public void FFmpegDataConverted(FFmpegResult ffmpegResult)
		{
			if (_previewWindow != null)
			{
				_previewWindow.UpdateResult(ffmpegResult);
				_activeFrameSourceNode = ffmpegResult.SourcePacket;
			}
		}

        #region Event handlers
        private void ProjectChanged(object sender, ProjectChangedEventArgs e)
        {
            switch (e.Type)
            {
                case ProjectChangedType.FileDeleted:
				case ProjectChangedType.Closed:
                    if (_previewWindow != null)
                    {
                    	ClearWindow();
                    }
                    break;
            }
        }

        private void PreviewFormClosed(object sender, FormClosedEventArgs e)
        {
            _previewWindow.FormClosed -= new FormClosedEventHandler(PreviewFormClosed);
            _previewWindow = null;
        	_activeFrameSourceNode = null;
        }

		private void defaultCodecHeaderManager_CodecHeaderChanged(CodecID changedCodecId, EventArgs e)
		{
			if (_activeFrameSourceNode != null && _activeFrameSourceNode.DataFormat == changedCodecId)
			{
				ReloadFramePreview();
			}
		}
        #endregion Event handlers
	}
}
