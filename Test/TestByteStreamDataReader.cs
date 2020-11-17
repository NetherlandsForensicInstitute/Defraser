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
using Defraser.Detector.Common;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestByteStreamDataReader : DataReaderTest<ByteStreamDataReader>
	{
		public const int BlockSize = 32;

		// Sample data from 'skating-dog.3gp'
		private static readonly byte[] _qtData =
		{
			0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70,
			0x33, 0x67, 0x70, 0x34, 0x00, 0x00, 0x02, 0x00,
			0x33, 0x67, 0x70, 0x34, 0x00, 0x06, 0x2D, 0x40,
			0x6D, 0x64, 0x61, 0x74, 0x00, 0x00, 0x80, 0x02,
			0x08, 0x03, 0x16, 0xDB, 0x02, 0x14, 0x57, 0x44,
			0xA0, 0x0F, 0xB3, 0xDE, 0x08, 0x22, 0x55, 0x98,
			0xA1, 0x52, 0xB5, 0x22, 0x5F, 0x28, 0x30, 0x40,
			0xB0, 0x47, 0x11, 0xEE, 0x7F, 0x8D, 0x9E, 0x45,
			0x06, 0x00, 0x4C, 0xBA, 0x09, 0x41, 0x03, 0xC5,
			0xD2, 0x5F, 0x4E, 0xFD, 0x40, 0x18, 0xAA, 0xEA,
			0x9B, 0xE6, 0x1A, 0x7B, 0x88, 0x20, 0x80, 0x62,
			0xA0, 0x60, 0x7C, 0xC2, 0x00, 0x92, 0x10, 0xC1,
			0x04, 0x49, 0x2F, 0x12, 0xC4, 0x80, 0x0F, 0x12,
			0x07, 0x8A, 0x8B
		};

		private MockDataReader _dataReaderMock;

		#region Properties
		public override byte[] DataReaderData { get { return _qtData; } }
		#endregion Properties


		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			_dataReaderMock = new MockDataReader(_qtData, InputFile);
			_dataReader = new ByteStreamDataReader(_dataReaderMock, BlockSize);
		}

		#region Common data reader tests
		[Test]
		public void TestPosition()
		{
			base.GetPositionStartMiddleEnd();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNegativePosition()
		{
			base.SetNegativePosition();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestInvalidPosition()
		{
			base.SetInvalidPosition();
		}

		[Test]
		public void TestLength()
		{
			base.GetLength();
		}

		[Test]
		public void TestState()
		{
			base.GetStateAll();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestGetDataPacketNegativeOffset()
		{
			base.GetDataPacketNegativeOffset();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestGetDataPacketNegativeLength()
		{
			base.GetDataPacketNegativeLength();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestGetDataPacketLengthOverflow()
		{
			base.GetDataPacketLengthOverflow();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestGetDataPacketOffsetPlusLengthOverflow()
		{
			base.GetDataPacketOffsetPlusLengthOverflow();
		}

		[Test]
		public void TestGetDataPacket()
		{
			base.GetDataPacketEntireFile();
		}

		[Test]
		public void TestGetDataPacketInputFile()
		{
			base.GetDataPacketInputFile();
		}

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void TestReadDisposed()
		{
			base.ReadDisposed();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestReadArrayNull()
		{
			base.ReadArrayNull();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestReadNegativeArrayOffset()
		{
			base.ReadNegativeArrayOffset();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestReadNegativeCount()
		{
			base.ReadNegativeCount();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestReadCountOverflow()
		{
			base.ReadCountOverflow();
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestReadArrayOffsetPlusCountOverflow()
		{
			base.ReadArrayOffsetPlusCountOverflow();
		}
		#endregion Common data reader tests

		#region ByteStreamDataReader specific tests
		[Test]
		public void TestConstructor()
		{
			using (new ByteStreamDataReader(_dataReaderMock))
			{
				Assert.IsTrue(true, "1 argument constructor, default block size");
			}
		}

		[Test]
		public void TestLittleEndiannessConstructorWithTwoArguments()
		{
			using (new ByteStreamDataReader(_dataReaderMock, Endianness.Little))
			{
				Assert.IsTrue(true, "2 argument constructor, default block size, litte endian");
			}
		}

		[Test]
		public void TestLittleEndiannessConstructorWithThreeArguments()
		{
			using (new ByteStreamDataReader(_dataReaderMock, BlockSize, Endianness.Little))
			{
				Assert.IsTrue(true, "3 argument constructor, litte endian");
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderNull()
		{
			using (new ByteStreamDataReader(null))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorBlockSizeInvalid()
		{
			using (new ByteStreamDataReader(_dataReaderMock, 7))
			{
			}
		}

		[Test]
		public void TestGetByte()
		{
			Assert.AreEqual(0x00, _dataReader.GetByte(), "GetByte()");
			_dataReader.Position = 8;
			Assert.AreEqual(0x33, _dataReader.GetByte(), "GetByte()");
			_dataReader.Position = 31;
			_dataReader.GetByte();
			Assert.AreEqual(0x08, _dataReader.GetByte(), "GetByte(), crossing buffer boundary");
			_dataReader.Position = DataReaderLength - 1;
			Assert.AreEqual(0x8B, _dataReader.GetByte(), "GetByte(), last byte");
			Assert.AreEqual(0x00, _dataReader.GetByte(), "GetByte(), zero padding 1");
			Assert.AreEqual(0x00, _dataReader.GetByte(), "GetByte(), zero padding 2");
		}

		[Test]
		public void TestGetShort()
		{
			_dataReader.Position = 8;
			Assert.AreEqual(0x3367, _dataReader.GetShort(), "GetShort()");
			Assert.AreEqual(0x7034, _dataReader.GetShort(), "GetShort(), buffered");
			_dataReader.Position = 39;
			Assert.AreEqual(0x44A0, _dataReader.GetShort(), "GetShort(), crossing buffer boundary");
		}

		[Test]
		public void TestGetThreeBytes()
		{
			_dataReader.Position = 3;
			Assert.AreEqual(0x146674, _dataReader.GetThreeBytes(), "GetThreeBytes()");
			Assert.AreEqual(0x797033, _dataReader.GetThreeBytes(), "GetThreeBytes(), buffered");
			_dataReader.Position = 33;
			Assert.AreEqual(0x0316DB, _dataReader.GetThreeBytes(), "GetThreeBytes(), crossing buffer boundary");
		}

		[Test]
		public void TestGetInt()
		{
			_dataReader.Position = 51;
			Assert.AreEqual(0x225F2830, _dataReader.GetInt(), "GetInt()");
			Assert.AreEqual(0x40B04711, _dataReader.GetInt(), "GetInt(), buffered");
			_dataReader.Position = 80;
			Assert.AreEqual(0x9BE61A7B, (uint)_dataReader.GetInt(), "GetInt(), crossing buffer boundary");
		}

		[Test]
		public void TestGetLong()
		{
			Assert.AreEqual(0x0000001466747970L, _dataReader.GetLong(), "GetLong()");
			Assert.AreEqual(0x3367703400000200L, _dataReader.GetLong(), "GetLong(), buffered");
			_dataReader.Position = 30;
			Assert.AreEqual(0x8002080316DB0214L, (ulong)_dataReader.GetLong(), "GetLong(), crossing buffer boundary");
		}

		[Test]
		public void TestLittleEndiannessGetMethods()
		{
			ByteStreamDataReader dataReader = new ByteStreamDataReader(_dataReaderMock, BlockSize, Endianness.Little);
			Assert.AreEqual(0x7079746614000000L, dataReader.GetLong(), "GetLong()");

			dataReader.Position = 4;
			Assert.AreEqual(0x70797466, dataReader.GetInt(), "GetInt()");

			dataReader.Position = 4;
			Assert.AreEqual(0x7466, dataReader.GetShort(), "GetShort()");

			dataReader.Position = 4;
			Assert.AreEqual(0x66, dataReader.GetByte(), "GetByte()");

			dataReader.Position = 4;
			Assert.AreEqual(0x00667479, dataReader.GetThreeBytes(), "GetThreeBytes()");

			dataReader.Dispose();
		}
		#endregion ByteStreamDataReader specific tests
	}
}
