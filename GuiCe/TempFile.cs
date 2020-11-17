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
using Defraser.GuiCe.Properties;

namespace Defraser.GuiCe
{
	static class TempFile
	{
		const string TempFilePrefix = "Defraser_";

		/// <summary>The system defined temp folder</summary>
		internal static string SystemDefaultTempPath
		{
			get { return Path.GetTempPath(); }
		}

		private static string DefraserTempFileFullPath
		{
			get { return string.Format("{0}\\{1}{2}", Settings.Default.TempDirectory, TempFilePrefix, Path.GetRandomFileName()); }
		}

		/// <summary>
		/// Create a unique temporary file name (full path).
		/// If the user has specified a temporary directory, that directory will be used.
		/// </summary>
		/// <returns>the name of the temporary file</returns>
		internal static string GetTempFileName()
		{
			if (!Settings.Default.UserDefinedTempDirectory || string.IsNullOrEmpty(Settings.Default.TempDirectory))
			{
				// Note: Path.GetTempFileName() create zero-byte temporary file on disk and return the full path of that file
				string tempFileName = Path.GetTempFileName();
				try { File.Delete(tempFileName); } // delete the file created by Path.GetTempFileName()
				catch(Exception) {}

				string tempFilePath = string.Format("{0}\\{1}{2}", Path.GetDirectoryName(tempFileName), TempFilePrefix, Path.GetFileName(tempFileName));

				Trace.WriteLine(String.Format("GetTempFileName returns (no temp directory specified by user): {0}", tempFilePath));
				return tempFilePath;
			}

			string path = DefraserTempFileFullPath;
			while (File.Exists(path))
			{
				path = DefraserTempFileFullPath;
			}
			Trace.WriteLine(String.Format("GetTempFileName returns (temp directory specified by user): {0}", path));
			return path;
		}

		/// <summary>
		/// Delete the defraser temp files from both windows temp directory
		/// and the user defined directory (if that exists)
		/// </summary>
		internal static void RemoveTempFiles()
		{
			if (!string.IsNullOrEmpty(Settings.Default.TempDirectory))
			{
				DeleteFiles(Settings.Default.TempDirectory);
			}
			DeleteFiles(SystemDefaultTempPath);
		}

		private static void DeleteFiles(string directory)
		{
			string[] tempFiles = Directory.GetFiles(directory, String.Format("{0}*", TempFilePrefix), SearchOption.TopDirectoryOnly);

			foreach (string tempFile in tempFiles)
			{
				try { File.Delete(tempFile); }
				catch (Exception) { }
			}
		}
	}
}
