﻿/*
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

namespace Defraser.DataStructures
{
	public sealed class ResultNodeBuilder : IResultNodeBuilder
	{
		#region Inner classes
		private sealed class ResultNode : IResultNode
		{
			private readonly string _name;
			private readonly IList<IResultAttribute> _attributes;
			private readonly IList<IResultNode> _children;
			private readonly IMetadata _metadata;
			private readonly IDataPacket _dataPacket;

			#region Properties
			public string Name { get { return _name; } }
			public IList<IResultAttribute> Attributes { get { return _attributes; } }
			public bool Valid { get; set; }
			public IList<IResultNode> Children { get { return _children; } }
			public IResultNode Parent { get; set; }
			public CodecID DataFormat { get { return _metadata.DataFormat; } }
			public IEnumerable<IDetector> Detectors { get { return _metadata.Detectors; } }
			public IInputFile InputFile { get { return _dataPacket.InputFile; } }
			public long Length { get { return _dataPacket.Length; } }
			public long StartOffset { get { return _dataPacket.StartOffset; } }
			public long EndOffset { get { return _dataPacket.EndOffset; } }
			#endregion Properties

			internal ResultNode(ResultNodeBuilder builder)
			{
				_name = builder.Name;
				Valid = builder._valid;
				_attributes = new List<IResultAttribute>(builder._attributes).AsReadOnly();
				_children = new List<IResultNode>();
				_metadata = builder.Metadata;
				_dataPacket = builder.DataPacket;
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
		}
		#endregion Inner classes

		private readonly IList<IResultAttribute> _attributes;
		private bool _valid;

		#region Properties
		public string Name { private get; set; }
		public IMetadata Metadata { private get; set; }
		public IDataPacket DataPacket { private get; set; }
		#endregion Properties

		public ResultNodeBuilder()
		{
			_attributes = new List<IResultAttribute>();
			_valid = true;
		}

		public void Reset()
		{
			_attributes.Clear();
			_valid = true;
		}

		public void Invalidate()
		{
			_valid = false;
		}

		public void AddAttribute(IResultAttribute attribute)
		{
			_attributes.Add(attribute);
		}

		public IResultNode Build()
		{
			return new ResultNode(this);
		}
	}
}
