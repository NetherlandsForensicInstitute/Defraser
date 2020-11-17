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
using System.Linq;
using Defraser.DataStructures;
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Provides the basic functions for implementing result headers.
	/// </summary>
	/// <typeparam name="THeader">the header type</typeparam>
	/// <typeparam name="THeaderName">the header name enumeration type</typeparam>
	/// <typeparam name="TParser">the parser type</typeparam>
	public abstract class Header<THeader, THeaderName, TParser> : IResultNode
			where THeader : Header<THeader, THeaderName, TParser>
			// where THeaderName : enum
			where TParser : Parser<THeader, THeaderName, TParser>
	{
		private readonly IEnumerable<IDetector> _detectors;
		private readonly THeaderName _headerName;
		private readonly List<IResultNode> _children;
		protected long _offset;
		private readonly List<IResultAttribute> _attributes;

		#region Properties
		public virtual string Name { get { return Enum.GetName(typeof(THeaderName), _headerName); } }
		public IList<IResultAttribute> Attributes { get { return _attributes; } }
		public abstract CodecID DataFormat { get; }
		public IEnumerable<IDetector> Detectors { get { return _detectors; } }
		public IInputFile InputFile { get { return (DataPacket == null) ? null : DataPacket.InputFile; } }
		public long Length { get { return (DataPacket == null) ? 0L : DataPacket.Length; } }
		public long StartOffset { get { return (DataPacket == null) ? 0L : DataPacket.StartOffset; } }
		public long EndOffset { get { return (DataPacket == null) ? 0L : DataPacket.EndOffset; } }
		virtual public IList<IResultNode> Children { get { return _children; } }

		protected IDataPacket DataPacket { get; set; }
		public bool Valid { get; set; }
		public IResultNode Parent { get; set; }

		/// <summary>The header name type.</summary>
		virtual public THeaderName HeaderName { get { return _headerName; } }
		/// <summary>The relative offset of this header.</summary>
		virtual public long Offset { get { return _offset; } }
		/// <summary>The first child of this header, <c>null</c> if it has no children.</summary>
		public THeader FirstChild
		{
			get
			{
				if (!this.HasChildren())
				{
					return null;
				}
				return _children[0] as THeader;
			}
		}
		/// <summary>
		/// The last direct child of this header, <c>null</c> if it has no children.
		/// </summary>
		public THeader LastChild
		{
			get
			{
				if (!this.HasChildren())
				{
					return null;
				}
				return _children[_children.Count - 1] as THeader;
			}
		}
		/// <summary>
		/// The last header in the tree of headers with this header as root.
		/// This returns the header itself if it has no children.
		/// </summary>
		public THeader LastHeader
		{
			get
			{
				THeader child = LastChild;
				return (child == null) ? this as THeader : child.LastHeader;
			}
		}
		/// <summary>The root node.</summary>
		public THeader Root
		{
			get
			{
				THeader parent = Parent as THeader;
				return (parent == null) ? (THeader)this : parent.Root;
			}
		}
		/// <summary>Whether this header is the root.</summary>
		virtual public bool IsRoot { get { return Parent == null; } }
		#endregion Properties


		protected Header(IEnumerable<IDetector> detectors, THeaderName headerName)
		{
			if (detectors == null)
			{
				throw new ArgumentNullException("detectors");
			}

			_detectors = detectors;
			_headerName = headerName;
			_children = new List<IResultNode>();
			_attributes = null;
			Parent = null;
			Valid = true;
		}

		/// <summary>
		/// Creates a new header after the <paramref name="previousHeader"/>.
		/// </summary>
		/// <param name="previousHeader">the directly preceeding header</param>
		/// <param name="headerName">the type of header</param>
		/// <remarks>
		/// The <paramref name="previousHeader"/> is set as initial <c>DataBlock</c>
		/// and is used to locate other headers.
		/// </remarks>
		public Header(THeader previousHeader, THeaderName headerName)
		{
			if (previousHeader == null)
			{
				throw new ArgumentNullException("previousHeader");
			}

			_detectors = previousHeader._detectors;
			_headerName = headerName;
			_children = new List<IResultNode>();
			_attributes = new List<IResultAttribute>();
			Parent = previousHeader;
			Valid = true;
		}

		#region IResultNode methods
		public IResultAttribute FindAttributeByName(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (_attributes != null)
			{
				foreach (IResultAttribute attribute in _attributes)
				{
					if (attribute.Name == name)
					{
						return attribute;
					}
				}
			}
			return null;
		}
		#endregion IResultNode methods

		#region IData methods
		public IDataPacket Append(IDataPacket dataPacket)
		{
			return DataPacket.Append(dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			return DataPacket.GetSubPacket(offset, length);
		}

		public IDataPacket GetFragment(long offset)
		{
			return DataPacket.GetFragment(offset);
		}
		#endregion IData methods

		/// <summary>
		/// Parses the header information from <paramref name="parser"/>.
		/// The <c>Offset</c> of this header is set to the current <c>Position</c>
		/// of <c>parser.DataReader</c>.
		/// </summary>
		/// <param name="parser">the parser</param>
		/// <returns>whether the header was parsed successfully</returns>
		public virtual bool Parse(TParser parser) //TODO: make void (throws exception on failure)
		{
			_offset = parser.Position;
			return true;
		}

		/// <summary>
		/// Ends parsing this header. This constructs the data packet.
		/// The <c>Length</c> of this header is set to the number of bytes
		/// parsed since the last invoke of the <c>Parse()</c> method.
		/// </summary>
		/// <param name="parser">the parser</param>
		/// <returns>whether the header was valid</returns>
		public virtual bool ParseEnd(TParser parser)
		{
			long length = (parser.Position - _offset);
			if (length <= 0)
			{
				return false;
			}
			DataPacket = parser.GetDataPacket(_offset, length);
			return true;
		}

		/// <summary>
		/// Tests whether this header directly precedes <paramref name="header"/>.
		/// </summary>
		/// <param name="header">the header to test</param>
		/// <returns>whether this header directly precedes the given header</returns>
		public virtual bool IsBackToBack(THeader header)
		{
			Debug.Assert(header != null);
			return (this.Offset + this.Length) == header.Offset;
		}

		/// <summary>
		/// Tests whether the given <paramref name="parent"/> is suitable for this header.
		/// </summary>
		/// <param name="parent">the parent to test</param>
		/// <returns>whether the given parent is suitable for this header</param>
		public virtual bool IsSuitableParent(THeader parent)
		{
			Debug.Assert(parent.Offset <= this.Offset);
			return parent.Parent == null;
		}

		/// <summary>
		/// Finds a <em>suitable</em> parent for this header using the
		/// <c>previousHeader</c> provided to the constructor.
		/// Checks the relation between this header and the current results
		/// and returns <c>null</c> if this header is not valid at this point.
		/// </summary>
		/// <returns>the parent or <c>null</c> if invalid</returns>
		/// <seealso cref="IsSuitableParent(THeader)"/>
		public THeader FindSuitableParent()
		{
			THeader previousHeader = Parent as THeader;
			for (THeader parent = previousHeader; parent != null; parent = parent.Parent as THeader)
			{
				if (IsSuitableParent(parent))
				{
					return parent;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first ancestor with the given <paramref name="headerName"/>.
		/// </summary>
		/// <param name="headerName">the header name of the parent to find</param>
		/// <returns>the parent or <c>null</c> for none</returns>
		public THeader FindParent(THeaderName headerName)
		{
			for (THeader parent = Parent as THeader; parent != null; parent = parent.Parent as THeader)
			{
				if (parent.HeaderName.Equals(headerName))
				{
					return parent;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first child with the given <paramref name="headerName"/>.
		/// This method is non-recursive and only searches in <c>Children</c>.
		/// </summary>
		/// <param name="headerName">the header name of the child to find</param>
		/// <returns>the child or <c>null</c> for none</returns>
		public THeader FindChild(THeaderName headerName)
		{
			return FindChild(headerName, false);
		}

		/// <summary>
		/// Finds the first child with the given <paramref name="headerName"/>.
		/// </summary>
		/// <param name="headerName">the header name of the child to find</param>
		/// <param name="recursive">whether to search recursively</param>
		/// <returns>the child or <c>null</c> for none</returns>
		public THeader FindChild(THeaderName headerName, bool recursive)
		{
			foreach (THeader child in Children)
			{
				if (child.HeaderName.Equals(headerName))	// TODO: use '==' on enum type
				{
					return child;
				}
			}
			if (recursive)
			{
				foreach (THeader child in Children)
				{
					THeader header = child.FindChild(headerName, recursive);
					if (header != null)
					{
						return header;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Finds one of the children with the given <paramref name="headerNames"/>.
		/// </summary>
		/// <param name="headerNames">the header names of the children to find</param>
		/// <param name="recursive">whether to search recursively</param>
		/// <returns>the child or <c>null</c> for none</returns>
		public THeader FindChildren(THeaderName[] headerNames, bool recursive)
		{
			foreach (THeader child in Children)
			{
				if (headerNames.Contains(child.HeaderName))
				{
					return child;
				}
			}
			if (recursive)
			{
				foreach (THeader child in Children)
				{
					THeader header = child.FindChildren(headerNames, recursive);
					if (header != null)
					{
						return header;
					}
				}
			}
			return null;
		}

		// TODO: comment and add unit test
		public bool ContainsChild(THeaderName headerName)
		{
			return (FindChild(headerName) != null);
		}

		public bool ContainsChild(THeaderName headerName, bool recursive)
		{
			return (FindChild(headerName, recursive) != null);
		}

		/// <summary>
		/// return true when the this header contains one of the headers specified
		/// </summary>
		public bool ContainsChildren(THeaderName[] headerNames, bool recursive)
		{
			return (FindChildren(headerNames, recursive) != null);
		}

		protected double ByteEntropy(TParser parser)
		{
			long currentPosition = parser.Position;

#if DEBUG
			long position = parser.Position;
#endif // DEBUG
			double entropy = 0.0;

			// Wrap the data reader
			ByteStreamDataReader byteStreamDataReader = new ByteStreamDataReader(parser);
			byteStreamDataReader.Position = this.Offset + 2;

			long[] byteValueCountArray = new long[256];
			long endOffset = this.Offset + this.Length;
			while (byteStreamDataReader.Position < endOffset)
			{
				byteValueCountArray[byteStreamDataReader.GetByte()]++;
			}

			for (int byteIndex = 0; byteIndex < 256; byteIndex++)
			{
				double p = byteValueCountArray[byteIndex] / (double)this.Length;
				if (p > 0.0)
				{
					entropy += -(double)(p * Math.Log(p, 2.0));
				}
			}

			parser.Position = currentPosition;

#if DEBUG
			Debug.Assert(parser.Position == position);
#endif // DEBUG

			return entropy;
			
		}
	}
}
