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

namespace Defraser.Test
{
	class DummyResult : IResultNode
	{
		#region Properties
		public string Name
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public IList<IResultAttribute> Attributes
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public bool Valid
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		public CodecID DataFormat { get { throw new NotImplementedException(); } }
		public IEnumerable<IDetector> Detectors { get { throw new NotImplementedException(); } }
		public IInputFile InputFile { get { throw new NotImplementedException(); } }
		public long Length{ get { throw new NotImplementedException(); } }
		public long StartOffset { get { throw new NotImplementedException(); } }
		public long EndOffset { get { throw new NotImplementedException(); } }

		public IResultNode Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		public IList<IResultNode> Children
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}
		#endregion Properties

		public IResultAttribute FindAttributeByName(string name)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			throw new NotImplementedException();
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			throw new NotImplementedException();
		}

		public IDataPacket GetFragment(long offset)
		{
			throw new NotImplementedException();
		}
	}
}
