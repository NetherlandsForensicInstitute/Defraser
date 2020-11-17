/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Defraser.GuiCe.sendtolist;
using Defraser.Interface;
using Rhino.Mocks;
using Defraser.GuiCe;
using System.IO;

namespace Defraser.Test
{

	[TestFixture]
	public class TestParametersChecker
	{

		public const string outputFilename = "out file";

		#region Mocks and stubs
		private MockRepository _mockRepository;
		private IForensicIntegrityLog _logger;
		private ISelection _selection;
		private IDataPacket _dataPacket;
		private IResultNode _resultNode;
		private IInputFile _inputFile;
		#endregion Mocks and stubs

		private ParametersChecker _checker;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
		}

		[SetUp]
		public void SetUp()
		{
			_mockRepository = new MockRepository();
			_logger = MockRepository.GenerateStub<IForensicIntegrityLog>();

			_selection = MockRepository.GenerateStub<ISelection>();
			_dataPacket = MockRepository.GenerateStub<IDataPacket>();
			_resultNode = MockRepository.GenerateStub<IResultNode>();
			_inputFile = MockRepository.GenerateStub<IInputFile>();

			_checker = new ParametersChecker(_logger);

		}

		[TearDown]
		public void TearDown()
		{
			_checker = null;
			_inputFile = null;
			_resultNode = null;
			_dataPacket = null;
			_selection = null;

			_logger = null;
			_mockRepository = null;
		}

		#region Tests for IsValidParametersString() method
		[Test]
		public void TestValidParametersString()
		{
			Assert.IsTrue(_checker.IsValidParameterString("[DAT][LOG][SRC][COUNT][OFFS][OFFE][SIZES][SIZEH][SIZET][LISTS][LISTO][DET]"));
			Assert.IsTrue(_checker.IsValidParameterString("-abc[LOG]xyz[OFFS]  [DET] -q "));
			Assert.IsTrue(_checker.IsValidParameterString("-abc"));
			Assert.IsTrue(_checker.IsValidParameterString(""));

			Assert.IsTrue(_checker.IsValidParameterString("[DAT]]"));
			Assert.IsTrue(_checker.IsValidParameterString("\\[[DAT]"));
			Assert.IsTrue(_checker.IsValidParameterString("\\[[DAT]\\]"));
			Assert.IsTrue(_checker.IsValidParameterString("\\[ [DAT] [LOG] \\]"));
		}

		[Test]
		public void TestInvalidParametersString()
		{
			Assert.IsFalse(_checker.IsValidParameterString("[[DAT]")); // [DAT no valid parameter
			Assert.IsFalse(_checker.IsValidParameterString("[[DAT]]")); // [DAT] no valid parameter
			Assert.IsFalse(_checker.IsValidParameterString("[AT]"));    // AT not valid
			Assert.IsFalse(_checker.IsValidParameterString("[DAT\\]]")); // DAT] not valid
			Assert.IsFalse(_checker.IsValidParameterString("[\\[DAT]")); // [DAT not valid
			Assert.IsFalse(_checker.IsValidParameterString("[[DAT]\\]"));
			Assert.IsFalse(_checker.IsValidParameterString("[ [DAT] [LOG] ]"));
		}
		#endregion Tests for IsValidParametersString() method

		#region Tests for Substitute() method
		[Test]
		public void TestSubstDat()
		{
			String parameters = "-l [DAT]";

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l \"out file\"");
		}

		[Test]
		public void TestWriteForensicLog()
		{
			String parameters = "-l [LOG]";
			IDetector[] detectors = new IDetector[0];

			_selection.Stub(s => s.Results).Return(new IResultNode[] { _resultNode });
			_resultNode.Stub(r => r.Detectors).Return(detectors);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l \"out file.csv\"");

			With.Mocks(_mockRepository).Expecting(delegate
			{
			}).Verify(delegate
			{
				_logger.Log(_dataPacket, detectors, outputFilename, Arg<FileStream>.Is.Anything, ForensicLogType.CopiedData);
			});
		}

		[Test]
		public void TestSubstSRC()
		{
			String parameters = "-l [SRC]";
			String sourceFile = "source file";
			_inputFile.Stub(i => i.Name).Return(sourceFile);
			_dataPacket.Stub(d => d.InputFile).Return(_inputFile);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l \"source file\"");
		}

		[Test]
		public void TestSubstCount()
		{
			String parameters = "-l [COUNT]";

			IResultNode top1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child2 = MockRepository.GenerateStub<IResultNode>();
			top1.Stub(t => t.Children).Return(new IResultNode[] { child1, child2 });
			child1.Stub(c => c.Children).Return(new IResultNode[0]);
			child2.Stub(c => c.Children).Return(new IResultNode[0]);
			IResultNode top2 = MockRepository.GenerateStub<IResultNode>();
			top2.Stub(t => t.Children).Return(new IResultNode[0]);

			_selection.Stub(s => s.Results).Return(new IResultNode[] { top1, top2 });

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l 4");
		}

		[Test]
		public void TestSubstOFFS()
		{
			String parameters = "-l [OFFS]";
			_dataPacket.Stub(d => d.StartOffset).Return(666);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l 666");
		}

		[Test]
		public void TestSubstOFFE()
		{
			String parameters = "-l [OFFE]";
			_dataPacket.Stub(d => d.EndOffset).Return(999);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-l 999");
		}

		[Test]
		public void TestSubstSIZES()
		{
			String parameters = "-q [SIZES]";
			_selection.Stub(s => s.Results).Return(new IResultNode[0]);
			_inputFile.Stub(i => i.Length).Return(666);
			_dataPacket.Stub(d => d.InputFile).Return(_inputFile);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-q 666");
		}

		[Test]
		public void TestSubstSIZEH()
		{
			String parameters = "-abc [SIZEH]";
			IResultNode child = MockRepository.GenerateStub<IResultNode>();

			_selection.Stub(s => s.Results).Return(new IResultNode[] { child });
			child.Stub(c => c.Length).Return(666);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-abc 666");
		}

		[Test]
		public void TestSubstSIZET()
		{
			String parameters = "-klm [SIZET]";
			_dataPacket.Stub(d => d.Length).Return(666);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-klm 666");
		}

		[Test]
		public void TestSubstLISTS()
		{
			String parameters = "-klm [LISTS]";

			IResultNode top1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child2 = MockRepository.GenerateStub<IResultNode>();
			top1.Stub(t => t.Children).Return(new IResultNode[] { child1, child2 });
			child1.Stub(c => c.Children).Return(new IResultNode[0]);
			child2.Stub(c => c.Children).Return(new IResultNode[0]);

			IResultNode top2 = MockRepository.GenerateStub<IResultNode>();
			top2.Stub(t => t.Children).Return(new IResultNode[0]);

			_selection.Stub(s => s.Results).Return(new IResultNode[] { top1, top2 });

			top1.Stub(t => t.Length).Return(8);
			top2.Stub(t => t.Length).Return(17);
			child1.Stub(c => c.Length).Return(99);
			child2.Stub(c => c.Length).Return(66);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-klm 8 99 66 17");
		}

		[Test]
		public void TestSubstLISTO()
		{
			String parameters = "-z [LISTO]";

			IResultNode top1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child2 = MockRepository.GenerateStub<IResultNode>();
			top1.Stub(t => t.Children).Return(new IResultNode[] { child1, child2 });
			child1.Stub(c => c.Children).Return(new IResultNode[0]);
			child2.Stub(c => c.Children).Return(new IResultNode[0]);

			IResultNode top2 = MockRepository.GenerateStub<IResultNode>();
			top2.Stub(t => t.Children).Return(new IResultNode[0]);

			_selection.Stub(s => s.Results).Return(new IResultNode[] { top1, top2 });

			top1.Stub(t => t.Length).Return(8);
			top1.Stub(t => t.StartOffset).Return(1);
			top2.Stub(t => t.Length).Return(17);
			top2.Stub(t => t.StartOffset).Return(2);
			child1.Stub(c => c.Length).Return(99);
			child1.Stub(c => c.StartOffset).Return(3);
			child2.Stub(c => c.Length).Return(66);
			child2.Stub(c => c.StartOffset).Return(4);

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-z 1 8 3 99 4 66 2 17");
		}

		[Test]
		public void TestSubstDETSingleDetector()
		{
			String parameters = "-a [DET]";

			IResultNode top1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode top2 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child1 = MockRepository.GenerateStub<IResultNode>();
			top1.Stub(t => t.Children).Return(new IResultNode[] { child1 });
			child1.Stub(c => c.Children).Return(new IResultNode[0]);
			top2.Stub(t => t.Children).Return(new IResultNode[0]);
			_selection.Stub(s => s.Results).Return(new IResultNode[] { top1, top2 });

			ICodecDetector detector = MockRepository.GenerateStub<ICodecDetector>();
			detector.Stub(d => d.Name).Return("xyz detector");

			top1.Stub(t => t.Detectors).Return(new IDetector[] { detector });
			top2.Stub(t => t.Detectors).Return(new IDetector[] { detector });
			child1.Stub(t => t.Detectors).Return(new IDetector[] { detector });

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-a xyz detector");
		}

		[Test]
		public void TestSubstDETMultipleDetectors()
		{
			String parameters = "-a [DET]";

			IResultNode top1 = MockRepository.GenerateStub<IResultNode>();
			IResultNode top2 = MockRepository.GenerateStub<IResultNode>();
			IResultNode child1 = MockRepository.GenerateStub<IResultNode>();
			top1.Stub(t => t.Children).Return(new IResultNode[] { child1 });
			child1.Stub(c => c.Children).Return(new IResultNode[0]);
			top2.Stub(t => t.Children).Return(new IResultNode[0]);
			_selection.Stub(s => s.Results).Return(new IResultNode[] { top1, top2 });

			ICodecDetector okDetector = MockRepository.GenerateStub<ICodecDetector>();
			okDetector.Stub(d => d.Name).Return("ok detector");
			IDetector notOkDetector = MockRepository.GenerateStub<IDetector>();
			notOkDetector.Stub(d => d.Name).Return("not ok detector");

			top1.Stub(t => t.Detectors).Return(new IDetector[] { okDetector });
			top2.Stub(t => t.Detectors).Return(new IDetector[] { notOkDetector });
			child1.Stub(t => t.Detectors).Return(new IDetector[] { okDetector });

			String subst = _checker.Substitute(parameters, _selection, _dataPacket, outputFilename);

			Assert.AreEqual(subst, "-a \"ok detector\"");
		}
		#endregion
	}
}
