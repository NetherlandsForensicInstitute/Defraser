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
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="SerializationContextDataContractSurrogate"/> is used to
	/// specify the neccessary steps to complete serialization of objects of a
	/// given <see cref="Type"/>.
	/// Although it is similar to <see cref="IDeserializationCallback"/> or using
	/// the <see cref="OnDeserializedAttribute"/>, the <see cref="ISerializationContext"/>
	/// can supply additional state to the objects being deserialized. This allows
	/// for dependencies to be resolved without the need for a service locator or
	/// singleton.
	/// </summary>
	public sealed class SerializationContextDataContractSurrogate : IDataContractSurrogate
	{
		private readonly IDictionary<Type, ISerializationContext> _serializationContexts;

		/// <summary>
		/// Creates a new <see cref="SerializationContextDataContractSurrogate"/>.
		/// </summary>
		/// <param name="serializationContexts">
		/// The serialization contexts. Invoked when an object of the corresponding
		/// type is deserialized, where the deserialized object is supplied as argument
		/// to <see cref="ISerializationContext.CompleteDeserialization"/>.
		/// </param>
		public SerializationContextDataContractSurrogate(IEnumerable<ISerializationContext> serializationContexts)
		{
			_serializationContexts = serializationContexts.ToDictionary(c => c.Type);
		}

		#region IDataContractSurrogate members
		public Type GetDataContractType(Type type)
		{
			return type;
		}

		public object GetObjectToSerialize(object obj, Type targetType)
		{
			//if (obj is IDetector)
			//{
				// TODO: use surrogate

				// -SERIALIZE-
				//    IConfigurable configurableDetector = detector as IConfigurable;
				//    if (configurableDetector == null) continue;
				//
				//    List<Pair<string, string>> configuration = new List<Pair<string,string>>();
				//    foreach (IConfigurationItem configurationItem in configurableDetector.Configuration)
				//    {
				//        configuration.Add(new Pair<string, string>(configurationItem.Description, configurationItem.Value.ToString()));
				//    }
				//    _detectorConfiguration.Add(detector.Name, configuration);

				// -DESERIALIZE-
				//    ICollection<Pair<string,string>> configuration;
				//    _detectorConfiguration.TryGetValue(detector.Name, out configuration);
				//    if(configuration==null) continue;
				//
				//    foreach (Pair<string, string> configurationItem in configuration)
				//    {
				//        detector.SetConfigurationItem(configurationItem.First, configurationItem.Second);
				//    }
			//}
			return obj;
		}

		public object GetDeserializedObject(object obj, Type targetType)
		{
			CompleteDeserialization(obj);
			return obj;
		}

		public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
		{
			return null;
		}

		public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
		{
			return typeDeclaration;
		}

		public object GetCustomDataToExport(Type clrType, Type dataContractType)
		{
			return null;
		}

		public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
		{
			return null;
		}

		public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
		{
		}
		#endregion IDataContractSurrogate members

		private void CompleteDeserialization(object obj)
		{
			ISerializationContext serializationContext;
			if (_serializationContexts.TryGetValue(obj.GetType(), out serializationContext))
			{
				serializationContext.CompleteDeserialization(obj);
			}
		}
	}
}
