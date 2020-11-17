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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	internal static class VirtualTreeExtensions
	{
		internal static bool ContainsInputFile(this RowSelectionList rowSelectionList)
		{
			foreach (Row row in rowSelectionList)
			{
				if(row.Item is IInputFile) return true;
			}
			return false;
		}

		public static IFragment[] GetSelectedFragmentsInGuiOrder(this RowSelectionList selectedRows)
		{
			ICollection<IFragmentContainer> handledFragmentContrainers = new HashSet<IFragmentContainer>();
			var fragments = new List<IFragment>();
			foreach (Row row in selectedRows.GetRowsInGuiOrder())
			{
				var fragment = row.Item as IFragment;
				if (fragment == null) continue;

				if (fragment.FragmentContainer != null)
				{
					if (!handledFragmentContrainers.Contains(fragment.FragmentContainer))
					{
						handledFragmentContrainers.Add(fragment.FragmentContainer);

						foreach (IFragment fragment1 in fragment.FragmentContainer)
						{
							fragments.Add(fragment1);
						}
					}
				}
				else
				{
					fragments.Add(fragment);
				}
			}
			return fragments.ToArray();
		}

		private static Row[] GetRowsInGuiOrder(this RowSelectionList rowSelectionList)
		{
			if (rowSelectionList == null || rowSelectionList.Count == 0) return new Row[0];

			return rowSelectionList.GetRows().GetRowsInGuiOrder();
		}

		/// <summary>
		/// Gets a unique subset of the rows from <paramref name="rowSelectionList"/>, so that
		/// no row is a descendant of any other row.
		/// The rows are sorted in the order they appear in the tree.
		/// </summary>
		/// <param name="rowSelectionList">the rows</param>
		/// <returns>the unique rows in GUI order</returns>
		internal static Row[] GetUniqueRowsInGuiOrder(this RowSelectionList rowSelectionList)
		{
			if (rowSelectionList == null || rowSelectionList.Count == 0) return new Row[] { };

			return rowSelectionList.GetRows().GetUniqueRowsInGuiOrder();
		}

		/// <summary>
		/// Returns whether <paramref name="row"/> contains an item of
		/// type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">the type of item</typeparam>
		/// <param name="row">the row</param>
		/// <returns>whether the row contains an item of type T</returns>
		internal static bool RowContainsItem<T>(this Row row) where T : class
		{
			// TODO: use System.Type.IsAssignableFrom(row.Item.GetType())
			return (row != null) && (row.Item is T);
		}

		/// <summary>
		/// Find out whether the parent of the <paramref name="row"/> are expanded.
		/// </summary>
		/// <param name="row">The row to check if all its parents are expanded.</param>
		/// <returns>true when all parents are expanded or if the row has no parent. False otherwise.</returns>
		internal static bool AllParentsExpanded(this Row row)
		{
			if (row.ParentRow == null) return true;

			if (row.ParentRow != null && !row.ParentRow.Expanded)
			{
				return false;
			}
			return AllParentsExpanded(row.ParentRow);
		}

		/// <summary>
		/// Gets a unique subset of the <paramref name="rows"/>, so that
		/// no row is a descendant of any other row.
		/// The rows are sorted in the order they appear in the tree.
		/// </summary>
		/// <param name="rows">the rows</param>
		/// <returns>the unique rows in GUI order</returns>
		internal static Row[] GetUniqueRowsInGuiOrder(this Row[] rows)
		{
			// Filter out any children of the rows
			// The result represents a unique part of the tree
			List<Row> uniqueRows = new List<Row>();
			Row previousRow = null;
			foreach (Row row in rows.GetRowsInGuiOrder())
			{
				if (previousRow == null || !row.IsDescendant(previousRow))
				{
					uniqueRows.Add(row);
					previousRow = row;
				}
			}
			return uniqueRows.ToArray();
		}

		/// <summary>
		/// Get the rows in the order they appear in the tree.
		/// </summary>
		/// <param name="rows">the rows</param>
		/// <returns>The rows in GUI order</returns>
		internal static Row[] GetRowsInGuiOrder(this Row[] rows)
		{
			SortedDictionary<int, Row> sortedRows = new SortedDictionary<int, Row>();
			foreach (Row row in rows)
			{
				if (row.AllParentsExpanded())
				{
					sortedRows.Add(row.RowIndex, row);
				}
			}
			return sortedRows.Values.ToArray();
		}
	}
}
