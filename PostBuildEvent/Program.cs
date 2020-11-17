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
using System.IO;
using System.Linq;

namespace Defraser.PostBuildEvent
{
	class Program
	{
		enum Command
		{
			Undefined,
			RemoveProjectFromSolutionFile,
			RemoveLicensesLicxLineFormProjectFile
		}

		enum ExitCode
		{
			InvalidNumberOfArguments = 1,
			CanNotOpenOutputFile,
			CanNotOpenFileForReading,
			CanNotOpenExcludeFile,
			UnknownCommand,
			ProjectNotFoundInSolution,
			ProjectGuidNotFound,
		}

		static void Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Console.WriteLine("No arguments specified");
				Environment.Exit((int)ExitCode.InvalidNumberOfArguments);
			}

			int expectedArgumentCount;
			Command command = Command.Undefined;
			if(args[0] == "-rl")
			{
				command = Command.RemoveLicensesLicxLineFormProjectFile;
				expectedArgumentCount = 3;
			}
			else if (args[0] == "-rp")
			{
				command = Command.RemoveProjectFromSolutionFile;
				expectedArgumentCount = 4;
			}
			else
			{
				expectedArgumentCount = 0;
				Console.WriteLine("Unknown command argument '{0}'. Expected '-rl' or '-rp'", args[0]);
				Environment.Exit((int)ExitCode.UnknownCommand);
			}

			if (args.Length != expectedArgumentCount)
			{
				Console.WriteLine("Invalid Number Of Arguments. Expected {0} arguments but got {1} arguments", expectedArgumentCount, args.Length);
				Environment.Exit((int)ExitCode.InvalidNumberOfArguments);
			}

			switch (command)
			{
				case Command.Undefined:
					break;
				case Command.RemoveLicensesLicxLineFormProjectFile:
					RemoveLicensesLicxLineFromProjectFile(args);
					break;
				case Command.RemoveProjectFromSolutionFile:
					RemoveProjectFromSolutionFile(args);
					break;
			}
		}

		private static string[] GetAllInputLines(string inputFilename)
		{
			string[] allInputLines = null;
			try
			{
				allInputLines = File.ReadAllLines(inputFilename);
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to read from file {0}. {1}", inputFilename, e.Message);
				Environment.Exit((int)ExitCode.CanNotOpenFileForReading);
			}
			return allInputLines;
		}

		private static void RemoveProjectFromSolutionFile(string[] args)
		{
			if (args.Length != 4) return;

			string projectName = args[1];
			string inputFilename = args[2];
			string outputFilename = args[3];

			string[] allInputLines = GetAllInputLines(inputFilename);
			if (allInputLines == null || allInputLines.Length == 0) return;

			try
			{
				string lineContainingProjectName = allInputLines.SingleOrDefault(line => line.Contains(projectName));
				if (string.IsNullOrEmpty(lineContainingProjectName))
				{
					Console.WriteLine("Project '{0}' not found in the input file '{1}'", projectName, inputFilename);
					Environment.Exit((int)ExitCode.ProjectNotFoundInSolution);
				}

				// Get the project GUID of the project to be removed
				int projectGuidStartPosition = lineContainingProjectName.LastIndexOf('{');
				if (projectGuidStartPosition == -1)
				{
					Console.WriteLine("Project GUID not found in line '{0}'", lineContainingProjectName);
					Environment.Exit((int)ExitCode.ProjectGuidNotFound);
				}
				projectGuidStartPosition++;
				int projectGuidEndPosition = lineContainingProjectName.LastIndexOf('}');
				if (projectGuidEndPosition == -1)
				{
					Console.WriteLine("Project GUID not found in line '{0}'", lineContainingProjectName);
					Environment.Exit((int)ExitCode.ProjectGuidNotFound);
				}
				int projectGuidLength = projectGuidEndPosition - projectGuidStartPosition;

				string projectGuid = lineContainingProjectName.Substring(projectGuidStartPosition, projectGuidLength);

				// Filter the lines containing the
				// - '<projectName>' and the next line containing 'EndProject'.
				// - lines containing the project GUID
				List<string> allOutputLines = new List<string>();
				string previousLine = string.Empty;
				foreach (string line in allInputLines)
				{
					if (!line.Contains(projectName) &&
					    !previousLine.Contains(projectName) &&
					    !line.Contains(projectGuid))
					{
						allOutputLines.Add(line);
					}
					previousLine = line;
				}
				File.WriteAllLines(outputFilename, allOutputLines.ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to write to file {0}. {1}", outputFilename, e.Message);
				Environment.Exit((int)ExitCode.CanNotOpenOutputFile);
			}
		}

		/// <summary>
		/// Remove the line containing 'licenses.licx' from the GuiCe project file
		/// </summary>
		/// <param name="args"></param>
		/// <param name="allInputLines"></param>
		private static void RemoveLicensesLicxLineFromProjectFile(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Expected 3 arguments, got "+args.Length);
				return;
			}

			string inputFilename = args[1];
			string outputFilename = args[2];

			string[] allInputLines = GetAllInputLines(inputFilename);
			if (allInputLines == null || allInputLines.Length == 0)
			{
				Console.WriteLine("File is empty");
				return;
			}

			try
			{
				// Filter the line containing the text 'licenses.licx'
				List<string> allOutputLines = new List<string>();
				foreach (string line in allInputLines)
				{
					if (!line.Contains("licenses.licx"))
					{
						allOutputLines.Add(line);
					}
				}
				File.WriteAllLines(outputFilename, allOutputLines.ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to write to file {0}. {1}", outputFilename, e.Message);
				Environment.Exit((int)ExitCode.CanNotOpenOutputFile);
			}
		}

#if _TODO_perform_gzip_in_post_build_event_
	// Create a gzip containing the source code
		static void Main(string[] args)
		{
			const int ExcpectedNumberOfArguments = 3;
			if (args.Length != ExcpectedNumberOfArguments)
			{
				Console.WriteLine("Invalid Number Of Arguments. Expected {0} arguments but got {1} arguments", ExcpectedNumberOfArguments, args.Length);
				Environment.Exit((int)ExitCode.InvalidNumberOfArguments);
			}

			string targetPath = args[0];
			string sourceDirectory = args[1];
			string excludeListPath = args[2];

			Console.WriteLine("Creating source zip file: '{0}'.", targetPath);

			FileStream fileStream = null;
			GZipStream zipStream = null;
			try
			{
				fileStream = File.OpenWrite(targetPath);
				zipStream = new GZipStream(fileStream, CompressionMode.Compress);

				List<string> excludeList = CreateExludeList(excludeListPath);
				// Exclude the zipfile itself
				excludeList.Add(Path.GetFileName(targetPath));

				ZipSource(zipStream, sourceDirectory, excludeList);
			}
			catch (Exception e)
			{
				Console.WriteLine("Can not open {0} for writing. {1}", fileStream, e.Message);
				Environment.Exit((int)ExitCode.CanNotOpenOutputFile);
			}
			finally
			{
				zipStream.Close();
				fileStream.Close();
			}
		}

		private static List<string> CreateExludeList(string excludeListPath)
		{
			List<string> excludeList = new List<string>();

			try
			{
				foreach (string line in File.ReadAllLines(excludeListPath))
				{
					excludeList.Add(line.Replace('/', Path.DirectorySeparatorChar));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Fail to open exclude file {0}. {1}", excludeListPath, e.Message);
				Environment.Exit((int)ExitCode.CanNotOpenExcludeFile);
			}

			foreach (string line in excludeList)
			{
				Console.WriteLine("lines in exclude list: {0}", line);
			}

			return excludeList;
		}

		static void ZipSource(GZipStream zipStream, string sourceDirectory, List<string> excludeList)
		{
			byte[] buffer = new byte[1024];
			foreach(string file in Directory.GetFiles(sourceDirectory))
			{
				FileStream inputFile = null;
				try
				{
					if (InExcludeList(excludeList, file)) continue;

					Console.WriteLine("Adding file: {0}", file);

					inputFile = File.OpenRead(file);
					int bytesRead = 0;
					do
					{
						bytesRead = inputFile.Read(buffer, 0, buffer.Length);
						zipStream.Write(buffer, 0, bytesRead);
					}
					while (bytesRead > 0);
				}
				catch (Exception e)
				{
					Console.WriteLine("Failed to add file {0}. {1}", file, e.Message);
					Environment.Exit((int)ExitCode.CanNotOpenFileForReading);
				}
				finally
				{
					if (inputFile != null)
					{
						inputFile.Close();
					}
				}
			}

			foreach(string subdirectory in Directory.GetDirectories(sourceDirectory))
			{
				if (InExcludeList(excludeList, subdirectory)) continue;

				ZipSource(zipStream, subdirectory, excludeList);
			}
		}

		private static bool InExcludeList(List<string> excludeList, string path)
		{
			foreach(string line in excludeList)
			{
				if (string.IsNullOrEmpty(line)) continue;

				// exclude rule between '*'
				if (line.StartsWith("*") && line.EndsWith("*"))
				{
					string substring = line.Substring(1, line.Length - 2);
					if (path.Contains(line.Substring(1, line.Length - 2)))
					{
						return true;
					}
				}
				// exclude rule start "./*" and ends with "*"
				else if (line.StartsWith("./*") && line.EndsWith("*"))
				{
					string substring = line.Substring(3, line.Length - 4);

					if (path.Contains(line.Substring(3, line.Length - 4)))
					{
						return true;
					}
				}
				// exclude rule ends with '*'
				else if (line.EndsWith("*"))
				{
					if (path.Contains(line.Substring(0, line.Length - 2)))
					{
						return true;
					}
				}
				// exclude rule starts with '*'
				else if (line.StartsWith("*"))
				{
					if (path.Contains(line.Substring(1)))
					{
						return true;
					}
				}
				// exclude rule contains no '*'
				else
				{
					if(path.Contains(line))
					{
						return true;
					}
				}
			}
			return false;
		}
#endif // _TODO_perform_gzip_in_post_build_event_
	}
}
