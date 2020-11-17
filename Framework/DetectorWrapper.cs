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
using System.Runtime.Serialization;
using Defraser.DataStructures;
using Defraser.Interface;
using log4net;

namespace Defraser.Framework
{
	[DataContract]
	public class DetectorWrapper : IDetector
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[DataMember]
		private readonly IDetector _detector;

		#region Properties
		public string Name { get { return _detector.Name; } }
		public string Description { get { return _detector.Description; } }
		public string OutputFileExtension { get { return _detector.OutputFileExtension; } }
		public IDictionary<string, string[]> Columns { get { return _detector.Columns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return _detector.SupportedFormats; } }
		public Type DetectorType { get { return _detector.GetType(); } }
		#endregion Properties

		internal DetectorWrapper(IDetector detector)
		{
			_detector = detector;
		}

		public int CompareTo(IDetector other)
		{
			return _detector.DetectorType.ToString().CompareTo(other.DetectorType.ToString());
		}

		public override string ToString()
		{
			return string.Format("Wrapped detector: {0}", _detector);
		}

		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			if (dataReader.State != DataReaderState.Ready) return null;

			try
			{
				long startPosition = dataReader.Position;
				IDataBlock dataBlock = _detector.DetectData(dataReader, dataBlockBuilder, context);

				if (dataReader.State == DataReaderState.Cancelled)
				{
					dataReader.Position = dataReader.Length;
				}
				else if (dataReader.Position <= startPosition)
				{
					// Force advance stream reader one byte
					Log.Error(string.Format("Detector {0} did not advance the stream reader position.", _detector.Name));
					dataReader.Position = startPosition + 1;
				}

				return dataBlock;
			}
			catch (OutOfMemoryException)
			{
				// TODO improve out of memory exception handling and remove reference to System.Windows.Forms
				System.Windows.Forms.MessageBox.Show("There is not enough memory available to continue scanning current data block (2); Position: " + dataReader.Position + "; Detector: " + _detector.Name + "; Header count: ?", "Out Of Memory");

				// Skip rest of the stream
				dataReader.Position = dataReader.Length;
				return null;
			}
		}

		public ICollection<IConfigurationItem> Configuration
		{
			get
			{
				IConfigurable configurableWrappedDetector = _detector as IConfigurable;
				if (configurableWrappedDetector == null)
				{
					throw new FrameworkException("Detector wrapped by DetectorWrapper is not configurable");
				}
				return configurableWrappedDetector.Configuration;
			}
		}
	}
}
