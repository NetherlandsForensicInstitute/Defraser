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

namespace Defraser.Detector.QT
{
	public enum ComponentType
	{
		mhlr = 0x6D686C72,
		dhlr = 0x64686C72
	}

	public enum ComponentSubType
	{
		vide = 0x76696465,
		soun = 0x736F756E,
		tmcd = 0x746D6364,
		sdsm = 0x7364736D,
		odsm = 0x6F64736D,
		hint = 0x68696E74
	}

	/// <summary>
	/// Specifies the media handler component used to interpret the media’s data. [hdlr]
	/// </summary>
	internal class HandlerReference : QtAtom
	{
		public new enum Attribute
		{
			// Field names from QuickTime spec

			/// <summary>
			/// A four-character code that identifies the type of the handler.
			/// Only two values are valid for this field: 'mhlr' for media
			/// handlers and 'dhlr' for data handlers.
			/// </summary>
			ComponentType,
			/// <summary>
			/// A four-character code that identifies the type of the media
			/// handler or data handler.
			/// </summary>
			ComponentSubType,
			/// <summary>
			/// Reserved. Set to 0.
			/// </summary>
			ComponentManufacturer,
			/// <summary>
			/// Reserved. Set to 0.
			/// </summary>
			ComponentFlags,
			/// <summary>
			/// Reserved. Set to 0.
			/// </summary>
			ComponentFlagsMask,
			/// <summary>
			/// A (counted) string that specifies the name of the
			/// component—that is, the media handler used when this media was
			/// created. This field may contain a zero-length (empty) string.
			/// </summary>
			ComponentName,

			// Same field names from MP4 spec
			HandlerType,
			Reserved	// For MP4 spec
		}

		#region Properties

		public ComponentSubType ComponentSubType { get; private set; }
		public string ComponentName { get; private set; }

		#endregion Properties

		public HandlerReference(QtAtom previousHeader)
			: base(previousHeader, AtomName.HandlerReference)
		{
		}

		public override bool Parse(QtParser parser)
		{
			if (!base.Parse(parser)) return false;

			uint componentType = parser.GetFourCC(Attribute.ComponentType);
			// Component type should be 'dhlr' or 'mhlr' according to QuickTime specs and 0 according to MP4 spec
			parser.CheckAttribute(Attribute.ComponentType, (componentType == (uint)ComponentType.dhlr || componentType == (uint)ComponentType.mhlr || componentType == 0), false);

			if (componentType == 0)	// Labels names from MP4 spec
			{
				ComponentSubType = (ComponentSubType)parser.GetFourCC(Attribute.HandlerType);
				parser.GetUInt(Attribute.Reserved);
				parser.GetUInt(Attribute.Reserved);
				parser.GetUInt(Attribute.Reserved);

				// Note: For 'MP4' format, the component name may be represented by a single '1'!

				if (parser.BytesRemaining > 0)
				{
					// FIXME: often, the 'ComponentName' is a Pascal string!!
					ComponentName = parser.GetCString(Attribute.ComponentName, (uint)(parser.BytesRemaining - 1));
				}
			}
			else	// Label names from QuickTime spec
			{
				ComponentSubType = (ComponentSubType) parser.GetFourCC(Attribute.ComponentSubType);
				// From QuickTime File Format Specification: 'Reserved. Set to 0.'
				// By finding quicktime files with a value filled in for this value
				// it is assumed that 'set to 0' is ment for quicktime file writers.
				// Not for file readers. That is why the attribute is not invalided
				// when a value != 0 is found.
				parser.GetFourCC(Attribute.ComponentManufacturer);
				// From QuickTime File Format Specification: 'Reserved. Set to 0.'
				// By finding quicktime files with a value filled in for this value
				// it is assumed that 'set to 0' is ment for quicktime file writers.
				// Not for file readers. That is why the attribute is not invalided
				// when a value != 0 is found.
				parser.GetInt(Attribute.ComponentFlags);
				// From QuickTime File Format Specification: 'Reserved. Set to 0.'
				// By finding quicktime files with a value filled in for this value
				// it is assumed that 'set to 0' is ment for quicktime file writers.
				// Not for file readers. That is why the attribute is not invalided
				// when a value != 0 is found.
				parser.GetInt(Attribute.ComponentFlagsMask);

				if (parser.BytesRemaining > 0)
				{
					ComponentName = parser.GetPascalString(Attribute.ComponentName, (int)parser.BytesRemaining);
				}
			}
			return Valid;
		}
	}
}
