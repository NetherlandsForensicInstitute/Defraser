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
using System.Text;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;

namespace Defraser.Detector.Mpeg2.Video
{
	/// ISO/IEC 13818-2, §6.3.4.1
	/// <remarks>
	/// A similar UserData class is used for the MPEG-4 detector.
	/// If you change something here, you might also want to change the
	/// MPEG-4 class.
	/// </remarks>
	internal class UserData : IVideoHeaderParser
	{
		#region Inner classes
		/// <summary>
		/// Describes the user data bytes.
		/// </summary>
		/// <remarks>The result may be truncated for performance reasons.</remarks>
		private class UserDataAttribute : IAttributeParser<IMpeg2VideoReader>
		{
			public void Parse(IMpeg2VideoReader reader, IAttributeState resultState)
			{
				resultState.Name = Attribute.UserDataBytes;

				// User data size limit (4 KB)
				uint maxUserDataLength = reader.State.Configuration.UserDataMaxLength;

				// Verify and copy user data bytes
				var sb = new StringBuilder();
				while ((reader.ShowBits(24) != 1) &&  reader.HasBytes(1))
				{
					// Verify that there are no 23 or more consecutive zero bits
					if ((reader.ShowBits(23) == 0) && reader.HasBytes(3))
					{
						if (!IsValidUserDataThatEndsWithOneOrTwoZeroBytes(reader))
						{
							resultState.Invalidate();
						}
						break;
					}

					// Read next byte
					sb.Append((char)reader.GetBits(8));

					if (sb.Length > maxUserDataLength)
					{
						resultState.Invalidate();
						break;
					}
				}

				// TODO: the formatter should perform the hexdump (to reduce memory footprint)
				resultState.Value = sb.ToString();
			}

			private static bool IsValidUserDataThatEndsWithOneOrTwoZeroBytes(IMpeg2VideoReader reader)
			{
				reader.FlushBits(8);
				if (reader.ShowBits(24) == 1)
				{
					// 1 byte '00', followed by next start code
					return true;
				}

				reader.FlushBits(8);
				if (reader.ShowBits(24) == 1)
				{
					// 2 bytes '00 00', followed by next start code
					return true;
				}
				return false;
			}
		}
		#endregion Inner classes

		private enum Attribute
		{
			UserDataBytes
		}

		internal const string Name = "UserData";

		private readonly IAttributeParser<IMpeg2VideoReader> _userDataAttribute;

		#region Properties
		public uint StartCode { get { return 0x1b2; } }
		#endregion Properties

		public UserData()
		{
			_userDataAttribute = new UserDataAttribute();
		}

		public void AddColumnsTo(IDetectorColumnsBuilder builder)
		{
			builder.AddColumnNames(Name, Enum.GetNames(typeof(Attribute)));
		}

		public void Parse(IMpeg2VideoReader reader, IResultNodeState resultState)
		{
			resultState.Name = Name;

			// Note: Last header must be a sequence, group of pictures or picture header, or an extension.
			// Note: Sequence end code always ends parsing of a block, so it cannot occur at this point!
			string lastHeader = reader.State.LastHeaderName;
			if ((lastHeader == Slice.Name) || (lastHeader == UserData.Name))
			{
				// User data cannot occur immediately after a slice or (another) user data
				resultState.Invalidate();
				return;
			}

			reader.GetAttribute(_userDataAttribute);
		}
	}
}
