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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class CopyrightExtension : IExtensionParser
	{
		#region Inner classes
		/// <summary>
		/// Describes a copyright number.
		/// </summary>
		private class CopyrightNumberAttribute : IAttributeParser<IMpeg2VideoReader>
		{
			private readonly IResultFormatter _copyrightNumberResultFormatter;

			internal CopyrightNumberAttribute()
			{
				_copyrightNumberResultFormatter = new StringResultFormatter("{0:X16}");
			}

			public void Parse(IMpeg2VideoReader reader, IAttributeState resultState)
			{
				resultState.Name = Attribute.CopyrightNumber;

				ulong copyrightNumber1 = reader.GetBits(20, "Bits [63..44]");
				reader.GetMarker();
				ulong copyrightNumber2 = reader.GetBits(22, "Bits [43..22]");
				reader.GetMarker();
				ulong copyrightNumber3 = reader.GetBits(22, "Bits [21..0]");

				resultState.Value = (copyrightNumber1 << 44) | (copyrightNumber2 << 22) | copyrightNumber3;
				resultState.Formatter = _copyrightNumberResultFormatter;
			}
		}
		#endregion Inner classes

		private enum Attribute
		{
			CopyrightFlag,
			CopyrightIdentifier,
			OriginalOrCopy,
			CopyrightNumber
		}

		internal const string Name = "CopyrightExtension";

		private readonly IAttributeParser<IMpeg2VideoReader> _copyrightNumberAttribute;

		#region Properties
		public ExtensionId ExtensionId { get { return ExtensionId.CopyrightExtensionId; } }
		#endregion Properties

		public CopyrightExtension()
		{
			_copyrightNumberAttribute = new CopyrightNumberAttribute();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;
			reader.GetFlag(Attribute.CopyrightFlag);
			reader.GetBits(8, Attribute.CopyrightIdentifier);
			reader.GetFlag(Attribute.OriginalOrCopy);	// TODO: true = original, false = copy
			reader.GetReservedBits(7);
			reader.GetMarker();
			reader.GetAttribute(_copyrightNumberAttribute);
		}
	}
}
