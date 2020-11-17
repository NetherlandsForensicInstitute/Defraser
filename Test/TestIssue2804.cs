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
using Defraser.DataStructures;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture, Description("DEFR-701, bugzilla 2804")]
	public class TestIssue2804
	{
		private const string FileNameProject = "issue2804.dpr";
		private IProject _project;

		private IDetector _detector;
		private readonly string _fileName = Util.TestdataPath + "Samsung U600 reference.mp4";

		[SetUp]
		public void Setup()
		{
			TestFramework.DetectorFactory.Initialize(".");
			_detector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, "MPEG-4 Video/H.263");
			_detector.ResetConfiguration();
		}

		[Test, Category("Regression")]
		public void Test2804()
		{
			var file = TestFramework.CreateInputFile(_fileName);
			var dataReader = TestFramework.CreateDataReader(file);
			var scanContext = TestFramework.CreateScanContext(_project);

			//File begins with VOPs
			IDataBlock vopBlock;
			IDataBlockBuilder vopBuilder;
			Assert.IsTrue(GetNextDataBlock(file, dataReader, scanContext, out vopBlock, out vopBuilder));
			Assert.IsNotNull(vopBlock);

			Assert.AreEqual(vopBlock.EndOffset,dataReader.Position);
			Assert.AreEqual(file.Length, dataReader.Length);

			//Next valid datablock is a VOL
			IDataBlock volBlock;
			IDataBlockBuilder volBuilder;
			Assert.IsTrue(GetNextDataBlock(file, dataReader, scanContext, out volBlock, out volBuilder));
			Assert.IsNotNull(volBlock);
			Assert.IsNotNull(volBuilder);

			IResultNode volResult = scanContext.Results;
			Assert.IsNotNull(volResult);

			var expectedVol = volResult.GetLastDescendant();
			Assert.AreEqual("VideoObjectLayer", expectedVol.Name);
		}

		private bool GetNextDataBlock(IInputFile file, IDataReader dataReader, IScanContext scanContext, out IDataBlock volBlock, out IDataBlockBuilder volBuilder)
		{
			IDataBlock volBlockAttempt=null;
			IDataBlockBuilder volBlockAttemptBuilder=null;
			while (volBlockAttempt == null && dataReader.State==DataReaderState.Ready)
			{
				volBlockAttemptBuilder= CreateBuilder(file);
				scanContext.Detectors = new[] { _detector };
				volBlockAttempt = _detector.DetectData(dataReader, volBlockAttemptBuilder, scanContext);
			}
			volBlock = volBlockAttempt;
			volBuilder = volBlockAttemptBuilder;
			return volBlock != null;
		}

		private IDataBlockBuilder CreateBuilder(IInputFile file)
		{
			IDataBlockBuilder builder = TestFramework.CreateDataBlockBuilder();
			builder.Detectors = new[] { _detector };
			builder.InputFile = file;
			return builder;
		}

		[TearDown]
		public void TearDown()
		{
			if (_project != null)
			{
				TestFramework.ProjectManager.CloseProject(_project);
				_project = null;
			}
			File.Delete(FileNameProject);
		}
	}
}
