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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Defraser.GuiCe
{
    public partial class CustomFlowLayoutPanel : FlowLayoutPanel
    {
        #region Properties
        public int ActiveThumbControlIndex { set; get; }
        #endregion Properties

        public CustomFlowLayoutPanel()
        {
            ActiveThumbControlIndex = -1;
            InitializeComponent();
        }

        public CustomFlowLayoutPanel(IContainer container)
        {
            ActiveThumbControlIndex = -1;
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// We have to use the 'ProcessCmdKey' method, 
        /// the normal OnKeyDown/OnKeyUp/OnKeyPress methods don't work for the arrow keys.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveThumbControlIndex == -1) return false;

            int nextSelectedThumbIndex = -1;
            switch (keyData)
            {
                case Keys.Left:
                    nextSelectedThumbIndex = ActiveThumbControlIndex - 1;
                    break;
                case Keys.Up:
                    nextSelectedThumbIndex = ActiveThumbControlIndex - GetNumItemsRow();
                    break;
                case Keys.Right:
                    nextSelectedThumbIndex = ActiveThumbControlIndex + 1;
                    break;
                case Keys.Down:
                    nextSelectedThumbIndex = ActiveThumbControlIndex + GetNumItemsRow();
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            if (nextSelectedThumbIndex >= 0 && nextSelectedThumbIndex < Controls.Count)
            {
                ThumbControl newThumbControl = Controls[nextSelectedThumbIndex] as ThumbControl;
                if (newThumbControl != null)
                {
                    newThumbControl.SelectThumb();
                }
            }
            return true;
        }

        /// <summary>
        /// Calculate the number of items on the first row of the FlowContainer.
        /// This is done by checking the Y-location of each control in the container.
        /// </summary>
        /// <returns></returns>
        private int GetNumItemsRow()
        {
            if (Controls.Count <= 0) return 0;

            int count = 0;
            int firstItemY = Controls[0].Location.Y;
            IEnumerator enumerator = Controls.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (((Control)enumerator.Current).Location.Y != firstItemY)
                    break;
                count++;
            }
            return count;
        }
    }
}
