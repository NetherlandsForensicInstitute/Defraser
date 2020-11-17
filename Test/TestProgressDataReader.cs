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
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestProgressDataReader : DataReaderTest<ProgressDataReader>
	{
		private MockDataReader _dataReaderMock;
		private IProgressReporter _progressReporterMock;


		public override void SetUp()
		{
			base.SetUp();

			_progressReporterMock = MockRepository.GenerateStub<IProgressReporter>();
			_dataReaderMock = new MockDataReader(base.DataReaderData, InputFile);
			_dataReader = new ProgressDataReader(_dataReaderMock, _progressReporterMock);
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

		#region ProgressDataReader specific tests
		[Test]
		public void TestConstructor()
		{
			using (new ProgressDataReader(_dataReaderMock, _progressReporterMock))
			{
				Assert.IsTrue(true, "2 argument constructor");
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderNull()
		{
			using (new ProgressDataReader(null, _progressReporterMock))
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorProgressReporterNull()
		{
			using (new ProgressDataReader(_dataReaderMock, null))
			{
			}
		}

		[Test]
		public void TestCancellationPending()
		{
			Assert.IsFalse(_progressReporterMock.CancellationPending, "CancellationPending (false)");
			Assert.AreEqual(DataReaderState.Ready, _dataReader.State, "DataReaderState (Ready)");
			_progressReporterMock.Stub(x => x.CancellationPending).Return(true);
			Assert.IsTrue(_progressReporterMock.CancellationPending, "CancellationPending (true)");
			Assert.AreEqual(DataReaderState.Cancelled, _dataReader.State, "DataReaderState (Cancelled)");
		}

		[Test]
		public void TestReportProgress()
		{
			// TODO: test automatic, Position-based progress reporting
		}
		#endregion ProgressDataReader specific tests
	}
}
