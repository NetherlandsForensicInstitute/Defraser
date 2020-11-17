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
using System.Diagnostics;
using System.IO;
using Defraser.Framework;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestFileDataReader : DataReaderTest<FileDataReader>
	{
		public override void SetUp()
		{
			base.SetUp();

			_dataReader = new FileDataReader(TestFramework.CreateDataPacket(InputFile, 0, DataFileSize));
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

		#region FileDataReader specific tests
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataPacketNull()
		{
			using (new FileDataReader(null))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void TestConstructorNonExistingFile()
		{
			using (new FileDataReader(TestFramework.CreateDataPacket(TestFramework.CreateInputFile("banaan.aap"), 0, 1)))
			{
			}
		}

		[Test]
		public void TestRead()
		{
			Read(8763, 1020);
			Read(1763, 77);
			Read(6763, 177);
			Read(46111, 3);
			Read(46111, 3);
		}

		[Test]
		public void TestGetDataPacketRandom()
		{
			Debug.Assert(DataReaderLength > 1187);
			TestDataReaderGetDataPacket(_dataReader, 193, 1187);
		}
		#endregion FileDataReader specific tests
	}
}
