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
using Defraser.Interface;

namespace Defraser.GuiCe
{
	/// <summary>
	/// Provides event arguments for <c>FocusRowResultsChanged</c> events.
	/// </summary>
	public class ResultsEventArgs : EventArgs
	{
		#region Properties
		/// <summary>The results for a detectable.</summary>
		public IResultNode Results { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new results event args for the given <paramref name="results"/>.
		/// </summary>
		/// <param name="results">the results</param>
		public ResultsEventArgs(IResultNode results)
		{
			this.Results = results;
		}
	}

	internal class RetrieveDataNotifyEventArgs : EventArgs
	{
		internal bool LabelIsVisible { get; private set; }
		internal IDataBlock DataBlock { get; private set; }
		internal ICodecStream CodecStream { get; private set; }

		internal RetrieveDataNotifyEventArgs(bool labelIsVisible)
		{
			this.LabelIsVisible = labelIsVisible;
		}
		internal RetrieveDataNotifyEventArgs(bool labelIsVisible, IDataBlock dataBlock)
		{
			this.LabelIsVisible = labelIsVisible;
			this.DataBlock = dataBlock;
		}
		internal RetrieveDataNotifyEventArgs(bool labelIsVisible, ICodecStream codecStream)
		{
			this.LabelIsVisible = labelIsVisible;
			this.CodecStream = codecStream;
		}
	}

	/// <summary>
	/// Provides event argument for <c>AddColumns</c> events.
	/// </summary>	
	public class AddColumnsEventArgs : EventArgs
	{
		#region Properties
		/// <summary>The names for the custom columns to add.</summary>
		public IList<string> ColumnNames { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates event arguments for the given column names.
		/// </summary>
		/// <param name="columnNames">the names for the columns</param>
		public AddColumnsEventArgs(IList<string> columnNames)
		{
			this.ColumnNames = columnNames;
		}
	}

	/// <summary>
	/// Provides event argument for <c>AddColumns</c> events.
	/// </summary>	
	public class AddColumnsDetectorEventArgs : EventArgs
	{
		#region Properties
		/// <summary>The detector to add custom columns for.</summary>
		public IDetector Detector { get; private set; }
		#endregion Properties


		/// <summary>
		/// Creates event arguments for the given <paramref name="detector"/>.
		/// </summary>
		/// <param name="detector">the detector for the columns</param>
		public AddColumnsDetectorEventArgs(IDetector detector)
		{
			this.Detector = detector;
		}
	}
}
