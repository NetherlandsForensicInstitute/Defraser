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
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="DataBlock"/> class.
	/// </summary>
	[TestFixture]
	public class TestDataBlock
	{
		private const string FileName1 = "movie.mpg";
		private const long FileLength1 = 128;
		private const string FileName2 = "trailer.avi";
		private const long FileLength2 = 4096;
		private const long StartOffset1 = 10;
		private const long EndOffset1= 100;
		private const long StartOffset2 = 1800;
		private const long EndOffset2 = 3633;

		private MockRepository _mockRepository;
		private IInputFile _inputFile1;
		private IInputFile _inputFile2;
		private IDetector _detector1;
		private IDetector _detector2;
		private IDataBlock _dataBlock1;
		private IDataBlock _dataBlock2;

		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile1 = _mockRepository.StrictMock<IInputFile>();
			_inputFile2 = _mockRepository.StrictMock<IInputFile>();
			_detector1 = _mockRepository.StrictMock<IDetector>();
			_detector2 = _mockRepository.StrictMock<IDetector>();

			With.Mocks(_mockRepository).Expecting(delegate
			{
				SetupResult.For(_inputFile1.Name).Return(FileName1);
				SetupResult.For(_inputFile1.Length).Return(FileLength1);
				SetupResult.For(_inputFile2.Name).Return(FileName2);
				SetupResult.For(_inputFile2.Length).Return(FileLength2);
			});

			_dataBlock1 = CreateDataBlockBuilder1().Build();
			_dataBlock2 = CreateDataBlockBuilder2().Build();
		}

		private IDataBlockBuilder CreateDataBlockBuilder1()
		{
			IDataBlockBuilder dataBlockBuilder1 = TestFramework.CreateDataBlockBuilder();
			dataBlockBuilder1.DataFormat = CodecID.Mpeg2System;
			dataBlockBuilder1.Detectors = new[] { _detector1 };
			dataBlockBuilder1.InputFile = _inputFile1;
			dataBlockBuilder1.StartOffset = StartOffset1;
			dataBlockBuilder1.EndOffset = EndOffset1;
			dataBlockBuilder1.IsFullFile = false;
			return dataBlockBuilder1;
		}

		private IDataBlockBuilder CreateDataBlockBuilder2()
		{
			IDataBlockBuilder dataBlockBuilder2 = TestFramework.CreateDataBlockBuilder();
			dataBlockBuilder2.DataFormat = CodecID.Avi;
			dataBlockBuilder2.Detectors = new[] { _detector2 };
			dataBlockBuilder2.InputFile = _inputFile2;
			dataBlockBuilder2.StartOffset = StartOffset2;
			dataBlockBuilder2.EndOffset = EndOffset2;
			dataBlockBuilder2.IsFullFile = true;
			return dataBlockBuilder2;
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile1 = null;
			_inputFile2 = null;
			_detector1 = null;
			_detector2 = null;
			_dataBlock1 = null;
			_dataBlock2 = null;
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildDetectorNull()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.Detectors = null;
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildInputFileNull()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.InputFile = null;
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildStartOffsetNegative()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.StartOffset = -1;
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildStartOffsetAfterEndOfFile()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.StartOffset = (FileLength1 + 1);
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildEndOffsetNegative()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.EndOffset = -1;
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildEndOffsetBeforeStartOffset()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.EndOffset = (StartOffset1 - 1);
			dataBlockBuilder.Build();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildEndOffsetEqualToStartOffset()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.EndOffset = StartOffset1;
			dataBlockBuilder.Build();
		}

		[Ignore("Disabled for new 'Reference Header' feature")]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildEndOffsetAfterEndOfFile()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.EndOffset = (FileLength1 + 1);
			dataBlockBuilder.Build();
		}

		[Test]
		public void DataFormat()
		{
			Assert.AreEqual(CodecID.Mpeg2System, _dataBlock1.DataFormat, "DataFormat (MPEG-2)");
			Assert.AreEqual(CodecID.Avi, _dataBlock2.DataFormat, "DataFormat (AVI)");
		}

		[Test]
		public void InputFile()
		{
			Assert.AreSame(_inputFile1, _dataBlock1.InputFile);
			Assert.AreSame(_inputFile2, _dataBlock2.InputFile);
		}

		[Test]
		public void Length()
		{
			Assert.AreEqual((EndOffset1 - StartOffset1), _dataBlock1.Length);
			Assert.AreEqual((EndOffset2 - StartOffset2), _dataBlock2.Length);
		}

		[Test]
		public void StartOffset()
		{
			Assert.AreEqual(StartOffset1, _dataBlock1.StartOffset);
			Assert.AreEqual(StartOffset2, _dataBlock2.StartOffset);
		}

		[Test]
		public void EndOffset()
		{
			Assert.AreEqual(EndOffset1, _dataBlock1.EndOffset);
			Assert.AreEqual(EndOffset2, _dataBlock2.EndOffset);
		}

		[Test]
		public void IsFullFile()
		{
			Assert.IsFalse(_dataBlock1.IsFullFile, "Partial file");
			Assert.IsTrue(_dataBlock2.IsFullFile, "Full file");
		}

		[Test]
		public void CodecStreams()
		{
			// TODO
		}

		[Test]
		public void Equals()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			Assert.IsTrue(_dataBlock1.Equals(dataBlockBuilder.Build()));
			Assert.IsFalse(_dataBlock1.Equals(_dataBlock2));
			Assert.IsFalse(_dataBlock2.Equals(_dataBlock1));
		}

		[Test]
		public void EqualsDifferentDataFormat()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.DataFormat = CodecID.Mpeg1System;
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentDetector()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.Detectors = new[] { _detector2 };
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentInputFile()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.InputFile = _inputFile2;
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentStartOffset()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.StartOffset = (StartOffset1 + 1);
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentEndOffset()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.EndOffset = (EndOffset1 + 1);
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentIsFullFile()
		{
			IDataBlockBuilder dataBlockBuilder = CreateDataBlockBuilder1();
			dataBlockBuilder.IsFullFile = true;
			Assert.IsFalse(_dataBlock1.Equals(dataBlockBuilder.Build()));
		}

		[Test]
		public void EqualsDifferentCodecStreams()
		{
			// TODO
		}
	}
}
