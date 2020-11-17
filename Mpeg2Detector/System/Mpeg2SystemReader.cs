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
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.Mpeg2.System.State;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg2.System
{
	internal sealed class Mpeg2SystemReader : IMpeg2SystemReader
	{
		#region Inner classes
		private sealed class StuffingBytesResultFormatter : IValidityResultFormatter
		{
			public string Format(object value)
			{
				return BitConverter.ToString((byte[])value).Replace('-', ' ');
			}

			public bool IsValid(object value)
			{
				foreach (byte b in (byte[])value)
				{
					if (b != 0xFF)
					{
						return false;
					}
				}
				return true;
			}
		}
		#endregion Inner classes

		private readonly BitStreamDataReader _dataReader;
		private readonly IMpeg2SystemState _state;
		private readonly IReaderState _readerState;
		private readonly uint _maxZeroByteStuffingLength;
		private readonly IValidityResultFormatter _stuffingBytesResultFormatter;

		#region Properties
		public long Position { get { return _dataReader.Position; } }
		public IMpeg2SystemState State { get { return _state; } }
		public bool Valid { get { return _readerState.Valid; } }
		public long BytesRemaining { get { return (_dataReader.Length - _dataReader.Position); } }

		// TODO: check for invalid state (null)!!
		private IResultState Result { get { return _readerState.ActiveState as IResultState; } }
		#endregion Properties

		public Mpeg2SystemReader(BitStreamDataReader dataReader, IMpeg2SystemState state, IReaderState readerState)
		{
			_dataReader = dataReader;
			_state = state;
			_readerState = readerState;

			_maxZeroByteStuffingLength = (uint) Mpeg2SystemDetector.Configurable[Mpeg2SystemDetector.ConfigurationKey.MaxZeroByteStuffingLength];
			_stuffingBytesResultFormatter = new StuffingBytesResultFormatter();
		}

		public void SetMpegFormat(CodecID format)
		{
			if (State.MpegFormat == CodecID.Unknown)
			{
				State.MpegFormat = format;
			}
			else if (State.MpegFormat != format)
			{
				_readerState.Invalidate();
			}
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
			Result.AddAttribute(Mpeg2SystemHeaderParser.Attribute.Reserved, value);
			return value;
		}

		public byte GetByte()
		{
			return _dataReader.GetByte();
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

		public bool GetFlag<T>(T attribute)
		{
			bool value = (GetBits(1) == 0x1);
			Result.AddAttribute(attribute, value);
			return value;
		}

		public void GetMarker()
		{
			bool value = (GetBits(1) == 1);

			// Add and verify the marker attribute
			if (value)
			{
				Result.AddAttribute(Mpeg2SystemHeaderParser.Attribute.Marker, value);
			}
			else
			{
				Result.AddInvalidAttribute(Mpeg2SystemHeaderParser.Attribute.Marker, value);
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

		private bool IsStartCode()
		{
			return ShowBits(24) == 1;
		}

		public uint ShowBits(int numBits)
		{
			return _dataReader.ShowBits(numBits);
		}

		public void GetAttribute(IAttributeParser<IMpeg2SystemReader> parser)
		{
			Result.Parse(parser, this);
		}

		public void BreakFragment()
		{
			_state.IsFragmented = true;

			Result.Invalidate();
		}

		public void GetStuffingBytes(int numBytes)
		{
			// Read stuffing bytes
			var b = new byte[numBytes];
			int bytesRead = _dataReader.Read(b, 0, b.Length);
			_dataReader.Position += bytesRead;
			// In case fewer bytes were read, truncate the data
			if (bytesRead != b.Length)
			{
				var subArray = new byte[bytesRead];
				Array.Copy(b, subArray, bytesRead);
				b = subArray;
			}
			// Add and verify stuffing bytes attribute
			Result.AddAttribute(Mpeg2SystemHeaderParser.Attribute.StuffingBytes, b, _stuffingBytesResultFormatter);
		}

		public int SkipBytes(int bytesToSkip)
		{
			for (int i = 0; i < bytesToSkip; i++)
			{
				GetBits(8);
			}
			return bytesToSkip;
		}
	}
}
