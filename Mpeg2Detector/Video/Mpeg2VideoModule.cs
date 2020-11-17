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
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	/// <summary>
	/// The <see cref="IModule"/> containing the components of the MPEG-2 video detector.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public sealed class Mpeg2VideoModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register implementations
			builder.Register<ResultNodeCallback>().As<IResultNodeCallback>().ContainerScoped();
			builder.Register<Mpeg2VideoCarver>().As<IDataBlockCarver>().ContainerScoped();
			builder.Register<Mpeg2VideoReader>().As<IMpeg2VideoReader>().As<IReader>().ContainerScoped();
			builder.Register<Mpeg2VideoHeaderParser>().As<IResultParser<IMpeg2VideoReader>>().As<IDetectorColumnsInitializer>().SingletonScoped();
			builder.Register(c => c.Resolve<IDataReader>() as BitStreamDataReader).FactoryScoped();
			#endregion Register implementations

			#region Register video headers
			builder.RegisterCollection<IVideoHeaderParser>();
			builder.Register<PictureHeader>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<UserData>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<SequenceHeader>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<ExtensionParser>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<SequenceEndCode>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<GroupOfPicturesHeader>().As<IVideoHeaderParser>().MemberOf<IEnumerable<IVideoHeaderParser>>();
			builder.Register<Slice>(); // Note: Should not be registered as part of the header collection!!
			#endregion Register video headers

			#region Register extensions (MPEG-2)
			builder.RegisterCollection<IExtensionParser>();
			builder.Register<SequenceExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<SequenceDisplayExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<QuantMatrixExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<CopyrightExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<SequenceScalableExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<PictureDisplayExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<PictureCodingExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<PictureSpatialScalableExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<PictureTemporalScalableExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<CameraParametersExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			builder.Register<ItuTExtension>().As<IExtensionParser>().MemberOf<IEnumerable<IExtensionParser>>();
			#endregion Register extensions (MPEG-2)

			builder.RegisterModule(new Mpeg2VideoStateModule());
		}
	}
}
