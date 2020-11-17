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
using Defraser;
using DetectorCommon;
using NUnit.Framework;

namespace TestDefraser
{
	[TestFixture]
	public class TestCodecStream
	{
		public const int CodecStreamIndex = 1;
		public const string CodecStreamName = "CODEC Stream #1";
		public const long CodecStreamLength = 100;
		public const CodecID CodecStreamFormat = CodecID.Unknown;

		private InputFile _inputFile;
		private DataPacket _dataPacket;
		private DataPacket _extraDataPacket;
		private DataBlock _dataBlock;
		private IDetector _mockDetector;
		private ICodecStream _codecStream;


		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_inputFile = new InputFile("movie.mpg");
			_dataPacket = new DataPacket(_inputFile, 10, CodecStreamLength);
			_extraDataPacket = new DataPacket(_inputFile, 1821309, 192);
			_mockDetector = new MockDetector();
			_dataBlock = new DataBlock(new DataPacket(_inputFile, 0, CodecStreamLength + 20), true, _mockDetector);
			_dataBlock.CodecStreams.Add(new CodecStream("Stream #1", _dataBlock, _mockDetector, CodecID.Mpeg1Video, 123));
			_dataBlock.CodecStreams.Add(new CodecStream("Stream #2", _dataBlock, null, CodecID.Unknown, 456));
			_dataBlock.CodecStreams.Add(new CodecStream("Stream #3", _dataBlock, _mockDetector, CodecID.Mpeg1Video, 789));
			_dataBlock.CodecStreams.Add(new CodecStream("Stream #4", _dataBlock, null, CodecID.AmrSpeech, 1010));
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_inputFile = null;
			_dataPacket = null;
			_extraDataPacket = null;
			_dataBlock = null;
		}

		[SetUp]
		public void SetUp()
		{
			_codecStream = new ContentBase(CodecStreamName, CodecStreamFormat, _dataPacket);
		}

		[TearDown]
		public void TearDown()
		{
			_codecStream = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestConstructorNegativeIndex()
		{
			//new CodecStream(-1, CodecStreamName, CodecStreamFormat, _dataPacket);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorNameNull()
		{
			//new CodecStream(CodecStreamIndex, null, CodecStreamFormat, _dataPacket);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDataNull()
		{
			//new CodecStream(CodecStreamIndex, CodecStreamName, CodecStreamFormat, null);
		}

		[Test]
		public void TestName()
		{
			Assert.AreEqual(CodecStreamName, _codecStream.Name, "Name");
		}

		[Test]
		public void TestLength()
		{
			Assert.AreEqual(CodecStreamLength, _codecStream.Length, "Length");
		}

		[Test]
		public void TestDataFormat()
		{
			Assert.AreEqual(CodecStreamFormat, _codecStream.DataFormat, "DataFormat");
		}

		[Test]
		public void TestData()
		{
			Assert.AreEqual(_dataPacket, _codecStream.Data, "Data");
			Assert.AreNotEqual(_mockDetector, _codecStream.Detector, "Detector");
		}

		[Test]
		public void TestDetector()
		{
			Assert.AreNotEqual(_mockDetector, _codecStream.Detector, "Detector");
			Assert.IsInstanceOfType(typeof(UnknownFormatDetector), _codecStream.Detector, "Detector");
		}

		[Test]
		public void TestDataBlock()
		{
			Assert.IsNull(_codecStream.DataBlock, "DataBlock");
			Assert.AreEqual(_mockDetector, _codecStream.Detector, "Detector");
		}
	}
}
