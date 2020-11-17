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
using System.Globalization;
using System.Text;
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	public class ConfigurationItem : IConfigurationItem
	{
		private string _description;

		#region Properties
		/// <summary>
		/// The values hardcoded in the plug-in
		/// </summary>
		public object DefaultValue { get; private set; }
		/// <summary>
		/// The user value that overwrites the default value
		/// </summary>
		public object UserValue { get; private set; /* Use SetUserValue to set the user value */ }
		/// <summary>
		/// Return the User value when specified, else return the Default value.
		/// </summary>
		public object Value { get { return UserValue ?? DefaultValue; } }
		/// <summary>
		/// The description of the value
		/// </summary>
		public string Description
		{
			get
			{
				StringBuilder result = new StringBuilder();
				for (int charIndex = 0; charIndex < _description.Length; charIndex++)
				{
					char c = _description[charIndex];
					if (char.IsUpper(c) && charIndex != 0 && !char.IsWhiteSpace(_description[charIndex-1]))
					{
						result.Append(' ');
					}
					result.Append(c);
				}
				return result.ToString();
			}
			private set { _description = value; }
		}
		public bool HasUserValueSpecified
		{
			get
			{
				if (UserValue == null) return false;

				TypeCode typeCode = Type.GetTypeCode(DefaultValue.GetType());
				switch (typeCode)
				{
					case TypeCode.Boolean: return ((bool)DefaultValue != (bool)UserValue);
					case TypeCode.Int32: return ((Int32)DefaultValue != (Int32)UserValue);
					case TypeCode.UInt32: return ((UInt32)DefaultValue != (UInt32)UserValue);
					case TypeCode.Double: return ((Double)DefaultValue != (Double)UserValue);
					case TypeCode.Byte: return ((byte)DefaultValue != (byte)UserValue);
				}
				return true;
			}
		}
		#endregion Properties

		public ConfigurationItem(object defaultValue, string description)
		{
			if (defaultValue == null) throw new ArgumentNullException("defaultValue");

			TypeCode typeCode = Type.GetTypeCode(defaultValue.GetType());
			if( typeCode != TypeCode.Int32 &&
				typeCode != TypeCode.UInt32 &&
				typeCode != TypeCode.Boolean &&
				typeCode != TypeCode.Byte &&
				typeCode != TypeCode.Double)
			{
				string message = string.Format("value of type {0} not supported", typeCode.ToString());
				throw new ArgumentException(message,  "defaultValue");
			}

			if (string.IsNullOrEmpty(description)) throw new ArgumentNullException("defaultValue");

			DefaultValue = defaultValue;
			Description = description;
		}

		/// <summary>
		/// Revert to the default value
		/// </summary>
		public void ResetDefault()
		{
			UserValue = null;
		}

		/// <summary>
		/// Check if the user input is correct. Ie is of the same type as the 
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool IsValidUserInput(string userValue)
		{
			Type type = DefaultValue.GetType();

			switch (Type.GetTypeCode(DefaultValue.GetType()))
			{
				case TypeCode.UInt32:
					uint uint32Result;
					return uint.TryParse(userValue, out uint32Result);
				case TypeCode.Int32:
					int int32Result;
					return int.TryParse(userValue, out int32Result);
				case TypeCode.Boolean:
					bool boolResult;
					return bool.TryParse(userValue, out boolResult);
				case TypeCode.Double:
					double doubleResult;
					NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
					numberFormatInfo.NumberDecimalSeparator = ".";
					return double.TryParse(userValue, NumberStyles.Float, numberFormatInfo, out doubleResult);
				case TypeCode.Byte:
					byte byteResult;
					return byte.TryParse(userValue, out byteResult);
			}
			return false;
		}

		public void SetUserValue(string value)
		{
			if(string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

			if(!IsValidUserInput(value))
			{
				string message = string.Format("Is not valid for type {0}", Type.GetTypeCode(DefaultValue.GetType()).ToString());
				throw new ArgumentException(message, "value");
			}

			Type type = DefaultValue.GetType();

			switch (Type.GetTypeCode(DefaultValue.GetType()))
			{
				case TypeCode.UInt32: UserValue = uint.Parse(value); break;
				case TypeCode.Int32: UserValue = int.Parse(value); break;
				case TypeCode.Boolean: UserValue = bool.Parse(value); break;
				case TypeCode.Double:
					NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
					numberFormatInfo.NumberDecimalSeparator = ".";
					UserValue = double.Parse(value, NumberStyles.Float, numberFormatInfo);
					break;
				case TypeCode.Byte: UserValue = byte.Parse(value); break;
			}
		}
	}
}
