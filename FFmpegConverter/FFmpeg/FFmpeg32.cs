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
using System.Security;

namespace Defraser.FFmpegConverter.FFmpeg
{
	public sealed class FFmpeg32 : IFFmpeg
	{
		// ReSharper disable InconsistentNaming

		// av_free_packet is internal, so reimplemented in managed code
		private static void av_free_packet(IntPtr pAVPacket)
		{
			if (pAVPacket == IntPtr.Zero)
				return;

			AvPacket packet = (AvPacket)Marshal.PtrToStructure(pAVPacket, typeof(AvPacket));
			if (packet.Destruct == null)
				return;

			packet.Destruct(pAVPacket);
		}

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int register_protocol(IntPtr protocol);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern void av_register_all();

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int av_open_input_file([Out]out IntPtr pFormatContext, [MarshalAs(UnmanagedType.LPStr)]String filename, IntPtr pAvInputFormat, int bufSize, IntPtr pAvFormatParameters);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int av_find_stream_info(IntPtr pAvFormatContext);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int av_read_frame(IntPtr pAvFormatContext, IntPtr pAvPacket);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern void av_close_input_file(IntPtr pAvFormatContext);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern void av_free(IntPtr ptr);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern void sws_freeContext(IntPtr swsContext);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr sws_getContext(int sourceWidth, int sourceHeight, int sourcePixFmt, int destWidth, int destHeight, int destPixFmt, int flags, IntPtr srcFilter, IntPtr destFilter, IntPtr param);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int sws_scale(IntPtr swsContext, IntPtr[] srcSlice, int[] srcStride, int srcSliceY, int srcSliceH, IntPtr[] dstSlice, int[] dstStride);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avpicture_fill(IntPtr pAVPicture, IntPtr ptr, int pix_fmt, int width, int height);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avpicture_get_size(int pix_fmt, int width, int height);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr avcodec_find_encoder(CodecId id);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr avcodec_find_decoder(CodecId id);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr avcodec_alloc_context();

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr avcodec_alloc_frame();

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avcodec_open(IntPtr pAvCodecContext, IntPtr pAvCodec);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avcodec_decode_video(IntPtr pAvCodecContext, IntPtr pAvFrame, ref int gotPicturePtr, IntPtr buf, int bufSize);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avcodec_encode_video(IntPtr pAvCodecContext, IntPtr buf, int bufSize, IntPtr pAvFrame);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern int avcodec_close(IntPtr pAvCodecContext);

		[DllImport("ffmpeg32.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		private static extern void av_free_static();
		// ReSharper restore InconsistentNaming

		public int RegisterProtocol(IntPtr protocol)
		{
			return register_protocol(protocol);
		}

		public void AvRegisterAll()
		{
			av_register_all();
		}

		public void AvFreePacket(IntPtr pAvPacket)
		{
			av_free_packet(pAvPacket);
		}

		public int AvOpenInputFile(out IntPtr pFormatContext, string filename, IntPtr pAvInputFormat, int bufSize, IntPtr pAvFormatParameters)
		{
			return av_open_input_file(out pFormatContext, filename, pAvInputFormat, bufSize, pAvFormatParameters);
		}

		public int AvFindStreamInfo(IntPtr pAvFormatContext)
		{
			return av_find_stream_info(pAvFormatContext);
		}

		public int AvReadFrame(IntPtr pAvFormatContext, IntPtr pAvPacket)
		{
			return av_read_frame(pAvFormatContext, pAvPacket);
		}

		public void AvCloseInputFile(IntPtr pAvFormatContext)
		{
			av_close_input_file(pAvFormatContext);
		}

		public void AvFree(IntPtr ptr)
		{
			av_free(ptr);
		}

		public void SwsFreeContext(IntPtr swsContext)
		{
			sws_freeContext(swsContext);
		}

		public IntPtr SwsGetContext(int sourceWidth, int sourceHeight, int sourcePixFmt, int destWidth, int destHeight, int destPixFmt, int flags, IntPtr srcFilter, IntPtr destFilter, IntPtr param)
		{
			return sws_getContext(sourceWidth, sourceHeight, sourcePixFmt, destWidth, destHeight, destPixFmt, flags, srcFilter, destFilter, param);
		}

		public int SwsScale(IntPtr swsContext, IntPtr[] srcSlice, int[] srcStride, int srcSliceY, int srcSliceH, IntPtr[] dstSlice, int[] dstStride)
		{
			return sws_scale(swsContext, srcSlice, srcStride, srcSliceY, srcSliceH, dstSlice, dstStride);
		}

		public int AvpictureFill(IntPtr pAvPicture, IntPtr ptr, int pixFmt, int width, int height)
		{
			return avpicture_fill(pAvPicture, ptr, pixFmt, width, height);
		}

		public int AvpictureGetSize(int pixFmt, int width, int height)
		{
			return avpicture_get_size(pixFmt, width, height);
		}

		public IntPtr AvcodecFindEncoder(CodecId id)
		{
			return avcodec_find_encoder(id);
		}

		public IntPtr AvcodecFindDecoder(CodecId id)
		{
			return avcodec_find_decoder(id);
		}

		public IntPtr AvcodecAllocContext()
		{
			return avcodec_alloc_context();
		}

		public IntPtr AvcodecAllocFrame()
		{
			return avcodec_alloc_frame();
		}

		public int AvcodecOpen(IntPtr pAvCodecContext, IntPtr pAvCodec)
		{
			return avcodec_open(pAvCodecContext, pAvCodec);
		}

		public int AvcodecDecodeVideo(IntPtr pAvCodecContext, IntPtr pAvFrame, ref int gotPicturePtr, IntPtr buf, int bufSize)
		{
			return avcodec_decode_video(pAvCodecContext, pAvFrame, ref gotPicturePtr, buf, bufSize);
		}

		public int AvcodecEncodeVideo(IntPtr pAvCodecContext, IntPtr buf, int bufSize, IntPtr pAvFrame)
		{
			return avcodec_encode_video(pAvCodecContext, buf, bufSize, pAvFrame);
		}

		public int AvcodecClose(IntPtr pAvCodecContext)
		{
			return avcodec_close(pAvCodecContext);
		}

		public void AvFreeStatic()
		{
			av_free_static();
		}
	}
}
