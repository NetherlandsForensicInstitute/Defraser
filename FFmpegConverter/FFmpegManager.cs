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
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.FFmpegConverter
{
    public class FFmpegManager : IDisposable
    {
        #region Attributes
		private System.Collections.Generic.Queue<FFmpegTask> _queue;
        private Thread _readThread;
        private readonly ICodecHeaderSource _codecHeaderSource;
        private FrameConverter _frameConverter;
        private int _queueId;
		#endregion Attributes

		#region Properties
		public FrameConverter FrameConvertor { get { return _frameConverter; } }
    	#endregion Properties

		public FFmpegManager(FrameConverter frameConverter, ICodecHeaderSource codecHeaderSource)
        {
			_frameConverter = frameConverter;
			_codecHeaderSource = codecHeaderSource;

            _queue = new Queue<FFmpegTask>();
            _queueId = 0;

            _readThread = new Thread(ReadConvertQueue);
            _readThread.Name = "FFmpegConverter";
            _readThread.Start();

            // Load the FFmpeg module (DDL's)
        	_frameConverter.RegisterFFmpeg();
        }

        public void NewQueue()
        {
            if (_queue == null) return;
            ClearQueue();
            _queueId++;
        }

        public void ClearQueue()
        {
            lock (this)
            {
				// List of tasks that should be re-add after the queue is cleared.
				// This are tasks that are not allowed to be cancelled.
				List<FFmpegTask> addAgainAfterClear = new List<FFmpegTask>();

				// Clear all task references and empty queue.
                while (_queue.Count > 0)
                {
                    FFmpegTask task = _queue.Dequeue();

					if (task.ForceCompletion)
					{
						addAgainAfterClear.Add(task);
					}
                }
                _queue.Clear();

				// Add the "forced to complete" items again to the queue.
            	foreach (FFmpegTask task in addAgainAfterClear)
				{
					_queue.Enqueue(task);
            	}
			}
        }

		public void AddToConvertQueue(IResultNode resultNode, IFFmpegCallback callbackObj)
		{
			AddToConvertQueue(resultNode, callbackObj, false);
		}

    	public void AddToConvertQueue(IResultNode resultNode, IFFmpegCallback callbackObj, bool forceCompletion)
        {
            if (_queue == null) return;
            
            // add element to be converted to queue
            // call is thread safe
			FFmpegTask task = new FFmpegTask { QueueId = _queueId, DataPacket = resultNode, Callback = callbackObj, ForceCompletion = forceCompletion };
			lock (this)
            {
            	_queue.Enqueue(task);

				// report to thread is has 1 (or more) elements in queue
				Monitor.Pulse(this);
            }
        }

        /// <summary>
		/// run() for Convert thread.
        /// </summary>
        public void ReadConvertQueue()
        {
            while (true)
            {
                int count;
				lock (this) count = _queue.Count;
                while(count > 0)
                {
                    FFmpegTask task;
                    lock (this)
                    {
                        try
                        {
                            task = _queue.Dequeue();
                            count = _queue.Count;   
                        }catch(InvalidOperationException)
                        {
                            break;
                        }
                    }

                    // run queue task and report to callback object
                    Bitmap bitmap = _frameConverter.FrameToBitmap(task.DataPacket);
					
                    // When bitmap == null (decode failed), repeat frame decode with codec reference header (when set).
                    IResultNode headerSource = null;
                    if (bitmap == null)
                    {
                        // Try to select the reference header for the current codec.
                        // When not set; use resultNode, to make sure the headersource is always set.
                        headerSource = (_codecHeaderSource != null) ? _codecHeaderSource.GetHeaderSourceForCodec(task.DataPacket.DataFormat) : null;
                        if(headerSource != null)
                        {
							Bitmap bitmapWithReferenceHeader = _frameConverter.FrameToBitmap(task.DataPacket, headerSource);
                            if(bitmapWithReferenceHeader != null)
                            {
                                // We have a result using the reference header, update the result's bitmap.
                                bitmap = bitmapWithReferenceHeader;
                            }
                            else
                            {
                                // The decode with the reference header failed, unset headersource (we didn't use it after all).
                                headerSource = null;
                            }
                        }
                    }

					if (task.QueueId == _queueId || task.ForceCompletion)
                    {
                        task.Callback.FFmpegDataConverted(new FFmpegResult { Bitmap = bitmap, SourcePacket = task.DataPacket, HeaderSource = headerSource });
                    }
                }

                // Wait for add signal.
                // This means that the queue is only running when there are elements in the queue.
                // Otherwise the thread will be sleeping
                try
                {
					lock (this)
					{
						if (_queue.Count == 0) Monitor.Wait(this);
					}
                }
                catch (Exception e)
                {
                	Debug.WriteLine(e.Message);
                }
            }
		}

		public void Dispose()
        {
            _readThread.Abort();
            _readThread.Join();
            _readThread = null;

            ClearQueue();
            _queue = null;

            _frameConverter = null;
        }
    }
}
