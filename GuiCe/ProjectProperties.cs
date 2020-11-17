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
using System.Windows.Forms;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	public partial class ProjectProperties : Form
	{
		private readonly IDictionary<ProjectMetadataKey, string> _projectMetadata;
		private readonly IFormFactory _formFactory;

		public ProjectProperties(IDictionary<ProjectMetadataKey, string> projectMetadata, IFormFactory formFactory)
		{
			_projectMetadata = projectMetadata;
			_formFactory = formFactory;

			InitializeComponent();
		}

		private void ProjectProperties_Load(object sender, EventArgs e)
		{
			textBoxProjectDescription.Text = _projectMetadata[ProjectMetadataKey.ProjectDescription];
			DateTime dateCreated = DateTime.Parse(_projectMetadata[ProjectMetadataKey.DateCreated], System.Globalization.CultureInfo.InvariantCulture);
			textBoxDateCreated.Text = string.Format("{0} {1}", dateCreated.ToLongDateString(), dateCreated.ToLongTimeString());

			string dateLastModifiedValue;
			if (_projectMetadata.TryGetValue(ProjectMetadataKey.DateLastModified, out dateLastModifiedValue))
			{
				DateTime dateLastModified = DateTime.Parse(dateLastModifiedValue, System.Globalization.CultureInfo.InvariantCulture);
				textBoxDateLastModified.Text = string.Format("{0} {1}", dateLastModified.ToLongDateString(), dateLastModified.ToLongTimeString());
			}

			textBoxInvestigatorName.Text = _projectMetadata[ProjectMetadataKey.InvestigatorName];
		}

		private void buttonAdvancedPluginConfiguration_Click(object sender, EventArgs e)
		{
			_formFactory.Create<AdvancedDetectorConfiguration>().ShowDialog(this);
		}
	}
}
