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
using System.Runtime.Serialization;
using Autofac;
using Autofac.Builder;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The <see cref="IModule"/> containing the <see cref="DataContractSerializer"/>
	/// implementation and related components for the Defraser Framework.
	/// </summary>
	/// <remarks>
	/// Some registrations of this module depend on the <see cref="FrameworkModule"/>
	/// for proper operation.
	/// </remarks>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public class DataContractSerializerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			#region Register DataContractSerializer
			builder.Register(c => CreateDataContractSerializer(typeof(Project), c.Resolve<IDataContractSurrogate>())).FactoryScoped().As<XmlObjectSerializer>();
			builder.Register<SerializationContextDataContractSurrogate>().As<IDataContractSurrogate>()
				.OnActivating((s, e) => e.Instance = new DetectorDataContractSurrogate(e.Context.Resolve<IDetectorFactory>(), e.Instance as IDataContractSurrogate));
			#endregion Register DataContractSerializer

			#region Register serialization contexts
			builder.RegisterCollection<ISerializationContext>();
			builder.Register<CodecStreamBuilder.CodecStream.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<DataBlockBuilder.DataBlock.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<DataPacket.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<InputFile.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<Project.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<ReferenceHeaderFile.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			builder.Register<ReferenceHeaderDatabase.SerializationContext>().As<ISerializationContext>().MemberOf<IEnumerable<ISerializationContext>>();
			#endregion Register serialization contexts

			#region Register generated factories
			builder.RegisterCreator<XmlObjectSerializer>();
			#endregion Register generated factories

			#region Register factory methods
			builder.Register<Creator<XmlObjectSerializer, Type>>(c => (t => CreateDataContractSerializer(t, c.Resolve<IDataContractSurrogate>())));
			#endregion Register factory methods
		}

		private static DataContractSerializer CreateDataContractSerializer(Type type, IDataContractSurrogate dataContractSurrogate)
		{
			IList<Type> knownTypes = new List<Type>()
			                         	{
			                         		typeof(InputFile),
			                         		typeof(ColumnInfo),
			                         		typeof(DataPacket),
			                         		typeof(DataBlockBuilder.DataBlock),
			                         		typeof(CodecStreamBuilder.CodecStream),
			                         		typeof(FragmentContainer),
			                         		typeof(DetectorDataContractSurrogate.DetectorSurrogate),
											typeof(CodecParametersBuilder.CodecParameters),
											typeof(ReferenceHeader),
											typeof(ReferenceHeaderFile),
											typeof(ReferenceHeaderDatabase),
			                         	};

			return new DataContractSerializer(type, knownTypes, int.MaxValue, false, true, dataContractSurrogate);
		}
	}
}
