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

using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Defraser.GuiCe
{
	public partial class AboutBox : Comcomr.AboutBox
	{
		/// <summary>
		/// The license text
		/// </summary>
		private static string _licenseText;
		/// <summary>The regular expression for locating the copyright start- and end year.</summary>
		private const string CopyrightRegex = @"[cC]opyright\D*(\d+)-(\d+), N\w+ F\w+ I\w+";

		#region Properties
		/// <summary>The Defraser license text.</summary>
		private static string LicenseText
		{
			get
			{
				if(string.IsNullOrEmpty(_licenseText))
				{
					const string resourceUri = "Defraser.GuiCe.license.txt";

					Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceUri);
					if(stream != null)
					{
						using(StreamReader streamReader = new StreamReader(stream))
						{
							_licenseText = streamReader.ReadToEnd();
						}
					}
				}
				return _licenseText;
			}
		}
		/// <summary>The year when development started.</summary>
		private static int CopyrightStartYear
		{
			get
			{
				Match match = Regex.Match(LicenseText, CopyrightRegex);
				return match.Success ? int.Parse(match.Result("$1")) : -1;
			}
		}
		/// <summary>The year of the current version.</summary>
		private static int CopyrightEndYear
		{
			get
			{
				Match match = Regex.Match(LicenseText, CopyrightRegex);
				return match.Success ? int.Parse(match.Result("$2")) : -1;
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="AboutBox"/>.
		/// </summary>
		public AboutBox()
			: base(MainForm.ApplicationName, string.Empty, MainForm.ApplicationVersion,
				   CopyrightStartYear, CopyrightEndYear, LicenseText)
		{
			InitializeComponent();
		}
	}
}
