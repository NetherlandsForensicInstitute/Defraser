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
using System.Diagnostics;
using System.Globalization;

namespace Defraser.Detector.Common
{
	public enum NumberOfEntriesType
	{
		Byte,
		UShort,
		UInt
	}

	public abstract class ByteStreamParser<THeader, THeaderName, TParser> : Parser<THeader, THeaderName, TParser>
		where THeader : Header<THeader, THeaderName, TParser>
		where TParser : Parser<THeader, THeaderName, TParser>
	{
		public bool ReadOverflow { get; protected set; }

		protected ByteStreamDataReader DataReader { get; set; }

		protected ByteStreamParser(ByteStreamDataReader dataReader)
			: base(dataReader)
		{
			DataReader = dataReader;
		}

		/// <summary>
		/// Reads the next byte from the data stream without adding a new attribute for this value.
		/// </summary>
		/// <returns>the byte value</returns>
		public byte GetByte()
		{
			if (!CheckRead(1)) return 0;

			byte value = DataReader.GetByte();
			return value;
		}

		/// <summary>
		/// Reads the next byte from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the byte value</returns>
		public byte GetByte<T>(T attributeName)
		{
			if (!CheckRead(1)) return 0;

			byte value = DataReader.GetByte();
			AddAttribute(new FormattedAttribute<T, byte>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Reads a short value from the data stream without adding
		/// a new attribute for this value.
		/// </summary>
		/// <returns>the short value</returns>
		public short GetShort()
		{
			if (!CheckRead(2)) return 0;

			return DataReader.GetShort();
		}

		/// <summary>
		/// Reads a short value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the short value</returns>
		public short GetShort<T>(T attributeName)
		{
			if (!CheckRead(2)) return 0;

			short value = DataReader.GetShort();
			AddAttribute(new FormattedAttribute<T, short>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Read a short from the data stream and add a pretty print attribute
		/// for this value using the <paramref name="enumRepresentation"/>
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="enumRepresentation">the enumeration the pretty print the value</param>
		/// <returns>the short read from the data stream</returns>
		public short GetShort<T>(T attributeName, Type enumRepresentation)
		{
			if (!CheckRead(2)) return 0;

			short value = DataReader.GetShort();

			if (Enum.IsDefined(enumRepresentation, (int)value))
			{
				AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Enum.GetName(enumRepresentation, value), value)));
			}
			else
			{
				AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0}", value)));
			}
			return value;
		}

		/// <summary>
		/// Reads a unsigned short value from the data stream without adding
		/// a new attribute for this value.
		/// </summary>
		/// <returns>the unsigned short value</returns>
		public ushort GetUShort()
		{
			if (!CheckRead(2)) return 0;

			return (ushort)DataReader.GetShort();
		}

		/// <summary>
		/// Reads a unsigned short value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the unsigned short value</returns>
        public ushort GetUShort<T>(T attributeName)
		{
            if (!CheckRead(2)) return 0;

            ushort value = (ushort)DataReader.GetShort();
            AddAttribute(new FormattedAttribute<T, ushort>(attributeName, value));
            return value;
		}

        /// <summary>
        /// Reads a unsigned short value from the data stream and
        /// adds a new attribute for this value.
        /// </summary>
        /// <typeparam name="T">the attribute name enumeration type</typeparam>
        /// <param name="attributeName">the name for the attribute</param>
        /// <param name="dataFormat">parameter for dataformatting, e.g. hex ('{0:X}').</param>
        /// <returns>the unsigned short value</returns>
		public ushort GetUShort<T>(T attributeName, string dataFormat)
		{
			if (!CheckRead(2)) return 0;

			ushort value = (ushort)DataReader.GetShort();
			AddAttribute(new FormattedAttribute<T, ushort>(attributeName, value, dataFormat));
			return value;
		}

		/// <summary>
		/// Reads an integer value from the data stream whitout adding a new
		/// attribute for this value.
		/// </summary>
		/// <returns>the integer value</returns>
		public int GetInt()
		{
			if (!CheckRead(4)) return 0;
			int value = DataReader.GetInt();
			return value;
		}

		/// <summary>
		/// Reads an integer value from the data stream whitout adding a new
		/// attribute for this value.
		/// </summary>
		/// <returns>the integer value</returns>
		public int GetInt(Endianness endianness)
		{
			if (!CheckRead(4)) return 0;
			int value = DataReader.GetInt(endianness);
			return value;
		}

		/// <summary>
		/// Reads an integer value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the integer value</returns>
		public int GetInt<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0;

			int value = DataReader.GetInt();
			AddAttribute(new FormattedAttribute<T, int>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Read an integer from the data stream and add a pretty print attribute
		/// for this value using the <paramref name="enumRepresentation"/>
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="enumRepresentation">the enumeration the pretty print the value</param>
		/// <returns>the integer read from the data stream</returns>
		public int GetInt<T>(T attributeName, Type enumRepresentation)
		{
			if (!CheckRead(4)) return 0;

			int value = DataReader.GetInt();

			if (Enum.IsDefined(enumRepresentation, value))
			{
				AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Enum.GetName(enumRepresentation, value), value)));
			}
			else
			{
				AddAttribute(new FormattedAttribute<T, string>(attributeName, string.Format(CultureInfo.CurrentCulture, "{0}", value)));
			}
			return value;
		}

		/// <summary>
		/// Reads an unsigned integer value from the data stream whitout adding a new
		/// attribute for this value.
		/// </summary>
		/// <returns>the unsigned integer value</returns>
		public uint GetUInt()
		{
			if (!CheckRead(4)) return 0;

			return (uint)DataReader.GetInt();
		}

		/// <summary>
		/// Reads an unsigned integer value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the unsigned integer value</returns>
		public uint GetUInt<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0;

			uint value = (uint)DataReader.GetInt();
			AddAttribute(new FormattedAttribute<T, uint>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Reads an unsigned integer value from the data stream and
		/// adds a new attribute for this value using the <paramref name="format"/>
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <param name="format">the format to present the value read</param>
		/// <returns>the unsigned integer read from the data stream</returns>
		public uint GetUInt<T>(T attributeName, string format)
		{
			if (!CheckRead(4)) return 0;

			uint value = (uint)DataReader.GetInt();
			AddAttribute(new FormattedAttribute<T, uint>(attributeName, value, format));
			return value;
		}

		/// <summary>
		/// Reads a long value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <returns>the long value</returns>
		public long GetLong()
		{
			if (!CheckRead(8)) return 0;
			long value = DataReader.GetLong();
			return value;
		}

		/// <summary>
		/// Reads a long value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the long value</returns>
		public long GetLong<T>(T attributeName)
		{
			if (!CheckRead(8)) return 0;
			long value = DataReader.GetLong();
			AddAttribute(new FormattedAttribute<T, long>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Reads an unsigned long value from the data stream whitout adding a new
		/// attribute for this value.
		/// </summary>
		/// <returns>the unsigned long value</returns>
		public ulong GetULong()
		{
			if (!CheckRead(8)) return 0UL;

			return (ulong)DataReader.GetLong();
		}

		/// <summary>
		/// Reads an unsigned long value from the data stream and
		/// adds a new attribute for this value.
		/// </summary>
		/// <typeparam name="T">the attribute name enumeration type</typeparam>
		/// <param name="attributeName">the name for the attribute</param>
		/// <returns>the unsigned long value</returns>
		public ulong GetULong<T>(T attributeName)
		{
			if (!CheckRead(8)) return 0UL;

			ulong value = (ulong)DataReader.GetLong();
			AddAttribute(new FormattedAttribute<T, ulong>(attributeName, value));
			return value;
		}

		public void GetHexDump<T>(T attributeName, int totalBytesToReadCount)
		{
			GetHexDump(attributeName, totalBytesToReadCount, 128);
		}

		public void GetHexDump<T>(T attributeName, int totalBytesToReadCount, int maxBytesOnDisplayCount)
		{
			if (!CheckRead(totalBytesToReadCount)) return;

			Parse(new HexDump<T, THeader, THeaderName, TParser>(attributeName, totalBytesToReadCount, maxBytesOnDisplayCount));
		}

		public void GetTable<T>(T tableName, uint numberOfEntries, uint entrySize, Table<T, THeader, THeaderName, TParser>.CreateTableEntryDelegate createTableEntry)
		{
			if (Result.Valid)
			{
				Parse(new Table<T, THeader, THeaderName, TParser>(tableName, numberOfEntries, entrySize, createTableEntry));
			}
		}

		public void GetTable<T>(T tableName, T numberOfEntriesName, NumberOfEntriesType numberOfEntriesType, uint entrySize, Table<T, THeader, THeaderName, TParser>.CreateTableEntryDelegate createTableEntry, ulong bytesRemaining)
		{
			uint numberOfEntries = GetNumberOfEntries(numberOfEntriesType, numberOfEntriesName);

			GetTable(tableName, numberOfEntriesName, numberOfEntries, entrySize, createTableEntry, bytesRemaining - NumberOfEntriesTypeLength(numberOfEntriesType));
		}

		public void GetTable<T>(T tableName, T numberOfEntriesName, uint numberOfEntries, uint entrySize, Table<T, THeader, THeaderName, TParser>.CreateTableEntryDelegate createTableEntry, ulong bytesRemaining)
		{
			if (numberOfEntries == 0) return;

			if (entrySize == 0)	// Variable length
			{
				Parse(new Table<T, THeader, THeaderName, TParser>(tableName, numberOfEntries, entrySize, createTableEntry));
			}
			else
			{
				// Use long to avoid overflow
				ulong tableSize = (ulong)numberOfEntries * entrySize;
				CheckAttribute(numberOfEntriesName, tableSize == bytesRemaining);
				if (Result.Valid)
				{
					Parse(new Table<T, THeader, THeaderName, TParser>(tableName, numberOfEntries, entrySize, createTableEntry));
				}
			}
		}

		/// <summary>
		/// Checks whether the data reader has at least <paramref name="byteCount"/>
		/// available for reading.
		/// </summary>
		/// <param name="byteCount">the number of bytes to check</param>
		/// <returns>true if the number of bytes are available, false otherwise</returns>
		protected bool CheckRead(int byteCount)
		{
			//TODO: why not just: return Position <= (Length - byteCount);
			if (ReadOverflow)
			{
				return false;
			}
			if (Position > (Length - byteCount))
			{
				Position = Length;
				ReadOverflow = true;
				throw new ReadOverflowException();
			}
			return true;
		}

		private uint GetNumberOfEntries<T>(NumberOfEntriesType numberOfEntriesType, T numberOfEntriesName)
		{
			switch (numberOfEntriesType)
			{
				case NumberOfEntriesType.Byte: return GetByte(numberOfEntriesName);
				case NumberOfEntriesType.UShort: return GetUShort(numberOfEntriesName);
				case NumberOfEntriesType.UInt: return GetUInt(numberOfEntriesName);
				default:
					Debug.Fail(string.Format("Please add support for the missing type: {0}", Enum.GetName(typeof(NumberOfEntriesType), numberOfEntriesType)));
					return 0U;
			}
		}

		/// <summary>
		/// return the length in bytes of the NumberOfEntriesType
		/// byte = 1, ushort = 2 and uint = 3
		/// </summary>
		private static uint NumberOfEntriesTypeLength(NumberOfEntriesType numberOfEntriesType)
		{
			switch (numberOfEntriesType)
			{
				case NumberOfEntriesType.Byte: return 1;
				case NumberOfEntriesType.UShort: return 2;
				case NumberOfEntriesType.UInt: return 4;
				default:
					Debug.Fail(string.Format("Please add support for the missing type: {0}", Enum.GetName(typeof(NumberOfEntriesType), numberOfEntriesType)));
					return 0;
			}
		}
	}
}
