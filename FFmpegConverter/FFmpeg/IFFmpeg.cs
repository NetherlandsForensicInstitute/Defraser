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
using System.Runtime.InteropServices;

namespace Defraser.FFmpegConverter.FFmpeg
{
	public enum PixelFormat
	{
		PixFmtBgra = 30  // packed BGRA 8:8:8:8, 32bpp, BGRABGRA...
	}

	public enum CodecId
	{
		CodecIdRawvideo = 14
	}

	public enum CodecType
	{
		CodecTypeVideo = 0
	}

	public enum AvDiscard { }

	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct AvRational { }

	public struct AvFrame
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public IntPtr[] Data;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public int[] Linesize;

		// [...]
	}

	public struct AvCodecContext
	{
		private IntPtr _avClass; // AVClass *av_class;
		private int _bitRate;
		private int _bitRateTolerance;
		private int _flags;
		private int _subId;
		private int _meMethod;
		private IntPtr _extradata; // void* extradata;
		private int _extradataSize;
		private AvRational _timeBase;

		public int Width;
		public int Height;

		private int _gopSize;

		public PixelFormat PixFmt;

		private int _rateEmu;
		[MarshalAs(UnmanagedType.FunctionPtr)]
		private Action _drawHorizBand;
		private int _sampleRate;
		private int _channels;
		private AvDiscard _sampleFmt;
		private int _frameSize;
		private int _frameNumber;
		private int _realPictNum;
		private int _delay;
		private float _qcompress;
		private float _qblur;
		private int _qmin;
		private int _qmax;
		private int _maxQdiff;
		private int _maxBFrames;
		private float _bQuantFactor;
		private int _rcStrategy;
		private int _bFrameStrategy;
		private int _hurryUp;
		private IntPtr _codec; // AVCodec
		private IntPtr _privData;
		private int _rtpPayloadSize;
		[MarshalAs(UnmanagedType.FunctionPtr)]
		private Action _rtpCallback;

		/* statistics, used for 2-pass encoding */
		private int _mvBits;
		private int _headerBits;
		private int _iTexBits;
		private int _pTexBits;
		private int _iCount;
		private int _pCount;
		private int _skipCount;
		private int _miscBits;
		private int _frameBits;

		private IntPtr _opaque;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		private byte[] _codecName;

		public CodecType CodecType;
		public CodecId CodecId;

		// [...]
	}

	public struct AvPacket
	{
		public delegate void DestructCallback(IntPtr pAvPacket);

		private Int64 _pts;
		private Int64 _dts;

		public IntPtr Data;
		public int Size;
		public int StreamIndex;

		private int _flags;
		private int _duration;

		public DestructCallback Destruct;

		private IntPtr _priv;
		private Int64 _pos;
		private Int64 _convergenceDuration;
	}

	public struct AvStream
	{
		private int _index;
		private int _id;

		public IntPtr Codec; // AvCodecContext

		// [...]
	}

	public struct AvFormatContext
	{
		private const int MaxStreams = 20;

		private IntPtr _pAvClass;
		private IntPtr _pAvInputFormat;
		private IntPtr _pAvOutputFormat;
		private IntPtr _privData;
		private IntPtr _pb; // ByteIOContext

		public int NbStreams;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxStreams)]
		public IntPtr[] Streams; // AvStream

		// [...]
	}

	public interface IFFmpeg
	{
		int RegisterProtocol(IntPtr protocol);
		void AvRegisterAll();

		void AvFreePacket(IntPtr pAvPacket);

		int AvOpenInputFile([Out]out IntPtr pFormatContext, [MarshalAs(UnmanagedType.LPStr)]String filename, IntPtr pAvInputFormat, int bufSize, IntPtr pAvFormatParameters);
		int AvFindStreamInfo(IntPtr pAvFormatContext);
		int AvReadFrame(IntPtr pAvFormatContext, IntPtr pAvPacket);
		void AvCloseInputFile(IntPtr pAvFormatContext);
		void AvFree(IntPtr ptr);

		void SwsFreeContext(IntPtr swsContext);
		IntPtr SwsGetContext(int sourceWidth, int sourceHeight, int sourcePixFmt, int destWidth, int destHeight, int destPixFmt, int flags, IntPtr srcFilter, IntPtr destFilter, IntPtr param);
		int SwsScale(IntPtr swsContext, IntPtr[] srcSlice, int[] srcStride, int srcSliceY, int srcSliceH, IntPtr[] dstSlice, int[] dstStride);

		int AvpictureFill(IntPtr pAvPicture, IntPtr ptr, int pixFmt, int width, int height);
		int AvpictureGetSize(int pixFmt, int width, int height);

		IntPtr AvcodecFindEncoder(CodecId id);
		IntPtr AvcodecFindDecoder(CodecId id);
		IntPtr AvcodecAllocContext();
		IntPtr AvcodecAllocFrame();
		int AvcodecOpen(IntPtr pAvCodecContext, IntPtr pAvCodec);
		int AvcodecDecodeVideo(IntPtr pAvCodecContext, IntPtr pAvFrame, ref int gotPicturePtr, IntPtr buf, int bufSize);
		int AvcodecEncodeVideo(IntPtr pAvCodecContext, IntPtr buf, int bufSize, IntPtr pAvFrame);
		int AvcodecClose(IntPtr pAvCodecContext);

		void AvFreeStatic();
	}
}
