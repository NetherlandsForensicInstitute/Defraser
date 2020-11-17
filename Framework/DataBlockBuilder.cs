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
using System.Linq;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;
using System.Diagnostics;

namespace Defraser.Framework
{
	/// <summary>
	/// The default implementation of <see cref="IDataBlockBuilder"/>.
	/// </summary>
	public sealed class DataBlockBuilder : IDataBlockBuilder
	{
		#region Inner classes
		/// <summary>
		/// Serializable implementation of <see cref="IDataBlock"/>.
		/// </summary>
		[DataContract]
		public sealed class DataBlock : IDataBlock, IEquatable<DataBlock>
		{
			#region Inner classes
			public sealed class SerializationContext : ISerializationContext
			{
				private readonly Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;

				#region Properties
				public Type Type { get { return typeof(DataBlock); } }
				#endregion Properties

				public SerializationContext(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets)
				{
					PreConditions.Argument("appendDataPackets").Value(appendDataPackets).IsNotNull();

					_appendDataPackets = appendDataPackets;
				}

				public void CompleteDeserialization(object obj)
				{
					((DataBlock)obj).CompleteDeserialization(_appendDataPackets);
				}
			}
			#endregion Inner classes

			private Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;

			[DataMember]
			private readonly CodecID _dataFormat;
			[DataMember]
			private readonly IEnumerable<IDetector> _detectors;
			[DataMember]
			private readonly IInputFile _inputFile;
			[DataMember]
			private readonly long _startOffset;
			[DataMember]
			private readonly long _endOffset;
			[DataMember]
			private readonly bool _isFullFile;
			[DataMember]
			private readonly long _referenceHeaderOffset;
			[DataMember]
			private readonly IDataPacket _referenceHeader;
			[DataMember]
			private readonly IList<ICodecStream> _codecStreams;

			#region Properties
			public CodecID DataFormat { get { return _dataFormat; } }
			public IEnumerable<IDetector> Detectors { get { return _detectors; } }
			public IInputFile InputFile { get { return _inputFile; } }
			public long Length { get { return (_endOffset - _startOffset); } }
			public long StartOffset { get { return _startOffset; } }
			public long EndOffset { get { return _endOffset; } }
			public bool IsFullFile { get { return _isFullFile; } }
			public IList<ICodecStream> CodecStreams { get { return _codecStreams; } }

			[DataMember]
			public int FragmentIndex { get; private set; }
			[DataMember]
			public bool IsFragmented { get; private set; }
			[DataMember]
			public IFragmentContainer FragmentContainer { get; set; }

			public long ReferenceHeaderOffset { get { return _referenceHeaderOffset; } }
			public IDataPacket ReferenceHeader { get { return _referenceHeader; } }
			#endregion Properties

			internal DataBlock(DataBlockBuilder builder)
			{
				_appendDataPackets = builder._appendDataPackets;
				_dataFormat = builder.DataFormat;
				_detectors = builder.Detectors;
				_inputFile = builder.InputFile;
				_startOffset = builder.StartOffset;
				_endOffset = builder.EndOffset;
				_isFullFile = builder.IsFullFile;
				_referenceHeaderOffset = builder.ReferenceHeaderOffset;
				_referenceHeader = builder.ReferenceHeader;
				_codecStreams = builder.BuildCodecStreams(this);

				IsFragmented = builder.IsFragmented;

				// Check for previous fragment and connect if one exists
				IFragment previousFragment = builder.PreviousFragment;
				if ((previousFragment != null) && previousFragment.IsFragmented)
				{
					if (previousFragment.FragmentContainer == null)
					{
						previousFragment.FragmentContainer = new FragmentContainer();
						previousFragment.FragmentContainer.Add(previousFragment);
					}

					FragmentContainer = previousFragment.FragmentContainer;
					FragmentContainer.Add(this);

					FragmentIndex = previousFragment.FragmentIndex + 1;
				}
			}

			private DataBlock(DataBlock dataBlock, long startOffset, long endOffset)
			{
				_appendDataPackets = dataBlock._appendDataPackets;
				_dataFormat = dataBlock.DataFormat;
				Debug.Assert(dataBlock.Detectors.Count() <= 2);
				_detectors = dataBlock.Detectors;
				_inputFile = dataBlock.InputFile;
				_isFullFile = dataBlock.IsFullFile;
				_referenceHeader = dataBlock.ReferenceHeader;
				_codecStreams = new ICodecStream[0];

				_startOffset = startOffset;
				_endOffset = endOffset;
			}

			private void CompleteDeserialization(Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets)
			{
				_appendDataPackets = appendDataPackets;
			}

			public IDataPacket Append(IDataPacket dataPacket)
			{
				return _appendDataPackets(this, dataPacket);
			}

			public IDataPacket GetSubPacket(long offset, long length)
			{
				return CreateSubPacket(offset, length);
			}

			public IDataPacket GetFragment(long offset)
			{
				return CreateSubPacket(offset, (Length - offset));
			}

			private IDataPacket CreateSubPacket(long offset, long length)
			{
				PreConditions.Argument("offset").Value(offset).InRange(0L, (Length - 1L));
				PreConditions.Argument("length").Value(length).InRange(1L, (Length - offset));

				if ((offset == 0L) && (length == Length))
				{
					return this;	// Sub-packet is the entire packet
				}

				return new DataBlock(this, (StartOffset + offset), (StartOffset + offset + length));
			}

			#region Equals() and GetHashCode()
			public override bool Equals(object obj)
			{
				return Equals(obj as DataBlock);
			}

			public bool Equals(DataBlock other)
			{
				if (other == null) return false;
				if (other == this) return true;

				if (other.DataFormat != DataFormat) return false;
				if (!other.Detectors.Count().Equals(Detectors.Count())) return false;
				for (int i = 0; i < other.Detectors.Count(); i++ )
				{
					if (!other.Detectors.ElementAt(i).Equals(Detectors.ElementAt(i))) return false;
				}
				if (!other.InputFile.Equals(InputFile)) return false;
				if (other.StartOffset != StartOffset) return false;
				if (other.EndOffset != EndOffset) return false;
				if (other.IsFullFile != IsFullFile) return false;
				if (!Equals(other.ReferenceHeader, ReferenceHeader)) return false;
				if (!other.CodecStreams.SequenceEqual(other.CodecStreams)) return false;
				return true;
			}

			public override int GetHashCode()
			{
				return DataFormat.GetHashCode()
					.CombineHashCode(Detectors)
					.CombineHashCode(InputFile)
					.CombineHashCode(StartOffset)
					.CombineHashCode(EndOffset)
					.CombineHashCode(IsFullFile)
					.CombineHashCode(ReferenceHeader)
					.CombineHashCode(CodecStreams);
			}
			#endregion Equals() and GetHashCode()
		}
		#endregion Inner classes

		private readonly Creator<ICodecStreamBuilder> _createCodecStreamBuilder;
		private readonly Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;
		private readonly IList<ICodecStreamBuilder> _codecStreamBuilders;

		#region Properties
		public CodecID DataFormat { get; set; }
		public IEnumerable<IDetector> Detectors { get; set; }
		public IInputFile InputFile { get; set; }
		public long StartOffset { private get; set; }
		public long EndOffset { private get; set; }
		public bool IsFullFile { private get; set; }
		public bool IsFragmented { get; set; }
		public IFragment PreviousFragment { private get; set; }
		public long ReferenceHeaderOffset { private get; set; }
		public IDataPacket ReferenceHeader { private get; set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="DataBlockBuilder"/>.
		/// </summary>
		/// <param name="createCodecStreamBuilder">
		/// The factory method for creating codec stream builders, used for adding
		/// codec streams to the data block</param>
		/// <param name="appendDataPackets">
		/// The factory method for creating one data packet by appending two existing
		/// data packets. This delegate is used by <see cref="DataBlock.Append(IDataPacket)"/>.
		/// </param>
		public DataBlockBuilder(Creator<ICodecStreamBuilder> createCodecStreamBuilder, Creator<IDataPacket, IDataPacket, IDataPacket> appendDataPackets)
		{
			PreConditions.Argument("createCodecStreamBuilder").Value(createCodecStreamBuilder).IsNotNull();
			PreConditions.Argument("appendDataPackets").Value(appendDataPackets).IsNotNull();

			_createCodecStreamBuilder = createCodecStreamBuilder;
			_appendDataPackets = appendDataPackets;
			_codecStreamBuilders = new List<ICodecStreamBuilder>();

			DataFormat = CodecID.Unknown;
		}

		public ICodecStreamBuilder AddCodecStream()
		{
			ICodecStreamBuilder codecStreamBuilder = _createCodecStreamBuilder();
			codecStreamBuilder.StreamNumber = _codecStreamBuilders.Count;
			_codecStreamBuilders.Add(codecStreamBuilder);
			return codecStreamBuilder;
		}

		public IDataBlock Build()
		{
			PreConditions.Operation().IsInvalidIf((Detectors == null || Detectors.Count() == 0 ), "Detector was not set")
				.And.IsInvalidIf((InputFile == null), "InputFile was not set")
				.And.IsInvalidIf(((StartOffset < 0) || (StartOffset >= InputFile.Length)), "StartOffset is invalid")
				.And.IsInvalidIf(((EndOffset <= StartOffset) /*|| (EndOffset > InputFile.Length)*/), "EndOffset is invalid");

			return new DataBlock(this);
		}

		private IList<ICodecStream> BuildCodecStreams(IDataBlock dataBlock)
		{
			List<ICodecStream> codecStreams = new List<ICodecStream>();
			IFragment previousFragment = null;
			foreach (ICodecStreamBuilder codecStreamBuilder in _codecStreamBuilders)
			{
				codecStreamBuilder.DataBlock = dataBlock;
				codecStreamBuilder.PreviousFragment = previousFragment;
				ICodecStream codecStream = codecStreamBuilder.Build();

				codecStreams.Add(codecStream);
				previousFragment = codecStream;
			}
			return codecStreams.AsReadOnly();
		}
	}
}
