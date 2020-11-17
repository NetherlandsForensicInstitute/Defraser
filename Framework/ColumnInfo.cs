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
using System.Drawing;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	[DataContract]
	public class ColumnInfo : IColumnInfo, IEquatable<ColumnInfo>
	{
		/// <summary>Default column width.</summary>
		public const int ColumnWidth = 100;

		[DataMember]
		private readonly string _caption;
		[DataMember]
		private readonly string _name;
		[DataMember]
		private readonly int _width;
		[DataMember]
		private readonly StringAlignment _horizontalAlignment;

		#region Properties
		public string Caption { get { return _caption; } }
		public string Name { get { return _name; } }
		public StringAlignment HorizontalAlignment { get { return _horizontalAlignment; } }
		public int Width { get { return _width; } }
		#endregion Properties

		public ColumnInfo(string caption, string name, StringAlignment horizontalAlignment, int width)
		{
			PreConditions.Argument("caption").Value(caption).IsNotNull();
			PreConditions.Argument("name").Value(name).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("horizontalAlignment").Value(horizontalAlignment).IsDefinedOnEnum(typeof(StringAlignment));
			PreConditions.Argument("width").Value(width).IsNotNegative();

			_caption = caption;
			_name = name;
			_horizontalAlignment = horizontalAlignment;
			_width = width;
		}

		public IColumnInfo UpdateWidth(int width)
		{
			return new ColumnInfo(Caption, Name, HorizontalAlignment, width);
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			ColumnInfo otherColumnInfo = obj as ColumnInfo;
			if (otherColumnInfo == null) return false;
			return Equals(otherColumnInfo);
		}

		public bool Equals(ColumnInfo other)
		{
			if (other == this) return true;

			if (other.Caption != Caption) return false;
			if (other.Name != Name) return false;
			if (other.Width != Width) return false;
			if (other.HorizontalAlignment != HorizontalAlignment) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return Caption.GetHashCode()
				.CombineHashCode(Name)
				.CombineHashCode(Width)
				.CombineHashCode(HorizontalAlignment);
		}
		#endregion Equals method
	}
}
