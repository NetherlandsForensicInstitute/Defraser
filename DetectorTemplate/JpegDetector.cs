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
using System.Collections.Generic;
using Defraser;
using DetectorCommon;

namespace DetectorTemplate
{
	[Serializable]
	public sealed class JpegDetector : IDetector
	{
		private static readonly Dictionary<string, string[]> _columns = Detector.GetColumnNames<JpegHeaderName, JpegHeader>();

		#region Properties
		public string Name { get { return "Basic JPEG"; } }
		public string Description { get { return "The JPEG detection plug-in of choice."; } }
		public string OutputFileExtension { get { return ".jpg"; } }
		public Dictionary<string, string[]> Columns { get { return _columns; } }
		#endregion Properties

		/// <summary>
		/// Search for headers in the data and store the results.
		/// </summary>
		/// <param name="dataReader">the data to read the bytes and bits from</param>
		public IResultNode DetectData(IDataReader dataReader, IDetectionProperties properties)
		{
			JpegHeader root = new JpegHeader(this);

			using (JpegParser parser = new JpegParser(dataReader))
			{
				JpegHeader previousHeader = root;
				ushort marker;

				while ((marker = parser.NextMarker()) != 0)
				{
					JpegHeader header = HeaderForMarker(marker, previousHeader);

					if (header != null && header.HeaderName == JpegHeaderName.StartOfImage && root.Children.Count > 0)
					{
						root.Children.Clear();
					}
					if (header != null && parser.Parse(header))
					{
						previousHeader = header;

						if (IsFullFile(root))
						{
							break;
						}
					}
					else
					{
						parser.DataReader.Position++;
					}
				}
			}

			if (dataReader.State == DataReaderState.Cancelled || !IsFullFile(root))
			{
				root = null;
			}
			else
			{
				properties.DataFormat = CodecID.Jpeg;
				properties.IsFullFile = true;
			}
			return root;
		}

		private static bool IsFullFile(JpegHeader root)
		{
			return root.FirstChild.HeaderName == JpegHeaderName.StartOfImage &&
					root.LastChild.HeaderName == JpegHeaderName.EndOfImage &&
					root.FindChild(JpegHeaderName.StartOfScan, true) != null;
		}

		private static JpegHeader HeaderForMarker(ushort startCode, JpegHeader previousHeader)
		{
			switch (startCode)
			{
				case 0xFFD8: return new JpegHeader(previousHeader, JpegHeaderName.StartOfImage);
				case 0xFFD9: return new JpegHeader(previousHeader, JpegHeaderName.EndOfImage);
				case 0xFFDA: return new JpegHeader(previousHeader, JpegHeaderName.StartOfScan);
				case 0xFFE0: return new ApplicationSegment0(previousHeader);
				default:
					return null;
			}
		}
	}
}
