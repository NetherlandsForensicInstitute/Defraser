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
using System.IO;
using System.IO.Compression;

namespace Defraser.Detector.QT
{
	internal class CompressedMovieData : QtAtom
	{
		public new enum Attribute
		{
			UncompressedSize
		}

		public CompressedMovieData(QtAtom previousHeader)
			: base(previousHeader, AtomName.CompressedMovieData)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			uint uncompressedSize = parser.GetUInt(Attribute.UncompressedSize);

			// TODO: finish decompression method and parse the decompressed movie atom
			//byte[] compressedData = new byte[parser.BytesRemaining];
			//parser.DataReader.Read(compressedData, 0, compressedData.Length);

			//DecompressZlib(compressedData, uncompressedSize);

			return this.Valid;
		}

		/// <summary>
		/// Decompressed a zlib-compressed movie atom.
		/// </summary>
		/// <param name="compressedData">the zlib-compressed data</param>
		/// <param name="uncompressedSize">the original size of the movie atom</param>
		/// <returns>the decompressed movie atom bytes</returns>
		private static byte[] DecompressZlib(byte[] compressedData, uint uncompressedSize)
		{
			byte[] b2 = new byte[uncompressedSize];

			Stream stream;
			if (compressedData[0] == 0x78 && compressedData[1] == 0x9C)
			{
				// Skip 2-byte zlib header
				stream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
			}
			else
			{
				stream = new MemoryStream(compressedData);
			}

			using (stream)
			{
				// TODO in the data compression atom you can find what
				// lossless data compression algorithm was used to compress
				// the movie resource.
				using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
				{
					try
					{
						int pos = 0;
						while (deflateStream.CanRead && pos < b2.Length)
						{
							b2[pos++] = (byte)deflateStream.ReadByte();
						}
					}
					catch (InvalidDataException e)
					{
						Console.WriteLine(e.ToString());
					}
				}
			}

			// Dump decompressed data to console out
			for (int i = 0; i < b2.Length; i++)
			{
				Console.Write(" {0:X2}", b2[i]);

				if ((i % 16) == 15)
				{
					Console.Write(" | ");
					for (int j = i - 15; j <= i; j++)
					{
						Console.Write("{0}", Char.IsControl((char)b2[j]) ? '.' : (char)b2[j]);
					}
					Console.WriteLine();
				}
			}
			return b2;
		}
	}
}
