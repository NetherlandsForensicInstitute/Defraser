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
using Defraser.Detector.Common;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestAttributes
	{
		public enum MockAttributeNameEnum { MyEnumFirst, MyEnumSecond };
		public const string AttributeName = "MyAttribute";
		public const uint AttributeValue = 0x6D6F6F76;
		public const string AttributeValueAsString = "1836019574";
		public const string SubAttributeName1 = "SubAttribute1";
		public const string SubAttributeName2 = "SubAttribute2";

		public const string FormattedAttributeFormat = "{0:X8}";
		public const string FormattedAttributeValueAsString = "6D6F6F76";
		public string[] OptionAttributeOptions = { "Yes", "No", "Maybe" };
		public const int OptionAttributeValue = 2;
		public string OptionAttributeValueAsString = "Maybe (2)"; // Note: 2 is the index in OptionAttributeOptions

		public string[] FourCCAttributeDescriptions = { "option1 (opt1)", "option2 (opt2)", "option3 (opt3)" };
		public const uint FourCCAttributeValueInRange = (uint)Option.option3;
		public const uint FourCCAttributeValueOutsideRange = 0x6F707434; // 'opt4'
		public const string FourCCAttributeValueAsString = "option3 (opt3)";
		public const int opt4 = 0x6F707434;	// 'opt4'; value outside Option enum

		public Dictionary<uint, Option> _fourCCWithOptionalDescriptionAttributeDescriptions;

		public Dictionary<ushort, ShortOption> _shortOptionalDescriptionAttributeDescriptions;

		public enum Option
		{
			option1 = 0x6F707431,	// 'opt1'
			option2 = 0x6F707432,	// 'opt2'
			option3 = 0x6F707433	// 'opt3'
		}

		public enum ShortOption
		{
			option1 = 0x6F31,	// 'o1'
			option2 = 0x6F32,	// 'o2'
			option3 = 0x6F33,	// 'o3'
		}

		private FormattedAttribute<string, uint> _formattedAttribute;
		private OptionAttribute<string> _optionAttribute;
		private FourCCAttribute<string> _fourCcAttribute;
		private MockCompositeAttribute _compositeAttribute;
		private FourCCWithOptionalDescriptionAttribute<string, Option> _fourCCWithOptionalDescriptionAttributeInRange;
		private FourCCWithOptionalDescriptionAttribute<string, Option> _fourCCWithOptionalDescriptionAttributeOutsideRange;

		[SetUp]
		public void SetUp()
		{
			_formattedAttribute = new FormattedAttribute<string, uint>(AttributeName, AttributeValue, FormattedAttributeFormat);
			_optionAttribute = new OptionAttribute<string>(AttributeName, OptionAttributeValue, OptionAttributeOptions, false);
			_fourCcAttribute = new FourCCAttribute<string>(AttributeName, AttributeValue);
			_compositeAttribute = new MockCompositeAttribute(AttributeName, AttributeValue);

			_fourCCWithOptionalDescriptionAttributeDescriptions = new Dictionary<uint, Option>();
			foreach(Option option in Enum.GetValues(typeof(Option)))
			{
				_fourCCWithOptionalDescriptionAttributeDescriptions.Add((uint)option, option);
			}
			_fourCCWithOptionalDescriptionAttributeInRange = new FourCCWithOptionalDescriptionAttribute<string, Option>(AttributeName, FourCCAttributeValueInRange, _fourCCWithOptionalDescriptionAttributeDescriptions);
			_fourCCWithOptionalDescriptionAttributeOutsideRange = new FourCCWithOptionalDescriptionAttribute<string, Option>(AttributeName, FourCCAttributeValueOutsideRange, _fourCCWithOptionalDescriptionAttributeDescriptions);

			_shortOptionalDescriptionAttributeDescriptions = new Dictionary<ushort, ShortOption>();
			foreach(ShortOption shortOption in Enum.GetValues(typeof(ShortOption)))
			{
				_shortOptionalDescriptionAttributeDescriptions.Add((ushort)shortOption, shortOption);
			}
		}

		[TearDown]
		public void TearDown()
		{
			_formattedAttribute = null;
			_optionAttribute = null;
			_fourCcAttribute = null;
			_compositeAttribute = null;
			_fourCCWithOptionalDescriptionAttributeInRange = null;
			_fourCCWithOptionalDescriptionAttributeOutsideRange = null;
			_fourCCWithOptionalDescriptionAttributeDescriptions = null;
			_shortOptionalDescriptionAttributeDescriptions = null;
		}

		#region Attribute common tests
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestConstructorNameNull()
		{
			new FormattedAttribute<string, string>(null, string.Empty);
		}

		[Test]
		public void TestNoAttributes()
		{
			Assert.IsNull(_formattedAttribute.Attributes, "FormattedAttribute.Attributes");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestFindAttributeByNameNull()
		{
			_formattedAttribute.FindAttributeByName(null);
		}

		[Test]
		public void TestName()
		{
			FormattedAttribute<MockAttributeNameEnum, uint> attributeEnum = new FormattedAttribute<MockAttributeNameEnum, uint>(MockAttributeNameEnum.MyEnumFirst, AttributeValue);
			Assert.AreEqual("MyEnumFirst", attributeEnum.Name, "Attribute.Name (enum)");
			FormattedAttribute<string, uint> attributeString = new FormattedAttribute<string, uint>(AttributeName, AttributeValue);
			Assert.AreEqual(AttributeName, attributeString.Name, "Attribute.Name (string)");
		}

		[Test]
		public void TestValid()
		{
			Assert.IsTrue(_formattedAttribute.Valid, "Valid (get)");
			_formattedAttribute.Valid = false;
			Assert.IsFalse(_formattedAttribute.Valid, "Valid (set to false)");
			_formattedAttribute.Valid = true;
			Assert.IsTrue(_formattedAttribute.Valid, "Valid (set to true)");
		}

		[Test]
		public void TestTypedValue()
		{
			Assert.AreEqual(AttributeValue, _formattedAttribute.TypedValue, "TypedValue (get)");
			_formattedAttribute.TypedValue = AttributeValue - 1;
			Assert.AreEqual(AttributeValue - 1, _formattedAttribute.TypedValue, "TypedValue (set)");
		}

		[Test]
		public void TestFindAttributeByName()
		{
			Assert.IsNull(_formattedAttribute.FindAttributeByName("NoSubAttributes"), "Attribute has no sub-attributes");
		}
		#endregion Attribute common tests

		#region FormattedAttribute specific tests
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestFormattedAttributeValueNull()
		{
			new FormattedAttribute<string, string>(AttributeName, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestFormattedAttributeFormatNull()
		{
			new FormattedAttribute<string, uint>(AttributeName, AttributeValue, (string)null);
		}

		[Test]
		public void TestFormattedAttributeValueAsString()
		{
			Assert.AreEqual(FormattedAttributeValueAsString, _formattedAttribute.ValueAsString, "ValueAsString (hex format)");
			FormattedAttribute<string, uint> attribute = new FormattedAttribute<string, uint>(AttributeName, AttributeValue);
			Assert.AreEqual(AttributeValueAsString, attribute.ValueAsString, "ValueAsString (default format)");
		}
		#endregion FormattedAttribute specific tests

		#region OptionAttribute specific tests
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestOptionAttributeInvalidValue()
		{
			new OptionAttribute<string>(AttributeName, -1, OptionAttributeOptions, false);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestOptionAttributeOptionsNull()
		{
			new OptionAttribute<string>(AttributeName, OptionAttributeValue, null, false);
		}

		[Test]
		public void TestOptionAttributeOptions()
		{
			Assert.AreSame(OptionAttributeOptions, _optionAttribute.Options, "Options");
		}

		[Test]
		public void TestOptionAttributeValueAsString()
		{
			Assert.AreEqual(OptionAttributeValueAsString, _optionAttribute.ValueAsString, "ValueAsString (third option)");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestOptionAttributeValueOutsideOptionRangeFail()
		{
			OptionAttribute<string> optionAttribute = new OptionAttribute<string>(AttributeName, 4, OptionAttributeOptions, false);
		}

		[Test]
		public void TestOptionAttributeValueOutsideOptionRangeHappyFlow()
		{
			OptionAttribute<string> optionAttribute = new OptionAttribute<string>(AttributeName, 4, OptionAttributeOptions, true);
			Assert.That(optionAttribute.ValueAsString, Is.EqualTo("4"));
			optionAttribute = null;
		}
		#endregion OptionAttribute specific tests

		#region FourCCWithOptionalDescriptionAttribute specific tests
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestFourCCWithOptionalDescriptionAttributeConstructorOptionsNull()
		{
			new FourCCWithOptionalDescriptionAttribute<string, Option>(AttributeName, OptionAttributeValue, null);
		}

		[Test]
		public void TestFourCCWithOptionalDescriptionAttributeOptions()
		{
			Assert.AreSame(_fourCCWithOptionalDescriptionAttributeDescriptions, _fourCCWithOptionalDescriptionAttributeInRange.Options, "Options");
		}

		[Test]
		public void TestFourCCWithOptionalDescriptionAttributeValueAsStringValueInRange()
		{
			// Test method for value with description
			Assert.AreEqual("option3 (opt3)", _fourCCWithOptionalDescriptionAttributeInRange.ValueAsString);
		}

		[Test]
		public void TestFourCCWithOptionalDescriptionAttributeValueAsStringValueOutsideRange()
		{
			// Test method for value without description
			Assert.AreEqual("opt4", _fourCCWithOptionalDescriptionAttributeOutsideRange.ValueAsString);
		}
		#endregion FourCCWithOptionalDescriptionAttribute specific tests

		[Test]
		public void TestFourCCAttribute()
		{
			Assert.AreEqual(AttributeValue, _fourCcAttribute.Value, "FourCCAttribute, Values");
			Assert.AreEqual("moov", _fourCcAttribute.ValueAsString, "FourCCAttribute, ValuesAsString (4CC)");
			Assert.AreEqual(string.Empty, new FourCCAttribute<string>(AttributeName, 0).ValueAsString, _fourCcAttribute.ValueAsString, "FourCCAttribute, ValueAsString (0)");
		}

		[Test]
		public void TestCompositeAttribute()
		{
			Assert.IsNotNull(_compositeAttribute.Attributes, "CompositeAttribute.Attributes");
			_compositeAttribute.Attributes.Add(new FormattedAttribute<string, uint>(SubAttributeName1, 1));
			_compositeAttribute.Attributes.Add(new FormattedAttribute<string, uint>(SubAttributeName2, 2));
			Assert.IsNotNull(_compositeAttribute.FindAttributeByName(SubAttributeName2), "CompositeAttribute.FindAttributeByName");
			Assert.IsNull(_compositeAttribute.FindAttributeByName("NoSuchAttribute"), "CompositeAttribute.FindAttributeByName (non-existing)");
		}

		[Test]
		public void TestCompositeAttributeFormat()
		{
			CompositeAttribute<string, uint, object> compositeAttribute = new MockCompositeAttribute(AttributeName, AttributeValue, "{0:X8}");
			Assert.AreEqual("6D6F6F76", compositeAttribute.ValueAsString, "CompositeAttribute.ValueAsString, with hex formatting");
		}

		//[Test]
		//[ExpectedException(typeof(ArgumentException))]
		//public void TestInvalidateAttributeInvalid()
		//{
		//    _compositeAttribute.InvalidateAttribute((MockAttributeNameEnum)300);
		//}

		//[Test]
		//[ExpectedException(typeof(ArgumentException))]
		//public void TestInvalidateAttributeNotFound()
		//{
		//    _compositeAttribute.InvalidateAttribute(MockAttributeNameEnum.MyEnumSecond);
		//}

		//[Test]
		//public void TestInvalidateAttribute()
		//{
		//    _compositeAttribute.Attributes.Add(new FormattedAttribute<MockAttributeNameEnum, int>(MockAttributeNameEnum.MyEnumFirst, 3));
		//    Assert.IsTrue(_compositeAttribute.Attributes[0].Valid, "InvalidateAttribute(), before: attribute valid");
		//    _compositeAttribute.InvalidateAttribute(MockAttributeNameEnum.MyEnumFirst);
		//    Assert.IsFalse(_compositeAttribute.Attributes[0].Valid, "InvalidateAttribute(), after: attribute invalid");
		//}
	}
}
