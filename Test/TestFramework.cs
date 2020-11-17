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
using System.Runtime.Serialization;
using System.Text;
using Autofac;
using Autofac.Builder;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Test.Common;
using Defraser.Util;
using Rhino.Mocks;

namespace Defraser.Test
{
	public static class TestFramework
	{
		private static readonly IContainer _container;

		#region Properties
		internal static IDetectorFactory DetectorFactory
		{
			get
			{
				IDetectorFactory detectorFactory = _container.Resolve<IDetectorFactory>();
				return detectorFactory;
			}
		}

		internal static DataBlockScanner DataBlockScanner
		{
			get
			{
				return _container.Resolve<DataBlockScanner>();
			}
		}

		internal static ProjectManager ProjectManager
		{
			get
			{
				return _container.Resolve<ProjectManager>();
			}
		}

		internal static IDataContractSurrogate DataContractSurrogate
		{
			get
			{
				return _container.Resolve<IDataContractSurrogate>();
			}
		}
		#endregion Properties

		static TestFramework()
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule(new FrameworkModule());
			builder.RegisterModule(new DataContractSerializerModule());
			builder.RegisterCreator<IDataReaderPool>();

			_container = builder.Build();
		}

		internal static IDataReaderPool CreateDataReaderPool()
		{
			return _container.Resolve<IDataReaderPool>();
		}

		internal static FrameConverter CreateFrameConverter()
		{
			return _container.Resolve<FrameConverter>();
		}

		internal static Creator<IDataReaderPool> CreateDataReaderPoolCreator()
		{
			return _container.Resolve<Creator<IDataReaderPool>>();
		}

		internal static XmlObjectSerializer CreateXmlObjectSerializer()
		{
			return _container.Resolve<XmlObjectSerializer>();
		}

		internal static IInputFile CreateInputFile(string name)
		{
			IProject project = MockRepository.GenerateStub<IProject>();
			return _container.Resolve<Creator<IInputFile, IProject, string>>()(project, name);
		}

		internal static IInputFile DetectData(IDetector detector, IProject project, string fileName)
		{
			return DetectData(new IDetector[0], new[] { detector }, project, fileName);
		}

		internal static IInputFile DetectData(IList<IDetector> containerDetectors, IList<IDetector> codecDetectors, IProject project, string fileName)
		{
			IFileScanner fileScanner = _container.Resolve<IFileScanner>();
			fileScanner.ContainerDetectors = containerDetectors;
			fileScanner.CodecDetectors = codecDetectors;
			fileScanner.DataBlockDetected += (s, e) => project.AddDataBlock(e.DataBlock);
			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);
			IInputFile inputFile = project.AddFile(fileName, detectors);
			fileScanner.Scan(inputFile, new NullProgressReporter());
			return inputFile;
		}

		internal static IDataBlock CreateDataBlock(IDetector detector, IDataPacket data, bool isFullFile, IDataBlock lastDataBlock)
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder();
			dataBlockBuilder.DataFormat = CodecID.Unknown;
			dataBlockBuilder.Detectors = new[] { detector };
			dataBlockBuilder.InputFile = data.InputFile;
			dataBlockBuilder.StartOffset = data.StartOffset;
			dataBlockBuilder.EndOffset = data.EndOffset;
			dataBlockBuilder.IsFullFile = isFullFile;
			dataBlockBuilder.IsFragmented = false;
			return dataBlockBuilder.Build();
		}

		internal static IDataReader CreateDataReader(IInputFile file)
		{
			return CreateDataReaderPool().CreateDataReader(CreateDataPacket(file, 0, file.Length));
		}

		internal static IDataBlockBuilder CreateDataBlockBuilder()
		{
			return _container.Resolve<IDataBlockBuilder>();
		}

		internal static IScanContext CreateScanContext(IProject project)
		{
			return _container.Resolve<Creator<IScanContext, IProject>>()(project);
		}

		internal static IDataPacket CreateDataPacket(IInputFile inputFile, long offset, long length)
		{
			Creator<IDataPacket, IInputFile> createDataPacket = _container.Resolve<Creator<IDataPacket, IInputFile>>();
			return createDataPacket(inputFile).GetSubPacket(offset, length);
		}

		/// <summary>
		/// Writes <paramref name="results"/> recursively to file with
		/// name <paramref name="fileName"/>.
		/// </summary>
		/// <param name="results">the results to write</param>
		/// <param name="fileName">the name of the file to write to/param>
		internal static void WriteResults(IResultNode[] results, string fileName)
		{
			long dummy = 0L;
			using (IDataReaderPool dataReaderPool = TestFramework.CreateDataReaderPool())
			using (ResultWriter writer = new ResultWriter(File.Create(fileName), dataReaderPool))
			{
				foreach (IResultNode result in results)
				{
					writer.WriteResult(result, null, ref dummy, 0L);
				}
			}
		}

		internal static void SaveAsSeparateFiles(IEnumerable<object> items, string directory)
		{
			_container.Resolve<IFileExport>().SaveAsSeparateFiles(items, directory, false);
		}

		internal static string ExportToXml(IEnumerable<IFragment> fragments)
		{
			StringBuilder xmlStringBuider = new StringBuilder();
			ExportToXml exportToXml = new ExportToXml((f) => new StringWriter(xmlStringBuider), DataBlockScanner, new DefaultDetectorFormatter());
			using (IDataReaderPool dataReaderPool = _container.Resolve<IDataReaderPool>())
			{
				exportToXml.Export(fragments, dataReaderPool, "<path>", MockRepository.GenerateStub<IProgressReporter>());
				return xmlStringBuider.ToString();
			}
		}

		internal static IResultNode GetResults(IFragment fragment)
		{
			using (IDataReaderPool dataReaderPool = _container.Resolve<IDataReaderPool>())
			{
				return DataBlockScanner.GetResults(fragment, new NullProgressReporter(), dataReaderPool);
			}
		}

		public static String GetDescriptionText(IResultNode result)
		{
			var descr = new ResultNodeDescriptionBuilder();
			new ResultNodeDescriber().Describe(result, descr);
			var descrText = descr.ToString();
			return descrText;
		}

		/// <summary>
		/// Writes <paramref name="detectables"/> sequentially to file with
		/// name <paramref name="fileName"/>.
		/// </summary>
		/// <param name="detectables">the detectables to write</param>
		/// <param name="fileName">the name of the file to write to</param>
		/// <typeparam name="T">the type of detectables to write</typeparam>
		internal static void WriteDetectables<T>(IList<T> detectables, string fileName, IProgressReporter progressReporter, ref long handledBytes, long totalBytes) where T : IDataPacket
		{
			if (detectables == null)
			{
				throw new ArgumentNullException("detectables");
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException("fileName");
			}
			using (IDataReaderPool dataReaderPool = CreateDataReaderPool())
			using (ResultWriter writer = new ResultWriter(File.Create(fileName), dataReaderPool))
			{
				foreach (T detectable in detectables)
				{
					IDataPacket dataPacket;

					if (detectable is ICodecStream)
					{
						// It's a codec stream
						dataPacket = DataBlockScanner.GetData(detectable as ICodecStream, progressReporter, dataReaderPool);
					}
					else
					{
						// It's a container stream
						dataPacket = detectable;
					}

					writer.WriteDataPacket(dataPacket, progressReporter, ref handledBytes, totalBytes);
				}
			}
		}
	}
}
