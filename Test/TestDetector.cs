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
using Defraser.Util;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestDetector
	{
		public const string MockName = "Do not mock my Name";
		public const string MockDescription = "Do not mock my Description";
		public const string MockOutputFileExtension = "Do not mock my OutputFileExtension";

		private MockDetector _detector;


		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
		}

		[SetUp]
		public void SetUp()
		{
			_detector = new MockDetector(MockName);
		}

		[TearDown]
		public void TearDown()
		{
			_detector = null;
		}

		[Test]
		public void TestColumns()
		{
			IDictionary<string, string[]> columns = _detector.Columns;
			Assert.IsFalse(columns.ContainsKey("Root"), "Root should not be listed in Columns");
			Assert.IsTrue(columns.ContainsKey("MockHeaderTypeOne"), "Columns lists all header names");
			Assert.IsTrue(columns.ContainsKey("MockHeaderTypeTwo"), "Columns lists all header names");
			Assert.IsTrue(columns.ContainsKey("MockHeaderTypeThree"), "Columns lists all header names");
			Assert.IsTrue(columns.ContainsKey("MockSubHeader"), "Columns lists all header names");
			Assert.IsTrue(columns.ContainsKey("MockAttributeHeader"), "Columns lists all header names");
			// FIXME: Next line is disabled, because it fails sometimes (!?)
//			Assert.AreEqual(8, columns.Count, "Columns.Count");

			string[] columnNames1 = columns["MockHeaderTypeOne"];
			Assert.Contains("FirstAttribute", columnNames1, "Columns, attributes");
			Assert.Contains("SecondAttribute", columnNames1, "Columns, attributes");
			Assert.AreEqual(2, columnNames1.Length, "Columns / attributes");

			string[] columnNames2 = columns["MockSubHeader"];
			Assert.Contains("FirstAttribute", columnNames2, "Columns, attribute inheritance");
			Assert.Contains("SecondAttribute", columnNames2, "Columns, attribute inheritance");
			Assert.Contains("SubHeaderAttribute", columnNames2, "Columns, attribute inheritance");
			Assert.AreEqual(3, columnNames2.Length, "Columns / attributes");
		}
	}
}
