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
using System.Text;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// Data reader for parsing QuickTime atoms.
	/// </summary>
	public class QtParser : ByteStreamParser<QtAtom, AtomName, QtParser>
	{
        private static readonly DateTime BaseTimeUnix = new DateTime(1904, 1, 1, 0, 0, 0);
        private static readonly DateTime BaseTimeMac = new DateTime(1970, 1, 1, 0, 0, 0);

		#region Internal classes
		private class MatrixAttribute<TEnum> : CompositeAttribute<TEnum, string, QtParser>
		{
			public Matrix Matrix { get; private set; }

			public MatrixAttribute(TEnum attributeName)
				: base(attributeName, string.Empty, "{0}")
			{
				Matrix = new Matrix();
			}

			public override bool Parse(QtParser parser)
			{
				Matrix.Parse(parser);

				TypedValue = string.Format("({0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8})", Matrix.A, Matrix.B, Matrix.U, Matrix.C, Matrix.D, Matrix.V, Matrix.X, Matrix.Y, Matrix.W);

				return Valid;
			}
		}

		private class Opcolor<TEnum> : CompositeAttribute<TEnum, string, QtParser>
		{
			public enum LAttribute
			{
				Red,
				Green,
				Blue,
			}

			public Opcolor(TEnum attributeName)
				: base(attributeName, string.Empty, "{0}")
			{
			}

			public override bool Parse(QtParser parser)
			{
				short red = parser.GetShort(LAttribute.Red);
				short green = parser.GetShort(LAttribute.Green);
				short blue = parser.GetShort(LAttribute.Blue);

				TypedValue = string.Format("({0}, {1}, {2})", red, green, blue);

				return Valid;
			}
		}
		#endregion Internal classes

		#region Properties
		public ulong BytesRemaining
		{
			get
			{
				QtAtom atom = Result as QtAtom;
				Debug.Assert(atom != null);
				return Math.Min((ulong)Length, (ulong)atom.RelativeEndOffset) - (ulong)Position;
			}
		}
		#endregion Properties

		public QtParser(ByteStreamDataReader dataReader)
			: base(dataReader)
		{
		}

		public override QtAtom FindFirstHeader(QtAtom root, long offsetLimit)
		{
			long atomOffsetLimit = Math.Min(offsetLimit, (Length - 8));
			uint atomType;

			while ((atomType = NextAtomType(atomOffsetLimit, false)) != 0)
			{
				QtAtom header = CreateAtom(root, atomType.GetAtomName(), atomType);

				if (header.HeaderName != AtomName.Unknown)
				{
					return header;
				}
			}
			return null;
		}

		public override QtAtom GetNextHeader(QtAtom previousHeader, long offsetLimit)
		{
			// Some atoms may be closed by a four byte terminating zero (0).
			// On the other hand four zero bytes followed by a mdat atom
			// might indicate that while making the movie using a phone,
			// the battery ran out and the movie size was not written
			// to the mdat atom.
			if (Position > (Length - 4))
			{
				return null;
			}
			int zero = GetInt();
			if(Position <(Length-4))
			{
				uint type = GetUInt();
				Position -= 8;	// Also needed for Terminating Zero.
				// Content will be validated by parsed method.
				if (zero == 0 && type.GetAtomName() != AtomName.MediaData)
				{
					return CreateAtom(previousHeader, AtomName.TerminatingZero, 0U);
				}
			}else
			{
				Position -= 4;
			}
			// Try to read the directly succeeding atom
			uint atomType = NextAtomType(Position, true);
			if (atomType == 0)
			{
				return null;
			}
			return CreateAtom(previousHeader, atomType.GetAtomName(), atomType);
		}

		public uint GetThreeBytes<T>(T attributeName)
		{
			if (!CheckRead(3)) return 0;

			uint value = (uint)DataReader.GetThreeBytes();
			AddAttribute(new FormattedAttribute<T, uint>(attributeName, value, "{0:X6}"));
			return value;
		}

		public double GetFixed8_8<T>(T attributeName)
		{
			if (!CheckRead(2)) return 0.0;

			const double twoPow8 = 256.0;
			double value = ((ushort)DataReader.GetShort()) / twoPow8;
			AddAttribute(new FormattedAttribute<T, double>(attributeName, value));
			return value;
		}

		public double GetFixed16_16<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0.0;

			const double twoPow16 = 65536.0;
			double value = ((uint)DataReader.GetInt()) / twoPow16;
			AddAttribute(new FormattedAttribute<T, double>(attributeName, value));
			return value;
		}

		internal double GetFixed2_30<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0.0;

			const double twoPow30 = 1073741824.0;
			double value = ((uint)DataReader.GetInt()) / twoPow30;
			AddAttribute(new FormattedAttribute<T, double>(attributeName, value));
			return value;
		}

		// Get Date and Time (if stored as number of seconds since Jan. 1, 1904, in UTC Time)
		// TODO: (Rikkert) Make different GetDateTime methods, for different reference dates (or make it a variable)
		public void GetLongDateTime<T>(T attributeName, string format)
		{
			if (!CheckRead(8)) return;

            AddDateTimeAttribute(attributeName, format, (ulong)DataReader.GetLong());
		}

		// Get Date and Time (if stored as number of seconds since Jan. 1, 1904, in UTC Time)
		// TODO: (Rikkert) Make different GetDateTime methods, for different reference dates (or make it a variable)
		public void GetDateTime<T>(T attributeName, string format)
		{
			if (!CheckRead(4)) return;

            AddDateTimeAttribute(attributeName, format, (uint)DataReader.GetInt());
        }

        private void AddDateTimeAttribute<T>(T attributeName, string format, ulong seconds)
        {
            DateTime baseTime = DetermineBaseTime(seconds);
            try
            {
                AddAttribute(new FormattedAttribute<T, DateTime>(attributeName, baseTime.AddSeconds(seconds), format));
            }
            catch (ArgumentOutOfRangeException e)
            {
                IResultAttribute attribute = new FormattedAttribute<T, DateTime>(attributeName, baseTime, format);
                AddAttribute(attribute);
                attribute.Valid = false; // The date/time field is shown as invalid, but the result is not discarded!
            }
        }

        private static DateTime DetermineBaseTime(ulong seconds)
        {
            return (BaseTimeUnix.AddSeconds(seconds) < BaseTimeMac) ? BaseTimeMac : BaseTimeUnix;
        }

		public uint GetFourCC<T>(T attributeName)
		{
			if (!CheckRead(4)) return 0;

			uint value = (uint)DataReader.GetInt();
			AddAttribute(new FourCCAttribute<T>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Get a C-style (null-terminated) string
		/// </summary>
		/// <typeparam name="T">the type of attribute</typeparam>
		/// <param name="attributeName">the attribute name</param>
		/// <param name="maxLength">the maximum bytes to read</param>
		/// <returns>the string, null on error</returns>
		internal string GetCString<T>(T attributeName, uint maxLength)
		{
			StringBuilder stringBuilder = new StringBuilder();

			byte b;
			int count = 0;
			while ((b = GetByte()) != 0 && (count++) < maxLength)
			{
				stringBuilder.Append((char)b);
			}

			string value = stringBuilder.ToString();
			AddAttribute(new FormattedAttribute<T, string>(attributeName, value));
			return value;
		}

		/// <summary>
		/// Gets a Pascal (aka counted) string
		/// </summary>
		/// <remarks>
		/// The pascal string is assumed to contain no 0.
		/// When a 0 is encountered before the last character,
		/// a c-string is assumed.
		/// </remarks>
		/// <typeparam name="T">the type of attribute</typeparam>
		/// <param name="attributeName">the attribute name</param>
		/// <param name="byteNumRead">the number of bytes read</param>
		/// <returns>the string, null on error</returns>
		private string GetPascalString<T>(T attributeName, out int byteNumRead)
		{
			StringBuilder sb = new StringBuilder();

			byte countedStringLength = GetByte();
			byteNumRead = 1;

			for (byte charIndex = 0; charIndex < countedStringLength; charIndex++)
			{
				byte b = GetByte();
				byteNumRead++;

				if (b == 0)
				{
					if (((char)countedStringLength) >= ' ')
					{
						sb.Insert(0, (char)countedStringLength);
					}
					break;
				}
				sb.Append((char)b);
			}
			string s = sb.ToString();
			AddAttribute(new FormattedAttribute<T, string>(attributeName, s));
			return s;
		}

		/// <summary>
		/// Gets a Pascal (aka counted) string
		/// </summary>
		/// <typeparam name="T">the type of attribute</typeparam>
		/// <param name="attributeName">the attribute name</param>
		/// <param name="byteNumMustRead">the number of bytes that must be read</param>
		/// <returns>the string, null on error</returns>
		public string GetPascalString<T>(T attributeName, int byteNumMustRead)
		{
			int byteNumRead;
			string text = GetPascalString(attributeName, out byteNumRead);

			// Read away the bytes beyond the pascal string
			if (byteNumMustRead > byteNumRead)
			{
				if (CheckRead(byteNumMustRead - byteNumRead))
				{
					Position += (byteNumMustRead - byteNumRead);
				}
			}
			return text;
		}

		public string GetFixedLengthString<T>(T attributeName, int length)
		{
			StringBuilder sb = new StringBuilder();
			for (int charIndex = 0; charIndex < length; charIndex++)
			{
				sb.Append((char)GetByte());
			}
			string s = sb.ToString();
			AddAttribute(new FormattedAttribute<T, string>(attributeName, s));
			return s;
		}

		public Matrix GetMatrix<T>(T attributeName)
		{
			if (!CheckRead(36)) return null;

			MatrixAttribute<T> matrixAttribute = new MatrixAttribute<T>(attributeName);
			Parse(matrixAttribute);

			return matrixAttribute.Matrix;
		}

		public void GetOpcolor<T>(T attributeName)
		{
			if (!CheckRead(6)) return;

			Parse(new Opcolor<T>(attributeName));
		}

		public void GetFlags<T, TFlag>(T attributeName, int size)
		{
			if (!CheckRead(size)) return;

			Parse(new FlagsAttribute<T, TFlag>(attributeName, size));
		}

		/// <summary>
		/// Finds and returns the (known) type of the next atom.
		/// The next atom type is the (known) type of the next atom.
		/// </summary>
		/// <param name="atomOffsetLimit">the maximum offset for the next atom</param>
		/// <param name="allowUnknownsAtoms">if <code>true</code>, allow unknown atoms</param>
		/// <returns>the type (4CC) of the next atom, <code>0</code> for none</returns>
		private uint NextAtomType(long atomOffsetLimit, bool allowUnknownsAtoms)
		{
			if ((State != DataReaderState.Ready) || (Position > Math.Min(atomOffsetLimit, (Length - 8))))
			{
				return 0;
			}

			ulong code = (ulong)DataReader.GetLong();

			// Try to read the next atom header (8 or 16 bytes)
			while (State != DataReaderState.Cancelled)
			{
				long offset = Position - 8;
				long size = (long)(code >> 32);
				uint type = (uint)code;

				// Validate the atom type
				if (type.IsKnownAtomType() || (allowUnknownsAtoms && type.IsValid4CC()))
				{
					long minSize = 8;

					if ((size == 1) && (Position <= (Length - 8)))
					{
						// Atom with 64-bit size
						size = DataReader.GetLong();
						Position -= 8;
						minSize = 16;
					}
					// mdat (MediaData) atoms of size 0 are possible
					// when during the recording the phone battery
					// runs out.
					if (size >= minSize || type.GetAtomName() == AtomName.MediaData)
					{
						Position = offset;
						return type;
					}
				}
				if (offset >= atomOffsetLimit)
				{
					break;
				}

				// Shift buffer, read next byte
				code = (code << 8) | DataReader.GetByte();
			}
			return 0;
		}

		/// <summary>
		/// Creates an atom of the given <param name="atomName"/>.
		/// </summary>
		/// <param name="previousHeader">the previous header (atom)</param>
		/// <param name="atomName">the atom to create</param>
		/// <param name="atomType">the type of atom to create</param>
		/// <returns>the atom</returns>
		internal static QtAtom CreateAtom(QtAtom previousHeader, AtomName atomName, uint atomType)
		{
			switch (atomName)
			{
				case AtomName.Root:
					return new QtAtom(previousHeader, atomName);
				// Top-level atoms
				case AtomName.MediaData:
					return new MediaData(previousHeader);
				case AtomName.FileType:
					return new FileType(previousHeader);
				// Regular atoms
				case AtomName.MovieHeader:
					return new MovieHeader(previousHeader);
				case AtomName.TrackHeader:
					return new TrackHeader(previousHeader);
				case AtomName.EditList:
					return new EditList(previousHeader);
				case AtomName.MediaHeader:
					return new MediaHeader(previousHeader);
				case AtomName.VideoMediaInformationHeader:
					return new VideoMediaInformationHeader(previousHeader);
				case AtomName.SoundMediaInformationHeader:
					return new SoundMediaInformationHeader(previousHeader);
				case AtomName.HandlerReference:
					return new HandlerReference(previousHeader);
				case AtomName.SampleSize:
					return new SampleSize(previousHeader);
				case AtomName.ChunkOffset:
					return new ChunkOffset(previousHeader, (ChunkOffset.AtomType)atomType);
				case AtomName.SampleDescription:
					return new SampleDescription(previousHeader);
				case AtomName.SampleToChunk:
					return new SampleToChunk(previousHeader);
				case AtomName.ElementaryStreamDescriptor:
					return new ElementaryStreamDescriptor(previousHeader);
				case AtomName.DataReference:
					return new DataReference(previousHeader);
				case AtomName.ReferenceMovieDataReference:
					return new ReferenceMovieDataReference(previousHeader);
				case AtomName.TimeToSample:
					return new TimeToSample(previousHeader);
				case AtomName.SyncSample:
					return new SyncSample(previousHeader);
				case AtomName.DataCompression:
					return new DataCompression(previousHeader);
				case AtomName.CompressedMovieData:
					return new CompressedMovieData(previousHeader);
				case AtomName.Profile:
					return new Profile(previousHeader);
				case AtomName.ColorTable:
					return new ColorTable(previousHeader);
				case AtomName.Quality:
					return new Quality(previousHeader);
				case AtomName.ComponentDetect:
					return new ComponentDetect(previousHeader);
				case AtomName.VersionCheck:
					return new VersionCheck(previousHeader);
				case AtomName.CpuSpeed:
					return new CpuSpeed(previousHeader);
				case AtomName.DataRate:
					return new DataRate(previousHeader);
				case AtomName.ClippingRegion:
					return new ClippingRegion(previousHeader);
				case AtomName.Unknown:
					return new Unknown(previousHeader);
				case AtomName.TerminatingZero:
					return new TerminatingZero(previousHeader);
				case AtomName.TrackLoadSettings:
					return new TrackLoadSettings(previousHeader);
				case AtomName.CompositionTimeToSample:
					return new CompositionTimeToSample(previousHeader);
				case AtomName.BaseMediaInfo:
					return new BaseMediaInfo(previousHeader);
				case AtomName.InitialObjectDescriptor:
					return new InitialObjectDescriptor(previousHeader);
				case AtomName.VideoSampleDescription:
					return new VideoSampleDescription(previousHeader);
				case AtomName.SoundSampleDescription:
					return new SoundSampleDescription(previousHeader);
				case AtomName.MpegSampleDescription:
					return new QtSampleDescriptionAtom(previousHeader, AtomName.MpegSampleDescription);
				case AtomName.H263Specific:
					return new H263Specific(previousHeader);
				case AtomName.Bitrate:
					return new Bitrate(previousHeader);
				case AtomName.AmrSpecific:
					return new AmrSpecific(previousHeader);
				case AtomName.AmrwpSpecific:
					return new AmrwpSpecific(previousHeader);
				case AtomName.TimeCodeSampleDescription:
					return new TimeCodeSampleDescription(previousHeader);
				case AtomName.TrackReferenceType:
					return new TrackReferenceType(previousHeader);
				case AtomName.DataReferenceEntry:
					return new DataReferenceEntry(previousHeader);
				case AtomName.MetaData:
					return new MetaData(previousHeader);
				case AtomName.PixelAspectRatio:
					return new PixelAspectRatio(previousHeader);
				// H.264 boxes
				case AtomName.AvcConfigurationBox:
					return new AvcConfigurationBox(previousHeader);
				case AtomName.AvcLayerEntry:
					return new AvcLayerEntry(previousHeader);
				case AtomName.AvcParameterSampleEntry:
					return new AvcParameterSampleEntry(previousHeader);
				case AtomName.AvcSubSequenceEntry:
					return new AvcSubSequenceEntry(previousHeader);
				case AtomName.Mpeg4BitRateBox:
					return new Mpeg4BitRateBox(previousHeader);
				case AtomName.Mpeg4ExtensionDescriptorsBox:
					return new Mpeg4ExtensionDescriptorsBox(previousHeader);
				case AtomName.SampleDependencyBox:
					return new SampleDependencyBox(previousHeader);
				case AtomName.VisualRollRecoveryEntry:
					return new VisualRollRecoveryEntry(previousHeader);
				case AtomName.ColorParameter:
					return new ColorParameter(previousHeader);
				case AtomName.Wide:
					return new Wide(previousHeader);
				case AtomName.CompressedMatte:
					return new CompressedMatte(previousHeader);
				case AtomName.TrackInput:
					return new TrackInput(previousHeader);
				case AtomName.SampleToGroupBox:
					return new SampleToGroupBox(previousHeader);
				case AtomName.SampleGroupDescriptionBox:
					return new SampleGroupDescriptionBox(previousHeader);
			}

			if (atomName.IsFlagSet(AtomFlags.UserDataAtom))
			{
				return new QtUserDataAtom(previousHeader, atomName);
			}

			return new QtAtom(previousHeader, atomName);
		}
	}
}
