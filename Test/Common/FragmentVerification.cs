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
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test.Common
{
	public class FragmentVerification
	{
		private readonly IResultNode _resultNode;
		private readonly IFragment _fragment;
		private readonly String _description;

		public FragmentVerification(IFragment fragment, IResultNode resultNode, String description)
		{
			_fragment = fragment;
			_resultNode = resultNode;
			_description = description;
		}

		public FragmentVerification IsFragmented()
		{
			return Fragmented(Is.EqualTo(true));
		}

		public FragmentVerification Fragmented(Constraint isFragmented)
		{
			Assert.That(_fragment.IsFragmented, isFragmented, "Is Fragmented of "+_description);
			return this;
		}

		public FragmentVerification Container(Constraint fragmentContainer)
		{
			Assert.That(_fragment.FragmentContainer, fragmentContainer, "fragmentContainer of "+_description);
			return this;
		}

		public FragmentVerification IsContained()
		{
			return Container(Is.Not.Null);
		}

		public FragmentVerification Index(Constraint fragmentedIndex)
		{
			Assert.That(_fragment.FragmentIndex, fragmentedIndex, "Fragment index of "+_description);
			return this;
		}

		public MetaDataVerification VerifyMetaData()
		{
			return new MetaDataVerification(_fragment, _description);
		}

		public DataPacketVerification VerifyDataPacket()
		{
			return new DataPacketVerification(_fragment, _description);
		}

		public ResultNodeVerification VerifyResultNode()
		{
			return new ResultNodeVerification(_resultNode,_description);
		}

		public FragmentVerification DataFormat(Constraint format)
		{
			VerifyMetaData().DataFormat(format);
			return this;
		}

		public FragmentVerification Length(Constraint length)
		{
			VerifyDataPacket().Length(length);
			return this;
		}
	}
}
