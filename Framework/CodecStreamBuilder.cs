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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// The default implementation of <see cref="ICodecStreamBuilder"/>.
	/// </summary>
	public sealed class CodecStreamBuilder : ICodecStreamBuilder
	{
		#region Inner classes
		/// <summary>
		/// Serializable implementation of <see cref="ICodecStream"/>.
		/// </summary>
		[DataContract]
		public sealed class CodecStream : ICodecStream
		{
			#region Inner classes
			public sealed class SerializationContext : ISerializationContext
			{
				private readonly IDetectorFactory _detectorFactory;

				#region Properties
				public Type Type { get { return typeof(CodecStream); } }
				#endregion Properties

				public SerializationContext(IDetectorFactory detectorFactory)
				{
					PreConditions.Argument("detectorFactory").Value(detectorFactory).IsNotNull();

					_detectorFactory = detectorFactory;
				}

				public void CompleteDeserialization(object obj)
				{
					((CodecStream)obj).CompleteDeserialization(_detectorFactory);
				}
			}
			#endregion Inner classes

			[DataMember]
			private readonly CodecID _dataFormat;
			[DataMember]
			private IDetector _detector;
			[DataMember]
			private readonly IDataPacket _dataPacket;	// stream, offset, length
			[DataMember]
			private readonly int _streamNumber;
			[DataMember]
			private readonly string _name;
			[DataMember]
			private readonly IDataBlock _dataBlock;
			[DataMember]
			private readonly long _absoluteStartOffset;
			[DataMember]
			private readonly long _referenceHeaderOffset;
			[DataMember]
			private readonly IDataPacket _referenceHeader;

			#region Properties
			public CodecID DataFormat { get { return _dataFormat; } }
			public IDetector Detector { get { return _detector; } }
			public IEnumerable<IDetector> Detectors
			{
				get
				{
					List<IDetector> detectors = new List<IDetector> { _detector };
					if (DataBlock != null)
					{
						Debug.Assert(DataBlock.Detectors.Count() == 1);
						detectors.AddRange(DataBlock.Detectors);
					}
					return detectors;
				}
			}
			public IInputFile InputFile { get { return DataBlock.InputFile; } }
			public long Length { get { return _dataPacket.Length; } }
			public long StartOffset { get { return _dataPacket.StartOffset; } }
			public long AbsoluteStartOffset { get { return _absoluteStartOffset; } }
			public long EndOffset { get { return _dataPacket.EndOffset; } }
			public int StreamNumber { get { return _streamNumber; } }
			public string Name { get { return _name; } }
			public IDataBlock DataBlock { get { return _dataBlock; } }

			[DataMember]
			public int FragmentIndex { get; private set; }
			[DataMember]
			public bool IsFragmented { get; private set; }
			[DataMember]
			public IFragmentContainer FragmentContainer { get; set; }

			public long ReferenceHeaderOffset { get { return _referenceHeaderOffset; } }
			public IDataPacket ReferenceHeader { get { return _referenceHeader; } }
			#endregion Properties

			internal CodecStream(CodecStreamBuilder builder)
			{
				_dataFormat = builder.DataFormat;
				_detector = builder.Detector;
				_dataPacket = builder.Data;
				_streamNumber = builder.StreamNumber;
				_name = RemoveIllegalCharatersForXml(builder.Name);
				_dataBlock = builder.DataBlock;
				_absoluteStartOffset = builder.AbsoluteStartOffset;
				_referenceHeaderOffset = builder.ReferenceHeaderOffset;
				_referenceHeader = builder.ReferenceHeader;

				IsFragmented = builder.IsFragmented;

				// Check for previous fragment and connect if one exists
				IFragment previousFragment = builder.PreviousFragment;
				if ((previousFragment != null) && previousFragment.IsFragmented)
				{
					FragmentContainer = previousFragment.FragmentContainer;
					if (FragmentContainer == null)
					{
						FragmentContainer = new FragmentContainer();
						FragmentContainer.Add(previousFragment);
						previousFragment.FragmentContainer = FragmentContainer;
					}

					FragmentContainer.Add(this);

					FragmentIndex = previousFragment.FragmentIndex + 1;
				}
			}

			private static string RemoveIllegalCharatersForXml(string s)
			{
				char[] chars = s.ToCharArray();
				for (int i = 0; i < chars.Length; i++)
				{
					if ((chars[i] < ' ') || (chars[i] > 255))
					{
						chars[i] = ' ';
					}
				}
				return new string(chars);
			}

			private void CompleteDeserialization(IDetectorFactory detectorFactory)
			{
				if (Detector != null)
				{
					_detector = detectorFactory.GetDetector(Detector.DetectorType) ?? Detector;
				}
			}

			public IDataPacket Append(IDataPacket dataPacket)
			{
				if (Detector != null)
				{
					throw new ArgumentException("Can not append to a codec stream");
				}
				return _dataPacket.Append(dataPacket);
			}

			public IDataPacket GetSubPacket(long offset, long length)
			{
				if (Detector != null)
				{
					throw new ArgumentException("Can not append to a codec stream");
				}
				return _dataPacket.GetSubPacket(offset, length);
			}

			public IDataPacket GetFragment(long offset)
			{
                if (Detector != null)
                {
                    throw new ArgumentException("Can not append to a codec stream");
                }
                return _dataPacket.GetFragment(offset);
			}

			#region Equals() and GetHashCode()
			public override bool Equals(object obj)
			{
				return Equals(obj as CodecStream);
			}

			public bool Equals(CodecStream other)
			{
				if (other == null) return false;
				if (other == this) return true;

				if (other.Name != Name) return false;
				if (!Equals(other.Detector, Detector)) return false;
				if (other.DataFormat != DataFormat) return false;
				if (!other._dataPacket.Equals(_dataPacket)) return false;
				if ((other.DataBlock == null) != (DataBlock == null)) return false;
				return true;
			}

			public override int GetHashCode()
			{
				return Name.GetHashCode()
					.CombineHashCode(Detector)
					.CombineHashCode(DataFormat)
					.CombineHashCode(_dataPacket)
					.CombineHashCode((DataBlock != null));
			}
			#endregion Equals() and GetHashCode()
		}
		#endregion Inner classes

		private string _name;

		#region Properties
		public int StreamNumber { internal get; set; }
		public string Name
		{
			internal get { return _name; }
			set
			{
				PreConditions.Argument("value").Value(value).IsNotNull();

				_name = value;
			}
		}
		public CodecID DataFormat { private get; set; }
		/// <summary>Setter for the codec detector</summary>
		public IDetector Detector { private get; set; }
		public IDataPacket Data { get; set; }
		public IDataBlock DataBlock { private get; set; }
		public bool IsFragmented { private get; set; }
		public IFragment PreviousFragment { private get; set; }
		public long AbsoluteStartOffset { private get; set; }
		public long ReferenceHeaderOffset { private get; set; }
		public IDataPacket ReferenceHeader { private get; set; }
		#endregion Properties

		public CodecStreamBuilder()
		{
			Name = string.Empty;
			DataFormat = CodecID.Unknown;
		}

		public ICodecStream Build()
		{
			PreConditions.Operation().IsInvalidIf((Data == null), "Data was not set")
				.And.IsInvalidIf((DataBlock == null), "DataBlock was not set");

			return new CodecStream(this);
		}
	}
}
