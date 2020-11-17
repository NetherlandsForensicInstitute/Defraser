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

using System;
using System.Linq;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	[DataContract]
	public sealed class ReferenceHeader : IReferenceHeader, IEquatable<ReferenceHeader>
	{
		#region Properties
		[DataMember]
		public string Brand { get; set; }
		[DataMember]
		public string Model { get; set; }
		[DataMember]
		public string Setting { get; set; }
		[DataMember]
		public byte[] Data { get; private set; }
		[DataMember]
		public ICodecParameters CodecParameters { get; private set; }
		#endregion Properties

		public ReferenceHeader(byte[] data, ICodecParameters codecParameters)
		{
			Data = CopyOf(data);
			CodecParameters = codecParameters;
		}

		private static T[] CopyOf<T>(T[] array)
		{
			var newArray = new T[array.Length];
			Array.Copy(array, newArray, array.Length);
			return newArray;
		}

		#region Equals() and GetHashCode()
		public override bool Equals(Object obj)
		{
			return Equals(obj as ReferenceHeader);
		}

		public bool Equals(ReferenceHeader other)
		{
			if (other == null) return false;
			if (other == this) return true;

			if (!string.Equals(Brand, other.Brand)) return false;
			if (!string.Equals(Model, other.Model)) return false;
			if (!string.Equals(Setting, other.Setting)) return false;
			if (!CodecParameters.Equals(other.CodecParameters)) return false;

			return Data.SequenceEqual(other.Data);
		}

		public override int GetHashCode()
		{
			// TODO: Include 'Data' for a better hash code!
			return HashCode.EmptyHashCode
				.CombineHashCode(Brand)
				.CombineHashCode(Model)
				.CombineHashCode(Setting)
				.CombineHashCode(CodecParameters);
		}
		#endregion Equals() and GetHashCode()
	}
}
