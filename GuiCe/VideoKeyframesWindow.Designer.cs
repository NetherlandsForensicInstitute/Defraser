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

namespace Defraser.GuiCe
{
    partial class VideoKeyframesWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                KeyframeSourceTree = null;
            	_videoKeyframesManager = null;

				if (components != null)
				{
					components.Dispose();
				}
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoKeyframesWindow));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.thumbsContainer = new Defraser.GuiCe.CustomFlowLayoutPanel(this.components);
            this.maxThumbsPanel = new System.Windows.Forms.Panel();
            this.labelMaxThumbs = new System.Windows.Forms.Label();
            this.buttonApplyMaxThumbs = new System.Windows.Forms.Button();
            this.textBoxMaxNumThumbs = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.maxThumbsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.thumbsContainer, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.maxThumbsPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(792, 573);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // thumbsContainer
            // 
            this.thumbsContainer.ActiveThumbControlIndex = -1;
            this.thumbsContainer.AutoScroll = true;
            this.thumbsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thumbsContainer.Location = new System.Drawing.Point(3, 3);
            this.thumbsContainer.Name = "thumbsContainer";
            this.thumbsContainer.Size = new System.Drawing.Size(786, 537);
            this.thumbsContainer.TabStop = true;
            this.thumbsContainer.TabIndex = 1;
            // 
            // maxThumbsPanel
            // 
            this.maxThumbsPanel.BackColor = System.Drawing.SystemColors.Control;
            this.maxThumbsPanel.Controls.Add(this.labelMaxThumbs);
            this.maxThumbsPanel.Controls.Add(this.buttonApplyMaxThumbs);
            this.maxThumbsPanel.Controls.Add(this.textBoxMaxNumThumbs);
            this.maxThumbsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maxThumbsPanel.Location = new System.Drawing.Point(0, 543);
            this.maxThumbsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.maxThumbsPanel.Name = "maxThumbsPanel";
            this.maxThumbsPanel.Size = new System.Drawing.Size(792, 30);
            this.maxThumbsPanel.TabIndex = 2;
            // 
            // labelMaxThumbs
            // 
            this.labelMaxThumbs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelMaxThumbs.AutoSize = true;
            this.labelMaxThumbs.Location = new System.Drawing.Point(494, 8);
            this.labelMaxThumbs.Name = "labelMaxThumbs";
            this.labelMaxThumbs.Size = new System.Drawing.Size(141, 13);
            this.labelMaxThumbs.TabIndex = 3;
            this.labelMaxThumbs.Text = "Maximum number of &thumbs:";
            // 
            // buttonApplyMaxThumbs
            // 
            this.buttonApplyMaxThumbs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonApplyMaxThumbs.Location = new System.Drawing.Point(716, 4);
            this.buttonApplyMaxThumbs.Name = "buttonApplyMaxThumbs";
            this.buttonApplyMaxThumbs.Size = new System.Drawing.Size(75, 23);
            this.buttonApplyMaxThumbs.TabIndex = 5;
            this.buttonApplyMaxThumbs.Text = "Apply";
            this.buttonApplyMaxThumbs.UseVisualStyleBackColor = true;
            this.buttonApplyMaxThumbs.Click += new System.EventHandler(this.buttonApplyMaxThumbs_Click);
            // 
            // textBoxMaxNumThumbs
            // 
            this.textBoxMaxNumThumbs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.textBoxMaxNumThumbs.Location = new System.Drawing.Point(641, 5);
            this.textBoxMaxNumThumbs.MaxLength = 5;
            this.textBoxMaxNumThumbs.Name = "textBoxMaxNumThumbs";
            this.textBoxMaxNumThumbs.Size = new System.Drawing.Size(70, 20);
            this.textBoxMaxNumThumbs.TabIndex = 4;
            this.textBoxMaxNumThumbs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxMaxNumThumbs_KeyPress);
            // 
            // VideoKeyframesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "VideoKeyframesWindow";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.maxThumbsPanel.ResumeLayout(false);
            this.maxThumbsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private CustomFlowLayoutPanel thumbsContainer;
		private System.Windows.Forms.Panel maxThumbsPanel;
		private System.Windows.Forms.Button buttonApplyMaxThumbs;
		private System.Windows.Forms.TextBox textBoxMaxNumThumbs;
		private System.Windows.Forms.Label labelMaxThumbs;

	}
}
