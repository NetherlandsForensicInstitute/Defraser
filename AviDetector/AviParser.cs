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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Avi
{
	public class AviParser : ByteStreamParser<AviChunk, AviChunkName, AviParser>
	{
		#region Properties
		public ulong BytesRemaining
		{
			get
			{
				AviChunk chunk = Result as AviChunk;
				Debug.Assert(chunk != null);
				return Math.Min((ulong)Length, chunk.RelativeEndOffset) - (ulong)Position;
			}
		}
		#endregion Properties

		public AviParser(ByteStreamDataReader dataReader)
			: base(dataReader)
		{
		}

		public override AviChunk FindFirstHeader(AviChunk root, long offsetLimit)
		{
			long chunkOffsetLimit = Math.Min(offsetLimit, Length - 8);
			uint chunkType;

			while ((chunkType = NextChunkType(chunkOffsetLimit, false)) != 0)
			{
				AviChunk header = CreateChunk(root, chunkType);

				if ((header != null                           ) &&
					(header.HeaderName != AviChunkName.Unknown)    )
				{
					return header;
				}
			}
			return null;
		}

		public override AviChunk GetNextHeader(AviChunk previousHeader, long offsetLimit)
		{
			const int NextHeaderLength = 4;

			if (Position > (Length - NextHeaderLength))
			{
				return null;
			}

			// Try to read the directly succeeding chunk
			const int SizeOfPaddingByte = 1;
			uint nextChunkType = NextChunkType(Position, true);

			// Chunks are padded to 2*n offset.
			// This may be true in this case.
			if (!nextChunkType.IsKnownChunkType() && ((Position + SizeOfPaddingByte) < Length))
			{
				if(nextChunkType == 0)
				{
					Position -= NextHeaderLength;
				}
				Position += SizeOfPaddingByte;
				uint nextChunkTypeAfterPaddingByte = NextChunkType(Position, true);
				Position -= SizeOfPaddingByte;

				if (nextChunkTypeAfterPaddingByte.IsKnownChunkType() ||
					(nextChunkTypeAfterPaddingByte.IsValid4CC() && nextChunkType == 0))
				{
					return new PaddingByte(previousHeader);
				}
				if (nextChunkType == 0 && nextChunkTypeAfterPaddingByte == 0)
				{
					return null;
				}
				if (nextChunkTypeAfterPaddingByte == 0)
				{
					Position -= NextHeaderLength;
				}
			}
			return CreateChunk(previousHeader, nextChunkType);
		}

		/// <summary>
		/// Reads a FourCC code from the data stream and adds a new attribute.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the FourCC value</returns>
		public uint GetFourCC<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0;

			uint value = (uint)DataReader.GetInt(Endianness.Big); // The Four CC is Big Endian!
			AddAttribute(new FourCCAttribute<T>(attributeName, value));
			return value;
		}

		public uint GetFrameAspectRatio<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0;

			uint value = (uint)DataReader.GetInt();
			AddAttribute(new FrameAspectRatio<T>(attributeName, value));
			return value;
		}

		public uint GetFourCCWithOptionalDescriptionAttribute<T1, T2>(T1 attributeName, Dictionary<uint, T2> optionPairs)
		{
			if (!CheckRead(4)) return 0;

			uint fourCC = (uint)DataReader.GetInt();
			AddAttribute(new FourCCWithOptionalDescriptionAttribute<T1, T2>(attributeName, fourCC, optionPairs));
			return fourCC;
		}

		/// <summary>
		/// Reads a string from the data stream and adds a new attribute.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="bytesToRead">the number of bytes to read</param>
		/// <returns>the string value</returns>
		public string GetString<T>(T attributeName, ulong bytesToRead)
		{
			if (!CheckRead((int)bytesToRead) || bytesToRead == 0) return string.Empty;

			uint maxAttributeStringLength = (uint)AviDetector.Configurable[AviDetector.ConfigurationKey.AttributeMaxStringLength];

			// TODO use more than one line to display the string. Use /r/n formating from the string or break on line length.
			StringBuilder stringBuilder = new StringBuilder();
			ulong l = 0;
			while (l++ < bytesToRead && l < maxAttributeStringLength)
			{
				char c = (char)DataReader.GetByte();
				stringBuilder.Append(c);
			}
			string value = stringBuilder.ToString();
			AddAttribute(new FormattedAttribute<T, string>(attributeName, value));
			return value;
		}

		public void GetFrame<T>(T attributeName, FrameFieldType frameFieldType)
		{
			if (!CheckRead(8)) return;

			Parse(new Frame<T>(attributeName, frameFieldType));
		}

		/// <summary>
		/// Finds and returns the (known) type of the next chunk.
		/// The next chunk type is the (known) type of the next chunk.
		/// </summary>
		/// <param name="chunkOffsetLimit">the maximum offset for the next chunk</param>
		/// <param name="allowUnknownChunks">if <code>true</code>, allow unknown chunks</param>
		/// <returns>the type (4CC) of the next chunk, <code>0</code> for none</returns>
		private uint NextChunkType(long chunkOffsetLimit, bool allowUnknownChunks)
		{
			if ((State != DataReaderState.Ready) || (Position > Math.Min(chunkOffsetLimit, (Length - 8))))
			{
				return 0;
			}

			// GetFourCC
			uint type = (uint)DataReader.GetInt(Endianness.Big);

			// Try to read the next chunk header
			while (State != DataReaderState.Cancelled)
			{
				long offset = Position - sizeof(uint);

				// Validate the chunk type
				if (type.IsKnownChunkType() || (allowUnknownChunks && type.IsValid4CC()))
				{
					Position = offset;
					return type;
				}
				if (offset >= chunkOffsetLimit)
				{
					break;
				}

				// Shift buffer, read next byte
				type = (type << 8) | DataReader.GetByte();
			}
			return 0;
		}

		private static AviChunk CreateChunk(AviChunk previousHeader, uint startCode)
		{
			if (startCode.IsMovieEntry())
			{
				return new MovieEntry(previousHeader);
			}

			AviChunkName chunkName = startCode.GetChunkName();

			switch (chunkName)
			{
				case AviChunkName.Riff: return new Riff(previousHeader);
				case AviChunkName.HeaderList: return new HeaderList(previousHeader);
				case AviChunkName.AviMainHeader: return new AviMainHeader(previousHeader);
				case AviChunkName.AviStreamHeader: return new AviStreamHeader(previousHeader);
				case AviChunkName.AviStreamFormat: return new AviStreamFormat(previousHeader);
				case AviChunkName.IndexChunk: return new IndexChunk(previousHeader);
				case AviChunkName.ExtendedAviHeader: return new ExtendedAviHeader(previousHeader);
				case AviChunkName.DigitizingDate: return new DigitizingDate(previousHeader);
				case AviChunkName.VideoPropertiesHeader: return new VideoPropertiesHeader(previousHeader);
				case AviChunkName.AviStreamIndex: return new AviStreamIndex(previousHeader);
				case AviChunkName.AviStreamName: return new AviStreamName(previousHeader);
			}
			return new AviChunk(previousHeader, chunkName);
		}
	}
}
