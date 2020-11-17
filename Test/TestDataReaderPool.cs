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
	/// Unit tests for the <see cref="DataReaderPool"/> class.
	/// </summary>
	[TestFixture]
	public class TestDataReaderPool
	{
		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile1;
		private IInputFile _inputFile2;
		private IDataPacket _dataPacket;
		private IDataReader _dataReader1;
		private IDataReader _dataReader2;
		private IDataReader _dataReaderOld1;
		private IDataReader _dataReaderOld2;
		private IDataReader _dataReaderOld3;
		private IDataReader _dataReaderOld4;
		private IProgressReporter _progressReporter;
		private Creator<IDataReader, IDataPacket, IDataReaderPool> _createDataReader;
		private Creator<IDataReader, IDataReader, IProgressReporter> _createProgressDataReader;
		private DataBlockScanner _dataBlockScanner;
		#endregion Mocks and stubs

		#region Objects under test
		private IDataReaderPool _dataReaderPool;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile1 = MockRepository.GenerateStub<IInputFile>();
			_inputFile2 = MockRepository.GenerateStub<IInputFile>();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataReader1 = MockRepository.GenerateStub<IDataReader>();
			_dataReader2 = MockRepository.GenerateStub<IDataReader>();
			_dataReaderOld1 = MockRepository.GenerateStub<IDataReader>();
			_dataReaderOld2 = MockRepository.GenerateStub<IDataReader>();
			_dataReaderOld3 = MockRepository.GenerateStub<IDataReader>();
			_dataReaderOld4 = MockRepository.GenerateStub<IDataReader>();
			_progressReporter = MockRepository.GenerateStub<IProgressReporter>();
			_createDataReader = _mockRepository.StrictMock<Creator<IDataReader, IDataPacket, IDataReaderPool>>();
			_createProgressDataReader = _mockRepository.StrictMock<Creator<IDataReader, IDataReader, IProgressReporter>>();
			_dataBlockScanner = TestFramework.DataBlockScanner;

			_dataReaderPool = new DataReaderPool(_createDataReader, _createProgressDataReader, _dataBlockScanner);

			_inputFile1.Stub(x => x.CreateDataReader()).Return(_dataReader1);
			_inputFile2.Stub(x => x.CreateDataReader()).Return(_dataReader2);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile1 = null;
			_inputFile2 = null;
			_dataPacket = null;
			_dataReader1 = null;
			_dataReader2 = null;
			_dataReaderOld1 = null;
			_dataReaderOld2 = null;
			_dataReaderOld3 = null;
			_dataReaderOld4 = null;
			_progressReporter = null;
			_createDataReader = null;
			_createProgressDataReader = null;
			_dataBlockScanner = null;
			_dataReaderPool = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataReaderCreatorNull()
		{
			new DataReaderPool(null, _createProgressDataReader, _dataBlockScanner);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorProgressDataReaderCreatorNull()
		{
			new DataReaderPool(_createDataReader, null, _dataBlockScanner);
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataBlockScannerNull()
		{
			new DataReaderPool(_createDataReader, _createProgressDataReader, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Dispose() method
		[Test]
		public void DisposeFileDataReaders()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataReader1.Expect(x => x.Dispose());
			}).Verify(delegate
			{
				_dataReaderPool.ReadInputFile(_inputFile1, 0L, new byte[16], 0, 8);
				_dataReaderPool.Dispose();
			});
		}
		#endregion Tests for Dispose() method

		//#region Tests for GetDataReader() method
		//[Test]
		//public void GetDataReader()
		//{
		//    With.Mocks(_mockRepository).Expecting(delegate
		//    {
		//        _createFileDataReader.Expect(x => x(_inputFile1)).Return(_dataReaderOld1);
		//    }).Verify(delegate
		//    {
		//        Assert.AreSame(_dataReaderOld1, _dataReaderPool.GetDataReader(_inputFile1));
		//    });
		//}

		//[Test]
		//public void GetDataReaderCached()
		//{
		//    With.Mocks(_mockRepository).Expecting(delegate
		//    {
		//        _createFileDataReader.Expect(x => x(_inputFile1)).Return(_dataReaderOld1);
		//    }).Verify(delegate
		//    {
		//        IDataReader dataReader = _dataReaderPool.GetDataReader(_inputFile1);
		//        Assert.AreSame(dataReader, _dataReaderPool.GetDataReader(_inputFile1));
		//    });
		//}

		//[Test]
		//public void GetDataReaderCachedTwoDataReaders()
		//{
		//    With.Mocks(_mockRepository).Expecting(delegate
		//    {
		//        _createFileDataReader.Expect(x => x(_inputFile1)).Return(_dataReaderOld1);
		//        _createFileDataReader.Expect(x => x(_inputFile2)).Return(_dataReaderOld2);
		//    }).Verify(delegate
		//    {
		//        IDataReader dataReader = _dataReaderPool.GetDataReader(_inputFile1);
		//        Assert.AreSame(_dataReaderOld2, _dataReaderPool.GetDataReader(_inputFile2));
		//        Assert.AreSame(dataReader, _dataReaderPool.GetDataReader(_inputFile1));
		//    });
		//}

		//[Test]
		//public void GetDataReaderNewDataReaderAfterDispose()
		//{
		//    With.Mocks(_mockRepository).Expecting(delegate
		//    {
		//        _createFileDataReader.Expect(x => x(_inputFile1)).Return(_dataReaderOld1);
		//        _createFileDataReader.Expect(x => x(_inputFile1)).Return(_dataReaderOld2);
		//    }).Verify(delegate
		//    {
		//        _dataReaderPool.GetDataReader(_inputFile1);
		//        _dataReaderPool.Dispose();
		//        Assert.AreSame(_dataReaderOld2, _dataReaderPool.GetDataReader(_inputFile1));
		//    });
		//}

		//[Test]
		//[ExpectedException(typeof(ArgumentNullException))]
		//public void GetDataReaderNull()
		//{
		//    _dataReaderPool.GetDataReader(null);
		//}
		//#endregion Tests for GetDataReader() method

		#region Tests for CreateDataReader(IDataPacket) method
		[Test]
		public void CreateDataReader()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld1);
			}).Verify(delegate
			{
				Assert.AreSame(_dataReaderOld1, _dataReaderPool.CreateDataReader(_dataPacket));
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CreateDataReaderNull()
		{
			_dataReaderPool.CreateDataReader(null);
		}

		[Test]
		public void CreateDataReaderNotCached()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld1);
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld2);
			}).Verify(delegate
			{
				_dataReaderPool.CreateDataReader(_dataPacket);
				Assert.AreSame(_dataReaderOld2, _dataReaderPool.CreateDataReader(_dataPacket));
			});
		}
		#endregion Tests for CreateDataReader(IDataPacket) method

		#region Tests for CreateDataReader(IDataPacket, IProgressReporter) method
		[Test]
		public void CreateDataReaderWithProgressReporter()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld1);
				_createProgressDataReader.Expect(x => x(_dataReaderOld1, _progressReporter)).Return(_dataReaderOld2);
			}).Verify(delegate
			{
				Assert.AreSame(_dataReaderOld2, _dataReaderPool.CreateDataReader(_dataPacket, _progressReporter));
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CreateDataReaderDataPacketNull()
		{
			_dataReaderPool.CreateDataReader(null, _progressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CreateDataReaderProgressReporterNull()
		{
			_dataReaderPool.CreateDataReader(_dataPacket, null);
		}

		[Test]
		public void CreateDataReaderWithProgressReporterNotCached()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld1);
				_createProgressDataReader.Expect(x => x(_dataReaderOld1, _progressReporter)).Return(_dataReaderOld2);
				_createDataReader.Expect(x => x(_dataPacket, _dataReaderPool)).Return(_dataReaderOld3);
				_createProgressDataReader.Expect(x => x(_dataReaderOld3, _progressReporter)).Return(_dataReaderOld4);
			}).Verify(delegate
			{
				_dataReaderPool.CreateDataReader(_dataPacket, _progressReporter);
				Assert.AreSame(_dataReaderOld4, _dataReaderPool.CreateDataReader(_dataPacket, _progressReporter));
			});
		}
		#endregion Tests for CreateDataReader(IDataPacket, IProgressReporter) method
	}
}
