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
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestBitStreamDataReader : DataReaderTest<BitStreamDataReader>
	{
		private const int BlockSize = 32;

		// Sample data from 'FalseDetectionOfDataBlock_MPEG-2_met_geluid_NFI_part_0-1FFF_1E12800-1E13800.mpg'
		private static readonly byte[] MpegData =
		{
			0x00, 0x00, 0x01, 0xBA, 0x44, 0x00, 0x0C, 0x65,
			0x04, 0x01, 0x01, 0x89, 0xC3, 0xF8, 0x00, 0x00,
			0x01, 0xBB, 0x00, 0x12, 0x80, 0xC4, 0xE1, 0x04,
			0xE1, 0x7F, 0xB9, 0xE0, 0xE8, 0xB8, 0xC0, 0x20,
			0xBD, 0xE0, 0x3A, 0xBF, 0xE0, 0x02, 0x00, 0x00,
			0x01, 0xBF, 0x03, 0xD4, 0x00, 0x00, 0x1A, 0xB8,
			0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x01
		};

		private static readonly byte[] MpegDataEnd =
		{
			0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x01, 0xB9
		};

		private MockDataReader _dataReaderMock;

		#region Properties
		public override byte[] DataReaderData { get { return MpegData; } }
		#endregion Properties


		public override void SetUp()
		{
			base.SetUp();

			_dataReaderMock = new MockDataReader(MpegData, InputFile);
			_dataReader = new BitStreamDataReader(_dataReaderMock, BlockSize);
		}

		#region BitStreamDataReader specific tests
		[Test]
		public void TestConstructor()
		{
			using (new BitStreamDataReader(_dataReaderMock))
			{
				Assert.IsTrue(true, "1 argument constructor, default block size");
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderNull()
		{
			using (new BitStreamDataReader(null))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorBlockSizeInvalid()
		{
			using (new BitStreamDataReader(_dataReaderMock, 7))
			{
			}
		}

		[Test]
		public void TestPositionBitsInCache()
		{
			_dataReader.Position = 2;
			_dataReader.GetBits(8);
			Assert.AreEqual(3, _dataReader.Position, "Position, with bits in cache, byte-aligned");
			_dataReader.Position = 3;
			Assert.AreEqual(3, _dataReader.Position, "Position (set), bit cache flushed");
			_dataReader.GetBits(5);
			_dataReader.ShowBits(32);
			Assert.AreEqual(4, _dataReader.Position, "Position, with bits in cache, unaligned");
		}

		[Test]
		public void TestPositionNoBitsInCache()
		{
			_dataReader.Position = 3;
			Assert.AreEqual(3, _dataReader.Position, "Position (set)");
			_dataReader.Position = 5;
			Assert.AreEqual(5, _dataReader.Position, "Position (set)");
		}

		[Test]
		public void TestReadCorruptsReadBack()
		{
			_dataReader.Position = 0;
			_dataReader.GetBits(15);
			_dataReader.Read(Buffer, 0, 2 * BlockSize);
			Assert.AreEqual(0x01, _dataReader.GetByte(), "GetByte() after corrupting Read()");
			Assert.AreEqual(3, _dataReader.Position, "Position after corrupting Read()");
		}

		[Test]
		public void TestReadBitsInCache()
		{
			_dataReader.Position = 2;
			_dataReader.Read(Buffer, 0, BlockSize / 2);
			Assert.IsTrue(CompareArrays(Buffer, 0, DataReaderData, 2, BlockSize / 2), "Read() with empty bit cache");
			_dataReader.Position = 0;
			_dataReader.GetBits(15);
			Assert.AreEqual(2, _dataReader.Position, "Position before Read()");
			_dataReader.Read(Buffer, 0, BlockSize / 2);
			Assert.IsTrue(CompareArrays(Buffer, 0, DataReaderData, 2, BlockSize / 2), "Read() with bits in cache");
		}

		[Test]
		public void TestGetByte()
		{
			_dataReader.ShowBits(32);
			_dataReader.Position = 2;
			_dataReader.ShowBits(32);

			int bytesRead = 0;
			long previousPosition = _dataReader.Position;

			while (_dataReader.Position < DataReaderLength)
			{
				Assert.AreEqual(DataReaderState.Ready, _dataReader.State, "State ready before GetByte()");
				Assert.AreEqual(MpegData[2 + bytesRead++], _dataReader.GetByte(), "GetByte()");
				Assert.AreEqual(previousPosition + 1, _dataReader.Position, "Sequential read using GetByte()");
				previousPosition = _dataReader.Position;
			}

			Assert.AreEqual(DataReaderLength - 2, bytesRead, "Number of bytes read");
			Assert.AreEqual(DataReaderLength, _dataReader.Position, "Position after sequential read");
			Assert.AreEqual(DataReaderState.EndOfInput, _dataReader.State, "State after sequential read");
		}

		[Test]
		public void TestGetBits()
		{
			Assert.AreEqual(442, _dataReader.GetBits(32), "GetBits(32) with empty bitcache");
			Assert.AreEqual(0, _dataReader.GetBits(1), "GetBits(1) with empty bitcache");
			Assert.AreEqual(1, _dataReader.GetBits(1), "GetBits(1) with non-empty bitcache");
			Assert.AreEqual(16778009, _dataReader.GetBits(28), "GetBits(28) with sufficient bitcache");
			Assert.AreEqual(8, _dataReader.GetBits(5), "GetBits(5) with insufficient bitcache");
			Assert.AreEqual(537398350, _dataReader.GetBits(32), "GetBits(32) with half-filled bitcache");
			_dataReader.NextStartCode(24, 0x000001, 8);
			Assert.AreEqual(443, _dataReader.GetBits(32), "GetBits(32) with full bitcache");
			Assert.AreEqual(0, _dataReader.GetBits(5), "GetBits(5) with empty bitcache");
			Assert.AreEqual(38803612, _dataReader.GetBits(32), "GetBits(32) at bit position 0");
			Assert.AreEqual(547106807, _dataReader.GetBits(32), "GetBits(32) at bit position 5");
		}

		[Test]
		public void TestShowBits()
		{
			Assert.AreEqual(0x000001BA, _dataReader.ShowBits(32), "ShowBits(32) with empty bit cache");
			Assert.AreEqual(0x000001BA, _dataReader.ShowBits(32), "ShowBits(32) with full bit cache");
			_dataReader.Position = 3;
			Assert.AreEqual(0x2E9, _dataReader.ShowBits(10), "ShowBits(10) with empty bit cache");
			_dataReader.GetBits(29);
			Assert.AreEqual(0x11, _dataReader.ShowBits(5), "ShowBits(5) with insufficient bit cache");
			_dataReader.GetBits(3);
			Assert.AreEqual(0x65, _dataReader.ShowBits(8), "ShowBits(8) with sufficient bit cache");
		}

		[Test]
		public void TestByteAlign()
		{
			_dataReader.Position = 0;
			_dataReader.NextStartCode(24, 0x000001, 8);
			_dataReader.GetBits(28);
			_dataReader.ByteAlign();
			Assert.AreEqual(4, _dataReader.Position, "Position after ByteAlign()");
			_dataReader.Position = 0;
			_dataReader.GetBits(3);
			_dataReader.ShowBits(32);
			_dataReader.GetBits(30);
			_dataReader.ByteAlign();
			Assert.AreEqual(5, _dataReader.Position, "Position after ByteAlign()");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNextStartCodeNegativePrefixBits()
		{
			_dataReader.NextStartCode(-1, 0x0, 8);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNextStartCodeZeroPrefixBits()
		{
			_dataReader.NextStartCode(0, 0x0, 8);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNextStartCodeOverflowPrefixBits()
		{
			_dataReader.NextStartCode(33, 0x000000001, 8);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNextStartCodeNegativeSuffixBits()
		{
			_dataReader.NextStartCode(24, 0x000001, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestNextStartCodeOverflowSuffixBits()
		{
			_dataReader.NextStartCode(24, 0x000001, 9);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestNextStartCodeInvalidPrefixCode()
		{
			_dataReader.NextStartCode(24, 0x1FFFFFF, 8);
		}

		[Test]
		public void TestNextStartCode()
		{
			Assert.AreEqual(0x000001BA, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() at begin");
			Assert.AreEqual(0, _dataReader.Position, "Position after NextStartCode()");
			Assert.AreEqual(0x000001BA, _dataReader.ShowBits(32), "ShowBits(32) after NextStartCode()");
			Assert.AreEqual(0x000001BA, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() re-invoked");
			Assert.AreEqual(0x000001BA, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() re-invoked");
			_dataReader.GetBits(8);
			Assert.AreEqual(0x000001BB, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() from Position 1");
			Assert.AreEqual(14, _dataReader.Position, "Position after NextStartCode()");
			_dataReader.GetBits(32);
			_dataReader.NextStartCode(24, 0x000001, 8);
			_dataReader.GetBits(32);
			Assert.AreEqual(42, _dataReader.Position, "Position before NextStartCode()");
			Assert.AreEqual(0, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() from Position 42");
			Assert.AreEqual(DataReaderLength, _dataReader.Position, "Position after unsuccessful call to NextStartCode()");
			Assert.AreEqual(DataReaderState.EndOfInput, _dataReader.State, "State after unsuccessful call to NextStartCode()");
			Assert.AreEqual(0, _dataReader.NextStartCode(24, 0x000001, 8), "NextStartCode() from end-of-input");
			Assert.AreEqual(DataReaderState.EndOfInput, _dataReader.State, "State after unsuccessful call to NextStartCode()");
			_dataReader.GetBits(32);
			Assert.AreEqual(0, _dataReader.GetBits(32), "Zero padding");
			_dataReader.Position -= 8;
			Assert.AreEqual(0, _dataReader.NextStartCode(24, 0x000001, 8), "Partial start code at end-of-input");
			Assert.AreEqual(DataReaderState.EndOfInput, _dataReader.State, "State after partial start code at end-of-input");
		}

		[Test]
		public void TestStartCodeAtEndOfData()
		{
			using (var dataReader = new BitStreamDataReader(new MockDataReader(MpegDataEnd, InputFile)))
			{
				Assert.AreEqual(0x000001B9, dataReader.NextStartCode(24, 0x000001, 8), "Start code at end of data");
			}
		}

		[Test]
		public void TestBufferOffsetAdjustment()
		{
			// see BufferedDataReader.FillBuffer(int), last line
			using (var dataReader = new BitStreamDataReader(new MockDataReader(MpegDataEnd, InputFile), 8))
			{
				dataReader.GetBits(32);
				dataReader.GetBits(32);
				Assert.AreEqual(8, dataReader.Position, "Position at end-of-input");
				dataReader.ShowBits(32);
				Assert.AreEqual(8, dataReader.Position, "Position at end-of-input in zero-padding area");
				dataReader.Read(Buffer, 0, 16);
				Assert.AreEqual(8, dataReader.Position, "Position at end-of-input after Read()");
				dataReader.NextStartCode(24, 0x000001, 8);
				Assert.AreEqual(0, dataReader.NextStartCode(24, 0x000001, 8), "Start code after end of data");
			}
		}

		[Test]
		public void TestFillBitCache()
		{
			Assert.AreEqual(0, _dataReader.GetBits(3), "FillBitCache() with no bits in cache, byte aligned");
			Assert.AreEqual(0xDD2, _dataReader.GetBits(32), "FillBitCache() with bits in cache or empty, unaligned");
			Assert.AreEqual(0x04, _dataReader.ShowBits(5), "ShowBits(5) after FillBitCache() unaligned with empty bit cache");
			_dataReader.Position = BlockSize - 2;
			Assert.AreEqual(0xC020BDE0, _dataReader.ShowBits(32), "ShowBits(32) after FillBitCache() crossing internal buffer boundary");
			Assert.AreEqual(BlockSize - 2, _dataReader.Position, "Position after FillBitCache() crossing internal buffer boundary");
			_dataReader.Position = 0;
			_dataReader.GetBits(3);
			_dataReader.ShowBits(32);
			_dataReader.GetBits(1);
			_dataReader.ShowBits(32);
			Assert.AreEqual(0x00001BA4, _dataReader.ShowBits(32), "ShowBits(32) after FillBitCache() remaining bits from current byte");
		}

		[Test]
		public void TestByteAligned()
		{
			_dataReader.Position = 0;
			// No bits read yet; check that the stream is byte aligned
			Assert.That(_dataReader.ByteAligned, Is.True);
			// Read the first bit and check that the stream is no longer byte aligned
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			_dataReader.GetBits(1);
			// After reading 8 bits the stream should be byte aligned again
			Assert.That(_dataReader.ByteAligned, Is.True);
			// After reading a complete byte, the stream should still be byte aligned
			_dataReader.GetByte();
			Assert.That(_dataReader.ByteAligned, Is.True);
			_dataReader.GetByte();
			_dataReader.GetByte();
			// One complete int is read, check still byte aligned
			Assert.That(_dataReader.ByteAligned, Is.True);
			// Show bits should not have any impact the result
			_dataReader.ShowBits(1);
			Assert.That(_dataReader.ByteAligned, Is.True);
			_dataReader.ShowBits(10);
			Assert.That(_dataReader.ByteAligned, Is.True);
			// Move the position one bit; check no longer byte aligned
			_dataReader.GetBits(1);
			Assert.That(_dataReader.ByteAligned, Is.False);
			// Call ByteAlign to align the stream and check the result
			_dataReader.ByteAlign();
			Assert.That(_dataReader.ByteAligned, Is.True);
		}
		#endregion BitStreamDataReader specific tests
	}
}
