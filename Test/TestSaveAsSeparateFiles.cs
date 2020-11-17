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
	/// Unit tests for the <see cref="SaveAsSeparateFiles"/> class.
	/// </summary>
	[TestFixture]
	public class TestSaveAsSeparateFiles
	{
		#region Test data
		/// <summary>The path name of the file to write to.</summary>
		private const string FileName = "TestSaveAsSeparateFiles__output.m2v";
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private ICodecStream _codecStream;
		private IDataWriter _dataWriter;
		private IProgressReporter _progressReporter1;
		private IEnumerable<IDetector> _detectors;
		private IDataReaderPool _dataReaderPool;
		private Creator<IDataWriter, string> _createDataWriter;
		private Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
		private IForensicIntegrityLog _forensicIntegrityLog;
		#endregion Mocks and stubs

		#region Objects under test
		private SaveAsSeparateFiles _saveAsSeparateFiles;
		#endregion Objects under test

		#region Properties
		private IEnumerable<object> NoItems { get { return new List<object>(); } }
		private IEnumerable<object> OneCodecStream { get { return new List<object> { _codecStream }; } }
		#endregion Properties

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_codecStream = MockRepository.GenerateStub<ICodecStream>();
			_dataWriter = _mockRepository.StrictMock<IDataWriter>();
			_progressReporter1 = MockRepository.GenerateStub<IProgressReporter>();
			_detectors = Enumerable.Empty<IDetector>();
			_dataReaderPool = MockRepository.GenerateStub<IDataReaderPool>();
			_createDataWriter = MockRepository.GenerateStub<Creator<IDataWriter, string>>();
			_createSubProgressReporter = MockRepository.GenerateStub<Creator<IProgressReporter, IProgressReporter, long, long, long>>();
			_forensicIntegrityLog = MockRepository.GenerateStub<IForensicIntegrityLog>();

			_createDataWriter.Stub(x => x(FileName)).Return(_dataWriter).Repeat.Once();

			_saveAsSeparateFiles = new SaveAsSeparateFiles(_createDataWriter, _createSubProgressReporter, TestFramework.DataBlockScanner, _forensicIntegrityLog);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_codecStream = null;
			_dataWriter = null;
			_progressReporter1 = null;
			_detectors = null;
			_dataReaderPool = null;
			_createDataWriter = null;
			_createSubProgressReporter = null;
			_saveAsSeparateFiles = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataWriterCreatorNull()
		{
			new SaveAsSeparateFiles(null, _createSubProgressReporter, TestFramework.DataBlockScanner, _forensicIntegrityLog);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorSubProgressReporterCreatorNull()
		{
			new SaveAsSeparateFiles(_createDataWriter, null, TestFramework.DataBlockScanner, _forensicIntegrityLog);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataBlockScannerNull()
		{
			new SaveAsSeparateFiles(_createDataWriter, _createSubProgressReporter, null, _forensicIntegrityLog);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorForensicIntegrityLogNull()
		{
			new SaveAsSeparateFiles(_createDataWriter, _createSubProgressReporter, TestFramework.DataBlockScanner, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Save() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveItemsNull()
		{
			_saveAsSeparateFiles.Save(null, _detectors, _dataReaderPool, FileName, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SaveItemsEmpty()
		{
			_saveAsSeparateFiles.Save(new List<object>(), _detectors, _dataReaderPool, FileName, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDetectorsNull()
		{
			_saveAsSeparateFiles.Save(OneCodecStream, null, _dataReaderPool, FileName, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDataReaderPoolNull()
		{
			_saveAsSeparateFiles.Save(OneCodecStream, _detectors, null, FileName, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveDirectoryNull()
		{
			_saveAsSeparateFiles.Save(OneCodecStream, _detectors, _dataReaderPool, null, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SaveDirectoryEmpty()
		{
			_saveAsSeparateFiles.Save(OneCodecStream, _detectors, _dataReaderPool, string.Empty, _progressReporter1, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveProgressReporterNull()
		{
			_saveAsSeparateFiles.Save(OneCodecStream, _detectors, _dataReaderPool, FileName, null, false);
		}

		// TODO: 'real' tests

		[Test]
		public void SaveCancellationPendingUponEntry()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_progressReporter1.Stub(x => x.CancellationPending).Return(true);
			}).Verify(delegate
			{
				_saveAsSeparateFiles.Save(OneCodecStream, _detectors, _dataReaderPool, FileName, _progressReporter1, false);
			});
		}
		#endregion Tests for Save() method
	}
}
