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
using Defraser.Detector.Common;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestConfigurationItem
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullForFirstArgument()
		{
			new ConfigurationItem(null, "<description>");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullForSecondArgument()
		{
			new ConfigurationItem(123, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorWithEmptyStringForSecondArgument()
		{
			new ConfigurationItem(123, string.Empty);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstructorWithUnsupportedTypeForTheFirstArgument()
		{
			new ConfigurationItem(123.0F, "<description>");
		}

		[Test]
		public void TestDescription()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(0, "<description>");
			Assert.That(configurationItem.Description, Is.EqualTo("<description>"));

			configurationItem = new ConfigurationItem(0, "Test123");
			Assert.That(configurationItem.Description, Is.EqualTo("Test123"));

			configurationItem = new ConfigurationItem(0, "TestOneTwoThree");
			Assert.That(configurationItem.Description, Is.EqualTo("Test One Two Three"));

			configurationItem = new ConfigurationItem(0, " Test One Two Three ");
			Assert.That(configurationItem.Description, Is.EqualTo(" Test One Two Three "));
		}

		[Test]
		public void TestValue()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(123, "<description>");
			Assert.That(configurationItem.DefaultValue, Is.EqualTo(123));
			Assert.That(configurationItem.UserValue, Is.EqualTo(null));
			Assert.That(configurationItem.Value, Is.EqualTo(123));

			configurationItem.SetUserValue("456");

			Assert.That(configurationItem.DefaultValue, Is.EqualTo(123));
			Assert.That(configurationItem.UserValue, Is.EqualTo(456));
			Assert.That(configurationItem.Value, Is.EqualTo(456));

			configurationItem.ResetDefault();
			Assert.That(configurationItem.DefaultValue, Is.EqualTo(123));
			Assert.That(configurationItem.UserValue, Is.EqualTo(null));
			Assert.That(configurationItem.Value, Is.EqualTo(123));
		}

		[Test]
		public void TestIsValidUserInput()
		{
			ConfigurationItem configurationItem = new ConfigurationItem((int)123, "<description>");
			Assert.That(configurationItem.IsValidUserInput("456"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("\t 456 "), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("1.3"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("1,3"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("1e3"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("abc"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("-1"), Is.EqualTo(true));

			configurationItem = new ConfigurationItem((double)123.456, "<description>");
			Assert.That(configurationItem.IsValidUserInput("456"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("\t 456 "), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput(" 1.3 "), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("1e3"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput(" -1.0 "), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("1,3"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("abc"), Is.EqualTo(false));

			configurationItem = new ConfigurationItem((bool)true, "<description>");
			Assert.That(configurationItem.IsValidUserInput("true"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("TRUE"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("false"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("FaLsE"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("1"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("0"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("waar"), Is.EqualTo(false));

			configurationItem = new ConfigurationItem((byte)0, "<description>");
			Assert.That(configurationItem.IsValidUserInput("255"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("true"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("256"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("-1"), Is.EqualTo(false));

			configurationItem = new ConfigurationItem((uint)0, "<description>");
			Assert.That(configurationItem.IsValidUserInput("256"), Is.EqualTo(true));
			Assert.That(configurationItem.IsValidUserInput("true"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("-1"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("0.1"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("0,1"), Is.EqualTo(false));
			Assert.That(configurationItem.IsValidUserInput("0e1"), Is.EqualTo(false));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSetUserValueWithNull()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(123, "<description>");
			configurationItem.SetUserValue(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSetUserValueWithEmptyString()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(123, "<description>");
			configurationItem.SetUserValue(string.Empty);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestSetUserValueWithInvalidType()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(123, "<description>");
			configurationItem.SetUserValue("1.3");
		}

		[Test]
		public void TestSetUserValueWithValidType()
		{
			ConfigurationItem configurationItem = new ConfigurationItem(123, "<description>");
			configurationItem.SetUserValue("456");
			Assert.That(configurationItem.UserValue, Is.EqualTo(456));
		}

		[Test]
		public void TestHasUserValueSpecified()
		{
			ConfigurationItem configurationItem = new ConfigurationItem((byte)123, "<description>");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("123");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("124");
			Assert.That(configurationItem.HasUserValueSpecified == true);
			configurationItem.ResetDefault();
			Assert.That(configurationItem.HasUserValueSpecified == false);

			configurationItem = new ConfigurationItem((int)123, "<description>");
			configurationItem.SetUserValue("123");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("124");
			Assert.That(configurationItem.HasUserValueSpecified == true);

			configurationItem = new ConfigurationItem((uint)123, "<description>");
			configurationItem.SetUserValue("123");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("124");
			Assert.That(configurationItem.HasUserValueSpecified == true);

			configurationItem = new ConfigurationItem((double)123.0, "<description>");
			configurationItem.SetUserValue("123.0");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("124.0");
			Assert.That(configurationItem.HasUserValueSpecified == true);

			configurationItem = new ConfigurationItem((bool)true, "<description>");
			configurationItem.SetUserValue("true");
			Assert.That(configurationItem.HasUserValueSpecified == false);
			configurationItem.SetUserValue("false");
			Assert.That(configurationItem.HasUserValueSpecified == true);
		}
	}
}
