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
    public class ResultNode
    {
        private string _name;
        private DataFormat _dataFormat;
        private long _offset;
        private long _length;
        private RangeList _dataRanges;
        
        public ResultNode(string name, CodecID dataFormat, bool isH264AVCC, long offset, long length, RangeList dataRanges)
        {
            _name = name;
            _dataFormat = new DataFormat(dataFormat, isH264AVCC);
            _offset = offset;
            _length = length;
            _dataRanges = dataRanges;
        }

        public string GetName()
        {
            return _name;
        }

        public DataFormat GetDataFormat()
        {
            return _dataFormat;
        }

        public virtual bool isContainer()
        {
            return false;
        }

        public long GetOffset()
        {
            return _offset;
        }

        public long GetLength()
        {
            return _length;
        }

        public RangeList GetDataRanges()
        {
            return _dataRanges;
        }

        public virtual CodecStreamNodeList GetChildren()
        {
            throw new Exception("Codec streams do not have children");
        }

        public virtual KeyFrameList GetKeyFrames()
        {
            throw new Exception("Containers do not have keyframes");
        }
    }

    public class ContainerNode : ResultNode
    {
        private CodecStreamNodeList _children;

        public ContainerNode(IDataBlock iDataBlock, IList<CodecStreamNode> children, RangeList dataRanges) : base(GetNameFor(iDataBlock), GetFormatFor(iDataBlock), false, iDataBlock.StartOffset, iDataBlock.EndOffset - iDataBlock.StartOffset, dataRanges)
        {
            _children = new CodecStreamNodeList(children);
        }

        private static string GetNameFor(IDataBlock dataBlock)
        {
            // More-or-less copied from IFragmentExtensions.cs (GUI code)
            if (IsUnknownDataBlock(dataBlock))
            {
                return dataBlock.Detectors.First().Name;
            }

            return dataBlock.DataFormat.GetName();
        }

        private static CodecID GetFormatFor(IDataBlock dataBlock)
        {
            if (IsUnknownDataBlock(dataBlock))
            {
                // Defaulting to Mpeg4System is the safest call here
                return CodecID.Mpeg4System;
            }

            return dataBlock.DataFormat;
        }

        private static bool IsUnknownDataBlock(IDataBlock dataBlock) {
            return ((dataBlock.DataFormat == CodecID.Unknown) && (dataBlock.Detectors != null && dataBlock.Detectors.Count() > 0) && !(dataBlock.Detectors.First() is ICodecDetector));
        }

        public override bool isContainer()
        {
            return true;
        }

        public override CodecStreamNodeList GetChildren()
        {
            return _children;
        }
    }

    public class CodecStreamNode : ResultNode
    {
        private KeyFrameList _keyFrames;
        // TODO: save fragment, resultnode here

        private CodecStreamNode(string name, CodecID dataFormat, bool isH264AVCC, long offset, long length, KeyFrameList keyFrameList, RangeList dataRanges) : base(name, dataFormat, isH264AVCC, offset, length, dataRanges)
        {
            _keyFrames = keyFrameList;
        }

        public CodecStreamNode(ICodecStream stream, CodecID dataFormat, KeyFrameList keyFrameList, RangeList dataRanges) :
        this(stream.Name, dataFormat, false, stream.AbsoluteStartOffset, stream.Length, keyFrameList, dataRanges)
        {
        }

        public CodecStreamNode(ICodecStream stream, CodecID dataFormat, bool isH264AVCC, KeyFrameList keyFrameList, RangeList dataRanges) :
            this(stream.Name, dataFormat, isH264AVCC, stream.AbsoluteStartOffset, stream.Length, keyFrameList, dataRanges)
        {
        }

        public CodecStreamNode(IDataBlock block, CodecID dataFormat, KeyFrameList keyFrameList, RangeList dataRanges) :
            this(block.DataFormat.GetName(), dataFormat, false, block.StartOffset, block.EndOffset - block.StartOffset, keyFrameList, dataRanges)
        {
        }

        public CodecStreamNode(IDataBlock block, CodecID dataFormat, bool isH264AVCC, KeyFrameList keyFrameList, RangeList dataRanges) :
            this(block.DataFormat.GetName(), dataFormat, isH264AVCC, block.StartOffset, block.EndOffset - block.StartOffset, keyFrameList, dataRanges)
        {
        }

        public override KeyFrameList GetKeyFrames()
        {
            return _keyFrames;
        }
    }

    public class DataFormat
    {
        private string _name;
        private string _descriptiveName;
        private string _outputFileExtension;
        private bool _isH264AVCC;

        public DataFormat(CodecID dataFormat, bool isH264AVCC)
        {
            _name = dataFormat.GetName();
            _descriptiveName = dataFormat.GetDescriptiveName();
            _outputFileExtension = dataFormat.GetOutputFileExtension();
            _isH264AVCC = isH264AVCC;
        }

        public DataFormat(string name, string descriptiveName, string outputFileExtension, bool isH264AVCC)
        {
            _name = name;
            _descriptiveName = descriptiveName;
            _outputFileExtension = outputFileExtension;
            _isH264AVCC = isH264AVCC;
        }

        public string GetName()
        {
            // h264_a or h264_b
            return _name;
        }

        public string GetDescriptiveName()
        {
            return _descriptiveName;
        }

        public string GetOutputFileExtension()
        {
            return _outputFileExtension;
        }

        public bool IsH264AVCC()
        {
            return _isH264AVCC;
        }
    }

    public class CodecStreamNodeList
    {
        private IList<CodecStreamNode> _codecStreams;

        public CodecStreamNodeList(IList<CodecStreamNode> codecStreams)
        {
            _codecStreams = codecStreams;
        }

        public void Add(CodecStreamNode codecStream)
        {
            _codecStreams.Add(codecStream);
        }

        public void RemoveAt(int index)
        {
            _codecStreams.RemoveAt(index);
        }

        public CodecStreamNode Get(int index)
        {
            return _codecStreams[index];
        }

        public int Count()
        {
            return _codecStreams.Count;
        }

        public bool Contains(CodecStreamNode codecStream)
        {
            return _codecStreams.Contains(codecStream);
        }
    }
}
