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

namespace Defraser.Interface
{
	/// <summary>
	/// Specifies a change in a project.
	/// </summary>
	public enum ProjectChangedType
	{
		/// <summary>A data block was added to the project.</summary>
		DataBlockAdded,
		/// <summary>A data block was deleted from the project.</summary>
		DataBlockDeleted,
		/// <summary>A file was added to the project.</summary>
		FileAdded,
		/// <summary>A file was deleted from the project.</summary>
		FileDeleted,
		/// <summary>The project metadata was changed.</summary>
		MetadataChanged,
		/// <summary>The visible columns were changed.</summary>
		VisibleColumnsChanged,
		/// <summary>The file name of the project changed.</summary>
		FileNameChanged,
		/// <summary>The project was created</summary>
		Created,
		/// <summary>The project was opened</summary>
		Opened,
		/// <summary>The project was closed</summary>
		Closed
	}

	/// <summary>
	/// Provides data for <c>ProjectChanged</c> events.
	/// </summary>	
	public sealed class ProjectChangedEventArgs : EventArgs
	{
		/// <value>The type of changed.</value>
		public ProjectChangedType Type { get; private set; }
		/// <value>The affected item.</value>
		public Object Item { get; private set; }

		/// <summary>
		/// Creates a new event args of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">the type of change</param>
		public ProjectChangedEventArgs(ProjectChangedType type)
		{
			PreConditions.Argument("type").Value(type).IsDefinedOnEnum(typeof(ProjectChangedType));

			Type = type;
			Item = null;
		}

		/// <summary>
		/// Creates a new event args of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">the type of change</param>
		/// <param name="item">the affected item</param>
		public ProjectChangedEventArgs(ProjectChangedType type, Object item)
		{
			PreConditions.Argument("type").Value(type).IsDefinedOnEnum(typeof (ProjectChangedType));

			Type = type;
			Item = item;
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return Equals(obj as ProjectChangedEventArgs);
		}

		public bool Equals(ProjectChangedEventArgs other)
		{
			if (other == null) return false;
			if (other == this) return true;

			return (other.Type == Type) && Equals(other.Item, Item);
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode()
				.CombineHashCode(Item);
		}
		#endregion Equals method
	}
}
