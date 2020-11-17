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

namespace Defraser.Framework
{
	internal sealed class RescannedCodecStream : ICodecStream
	{
		private readonly ICodecStream _codecStream;
		private readonly IDataPacket _data;

		#region Properties
		public CodecID DataFormat { get { return _codecStream.DataFormat; } }
		public IEnumerable<IDetector> Detectors { get { return _codecStream.Detectors; } }
		public IInputFile InputFile { get { return _data.InputFile; } }
		public long Length { get { return _data.Length; } }
		public long StartOffset { get { return _data.StartOffset; } }
		public long EndOffset { get { return _data.EndOffset; } }
		public long AbsoluteStartOffset { get { return _codecStream.AbsoluteStartOffset; } }
		public int StreamNumber { get { return _codecStream.StreamNumber; } }
		public string Name { get { return _codecStream.Name; } }
		public IDataBlock DataBlock { get { return _codecStream.DataBlock; } }
		public int FragmentIndex { get { return _codecStream.FragmentIndex; } }
		public bool IsFragmented { get { return _codecStream.IsFragmented; } }
		public IFragmentContainer FragmentContainer
		{
			get { return _codecStream.FragmentContainer; }
			set { }
		}
		public long ReferenceHeaderOffset { get { return _codecStream.ReferenceHeaderOffset; } }
		public IDataPacket ReferenceHeader { get { return _codecStream.ReferenceHeader; } }
		#endregion Properties

		public RescannedCodecStream(ICodecStream codecStream, IDataPacket data)
		{
			_codecStream = codecStream;
			_data = data;
		}

		public IDataPacket Append(IDataPacket dataPacket)
		{
			return _data.Append(dataPacket);
		}

		public IDataPacket GetSubPacket(long offset, long length)
		{
			return _data.GetSubPacket(offset, length);
		}

		public IDataPacket GetFragment(long offset)
		{
			return _data.GetFragment(offset);
		}
	}
}
