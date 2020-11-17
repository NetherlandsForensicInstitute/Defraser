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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Defraser.Interface;
using Defraser.Util;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	class FileDetails : DetailsBase
	{
		override internal string Title { get { return "File Details"; } }
		override internal string Column0Caption { get { return "Detail"; } }
		override internal string Column1Caption { get { return "Description"; } }

		override internal IList GetChildrenForRow(Row row)
		{
			const string Detectors = "Detectors";

			IInputFile inputFile = row.Item as IInputFile;
			Pair<string, string> pair = row.Item as Pair<string, string>;

			if (inputFile != null)
			{
				ArrayList details = new ArrayList();
				details.Add(new Pair<string, string>("File Size", string.Format("{0:0,0} bytes", inputFile.Length)));
				details.Add(new Pair<string, string>("Scan Duration", FormatTimeSpan(GetScanDuration(inputFile))));
				details.Add(new Pair<string, string>("Performance", string.Format("{0:0.00} MB/s", GetScanRate(inputFile))));
				details.Add(new Pair<string, string>(Detectors, string.Empty));
				//details.Add(new Pair<string, string>("MD5 checksum", inputFile.MD5Checksum)); Release 0.7.0
				return details;
			}

			if (pair == null) return null;

			IList<Pair<string, string>> detectorDetails = new List<Pair<string, string>>();
			inputFile = row.ParentRow.Item as IInputFile;

			if ((pair.First == Detectors) && (inputFile != null))
			{
				foreach (IDetector detector in inputFile.Project.GetDetectors(inputFile))
				{
					detectorDetails.Add(new Pair<string, string>("Detector", detector.Name));
				}
			}
			return new ReadOnlyCollection<Pair<string, string>>(detectorDetails);
		}

		private static string FormatTimeSpan(TimeSpan timeSpan)
		{
			// TODO: Use 'TimeSpan.Tostring(...)' when switching to .NET 4.0
			return new DateTime(timeSpan.Ticks).ToString("HH:mm:ss.fff");
		}

		private static TimeSpan GetScanDuration(IInputFile inputFile)
		{
			return inputFile.Project.GetScanDuration(inputFile);
		}

		private static double GetScanRate(IInputFile inputFile)
		{
			double fileSizeInMbs = (double) inputFile.Length/(1024*1024);
			double seconds = 0.001 * GetScanDuration(inputFile).TotalMilliseconds;
			return fileSizeInMbs / seconds;
		}
	}
}
