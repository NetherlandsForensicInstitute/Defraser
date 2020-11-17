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
using Defraser.Util;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="FileScanner"/> class.
	/// </summary>
	[TestFixture]
	public class TestFileScanner
	{
		#region Test data
		/// <summary>The <c>Name</c> of the test file.</summary>
		private const string TestFileName = "test.txt";
		/// <summary>The <c>Length</c> of the test file (for stub behavior).</summary>
		private const long TestFileLength = 4096L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IDataScanner _containerDataScanner;
		private IDataScanner _codecDataScanner;
		private IDataScanner _codecStreamDataScanner;
		private IDataReaderPool _dataReaderPool;
		private IProgressReporter _progressReporter;
		private Creator<IDataBlockBuilder> _createDataBlockBuilder;
		private Creator<IProgressReporter, IProgressReporter, long, long, long> _createSubProgressReporter;
		private IInputFile _inputFile;
		#endregion Mocks and stubs

		#region Objects under test
		private FileScanner _fileScanner;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_containerDataScanner = MockRepository.GenerateStub<IDataScanner>();
			_codecDataScanner = MockRepository.GenerateStub<IDataScanner>();
			_codecStreamDataScanner = MockRepository.GenerateStub<IDataScanner>();
			_dataReaderPool = MockRepository.GenerateStub<IDataReaderPool>();
			_progressReporter = MockRepository.GenerateStub<IProgressReporter>();
			_createDataBlockBuilder = MockRepository.GenerateStub<Creator<IDataBlockBuilder>>();
			_createSubProgressReporter = MockRepository.GenerateStub<Creator<IProgressReporter, IProgressReporter, long, long, long>>();
			_inputFile = MockRepository.GenerateStub<IInputFile>();

			_fileScanner = new FileScanner(_containerDataScanner, _codecDataScanner, _codecStreamDataScanner, _dataReaderPool, _createDataBlockBuilder, _createSubProgressReporter);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_containerDataScanner = null;
			_codecDataScanner = null;
			_codecStreamDataScanner = null;
			_dataReaderPool = null;
			_progressReporter = null;
			_createDataBlockBuilder = null;
			_createSubProgressReporter = null;
			_inputFile = null;
			_fileScanner = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorContainerDataScannerNull()
		{
			new FileScanner(null, _codecDataScanner, _codecStreamDataScanner, _dataReaderPool, _createDataBlockBuilder, _createSubProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorCodecDataScannerNull()
		{
			new FileScanner(_containerDataScanner, null, _codecStreamDataScanner, _dataReaderPool, _createDataBlockBuilder, _createSubProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorCodecStreamDataScannerNull()
		{
			new FileScanner(_containerDataScanner, _codecDataScanner, null, _dataReaderPool, _createDataBlockBuilder, _createSubProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataReaderPoolNull()
		{
			new FileScanner(_containerDataScanner, _codecDataScanner, _codecStreamDataScanner, null, _createDataBlockBuilder, _createSubProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataBlockBuilderCreatorNull()
		{
			new FileScanner(_containerDataScanner, _codecDataScanner, _codecStreamDataScanner, _dataReaderPool, null, _createSubProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorSubProgressReporterCreatorNull()
		{
			new FileScanner(_containerDataScanner, _codecDataScanner, _codecStreamDataScanner, _dataReaderPool, _createDataBlockBuilder, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Scan() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ScanInputFileNull()
		{
			_fileScanner.Scan((IInputFile)null, _progressReporter);
		}

		[Test]
		public void ScanEmptyInputFile()
		{
			IInputFile emptyInputFile = MockRepository.GenerateStub<IInputFile>();
			emptyInputFile.Stub(x => x.Name).Return("TestFileName");
			emptyInputFile.Stub(x => x.Length).Return(0L);

			With.Mocks(_mockRepository).Expecting(delegate
			{
			}).Verify(delegate
			{
				_fileScanner.Scan(emptyInputFile, _progressReporter);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ScanProgressReporterNull()
		{
			_fileScanner.Scan(_inputFile, null);
		}
		#endregion Tests for Scan() method
	}
}
