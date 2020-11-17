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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="SubProgressReporter"/> class.
	/// </summary>
	[TestFixture]
	public class TestFileDataWriter
	{
		#region Test data
		/// <summary>The path name of the file to write to.</summary>
		private const string FileName = "TestFileDataWriter__output.mpg";
		/// <summary><c>Length</c> of short <see cref="IDataReader"/> for reading one byte at-a-time.</summary>
		private const long ShortDataReaderLength = 25L;
		/// <summary><c>Length</c> of the first <see cref="IDataReader"/> (data) to write.</summary>
		private const long DataReaderLength1 = 20000L;
		/// <summary><c>Length</c> of the second <see cref="IDataReader"/> (data) to write.</summary>
		private const long DataReaderLength2 = 40000L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IDataReader _dataReader;
		#endregion Mocks and stubs

		#region Objects under test
		private IDataWriter _dataWriter;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			File.Delete(FileName);

			_mockRepository = new MockRepository();
			_dataReader = _mockRepository.StrictMock<IDataReader>();

			_dataWriter = new FileDataWriter(FileName);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_dataReader = null;
			_dataWriter.Dispose();
			_dataWriter = null;

			File.Delete(FileName);
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorNull()
		{
			new FileDataWriter(null);
		}

		[Test]
		[ExpectedException(typeof(IOException))]
		public void ConstructorOpensOutputFile()
		{
			File.Delete(FileName);
		}
		#endregion Tests for constructor arguments

		#region Tests for Dispose() method
		[Test]
		public void DisposeClosesOutputFile()
		{
			_dataWriter.Dispose();
			File.Delete(FileName);
		}
		#endregion Tests for Dispose() method

		#region Tests for Write() method
		[Test]
		public void WriteDataOneByteAtATime()
		{
			IDataReader dataReader = CreateFakeDataReader(ShortDataReaderLength, ReadOneByte);

			_dataWriter.Write(dataReader);
			_dataWriter.Dispose();

			AssertFileDataIsCorrect(AssertReadFileData(ShortDataReaderLength));
		}

		[Test]
		public void WriteDataInBlocks()
		{
			IDataReader dataReader = CreateFakeDataReader(DataReaderLength1, ReadBlock);

			_dataWriter.Write(dataReader);
			_dataWriter.Dispose();

			AssertFileDataIsCorrect(AssertReadFileData(DataReaderLength1));
		}

		[Test]
		public void WriteDataReaderReadShouldReceiveValidArguments()
		{
			IDataReader dataReader = CreateFakeDataReader(DataReaderLength1, ReadValidateArguments);

			_dataWriter.Write(dataReader);
		}

		[Test]
		public void WriteAppendToPreviousData()
		{
			IDataReader dataReader1 = CreateFakeDataReader(DataReaderLength1, ReadBlock);
			IDataReader dataReader2 = CreateFakeDataReader(DataReaderLength2, ReadBlock);

			_dataWriter.Write(dataReader1);
			_dataWriter.Write(dataReader2);
			_dataWriter.Dispose();

			byte[] data = AssertReadFileData(DataReaderLength1 + DataReaderLength2);
			AssertFileDataIsCorrect(data.Take((int)DataReaderLength1));
			AssertFileDataIsCorrect(data.Skip((int)DataReaderLength1));
		}

		[Test]
		public void WriteCancelledUponEntry()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataReader.Expect(x => x.State).Return(DataReaderState.Cancelled).Repeat.AtLeastOnce();
				_dataReader.Stub(x => x.Position).PropertyBehavior();
			}).Verify(delegate
			{
				_dataWriter.Write(_dataReader);
			});
		}

		[Test]
		public void WriteCancelledBetweenReads()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				bool read = false;
				_dataReader.Stub(x => x.Position).PropertyBehavior();
				_dataReader.Stub(x => x.Length).Return(DataReaderLength1);
				_dataReader.Stub(x => x.State).Return(DataReaderState.Ready)
						.WhenCalled(i => i.ReturnValue = read ? DataReaderState.Cancelled : DataReaderState.Ready);
				_dataReader.Expect(x => x.Read(Arg<byte[]>.Is.NotNull, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(1)
						.WhenCalled(i => read = true);
			}).Verify(delegate
			{
				_dataWriter.Write(_dataReader);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void WriteNull()
		{
			_dataWriter.Write(null);
		}
		#endregion Tests for Write() method

		#region Setup and expectation helpers
		private static IDataReader CreateFakeDataReader(long length, Func<IDataReader, byte[], int, int, int> readFunc)
		{
			IDataReader dataReader = MockRepository.GenerateStub<IDataReader>();
			dataReader.Stub(x => x.Length).Return(length);
			dataReader.Stub(x => x.State).Return(DataReaderState.Ready)
					.WhenCalled(i => i.ReturnValue = GetDataReaderState(dataReader));
			dataReader.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Return(0)
					.WhenCalled(i =>
					{
						byte[] array = i.Arguments[0] as byte[];
						int arrayOffset = (int)i.Arguments[1];
						int count = (int)i.Arguments[2];
						i.ReturnValue = readFunc(dataReader, array, arrayOffset, count);
					});
			return dataReader;
		}

		private static DataReaderState GetDataReaderState(IDataReader dataReader)
		{
			return (dataReader.Position >= dataReader.Length) ? DataReaderState.EndOfInput : DataReaderState.Ready;
		}

		private static int ReadOneByte(IDataReader dataReader, byte[] array, int arrayOffset, int count)
		{
			array[arrayOffset] = GetFakeDataByte(dataReader.Position + arrayOffset);
			return 1;
		}

		private static int ReadBlock(IDataReader dataReader, byte[] array, int arrayOffset, int count)
		{
			long position = dataReader.Position;
			int bytesRead = (int)Math.Min(count, (dataReader.Length - position));
			for (int i = 0; i < bytesRead; i++)
			{
				array[arrayOffset + i] = GetFakeDataByte(position + arrayOffset + i);
			}
			return bytesRead;
		}

		private static int ReadValidateArguments(IDataReader dataReader, byte[] array, int arrayOffset, int count)
		{
			Assert.IsNotNull(array, "array");
			Assert.GreaterOrEqual(arrayOffset, 0, "arrayOffset not negative");
			Assert.LessOrEqual(arrayOffset, array.Length, "arrayOffset within array");
			Assert.Greater(count, 1, "count positive");
			Assert.LessOrEqual(count, (array.Length - arrayOffset), "count within array");
			Assert.LessOrEqual(count, (dataReader.Length - dataReader.Position), "count within data reader limit");
			return count;
		}

		/// <summary>
		/// Returns the data byte at the given <paramref name="position"/> in the
		/// fake data reader.
		/// This function generates a sequence of pseudo-random bytes for testing
		/// whether the bytes read from the fake data reader are correctly written
		/// to the output file.
		/// </summary>
		/// <param name="position">The position of the data byte to return</param>
		/// <returns>The data byte</returns>
		private static byte GetFakeDataByte(long position)
		{
			return (byte)((43 * position) ^ 0xda);
		}

		private static byte[] AssertReadFileData(long length)
		{
			byte[] data = File.ReadAllBytes(FileName);
			Assert.AreEqual(length, data.Length, "Number of bytes written");
			return data;
		}

		private static void AssertFileDataIsCorrect(IEnumerable<byte> data)
		{
			Assert.AreEqual(data.Count(), data.Where((b, i) => GetFakeDataByte(i) == b).Count(), "File data");
		}
		#endregion Setup and expectation helpers
	}
}
