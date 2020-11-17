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
	partial class ReferenceHeaderGridView
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.columnId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnIncluded = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.columnBrand = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnModel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnSetting = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnCodec = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnHeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnFrameRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnData = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// columnId
			// 
			this.columnId.HeaderText = "Id";
			this.columnId.Name = "columnId";
			this.columnId.ReadOnly = true;
			this.columnId.Visible = false;
			// 
			// columnIncluded
			// 
			this.columnIncluded.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnIncluded.HeaderText = "";
			this.columnIncluded.Name = "columnIncluded";
			this.columnIncluded.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.columnIncluded.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.columnIncluded.Width = 19;
			// 
			// columnBrand
			// 
			this.columnBrand.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnBrand.HeaderText = "Camera Brand";
			this.columnBrand.Name = "columnBrand";
			this.columnBrand.ReadOnly = true;
			this.columnBrand.ToolTipText = "Device or (software) application brand";
			this.columnBrand.Width = 99;
			// 
			// columnModel
			// 
			this.columnModel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnModel.HeaderText = "Camera Model";
			this.columnModel.Name = "columnModel";
			this.columnModel.ReadOnly = true;
			this.columnModel.ToolTipText = "Device model or application version";
			// 
			// columnSetting
			// 
			this.columnSetting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnSetting.HeaderText = "Info / Setting";
			this.columnSetting.Name = "columnSetting";
			this.columnSetting.ReadOnly = true;
			this.columnSetting.ToolTipText = "Device or application settings and/or information";
			this.columnSetting.Width = 94;
			// 
			// columnCodec
			// 
			this.columnCodec.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnCodec.HeaderText = "Video Codec";
			this.columnCodec.Name = "columnCodec";
			this.columnCodec.ReadOnly = true;
			this.columnCodec.Width = 93;
			// 
			// columnWidth
			// 
			this.columnWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnWidth.HeaderText = "Width";
			this.columnWidth.Name = "columnWidth";
			this.columnWidth.ReadOnly = true;
			this.columnWidth.Width = 60;
			// 
			// columnHeight
			// 
			this.columnHeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnHeight.HeaderText = "Height";
			this.columnHeight.Name = "columnHeight";
			this.columnHeight.ReadOnly = true;
			this.columnHeight.Width = 63;
			// 
			// columnFrameRate
			// 
			this.columnFrameRate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.columnFrameRate.HeaderText = "Frame Rate";
			this.columnFrameRate.Name = "columnFrameRate";
			this.columnFrameRate.ReadOnly = true;
			this.columnFrameRate.Width = 87;
			// 
			// columnData
			// 
			this.columnData.HeaderText = "Header Data (Hex)";
			this.columnData.Name = "columnData";
			this.columnData.ReadOnly = true;
			this.columnData.ToolTipText = "Hexadecimal representation of the header bytes";
			// 
			// ReferenceHeaderGridView
			// 
			this.AllowUserToAddRows = false;
			this.AllowUserToDeleteRows = false;
			this.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
			this.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.BackgroundColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnId,
            this.columnIncluded,
            this.columnBrand,
            this.columnModel,
            this.columnSetting,
            this.columnCodec,
            this.columnWidth,
            this.columnHeight,
            this.columnFrameRate,
            this.columnData});
			this.RowHeadersVisible = false;
			this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ReferenceHeaderGridView_CellValueChanged);
			this.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ReferenceHeaderGridView_CellFormatting);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridViewTextBoxColumn columnId;
		private System.Windows.Forms.DataGridViewCheckBoxColumn columnIncluded;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnBrand;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnModel;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnSetting;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnCodec;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnWidth;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnHeight;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnFrameRate;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnData;













	}
}
