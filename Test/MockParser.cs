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
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Test
{
	public class MockParser : Parser<MockHeader, MockHeaderName, MockParser>
	{
		public MockParser(IDataReader dataReader)
			: base(dataReader)
		{
		}

		public int GetTenBytes<T>(T attributeName)
		{
			int numBytes = (int)Math.Min(10, (Length - Position));
			Position += numBytes;
			FormattedAttribute<T, int> attribute = new FormattedAttribute<T, int>(attributeName, numBytes);
			attribute.Valid = (numBytes == 10);
			AddAttribute(attribute);
			return numBytes;
		}

		public override MockHeader FindFirstHeader(MockHeader root, long offsetLimit)
		{
			if (Position >= offsetLimit) return null;

			MockHeader header = new MockHeader(root, MockHeaderName.MockHeaderTypeOne);
			header.Attributes.Add(new FormattedAttribute<string, long>("OffsetLimit", offsetLimit));
			return header;
		}

		public override MockHeader GetNextHeader(MockHeader previousHeader, long offsetLimit)
		{
			if (Position > 4)
			{
				return null;
			}
			return new MockHeader(previousHeader, MockHeaderName.MockHeaderTypeTwo);
		}
	}
}
