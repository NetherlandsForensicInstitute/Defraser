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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Defraser.FFmpegConverter.FFmpeg;
using Defraser.Interface;
using Defraser.Util;
using log4net;
using PixelFormat=Defraser.FFmpegConverter.FFmpeg.PixelFormat;

// Do not remove: used in debug mode with logging enabled

namespace Defraser.FFmpegConverter
{
	public class FrameConverter : IDisposable
	{

		#region Log
#if DEBUG
		private static readonly ILog Log =
			LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
		#endregion Log

		#region CustomProtocolDelegates
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int UrlOpen(IntPtr urlcontext, string filename, int flags);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int UrlRead(IntPtr urlcontext, IntPtr buffer, int size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long UrlSeek(IntPtr urlcontext, long pos, int whence);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int UrlClose(IntPtr urlcontext);
		#endregion CustomProtocolDelegates

		#region CustomProtocolClasses
		// Struct attributes have the same names as in the FFmpeg.dll
		// Therefore the names are deviated from the normal C# naming.
		[StructLayout(LayoutKind.Sequential)]
		class URLProtocol
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string name;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public UrlOpen openfunc;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public UrlRead readfunc;
			IntPtr writefunc;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public UrlSeek seekfunc;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public UrlClose closefunc;
			IntPtr next;
		}

		[StructLayout(LayoutKind.Sequential)]
		// Struct attributes have the same names as in the FFmpeg.dll
		// Therefore the names are deviated from the normal C# naming.
		struct URLContext
		{
			public IntPtr protocol;
			public int flags;
			public int is_streamed;
			public int max_packet_size;
			public IntPtr privdata;
			[MarshalAs(UnmanagedType.LPStr)]
			public string filename;
		}
		#endregion CustomProtocolClasses

		#region Constants
		private const PixelFormat OutputPixelFormat = PixelFormat.PixFmtBgra;
		private const String DefraserProtocolPrefix = "defraser://";
		private const string NalUnitLengthName = "NalUnitLength";
		private const int SwsFastBilinear = 1;
		#endregion Constants

		#region PrivateClassDefines
		private class VideoInputData : IDisposable
		{
			#region Attributes
			private readonly IFFmpeg _ffmpeg;
			private readonly IntPtr _pInputCodecContext;
			private readonly AvCodecContext _videoCodecContext;
			private readonly IntPtr _pInputFormatContext;
			private readonly int _videoStartIndex;
			#endregion Attributes

			#region Properties
			public IntPtr PInputFormatContext { get { return _pInputFormatContext; } }
			public IntPtr PInputCodecContext { get { return _pInputCodecContext; } }
			public AvCodecContext VideoCodecContext { get { return _videoCodecContext; } }
			public int VideoStartIndex { get { return _videoStartIndex; } }

			public int Width { get { return _videoCodecContext.Width; } }
			public int Height { get { return _videoCodecContext.Height; } }
			#endregion Properties

			public VideoInputData(IFFmpeg ffmpeg, IntPtr pInputFormatContext, IntPtr pInputCodecContext, AvCodecContext videoCodecContext, int videoStartIndex)
			{
				_ffmpeg = ffmpeg;
				_pInputFormatContext = pInputFormatContext;
				_pInputCodecContext = pInputCodecContext;
				_videoCodecContext = videoCodecContext;
				_videoStartIndex = videoStartIndex;
			}

			public void Dispose()
			{
				if (_pInputCodecContext != IntPtr.Zero)
					_ffmpeg.AvcodecClose(_pInputCodecContext);
				if (_pInputFormatContext != IntPtr.Zero)
					_ffmpeg.AvCloseInputFile(_pInputFormatContext);
			}
		}

		private class VideoOuputData : IDisposable
		{
			#region Attributes
			private readonly IFFmpeg _ffmpeg;
			private readonly IntPtr _pOutputCodecContext;
			#endregion Attributes

			#region Properties
			public IntPtr POutputCodecContext { get { return _pOutputCodecContext; } }
			#endregion Properties

			public VideoOuputData(IFFmpeg ffmpeg, IntPtr pOutputCodecContext)
			{
				_ffmpeg = ffmpeg;
				_pOutputCodecContext = pOutputCodecContext;
			}

			public void Dispose()
			{
				if (_pOutputCodecContext != IntPtr.Zero)
					_ffmpeg.AvcodecClose(_pOutputCodecContext);
			}
		}

		#endregion PrivateClassDefines

		#region Attributes

		// Custom defined Defraser FFmpeg protocol (static)
		private static bool _ffmpegRegistered;
		private static URLProtocol _protocol;
		private static IntPtr _pDefraserProtocol;
		private static Dictionary<int, MemoryStream> _videoMemoryStreams;
		private static int _nextConvertId;

		// Pool to read frame/packet Data from video
		private readonly Creator<IDataReaderPool> _poolCreator;
		private readonly IFFmpeg _ffmpeg;
		private int _convertId;

		#endregion Attributes

		public FrameConverter(Creator<IDataReaderPool> poolCreator, IFFmpeg ffmpeg)
		{
			_poolCreator = poolCreator;
			_ffmpeg = ffmpeg;
			_convertId = -1;
		}

		public void Dispose()
		{
			UnregisterFFmpeg();
		}

		/// <summary>
		/// Register FFmpeg for all convert requests.
		/// Method is called once by FFmpegManager on application load.
		/// </summary>
		public void RegisterFFmpeg()
		{
			if (_ffmpegRegistered)
				return;

			try
			{
				// Setup Data list / attributes
				_nextConvertId = 0;
				_videoMemoryStreams = new Dictionary<int, MemoryStream>();

				// Create defraser (input) protocol
				_protocol = new URLProtocol();
				_protocol.openfunc = ProtocolOpen;
				_protocol.readfunc = ProtocolRead;
				_protocol.seekfunc = ProtocolSeek;
				_protocol.closefunc = ProtocolClose;
				_protocol.name = "defraser";

				// Write protocol to pointer
				_pDefraserProtocol = Allocate<URLProtocol>();
				Marshal.StructureToPtr(_protocol, _pDefraserProtocol, false);

				// Register DLL and 'defraser' protocol
				_ffmpeg.AvRegisterAll();
				_ffmpeg.RegisterProtocol(_pDefraserProtocol); // calls av_register_protocol
				_ffmpegRegistered = true;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				//MessageBox.Show("FFmpeg was not able to register itself, frame visualisation is not available." + Environment.NewLine + e.Message,"FFmpeg not available",MessageBoxButtons.OK,MessageBoxIcon.Error); //FIXME: add commandline argument to be visual-style or user interaction allowed or not.
			}
		}

		/// <summary>
		/// Unload FFmpeg and libraries.
		/// </summary>
		public void UnregisterFFmpeg()
		{
			if (!_ffmpegRegistered)
				return;

			try
			{
				_ffmpeg.AvFreeStatic();
			}
			catch (Exception)
			{
			}
			finally
			{
				_ffmpegRegistered = false;
			}

			if (_pDefraserProtocol != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_pDefraserProtocol);
				_pDefraserProtocol = IntPtr.Zero;
			}
		}


		/// <summary>
		/// Creates an input codec, based on the settings found in the dataPacket/headerPacket.
		/// </summary>
		/// <param name="dataPacket">Contains the frame Data.</param>
		/// <param name="headerPacket">Contains the frame header Data.</param>
		/// <returns></returns>
		private VideoInputData CreateCodecFromPacket(IResultNode dataPacket, IResultNode headerPacket)
		{
			// Load the framedata from the packet to the byte buffer
			if (!PacketDataToBuffer(dataPacket, headerPacket))
			{
#if DEBUG
				Log.Info("No frame header information is available.");
#endif
				return null;
			}

			IntPtr pInputFormatContext;
			if (_ffmpeg.AvOpenInputFile(out pInputFormatContext, DefraserProtocolPrefix + _convertId, IntPtr.Zero, 0, IntPtr.Zero) < 0)
			{
#if DEBUG
				Log.Info("Could not open input file.");
#endif
				return null;
			}

			if (_ffmpeg.AvFindStreamInfo(pInputFormatContext) < 0)
			{
#if DEBUG
				Log.Info("Could not find (input)stream information.");
#endif
				return null;
			}

			VideoInputData videoInputData = null;
			AvFormatContext formatContext = PtrToStructure<AvFormatContext>(pInputFormatContext);
			for (int i = 0; i < formatContext.NbStreams; ++i)
			{
				AvStream stream = PtrToStructure<AvStream>(formatContext.Streams[i]);
				AvCodecContext codec = PtrToStructure<AvCodecContext>(stream.Codec);

				if (codec.CodecType == CodecType.CodecTypeVideo)
				{
					AvCodecContext videoCodecContext = PtrToStructure<AvCodecContext>(stream.Codec);
					IntPtr pVideoCodec = _ffmpeg.AvcodecFindDecoder(videoCodecContext.CodecId);
					if (pVideoCodec == IntPtr.Zero)
					{
#if DEBUG
						Log.Info("could not find input codec");
#endif
						return null;
					}

					if (_ffmpeg.AvcodecOpen(stream.Codec, pVideoCodec) < 0)
					{
#if DEBUG
						Log.Info("Could not open input codec.");
#endif
						return null;
					}

					// setup object used to decode (input) frames
					videoInputData = new VideoInputData(_ffmpeg, pInputFormatContext, stream.Codec, PtrToStructure<AvCodecContext>(stream.Codec), i);

					break; // stop seaching for video stream
				}
			}

			if (videoInputData == null)
			{
#if DEBUG
				Log.Info("Could not find video stream.");
#endif
				return null;
			}
			return videoInputData;
		}

		/// <summary>
		/// This method reads the video Data/headers from the file and creates a buffer.
		/// The buffer is added to the '_videoMemoryStreams' Dictionary so it can be used at one of the FFmpeg callback methods.
		/// </summary>
		/// <param name="dataPacket">Contains the frame Data.</param>
		/// <param name="headerPacket">Contains the header Data, when null, dataPacket is used as headersource.</param>
		/// <returns></returns>
		private bool PacketDataToBuffer(IResultNode dataPacket, IResultNode headerPacket)
		{
			// Find current codec detector in packet
			ICodecDetector codecDetector = (ICodecDetector) dataPacket.Detectors.Where(d => d is ICodecDetector).FirstOrDefault();
			if (codecDetector == null) return false;

			// Resolve frame header info
			IDataPacket videoHeaders = codecDetector.GetVideoHeaders(headerPacket);
			IDataPacket videoData = codecDetector.GetVideoData(dataPacket);
			if (videoData == null) return false;

			// Copy all the Data, both header- and framedata, to a buffer.
			byte[] dataBuf = ReadDataPacket(videoData);
			if (IsH264NalUnitResult(dataPacket))
			{
				// Automatically convert H.264 'NAL unit stream' to 'byte stream', because FFmpeg
				// does not support the 'NAL unit stream' format.
				dataBuf = ConvertH264NalUnitStreamVideoDataToByteStream(dataBuf);
			}
			if (videoHeaders != null)
			{
				byte[] headerData = ReadDataPacket(videoHeaders);
				if (IsH264NalUnitResult(headerPacket))
				{
					// Automatically convert H.264 'NAL unit stream' to 'byte stream', because FFmpeg
					// does not support the 'NAL unit stream' format.
					headerData = ConvertH264NalUnitStreamHeaderDataToByteStream(headerData);
				}

				dataBuf = headerData.Concat(dataBuf).ToArray();
			}

			// Put the Data in a memory stream, used by FFmpeg to read the 'file' from memory.
			lock (_videoMemoryStreams)
			{
				_convertId = _nextConvertId;
				_nextConvertId++;

				_videoMemoryStreams.Add(_convertId, new MemoryStream(dataBuf));
			}
			return true;
		}

		private static bool IsH264NalUnitResult(IResultNode result)
		{
			return (result.DataFormat == Interface.CodecID.H264) && (result.FindAttributeByName(NalUnitLengthName) != null);
		}

		private byte[] ReadDataPacket(IDataPacket dataPacket)
		{
			var data = new byte[dataPacket.Length];

			using (IDataReaderPool pool = _poolCreator())
			using (IDataReader reader = pool.CreateDataReader(dataPacket))
			{
				reader.Read(data, 0, data.Length);
			}
			return data;
		}

		private static byte[] ConvertH264NalUnitStreamVideoDataToByteStream(byte[] data)
		{
			var tmp = new byte[data.Length];
			int destPos = 0;

			for (int srcPos = 0; srcPos < (data.Length - 5); )
			{
				uint nalUnitLength = ToUInt32(data, srcPos);
				srcPos += 4;

				if ((srcPos + nalUnitLength) > data.Length)
				{
					break;
				}

				// Add start code prefix
				tmp[destPos++] = 0;
				tmp[destPos++] = 0;
				tmp[destPos++] = 1;

				// Copy NAL unit Data, but omit the length field
				Array.Copy(data, srcPos, tmp, destPos, nalUnitLength);
				srcPos += (int)nalUnitLength;
				destPos += (int)nalUnitLength;
			}

			// Return a byte array that exactly fits the Data
			var convertedData = new byte[destPos];
			Array.Copy(tmp, convertedData, destPos);
			return convertedData;
		}

		private static byte[] ConvertH264NalUnitStreamHeaderDataToByteStream(byte[] data)
		{
			var tmp = new byte[data.Length + 3];
			int destPos = 1; // Note: first byte is '0'

			// Note: 'Data' is expected to contain a Sequence Parameter Set and a Picture Parameter Set, in that order.
			for (int srcPos = 0; srcPos < (data.Length - 2); )
			{
				uint nalUnitLength = ToUInt16(data, srcPos);
				srcPos += 2;

				if ((srcPos + nalUnitLength) > data.Length)
				{
					return new byte[0]; // Error!
				}

				// Add start code prefix
				tmp[destPos++] = 0;
				tmp[destPos++] = 0;
				tmp[destPos++] = 1;

				// Copy NAL unit Data, but omit the length field
				Array.Copy(data, srcPos, tmp, destPos, nalUnitLength);
				srcPos += (int)nalUnitLength;
				destPos += (int)nalUnitLength;
			}
			return tmp;
		}

		private static uint ToUInt16(byte[] data, int offset)
		{
			return ((uint)data[offset] << 8) | data[offset + 1];
		}

		private static uint ToUInt32(byte[] data, int offset)
		{
			return ((uint)data[offset] << 24) |
				   ((uint)data[offset + 1] << 16) |
				   ((uint)data[offset + 2] << 8) |
						   data[offset + 3];
		}

		/// <summary>
		/// Create an output codec using the settings found in the VideoInputData.
		/// </summary>
		/// <param name="videoInputData">The input video settings, probably found in the source packet.</param>
		/// <returns>The ouput codec settings, RAW video codec.</returns>
		private VideoOuputData CreateOutputCodec(VideoInputData videoInputData)
		{
			// open output codec
			IntPtr pOutputCodec = _ffmpeg.AvcodecFindEncoder(CodecId.CodecIdRawvideo);

			if (pOutputCodec == IntPtr.Zero)
			{
#if DEBUG
				Log.Info("Could not load output codec.");
#endif
				return null;
			}

			// Setup target encoding context (output settings)
			VideoOuputData videoOutputData = new VideoOuputData(_ffmpeg, _ffmpeg.AvcodecAllocContext());
			AvCodecContext outputCodecContext = PtrToStructure<AvCodecContext>(videoOutputData.POutputCodecContext);
			outputCodecContext.Width = videoInputData.Width;
			outputCodecContext.Height = videoInputData.Height;
			outputCodecContext.PixFmt = OutputPixelFormat;

			Marshal.StructureToPtr(outputCodecContext, videoOutputData.POutputCodecContext, false);
			if (_ffmpeg.AvcodecOpen(videoOutputData.POutputCodecContext, pOutputCodec) < 0)
			{
#if DEBUG
				Log.Info("Could not open output codec.");
#endif
				return null;
			}
			return videoOutputData;
		}

		/// <summary>
		/// Start a frame convert using the Data from the dataPacket.
		/// This method using the same dataPacket as headerSource.
		/// </summary>
		/// <param name="dataPacket">Contains the frame Data.</param>
		/// <returns>The Bitmap as result from the frame convertion, null when operation failed.</returns>
		public Bitmap FrameToBitmap(IResultNode dataPacket)
		{
			return FrameToBitmap(dataPacket, null);
		}

		/// <summary>
		/// Start a frame convert using the Data from the dataPacket.
		/// If headerPacket is not null, it will be used as header source.
		/// </summary>
		/// <param name="dataPacket">Contains the frame Data.</param>
		/// <param name="headerPacket">Datapacket as headersource, null is allowed.</param>
		/// <returns>The Bitmap as result from the frame convertion, null when operation failed.</returns>
		public Bitmap FrameToBitmap(IResultNode dataPacket, IResultNode headerPacket)
		{
			if (!_ffmpegRegistered) return null;

			if (dataPacket == null)
			{
				throw new ArgumentNullException("dataPacket", "Invalid input parameters(null) for FFmpeg frame convertion.");
			}
			if (headerPacket == null)
			{
				headerPacket = dataPacket;
			}

			try
			{
				// Detect the codec from the Data packet
				using (VideoInputData videoInputData = CreateCodecFromPacket(dataPacket, headerPacket))
				{
					if (videoInputData == null) return null;

					// Create the RAW video output codec
					using (VideoOuputData videoOutputData = CreateOutputCodec(videoInputData))
					{
						if (videoOutputData == null) return null;

						// Create to bitmap from the first video frame in Data
						return VideoDataToBitmap(videoInputData, videoOutputData);
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				lock (_videoMemoryStreams)
				{
					_videoMemoryStreams.Remove(_convertId);
				}
			}
		}

		/// <summary>
		/// Create the actual Bitmap from the dataPacket.
		/// This done by using both the input- and outputdata.
		/// </summary>
		/// <param name="videoInputData">Input Data, contains input codec, frame Data etc.</param>
		/// <param name="videoOutputData">Output Data, contains output codec (settings).</param>
		/// <returns>The Bitmap as result from the frame convertion, null when operation failed.</returns>
		private Bitmap VideoDataToBitmap(VideoInputData videoInputData, VideoOuputData videoOutputData)
		{
			// Allocate video frames
			IntPtr pFrame = _ffmpeg.AvcodecAllocFrame();
			IntPtr pOutFrame = _ffmpeg.AvcodecAllocFrame();

			// Init some attributes
			int gotPicture = 0;

			// Return / output bitmap object
			Bitmap bitmap = null;

			// Video packet (pointer) to decode
			IntPtr pPacket = Allocate<AvPacket>();
			AvPacket packet;

			try
			{
				while (_ffmpeg.AvReadFrame(videoInputData.PInputFormatContext, pPacket) >= 0)
				{
					packet = PtrToStructure<AvPacket>(pPacket);

					// Is this a packet from the video stream?
					if (packet.StreamIndex == videoInputData.VideoStartIndex)
					{
						// Decode (input) video frame
						_ffmpeg.AvcodecDecodeVideo(videoInputData.PInputCodecContext, pFrame, ref gotPicture, packet.Data, packet.Size);

						if (gotPicture != 0)
						{
							// output encoding
							// create output buffer
							int bufSize = _ffmpeg.AvpictureGetSize((int)PixelFormat.PixFmtBgra, videoInputData.Width, videoInputData.Height);

							byte[] outbuf = new byte[bufSize];
							_ffmpeg.AvpictureFill(pOutFrame, Marshal.UnsafeAddrOfPinnedArrayElement(outbuf, 0),
												  (int)PixelFormat.PixFmtBgra, videoInputData.Width, videoInputData.Height);

							AvFrame frame = PtrToStructure<AvFrame>(pFrame);
							AvFrame outFrame = PtrToStructure<AvFrame>(pOutFrame);

							IntPtr swsContext = IntPtr.Zero;
							try
							{
								swsContext = _ffmpeg.SwsGetContext(videoInputData.Width, videoInputData.Height, (int)videoInputData.VideoCodecContext.PixFmt,
																   videoInputData.Width, videoInputData.Height,
																   (int)PixelFormat.PixFmtBgra,
																   SwsFastBilinear, IntPtr.Zero,
																   IntPtr.Zero, IntPtr.Zero);

								_ffmpeg.SwsScale(swsContext, frame.Data, frame.Linesize, 0, videoInputData.Height, outFrame.Data, outFrame.Linesize);

								// encode RAW output frame
								int outLength = _ffmpeg.AvcodecEncodeVideo(videoOutputData.POutputCodecContext,
																			Marshal.UnsafeAddrOfPinnedArrayElement(
																				outbuf, 0),
																			bufSize, pOutFrame);

								// check if frame encode succeeded
								if (outLength > 0)
								{
									try
									{
										bitmap = CopyRawDataToBitmap(videoInputData, outbuf);
										break; // A bitmap has been created, stop searching for a frame.
									}
									catch (Exception)
									{
#if DEBUG
										Log.Info("An error occurred while creating the output bitmap.");
#endif
									}
								}
#if DEBUG
								else
								{

									Log.Info("Ouput buffer is empty, no bitmap was created.");
								}
#endif
							}
							finally
							{
								if (swsContext != IntPtr.Zero) _ffmpeg.SwsFreeContext(swsContext);
							}
						}
					}
				}
			}
			finally
			{
				_ffmpeg.AvFree(pOutFrame);
				_ffmpeg.AvFree(pFrame);

				_ffmpeg.AvFreePacket(pPacket);
				Marshal.FreeHGlobal(pPacket);
			}
			return bitmap;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="videoInputData">Input Data object, used for output settings.</param>
		/// <param name="outbuf">Raw Data buffer.</param>
		/// <returns>RGB24 Bitmap, created from the output buffer.</returns>
		private static Bitmap CopyRawDataToBitmap(VideoInputData videoInputData, byte[] outbuf)
		{
			// Unsafe block to boost performance of bitmap copy
			Bitmap bitmap;
			bitmap = new Bitmap(videoInputData.Width, videoInputData.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			Rectangle bitmapRect = new Rectangle(0, 0, videoInputData.Width, videoInputData.Height);
			BitmapData bitmapData = bitmap.LockBits(bitmapRect, ImageLockMode.WriteOnly,
														System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			var imageStride = videoInputData.Width * 4;
			if (bitmapData.Stride == imageStride)
			{
				Marshal.Copy(outbuf, 0, bitmapData.Scan0, outbuf.Length);
			}
			else
			{
				for (var i = 0; i < videoInputData.Height; i++)
				{
					var start = new IntPtr(bitmapData.Scan0.ToInt64() + i * bitmapData.Stride);
					Marshal.Copy(Marshal.UnsafeAddrOfPinnedArrayElement(outbuf, i * imageStride), new[] { start }, 0, imageStride);
				}
			}
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		#region ProtocolCallbacks
		// These procol callback are called from the FFmpeg library.

		/// <summary>
		/// Not Used, but required by FFmpeg custom protocol.
		/// </summary>
		/// <param name="pUrlcontext">Settings for the custom Defraser protol.</param>
		/// <param name="filename">The name of the file that should be opened.</param>
		/// <param name="flags"></param>
		/// <returns>Status, 0 when no errors occurred.</returns>
		private static int ProtocolOpen(IntPtr pUrlcontext, string filename, int flags)
		{
			return 0;
		}

		/// <summary>
		/// Reads the required header and frame Data from a MemoryStream to a buffer for Defraser.
		/// </summary>
		/// <param name="pUrlcontext">Settings for the custom Defraser protol.</param>
		/// <param name="buffer">Buffer for FFmpeg which contains the Data read from the MemoryStream.</param>
		/// <param name="size">The size of the buffer to read to.</param>
		/// <returns>Size of the Data read and put into the buffer.</returns>
		private static int ProtocolRead(IntPtr pUrlcontext, IntPtr buffer, int size)
		{
			URLContext context = PtrToStructure<URLContext>(pUrlcontext);

			// Get video packet id from filename
			int index = Int32.Parse(context.filename.Substring(DefraserProtocolPrefix.Length));
			MemoryStream videoStream = _videoMemoryStreams[index];

			byte[] temp = new byte[size];
			int count = videoStream.Read(temp, 0, size);
			Marshal.Copy(temp, 0, buffer, count);
			return count;
		}

		/// <summary>
		/// Seeks to the right position in the memorystream.
		/// </summary>
		/// <param name="pUrlcontext">Settings for the custom Defraser protol.</param>
		/// <param name="pos">The seek offset from 'whence'.</param>
		/// <param name="whence">The position from which the seek starts.</param>
		/// <returns>The new (seeked) position in the file/Data.</returns>
		private static long ProtocolSeek(IntPtr pUrlcontext, long pos, int whence)
		{
			URLContext context = PtrToStructure<URLContext>(pUrlcontext);
			int index = Int32.Parse(context.filename.Substring(_protocol.name.Length + 3));
			MemoryStream videoStream = _videoMemoryStreams[index];

			SeekOrigin so = SeekOrigin.Current;
			switch (whence)
			{
				case 0:
					so = SeekOrigin.Begin;
					break;
				case 1:
					so = SeekOrigin.Current;
					break;
				case 3:
					so = SeekOrigin.End;
					break;
				case 0x10000:
					return videoStream.Length;
			}
			return videoStream.Seek(pos, so);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pUrlcontext">Settings for the custom Defraser protol.</param>
		/// <returns>Status, 0 when no errors occurred.</returns>
		private static int ProtocolClose(IntPtr pUrlcontext)
		{
			URLContext context = PtrToStructure<URLContext>(pUrlcontext);
			int index = Int32.Parse(context.filename.Substring(DefraserProtocolPrefix.Length));
			_videoMemoryStreams[index].Close();
			return 0;
		}

		#endregion

		#region StaticUtilMethods
		static T PtrToStructure<T>(IntPtr pointer)
		{
			return (T)Marshal.PtrToStructure(pointer, typeof(T));
		}

		static IntPtr Allocate<T>()
		{
			return Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
		}
		#endregion
	}
}
