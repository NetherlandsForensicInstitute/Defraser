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
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Test.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	static class Util
	{
		private const string TestDataPathSpecificPlatformOnBuildServer = "../../../../ROOT/testdata/";//Down from /Test/bin/x86/Debug/~.dll
		private const string TestDataPathVisualStudio = "../../../ROOT/testdata/";
		private const string TestDataPathBuildServer = "ROOT/testdata/";
		internal static string TestdataPath
		{
			get
			{
				if (Directory.Exists(TestDataPathVisualStudio))
					return TestDataPathVisualStudio;
				if (Directory.Exists(TestDataPathBuildServer))
					return TestDataPathBuildServer;
				if (Directory.Exists(TestDataPathSpecificPlatformOnBuildServer))
					return TestDataPathSpecificPlatformOnBuildServer;
				throw new FileNotFoundException("Could not find the directory with test data, tried " + Path.GetFullPath(TestDataPathBuildServer) + " and " + Path.GetFullPath(TestDataPathVisualStudio)+ " and " +Path.GetFullPath(TestDataPathSpecificPlatformOnBuildServer));
			}
		}

		internal static void Scan(string scanFileName, IProject projectIssue2332, string containerDetectorName, KeyValuePair<string, string> containerDetectorConfiguration, string codecDetectorName, KeyValuePair<string, string> codecDetectorConfiguration)
		{
			// Locate detectors (and change configuration)
			TestFramework.DetectorFactory.Initialize(".");

			IDetector systemDetector = FindDetector(TestFramework.DetectorFactory.ContainerDetectors, containerDetectorName);
			systemDetector.ResetConfiguration();
			IDetector videoDetector = FindDetector(TestFramework.DetectorFactory.CodecDetectors, codecDetectorName);
			videoDetector.ResetConfiguration();

			if (!string.IsNullOrEmpty(containerDetectorConfiguration.Key))
			{
				systemDetector.SetConfigurationItem(containerDetectorConfiguration.Key, containerDetectorConfiguration.Value);
			}
			if (!string.IsNullOrEmpty(codecDetectorConfiguration.Key))
			{
				videoDetector.SetConfigurationItem(codecDetectorConfiguration.Key, codecDetectorConfiguration.Value);
			}

			IDetector[] containerDetectors = new[] { systemDetector };
			IDetector[] codecDetectors = new[] { videoDetector };

			List<IDetector> detectors = new List<IDetector>();
			detectors.AddRange(containerDetectors);
			detectors.AddRange(codecDetectors);

			// Scan test file
			TestFramework.DetectData(containerDetectors, codecDetectors, projectIssue2332, scanFileName);

			if (!string.IsNullOrEmpty(containerDetectorConfiguration.Key))
			{
				systemDetector.ResetConfigurationItem(containerDetectorConfiguration.Key);
			}
			if (!string.IsNullOrEmpty(codecDetectorConfiguration.Key))
			{
				videoDetector.ResetConfigurationItem(codecDetectorConfiguration.Key);
			}
		}

		internal static void TestOneHeader(IFragment fragment, Constraint name, Constraint offset, Constraint length)
		{
			IResultNode results = TestFramework.GetResults(fragment);

			Assert.That(results.Children.Count, Is.EqualTo(1), "There should by only one child"); // assert that there is only one header
			Assert.That(results.Children[0].Name, name, "Name");
			Assert.That(results.Children[0].StartOffset, offset, "Offset");
			Assert.That(results.Children[0].Length, length, "Length");
		}

		internal static void TestFirstHeader(IFragment fragment, Constraint name, Constraint offset, Constraint length)
		{
			IResultNode results = TestFramework.GetResults(fragment);

			Assert.That(results.Children[0].Name, name, "Name");
			Assert.That(results.Children[0].StartOffset, offset, "Offset");
			Assert.That(results.Children[0].Length, length, "Length");
		}

		internal static IResultNode GetLastResultNode(IResultNode result)
		{
			if (result.Children.Count > 0)
			{
				return GetLastResultNode(result.Children[result.Children.Count - 1]);
			}
			return result;
		}

		internal static void TestLastHeader(IFragment fragment, Constraint name, Constraint offset, Constraint length)
		{
			IResultNode result = GetLastResultNode(TestFramework.GetResults(fragment));

			Assert.That(result.Name, name, "Name");
			Assert.That(result.StartOffset, offset, "Offset");
			Assert.That(result.Length, length, "Length");
		}

		internal static void TestStream(IDataBlock containerStream, Constraint dataFormat, Constraint offset, Constraint length,
			Constraint maxHeaderCountReached, Constraint fragmentedContrainer, Constraint fragmentedIndex,
			Constraint headerCount, Constraint streamCount)
		{
			Assert.That(containerStream.CodecStreams.Count, streamCount, "Number of streams");
			Assert.That(containerStream.StartOffset, offset, "Offset");

			TestStream(containerStream, dataFormat, length, maxHeaderCountReached, fragmentedContrainer, fragmentedIndex, headerCount);
		}

		internal static void TestStream(IFragment detectable, Constraint dataFormat, Constraint length,
			Constraint isFragmented, Constraint fragmentedContrainer, Constraint fragmentedIndex,
			Constraint headerCount)
		{
			IResultNode results = TestFramework.GetResults(detectable);
			Assert.That(results.Children.Count, headerCount, "Number of headers");
			Assert.That(detectable.DataFormat, dataFormat, "Data format");
			Assert.That(detectable.Length, length, "Length");
			Assert.That(detectable.IsFragmented, isFragmented, "Is fragmented");
			Assert.That(detectable.FragmentContainer, fragmentedContrainer, "Fragment container");
			Assert.That(detectable.FragmentIndex, fragmentedIndex, "Fragment index");
		}

		static internal void AssertArrayEqualsFile(byte[] contents, string testOutputFile)
		{
			byte[] actual = File.ReadAllBytes(testOutputFile);
			Assert.AreEqual(contents.Length, actual.Length, "Length does not match.");
			for (int i = 0; i < actual.Length; i++)
			{
				Assert.AreEqual(contents[i], actual[i], "Mismatch at location " + i);
			}
		}

		static internal IInputFile FindInputFile(IList<IInputFile> inputFiles, string name)
		{
			foreach (IInputFile inputFile in inputFiles)
			{
				if (inputFile.Name == name)
				{
					return inputFile;
				}
			}
			throw new Exception("InputFile not found.");
		}

		static internal IDetector FindDetector(ICollection<IDetector> detectors, string name)
		{
			foreach (IDetector detector in detectors)
			{
				if (detector.Name == name)
				{
					return detector;
				}
			}
			throw new Exception("Detector not found.");
		}

		static internal void WriteData(string fileName, byte[] data)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				stream.Write(data, 0, data.Length);
				stream.Flush();
				stream.Close();
			}
		}

		internal static void CompareResultWithReference(IProject project, IDetector detector, string completeFilePath)
		{
			CompareResultWithReference(project, detector, completeFilePath, string.Empty);
		}

		internal static void CompareResultWithReference(IProject project, IDetector detector, string completeFilePath, string referenceFileNamePostFix)
		{
			TestFramework.DetectData(detector, project, completeFilePath);

			IList<IDataBlock> dataBlocks = project.GetDataBlocks(GetInputFile(completeFilePath));

			// Make sure that there are results
			Assert.That(dataBlocks.Count, Is.GreaterThan(0), string.Format("Number of datablocks found in '{0}'", completeFilePath));

			for (int dataBlockIndex = 0; dataBlockIndex < dataBlocks.Count; dataBlockIndex++)
			{
				CompareResultWithReference(dataBlocks[dataBlockIndex], dataBlockIndex, completeFilePath, referenceFileNamePostFix);
			}
		}

		private static void CompareResultWithReference(IDataBlock dataBlock, int dataBlockIndex, string completeFilePath, string referenceFileNamePostFix)
		{
			// Create the actual data
			string s = TestFramework.ExportToXml(new[] { dataBlock });
			string[] xmlStringArray = s.Split(new[] { '\n' });

			// Create the expected data
			string referenceFilePath = Path.GetDirectoryName(completeFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(completeFilePath) + dataBlockIndex.ToString() + referenceFileNamePostFix + ".xml";
			Assert.That(File.Exists(referenceFilePath), Is.True, string.Format("The file '{0}' exists", referenceFilePath));

			using (StreamReader xmlReferenceFile = new StreamReader(referenceFilePath))
			{
				// Compare all lines one by one
				// Read first expected line
				int lineIndex = 0;
				string expectedLine = xmlReferenceFile.ReadLine();
				while (!xmlReferenceFile.EndOfStream)
				{
					if (lineIndex > 2) // Skip first three lines
					{
						string actualLine = xmlStringArray[lineIndex];
						actualLine = actualLine.Trim('\r');

						Assert.AreEqual(expectedLine, actualLine);
					}
					lineIndex++;

					// Read next expected line
					expectedLine = xmlReferenceFile.ReadLine();
				}
			}
		}

		/// <summary>
		/// Get the inputfile from the project defined by <paramref name="fileName"/>
		/// </summary>
		/// <param name="fileName">The complete path of the file</param>
		/// <returns>The input file object specified by <paramref name="fileName"/></returns>
		private static IInputFile GetInputFile(string fileName)
		{
			foreach (IInputFile inputFile in TestFramework.ProjectManager.Project.GetInputFiles())
			{
				if (inputFile.Name == fileName)
				{
					return inputFile;
				}
			}
			throw new FileNotFoundException("Could not find the inputfile ("+fileName+") of the project.", fileName);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestOneHeader(IFragmentedVerification fragmented, Constraint name, Constraint offset, Constraint length)
		{
			TestOneHeader(fragmented.VerifyFragmentation(), name, offset, length);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestStream(DataBlockVerification containerStream, Constraint dataFormat, Constraint offset, Constraint length,
										Constraint maxHeaderCountReached, Constraint fragmentedContrainer, Constraint fragmentedIndex,
										Constraint headerCount, Constraint streamCount)
		{
			containerStream.CodecStreamCount(streamCount);
			containerStream.StartOffset(offset);
			TestStream(containerStream, dataFormat, length, maxHeaderCountReached, fragmentedContrainer, fragmentedIndex, headerCount);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestStream(DataBlockVerification detectable, Constraint dataFormat, Constraint length,
			Constraint isFragmented, Constraint fragmentedContrainer, Constraint fragmentedIndex,
			Constraint headerCount)
		{
			detectable.DataFormat(dataFormat).Length(length);
			var fragment = detectable.VerifyFragmentation();
			fragment.Fragmented(isFragmented).Container(fragmentedContrainer).Index(fragmentedIndex);
			fragment.VerifyResultNode().HeaderCount(headerCount);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		public static void TestStream(IFragmentedVerification containerStream, Constraint dataFormat, Constraint length,
			Constraint isFragmented, Constraint fragmentedContrainer, Constraint fragmentedIndex,
			Constraint headerCount)
		{
			TestStream(containerStream.VerifyFragmentation(), dataFormat, length, isFragmented, fragmentedContrainer, fragmentedIndex, headerCount);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestStream(FragmentVerification fragment, Constraint dataFormat, Constraint length,
			Constraint isFragmented, Constraint fragmentedContrainer, Constraint fragmentedIndex,
			Constraint headerCount)
		{
			fragment.VerifyResultNode().HeaderCount(headerCount);
			fragment.DataFormat(dataFormat).Length(length).Fragmented(isFragmented).Container(fragmentedContrainer).Index(fragmentedIndex);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestFirstHeader(FragmentVerification fragment, Constraint name, Constraint offset, Constraint length)
		{
			var result = fragment.VerifyResultNode().VerifyChild(0);
			result.Name(name).StartOffset(offset).Length(length);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestFirstHeader(IFragmentedVerification fragmented, Constraint name, Constraint offset, Constraint length)
		{
			TestFirstHeader(fragmented.VerifyFragmentation(), name, offset, length);
		}


		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestLastHeader(FragmentVerification fragment, Constraint name, Constraint offset, Constraint length)
		{
			var result = fragment.VerifyResultNode().Last();
			result.Name(name).StartOffset(offset).Length(length);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestLastHeader(IFragmentedVerification fragmented, Constraint name, Constraint offset, Constraint length)
		{
			TestLastHeader(fragmented.VerifyFragmentation(), name, offset, length);
		}

		/// <summary>
		/// Usefull to easily refactor old test code to new fluent api test code. Use this method, inline it (but keep its defintion), and polish
		/// </summary>
		internal static void TestOneHeader(FragmentVerification fragment, Constraint name, Constraint offset, Constraint length)
		{
			var result = fragment.VerifyResultNode().HeaderCountEquals(1).VerifyChild(0);
			result.Name(name).StartOffset(offset).Length(length);
		}
	}
}
