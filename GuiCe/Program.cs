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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autofac.Builder;
using Defraser.Framework;

namespace Defraser.GuiCe
{
	static class Program
	{
		/// <summary>The regular expression for uninstall argument: <b>/u=[PRODUCT_CODE]</b>.</summary>
		private const string UninstallRegex = @"/[uU]=(.+)";

        [STAThread]
		static void Main()
		{
            // Check for command line arguments to uninstall the application
            // and perform the uninstallation when required.
            foreach (string arg in Environment.GetCommandLineArgs())
			{
				Match match = Regex.Match(arg, UninstallRegex);
				if (match.Success)
				{
					Uninstall(match.Result("$1"));
					return;
				}
			}

			// Normal application initialisation and start
			Application.AddMessageFilter(new TabKeyCatcher());
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var builder = new ContainerBuilder();

			builder.RegisterModule(new FrameworkModule());
			builder.RegisterModule(new DataContractSerializerModule());
			builder.RegisterModule(new GuiCeModule());

			using (var container = builder.Build())
			{
				// Open the main interface
				MainForm mainForm = container.Resolve<MainForm>();
				Application.Run(mainForm);
			}       
		}

		private static void Uninstall(string productCode)
		{
			string path = Environment.GetFolderPath(System.Environment.SpecialFolder.System);
			string msiexecPath = Path.Combine(path, "msiexec.exe");
			ProcessStartInfo msiexecProcess = new ProcessStartInfo(msiexecPath);
			msiexecProcess.Arguments = string.Format("/x {0}", productCode);
			msiexecProcess.UseShellExecute = false;
			Process.Start(msiexecProcess);
		}
	}
}
