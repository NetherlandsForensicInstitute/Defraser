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
using System.Drawing;
using System.Windows.Forms;
using Defraser.FFmpegConverter;

namespace Defraser.GuiCe
{
    public partial class ThumbControl : Control, IBitmapThumbTarget
    {
    	private IThumbsControlContainer _thumbsControlContainer;
        private FFmpegResult _thumbResult;
        private Bitmap _bitmap;

    	private bool _hasMouseFocus;
    	private bool _isSelected;

        #region Properties
        public bool IsSelected
        {
            get { return _isSelected; }
        }

        public bool HasMouseFocus
        {
            get { return _hasMouseFocus; }
        }
        #endregion Properties

        public ThumbControl() :
            this(null, null, ThumbUtil.ThumbWidth, ThumbUtil.ThumbHeight)
		{
			
		}

		public ThumbControl(IThumbsControlContainer thumbsControlContainer, FFmpegResult thumbResult) :
            this(thumbsControlContainer, thumbResult, ThumbUtil.ThumbWidth, ThumbUtil.ThumbHeight)
        {
			InitializeComponent();
        }

        public ThumbControl(IThumbsControlContainer thumbsControlContainer, FFmpegResult thumbResult, int width, int height)
        {
			Width = width;
			Height = height;

			InitializeComponent();
        	UpdateContainerAndResult(thumbsControlContainer, thumbResult);
		}

		public void UpdateContainerAndResult(IThumbsControlContainer thumbsControlContainer, FFmpegResult thumbResult)
		{
			_thumbsControlContainer = thumbsControlContainer;
			_thumbResult = thumbResult;
			if (thumbResult != null)
			{
				_bitmap = (thumbResult.Bitmap != null) ? ThumbUtil.ResizeBitmap(thumbResult.Bitmap, Width, Height) : ThumbUtil.GetErrorBitmap();
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(Color.White);

			if(_bitmap != null) g.DrawImage(_bitmap, 0, 0);
		    ThumbUtil.DrawBitmapRect(g, this, _thumbResult);

			base.OnPaint(e);
		}

		public void UpdateSelectedState(Object selectedItem)
		{
			bool newSelectState = (selectedItem != null && _thumbResult != null && selectedItem == _thumbResult.SourcePacket);

			if (newSelectState != _isSelected)
			{
				_isSelected = newSelectState;

				if (newSelectState)
				{
					if (_thumbsControlContainer != null)
					{
						_thumbsControlContainer.ThumbSelected(this);
					}
				}
				Invalidate();
			}
		}

		public void SelectThumb()
		{
			if (_thumbsControlContainer != null && _thumbResult != null)
			{
                _thumbsControlContainer.SelectPacketInTree(_thumbResult.SourcePacket, (Parent != null) ? Parent.Parent : null);
			}
		}

		#region EventHandlers
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			_hasMouseFocus = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			_hasMouseFocus = false;
			Invalidate();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
            Select(); // Give focus
			SelectThumb();

			if (_thumbsControlContainer != null && _thumbResult != null && _thumbResult.Bitmap == null)
			{
                _thumbsControlContainer.AskChangeDefaultCodecHeader(_thumbResult);
			}
		}
		#endregion EventHandlers
    }
}
