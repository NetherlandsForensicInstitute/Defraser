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

namespace Defraser.DataStructures
{
	public sealed class RootResult : IResultNode
	{
		private static readonly IList<IResultAttribute> EmptyAttributes = new List<IResultAttribute>().AsReadOnly();

		private readonly IMetadata _metadata;
		private readonly IInputFile _inputFile;
		private readonly IList<IResultNode> _children;

		#region Properties
		public CodecID DataFormat { get { return _metadata.DataFormat; } }
		public IEnumerable<IDetector> Detectors { get { return _metadata.Detectors; } }
		public IInputFile InputFile { get { return _inputFile; } }
		public long Length { get { return 0L; } }
		public long StartOffset { get { return 0L; } }
		public long EndOffset { get { return 0L; } }
		public string Name { get { return "Root"; } }
		public IList<IResultAttribute> Attributes { get { return EmptyAttributes; } }
		public bool Valid { get { return true; } set { } }
		public IList<IResultNode> Children { get { return _children; } }
		public IResultNode Parent { get; set; }
		#endregion Properties

		public RootResult(IMetadata metadata, IInputFile inputFile)
		{
			_metadata = metadata;
			_inputFile = inputFile;
			_children = new List<IResultNode>();
		}

		public IResultAttribute FindAttributeByName(string name)
		{
			return null;
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			return dataPacket;
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			if ((offset != 0L) || (length != 0L))
			{
				throw new ArgumentException("root data packets are empty, so offset and length must be 0");
			}

			return this;
		}

		public IDataPacket GetFragment(long offset)
		{
			if (offset != 0L)
			{
				throw new ArgumentException("root data packets are empty, so offset must be 0");
			}

			return this;
		}
	}
}
