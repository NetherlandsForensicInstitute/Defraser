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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Defraser.DataStructures;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Removed the '&' and trailing '...' characters from the string
		/// </summary>
		/// <param name="value">the string the remove the characters from</param>
		/// <returns>the result</returns>
		public static string StripMenuCharacters(this string value)
		{
			StringBuilder result = new StringBuilder(value.Length);

			// copy the string while leaving the '&' character out
			foreach (char c in value)
			{
				if (c != '&')
				{
					result.Append(c);
				}
			}

			// remove optional '...'
			if (result.Length > 3 && result.ToString(result.Length - 3, 3) == "...")
			{
				result.Remove(result.Length - 3, 3);
			}
			return result.ToString();
		}

		/// <summary>
		/// Sorts the <paramref name="dataBlocks"/> on a user selected column.
		/// </summary>
		/// <param name="dataBlocks">the data blocks to sort</param>
		/// <param name="columnName">the name of the column to compare</param>
		/// <param name="sortDirection">the sort direction</param>
		/// <returns>the sorted data blocks</returns>
		public static IEnumerable<IDataBlock> Sort(this IEnumerable<IDataBlock> dataBlocks, string columnName, ListSortDirection sortDirection)
		{
			PreConditions.Argument("dataBlocks").Value(dataBlocks).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull();
			DefaultColumnIndex index;
			if (DefaultColumnExtensions.TryParse(columnName, out index))
			{
				switch (index)
				{
					case DefaultColumnIndex.Name:
						return dataBlocks;
					case DefaultColumnIndex.Detector:
						return dataBlocks.OrderBy(d => d.Detectors.First().Name, sortDirection);
					case DefaultColumnIndex.DetectorVersion:
						return dataBlocks.OrderBy(d => d.Detectors.First().VersionString(), sortDirection);
					case DefaultColumnIndex.Offset:
						return dataBlocks.OrderBy(d => d.StartOffset, sortDirection);
					case DefaultColumnIndex.Length:
						return dataBlocks.OrderBy(d => d.Length, sortDirection);
					case DefaultColumnIndex.EndOffset:
						return dataBlocks.OrderBy(d => d.EndOffset, sortDirection);
				}
			}
			throw new ArgumentException("Invalid sort column name.", "columnName");
		}

		/// <summary>
		/// Sorts the <paramref name="detectors"/> on a user selected column.
		/// </summary>
		/// <param name="detectors">the detectors to sort</param>
		/// <param name="columnName">the name of the column to compare</param>
		/// <param name="sortDirection">the sort direction</param>
		/// <returns>the sorted data blocks</returns>
		public static IEnumerable<IDetector> Sort(this IEnumerable<IDetector> detectors, string columnName, ListSortDirection sortDirection)
		{
			PreConditions.Argument("detectors").Value(detectors).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull();

			switch (columnName)
			{
				case "columnName":
					return detectors.OrderBy(d => d.Name, sortDirection);
				case "columnVersion":
					return detectors.OrderBy(d => d.VersionString(), sortDirection);
				case "columnDefaultHeaders":
					return detectors;
			}
			throw new ArgumentException(string.Format("Invalid sort column name: '{0}'.", columnName), "columnName");
		}

		/// <summary>
		/// Sorts the <paramref name="inputFiles"/> on a user selected column.
		/// </summary>
		/// <param name="inputFiles">the input files to sort</param>
		/// <param name="columnName">the name of the column to compare</param>
		/// <param name="sortDirection">the sort direction</param>
		/// <returns>the sorted input files</returns>
		public static IEnumerable<IInputFile> Sort(this IEnumerable<IInputFile> inputFiles, string columnName, ListSortDirection sortDirection)
		{
			PreConditions.Argument("inputFiles").Value(inputFiles).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull();

			if (columnName == DefaultColumnIndex.Name.GetName())
			{
				return inputFiles.OrderBy(f => f.Name, sortDirection);
			}
			return inputFiles;
		}

		/// <summary>
		/// Sorts the <paramref name="results"/> on a user selected column.
		/// This function returns <paramref name="results"/> unmodified if
		/// the list is already sorted 
		/// </summary>
		/// <param name="results">the resuls to sort</param>
		/// <param name="columnName">the name of the column to compare</param>
		/// <param name="sortDirection">the sort direction</param>
		/// <returns>the results list (sorted)</returns>
		public static IEnumerable<IResultNode> Sort(this IEnumerable<IResultNode> results, string columnName, ListSortDirection sortDirection)
		{
			PreConditions.Argument("results").Value(results).IsNotNull();
			PreConditions.Argument("columnName").Value(columnName).IsNotNull();

			DefaultColumnIndex index;
			if (DefaultColumnExtensions.TryParse(columnName, out index))
			{
				switch (index)
				{
					case DefaultColumnIndex.Name:
						return results.OrderBy(r => r.Name, sortDirection);
					case DefaultColumnIndex.Detector:
						return results.OrderBy(r => r.Detectors.First().Name, sortDirection);
					case DefaultColumnIndex.DetectorVersion:
						return results.OrderBy(r => r.Detectors.First().VersionString(), sortDirection);
					case DefaultColumnIndex.Offset:
						return results.OrderBy(r => r.StartOffset, sortDirection);
					case DefaultColumnIndex.Length:
						return results.OrderBy(r => r.Length, sortDirection);
					case DefaultColumnIndex.EndOffset:
						return results.OrderBy(r => r.EndOffset, sortDirection);
					case DefaultColumnIndex.File:
						return results.OrderBy(r => GetResultFileName(r), sortDirection);
				}
			}
			return results.OrderBy(r => r.FindAttributeByName(columnName) == null ? null : r.FindAttributeByName(columnName).Value, sortDirection);
		}

		/// <summary>
		/// Gets the file name for a <paramref name="dataPacket"/>.
		/// </summary>
		/// <param name="dataPacket">the dataPacket</param>
		/// <returns>the file name</returns>
		private static string GetResultFileName(IDataPacket dataPacket)
		{
			IInputFile inputFile = dataPacket.InputFile;
			return (inputFile == null) ? string.Empty : inputFile.Name;
		}
	}
}
