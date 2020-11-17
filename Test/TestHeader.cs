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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestHeader
	{
		/// <summary>The <c>Name</c> of the test file.</summary>
		private const string TestFileName = "test.txt";
		/// <summary>The <c>Length</c> of the test file (for stub behavior).</summary>
		private const long TestFileLength = 4096L;

		public const string AttributeName = "BlockSize";

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

		private IInputFile _inputFile;
		private IDetector _detector;
		private IDataReader _dataReader;
		private MockParser _parser;
		private MockHeader _root;
		private MockHeader _header;
		private MockHeader _childHeader;
		private MockHeader _lastHeader;


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
			_root = new MockHeader(Enumerable.Repeat(_detector, 1), MockHeaderName.Root);
			_header = new MockHeader(_root, MockHeaderName.MockHeaderTypeTwo);
			_childHeader = new MockSubHeader(_header);
			_lastHeader = new MockHeaderTypeThree(_childHeader);
		}

		[TearDown]
		public void TearDown()
		{
			_inputFile = null;
			_dataReader = null;
			_root = null;
			_header = null;
			_childHeader = null;
			_lastHeader = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorDetectorNull()
		{
			new MockHeader((IEnumerable<IDetector>)null, MockHeaderName.Root);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorPreviousHeaderNull()
		{
			new MockHeader((MockHeader)null, MockHeaderName.MockHeaderTypeOne);
		}

		[Test]
		public void TestName()
		{
			Assert.AreEqual("Root", _root.Name, "Name");
			Assert.AreEqual("MockHeaderTypeTwo", _header.Name, "Name");
		}

		[Test]
		public void TestAttributes()
		{
			Assert.IsNull(_root.Attributes,"Attributes (null for root)");
			Assert.IsNotNull(_header.Attributes, "Attributes (not null)");
			Assert.AreEqual(0, _header.Attributes.Count, "Attributes (empty)");
			Assert.IsFalse(_header.Attributes.IsReadOnly, "Attributes (not read-only)");
		}

		[Test]
		public void TestDetector()
		{
			Assert.AreSame(_detector, _root.Detectors.First(), "Detector");
		}

		[Test]
		public void TestDataPacket()
		{
			Assert.IsNull(_root.InputFile, "DataPacket");
			_dataReader.Position = 11;
			_root.Parse(_parser);
			_dataReader.Position = 28;
			_root.ParseEnd(_parser);
			Assert.IsNotNull(_root.InputFile, "DataPacket.InputFile (not null)");
			Assert.AreEqual(11, _root.StartOffset, "DataPacket.Offset");
			Assert.AreEqual(17, _root.Length, "DataPacket.Length");
		}

		[Test]
		public void TestLength()
		{
			Assert.AreEqual(0, _header.Length, "Length");
			Assert.IsTrue(_parser.ParseAndAppend(_header));
			Assert.AreEqual(4, _header.Length, "Length");
		}

		[Test]
		public void TestChildren()
		{
			Assert.IsNotNull(_root.Children, "Children (not null)");
			Assert.AreEqual(0, _root.Children.Count, "Children (empty)");
			Assert.IsFalse(_root.Children.IsReadOnly, "Children (not read-only)");
		}

		[Test]
		public void TestValid()
		{
			Assert.IsTrue(_root.Valid, "Valid (true)");
			_root.Valid = false;
			Assert.IsFalse(_root.Valid, "Valid (false)");
			_root.Valid = true;
			Assert.IsTrue(_root.Valid, "Valid (true)");
		}

		[Test]
		public void TestParent()
		{
			Assert.AreSame(_root, _header.Parent, "DataBlock");
			_header.Parent = null;
			Assert.IsNull(_header.Parent, "DataBlock (null)");
			Assert.IsNull(_root.Parent, "DataBlock (null)");
		}

		[Test]
		public void TestHeaderName()
		{
			Assert.AreEqual(MockHeaderName.Root, _root.HeaderName, "HeaderName");
			Assert.AreEqual(MockHeaderName.MockHeaderTypeTwo, _header.HeaderName, "HeaderName");
		}

		[Test]
		public void TestOffset()
		{
			Assert.AreEqual(0, _root.Offset, "Offset");
			_dataReader.Position = 33;
			_root.Parse(_parser);
			Assert.AreEqual(33, _root.Offset, "Offset");
		}

		[Test]
		public void TestFirstChild()
		{
			Assert.IsNull(_root.FirstChild, "FirstChild (null)");
			_root.Children.Add(_header);
			Assert.AreSame(_header, _root.FirstChild, "FirstChild (1 child)");
			_root.Children.Add(_childHeader);
			Assert.AreSame(_header, _root.FirstChild, "FirstChild (2 children)");
		}

		[Test]
		public void TestLastChild()
		{
			Assert.IsNull(_root.LastChild, "LastChild (null)");
			_root.Children.Add(_header);
			Assert.AreSame(_header, _root.LastChild, "LastChild (1 child)");
			_root.Children.Add(_childHeader);
			Assert.AreSame(_childHeader, _root.LastChild, "LastChild (2 children)");
		}

		[Test]
		public void TestLastHeader()
		{
			Assert.AreSame(_root, _root.LastHeader, "LastHeader (no children)");
			_root.Children.Add(_header);
			Assert.AreSame(_header, _root.LastHeader, "LastHeader (1 child)");
			_root.Children.Add(_childHeader);
			Assert.AreSame(_childHeader, _root.LastHeader, "LastHeader (2 children)");
			_childHeader.Children.Add(_lastHeader);
			Assert.AreSame(_lastHeader, _root.LastHeader, "LastHeader (2 children, 1 grandchild)");
		}

		[Test]
		public void TestRoot()
		{
			Assert.AreSame(_root, _root.Root, "Root (this)");
			_root.AddChild(_header);
			Assert.AreSame(_root, _header.Root, "Root (ancestor)");
		}

		[Test]
		public void TestIsRoot()
		{
			Assert.IsTrue(_root.IsRoot, "IsRoot (yes)");
			_root.AddChild(_header);
			Assert.IsFalse(_header.IsRoot, "IsRoot (no)");
		}

		// TODO: this is actually an extension method of IResultNode
		[Test]
		public void TestHasChildren()
		{
			Assert.IsFalse(_root.HasChildren(), "HasChildren (no)");
			_root.Children.Add(_header);
			Assert.IsTrue(_root.HasChildren(), "HasChildren (yes)");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestFindAttributeByNameNull()
		{
			_root.FindAttributeByName(null);
		}

		[Test]
		public void TestFindAttributeByName()
		{
			Assert.IsNull(_root.FindAttributeByName(AttributeName), "FindAttributeByName (no attribute)");
			Assert.IsNull(_header.FindAttributeByName(AttributeName), "FindAttributeByName (no attribute)");
			IResultAttribute attribute = new FormattedAttribute<string, int>(AttributeName, 512);
			_header.Attributes.Add(attribute);
			Assert.AreSame(attribute, _header.FindAttributeByName(AttributeName), "FindAttributeByName");
			Assert.IsNull(_header.FindAttributeByName("MyDummyName"), "FindAttributeByName (not listed)");
		}

		[Test]
		public void TestParse()
		{
			Assert.IsTrue(_root.Parse(_parser), "Parse(), success");
			_dataReader.Position = 0;
			Assert.IsFalse(_lastHeader.Parse(_parser), "Parse(), overridden, fail");
			_dataReader.Position = 3;
			Assert.IsTrue(_lastHeader.Parse(_parser), "Parse(), overridden, success");
		}

		[Test]
		public void TestParseEnd()
		{
			_root.Parse(_parser);
			_dataReader.Position -= 4;
			Assert.IsFalse(_root.ParseEnd(_parser), "ParseEnd(), zero length, fail");
			_root.Parse(_parser);
			_dataReader.Position += 4;
			Assert.IsTrue(_root.ParseEnd(_parser), "ParseEnd(), success");

			_dataReader.Position = 0;
			_childHeader.Parse(_parser);
			_dataReader.Position = _dataReader.Length;
			Assert.IsFalse(_childHeader.ParseEnd(_parser), "ParseEnd(), overridden, fail");
			_dataReader.Position = 0;
			_childHeader.Parse(_parser);
			_dataReader.Position = 7;
			Assert.IsTrue(_childHeader.ParseEnd(_parser), "ParseEnd(), overridden, success");
		}

		[Test]
		public void TestIsBackToBack()
		{
			_parser.ParseAndAppend(_header);
			_parser.ParseAndAppend(_childHeader);
			Assert.IsTrue(_header.IsBackToBack(_childHeader), "IsBackToBack(), yes");

			_parser.ParseAndAppend(_lastHeader);
			Assert.IsFalse(_childHeader.IsBackToBack(_lastHeader), "IsBackToBack(), overridden, no");
		}

		[Test]
		public void TestIsSuitableParent()
		{
			Assert.IsTrue(_header.IsSuitableParent(_root), "IsSuitableParent(), yes");
			Assert.IsFalse(_header.IsSuitableParent(_lastHeader), "IsSuitableParent(), no");
			Assert.IsTrue(_childHeader.IsSuitableParent(_header), "IsSuitableParent(), overridden, yes");
			Assert.IsFalse(_childHeader.IsSuitableParent(_root), "IsSuitableParent(), overridden, no");
		}

		[Test]
		public void TestFindSuitableParent()
		{
			Assert.AreSame(_root, _lastHeader.FindSuitableParent(), "FindSuitableParent()");
			Assert.AreSame(_header, _childHeader.FindSuitableParent(), "FindSuitableParent()");
			Assert.AreSame(_root, _header.FindSuitableParent(), "FindSuitableParent()");
			Assert.IsNull(_root.FindSuitableParent(), "FindSuitableParent()");

			_childHeader.Parent = _root;
			Assert.IsNull(_childHeader.FindSuitableParent(), "FindSuitableParent()");
		}

		// TODO: this is actually an extension method of IResultNode
		[Test]
		public void TestAddChild()
		{
			_header.Parent = null;
			Assert.AreEqual(0, _root.Children.Count, "AddChild (Children empty, before)");
			Assert.IsNull(_header.Parent, "AddChild (DataBlock null, before)");
			_root.AddChild(_header);
			Assert.AreEqual(1, _root.Children.Count, "AddChild (Children, after)");
			Assert.AreSame(_header, _root.FirstChild, "AddChild (FirstChild, after)");
			Assert.AreSame(_root, _header.Parent, "AddChild (DataBlock after)");
		}

		[Test]
		public void TestFindParent()
		{
			_header.Parent = null;
			_childHeader.Parent = null;
			Assert.IsNull(_root.FindParent(MockHeaderName.Root), "FindParent (root, no parent)");
			Assert.IsNull(_root.FindParent(MockHeaderName.MockHeaderTypeTwo), "FindParent (other, no parent)");
			_header.AddChild(_childHeader);
			Assert.AreSame(_header, _childHeader.FindParent(MockHeaderName.MockHeaderTypeTwo), "FindParent (parent, no root)");
			Assert.IsNull(_childHeader.FindParent(MockHeaderName.MockHeaderTypeThree), "FindParent (other, no root)");
			_root.AddChild(_header);
			Assert.AreSame(_root, _childHeader.FindParent(MockHeaderName.Root), "FindParent (root, as grand parent)");
			Assert.AreSame(_root, _header.FindParent(MockHeaderName.Root), "FindParent (root, as parent)");
			Assert.AreSame(_header, _childHeader.FindParent(MockHeaderName.MockHeaderTypeTwo), "FindParent (parent)");
			Assert.IsNull(_childHeader.FindParent(MockHeaderName.MockHeaderTypeThree), "FindParent (other)");
		}

		[Test]
		public void TestFindChild()
		{
			_root.AddChild(_header);
			_header.AddChild(_childHeader);
			_childHeader.AddChild(_lastHeader);
			Assert.AreSame(_header, _root.FindChild(MockHeaderName.MockHeaderTypeTwo), "FindChild (non-recursive)");
			Assert.IsNull(_root.FindChild(MockHeaderName.MockSubHeader), "FindChild (non-recursive, fail)");
			Assert.IsNull(_root.FindChild(MockHeaderName.MockSubHeader, false), "FindChild (non-recursive, fail)");
			Assert.AreSame(_childHeader, _root.FindChild(MockHeaderName.MockSubHeader, true), "FindChild (recursive)");
			Assert.IsNull(_root.FindChild(MockHeaderName.MockHeaderTypeOne, true), "FindChild (recursive, fail)");
		}
	}
}
