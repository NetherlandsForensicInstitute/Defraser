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

using Autofac;
using Autofac.Builder;
using Defraser.DataStructures;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// The <see cref="IModule"/> containing common detector components.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public sealed class DetectorCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register implementations
			builder.Register<Carver.Carver>().As<ICarver>().FactoryScoped();
			builder.Register<DetectorColumnsBuilder>().As<IDetectorColumnsBuilder>().FactoryScoped();
			builder.Register(c => CreateDetectorColumns(c.Resolve<IDetectorColumnsBuilder>(), c.Resolve<IDetectorColumnsInitializer>())).FactoryScoped();
			builder.Register<ResultAttributeBuilder>().As<IResultAttributeBuilder>().FactoryScoped();
			builder.Register<ResultNodeBuilder>().As<IResultNodeBuilder>().FactoryScoped();
			builder.RegisterGeneric(typeof(EnumResultFormatter<>)).FactoryScoped();
			#endregion Register implementations

			#region Register generated factories
			builder.Register<Creator<ICarverState, IDataReader, IDataBlockBuilder, IScanContext>>(c => (r, b, s) => CreateCarver(c, r, b, s));
			#endregion Register generated factories
		}

		private static ICarverState CreateCarver(IContext context, IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext scanContext)
		{
			IContainer childContainer = context.Resolve<IContainer>().CreateInnerContainer();
			var builder = new ContainerBuilder();
			// Register parameters
			builder.Register(dataReader).As<IDataReader>();
			builder.Register(dataBlockBuilder).As<IDataBlockBuilder>();
			builder.Register(scanContext).As<IScanContext>();
			// Register carver components and build child container
			builder.RegisterModule(new DetectorCommonCarverModule());
			builder.Build(childContainer);
			return childContainer.Resolve<ICarverState>();
		}

		private static IDetectorColumns CreateDetectorColumns(IDetectorColumnsBuilder detectorColumnsBuilder, IDetectorColumnsInitializer detectorColumnsInitializer)
		{
			detectorColumnsInitializer.AddColumnsTo(detectorColumnsBuilder);
			return detectorColumnsBuilder.Build();
		}
	}
}
