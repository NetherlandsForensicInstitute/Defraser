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

using System.Drawing;

namespace Defraser.GuiCe.sourceinfo
{
	internal class RichtTextPainter : IRichTextView
	{
		private readonly Graphics _g;
		private RectangleF _paintingArea;
		private readonly Color _defaultColor;
		private PointF _paintingOffset;

		public RichtTextPainter(Graphics g, RectangleF paintingArea, Color defaultColor)
		{
			_g = g;
			_paintingArea = paintingArea;
			_defaultColor = defaultColor;
			_paintingOffset = paintingArea.Location;
		}

		private float GetLineSpacing(Font f)
		{
			return f.GetHeight(_g);
		}

		public void Paint(string text, Font font)
		{
			Paint(text, font, _defaultColor);
		}

		public void Paint(string text, Font font, Color color)
		{
			var remainingSizeOnLine = new SizeF(_paintingArea.Right - _paintingOffset.X, GetLineSpacing(font));
			int chars;
			int lines;
			var sizeLinePart = _g.MeasureString(text, font, remainingSizeOnLine, StringFormat.GenericDefault, out chars, out lines); //Unfortunately, sometimes (or always?) sizeLinePart.Width > remainingSizeOnLine.Width
			if (sizeLinePart.Width > remainingSizeOnLine.Width) //Correct for unexpected behaviour of Graphics.MeasureString
			{
				chars = (int)(chars * (remainingSizeOnLine.Width / sizeLinePart.Width)); //Thus chars will be < text.Length
				sizeLinePart = new SizeF(remainingSizeOnLine.Width, GetLineSpacing(font));
			}
			if (chars > 0)
			{
				//Draw the string, which unfortunately does not take care of wrapping in the painting region (_paintingarea), so only paint the ones that fit.
				_g.DrawString(text.Substring(0, chars), font, new SolidBrush(color), _paintingOffset);
			}
			if (chars < text.Length)
			{
				//Not all characters fitted, start new line, and continue:
				MoveCarretToNewLine(font);
				Paint(text.Substring(chars), font, color);
			}
			else
			{
				//Everything fitted, move offset to end of printed chars
				_paintingOffset = new PointF(_paintingOffset.X + sizeLinePart.Width, _paintingOffset.Y);
			}
		}

		public void PaintLine(string text, Font font)
		{
			PaintLine(text, font, _defaultColor);
		}

		public void PaintLine(string text, Font font, Color color)
		{
			//we are halfway the line, so continue with the offsetted paint:
			Paint(text, font, color);
			//But, add the newline:
			if (_paintingOffset.X > _paintingArea.X)
			{
				MoveCarretToNewLine(font);
			}
			// I would like to do the following if _paintingOffset.x<=_paintingArea.x, but sizeLine.Height is bigger than the number of printed lines * _lineSpacing.
			/*
				var sizeLine = _g.MeasureString(text, font, _paintingArea.Size);
				_g.DrawString(text, font, new SolidBrush(color), _paintingArea); //The rectangle version does auto wrapping
				MoveAreaDown(sizeLine.Height);
				 */
		}

		private void MoveCarretToNewLine(Font font)
		{
			MoveAreaDown(GetLineSpacing(font));
		}

		private void MoveAreaDown(float filledHeight)
		{
			_paintingArea = new RectangleF(_paintingArea.Left, _paintingArea.Top + filledHeight, _paintingArea.Width, _paintingArea.Height - filledHeight);
			_paintingOffset = _paintingArea.Location;
		}
	}
}
