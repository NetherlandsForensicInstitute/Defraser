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

using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.Cavlc;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	/// <summary>
	/// The <see cref="IModule"/> containing the components of the H.264 detector.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public sealed class H264Module : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register implementations
			builder.Register(c => new Metadata(CodecID.H264, c.Resolve<IScanContext>().Detectors)).As<IMetadata>().SingletonScoped();
			builder.Register<H264ResultNodeCallback>().As<IResultNodeCallback>().ContainerScoped();
			builder.Register<H264ResultTreeBuilder>().As<IDataBlockCarver>().ContainerScoped();
			builder.Register<H264Reader>().As<IH264Reader>().As<IReader>().ContainerScoped();
			builder.Register<NalUnitParser>().As<INalUnitParser>().As<IDetectorColumnsInitializer>().SingletonScoped();
			builder.Register<ByteStreamParser>();
			builder.Register<NalUnitStreamParser>();
			builder.Register(c => c.Resolve<IDataReader>() as BitStreamDataReader).As<BitStreamDataReader>().ContainerScoped();
			#endregion Register implementations

			#region Register NAL unit parsers
			builder.RegisterCollection<INalUnitPayloadParser>();
			builder.Register<SequenceParameterSet>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<PictureParameterSet>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<SupplementalEnhancementInformation>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<IdrPictureSlice>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<NonIdrPictureSlice>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<EndOfSequence>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<EndOfStream>().As<INalUnitPayloadParser>().MemberOf<IEnumerable<INalUnitPayloadParser>>();
			builder.Register<Slice>().FactoryScoped();
			builder.Register<SliceHeader>().FactoryScoped();
			builder.Register<SliceData>().FactoryScoped();
			#endregion Register NAL unit parsers

			builder.RegisterModule(new H264StateModule());
		}
	}
}
