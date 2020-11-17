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
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	/// <summary>
	/// Manages open projects.
	/// Provides methods for creating, opening and closing projects.
	/// </summary>
	/// <remarks>
	/// Use <see cref="Dispose()"/> to close all open projects.
	/// </remarks>
	public class ProjectManager : IDisposable
	{
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Format used for creation and modification dates.</summary>
		public const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";

		private readonly Creator<IProject, string> _createProject;
		private readonly Creator<XmlObjectSerializer> _createXmlObjectSerializer;
		private readonly List<IProject> _projects;

		#region Properties
		/// <summary>The current active project.</summary>
		public IProject Project { get { return _projects.FirstOrDefault(); } }
		#endregion Properties

		/// <summary>
		/// Creates a new project manager.
		/// </summary>
		public ProjectManager(Creator<IProject, string> createProject, Creator<XmlObjectSerializer> createXmlObjectSerializer)
		{
			PreConditions.Argument("createProject").Value(createProject).IsNotNull();
			PreConditions.Argument("createXmlObjectSerializer").Value(createXmlObjectSerializer).IsNotNull();

			_createProject = createProject;
			_createXmlObjectSerializer = createXmlObjectSerializer;
			_projects = new List<IProject>();
		}

		/// <summary>
		/// Creates and opens a new project.
		/// </summary>
		/// <param name="path">the path for the project file</param>
		/// <param name="investigatorName">the name of the project investigator</param>
		/// <param name="creationDate">the project creation date</param>
		/// <param name="description">the project description</param>
		/// <returns>the newly created project</returns>
		public IProject CreateProject(string path, string investigatorName, DateTime creationDate, string description)
		{
			PreConditions.Argument("path").Value(path).IsNotNull().And.IsNotEmpty();
			PreConditions.Operation().IsInvalidIf(IsOpenProject(path), "Project already open.");
			PreConditions.Argument("investigatorName").Value(investigatorName).IsNotNull();
			PreConditions.Argument("description").Value(description).IsNotNull();

			IProject project = _createProject(path);
			_projects.Add(project);

			project.ProjectChanged += OnProjectChanged;
			project.PropertyChanged += OnPropertyChanged;
			OnProjectChanged(this, new ProjectChangedEventArgs(ProjectChangedType.Created, project));

			project.SetMetadata(CreateMetadata(investigatorName, description, creationDate));

			return project;
		}

		private Dictionary<ProjectMetadataKey, string> CreateMetadata(string investigatorName, string description, DateTime creationDate)
		{
			var metadata = new Dictionary<ProjectMetadataKey, string>();
			metadata[ProjectMetadataKey.ProjectDescription] = description;
			metadata[ProjectMetadataKey.InvestigatorName] = investigatorName;
			metadata[ProjectMetadataKey.DateCreated] = creationDate.ToString(DateTimeFormat);
			return metadata;
		}

		/// <summary>
		/// Raises the <c>PropertyChanged</c> event
		/// </summary>
		/// <param name="e">the event args for the <c>PropertyChanged</c> event</param>
		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(sender, e);
			}
		}

		/// <summary>
		/// Raises the <c>ProjectChanged</c> event
		/// </summary>
		/// <param name="e">the event args for the <c>ProjectChanged</c> event</param>
		private void OnProjectChanged(object sender, ProjectChangedEventArgs e)
		{
			if (ProjectChanged != null)
			{
				ProjectChanged(sender, e);
			}
		}

		/// <summary>
		/// Opens an existing project.
		/// </summary>
		/// <param name="path">the path for the project file</param>
		/// <returns>the project</returns>
		public IProject OpenProject(string path)
		{
			PreConditions.Argument("path").Value(path).IsNotNull().And.IsNotEmpty().And.IsExistingFile();
			PreConditions.Operation().IsInvalidIf(IsOpenProject(path), "Project already open");

			// Deserialize the project
			XmlObjectSerializer dataContractSerializer = _createXmlObjectSerializer();
			using (XmlReader reader = XmlReader.Create(path))
			{
				try
				{
					IProject project = dataContractSerializer.ReadObject(reader) as IProject;
					_projects.Add(project);

					project.ProjectChanged += OnProjectChanged;
					project.PropertyChanged += OnPropertyChanged;
					OnProjectChanged(this, new ProjectChangedEventArgs(ProjectChangedType.Opened, project));

					return project;
				}
				catch (SerializationException se)
				{
					throw new InvalidProjectException("Project is invalid.", path, se);
				}
			}
		}

		/// <summary>
		/// Saves the given <paramref name="project"/>.
		/// </summary>
		/// <remarks>
		/// Saving a project automatically updates its modifcation date.
		/// </remarks>
		/// <param name="project">the project to save</param>
		public void SaveProject(IProject project)
		{
			PreConditions.Argument("project").Value(project).IsNotNull();
			PreConditions.Operation().IsInvalidIf(!_projects.Contains(project), "Project is not open");

			// Update modification date
			var metadata = new Dictionary<ProjectMetadataKey, string>(project.GetMetadata());
			metadata[ProjectMetadataKey.DateLastModified] = DateTime.Now.ToString(DateTimeFormat);

			project.SetMetadata(metadata);
            //ValidateFragmentenDataBlocks(project);

			// Serialize (save) the project
		    try
		    {
                var xmlObjectSerializer = _createXmlObjectSerializer();
			    var writerSettings = new XmlWriterSettings { Indent = true };
			    using (XmlWriter writer = XmlWriter.Create(project.FileName, writerSettings))
			    {
				    xmlObjectSerializer.WriteObject(writer, project);
			    }

			    project.Dirty = false;
		    }
            catch (Exception exception)
		    {
                MessageBox.Show(exception.Message,
                        "Failed to save the project",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
		    }
			
		}

        /*private static void ValidateFragmentenDataBlocks(IProject project)
        {
            IList<IInputFile> inputFiles = project.GetInputFiles();
            if (inputFiles.Count == 0) return;

            IList<IDataBlock> dataBlocks = project.GetDataBlocks(inputFiles[0]);
            IList<IList<IDataBlock>> dataBlockGroups = new List<IList<IDataBlock>>();

            // Create a group for each fragmentcontainer
            int blockIndex = 0;
            IList<IDataBlock> currentGroup = null;
            foreach (IDataBlock dataBlock in dataBlocks)
            {
                if (currentGroup != null)
                {
                    if (dataBlock.FragmentIndex > 0)
                    {
                        currentGroup.Add(dataBlock);
                        Console.WriteLine("Datablock {0} added for fragment index: {1}", blockIndex, dataBlock.FragmentIndex);
                    }
                    else
                    {
                        dataBlockGroups.Add(currentGroup);
                        currentGroup = null;
                    }
                }

                if (currentGroup == null && dataBlock.IsFragmented && dataBlock.FragmentIndex == 0)
                {
                    currentGroup = new List<IDataBlock>();
                    currentGroup.Add(dataBlock);
                    Console.WriteLine("Datablock {0} added for fragment index: 0", blockIndex);
                }
                blockIndex++;
            }

            // Maybe at the last group we found.
            if (currentGroup != null)
            {
                dataBlockGroups.Add(currentGroup);
                currentGroup = null;
            }

            // Report the group count
            Console.WriteLine("{0} datablock groups created.", dataBlockGroups.Count);
            
            int groupIndex = 0;
            foreach (IList<IDataBlock> group in dataBlockGroups)
            {
                //IFragmentContainer groupFragmentContainer = group.FirstOrDefault().FragmentContainer;
                if (group.Count > 1)
                {
                    // Get it from the second array element, the FragmentContainer of the first item is always NULL.
                    IFragmentContainer groupFragmentContainer = group[1].FragmentContainer;
                    if (groupFragmentContainer != null)
                    {
                        foreach (IDataBlock dataBlock in group)
                        {
                            // Validate that the IFragment container are the same instance.
                            if (dataBlock.FragmentContainer != null && dataBlock.FragmentContainer != groupFragmentContainer)
                            {
                                Console.WriteLine(
                                    "ERROR: Not all DataBlocks have the same instance of FragmentContainer in group {0}",
                                    groupIndex);
                                //break;
                            }

                            // Check the current datablock is a the right index in the container.
                            if (groupFragmentContainer[dataBlock.FragmentIndex] != dataBlock)
                            {
                                Console.WriteLine(
                                    "ERROR: The current datablock has not the same index ({0}) in the FragmentContainer in group {1}",
                                    dataBlock.FragmentIndex, groupIndex);
                                //break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("WARNING: Can not check group, FragmentContainer is NULL in group {0}",
                                    groupIndex);
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Can not check group, group {0} has only 1 item.",
                                    groupIndex);
                }
                groupIndex++;
            } 
            Console.WriteLine("All groups are checked. Validation completed.");
        }*/

	    /// <summary>
		/// Closes the given <paramref name="project"/>.
		/// </summary>
		/// <param name="project">the project</param>
		public void CloseProject(IProject project)
		{
			PreConditions.Argument("project").Value(project).IsNotNull();
			PreConditions.Operation().IsInvalidIf(!_projects.Contains(project), "Project is not open");

			project.ProjectChanged -= OnProjectChanged;
			project.PropertyChanged -= OnPropertyChanged;
			OnProjectChanged(this, new ProjectChangedEventArgs(ProjectChangedType.Closed, project));

			_projects.Remove(project);
			project.Dispose();
		}

		/// <summary>
		/// Closes all open projects.
		/// </summary>
		public void Dispose()
		{
			foreach (IProject project in _projects.ToArray())
			{
				CloseProject(project);
			}

			_projects.Clear();
		}

		/// <summary>
		/// Returns whether there exists an open project for the
		/// given <paramref name="path"/>.
		/// </summary>
		/// <param name="path">the path of the project file</param>
		/// <returns>true if there exists such a project, false otherwise</returns>
		private bool IsOpenProject(string path)
		{
			foreach (IProject project in _projects)
			{
				if (project.FileName == path)
				{
					return true;
				}
			}
			return false;
		}
	}
}
