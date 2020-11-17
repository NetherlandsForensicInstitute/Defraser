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
using System.Reflection;
using Defraser.Detector.UnknownFormat;
using Defraser.Interface;
using Defraser.Util;
using log4net;

namespace Defraser.Framework
{
	/// <summary>
	/// Use the detector factory for querying installed detector plugins.
	/// </summary>
	public class DetectorFactory : IDetectorFactory
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static List<IDetector> _detectors = new List<IDetector>();
		private static List<IDetector> _containerDetectors = new List<IDetector>();
		private static List<IDetector> _codecDetectors = new List<IDetector>();
		private const string AssemblyPattern = "*.dll";
		private readonly string[] _skipFiles =
                                                {
                                                    "ffmpeg32.dll",
                                                    "ffmpeg64.dll"
                                                }; // FFmpeg DDL's

		#region Properties
		/// <summary>The installed (all) detectors.</summary>
		public IList<IDetector> Detectors { get { return _detectors.AsReadOnly(); } }
		/// <summary>The installed container detectors.</summary>
		public IList<IDetector> ContainerDetectors { get { return _containerDetectors.AsReadOnly(); } }
		/// <summary>The installed codec detectors.</summary>
		public IList<IDetector> CodecDetectors { get { return _codecDetectors.AsReadOnly(); } }
		#endregion Properties

		/// <summary>
		/// Lists the detectors that can handle <paramref name="codecId"/>.
		/// </summary>
		/// <remarks>this function might be called <c>WhoCanDo()</c></remarks>
		/// <param name="codecId">the codec identifier</param>
		/// <returns>the codec detectors</returns>
		public IList<IDetector> GetDetectorsForCodec(CodecID codecId)
		{
			List<IDetector> codecDetectors = new List<IDetector>();

			foreach (IDetector detector in CodecDetectors)
			{
				if (detector.SupportedFormats.Contains(codecId))
				{
					codecDetectors.Add(detector);
				}
			}
			return codecDetectors;
		}

		/// <summary>
		/// Gets the single shared instance of the detector of the
		/// given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">the detector type</param>
		/// <returns>the single shared instance of the given type</returns>
		public IDetector GetDetector(Type type)
		{
			PreConditions.Argument("type").Value(type).IsNotNull();
			PreConditions.Argument("type").IsInvalidIf(!typeof(IDetector).IsAssignableFrom(type), "Not an IDetector type");

			//type = type.Assembly.CreateInstance(type.Name).GetType();
			foreach (IDetector factoryDetector in Detectors)
			{
				if (factoryDetector.DetectorType == type)
				{
					return factoryDetector;
				}
			}
			return null;
		}

		/// <summary>
		/// Initializes the detector factory with the specified plugin directory.
		/// </summary>
		/// <param name="pathName">the path name of the plugin directory</param>
		public void Initialize(string pathName)
		{
			PreConditions.Argument("pathName").Value(pathName).IsExistingDirectory();

			DirectoryInfo dirInfo = new DirectoryInfo(pathName);
			FileInfo[] files = dirInfo.GetFiles(AssemblyPattern);

			_detectors = new List<IDetector>();
			_containerDetectors = new List<IDetector>();
			_codecDetectors = new List<IDetector>();

			foreach (FileInfo fileInfo in files)
			{
				TryToLoadDetectorFrom(fileInfo);
			}
		}

		private void TryToLoadDetectorFrom(FileInfo fileInfo)
		{
			if (_skipFiles.Contains(fileInfo.Name))
				return;
			try
			{
				// Using 'LoadFile()' instead of 'LoadFrom()' causes the
				// 'GetDetector()' method to fail for unknown reason.
				LoadDetectorFrom(fileInfo);
			}
			catch (ReflectionTypeLoadException ex)
			{
				string message = string.Empty;
				Exception[] exceptions = ex.LoaderExceptions;
				foreach (Exception exception in exceptions)
				{
					if (exception != null)
					{
						message += exception.Message + Environment.NewLine;
					}
				}
				Log.Error("Failed to load detector dll " + fileInfo.FullName + Environment.NewLine + message, ex);
			}
			catch (BadImageFormatException ex)
			{
				Log.Error("Failed to load detector dll " + ex.FileName, ex);
			}
			catch (FileLoadException ex)
			{
				Log.Error("Failed to load detector dll " + ex.FileName, ex);
			}
			catch (FileNotFoundException ex)
			{
				Log.Error("Failed to load detector dll " + ex.FileName, ex);
			}
			catch (MissingMethodException ex)
			{
				Log.Error("Failed to load detector dll " + fileInfo.FullName, ex);
			}
		}

		private void LoadDetectorFrom(FileInfo fileInfo)
		{
			Assembly assembly = Assembly.LoadFrom(fileInfo.FullName);

			foreach (Type type in assembly.GetTypes())
			{
				if (IsValidDetector(type))
				{
					var detector = (IDetector)assembly.CreateInstance(type.FullName);
					Log.Debug("Found detector in " + detector.Name + " in file " + fileInfo.FullName);

					// Adds the detector to the (non)-container detector list
					if (detector is ICodecDetector)
					{
						var codecDetector = new CodecDetectorWrapper(detector as ICodecDetector);
						_detectors.Add(codecDetector);
						_codecDetectors.Add(codecDetector);
					}
					else
					{
						var containerDetector = new DetectorWrapper(detector);
						_detectors.Add(containerDetector);
						_containerDetectors.Add(containerDetector);
					}
				}
			}
		}

		private static bool IsValidDetector(Type type)
		{
			return typeof(IDetector).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType && type.GetConstructor(new Type[0]) != null && !typeof(UnknownFormatDetector).Equals(type);
		}
	}
}
