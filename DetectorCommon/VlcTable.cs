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

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Provides efficient decoding of Variable-Length-Codes (VLC) using
	/// look-up tables, i.e. <see cref="VlcLut"/>.
	/// </summary>
	/// <typeparam name="T">the type of result values</typeparam>
	public class VlcTable<T>
	{
		public const byte InvalidInteger = byte.MaxValue;
		public static readonly int[] Invalid = new int[] { InvalidInteger, InvalidInteger };

		private readonly Dictionary<Vlc, T> _values;
		private readonly VlcLut _lut;
		private readonly T _defaultValue;

		#region Properties
		/// <summary>The maximum number of bits for VLCs.</summary>
		public int MaxBits { get { return _lut.Bits; } }
		/// <summary>The value for given <paramref name="vlc"/></summary>
		public T this[Vlc vlc] { get { return _values[vlc]; } }
		/// <summary>The value returned for a bitvalue not found in the VLC-table</summary>
		public T DefaultValue { get {return _defaultValue; } }
		#endregion Properties

		/// <summary>
		/// Creates a VLC-table for the bit codes in <param name="data"/>.
		/// The <paramref name="data"/> is a list of bitcode and value pairs.
		/// Bitcode is encoded as a <c>string</c>, e.g. <c>"0010"</c>,
		/// and value is expected to be of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="data">array of bitcode, value pairs</param>
		/// <param name="defaultValue">the value returned for a value not found in the VLC-table</param>
		public VlcTable(object[,] data, T defaultValue)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			_defaultValue = defaultValue;
			_values = new Dictionary<Vlc, T>();
			_lut = new VlcLut(0, 0, GetMaxBits(data));

			// Add the codes to the table
			for (int i = 0; i < data.GetLength(0); i++)
			{
				string bitcode = data[i, 0] as string;
				if (bitcode == null || !(data[i, 1] is T))
				{
					throw new ArgumentException(string.Format("Invalid data at index {0}", i), "data");
				}

				Vlc vlc = new Vlc(Convert.ToUInt32(bitcode, 2), bitcode.Length);
				_values[vlc] = (T)data[i, 1];
				_lut.AddVlc(vlc);
			}
		}

		/// <summary>
		/// Retrieves the VLC code correspondig to a bit <paramref name="code"/>.
		/// </summary>
		/// <param name="code">the code of length <c>Bits</c></param>
		/// <returns>the VLC code, null if </c>code<c> is not in this table</returns>
		public Vlc GetVlc(uint code)
		{
			return _lut.GetVlc(code);
		}

		/// <summary>
		/// Computes the maximum number of bits in a VLC, which determines
		/// the size of the LUT.
		/// </summary>
		/// <param name="data">the VLC data</param>
		/// <returns>the maximum number of bits in any VLC</returns>
		private static int GetMaxBits(object[,] data)
		{
			int bits = 0;
			for (int i = 0; i < data.GetLength(0); i++)
			{
				bits = Math.Max(bits, ((string) data[i, 0]).Length);
			}
			return bits;
		}
	}


	/// <summary>
	/// Variable-Length-Codes (VLC) lookup table for efficient decoding.
	/// </summary>
	internal class VlcLut : Vlc
	{
		/// <summary>Maximum size of the lookup tables (LUT) in bits.</summary>
		public static readonly int MaxLutSize = 8;

		private readonly Vlc[] _lut;
		private readonly int _bits;

		#region Properties
		/// <summary>The (maximum) number of bits for VLC codes.</summary>
		public int Bits { get { return _bits; } }
		#endregion Properties

		/// <summary>
		/// Creates a VLC lookup table for bit patterns that start
		/// with <paramref name="prefixCode"/>.
		/// </summary>
		/// <param name="prefixCode">the code prefix</param>
		/// <param name="prefixBits">the prefix length in bits</param>
		/// <param name="numBits">the total number of bits</param>
		public VlcLut(uint prefixCode, int prefixBits, int numBits)
			: base(prefixCode << GetLutBits(prefixBits, numBits),
				   prefixBits + GetLutBits(prefixBits, numBits))
		{
			_lut = new Vlc[1 << GetLutBits(prefixBits, numBits)];
			_bits = numBits;
		}

		/// <summary>
		/// Retrieves the VLC code correspondig to a bit <paramref name="code"/>.
		/// </summary>
		/// <param name="code">the code of length <c>Bits</c></param>
		/// <returns>the VLC code, null if </c>code<c> is not in this table</returns>
		public Vlc GetVlc(uint code)
		{
			Vlc vlc = _lut[(code >> (_bits - Length)) - Code];
			VlcLut table = (vlc as VlcLut);

			return (table == null) ? vlc : table.GetVlc(code);
		}

		/// <summary>
		/// Adds <paramref name="vlc"/> to this table and recursively creates
		/// new tables when needed.
		/// </summary>
		/// <param name="vlc">the VLC to add</param>
		public void AddVlc(Vlc vlc)
		{
			int n = Length - vlc.Length;

			if (n >= 0)
			{
				uint c = (vlc.Code << n) - Code;

				// Insert code into this table
				for (int i = 0; i < (1 << n); i++)
				{
					_lut[c + i] = vlc;
				}
			}
			else
			{
				uint c = (vlc.Code >> (-n)) - Code;
				VlcLut table = _lut[c] as VlcLut;

				// Follow chain of LUTs
				if (table == null)
				{
					_lut[c] = table = new VlcLut(Code + c, Length, Bits);
				}

				table.AddVlc(vlc);
			}
		}

		private static int GetLutBits(int prefixBits, int numBits)
		{
			return Math.Min(MaxLutSize, numBits - prefixBits);
		}
	}
}
