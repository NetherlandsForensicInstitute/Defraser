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
using Defraser.Detector.Common;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestVlcTable
	{
		private static readonly object[,] MockVlcData = new object[,]
		{
			{ "1",		1 },	{ "0011",	4 },
			{ "011",	2 },	{ "0010",	5 },
			{ "010",	3 },	{ "000111111", -1 },
		};

		private static readonly object[,] InvalidData = new object[,]{
			{"001",		4 },	{ "0001", false }
		};

		private VlcTable<int> _mockTable;
		private IInputFile _inputFile;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mockTable = new VlcTable<int>(MockVlcData, 0);
			_inputFile = MockRepository.GenerateStub<IInputFile>();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_mockTable = null;
			_inputFile = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataNull()
		{
			new VlcTable<object>(null, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstructorDataInvalid()
		{
			new VlcTable<int>(InvalidData, 0);
		}

		[Test]
		public void TestMaxBits()
		{
			Assert.AreEqual(9, _mockTable.MaxBits, "MaxBits");
		}

		[Test]
		public void TestIndexer()
		{
			Assert.AreEqual(3, _mockTable[new Vlc(0x2, 3)]);
			Assert.AreEqual(-1, _mockTable[new Vlc(0x03F, 9)]);
		}

		[Test]
		[ExpectedException(typeof(KeyNotFoundException))]
		public void TestIndexerInvalid()
		{
			Assert.AreEqual(3, _mockTable[new Vlc(0xFFF, 12)]);
		}

		[Test]
		public void TestGetVlc()
		{
			Assert.AreEqual(new Vlc(0x02, 4), _mockTable.GetVlc(0x040), "GetVlc()");

			// Brute-force test
			int numBits = _mockTable.MaxBits;
			for (uint code = 0U; code < (1U << numBits); code++)
			{
				string bitstring = (new Vlc(code, numBits)).ToString();
				Vlc vlc = _mockTable.GetVlc(code);

				if (vlc != null)
				{
					Assert.IsTrue(bitstring.StartsWith(vlc.ToString()), "GetVlc(), valid code");
				}
				else
				{
					for (int i = 0; i < MockVlcData.GetLength(0); i++)
					{
						Assert.IsFalse(bitstring.StartsWith(MockVlcData[i, 0] as string), "GetVlc(), invalid code");
					}
				}
			}
		}

		[Test]
		public void TestBitStreamDataReaderGetVlc()
		{
			using (BitStreamDataReader dataReader = new BitStreamDataReader(new MockDataReader(new byte[] { 0x40 }, _inputFile)))
			{
				Assert.AreEqual(3, dataReader.GetVlc(_mockTable), "BitStreamDataReader.GetVlc()");
			}
		}

		[Test]
		public void TestBitStreamDataReaderGetVlcIncrementsPosition()
		{
			using (BitStreamDataReader dataReader = new BitStreamDataReader(new MockDataReader(new byte[] { 0x40 }, _inputFile)))
			{
				Pair<byte,long> bitPosition = dataReader.BitPosition;
				Assert.AreEqual(3, dataReader.GetVlc(_mockTable), "BitStreamDataReader.GetVlc()");
				Assert.That(dataReader.BitPosition, Is.Not.EqualTo(bitPosition));
			}
		}

		[Test]
		public void TestBitPositionEqualTo()
		{
			// Test method Is.EqualTo for Pair<byte,long>
			Pair<byte, long> bitPosition = new Pair<byte,long>(1,2);
			Assert.That(new Pair<byte, long>(1,2), Is.EqualTo(bitPosition));
			Assert.That(new Pair<byte, long>(2,1), Is.Not.EqualTo(bitPosition));
		}
	}
}
