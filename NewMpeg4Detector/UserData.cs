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

using System.Text;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg4
{
	/// <summary>
	/// This is an 8 bit integer, an arbitrary number of which may follow one
	/// another. User data is defined by users for their specific applications.
	/// In the series of consecutive user_data bytes there shall not be a
	/// string of 23 or more consecutive zero bits.
	/// The user data continues until receipt of another start code.
	/// </summary>
	/// <remarks>
	/// An User Data header followed by an invalid header is also considered
	/// invalid itself. This UserData header will be removed (trimmed) from
	/// the results. Removing this header is done afterwards in
	/// Mpeg4Detector.DetectData().
	/// </remarks>
	/// <remarks>
	/// The same UserData class is used for the MPEG-2 detector.
	/// If you change something here, you might also want to change the
	/// MPEG-2 class.
	/// </remarks>

	internal class UserData : Mpeg4Header
	{
		#region Inner classes
		/// <summary>
		/// Describes the user data bytes.
		/// </summary>
		/// <remarks>The result may be truncated for performance reasons.</remarks>
		private class UserDataAttribute : CompositeAttribute<Attribute, string, Mpeg4Parser>
		{
			public UserDataAttribute()
				: base(Attribute.UserDataBytes, string.Empty)
			{
			}

			public override bool Parse(Mpeg4Parser parser)
			{
				// User data size limit
				uint maxUserDataLength = (uint)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.MaxUserDataLength];

				// Verify and copy user data bytes
				StringBuilder sb = new StringBuilder();
				while (parser.State == DataReaderState.Ready &&
					parser.ShowBits(24) != 1 &&	// normal header
					parser.ShowBits(22) != 0x20)	// short header
				{
					//// Verify that there are no 23 or more consecutive zero bits
					//if (parser.ShowBits(23) == 0 && parser.DataReader.Position < (parser.DataReader.Length - 2))
					//{
					//    if (!IsValidUserDataThatEndsWithOneOrTwoZeroBytes(parser))
					//    {
					//        this.Valid = false;
					//    }
					//    break;
					//}

					// Read next byte
					sb.Append((char)parser.GetBits(8));

					if (sb.Length > maxUserDataLength)
					{
						this.Valid = false;
						break;
					}
				}

				this.TypedValue = sb.ToString();

				return this.Valid;
			}

			//private static bool IsValidUserDataThatEndsWithOneOrTwoZeroBytes(Mpeg4Parser parser)
			//{
			//    parser.DataReader.GetByte();
			//    if (parser.ShowBits(24) == 1 ||		// Normal header
			//        parser.ShowBits(22) == 0x20)	// Short header
			//    {
			//        // 1 byte '00', followed by next start code
			//        return true;
			//    }

			//    parser.DataReader.GetByte();
			//    if (parser.ShowBits(24) == 1 ||		// Normal header
			//        parser.ShowBits(22) == 0x20)	// Short header
			//    {
			//        // 2 bytes '00 00', followed by next start code
			//        return true;
			//    }
			//    return false;
			//}
		}
		#endregion Inner classes

		public new enum Attribute
		{
			UserDataBytes
		}

		public UserData(Mpeg4Header previousHeader)
			: base(previousHeader, Mpeg4HeaderName.UserData)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			if (!parser.Parse(new UserDataAttribute()))
			{
				this.Valid = false;
			}
			return this.Valid;
		}
	}
}
