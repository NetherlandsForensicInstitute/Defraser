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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Serializable implementation of <see cref="IInputFile"/>.
	/// </summary>
	/// <remarks>
	/// The <see cref="Length"/> is the length of the file on disk.
	/// </remarks>
	[DataContract]
	public class InputFile : IInputFile, IEquatable<InputFile>
	{
		#region Inner classes
		public sealed class SerializationContext : ISerializationContext
		{
			private readonly Creator<IDataPacket, IInputFile> _createDataPacket;
			private readonly Creator<IDataReader, IDataPacket> _createDataReader;

			#region Properties
			public Type Type { get { return typeof(InputFile); } }
			#endregion Properties

			public SerializationContext(Creator<IDataPacket, IInputFile> createDataPacket, Creator<IDataReader, IDataPacket> createDataReader)
			{
				PreConditions.Argument("createDataPacket").Value(createDataPacket).IsNotNull();
				PreConditions.Argument("createDataReader").Value(createDataReader).IsNotNull();

				_createDataPacket = createDataPacket;
				_createDataReader = createDataReader;
			}

			public void CompleteDeserialization(Object obj)
			{
				((InputFile)obj).CompleteDeserialization(_createDataPacket, _createDataReader);
			}
		}
		#endregion Inner classes

		private Creator<IDataPacket, IInputFile> _createDataPacket;
		private Creator<IDataReader, IDataPacket> _createDataReader;

		[DataMember]
		private readonly IProject _project;
		[DataMember]
		private readonly string _name;
		private string _md5Hash;
		[DataMember]
		private long _length;

		#region Properties
		public IProject Project { get { return _project; } }
		public string Name { get { return _name; } }
		public long Length
		{
			get
			{
				if(_length == 0)
				{
					if(!File.Exists(Name)) throw new FileNotFoundException("Can not determine the length of a file that does not exist", "Name");
					_length = new FileInfo(Name).Length;
				}
				return _length;
			}
		}
		public string Checksum
		{
			get
			{
				if (string.IsNullOrEmpty(_md5Hash))
				{
					using (FileStream fileStream = new FileStream(Name, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						byte[] hash = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(fileStream);

						StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
						foreach (byte value in hash)
						{
							stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0:X02}", value));
						}

						_md5Hash = stringBuilder.ToString();
					}
				}
				return _md5Hash;
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="InputFile"/> with the given path <paramref name="name"/>.
		/// </summary>
		/// <param name="createDataPacket">The factory method for creating a data packet</param>
		/// <param name="createDataReader">The factory method for creating a data reader</param>
		/// <param name="project">The project the file belongs to</param>
		/// <param name="name">The path name of the file</param>
		public InputFile(Creator<IDataPacket, IInputFile> createDataPacket, Creator<IDataReader, IDataPacket> createDataReader, IProject project, string name)
		{
			PreConditions.Argument("createDataPacket").Value(createDataPacket).IsNotNull();
			PreConditions.Argument("createDataReader").Value(createDataReader).IsNotNull();
			PreConditions.Argument("project").Value(project).IsNotNull();
			PreConditions.Argument("name").Value(name).IsNotNull().And.IsNotEmpty();

			_createDataPacket = createDataPacket;
			_createDataReader = createDataReader;
			_project = project;
			_name = name;
		}

		private void CompleteDeserialization(Creator<IDataPacket, IInputFile> createDataPacket, Creator<IDataReader, IDataPacket> createDataReader)
		{
			_createDataPacket = createDataPacket;
			_createDataReader = createDataReader;
		}

		public IDataPacket CreateDataPacket()
		{
			return _createDataPacket(this);
		}

		public IDataReader CreateDataReader()
		{
			return _createDataReader(CreateDataPacket());
		}

		public override string ToString()
		{
			return Name;
		}

		#region Equals() and GetHashCode()
		public override bool Equals(object obj)
		{
			return Equals(obj as InputFile);
		}

		public bool Equals(InputFile inputFile)
		{
			if (inputFile == null) return false;
			if (inputFile == this) return true;

			return (Project == inputFile.Project) && (Name == inputFile.Name);
		}

		public override int GetHashCode()
		{
			return Project.GetHashCode()
				.CombineHashCode(Name);
		}
		#endregion Equals() and GetHashCode()
	}
}
