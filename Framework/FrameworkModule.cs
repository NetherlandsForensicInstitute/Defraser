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
using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.FFmpegConverter;
using Defraser.FFmpegConverter.FFmpeg;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="IModule"/> containing the components of the Defraser Framework.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public class FrameworkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register implementations
			builder.Register<ByteArrayDataReader>().FactoryScoped();
			builder.Register<CodecStreamBuilder>().FactoryScoped().As<ICodecStreamBuilder>();
			builder.Register<DataBlockBuilder>().FactoryScoped().As<IDataBlockBuilder>();
			builder.Register<DataBlockScanner>();
			builder.Register<DataPacket>().FactoryScoped().As<IDataPacket>();
			builder.Register<DataReaderPool>().FactoryScoped().As<IDataReaderPool>();
			builder.Register<DataScanner>().FactoryScoped().As<IDataScanner>();
			builder.Register<DetectorFactory>().SingletonScoped().As<IDetectorFactory>();
			builder.Register<ExportToXml>();
			builder.Register<FFmpegManager>().SingletonScoped();
			builder.Register<FrameConverter>().FactoryScoped();
			builder.Register(c => IntPtr.Size == 8 ? new FFmpeg64() : (IFFmpeg)new FFmpeg32()).As<IFFmpeg>().ContainerScoped();
			builder.Register<FileDataReader>().FactoryScoped().As<IDataReader>();
			builder.Register<FileDataWriter>().FactoryScoped().As<IDataWriter>();
			builder.Register<FileExport>().As<IFileExport>();
			builder.Register<FileScanner>().FactoryScoped().As<IFileScanner>();	// TODO: singleton?
			builder.Register<ForensicIntegrityLog>().As<IForensicIntegrityLog>();
			builder.Register<InputFile>().FactoryScoped().As<IInputFile>();
			builder.Register<Project>().FactoryScoped().As<IProject>();
			builder.Register<ProjectManager>();
			builder.Register<SaveAsContiguousFile>().As<IExportStrategy<IDataPacket>>();
			builder.Register<SaveAsSeparateFiles>().As<IExportStrategy<IEnumerable<object>>>();
			builder.Register<SaveAsSingleFile>().As<IExportStrategy<IInputFile>>();
			builder.Register<ScanContext>().FactoryScoped().As<IScanContext>();
			builder.Register<StreamWriter>().FactoryScoped().As<TextWriter>();
			builder.Register<DefaultDetectorFormatter>().As<IDetectorFormatter>();
			builder.Register<ReferenceHeaderFile>().FactoryScoped().As<IReferenceHeaderFile>();
			builder.Register<ReferenceHeaderDatabase>().SingletonScoped().As<IReferenceHeaderDatabase>();
			#endregion Register implementations

			#region Register generated factories
			builder.RegisterCreator<ByteArrayDataReader, IDataPacket, byte[]>();
			builder.RegisterCreator<ICodecStreamBuilder>();
			builder.RegisterCreator<IDataBlockBuilder>();
			builder.RegisterCreator<IDataReader, IDataPacket>();
			builder.RegisterCreator<IDataReaderPool>();
			builder.RegisterCreator<IDataPacket, IInputFile>();
			builder.RegisterCreator<IDataScanner>();
			builder.RegisterCreator<IDataWriter, string>();
			builder.RegisterCreator<IInputFile, IProject, string>();
			builder.RegisterCreator<IProject, string>();
			builder.RegisterCreator<IScanContext, IProject>();
			builder.RegisterCreator<IReferenceHeaderFile, IProject, IReferenceHeader>();
			builder.RegisterCreator<TextWriter, string>();
			#endregion Register generated factories

			#region Register factory methods for decorators and wrappers
			builder.Register<Creator<IDataPacket, IDataPacket, IDataPacket>>(c => ((x, y) => new DataPacketNode(x, y)));
			builder.Register<Creator<IDataReader, IDataPacket, IDataReaderPool>>(c => ((d, p) => new FragmentedDataReader(d, p)));
			builder.Register<Creator<IDataReader, IDataReader, IProgressReporter>>(c => ((d, p) => new ProgressDataReader(d, p)));
			builder.Register<Creator<IProgressReporter, IProgressReporter, int, int>>(c => ((p, s, e) => new SubProgressReporter(p, s, e)));
			builder.Register<Creator<IProgressReporter, IProgressReporter, long, long, long>>(c => ((p, s, l, t) => new SubProgressReporter(p, s, l, t)));
			#endregion Register factory methods for decorators and wrappers
		}
	}
}
