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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Defraser.DataStructures;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	[DataContract]
	public sealed class ReferenceHeaderFile : IReferenceHeaderFile, IEquatable<ReferenceHeaderFile>
	{
		#region Inner classes
		public sealed class SerializationContext : ISerializationContext
		{
			private readonly Creator<IDataPacket, IInputFile> _createDataPacket;
			private readonly Creator<ByteArrayDataReader, IDataPacket, byte[]> _createDataReader;

			#region Properties
			public Type Type { get { return typeof(ReferenceHeaderFile); } }
			#endregion Properties

			public SerializationContext(Creator<IDataPacket, IInputFile> createDataPacket, Creator<ByteArrayDataReader, IDataPacket, byte[]> createDataReader)
			{
				PreConditions.Argument("createDataPacket").Value(createDataPacket).IsNotNull();
				PreConditions.Argument("createDataReader").Value(createDataReader).IsNotNull();

				_createDataPacket = createDataPacket;
				_createDataReader = createDataReader;
			}

			public void CompleteDeserialization(Object obj)
			{
				((ReferenceHeaderFile)obj).CompleteDeserialization(_createDataPacket, _createDataReader);
			}
		}
		#endregion Inner classes

		private Creator<IDataPacket, IInputFile> _createDataPacket;
		private Creator<ByteArrayDataReader, IDataPacket, byte[]> _createDataReader;

		[DataMember]
		private readonly IProject _project;
		[DataMember]
		private readonly IReferenceHeader _referenceHeader;
		// Descriptive name that includes a hexadecimal representation of 'data'. Used for forensic logging.
		private string _descriptiveName;

		#region Properties
		public IProject Project { get { return _project; } }
		public string Name { get { return _descriptiveName; } }
		public long Length { get { return _referenceHeader.Data.Length; } }
		public string Checksum
		{
			get
			{
				byte[] hash = new MD5CryptoServiceProvider().ComputeHash(_referenceHeader.Data);
				return BitConverter.ToString(hash).Replace("-", string.Empty);
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="ReferenceHeaderFile"/>.
		/// </summary>
		/// <param name="createDataPacket">The factory method for creating a data packet</param>
		/// <param name="createDataReader">The factory method for creating a data reader</param>
		/// <param name="project">The project the file belongs to</param>
		/// <param name="referenceHeader">The reference header</param>
		public ReferenceHeaderFile(Creator<IDataPacket, IInputFile> createDataPacket, Creator<ByteArrayDataReader, IDataPacket, byte[]> createDataReader, IProject project, IReferenceHeader referenceHeader)
		{
			PreConditions.Argument("createDataPacket").Value(createDataPacket).IsNotNull();
			PreConditions.Argument("createDataReader").Value(createDataReader).IsNotNull();
			PreConditions.Argument("project").Value(project).IsNotNull();
			PreConditions.Argument("referenceHeader").Value(referenceHeader).IsNotNull();

			_createDataPacket = createDataPacket;
			_createDataReader = createDataReader;
			_project = project;
			_referenceHeader = referenceHeader;
			_descriptiveName = GetDescriptiveName();
		}

		private string GetDescriptiveName()
		{
			var sb = new StringBuilder();
			sb.Append("Reference header '");
			sb.Append(_referenceHeader.Brand);
			sb.Append(' ');
			sb.Append(_referenceHeader.Model);
			sb.Append(" (");
			sb.Append(_referenceHeader.Setting);
			sb.Append(")' (= 0x");
			sb.Append(BitConverter.ToString(_referenceHeader.Data).Replace("-", string.Empty));
			sb.Append(')');
			return sb.ToString();
		}

		private void CompleteDeserialization(Creator<IDataPacket, IInputFile> createDataPacket, Creator<ByteArrayDataReader, IDataPacket, byte[]> createDataReader)
		{
			_createDataPacket = createDataPacket;
			_createDataReader = createDataReader;
			_descriptiveName = GetDescriptiveName();
		}

		public IDataPacket CreateDataPacket()
		{
			return _createDataPacket(this);
		}

		public IDataReader CreateDataReader()
		{
			return _createDataReader(CreateDataPacket(), _referenceHeader.Data);
		}

		public override string ToString()
		{
			return Name;
		}

		#region Equals() and GetHashCode()
		public override bool Equals(object obj)
		{
			return Equals(obj as ReferenceHeaderFile);
		}

		public bool Equals(ReferenceHeaderFile inputFile)
		{
			if (inputFile == null) return false;
			if (inputFile == this) return true;

			return (Project == inputFile.Project) && _referenceHeader.Equals(inputFile._referenceHeader);
		}

		public override int GetHashCode()
		{
			return Project.GetHashCode().CombineHashCode(_referenceHeader);
		}
		#endregion Equals() and GetHashCode()
	}
}
