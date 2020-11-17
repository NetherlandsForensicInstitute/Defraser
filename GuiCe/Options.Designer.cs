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
	partial class Options
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonBrowseTempDirectory = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonBrowseExternalLogViewer = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButtonSystemDefinedTempFileLocation = new System.Windows.Forms.RadioButton();
			this.buttonResetToDefaults = new System.Windows.Forms.Button();
			this.userInterfaceLabel = new System.Windows.Forms.Label();
			this.cbForensicIntegrity = new System.Windows.Forms.CheckBox();
			this.radioButtonUserDefinedTempFileLocation = new System.Windows.Forms.RadioButton();
			this.checkBoxDeleteTempFilesAtApplicationStart = new System.Windows.Forms.CheckBox();
			this.textBoxTempDirectory = new System.Windows.Forms.TextBox();
			this.textBoxExternalLogViewerApplication = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(317, 214);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 7;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(236, 214);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 6;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonBrowseTempDirectory
			// 
			this.buttonBrowseTempDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseTempDirectory.Location = new System.Drawing.Point(307, 60);
			this.buttonBrowseTempDirectory.Name = "buttonBrowseTempDirectory";
			this.buttonBrowseTempDirectory.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowseTempDirectory.TabIndex = 4;
			this.buttonBrowseTempDirectory.Text = "B&rowse...";
			this.buttonBrowseTempDirectory.UseVisualStyleBackColor = true;
			this.buttonBrowseTempDirectory.Click += new System.EventHandler(this.buttonBrowseTempDirectory_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 44);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "&External log viewer";
			// 
			// buttonBrowseExternalLogViewer
			// 
			this.buttonBrowseExternalLogViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseExternalLogViewer.Location = new System.Drawing.Point(313, 39);
			this.buttonBrowseExternalLogViewer.Name = "buttonBrowseExternalLogViewer";
			this.buttonBrowseExternalLogViewer.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowseExternalLogViewer.TabIndex = 4;
			this.buttonBrowseExternalLogViewer.Text = "&Browse...";
			this.buttonBrowseExternalLogViewer.UseVisualStyleBackColor = true;
			this.buttonBrowseExternalLogViewer.Click += new System.EventHandler(this.buttonBrowseExternalLogViewer_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.radioButtonUserDefinedTempFileLocation);
			this.groupBox1.Controls.Add(this.checkBoxDeleteTempFilesAtApplicationStart);
			this.groupBox1.Controls.Add(this.radioButtonSystemDefinedTempFileLocation);
			this.groupBox1.Controls.Add(this.textBoxTempDirectory);
			this.groupBox1.Controls.Add(this.buttonBrowseTempDirectory);
			this.groupBox1.Location = new System.Drawing.Point(6, 90);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(393, 114);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Temporary Files";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Location";
			// 
			// radioButtonSystemDefinedTempFileLocation
			// 
			this.radioButtonSystemDefinedTempFileLocation.AutoSize = true;
			this.radioButtonSystemDefinedTempFileLocation.Location = new System.Drawing.Point(7, 39);
			this.radioButtonSystemDefinedTempFileLocation.Name = "radioButtonSystemDefinedTempFileLocation";
			this.radioButtonSystemDefinedTempFileLocation.Size = new System.Drawing.Size(97, 17);
			this.radioButtonSystemDefinedTempFileLocation.TabIndex = 1;
			this.radioButtonSystemDefinedTempFileLocation.TabStop = true;
			this.radioButtonSystemDefinedTempFileLocation.Text = "&System defined";
			this.radioButtonSystemDefinedTempFileLocation.UseVisualStyleBackColor = true;
			// 
			// buttonResetToDefaults
			// 
			this.buttonResetToDefaults.Location = new System.Drawing.Point(112, 8);
			this.buttonResetToDefaults.Name = "buttonResetToDefaults";
			this.buttonResetToDefaults.Size = new System.Drawing.Size(120, 23);
			this.buttonResetToDefaults.TabIndex = 1;
			this.buttonResetToDefaults.Text = "Reset to defaults";
			this.buttonResetToDefaults.UseVisualStyleBackColor = true;
			this.buttonResetToDefaults.Click += new System.EventHandler(this.buttonResetToDefaults_Click);
			// 
			// userInterfaceLabel
			// 
			this.userInterfaceLabel.AutoSize = true;
			this.userInterfaceLabel.Location = new System.Drawing.Point(10, 13);
			this.userInterfaceLabel.Name = "userInterfaceLabel";
			this.userInterfaceLabel.Size = new System.Drawing.Size(74, 13);
			this.userInterfaceLabel.TabIndex = 0;
			this.userInterfaceLabel.Text = "User &Interface";
			// 
			// cbForensicIntegrity
			// 
			this.cbForensicIntegrity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cbForensicIntegrity.AutoSize = true;
			this.cbForensicIntegrity.Checked = global::Defraser.GuiCe.Properties.Settings.Default.ExportAlsoForensicIntegrityLog;
			this.cbForensicIntegrity.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Defraser.GuiCe.Properties.Settings.Default, "ExportAlsoForensicIntegrityLog", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.cbForensicIntegrity.Location = new System.Drawing.Point(13, 67);
			this.cbForensicIntegrity.Name = "cbForensicIntegrity";
			this.cbForensicIntegrity.Size = new System.Drawing.Size(205, 17);
			this.cbForensicIntegrity.TabIndex = 8;
			this.cbForensicIntegrity.Text = "&Create forensic integrity log for exports";
			this.cbForensicIntegrity.UseVisualStyleBackColor = true;
			// 
			// radioButtonUserDefinedTempFileLocation
			// 
			this.radioButtonUserDefinedTempFileLocation.AutoSize = true;
			this.radioButtonUserDefinedTempFileLocation.Checked = global::Defraser.GuiCe.Properties.Settings.Default.UserDefinedTempDirectory;
			this.radioButtonUserDefinedTempFileLocation.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Defraser.GuiCe.Properties.Settings.Default, "UserDefinedTempDirectory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.radioButtonUserDefinedTempFileLocation.Location = new System.Drawing.Point(7, 63);
			this.radioButtonUserDefinedTempFileLocation.Name = "radioButtonUserDefinedTempFileLocation";
			this.radioButtonUserDefinedTempFileLocation.Size = new System.Drawing.Size(85, 17);
			this.radioButtonUserDefinedTempFileLocation.TabIndex = 2;
			this.radioButtonUserDefinedTempFileLocation.TabStop = true;
			this.radioButtonUserDefinedTempFileLocation.Text = "&User defined";
			this.radioButtonUserDefinedTempFileLocation.UseVisualStyleBackColor = true;
			this.radioButtonUserDefinedTempFileLocation.CheckedChanged += new System.EventHandler(this.radioButtonUserDefinedTempFileLocation_CheckedChanged);
			// 
			// checkBoxDeleteTempFilesAtApplicationStart
			// 
			this.checkBoxDeleteTempFilesAtApplicationStart.AutoSize = true;
			this.checkBoxDeleteTempFilesAtApplicationStart.Checked = global::Defraser.GuiCe.Properties.Settings.Default.DeleteTempFilesAtApplicationStart;
			this.checkBoxDeleteTempFilesAtApplicationStart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxDeleteTempFilesAtApplicationStart.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Defraser.GuiCe.Properties.Settings.Default, "DeleteTempFilesAtApplicationStart", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.checkBoxDeleteTempFilesAtApplicationStart.Location = new System.Drawing.Point(7, 88);
			this.checkBoxDeleteTempFilesAtApplicationStart.Name = "checkBoxDeleteTempFilesAtApplicationStart";
			this.checkBoxDeleteTempFilesAtApplicationStart.Size = new System.Drawing.Size(216, 17);
			this.checkBoxDeleteTempFilesAtApplicationStart.TabIndex = 5;
			this.checkBoxDeleteTempFilesAtApplicationStart.Text = "&Delete temporary files at application start";
			this.checkBoxDeleteTempFilesAtApplicationStart.UseVisualStyleBackColor = true;
			// 
			// textBoxTempDirectory
			// 
			this.textBoxTempDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTempDirectory.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Defraser.GuiCe.Properties.Settings.Default, "TempDirectory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.textBoxTempDirectory.Location = new System.Drawing.Point(101, 62);
			this.textBoxTempDirectory.Name = "textBoxTempDirectory";
			this.textBoxTempDirectory.ReadOnly = true;
			this.textBoxTempDirectory.Size = new System.Drawing.Size(200, 20);
			this.textBoxTempDirectory.TabIndex = 3;
			this.textBoxTempDirectory.Text = global::Defraser.GuiCe.Properties.Settings.Default.TempDirectory;
			// 
			// textBoxExternalLogViewerApplication
			// 
			this.textBoxExternalLogViewerApplication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExternalLogViewerApplication.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Defraser.GuiCe.Properties.Settings.Default, "ExternalLogViewerApplication", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.textBoxExternalLogViewerApplication.Location = new System.Drawing.Point(112, 41);
			this.textBoxExternalLogViewerApplication.Name = "textBoxExternalLogViewerApplication";
			this.textBoxExternalLogViewerApplication.Size = new System.Drawing.Size(195, 20);
			this.textBoxExternalLogViewerApplication.TabIndex = 3;
			this.textBoxExternalLogViewerApplication.Text = global::Defraser.GuiCe.Properties.Settings.Default.ExternalLogViewerApplication;
			// 
			// Options
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(404, 249);
			this.Controls.Add(this.cbForensicIntegrity);
			this.Controls.Add(this.userInterfaceLabel);
			this.Controls.Add(this.buttonResetToDefaults);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonBrowseExternalLogViewer);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBoxExternalLogViewerApplication);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(10000, 283);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 283);
			this.Name = "Options";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Options";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.TextBox textBoxTempDirectory;
		private System.Windows.Forms.Button buttonBrowseTempDirectory;
		private System.Windows.Forms.TextBox textBoxExternalLogViewerApplication;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button buttonBrowseExternalLogViewer;
		private System.Windows.Forms.CheckBox checkBoxDeleteTempFilesAtApplicationStart;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonUserDefinedTempFileLocation;
		private System.Windows.Forms.RadioButton radioButtonSystemDefinedTempFileLocation;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonResetToDefaults;
		private System.Windows.Forms.Label userInterfaceLabel;
		private System.Windows.Forms.CheckBox cbForensicIntegrity;
	}
}
