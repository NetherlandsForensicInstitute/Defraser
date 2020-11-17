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
	/// <summary>
	/// Decorates a result with a new list of children, as such providing
	/// a shallow copy of the result.
	/// </summary>
	internal sealed class ResultNodeCopy : IResultNode
	{
		private readonly IResultNode _result;
		private readonly List<IResultNode> _children;

		#region Properties
		public CodecID DataFormat { get { return _result.DataFormat; } }
		public IEnumerable<IDetector> Detectors { get { return _result.Detectors; } }
		public IInputFile InputFile { get { return _result.InputFile; } }
		public long Length { get { return _result.Length; } }
		public long StartOffset { get { return _result.StartOffset; } }
		public long EndOffset { get { return _result.EndOffset; } }
		public string Name { get { return _result.Name; } }
		public IList<IResultAttribute> Attributes { get { return _result.Attributes; } }
		public bool Valid { get { return _result.Valid; } set { } }

		public IList<IResultNode> Children { get { return _children; } }
		public IResultNode Parent { get; set;}
		#endregion Properties

		/// <summary>
		/// Creates a new shallow copy of the given <paramref name="result"/>.
		/// </summary>
		/// <param name="result">the result to copy</param>
		public ResultNodeCopy(IResultNode result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}

			// If already wrapped, use underlying result node
			var resultNodeCopy = result as ResultNodeCopy;
			_result = (resultNodeCopy == null) ? result : resultNodeCopy._result;
			_children = new List<IResultNode>();

			Parent = null;
		}

		public IResultAttribute FindAttributeByName(string name)
		{
			return _result.FindAttributeByName(name);
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			return _result.Append(dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			return _result.GetSubPacket(offset, length);
		}

		public IDataPacket GetFragment(long offset)
		{
			return _result.GetFragment(offset);
		}
	}
}
