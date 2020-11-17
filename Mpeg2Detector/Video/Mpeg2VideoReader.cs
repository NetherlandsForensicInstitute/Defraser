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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.Video.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.Video
{
	internal sealed class Mpeg2VideoReader : IMpeg2VideoReader
	{
		private static readonly IDictionary<IDataPacket, ISequenceState> NoDefaultHeaders = new ReadOnlyDictionary<IDataPacket, ISequenceState>(new Dictionary<IDataPacket, ISequenceState>());

		private readonly BitStreamDataReader _dataReader;
		private readonly IMpeg2VideoState _state;
		private readonly IReaderState _readerState;
		private readonly uint _maxZeroByteStuffingLength;

		#region Properties
		public long Position{ get { return _dataReader.Position; } }
		public IMpeg2VideoState State { get { return _state; } }
		public bool Valid { get { return _readerState.Valid; } }
		public IProject Project { get { return _dataReader.GetDataPacket(0, 1).InputFile.Project; } }
		public IDictionary<IDataPacket, ISequenceState> ReferenceHeaders { private get; set; }

		// TODO: check for invalid state (null)!!
		private IResultState Result { get { return _readerState.ActiveState as IResultState; } }
		#endregion Properties

		public Mpeg2VideoReader(BitStreamDataReader dataReader, IMpeg2VideoState state, IReaderState readerState)
		{
			_dataReader = dataReader;
			_state = state;
			_readerState = readerState;

			_maxZeroByteStuffingLength = (uint)Mpeg2VideoDetector.Configurable[Mpeg2VideoDetector.ConfigurationKey.ParserMaxZeroByteStuffingLength];

			ReferenceHeaders = NoDefaultHeaders;
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		public uint NextStartCode()
		{
			// Start code format is 23 bits (0), 1 bit (1), 8 bits (id)
			return _dataReader.NextStartCode(24, 0x000001, 8);
		}

		public void FlushBits(int numBits)
		{
			_dataReader.GetBits(numBits);
		}

		public uint GetReservedBits(int numBits)
		{
			uint value = GetBits(numBits);
			Result.AddAttribute(Mpeg2VideoHeaderParser.Attribute.Reserved, value);
			return value;
		}

		public uint GetBits(int numBits)
		{
			return _dataReader.GetBits(numBits);
		}

		public uint GetBits<T>(int numBits, T attribute)
		{
			uint value = GetBits(numBits);
			Result.AddAttribute(attribute, value);
			return value;
		}

		public uint GetBits<T>(int numBits, T name, Func<uint, bool> check)
		{
			uint value = GetBits(numBits);
			if (check(value))
			{
				Result.AddAttribute(name, value);
			}
			else
			{
				Result.AddInvalidAttribute(name, value);
			}
			return value;
		}

		public uint GetBits<T>(int numBits, T attribute, string format)
		{
			uint value = GetBits(numBits);
			Result.AddAttribute(attribute, value, new StringResultFormatter(format));
			return value;
		}

		public int GetBits<T>(int numBits, T attribute, IValidityResultFormatter optionResultFormatter)
		{
			int value = (int)GetBits(numBits);

			// Add and verify the option attribute
			Result.AddAttribute(attribute, value, optionResultFormatter);
			return value;
		}

		public bool GetFlag<T>(T attribute)
		{
			return GetFlag(1, 0x1, attribute);
		}

		public bool GetFlag<T>(int numBits, uint bitCode, T attribute)
		{
			bool value = (GetBits(numBits) == bitCode);
			Result.AddAttribute(attribute, value);
			return value;
		}

		public void GetMarker()
		{
			bool value = (GetBits(1) == 1);

			// Add and verify the marker attribute
			if (value)
			{
				Result.AddAttribute(Mpeg2VideoHeaderParser.Attribute.Marker, value);
			}
			else
			{
				Result.AddInvalidAttribute(Mpeg2VideoHeaderParser.Attribute.Marker, value);
			}
		}

		public void GetData<T>(T attribute, int numBytes)
		{
			for (int i = 0; i < numBytes; i++)
			{
				GetBits(8);
			}
			//_dataReader.Position += numBytes;
			Result.AddAttribute(attribute, numBytes);
		}

		public uint GetZeroByteStuffing<T>(T attributeName)
		{
			long position = _dataReader.Position;

			// Find start code within MaxZeroPadding bytes
			_dataReader.ByteAlign();

			for (uint byteCount = 0; byteCount < _maxZeroByteStuffingLength; byteCount++)
			{
				if (IsStartCode())
				{
					// Start code found: Zero padding is part of the header
					if (byteCount > 0)
					{
						Result.AddAttribute(attributeName, byteCount);
					}
					return byteCount;
				}
				if (GetBits(8) != 0)
				{
					break;
				}
			}

			// No start code found, rewind ...
			_dataReader.Position = position;

			return 0;
		}

		public void AddDerivedAttribute<T>(T attribute, uint value)
		{
			Result.AddAttribute(attribute, value);
		}

		private bool IsStartCode()
		{
			return ShowBits(24) == 1;
		}

		public uint ShowBits(int numBits)
		{
			return _dataReader.ShowBits(numBits);
		}

		public T GetVlc<T>(VlcTable<T> vlcTable)
		{
			return _dataReader.GetVlc(vlcTable);
		}

		public bool HasBytes(int requiredBytes)
		{
			var requiredEndPosition = _dataReader.Position+requiredBytes;
			return (requiredEndPosition <= _dataReader.Length);
		}

		public void GetAttribute(IAttributeParser<IMpeg2VideoReader> parser)
		{
			Result.Parse(parser, this);
		}

		public void BreakFragment()
		{
			_state.IsFragmented = true;

			Result.Invalidate();
		}

		public void InsertReferenceHeaderBeforeStartCode()
		{
			if (!_state.ReferenceHeadersTested && (ReferenceHeaders.Count >= 1))
			{
				_state.ReferenceHeaderPosition = Position - 4;
			}
		}

		public bool TryDefaultHeaders(IResultState resultState, Action evaluateHeader)
		{
			if (_state.ReferenceHeadersTested)
			{
				return false; // TODO: If the first picture fail to decode using any of the reference headers, subsequent pictures that may work will not be tested!
			}

			// This will make sure the default headers are tested just once and not for every slice
			_state.ReferenceHeadersTested = true;

			if (ReferenceHeaders.Count == 0)
			{
				return false;
			}

			long startPosition = Position;
			bool isMpeg2 = _state.Picture.HasExtension(ExtensionId.PictureCodingExtensionId);

			// Try reference headers to decode this slice
			foreach (var referenceHeader in ReferenceHeaders)
			{
				// Ignore reference headers with MPEG-1 <-> MPEG-2 mismatch
				if (referenceHeader.Value.HasExtension(ExtensionId.SequenceExtensionId) == isMpeg2)
				{
					referenceHeader.Value.CopyTo(_state.Sequence);

					evaluateHeader();
					//ParseSlice(reader, resultState);

					if (resultState.Valid)
					{
						_state.ReferenceHeader = referenceHeader.Key;
						return true; // Header successfully decoded!
					}
				}

				resultState.Reset();
				_dataReader.Position = startPosition;
			}

			// Not able to decode slice using any default header, defaulting to unparsable slices
			_state.Sequence.Reset();

			return false;
		}
	}
}
