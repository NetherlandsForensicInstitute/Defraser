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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	public sealed class ForensicIntegrityLog : IForensicIntegrityLog
	{
        #region Inner class
		private class LogWriter : IDisposable
		{
			/// <summary>The stream received in the constructor is wrapped by this TextWriter for easy text logging</summary>
			private readonly TextWriter _textWriter;
			/// <summary>The properties of the current project are written to the header of the log</summary>
			private readonly IProject _project;

			private readonly IDetectorFormatter _detectorFormatter;

			/// <summary>The SeparatorChar is used to </summary>
			private const char SeparatorChar = ';';

			internal LogWriter(Stream stream, IProject project, IDetectorFormatter detectorFormatter)
			{
				_textWriter = new StreamWriter(stream);
				_project = project;
				_detectorFormatter = detectorFormatter;
			}

			public void Dispose()
			{
				if (_textWriter != null)
				{
					_textWriter.Flush();
					_textWriter.Dispose();
				}
			}

			/// <summary>Write the project properties to the stream</summary>
			internal void WriteHeader()
			{
				IDictionary<ProjectMetadataKey, string> projectMetaData = _project.GetMetadata();

				if (!string.IsNullOrEmpty(_project.FileName)) { _textWriter.WriteLine("Project file name: {0}", _project.FileName); }
				_textWriter.WriteLine("Project creation date: {0}", projectMetaData.ContainsKey(ProjectMetadataKey.DateCreated) ? projectMetaData[ProjectMetadataKey.DateCreated] : "Unknown");
				_textWriter.WriteLine("Log file creation date: {0}", DateTime.Now.ToString(ProjectManager.DateTimeFormat));
				if (projectMetaData.ContainsKey(ProjectMetadataKey.DateLastModified))
				{
					_textWriter.WriteLine("Project last modified: {0}", projectMetaData[ProjectMetadataKey.DateLastModified]);
				}
				_textWriter.WriteLine("Defraser version: {0}", ExportToXml.ApplicationVersion);
				_textWriter.WriteLine("Investigator name: {0}", projectMetaData.ContainsKey(ProjectMetadataKey.InvestigatorName) ? projectMetaData[ProjectMetadataKey.InvestigatorName] : "Unknown");
			}

			/// <summary>Write the detectors used to the stream</summary>
			/// <param name="detectors">the detectors of the data packet that makes up the result file</param>
			internal void WriteUsedDetectors(IEnumerable<IDetector> detectors)
			{
				_textWriter.WriteLine();
				_textWriter.WriteLine("Container Detectors used:{0}Version:", SeparatorChar);
				foreach (IDetector detector in detectors.Where(d => !(d is ICodecDetector)))
				{
					_textWriter.WriteLine("{0}{1}{2}", detector.Name, SeparatorChar, _detectorFormatter.FormatVersion(detector));
				}

				_textWriter.WriteLine();
				_textWriter.WriteLine("Codec Detectors used:{0}Version:", SeparatorChar);
				foreach (ICodecDetector detector in detectors.Where(d => d is ICodecDetector))
				{
					_textWriter.WriteLine("{0}{1}{2}", detector.Name, SeparatorChar, _detectorFormatter.FormatVersion(detector));
				}
			}

			/// <summary>
			/// Create a header containing a list of all
			/// names of the input files used and their MD5 checksum
			/// </summary>
			/// <param name="dataPacket">the data packet that makes up the result file</param>
			internal void WriteSourceFileChecksum(IDataPacket dataPacket)
			{
				_textWriter.WriteLine();
				_textWriter.WriteLine("Source file name(s):{0}File size:{1}MD5 checksum:", SeparatorChar, SeparatorChar);

				foreach (IInputFile inputFile in GetInputFiles(dataPacket))
				{
					_textWriter.WriteLine("{0}{1}{2}{3}{4}", inputFile.Name, SeparatorChar, inputFile.Length, SeparatorChar, inputFile.Checksum);
				}
			}

			/// <summary>
			/// Write a checksum for the created result file
			/// </summary>
			/// <param name="resultFileName">The name of the result file</param>
			internal void WriteResultFileChecksum(string resultFileName)
			{
				using (FileStream fileStream = new FileStream(resultFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					byte[] hash = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(fileStream);

					StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
					foreach (byte value in hash)
					{
						stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0:X02}", value));
					}
					_textWriter.WriteLine();
					_textWriter.WriteLine("Resulting file name:{0}File size:{1}MD5 checksum:", SeparatorChar, SeparatorChar);
					_textWriter.WriteLine("{0}{1}{2}{3}{4}", resultFileName, SeparatorChar, fileStream.Length, SeparatorChar, stringBuilder.ToString());
				}
			}

			/// <summary>Create a list that contains the origin of all bytes in the result file</summary>
			/// <param name="dataPacket">the data packet that makes up the result file</param>
			/// <param name="forensicLogType">the type of data being logged</param>
			internal void WriteDetailInformation(IDataPacket dataPacket, ForensicLogType forensicLogType)
			{
				_textWriter.WriteLine();

			    string sourceHeaderText;
                if (forensicLogType == ForensicLogType.CopiedData)
                    sourceHeaderText = "Build-up of resulting file:";
                else if (forensicLogType == ForensicLogType.ConvertedData)
                    sourceHeaderText = "Build-up of source data (before being converted by FFmpeg):";
                else
                    sourceHeaderText = string.Empty;

                _textWriter.WriteLine(sourceHeaderText);
				_textWriter.WriteLine("From byte location:{0}To byte location:{1}Length:{2}Maps to source file(s):{3}From byte location:{4}To byte location:", SeparatorChar, SeparatorChar, SeparatorChar, SeparatorChar, SeparatorChar);

				for (long offset = 0L; offset < dataPacket.Length; )
				{
					IDataPacket subpacket = dataPacket.GetFragment(offset);
					_textWriter.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}",
						offset, SeparatorChar, offset + subpacket.Length, SeparatorChar, subpacket.Length,
						SeparatorChar, subpacket.InputFile.Name, SeparatorChar, subpacket.StartOffset,
						SeparatorChar, subpacket.EndOffset);
					offset += subpacket.Length;
				}
			}

			private static IEnumerable<IInputFile> GetInputFiles(IDataPacket dataPacket)
			{
				IList<IInputFile> inputFiles = new List<IInputFile> {dataPacket.InputFile};
				IDataPacket subpacket;
				for (long relativeOffset = 0L; relativeOffset < dataPacket.Length; relativeOffset += subpacket.Length)
				{
					subpacket = dataPacket.GetFragment(relativeOffset);

					if (!inputFiles.Contains(subpacket.InputFile))
					{
						inputFiles.Add(subpacket.InputFile);
					}
				}
				return inputFiles;
			}
		}
		#endregion Inner class

		private readonly ProjectManager _projectManager;
		private readonly IDetectorFormatter _detectorFormatter;

		public ForensicIntegrityLog(ProjectManager projectManager,IDetectorFormatter detectorFormatter )
		{
			PreConditions.Argument("projectManager").Value(projectManager).IsNotNull();

			_projectManager = projectManager;
			_detectorFormatter = detectorFormatter;
		}

		/// <summary>Create a forensic integrity log.</summary>
		/// <remarks>
		/// The log contains:
		/// <list type="bullet">
		/// <item>MD5 hashes of the all input files used.</item>
		/// <item>MD5 hash of result file.</item>
		/// <item>Detailed information about the source of each byte in the result file.</item>
		/// </list>
		/// </remarks>
		/// <param name="dataPacket">The data packet used to create the result file.</param>
		/// <param name="detectors">The metadata for the data packets</param>
		/// <param name="resultFile">The created result file.</param>
		/// <param name="stream">The stream to write the forensic integrity log information to.</param>
		public void Log(IDataPacket dataPacket, IEnumerable<IDetector> detectors, string resultFile, Stream stream, ForensicLogType forensicLogType)
		{
			PreConditions.Argument("dataPacket").Value(dataPacket).IsNotNull();
			PreConditions.Argument("detectors").Value(detectors).IsNotNull().And.DoesNotContainNull();
			PreConditions.Argument("resultFile").Value(resultFile).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("stream").Value(stream).IsNotNull();

			using(LogWriter logWriter = new LogWriter(stream, _projectManager.Project, _detectorFormatter))
			{
				logWriter.WriteHeader();

				logWriter.WriteUsedDetectors(detectors);

				logWriter.WriteSourceFileChecksum(dataPacket);

				logWriter.WriteResultFileChecksum(resultFile);

				logWriter.WriteDetailInformation(dataPacket, forensicLogType);
			}
		}
	}
}
