/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	internal sealed class H264ResultTreeBuilder : IDataBlockCarver
	{
		private static readonly IDictionary<IDataPacket, IPictureState> NoDefaultHeaders = new ReadOnlyDictionary<IDataPacket, IPictureState>(new Dictionary<IDataPacket, IPictureState>());

		private static readonly string FirstMacroblockInSliceName = Enum.GetName(typeof(SliceHeader.Attribute), SliceHeader.Attribute.FirstMacroblockInSlice);
		private static readonly string PictureParameterSetIdName = Enum.GetName(typeof(SliceHeader.Attribute), SliceHeader.Attribute.PictureParameterSetId);
		private static readonly string SliceTypeName = Enum.GetName(typeof(SliceHeader.Attribute), SliceHeader.Attribute.SliceType);

		private static readonly bool[] NalUnitStartByteSequenceParameterSet;
		private static readonly bool[] NalUnitStartByteByteStream;

		static H264ResultTreeBuilder()
		{
			NalUnitStartByteSequenceParameterSet = new bool[256];
			NalUnitStartByteByteStream = new bool[256];

			for (int nalRefIdc = 0; nalRefIdc < 4; nalRefIdc++)
			{
				int nonIdrPictureSliceIndex = (nalRefIdc << 5) | (int)NalUnitType.CodedSliceOfANonIdrPicture;
				NalUnitStartByteByteStream[nonIdrPictureSliceIndex] = true;
			}
			for (int nalRefIdc = 1; nalRefIdc < 4; nalRefIdc++)
			{
				int nalRefIdcBits = (nalRefIdc << 5);
				NalUnitStartByteByteStream[nalRefIdcBits | (int)NalUnitType.CodedSliceOfAnIdrPicture] = true;
				NalUnitStartByteByteStream[nalRefIdcBits | (int)NalUnitType.SequenceParameterSet] = true;
				NalUnitStartByteSequenceParameterSet[nalRefIdcBits | (int)NalUnitType.SequenceParameterSet] = true;
				NalUnitStartByteByteStream[nalRefIdcBits | (int)NalUnitType.PictureParameterSet] = true;
			}
		}

		private readonly IH264State _state;
		private readonly IScanContext _scanContext;
		private readonly bool _falseHitReduction;
		private readonly BitStreamDataReader _dataReader;
		private readonly IH264Reader _reader;
		private readonly NalUnitStreamParser _nalUnitStreamParser;
		private readonly ByteStreamParser _byteStreamParser;
		private readonly uint _maxGapBetweenNalUnits;
		private readonly uint _maxGapBetweenPpsAndSlice;
		private readonly uint _maxGapBetweenPpsAndSps;
		private IResultNode _lastHeader;
		private IStreamParser _streamParser;

		public H264ResultTreeBuilder(BitStreamDataReader dataReader, IH264Reader reader, IH264State state, IScanContext scanContext, NalUnitStreamParser nalUnitStreamParser, ByteStreamParser byteStreamParser)
		{
			_falseHitReduction = (bool)H264Detector.Configurable[H264Detector.ConfigurationKey.FalseHitReduction];
			_dataReader = dataReader;
			_reader = reader;
			_state = state;
			_scanContext = scanContext;
			_nalUnitStreamParser = nalUnitStreamParser;
			_byteStreamParser = byteStreamParser;

			Configurable<H264Detector.ConfigurationKey> configurable = H264Detector.Configurable;
			_maxGapBetweenPpsAndSlice = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenPpsAndSlice];
			_maxGapBetweenPpsAndSps = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenPpsAndSps];
			_maxGapBetweenNalUnits = (uint)configurable[H264Detector.ConfigurationKey.MaxGapBetweenNalUnits];
		}

		public bool Carve(long offsetLimit)
		{
			_state.Reset();
			_lastHeader = null;
			_reader.ReferenceHeaders = (_scanContext.ReferenceHeader as IDictionary<IDataPacket, IPictureState>) ?? NoDefaultHeaders;

			_streamParser = null;
			_streamParser = FindNalUnit(true);

			if ((_streamParser != null) && (_reader.ReferenceHeaders.Count >= 1))
			{
				_state.ReferenceHeaderPosition = _dataReader.Position;
				_state.ByteStreamFormat = (_streamParser is ByteStreamParser);
			}
			return (_streamParser != null);
		}

		private IStreamParser FindNalUnit(bool firstHeader)
		{
			bool detectByteStreamFormat = firstHeader || (_streamParser == _byteStreamParser);
			bool detectNalUnitStreamFormat = firstHeader || (_streamParser == _nalUnitStreamParser);

			while (_dataReader.State == DataReaderState.Ready)
			{
				// Note: This assumes that a valid NAL unit is less than 1MB in size.
				uint startCode = _dataReader.NextStartCode(9, 0, 23);
				long bytesRemaining = (_dataReader.Length - _dataReader.Position);
				if (bytesRemaining < 4)
				{
					_dataReader.Position = _dataReader.Length;
					break;
				}

				// Check for 'byte stream' format with '00 00 01' start code prefix
				if (detectByteStreamFormat)
				{
					if (((startCode & ~0xff) == 0x00000100) && NalUnitStartByteByteStream[startCode & 0xff])
					{
						return _byteStreamParser;
					}
				}

				// Check for 'NAL unit stream' format: "00 LL xx", where 'LL' is the length prefix and 'xx' is the NAL unit byte
				if (detectNalUnitStreamFormat)
				{
					if (_nalUnitStreamParser.IsShortLengthPrefixedNalUnit(startCode >> 8, bytesRemaining))
					{
						// Note: The data should not contain illegal sequences of '00 00 0x xx',
						//       other than start code prefix emulation prevention sequences.
						return _nalUnitStreamParser;
					}
				}

				// Advance to next position, i.e. the NAL unit byte
				_dataReader.Position++;

				// Check for 'byte stream' format with '00 00 00 01' start code prefix
				var nalUnitByte = (byte)(_dataReader.ShowBits(32) & 0xff);
				if (detectByteStreamFormat)
				{
					if ((startCode == 0x00000001) && NalUnitStartByteSequenceParameterSet[nalUnitByte])
					{
						_dataReader.Position--;
						return _byteStreamParser;
					}
				}

				// Check for 'NAL unit stream' format: "00 LL LL LL xx", where 'LLLLLL' is the length prefix and 'xx' is the NAL unit byte
				if (detectNalUnitStreamFormat)
				{
					ulong nextFiveBytes = ((ulong) startCode << 8) | nalUnitByte;
					if (_nalUnitStreamParser.IsLongLengthPrefixedNalUnit(nextFiveBytes, bytesRemaining))
					{
						// TODO: The data should not contain illegal sequences of '00 00 0x xx',
						//       other than start code prefix emulation prevention sequences.

						_dataReader.Position--;
						return _nalUnitStreamParser;
					}
				}
			}

			// No more NAL units!
			return null;
		}

		public void ParseHeader(IReaderState readerState)
		{
			bool firstHeader = (_state.NalUnitType == NalUnitType.Invalid);
			uint maxGapAfterNalUnit = GetMaxGapAfterNalUnit(_state.NalUnitType);
			long startPosition = _reader.Position;
			long maxNalUnitStartPosition = Math.Min(_reader.Length, (startPosition + maxGapAfterNalUnit));

			NalUnitType previousNalUnitType = _state.NalUnitType;

			while (_dataReader.State == DataReaderState.Ready)
			{
				if (!firstHeader)
				{
					while (true)
					{
						if ((FindNalUnit(false) == null) || (_reader.Position >= maxNalUnitStartPosition) || (_dataReader.State != DataReaderState.Ready))
						{
							_reader.Position = startPosition;
							readerState.Invalidate();
							return;
						}

						var nextNalUnitType = (NalUnitType) (GetNalUnitByte() & 0x1f);
						if ((_reader.Position - startPosition) <= GetMaxGapBetweenNalUnits(previousNalUnitType, nextNalUnitType))
						{
							break;
						}

						_reader.Position++;
					}
				}

				long nalUnitStartPosition = _reader.Position;
				readerState.Parse(_streamParser, _reader);

				if (readerState.Valid)
				{
					return; // Result is valid
				}
				if (firstHeader || (nalUnitStartPosition >= (maxNalUnitStartPosition - 1)))
				{
					return; // Done scanning for a valid result
				}

				readerState.Recover();
				_reader.Position = (nalUnitStartPosition + 1);
			}

			// Nothing was found, so the result is invalid
			readerState.Invalidate();
		}

		private uint GetMaxGapAfterNalUnit(NalUnitType nalUnitType)
		{
			if (nalUnitType == NalUnitType.Invalid)
			{
				return 0;
			}
			if (nalUnitType == NalUnitType.SequenceParameterSet)
			{
				return Math.Max(_maxGapBetweenPpsAndSps, _maxGapBetweenNalUnits);
			}
			if (nalUnitType == NalUnitType.PictureParameterSet)
			{
				return Math.Max(_maxGapBetweenPpsAndSlice, _maxGapBetweenNalUnits);
			}

			return _maxGapBetweenNalUnits;
		}

		private byte GetNalUnitByte()
		{
			uint startCode = _dataReader.ShowBits(32);
			if (((startCode & ~0xff) == 0x00000100) && NalUnitStartByteByteStream[startCode & 0xff])
			{
				return (byte)(startCode & 0x1f);
			}
			if (_nalUnitStreamParser.IsShortLengthPrefixedNalUnit(startCode >> 8, (_dataReader.Length - _dataReader.Position)))
			{
				return (byte)((startCode >> 8) & 0x1f);
			}

			_dataReader.Position++;
			var nalUnitByte = (byte)(_dataReader.ShowBits(32) & 0xff);

			_dataReader.Position--;
			return (byte)(nalUnitByte & 0x1f);
		}

		private uint GetMaxGapBetweenNalUnits(NalUnitType previousNalUnitType, NalUnitType nextNalUnitType)
		{
			if (previousNalUnitType == NalUnitType.Invalid)
			{
				return 0U;
			}
			if (previousNalUnitType == NalUnitType.SequenceParameterSet)
			{
				return (nextNalUnitType == NalUnitType.PictureParameterSet) ? _maxGapBetweenPpsAndSps : _maxGapBetweenNalUnits;
			}
			if (previousNalUnitType == NalUnitType.PictureParameterSet)
			{
				return (nextNalUnitType == NalUnitType.CodedSliceOfAnIdrPicture || nextNalUnitType == NalUnitType.CodedSliceOfANonIdrPicture) ? _maxGapBetweenPpsAndSlice : _maxGapBetweenNalUnits;
			}

			return _maxGapBetweenNalUnits;
		}

		private static bool IsSlice(IResult result)
		{
			return (result.Name == IdrPictureSlice.Name) || (result.Name == NonIdrPictureSlice.Name);
		}

		private bool IsValidResult(IResultNode rootResultNode)
		{
			if (_state.ParsedHeaderCount < 2)
			{
				return false; // Less than two headers: Invalid!
			}
			if (!_falseHitReduction)
			{
				return true; // No further checks performed
			}

			if (HasResultNode(rootResultNode, r => IsSlice(r) && r.Valid))
			{
				return true; // At least one validated slice
			}
			if (HasResultNode(rootResultNode, r => (r.Name == SequenceParameterSet.Name) && r.Valid) &&
			    HasResultNode(rootResultNode, r => (r.Name == PictureParameterSet.Name) && r.Valid))
			{
				return true; // At least a valid SPS and PPS
			}

			uint validSliceCount = GetValidSliceCount(rootResultNode);
			if (validSliceCount >= 4)
			{
				return true; // At least 4 slices that are likely valid
			}
			if (validSliceCount >= 2)
			{
				return !HasResultNode(rootResultNode, r => IsSlice(r) && IsPotentialFalseHit(r));
			}

			return false; // Dropping false hit
		}

		private static bool IsPotentialFalseHit(IResultNode resultNode)
		{
			if (resultNode.Length < 0x200)
			{
				return true;
			}

			var firstMacroblockInSlice = resultNode.FindAttributeByName(FirstMacroblockInSliceName);
			if (firstMacroblockInSlice == null)
			{
				return true;
			}

			var pictureParamaterSetId = resultNode.FindAttributeByName(PictureParameterSetIdName);
			if (pictureParamaterSetId == null)
			{
				return true;
			}

			var sliceType = resultNode.FindAttributeByName(SliceTypeName);
			if (sliceType == null)
			{
				return true;
			}

			return H264Utils.ComputeFalseHitLikelihoodScore((uint)firstMacroblockInSlice.Value, (uint)pictureParamaterSetId.Value, (SliceType)(int)sliceType.Value) >= 1;
		}

		private static uint? GetFirstMacroblockInSlice(IResultNode resultNode)
		{
			if (resultNode == null)
			{
				return null;
			}

			var firstMacroblockInSlice = resultNode.FindAttributeByName(FirstMacroblockInSliceName);
			if (firstMacroblockInSlice == null)
			{
				return null;
			}

			return (uint)firstMacroblockInSlice.Value;
		}

		private static bool HasResultNode(IResultNode resultNode, Func<IResultNode, bool> predicate)
		{
			if (predicate(resultNode))
			{
				return true;
			}
			if (resultNode.HasChildren())
			{
				foreach (IResultNode childNode in resultNode.Children)
				{
					if (HasResultNode(childNode, predicate))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static uint GetValidSliceCount(IResultNode resultNode)
		{
			uint count = 0;
			if (IsSlice(resultNode))
			{
				uint? firstMacroblockInSlice = GetFirstMacroblockInSlice(resultNode);
				if ((firstMacroblockInSlice == 0) || (firstMacroblockInSlice >= 60))
				{
					// TODO: check other attributes
					++count;
				}
			}
			if (resultNode.HasChildren())
			{
				foreach (IResultNode childNode in resultNode.Children)
				{
					count += GetValidSliceCount(childNode);
				}
			}
			return count;
		}

		public bool ValidateDataBlock(IDataBlockBuilder dataBlockBuilder, long startOffset, long endOffset)
		{
			if (!IsValidResult(_scanContext.Results)) return false;

			endOffset += TrimResult();
			dataBlockBuilder.DataFormat = CodecID.H264;
			dataBlockBuilder.StartOffset = startOffset;
			dataBlockBuilder.EndOffset = endOffset;
			// TODO: _dataBlockBuilder.IsFullFile = false;

			if (_state.ReferenceHeader != null)
			{
				dataBlockBuilder.ReferenceHeaderOffset = _state.ReferenceHeaderPosition - startOffset;
				dataBlockBuilder.ReferenceHeader = _state.ReferenceHeader;
			}
			if (_state.PictureStates.Initialized)
			{
				// TODO: make a defensive copy of  the picture state
				var pictureParameterSetId = _state.PictureStates.First();
				_scanContext.ReferenceHeader = _state.PictureStates[pictureParameterSetId];
			}
			return true;
		}

		private long TrimResult()
		{
			if (_lastHeader == null)
			{
				return 0L;
			}

			// Trim zero byte stuffing from last header (if any)
			string zeroByteStuffingName = Enum.GetName(typeof(NalUnitParser.Attribute), NalUnitParser.Attribute.TrailingZeroBytes);
			var zeroByteStuffingAttribute = _lastHeader.FindAttributeByName(zeroByteStuffingName);
			if (zeroByteStuffingAttribute == null)
			{
				return 0L;
			}

			uint byteCount = (uint)zeroByteStuffingAttribute.Value;

			// Rebuild the last header, omitting the zero byte stuffing attribute
			var resultNodeBuilder = new ResultNodeBuilder
			{
				Name = _lastHeader.Name,
				Metadata = _lastHeader,
				DataPacket = _lastHeader.GetSubPacket(0, (_lastHeader.Length - byteCount))
			};
			foreach (IResultAttribute attribute in _lastHeader.Attributes.Where(a => a.Name != zeroByteStuffingName))
			{
				resultNodeBuilder.AddAttribute(attribute);
			}

			// Replace the last header with the new (trimmed) header
			IResultNode parent = _lastHeader.Parent;
			parent.RemoveChild(_lastHeader);
			parent.AddChild(resultNodeBuilder.Build());

			// Compensate end offset for zero byte trimming
			return -byteCount;
		}
	}
}
