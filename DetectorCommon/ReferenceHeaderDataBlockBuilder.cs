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
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	public sealed class ReferenceHeaderDataBlockBuilder : IDataBlockBuilder
	{
		#region Inner classes
		private sealed class ReferenceHeaderDataBlock : IDataBlock
		{
			#region Properties
			public CodecID DataFormat { get; private set; }
			public IEnumerable<IDetector> Detectors { get { return Enumerable.Empty<IDetector>(); } }
			public IInputFile InputFile { get { return null; } }
			public long Length { get { return 0L; } }
			public long StartOffset { get { return 0L; } }
			public long EndOffset { get; private set; }
			public bool IsFragmented { get { return false; } }
			public int FragmentIndex { get { return 0; } }
			public IFragmentContainer FragmentContainer { get; set; }
			public bool IsFullFile { get { return false; } }
			public long ReferenceHeaderOffset { get { return 0; } }
			public IDataPacket ReferenceHeader { get { return null; } }
			public IList<ICodecStream> CodecStreams { get { return new List<ICodecStream>(); } }
			#endregion Properties

			internal ReferenceHeaderDataBlock(ReferenceHeaderDataBlockBuilder builder)
			{
				DataFormat = builder.DataFormat;
				EndOffset = builder.EndOffset;
			}

			public IDataPacket Append(IDataPacket dataPacket)
			{
				return this;
			}

			public IDataPacket GetSubPacket(long offset, long length)
			{
				return this;
			}

			public IDataPacket GetFragment(long offset)
			{
				return this;
			}
		}
		#endregion Inner classes

		#region Properties
		public CodecID DataFormat { private get; set; }
		public IEnumerable<IDetector> Detectors { set { } }
		public IInputFile InputFile { set { } }
		public long StartOffset { set { } }
		public long EndOffset { private get; set; }
		public bool IsFullFile { set { } }
		public bool IsFragmented { set { } }
		public IFragment PreviousFragment { set { } }
		public long ReferenceHeaderOffset { set { } }
		public IDataPacket ReferenceHeader { set { } }
		#endregion Properties

		public ICodecStreamBuilder AddCodecStream()
		{
			throw new NotImplementedException();
		}

		public IDataBlock Build()
		{
			return new ReferenceHeaderDataBlock(this);
		}
	}
}
