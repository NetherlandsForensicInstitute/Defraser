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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Defraser.GuiCe.sendtolist
{
	public sealed class SendToItemFactory
	{
		private static readonly string[] HexWorkshopApplicationNames = new[] {"HWorks64.exe", "HWorks32.exe"};
		private readonly IFormFactory _formFactory;

		public SendToItemFactory(IFormFactory formFactory)
		{
			_formFactory = formFactory;
		}

		public ISendToItem Create(string name, string path, string parameters)
		{
			if (IsHexWorkshopPath(path))
				return CreateHexWorkshop(name, path, parameters);
			return CreatePlayer(name, path, parameters);
		}

		private ISendToItem CreatePlayer(string name, string path, string parameters)
		{
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				return new PathValidatingSendToItem(name, new SendToPlayer(path,parameters,_formFactory));
			}
			return new InvalidSendToItem(name, path, parameters);
		}

		private ISendToItem CreateHexWorkshop(string name, string path, string parameters)
		{
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				return new PathValidatingSendToItem(name, new SendCopyToHexWorkshop(path, parameters, _formFactory));
			}
			return new InvalidSendToItem(name, path, parameters);
		}

		private sealed class SendCopyToHexWorkshop : SendToPlayer
		{
			public SendCopyToHexWorkshop(string path, string parameters, IFormFactory formFactory)
				: base(path, parameters, formFactory)
			{
			}

			public override bool IsHexWorkShop()
			{
				return true;
			}
		}

		public ISendToItem LoadFromSettings(string serializedItem)
		{
			string[] settingsArray = serializedItem.Split(new[] {';'});
            // Earlier versions didn't have the 'parameters' item
			Debug.Assert(settingsArray.Length == 2 || settingsArray.Length == 3);
			var name = settingsArray[0];
			var path = settingsArray[1];
            var parameters = settingsArray.Length == 3 ? settingsArray[2] : "";
			return Create(name, path, parameters);
		}

		private static bool IsHexWorkshopPath(string path)
		{
			foreach (var hexExeFileName in HexWorkshopApplicationNames)
			{
				if (path.EndsWith(hexExeFileName, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		}

		public void AddHexWorkshopTo(Action<ISendToItem> handler)
		{
			const string breakPoint = @"Software\BreakPoint";
			const string bookmarkAssociations = "bookmarkassociations";
			// Get all hex workshop versions on this system
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(breakPoint))
			{
				if (registryKey == null) return;

				var versions = new Dictionary<int, string>();
				string[] subKeyNames = registryKey.GetSubKeyNames();
				if (subKeyNames.Length == 0) return;
				foreach (string subKeyName in subKeyNames)
				{
					if (subKeyName.Contains("Hex Workshop"))
					{
						int version = GetHexWorkshopVersionFromString(subKeyName);
						if (!versions.ContainsKey(version))
						{
							versions.Add(GetHexWorkshopVersionFromString(subKeyName), subKeyName);
						}
					}
				}

				if (versions.Count == 0) return;

				// Use the key containing the highest version number to determine the path to HexWorkshop
				int highestVersion = versions.Keys.Max();

				string subKeyNameBookmark = string.Format(@"{0}\{1}\{2}", breakPoint, versions[highestVersion], bookmarkAssociations);
				using (RegistryKey registryKeyBookmark = Registry.CurrentUser.OpenSubKey(subKeyNameBookmark))
				{
					if (registryKeyBookmark == null) return;

					const string keyEnd = @"\BookMarks\icon";
					const string keyStart = @".ico=";
					string[] valueNames = registryKeyBookmark.GetValueNames();
					if (valueNames.Length == 0) return;
					foreach (string valueName in valueNames)
					{
						var value = registryKeyBookmark.GetValue(valueName) as string;
						if (!string.IsNullOrEmpty(value) && value.StartsWith(keyStart) && value.Length > keyEnd.Length)
						{
							int endIndex = value.IndexOf(keyEnd, StringComparison.InvariantCultureIgnoreCase);
							if (endIndex < 0)
								continue;
							string folderPath = value.Substring(keyStart.Length, endIndex - keyStart.Length);

							foreach (var exeFileName in HexWorkshopApplicationNames)
							{
								var exePath = Path.Combine(folderPath, exeFileName);
								if (File.Exists(exePath))
								{
                                    string defaultParameters = "[DAT]"; // the default is the selected headers
									var item = CreateHexWorkshop(string.Format("Hex Workshop {0}.{1}", highestVersion.ToString()[0], highestVersion.ToString()[1]), exePath, defaultParameters);
									handler(item);
								}
							}
						}
					}
				}
			}
		}

		private static int GetHexWorkshopVersionFromString(string subKeyName)
		{
			// Get all digits from the string
			var stringBuilder = new StringBuilder();
			foreach (char c in subKeyName)
			{
				if (Char.IsDigit(c))
				{
					stringBuilder.Append(c);
				}
			}
			// Append zero's to create a four digit number
			while (stringBuilder.Length < 4)
			{
				stringBuilder.Append('0');
			}
			return int.Parse(stringBuilder.ToString());
		}
	}
}
