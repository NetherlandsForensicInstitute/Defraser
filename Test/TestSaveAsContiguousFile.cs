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
using System.Linq;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="SaveAsContiguousFile"/> class.
	/// </summary>
	[TestFixture]
	public class TestSaveAsContiguousFile
	{
		#region Test data
		/// <summary>The path name of the file to write to.</summary>
		private const string FileName = "TestSaveAsContiguousFile__output.mpg";
		/// <summary><c>Length</c> of the first <see cref="IDataPacket"/> to write.</summary>
		private const long DataPacketLength1 = 200L;
		/// <summary><c>Length</c> of the second <see cref="IDataPacket"/> to write.</summary>
		private const long DataPacketLength2 = 177L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IDataPacket _dataPacket;
		private IDataReader _dataReader;
		private IDataWriter _dataWriter;
		private IProgressReporter _progressReporter;
		private IEnumerable<IDetector> _detectors;
		private IDataReaderPool _dataReaderPool;
		private Creator<IDataWriter, string> _createDataWriter;
		private IForensicIntegrityLog _forensicIntegrityLog;
		#endregion Mocks and stubs

		#region Objects under test
		private SaveAsContiguousFile _saveAsContiguousFile;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataReader = MockRepository.GenerateStub<IDataReader>();
			_dataWriter = _mockRepository.StrictMock<IDataWriter>();
			_progressReporter = MockRepository.GenerateStub<IProgressReporter>();
			_detectors = Enumerable.Empty<IDetector>();
			_dataReaderPool = MockRepository.GenerateStub<IDataReaderPool>();
			_createDataWriter = MockRepository.GenerateStub<Creator<IDataWriter, string>>();
			_forensicIntegrityLog = MockRepository.GenerateStub<IForensicIntegrityLog>();

			_dataPacket.Stub(x => x.Length).Return(DataPacketLength1);
			_createDataWriter.Stub(x => x(FileName)).Return(_dataWriter).Repeat.Once();

			_saveAsContiguousFile = new SaveAsContiguousFile(_createDataWriter, _forensicIntegrityLog);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_dataPacket = null;
			_dataReader = null;
			_dataWriter = null;
			_progressReporter = null;
			_detectors = null;
			_dataReaderPool = null;
			_createDataWriter = null;
			_forensicIntegrityLog = null;
			_saveAsContiguousFile = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataWriterCreatorNull()
		{
			new SaveAsContiguousFile(null, _forensicIntegrityLog);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorForensicIntegrityLogNull()
		{
			new SaveAsContiguousFile(_createDataWriter, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Save() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDataPacketsNull()
		{
			_saveAsContiguousFile.Save(null, _detectors, _dataReaderPool, FileName, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDetectorsNull()
		{
			_saveAsContiguousFile.Save(_dataPacket, null, _dataReaderPool, FileName, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDataReaderPoolNull()
		{
			_saveAsContiguousFile.Save(_dataPacket, _detectors, null, FileName, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveFilePathNull()
		{
			_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, null, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SaveFilePathEmpty()
		{
			_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, string.Empty, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveProgressReporterNull()
		{
			_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, null, false);
		}

		[Test]
		public void SaveDataPacket()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataReaderPool.Stub(x => x.CreateDataReader(_dataPacket, _progressReporter)).Return(_dataReader);
				_dataWriter.Stub(x => x.Dispose());
				_dataWriter.Expect(x => x.Write(_dataReader));
			}).Verify(delegate
			{
				_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveDataPacketDataReaderDisposed()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				bool written = false;
				_dataReaderPool.Stub(x => x.CreateDataReader(_dataPacket, _progressReporter)).Return(_dataReader);
				_dataWriter.Stub(x => x.Dispose());
				_dataWriter.Expect(x => x.Write(_dataReader)).WhenCalled(i => written = true);
				_dataReader.Expect(x => x.Dispose()).WhenCalled(i => Assert.IsTrue(written));
			}).Verify(delegate
			{
				_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveDataWriterDisposed()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_dataReaderPool.Stub(x => x.CreateDataReader(_dataPacket, _progressReporter)).Return(_dataReader);
				_dataWriter.Expect(x => x.Write(_dataReader));
				_dataWriter.Expect(x => x.Dispose());
			}).Verify(delegate
			{
				_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveDataReaderPoolNotDisposed()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataReaderPool.Stub(x => x.CreateDataReader(_dataPacket, _progressReporter)).Return(_dataReader);
				_dataWriter.Stub(x => x.Dispose());
				_dataWriter.Expect(x => x.Write(_dataReader));
				_dataReaderPool.Expect(x => x.Dispose()).Repeat.Never();
			}).Verify(delegate
			{
				_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveCancellationPendingUponEntry()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_progressReporter.Stub(x => x.CancellationPending).Return(true);
			}).Verify(delegate
			{
				_saveAsContiguousFile.Save(_dataPacket, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}
		#endregion Tests for Save() method
	}
}
