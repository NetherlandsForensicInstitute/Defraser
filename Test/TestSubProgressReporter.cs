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
	public class TestSubProgressReporter
	{
		#region Test data
		/// <summary>The default start progress percentage.</summary>
		private const int StartProgressPercentage = 20;
		/// <summary>The default end progress percentage.</summary>
		private const int EndProgressPercentage = 70;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IProgressReporter _overallProgressReporter;
		private Object _userState1;
		private Object _userState2;
		private Object _userState3;
		#endregion Mocks and stubs

		#region Objects under test
		private IProgressReporter _progressReporter;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_overallProgressReporter = _mockRepository.StrictMock<IProgressReporter>();
			_userState1 = new Object();
			_userState2 = new Object();
			_userState3 = new Object();

			_progressReporter = new SubProgressReporter(_overallProgressReporter, StartProgressPercentage, EndProgressPercentage);
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_overallProgressReporter = null;
			_userState1 = null;
			_userState2 = null;
			_userState3 = null;
			_progressReporter = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorProgressReporterNull()
		{
			new SubProgressReporter(null, StartProgressPercentage, EndProgressPercentage);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ConstructorStartProgressPercentageNegative()
		{
			new SubProgressReporter(_overallProgressReporter, -1, EndProgressPercentage);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ConstructorEndProgressPercentageLessThanStartProgressPercentage()
		{
			new SubProgressReporter(_overallProgressReporter, StartProgressPercentage, (StartProgressPercentage - 1));
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ConstructorEndProgressPercentageMoreThanOneHundred()
		{
			new SubProgressReporter(_overallProgressReporter, StartProgressPercentage, 101);
		}
		#endregion Tests for constructor arguments

		#region Tests for CancellationPending property
		[Test]
		public void CancellationPendingTrue()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.CancellationPending).Return(true);
			}).Verify(delegate
			{
				Assert.IsTrue(_progressReporter.CancellationPending);
			});
		}

		[Test]
		public void CancellationPendingFalse()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.CancellationPending).Return(false);
			}).Verify(delegate
			{
				Assert.IsFalse(_progressReporter.CancellationPending);
			});
		}
		#endregion Tests for CancellationPending property

		#region Tests for ReportProgress(int) method
		[Test]
		public void ReportProgress()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReportProgressNegative()
		{
			_progressReporter.ReportProgress(-1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReportProgressMoreThanOneHundred()
		{
			_progressReporter.ReportProgress(101);
		}

		[Test]
		public void ReportProgressInOrder()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30));
				_overallProgressReporter.Expect(x => x.ReportProgress(35));
				_overallProgressReporter.Expect(x => x.ReportProgress(40));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20);
				_progressReporter.ReportProgress(30);
				_progressReporter.ReportProgress(40);
			});
		}

		[Test]
		public void ReportProgressZero()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(StartProgressPercentage));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(0);
			});
		}

		[Test]
		public void ReportProgressOneHundred()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(EndProgressPercentage));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(100);
			});
		}

		[Test]
		public void ReportProgressSuppressedForSameOverallProgress()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20);
				_progressReporter.ReportProgress(21);
			});
		}

		[Test]
		public void ReportProgressAfterUserStateNotSuppressed()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30));
				_overallProgressReporter.Expect(x => x.ReportProgress(30, _userState1));
				_overallProgressReporter.Expect(x => x.ReportProgress(30));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20);
				_progressReporter.ReportProgress(20, _userState1);
				_progressReporter.ReportProgress(20);
			});
		}
		#endregion Tests for ReportProgress(int) method

		#region Tests for ReportProgress(int, Object) method
		[Test]
		public void ReportProgressWithUserState()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30, _userState1));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20, _userState1);
			});
		}

		[Test]
		public void ReportProgressUserStateNull()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30, null));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20, null);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReportProgressWithUserStateNegative()
		{
			_progressReporter.ReportProgress(-1, _userState1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ReportProgressWithUserStateMoreThanOneHundred()
		{
			_progressReporter.ReportProgress(101, _userState1);
		}

		[Test]
		public void ReportProgressWithUserStateInOrder()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30, _userState1));
				_overallProgressReporter.Expect(x => x.ReportProgress(35, _userState2));
				_overallProgressReporter.Expect(x => x.ReportProgress(40, _userState3));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20, _userState1);
				_progressReporter.ReportProgress(30, _userState2);
				_progressReporter.ReportProgress(40, _userState3);
			});
		}

		[Test]
		public void ReportProgressWithUserStateZero()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(StartProgressPercentage, _userState1));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(0, _userState1);
			});
		}

		[Test]
		public void ReportProgressWithUserStateOneHundred()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(EndProgressPercentage, _userState1));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(100, _userState1);
			});
		}

		[Test]
		public void ReportProgressWithUserStateNotSuppressed()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_overallProgressReporter.Expect(x => x.ReportProgress(30, _userState1));
				_overallProgressReporter.Expect(x => x.ReportProgress(30, _userState1));
			}).Verify(delegate
			{
				_progressReporter.ReportProgress(20, _userState1);
				_progressReporter.ReportProgress(21, _userState1);
			});
		}
		#endregion Tests for ReportProgress(int, Object) method
	}
}
