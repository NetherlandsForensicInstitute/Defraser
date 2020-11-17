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
using System.Xml;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	public class ExportToXml // TODO : IExportStrategy<IEnumerable<IDataPacket>>
	{
		internal class NameValuePair
		{
			private readonly string _name;
			private readonly string _value;

			internal string Name { get { return _name; } }
			internal string Value { get { return _value; } }

			internal NameValuePair(string name, string value)
			{
				_name = name;
				_value = value;
			}
		}

		private readonly Creator<TextWriter, string> _createTextWriter;
		private readonly DataBlockScanner _dataBlockScanner;
		private readonly IDetectorFormatter _detectorFormatter;

		// The current number of exported bytes
		private long _handledBytes;
		// The total number of bytes to export
		private long _totalBytes;

		#region Properties
		public static string ApplicationName
		{
			get
			{
				if (Assembly.GetEntryAssembly() == null) return "<application name>"; // Loaded from unmanaged application
				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static string ApplicationVersion
		{
			get
			{
				if (Assembly.GetEntryAssembly() == null) return "<version>"; // Loaded from unmanaged application

				// Return only the first 3 digits of the version number
				string fourDigitVersionString = Assembly.GetEntryAssembly().GetName().Version.ToString();
				string threeDigitVersionString = fourDigitVersionString.Substring(0, fourDigitVersionString.LastIndexOf('.'));
				return threeDigitVersionString;
			}
		}
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="ExportToXml"/> strategy.
		/// </summary>
		/// <param name="createTextWriter">The factory method for creating a text writer</param>
		public ExportToXml(Creator<TextWriter, string> createTextWriter, DataBlockScanner dataBlockScanner, IDetectorFormatter detectorFormatter)
		{
			PreConditions.Argument("createTextWriter").Value(createTextWriter).IsNotNull();
			PreConditions.Argument("dataBlockScanner").Value(dataBlockScanner).IsNotNull();

			_createTextWriter = createTextWriter;
			_dataBlockScanner = dataBlockScanner;
			_detectorFormatter = detectorFormatter;
		}

		/// <summary>
		/// Start the export to XML.
		/// The export process is done using a background worker.
		/// This gives the posibility to show progress and cancel the export any moment.
		/// </summary>
		/// <param name="resultTreeList">used to </param>
		/// <param name="fragments">The fragments to export</param>
		/// <param name="xmlExportFileName">The filename to export the detectables to</param>
		public void Export(IEnumerable<IFragment> fragments, IDataReaderPool dataReaderPool, string filePath, IProgressReporter progressReporter)
		{
			PreConditions.Argument("fragments").Value(fragments).IsNotNull().And.IsNotEmpty().And.DoesNotContainNull();
			PreConditions.Argument("dataReaderPool").Value(dataReaderPool).IsNotNull();
			PreConditions.Argument("filePath").Value(filePath).IsNotNull().And.IsNotEmpty();
			PreConditions.Argument("progressReporter").Value(progressReporter).IsNotNull();

			// Create the XML document
			XmlDocument xmlDocument = new XmlDocument();

			// Create the root element and add it to the document
			List<NameValuePair> infoNameValuePairs = new List<NameValuePair>();
			infoNameValuePairs.Add(new NameValuePair("CreatedWith", ApplicationName));
			infoNameValuePairs.Add(new NameValuePair("Version", ApplicationVersion));
			infoNameValuePairs.Add(new NameValuePair("User", Environment.UserName));
			DateTime currentDateTime = DateTime.Now;
			infoNameValuePairs.Add(new NameValuePair("Date", currentDateTime.ToLongDateString()));
			infoNameValuePairs.Add(new NameValuePair("Time", currentDateTime.ToLongTimeString()));
			XmlElement root = CreateElement(xmlDocument, "FileInfo", infoNameValuePairs.ToArray());

			xmlDocument.AppendChild(root);

			// The progress must be measured and reported to the user;
			// for that the total number of bytes to export is counted.
			_totalBytes = fragments.Sum(x => x.Length);

			foreach (IFragment fragment in fragments)
			{
				Export(root, fragment, dataReaderPool, progressReporter);

				if (progressReporter.CancellationPending)
				{
					break;
				}
			}

			// If the operation was canceled by the user,
			// set the DoWorkEventArgs.Cancel property to true.
			// This property is used in the RunWorkerCompleted method.
			if (!progressReporter.CancellationPending)
			{
				using (TextWriter xmlExportWriter = _createTextWriter(filePath))
				{
					// Wait on completion of asynchronous method above before executing this method
					xmlDocument.Save(xmlExportWriter);
				}
			}
		}

		private void Export(XmlNode parent, IFragment fragment, IDataReaderPool dataReaderPool, IProgressReporter progressReporter)
		{
			if (fragment == null) return;

			IResultNode results = _dataBlockScanner.GetResults(fragment, new NullProgressReporter(), dataReaderPool);

			if (results == null)
			{
				// No results were found in the data block
				// Add the bytes from this data block to the _byteCount variable
				// to keep the progress bar correct.
				_handledBytes += fragment.Length;
				progressReporter.ReportProgress(_totalBytes == 0 ? 0 : (int)((_handledBytes * 100) / _totalBytes));
				return;
			}

			List<NameValuePair> detectableNameValuePairs = new List<NameValuePair>();
			detectableNameValuePairs.Add(new NameValuePair("File", fragment.InputFile.Name));
			if (fragment.Detectors != null)
			{
				foreach (IDetector detector in fragment.Detectors)
				{
					detectableNameValuePairs.Add(new NameValuePair(_detectorFormatter.DetectorType(detector), detector.Name));
					detectableNameValuePairs.Add(new NameValuePair(_detectorFormatter.DetectorType(detector)+"Version",_detectorFormatter.FormatVersion(detector)));
				}
			}
			detectableNameValuePairs.Add(new NameValuePair("Offset", fragment.StartOffset.ToString()));
			detectableNameValuePairs.Add(new NameValuePair("Length", fragment.Length.ToString()));

			string nodeName = "UnknownContainerOrCodec";
			if (fragment is IDataBlock)
			{
				nodeName = "Container";
			}
			else
			{
				nodeName = "Codec";
			}
			XmlElement fragmentElement = CreateElement(parent.OwnerDocument, nodeName, detectableNameValuePairs.ToArray());
			parent.AppendChild(fragmentElement);

			Export(fragmentElement, results.Children, progressReporter);
		}

		// Create an xml document from the results
		private void Export(XmlNode parent, ICollection<IResultNode> results, IProgressReporter progressReporter)
		{
			if (parent == null) throw new ArgumentNullException("parent");
			if (results == null) throw new ArgumentNullException("results");

			long currentValue = 0L;

			foreach (IResultNode result in results)
			{
				if (progressReporter.CancellationPending) return;

				_handledBytes += result.Length;

				progressReporter.ReportProgress(_totalBytes == 0 ? 0 : (int)((_handledBytes * 100) / _totalBytes));

				List<NameValuePair> resultNameValuePairs = new List<NameValuePair>();
				resultNameValuePairs.Add(new NameValuePair("Name", result.Name));
				if (result.InputFile != null)
				{
					resultNameValuePairs.Add(new NameValuePair("Offset", result.StartOffset.ToString()));
					//resultNameValuePairs.Add(new NameValuePair("OffsetHex", result.DataPacket.Offset.ToString("X")));
				}
				resultNameValuePairs.Add(new NameValuePair("Length", result.Length.ToString()));
				//resultNameValuePairs.Add(new NameValuePair("LengthHex", result.Length.ToString("X")));
				XmlElement resultElement = CreateElement(parent.OwnerDocument, "Result", resultNameValuePairs.ToArray());

				if (result.Attributes != null && result.Attributes.Count > 0)
				{
					XmlElement attributesElement = parent.OwnerDocument.CreateElement("Attributes");

					foreach (IResultAttribute attribute in result.Attributes)
					{
						XmlElement attributeElement = CreateElement(parent.OwnerDocument, "Attribute",
						                                            new NameValuePair[] {
						                                                                	new NameValuePair("Name", attribute.Name),
						                                                                	new NameValuePair("Value", attribute.ValueAsString) });
						attributesElement.AppendChild(attributeElement);
					}
					resultElement.AppendChild(attributesElement);
				}
				parent.AppendChild(resultElement);

				if (result.Children.Count > 0)
				{
					Export(resultElement, result.Children, progressReporter);
				}
				currentValue++;
			}
		}

		private XmlElement CreateElement(XmlDocument document, string nodeName, NameValuePair[] nameValuePairs)
		{
			XmlElement element = document.CreateElement(nodeName);
			foreach (NameValuePair nameValuePair in nameValuePairs)
			{
				element.SetAttribute(nameValuePair.Name, nameValuePair.Value);
			}
			return element;
		}
	}
}
