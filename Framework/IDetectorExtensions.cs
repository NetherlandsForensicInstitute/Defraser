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
using System.Drawing;
using System.Linq;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Provides extension methods to <c>IDetector</c> implementations.
	/// </summary>
	public static class IDetectorExtensions
	{
		private static readonly IList<IColumnInfo> noColumns = (new List<IColumnInfo>()).AsReadOnly();
		private static readonly Dictionary<IDetector, IList<IColumnInfo>> _columnsCache = new Dictionary<IDetector, IList<IColumnInfo>>();

		private const string Detector = "Detector";
		private const string Name = "Name";
		private const string ConfigurationItem = "ConfigurationItem";
		private const string UserValueKey = "UserValueKey";
		private const string UserValueValue = "UserValueValue";

		/// <summary>
		/// Gets the columns for the given <paramref name="detector"/>.
		/// This returns the list of all possible columns for all possible
		/// headers for the given detector.
		/// </summary>
		/// <remarks>
		/// The results of this method are cached for efficiency.
		/// </remarks>
		/// <param name="detector">the detector to return the columns for</param>
		/// <returns>the list of column infos (read-only)</returns>
		/// <seealso cref="IDetector.Columns"/>
		public static IList<IColumnInfo> GetColumns(this IDetector detector)
		{
			if (detector == null)
			{
				return noColumns;
			}

			// Try to return a cached copy
			IList<IColumnInfo> columnInfos;
			if (_columnsCache.TryGetValue(detector, out columnInfos))
			{
				return columnInfos;
			}

			// Retrieve the unique column names
			HashSet<string> columnNames = new HashSet<string>();
			foreach (KeyValuePair<string, string[]> keyValuePair in detector.Columns)
			{
				foreach (string columnName in keyValuePair.Value)
				{
					columnNames.Add(columnName);
				}
			}

			// Sort the column names
			List<string> sortedColumnNames = new List<string>(columnNames);
			sortedColumnNames.Sort();

			// Create the column infos
			List<IColumnInfo> columnInfoList = new List<IColumnInfo>();
			foreach (string name in sortedColumnNames)
			{
				// TODO: use type info for alignment (when it becomes available)
				columnInfoList.Add(new ColumnInfo(name, name, StringAlignment.Near, ColumnInfo.ColumnWidth));
			}

			// Protect agains modifications and store in cache
			columnInfos = columnInfoList.AsReadOnly();
			_columnsCache.Add(detector, columnInfos);

			return columnInfos;
		}

		/// <summary>
		/// Checks whether there exists a column with the given name in the
		/// list of possible columns for the given header produced by the
		/// specified <paramref name="detector"/>.
		/// <param name="detector">the detector</param>
		/// <param name="headerName">the name of the header</param>
		/// <param name="columnName">the name of the column to test</param>
		/// <returns>true if the column exists for the given header, false otherwise</returns>
		public static bool ColumnInHeader(this IDetector detector, string headerName, string columnName)
		{
			string[] columns;
			if (detector.Columns.TryGetValue(headerName, out columns))
			{
				if (Array.IndexOf(columns, columnName) != -1)
				{
					return true;
				}
			}
			return false;
		}

		private static IConfigurationItem GetConfigurationItem(IDetector detector, string configurationItemKey)
		{
			IConfigurable configurableDetector = detector as IConfigurable;
			if (configurableDetector == null) return null;

			ICollection<IConfigurationItem> configurationItems = configurableDetector.Configuration;
			return configurationItems.SingleOrDefault(configItem => configItem.Description == configurationItemKey);
		}

		/// <summary>
		/// Set a configuration item
		/// </summary>
		/// <param name="detector">the detector to set the configuration item on</param>
		/// <param name="configurationItemKey">the configuration item to set</param>
		/// <param name="userValue">the new value for the configuration item</param>
		public static void SetConfigurationItem(this IDetector detector, string configurationItemKey, string userValue)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			PreConditions.Argument("configurationItemKey").Value(configurationItemKey).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("userValue").Value(userValue).IsNotNull().And.IsNotEmpty();

			GetConfigurationItem(detector, configurationItemKey).SetUserValue(userValue);
		}

		/// <summary>
		/// Reset one configuration item
		/// </summary>
		/// <param name="detector">the detector to set the configuration item on</param>
		/// <param name="configurationItemKey">The configuration item to reset to its default value</param>
		public static void ResetConfigurationItem(this IDetector detector, string configurationItemKey)
		{
			PreConditions.Argument("detector").Value(detector).IsNotNull();
			PreConditions.Argument("configurationItemKey").Value(configurationItemKey).IsNotNull().And.IsNotEmpty();

			GetConfigurationItem(detector, configurationItemKey).ResetDefault();
		}

		/// <summary>
		/// Reset all configuration items of <paramref name="detector"/>
		/// </summary>
		/// <param name="detector">The detector to reset all configuration items on</param>
		public static void ResetConfiguration(this IDetector detector)
		{
			IConfigurable configurableDetector = detector as IConfigurable;
			if (configurableDetector == null) return;

			foreach(IConfigurationItem configurationItem in configurableDetector.Configuration)
			{
				configurationItem.ResetDefault();
			}
		}
	}
}
