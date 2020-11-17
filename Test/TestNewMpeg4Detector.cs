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
using System.IO;
using Defraser.Detector.Mpeg4;
using Defraser.Framework;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestNewMpeg4Detector
	{
		private const string Mpeg4ProjectFile = @"./mpeg4-project.dpr";
		private readonly string TestFilePath = Util.TestdataPath;
		private const string ReferenceFilePostFix = "_new";

		private IDetector _mpeg4Detector;
		private IProject _mpeg4Project;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			DeleteProjectsFiles();

			TestFramework.DetectorFactory.Initialize(".");
			_mpeg4Detector = Util.FindDetector(TestFramework.DetectorFactory.CodecDetectors, "MPEG-4 Video/H.263");

			_mpeg4Project = TestFramework.ProjectManager.CreateProject(Mpeg4ProjectFile, "Kerst Klaas", DateTime.Now, "<description>");
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			// Close the project files
			TestFramework.ProjectManager.CloseProject(_mpeg4Project);

			DeleteProjectsFiles();
		}

		[Test]
		public void TestMax2Log()
		{
			Assert.That(1, Is.EqualTo(Mpeg4Math.Max2Log(1)));
			Assert.That(1, Is.EqualTo(Mpeg4Math.Max2Log(2)));
			Assert.That(2, Is.EqualTo(Mpeg4Math.Max2Log(3)));
			Assert.That(2, Is.EqualTo(Mpeg4Math.Max2Log(4)));
			Assert.That(3, Is.EqualTo(Mpeg4Math.Max2Log(5)));
			Assert.That(3, Is.EqualTo(Mpeg4Math.Max2Log(8)));
			Assert.That(4, Is.EqualTo(Mpeg4Math.Max2Log(9)));
			Assert.That(4, Is.EqualTo(Mpeg4Math.Max2Log(16)));
			Assert.That(5, Is.EqualTo(Mpeg4Math.Max2Log(17)));
			Assert.That(5, Is.EqualTo(Mpeg4Math.Max2Log(32)));
			Assert.That(6, Is.EqualTo(Mpeg4Math.Max2Log(33)));
		}

		[Test, Category("Regression")]
		public void TestDancingSkeleton()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "dancing-skeleton.3gp"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void TestSkatingDog()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "skating-dog.3gp"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void Test3GPMP4V()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "3GP_mp4v.3gp"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void TestNewResultWhenH263VopFoundAfterTwoMpeg4Vops()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "Mpeg4DetectorNewResultWhenH263VopFoundAfterTwoMpeg4Vops.m4v"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void TestNewResultWhenTwoMpeg4VopsFoundAfterH263Vop()
		{
			const string minVopShortHeaderCount = "Min Vop Short Header Count";

			_mpeg4Detector.SetConfigurationItem(minVopShortHeaderCount, "1");

			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "Mpeg4DetectorNewResultWhenTwoMpeg4VopsFoundAfterH263Vop.m4v"), ReferenceFilePostFix);

			_mpeg4Detector.ResetConfigurationItem(minVopShortHeaderCount);
		}

		[Test, Category("Regression")]
		public void TestNewResultWhenH263VopFoundInMpeg4HeaderGroupVop()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "Mpeg4DetectorNewResultWhenH263VopFoundAfterMpeg4GroupVop.m4v"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void TestNewResultWhenH263VopFoundInMpeg4HeaderVideoObjectLayer()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "Mpeg4DetectorNewResultWhenH263VopFoundAfterMpeg4VideoObjectLayer.m4v"), ReferenceFilePostFix);
		}

		[Test, Category("Regression")]
		public void TestNewResultWhenH263VopFoundAfterMpeg4VopInVol()
		{
			Util.CompareResultWithReference(_mpeg4Project, _mpeg4Detector, Path.Combine(TestFilePath, "Mpeg4DetectorNewResultWhenH263VopFoundAfterMpeg4VopInVol.m4v"), ReferenceFilePostFix);
		}

		private void DeleteProjectsFiles()
		{
			if (File.Exists(Mpeg4ProjectFile)) File.Delete(Mpeg4ProjectFile);
		}
	}
}
