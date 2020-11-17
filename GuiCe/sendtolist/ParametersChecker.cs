/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Linq;
using System.Text;
using Defraser.Interface;
using Defraser.Util;
using System.IO;

namespace Defraser.GuiCe.sendtolist
{
	public sealed class ParametersChecker
	{

		private IForensicIntegrityLog _forensicIntegretyLog;

		public ParametersChecker(IForensicIntegrityLog forensicIntegrityLog)
		{
			PreConditions.Argument("forensicIntegrityLog").Value(forensicIntegrityLog).IsNotNull();

			_forensicIntegretyLog = forensicIntegrityLog;
		}

		private enum PNAME { DAT, LOG, SRC, COUNT, OFFS, OFFE, SIZES, SIZEH, SIZET, LISTS, LISTO, DET };
		private Dictionary<PNAME, string> validParms = new Dictionary<PNAME, string>() {
            {PNAME.DAT, "[DAT]"},
            {PNAME.LOG, "[LOG]"},
            {PNAME.SRC, "[SRC]"},
            {PNAME.COUNT, "[COUNT]"},
            {PNAME.OFFS, "[OFFS]"},
            {PNAME.OFFE,"[OFFE]"},
            {PNAME.SIZES, "[SIZES]"},
            {PNAME.SIZEH, "[SIZEH]"},
            {PNAME.SIZET, "[SIZET]"},
            {PNAME.LISTS, "[LISTS]"},
            {PNAME.LISTO, "[LISTO]"},
            {PNAME.DET, "[DET]"}
        };

		/// <summary>
		/// Substitutes the parameters string with the given values in selection, dataPacket and outputFileName.
		/// </summary>
		/// <param name="parameters">The parameters string</param>
		/// <param name="selection">The current selection</param>
		/// <param name="dataPacket">The dataPacket for this selection</param>
		/// <param name="outputFileName">The name of the output-file</param>
		/// <returns>The substituted parameters string</returns>
		public string Substitute(string parameters, ISelection selection, IDataPacket dataPacket, string outputFileName)
		{
			PreConditions.Argument("outputFileName").Value(outputFileName).IsNotEmpty();

			StringBuilder sb = new StringBuilder(parameters);

			// [DAT] = Full Path to Temp File with selected headers (which is created in temp folder)
			sb.Replace(validParms[PNAME.DAT], string.Format("\"{0}\"", outputFileName));

			//[LOG] = Full Path to Log File (which is created in temp folder)
			if (parameters.Contains(validParms[PNAME.LOG]))
			{
				WriteForensicLog(dataPacket, selection, outputFileName);
				string logFileName = string.Format("{0}.csv", outputFileName);
				sb.Replace(validParms[PNAME.LOG], string.Format("\"{0}\"", logFileName));
			}

			//[SRC] = Full Path to the Source File
			if (parameters.Contains(validParms[PNAME.SRC]))
			{
				string pathToSource = dataPacket.InputFile.Name;
				sb.Replace(validParms[PNAME.SRC], string.Format("\"{0}\"", pathToSource));
			}

			// [COUNT] = Total number of Selected Headers
			if (parameters.Contains(validParms[PNAME.COUNT]))
			{
				string count = countNodes(selection.Results).ToString();
				sb.Replace(validParms[PNAME.COUNT], count);
			}

			//[OFFS]= Offset of the Start of the Selection in the Source File
			if (parameters.Contains(validParms[PNAME.OFFS]))
			{
				string offsetStart = dataPacket.StartOffset.ToString();
				sb.Replace(validParms[PNAME.OFFS], offsetStart);
			}

			//[OFFE]= Offset of the End of the Selection in the Source File
			if (parameters.Contains(validParms[PNAME.OFFE]))
			{
				string offsetEnd = dataPacket.EndOffset.ToString();
				sb.Replace(validParms[PNAME.OFFE], offsetEnd);
			}

			//[SIZES]= Size of the Source File
			if (parameters.Contains(validParms[PNAME.SIZES]))
			{
				string sizeSource = dataPacket.InputFile.Length.ToString();
				sb.Replace(validParms[PNAME.SIZES], sizeSource);
			}

			//[SIZEH]= Size of the first header of the selection
			if (parameters.Contains(validParms[PNAME.SIZEH]) && selection.Results.Length > 0)
			{
				string sizeFirstHeader = selection.Results[0].Length.ToString();
				sb.Replace(validParms[PNAME.SIZEH], sizeFirstHeader);
			}

			//[SIZET] = Total Size of the Selected Headers
			if (parameters.Contains(validParms[PNAME.SIZET]))
			{
				string sizeSelectedHeaders = dataPacket.Length.ToString();
				sb.Replace(validParms[PNAME.SIZET], sizeSelectedHeaders);
			}

			//[LISTS] = List of Sizes of each Selected Header. Values are separated by spaces. So, this will be formatted as SIZE1 SIZE2 etc.
			if (parameters.Contains(validParms[PNAME.LISTS]))
			{
				StringBuilder sizeListSb = new StringBuilder();
				SizeList(selection.Results, sizeListSb);
				sb.Replace(validParms[PNAME.LISTS], sizeListSb.ToString());
			}

			//[LISTO]   = List of Offsets (in Source File) and sizes of each selected header. Values are separated by spaces. So, this will be formatted as OFFSET1 SIZE1 OFFSET2 SIZE2 etc.
			if (parameters.Contains(validParms[PNAME.LISTO]))
			{
				StringBuilder sizeAndOffsetSb = new StringBuilder();
				SizeAndOffsetList(selection.Results, sizeAndOffsetSb);
				sb.Replace(validParms[PNAME.LISTO], sizeAndOffsetSb.ToString());
			}

			//[DET] = Name of the relevant detector:
			if (parameters.Contains(validParms[PNAME.DET]))
			{
				sb.Replace(validParms[PNAME.DET], DetectorString(selection.Results));
			}
			return sb.ToString();
		}


		private int countNodes(IList<IResultNode> nodes)
		{
			int count = nodes.Count;
			foreach (IResultNode n in nodes)
			{
				count += countNodes(n.Children);
			}
			return count;
		}

		/// <summary>
		/// Checks the validity of the parameters string.
		/// </summary>
		/// <param name="parameters">The parameters string to check</param>
		/// <returns>True if the parameters string was valid, false otherwise</returns>
		public bool IsValidParameterString(string parameters)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].Equals('['))
				{
					if (i > 0 && parameters[i - 1].Equals('\\'))
					{
						continue; // ignore \[
					}
					int foundAt = parameters.IndexOf("]", i);
					if (foundAt == -1)
					{
						return false; // ] no matching ] found
					}
					string sub = parameters.Substring(i, (foundAt - i) + 1);
					if (!IsValidParameter(sub))
					{
						return false;
					}
					else
					{
						i = foundAt;
					}
				}
			}
			return true;
		}

		private bool IsValidParameter(string param)
		{
			foreach (string s in validParms.Values)
			{
				if (s.Equals(param))
				{
					return true;
				}
			}
			return false;
		}

		private void WriteForensicLog(IDataPacket dataPacket, ISelection selection, string outputFileName)
		{
			string logFileName = string.Format("{0}.csv", outputFileName);

			using (FileStream fs = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				_forensicIntegretyLog.Log(dataPacket, selection.Results[0].Detectors, outputFileName, fs, ForensicLogType.CopiedData);
			}
		}
		private void SizeList(IList<IResultNode> nodes, StringBuilder sb)
		{
			foreach (IResultNode node in nodes)
			{
				if (sb.Length == 0)
				{
					// the first element isn't preceded with a space
					sb.Append(node.Length);
				}
				else
				{
					sb.Append(" " + node.Length);
				}
				SizeList(node.Children, sb);
			}
		}

		private void SizeAndOffsetList(IList<IResultNode> nodes, StringBuilder sb)
		{
			foreach (IResultNode node in nodes)
			{
				if (sb.Length == 0)
				{
					// the first element isn't preceded with a space
					sb.Append(node.StartOffset + " " + node.Length);
				}
				else
				{
					sb.Append(" " + node.StartOffset + " " + node.Length);
				}
				SizeAndOffsetList(node.Children, sb);
			}
		}

		private string DetectorString(IList<IResultNode> nodes)
		{
			HashSet<IDetector> detectors = new HashSet<IDetector>();
			CollectDetectors(nodes, detectors);

			if (detectors.Count == 1)
				return detectors.First<IDetector>().Name;

			StringBuilder sb = new StringBuilder();
			foreach (ICodecDetector detector in detectors.Where(d => d is ICodecDetector))
			{
				String formattedStr = string.Format("\"{0}\"", detector.Name);
				if (sb.Length == 0)
				{
					// the first element isn't preceded with a space
					sb.Append(formattedStr);
				}
				else
				{
					sb.Append(" " + formattedStr);
				}
			}
			return sb.ToString();
		}


		private void CollectDetectors(IList<IResultNode> nodes, HashSet<IDetector> detectorStrings)
		{
			foreach (IResultNode node in nodes)
			{
				foreach (IDetector detector in node.Detectors)
				{
					detectorStrings.Add(detector);
				}
				CollectDetectors(node.Children, detectorStrings);
			}
		}
	}
}
