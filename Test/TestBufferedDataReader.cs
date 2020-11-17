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
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestBufferedDataReader : DataReaderTest<BufferedDataReader>
	{
		public const int BufferSize = 1024;

		private MockDataReader _dataReaderMock;

		public override void SetUp()
		{
			base.SetUp();

			_dataReaderMock = new MockDataReader(base.DataReaderData, InputFile);
			_dataReader = new BufferedDataReader(_dataReaderMock, BufferSize);
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

		#region BufferedDataReader specific tests
		[Test]
		public void TestBackwardIssue2435()
		{
			Read(41605, 100);
			// The start and end position of the second block are before and after
			// the start and end position of the first block of data.
			Read(41505, 273);
		}

		[Test]
		public void TestConstructorDefaultBufferSize()
		{
			using (new BufferedDataReader(_dataReaderMock))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderNull()
		{
			using (new BufferedDataReader(null))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorBufferSizeInvalid()
		{
			using (new BufferedDataReader(_dataReaderMock, 0))
			{
			}
		}

		[Test]
		public void TestMockState()
		{
			_dataReaderMock.Dispose();
			Assert.AreEqual(DataReaderState.Cancelled, _dataReader.State, "BufferedDataReader.State (mock disposed)");
		}

		[Test]
		public void TestReadForwardSequential()
		{
			int position = 111;
			Read(position, BufferSize / 2);
			// No overlap
			position += BufferSize / 2;
			Read(position, BufferSize);
			// Overlap and read-back
			position += BufferSize / 2;
			Read(position, BufferSize / 2);
			// Overlap and read-back (2)
			position += 13;
			Read(position, BufferSize / 2);
			// Full block
			position += BufferSize / 2;
			Read(position, BufferSize);
		}

		[Test]
		public void TestReadForwardNonSequential()
		{
			Read(1000, 100);
			Read(1110, 100);
			Read(1220, 100);
		}

		[Test]
		public void TestReadBackwardSequential()
		{
			Read(3778, BufferSize);
			Read(3327, BufferSize / 2);
			Read(2913, BufferSize / 2);
			Read(2417, BufferSize / 2);
		}

		[Test]
		public void TestReadRandomOffset()
		{
			Read(3178, BufferSize / 2);
			Read(62736, BufferSize / 2);
			Read(26183, BufferSize / 2);
		}

		[Test]
		public void TestReadUncached()
		{
			Read(11532, Buffer.Length);
		}

		[Test]
		public void TestZeroPadding()
		{
			// Set buffer to all '6'
			for (int i = 0; i < BufferSize; i++)
			{
				Buffer[i] = 6;
			}

			int position = DataReaderLength - 100;
			_dataReader.Position = position;
			Assert.AreEqual(100, _dataReader.Read(Buffer, 0, BufferSize), "BufferedDataReader number of bytes read");
			Assert.AreEqual(position, _dataReader.Position, "BufferedDataReader.Position (after read)");
			Assert.IsTrue(CompareArrays(Buffer, 0, DataReaderData, position, 100), "BufferedDataReader.Read()");

			// Zero padding is not returned by calls to Read()
			for (int i = 100; i < BufferSize; i++)
			{
				Assert.AreEqual(6, Buffer[i], "BufferedDataReader.Read() zero padding");
			}
		}

		[Test]
		public void TestAllZeroPadding()
		{
			// Set buffer to all '6'
			for (int i = 0; i < BufferSize; i++)
			{
				Buffer[i] = 6;
			}

			int position = DataReaderLength;
			_dataReader.Position = position;
			Assert.AreEqual(0, _dataReader.Read(Buffer, 0, BufferSize), "BufferedDataReader number of bytes read");
			Assert.AreEqual(position, _dataReader.Position, "BufferedDataReader.Position (after read)");

			// Zero padding is not returned by calls to Read()
			for (int i = 0; i < BufferSize; i++)
			{
				Assert.AreEqual(6, Buffer[i], "BufferedDataReader.Read() zero padding");
			}
		}

		[Test]
		public void TestReadNoZeroPadding()
		{
			// Zero padding should not be included in the result of Read()
			_dataReader.Position = DataReaderLength - 10;
			Assert.AreEqual(10, _dataReader.Read(Buffer, 0, 200), "BufferedDataReader number of bytes read");
			_dataReader.Position = DataReaderLength - 20;
			Assert.AreEqual(20, _dataReader.Read(Buffer, 0, 100), "BufferedDataReader number of bytes read");
		}

		[Test]
		public void TestDiscardBuffer()
		{
			// This specifically tests the 'DiscardBuffer()' method
			Read(0, 200);
			_dataReader.Position = 10000;
			Read(10050, 100);
		}

		[Test]
		public void TestFlush()
		{
			Assert.AreEqual(0, _dataReaderMock.Position, "Flush(), before");
			_dataReader.Position = 30;
			Assert.AreEqual(0, _dataReaderMock.Position, "Flush(), position set on mock");
			(_dataReader as BufferedDataReader).Flush();
			Assert.AreEqual(30, _dataReaderMock.Position, "Flush(), positions synchronized");
		}
		#endregion BufferedDataReader specific tests
	}
}
