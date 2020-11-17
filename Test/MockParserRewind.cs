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

using System.Collections.Generic;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Test
{
	internal class MockParserRewind : Parser<MockBrokenHeader, MockBrokenHeaderName, MockParserRewind>
	{
		private readonly List<long> _firstHeaderStartPositions = new List<long>();

		public IList<long> FirstHeaderStartPositions { get { return _firstHeaderStartPositions.AsReadOnly(); } }

		public MockParserRewind(IDataReader dataReader)
			: base(dataReader)
		{
		}

		public override MockBrokenHeader FindFirstHeader(MockBrokenHeader root, long offsetLimit)
		{
			_firstHeaderStartPositions.Add(Position);
			return new MockBrokenHeader(root);
		}

		public override MockBrokenHeader GetNextHeader(MockBrokenHeader previousHeader, long offsetLimit)
		{
			return null;
		}

		public int GetInt<T>(T attributeName)
		{
			if (Position > (Length - 4))
			{
				Position = Length;
				return 0;
			}

			// Read big-endian integer
			byte[] b = new byte[4];
			Read(b, 0, 4);
			Position += 4;
			int value = (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[0];

			// Add attribute
			AddAttribute(new FormattedAttribute<T, int>(attributeName, value));
			return value;
		}
	}
}
