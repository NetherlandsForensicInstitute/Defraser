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
using System.ComponentModel;
using System.Windows.Forms;
using Defraser.Interface;
using Defraser.DataStructures;

namespace Defraser.GuiCe
{
	public partial class SearchHeader : Form
	{
		private enum SearchMode
		{
			DataBlock,
			CodecStream
		}

	    private readonly DefaultCodecHeaderManager _defaultCodecHeaderManager;
		private readonly BackgroundDataBlockScanner _backgroundDataBlockScanner;

		private SearchMode _activeSearchMode;
		private IList<ICodecStream> _searchCodecStreams;
		private int _currentCodecStreamIndex;

		public SearchHeader(DefaultCodecHeaderManager defaultCodecHeaderManager,
			BackgroundDataBlockScanner backgroundDataBlockScanner)
		{
		    _defaultCodecHeaderManager = defaultCodecHeaderManager;
			_backgroundDataBlockScanner = backgroundDataBlockScanner;
			InitializeComponent();
		}

		public bool RunSearchHeader(IInputFile inputFile, CodecID codecId)
		{
			IProject project = inputFile.Project;
			IList<IDataBlock> dataBlocks = project.GetDataBlocks(inputFile);

			if (dataBlocks.Count > 0)
			{
				foreach (IDataBlock dataBlock in dataBlocks)
				{
					if (DataBlockHasCodec(dataBlock, codecId))
					{
						if (dataBlock.CodecStreams.Count > 0)
						{
							return SearchBlockOrStream(dataBlock.CodecStreams, SearchMode.CodecStream);
						}
						else
						{
							return SearchBlockOrStream(dataBlock, SearchMode.DataBlock);
						}
					}
				}

				MessageBox.Show(this, "Selected codec stream was not found in file. Please select a different header source.",
					"Couldn't find detector.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			else
			{
				MessageBox.Show(this, "No datablocks are available. Please select a different header source.",
					"Couldn't detect keyframe.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			return false;
		}

		private bool SearchBlockOrStream(IList<ICodecStream> streams, SearchMode searchMode)
		{
			if (streams != null && _currentCodecStreamIndex < streams.Count)
			{
				_searchCodecStreams = streams;
				_activeSearchMode = searchMode;

			    DetectResultsInFragment(_searchCodecStreams[_currentCodecStreamIndex]);
                _currentCodecStreamIndex++;
				return true;
			}
			ShowSearchStartErrorMessage();
			return false;
		}

		private bool SearchNextStream()
		{
			return SearchBlockOrStream(_searchCodecStreams, SearchMode.CodecStream);
		}

		private bool SearchBlockOrStream(IDataBlock dataBlock, SearchMode searchMode)
		{
			if (dataBlock != null)
			{
				_activeSearchMode = searchMode;

				DetectResultsInFragment(dataBlock);
				return true;
			}
			ShowSearchStartErrorMessage();
			return false;
		}
		
		private void ShowSearchStartErrorMessage()
		{
			MessageBox.Show(this, "Defraser couldn't detect a keyframe in this file or stream. Please select a different header source.",
				"Couldn't detect keyframe.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
		}

		private void DetectResultsInFragment(IFragment fragment)
		{
			// Stop scan, only when a scan is still active.
			_backgroundDataBlockScanner.Stop();

			// Start scanning the header source file for results.
			_backgroundDataBlockScanner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(_backgroundDataBlockScanner_RunWorkerCompleted);
			_backgroundDataBlockScanner.Scan(fragment);
		}

		private void FindKeyframeInResults(IResultNode node)
		{
			IResultNode keyframeNode = RecursiveKeyframeLookup(node);

			if(keyframeNode != null)
			{
				_defaultCodecHeaderManager.AddDefaultCodecHeader(node.DataFormat, keyframeNode);
				//MessageBox.Show(this, "The header was found in the source file or stream. It will be used during the next frame decodes.", 
				//	"Header found", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Close();
			}
			else if (_activeSearchMode == SearchMode.CodecStream)
			{
				SearchNextStream();
			}
		}

		#region EventHandlers
		private void _backgroundDataBlockScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_backgroundDataBlockScanner.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(_backgroundDataBlockScanner_RunWorkerCompleted);
            if (e.Cancelled) return;

			if (e.Error != null)
			{
				MessageBox.Show(this, "Header source scan failed.",
					e.Error.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			else
			{
				FindKeyframeInResults(e.Result as IResultNode);
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
		    buttonCancel.Enabled = false;
		    buttonCancel.Text = "Cancelling...";

			_backgroundDataBlockScanner.Stop();
			Close();
		}
		#endregion EventHandlers

		#region static
		private static bool DataBlockHasCodec(IDataBlock dataBlock, CodecID codecId)
		{
			if(dataBlock.DataFormat == codecId)
				return true;

			foreach(ICodecStream codecStream in dataBlock.CodecStreams)
			{
				if(codecStream.DataFormat == codecId)
					return true;
			}
			return false;
		}
		private static IResultNode RecursiveKeyframeLookup(IResultNode node)
		{
			if (node.IsKeyframe()) return node;

			foreach (IResultNode childNode in node.Children)
			{
				IResultNode childResult = RecursiveKeyframeLookup(childNode);
				if (childResult != null) return childResult;
			}
			return null;
		}
		#endregion static
	}
}
