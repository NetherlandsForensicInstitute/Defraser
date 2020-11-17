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

namespace Defraser.Framework
{
	/// <summary>
	/// Buffers data reads for the underlying data reader. Direct access to
	/// the internal buffer and other attributes can be used to implement
	/// streaming data readers.
	///
	/// The caching strategy will automatically detect and optimize forward or
	/// backward sequential reading by using data read-ahead or read-back.
	/// Random access will effectively disable read-ahead, caching only the
	/// last block of data read.
	/// </summary>
	/// <remarks>
	/// This implementation also caches <c>Length</c> and <c>Position</c> properties.
	/// Changes in <c>Position</c> or <c>Length</c> of the underlying data reader
	/// are therefore not reflected in the buffered data reader.
	/// </remarks>
	public class BufferedDataReader : IDataReader
	{
		/// <summary>The fraction of the buffer to use for read-back.</summary>
		public const int BytesBeforeFactor = 32;
		/// <summary>The default buffer size in bytes.</summary>
		public const int DefaultBufferSize = 256 * 1024;
		/// <summary>The minimum buffer size in bytes.</summary>
		public const int MinimumBufferSize = 16;

		/// <summary>The underlying data reader</summary>
		protected IDataReader _dataReader;
		/// <summary>Buffer cache.</summary>
		protected readonly byte[] _buffer;
		/// <summary>Position corresponding to start of buffer.</summary>
		protected long _bufferPosition;
		/// <summary>Current position in the buffer.</summary>
		protected int _bufferOffset;
		/// <summary>Number of bytes currently in the buffer.</summary>
		protected int _bytesInBuffer;
		/// <summary>Number of padding bytes.</summary>
		protected int _zeroPadding;
		/// <summary>Length of the data.</summary>
		protected readonly long _length;

		private long _discardedBufferPosition;
		private int _discardedBufferSize;

		#region Properties
		public virtual long Position
		{
			get
			{
				long position = _bufferPosition + _bufferOffset;
				return Math.Min(position, _length);
			}
			set
			{
				if ((value < 0) || (value > _length))
				{
					throw new ArgumentOutOfRangeException("value");
				}

				if ((value >= _bufferPosition) && (value <= (_bufferPosition + _bytesInBuffer)))
				{
					_bufferOffset = (int)(value - _bufferPosition);
				}
				else
				{
					DiscardBuffer();

					_bufferPosition = value;
				}
			}
		}

		public long Length { get { return _length; } }

		public DataReaderState State
		{
			get
			{
				if ((_dataReader == null) || (_dataReader.State == DataReaderState.Cancelled))
				{
					return DataReaderState.Cancelled;
				}
				else if (Position >= _length)
				{
					return DataReaderState.EndOfInput;
				}
				else
				{
					return DataReaderState.Ready;
				}
			}
		}
		#endregion Properties


		/// <summary>
		/// Creates a new data reader for buffering data for the
		/// given <paramref name="dataReader"/> with default buffer size.
		/// <see cref="DefaultBufferSize"/>
		/// </summary>
		/// <param name="dataReader">the underlying data reader</param>
		public BufferedDataReader(IDataReader dataReader)
			: this(dataReader, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Creates a new data reader for buffering data for the given
		/// <paramref name="dataReader"/> using the given <param name="bufferSize"/>.
		/// </summary>
		/// <param name="dataReader">the underlying data reader</param>
		/// <param name="bufferSize">the size for the internal buffer</param>
		public BufferedDataReader(IDataReader dataReader, int bufferSize)
		{
			if (dataReader == null)
			{
				throw new ArgumentNullException("dataReader");
			}
			if (bufferSize < MinimumBufferSize)
			{
				throw new ArgumentOutOfRangeException("bufferSize", "Buffer size too small");
			}

			_dataReader = dataReader;
			_length = dataReader.Length;
			_buffer = new byte[bufferSize];
			_bufferPosition = dataReader.Position;
			_bufferOffset = 0;
			_bytesInBuffer = 0;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Synchronizes the position of the underlying data reader.
		/// </summary>
		public void Flush()
		{
			_dataReader.Position = Position;
		}

		public IDataPacket GetDataPacket(long offset, long length)
		{
			return _dataReader.GetDataPacket(offset, length);
		}

		public int Read(byte[] array, int arrayOffset, int count)
		{
			if (_dataReader == null)
			{
				throw new ObjectDisposedException("BufferedDataReader has been disposed");
			}
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if ((arrayOffset < 0) || (arrayOffset > array.Length))
			{
				throw new ArgumentOutOfRangeException("arrayOffset");
			}
			if ((count < 0) || (count > (array.Length - arrayOffset)))
			{
				throw new ArgumentOutOfRangeException("count");
			}

			if (count > _buffer.Length)
			{
				// Block does not fit in buffer, use uncached read
				_dataReader.Position = Position;
				count = _dataReader.Read(array, arrayOffset, count);
			}
			else
			{
				long currentPosition = Position;
				if ((_bytesInBuffer == 0) || (currentPosition + count) > (_bufferPosition + _bytesInBuffer))
				{
					FillBuffer(count);
				}

				// Compute actual buffer offset from current Position
				int bufferOffset = (int)(currentPosition - _bufferPosition);

				// Cached result, copy part of the buffer
				count = Math.Min(count, _bytesInBuffer - _zeroPadding - bufferOffset);
				if (count > 0)
				{
					Array.Copy(_buffer, bufferOffset, array, arrayOffset, count);
				}
			}

			return count;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_dataReader != null)
				{
					_dataReader.Dispose();
					_dataReader = null;
				}
			}
		}

		/// <summary>
		/// Fills the buffer with bytes read from the current <c>Position</c>.
		/// </summary>
		/// <param name="count">the number of bytes to buffer (read)</param>
		protected void FillBuffer(int count)
		{
			long position = Position;

			RestoreBuffer();
			ReadBuffer(position, count);
			ZeroPadBuffer(count);

			// Adjust buffer offset for desired position
			// This is needed for subclasses that override Position, e.g. BitStreamDataReader
			long desiredPosition = (_bufferPosition + _bufferOffset);
			_bufferOffset += (int)(desiredPosition - Position);
		}

		/// <summary>
		/// Discards the buffer.
		/// </summary>
		private void DiscardBuffer()
		{
			int nonPaddingBytes = (_bytesInBuffer - _zeroPadding);
			if (nonPaddingBytes == 0) return;

			_discardedBufferPosition = _bufferPosition;
			_discardedBufferSize = nonPaddingBytes;
			_bufferOffset = 0;
			_bytesInBuffer = 0;
			_zeroPadding = 0;
		}

		private void RestoreBuffer()
		{
			_bytesInBuffer -= _zeroPadding;
			_zeroPadding = 0;

			// Restore discarded buffer
			if ((_bytesInBuffer == 0) && (_discardedBufferSize > 0))
			{
				_bufferPosition = _discardedBufferPosition;
				_bytesInBuffer = _discardedBufferSize;
			}

			_discardedBufferPosition = 0L;
			_discardedBufferSize = 0;
		}

		private int GetExtraBytesToBuffer(long position, int count)
		{
			// Buffer fullness increases exponentially to its maximum
			long offset = (position - _bufferPosition);
			return ((offset < 0) || (offset > (_bytesInBuffer + count))) ? 0 : (int)Math.Min(offset, _bytesInBuffer);
		}

		private void ReadBuffer(long position, int count)
		{
			int bytesToRead = count;
			int extraBytesToBuffer = GetExtraBytesToBuffer(position, count);

			if (State != DataReaderState.Ready)
			{
				// End-of-input or cancelled, no bytes to read (fill buffer with padding bytes)
				bytesToRead = 0;
				extraBytesToBuffer = 0;
			}
			else if (ReadOverlapsWithBuffer(position, bytesToRead))
			{
				if (position >= _bufferPosition)
				{
					// Optimization for forward overlapped reading only
					ReadForwardOverlappedData(position, count, extraBytesToBuffer);
					return;
				}

				if ((position + bytesToRead) <= (_bufferPosition + _bytesInBuffer))
				{
					// Optimization for backward overlapped reading only
					ReadBackwardOverlappedData(position, extraBytesToBuffer);
					return;
				}

				// Both forward and backward overlapped reading (not optimized)
			}

			// Can not reuse buffered data
			_bufferPosition = position;
			_bufferOffset = 0;
			_bytesInBuffer = 0;

			ReadDataIntoBuffer(position, 0, bytesToRead, extraBytesToBuffer);
		}

		private bool ReadOverlapsWithBuffer(long position, int count)
		{
			long bufferEndPosition = (_bufferPosition + _bytesInBuffer);
			return ((position + count) >= _bufferPosition) && (position <= bufferEndPosition);
		}

		private void ReadForwardOverlappedData(long position, int count, int extraBytesToBuffer)
		{
			int maxPreviousBytes = Math.Min((_buffer.Length / BytesBeforeFactor), (_buffer.Length - count));
			_bufferOffset = (int)Math.Min((position - _bufferPosition), maxPreviousBytes);

			int remainingBytes = (int)((_bufferPosition + _bytesInBuffer) - position);
			position += remainingBytes;
			int bufferReadOffset = (_bufferOffset + remainingBytes);
			int bytesToRead = Math.Max(0, (count - remainingBytes));

			// Shift buffer left, keep part as read-back
			Array.Copy(_buffer, (_bytesInBuffer - bufferReadOffset), _buffer, 0, bufferReadOffset);

			_bufferPosition = (position - remainingBytes - _bufferOffset);
			_bytesInBuffer = bufferReadOffset;

			ReadDataIntoBuffer(position, bufferReadOffset, bytesToRead, extraBytesToBuffer);
		}

		private void ReadBackwardOverlappedData(long position, int extraBytesToBuffer)
		{
			int bytesToRead = (int)(_bufferPosition - position);

			_bufferPosition = position;
			_bufferOffset = 0;
			_bytesInBuffer = Math.Min(_bytesInBuffer, (_buffer.Length - bytesToRead));

			// Shift buffer right, keep as read-ahead
			Array.Copy(_buffer, 0, _buffer, bytesToRead, _bytesInBuffer);

			ReadDataIntoBuffer(position, 0, bytesToRead, extraBytesToBuffer);
		}

		private void ReadDataIntoBuffer(long position, int bufferReadOffset, int bytesToRead, int extraBytesToBuffer)
		{
			bytesToRead += extraBytesToBuffer;
			bytesToRead = Math.Min(bytesToRead, (_buffer.Length - _bytesInBuffer));
			bytesToRead = (int)Math.Min(bytesToRead, (Length - position));

			if (bytesToRead > 0)
			{
				_dataReader.Position = position;
				_bytesInBuffer += _dataReader.Read(_buffer, bufferReadOffset, bytesToRead);
			}
		}

		/// <summary>
		/// Fills the remainder of the buffer with zero padding bytes so that at
		/// least <paramref name="count"/> bytes are available.
		/// </summary>
		/// <param name="count">the minimum number of bytes available in the buffer</param>
		private void ZeroPadBuffer(int count)
		{
			int requestedBytesInBuffer = (_bufferOffset + count);
			if (_bytesInBuffer < requestedBytesInBuffer)
			{
				_zeroPadding = (requestedBytesInBuffer - _bytesInBuffer);
				_bytesInBuffer += _zeroPadding;
				Array.Clear(_buffer, (_bytesInBuffer - _zeroPadding), _zeroPadding);
			}
		}
	}
}
