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
using System.Linq;
using Defraser.DataStructures;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestParser
	{
		/// <summary>The <c>Name</c> of the test file.</summary>
		private const string TestFileName = "test.txt";
		/// <summary>The <c>Length</c> of the test file (for stub behavior).</summary>
		private const long TestFileLength = 4096L;

		// Dummy data for the MockDataReader
		private static readonly byte[] _mockData =
		{
			0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
			0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
			0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
			0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
			0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
			0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67,
			0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77,
			0xF0, 0x0F
		};

		private IDetector _detector;
		private IInputFile _inputFile;
		private IDataReader _dataReader;
		private MockParser _parser;
		private MockHeader _header;
		private MockHeader _childHeader;
		private MockHeader _grandChildHeader;
		private MockHeader _mockAttributeHeader;


		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_detector = new MockDetectorDataStructures();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_detector = null;
		}

		[SetUp]
		public void SetUp()
		{
			_inputFile = MockRepository.GenerateStub<IInputFile>();
			_inputFile.Stub(x => x.Name).Return(TestFileName);
			_inputFile.Stub(x => x.Length).Return(TestFileLength);

			_dataReader = new MockDataReader(_mockData, _inputFile);
			_parser = new MockParser(_dataReader);
			_header = new MockHeader(Enumerable.Repeat(_detector, 1), MockHeaderName.Root);
			_childHeader = new MockHeader(_header, MockHeaderName.MockHeaderTypeTwo);
			_grandChildHeader = new MockSubHeader(_childHeader);
			_mockAttributeHeader = new MockAttributeHeader(_grandChildHeader);
		}

		[TearDown]
		public void TearDown()
		{
			_inputFile = null;
			_dataReader = null;
			_header = null;
			_childHeader = null;
			_grandChildHeader = null;
			_mockAttributeHeader = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorNull()
		{
			new MockParser(null);
		}

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void TestParseDisposed()
		{
			_parser.Dispose();
			_parser.ParseAndAppend(_grandChildHeader);
		}

		[Test]
		public void TestParseFailHeaderParse()
		{
			_dataReader.Position = 0;
			Assert.IsFalse(_parser.ParseAndAppend(_grandChildHeader), "Process(), fail on Parse()");
		}

		[Test]
		public void TestParseFailHeaderParseEnd()
		{
			_dataReader.Position = _dataReader.Length - 1;
			Assert.IsFalse(_parser.ParseAndAppend(_grandChildHeader), "Process(), fail on ParseEnd()");
			_dataReader.Position = _dataReader.Length;
			Assert.IsFalse(_parser.ParseAndAppend(_header), "Process(), fail on ParseEnd()");
		}

		[Test]
		public void TestParseFailHeaderIsBackToBack()
		{
			_dataReader.Position = 0;
			_childHeader.Parse(_parser);
			_dataReader.Position++;
			_childHeader.ParseEnd(_parser);
			_dataReader.Position++;
			Assert.IsFalse(_parser.ParseAndAppend(_grandChildHeader), "Process(), fail on IsBackToBack()");
		}

		[Test]
		public void TestParseFailHeaderFindSuitableParent()
		{
			_grandChildHeader.Parent = _header;
			_dataReader.Position = 1;
			_header.Parse(_parser);
			_dataReader.Position++;
			_header.ParseEnd(_parser);
			Assert.IsFalse(_parser.ParseAndAppend(_grandChildHeader), "Process(), fail on FindSuitableParent()");
		}

		[Test]
		public void TestParseSuccess()
		{
			_dataReader.Position = 1;
			_childHeader.Parse(_parser);
			_dataReader.Position++;
			_childHeader.ParseEnd(_parser);
			Assert.IsTrue(_parser.ParseAndAppend(_grandChildHeader), "Process(), success");
		}

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void TestParseCompositeAttributeDisposed()
		{
			_parser.Dispose();
			_parser.Parse<string, int>(new MockAttribute<string>("Mock"));
		}

		[Test]
		public void TestParseCompositeAttribute()
		{
			_dataReader.Position = 1;
			_parser.ParseAndAppend(_mockAttributeHeader);
			Assert.AreEqual(1, _mockAttributeHeader.Attributes.Count, "Parse(), valid composite attribute");
		}

		[Test]
		public void TestParseCompositeAttributeInvalid()
		{
			_dataReader.Position = 0;
			_parser.ParseAndAppend(_mockAttributeHeader);
			Assert.AreEqual(0, _mockAttributeHeader.Attributes.Count, "Parse(), invalid composite attribute");
		}

		[Test]
		public void TestAddAttribute()
		{
			_dataReader.Position = 1;
			_parser.ParseAndAppend(_grandChildHeader);
			Assert.AreEqual(1, _grandChildHeader.Attributes.Count, "AddAttribute()");
		}

		[Test]
		public void TestAddAttributeInvalid()
		{
			_dataReader.Position = _dataReader.Length - 1;
			_parser.ParseAndAppend(_grandChildHeader);
			Assert.AreEqual(1, _grandChildHeader.Attributes.Count, "AddAttribute(), invalid attribute");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestParseRootNull()
		{
			_parser.ParseRoot(null, _dataReader.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestParseRootNonRoot()
		{
			_parser.ParseRoot(_childHeader, _dataReader.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestParseRootNegativeOffsetLimit()
		{
			_parser.ParseRoot(_header, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestParseRootInvalidOffsetLimit()
		{
			_parser.ParseRoot(_header, 1000000);
		}

		[Test]
		public void TestParseRootEndOfInput()
		{
			_dataReader.Position = _dataReader.Length;
			Assert.IsFalse(_parser.ParseRoot(_header, _dataReader.Length), "ParseRoot(), end-of-input");
		}

		[Test]
		public void TestParseRoot()
		{
			Assert.IsTrue(_parser.ParseRoot(_header, _mockData.Length), "ParseRoot()");
			Assert.AreEqual(2, _header.Children.Count, "ParserRoot(), children (count)");
			Assert.AreEqual(1, _header.Children[0].Attributes.Count, "ParserRoot(), child-0 Attributes");
			Assert.AreEqual(_mockData.Length, _header.Children[0].Attributes[0].Value, "ParserRoot(), offset limit");
			Assert.AreEqual(0, _header.Children[0].Children.Count, "ParserRoot(), child-0 Children");
			Assert.AreEqual(0, _header.Children[0].StartOffset, "ParserRoot(), child-0 Offset");
			Assert.AreEqual(4, _header.Children[0].Length, "ParserRoot(), child-0 Length");
			Assert.That(_header.Children[0].IsFragmented(), Is.False, "ParserRoot(), child-0 (unfragmented data)");
			Assert.AreEqual(0, _header.Children[1].Attributes.Count, "ParserRoot(), child-1 Attributes");
			Assert.AreEqual(0, _header.Children[1].Children.Count, "ParserRoot(), child-1 Children");
			Assert.AreEqual(4, _header.Children[1].StartOffset, "ParserRoot(), child-1 Offset");
			Assert.AreEqual(4, _header.Children[1].Length, "ParserRoot(), child-1 Length");
			Assert.That(_header.Children[1].IsFragmented(), Is.False, "ParserRoot(), child-1 (unfragmented data)");
		}

		[Test]
		public void TestParseRootNoFirstHeader()
		{
			MockParserNoFirstHeader parser = new MockParserNoFirstHeader(_dataReader);
			Assert.IsFalse(parser.ParseRoot(_header, _dataReader.Length - 3), "ParseRoot(), no first header");
			Assert.AreEqual(_dataReader.Length - 3, _dataReader.Position, "ParseRoot(), no first header, end position");
		}

		[Test]
		public void TestParseRootRewind()
		{
			MockParserRewind parser = new MockParserRewind(_dataReader);
			MockBrokenHeader header = new MockBrokenHeader(Enumerable.Repeat(_detector, 1));
			Assert.IsFalse(parser.ParseRoot(header, _dataReader.Length), "ParseRoot(), no first header");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestCheckAttributeNonExisting()
		{
			MockHeader header = new MockHeaderCheckAttributeNonExisting(_header);
			_parser.ParseAndAppend(header);
		}

		//[Test]
		//public void TestCheckAttribute()
		//{
		//    _dataReader.Position = 1;
		//    _parser.Parse(_childHeader);
		//    Assert.AreEqual(1, _childHeader.Attributes.Count, "CheckAttribute, attribute count");
		//    Assert.IsTrue(_childHeader.CheckAttribute(MockSubHeader.Attribute.SubHeaderAttribute, true, false), "CheckAttribute() returns condition (true)");
		//    Assert.IsTrue(_childHeader.Attributes[0].Valid, "CheckAttribute, attribute not invalidated");
		//    Assert.IsFalse(_childHeader.CheckAttribute(MockSubHeader.Attribute.SubHeaderAttribute, false, false), "CheckAttribute() returns condition (false)"); ;
		//    Assert.IsFalse(_childHeader.Attributes[0].Valid, "CheckAttribute, attribute invalidated");
		//    Assert.IsTrue(_childHeader.Valid, "CheckAttribute, header not invalidated");
		//}

		[Test]
		public void TestCheckAttributeInvalidateResult()
		{
			MockHeader header = new MockHeaderCheckAttributeInvalidateResult(_header);
			_dataReader.Position = 1;
			_parser.ParseAndAppend(header);
			Assert.AreEqual(1, header.Attributes.Count, "InvalidateAttribute, attribute count");
			Assert.IsFalse(header.Attributes[0].Valid, "InvalidateAttribute, attribute invalidated");
			Assert.IsFalse(header.Valid, "InvalidateAttribute, header invalidated");
		}
	}
}
