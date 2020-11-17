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
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// 3GPP/QuickTime detector.
	/// </summary>
	/// <remarks>
	/// 3GP is a simplified version of the MPEG-4 Part 14 (MP4) container format,
	/// designed to decrease storage and bandwidth requirements in order to
	/// accommodate mobile phones. It stores video streams as MPEG-4 Part 2 or
	/// H.263 or MPEG-4 Part 10 (AVC/H.264). A 3GP file is always big-endian,
	/// storing and transferring the most significant bytes first. It also contains
	/// descriptions of image sizes and bitrate. There are two different standards
	/// for this format:
	/// <ul>
	///	<li> 3GPP (for GSM-based Phones, may have filename extension .3gp)
	///	<li> 3GPP2 (for CDMA-based Phones, may have filename extension .3g2)
	/// </ul>
	/// Both are based on MPEG-4 and H.263 video.
	/// <b>Source:</b> http://www.wikipedia.org
	/// </remarks>
	[DataContract]
	public class QtDetector : Detector<QtAtom, AtomName, QtParser>
	{
		internal enum ConfigurationKey
		{
			QtAtomMaxUnparsedBytes,
			FileTypeMaxUnparsedBytes,
			MediaDateMaxUnparsedBytes,
			ReferenceMovieDataReferenceMaxUrlLength
		}

		//private const int MaxMediaDataUnallocatedBytes = 64;

		#region Properties
		override public string Name { get { return "3GPP/QT/MP4"; } }
		override public string Description { get { return "3GPP/QuickTime detector"; } }
		override public string OutputFileExtension { get { return ".3gp"; } }
		override public Type DetectorType { get { return GetType(); } }

		override public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Properties

		static QtDetector()
		{
			_supportedFormats = new[] { CodecID.QuickTime, CodecID.Itu3Gpp };

			Configurable = new Configurable<ConfigurationKey>();

			Configurable.Add(ConfigurationKey.QtAtomMaxUnparsedBytes, (uint)65536);
			Configurable.Add(ConfigurationKey.FileTypeMaxUnparsedBytes, (uint)1000);
			Configurable.Add(ConfigurationKey.MediaDateMaxUnparsedBytes, (uint)64);
			Configurable.Add(ConfigurationKey.ReferenceMovieDataReferenceMaxUrlLength, (uint)1000);
		}

		protected bool IsFullFile(QtAtom root)
		{
			//FileType fileType = root.FindChild(AtomName.FileType) as FileType;
			QtAtom movie = root.FindChild(AtomName.Movie);
			MediaData mediaData = root.FindChild(AtomName.MediaData) as MediaData;

			return (root.FirstChild.HeaderName == AtomName.FileType) && (movie != null) && (mediaData != null);
		}

		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			QtAtom root = new QtAtom(context.Detectors);

			// Try to parse a contiguous block of headers
			using (QtParser parser = new QtParser(new ByteStreamDataReader(dataReader)))
			{
				if (!parser.ParseRoot(root, dataReader.Length))
				{
					dataReader.Position = dataReader.Length;
					return null;
				}

				// Synchronize the position of the data reader
				dataReader.Position = parser.Position;
			}

			// Trim trailing unknown or large unparsable atoms
			QtAtom lastHeader = root.LastHeader;
            long firstHeaderOffset = ((QtAtom)root.Children[0]).Offset;
			while (lastHeader.IsUnknown && lastHeader.Parent != null)
			{
				if (lastHeader.RelativeEndOffset == ((QtAtom)lastHeader.Parent).RelativeEndOffset)
				{
					break;	// Last child perfectly aligned with parent atom
				}

				lastHeader.Parent.RemoveChild(lastHeader);
				lastHeader = root.LastHeader;
			}

			// Discard data blocks of only 1 atom
			if ((dataReader.State == DataReaderState.Cancelled) ||
				(root.Children.Count == 0) ||
				((root.Children.Count == 1) && !root.FirstChild.HasChildren()))
			{
				// Rewind data reader
                dataReader.Position = firstHeaderOffset + 1; // lastHeader.Offset + lastHeader.Length; // != lastHeader.RelativeEndOffset; !!!

				return null;
			}

			context.Results = root;

			var codecStream = new CodecStream(dataReader, dataBlockBuilder, root);
			long? rescanPosition = codecStream.CreateCodecStreams();

			// TODO: check for (complete) mdat block (for IsFullFile)

			// Note: For consistency, always report the name of the detector itself, not a specific data format!
			//dataBlockBuilder.DataFormat = root.DataFormat;
			dataBlockBuilder.IsFullFile = IsFullFile(root);
			var firstChild = ((QtAtom)root.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((QtAtom)root.GetLastDescendant());
			var endOffset = (lastChild.Offset + lastChild.Length);

			dataBlockBuilder.EndOffset = endOffset;
			dataReader.Position = rescanPosition ?? endOffset;

			// If the last atom is an 'mdat' atom that was truncated for rescan to its minimum size, truncate the actual result!
			if ((rescanPosition == (lastChild.Offset + 8)) && (lastChild == root.FindChild(AtomName.MediaData)))
			{
				dataBlockBuilder.EndOffset = rescanPosition.Value;
			}
			return dataBlockBuilder.Build();
		}
	}
}
