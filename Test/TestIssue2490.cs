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
using Defraser.Detector.Mpeg2.Video;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	/// <summary>
	/// Issue 2490 - Save divided (=fragmented) results as one file.
	/// Save results that were divided into smaller results to prevent the application
	/// from an out-of-memory situation as one file.
	/// </summary>
	[TestFixture]
	public class TestIssue2490
	{
		private const string SavedAudioCodecStreamFileName = "Back_Up.mpg_MPEG-1_2_Systems_extracted_Unknown_stream_0000000000001B55.bin";
		private const string SavedVideoCodecStreamFileName = "Back_Up.mpg_MPEG-1_2_Systems_extracted_Mpeg1Video_stream_0000000000001246.m2v";
		private const string SavedContainerStreamFileName = "Back_Up.mpg_MPEG-1_2_Systems_0000000000000000-00000000003A280C.mpg";

		private const string ReferenceAudioCodecStreamFileName = "Reference_Back_Up.mpg_MPEG-1_2_Systems_extracted_Unknown_stream_0000000000001B55.bin";
		private const string ReferenceVideoCodecStreamFileName = "Reference_Back_Up.mpg_MPEG-1_2_Systems_extracted_Mpeg1Video_stream_0000000000001246.m2v";
		private const string ReferenceContainerStreamFileName = "Reference_Back_Up.mpg_MPEG-1_2_Systems_0000000000000000-00000000003A280C.mpg";

		private const string FileNameProjectIssue2490 = "issue2490.dpr";
		private readonly string FileNameIssue2490 = Util.TestdataPath + "Back_Up.mpg";
		private const string MaxSystemHeaderCount = "Max System Header Count";
		private const string MaxVideoHeaderCount = "Max Video Header Count";

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			DeleteRefFiles();

			// Create reference files by
			// scanning files without creating fragmented results
			ScanAndSaveAsSeparateFiles(default(KeyValuePair<string, string>), default(KeyValuePair<string, string>));

			// and copying the results to a different file
			File.Move(SavedAudioCodecStreamFileName, ReferenceAudioCodecStreamFileName);
			File.Move(SavedVideoCodecStreamFileName, ReferenceVideoCodecStreamFileName);
			File.Move(SavedContainerStreamFileName, ReferenceContainerStreamFileName);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			DeleteSavedFiles();
			DeleteRefFiles();
		}

		[Ignore("Can not get it to work without breaking other code"), Test, Category("Regression")]
		public void TestSaveFragmentedSystemResultsUnfragmentedVideoResults()
		{
			ScanAndSaveAsSeparateFiles(new KeyValuePair<string, string>(MaxSystemHeaderCount, "2000"), default(KeyValuePair<string, string>));

			TestFilesAreEqual(SavedAudioCodecStreamFileName, ReferenceAudioCodecStreamFileName);
			TestFilesAreEqual(SavedVideoCodecStreamFileName, ReferenceVideoCodecStreamFileName);
			TestFilesAreEqual(SavedContainerStreamFileName, ReferenceContainerStreamFileName);
		}

		[Test, Category("Regression")]
		public void TestSaveUnfragmentedSystemResultsFragmentedVideoResults()
		{
			ScanAndSaveAsSeparateFiles(default(KeyValuePair<string, string>), new KeyValuePair<string, string>(MaxVideoHeaderCount, "400"));

			TestFilesAreEqual(SavedAudioCodecStreamFileName, ReferenceAudioCodecStreamFileName);
			TestFilesAreEqual(SavedVideoCodecStreamFileName, ReferenceVideoCodecStreamFileName);
			TestFilesAreEqual(SavedContainerStreamFileName, ReferenceContainerStreamFileName);
		}

		[Ignore("Can not get it to work without breaking other code"), Test, Category("Regression")]
		public void TestSaveFragmentedSystemResultsFragmentedVideoResults()
		{
			ScanAndSaveAsSeparateFiles(new KeyValuePair<string, string>(MaxSystemHeaderCount, "2000"),
				new KeyValuePair<string, string>(MaxVideoHeaderCount, "400"));

			TestFilesAreEqual(SavedAudioCodecStreamFileName, ReferenceAudioCodecStreamFileName);
			TestFilesAreEqual(SavedVideoCodecStreamFileName, ReferenceVideoCodecStreamFileName);
			TestFilesAreEqual(SavedContainerStreamFileName, ReferenceContainerStreamFileName);
		}

		private void ScanAndSaveAsSeparateFiles(KeyValuePair<string,string> systemConfiguration, KeyValuePair<string,string> videoConfiguration)
		{
			DeleteSavedFiles();

			IProject projectIssue2490 = TestFramework.ProjectManager.CreateProject(FileNameProjectIssue2490, "S. Holmes", DateTime.Now, "Scan file 1");

			// Locate detectors (and change configuration)
			TestFramework.DetectorFactory.Initialize(".");

			IDetector systemDetector = Util.FindDetector(TestFramework.DetectorFactory.ContainerDetectors, "MPEG-1/2 Systems");
			IDetector videoDetector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, Mpeg2VideoDetector.DetectorName);

			if (!string.IsNullOrEmpty(systemConfiguration.Key))
			{
				systemDetector.SetConfigurationItem(systemConfiguration.Key, systemConfiguration.Value);
			}
			if (!string.IsNullOrEmpty(videoConfiguration.Key))
			{
				videoDetector.SetConfigurationItem(videoConfiguration.Key, videoConfiguration.Value);
			}

			IDetector[] containerDetectors = new IDetector[] { systemDetector };
			IDetector[] codecDetectors = new IDetector[] { videoDetector };

			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);

			// Scan test file
			IInputFile fileIssue2490 = TestFramework.DetectData(containerDetectors, codecDetectors, projectIssue2490, FileNameIssue2490);

			if (!string.IsNullOrEmpty(systemConfiguration.Key))
			{
				systemDetector.ResetConfigurationItem(systemConfiguration.Key);
			}
			if (!string.IsNullOrEmpty(videoConfiguration.Key))
			{
				videoDetector.ResetConfigurationItem(videoConfiguration.Key);
			}

			TestFramework.SaveAsSeparateFiles(new List<object> { fileIssue2490 }, ".");

			if (projectIssue2490 != null)
			{
				TestFramework.ProjectManager.CloseProject(projectIssue2490);
				projectIssue2490 = null;
			}
		}

		private void TestFilesAreEqual(string savedFile, string refFile)
		{
			byte[] savedBytes = File.ReadAllBytes(savedFile);
			byte[] refBytes = File.ReadAllBytes(refFile);

			Assert.That(savedBytes.Length, Is.EqualTo(refBytes.Length), "File '{0}' and '{1}' are different in length.", savedFile, refFile, savedBytes.Length, refBytes.Length);

			for (long byteIndex = 0; byteIndex < savedBytes.Length; byteIndex++)
			{
				Assert.That(savedBytes[byteIndex] == refBytes[byteIndex], "Content of the saved file '{0}' differs from the reference file '{1}' at location {2}", savedFile, refFile, byteIndex);
			}
		}

		private void DeleteSavedFiles()
		{
			File.Delete(FileNameProjectIssue2490);

			File.Delete(SavedAudioCodecStreamFileName);
			File.Delete(SavedVideoCodecStreamFileName);
			File.Delete(SavedContainerStreamFileName);
		}

		private void DeleteRefFiles()
		{
			File.Delete(ReferenceAudioCodecStreamFileName);
			File.Delete(ReferenceVideoCodecStreamFileName);
			File.Delete(ReferenceContainerStreamFileName);
		}
	}
}
