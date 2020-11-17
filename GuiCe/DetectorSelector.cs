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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using Defraser.DataStructures;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Provides a UI for selecting detectors from a list of available detectors.
	/// </summary>
	internal sealed partial class DetectorSelector : Tree
	{
		// The selected detectors
		private readonly List<IDetector> _selectedDetectors = new List<IDetector>();

		#region Properties
		/// <summary>
		/// The list of available detectors.
		/// </summary>
		internal IList<IDetector> AvailableDetectors
		{
			get { return DataSource as IList<IDetector>; }
			set { DataSource = value; }
		}

		/// <summary>
		/// The selected detectors.
		/// </summary>
		internal IList<IDetector> SelectedDetectors
		{
			get { return _selectedDetectors.AsReadOnly(); }
		}
		#endregion

		internal DetectorSelector()
		{
			InitializeComponent();
		}

		internal DetectorSelector(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		#region Programmatic data binding
		protected override IList GetChildrenForRow(Row row)
		{
			var detectors = row.Item as IEnumerable<IDetector>;
			return (detectors == null) ? null : detectors.Sort(SortColumn.Name, SortColumn.SortDirection).ToList().AsReadOnly();
		}

		protected override void OnGetCellData(Row row, Column column, CellData cellData)
		{
			base.OnGetCellData(row, column, cellData);

			var detector = row.Item as IDetector;
			if (detector != null)
			{
				if (column == columnCheckBox)
				{
					cellData.Value = _selectedDetectors.Contains(detector);
				}
				else if (column == columnName)
				{
					cellData.Value = detector.Name;
				}
				else if (column == columnVersion)
				{
					cellData.Value = detector.VersionString();
				}
				else if (column.Name == "columnReferenceHeaders")
				{
					if (detector is ICodecDetector)
					{
						cellData.Value = GetDefaultHeaders(detector as ICodecDetector);
					}
				}
			}
		}

		private static string GetDefaultHeaders(ICodecDetector detector)
		{
			var sb = new StringBuilder();
			if (detector.ReferenceHeaders != null)
			{
				foreach (IReferenceHeader referenceHeader in detector.ReferenceHeaders)
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}


					sb.Append(GetDescriptiveName(referenceHeader));
				}
			}
			return sb.ToString();
		}

		private static string GetDescriptiveName(IReferenceHeader referenceHeader)
		{
			var sb = new StringBuilder();
			sb.Append(referenceHeader.Brand);
			if (!string.IsNullOrEmpty(referenceHeader.Model))
			{
				if (sb.Length > 0)
				{
					sb.Append(' ');
				}
				sb.Append(referenceHeader.Model);
			}
			if (!string.IsNullOrEmpty(referenceHeader.Setting))
			{
				if (sb.Length > 0)
				{
					sb.Append(' ');
				}
				sb.Append('(');
				sb.Append(referenceHeader.Setting);
				sb.Append(')');
			}
			return sb.ToString();
		}

		protected override bool SetValueForCell(Row row, Column column, object oldValue, object newValue)
		{
			if (column == columnCheckBox)
			{
				if (true.Equals(newValue))
				{
					_selectedDetectors.Add(row.Item as IDetector);
				}
				else
				{
					_selectedDetectors.Remove(row.Item as IDetector);
				}
			}
			return true;
		}
		#endregion Programmatic data binding
	}
}
