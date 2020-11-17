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
using Defraser.Util;

namespace Defraser.GuiCe
{
	public sealed class HexWorkshopEventArgs : EventArgs, IEquatable<HexWorkshopEventArgs>
	{
		#region Properties
		/// <summary>The path of the file to open.</summary>
		public string FilePath { get; private set; }
		/// <summary>The offset in the file.</summary>
		public long Offset { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="HexWorkshopEventArgs"/>.
		/// </summary>
		/// <param name="filePath">the path of the file to open</param>
		/// <param name="offset">the offset in the file</param>
		public HexWorkshopEventArgs(string filePath, long offset)
		{
			PreConditions.Argument("filePath").Value(filePath).IsNotNull();
			PreConditions.Argument("offset").Value(offset).IsNotNegative();

			FilePath = filePath;
			Offset = offset;
		}

		public override string ToString()
		{
			return string.Format("{0}[FilePath={1}, Offset={2}]", GetType().Name, FilePath, Offset);
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return Equals(obj as HexWorkshopEventArgs);
		}

		public bool Equals(HexWorkshopEventArgs other)
		{
			if (other == null) return false;
			if (other == this) return true;

			if (FilePath != other.FilePath) return false;
			if (Offset != Offset) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return FilePath.GetHashCode() ^ Offset.GetHashCode();
		}
		#endregion Equals method
	}
}
