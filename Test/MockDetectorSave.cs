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
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Test
{
	[DataContract]
	internal sealed class MockDetectorSave : Detector<MockHeader, MockHeaderName, MockParser>, ICodecDetector
	{
		#region Inner classes
#if _obsolete_
		private class MockParser : Parser<MockResult, string, MockParser, IDataReader>
		{
			public MockParser(IDataReader dataReader)
				: base(dataReader)
			{
			}
		}

		private class MockResult : Header<MockResult, string, MockParser, IDataReader>
		{
			private readonly long _offset;
			private readonly long _length;


			public MockResult(IDetector detector)
				: base(detector, "Root")
			{
			}

			/*TODO
			public MockResult(IResultNode parent, MockParser parser, long offset, long length, string name)
				: base(parent, name)
			{
				_offset = offset;
				_length = length;

				parser.Parse(this);
			}*/

			public override bool Parse(MockParser parser)
			{
				parser.DataReader.Position = _offset;
				return base.Parse(parser);
			}

			public override bool ParseEnd(MockParser parser)
			{
				parser.DataReader.Position = _offset + _length;
				return base.ParseEnd(parser);
			}

			public override bool IsBackToBack(MockResult header)
			{
				return true;
			}

			public override bool IsSuitableParent(MockResult parent)
			{
				return true;
			}
		}
#endif //_obsolete_
		#endregion Inner classes

		#region Properties
		override public string Name { get { return "TestDefraser.MockDetectorSave"; } }
		override public string Description { get { return "The mock detector to test data block and result saving."; } }
		override public string OutputFileExtension { get { return ".dat"; } }
		override public Type DetectorType { get { return GetType(); } }
		#endregion Properties

		static MockDetectorSave()
		{
			_supportedFormats = new CodecID[] {};
		}

		override public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			ByteStreamDataReader byteStreamDataReader = new ByteStreamDataReader(dataReader);
			int c = (byteStreamDataReader.GetByte() % 51);

			byteStreamDataReader.Position--;

			switch (c)
			{
				case 0:
					return DetectData1(byteStreamDataReader, dataBlockBuilder, context);
				case 4:
					return DetectData2(byteStreamDataReader, dataBlockBuilder, context);
				case 7:
					return DetectData3(byteStreamDataReader, dataBlockBuilder, context);
			}

			dataBlockBuilder.IsFullFile = false;
			return null;
		}

		private IDataBlock DetectData1(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			if ((dataReader.Position + 51) > dataReader.Length) return null;

			IResultNode root = new MockResult(this);
			IResultNode parent = root;
			int i = (int)(dataReader.GetDataPacket(0, 1).StartOffset + (dataReader.Position) / 51);
			for (int j = 0; j < 3; j++)
			{
				long offset = dataReader.Position;
				const long length = 17;
				parent = new MockResult(parent, dataReader, offset, length, "result: " + i + ", " + j);
				dataReader.Position += length;
			}
			context.Results = root;
			var firstChild = ((MockResult)context.Results.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((MockResult)context.Results.GetLastDescendant());
			dataBlockBuilder.EndOffset = (lastChild.Offset + lastChild.Length);
			return dataBlockBuilder.Build();
		}

		private IDataBlock DetectData2(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			IResultNode root = new MockResult(this);
			IResultNode parent = root;
			for (int i = 0; i < 51; i++)
			{
				long offset = i * 5;
				const long length = 5;
				parent = new MockResult(parent, dataReader, offset, length, "result: " + i);
			}
			context.Results = root;
			var firstChild = ((MockResult)context.Results.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((MockResult)context.Results.GetLastDescendant());
			dataBlockBuilder.EndOffset = (lastChild.Offset + lastChild.Length);
			return dataBlockBuilder.Build();
		}

		private IDataBlock DetectData3(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			IResultNode root = new MockResult(this);
			for (int i = 0; i < 17; i++)
			{
				long offset = i * 15;
				const long length = 15;
				new MockResult(root, dataReader, offset, length, "result: " + i);
			}
			context.Results = root;
			var firstChild = ((MockResult)context.Results.Children[0]);
			dataBlockBuilder.StartOffset = firstChild.Offset;
			var lastChild = ((MockResult)context.Results.GetLastDescendant());
			dataBlockBuilder.EndOffset = (lastChild.Offset + lastChild.Length);
			return dataBlockBuilder.Build();
		}
	}
}
