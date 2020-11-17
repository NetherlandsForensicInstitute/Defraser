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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Defraser.GuiCe.sourceinfo
{
	internal class MeasuringRichTextView : IRichTextView
	{
		private readonly Graphics _g;
		private readonly Stack<SizeF> _blocks = new Stack<SizeF>();
		private readonly Stack<SizeF> _lines = new Stack<SizeF>();

		public MeasuringRichTextView(Graphics g)
		{
			_g = g;
		}

		private static SizeF Sideways(IEnumerable<SizeF> sizeSegments)
		{
			return new SizeF(sizeSegments.Sum(part => part.Width), sizeSegments.Max(part => part.Height));
		}

		public SizeF Size()
		{
			if(_blocks.Count>0)
			{
				MoveToNewLine();
			}
			return new SizeF(_lines.Max(b => b.Width),_lines.Sum(b => b.Height));
		}

		public void PaintLine(string text, Font font)
		{
			_blocks.Push(_g.MeasureString(text,font));
			MoveToNewLine();
		}

		private void MoveToNewLine()
		{
			_lines.Push(Sideways(_blocks));
			_blocks.Clear();
		}

		public void PaintLine(string text, Font font, Color color)
		{
			PaintLine(text, font);
		}

		public void Paint(string text, Font font)
		{
			_blocks.Push(_g.MeasureString(text, font));
		}

		public void Paint(string text, Font font, Color color)
		{
			Paint(text, font);
		}
	}
}
