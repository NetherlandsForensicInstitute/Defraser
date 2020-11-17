/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System.Globalization;
using System.Text;
using System;

namespace Defraser.Detector.Common
{
	public abstract class BitStreamParser<THeader, THeaderName, TParser> : Parser<THeader, THeaderName, TParser>
		where THeader : Header<THeader, THeaderName, TParser>
		where TParser : Parser<THeader, THeaderName, TParser>
	{
		public bool ByteAligned { get { return DataReader.ByteAligned; } }
		public bool ReadOverflow { get; private set; }
		protected abstract uint MaxZeroByteStuffingLength { get; }
		protected abstract bool IsStartCode();
		protected BitStreamDataReader DataReader { get; set; }

		protected BitStreamParser(BitStreamDataReader dataReader)
			: base(dataReader)
		{
			DataReader = dataReader;
		}

		/// <summary>
		/// Retrieves the next 8-bits or next byte-aligned byte from the data and
		/// increments <c>Position</c>.
		/// </summary>
		/// <returns>the byte or 8-bits read</returns>
		public byte GetByte(bool align)
		{
			if (!CheckRead(1)) return 0;

			if (align)
			{
				return DataReader.GetByte();
			}
			return (byte)DataReader.GetBits(8);
		}

		/// <summary>
		/// Retrieves the next byte from the data (byte-aligned) and
		/// increments <c>Position</c>.
		/// </summary>
		/// <returns>the byte read</returns>
		public byte GetByte()
		{
			return GetByte(true);
		}

		/// <summary>
		/// Retrieves the next byte from the data (byte-aligned) and
		/// increments <c>Position</c>.
		/// </summary>
		/// <returns>the byte read</returns>
		public byte GetByte<T>(bool align, T attributeName)
		{
			if (!CheckRead(1)) return 0;

			byte value = GetByte(align);
			AddAttribute(new FormattedAttribute<T, byte>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Retrieves the next byte from the data (byte-aligned) and
		/// increments <c>Position</c>.
		/// </summary>
		/// <returns>the byte read</returns>
		public byte GetByte<T>(T attributeName)
		{
			return GetByte(true, attributeName);
		}

		/// <summary>
		/// Shows (peeks) the value of the next bit from the underlying
		/// data stream without advancing the stream pointer.
		/// </summary>
		/// <returns>the value of the bit. True for 1, false for 0</returns>
		public bool ShowBit()
		{
			if (!CheckReadBits(1)) return false;

			return DataReader.ShowBits(1) != 0;
		}

		/// <summary>
		/// Get a marker bit from the stream
		/// </summary>
		/// <returns>true when the marker bit is set, else false</returns>
		public bool GetMarkerBit()
		{
			return GetBit();
		}

		/// <summary>
		/// Get a zero bit from the data stream
		/// </summary>
		/// <returns>true when the bit is zero, else false</returns>
		public bool GetZeroBit()
		{
			return GetBit() == false;
		}

		/// <summary>
		/// Get a one bit from the data stream
		/// </summary>
		/// <returns>true when the bit is one, else false</returns>
		public bool GetOneBit()
		{
			return GetBit();
		}

		/// <summary>
		/// Retrieves one bit from the underlying data stream.
		/// </summary>
		/// <returns>the value of the bit. true for 1, false for 0</returns>
		public bool GetBit()
		{
			if (!CheckReadBits(1)) return false;

			return GetBits(1) == 0 ? false : true;
		}

		public bool GetBit<T>(T attributeName)
		{
			if (!CheckReadBits(1)) return false;

			bool value = GetBits(1) == 0 ? false : true;
			AddAttribute(new FormattedAttribute<T, bool>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Retrieves given number of bits from the underlying data stream.
		/// </summary>
		/// <param name="numBits">the number of bits to retrieve</param>
		/// <returns>the bits</returns>
		public uint GetBits(int numBits)
		{
			if (!CheckReadBits(numBits)) return 0;

			return DataReader.GetBits(numBits);
		}

		public uint GetBits<T>(int numBits, T attribute)
		{
			if (!CheckReadBits(numBits)) return 0;

			uint value = GetBits(numBits);

			if (attribute is int)
			{
				uint correctValue = (uint)(int)(object)attribute;

				// Add and verify the attribute
				FormattedAttribute<string, string> formattedAttribute = new FormattedAttribute<string, string>(GetBitString(numBits, correctValue), GetBitString(numBits, value));
				formattedAttribute.Valid = (value == correctValue);
				AddAttribute(formattedAttribute);
			}
			else
			{
				AddAttribute(new FormattedAttribute<T, uint>(attribute, value));
			}
			return value;
		}


		public uint GetBits<T>(int numBits, T attribute, Type enumRepresentation)
		{
			if (!CheckReadBits(numBits)) return 0;

			uint value = GetBits(numBits);

			if (attribute is int)
			{
				uint correctValue = (uint)(int)(object)attribute;

				// Add and verify the attribute
				FormattedAttribute<string, string> formattedAttribute = new FormattedAttribute<string, string>(GetBitString(numBits, correctValue), GetBitString(numBits, value));
				formattedAttribute.Valid = (value == correctValue);
				AddAttribute(formattedAttribute);
			}
			else
			{
				if (Enum.IsDefined(enumRepresentation, (int)value))
				{
					AddAttribute(new FormattedAttribute<T, string>(attribute, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Enum.GetName(enumRepresentation, value), value)));
				}
				else
				{
					AddAttribute(new FormattedAttribute<T, uint>(attribute, value));
				}
			}
			return value;
		}

		/// <summary>
		/// Proceed the bit position <paramref name="numBits"/> bits.
		/// </summary>
		/// <param name="numBits">the number of bits to flush</param>
		public void FlushBits(int numBits)
		{
			if (numBits == 0 || !CheckReadBits(numBits)) return;

			GetBits(numBits);
		}

		/// <summary>
		/// Shows (peeks) the next number of bits from the underlying
		/// data stream without advancing the stream pointer.
		/// </summary>
		/// <param name="numBits">the number of bits to show</param>
		/// <returns>the bits</returns>
		public uint ShowBits(int numBits)
		{
			if (!CheckReadBits(numBits)) return 0;

			return DataReader.ShowBits(numBits);
		}

		public void ByteAlign()
		{
			DataReader.ByteAlign();
		}

		/// <summary>
		/// Parses zero byte stuffing after the header.
		/// </summary>
		/// <returns>the number of zero stuffing bytes</returns>
		public uint GetZeroByteStuffing<T>(T attributeName)
		{
			long position = Position;

			// Maximum number of zero stuffing bytes.
			uint maxZeroByteStuffingLength = MaxZeroByteStuffingLength;

			// Find start code within MaxZeroPadding bytes
			ByteAlign();

			for (uint byteCount = 0; byteCount < maxZeroByteStuffingLength; byteCount++)
			{
				if (IsStartCode())
				{
					// Start code found: Zero padding is part of the header
					if (byteCount > 0)
					{
						AddAttribute(new FormattedAttribute<T, uint>(attributeName, byteCount));
					}
					return byteCount;
				}
				if (GetBits(8) != 0)
				{
					break;
				}
			}

			// No start code found, rewind ...
			Position = position;

			return 0;
		}

		/// <summary>
		/// Checks whether the data reader has at least <paramref name="numBytes"/>
		/// available for reading.
		/// </summary>
		/// <param name="numBytes">the number of bytes to check</param>
		/// <returns>true if the number of bytes are available, false otherwise</returns>
		protected bool CheckRead(int numBytes)
		{
			if (ReadOverflow)
			{
				return false;
			}
			if (Position > (Length - numBytes))
			{
				Position = Length;
				ReadOverflow = true;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Checks whether the data reader has at least <paramref name="numBits"/>
		/// available for reading.
		/// </summary>
		/// <param name="numBits">the number of bits to check</param>
		/// <returns>true if the number of bits are available, false otherwise</returns>
		protected bool CheckReadBits(int numBits)
		{
			if (ReadOverflow)
			{
				return false;
			}
			//if (_dataReader.Position > _dataReader.Length - numBits) // TODO implement
			//{
			//    _dataReader.Position = _dataReader.Length;
			//    ReadOverflow = true;
			//    return false;
			//}
			return true;
		}

		private static string GetBitString(int bits, uint value)
		{
			StringBuilder sb = new StringBuilder(bits + 2);
			sb.Append('\'');
			for (int i = 0; i < bits; i++)
			{
				sb.Append(((value & (1 << (bits - 1 - i))) == 0) ? '0' : '1');
			}
			sb.Append('\'');
			return sb.ToString();
		}

		public int GetBits<T>(int numBits, T attribute, string[] options)
		{
			int value = (int)GetBits(numBits);

			// Add and verify the option attribute
			OptionAttribute<T> optionAttribute = new OptionAttribute<T>(attribute, value, options, false);
			optionAttribute.Valid = (options[value] != null);
			AddAttribute(optionAttribute);
			return value;
		}

		public uint GetBits<T>(int numBits, T attribute, string format)
		{
			uint value = GetBits(numBits);
			AddAttribute(new FormattedAttribute<T, uint>(attribute, value, format));
			return value;
		}

		public T GetVlcResult<T>(VlcTable<T> vlcTable)
		{
			return DataReader.GetVlc(vlcTable);
		}

		public Vlc GetVlc<T>(VlcTable<T> vlcTable)
		{
			Vlc vlc = vlcTable.GetVlc(ShowBits(vlcTable.MaxBits));

			if (vlc != null)	// Flush the bits
			{
				DataReader.GetBits(vlc.Length);
			}
			return vlc;
		}
	}
}
