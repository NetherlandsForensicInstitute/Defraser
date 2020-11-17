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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
		/// <summary>
	/// The default implementation of <see cref="ICodecParametersBuilder"/>.
	/// </summary>
	public sealed class CodecParametersBuilder : ICodecParametersBuilder
	{
		#region Inner classes
		/// <summary>
		/// Serializable implementation of <see cref="ICodecParameters"/>.
		/// </summary>
		[DataContract]
		public sealed class CodecParameters : ICodecParameters, IEquatable<CodecParameters>
		{
			[DataMember]
			private readonly CodecID _codec;
			[DataMember]
			private readonly uint _width;
			[DataMember]
			private readonly uint _height;
			[DataMember]
			private readonly string _frameRate;
			[DataMember]
			private readonly IDictionary<string, object> _parameters;

			#region Properties
			public CodecID Codec { get { return _codec; } }
			public uint Width { get { return _width; } }
			public uint Height { get { return _height; } }
			public string FrameRate { get { return _frameRate; } }
			public object this[string name] { get { return _parameters[name]; } }
			#endregion Properties

			internal CodecParameters(CodecParametersBuilder builder)
			{
				_codec = builder.Codec;
				_width = builder.Width;
				_height = builder.Height;
				_frameRate = builder.FrameRate;
				_parameters = new Dictionary<string, object>(builder._values);
			}

			public IEnumerator<string> GetEnumerator()
			{
				return _parameters.Keys.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _parameters.Keys.GetEnumerator();
			}

			#region Equals() and GetHashCode()
			public override bool Equals(Object obj)
			{
				return Equals(obj as CodecParameters);
			}

			public bool Equals(CodecParameters other)
			{
				if (other == null) return false;
				if (other == this) return true;

				if (Codec != other.Codec) return false;
				if (Width != other.Width) return false;
				if (Height != other.Height) return false;
				if (!string.Equals(FrameRate, FrameRate)) return false;

				// Note: Parameters are based on the header data (bytes), so they do not have to be checked!

				return true;
			}

			public override int GetHashCode()
			{
				return Codec.GetHashCode()
					.CombineHashCode(Width)
					.CombineHashCode(Height)
					.CombineHashCode(FrameRate)
					.CombineHashCode(_parameters);
			}
			#endregion Equals() and GetHashCode()
		}
		#endregion Inner classes

		private readonly IDictionary<string, object> _values;

		#region Properties
		public CodecID Codec { private get; set; }
		public uint Width { private get; set; }
		public uint Height { private get; set; }
		public string FrameRate { private get; set; }

		public object this[string name]
		{
			set { _values[name] = value; }
		}
		#endregion Properties

		public CodecParametersBuilder()
		{
			_values = new Dictionary<string, object>();
		}

		public ICodecParameters Build()
		{
			PreConditions.Operation().IsInvalidIf((Codec == CodecID.Unknown), "Codec was not set")
				.And.IsInvalidIf((Width <= 0), "Width is invalid")
				.And.IsInvalidIf((Height <= 0), "Height is invalid");

			// Note: 'FrameRate' is optional, until we can guarantee that every video codec has this property!

			return new CodecParameters(this);
		}
	}
}
