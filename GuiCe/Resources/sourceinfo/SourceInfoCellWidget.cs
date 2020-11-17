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
using Defraser.GuiCe.Properties;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe.sourceinfo
{
	public class SourceInfoCellWidget : CellWidget
	{
		private readonly Font _font = new Font("Tahoma", 8, FontStyle.Regular);

		public SourceInfoCellWidget(RowWidget rowWidget, Column column)
			: base(rowWidget, column)
		{
		}

		public override int GetOptimalHeight(Graphics graphics)
		{
			return (int)MeasurePaintSize(graphics).Height;
		}

		public override int GetOptimalWidth(Graphics graphics)
		{
			return (int)MeasurePaintSize(graphics).Width;
		}

		private SizeF MeasurePaintSize(Graphics graphics)
		{
			var sourceInfo = CellData.Value as ISourceInfo;
			if (sourceInfo == null)
			{
				return new SizeF(0, 0);
			}

			var measurer = new MeasuringRichTextView(graphics);
			PresentDataOn(measurer, sourceInfo);
			return measurer.Size();
		}

		private void PresentDataOn(IRichTextView dataView, ISourceInfo sourceInfo)
		{
			var displayHex = (Settings.Default.DisplayMode == DisplayMode.Hex);
			sourceInfo.PrintTo(new SourceInfoPresenter(_font, displayHex, dataView));
		}

		public override void OnPaint(Graphics graphics)
		{
			var sourceInfo = CellData.Value as ISourceInfo;
			if (sourceInfo == null)
			{
				base.OnPaint(graphics);
				return;
			}

			var defaultColor = RowWidget.GetActiveStyle().ForeColor;
			PresentDataOn(new RichtTextPainter(graphics, Bounds, defaultColor), sourceInfo);
		}
	}
}
