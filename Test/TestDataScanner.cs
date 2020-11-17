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
using System.ComponentModel;
using System.Linq;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="DataScanner"/> class.
	/// </summary>
	[TestFixture]
	public class TestDataScanner
	{
		#region Inner (test) classes
		public class DataScannerTester : DataScanner
		{
			public DataScannerTester(Creator<IDataBlockBuilder> createDataBlockBuilder, Creator<IDataReader, IDataReader, IProgressReporter> createProgressDataReader, Creator<IScanContext, IProject> createScanContext)
				: base(createDataBlockBuilder, createProgressDataReader, createScanContext)
			{
			}

			protected override void OnDataBlockDetected(DataBlockDetectedEventArgs e)
			{
				TestOnDataBlockDetected(e);
			}

			public virtual void TestOnDataBlockDetected(DataBlockDetectedEventArgs e)
			{
				base.OnDataBlockDetected(e);
			}

			protected override void OnDataBlockDiscarded(SingleValueEventArgs<IDataBlock> e)
			{
				TestOnDataBlockDiscarded(e);
			}

			public virtual void TestOnDataBlockDiscarded(SingleValueEventArgs<IDataBlock> e)
			{
				base.OnDataBlockDiscarded(e);
			}

			protected override void OnUnknownDataDetected(UnknownDataDetectedEventArgs e)
			{
				TestOnUnknownDataDetected(e);
			}

			public virtual void TestOnUnknownDataDetected(UnknownDataDetectedEventArgs e)
			{
				base.OnUnknownDataDetected(e);
			}
		}
		#endregion Inner (test) classes

		#region Test data
		/// <summary><c>Length</c> of <see cref="IDataReader"/> (data) to scan.</summary>
		private const long DataReaderLength = 200L;
		/// <summary><c>Position</c> of the data reader after unknown data.</summary>
		private const long NoDataBlockDetectedPosition = 20L;
		/// <summary><c>StartOffset</c> of first non-overlapping data block.</summary>
		private const long NonOverlappingDataBlockStartOffset1 = 0L;
		/// <summary><c>EndOffset</c> of first non-overlapping data block.</summary>
		private const long NonOverlappingDataBlockEndOffset1 = 25L;
		/// <summary><c>StartOffset</c> of second non-overlapping data block.</summary>
		private const long NonOverlappingDataBlockStartOffset2 = 25L;
		/// <summary><c>EndOffset</c> of second non-overlapping data block.</summary>
		private const long NonOverlappingDataBlockEndOffset2 = 50L;
		/// <summary><c>StartOffset</c> of third droppped data block.</summary>
		private const long NonOverlappingDataBlockStartOffset3 = 50L;
		/// <summary><c>EndOffset</c> of third dropped data block.</summary>
		private const long NonOverlappingDataBlockEndOffset3 = DataReaderLength;
		/// <summary><c>StartOffset</c> of data block partially overlapping with data blocks 1 and 2.</summary>
		private const long PartiallyOverlappingDataBlockStartOffset = 10L;
		/// <summary><c>EndOffset</c> of data block partially overlapping with data blocks 1 and 2.</summary>
		private const long PartiallyOverlappingDataBlockEndOffset = 40L;
		#endregion Test data

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IInputFile _inputFile;
		private IDataPacket _dataPacket;
		private IDataReader _dataReader;
		private IDetector _detector1;
		private IDetector _detector2;
		private IDataBlock _nonOverlappingDataBlock1;
		private IDataBlock _nonOverlappingDataBlock2;
		private IDataBlock _nonOverlappingDataBlock3;
		private IDataBlock _partiallyOverlappingDataBlock;
		private IDataBlock _entireStreamDataBlock;
		private IProgressReporter _mockProgressReporter;
		private IProgressReporter _dummyProgressReporter;
		private ProgressChangedEventHandler _progressChangedEventHandler;
		private EventHandler<DataBlockDetectedEventArgs> _dataBlockDetectedEventHandler;
		private EventHandler<SingleValueEventArgs<IDataBlock>> _dataBlockDiscardedEventHandler;
		private EventHandler<UnknownDataDetectedEventArgs> _unknownDataDetectedEventHandler;
		private Creator<IDataBlockBuilder> _createDataBlockBuilder;
		private Creator<IDataReader, IDataReader, IProgressReporter> _createProgressDataReader;
		private Creator<IScanContext, IProject> _createScanContext;
		private DataScannerTester _dataScannerTester;
		#endregion Mocks and stubs

		#region Objects under test
		private IDataScanner _dataScanner;
		private IProgressReporter _progressReporter;
		//private bool _cancellationPending;
		#endregion Objects under test

		#region Properties
		private IEnumerable<IDetector> OneDetector { get { return Enumerable.Repeat<IDetector>(_detector1, 1); } }
		private IEnumerable<IDetector> TwoDetectors { get { return new List<IDetector> { _detector1, _detector2 }; } }
		#endregion Properties

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_inputFile = MockRepository.GenerateStub<IInputFile>();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_dataReader = _mockRepository.StrictMock<IDataReader>();
			_detector1 = _mockRepository.StrictMock<IDetector>();
			_detector2 = _mockRepository.StrictMock<IDetector>();
			_nonOverlappingDataBlock1 = CreateDataBlockStub(NonOverlappingDataBlockStartOffset1, NonOverlappingDataBlockEndOffset1);
			_nonOverlappingDataBlock2 = CreateDataBlockStub(NonOverlappingDataBlockStartOffset2, NonOverlappingDataBlockEndOffset2);
			_nonOverlappingDataBlock3 = CreateDataBlockStub(NonOverlappingDataBlockStartOffset3, NonOverlappingDataBlockEndOffset3);
			_partiallyOverlappingDataBlock = CreateDataBlockStub(PartiallyOverlappingDataBlockStartOffset, PartiallyOverlappingDataBlockEndOffset);
			_entireStreamDataBlock = CreateDataBlockStub(0L, DataReaderLength);
			_mockProgressReporter = _mockRepository.StrictMock<IProgressReporter>();
			_dummyProgressReporter = MockRepository.GenerateStub<IProgressReporter>();
			_progressChangedEventHandler = _mockRepository.StrictMock<ProgressChangedEventHandler>();
			_dataBlockDetectedEventHandler = _mockRepository.StrictMock<EventHandler<DataBlockDetectedEventArgs>>();
			_dataBlockDiscardedEventHandler = _mockRepository.StrictMock<EventHandler<SingleValueEventArgs<IDataBlock>>>();
			_unknownDataDetectedEventHandler = _mockRepository.StrictMock<EventHandler<UnknownDataDetectedEventArgs>>();
			_createDataBlockBuilder = MockRepository.GenerateStub<Creator<IDataBlockBuilder>>();
			_createProgressDataReader = MockRepository.GenerateStub<Creator<IDataReader, IDataReader, IProgressReporter>>();
			_createScanContext = MockRepository.GenerateStub<Creator<IScanContext, IProject>>();
			_dataScannerTester = _mockRepository.PartialMock<DataScannerTester>(_createDataBlockBuilder, _createProgressDataReader, _createScanContext);

			_dataScanner = new DataScanner(_createDataBlockBuilder, _createProgressDataReader, _createScanContext);
			_progressReporter = null;

			// TODO: state of _dataReader is state of _progressDataReader

			_inputFile.Stub(x => x.Length).Return(DataReaderLength);
			_inputFile.Stub(x => x.Name).Return(string.Empty);

			_dataPacket.Stub(x => x.InputFile).Return(_inputFile);
			_dataReader.Stub(x => x.Length).Return(DataReaderLength);
			_mockProgressReporter.Stub(x => x.CancellationPending).Return(false).Repeat.Any();

			_createDataBlockBuilder.Stub(x => x())
					.Return(null)
					.WhenCalled(i => i.ReturnValue = MockRepository.GenerateStub<IDataBlockBuilder>());
			_createProgressDataReader.Stub(c => c(Arg<IDataReader>.Is.Same(_dataReader), Arg<IProgressReporter>.Is.NotNull))
					.Return(null)
					.WhenCalled(i =>
					{
						_progressReporter = i.Arguments[1] as IProgressReporter;
						i.ReturnValue = CreateProgressDataReaderStub(_progressReporter);
					})
					.Repeat.Once();
			_createScanContext.Stub(x => x(Arg<IProject>.Is.Anything))
					.Return(null)
					.WhenCalled(i => i.ReturnValue = MockRepository.GenerateStub<IScanContext>());
		}

		[TearDown]
		public void TearDown()
		{
			_mockRepository = null;
			_inputFile = null;
			_dataPacket = null;
			_dataReader = null;
			_detector1 = null;
			_detector2 = null;
			_nonOverlappingDataBlock1 = null;
			_nonOverlappingDataBlock2 = null;
			_nonOverlappingDataBlock3 = null;
			_partiallyOverlappingDataBlock = null;
			_entireStreamDataBlock = null;
			_mockProgressReporter = null;
			_dummyProgressReporter = null;
			_progressChangedEventHandler = null;
			_dataBlockDetectedEventHandler = null;
			_dataBlockDiscardedEventHandler = null;
			_unknownDataDetectedEventHandler = null;
			_createDataBlockBuilder = null;
			_createProgressDataReader = null;
			_createScanContext = null;
			_dataScannerTester = null;
			_dataScanner = null;
			_progressReporter = null;
		}
		#endregion Test initialization and cleanup

		#region Tests for constructor arguments
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorDataBlockBuilderCreatorNull()
		{
			new DataScanner(null, _createProgressDataReader, _createScanContext);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorProgressDataReaderCreatorNull()
		{
			new DataScanner(_createDataBlockBuilder, null, _createScanContext);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorScanContextCreatorNull()
		{
			new DataScanner(_createDataBlockBuilder, _createProgressDataReader, null);
		}
		#endregion Tests for constructor arguments

		#region Tests for Detectors property
		[Test]
		public void Detectors()
		{
			Assert.AreEqual(0, _dataScanner.Detectors.Count(), "Detectors is empty by default");
		}

		[Test]
		public void DetectorsSetToOneDetector()
		{
			_dataScanner.Detectors = OneDetector;
			Assert.IsTrue(_dataScanner.Detectors.SequenceEqual(OneDetector), "Detectors property should store detectors");
		}

		[Test]
		public void DetectorsSetToTwoDetectors()
		{
			_dataScanner.Detectors = TwoDetectors;
			Assert.IsTrue(_dataScanner.Detectors.SequenceEqual(TwoDetectors), "Detectors property should preserve order");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DetectorsSetToNull()
		{
			_dataScanner.Detectors = null;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DetectorsSetToCollectionContainingNull()
		{
			_dataScanner.Detectors = new IDetector[] { null };
		}
		#endregion Tests for Detectors property

		#region Tests for CancellationPending property
		[Test]
		public void CancellationPendingCancelsScan()
		{
			bool cancellationPending = false;
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dummyProgressReporter.Stub(x => x.CancellationPending).Return(false)
						.WhenCalled(i => i.ReturnValue = cancellationPending);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1)
						.WhenCalled(i => cancellationPending = true);
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test(Description="Test for ProgressDataReader that does not work properly")]
		public void ProgressDataReaderReadyButCancellationPendingCancelsScan()
		{
			var createProgressDataReader = MockRepository.GenerateStub<Creator<IDataReader, IDataReader, IProgressReporter>>();

			createProgressDataReader.Stub(c => c(Arg<IDataReader>.Is.Same(_dataReader), Arg<IProgressReporter>.Is.NotNull))
					.Return(null)
					.WhenCalled(i => i.ReturnValue = CreateProgressDataReaderStub(MockRepository.GenerateStub<IProgressReporter>()))
					.Repeat.Once();

			bool cancellationPending = false;
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dummyProgressReporter.Stub(x => x.CancellationPending).Return(false)
						.WhenCalled(i => i.ReturnValue = cancellationPending);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1)
						.WhenCalled(i => cancellationPending = true);
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void CancellationPendingSuppressesDataBlock()
		{
			bool cancellationPending = false;
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dummyProgressReporter.Stub(x => x.CancellationPending).Return(false).WhenCalled(i => i.ReturnValue = cancellationPending);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3).WhenCalled(i => cancellationPending = true);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void CancellationPendingSuppressesUnknownDataAtEnd()
		{
			bool cancellationPending = false;
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dummyProgressReporter.Stub(x => x.CancellationPending).Return(false).WhenCalled(i => i.ReturnValue = cancellationPending);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(i => cancellationPending = true);
			}).Verify(delegate
			{
				_dataScanner.UnknownDataDetected += _unknownDataDetectedEventHandler;
				ScanWithDetectors(OneDetector);
			});
		}
		#endregion Tests for CancellationPending property

		#region Tests for Scan() method
		[Test]
		public void Scan()
		{			
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectUnknownDataDetectedEvent(0L, NonOverlappingDataBlockStartOffset2);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock2);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(NonOverlappingDataBlockEndOffset2, (DataReaderLength - NonOverlappingDataBlockEndOffset2));
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ScanDataReaderNull()
		{
			_dataScanner.Scan(null, _dummyProgressReporter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ScanProgressReporterNull()
		{
			_dataScanner.Scan(_dataReader, null);
		}

		[Test(Description = "Scan with no detectors should report the entire stream as unknown data")]
		public void ScanNoDetectors()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectUnknownDataDetectedEvent(0L, DataReaderLength);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(Enumerable.Empty<IDetector>());
			});
		}

		[Test]
		public void ScanNonOverlappingDataBlocks()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock2);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock1);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock2);
				ExpectDetectNoDataBlock(_detector2, DataReaderLength);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void ScanPartiallyOverlappingDataBlocks()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectDetectNextDataBlock(_detector2, _partiallyOverlappingDataBlock);
				ExpectDataBlockDiscardedEvent(_nonOverlappingDataBlock2);
				ExpectUnknownDataDetectedEvent(0L, PartiallyOverlappingDataBlockStartOffset);
				ExpectDataBlockDetectedEvent(_partiallyOverlappingDataBlock);
				ExpectDetectNoDataBlock(_detector2, DataReaderLength);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(PartiallyOverlappingDataBlockEndOffset, (DataReaderLength - PartiallyOverlappingDataBlockEndOffset));
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(TwoDetectors);
			});
		}
		#endregion Tests for Scan() method

		#region Tests for DataBlockBuilder passed to DetectData() method
		[Test]
		public void DataBlockBuilderDetectorOneDetector()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.NotNull(), Is.Matching<IScanContext>(c => c.Detectors.First() == _detector1));
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockBuilderDetectorTwoDetectors()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.NotNull(), Is.Matching<IScanContext>(c => c.Detectors.First() == _detector1));
				ExpectDetectNextDataBlock(_detector2, _entireStreamDataBlock).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.NotNull(), Is.Matching<IScanContext>(c => c.Detectors.First() == _detector2));
			}).Verify(delegate
			{
				ScanWithDetectors(TwoDetectors);
			});
		}
		#endregion Tests for DataBlockBuilder passed to DetectData() method

		#region Tests for DataBlockBuilder passed to DetectData() method, value of PreviousFragment property
		[Test]
		public void DataBlockBuilderPreviousFragmentNull()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == null), Is.NotNull());
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockBuilderPreviousFragmentOneDetector()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == _nonOverlappingDataBlock2), Is.NotNull());
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockBuilderPreviousFragmentTwoDetectors()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == _nonOverlappingDataBlock1), Is.NotNull());
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
			}).Verify(delegate
			{
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void DataBlockBuilderPreviousFragmentDropped()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock2);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == null), Is.NotNull());
			}).Verify(delegate
			{
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void DataBlockBuilderPreviousFragmentDiscardedLargerBlockDetected()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectDetectNextDataBlock(_detector2, _entireStreamDataBlock);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == null), Is.NotNull());
			}).Verify(delegate
			{
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void DataBlockBuilderPreviousFragmentNullAfterUnknownData()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDetectNoDataBlock(_detector1, NoDataBlockDetectedPosition);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3).IgnoreArguments()
						.Constraints(Is.NotNull(), Is.Matching<IDataBlockBuilder>(d => GetPreviousFragment(d) == null), Is.NotNull());
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}
		#endregion Tests for value of PreviousFragment property of DataBlockBuilder passed to DetectData() method

		#region Tests for DataBlockDetected event
		[Test]
		public void DataBlockDetected()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectDataBlockDetectedEvent(_entireStreamDataBlock);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockDetectedAfterUnknownData()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNoDataBlock(_detector1, NoDataBlockDetectedPosition);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockDetectedOrderedByStartOffsetOneDetector()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock1);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock2);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void DataBlockDetectedOrderedByStartOffsetTwoDetectorsInOrder()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock2);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock1);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock2);
				ExpectDetectNoDataBlock(_detector2, DataReaderLength);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void DataBlockDetectedOrderedByStartOffsetTwoDetectorsOutOfOrder()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock1);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock1);
				ExpectDetectNoDataBlock(_detector2, DataReaderLength);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock2);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void OnDataBlockDetectedRaisesEvent()
		{
			var e = new DataBlockDetectedEventArgs(_entireStreamDataBlock);
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_dataBlockDetectedEventHandler.Expect(x => x(_dataScannerTester, e));
			}).Verify(delegate
			{
				_dataScannerTester.DataBlockDetected += _dataBlockDetectedEventHandler;
				_dataScannerTester.TestOnDataBlockDetected(e);
			});
		}

		[Test]
		public void OnDataBlockDetectedUsedForRaisingEvents()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				_dataScannerTester.Expect(x => x.TestOnDataBlockDetected(new DataBlockDetectedEventArgs(_entireStreamDataBlock)));
			}).Verify(delegate
			{
				_dataScannerTester.Detectors = OneDetector;
				_dataScannerTester.Scan(_dataReader, _dummyProgressReporter);
			});
		}
		#endregion Tests for DataBlockDetected event

		#region Tests for UnknownDataDetected event
		[Test]
		public void UnknownDataDetected()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(0L, DataReaderLength);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void UnknownDataDetectedBeforeDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectUnknownDataDetectedEvent(0L, NonOverlappingDataBlockStartOffset3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void UnknownDataDetectedAfterDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock1);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(NonOverlappingDataBlockEndOffset1, (DataReaderLength - NonOverlappingDataBlockEndOffset1));
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void UnknownDataDetectedExtendedTwiceUptoEndOfInput()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, NonOverlappingDataBlockEndOffset1);
				ExpectDetectNoDataBlock(_detector1, NonOverlappingDataBlockEndOffset2);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(0L, DataReaderLength);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void UnknownDataDetectedExtendedUptoDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, NoDataBlockDetectedPosition);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectUnknownDataDetectedEvent(0L, NonOverlappingDataBlockStartOffset3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void UnknownDataDetectedTwoDetectorsExtendedUptoDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, NoDataBlockDetectedPosition);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectUnknownDataDetectedEvent(0L, NonOverlappingDataBlockStartOffset3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void UnknownDataDetectedTwoDetectorsTruncatedToDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				ExpectUnknownDataDetectedEvent(0L, NonOverlappingDataBlockStartOffset3);
				ExpectDataBlockDetectedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void OnUnknownDataDetected()
		{
			UnknownDataDetectedEventArgs e = new UnknownDataDetectedEventArgs(NonOverlappingDataBlockStartOffset3, NonOverlappingDataBlockEndOffset3);
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_unknownDataDetectedEventHandler.Expect(x => x(_dataScannerTester, e));
			}).Verify(delegate
			{
				_dataScannerTester.UnknownDataDetected += _unknownDataDetectedEventHandler;
				_dataScannerTester.TestOnUnknownDataDetected(e);
			});
		}

		[Test]
		public void OnUnknownDataDetectedUptoNextDataBlockUsedForRaisingEvents()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				_dataScannerTester.Expect(x => x.TestOnUnknownDataDetected(new UnknownDataDetectedEventArgs(0L, NonOverlappingDataBlockStartOffset3)));
			}).Verify(delegate
			{
				_dataScannerTester.Detectors = OneDetector;
				_dataScannerTester.Scan(_dataReader, _dummyProgressReporter);
			});
		}

		[Test]
		public void OnUnknownDataDetectedUptoEndOfInputUsedForRaisingEvents()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				_dataScannerTester.Expect(x => x.TestOnUnknownDataDetected(new UnknownDataDetectedEventArgs(0L, DataReaderLength)));
			}).Verify(delegate
			{
				_dataScannerTester.Detectors = OneDetector;
				_dataScannerTester.Scan(_dataReader, _dummyProgressReporter);
			});
		}
		#endregion Unit tests for UnknownDataDetected event

		#region Tests for DataBlockDiscarded event. Note: Ordering of discarded data blocks is not important!
		[Test]
		public void DataBlockDiscardedDropped()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				ExpectDataBlockDiscardedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void DataBlockDiscardedLargerBlockDetected()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectDetectNextDataBlock(_detector2, _entireStreamDataBlock);
				ExpectDataBlockDiscardedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		// Discarding a fragment of a data block may split a data block in two, which is not intended behavior!

		[Test]
		public void FragmentedDataBlockDiscardedDropped()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				IFragmentContainer fragmentContainer = _mockRepository.DynamicMock<IFragmentContainer>();
				_nonOverlappingDataBlock3.FragmentContainer = fragmentContainer;

				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				fragmentContainer.Expect(x => x.Remove(_nonOverlappingDataBlock3));
				ExpectDataBlockDiscardedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void FragmentedDataBlockDiscardedLargerBlockDetected()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				IFragmentContainer fragmentContainer = _mockRepository.DynamicMock<IFragmentContainer>();
				_nonOverlappingDataBlock3.FragmentContainer = fragmentContainer;

				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectDetectNextDataBlock(_detector2, _entireStreamDataBlock);
				fragmentContainer.Expect(x => x.Remove(_nonOverlappingDataBlock3));
				ExpectDataBlockDiscardedEvent(_nonOverlappingDataBlock3);
			}).Verify(delegate
			{
				_dataScanner.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
				ScanWithDetectors(TwoDetectors);
			});
		}

		[Test]
		public void OnDataBlockDiscarded()
		{
			var e = new SingleValueEventArgs<IDataBlock>(_entireStreamDataBlock);
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				_dataBlockDiscardedEventHandler.Expect(x => x(_dataScannerTester, e));
			}).Verify(delegate
			{
				_dataScannerTester.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
				_dataScannerTester.TestOnDataBlockDiscarded(e);
			});
		}

		[Test]
		public void OnDataBlockDiscardedDroppedUsedForRaisingEvents()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock3);
				_dataScannerTester.Expect(x => x.TestOnDataBlockDiscarded(new SingleValueEventArgs<IDataBlock>(_nonOverlappingDataBlock3)));
			}).Verify(delegate
			{
				_dataScannerTester.Detectors = TwoDetectors;
				_dataScannerTester.Scan(_dataReader, _dummyProgressReporter);
			});
		}

		[Test]
		public void OnDataBlockDiscardedLargerBlockDetectedUsedForRaisingEvents()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectDetectNextDataBlock(_detector2, _entireStreamDataBlock);
				_dataScannerTester.Expect(x => x.TestOnDataBlockDiscarded(new SingleValueEventArgs<IDataBlock>(_nonOverlappingDataBlock3)));
			}).Verify(delegate
			{
				_dataScannerTester.Detectors = TwoDetectors;
				_dataScannerTester.Scan(_dataReader, _dummyProgressReporter);
			});
		}
		#endregion Tests for DataBlockDiscarded event. Note: Ordering of discarded data blocks is not important!

		// TODO: this tests IProgressReporter.ReportProgress(int) passed to Scan()
		#region Tests for ProgressChanged event
		[Test]
		public void ProgressChangedAtBeginAndEndOfScan()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedSuppressedAtEndOfScan()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(100));
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedAfterDetectedDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock1);
				ExpectProgressChangedEvent(12);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedSuppressedUnknownDataBeforeDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectProgressChangedEvent(25);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedBetweenUnknownDataAndDataBlock()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, NonOverlappingDataBlockStartOffset3);
				ExpectProgressChangedEvent(25);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock3);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedBetweenUnknownData()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, 50L);
				ExpectProgressChangedEvent(25);
				ExpectDetectNoDataBlock(_detector1, 100L);
				ExpectProgressChangedEvent(50);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedRaisedBeforeDataBlockDetectedEvent()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
				ExpectProgressChangedEvent(100);
				ExpectDataBlockDetectedEvent(_entireStreamDataBlock);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedRaisedBeforeUnknownDataDetectedEvent()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(100);
				ExpectUnknownDataDetectedEvent(0L, DataReaderLength);
			}).Verify(delegate
			{
				SubscribeToReportDataEvents();
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressChangedTwoDetectors()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNextDataBlock(_detector1, _partiallyOverlappingDataBlock);
				ExpectProgressChangedEvent(10);		// First detector at 20%, second detector at 0%
				ExpectDetectNextDataBlock(_detector2, _nonOverlappingDataBlock2);
				ExpectProgressChangedEvent(22);		// First detector at 20%, second detector at 25%
				ExpectDetectNoDataBlock(_detector1, DataReaderLength);
				ExpectProgressChangedEvent(62);		// First detector at 100%, second detector at 25%
				ExpectDetectNoDataBlock(_detector2, DataReaderLength);
				ExpectProgressChangedEvent(100);	// Both detectors at 100%
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(TwoDetectors);
			});
		}
		#endregion Tests for ProgressChanged event

		// TODO
		#region Tests for ProgressReporter: Reports progress during detection of a single data block.
		[Test]
		public void ProgressReporterCancellationPendingFalse()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				_dummyProgressReporter.Stub(x => x.CancellationPending).Return(false);
				_createProgressDataReader.Stub(c => c(Arg<IDataReader>.Is.Same(_dataReader), Arg<IProgressReporter>.Is.NotNull))
						.Return(null)
						.WhenCalled(i =>
						{
							IProgressReporter progressReporter = i.Arguments[1] as IProgressReporter;
							Assert.IsFalse(progressReporter.CancellationPending);
							i.ReturnValue = CreateProgressDataReaderStub(progressReporter);
						});
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock);
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void ProgressReporterCancellationPendingTrue()
		{
			With.Mocks(_mockRepository).Expecting(delegate
			{
				ExpectDetectNextDataBlock(_detector1, _entireStreamDataBlock)
						.WhenCalled(invocation => 
						{
							_dummyProgressReporter.Stub(x => x.CancellationPending).Return(true);
							Assert.IsTrue(_progressReporter.CancellationPending);
						});
			}).Verify(delegate
			{
				ScanWithDetectors(OneDetector);
			});
		}

		[Test]
		public void ProgressReporterReportProgressOneDetector()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(50));
				ExpectProgressChangedEvent(50);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressReporterReportProgressTwoDetectors()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(50));
				ExpectProgressChangedEvent(25);
				ExpectProgressChangedEvent(50);
				ExpectDetectNoDataBlock(_detector2, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(50));
				ExpectProgressChangedEvent(75);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(TwoDetectors);
			});
		}

		[Test]
		public void ProgressReporterSuppressReportProgressAtBegin()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(0));
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressReporterSuppressReportProgressCurrentPosition()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNextDataBlock(_detector1, _nonOverlappingDataBlock2);
				ExpectProgressChangedEvent(25);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation => _progressReporter.ReportProgress(25));
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}

		[Test]
		public void ProgressReporterSuppressReportProgressRepeatedPercentage()
		{
			With.Mocks(_mockRepository).ExpectingInSameOrder(delegate
			{
				ExpectProgressChangedEvent(0);
				ExpectDetectNoDataBlock(_detector1, DataReaderLength).WhenCalled(invocation =>
				{
					_progressReporter.ReportProgress(25);
					_progressReporter.ReportProgress(25);
				});
				ExpectProgressChangedEvent(25);
				ExpectProgressChangedEvent(100);
			}).Verify(delegate
			{
				ScanWithDetectorsAndProgress(OneDetector);
			});
		}
		#endregion Tests for ProgressReporter: Reports progress during detection of a single data block.

		#region Setup and expectation helpers
		private IDataBlock CreateDataBlockStub(long startOffset, long endOffset)
		{
			IDataBlock dataBlock = MockRepository.GenerateStub<IDataBlock>();
			dataBlock.Stub(x => x.InputFile).Return(_inputFile);
			dataBlock.Stub(x => x.Length).Return(endOffset - startOffset);
			dataBlock.Stub(x => x.StartOffset).Return(startOffset);
			dataBlock.Stub(x => x.EndOffset).Return(endOffset);
			return dataBlock;
		}

		private IDataReader CreateProgressDataReaderStub(IProgressReporter progressReporter)
		{
			IDataReader progressDataReader = MockRepository.GenerateStub<IDataReader>();
			progressDataReader.Stub(x => x.Length).Return(DataReaderLength);
			progressDataReader.Stub(x => x.GetDataPacket(Arg<long>.Is.Anything, Arg<long>.Is.Anything)).Return(_dataPacket);
			progressDataReader.Stub(x => x.State).Return(default(DataReaderState))
					.WhenCalled(i => i.ReturnValue = GetProgressDataReaderState(progressDataReader.Position, progressReporter.CancellationPending));
			return progressDataReader;
		}

		private static DataReaderState GetProgressDataReaderState(long position, bool cancellationPending)
		{
			if (cancellationPending) return DataReaderState.Cancelled;
			if (position >= DataReaderLength) return DataReaderState.EndOfInput;
			return DataReaderState.Ready;
		}

		private static IMethodOptions<IDataBlock> ExpectDetectNoDataBlock(IDetector detector, long endPosition)
		{
			return detector.Expect(x => x.DetectData(Arg<IDataReader>.Is.NotNull, Arg<IDataBlockBuilder>.Is.NotNull, Arg<IScanContext>.Is.NotNull)).Return(null)
					.WhenCalled(i => (i.Arguments[0] as IDataReader).Position = endPosition);
		}

		private static IMethodOptions<IDataBlock> ExpectDetectNextDataBlock(IDetector detector, IDataBlock result)
		{
			return detector.Expect(x => x.DetectData(Arg<IDataReader>.Is.NotNull, Arg<IDataBlockBuilder>.Is.NotNull, Arg<IScanContext>.Is.NotNull)).Return(result)
					.WhenCalled(i => (i.Arguments[0] as IDataReader).Position = result.EndOffset);
		}

		private static IDataBlock GetPreviousFragment(IDataBlockBuilder dataBlockBuilder)
		{
			return GetWriteOnlyPropertyValue<IDataBlockBuilder, IDataBlock>(dataBlockBuilder, b => b.PreviousFragment = Arg<IDataBlock>.Is.Anything);
		}

		private static TResult GetWriteOnlyPropertyValue<T, TResult>(T mock, Action<T> action)
		{
			IList<object[]> arguments = mock.GetArgumentsForCallsMadeOn(action);
			if (arguments.Count() == 0)
			{
				return default(TResult);
			}
			object[] args = arguments.LastOrDefault();
			return (TResult)args[0];
		}

		private void ExpectProgressChangedEvent(int progressPercentage)
		{
			_mockProgressReporter.Expect(x => x.ReportProgress(progressPercentage));
		}

		private void ExpectDataBlockDetectedEvent(IDataBlock dataBlock)
		{
			_dataBlockDetectedEventHandler.Expect(e => e(_dataScanner, new DataBlockDetectedEventArgs(dataBlock)));
		}

		private void ExpectDataBlockDiscardedEvent(IDataBlock dataBlock)
		{
			_dataBlockDiscardedEventHandler.Expect(e => e(_dataScanner, new SingleValueEventArgs<IDataBlock>(dataBlock)));
		}

		private void ExpectUnknownDataDetectedEvent(long offset, long length)
		{
			_unknownDataDetectedEventHandler.Expect(e => e(_dataScanner, new UnknownDataDetectedEventArgs(offset, length)));
		}

		private void SubscribeToReportDataEvents()
		{
			_dataScanner.DataBlockDetected += _dataBlockDetectedEventHandler;
			_dataScanner.DataBlockDiscarded += _dataBlockDiscardedEventHandler;
			_dataScanner.UnknownDataDetected += _unknownDataDetectedEventHandler;
		}

		private void ScanWithDetectors(IEnumerable<IDetector> detectors)
		{
			_dataScanner.Detectors = detectors;
			_dataScanner.Scan(_dataReader, _dummyProgressReporter);
		}

		private void ScanWithDetectorsAndProgress(IEnumerable<IDetector> detectors)
		{
			_dataScanner.Detectors = detectors;
			_dataScanner.Scan(_dataReader, _mockProgressReporter);
		}
		#endregion Setup and expectation helpers
	}
}
