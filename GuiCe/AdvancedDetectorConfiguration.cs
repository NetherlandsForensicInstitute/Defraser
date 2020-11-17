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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public partial class AdvancedDetectorConfiguration : Form
	{
		private static readonly Padding AlignWithTextBoxContent = new Padding(0, 6, 0, 0);

		public static string DetectorConfigurationFileName { get { return Path.Combine(MainForm.ApplicationDataPath, "AdvancedDetectorUserConfiguration.xml"); } }

		// Once there is a scanned file in the project,
		// the user should no longer be able to change the plug-in configuration.
		private readonly bool _editMode;
		private readonly IDetectorFactory _detectorFactory;

		public AdvancedDetectorConfiguration(bool editMode, IDetectorFactory detectorFactory)
		{
			PreConditions.Argument("detectorFactory").Value(detectorFactory).IsNotNull();

			_editMode = editMode;
			_detectorFactory = detectorFactory;

			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CreateTabPages();
		}

		/// <summary>
		/// Workaround for Microsoft bug with ID=116242
		/// "CausesValidation has no effect on Button when PerformClick is called."
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if(keyData == Keys.Escape)
			{
				AutoValidate = AutoValidate.Disable;
				buttonCancel.PerformClick();
				AutoValidate = AutoValidate.Inherit;
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void CreateTabPages()
		{
			IList<IDetector> detectors = _detectorFactory.Detectors;

			foreach (IDetector detector in detectors)
			{
				tabControlConfiguration.TabPages.Add(CreateTabPage(detector));
			}
		}

		private TabPage CreateTabPage(IDetector detector)
		{
			if(detector == null)
			{
				throw new ArgumentNullException("detector");
			}

			IConfigurable configurable;
			if ((configurable = detector as IConfigurable) == null)
			{
				throw new ArgumentException("is not configurable", "detector");
			}

			TabPage tabPage = new TabPage(detector.Name);

			ICollection<IConfigurationItem> configurations = configurable.Configuration;

			TableLayoutPanel panel = new TableLayoutPanel();
			panel.Dock = DockStyle.Fill;
			panel.ColumnCount = 3;
			panel.RowCount = configurations.Count + 1/* +1 for column header*/;
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

			// Column Header
			Label label = new Label();
			label.Text = "Description";
			label.Margin = AlignWithTextBoxContent;
			panel.Controls.Add(label);
			label = new Label();
			label.Text = "Default Value";
			label.Margin = AlignWithTextBoxContent;
			panel.Controls.Add(label);
			label = new Label();
			label.Text = "Overwrite Default";
			label.Margin = AlignWithTextBoxContent;
			panel.Controls.Add(label);

			foreach(IConfigurationItem configurationItem in configurations)
			{
				Label description = new Label();
				description.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				description.Text = configurationItem.Description;
				description.Margin = AlignWithTextBoxContent;
				panel.Controls.Add(description);

				Label defaultValue = new Label();
				defaultValue.Text = configurationItem.DefaultValue.ToString();
				defaultValue.Margin = AlignWithTextBoxContent;
				panel.Controls.Add(defaultValue);

				TextBox userValue = new TextBox();
				userValue.Enabled = _editMode;
				userValue.Text = configurationItem.Value.ToString();
				userValue.Anchor = AnchorStyles.Right| AnchorStyles.Top;
				userValue.Tag = configurationItem;

				userValue.Validating += UserValueValidating;

				panel.Controls.Add(userValue);
			}
			tabPage.Controls.Add(panel);

			return tabPage;
		}

		private void UserValueValidating(object sender, CancelEventArgs e)
		{
			TextBox userValue = sender as TextBox;
			if (userValue == null) return;

			IConfigurationItem configurationItem = userValue.Tag as IConfigurationItem;
			if (configurationItem != null)
			{
				if (!configurationItem.IsValidUserInput(userValue.Text))
				{
					userValue.BackColor = Color.Red;
					e.Cancel = true;
				}
				else
				{
					userValue.BackColor = new TextBox().BackColor;
				}
			}
		}

		/// <summary>
		/// Accept the changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonOKClick(object sender, EventArgs e)
		{
			foreach (Control control in tabControlConfiguration.Controls)
			{
				if (!(control is TabPage)) continue;

				foreach (Control tableLayoutControl in control.Controls)
				{
					TableLayoutPanel tableLayoutPanel = tableLayoutControl as TableLayoutPanel;
					if (tableLayoutPanel == null) continue;

					for (int rowIndex = 1; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
					{
						TextBox userValue = tableLayoutPanel.GetControlFromPosition(2, rowIndex) as TextBox;

						IConfigurationItem configurationItem = userValue.Tag as IConfigurationItem;

						configurationItem.SetUserValue(userValue.Text);
					}
				}
			}
		}
	}
}
