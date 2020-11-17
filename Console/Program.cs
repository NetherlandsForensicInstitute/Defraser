/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac.Builder;
using Defraser.Framework;
using Defraser.Interface;
using log4net;
using log4net.Config;

namespace Defraser.Console
{
	public class ConsoleTest
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			if (args.Length != 2)
			{
				System.Console.WriteLine("usage:\n  Console [filename] [storage]");
				return;
			}
			FileInfo file = new FileInfo(args[0]);
			if (!file.Exists)
			{
				Log.Error("File " + args[0] + " does not exist!");
				throw new Exception("File " + args[0] + " does not exist!");
			}

			DetectFile(args[0], args[1]);
		}

		public static void DetectFile(string filePath, string projectFileName)
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule(new FrameworkModule());
			builder.RegisterModule(new DataContractSerializerModule());

			using (var container = builder.Build())
			{
				string detectorPath = ConfigurationManager.AppSettings.Get("Defraser.DetectorPath") ?? AppDomain.CurrentDomain.BaseDirectory;
				IDetectorFactory detectorFactory = container.Resolve<IDetectorFactory>();
				detectorFactory.Initialize(detectorPath);

				File.Delete(projectFileName);
				ProjectManager projectManager = container.Resolve<ProjectManager>();
				IProject project = projectManager.CreateProject(projectFileName, "Console", DateTime.Now, "Batch process");

				IInputFile inputFile = project.AddFile(filePath, detectorFactory.ContainerDetectors.Union(detectorFactory.CodecDetectors));
				IFileScanner fileScanner = container.Resolve<IFileScanner>();
				fileScanner.ContainerDetectors = detectorFactory.ContainerDetectors;
				fileScanner.CodecDetectors = detectorFactory.CodecDetectors;
				fileScanner.DataBlockDetected += (s, e) => project.AddDataBlock(e.DataBlock);
				fileScanner.Scan(inputFile, container.Resolve<IProgressReporter>());

				projectManager.SaveProject(project);
				projectManager.CloseProject(project);
			}
		}
	}
}
