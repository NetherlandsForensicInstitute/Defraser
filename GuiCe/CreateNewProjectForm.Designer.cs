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
	partial class CreateNewProjectForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewProjectForm));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxProjectDirectory = new System.Windows.Forms.TextBox();
			this.textBoxProjectDescription = new System.Windows.Forms.TextBox();
			this.textBoxInvestigatorName = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxCreationDate = new System.Windows.Forms.TextBox();
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.label5 = new System.Windows.Forms.Label();
			this.textBoxProjectFileName = new System.Windows.Forms.TextBox();
			this.buttonAdvancedPluginConfiguration = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(510, 263);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 13;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Location = new System.Drawing.Point(429, 263);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 12;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(103, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Project File &Location";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 206);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(93, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "&Investigator Name";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 232);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "&Creation Date";
			// 
			// textBoxProjectDirectory
			// 
			this.textBoxProjectDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxProjectDirectory.Location = new System.Drawing.Point(121, 36);
			this.textBoxProjectDirectory.Name = "textBoxProjectDirectory";
			this.textBoxProjectDirectory.ReadOnly = true;
			this.textBoxProjectDirectory.Size = new System.Drawing.Size(383, 20);
			this.textBoxProjectDirectory.TabIndex = 3;
			// 
			// textBoxProjectDescription
			// 
			this.textBoxProjectDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxProjectDescription.Location = new System.Drawing.Point(121, 63);
			this.textBoxProjectDescription.Multiline = true;
			this.textBoxProjectDescription.Name = "textBoxProjectDescription";
			this.textBoxProjectDescription.Size = new System.Drawing.Size(464, 134);
			this.textBoxProjectDescription.TabIndex = 6;
			// 
			// textBoxInvestigatorName
			// 
			this.textBoxInvestigatorName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxInvestigatorName.Location = new System.Drawing.Point(121, 203);
			this.textBoxInvestigatorName.Name = "textBoxInvestigatorName";
			this.textBoxInvestigatorName.Size = new System.Drawing.Size(464, 20);
			this.textBoxInvestigatorName.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 66);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Project &Description";
			// 
			// textBoxCreationDate
			// 
			this.textBoxCreationDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCreationDate.Location = new System.Drawing.Point(121, 229);
			this.textBoxCreationDate.Name = "textBoxCreationDate";
			this.textBoxCreationDate.ReadOnly = true;
			this.textBoxCreationDate.Size = new System.Drawing.Size(464, 20);
			this.textBoxCreationDate.TabIndex = 10;
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowse.Location = new System.Drawing.Point(510, 34);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowse.TabIndex = 4;
			this.buttonBrowse.Text = "&Browse...";
			this.buttonBrowse.UseVisualStyleBackColor = true;
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Location to create the new project file";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Project &File Name";
			// 
			// textBoxProjectFileName
			// 
			this.textBoxProjectFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxProjectFileName.Location = new System.Drawing.Point(121, 9);
			this.textBoxProjectFileName.Name = "textBoxProjectFileName";
			this.textBoxProjectFileName.Size = new System.Drawing.Size(464, 20);
			this.textBoxProjectFileName.TabIndex = 1;
			// 
			// buttonAdvancedPluginConfiguration
			// 
			this.buttonAdvancedPluginConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonAdvancedPluginConfiguration.Location = new System.Drawing.Point(15, 255);
			this.buttonAdvancedPluginConfiguration.Name = "buttonAdvancedPluginConfiguration";
			this.buttonAdvancedPluginConfiguration.Size = new System.Drawing.Size(184, 23);
			this.buttonAdvancedPluginConfiguration.TabIndex = 11;
			this.buttonAdvancedPluginConfiguration.Text = "&Advanced Plug-in Configuration...";
			this.buttonAdvancedPluginConfiguration.UseVisualStyleBackColor = true;
			this.buttonAdvancedPluginConfiguration.Visible = false;
			this.buttonAdvancedPluginConfiguration.Click += new System.EventHandler(this.buttonAdvancedPluginConfiguration_Click);
			// 
			// CreateNewProjectForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(597, 298);
			this.Controls.Add(this.buttonAdvancedPluginConfiguration);
			this.Controls.Add(this.textBoxProjectFileName);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.buttonBrowse);
			this.Controls.Add(this.textBoxCreationDate);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBoxInvestigatorName);
			this.Controls.Add(this.textBoxProjectDescription);
			this.Controls.Add(this.textBoxProjectDirectory);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CreateNewProjectForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Create New Project";
			this.Load += new System.EventHandler(this.CreateNewProjectForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxProjectDirectory;
		private System.Windows.Forms.TextBox textBoxProjectDescription;
		private System.Windows.Forms.TextBox textBoxInvestigatorName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBoxCreationDate;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBoxProjectFileName;
		private System.Windows.Forms.Button buttonAdvancedPluginConfiguration;
	}
}
