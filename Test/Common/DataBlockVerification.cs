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

using System;
using System.Collections.Generic;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Common
{
	public class DataBlockVerification : IFragmentedVerification
	{
		private readonly IDataBlock _dataBlock;
		private IResultNode _resultNode;
		private readonly string _description;

		public DataBlockVerification(IDataBlock block, IResultNode resultNode, String description)
		{
			_dataBlock = block;
			_resultNode = resultNode;
			_description = description;
		}

		public CodecStreamVerification[] CodecStreams
		{
			get { return new List<ICodecStream>(_dataBlock.CodecStreams).ConvertAll(stream =>
			                                                                        	{
			                                                                        		var results = TestFramework.GetResults(stream);
			                                                                        		return new CodecStreamVerification(stream, results, TestFramework.GetDescriptionText(results));
			                                                                        	}).ToArray(); }
		}

		public IResultNode ResultNode
		{
			get
			{
				return _resultNode ?? (_resultNode=TestFramework.GetResults(_dataBlock));
			}
		}

		public IDataBlock DataBlock
		{
			get {
				return _dataBlock;
			}
		}

		public DataBlockVerification CodecStreamCount(Constraint streamCount)
		{
			Assert.That(_dataBlock.CodecStreams.Count, streamCount, "Number of streams in "+_description);
			return this;
		}

		public DataBlockVerification CodecStreamCountEquals(int streamCount)
		{
			return CodecStreamCount(Is.EqualTo(streamCount));
		}

		public MetaDataVerification VerifyMetaData()
		{
			return new MetaDataVerification(_dataBlock, _description);
		}

		public DataPacketVerification VerifyDataPacket()
		{
			return new DataPacketVerification(_dataBlock, _description);
		}

		public FragmentVerification VerifyFragmentation()
		{
			return new FragmentVerification(_dataBlock, _resultNode, _description);
		}

		public CodecStreamVerification VerifyCodecStream(int index)
		{
			var stream = _dataBlock.CodecStreams[index];
			var result = TestFramework.GetResults(stream);
			return new CodecStreamVerification(stream,result,"Stream "+stream.StreamNumber+"("+stream.Name+") from "+_description+", with Results:"+Environment.NewLine+TestFramework.GetDescriptionText(result));
		}

		public DataBlockVerification StartOffset(Constraint offset)
		{
			VerifyDataPacket().StartOffset(offset);
			return this;
		}

		public DataBlockVerification DataFormat(Constraint dataFormat)
		{
			VerifyMetaData().DataFormat(dataFormat);
			return this;
		}

		public DataBlockVerification Length(Constraint length)
		{
			VerifyDataPacket().Length(length);
			return this;
		}

		public ResultNodeVerification VerifyResultNode()
		{
			return new ResultNodeVerification(ResultNode, _description);
		}
	}
}
