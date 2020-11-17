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

namespace Defraser.Detector.QT
{
	/// <summary>
	/// A data reference atom contains the information necessary to locate a movie, or a stream or file that
	/// QuickTime can play, typically in the form of a URL or a file alias.
	/// </summary>
	/// <remarks>
	/// Type: 'rdrf'
	/// Container: Reference Movie Descriptor 'rmda'
	/// </remarks>
	internal class ReferenceMovieDataReference : QtAtom
	{
		[Flags]
		public enum Flag { MovieIsSelfContained = 0x0000001 };

		public new enum Attribute
		{
			/// <summary>
			/// A 32-bit integer containing flags. One flag is currently defined: movie is self-contained. If the
			/// least-significant bit is set to 1, the movie is self-contained. This requires that the parent movie
			/// contain a movie header atom as well as a reference movie atom. In other words, the current
			/// 'moov' atom must contain both a 'rmra' atom and a 'mvhd' atom. To resolve this data
			/// reference, an application uses the movie defined in the movie header atom, ignoring the
			/// remainder of the fields in this data reference atom, which are used only to specify external
			/// movies.
			/// </summary>
			Flags,
			/// <summary>
			/// The data reference type.
			/// </summary>
			DataReferenceType,
			/// <summary>
			/// The size of the data reference in bytes, expressed as a 32-bit integer.
			/// </summary>
			DataReferenceSize,
			/// <summary>
			/// A data reference to a QuickTime movie, or to a stream or file that QuickTime can play. If the
			/// reference type is 'alis' this field contains the contents of an AliasHandle. If the reference
			/// type is 'url ' this field contains a null-terminated string that can be interpreted as a URL.
			/// The URL can be absolute or relative, and can specify any protocol that QuickTime supports,
			/// including http://, ftp://, rtsp://, file:///, and data:.
			/// </summary>
			DataReference,
		}

		/// <summary>Data reference types.</summary>
		public enum DataReferenceType : uint
		{
			/// <summary>File system alias record.</summary>
			alis = 0x616C6973,
			/// <summary>URL as zero-terminated string.</summary>
			url_ = 0x75726C20,
		}

		public ReferenceMovieDataReference(QtAtom previousHeader)
			: base(previousHeader, AtomName.ReferenceMovieDataReference)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			parser.GetFlags<Attribute, Flag>(Attribute.Flags, 4);

			uint dataReferenceType = parser.GetFourCC(Attribute.DataReferenceType);
			int dataReferenceSize = parser.GetInt(Attribute.DataReferenceSize);
			if(dataReferenceType == (uint)DataReferenceType.alis)
			{
				parser.GetUInt(Attribute.DataReference); // TODO test
			}
			else if(dataReferenceType == (uint)DataReferenceType.url_)
			{
				uint maxUrlLength = (uint)QtDetector.Configurable[QtDetector.ConfigurationKey.ReferenceMovieDataReferenceMaxUrlLength];
				parser.GetCString(Attribute.DataReference, maxUrlLength);
			}
			return this.Valid;
		}
	}
}
