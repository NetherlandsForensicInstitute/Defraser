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
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Example
{
	internal sealed class ExampleReader : IExampleReader
	{
		private readonly ByteStreamDataReader _dataReader;

		#region Properties
		public long Position { get { return _dataReader.Position; } }
		private IResultState Result { get; set; }
		#endregion Properties

		public ExampleReader(ByteStreamDataReader dataReader)
		{
			_dataReader = dataReader;
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		/// <summary>Searches for the exampleStartCode.</summary>
		public byte NextExampleHeaderType()
		{
			// Traverse the dataReader and look for the start of a header.
			while (_dataReader. State == DataReaderState.Ready)
			{
				if (_dataReader.GetByte() == ExampleHeader.StartCode)
				{
					_dataReader.Position--; // set the read pointer at the start of the header.
					return ExampleHeader.StartCode;
				}
			}
			return 0;
		}

		/// <summary>Count the sequence of consecutive bytes with the value ExampleStartCode.</summary>
		public void GetEqualByteSequence<T>(T attributeName)
		{
			int exampleCount = 0;
			while (_dataReader.State == DataReaderState.Ready)
			{
				if (_dataReader.GetByte() == (byte)ExampleHeader.StartCode)
				{
					exampleCount++;
				}
				else
				{
					_dataReader.Position--;
					break;
				}
			}
			AddAttribute(attributeName, exampleCount);
		}

		/// <summary>Fetch the last byte of the ExampleHeader and put it in an attribute.</summary>
		public bool GetFinalExampleHeaderByte<T>(T attributeName)
		{
			if (_dataReader.Position < _dataReader.Length)
			{
				AddAttribute(attributeName, _dataReader.GetByte());
				return true;
			}
			return false;
		}

		private void AddAttribute<T>(T name, object value)
		{
			Result.AddAttribute(name, value);
		}

		//public override ExampleHeader GetNextHeader(ExampleHeader previousHeader, long offsetLimit)
		//{
		//    const int nextHeaderLength = 1;

		//    if (Position > (Length - nextHeaderLength))
		//    {
		//        return null;
		//    }

		//    // Try to read the directly succeeding chunk
		//    byte nextExampleHeaderType = NextExampleHeaderType();

		//    return CreateExampleHeader(previousHeader, nextExampleHeaderType);
		//}

		///// <summary>
		///// Creates an example header of the given <param name="exampleHeaderType"/>.
		///// </summary>
		///// <param name="previousHeader">the directly preceeding header</param>
		///// <param name="exampleHeaderType">the type of example header to create</param>
		///// <returns>the example header</returns>
		//private static ExampleHeader CreateExampleHeader(ExampleHeader previousHeader, byte exampleHeaderType)
		//{
		//    switch (exampleHeaderType)
		//    {
		//        case (byte)ExampleHeaderName.ExampleHeader: return new ExampleHeader(previousHeader);
		//    }
		//    return null;
		//}

		public void StartResult(IResultState resultState)
		{
			Result = resultState;
		}

		public void EndResult()
		{
			Result = null;
		}
	}
}
