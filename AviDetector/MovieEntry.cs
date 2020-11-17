/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
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

namespace Defraser.Detector.Avi
{
	public enum InformationType : short
	{
		UncompressedVideoFrame = 0x6462,	// db
		CompressedVideoFrame = 0x6463,	// dc
		PaletteChanged = 0x7063,	// pc
		AudioData = 0x7762	// wb
	}

	public static class MovieEntryExtensionMethods
	{
		static public bool IsMovieEntry(this uint type)
		{
			char b1 = (char)((type >> 24) & 0xFF);
			char b2 = (char)((type >> 16) & 0xFF);
			short informationType = (short)type;

			if (b1.IsHexDigit() && b2.IsHexDigit() &&
				((informationType == (short)InformationType.UncompressedVideoFrame) ||
				 (informationType == (short)InformationType.CompressedVideoFrame) ||
				 (informationType == (short)InformationType.PaletteChanged) ||
				 (informationType == (short)InformationType.AudioData)))
			{
				return true;
			}
			return false;
		}
	}

	internal class MovieEntry : AviChunk
	{
		public new enum Attribute
		{
			Channel,
			InformationType,
			NumberOfBytesForCodecDetector
		}

		public IDataPacket StreamData { get; set; }
		public InformationType StreamType { get; set; }
		public byte Channel { get; set; }

		public MovieEntry(AviChunk previousHeader)
			: base(previousHeader, AviChunkName.MovieEntry)
		{
		}

		public override bool Parse(AviParser parser)
		{
			if (!base.Parse(parser)) return false;

			if ((uint)Size > MaxUnparsedBytes) return false;

			// Put the position back to the beginning of the header.
			// We want to be able the read the channel property.
			parser.Position -= 8;

			// Read the channel property (first 2 bytes of fourcc)
			if (!ParseChannel(parser)) return false;

			// Read the information type property (last 2 bytes of fourcc)
			ParseStreamType(parser);

			// Skip the 'Size' bytes
			parser.Position += 4;

			ulong size = Math.Min(Size, parser.BytesRemaining);
			if(size > 0)
			{
				StreamData = parser.GetDataPacket(parser.Position, (long)size);
			}

			// Show the number of bytes that go to the codec detector
			parser.AddAttribute(new FormattedAttribute<Attribute, ulong>(Attribute.NumberOfBytesForCodecDetector, parser.BytesRemaining));

			// Prevent AviChunk.ParseEnd() from displaying the unparsed bytes as hex dump
			parser.Position += (long)parser.BytesRemaining;

			return Valid;
		}

		private bool ParseChannel(AviParser parser)
		{
			byte AsciiValueOfCharacterZero = Convert.ToByte('0');
			byte channelByte0 = parser.GetByte();
			byte channelByte1 = parser.GetByte();

			if (!((char)channelByte0).IsHexDigit() || !((char)channelByte1).IsHexDigit())
			{
				this.Valid = false;
				return this.Valid;
			}

			Channel = (byte)((16 * (channelByte0-AsciiValueOfCharacterZero)) + (channelByte1-AsciiValueOfCharacterZero));

			parser.AddAttribute(new FormattedAttribute<Attribute, byte>(Attribute.Channel, Channel));

			return true;
		}

		private void ParseStreamType(AviParser parser)
		{
			byte streamTypeByte0 = parser.GetByte();
			byte streamTypeByte1 = parser.GetByte();

			StreamType = (InformationType)(((short)streamTypeByte0 << 8) | streamTypeByte1);

			parser.AddAttribute(new FormattedAttribute<Attribute, InformationType>(Attribute.InformationType, StreamType));
		}
	}
}
