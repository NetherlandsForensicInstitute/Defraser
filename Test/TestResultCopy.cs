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
using NUnit.Framework;
using Defraser;
using DetectorCommon;

namespace TestDefraser
{
	[TestFixture]
	public class TestResultCopy
	{
		private IResultNode _resultMock;
		private IResultNode _resultCopy;

		[SetUp]
		public void SetUp()
		{
			_resultMock = new MockResult(null);
			_resultCopy = new ResultCopy(_resultMock);
		}

		[SetUp]
		public void TearDown()
		{
			_mockResult = new MockResult(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorNull()
		{
			new ResultCopy(null);
		}

		[Test]
		public void TestName()
		{
			_resultMock.Name = "x";
			Assert.AreSame(_resultMock.Name, _resultCopy.Name, "Name");
			_resultMock.Name = "y";
			Assert.AreSame(_resultMock.Name, _resultCopy.Name, "Name");
		}

		[Test]
		public void TestAttributes()
		{
			Assert.AreSame(_resultMock.Attributes, _resultCopy.Attributes, "Attributes");
		}

		[Test]
		public void TestValid()
		{
			_resultMock.Valid = true;
			Assert.AreEqual(_resultMock.Valid, _resultCopy.Valid, "Valid");
			_resultMock.Valid = false;
			Assert.AreEqual(_resultMock.Valid, _resultCopy.Valid, "Valid");
		}

		[Test]
		public void TestDetector()
		{
			_resultMock.Detector = x;
			Assert.AreSame(_resultMock.Detector, _resultCopy.Detector, "Detector");
			_resultMock.Detector = y;
			Assert.AreSame(_resultMock.Detector, _resultCopy.Detector, "Detector");
		}

		[Test]
		public void TestDataPacket()
		{
			_resultMock.DataPacket = x;
			Assert.AreSame(_resultMock.DataPacket, _resultCopy.DataPacket, "DataPacket");
			_resultMock.DataPacket = y;
			Assert.AreSame(_resultMock.DataPacket, _resultCopy.DataPacket, "DataPacket");
		}

		[Test]
		public void TestLength()
		{
			_resultMock.Length = x;
			Assert.AreSame(_resultMock.Length, _resultCopy.Length, "Length");
			_resultMock.Length = y;
			Assert.AreSame(_resultMock.Length, _resultCopy.Length, "Length");
		}

		[Test]
		public void TestParent()
		{
			_resultMock.Parent = x;
			Assert.AreSame(_resultMock.Parent, _resultCopy.Parent, "Parent");
			_resultMock.Parent = y;
			Assert.AreSame(_resultMock.Parent, _resultCopy.Parent, "Parent");
		}

		[Test]
		public void TestChildren()
		{
			Assert.AreNotSame(_resultMock.Children, _resultCopy.Children, "Children");
		}

		[Test]
		public void TestFindAttributeByName()
		{
		}

		[Test]
		public void TestAddChild()
		{
		}

		[Test]
		public void TestInsertChild()
		{
		}

		[Test]
		public void TestRemoveChild()
		{
		}

		[Test]
		public void TestShallowCopy()
		{
		}

		[Test]
		public void TestDeepCopy()
		{
		}
	}
}
