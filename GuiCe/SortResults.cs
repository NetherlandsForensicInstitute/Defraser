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
using Defraser.Framework;
using Defraser.Interface;

namespace Defraser.GuiCe
{
	internal sealed class SortResults
	{
			private static readonly IComparer<IDetector> DetectorComparer= new DetectorComparer();
		/// <summary>
		/// Sorts the <paramref name="results"/> on a user selected column.
		/// This function returns <paramref name="results"/> unmodified if
		/// the list is already sorted 
		/// </summary>
		/// <param name="results">the resuls to sort</param>
		/// <param name="columnName">the name of the column to compare</param>
		/// <param name="sortDirection">the sort direction</param>
		/// <returns>the results list (sorted)</returns>
		public static IList<IResultNode> Sort(IList<IResultNode> results, string columnName, ListSortDirection sortDirection)
		{
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}
			if (columnName == null)
			{
				throw new ArgumentNullException("columnName");
			}

			Comparison<IResultNode> resultComparison = GetResultComparison(columnName);

			if (sortDirection == ListSortDirection.Descending)
			{
				Comparison<IResultNode> aResultComparison = resultComparison;
				resultComparison = delegate(IResultNode x, IResultNode y) { return -aResultComparison(x, y); };
			}

			if (!IsSorted(results, resultComparison))
			{
				List<IResultNode> resultsList = results as List<IResultNode>;

				if (resultsList != null)
				{
					resultsList.Sort(resultComparison);
				}
				else
				{
					// Copy results to new list and sort
					List<IResultNode> resultsCopy = new List<IResultNode>(results);
					resultsCopy.Sort(resultComparison);

					// Copy sorted results back into original list
					results.Clear();

					foreach (IResultNode result in resultsCopy)
					{
						results.Add(result);
					}
				}
			}
			return results;
		}

		/// <summary>
		/// Tests whether the <paramref name="results"/> are sorted.
		/// </summary>
		/// <param name="results">the results to test</param>
		/// <param name="resultComparison">the comparison delegate</param>
		/// <returns>whether the results are sorted</returns>
		private static bool IsSorted(IList<IResultNode> results, Comparison<IResultNode> resultComparison)
		{
			IResultNode prevResult = null;

			foreach (IResultNode result in results)
			{
				if (prevResult != null && resultComparison(prevResult, result) > 0)
				{
					return false;
				}
				prevResult = result;
			}
			return true;
		}

		/// <summary>
		/// Gets a <c>Comparison</c> delegate for comparing results.
		/// The results are compared by the given column name.
		/// </summary>
		/// <param name="columnName">the name of the column to compare</param>
		/// <returns>the comparison delegate</returns>
		private static Comparison<IResultNode> GetResultComparison(string columnName)
		{
			DefaultColumnIndex index;
			if (DefaultColumnExtensions.TryParse(columnName, out index))
			{
				switch (index)
				{
					case DefaultColumnIndex.Name:
						return delegate(IResultNode x, IResultNode y) { return x.Name.CompareTo(y.Name); };
					case DefaultColumnIndex.Detector:
						return delegate(IResultNode x, IResultNode y) { return x.Detectors.First().Name.CompareTo(y.Detectors.First().Name); };
					case DefaultColumnIndex.DetectorVersion:
						return delegate(IResultNode x, IResultNode y) { return DetectorComparer.Compare(x.Detectors.First(),y.Detectors.First()); };
					case DefaultColumnIndex.Offset:
						return delegate(IResultNode x, IResultNode y) { return x.StartOffset.CompareTo(y.StartOffset); };
					case DefaultColumnIndex.Length:
						return delegate(IResultNode x, IResultNode y) { return x.Length.CompareTo(y.Length); };
					case DefaultColumnIndex.EndOffset:
						return delegate(IResultNode x, IResultNode y) { return x.EndOffset.CompareTo(y.EndOffset); };
					case DefaultColumnIndex.File:
						return delegate(IResultNode x, IResultNode y) { return GetResultFileName(x).CompareTo(GetResultFileName(y)); };
				}
			}
			return delegate(IResultNode x, IResultNode y) { return CompareResultsByAttributeValue(x, y, columnName); };
		}

		/// <summary>
		/// Compares results on the values of an attribute.
		/// </summary>
		/// <param name="x">the first result to compare</param>
		/// <param name="y">the second result to compare</param>
		/// <param name="attributeName">the name of the attribute to compare</param>
		private static int CompareResultsByAttributeValue(IResultNode x, IResultNode y, string attributeName)
		{
			if (x == y)
			{
				return 0;
			}

			IResultAttribute attributeX = x.FindAttributeByName(attributeName);
			IResultAttribute attributeY = y.FindAttributeByName(attributeName);

			if (attributeX == attributeY)
			{
				return 0;
			}
			if (attributeX == null || attributeY == null)
			{
				return (attributeX == null) ? -1 : 1;
			}

			// Compare by object value if appropriate types
			object valueX = attributeX.Value;
			object valueY = attributeY.Value;

			if (valueX is byte && valueY is byte)
			{
				return ((byte)valueX).CompareTo((byte)valueY);
			}
			else if (valueX is short && valueY is short)
			{
				return ((short)valueX).CompareTo((short)valueY);
			}
			else if (valueX is ushort && valueY is ushort)
			{
				return ((ushort)valueX).CompareTo((ushort)valueY);
			}
			else if (valueX is int && valueY is int)
			{
				return ((int)valueX).CompareTo((int)valueY);
			}
			else if (valueX is uint && valueY is uint)
			{
				return ((uint)valueX).CompareTo((uint)valueY);
			}
			else if (valueX is long && valueY is long)
			{
				return ((long)valueX).CompareTo((long)valueY);
			}
			else if (valueX is ulong && valueY is ulong)
			{
				return ((ulong)valueX).CompareTo((ulong)valueY);
			}
			else if (valueX is bool && valueY is bool)
			{
				return ((bool)valueX).CompareTo((bool)valueY);
			}
			else if (valueX is float && valueY is float)
			{
				return ((float)valueX).CompareTo((float)valueY);
			}
			else if (valueX is double && valueY is double)
			{
				return ((double)valueX).CompareTo((double)valueY);
			}
			else if (valueX is decimal && valueY is decimal)
			{
				return ((decimal)valueX).CompareTo((decimal)valueY);
			}

			// Compare by string value
			string stringX = attributeX.ValueAsString;
			string stringY = attributeY.ValueAsString;

			if (stringX == stringY)
			{
				return 0;
			}
			else if (stringX == null || stringY == null)
			{
				return (stringX == null) ? -1 : 1;
			}
			else
			{
				return stringX.CompareTo(stringY);
			}
		}

		/// <summary>
		/// Gets the file name for a <paramref name="result"/>.
		/// </summary>
		/// <param name="result">the result</param>
		/// <returns>the file name</returns>
		private static string GetResultFileName(IResultNode result)
		{
			IInputFile inputFile = result.InputFile;
			return (inputFile == null) ? string.Empty : inputFile.Name;
		}
	}
}
