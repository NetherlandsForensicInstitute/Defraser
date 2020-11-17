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
using Defraser.Interface;

namespace Defraser.Detector.Common
{
	/// <summary>
	/// Provides header and attribute parsing.
	/// Detector implementations are advised to extend this class with format
	/// specific parsing capabilities.
	/// </summary>
	/// <typeparam name="THeader">the type header</typeparam>
	/// <typeparam name="THeaderName">the type of header name</typeparam>
	/// <typeparam name="TParser">the type of parser</typeparam>
	public abstract class Parser<THeader, THeaderName, TParser> : IDataReader
		where THeader : Header<THeader, THeaderName, TParser>
		where TParser : Parser<THeader, THeaderName, TParser>
	{
		private IDataReader _dataReader;

		#region Properties
		public long Position { get { return _dataReader.Position; } set { _dataReader.Position = value; } }
		public long Length { get { return _dataReader.Length; } }
		public DataReaderState State { get { return _dataReader.State; } }
		/// <summary>The attribute or result node currently being parsed.</summary>
		protected IResult Result { get; set; }
		#endregion

		/// <summary>
		/// Creates a new parser for the given <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data reader</param>
		public Parser(IDataReader dataReader)
		{
			if (dataReader == null)
			{
				throw new ArgumentNullException("dataReader");
			}

			_dataReader = dataReader;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			return _dataReader.Read(array, arrayOffset, count);
		}

		/// <summary>
		/// Finds the first header.
		/// </summary>
		/// <param name="root">the root header</param>
		/// <param name="offsetLimit">the maximum offset for the first header</param>
		/// <returns>the header or <code>null</code> if none found</returns>
		public abstract THeader FindFirstHeader(THeader root, long offsetLimit);

		/// <summary>
		/// Gets the next directly succeeding header.
		/// </summary>
		/// <returns>the next header, <code>null</code> if invalid or none</returns>
		public abstract THeader GetNextHeader(THeader previousHeader, long offsetLimit);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="firstHeader"></param>
		/// <returns></returns>
		public virtual void AppendHeaders(THeader firstHeader)
		{
			THeader previousHeader = firstHeader;

			// Append headers
			while (State == DataReaderState.Ready)
			{
				THeader header;

				if ((header = GetNextHeader(previousHeader, Length)) == null || !ParseAndAppend(header))
				{
					break;
				}

				previousHeader = header;
			}
		}

		/// <summary>
		/// Parses the given <paramref name="root"/> header.
		/// This locates the first continguous block of valid headers.
		/// </summary>
		/// <remarks>
		/// If no valid headers could be found, this method returns
		/// <code>false</code> and <code>DataReader</code> is positioned
		/// at <paramref name="offsetLimit"/> to indicate there are no
		/// more headers in the input.
		/// </remarks>
		/// <param name="root">the root header to parse</param>
		/// <param name="offsetLimit">the maximum offset for the first header</param>
		/// <returns>true on success, false otherwise</returns>
		public bool ParseRoot(THeader root, long offsetLimit)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root");
			}
			if (!root.IsRoot)
			{
				throw new ArgumentException("Not a root header.", "root");
			}
			if ((offsetLimit < 0) || (offsetLimit > Length))
			{
				throw new ArgumentOutOfRangeException("offsetLimit");
			}

			// Find valid header sequence
			while ((State != DataReaderState.Cancelled) && (Position < offsetLimit))
			{
				THeader firstHeader = null;
				try
				{
					firstHeader = FindFirstHeader(root, offsetLimit);
				}
				catch (ReadOverflowException e)
				{
					// TODO: log error?
				}
				if (firstHeader == null)
				{
					break;
				}

				// Try to construct a valid header sequence
				if (ParseAndAppend(firstHeader))
				{
					AppendHeaders(firstHeader);
					return (State != DataReaderState.Cancelled);
				}

				// Not a valid header sequence: Rewind and skip 1 byte
				Position = Math.Min((firstHeader.Offset + 1), Length);
			}

			// No valid headers found or cancelled
			Position = Math.Max(Position, offsetLimit);
			return false;
		}

		/// <summary>
		/// Parses the given <paramref name="header"/> and adds it to
		/// a <em>suitable</em> parent if the header is valid.
		/// </summary>
		/// <param name="header">the header to parse</param>
		/// <returns>true on success, false otherwise</returns>
		public bool ParseAndAppend(THeader header)
		{
			if (_dataReader == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			THeader previousHeader = header.Parent as THeader;
			if (previousHeader == null)
			{
				return false;
			}

			// Change Result to header
			IResult saveResult = Result;
			Result = header;

			// Parse the header
			TParser parser = this as TParser;
			try
			{
				if (!header.Parse(parser))
				{
					Result = saveResult;
					return false;
				}
			}
			catch (ReadOverflowException)
			{
				Result = saveResult;
				return false;
			}
			if (!header.ParseEnd(parser))
			{
				Result = saveResult;
				return false;
			}

			// Restore previous result
			Result = saveResult;

			// Check back-to-back
			if (!previousHeader.IsRoot && !previousHeader.IsBackToBack(header))
			{
				return false;
			}

			// Find suitable parent
			THeader parent = header.FindSuitableParent();
			if (parent == null)
			{
				return false;
			}

			// Add this header as a child to parent
			parent.AddChild(header);

			return true;
		}

		/// <summary>
		/// Parses the given <param name="attribute"/> and adds it to the
		/// <c>Result</c> currently being parsed if the attribute is valid.
		/// </summary>
		/// <typeparam name="TEnum">the attribute enumeration type or string</typeparam>
		/// <typeparam name="TValue">the scalar value type for the attribute</typeparam>
		/// <param name="attribute">the attribute to parse</param>
		/// <returns>true on success, false otherwise</returns>
		public bool Parse<TEnum, TValue>(CompositeAttribute<TEnum, TValue, TParser> attribute)
		{
			if (_dataReader == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			// Change Result to composite attribute
			IResult saveResult = Result;
			Result = attribute;

			// Parse composite attribute
			bool valid = attribute.Parse(this as TParser);

			// Restore previous result
			Result = saveResult;

			// Add attribute (if valid) and return its value
			if (valid)
			{
				AddAttribute(attribute);
			}
			return valid;
		}

		/// <summary>
		/// Adds the given <paramref name="attribute"/> to the <c>Result</c>.
		/// </summary>
		/// <param name="attribute">the attribute to add</param>
		public void AddAttribute(IResultAttribute attribute)
		{
			// TODO: if past end-of-input, do not add attribute, invalidate result
			Result.Attributes.Add(attribute);

			if (!attribute.Valid)
			{
				Result.Valid = false;
			}
		}

		/// <summary>
		/// Checks the attribute specified by <paramref name="attributeName"/>
		/// and invalidates the attribute if the specified <param name="condition"/>
		/// is <code>false</code>.
		/// </summary>
		/// <remarks>
		/// The type of <typeref name="T"/> should be an enum.
		/// </remarks>
		/// <typeparam name="T">the attribute enumeration type</typeparam>
		/// <param name="attributeName">the name of the attribute to check</param>
		/// <param name="condition">the condition</param>
		/// <returns>condition</returns>
		public bool CheckAttribute<T>(T attributeName, bool condition)
		{
			return CheckAttribute(attributeName, condition, true);
		}

		/// <summary>
		/// Checks the attribute specified by <paramref name="attributeName"/>
		/// and invalidates the attribute if the specified <param name="condition"/>
		/// is <code>false</code>.
		/// Also invalidates the <c>Result</c> currently being parsed and containing
		/// the attribute if <param name="invalidateResult"/> is <code>true</code>.
		/// </summary>
		/// <remarks>
		/// The type of <typeref name="T"/> should be an enum.
		/// </remarks>
		/// <typeparam name="T">the attribute enumeration type</typeparam>
		/// <param name="attributeName">the name of the attribute to check</param>
		/// <param name="condition">the condition</param>
		/// <param name="invalidateResult">invalidates the <c>Result</c> if true (default)</param>
		/// <returns>condition</returns>
		public bool CheckAttribute<T>(T attributeName, bool condition, bool invalidateResult)
		{
            if (!condition)
			{
				string name = Enum.GetName(typeof(T), attributeName);

				IResultAttribute attribute = Result.FindAttributeByName(name);
				if (attribute == null)
				{
					throw new ArgumentException("No such attribute.", "attributeName");
				}

				attribute.Valid = false;

				if (invalidateResult)
				{
					Result.Valid = false;
				}
			}
			return condition;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_dataReader = null;
			}
		}
	}
}
