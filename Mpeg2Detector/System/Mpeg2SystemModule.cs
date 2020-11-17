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

using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.System.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	/// <summary>
	/// The <see cref="IModule"/> containing the components of the MPEG-2 system detector.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public sealed class Mpeg2SystemModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register implementations
			builder.Register<ResultNodeCallback>().As<IResultNodeCallback>().ContainerScoped();
			builder.Register<Mpeg2SystemCarver>().As<IDataBlockCarver>().ContainerScoped();
			builder.Register<Mpeg2SystemReader>().As<IMpeg2SystemReader>().As<IReader>().ContainerScoped();
			builder.Register<Mpeg2SystemHeaderParser>().As<IResultParser<IMpeg2SystemReader>>().As<IDetectorColumnsInitializer>().ContainerScoped();
			builder.Register(c => c.Resolve<IDataReader>() as BitStreamDataReader).FactoryScoped();
			#endregion Register implementations

			#region Register system headers
			builder.RegisterCollection<ISystemHeaderParser>();
			builder.Register<ProgramEndCode>().As<ISystemHeaderParser>().MemberOf<IEnumerable<ISystemHeaderParser>>();
			builder.Register<PackHeader>().As<ISystemHeaderParser>().MemberOf<IEnumerable<ISystemHeaderParser>>();
			builder.Register<SystemHeader>().As<ISystemHeaderParser>().MemberOf<IEnumerable<ISystemHeaderParser>>();
			builder.Register<ProgramStreamMap>().As<ISystemHeaderParser>().MemberOf<IEnumerable<ISystemHeaderParser>>();
			builder.Register<ProgramStreamDirectory>().As<ISystemHeaderParser>().MemberOf<IEnumerable<ISystemHeaderParser>>();
			builder.Register<PesPacket>(); // Note: Should not be registered as part of the header collection!!
			#endregion Register system headers

			builder.RegisterModule(new Mpeg2SystemStateModule());
		}
	}
}
