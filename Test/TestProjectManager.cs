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
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestProjectManager
	{
		private const string BogusPath = "No such path!";
		private const string DummyFile = "dummy.txt";
		private const string EmptyFile = "empty.txt";
		private const string ProjectPath = "test.dpr";
		private const string Project2Path = "test2.dpr";
		private const string ProjectInvestigator = "Paulus Pietsma";
		private readonly DateTime ProjectCreationDate = DateTime.Now.AddSeconds(-2);
		private const string ProjectDescription = "<description>";

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			TestFramework.ProjectManager.Dispose();

			if (File.Exists(DummyFile))
			{
				File.Delete(DummyFile);
			}
			if (File.Exists(EmptyFile))
			{
				File.Delete(EmptyFile);
			}

			using (StreamWriter streamWriter = File.CreateText(DummyFile))
			{
				streamWriter.WriteLine("dummy text file");
			}

			File.CreateText(EmptyFile).Close();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			if (File.Exists(DummyFile))
			{
				File.Delete(DummyFile);
			}
			if (File.Exists(EmptyFile))
			{
				File.Delete(EmptyFile);
			}
		}

		[SetUp]
		public void SetUp()
		{
			if (File.Exists(ProjectPath))
			{
				File.Delete(ProjectPath);
			}
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(ProjectPath))
			{
				File.Delete(ProjectPath);
			}

			TestFramework.ProjectManager.Dispose();
		}

		[Test]
		public void TestProject()
		{
			Assert.IsNull(TestFramework.ProjectManager.Project, "Project, no projects (null)");
			IProject project1 = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			Assert.AreSame(project1, TestFramework.ProjectManager.Project, "Project, after creating project (newly created project)");
			IProject project2 = TestFramework.ProjectManager.CreateProject(Project2Path, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			Assert.AreSame(project1, TestFramework.ProjectManager.Project, "Project, after creating 2 projects (first project)");
			TestFramework.ProjectManager.CloseProject(project1);
			Assert.AreSame(project2, TestFramework.ProjectManager.Project, "Project, after closing first project (second project)");
			TestFramework.ProjectManager.CloseProject(project2);
			Assert.IsNull(TestFramework.ProjectManager.Project, "Project, after closing all projects (null)");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestCreateProjectPathNull()
		{
			TestFramework.ProjectManager.CreateProject(null, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestCreateProjectPathEmpty()
		{
			TestFramework.ProjectManager.CreateProject(string.Empty, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
		}

		[Test]
		public void TestCreateProjectFileExists()
		{
			TestFramework.ProjectManager.CreateProject(DummyFile, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			Assert.IsTrue(true);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestCreateOpenProject()
		{
			TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestCreateProjectInvestigatorNameNull()
		{
			TestFramework.ProjectManager.CreateProject(ProjectPath, null, ProjectCreationDate, ProjectDescription);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestCreateProjectDescriptionNull()
		{
			TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, null);
		}

		[Test]
		public void TestCreateProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			Assert.AreEqual(0, project.GetInputFiles().Count, "New project has no input files.");
			IDictionary<ProjectMetadataKey, string> metadata = project.GetMetadata();
			Assert.AreEqual(4, metadata.Count, "New project metadata entries.");
			Assert.AreEqual(ProjectPath, project.FileName, "New project file name.");
			Assert.AreEqual("1.0.0.4", metadata[ProjectMetadataKey.FileVersion], "New project file version");
			Assert.AreEqual(ProjectInvestigator, metadata[ProjectMetadataKey.InvestigatorName], "New project investigator name.");
			Assert.AreEqual(ProjectCreationDate.ToString(), DateTime.Parse(metadata[ProjectMetadataKey.DateCreated]).ToString(), "New project creation date.");
			Assert.AreEqual(ProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "New project description.");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestOpenProjectNull()
		{
			TestFramework.ProjectManager.OpenProject(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestOpenProjectEmptyPath()
		{
			TestFramework.ProjectManager.OpenProject(string.Empty);
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void TestOpenProjectNonExisting()
		{
			TestFramework.ProjectManager.OpenProject(BogusPath);
		}

		[Test]
		[ExpectedException(typeof(InvalidProjectException))]
		public void TestOpenProjectZeroLength()
		{
			TestFramework.ProjectManager.OpenProject(EmptyFile);
		}

		[Test]
		[ExpectedException(typeof(InvalidProjectException))]
		public void TestOpenProjectInvalid()
		{
			TestFramework.ProjectManager.OpenProject(DummyFile);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestOpenOpenProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.ProjectManager.SaveProject(project);
			TestFramework.ProjectManager.OpenProject(ProjectPath);
		}

		[Test]
		public void TestOpenProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.ProjectManager.SaveProject(project);
			TestFramework.ProjectManager.CloseProject(project);
			project = TestFramework.ProjectManager.OpenProject(ProjectPath);
			IDictionary<ProjectMetadataKey, string> metadata = project.GetMetadata();
			Assert.AreEqual(5, metadata.Count, "Reopened project metadata entries.");
			Assert.AreEqual(ProjectPath, project.FileName, "Reopened project file name.");
			Assert.AreEqual("1.0.0.4", metadata[ProjectMetadataKey.FileVersion], "Reopened project file version.");
			Assert.AreEqual(ProjectInvestigator, metadata[ProjectMetadataKey.InvestigatorName], "Reopened project investigator name.");
			Assert.AreEqual(ProjectCreationDate.ToString(), DateTime.Parse(metadata[ProjectMetadataKey.DateCreated]).ToString(), "Reopened project creation date.");
			Assert.Greater(DateTime.Parse(metadata[ProjectMetadataKey.DateLastModified]), ProjectCreationDate, "Reopened project modification date.");
			Assert.AreEqual(ProjectDescription, metadata[ProjectMetadataKey.ProjectDescription], "Reopened project description.");
			TestFramework.ProjectManager.CloseProject(project);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSaveProjectNull()
		{
			TestFramework.ProjectManager.SaveProject(null);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestSaveProjectNotOpen()
		{
			TestFramework.ProjectManager.SaveProject(new DummyProject());
		}

		[Test]
		public void TestSaveProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.ProjectManager.SaveProject(project);
			IDictionary<ProjectMetadataKey, string> metadata = project.GetMetadata();
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateCreated), "Saved project creation date.");
			Assert.IsTrue(metadata.ContainsKey(ProjectMetadataKey.DateLastModified), "Saved project modification date.");
			Assert.Greater(DateTime.Parse(metadata[ProjectMetadataKey.DateLastModified]), ProjectCreationDate, "Saved project modification date.");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestCloseProjectNull()
		{
			TestFramework.ProjectManager.CloseProject(null);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestCloseProjectNotOpen()
		{
			TestFramework.ProjectManager.CloseProject(new DummyProject());
		}

		[Test]
		public void TestCloseProject()
		{
			IProject project = TestFramework.ProjectManager.CreateProject(ProjectPath, ProjectInvestigator, ProjectCreationDate, ProjectDescription);
			TestFramework.ProjectManager.CloseProject(project);
		}
	}
}
