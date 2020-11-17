/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Text;
using Defraser.Interface;

namespace Defraser.Test.Common
{
	internal class ResultNodeDescriptionBuilder{
		private readonly StringBuilder _text = new StringBuilder();
		private int _depth;
		private readonly Stack<int> _childrenTotals = new Stack<int>();
		private readonly Stack<int> _childrenCount = new Stack<int>();

		public override string  ToString()
		{
			return _text.ToString();
		}

		public ResultNodeDescriptionBuilder()
		{
			_childrenTotals.Push(0);
			_childrenCount.Push(0);
		}

		public void NewNode()
		{
			_childrenCount.Push(_childrenCount.Pop() + 1);
			AddLine("Node:");
			_depth++;
			_childrenTotals.Push(0);
			_childrenCount.Push(0);
		}

		public void EndNode()
		{
			var total=_childrenTotals.Pop();
			var count = _childrenCount.Pop();
			if(count<total)
			{
				AddLine(".."+(total-count)+" children");
			}
			_depth--;
		}

		public void StartOffset(long offset)
		{
			AddLine("StartOffset=" + offset);
		}

		public void AddLine(string s)
		{
			_text.Append(new string('\t', _depth));
			_text.AppendLine(s);
		}

		public void Length(long length)
		{
			AddLine("Length="+length);
		}

		public void EndOffset(long offset)
		{
			AddLine("EndOffset=" + offset);
		}

		public void StartChildren(int count)
		{
			AddLine("Children.Count="+count);
			_childrenTotals.Pop();
			_childrenTotals.Push(count);
		}

		public void Attribute(IResultAttribute attribute)
		{
			AddLine(attribute.Name+":"+attribute.ValueAsString);
		}
	}
}
