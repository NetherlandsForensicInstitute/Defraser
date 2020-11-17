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
using System.IO;
using System.Windows.Forms;
using Defraser.DataStructures;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.GuiCe
{
    public class DefaultCodecHeaderManager : ICodecHeaderSource
	{
		#region PrivateClassDefines

		public class CodecListItem
		{
			private readonly CodecID _codecId;
			private readonly String _referenceName;

			public CodecListItem(CodecID codecId, String referenceName)
			{
				_codecId = codecId;
				_referenceName = referenceName;
			}

			#region Properties
			public CodecID Codec
			{
				get { return _codecId; }
			}
			
			public String ReferenceName
			{
				get { return _referenceName; }
			}
			#endregion Properties
		}

		#endregion PrivateClassDefines

		#region CustomEvents
		public delegate void DefaultCodecHeaderChangeHandler(CodecID changedCodecId, EventArgs e);
		public event DefaultCodecHeaderChangeHandler CodecHeaderChanged;
		#endregion CustomEvents

		private readonly BackgroundFileScanner _backgroundFileScanner;
		private readonly IDetectorFactory _detectorFactory;
		private EditDefaultCodecHeader _editDefaultCodecHeader;
		private readonly Creator<EditDefaultCodecHeader> _editDefaultCodecHeaderCreator;
		private readonly Dictionary<CodecID, IResultNode> _codecHeaderSource;

		public DefaultCodecHeaderManager(BackgroundFileScanner backgroundFileScanner,
										 IDetectorFactory detectorFactory, ProjectManager projectManager, 
										 Creator<EditDefaultCodecHeader> editDefaultCodecHeaderCreator)
		{
			_backgroundFileScanner = backgroundFileScanner;
			_detectorFactory = detectorFactory;
            _editDefaultCodecHeaderCreator = editDefaultCodecHeaderCreator;

			_codecHeaderSource = new Dictionary<CodecID, IResultNode>();
			projectManager.ProjectChanged += ProjectManager_ProjectChanged;
		}

		/// <summary>
		/// This method add the codec types for each available codec format.
		/// Method should be called after the detectfactory has been initalized.
		/// </summary>
		public void Initialize()
		{
			if(_codecHeaderSource.Count > 0) return;

			IList<IDetector> detectors = _detectorFactory.CodecDetectors;
			foreach (IDetector detector in detectors)
			{
				foreach (CodecID codec in detector.SupportedFormats)
				{
					_codecHeaderSource.Add(codec, null);
				}
			}
		}

		public EditDefaultCodecHeader CreateEditDefaultCodecHeaderWindow()
		{
			if (_editDefaultCodecHeader != null) return _editDefaultCodecHeader;

            _editDefaultCodecHeader = _editDefaultCodecHeaderCreator();
			_editDefaultCodecHeader.Closed += new EventHandler(editDefaultCodecHeaderWindow_Closed);
			return _editDefaultCodecHeader;
		}

        public bool IsWindowOpen()
        {
            return (_editDefaultCodecHeader != null);
        }

		public void AddDefaultCodecHeader(CodecID codecId, IResultNode codecHeader)
		{
            _codecHeaderSource[codecId] = codecHeader;
			CodecHeaderChanged(codecId, EventArgs.Empty);
		}

		public void RemoveDefaultCodecHeader(CodecID codecId)
		{
			_codecHeaderSource[codecId] = null;
			CodecHeaderChanged(codecId, EventArgs.Empty);
		}

		public void ClearDefaultCodecHeaders()
		{
			List<CodecID> codecList = new List<CodecID>(_codecHeaderSource.Keys);
			foreach (CodecID codecId in codecList)
			{
				_codecHeaderSource[codecId] = null;
			}
		}

        public IResultNode GetHeaderSourceForCodec(CodecID codecId)
        {
		    IResultNode headerNode;
            _codecHeaderSource.TryGetValue(codecId, out headerNode);
		    return headerNode;
        }

		public List<CodecListItem> GetCodecList()
		{
			List<CodecListItem> codecList = new List<CodecListItem>();
			foreach(KeyValuePair<CodecID, IResultNode> keyPair in _codecHeaderSource)
			{
				IResultNode resultNode = keyPair.Value;
				String referenceName = (resultNode != null && resultNode.InputFile != null) ? new FileInfo(resultNode.InputFile.Name).Name : "None";
				codecList.Add(new CodecListItem(keyPair.Key, referenceName));
			}
			return codecList;
		}
        
		#region EventHandlers
		private void editDefaultCodecHeaderWindow_Closed(object sender, EventArgs e)
		{
			_editDefaultCodecHeader.Closed -= new EventHandler(editDefaultCodecHeaderWindow_Closed);
			_editDefaultCodecHeader = null;
		}
		
		private void ProjectManager_ProjectChanged(object sender, ProjectChangedEventArgs e)
		{
			switch (e.Type)
			{
				case ProjectChangedType.Closed:
					ClearDefaultCodecHeaders();
					break;
				case ProjectChangedType.FileDeleted:
					IInputFile inputFile = e.Item as InputFile;
					if (inputFile != null)
					{
						List<CodecID> toRemoveKeys = new List<CodecID>();
						foreach(KeyValuePair<CodecID, IResultNode> keyPair in _codecHeaderSource)
						{
							if (keyPair.Value != null && keyPair.Value.InputFile == inputFile)
							{
								toRemoveKeys.Add(keyPair.Key);
							}
						}
						foreach (CodecID codecId in toRemoveKeys)
						{
							RemoveDefaultCodecHeader(codecId);
						}
					}
					break;
			}
		}
		#endregion EventHandlers

		public void AskChangeDefaultCodecHeader(FFmpegResult ffmpegResult, IWin32Window windowOwner)
		{
			if (ffmpegResult.Bitmap == null && !IsWindowOpen() && !_backgroundFileScanner.IsBusy)
			{
				// Check if it is a key frame. When true we should be able to decode it.
                IResultNode resultNode = ffmpegResult.SourcePacket;

				if (resultNode != null && resultNode.IsKeyframe())
				{
					if (DialogResult.Yes ==
						MessageBox.Show(
							"Defraser couldn't detect the video headers of this frame. Do you want to use a reference header to decode this frame?",
							"Header Not Found.", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2))
					{
                        EditDefaultCodecHeader editDefaultCodecHeader = CreateEditDefaultCodecHeaderWindow();
						editDefaultCodecHeader.SelectCodec(ffmpegResult.SourcePacket.DataFormat);
						editDefaultCodecHeader.ShowDialog(windowOwner);
					}
				}
			}
		}
	}
}
