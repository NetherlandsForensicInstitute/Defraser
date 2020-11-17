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
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg4
{
	public class Mpeg4Parser2 : Parser<Mpeg4Header, Mpeg4HeaderName,Mpeg4Reader>
	{
		
		/// <summary>
		/// Search for more headers and append them to <paramref name="firstHeader"/>.
		/// When a startcode is found for a header that could not be correctly parsed,
		/// try to find the next startcode.
		/// </summary>
		/// <param name="firstHeader"></param>
		public override void AppendHeaders(Mpeg4Header firstHeader)
		{
			int maxOffsetBetweenHeaders = (int)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.MaxOffsetBetweenHeaders];

			Mpeg4Header previousHeader = firstHeader;
			while (State == DataReaderState.Ready)
			{
				long nextHeaderMustBeFoundBeforeOffset = previousHeader.Offset + previousHeader.Length + maxOffsetBetweenHeaders;

				Mpeg4Header header;
				if ((header = GetNextHeader(previousHeader, nextHeaderMustBeFoundBeforeOffset)) != null && ParseAndAppend(header))
				{
					previousHeader = header;
				}
				else if (Position < nextHeaderMustBeFoundBeforeOffset)
				{
					if (header == null)
					{
						if (Position >= (Length - 1))
						{
							break;
						}
						Position++;
					}
					continue;
				}
				else
				{
					// Rewind to previous header and skip 1 byte
					Position = Math.Min((firstHeader.Offset + 1), Length);
					break;
				}
			}
		}
	}
}
