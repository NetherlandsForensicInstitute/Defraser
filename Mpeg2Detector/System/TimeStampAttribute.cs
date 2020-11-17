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

using Defraser.Detector.Common.Carver;

namespace Defraser.Detector.Mpeg2.System
{
	/// <summary>
	/// Describes a time stamp and possible extension.
	/// </summary>
	/// <typeparam name="TEnum">the enumeration type or <c>string</c></typeparam>
	internal class TimeStampAttribute<TEnum> : IAttributeParser<IMpeg2SystemReader>
	{
		private readonly TEnum _attributeName;
		private readonly bool _hasExtension;

		public TimeStampAttribute(TEnum attributeName, bool hasExtension)
		{
			_attributeName = attributeName;
			_hasExtension = hasExtension;
		}

		public void Parse(IMpeg2SystemReader reader, IAttributeState resultState)
		{
			resultState.Name = _attributeName;

			ulong bits32To30 = reader.GetBits(3, "Bits [32..30]");
			reader.GetMarker();
			ulong bits29To15 = reader.GetBits(15, "Bits [29..15]");
			reader.GetMarker();
			ulong bits14To0 = reader.GetBits(15, "Bits [14..0]");
			reader.GetMarker();

			ulong value = (bits32To30 << 30) | (bits29To15 << 15) | bits14To0;

			if (_hasExtension && reader.State.IsMpeg2())
			{
				ulong extension = reader.GetBits(9, "Extension");

				// TODO: what if extension >= 300 ?

				value = (300 * value) + extension;
			}

			resultState.Value = value;

			reader.State.LastTimestamp = value;
		}
	}
}
