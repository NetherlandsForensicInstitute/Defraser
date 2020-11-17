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
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Asf
{
	[DataContract]
	class AsfDetector : Detector<AsfObject, AsfObjectName, AsfParser>, IConfigurable
	{
		internal enum ConfigurationKey
		{
			AsfObjectMaxUnparsedByteCount,
			AttributeMaxStringLength,
		}

		#region Properties
		override public string Name { get { return "ASF/WMV"; } }
		override public string Description { get { return "ASF/WMV detector"; } }
		override public string OutputFileExtension { get { return ".asf"; } }
		override public Type DetectorType { get { return GetType(); } }

		override public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Properties

		static AsfDetector()
		{
			_supportedFormats = new[] { CodecID.Asf };

			Configurable = new Configurable<ConfigurationKey>();

			Configurable.Add(ConfigurationKey.AsfObjectMaxUnparsedByteCount, (uint)65536);
			Configurable.Add(ConfigurationKey.AttributeMaxStringLength, (uint)100000);
		}

		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			AsfObject root = new AsfObject(context.Detectors);

			// Try to parse a contiguous block of ASF objects
			using (AsfParser parser = new AsfParser(new ByteStreamDataReader(dataReader, Endianness.Little)))
			{
				if (!parser.ParseRoot(root, dataReader.Length))
				{
					dataReader.Position = dataReader.Length;
					return null;
				}

				// Synchronize the position of the data reader
				dataReader.Position = parser.Position;
			}

			// Trim trailing unknown, padding byte or large unparsable ASF objects
			AsfObject lastAsfObject = root.LastHeader;
			while (lastAsfObject.IsUnknown && lastAsfObject.Parent != null)
			{
				if (lastAsfObject.RelativeEndOffset == ((AsfObject)lastAsfObject.Parent).RelativeEndOffset)
				{
					break;	// Last child perfectly aligned with parent object
				}

				lastAsfObject.Parent.RemoveChild(lastAsfObject);
				lastAsfObject = root.LastHeader;
			}

			if ((dataReader.State == DataReaderState.Cancelled) || 
				(root.Children.Count == 0) ||
				(root.Children.Count == 1 && !root.FirstChild.HasChildren()))	// Discard data blocks of only 1 ASF object
			{
				// Rewind data reader
				dataReader.Position = lastAsfObject.Offset + lastAsfObject.Length;	// != lastHeader.RelativeEndOffset; !!!

				return null;
			}

			context.Results = root;

			dataBlockBuilder.DataFormat = root.DataFormat;
			var firstChild = ((AsfObject)root.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((AsfObject)root.GetLastDescendant());
			var endOffset = (lastChild.Offset + lastChild.Length);
			dataBlockBuilder.EndOffset = endOffset;

			CodecStream codecStream = new CodecStream(root);
			codecStream.CreateCodecStreams(dataBlockBuilder);

			dataReader.Position = endOffset;
			return dataBlockBuilder.Build();
		}
	}
}
