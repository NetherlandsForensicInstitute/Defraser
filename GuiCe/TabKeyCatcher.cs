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

using System.Windows.Forms;

namespace Defraser.GuiCe
{
	class TabKeyCatcher : IMessageFilter
	{
		public const int WM_KEYDOWN = 0x0100;

		enum Direction { Unknown, Forward, Backward };

		static private void TraverseForms(Direction direction)
		{
			FormCollection forms = Application.OpenForms;

			if (direction == Direction.Forward)
			{
				for (int formIndex = 0; formIndex < forms.Count; formIndex++ )
				{
					if (forms[formIndex] == Form.ActiveForm)
					{
						if (formIndex == forms.Count - 1) // If last form has the focus, set focus the first form
						{
							forms[0].Activate();
						}
						else
						{
							forms[++formIndex].Activate();
						}
					}
				}
			}
			else if (direction == Direction.Backward)
			{
				for (int formIndex = forms.Count - 1; formIndex >= 0; formIndex--)
				{
					if (forms[formIndex] == Form.ActiveForm)
					{
						if (formIndex == 0) // If first form has the focus, set focus to the last form
						{
							forms[forms.Count - 1].Activate();
						}
						else
						{
							forms[--formIndex].Activate();
						}
					}
				}
			}
		}

		public bool PreFilterMessage(ref Message message)
		{
			if (message.Msg == WM_KEYDOWN)
			{
				if (message.WParam.ToInt32() == (int)Keys.Tab)
				{
					if (Control.ModifierKeys == Keys.Control)
					{
						TraverseForms(Direction.Forward);
						return true; // Do not perform normal tab key action
					}
					else if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
					{
						TraverseForms(Direction.Backward);
						return true; // Do not perform normal tab key action
					}
				}
			}
			return false;	// Handle the key
		}
	}
}
