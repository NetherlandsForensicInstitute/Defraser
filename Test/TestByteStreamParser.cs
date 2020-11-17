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

using System.Linq;
using Defraser.Detector.Common;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using System;

namespace Defraser.Test
{
	[TestFixture]
	public class TestByteStreamParser
	{
		/// <summary>The <c>Name</c> of the test file.</summary>
		private const string TestFileName = "test.txt";
		/// <summary>The <c>Length</c> of the test file (for stub behavior).</summary>
		private const long TestFileLength = 4096L;

		// Dummy data for the MockDataReader
		private static readonly byte[] MockData =
		{
			0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
			0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
			0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
			0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
			0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
			0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67,
			0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77,
			0xF0, 0x0F
		};

		private IDetector _detector;
		private IInputFile _inputFile;
		private IDataReader _dataReader;
		private ByteStreamDataReader _byteStreamDataReader;
		private MockByteStreamParser _byteStreamParser;
		private MockHeader _header;
		private MockHeader _childHeader;
		private MockHeader _grandChildHeader;
		private MockHeader _mockAttributeHeader;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_detector = new MockDetectorDataStructures();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_detector = null;
		}

		[SetUp]
		public void SetUp()
		{
			_inputFile = MockRepository.GenerateStub<IInputFile>();
			_inputFile.Stub(x => x.Name).Return(TestFileName);
			_inputFile.Stub(x => x.Length).Return(TestFileLength);

			_dataReader = new MockDataReader(MockData, _inputFile);
			_byteStreamDataReader = new ByteStreamDataReader(_dataReader);
			_byteStreamParser = new MockByteStreamParser(_byteStreamDataReader);
			_header = new MockHeader(Enumerable.Repeat(_detector, 1), MockHeaderName.Root);
			_childHeader = new MockHeader(_header, MockHeaderName.MockHeaderTypeTwo);
			_grandChildHeader = new MockSubHeader(_childHeader);
			_mockAttributeHeader = new MockAttributeHeader(_grandChildHeader);
		}

		[TearDown]
		public void TearDown()
		{
			_inputFile = null;
			_dataReader = null;
			_byteStreamDataReader = null;
			_byteStreamParser = null;
			_header = null;
			_childHeader = null;
			_grandChildHeader = null;
			_mockAttributeHeader = null;
		}

		[Test]
		public void TestThatByteStreamReaderIsNotNull()
		{
			Assert.That(_byteStreamParser, Is.Not.Null);
		}

		[Test]
		public void TestThatGetByteReturnCorrectValue()
		{
			_byteStreamParser.Position = 4;
			Assert.That(_byteStreamParser.GetByte(), Is.EqualTo(4));

		}

		[Test]
		public void TestThatGetByteProgressesPosition()
		{
			_byteStreamParser.Position = 4;
			Assert.That(_byteStreamParser.GetByte(), Is.EqualTo(4));
			Assert.That(_byteStreamParser.GetByte(), Is.EqualTo(5));
		}

		[Test]
		public void TestGetIntWithEndianArgument()
		{
			_byteStreamParser.Position = 0;

			_byteStreamDataReader.Endianness = Endianness.Big;

			Assert.That(_byteStreamParser.GetInt(Endianness.Little), Is.EqualTo(BitConverter.ToInt32(new byte[] { 0, 1, 2, 3 }, 0)));
		}

		[Test]
		public void TestGetFirstValueFromDataReaderLittleEndian()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetShort(), Is.EqualTo(BitConverter.ToInt16(new byte[] { 0, 1 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetUShort(), Is.EqualTo(BitConverter.ToUInt16(new byte[] { 0, 1 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetInt(), Is.EqualTo(BitConverter.ToInt32(new byte[] { 0, 1, 2, 3 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetUInt(), Is.EqualTo(BitConverter.ToUInt32(new byte[] { 0, 1, 2, 3 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetLong(), Is.EqualTo(BitConverter.ToInt64(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetULong(), Is.EqualTo(BitConverter.ToUInt64(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0)));
		}

		[Test]
		public void TestGetFirstValueFromDataReaderBigEndian()
		{
			_byteStreamDataReader.Endianness = Endianness.Big;

			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetShort(), Is.EqualTo(BitConverter.ToInt16(new byte[] { 1, 0 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetUShort(), Is.EqualTo(BitConverter.ToUInt16(new byte[] { 1, 0 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetInt(), Is.EqualTo(BitConverter.ToInt32(new byte[] { 3, 2, 1, 0 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetUInt(), Is.EqualTo(BitConverter.ToUInt32(new byte[] { 3, 2, 1, 0 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetLong(), Is.EqualTo(BitConverter.ToInt64(new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 }, 0)));
			_byteStreamParser.Position = 0;
			Assert.That(_byteStreamParser.GetULong(), Is.EqualTo(BitConverter.ToUInt64(new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 }, 0)));
		}

		[Test]
		public void TestGetLastValueFromReader()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 65;
			Assert.That(_byteStreamParser.GetByte(), Is.EqualTo(0x0F));
			_byteStreamParser.Position = 64;
			Assert.That(_byteStreamParser.GetShort(), Is.EqualTo(BitConverter.ToInt16(new byte[] { 0xF0, 0x0F }, 0)));
			_byteStreamParser.Position = 64;
			Assert.That(_byteStreamParser.GetUShort(), Is.EqualTo(BitConverter.ToUInt16(new byte[] { 0xF0, 0x0F }, 0)));
			_byteStreamParser.Position = 62;
			Assert.That(_byteStreamParser.GetInt(), Is.EqualTo(BitConverter.ToInt32(new byte[] { 0x76, 0x77, 0xF0, 0x0F }, 0)));
			_byteStreamParser.Position = 62;
			Assert.That(_byteStreamParser.GetUInt(), Is.EqualTo(BitConverter.ToUInt32(new byte[] { 0x76, 0x77, 0xF0, 0x0F }, 0)));
			_byteStreamParser.Position = 58;
			Assert.That(_byteStreamParser.GetLong(), Is.EqualTo(BitConverter.ToInt64(new byte[] { 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0xF0, 0x0F }, 0)));
			_byteStreamParser.Position = 58;
			Assert.That(_byteStreamParser.GetULong(), Is.EqualTo(BitConverter.ToUInt64(new byte[] { 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0xF0, 0x0F }, 0)));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetByteOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 66;
			Assert.That(_byteStreamParser.GetByte(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetShortOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 65;
			Assert.That(_byteStreamParser.GetShort(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetUShortOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 65;
			Assert.That(_byteStreamParser.GetUShort(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetIntOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 63;
			Assert.That(_byteStreamParser.GetInt(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetUIntOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 63;
			Assert.That(_byteStreamParser.GetUInt(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetLongOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 59;
			Assert.That(_byteStreamParser.GetLong(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(ReadOverflowException))]
		public void TestGetULongOneByteBeyondTheEnd()
		{
			_byteStreamDataReader.Endianness = Endianness.Little;

			_byteStreamParser.Position = 59;
			Assert.That(_byteStreamParser.GetULong(), Is.EqualTo(0));
		}
	}
}
