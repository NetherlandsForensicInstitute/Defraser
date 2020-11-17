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
using System.IO;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Provides a template for testing <typeparamref name="TDataReader"/>.
	/// The setup and teardown methods can be overridden to implement custom
	/// initialization of test data.
	/// </summary>
	/// <remarks>
	/// The <em>Common tests</em> region contains the actual tests, which
	/// have to be called from a subclass in order to be executed.
	/// </remarks>
	/// <typeparam name="TDataReader">the type of data reader to test</typeparam>
	public abstract class DataReaderTest<TDataReader> where TDataReader : class, IDataReader
	{
		/// <summary>The name of the data file, generated on-the-fly.</summary>
		protected const string DataFileName = "fibonacci.dat";
		/// <summary>The length of the data file in bytes.</summary>
		protected const int DataFileSize = 80137;
		/// <summary>The size of the read buffer.</summary>
		private const int ReadBufferSize = 4096;

		/// <summary>The read buffer.</summary>
		protected byte[] Buffer;
		/// <summary>The input file.</summary>
		protected IInputFile InputFile;
		/// <summary>The data reader being tested.</summary> 
		protected TDataReader _dataReader;

		private byte[] _fibonacci;

		#region Properties
		/// <summary>The data reader bytes used for verification.</summary>
		public virtual byte[] DataReaderData
		{
			get { return _fibonacci; }
		}

		/// <summary>The length of <c>DataReaderData</c>.</summary>
		public int DataReaderLength { get { return DataReaderData.Length; } }
		#endregion Properties


		[TestFixtureSetUp]
		public virtual void TestFixtureSetup()
		{
			File.Delete(DataFileName);

			_fibonacci = new byte[DataFileSize];
			Buffer = new byte[ReadBufferSize];
			InputFile = MockRepository.GenerateStub<IInputFile>();
			InputFile.Stub(x => x.Name).Return(DataFileName);
			InputFile.Stub(x => x.Length).Return(DataFileSize);

			// Generate fibonacci numbers (periodic function)
			_fibonacci[0] = 0;
			_fibonacci[1] = 1;
			for (int i = 2; i < _fibonacci.Length; i++)
			{
				_fibonacci[i] = (byte)(_fibonacci[i - 1] + _fibonacci[i - 2]);
			}

			// Write test data to file
			using (FileStream fileStream = new FileStream(DataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				fileStream.Write(_fibonacci, 0, _fibonacci.Length);
				fileStream.Flush();
			}
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTeardown()
		{
			File.Delete(DataFileName);

			_fibonacci = null;
			Buffer = null;
			InputFile = null;
		}

		[SetUp]
		public virtual void SetUp()
		{
			_dataReader = null;
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (_dataReader != null)
			{
				_dataReader.Dispose();
				_dataReader = null;
			}
		}

		#region Common tests
		[Test]
		public void GetPositionStartMiddleEnd()
		{
			Assert.AreEqual(0, _dataReader.Position, "Position (initial)");
			_dataReader.Position = 17;
			_dataReader.Read(Buffer, 0, 20);
			Assert.AreEqual(17, _dataReader.Position, "Position (after read)");
			_dataReader.Position = 0;
			_dataReader.Position = DataReaderLength;
			Assert.AreEqual(DataReaderLength, _dataReader.Position, "Position (end-of-input)");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetNegativePosition()
		{
			_dataReader.Position = -1;
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetInvalidPosition()
		{
			_dataReader.Position = DataReaderLength + 1;
		}

		[Test]
		public void GetLength()
		{
			Assert.AreEqual(DataReaderLength, _dataReader.Length, "Length");
		}

		[Test]
		public void GetStateAll()
		{
			Assert.AreEqual(DataReaderState.Ready, _dataReader.State, "Initial State");
			_dataReader.Position = DataReaderLength;
			Assert.AreEqual(DataReaderState.EndOfInput, _dataReader.State, "State at end-of-input");
			_dataReader.Dispose();
			Assert.AreEqual(DataReaderState.Cancelled, _dataReader.State, "State after data reader has been disposed");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetDataPacketNegativeOffset()
		{
			_dataReader.GetDataPacket(-1, DataReaderLength);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetDataPacketNegativeLength()
		{
			_dataReader.GetDataPacket(0, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetDataPacketLengthOverflow()
		{
			_dataReader.GetDataPacket(0, DataReaderLength + 10);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetDataPacketOffsetPlusLengthOverflow()
		{
			_dataReader.GetDataPacket(10, DataReaderLength);
		}

		[Test]
		public virtual void GetDataPacketEntireFile()
		{
			TestDataReaderGetDataPacket(_dataReader, 0, DataReaderLength);
		}

		[Test]
		public void GetDataPacketInputFile()
		{
			Assert.AreEqual(InputFile, _dataReader.GetDataPacket(0, DataReaderLength).InputFile,
							"GetDataPacket().InputFile should be argument of constructor");
		}

		public void Read(long position, int length)
		{
			_dataReader.Position = position;
			Assert.AreEqual(length, _dataReader.Read(Buffer, 0, length), "Number of bytes read returned by Read()");
			Assert.AreEqual(position, _dataReader.Position, "Position after Read()");
			DataReaderState state = (_dataReader.Position == DataReaderLength) ? DataReaderState.EndOfInput : DataReaderState.Ready;
			Assert.AreEqual(state, _dataReader.State, "State after Read()");
			Assert.IsTrue(CompareArrays(Buffer, 0, DataReaderData, (int)position, length), "Result (data) of Read()");
		}

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void ReadDisposed()
		{
			_dataReader.Dispose();
			_dataReader.Read(Buffer, 0, Buffer.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ReadArrayNull()
		{
			_dataReader.Read(null, 0, Buffer.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadNegativeArrayOffset()
		{
			_dataReader.Read(Buffer, -1, Buffer.Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadNegativeCount()
		{
			_dataReader.Read(Buffer, 0, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReadCountOverflow()
		{
			_dataReader.Read(Buffer, 0, 2 * Buffer.Length);
		}

		public void ReadArrayOffsetPlusCountOverflow()
		{
			_dataReader.Read(Buffer, 100, Buffer.Length);
		}
		#endregion Common tests

		/// <summary>
		/// Tests <c>GetDataPacket()</c> method for <paramref name="dataReader"/>.
		/// </summary>
		/// <param name="dataReader">the data reader</param>
		/// <param name="offset">the offset for the data packet</param>
		/// <param name="length">the length for the data packet</param>
		protected void TestDataReaderGetDataPacket(IDataReader dataReader, long offset, long length)
		{
			string name = dataReader.GetType().Name;
			IDataPacket dataPacket = dataReader.GetDataPacket(offset, length);
			Assert.IsNotNull(dataPacket.InputFile, "{0}.GetDataPacket().InputFile", name);
			Assert.AreEqual(offset, dataPacket.StartOffset, "{0}.GetDataPacket().StartOffset", name);
			Assert.AreEqual(length, dataPacket.Length, "{0}.GetDataPacket().Length", name);
			Assert.AreEqual(dataPacket.Length, dataPacket.GetFragment(0).Length, "{0}.GetDataPacket().GetFragment() - Single fragment (unfragmented)", name);
		}

		/// <summary>
		/// Compares a subsection of two arrays for equality.
		/// </summary>
		/// <param name="array">the array to compare</param>
		/// <param name="index">the index in the array to compare</param>
		/// <param name="toArray">the array to compare to</param>
		/// <param name="toIndex">the index in the array to compare to</param>
		/// <param name="length">the number of bytes to compare</param>
		/// <returns>whether the subsections of arrays are equal</returns>
		protected bool CompareArrays(byte[] array, int index, byte[] toArray, int toIndex, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (array[i + index] != toArray[i + toIndex])
				{
					return false;
				}
			}
			return true;
		}
	}
}

