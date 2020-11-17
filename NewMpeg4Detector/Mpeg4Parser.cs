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
using System.Globalization;
using Defraser.Detector.Common;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Mpeg4
{
	public class Mpeg4Parser : BitStreamParser<Mpeg4Header, Mpeg4HeaderName, Mpeg4Parser>
	{
		private static readonly MP4Vlc1[] DcLuminanceTable = new MP4Vlc1[] { new MP4Vlc1(0, 0), new MP4Vlc1(4, 3), new MP4Vlc1(3, 3), new MP4Vlc1(0, 3), new MP4Vlc1(2, 2), new MP4Vlc1(2, 2), new MP4Vlc1(1, 2), new MP4Vlc1(1, 2) };

		// Table for Motion Vector
		internal protected static readonly MP4Vlc1[] mp4_MVD_B12_1 = new MP4Vlc1[] {
			new MP4Vlc1 (32,12), new MP4Vlc1 (31,12),
			new MP4Vlc1 (30,11),new MP4Vlc1 (30,11),new MP4Vlc1 (29,11),new MP4Vlc1 (29,11),new MP4Vlc1 (28,11),new MP4Vlc1 (28,11),
			new MP4Vlc1 (27,11),new MP4Vlc1 (27,11),new MP4Vlc1 (26,11),new MP4Vlc1 (26,11),new MP4Vlc1 (25,11),new MP4Vlc1 (25,11),
			new MP4Vlc1 (24,10),new MP4Vlc1 (24,10),new MP4Vlc1 (24,10),new MP4Vlc1 (24,10),new MP4Vlc1 (23,10),new MP4Vlc1 (23,10),new MP4Vlc1 (23,10),new MP4Vlc1 (23,10),
			new MP4Vlc1 (22,10),new MP4Vlc1 (22,10),new MP4Vlc1 (22,10),new MP4Vlc1 (22,10),new MP4Vlc1 (21,10),new MP4Vlc1 (21,10),new MP4Vlc1 (21,10),new MP4Vlc1 (21,10),
			new MP4Vlc1 (20,10),new MP4Vlc1 (20,10),new MP4Vlc1 (20,10),new MP4Vlc1 (20,10),new MP4Vlc1 (19,10),new MP4Vlc1 (19,10),new MP4Vlc1 (19,10),new MP4Vlc1 (19,10),
			new MP4Vlc1 (18,10),new MP4Vlc1 (18,10),new MP4Vlc1 (18,10),new MP4Vlc1 (18,10),new MP4Vlc1 (17,10),new MP4Vlc1 (17,10),new MP4Vlc1 (17,10),new MP4Vlc1 (17,10),
			new MP4Vlc1 (16,10),new MP4Vlc1 (16,10),new MP4Vlc1 (16,10),new MP4Vlc1 (16,10),new MP4Vlc1 (15,10),new MP4Vlc1 (15,10),new MP4Vlc1 (15,10),new MP4Vlc1 (15,10),
			new MP4Vlc1 (14,10),new MP4Vlc1 (14,10),new MP4Vlc1 (14,10),new MP4Vlc1 (14,10),new MP4Vlc1 (13,10),new MP4Vlc1 (13,10),new MP4Vlc1 (13,10),new MP4Vlc1 (13,10),
			new MP4Vlc1 (12,10),new MP4Vlc1 (12,10),new MP4Vlc1 (12,10),new MP4Vlc1 (12,10),new MP4Vlc1 (11,10),new MP4Vlc1 (11,10),new MP4Vlc1 (11,10),new MP4Vlc1 (11,10),
			new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),new MP4Vlc1 (10, 9),
			new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),new MP4Vlc1 ( 9, 9),
			new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),new MP4Vlc1 ( 8, 9),
			new MP4Vlc1 (0x7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),
			new MP4Vlc1 (0x7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),
			new MP4Vlc1 (0x7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),
			new MP4Vlc1 (0x7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7),new MP4Vlc1 ( 7, 7)
		};

		// Table for Motion Vector
		internal protected static readonly MP4Vlc1[] mp4_MVD_B12_2 = new MP4Vlc1[] {
			new MP4Vlc1 ( 6, 7),new MP4Vlc1 ( 5, 7),new MP4Vlc1 ( 4, 6),new MP4Vlc1 ( 4, 6),
			new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),new MP4Vlc1 ( 3, 4),
			new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),
			new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),new MP4Vlc1 ( 2, 3),
			new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),
			new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),
			new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),
			new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),new MP4Vlc1 ( 1, 2),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),
			new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1),new MP4Vlc1 ( 0, 1)
		};

		private const int Mpeg4StartCodeLength = 4;

		private long _referenceHeaderPosition;
		private bool _referenceHeadersTested;
		private IDataPacket _referenceHeader;

		#region Properties
		internal Pair<byte, long> BitPosition
		{
			get { return DataReader.BitPosition; }
			set { DataReader.BitPosition = value; }
		}

		internal bool SeenVop { get; set; }
		internal IDictionary<IDataPacket, VideoObjectLayer> ReferenceHeaders { private get; set; }
		internal IDataPacket ReferenceHeader { get { return _referenceHeader; } }
		internal long ReferenceHeaderPosition { get { return _referenceHeaderPosition; } }
		#endregion Properties

		public Mpeg4Parser(BitStreamDataReader dataReader)
			: base(dataReader)
		{
			ReferenceHeaders = new Dictionary<IDataPacket, VideoObjectLayer>();
		}

		public override Mpeg4Header FindFirstHeader(Mpeg4Header root, long offsetLimit)
		{
			SeenVop = false;
			_referenceHeaderPosition = 0;
			_referenceHeader = null;
			_referenceHeadersTested = false;

			long headerOffsetLimit = Math.Min(offsetLimit, (Length - Mpeg4StartCodeLength));
			uint headerStartCode;

			while ((headerStartCode = NextHeaderStartCode(null, headerOffsetLimit)) != 0)
			{
				Mpeg4Header header = CreateHeader(root, headerStartCode);

				if (header != null)
				{
					return header;
				}
				Position++;
			}
			return null;
		}

		/// <summary>
		/// Finds and returns the (known) type of the next header.
		/// The next header start code is the (known) start code of the next header.
		/// The position of the data reader is set to the begin of the start code.
		/// </summary>
		/// <param name="previousHeader">the directly preceeding header</param>
		/// <param name="headerOffsetLimit">the maximum offset for the next header</param>
		/// <returns>the type (start code) of the next header, <code>0</code> for none</returns>
		private uint NextHeaderStartCode(Mpeg4Header previousHeader, long headerOffsetLimit)
		{
			if ((State != DataReaderState.Ready) || (Position > Math.Min(headerOffsetLimit, (Length - 4))))
			{
				return 0U;
			}

			byte[] buf = new byte[4];

			// Try to read start codes of at least 3 bytes
			long searchEndPosition = Math.Min(Length - 3, headerOffsetLimit);
			while ((Position <= searchEndPosition) && (State == DataReaderState.Ready))
			{
				int bytesRead = Read(buf, 0, 4);

				// If there are only 3 bytes available, force short header
				bool shortVideoHeader = ((buf[2] & 0xFC) == 0x80);
				if (bytesRead <= 3 && !shortVideoHeader)
					break;


				// Check for valid start code
				if (buf[0] == 0 && buf[1] == 0 && (buf[2] == 1 /*MPEG-4*/ || shortVideoHeader /*H263*/))
				{
					uint startCode = buf[2];
					startCode <<= 8;
					startCode += buf[3];

					if (startCode.IsKnownHeaderStartCode(previousHeader))
					{
						return startCode;
					}
				}
				int skipBytes = (buf[2] >= 2 && (buf[2] & 0xFC) < 0x80) ? 3 : (buf[1] == 0 && buf[2] == 0 && buf[3] != 0) ? 1 : 2;

				Position = Math.Min((Position + skipBytes), Length);
			}
			return 0U;
		}

		public override Mpeg4Header GetNextHeader(Mpeg4Header previousHeader, long offsetLimit)
		{
			if (Position > (Length - Mpeg4StartCodeLength))
			{
				return null;
			}

			// Try to read the directly succeeding header
			uint nextHeaderStartCode = NextHeaderStartCode(previousHeader, offsetLimit);

			return CreateHeader(previousHeader, nextHeaderStartCode);
		}

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

		internal uint GetStartCode(bool shortStartCode)
		{
			if (!CheckRead(shortStartCode ? 3 : 4)) return 0;

			return GetBits(shortStartCode ? 22 /*ShortHeaderLengthInBits*/ : 32 /*HeaderLengthInBits*/);
		}

		internal uint GetStartCode<T>(T attributeName, bool shortStartCode)
		{
			if (!CheckRead(shortStartCode ? 3 : 4)) return 0;

			uint value = GetStartCode(shortStartCode);
			AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.InvariantCulture,"0x{0}", value.ToString(shortStartCode ? "X06" : "X08", CultureInfo.InvariantCulture))));
			return value;
		}

		/// <summary>
		/// Perform byte alignment. When there is already byte alignment,
		/// read the next byte when it has the value of 0x7F.
		/// </summary>
		internal void AlignBits7F()
		{
			if (DataReader.ByteAligned)
			{
				if (ShowBits(8) == 0x7F)
				{
					GetByte();
				}
			}
			else
			{
				ByteAlign();
			}
		}

		protected override uint MaxZeroByteStuffingLength
		{
			get { return (byte)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.ParserMaxZeroByteStuffingLength]; }
		}

		protected override bool IsStartCode()
		{
			return (ShowBits(24) == 1 ||	// Normal header
					ShowBits(22) == 0x20);	// Short (Picture) header
		}

		private static Mpeg4Header CreateHeader(Mpeg4Header previousHeader, uint startCode)
		{
			Mpeg4HeaderName headerName = startCode.GetHeaderName();

			switch (headerName)
			{
				case Mpeg4HeaderName.Extension: return new Extension(previousHeader);
				case Mpeg4HeaderName.FbaObject: return new FbaObject(previousHeader);
				case Mpeg4HeaderName.FbaObjectPlane: return new FbaObjectPlane(previousHeader);
				case Mpeg4HeaderName.FgsBp: return new FgsBp(previousHeader);
				case Mpeg4HeaderName.FgsVop: return new FgsVOP(previousHeader);
				case Mpeg4HeaderName.GroupOfVop: return new GroupOfVop(previousHeader);
				case Mpeg4HeaderName.MeshObject: return new MeshObject(previousHeader);
				case Mpeg4HeaderName.MeshObjectPlane: return new MeshObjectPlane(previousHeader);
				case Mpeg4HeaderName.VopWithShortHeader: return new VopWithShortHeader(previousHeader);
				case Mpeg4HeaderName.Slice: return new Slice(previousHeader);
				case Mpeg4HeaderName.StillTextureObject: return new StillTextureObject(previousHeader);
				case Mpeg4HeaderName.Stuffing: return new Stuffing(previousHeader);
				case Mpeg4HeaderName.TextureShapeLayer: return new TextureShapeLayer(previousHeader);
				case Mpeg4HeaderName.TextureSnrLayer: return new TextureSnrLayer(previousHeader);
				case Mpeg4HeaderName.TextureSpatialLayer: return new TextureSpatialLayer(previousHeader);
				case Mpeg4HeaderName.TextureTile: return new TextureTile(previousHeader);
				case Mpeg4HeaderName.UserData: return new UserData(previousHeader);
				case Mpeg4HeaderName.VideoObject: return new VideoObject(previousHeader);
				case Mpeg4HeaderName.VideoObjectLayer: return new VideoObjectLayer(previousHeader);
				//case Mpeg4HeaderName.VideoSessionError:
				case Mpeg4HeaderName.VisualObject: return new VisualObject(previousHeader);
				case Mpeg4HeaderName.VisualObjectSequenceEnd: return new VisualObjectSequenceEnd(previousHeader);
				case Mpeg4HeaderName.VisualObjectSequenceStart: return new VisualObjectSequenceStart(previousHeader);
				case Mpeg4HeaderName.Vop: return new Vop(previousHeader);
			}
			return null;
		}

		/// <summary>
		/// Valid stuffing:
		/// 0 (1bit)
		/// 01
		/// 011
		/// ...
		/// 0111111 (7bits)
		///
		/// When byte aligned, the next byte must contain the value
		/// '01111111' to be valid stuffing.
		///
		/// Stuffing is followed by a startcode. This method checks for the
		/// existance of this code.
		/// </summary>
		/// <returns></returns>
		internal bool ValidStuffing()
		{
			// Already byte aligned, next byte must be 0x7F for valid byte stuffing
			if (ByteAligned)
			{
				return ShowBits(8) == 0x7F;
			}

			// if the first != zero, no valid stuffing
			if (ShowBit() == true) return false;

			// Check the remaining bits for stuffing
			uint stuffing = ShowBits(8 - DataReader.BitOffset); // get the remaining bits for this byte
			// 0x3F || 0x1F || 0x0F || 0x07 || 0x3 || 0x01
			//if (stuffing == 0) return false;
			if ((stuffing & (stuffing + 1)) == 0) return true;

			return false;
		}

		/// <summary>
		/// From ISO-IEC 14496-2-2004-MPEG-4-Video, page 17:
		/// The function nextbits_bytealigned() returns a bit string starting from
		/// the next byte aligned position. This permits comparison of a bit string
		/// with the next byte aligned bits to be decoded in the bitstream. If the
		/// current location in the bitstream is already byte aligned and the 8 bits
		/// following the current location are ‘01111111’, the bits subsequent to
		/// these 8 bits are returned. The current location in the bitstream is not
		/// changed by this function.
		/// </summary>
		internal uint ShowNextBitsByteAligned(int numBits)
		{
			uint nextBitsByteAligned;

			// Store the current location
			Pair<byte, long> bitPosition = BitPosition;

			if (ByteAligned && ShowBits(8) == 0x7F)
			{
				FlushBits(8);

				nextBitsByteAligned = GetBits(numBits);
			}
			else
			{
				ByteAlign();

				nextBitsByteAligned = GetBits(numBits);
			}

			// Restore the location
			BitPosition = bitPosition;

			return nextBitsByteAligned;
		}

#if DEBUG
		internal void BreakAtOffset()
		{
			DataReader.BreakAtOffset();
		}
#endif // DEBUG

		/// <summary>
		/// See page 17 of ISO-IEC 14496-2-2004-MPEG-4-Video
		/// The next_resync_marker() function removes any zero bit and a string
		/// of 0 to 7 ‘1’ bits used for stuffing and locates the next resync
		/// marker; it thus performs similar operation as next_start_code() but
		/// for resync_marker.
		/// </summary>
		/// <returns></returns>
		internal bool NextResyncMarker()
		{
			if (!GetZeroBit()) return false;

			while (!ByteAligned)
			{
				if (!GetOneBit()) return false;
			}
			return true;
		}

		internal int GetDCSizeLum()
		{
			int code = (int)ShowBits(11);

			for (short i = 11; i > 3; i--)
			{
				if (code == 1)
				{
					FlushBits(i);
					return i + 1;
				}
				code >>= 1;
			}

			FlushBits((short)DcLuminanceTable[code].len);
			return (int)DcLuminanceTable[code].code;
		}

		internal int GetDCSizeChrom()
		{
			int code = (int)ShowBits(12);
			for (short i = 12; i > 2; i--)
			{
				if (code == 1)
				{
					FlushBits(i);
					return i;
				}
				code >>= 1;
			}

			return (int)(3 - GetBits(2));
		}

		internal void GetLuminanceChrominance(int index)
		{
			if (index < 4)
			{
				int luminance = GetDCSizeLum();
				if (luminance != 0)
				{
					GetBits((short)luminance);
					if (luminance > 8)
					{
						GetMarkerBit();
					}
				}
			}
			else
			{
				int chrominance = GetDCSizeChrom();
				if (chrominance != 0)
				{
					GetBits((short)chrominance);
					if (chrominance > 8)
					{
						GetMarkerBit();
					}
				}
			}
		}

		internal void GetMotionVector(uint fcode)
		{
			/* decode MVDx */
			MP4Vlc1 tab;
			uint factor = fcode - 1;
			uint code = ShowBits(12);
			if (code >= 128)
			{
				tab = mp4_MVD_B12_2[((code - 128) >> 5)];
			}
			else if (code >= 2)
			{
				tab = mp4_MVD_B12_1[(code - 2)];
			}
			else
			{
				return;
			}
			FlushBits((short)tab.len);
			if (tab.code != 0)
			{
				GetBit();
				if (factor != 0)
				{
					code = GetBits((short)factor);
				}
			}

			/* decode MVDy */
			code = ShowBits(12);
			if (code >= 128)
			{
				tab = mp4_MVD_B12_2[((code - 128) >> 5)];
			}
			else if (code >= 2)
			{
				tab = mp4_MVD_B12_1[(code - 2)];
			}
			else
			{
				return;
			}
			FlushBits((short)tab.len);
			if (tab.code != 0)
			{
				GetBit();
				if (factor > 0)
				{
					GetBits((short)factor);
				}
			}
		}

		internal bool TryDefaultHeaders(Func<VideoObjectLayer, bool> evaluateHeader)
		{
			if (_referenceHeadersTested)
			{
				return false;
			}

			// This will make sure the default headers are tested just once and not for every VOP
			_referenceHeadersTested = true;

			if (ReferenceHeaders.Count == 0)
			{
				return false;
			}

			long startPosition = Position;

			// Try reference headers to decode this VOP
			foreach (var referenceHeader in ReferenceHeaders)
			{
				if (evaluateHeader(referenceHeader.Value))
				{
					_referenceHeader = referenceHeader.Key;
					return true; // Header successfully decoded!
				}

				Position = startPosition;
			}

			// Not able to decode VOP using any reference header, defaulting to unparsable VOPs

			return false;
		}

		internal void InsertReferenceHeaderBeforeStartCode()
		{
			if (!_referenceHeadersTested && (ReferenceHeaders.Count >= 1))
			{
				_referenceHeaderPosition = (Position - 4);
			}
		}
	}
}
