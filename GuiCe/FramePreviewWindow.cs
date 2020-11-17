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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Defraser.DataStructures;
using Defraser.FFmpegConverter;
using Defraser.Interface;

namespace Defraser.GuiCe
{
    public partial class FramePreviewWindow : ToolWindow
    {
        private FFmpegResult _ffmpegResult;
    	private readonly IForensicIntegrityLog _forensicIntegrityLog;

		public FramePreviewWindow(IForensicIntegrityLog forensicIntegrityLog)
        {
			_forensicIntegrityLog = forensicIntegrityLog;

            InitializeComponent();
            UpdateTitle();
        	DoubleBuffered = true;
			UpdateResult(null);
        }

		public void UpdateResult(FFmpegResult ffmpegResult)
		{
            if (IsDisposed) return;

            if (_ffmpegResult != null) _ffmpegResult.Dispose();
            _ffmpegResult = ffmpegResult;

            if(_ffmpegResult != null)
            {
                Invoke((MethodInvoker) UpdateTitle);
            }

            Invalidate(); // Causes bitmap redraw (OnPaint).
		}

        private void UpdateTitle()
        {
            Text = "Frame Preview";

            if(_ffmpegResult != null && _ffmpegResult.IsUsingCustomHeaderSource())
            {
                Text += " (Using Default Codec Header)";
            }
        }

    	protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
			g.Clear(Color.Black);

            if (_ffmpegResult != null && _ffmpegResult.Bitmap != null)
            {
                Bitmap bitmap = _ffmpegResult.Bitmap;
                float containerWidth = Width;
                float containerHeight = Height;

                float bitmapWidth = bitmap.Width;
                float bitmapHeight = bitmap.Height;

                float scaleX = containerWidth / bitmapWidth;
                float scaleY = containerHeight / bitmapHeight;

                float scale;
                int x = 0;
                int y = 0;
                if (scaleX > scaleY)
                {
                    scale = scaleY;
                    x = (int)(containerWidth / 2.0 - scale * bitmapWidth / 2.0);
                }
                else
                {
                    scale = scaleX;
                    y = (int)(containerHeight / 2.0 - scale * bitmapHeight / 2.0);
                }

                int width = (int)(scale * bitmapWidth);
                int height = (int)(scale * bitmapHeight);
                g.DrawImage(bitmap, x, y, width, height);

                if(_ffmpegResult.IsUsingCustomHeaderSource())
                {
                    var pen = new Pen(ThumbUtil.ColorBitmapUsingHeaderSource);
                    g.DrawRectangle(pen, x + 1, y + 1, width - 3, height - 3);
                    g.DrawRectangle(pen, x, y, width - 1, height - 1);
                    pen.Dispose();
                }
            }
            base.OnPaint(e);
        }

        private void WindowResize(object sender, EventArgs e)
        {
            Invalidate();
        }

		private void SaveCurrentBitmapToFile(string filePath, ImageFormat format, bool createForensicIntegrityLog)
		{
            if (_ffmpegResult != null && _ffmpegResult.SourcePacket != null && _ffmpegResult.Bitmap != null)
			{
                _ffmpegResult.Bitmap.Save(filePath, format);

				if (createForensicIntegrityLog)
				{
                    if (_ffmpegResult.SourcePacket != null)
					{
                        IResultNode headerSourceResult = _ffmpegResult.HeaderSource ?? _ffmpegResult.SourcePacket;
						ICodecDetector codecDetector = headerSourceResult.Detectors.FirstOrDefault() as ICodecDetector;
						if (codecDetector == null) return;
						// FIXME: The detectors of the video headers cannot be determined (for forensic logging purposes)!!
						IDataPacket usedBitmapHeaders = codecDetector.GetVideoHeaders(headerSourceResult);

						string logFileName = string.Format("{0}.csv", filePath);
						using (FileStream fs = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
						{
							IDataPacket allUsedBitmapNodes = _ffmpegResult.SourcePacket;
							// Prefix with bitmap headers (if any)
							if (usedBitmapHeaders != null)
							{
								allUsedBitmapNodes = usedBitmapHeaders.Append(allUsedBitmapNodes);
							}
							// Append child nodes and determine detectors that were used
							var detectors = new HashSet<IDetector>(_ffmpegResult.SourcePacket.Detectors);
							foreach (var dataPacket in _ffmpegResult.SourcePacket.Children)
							{
								allUsedBitmapNodes = allUsedBitmapNodes.Append(dataPacket);
								foreach (IDetector detector in dataPacket.Detectors)
								{
									detectors.Add(detector);
								}
							}
							_forensicIntegrityLog.Log(allUsedBitmapNodes, detectors, filePath, fs, ForensicLogType.ConvertedData);
						}
					}
					else
					{
						MessageBox.Show("Can't save integrity log. There is no packetdata available for the active bitmap.", "Save Integrity Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			else
			{
				MessageBox.Show("There is no frame opened, can't save empty frame to image.", "Save Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
            saveFrameAsImageToolStripMenuItem.Enabled = (_ffmpegResult != null && _ffmpegResult.Bitmap != null);
		}

		private void saveFrameAsImage_Click(object sender, EventArgs e)
		{
			var fileDialog = new SaveFilePresenter();
			fileDialog.FilterImages();
            fileDialog.FilterImagesIndex();
			fileDialog.Title = "Save frame as Image";
			FileInfo imageFile;
			if (fileDialog.ShowDialog(this, out imageFile) )
			{
				SaveCurrentBitmapToFile(imageFile.FullName, GetImageFormatFromFileInfo(imageFile), SaveFilePresenter.ExportForensicIntegrityLog);
			}
		}

    	private static ImageFormat GetImageFormatFromFileInfo(FileSystemInfo imageFile)
    	{
    		switch (imageFile.Extension.ToLower())
    		{
				case ".jpg":
				case ".jpeg":
					return ImageFormat.Jpeg;
				case ".png":
					return ImageFormat.Png;
				case ".bmp":
					return ImageFormat.Bmp;
			}

			// Use PNG as default format
    		return ImageFormat.Png;
    	}
    }
}
