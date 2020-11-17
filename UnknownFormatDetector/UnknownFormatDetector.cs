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
using Defraser.Interface;
using Defraser.DataStructures;

namespace Defraser.Detector.UnknownFormat
{
	/// <summary>
	/// Describes data in an unknown, unsupported or disabled format or codec.
	/// </summary>
	public sealed class UnknownFormatDetector : ICodecDetector
	{
		private static readonly IDictionary<string, string[]> EmptyColumns = new Dictionary<string, string[]>();
		private static readonly ICollection<IConfigurationItem> ConfigurationItems = new List<IConfigurationItem>();

		#region Properties
		public string Name { get { return "Unknown"; } }
		public string Description { get { return "Unknown format or codec"; } }
		public string OutputFileExtension { get { return ".bin"; } }
		public IDictionary<string, string[]> Columns { get { return EmptyColumns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return new CodecID[0]; } }
		public Type DetectorType { get { return GetType(); } }
		public ICollection<IConfigurationItem> Configuration { get { return ConfigurationItems; } }
		public IEnumerable<IReferenceHeader> ReferenceHeaders { get; set; }
		#endregion Properties

		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			long startOffset = dataReader.Position;
			long endOffset = dataReader.Length;

			IMetadata metadata = new Metadata(CodecID.Unknown, context.Detectors);
			IResultNode root = new RootResult(metadata, dataReader.GetDataPacket(0, 1).InputFile);
			IResultNodeBuilder resultNodeBuilder = new ResultNodeBuilder
               	{
               		Name = "Data",
               		Metadata = metadata,
               		DataPacket = dataReader.GetDataPacket(startOffset, (endOffset - startOffset))
               	};
			root.AddChild(resultNodeBuilder.Build());
			context.Results = root;

			dataBlockBuilder.DataFormat = CodecID.Unknown;
			dataBlockBuilder.StartOffset = startOffset;
			dataBlockBuilder.EndOffset = endOffset;
			dataReader.Position = endOffset;
			return dataBlockBuilder.Build();
		}

		public bool IsKeyFrame(IResultNode resultNode)
		{
			return false;
		}

		public IDataPacket GetVideoHeaders(IResultNode headerPacket)
		{
			return null;
		}

		public IDataPacket GetVideoData(IResultNode resultNode)
		{
			return null;
		}

		public IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec context)
		{
			return null;
		}
	}
}
