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

namespace Defraser.Test
{
	[DataContract]
	internal sealed class MockDetectorReaderWriter : Detector<MockHeader, MockHeaderName, MockParser>, ICodecDetector
	{
		#region Inner classes
		private sealed class MockResult1 : MockResult
		{
			public MockResult1(IResultNode parent, IDataReader dataReader, long offset, long length)
				: base(parent, dataReader, offset, length, "correct type 1")
			{
				Attributes.Add(new FormattedAttribute<string, string>("General first", "type1 first"));
				Attributes.Add(new FormattedAttribute<string, string>("Type1 specific second", "type1 unique second"));
			}
		}

		private sealed class MockResult2 : MockResult
		{
			public MockResult2(IResultNode parent, IDataReader dataReader, long offset, long length)
				: base(parent, dataReader, offset, length, "correct type 2")
			{
				Attributes.Add(new FormattedAttribute<string, string>("General first", "type2 first"));
				Attributes.Add(new FormattedAttribute<string, string>("Type2 specific second", "type2 unique second"));
				Attributes.Add(new FormattedAttribute<string, string>("General third", "type2 unique second"));
				Attributes.Add(new FormattedAttribute<string, string>("General fourth", "type2 unique second"));
			}
		}
		#endregion Inner classes

		#region Properties
		override public string Name { get { return "TestDefraser.MockDetectorReaderWriter"; } }
		override public string Description { get { return "The dummy plug-in to test the framework."; } }
		override public string OutputFileExtension { get { return ".dat"; } }
		override public Type DetectorType { get { return GetType(); } }
		#endregion Properties

		/// <summary>Static data initialization.</summary>
		static MockDetectorReaderWriter()
		{
			_columns["correct type 1"] = new string[4] { "General first", "Type1 specific second", "General third", "General fourth" };
			_columns["correct type 2"] = new string[4] { "General first", "Type2 specific second", "General third", "General fourth" };
			_supportedFormats = new CodecID[] {};
		}

		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			IResultNode root = new MockResult(this);
			ByteStreamDataReader byteStreamDataReader = new ByteStreamDataReader(dataReader);
			dataReader = byteStreamDataReader;

			long dataReaderOffset = dataReader.GetDataPacket(0, 1).StartOffset;
			long initialPosition = dataReader.Position + dataReaderOffset;

			if (initialPosition == 0 && dataReader.Length >= 268)
			{
				dataReader.Position = 0;
				long offset = 0L;
				long length;
				IResultNode parent = root;
				if (byteStreamDataReader.GetByte() == 69)
				{
					offset = 0L;
					length = 1L;
					parent = new MockResult1(parent, dataReader, offset, length);
				}
				long firstDataBlockOffset = offset;

				dataReader.Position = 255;
				if (byteStreamDataReader.GetByte() == 255)
				{
					offset = 255L;
					length = 2L;
					parent = new MockResult2(parent, dataReader, offset, length);
				}

				dataReader.Position = 260;
				if (byteStreamDataReader.GetByte() == 78)
				{
					offset = 260L;
					length = 3L;
					new MockResult1(parent, dataReader, offset, length);
				}

				offset = 264;
				length = 4;
				dataReader.Position = 264;
				if (byteStreamDataReader.GetByte() == 5)
				{
					new MockResult2(parent, dataReader, offset, length);
				}

				context.Results = root;

				var firstChild = ((MockResult)root.Children[0]);
				dataBlockBuilder.StartOffset = firstChild.Offset;
				var lastChild = ((MockResult)root.GetLastDescendant());
				dataBlockBuilder.EndOffset = (lastChild.Offset + lastChild.Length);
				dataBlockBuilder.IsFullFile = false;
				return dataBlockBuilder.Build();
			}

			if (initialPosition <= 517 && (initialPosition + dataReader.Length) >= 518)
			{
				long offset = 517L - dataReaderOffset;
				const long length = 1L;

				dataReader.Position = offset;
				if (byteStreamDataReader.GetByte() == 2)
				{
					new MockResult1(root, dataReader, offset, length);
				}

				context.Results = root;

				var firstChild = ((MockResult)root.Children[0]);
				dataBlockBuilder.StartOffset = firstChild.Offset;
				var lastChild = ((MockResult)root.GetLastDescendant());
				dataBlockBuilder.EndOffset = (lastChild.Offset + lastChild.Length);
				dataBlockBuilder.IsFullFile = (root.Children.Count == 1);
				return dataBlockBuilder.Build();
			}
			return null;
		}

		public bool IsFullFile(IResultNode results)
		{
			return (results.Children.Count == 1) && (results.Children[0].StartOffset == 517L);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
