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

namespace Defraser.GuiCe
{
	partial class EditReferenceHeaderForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.labelBrand = new System.Windows.Forms.Label();
			this.labelModel = new System.Windows.Forms.Label();
			this.textBoxBrand = new System.Windows.Forms.TextBox();
			this.textBoxModel = new System.Windows.Forms.TextBox();
			this.labelSetting = new System.Windows.Forms.Label();
			this.textBoxSetting = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.89691F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.1031F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.labelBrand, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelModel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.textBoxBrand, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxModel, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSetting, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBoxSetting, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(311, 154);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
			this.flowLayoutPanel1.Controls.Add(this.buttonOK);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 115);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(291, 29);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonCancel.Location = new System.Drawing.Point(206, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(82, 24);
			this.buttonCancel.TabIndex = 7;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(118, 3);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(82, 24);
			this.buttonOK.TabIndex = 6;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// labelBrand
			// 
			this.labelBrand.AutoSize = true;
			this.labelBrand.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelBrand.Location = new System.Drawing.Point(13, 10);
			this.labelBrand.Name = "labelBrand";
			this.labelBrand.Size = new System.Drawing.Size(80, 30);
			this.labelBrand.TabIndex = 0;
			this.labelBrand.Text = "Camera &Brand:";
			this.labelBrand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelModel
			// 
			this.labelModel.AutoSize = true;
			this.labelModel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelModel.Location = new System.Drawing.Point(13, 40);
			this.labelModel.Name = "labelModel";
			this.labelModel.Size = new System.Drawing.Size(80, 30);
			this.labelModel.TabIndex = 2;
			this.labelModel.Text = "Camera &Model:";
			this.labelModel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxBrand
			// 
			this.textBoxBrand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxBrand.Location = new System.Drawing.Point(99, 15);
			this.textBoxBrand.Name = "textBoxBrand";
			this.textBoxBrand.Size = new System.Drawing.Size(199, 20);
			this.textBoxBrand.TabIndex = 1;
			// 
			// textBoxModel
			// 
			this.textBoxModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxModel.Location = new System.Drawing.Point(99, 45);
			this.textBoxModel.Name = "textBoxModel";
			this.textBoxModel.Size = new System.Drawing.Size(199, 20);
			this.textBoxModel.TabIndex = 3;
			// 
			// labelSetting
			// 
			this.labelSetting.AutoSize = true;
			this.labelSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelSetting.Location = new System.Drawing.Point(13, 70);
			this.labelSetting.Name = "labelSetting";
			this.labelSetting.Size = new System.Drawing.Size(80, 30);
			this.labelSetting.TabIndex = 4;
			this.labelSetting.Text = "Info / &Setting:";
			this.labelSetting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxSetting
			// 
			this.textBoxSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSetting.Location = new System.Drawing.Point(99, 75);
			this.textBoxSetting.Name = "textBoxSetting";
			this.textBoxSetting.Size = new System.Drawing.Size(199, 20);
			this.textBoxSetting.TabIndex = 5;
			// 
			// EditReferenceHeaderForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(311, 154);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "EditReferenceHeaderForm";
			this.Text = "Edit Reference Header";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditReferenceHeaderForm_FormClosing);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label labelBrand;
		private System.Windows.Forms.Label labelModel;
		private System.Windows.Forms.TextBox textBoxBrand;
		private System.Windows.Forms.TextBox textBoxModel;
		private System.Windows.Forms.Label labelSetting;
		private System.Windows.Forms.TextBox textBoxSetting;
	}
}
