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
using System.Linq;
using Defraser.DataStructures;
using Defraser.Interface;

namespace Defraser.Test
{
	public class MockResult : IResultNode
	{
		private readonly string _name;
		private readonly List<IResultAttribute> _attributes;
		private readonly IDetector _detector;
		private readonly IDataPacket _dataPacket;
		private readonly List<IResultNode> _children = new List<IResultNode>();
		private IResultNode _parent;

		#region Properties
		public string Name { get { return _name; } }
		public IList<IResultAttribute> Attributes { get { return _attributes; } }
		public bool Valid { get { return true; } set { } }
		public CodecID DataFormat { get { return CodecID.Unknown; } }
		public IEnumerable<IDetector> Detectors { get { return Enumerable.Repeat(_detector, 1); } }
		public IInputFile InputFile { get { return (_dataPacket == null) ? null : _dataPacket.InputFile; } }
		public long Length { get { return (_dataPacket == null) ? 0L : _dataPacket.Length; } }
		public long StartOffset { get { return (_dataPacket == null) ? 0L : _dataPacket.StartOffset; } }
		public long EndOffset { get { return (_dataPacket == null) ? 0L : _dataPacket.EndOffset; } }
		public IList<IResultNode> Children { get { return _children; } }

		public IResultNode Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		internal long Offset { get; private set; }
		#endregion Properties

		/// <summary>
		/// Constructor for root nodes for detector plugins.
		/// </summary>
		/// <param name="detector">the detector for the result node</param>
		public MockResult(IDetector detector)
		{
			if (detector == null)
			{
				throw new ArgumentNullException("detector");
			}

			_name = "[root]";
			_attributes = new List<IResultAttribute>();
			_dataPacket = null;
			_detector = detector;
			_parent = null;
		}

		public MockResult(IResultNode parent, IDataReader dataReader, long offset, long length, string name)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			if (dataReader == null)
			{
				throw new ArgumentNullException("dataReader");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (length <= 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			_name = name;
			_attributes = new List<IResultAttribute>();
			_dataPacket = dataReader.GetDataPacket(offset, length);
			_detector = parent.Detectors.First();
			Offset = offset;

			parent.AddChild(this);
		}

		public MockResult(MockResult mockResult)
		{
			_name = mockResult._name;
			_attributes = mockResult._attributes;
			_dataPacket = mockResult._dataPacket;
			_detector = mockResult._detector;
			_parent = null;
		}

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

		#region IDataPacket members
		public IDataPacket Append(IDataPacket dataPacket)
		{
			return _dataPacket.Append(dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			return _dataPacket.GetSubPacket(offset, length);
		}

		public IDataPacket GetFragment(long offset)
		{
			return _dataPacket.GetFragment(offset);
		}
		#endregion IDataPacket members
	}
}
