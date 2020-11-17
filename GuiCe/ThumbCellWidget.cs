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
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
    public class ThumbCellWidget : CellWidget, IBitmapThumbTarget
    {
        private Bitmap _bitmap;
        private bool _hasMouseFocus;

        #region Properties
        public int Width
        {
            get { return ThumbUtil.ThumbWidth; }
        }

        public int Height
        {
            get { return ThumbUtil.ThumbHeight; }
        }

        public bool IsSelected
        {
            get; set;
        }

        public bool HasMouseFocus
        {
            get { return _hasMouseFocus; }
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        public FFmpegResult CellFFmpegResult
        {
			get { return (CellData != null) ? CellData.Value as FFmpegResult : null; }
        }
        #endregion Properties

        public ThumbCellWidget(RowWidget rowWidget, Column column)
            : base(rowWidget, column)
        {
            
        }

        public override void UpdateData()
        {
            base.UpdateData();
            ReloadBitmap();
        }

        private void ReloadBitmap()
        {
            FFmpegResult result = CellFFmpegResult;
            if (_bitmap != null) _bitmap.Dispose();
            if (result != null)
            {
                _bitmap = ThumbUtil.ResizeBitmap(result.Bitmap, ThumbUtil.ThumbWidth, ThumbUtil.ThumbHeight) ?? ThumbUtil.GetErrorBitmap();
            }
        }

        public override void OnPaint(Graphics graphics)
        {
            base.OnPaint(graphics);

            if(_bitmap == null && CellData.Value != null)
            {
                ReloadBitmap();
            }

            int thumbX = Bounds.X + ((Bounds.Width / 2) - (ThumbUtil.ThumbWidth / 2));
            int thumbY = Bounds.Y + ((Bounds.Height / 2) - (ThumbUtil.ThumbHeight / 2));
            if(_bitmap != null)
            {
                graphics.DrawImage(_bitmap, thumbX, thumbY);
            }
            else
            {
                var brsh = new SolidBrush(Color.White);
                graphics.FillRectangle(brsh, new Rectangle(thumbX, thumbY, Width, Height));
                brsh.Dispose();

            }
			ThumbUtil.DrawBitmapRect(graphics, this, CellFFmpegResult, thumbX, thumbY);
        }

        public override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _hasMouseFocus = true;
            Invalidate();
        }

        public override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hasMouseFocus = false;
            Invalidate();
        }

        // Disable 'System.Drawing.Bitmap' tooltip (is the datafield objecttype).
        protected override string GetToolTipText()
        {
            return string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }
}
