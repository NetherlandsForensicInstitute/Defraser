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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Defraser.Framework;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Enumerates the default columns used in the file and header trees.
	/// </summary>
	public enum DefaultColumnIndex
	{
		/// <summary>The name of the data block or result.</summary>
		[DefaultColumn("Name", StringAlignment.Near, 300)]
		Name,
		/// <summary>The name of the detector used.</summary>
		[DefaultColumn("Detector", StringAlignment.Near, 100)]
		Detector,
		/// <summary>The version of the detector used.</summary>
		[DefaultColumn("Detector Version", StringAlignment.Far, 100)]
		DetectorVersion,
		/// <summary>The offset of the data block or result.</summary>
		[DefaultColumn("Offset ({0})", StringAlignment.Far, 80)]
		Offset,
		/// <summary>The length of the data block or result.</summary>
		[DefaultColumn("Length ({0})", StringAlignment.Far, 80)]
		Length,
		/// <summary>The end offset of the data block or result.</summary>
		[DefaultColumn("End Offset ({0})", StringAlignment.Far, 80)]
		EndOffset,
		/// <summary>The file that produced the result (header tree only).</summary>
		[DefaultColumn("File", StringAlignment.Near, 100)]
		File,
	}

	/// <summary>
	/// Provides extension and static methods for the <c>DefaultColumnIndex</c> enumeration.
	/// </summary>
	public static class DefaultColumnExtensions
	{
		private static readonly Dictionary<DefaultColumnIndex, Dictionary<DisplayMode, IColumnInfo>> ColumnInfos;

		private static readonly DefaultColumnIndex[] HeaderTreeDefaultColumns = 
		{
			DefaultColumnIndex.Name,
			DefaultColumnIndex.Detector,
			DefaultColumnIndex.DetectorVersion,
			DefaultColumnIndex.Offset,
			DefaultColumnIndex.Length,
			DefaultColumnIndex.EndOffset,
			DefaultColumnIndex.File
		};

		private static readonly DefaultColumnIndex[] FileTreeDefaultColumns = 
		{
			DefaultColumnIndex.Name,
			DefaultColumnIndex.Detector,
			DefaultColumnIndex.DetectorVersion,
			DefaultColumnIndex.Offset,
			DefaultColumnIndex.Length,
		};

		/// <summary>Static data initialization.</summary>
		static DefaultColumnExtensions()
		{
			ColumnInfos = new Dictionary<DefaultColumnIndex, Dictionary<DisplayMode, IColumnInfo>>();

			// Use reflection to find the attributes describing the default columns
			foreach (DefaultColumnIndex index in Enum.GetValues(typeof(DefaultColumnIndex)))
			{
				string name = Enum.GetName(typeof(DefaultColumnIndex), index);

				FieldInfo fieldInfo = typeof(DefaultColumnIndex).GetField(name);
				DefaultColumnAttribute[] attributes = (DefaultColumnAttribute[])fieldInfo.GetCustomAttributes(typeof(DefaultColumnAttribute), false);
				Debug.Assert((attributes != null && attributes.Length == 1), "DefaultColumnAttribute missing.");
				DefaultColumnAttribute attribute = attributes[0];

				ColumnInfos[index] = new Dictionary<DisplayMode, IColumnInfo>();

				foreach (DisplayMode displayMode in Enum.GetValues(typeof(DisplayMode)))
				{
					ColumnInfos[index][displayMode] = new ColumnInfo(string.Format(attribute.Caption, displayMode), name, attribute.HorizontalAlignment, attribute.Width);
				}
			}
		}

		/// <summary>
		/// Gets the <code>ColumnInfo</code> for the given <paramref name="index"/>.
		/// </summary>
		/// <param name="index">the index of the default column</param>
		/// <param name="displayMode">the display mode for the offset and length columns</param>
		/// <returns>the column info</returns>
		public static IColumnInfo GetColumnInfo(this DefaultColumnIndex index, DisplayMode displayMode)
		{
			Dictionary<DisplayMode, IColumnInfo> displayModeColumnInfos;
			if (!ColumnInfos.TryGetValue(index, out displayModeColumnInfos))
			{
				throw new ArgumentException("Invalid default column.", "index");
			}

			IColumnInfo columnInfo;
			if (!displayModeColumnInfos.TryGetValue(displayMode, out columnInfo))
			{
				throw new ArgumentException("Invalid display mode.", "index");
			}
			return columnInfo;
		}

		/// <summary>
		/// Gets the name of the default column for the given <paramref name="index"/>.
		/// </summary>
		/// <param name="index">the index of the default column</param>
		/// <returns>the name of the column</returns>
		public static string GetName(this DefaultColumnIndex index)
		{
			return index.GetColumnInfo(default(DisplayMode)).Name;
		}

		/// <summary>
		/// Gets the default columns for the <see cref="FileTreeObject"/>.
		/// </summary>
		/// <param name="displayMode">the display mode for the offset and length columns</param>
		/// <returns>the list of column infos</returns>
		public static IList<IColumnInfo> GetFileTreeDefaultColumns(DisplayMode displayMode)
		{
			List<IColumnInfo> visibleColumns = new List<IColumnInfo>();

			foreach (DefaultColumnIndex index in FileTreeDefaultColumns)
			{
				visibleColumns.Add(index.GetColumnInfo(displayMode));
			}
			return visibleColumns.AsReadOnly();
		}

		/// <summary>
		/// Gets the default columns for the <see cref="HeaderTree"/>.
		/// </summary>
		/// <param name="displayMode">the display mode for the offset and length columns</param>
		/// <returns>the list of column infos</returns>
		public static IList<IColumnInfo> GetHeaderTreeDefaultColumns(DisplayMode displayMode)
		{
			List<IColumnInfo> visibleColumns = new List<IColumnInfo>();

			foreach (DefaultColumnIndex index in HeaderTreeDefaultColumns)
			{
				visibleColumns.Add(index.GetColumnInfo(displayMode));
			}
			return visibleColumns.AsReadOnly();

		}

		/// <summary>
		/// Converts a column <paramref name="name"/> into a default column <paramref name="index"/>.s
		/// </summary>
		/// <param name="name">the name of the default column</param>
		/// <param name="index">receives the default column index</param>
		/// <returns>true if the name was successfully parsed, false on failure</returns>
		public static bool TryParse(string name, out DefaultColumnIndex index)
		{
			if (Enum.IsDefined(typeof(DefaultColumnIndex), name))
			{
				index = (DefaultColumnIndex)Enum.Parse(typeof(DefaultColumnIndex), name);
				return true;
			}
			index = default(DefaultColumnIndex);
			return false;
		}
	}

	/// <summary>
	/// Specifies the properties of a default column.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class DefaultColumnAttribute : Attribute
	{
		#region Properties
		/// <see cref="IColumnInfo.Caption"/>
		public string Caption { get; private set; }
		/// <see cref="IColumnInfo.HorizontalAlignment"/>
		public StringAlignment HorizontalAlignment { get; private set; }
		/// <see cref="IColumnInfo.Width"/>
		public int Width { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new default column attribute.
		/// </summary>
		/// <param name="caption">the column caption</param>
		/// <param name="horizontalAlignment">the caption and cell content alignment</param>
		/// <param name="width">the default column width</param>
		public DefaultColumnAttribute(string caption, StringAlignment horizontalAlignment, int width)
		{
			Caption = caption;
			HorizontalAlignment = horizontalAlignment;
			Width = width;
		}
	}
}
