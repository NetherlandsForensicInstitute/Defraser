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

namespace Defraser.GuiCe
{
	partial class ProjectProperties
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectProperties));
			this.textBoxDateCreated = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxInvestigatorName = new System.Windows.Forms.TextBox();
			this.textBoxProjectDescription = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonClose = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textBoxDateLastModified = new System.Windows.Forms.TextBox();
			this.textBoxFileVersion = new System.Windows.Forms.TextBox();
			this.buttonAdvancedPluginConfiguration = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBoxDateCreated
			// 
			this.textBoxDateCreated.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDateCreated.Location = new System.Drawing.Point(114, 142);
			this.textBoxDateCreated.Name = "textBoxDateCreated";
			this.textBoxDateCreated.ReadOnly = true;
			this.textBoxDateCreated.Size = new System.Drawing.Size(383, 20);
			this.textBoxDateCreated.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 15);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Project &Description";
			// 
			// textBoxInvestigatorName
			// 
			this.textBoxInvestigatorName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxInvestigatorName.Location = new System.Drawing.Point(114, 116);
			this.textBoxInvestigatorName.Name = "textBoxInvestigatorName";
			this.textBoxInvestigatorName.ReadOnly = true;
			this.textBoxInvestigatorName.Size = new System.Drawing.Size(383, 20);
			this.textBoxInvestigatorName.TabIndex = 3;
			// 
			// textBoxProjectDescription
			// 
			this.textBoxProjectDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxProjectDescription.Location = new System.Drawing.Point(114, 12);
			this.textBoxProjectDescription.Multiline = true;
			this.textBoxProjectDescription.Name = "textBoxProjectDescription";
			this.textBoxProjectDescription.ReadOnly = true;
			this.textBoxProjectDescription.Size = new System.Drawing.Size(383, 98);
			this.textBoxProjectDescription.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 145);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Date &Created";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 119);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(93, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "&Investigator Name";
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonClose.Location = new System.Drawing.Point(422, 255);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(75, 23);
			this.buttonClose.TabIndex = 11;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 171);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(99, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Date &Last Changed";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(13, 197);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Database &version";
			// 
			// textBoxDateLastModified
			// 
			this.textBoxDateLastModified.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDateLastModified.Location = new System.Drawing.Point(114, 168);
			this.textBoxDateLastModified.Name = "textBoxDateLastModified";
			this.textBoxDateLastModified.ReadOnly = true;
			this.textBoxDateLastModified.Size = new System.Drawing.Size(383, 20);
			this.textBoxDateLastModified.TabIndex = 7;
			// 
			// textBoxFileVersion
			// 
			this.textBoxFileVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxFileVersion.Location = new System.Drawing.Point(114, 194);
			this.textBoxFileVersion.Name = "textBoxFileVersion";
			this.textBoxFileVersion.ReadOnly = true;
			this.textBoxFileVersion.Size = new System.Drawing.Size(383, 20);
			this.textBoxFileVersion.TabIndex = 9;
			// 
			// buttonAdvancedPluginConfiguration
			// 
			this.buttonAdvancedPluginConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonAdvancedPluginConfiguration.Location = new System.Drawing.Point(16, 220);
			this.buttonAdvancedPluginConfiguration.Name = "buttonAdvancedPluginConfiguration";
			this.buttonAdvancedPluginConfiguration.Size = new System.Drawing.Size(181, 23);
			this.buttonAdvancedPluginConfiguration.TabIndex = 10;
			this.buttonAdvancedPluginConfiguration.Text = "&Advanced Plug-in Configuration...";
			this.buttonAdvancedPluginConfiguration.UseVisualStyleBackColor = true;
			this.buttonAdvancedPluginConfiguration.Click += new System.EventHandler(this.buttonAdvancedPluginConfiguration_Click);
			// 
			// ProjectProperties
			// 
			this.AcceptButton = this.buttonClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonClose;
			this.ClientSize = new System.Drawing.Size(517, 295);
			this.Controls.Add(this.buttonAdvancedPluginConfiguration);
			this.Controls.Add(this.textBoxFileVersion);
			this.Controls.Add(this.textBoxDateLastModified);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxDateCreated);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBoxInvestigatorName);
			this.Controls.Add(this.textBoxProjectDescription);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonClose);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ProjectProperties";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Properties";
			this.Load += new System.EventHandler(this.ProjectProperties_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxDateCreated;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBoxInvestigatorName;
		private System.Windows.Forms.TextBox textBoxProjectDescription;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBoxDateLastModified;
		private System.Windows.Forms.TextBox textBoxFileVersion;
		private System.Windows.Forms.Button buttonAdvancedPluginConfiguration;
	}
}
