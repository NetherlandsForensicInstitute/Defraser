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
using System.Drawing;
using System.Drawing.Imaging;
using Defraser.DataStructures;
using Defraser.FFmpegConverter;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public class ThumbUtil
	{
	    public const int ThumbWidth = 100;
	    public const int ThumbHeight = 75;

	    public static readonly Color ColorBitmapMouseOver = Color.Black;
        public static readonly Color ColorBitmapSelected = Color.Red;
        public static readonly Color ColorBitmapUsingHeaderSource = Color.Yellow;
        public static readonly Color ColorBitmapDefault = Color.LightGray;

	    public static List<IResultNode> GetAllKeyFrames(IResultNode[] resultNodes)
		{
			List<IResultNode> keyFrames = new List<IResultNode>();

			IResultNode node;
			for (int i = 0; i < resultNodes.Length; i++)
			{
				node = resultNodes[i];
				if (node.IsKeyframe()) keyFrames.Add(node);

				if (node.Children.Count > 0)
				{
					IResultNode[] childNodes = new IResultNode[node.Children.Count];
					node.Children.CopyTo(childNodes, 0);
					keyFrames.InsertRange(keyFrames.Count, GetAllKeyFrames(childNodes));
				}
			}
			return keyFrames;
		}

		public static List<IResultNode> CheckMaxThumbCount(List<IResultNode> keyFrames, int thumbLimit)
		{
			if(keyFrames.Count<=thumbLimit)
			{
				return keyFrames;
			}
			if(thumbLimit==0)
			{
				return new List<IResultNode>();
			}
			var resultNodes = new List<IResultNode>(thumbLimit);
			double stepRatioOriginalToNew = (keyFrames.Count - 1)/(double) (thumbLimit - 1);
			int steps = 0;
			for (double originalIndex = 0; steps < thumbLimit;originalIndex+=stepRatioOriginalToNew,steps++)
			{
				resultNodes.Add(keyFrames[(int)Math.Round(originalIndex)]);
			}
			return resultNodes;
		}

        public static void DrawBitmapRect(Graphics graphics, IBitmapThumbTarget bitmapThumbTarget, FFmpegResult ffmpegResult)
        {
            DrawBitmapRect(graphics, bitmapThumbTarget, ffmpegResult, 0, 0);
        }

        public static void DrawBitmapRect(Graphics graphics, IBitmapThumbTarget bitmapThumbTarget, FFmpegResult ffmpegResult, int x, int y)
        {
            Color color;
            if (bitmapThumbTarget.HasMouseFocus)
                color = ColorBitmapMouseOver;
            else if (bitmapThumbTarget.IsSelected)
                color = ColorBitmapSelected;
            else if (ffmpegResult != null && ffmpegResult.IsUsingCustomHeaderSource())
                color = ColorBitmapUsingHeaderSource;
            else
                color = ColorBitmapDefault;
            var pen = new Pen(color);
            graphics.DrawRectangle(pen, x+1, y+1, bitmapThumbTarget.Width - 3, bitmapThumbTarget.Height - 3);
            graphics.DrawRectangle(pen, x, y, bitmapThumbTarget.Width - 1, bitmapThumbTarget.Height - 1);
            pen.Dispose();
        }

        public static Bitmap GetErrorBitmap()
        {
            return ResizeBitmap(Properties.Resources.decoding_failed, ThumbWidth, ThumbHeight);
        }

        public static Bitmap GetEmptyBitmap()
        {
            return new Bitmap(ThumbWidth, ThumbHeight);
        }

	    public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
        {
            // When PixelFormat == PixelFormat.Undefined, the bitmap is probably disposed.
            if (bitmap == null || bitmap.PixelFormat == PixelFormat.Undefined) return null;
            Bitmap outBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(outBitmap))
            {
                g.DrawImage(bitmap, 0, 0, width, height);
            }
            return outBitmap;
        }
	}
}
