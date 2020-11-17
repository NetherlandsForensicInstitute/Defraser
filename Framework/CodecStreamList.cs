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

using Defraser.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Defraser.Framework
{
    public sealed class CodecStreamList
    {
        private IList<ICodecStream> _codecStreams;

        public CodecStreamList(ICollection<ICodecStream> codecStreams)
        {
            _codecStreams = new List<ICodecStream>();
            foreach (var codecStream in codecStreams)
            {
                _codecStreams.Add(codecStream);
            }
        }

        public void Add(ICodecStream codecStream)
        {
            _codecStreams.Add(codecStream);
        }

        public void RemoveAt(int index)
        {
            _codecStreams.RemoveAt(index);
        }

        public CodecStream Get(int index)
        {
            return new CodecStream(_codecStreams[index]);
        }

        public int Count()
        {
            return _codecStreams.Count;
        }

        public bool Contains(ICodecStream codecStream)
        {
            return _codecStreams.Contains(codecStream);
        }
    }

    public class CodecStream
    {
        private readonly ICodecStream _iCodecStream;
        private KeyFrameList _keyFrames;

        public CodecStream(ICodecStream iCodecStream)
        {
            _iCodecStream = iCodecStream;
            _keyFrames = new KeyFrameList();
        }

        public int GetStreamNumber()
        {
            return _iCodecStream.StreamNumber;
        }

        public string GetName()
        {
            return _iCodecStream.Name;
        }

        public long GetAbsoluteStartOffset()
        {
            return _iCodecStream.AbsoluteStartOffset;
        }

        public long GetLength()
        {
            return _iCodecStream.Length;
        }

        public KeyFrameList GetKeyFrames()
        {
            return _keyFrames;
        }

        // TODO: clean up
        public void SetKeyFrames(KeyFrameList keyFrames) {
            _keyFrames = keyFrames;
        }
    }
}
