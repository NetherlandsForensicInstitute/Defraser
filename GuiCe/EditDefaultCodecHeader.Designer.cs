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
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	partial class EditDefaultCodecHeader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditDefaultCodecHeader));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonSearchCodecHeader = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textDefaultCodecHeaderInformation = new System.Windows.Forms.TextBox();
            this.informationLabel = new System.Windows.Forms.Label();
            this.labelCodecHeaderSourceFile = new System.Windows.Forms.Label();
            this.listAvailableReferenceFiles = new System.Windows.Forms.ListBox();
            this.selectProjectDefaultCodecHeaders = new System.Windows.Forms.DataGridView();
            this.CodecName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DefaultReferenceFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelProjectDefaultCodecHeader = new System.Windows.Forms.Label();
            this.buttonResetDefaultCodecHeader = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectProjectDefaultCodecHeaders)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 83F));
            this.tableLayoutPanel1.Controls.Add(this.buttonSearchCodecHeader, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.textDefaultCodecHeaderInformation, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.informationLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelCodecHeaderSourceFile, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listAvailableReferenceFiles, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.selectProjectDefaultCodecHeaders, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelProjectDefaultCodecHeader, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonResetDefaultCodecHeader, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(512, 516);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonSearchCodecHeader
            // 
            this.buttonSearchCodecHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearchCodecHeader.Enabled = false;
            this.buttonSearchCodecHeader.Location = new System.Drawing.Point(304, 481);
            this.buttonSearchCodecHeader.Name = "buttonSearchCodecHeader";
            this.buttonSearchCodecHeader.Size = new System.Drawing.Size(122, 23);
            this.buttonSearchCodecHeader.TabIndex = 2;
            this.buttonSearchCodecHeader.Text = "Search Codec Header";
            this.buttonSearchCodecHeader.UseVisualStyleBackColor = true;
            this.buttonSearchCodecHeader.Click += new System.EventHandler(this.buttonSearchCodecHeader_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(432, 481);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(77, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textDefaultCodecHeaderInformation
            // 
            this.textDefaultCodecHeaderInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textDefaultCodecHeaderInformation.Location = new System.Drawing.Point(113, 351);
            this.textDefaultCodecHeaderInformation.Multiline = true;
            this.textDefaultCodecHeaderInformation.Name = "textDefaultCodecHeaderInformation";
            this.textDefaultCodecHeaderInformation.ReadOnly = true;
            this.textDefaultCodecHeaderInformation.Size = new System.Drawing.Size(313, 124);
            this.textDefaultCodecHeaderInformation.TabIndex = 10;
            this.textDefaultCodecHeaderInformation.Tag = "";
            this.textDefaultCodecHeaderInformation.Text = resources.GetString("textDefaultCodecHeaderInformation.Text");
            // 
            // informationLabel
            // 
            this.informationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.informationLabel.AutoSize = true;
            this.informationLabel.Location = new System.Drawing.Point(45, 348);
            this.informationLabel.Name = "informationLabel";
            this.informationLabel.Size = new System.Drawing.Size(62, 13);
            this.informationLabel.TabIndex = 11;
            this.informationLabel.Text = "Information:";
            // 
            // labelCodecHeaderSourceFile
            // 
            this.labelCodecHeaderSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCodecHeaderSourceFile.AutoSize = true;
            this.labelCodecHeaderSourceFile.Location = new System.Drawing.Point(9, 174);
            this.labelCodecHeaderSourceFile.Name = "labelCodecHeaderSourceFile";
            this.labelCodecHeaderSourceFile.Size = new System.Drawing.Size(98, 26);
            this.labelCodecHeaderSourceFile.TabIndex = 12;
            this.labelCodecHeaderSourceFile.Text = "Available &reference files in project:";
            this.labelCodecHeaderSourceFile.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // listAvailableReferenceFiles
            // 
            this.listAvailableReferenceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listAvailableReferenceFiles.FormattingEnabled = true;
            this.listAvailableReferenceFiles.Location = new System.Drawing.Point(113, 177);
            this.listAvailableReferenceFiles.Name = "listAvailableReferenceFiles";
            this.listAvailableReferenceFiles.Size = new System.Drawing.Size(313, 160);
            this.listAvailableReferenceFiles.TabIndex = 13;
            this.listAvailableReferenceFiles.SelectedIndexChanged += new System.EventHandler(this.listAvailableReferenceFiles_SelectedIndexChanged);
            // 
            // selectProjectDefaultCodecHeaders
            // 
            this.selectProjectDefaultCodecHeaders.AllowUserToAddRows = false;
            this.selectProjectDefaultCodecHeaders.AllowUserToDeleteRows = false;
            this.selectProjectDefaultCodecHeaders.AllowUserToResizeRows = false;
            this.selectProjectDefaultCodecHeaders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.selectProjectDefaultCodecHeaders.BackgroundColor = System.Drawing.SystemColors.Window;
            this.selectProjectDefaultCodecHeaders.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.selectProjectDefaultCodecHeaders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.selectProjectDefaultCodecHeaders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CodecName,
            this.DefaultReferenceFile});
            this.selectProjectDefaultCodecHeaders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectProjectDefaultCodecHeaders.Location = new System.Drawing.Point(113, 3);
            this.selectProjectDefaultCodecHeaders.MultiSelect = false;
            this.selectProjectDefaultCodecHeaders.Name = "selectProjectDefaultCodecHeaders";
            this.selectProjectDefaultCodecHeaders.ReadOnly = true;
            this.selectProjectDefaultCodecHeaders.RowHeadersVisible = false;
            this.selectProjectDefaultCodecHeaders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.selectProjectDefaultCodecHeaders.Size = new System.Drawing.Size(313, 168);
            this.selectProjectDefaultCodecHeaders.TabIndex = 16;
            this.selectProjectDefaultCodecHeaders.SelectionChanged += new System.EventHandler(this.selectProjectDefaultCodecHeaders_SelectionChanged);
            // 
            // CodecName
            // 
            this.CodecName.DataPropertyName = "Codec";
            this.CodecName.HeaderText = "Codec";
            this.CodecName.Name = "CodecName";
            this.CodecName.ReadOnly = true;
            // 
            // DefaultReferenceFile
            // 
            this.DefaultReferenceFile.DataPropertyName = "ReferenceName";
            this.DefaultReferenceFile.HeaderText = "Default Reference File";
            this.DefaultReferenceFile.Name = "DefaultReferenceFile";
            this.DefaultReferenceFile.ReadOnly = true;
            // 
            // labelProjectDefaultCodecHeader
            // 
            this.labelProjectDefaultCodecHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProjectDefaultCodecHeader.AutoSize = true;
            this.labelProjectDefaultCodecHeader.Location = new System.Drawing.Point(23, 0);
            this.labelProjectDefaultCodecHeader.Name = "labelProjectDefaultCodecHeader";
            this.labelProjectDefaultCodecHeader.Size = new System.Drawing.Size(84, 26);
            this.labelProjectDefaultCodecHeader.TabIndex = 17;
            this.labelProjectDefaultCodecHeader.Text = "&Project Default Codec Headers:";
            // 
            // buttonResetDefaultCodecHeader
            // 
            this.buttonResetDefaultCodecHeader.Location = new System.Drawing.Point(432, 3);
            this.buttonResetDefaultCodecHeader.Name = "buttonResetDefaultCodecHeader";
            this.buttonResetDefaultCodecHeader.Size = new System.Drawing.Size(75, 34);
            this.buttonResetDefaultCodecHeader.TabIndex = 18;
            this.buttonResetDefaultCodecHeader.Text = "Reset Header";
            this.buttonResetDefaultCodecHeader.UseVisualStyleBackColor = true;
            this.buttonResetDefaultCodecHeader.Click += new System.EventHandler(this.buttonResetDefaultCodecHeader_Click);
            // 
            // EditDefaultCodecHeader
            // 
            this.AcceptButton = this.buttonSearchCodecHeader;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(512, 516);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditDefaultCodecHeader";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Default Codec Header";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectProjectDefaultCodecHeaders)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonSearchCodecHeader;
        private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TextBox textDefaultCodecHeaderInformation;
        private System.Windows.Forms.Label informationLabel;
        private System.Windows.Forms.Label labelCodecHeaderSourceFile;
        private System.Windows.Forms.ListBox listAvailableReferenceFiles;
        private System.Windows.Forms.Label labelProjectDefaultCodecHeader;
		private System.Windows.Forms.DataGridView selectProjectDefaultCodecHeaders;
		private System.Windows.Forms.Button buttonResetDefaultCodecHeader;
		private System.Windows.Forms.DataGridViewTextBoxColumn CodecName;
		private System.Windows.Forms.DataGridViewTextBoxColumn DefaultReferenceFile;
	}
}
