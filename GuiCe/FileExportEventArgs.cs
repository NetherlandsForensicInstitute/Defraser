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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
	public sealed class FileExportEventArgs<T> : EventArgs, IEquatable<FileExportEventArgs<T>>
	{
		#region Properties
		/// <summary></summary>
		public T Items { get; private set; }
		/// <summary></summary>
		public string OutputPath { get; private set; }
		/// <summary></summary>
		public IProgressReporter ProgressReporter { get; set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="FileExportEventArgs"/>.
		/// </summary>
		/// <param name="items">the items to export</param>
		/// <param name="outputPath">the target file or directory for storing the items</param>
		/// <param name="progressReporter">the progress reporter</param>
		public FileExportEventArgs(T items, string outputPath, IProgressReporter progressReporter)
		{
			PreConditions.Argument("items").Value(items).IsNotNull();
			PreConditions.Argument("outputPath").Value(outputPath).IsNotNull();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			Items = items;
			OutputPath = outputPath;
			ProgressReporter = progressReporter;
		}

		public override string ToString()
		{
			return string.Format("{0}[Items={1}, OutputPath={2}]", GetType().Name, Items, OutputPath);
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return Equals(obj as FileExportEventArgs<T>);
		}

		public bool Equals(FileExportEventArgs<T> other)
		{
			if (other == null) return false;
			if (other == this) return true;

			if (!Items.Equals(other.Items)) return false;
			if (OutputPath != other.OutputPath) return false;
			if (ProgressReporter != other.ProgressReporter) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return Items.GetHashCode() ^ OutputPath.GetHashCode() ^ ProgressReporter.GetHashCode();
		}
		#endregion Equals method
	}
}
