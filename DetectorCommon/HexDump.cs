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
using System.Globalization;
using System.Text;

namespace Defraser.Detector.Common
{
	public class HexDump<TEnum, THeader, THeaderName, TParser> : CompositeAttribute<TEnum, string, TParser>
		where THeader : Header<THeader, THeaderName, TParser>
		where TParser : Parser<THeader, THeaderName, TParser>
	{
		private enum LAttribute { Unparsed, BytesNotDisplayedCount }

		private readonly long _totalBytesToReadCount;
		private readonly long _maxBytesOnDisplayCount;

		public HexDump(TEnum attributeName, int totalBytesToReadCount, int maxBytesOnDisplayCount)
			: base(attributeName, string.Empty, "{0}")
		{
			_totalBytesToReadCount = totalBytesToReadCount;
			_maxBytesOnDisplayCount = maxBytesOnDisplayCount;
		}

		public override bool Parse(TParser parser)
		{
			this.TypedValue = string.Format(CultureInfo.CurrentCulture, "Number of bytes: {0}", _totalBytesToReadCount);

			const int BytesPerRow = 16;
			int totalNumBytesRead = 0;

			while (totalNumBytesRead < _totalBytesToReadCount && totalNumBytesRead < _maxBytesOnDisplayCount)
			{
				int numBytesRead;
				string hexDumpLine = GetHexDumpLine(parser, (int)(_totalBytesToReadCount - totalNumBytesRead), BytesPerRow, out numBytesRead);
				if (numBytesRead == 0)
					break;	// Break out of the while loop if no bytes were read.

				parser.AddAttribute(new FormattedAttribute<LAttribute, string>(LAttribute.Unparsed, hexDumpLine));

				totalNumBytesRead += numBytesRead;
			}
			if (totalNumBytesRead < _totalBytesToReadCount)
			{
				long bytesNotDisplayedCount = _totalBytesToReadCount - totalNumBytesRead;
				long maxBytesToSkipCount = parser.Length - parser.Position;
				parser.Position += Math.Min(bytesNotDisplayedCount, maxBytesToSkipCount);
				parser.AddAttribute(new FormattedAttribute<LAttribute, long>(LAttribute.BytesNotDisplayedCount, bytesNotDisplayedCount));
			}
			return Valid;
		}

		private string GetHexDumpLine(TParser parser, int bytesToReadCount, int maxBytesOnDisplayCount, out int bytesRead)
		{
			bytesRead = 0;

			StringBuilder byteStringBuilder = new StringBuilder();
			StringBuilder charStringBuilder = new StringBuilder();

			for (int i = 0; i < maxBytesOnDisplayCount; i++)
			{
				if (i < bytesToReadCount)
				{
					byte[] buf = new byte[1];
					int bytesReadCount = parser.Read(buf, 0, 1);
					if(bytesReadCount ==0)
					{
						Valid = false;
						break;
					}
					byte b = buf[0];
					bytesRead++;
					parser.Position++;
					byteStringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0:X02}", b));
					charStringBuilder.Append(Char.IsControl((char)b) ? '.' : (char)b);
				}
				else
				{
					byteStringBuilder.Append("  ");
					charStringBuilder.Append(" ");
				}
				if ((i != 0) && (((i + 1) % 2) == 0))
				{
					byteStringBuilder.Append(' ');
				}
			}

			byteStringBuilder.Append("|" + charStringBuilder);

			return byteStringBuilder.ToString();
		}
	}
}
