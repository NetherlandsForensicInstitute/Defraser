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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	[DataContract]
	public sealed class ReferenceHeaderDatabase : IReferenceHeaderDatabase
	{
		private const uint DatabaseVersion = 140;

		#region Inner classes
		public sealed class SerializationContext : ISerializationContext
		{
			private readonly Creator<IInputFile, IProject, string> _createInputFile;
			private readonly Creator<XmlObjectSerializer, Type> _createXmlObjectSerializer;

			#region Properties
			public Type Type { get { return typeof(ReferenceHeaderDatabase); } }
			#endregion Properties

			public SerializationContext(Creator<IInputFile, IProject, string> createInputFile, Creator<XmlObjectSerializer, Type> createXmlObjectSerializer)
			{
				_createInputFile = createInputFile;
				_createXmlObjectSerializer = createXmlObjectSerializer;
			}

			public void CompleteDeserialization(Object obj)
			{
				((ReferenceHeaderDatabase)obj).CompleteDeserialization(_createInputFile, _createXmlObjectSerializer);
			}
		}
		#endregion Inner classes

		private Creator<IInputFile, IProject, string> _createInputFile;
		private Creator<XmlObjectSerializer, Type> _createXmlObjectSerializer;

		[DataMember]
		private readonly IList<IReferenceHeader> _referenceHeaders;
		[DataMember]
		private uint _databaseVersion;

		public ReferenceHeaderDatabase(Creator<IInputFile, IProject, string> createInputFile, Creator<XmlObjectSerializer, Type> createXmlObjectSerializer)
		{
			_createInputFile = createInputFile;
			_createXmlObjectSerializer = createXmlObjectSerializer;
			_databaseVersion = DatabaseVersion;
			_referenceHeaders = new List<IReferenceHeader>();
		}

		private void CompleteDeserialization(Creator<IInputFile, IProject, string> createInputFile, Creator<XmlObjectSerializer, Type> createXmlObjectSerializer)
		{
			_createInputFile = createInputFile;
			_createXmlObjectSerializer = createXmlObjectSerializer;

			if (_databaseVersion != DatabaseVersion)
			{
				throw new SerializationException("Incorrect database file version: " + _databaseVersion);
			}
		}

		public IEnumerable<IReferenceHeader> ListHeaders(Func<IReferenceHeader, bool> predicate)
		{
			return _referenceHeaders.Where(predicate);
		}

		public IReferenceHeader AddHeader(ICodecDetector detector, string filename)
		{
			IInputFile inputFile = _createInputFile(new Project(_createInputFile, "dummy.xml"), filename);
			using (var dataReader = inputFile.CreateDataReader())
			{
				var codecParametersBuilder = new CodecParametersBuilder();
				IDataPacket headerData = detector.FindReferenceHeader(dataReader, codecParametersBuilder);
				if (headerData == null)
				{
					return null; // No header detected
				}

				byte[] b = ReadDataPacketToByteArray(headerData, dataReader);
				var header = new ReferenceHeader(b, codecParametersBuilder.Build());
				// Use the filename as 'Setting' and leave 'Brand' and 'Model' as undefined.
				header.Setting = Path.GetFileNameWithoutExtension(filename);

				_referenceHeaders.Add(header);
				return header;
			}
		}

		private static byte[] ReadDataPacketToByteArray(IDataPacket dataPacket, IDataReader dataReader)
		{
			var b = new byte[dataPacket.Length];
			for (int offset = 0; offset < dataPacket.Length; )
			{
				IDataPacket fragment = dataPacket.GetFragment(offset);
				dataReader.Position = fragment.StartOffset;
				dataReader.Read(b, offset, (int)fragment.Length);
				offset += (int)fragment.Length;
			}
			return b;
		}

		public void RemoveHeader(IReferenceHeader header)
		{
			_referenceHeaders.Remove(header);
		}

		public void Import(string filename)
		{
			// Deserialize the database
			XmlObjectSerializer dataContractSerializer = _createXmlObjectSerializer(typeof(ReferenceHeaderDatabase));
			using (XmlReader reader = XmlReader.Create(filename))
			{
				var database = dataContractSerializer.ReadObject(reader) as ReferenceHeaderDatabase;
				foreach (IReferenceHeader header in database._referenceHeaders)
				{
					// Avoid duplicates between current and imported database
					if (!_referenceHeaders.Contains(header))
					{
						_referenceHeaders.Add(header);
					}
				}
			}
		}

		public void Export(string filename)
		{
			var xmlObjectSerializer = _createXmlObjectSerializer(typeof(ReferenceHeaderDatabase));
			var writerSettings = new XmlWriterSettings { Indent = true };
			using (XmlWriter writer = XmlWriter.Create(filename, writerSettings))
			{
				xmlObjectSerializer.WriteObject(writer, this);
			}
		}
	}
}
