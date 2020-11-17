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

using System.Diagnostics;
using System.Linq;
using Defraser.Detector.Asf;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestAsfObjectIsSuitableParent
	{
		#region Mocks and stubs
		private MockRepository _mockRepository;
		private AsfObject _parentAsfObject;
		private IDetector _detector;
		private AsfObject _childAsfObject;
		#endregion Mocks and stubs

		#region Objects under test
		private AsfObject _asfObject;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			// The string "DynamicProxyGenAssembly2" is used in AsfDetector class to grant Rhino Mocks
			// access to its internals.
			Debug.Assert(RhinoMocks.NormalName == "DynamicProxyGenAssembly2");

			_mockRepository = new MockRepository();

			// Mocks and stubs
			_detector = MockRepository.GenerateStub<IDetector>();
			//_resultNode = MockRepository.GenerateStub<IResultNode>();
			_childAsfObject = MockRepository.GenerateStub<AsfObject>(Enumerable.Repeat(_detector, 1));
			_parentAsfObject = _mockRepository.PartialMock<AsfObject>(Enumerable.Repeat(_detector, 1));

			_asfObject = _mockRepository.PartialMock<AsfObject>(Enumerable.Repeat(_detector, 1));
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_parentAsfObject = null;
			_detector = null;
			_asfObject = null;
			_childAsfObject = null;
		}
		#endregion Test initialization and cleanup

		[Test]
		public void BetweenParentOffsetBondries()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.Offset).Return(0L);
				_parentAsfObject.Stub(x => x.RelativeEndOffset).Return(200L);
				_asfObject.Stub(x => x.Offset).Return(10L);
				_asfObject.Stub(x => x.RelativeEndOffset).Return(100L);
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void BetweenParentOffsetBondriesFailsBecauseParentIsNotRoot()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Offset).Return(10L);
				_parentAsfObject.Stub(x => x.RelativeEndOffset).Return(100L);
				_asfObject.Stub(x => x.Offset).Return(0L);
				_asfObject.Stub(x => x.RelativeEndOffset).Return(200L);
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void BetweenParentOffsetBondriesFailsBecauseOffsetSmaller()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.Offset).Return(11L);
				_parentAsfObject.Stub(x => x.RelativeEndOffset).Return(200L);
				_asfObject.Stub(x => x.Offset).Return(10L);
				_asfObject.Stub(x => x.RelativeEndOffset).Return(100L);
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Expect(x => x.IsUnknown).Return(false).Repeat.Never();
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void BetweenParentOffsetBondriesFailsBecauseEndOffsetGreater()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.Offset).Return(0L);
				_parentAsfObject.Stub(x => x.RelativeEndOffset).Return(200L);
				_asfObject.Stub(x => x.Offset).Return(10L);
				_asfObject.Stub(x => x.RelativeEndOffset).Return(201L);
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Expect(x => x.IsUnknown).Return(false).Repeat.Never();
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void FirstAsfObjectShouldNotBeUnknown()
		{
			// ASF-object is unknown and parent is root and parent had no children
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Empty<IResultNode>().ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(true);
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void FirstAsfObjectShouldNotBeUnknownFailsBecauseIsKnown()
		{
			// ASF-object is known and parent is root and parent has no children
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Empty<IResultNode>().ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void FirstAsfObjectShouldNotBeUnknownFailsBecauseParentIsNotRoot()
		{
			// ASF-object is unknown and parent is not root and parent has no children
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Empty<IResultNode>().ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(true);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void FirstAsfObjectShouldNotBeUnknownFailsBecauseIsNotFirst()
		{
			// ASF-object is known and parent is root and already has children
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.DataObject);
				_asfObject.Stub(x => x.IsUnknown).Return(true);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void ShouldHaveNoDuplicatesInParentIfNoDuplicatesAllowsFlag()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		/// <summary>
		/// Duplicate objects are allowed in the root although they do not have
		/// a DuplicatesAllowed flag set when the objects are not top level object
		/// </summary>
		[Test]
		public void DuplicateNonTopLevelObjectsAreAllowedInRoot()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.FilePropertiesObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.FilePropertiesObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void IsSuitableParentForParentIsRoot()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.DataObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void IsSuitableParentForParentIsNotRoot()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.ScriptCommandObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.FilePropertiesObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsTrue(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void NoSuitableParentFound()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_parentAsfObject.Stub(x => x.IsRoot).Return(false);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Empty<IResultNode>().ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.FilePropertiesObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}

		[Test]
		public void StartNewResultWhenHeaderObjectIsFound()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_childAsfObject.Stub(x => x.HeaderName).Return(AsfObjectName.DataObject);
				_parentAsfObject.Stub(x => x.IsRoot).Return(true);
				_parentAsfObject.Stub(x => x.Children).Return(Enumerable.Repeat(_childAsfObject as IResultNode, 1).ToList());
				_asfObject.Stub(x => x.HeaderName).Return(AsfObjectName.HeaderObject);
				_asfObject.Stub(x => x.IsUnknown).Return(false);
			}).Verify(delegate
			{
				Assert.IsFalse(_asfObject.IsSuitableParent(_parentAsfObject));
			});
		}
	}
}
