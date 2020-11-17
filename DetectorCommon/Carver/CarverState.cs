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
using Defraser.Interface;

namespace Defraser.Detector.Common.Carver
{
	internal sealed class CarverState : ICarverState
	{
		private readonly IReaderState _readerState;
		private readonly IDataBlockCarver _dataBlockCarver;
		private readonly IResultMetadata _resultMetadata;
		private readonly IDataReader _dataReader;
		private readonly IDataBlockBuilder _dataBlockBuilder;

		#region Properties
		private long Position
		{
			get { return _dataReader.Position; }
			set { _dataReader.Position = value; }
		}
		private bool IsCancelled
		{
			get { return _dataReader.State == DataReaderState.Cancelled; }
		}
		#endregion Properties

		public CarverState(IReaderState readerState, IDataBlockCarver dataBlockCarver, IResultMetadata resultMetadata, IDataReader dataReader, IDataBlockBuilder dataBlockBuilder)
		{
			_readerState = readerState;
			_dataBlockCarver = dataBlockCarver;
			_resultMetadata = resultMetadata;
			_dataReader = dataReader;
			_dataBlockBuilder = dataBlockBuilder;
		}

		public IDataBlock Carve(long offsetLimit)
		{
			if ((offsetLimit < 0) || (offsetLimit > _dataReader.Length))
			{
				throw new ArgumentOutOfRangeException("offsetLimit");
			}

			try
			{
				_readerState.Reset();

				// Find possible start of header sequence
				while (!IsCancelled && (Position < offsetLimit) && _dataBlockCarver.Carve(offsetLimit))
				{
					// Try to construct a valid header sequence
					long dataBlockStartOffset = Position;
					long dataBlockEndOffset = Position;

					while (_readerState.Valid)
					{
						dataBlockEndOffset = Position;

						_dataBlockCarver.ParseHeader(_readerState);
					}

					// Try to create and return valid result!
					if (!IsCancelled && ValidateDataBlock(dataBlockStartOffset, dataBlockEndOffset))
					{
						var dataBlock = _dataBlockBuilder.Build();
						_resultMetadata.Init(dataBlock);

						Position = dataBlock.EndOffset;
						return dataBlock;
					}

					// TODO: The data block carver should be able to skip any number of bytes ... (to avoid O(N^2) performance in some cases)

					// Not a valid header sequence: Rewind and skip 1 byte
					// Rewind data reader for incorrect header to avoid skipping correct headers after a dummy start code
					Position = dataBlockStartOffset + 1;// Note: This is slightly inefficient for MPEG-2 (we could skip upto 4 bytes)

					// Start new block
					_readerState.Reset();
				}

				// No valid headers found or cancelled
				//Position = Math.Max(Position, offsetLimit);
				return null;
			}
			finally
			{
				FlushStream();
			}
		}

		private bool ValidateDataBlock(long startOffset, long endOffset)
		{
			_dataBlockBuilder.StartOffset = startOffset;
			_dataBlockBuilder.EndOffset = endOffset;
			return _dataBlockCarver.ValidateDataBlock(_dataBlockBuilder, startOffset, endOffset);
		}

		private void FlushStream()
		{
			BufferedDataReader bufferedDataReader = _dataReader as BufferedDataReader;
			if (bufferedDataReader != null)
			{
				bufferedDataReader.Flush();
			}
		}
	}
}
