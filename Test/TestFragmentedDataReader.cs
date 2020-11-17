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
using System.IO;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestFragmentedDataReader : DataReaderTest<FragmentedDataReader>
	{
		public const string DataFileNameCopy = DataFileName + ".copy";

		private IDataPacket _dataPacket;
		private byte[] _fragmentedData;
		private MockDataReader _dataReaderMock;
		private MockDataReaderPool _dataReaderPoolMock;

		#region Properties
		public override byte[] DataReaderData { get { return _fragmentedData; } }
		#endregion Properties


		public override void TestFixtureSetup()
		{
			base.TestFixtureSetup();

			File.Delete(DataFileNameCopy);

			// Make a copy
			FileInfo fileInfo = new FileInfo(DataFileName);
			fileInfo.CopyTo(DataFileNameCopy);

			// Create fragmented data packet and bytes
			_dataPacket = TestFramework.CreateDataPacket(InputFile, 10, 113);			// < 123
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 151, 89));	// < 240
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 255, 17));	// < 272
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 289, 1));		// < 290
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 393, 44));	// < 437
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 0, 5));		// < 5
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 511, 22));	// < 533
			_dataPacket = _dataPacket.Append(TestFramework.CreateDataPacket(InputFile, 140, 40));	// overlap!
			_fragmentedData = new byte[113 + 89 + 17 + 1 + 44 + 5 + 22 + 40];
			Array.Copy(base.DataReaderData, 10, _fragmentedData, 0, 113);
			Array.Copy(base.DataReaderData, 151, _fragmentedData, 113, 89);
			Array.Copy(base.DataReaderData, 255, _fragmentedData, 113 + 89, 17);
			Array.Copy(base.DataReaderData, 289, _fragmentedData, 113 + 89 + 17, 1);
			Array.Copy(base.DataReaderData, 393, _fragmentedData, 113 + 89 + 17 + 1, 44);
			Array.Copy(base.DataReaderData, 0, _fragmentedData, 113 + 89 + 17 + 1 + 44, 5);
			Array.Copy(base.DataReaderData, 511, _fragmentedData, 113 + 89 + 17 + 1 + 44 + 5, 22);
			Array.Copy(base.DataReaderData, 140, _fragmentedData, 113 + 89 + 17 + 1 + 44 + 5 + 22, 40);
		}

		public override void TestFixtureTeardown()
		{
			base.TestFixtureTeardown();

			File.Delete(DataFileNameCopy);

			_dataPacket = null;
			_fragmentedData = null;
		}

		public override void SetUp()
		{
			base.SetUp();

			_dataReaderMock = new MockDataReader(base.DataReaderData, InputFile);
			_dataReaderPoolMock = new MockDataReaderPool(_dataReaderMock);
			_dataReader = new FragmentedDataReader(_dataPacket, _dataReaderPoolMock);
		}

		#region Common data reader tests
		
		// Data is fragmented, so default test is not applicable
		[Test]
		public override void GetDataPacketEntireFile()
		{
		}
		#endregion Common data reader tests

		#region FragmentedDataReader specific tests
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataPacketNull()
		{
			using (new FragmentedDataReader(null, _dataReaderPoolMock))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderPoolNull()
		{
			using (new FragmentedDataReader(_dataPacket, null))
			{
			}
		}

		[Ignore("Implementation has changed significantly")]
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstructorInputFileMismatch()
		{
			using (new FragmentedDataReader(TestFramework.CreateDataPacket(TestFramework.CreateInputFile(DataFileNameCopy), 0, 100), _dataReaderPoolMock))
			{
			}
		}

		[Ignore("Implementation has changed significantly")]
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstructorInvalidDataReader()
		{
			using (new FragmentedDataReader(TestFramework.CreateDataPacket(InputFile, 0, 100), _dataReaderPoolMock))
			{
			}
		}

		[Test]
		public void TestConstructorDataPacket()
		{
			using (FragmentedDataReader fragmentedDataReader = new FragmentedDataReader(_dataPacket, _dataReaderPoolMock))
			{
				Read(6, 17);
				Read(63, 120);
				Read(17, 177);
				Read(234, 90);
			}
		}

		[Test]
		public void TestGetDataPacketRandom()
		{
			IDataPacket dataPacket = _dataReader.GetDataPacket(30, 20);
			Assert.IsTrue(dataPacket.Equals(TestFramework.CreateDataPacket(InputFile, 40, 20)), "GetDataPacket() for random, unfragmented packet");
		}

		[Test]
		public void TestGetDataPacketFragmented()
		{
			IDataPacket dataPacket = _dataReader.GetDataPacket(70, 70);
			Assert.IsTrue(dataPacket.GetFragment(0).Equals(TestFramework.CreateDataPacket(InputFile, 80, 43)), "GetDataPacket() for fragmented packet (a)");
			Assert.IsTrue(dataPacket.GetFragment(43).Equals(TestFramework.CreateDataPacket(InputFile, 151, 27)), "GetDataPacket() for fragmented packet (b)");
		}

		[Test]
		[Ignore("This is not the intended behavior!")]
		public void TestReadMockDisposed()
		{
			_dataReaderMock.Dispose();
			Assert.AreEqual(0, _dataReader.Read(Buffer, 0, Buffer.Length), "FragmentedDataReader.Read() after underlying reader has been disposed");
		}

		[Test]
		public void TestReadUnfragmented()
		{
			Read(6, 17);
		}

		[Test]
		public void TestReadFragmented()
		{
			Read(63, 120);
			Read(17, 177);
			Read(234, 90);
		}
		#endregion FragmentedDataReader specific tests
	}
}
