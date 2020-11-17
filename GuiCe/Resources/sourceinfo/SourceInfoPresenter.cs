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

namespace Defraser.GuiCe.sourceinfo
{
	internal class SourceInfoPresenter : ISourceInfoCell{

		private readonly Font _font;
		private readonly bool _useHex;
		private readonly Font _fontB;
		private readonly IRichTextView _view;

		public SourceInfoPresenter(Font font, bool useHexRepresentation,IRichTextView view)
		{
			_font = font;
			_useHex = useHexRepresentation;
			_fontB = new Font(font, FontStyle.Bold);
			_view = view;
		}

		public void ShowFile(string name)
		{
			_view.PaintLine(name, _font);
		}

		public void ShowBlock(string name, long startOffset, long endOffset, long length)
		{
			_view.Paint(name + ", from: ", _font);
			_view.Paint(ToString(startOffset), _fontB);
			_view.Paint(", to: ", _font);
			_view.Paint(ToString(endOffset), _fontB);
			_view.Paint(", length: ", _font);
			_view.PaintLine(length.ToString(), _fontB);
		}

		public void ShowBlock(string text)
		{
			_view.PaintLine(text, _font);
		}

		public void ShowNotScannedLog(string text)
		{
			_view.PaintLine(text, _font, Color.Red);
		}

		private string ToString(long offset)
		{
			if (_useHex)
				return string.Format("{0:X}", offset);
			return offset.ToString();
		}
	}
}
