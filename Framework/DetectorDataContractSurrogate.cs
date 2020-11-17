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
using Defraser.Detector.UnknownFormat;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	public sealed class DetectorDataContractSurrogate : IDataContractSurrogate
	{
		#region Inner classes
		[DataContract]
		public sealed class DetectorSurrogate
		{
			#region Properties
			[DataMember]
			public string DetectorType { get; private set; }
			[DataMember]
			public List<Pair<string, string>> Configuration { get; private set; }
			#endregion Properties

			public DetectorSurrogate(IDetector detector)
			{
				DetectorType = detector.DetectorType.FullName;

				// FIXME: detectors can be wrapped, where DetectorWrapper is IConfigurable, but the wrapped detector is not!!
				if (detector is IConfigurable)
				{
					IConfigurable configurableDetector = detector as IConfigurable;

					Configuration = new List<Pair<string, string>>();
					foreach (IConfigurationItem configurationItem in configurableDetector.Configuration)
					{
						Configuration.Add(new Pair<string, string>(configurationItem.Description, configurationItem.Value.ToString()));
					}
				}
			}

			public IDetector GetDetector(IDetectorFactory detectorFactory)
			{
				if (DetectorType == typeof(UnknownFormatDetector).FullName)
				{
					return new UnknownFormatDetector();
				}
				IDetector detector = detectorFactory.Detectors.Single(x => x.DetectorType.FullName == DetectorType);
				foreach (Pair<string, string> configurationItem in Configuration)
				{
					detector.SetConfigurationItem(configurationItem.First, configurationItem.Second);
				}
				return detector;
			}
		}
		#endregion Inner classes

		private readonly IDetectorFactory _detectorFactory;
		private readonly IDataContractSurrogate _dataContractSurrogate;

		public DetectorDataContractSurrogate(IDetectorFactory detectorFactory, IDataContractSurrogate dataContractSurrogate)
		{
			_detectorFactory = detectorFactory;
			_dataContractSurrogate = dataContractSurrogate;
		}

		#region IDataContractSurrogate members
		public Type GetDataContractType(Type type)
		{
			if (typeof(IDetector).IsAssignableFrom(type))
			{
				return typeof(DetectorSurrogate);
			}
			return _dataContractSurrogate.GetDataContractType(type);
		}

		public object GetObjectToSerialize(object obj, Type targetType)
		{
			if (obj is IDetector)
			{
				IDetector detector = obj as IDetector;
				return new DetectorSurrogate(detector);
			}
			return _dataContractSurrogate.GetObjectToSerialize(obj, targetType);
		}

		public object GetDeserializedObject(object obj, Type targetType)
		{
			if (obj is DetectorSurrogate)
			{
				DetectorSurrogate detectorSurrogate = obj as DetectorSurrogate;
				return detectorSurrogate.GetDetector(_detectorFactory);
			}
			return _dataContractSurrogate.GetDeserializedObject(obj, targetType);
		}

		public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
		{
			return _dataContractSurrogate.GetReferencedTypeOnImport(typeName, typeNamespace, customData);
		}

		public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
		{
			return _dataContractSurrogate.ProcessImportedType(typeDeclaration, compileUnit);
		}

		public object GetCustomDataToExport(Type clrType, Type dataContractType)
		{
			return _dataContractSurrogate.GetCustomDataToExport(clrType, dataContractType);
		}

		public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
		{
			return _dataContractSurrogate.GetCustomDataToExport(memberInfo, dataContractType);
		}

		public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
		{
			customDataTypes.Add(typeof(DetectorSurrogate));
			_dataContractSurrogate.GetKnownCustomDataTypes(customDataTypes);
		}
		#endregion IDataContractSurrogate members
	}
}
