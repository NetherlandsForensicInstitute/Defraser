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
using System.IO;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using System.Linq;

namespace Defraser.Test.Common
{
	public class DetectorTester : IDisposable
	{
		private IProject _project;
		private string _fileNameProject;
		private IDetector _videoDetector;
		private readonly MockRepository _mockRepository = new MockRepository();
		private IDetector _systemDetector;

		public DetectorTester WithProjectFile(string fileNameProject)
		{
			_fileNameProject = fileNameProject;
			_project = TestFramework.ProjectManager.CreateProject(fileNameProject, "S. Holmes", DateTime.Now, "Scan file 1");
			return this;
		}

		/// <summary>
		/// Finds the detector in the plugin directory based on the name. This takes one second more compared to <see cref="WithDetector(Defraser.Interface.IDetector)"/>
		/// </summary>
		/// <param name="detectorName"></param>
		/// <returns></returns>
		public DetectorTester WithDetector(string detectorName)
		{
			_videoDetector = new PluginTester().FindByName(detectorName);
			return this;
		}

		/// <summary>
		/// Uses the provided instance to scan the input file.
		/// </summary>
		/// <param name="detectorInstance"></param>
		/// <returns></returns>
		public DetectorTester WithDetector(IDetector detectorInstance)
		{
			Assert.IsNotNull(detectorInstance, "Incorrect Test Setup:Invalid IDetector instance.");
			_videoDetector = detectorInstance;
			return this;
		}

		public DetectorTester WithContainerDetector(IDetector systemDetector)
		{
			Assert.IsNotNull(systemDetector, "Incorrect Test Setup:Invalid IDetector instance.");
			_systemDetector = systemDetector;
			return this;
		}

		public void Dispose()
		{
			if (_project != null)
			{
				TestFramework.ProjectManager.CloseProject(_project);
				_project = null;
			}
		}

		/// <summary>
		/// You need a project file for this kind of scan
		/// </summary>
		/// <param name="fileName"></param>
		public DataBlockVerification VerifyScanOf(string fileName)
		{
			var dataBlocks = VerifyScanOfFragmented(fileName);
			Assert.That(dataBlocks.Count, Is.AtLeast(1), "Unexpected number of datablocks.");

			Assert.That(dataBlocks.Count, Is.EqualTo(1), "Unexpected number of datablocks: found datablocks:" + Environment.NewLine + Describe(dataBlocks) + Environment.NewLine + "Unexpected number of datablocks.");
			return dataBlocks[0];
		}

		private static String Describe(IEnumerable<DataBlockVerification> blockVerifications)
		{
			return String.Join(Environment.NewLine, blockVerifications.Select(verifier => Describe(verifier.DataBlock)).ToArray());
		}

		private static String Describe(IFragment dataBlock)
		{
			return TestFramework.GetDescriptionText(TestFramework.GetResults(dataBlock));
		}

		public IList<DataBlockVerification> VerifyScanOfFragmented(string fileName)
		{
			if (String.IsNullOrEmpty(_fileNameProject))
				Assert.Fail("Incorrect Test Setup:WithProjectFile has not been called.");
			if (!File.Exists(fileName))
				Assert.Fail("Incorrect Test Setup:No file exists at " + Path.GetFullPath(fileName));
			if (_videoDetector == null)
				Assert.Fail("Incorrect Test Setup:WithDetector has not been called.");
			File.Delete(_fileNameProject);
			// Scan test file
			IInputFile scannedFile = ScanFile(fileName);
			var blocks = _project.GetDataBlocks(scannedFile);
			var verifyers = new List<IDataBlock>(blocks).ConvertAll(block =>
																		{
																			var result = TestFramework.GetResults(block);
																			var descr = TestFramework.GetDescriptionText(result);
																			return new DataBlockVerification(block, result, "fragment " + block.FragmentIndex + " detected using " + (_systemDetector == null ? "" : _systemDetector.Name + " and ") + _videoDetector.Name + ", containing Results:" + descr);
																		});
			return verifyers;
		}


		private IInputFile ScanFile(string fileName)
		{
			IInputFile scannedFile;
			if (_systemDetector != null)
				scannedFile = TestFramework.DetectData(new[] { _systemDetector }, new[] { _videoDetector }, _project, fileName);
			else
				scannedFile = TestFramework.DetectData(new IDetector[] { }, new[] { _videoDetector }, _project, fileName);
			CollectionAssert.IsNotEmpty(_project.GetInputFiles().Select(file => file.Name), "Scanned files");
			return scannedFile;
		}

		/// <summary>
		/// You do not need a project file for this kind of scan
		/// </summary>
		/// <param name="byteStream"></param>
		public DataBlockVerification Scan(byte[] byteStream)
		{
			IScanContext scanContext = TestFramework.CreateScanContext(_project);
			IDataBlock dataBlock = ScanDataBlock(byteStream, scanContext);
			Assert.IsNotNull(dataBlock);

			// Data block detected, perform a rescan for the results
			IScanContext rescanContext = TestFramework.CreateScanContext(_project);
			rescanContext.Detectors = dataBlock.Detectors;
			var detectedByteStream = new byte[dataBlock.Length];
			Array.Copy(byteStream, dataBlock.StartOffset, detectedByteStream, 0, detectedByteStream.Length);
			ScanDataBlock(detectedByteStream, rescanContext);
			Assert.IsNotNull(rescanContext.Results);

			return new DataBlockVerification(dataBlock, rescanContext.Results, "using " + _videoDetector.Name + " detected datablock within memory buffer of size " + byteStream.Length + "." + TestFramework.GetDescriptionText(rescanContext.Results));
		}

		private IDataBlock ScanDataBlock(byte[] byteStream, IScanContext scanContext)
		{
			scanContext.Detectors = new[] { _videoDetector };
			IDataBlockBuilder builder = TestFramework.CreateDataBlockBuilder();
			builder.Detectors = scanContext.Detectors;
			var inputFile = _mockRepository.StrictMock<IInputFile>();
			With.Mocks(_mockRepository).Expecting(delegate
			                                      	{
			                                      		SetupResult.For(inputFile.Name).Return("<ByteArrayDataReader>");
			                                      		SetupResult.For(inputFile.Length).Return(byteStream.Length);
			                                      	});
			builder.InputFile = inputFile;
			var mockDataReader = new MockDataReader(byteStream, inputFile);
			IDataBlock dataBlock = _videoDetector.DetectData(mockDataReader, builder, scanContext);
			Assert.IsNotNull(dataBlock);
			return dataBlock;
		}
	}
}
