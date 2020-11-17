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
using System.Linq;
using Defraser.DataStructures;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="DataPacketNode"/> class.
	/// </summary>
	[TestFixture]
	public class TestDataPacketNode
	{
		#region Test data
		/// <summary><c>StartOffset</c> of first data packet.</summary>
		private const long StartOffset1 = 10L;
		/// <summary><c>EndOffset</c> of first data packet.</summary>
		private const long EndOffset1 = 127L;
		/// <summary><c>Length</c> of first data packet.</summary>
		private const long Length1 = 117L;
		/// <summary><c>StartOffset</c> of second data packet.</summary>
		private const long StartOffset2 = 299L;
		/// <summary><c>EndOffset</c> of second data packet.</summary>
		private const long EndOffset2 = 4583L;
		/// <summary><c>Length</c> of second data packet.</summary>
		private const long Length2 = 55L;
		/// <summary><c>StartOffset</c> of third data packet.</summary>
		private const long StartOffset3 = 6011L;
		/// <summary><c>EndOffset</c> of third data packet.</summary>
		private const long EndOffset3 = 6022L;
		/// <summary><c>Length</c> of third data packet.</summary>
		private const long Length3 = 11L;
		/// <summary>Relative offset of the sub-packet.</summary>
		private const long SubPacketOffset = 2L;
		/// <summary><c>Length</c> of the sub-packet.</summary>
		private const long SubPacketLength = 10L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile1;
		private IInputFile _inputFile2;
		private IInputFile _inputFile3;
		private IDataPacket _dataPacket1;
		private IDataPacket _dataPacket2;
		private IDataPacket _dataPacket3;
		private IDataPacket _subPacket1;
		private IDataPacket _subPacket2;
		#endregion Mocks and stubs

		#region Objects under test
		private IDataPacket _dataPacket;
		private IDataPacket _duplicateDataPacket;
		private IDataPacket _dataPacketDifferentFirst;
		private IDataPacket _dataPacketDifferentSecond;
		private IDataPacket _dataPacketSubPacketsOutOfOrder;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile1 = MockRepository.GenerateStub<IInputFile>();
			_inputFile2 = MockRepository.GenerateStub<IInputFile>();
			_inputFile3 = MockRepository.GenerateStub<IInputFile>();
			_dataPacket1 = CreateDataPacketStub(_inputFile1, Length1, StartOffset1, EndOffset1);
			_dataPacket2 = CreateDataPacketStub(_inputFile2, Length2, StartOffset2, EndOffset2);
			_dataPacket3 = CreateDataPacketStub(_inputFile3, Length3, StartOffset3, EndOffset3);
			_subPacket1 = MockRepository.GenerateStub<IDataPacket>();
			_subPacket2 = MockRepository.GenerateStub<IDataPacket>();

			_dataPacket = new DataPacketNode(_dataPacket1, _dataPacket2);
			_duplicateDataPacket = new DataPacketNode(_dataPacket1, _dataPacket2);
			_dataPacketDifferentFirst = new DataPacketNode(_dataPacket3, _dataPacket2);
			_dataPacketDifferentSecond = new DataPacketNode(_dataPacket1, _dataPacket3);
			_dataPacketSubPacketsOutOfOrder = new DataPacketNode(_dataPacket2, _dataPacket1);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile1 = null;
			_inputFile2 = null;
			_inputFile3 = null;
			_dataPacket1 = null;
			_dataPacket2 = null;
			_dataPacket3 = null;
			_dataPacket = null;
			_duplicateDataPacket = null;
			_dataPacketDifferentFirst = null;
			_dataPacketDifferentSecond = null;
			_dataPacketSubPacketsOutOfOrder = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorFirstNull()
		{
			new DataPacketNode(null, _dataPacket1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorSecondNull()
		{
			new DataPacketNode(_dataPacket1, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for properties
		public void InputFile()
		{
			Assert.AreSame(_inputFile1, _dataPacket.InputFile);
		}

		[Test]
		public void Length()
		{
			Assert.AreEqual((Length1 + Length2), _dataPacket.Length);
		}

		[Test]
		public void LengthIsCached()
		{
			IDataPacket subPacket1 = _mockRepository.StrictMock<IDataPacket>();
			IDataPacket subPacket2 = _mockRepository.StrictMock<IDataPacket>();
			IDataPacket dataPacket = null;

			With.Mocks(_mockRepository).Expecting(delegate
			{
				subPacket1.Expect(x => x.Length).Return(Length1).Repeat.Any();
				subPacket2.Expect(x => x.Length).Return(Length2).Repeat.Any();
			}).Verify(delegate
			{
				dataPacket = new DataPacketNode(subPacket1, subPacket2);
			});

			With.Mocks(_mockRepository).Expecting(delegate
			{
				// No calls on the sub-packets expected!!
			}).Verify(delegate
			{
				Assert.AreEqual((Length1 + Length2), dataPacket.Length);
			});
		}

		[Test]
		public void StartOffsetSubPacketsInOrder()
		{
			Assert.AreEqual(StartOffset1, _dataPacket.StartOffset);
		}

		[Test]
		public void StartOffsetSubPacketsOutOfOrder()
		{
			Assert.AreEqual(StartOffset1, _dataPacketSubPacketsOutOfOrder.StartOffset);
		}

		[Test]
		public void EndOffsetSubPacketsInOrder()
		{
			Assert.AreEqual(EndOffset2, _dataPacket.EndOffset);
		}

		[Test]
		public void EndOffsetSubPacketsOutOfOrder()
		{
			Assert.AreEqual(EndOffset2, _dataPacketSubPacketsOutOfOrder.EndOffset);
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
		public void AppendCreatesNewNode()
		{
			Assert.AreEqual(new DataPacketNode(_dataPacketSubPacketsOutOfOrder, _dataPacket3), _dataPacketSubPacketsOutOfOrder.Append(_dataPacket3));
		}

		[Test]
		public void AppendPreservesBalancedTree()
		{
			IDataPacket concatenatedSecondSubPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataPacket2.Stub(x => x.Append(_dataPacket3)).Return(concatenatedSecondSubPacket);
			Assert.AreEqual(new DataPacketNode(_dataPacket1, concatenatedSecondSubPacket), _dataPacket.Append(_dataPacket3));
		}
		#endregion Tests for Append() method

		#region Tests for GetSubPacket() method
		[Test]
		public void GetSubPacketFirstSubPacket()
		{
			_dataPacket1.Stub(x => x.GetSubPacket(SubPacketOffset, SubPacketLength)).Return(_subPacket1);
			Assert.AreSame(_subPacket1, _dataPacket.GetSubPacket(SubPacketOffset, SubPacketLength));
		}

		[Test]
		public void GetSubPacketSecondSubPacket()
		{
			_dataPacket2.Stub(x => x.GetSubPacket(SubPacketOffset, SubPacketLength)).Return(_subPacket1);
			Assert.AreSame(_subPacket1, _dataPacket.GetSubPacket((Length1 + SubPacketOffset), SubPacketLength));
		}

		[Test]
		public void GetSubPacketBothSubPackets()
		{
			_dataPacket1.Stub(x => x.GetSubPacket(SubPacketOffset, (Length1 - SubPacketOffset))).Return(_subPacket1);
			_dataPacket2.Stub(x => x.GetSubPacket(0L, SubPacketOffset)).Return(_subPacket2);
			IDataPacket concatenatedSubPackets = MockRepository.GenerateStub<IDataPacket>();
			_subPacket1.Stub(x => x.Append(_subPacket2)).Return(concatenatedSubPackets);
			
			Assert.AreSame(concatenatedSubPackets, _dataPacket.GetSubPacket(SubPacketOffset, Length1));
		}

		[Test]
		public void GetSubPacketEntirePacket()
		{
			Assert.AreSame(_dataPacket, _dataPacket.GetSubPacket(0L, (Length1 + Length2)));
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketNegativeOffset()
		{
			_dataPacket.GetSubPacket(-1, Length1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetSubPacketOffsetTooLarge()
		{
			_dataPacket.GetSubPacket((Length1 + Length2), 1L);
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
			_dataPacket.GetSubPacket(1L, (Length1 + Length2));
		}
		#endregion Tests for GetSubPacket() method

		#region Tests for GetFragment() method
		[Test]
		public void GetFragmentFirstFragment()
		{
			_dataPacket1.Stub(x => x.GetFragment(SubPacketOffset)).Return(_subPacket1);
			Assert.AreSame(_subPacket1, _dataPacket.GetFragment(SubPacketOffset));
		}

		[Test]
		public void GetFragmentSecondFragment()
		{
			_dataPacket2.Stub(x => x.GetFragment(SubPacketOffset)).Return(_subPacket1);
			Assert.AreSame(_subPacket1, _dataPacket.GetFragment(Length1 + SubPacketOffset));
		}

		[Test]
		public void GetFragmentOffsetZero()
		{
			Assert.AreNotSame(_dataPacket, _dataPacket.GetFragment(0L), "Should not return itself");
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
			_dataPacket.GetFragment((Length1 + Length2));
		}
		#endregion Tests for GetFragment() method

		#region Tests for IEquatable.Equals() method
		public void IEquatableEqualsDuplicateDataPacket()
		{
			Assert.IsTrue(((DataPacketNode)_dataPacket).Equals((DataPacketNode)_duplicateDataPacket));
		}

		[Test]
		public void IEquatableEqualsDifferentDataPacket()
		{
			Assert.IsFalse(((DataPacketNode)_dataPacket).Equals((DataPacketNode)_dataPacketDifferentFirst));
		}

		[Test]
		public void IEquatableEqualsReflexive()
		{
			Assert.IsTrue(((DataPacketNode)_dataPacket).Equals((DataPacketNode)_dataPacket));
		}

		[Test]
		public void IEquatableEqualsSymmetric()
		{
			Assert.IsTrue(((DataPacketNode)_duplicateDataPacket).Equals((DataPacketNode)_dataPacket), "Equal data packets");
			Assert.IsTrue(((DataPacketNode)_dataPacket).Equals((DataPacketNode)_duplicateDataPacket), "Equal data packets");
			Assert.IsFalse(((DataPacketNode)_dataPacket).Equals((DataPacketNode)_dataPacketDifferentFirst), "Different data packets");
			Assert.IsFalse(((DataPacketNode)_dataPacketDifferentFirst).Equals((DataPacketNode)_dataPacket), "Different data packets");
		}

		[Test]
		public void IEquatableEqualsNull()
		{
			Assert.IsFalse(((DataPacketNode)_dataPacket).Equals(null));
		}
		#endregion Tests for IEquatable.Equals() method

		#region Tests for Equals() method
		[Test]
		public void EqualsDuplicateDataPacket()
		{
			Assert.IsTrue(_dataPacket.Equals(_duplicateDataPacket));
		}

		[Test]
		public void EqualsDifferentFirstSubPacket()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentFirst));
		}

		[Test]
		public void EqualsDifferentSecondSubPacket()
		{
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentSecond));
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
			Assert.IsFalse(_dataPacket.Equals(_dataPacketDifferentFirst), "Different data packets");
			Assert.IsFalse(_dataPacketDifferentFirst.Equals(_dataPacket), "Different data packets");
		}

		[Test]
		public void EqualsDifferentClass()
		{
			Assert.IsFalse(_dataPacket.Equals(_subPacket1));
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
		public void GetHashCodeDependsOnFirstSubPacket()
		{
			Assert.AreNotEqual(_dataPacket.GetHashCode(), _dataPacketDifferentFirst);
		}

		[Test]
		public void GetHashCodeDependsOnSecondSubPacket()
		{
			Assert.AreNotEqual(_dataPacket.GetHashCode(), _dataPacketDifferentSecond);
		}
		#endregion Tests for GetHashCode() method

		#region Setup and expectation helpers
		private IDataPacket CreateDataPacketStub(IInputFile inputFile, long length, long startOffset, long endOffset)
		{
			IDataPacket dataPacket = MockRepository.GenerateStub<IDataPacket>();
			dataPacket.Stub(x => x.InputFile).Return(inputFile);
			dataPacket.Stub(x => x.Length).Return(length);
			dataPacket.Stub(x => x.StartOffset).Return(startOffset);
			dataPacket.Stub(x => x.EndOffset).Return(endOffset);
			return dataPacket;
		}
		#endregion Setup and expectation helpers
	}
}
