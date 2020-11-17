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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Defraser.FFmpegConverter;
using Defraser.GuiCe.Properties;
using Defraser.GuiCe.sourceinfo;
using Defraser.Interface;
using Infralution.Controls.VirtualTree;
using System;

namespace Defraser.GuiCe
{
	public class ProjectKeyframeOverviewRow : IFFmpegCallback
	{
		enum DetectorType
		{
			Container,
			Codec,
			Both
		}

		private readonly ProjectKeyframeOverviewTree _resultsTree;

		private readonly FFmpegResult[] _ffmpegResults;

		private IInputFile _inputFile;
		private int _activeThumbIndex;
		private int _countNotScanned;

		#region Properties
		public IFragment KeyframesSourceFragment
		{
			set;
			get;
		}

		public FileSourceInfo FileSourceInfo
		{
			get
			{
				string notScannedText = CountNotScannedText;
				var result = new FileSourceInfo();
				result.ContainerInfo = ContainerInfo;
				result.InputFileName = InputFileName;
				result.NotScannedText = notScannedText;
				return result;
			}
		}

		private string InputFileName
		{
			get
			{
				return (_inputFile != null) ? new FileInfo(_inputFile.Name).Name : string.Empty;
			}
		}

		/*
		private CodecDetectorInfo DetectorInfo
		{
			get
			{
				var detectorInfo = new CodecDetectorInfo();
				if (KeyframesSourceFragment != null)
				{
					var codecStream = KeyframesSourceFragment as ICodecStream;
					if (codecStream != null)
					{
						// It is a codecstream inside a container.
						IDataBlock datablock = codecStream.DataBlock;

						detectorInfo.Text= string.Format("{0}, from: {1}{2}{3}{4}, to: {5}{6}{7}{8}, length: {9}{10}{11}{12}{13}{14}, length: {15}{16}{17}{18}",
						                     datablock.GetDescriptiveName(),
						                     '{', RtfBold, ToOffsetValue(datablock.StartOffset), '}',
						                     '{', RtfBold, ToOffsetValue(datablock.EndOffset), '}',
						                     '{', RtfBold, ToOffsetValue(datablock.Length), '}',
						                     RtfNewLine,
						                     KeyframesSourceFragment.GetDescriptiveName(),
						                     '{', RtfBold, ToOffsetValue(KeyframesSourceFragment.Length), '}');
						return detectorInfo;

					}
					detectorInfo.Text=string.Format("{0}, from: {1}{2}{3}{4}, to: {5}{6}{7}{8}, length: {9}{10}{11}{12}",
					                     KeyframesSourceFragment.GetDescriptiveName(),
					                     '{', RtfBold, ToOffsetValue(KeyframesSourceFragment.StartOffset), '}',
					                     '{', RtfBold, ToOffsetValue(KeyframesSourceFragment.EndOffset), '}',
					                     '{', RtfBold, ToOffsetValue(KeyframesSourceFragment.Length), '}');
					return detectorInfo;
				}
				return detectorInfo;
			}
		}
		*/

		private static ISourceInfo Describe(IFragment fragment) //TODO: move to member method of Fragment
		{
			var codecStream = fragment as ICodecStream;
			var containerInfo = new CodecStreamContainerInfo();
			if (codecStream != null)
			{
				// It is a codecstream inside a container.
				containerInfo.DataBlock = Describe(codecStream.DataBlock);
			}
			containerInfo.Name = fragment.GetDescriptiveName();
			containerInfo.StartOffset = fragment.StartOffset;
			containerInfo.EndOffset = fragment.EndOffset;
			containerInfo.Length = fragment.Length;
			return containerInfo;
		}

		private ISourceInfo ContainerInfo
		{
			get
			{
				if (KeyframesSourceFragment != null)
				{
					return Describe(KeyframesSourceFragment);
				}
				return new UnknownContainerInfo();
			}
		}

		public string CountNotScannedText
		{
			get
			{
				if (_countNotScanned <= 0) return string.Empty;
				return string.Format("{0} video stream{1} not scanned in this container.", _countNotScanned,
									 (_countNotScanned > 1) ? "s are" : " is");
			}
		}

		public FFmpegResult Result1 { get { return _ffmpegResults[0]; } }
		public FFmpegResult Result2 { get { return _ffmpegResults[1]; } }
		public FFmpegResult Result3 { get { return _ffmpegResults[2]; } }
		public FFmpegResult Result4 { get { return _ffmpegResults[3]; } }
		public FFmpegResult Result5 { get { return _ffmpegResults[4]; } }

		public IInputFile InputFile
		{
			get { return _inputFile; }
			set
			{
				_inputFile = value;
				_activeThumbIndex = 0;
			}
		}
		#endregion Properties

		public ProjectKeyframeOverviewRow(ProjectKeyframeOverviewTree resultsTree)
		{
			_resultsTree = resultsTree;
			_ffmpegResults = new FFmpegResult[5];
		}

		~ProjectKeyframeOverviewRow()
		{
			foreach (FFmpegResult result in _ffmpegResults)
			{
				if (result != null) result.Dispose();
			}
		}

		public FFmpegResult GetThumbResult(int index)
		{
			return _ffmpegResults[index];
		}

		private void UpdateThumbResult(int thumbIndex, FFmpegResult result)
		{
			_ffmpegResults[thumbIndex] = result;
			UpdateRow();
		}

		public void FFmpegDataConverted(FFmpegResult ffmpegResult)
		{
			if (_activeThumbIndex >= _ffmpegResults.Length)
			{
				// FIXME: ewald@holmes.nl - Needs real fix: WHY does it report more than 5 thumbnails??
				Console.WriteLine("WARNING: Dropping unexpected extra thumbnail!");
				return;
			}

			UpdateThumbResult(_activeThumbIndex, ffmpegResult);
			_activeThumbIndex++;
		}

		public void WarnMoreCodecStreams(int countNotScanned)
		{
			_countNotScanned = countNotScanned;
			UpdateRow();
		}

		public void HideMoreCodecStreamsWarning()
		{
			_countNotScanned = 0;
			UpdateRow();
		}

		private void UpdateRow()
		{
			// Update the row in the tree.
			// This is executed in context of the main thread.
			if (_resultsTree == null || _resultsTree.IsDisposed) return;
			_resultsTree.Invoke((MethodInvoker)delegate
			{
				Row row = _resultsTree.FindRow(this);
				if (row != null)
				{
					_resultsTree.UpdateRowData(row);
				}
			});
		}

		private static IDetector GetDetectorWithType(IEnumerable<IDetector> detectors, DetectorType type)
		{
			foreach (IDetector detector in detectors)
			{
				if ((type == DetectorType.Both) || ((detector is ICodecDetector) == (type == DetectorType.Codec)))
				{
					return detector;
				}
			}
			return null;
		}
	}
}
