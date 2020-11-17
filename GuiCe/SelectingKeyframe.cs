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
using System.ComponentModel;
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
    public partial class SelectingKeyframe : Form
    {
        private readonly MainForm _mainForm;
        private BackgroundDataBlockScanner _dataBlockScanner;
		private IResultNode _keyframeToSelect;
        private readonly Timer _selectDelayTimer;


        public SelectingKeyframe(MainForm mainForm)
        {
            _mainForm = mainForm;

            _selectDelayTimer = new Timer();
            _selectDelayTimer.Interval = 1000;
            _selectDelayTimer.Tick += new EventHandler(selectDelayTimer_Tick);

            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void SelectKeyframeAfterFileScannerComplete(BackgroundDataBlockScanner dataBlockScanner, IResultNode keyframeToSelect)
        {
            _keyframeToSelect = keyframeToSelect;
            _dataBlockScanner = dataBlockScanner;

            if (_dataBlockScanner.IsBusy)
            {
                _dataBlockScanner.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fileScanner_RunWorkerCompleted);
            }
            else
            {
                ScannerCompleted();
            }
        }
        
        private void ScannerCompleted()
        {
            _mainForm.HeaderPanel.HeaderTree.SelectedItem = _keyframeToSelect;
            Close();
        }

        #region EventHandlers
        private void fileScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _selectDelayTimer.Start(); // Delay the item selection 1 second to make sure the HeaderTree is ready.
        }

        private void selectDelayTimer_Tick(object sender, EventArgs e)
        {
            _selectDelayTimer.Stop();
            ScannerCompleted();
        }
        #endregion EventHandlers
    }
}
