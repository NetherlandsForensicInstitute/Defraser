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
using Defraser.Interface;

namespace Defraser.GuiCe
{
    public partial class VideoThumbControl : Control
    {
        private readonly Bitmap _bitmap;
    	private readonly HeaderTree _headerTree;
		private readonly IDataPacket _sourceResultNode;

    	private bool _hasMouseFocus;
    	private bool _isSelected;

		public delegate void SelectEventHandler(object sender, EventArgs e);

    	public event SelectEventHandler Selected;

		public VideoThumbControl(Bitmap bitmap, HeaderTree headerTree, IDataPacket sourceResultNode) :
			this(bitmap, 100, 75, headerTree, sourceResultNode)
        {
            
        }

		public VideoThumbControl(Bitmap bitmap, int width, int height, HeaderTree headerTree, IDataPacket sourceResultNode)
        {
			_bitmap = ResizeBitmap(bitmap, width, height);
			_headerTree = headerTree;
			_sourceResultNode = sourceResultNode;
            InitializeComponent();

            Width = width;
            Height = height;

			if(_headerTree != null)
			{
				_headerTree.SelectionChanged += new EventHandler(headerTree_SelectionChanged);
				UpdateSelectedState();
			}
        }

    	protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if (_bitmap != null)
            {
                try
                {
                    g.DrawImage(_bitmap, 0, 0);
                }
                catch (Exception)
                {
                    Console.WriteLine("Thumb draw failed.");
                }
            }

            try
            {
            	Color color;
				if(_hasMouseFocus)
					color = Color.Black;
				else if(_isSelected)
					color = Color.Red;
				else
					color = Color.LightGray;

                g.DrawRectangle(new Pen(color), 1, 1, Width - 3, Height - 3);
				g.DrawRectangle(new Pen(color), 0, 0, Width - 1, Height - 1);
            }catch(Exception)
            {
                Console.WriteLine("Can not draw bitmap rect.");
            }
            base.OnPaint(e);
        }

		private void UpdateSelectedState()
		{
			bool newSelectState = (_headerTree.SelectedItem == _sourceResultNode);

			if (newSelectState != _isSelected)
			{
				_isSelected = newSelectState;

				if (newSelectState)
				{
					Panel panel = Parent as Panel;
					if (panel != null) panel.ScrollControlIntoView(this);
					if(Selected != null) Selected(this, EventArgs.Empty);
				}
				Invalidate();
			}
		}

		public void SelectThumb()
		{
			if (_headerTree != null)
			{
				_headerTree.SelectedItem = _sourceResultNode;
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
			SelectThumb();
		}

		private void headerTree_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelectedState();
		}
		#endregion EventHandlers

		public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
		{
			Bitmap outBitmap = new Bitmap(width, height);
			if (bitmap != null)
			{
				using (Graphics g = Graphics.FromImage(outBitmap))
				{
					g.DrawImage(bitmap, 0, 0, width, height);
				}	
			}
			return outBitmap;
		}
    }
}
