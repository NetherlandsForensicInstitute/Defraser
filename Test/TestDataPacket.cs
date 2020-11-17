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
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="DataPacket"/> class.
	/// </summary>
	[TestFixture]
	public class TestDataPacket
	{
		#region Test data
		/// <summary>The <c>Length</c> of the test file (of the data packet).</summary>
		private const long InputFileLength = 1024L;
		/// <summary><c>Length</c> of the data packet.</summary>
		private const long Length = 100L;
		/// <summary><c>StartOffset</c> of the data packet.</summary>
		private const long StartOffset = 10L;
		/// <summary><c>Length</c> of the data packet.</summary>
		private const long EndOffset = (StartOffset + Length);
		/// <summary>The <c>Length</c> of a different test file.</summary>
		private const long DifferentInputFileLength = 2048L;
		/// <summary><c>StartOffset</c> of a different data packet.</summary>
		private const long DifferentStartOffset = 99L;
		/// <summary><c>Length</c> of a different data packet.</summary>
		private const long DifferentLength = 817L;
		/// <summary><c>StartOffset</c> adjacent with first data packet.</summary>
		private const long AdjacentStartOffset = EndOffset;
		/// <summary><c>StartOffset</c> nonadjacent with first data packet.</summary>
		private const long NonAdjacentStartOffset = (EndOffset + 10L);
		/// <summary>Relative offset of the sub-packet.</summary>
		private const long SubPacketOffset = 2L;
		/// <summary><c>Length</c> of the sub-packet.</summary>
		private const long SubPacketLength = 10L;
		/// <summary><c>StartOffset</c> of the sub-packet.</summary>
		private const long SubPacketStartOffset = (StartOffset + SubPacketOffset);
		/// <summary><c>EndOffset</c> of the sub-packet.</summary>
		private const long SubPacketEndOffset = (SubPacketStartOffset + SubPacketLength);
		/// <summary>Relative offset of fragment.</summary>
		private const long FragmentOffset = 2L;
		/// <summary>The <c>Length</c> of the data packet for the <c>IsFragmented()</c> test.</summary>
		private const long DataPacketLength = 17L;
		/// <summary>The <c>Length</c> of the data packet fragment for the <c>IsFragmented()</c> test.</summary>
		private const long DataPacketFragmentLength = 8L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile1;
		private IInputFile _inputFile2;
		private IDataPacket _dataPacket1;
		private IDataPacket _dataPacket2;
		private Creator<IDataPacket, IDataPacket, IDataPacket> _appendDataPackets;
		#endregion Mocks and stubs

		#region Objects under test
		private IDataPacket _dataPacket;
		private IDataPacket _dataPacketConstructor1;
		private IDataPacket _differentDataPacket;
		private IDataPacket _duplicateDataPacket;
		private IDataPacket _dataPacketDifferentInputFile;
		private IDataPacket _dataPacketDifferentOffset;
		private IDataPacket _dataPacketDifferentLength;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile1 = MockRepository.GenerateStub<IInputFile>();
			_inputFile2 = MockRepository.GenerateStub<IInputFile>();
			_dataPacket1 = MockRepository.GenerateStub<IDataPacket>();
			_dataPacket2 = MockRepository.GenerateStub<IDataPacket>();
			_appendDataPackets = _mockRepository.StrictMock<Creator<IDataPacket, IDataPacket, IDataPacket>>();

			_inputFile1.Stub(x => x.Name).Return(string.Empty);
			_inputFile1.Stub(x => x.Length).Return(InputFileLength);
			_inputFile2.Stub(x => x.Name).Return(string.Empty);
			_inputFile2.Stub(x => x.Length).Return(DifferentInputFileLength);

			_dataPacket = new DataPacket(_appendDataPackets, _inputFile1, StartOffset, Length);
			_dataPacketConstructor1 = new DataPacket(_appendDataPackets, _inputFile1);
			_differentDataPacket = new DataPacket(_appendDataPackets, _inputFile2, DifferentStartOffset, DifferentLength);
			_duplicateDataPacket = new DataPacket(_appendDataPackets, _inputFile1, StartOffset, Length);
			_dataPacketDifferentInputFile = new DataPacket(_appendDataPackets, _inputFile2, StartOffset, Length);
			_dataPacketDifferentOffset = new DataPacket(_appendDataPackets, _inputFile1, DifferentStartOffset, Length);
			_dataPacketDifferentLength = new DataPacket(_appendDataPackets, _inputFile1, StartOffset, DifferentLength);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile1 = null;
			_inputFile2 = null;
			_dataPacket1 = null;
			_dataPacket2 = null;
			_appendDataPackets = null;
			_dataPacket = null;
			_dataPacketConstructor1 = null;
			_differentDataPacket = null;
			_duplicateDataPacket = null;
			_dataPacketDifferentInputFile = null;
			_dataPacketDifferentOffset = null;
			_dataPacketDifferentLength = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor1AppendDataPacketsNull()
		{
			new DataPacket(null, _inputFile1);
		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void Constructor1InputFileNull()
		{
			new DataPacket(_appendDataPackets, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor2AppendDataPacketsNull()
		{
			new DataPacket(null, _inputFile1, 0L, 1L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor2InputFileNull()
		{
			new DataPacket(_appendDataPackets, null, 0L, 1L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor2NegativeOffset()
		{
			new DataPacket(_appendDataPackets, _inputFile1, -1L, 1L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor2NegativeLength()
		{
			new DataPacket(_appendDataPackets, _inputFile1, 0L, -1L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor2ZeroLength()
		{
			new DataPacket(_appendDataPackets, _inputFile1, 0L, 0L);
		}

		[Test]
		public void Constructor2EndOffsetIsEndOfFile()
		{
			IDataPacket dataPacket = new DataPacket(_appendDataPackets, _inputFile1, 500L, 524L);
			Assert.AreEqual(InputFileLength, dataPacket.EndOffset);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor2EndOffsetBeyondEndOfFile()
		{
			new DataPacket(_appendDataPackets, _inputFile1, 500L, 525L);
		}
		#endregion Tests for constructor arguments

		#region Tests for properties
		[Test]
		public void InputFileConstructor1()
		{
			Assert.AreSame(_inputFile1, _dataPacketConstructor1.InputFile);
		}

		[Test]
		public void InputFileConstructor2()
		{
			Assert.AreSame(_inputFile1, _dataPacket.InputFile);
		}

		[Test]
		public void LengthConstructor1()
		{
			Assert.AreEqual(InputFileLength, _dataPacketConstructor1.Length);
		}

		[Test]
		public void LengthConstructor2()
		{
			Assert.AreEqual(Length, _dataPacket.Length);
		}

		[Test]
		public void StartOffsetConstructor1()
		{
			Assert.AreEqual(0L, _dataPacketConstructor1.StartOffset);
		}

		[Test]
		public void StartOffsetConstructor2()
		{
			Assert.AreEqual(StartOffset, _dataPacket.StartOffset);
		}

		[Test]
		public void EndOffsetConstructor1()
		{
			Assert.AreEqual(InputFileLength, _dataPacketConstructor1.EndOffset);
		}

		[Test]
		public void EndOffsetConstructor2()
		{
			Assert.AreEqual(EndOffset, _dataPacket.EndOffset);
		}
		#endregion Tests for properties

		#region Tests for Append() method
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AppendNull()
		{
			_dataPacket.Append(null);
		}

		[Test]
		public void AppendAdjacentDataPacket()
		{
			IDataPacket adjacentDataPacket = new DataPacket(_appendDataPackets, _inputFile1, AdjacentStartOffset, DifferentLength);
			IDataPacket concatenatedDataPacket = _dataPacket.Append(adjacentDataPacket);
			Assert.IsInstanceOfType(typeof(DataPacket), concatenatedDataPacket, "Type");
			Assert.AreSame(_inputFile1, concatenatedDataPacket.InputFile, "InputFile");
			Assert.AreEqual((Length + DifferentLength), concatenatedDataPacket.Length, "Length");
			Assert.AreEqual(StartOffset, concatenatedDataPacket.StartOffset, "StartOffset");
			Assert.AreEqual((EndOffset + DifferentLength), concatenatedDataPacket.EndOffset, "EndOffset");
		}

		[Test]
		public void AppendNonadjacentDataPacket()
		{
			IDataPacket nonAdjacentDataPacket = new DataPacket(_appendDataPackets, _inputFile1, NonAdjacentStartOffset, DifferentLength);
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_appendDataPackets.Expect(x => x(_dataPacket, nonAdjacentDataPacket)).Return(_dataPacket2);
			}).Verify(delegate
			{
				Assert.AreSame(_dataPacket2, _dataPacket.Append(nonAdjacentDataPacket));
			});
		}

		[Test]
		public void AppendAdjacentDataPacketDifferentInputFile()
		{
			IDataPacket adjacentDataPacketDifferentInputFile = new DataPacket(_appendDataPackets, _inputFile2, AdjacentStartOffset, DifferentLength);
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_appendDataPackets.Expect(x => x(_dataPacket, adjacentDataPacketDifferentInputFile)).Return(_dataPacket2);
			}).Verify(delegate
			{
				Assert.That(_dataPacket.Append(adjacentDataPacketDifferentInputFile), Is.SameAs(_dataPacket2), "Append() adjacent data packet with different input file");
			});
		}

		[Test]
		public void AppendDifferentClass()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_appendDataPackets.Expect(x => x(_dataPacket, _dataPacket1)).Return(_dataPacket2);
			}).Verify(delegate
			{
				Assert.AreSame(_dataPacket2, _dataPacket.Append(_dataPacket1));
			});
		}
		#endregion Tests for Append() method

		#region Tests for GetSubPacket() method
		[Test]
		public void GetSubPacket()
		{
			IDataPacket subPacket = _dataPacket.GetSubPacket(SubPacketOffset, SubPacketLength);
			Assert.IsInstanceOfType(typeof(DataPacket), subPacket, "Type");
			Assert.AreSame(_inputFile1, subPacket.InputFile, "InputFile");
			Assert.AreEqual(SubPacketLength, subPacket.Length, "Length");
			Assert.AreEqual(SubPacketStartOffset, subPacket.StartOffset, "StartOffset");
			Assert.AreEqual(SubPacketEndOffset, subPacket.EndOffset, "EndOffset");
		}

		[Test]
		public void GetSubPacketEntirePacket()
		{
			Assert.AreSame(_dataPacket, _dataPacket.GetSubPacket(0, Length), "GetSubPacket(0, Length) returns the data packet itself");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketNegativeOffset()
		{
			_dataPacket.GetSubPacket(-1, Length);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketOffsetTooLarge()
		{
			_dataPacket.GetSubPacket(Length, 1L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketNegativeLength()
		{
			_dataPacket.GetSubPacket(0L, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketZeroLength()
		{
			_dataPacket.GetSubPacket(0L, 0L);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketLengthTooLarge()
		{
			_dataPacket.GetSubPacket(1L, Length);
		}
		#endregion Tests for GetSubPacket() method

		#region Tests for GetFragment() method
		[Test]
		public void GetFragment()
		{
			IDataPacket fragment = _dataPacket.GetFragment(FragmentOffset);
			Assert.IsInstanceOfType(typeof(DataPacket), fragment, "Type");
			Assert.AreSame(_inputFile1, fragment.InputFile, "InputFile");
			Assert.AreEqual((Length - FragmentOffset), fragment.Length, "Length");
			Assert.AreEqual((StartOffset + FragmentOffset), fragment.StartOffset, "StartOffset");
			Assert.AreEqual(EndOffset, fragment.EndOffset, "EndOffset");
		}

		[Test]
		public void GetFragmentOffsetZero()
		{
			Assert.AreSame(_dataPacket, _dataPacket.GetFragment(0L), "GetFragment(0L) returns the data packet itself");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetFragmentOffsetNegative()
		{
			_dataPacket.GetFragment(-1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetFragmentOffsetTooLarge()
		{
			_dataPacket.GetFragment(Length);
		}
		#endregion Tests for GetFragment() method

		#region Tests for IEquatable.Equals() method
		[Test]
		public void IEquatableEqualsDuplicateDataPacket()
		{
			Assert.IsTrue(((DataPacket)_dataPacket).Equals((DataPacket)_duplicateDataPacket));
		}

		[Test]
		public void IEquatableEqualsDifferentDataPacket()
		{
			Assert.IsFalse(((DataPacket)_dataPacket).Equals((DataPacket)_differentDataPacket));
		}

		[Test]
		public void IEquatableEqualsDifferentInputFile()
		{
			Assert.IsFalse(((DataPacket)_dataPacket).Equals((DataPacket)_dataPacketDifferentInputFile));
		}

		[Test]
		public void IEquatableEqualsDifferentOffset()
		{
			Assert.IsFalse(((DataPacket)_dataPacket).Equals((DataPacket)_dataPacketDifferentOffset));
		}

		[Test]
		public void IEquatableEqualsDifferentLength()
		{
			Assert.IsFalse(((DataPacket)_dataPacket).Equals((DataPacket)_dataPacketDifferentLength));
		}

		[Test]
		public void IEquatableEqualsReflexive()
		{
			Assert.IsTrue(((DataPacket)_dataPacket).Equals((DataPacket)_dataPacket));
		}

		[Test]
		public void IEquatableEqualsSymmetric()
		{
			Assert.IsTrue(((DataPacket)_duplicateDataPacket).Equals((DataPacket)_dataPacket), "Equal data packets");
			Assert.IsTrue(((DataPacket)_dataPacket).Equals((DataPacket)_duplicateDataPacket), "Equal data packets");
			Assert.IsFalse(((DataPacket)_dataPacket).Equals((DataPacket)_differentDataPacket), "Different data packets");
			Assert.IsFalse(((DataPacket)_differentDataPacket).Equals((DataPacket)_dataPacket), "Different data packets");
		}

		[Test]
		public void IEquatableEqualsNull()
		{
			Assert.IsFalse(((DataPacket)_dataPacket).Equals(null));
		}
		#endregion Tests for IEquatable.Equals() method

		#region Tests for Equals() method
		[Test]
		public void EqualsDuplicateDataPacket()
		{
			Assert.IsTrue(_dataPacket.Equals(_duplicateDataPacket));
		}

		[Test]
		public void EqualsDifferentDataPacket()
		{
			Assert.IsFalse(_dataPacket.Equals(_differentDataPacket));
		}

		[Test]
		public void EqualsDifferentInputFile()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentInputFile));
		}

		[Test]
		public void EqualsDifferentOffset()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentOffset));
		}

		[Test]
		public void EqualsDifferentLength()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentLength));
		}

		[Test]
		public void EqualsReflexive()
		{
			Assert.IsTrue(_dataPacket.Equals(_dataPacket));
		}

		[Test]
		public void EqualsSymmetric()
		{
			Assert.IsTrue(_duplicateDataPacket.Equals(_dataPacket), "Equal data packets");
			Assert.IsTrue(_dataPacket.Equals(_duplicateDataPacket), "Equal data packets");
			Assert.IsFalse(_dataPacket.Equals(_differentDataPacket), "Different data packets");
			Assert.IsFalse(_differentDataPacket.Equals(_dataPacket), "Different data packets");
		}

		[Test]
		public void EqualsDifferentClass()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacket1));
		}

		[Test]
		public void EqualsNull()
		{
			Assert.IsFalse(_dataPacket.Equals(null));
		}
		#endregion Tests for Equals() method

		#region Tests for GetHashCode() method
		[Test]
		public void GetHashCodeConsistentWithEquals()
		{
			Assert.AreEqual(_dataPacket.GetHashCode(), _duplicateDataPacket.GetHashCode());
		}

		[Test]
		public void GetHashCodeDependsOnInputFile()
		{
			Assert.AreNotEqual(_dataPacket.GetHashCode(), _dataPacketDifferentInputFile.GetHashCode());
		}

		[Test]
		public void GetHashCodeDependsOnOffset()
		{
			Assert.AreNotEqual(_dataPacket.GetHashCode(), _dataPacketDifferentOffset.GetHashCode());
		}

		[Test]
		public void GetHashCodeDependsOnLength()
		{
			Assert.AreNotEqual(_dataPacket.GetHashCode(), _dataPacketDifferentLength.GetHashCode());
		}
		#endregion Tests for GetHashCode() method

		#region Tests for IsFragmented() extension method
		[Test]
		public void IsFragmentedFalse()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataPacket1.Stub(x => x.GetFragment(0L)).Return(_dataPacket2);
				_dataPacket1.Stub(x => x.Length).Return(DataPacketLength);
				_dataPacket2.Stub(x => x.Length).Return(DataPacketLength);
			}).Verify(delegate
			{
				Assert.IsFalse(_dataPacket1.IsFragmented());
			});
		}

		[Test]
		public void IsFragmentedTrue()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dataPacket1.Stub(x => x.GetFragment(0L)).Return(_dataPacket2);
				_dataPacket1.Stub(x => x.Length).Return(DataPacketLength);
				_dataPacket2.Stub(x => x.Length).Return(DataPacketFragmentLength);
			}).Verify(delegate
			{
				Assert.IsTrue(_dataPacket1.IsFragmented());
			});
		}
		#endregion Tests for IsFragmented() extension method
	}
}
