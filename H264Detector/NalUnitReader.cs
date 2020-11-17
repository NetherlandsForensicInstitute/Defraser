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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264.State;
using Defraser.Interface;

namespace Defraser.Detector.H264
{
	internal sealed class NalUnitReader : INalUnitReader
	{
		private static readonly byte[] ExpGolombNumBitsTable;

		static NalUnitReader()
		{
			ExpGolombNumBitsTable = new byte[65536];

			for (int leadingZeroBits = 0; leadingZeroBits < 16; leadingZeroBits++)
			{
				byte numBits = (byte)((2 * leadingZeroBits) + 1);
				int firstCodeNum = 1 << (15 - leadingZeroBits);
				// Suffix bit pattern upto bit 15 (including leading zeros)!
				for (int suffixBitPattern = 0; suffixBitPattern < firstCodeNum; suffixBitPattern++)
				{
					ExpGolombNumBitsTable[firstCodeNum | suffixBitPattern] = numBits;
				}
			}
		}

		private readonly IH264Reader _reader;
		private readonly BitStreamDataReader _dataReader;
		private readonly IH264State _state;
		private readonly uint _maxTrailingZeroByteLength;
		private readonly uint _maxReferenceHeaderRetriesPerFragment;

		#region Properties
		public IResultState Result { get; set; }
		public IH264State State { get { return _state; } }

		/// <summary>The current location to read from, in bytes.</summary>
		public long Position
		{
			get { return _dataReader.Position; }
			set { _dataReader.Position = value; }
		}

		/// <summary>
		/// The number of total bytes in the stream
		/// </summary>
		public long Length
		{
			get { return _dataReader.Length; }
		}

		private IDictionary<IDataPacket, IPictureState> ReferenceHeaders { get { return _reader.ReferenceHeaders; } }
		#endregion Properties

		public NalUnitReader(IH264Reader reader, BitStreamDataReader dataReader, IH264State state)
		{
			_reader = reader;
			_dataReader = dataReader;
			_state = state;

			_maxTrailingZeroByteLength = (uint)H264Detector.Configurable[H264Detector.ConfigurationKey.MaxTrailingZeroByteLength];
			_maxReferenceHeaderRetriesPerFragment = (uint)H264Detector.Configurable[H264Detector.ConfigurationKey.MaxReferenceHeaderRetriesPerFragment];
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		public uint ShowBits(int numBits)
		{
			return _dataReader.ShowBits(numBits);
		}

		public bool GetBit()
		{
			return _dataReader.GetBits(1) == 1;
		}

		public bool GetBit<T>(T attributeName)
		{
			bool value = GetBit();
			Result.AddAttribute(attributeName, value);
			return value;
		}

		public uint GetBits(int numbits)
		{
			return _dataReader.GetBits(numbits);
		}

		public uint GetBits<T>(int numBits, T attribute)
		{
			uint value = GetBits(numBits);
			Result.AddAttribute(attribute, value);
			return value;
		}

		public TEnum GetBits<TAttribute, TEnum>(int numBits, TAttribute attribute, EnumResultFormatter<TEnum> enumResultFormatter)
		{
			int value = (int)GetBits(numBits);

			// Add and verify the option attribute
			Result.AddAttribute(attribute, value, enumResultFormatter);
			if (!enumResultFormatter.IsValid(value))
			{
				return default(TEnum);
			}
			return (TEnum)Enum.ToObject(typeof(TEnum), value); // FIXME: this is possibly too slow!!
		}

		public byte GetByte(bool align)
		{
			if (!HasBytes(1))
			{
				Result.Invalidate();
				return 0;
			}
			if (align)
			{
				return _dataReader.GetByte();
			}

			return (byte)_dataReader.GetBits(8);
		}

		public byte GetByte<T>(bool align, T attributeName, byte maxValue)
		{
			byte value = GetByte(align);
			if (value > maxValue)
			{
				Result.AddInvalidAttribute(attributeName, value);
			}
			else
			{
				Result.AddAttribute(attributeName, value);
			}
			return Math.Min(value, maxValue);
		}

		public uint GetFixedBits<T>(int numBits, uint bitCode, T attributeName)
		{
			uint value = GetBits(numBits);
			if (value == bitCode)
			{
				Result.AddAttribute(attributeName, value);
			}
			else
			{
				Result.AddInvalidAttribute(attributeName, value);
			}
			return value;
		}

		public uint GetExpGolombCoded()
		{
			// Use efficient implementation for short (1-31 bits) exp-golomb codes
			uint nextSixteenBits = _dataReader.ShowBits(16);
			if (nextSixteenBits >= 0x0001)
			{
				byte numBits = ExpGolombNumBitsTable[nextSixteenBits];
				return _dataReader.GetBits(numBits) - 1;
			}

			// Longer exp-golomb codes (33-63 bits) require two GetBits() calls!!
			uint nextThirtyTwoBits = _dataReader.ShowBits(32);
			if (nextThirtyTwoBits >= 0x00000001)
			{
				byte numBitsMinusThirtyTwo = ExpGolombNumBitsTable[nextThirtyTwoBits];
				_dataReader.GetBits(numBitsMinusThirtyTwo);
				return _dataReader.GetBits(32) - 1;
			}

			// Note: exp-golomb codes of more than 63 bits are not supported!
			Result.Invalidate();
			return uint.MaxValue; // Indicates an illegal exp-golomb code!
		}

		public uint GetExpGolombCoded(uint maxCodeNum)
		{
			uint value = GetExpGolombCoded();
			if (value > maxCodeNum)
			{
				Result.Invalidate();
				value = maxCodeNum;
			}
			return value;
		}

		public uint GetExpGolombCoded<T>(T attributeName, uint maxCodeNum)
		{
			uint value = GetExpGolombCoded();
			if (value > maxCodeNum)
			{
				Result.AddInvalidAttribute(attributeName, value);
				return maxCodeNum;
			}

			Result.AddAttribute(attributeName, value);
			return value;
		}

		public uint GetExpGolombCoded<T>(T attributeName, IValidityResultFormatter optionResultFormatter)
		{
			uint value = GetExpGolombCoded();

			// Add and verify the option attribute
			// FIXME: is the cast to 'int' necessary!?
			Result.AddAttribute(attributeName, (int)value, optionResultFormatter);
			return value;
		}

		public int GetSignedExpGolombCoded()
		{
			uint codeNum = GetExpGolombCoded();
			int sign = (int)((codeNum & 1) << 1) - 1;
			return sign * (int)((codeNum + 1) >> 1);
		}

		public int GetSignedExpGolombCoded(int minValue, int maxValue)
		{
			int value = GetSignedExpGolombCoded();
			if ((value < minValue) || (value > maxValue))
			{
				Result.Invalidate();
				value = maxValue;
			}
			return value;
		}

		public int GetSignedExpGolombCoded<T>(T attributeName, int minValue, int maxValue)
		{
			int value = GetSignedExpGolombCoded();
			if ((value < minValue) || (value > maxValue))
			{
				Result.AddInvalidAttribute(attributeName, value);
				return maxValue;
			}

			Result.AddAttribute(attributeName, value);
			return value;
		}

		public void GetPcmSamples()
		{
			ReadZeroAlignmentBits(); // pcm_alignment / pcm_alignment_zero_bit

			if (Result.Valid)
			{
				ISequenceState sequence = State.SliceState.PictureState.SequenceState;
				var bitDepthLuma = (int)sequence.BitDepthLuma;
				for (uint i = 0; i < 256; i++)
				{
					GetBits(bitDepthLuma); // pcm_sample_luma[i]
				}
				if (sequence.ChromaFormat.NumC8X8 != 0)
				{
					uint chromaSamplesPerMb = (sequence.ChromaFormat.NumC8X8 << 7);
					var bitDepthChroma = (int)sequence.BitDepthChroma;
					for (uint i = 0; i < chromaSamplesPerMb; i++)
					{
						GetBits(bitDepthChroma); // pcm_sample_chroma[i]
					}
				}
			}
		}

		public void InsertReferenceHeaderBeforeNalUnit()
		{
			if (IsDetectingReferenceHeader())
			{
				_state.ReferenceHeaderPosition = _reader.Position;
			}
		}

		private bool IsDetectingReferenceHeader()
		{
			if (ReferenceHeaders.Count == 0)
			{
				return false; // No reference header configured
			}
			if (_state.ReferenceHeader != null)
			{
				return false; // Reference header already detected
			}
			if (_state.ReferenceHeadersTestCount < _maxReferenceHeaderRetriesPerFragment)
			{
				return true; // Not yet tested, or just once: Try all reference headers
			}
			if (_state.SliceCount == 0)
			{
				return true; // Already tested but no suitable reference header: Try again!
			}

			// TODO: If the first picture fails to decode using any of the reference headers, subsequent pictures that may work will not be tested!

			return false; // Already tested!
		}

		public bool TryDefaultHeaders(IResultNodeState resultState, Action evaluateHeader)
		{
			if (!IsDetectingReferenceHeader() || !resultState.Valid)
			{
				return false;
			}

			// This will make sure the reference headers are tested no more than twice (and not for every slice)
			_state.ReferenceHeadersTestCount++;

			long startPosition = Position;

			// Try reference headers to decode this slice
			foreach (var referenceHeader in ReferenceHeaders)
			{
				// Ignore reference headers with 'byte stream' <-> 'NAL unit stream' format mismatch
				if (referenceHeader.Value.SequenceState.ByteStreamFormat == _state.ByteStreamFormat)
				{
					referenceHeader.Value.CopyTo(_state);

					evaluateHeader();

					// The restult state should be valid *AND* the slice should actually have been decoded!!!!
					if (resultState.Valid && (_state.SliceState != null))
					{
						_state.ReferenceHeader = referenceHeader.Key;
						return true; // Header successfully decoded!
					}
				}

				_state.SequenceStates.Clear();
				_state.PictureStates.Clear();

				resultState.Reset();
				Position = startPosition;
			}

			// Not able to decode slice using any reference header, defaulting to unparsable slices
			return false;
		}

		public uint GetTruncatedExpGolombCoded(uint maxCodeNum)
		{
			if (maxCodeNum == 1)
			{
				return 1 - GetBits(1);
			}

			var value = GetExpGolombCoded();
			if (value > maxCodeNum)
			{
				Result.Invalidate();
				value = maxCodeNum;
			}
			return value;
		}

		public int GetLeadingZeroBits()
		{
			for (int leadingZeroBits = 0; leadingZeroBits < 32; leadingZeroBits++)
			{
				if (GetBit())
				{
					return leadingZeroBits;
				}
			}

			Result.Invalidate();
			return 32; // Insane large number
		}

		public T GetVlc<T>(VlcTable<T> vlcTable)
		{
			Vlc vlc = vlcTable.GetVlc(ShowBits(vlcTable.MaxBits));

			if (vlc == null)
			{
				return vlcTable.DefaultValue;
			}
			GetBits(vlc.Length);	// Flush the bits
			return vlcTable[vlc];
		}

		public void ReadZeroAlignmentBits()
		{
			while (!_dataReader.ByteAligned)			// while(!byte_aligned())
			{
				if (GetBits(1) != 0)	// rbsp_alignment_zero_bit (equal to 0)
				{
					Result.Invalidate();
					return;
				}
			}
		}

		public void GetCabacAlignmentOneBits()
		{
			while (!_dataReader.ByteAligned)			// while(!byte_aligned())
			{
				if (GetBits(1) != 1)	// cabac_alignment_one_bit (equal to 1)
				{
					Result.Invalidate();
					return;
				}
			}
		}

		private bool HasBytes(int requiredBytes)
		{
			var requiredEndPosition = _dataReader.Position + requiredBytes;
			return (requiredEndPosition <= _dataReader.Length);
		}

		public bool HasMoreRbspData()
		{
			if (!Result.Valid)
			{
				return false; // If the result has been invalidated, no more bits can be read
			}
			if (_dataReader.Position < (_dataReader.Length - 1))
			{
				return true; // Yes, we do have more RBSP data!
			}
			if ((_dataReader.Position < _dataReader.Length) && !_dataReader.ByteAligned)
			{
				return true; // More than 1 byte left, so there is more data to be read
			}

			// Except for the (possible) stop bit, all remaining bits must be (0) when
			// the end of the RBSP data has been reached.
			return (_dataReader.ShowBits(8) & 0x7f) != 0;
		}

		public bool IsPossibleStopBit()
		{
			int bitsRemaining = (8 - _dataReader.BitOffset);
			uint stopBitAndZeroPaddingBits = (1U << (bitsRemaining - 1));
			return _dataReader.ShowBits(bitsRemaining) == stopBitAndZeroPaddingBits;
		}
	}
}
