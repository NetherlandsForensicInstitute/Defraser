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
	/// Unit tests for the <see cref="SaveAsSingleFile"/> class.
	/// </summary>
	[TestFixture]
	public class TestSaveAsSingleFile
	{
		#region Test data
		/// <summary>The path name of the file to write to.</summary>
		private const string FileName = "TestSaveAsSingleFile__output.mpg";
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile;
		private IEnumerable<IDetector> _detectors;
		private IDataPacket _dataPacket;
		private IDataReader _dataReader;
		private IDataWriter _dataWriter;
		private IProgressReporter _progressReporter;
		private IDataReaderPool _dataReaderPool;
		private Creator<IDataWriter, string> _createDataWriter;
		#endregion Mocks and stubs

		#region Objects under test
		private SaveAsSingleFile _saveAsSingleFile;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile = MockRepository.GenerateStub<IInputFile>();
			_detectors = Enumerable.Empty<IDetector>();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataReader = MockRepository.GenerateStub<IDataReader>();
			_dataWriter = _mockRepository.StrictMock<IDataWriter>();
			_progressReporter = MockRepository.GenerateStub<IProgressReporter>();
			_dataReaderPool = MockRepository.GenerateStub<IDataReaderPool>();
			_createDataWriter = MockRepository.GenerateStub<Creator<IDataWriter, string>>();

			_inputFile.Stub(x => x.CreateDataPacket()).Return(_dataPacket);
			_dataReaderPool.Stub(x => x.CreateDataReader(_dataPacket, _progressReporter)).Return(_dataReader);
			_createDataWriter.Stub(x => x(FileName)).Return(_dataWriter).Repeat.Once();

			_saveAsSingleFile = new SaveAsSingleFile(_createDataWriter);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile = null;
			_detectors = null;
			_dataPacket = null;
			_dataReader = null;
			_dataWriter = null;
			_progressReporter = null;
			_dataReaderPool = null;
			_createDataWriter = null;
			_saveAsSingleFile = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataWriterCreatorNull()
		{
			new SaveAsSingleFile(null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Save() method
		[Test]
		public void Save()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataWriter.Stub(x => x.Dispose());
				_dataWriter.Expect(x => x.Write(_dataReader));
			}).Verify(delegate
			{
				_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveInputFileNull()
		{
			_saveAsSingleFile.Save(null, _detectors, _dataReaderPool, FileName, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDetectorsNull()
		{
			_saveAsSingleFile.Save(_inputFile, null, _dataReaderPool, FileName, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveFilePathNull()
		{
			_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, null, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SaveFilePathEmpty()
		{
			_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, string.Empty, _progressReporter, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveProgressReporterNull()
		{
			_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, FileName, null, false);
		}

		[Test]
		public void SaveDataReaderDisposed()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				bool written = false;
				_dataWriter.Stub(x => x.Dispose());
				_dataWriter.Expect(x => x.Write(_dataReader)).WhenCalled(i => written = true);
				_dataReader.Expect(x => x.Dispose()).WhenCalled(i => Assert.IsTrue(written));
			}).Verify(delegate
			{
				_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveDataWriterDisposed()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_dataWriter.Expect(x => x.Write(_dataReader));
				_dataWriter.Expect(x => x.Dispose());
			}).Verify(delegate
			{
				_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}

		[Test]
		public void SaveCancellationPending()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_progressReporter.Stub(x => x.CancellationPending).Return(true);
			}).Verify(delegate
			{
				_saveAsSingleFile.Save(_inputFile, _detectors, _dataReaderPool, FileName, _progressReporter, false);
			});
		}
		#endregion Tests for Save() method
	}
}
