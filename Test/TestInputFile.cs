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
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="InputFile"/> class.
	/// </summary>
	[TestFixture]
	public class TestInputFile
	{
		#region Inner classes
		[DataContract]
		private sealed class ProjectSurrogate
		{
		}

		private sealed class MockProjectDataContractSurrogate : IDataContractSurrogate
		{
			private readonly IProject _project;

			internal MockProjectDataContractSurrogate(IProject project)
			{
				_project = project;
			}

			#region IDataContractSurrogate members
			public Type GetDataContractType(Type type)
			{
				return typeof(IProject).IsAssignableFrom(type) ? typeof(ProjectSurrogate) : type;
			}

			public object GetObjectToSerialize(object obj, Type targetType)
			{
				return (obj == _project) ? new ProjectSurrogate() : obj;
			}

			public object GetDeserializedObject(object obj, Type targetType)
			{
				return (obj is ProjectSurrogate) ? _project : obj;
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
		}
		#endregion Inner classes

		#region Test data
		/// <summary>The <c>Name</c> of the first test file.</summary>
		private const string FileName1 = "TestInputFile__dummy.mpg";
		/// <summary>The <c>Length</c> of the first test file.</summary>
		private const long FileLength1 = 1024L;
		/// <summary>The <c>Name</c> of the second test file.</summary>
		private const string FileName2 = "TestInputFile__movie.avi";
		/// <summary>The <c>Length</c> of the second test file.</summary>
		private const long FileLength2 = 311L;
		/// <summary>The <c>Name</c> of the file for testing the <c>Length</c> property.</summary>
		private const string LengthCachedFileName = "TestInputFile__LengthCached.3gp";
		/// <summary>The <c>Length</c> of the file for testing the <c>Length</c> property.</summary>
		private const long LengthCachedFileLength = 255L;
		#endregion Test data

		#region Mocks and stubs
		private IProject _project1;
		private IProject _project2;
		private IDataPacket _dataPacket;
		private IDataReader _dataReader;
		private Creator<IDataPacket, IInputFile> _createDataPacket;
		private Creator<IDataReader, IDataPacket> _createDataReader;
		#endregion Mocks and stubs

		#region Objects under test
		private IInputFile _inputFile;
		private IInputFile _duplicateInputFile;
		private IInputFile _differentInputFile;
		private IInputFile _inputFileDifferentProject;
		private IInputFile _inputFileDifferentName;
		private ISerializationContext _serializationContext;
		#endregion Objects under test

		#region Test initialization and cleanup
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			File.WriteAllText(FileName1, new string('*', (int)FileLength1));
			File.WriteAllText(FileName2, new string('*', (int)FileLength2));
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			File.Delete(FileName1);
			File.Delete(FileName2);
		}

		[SetUp]
		public void SetUp()
		{
			_project1 = MockRepository.GenerateStub<IProject>();
			_project2 = MockRepository.GenerateStub<IProject>();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataReader = MockRepository.GenerateStub<IDataReader>();
			_createDataPacket = MockRepository.GenerateStub<Creator<IDataPacket, IInputFile>>();
			_createDataReader = MockRepository.GenerateStub<Creator<IDataReader, IDataPacket>>();

			_inputFile = new InputFile(_createDataPacket, _createDataReader, _project1, FileName1);
			_duplicateInputFile = new InputFile(_createDataPacket, _createDataReader, _project1, FileName1);
			_differentInputFile = new InputFile(_createDataPacket, _createDataReader, _project2, FileName2);
			_inputFileDifferentProject = new InputFile(_createDataPacket, _createDataReader, _project2, FileName1);
			_inputFileDifferentName = new InputFile(_createDataPacket, _createDataReader, _project1, FileName2);
			_serializationContext = new InputFile.SerializationContext(_createDataPacket, _createDataReader);

			_createDataPacket.Stub(x => x(_inputFile)).Return(_dataPacket);
			_createDataReader.Stub(x => x(_dataPacket)).Return(_dataReader);
		}

		[TearDown]
		public void TearDown()
		{
			_project1 = null;
			_project2 = null;
			_dataPacket = null;
			_dataReader = null;
			_createDataPacket = null;
			_createDataReader = null;
			_inputFile = null;
			_duplicateInputFile = null;
			_differentInputFile = null;
			_inputFileDifferentProject = null;
			_inputFileDifferentName = null;
			_serializationContext = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataPacketCreatorNull()
		{
			new InputFile(null, _createDataReader, _project1, FileName1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataReaderCreatorNull()
		{
			new InputFile(_createDataPacket, null, _project1, FileName1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorProjectNull()
		{
			new InputFile(_createDataPacket, _createDataReader, null, FileName1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorNameNull()
		{
			new InputFile(_createDataPacket, _createDataReader, _project1, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ConstructorNameEmpty()
		{
			new InputFile(_createDataPacket, _createDataReader, _project1, string.Empty);
		}
		#endregion Tests for constructor arguments

		#region Tests for (de)serialization
		[Test]
		public void SerializationContextType()
		{
			Assert.IsTrue(typeof(InputFile).IsAssignableFrom(_serializationContext.Type));
		}

		[Test]
		public void Serialization()
		{
			IDataContractSurrogate dataContractSurrogate = new MockProjectDataContractSurrogate(_project1);
			XmlObjectSerializer serializer = new DataContractSerializer(typeof(InputFile), new[] { typeof(ProjectSurrogate) }, int.MaxValue, true, true, dataContractSurrogate);

			InputFile deserializedInputFile;
			byte[] serializedData;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				serializer.WriteObject(memoryStream, _inputFile);
				memoryStream.Flush();
				serializedData = memoryStream.ToArray();
			}

			using (MemoryStream stream = new MemoryStream(serializedData))
			{
				deserializedInputFile = serializer.ReadObject(stream) as InputFile;
			}

			_serializationContext.CompleteDeserialization(deserializedInputFile);

			Assert.AreEqual(_inputFile, deserializedInputFile, "File");
			Assert.AreEqual(FileLength1, _inputFile.Length, "Length");
			Assert.AreSame(_dataPacket, deserializedInputFile.CreateDataPacket());
		}
		#endregion Tests for (de)serialization

		#region Tests for properties
		[Test]
		public void Project()
		{
			Assert.AreSame(_project1, _inputFile.Project);
		}

		[Test]
		public void Name()
		{
			Assert.AreEqual(FileName1, _inputFile.Name);
		}

		[Test]
		public void Length()
		{
			Assert.AreEqual(FileLength1, _inputFile.Length);
		}

		[Test]
		public void LengthIsCached()
		{
			try
			{
				File.WriteAllText(LengthCachedFileName, new string('*', (int)LengthCachedFileLength));
				IInputFile inputFile = new InputFile(_createDataPacket, _createDataReader, _project1, LengthCachedFileName);
				Assert.AreEqual(LengthCachedFileLength, inputFile.Length);
				File.Delete(LengthCachedFileName);
				Assert.AreEqual(LengthCachedFileLength, inputFile.Length);
			}
			finally
			{
				File.Delete(LengthCachedFileName);
			}
		}
		#endregion Tests for properties

		#region Tests for CreateDataReader() method
		[Test]
		public void CreateDataReader()
		{
			Assert.AreSame(_dataReader, _inputFile.CreateDataReader());
		}
		#endregion Tests for CreateDataReader() method

		#region Tests for ToString() method
		[Test]
		public void ToStringEqualsName()
		{
			Assert.AreEqual(_inputFile.Name, _inputFile.ToString());
		}
		#endregion Tests for ToString() method

		#region Tests for IEquatable.Equals() method
		[Test]
		public void IEquatableEqualsDuplicateInputFile()
		{
			Assert.IsTrue(((InputFile)_inputFile).Equals((InputFile)_duplicateInputFile));
		}

		[Test]
		public void IEquatableEqualsDifferentInputFile()
		{
			Assert.IsFalse(((InputFile)_inputFile).Equals((InputFile)_differentInputFile));
		}

		[Test]
		public void IEquatableEqualsDifferentProject()
		{
			Assert.IsFalse(((InputFile)_inputFile).Equals((InputFile)_inputFileDifferentProject));
		}

		[Test]
		public void IEquatableEqualsDifferentName()
		{
			Assert.IsFalse(((InputFile)_inputFile).Equals((InputFile)_inputFileDifferentName));
		}

		[Test]
		public void IEquatableEqualsReflexive()
		{
			Assert.IsTrue(((InputFile)_inputFile).Equals((InputFile)_inputFile));
		}

		[Test]
		public void IEquatableEqualsSymmetric()
		{
			Assert.IsTrue(((InputFile)_duplicateInputFile).Equals((InputFile)_inputFile), "Equal input files");
			Assert.IsTrue(((InputFile)_inputFile).Equals((InputFile)_duplicateInputFile), "Equal input files");
			Assert.IsFalse(((InputFile)_inputFile).Equals((InputFile)_differentInputFile), "Different input files");
			Assert.IsFalse(((InputFile)_differentInputFile).Equals((InputFile)_inputFile), "Different input files");
		}

		[Test]
		public void IEquatableEqualsNull()
		{
			Assert.IsFalse(((InputFile)_inputFile).Equals(null));
		}
		#endregion Tests for IEquatable.Equals() method

		#region Tests for Equals() method
		[Test]
		public void EqualsDuplicateInputFile()
		{
			Assert.IsTrue(_inputFile.Equals(_duplicateInputFile));
		}

		[Test]
		public void EqualsDifferentInputFile()
		{
			Assert.IsFalse(_inputFile.Equals(_differentInputFile));
		}

		[Test]
		public void EqualsDifferentProject()
		{
			Assert.IsFalse(_inputFile.Equals(_inputFileDifferentProject));
		}

		[Test]
		public void EqualsDifferentName()
		{
			Assert.IsFalse(_inputFile.Equals(_inputFileDifferentName));
		}

		[Test]
		public void EqualsReflexive()
		{
			Assert.IsTrue(_inputFile.Equals(_inputFile));
		}

		[Test]
		public void EqualsSymmetric()
		{
			Assert.IsTrue(_duplicateInputFile.Equals(_inputFile), "Equal input files");
			Assert.IsTrue(_inputFile.Equals(_duplicateInputFile), "Equal input files");
			Assert.IsFalse(_inputFile.Equals(_differentInputFile), "Different input files");
			Assert.IsFalse(_differentInputFile.Equals(_inputFile), "Different input files");
		}

		[Test]
		public void EqualsDifferentClass()
		{
			Assert.IsFalse(_inputFile.Equals(MockRepository.GenerateStub<IInputFile>()));
		}

		[Test]
		public void EqualsNull()
		{
			Assert.IsFalse(_inputFile.Equals(null));
		}
		#endregion Tests for Equals() method

		#region Tests for GetHashCode() method
		[Test]
		public void GetHashCodeConsistentWithEquals()
		{
			Assert.AreEqual(_inputFile.GetHashCode(), _duplicateInputFile.GetHashCode());
		}

		[Test]
		public void GetHashCodeDependsOnProject()
		{
			Assert.AreNotEqual(_inputFile.GetHashCode(), _inputFileDifferentProject.GetHashCode());
		}

		[Test]
		public void GetHashCodeDependsOnName()
		{
			Assert.AreNotEqual(_inputFile.GetHashCode(), _inputFileDifferentName.GetHashCode());
		}
		#endregion Tests for GetHashCode() method
	}
}
