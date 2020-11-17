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
using System.IO;
using Defraser.Detector.Mpeg2.Video;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	/// <summary>
	/// Issue 2814: Scan remainder of codec stream after issue in codec stream
	/// </summary>
	[TestFixture]
	public class TestIssue2814
	{
		private const string FileNameProjectIssue2814 = @"./issue2814.dpr";
		private IProject _projectIssue2814;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			File.Delete(FileNameProjectIssue2814);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			File.Delete(FileNameProjectIssue2814);
		}

		[SetUp] // Executed for each test in this fixture
		public void CreateProject()
		{
			_projectIssue2814 = TestFramework.ProjectManager.CreateProject(FileNameProjectIssue2814, "S. Holmes", DateTime.Now, "Scan file 1");
		}

		[TearDown] // Executed for each test in this fixture
		public void CloseProject()
		{
			if (_projectIssue2814 != null)
			{
				TestFramework.ProjectManager.CloseProject(_projectIssue2814);
				_projectIssue2814 = null;
			}
		}

		[Test, Category("Regression")]
		public void TestScanRemainderOfCodecStreamAfterIssueInMpeg1VideoCodecStream()
		{
			string mpeg1SourceFileName = Util.TestdataPath + "Back_Up.mpg";
			const string invalidMpeg1CopyFileName = "InvalidMpeg1CopyFileName";
			// Value of codec stream detector setting

			try
			{
				// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
				File.Delete(invalidMpeg1CopyFileName);
				File.Copy(mpeg1SourceFileName, invalidMpeg1CopyFileName);

				// Step 2: overwrite bytes of the codec stream to make it corrupt;
				using (FileStream fileStream = File.OpenWrite(invalidMpeg1CopyFileName))
				{
					fileStream.Seek(0x76B3EL, 0L);
					byte[] zeroArray = new byte[8];
					fileStream.Write(zeroArray, 0, zeroArray.Length);
					fileStream.Close();
				}

				// Step 3: setup the codec detector so that 'Max Offset Between Header = 0';
				// Step 4: scan the file;
				Util.Scan(invalidMpeg1CopyFileName, _projectIssue2814, "MPEG-1/2 Systems", default(KeyValuePair<string, string>), Mpeg2VideoDetector.DetectorName, default(KeyValuePair<string, string>));

				// Step 5: validate the result
				// Two codec streams are expected as the result:
				// - one containing the headers before the corrupt header and
				// - one containing the headers after the corrupt header
				Assert.That(_projectIssue2814.GetInputFiles().Count, Is.EqualTo(1));
				IList<IDataBlock> dataBlocks = _projectIssue2814.GetDataBlocks(_projectIssue2814.GetInputFiles()[0]);
				Assert.That(dataBlocks.Count, Is.EqualTo(1));

				// Check the data block itself and its content
				Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Mpeg1System), Is.EqualTo(0), Is.EqualTo(0x3A280C), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1642), Is.EqualTo(3));

				// Video stream before corrupt header
				Util.TestStream(dataBlocks[0].CodecStreams[0], Is.EqualTo(CodecID.Mpeg1Video), Is.EqualTo(0x62354), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(5));
				Util.TestFirstHeader(dataBlocks[0].CodecStreams[0], Is.EqualTo("SequenceHeader"), Is.EqualTo(0x1246), Is.EqualTo(0x4C));
				Util.TestLastHeader(dataBlocks[0].CodecStreams[0], Is.EqualTo("GroupOfPicturesHeader"), Is.EqualTo(0x76B36), Is.EqualTo(0x8));

				// Video stream after corrupt header
				Util.TestStream(dataBlocks[0].CodecStreams[1], Is.EqualTo(CodecID.Mpeg1Video), Is.EqualTo(0x293E15), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(44));
				Util.TestFirstHeader(dataBlocks[0].CodecStreams[1], Is.EqualTo("Slice"), Is.EqualTo(0x76B46), Is.EqualTo(0x60F3));
				Util.TestLastHeader(dataBlocks[0].CodecStreams[1], Is.EqualTo("SequenceEndCode"), Is.EqualTo(0x39765B), Is.EqualTo(0x4));

				// Audio stream
				Util.TestStream(dataBlocks[0].CodecStreams[2], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x93ADB), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1));
				Util.TestOneHeader(dataBlocks[0].CodecStreams[2], Is.EqualTo("Data"), Is.EqualTo(0x1B55), Is.EqualTo(0x93ADB));
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			File.Delete(invalidMpeg1CopyFileName);
		}

		[Test, Category("Regression")]
		public void TestScanRemainderOfCodecStreamAfterIssueInMpeg4CodecStream()
		{
			string mpeg4SourceFileName = Util.TestdataPath + "3GP_mp4v.3gp";
			const string invalidMpeg4CopyFileName = "InvalidMpeg4CopyFileName";
			// Value of codec stream detector setting
			const string maxOffsetBetweenHeaders = "Max Offset Between Headers";

			// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
			File.Delete(invalidMpeg4CopyFileName);
			File.Copy(mpeg4SourceFileName, invalidMpeg4CopyFileName);

			// Step 2: overwrite the start code of a codec stream header to make it corrupt;
			using (FileStream fileStream = File.OpenWrite(invalidMpeg4CopyFileName))
			{
				fileStream.Seek(0x4022L, 0L);
				byte[] zeroArray = new byte[4];
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				fileStream.Close();
			}

			// Step 3: setup the codec detector so that 'Max Offset Between Header = 0';
			// Step 4: scan the file;
			Util.Scan(invalidMpeg4CopyFileName, _projectIssue2814, "3GPP/QT/MP4", default(KeyValuePair<string, string>), "MPEG-4 Video/H.263", new KeyValuePair<string, string>(maxOffsetBetweenHeaders, "0"));

			// Step 5: validate the result
			// Two codec streams are expected as the result:
			// - one containing the headers before the corrupt header and
			// - one containing the headers after the corrupt header
			Assert.That(_projectIssue2814.GetInputFiles().Count, Is.EqualTo(1));
			IList<IDataBlock> dataBlocks = _projectIssue2814.GetDataBlocks(_projectIssue2814.GetInputFiles()[0]);

			// Note: There may be more than one data block detected, because Defraser will
			//       rescan the 'mdat' block of any corrupt 3GPP file using *ALL* detectors!
			Assert.That(dataBlocks.Count, Is.GreaterThanOrEqualTo(2));

			// Check the data block itself and its content
			Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0), Is.EqualTo(0x22F04), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(5), Is.EqualTo(2));

			// Audio stream
			Util.TestStream(dataBlocks[0].CodecStreams[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x960), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1));
			Util.TestOneHeader(dataBlocks[0].CodecStreams[0], Is.EqualTo("Data"), Is.EqualTo(0xB9C), Is.EqualTo(0x960));

			// Video stream before corrupt header
			Util.TestStream(dataBlocks[0].CodecStreams[1], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0x2B46), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1));
			Util.TestFirstHeader(dataBlocks[0].CodecStreams[1], Is.EqualTo("VisualObjectSequenceStart"), Is.EqualTo(0x485), Is.EqualTo(0x5));
			Util.TestLastHeader(dataBlocks[0].CodecStreams[1], Is.EqualTo("Vop"), Is.EqualTo(0x3F2B), Is.EqualTo(0xF7));

			// Video stream after corrupt header
			Util.TestStream(dataBlocks[1], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0x1EADB), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(131));
			Util.TestFirstHeader(dataBlocks[1], Is.EqualTo("Vop"), Is.EqualTo(0x4429), Is.EqualTo(0xA89));
			Util.TestLastHeader(dataBlocks[1], Is.EqualTo("Vop"), Is.EqualTo(0x22282), Is.EqualTo(0xC82));

			File.Delete(invalidMpeg4CopyFileName);
		}

		[Test, Category("Regression")]
		public void TestScanRemainderOfCodecStreamAfterIssueInMpeg4ShortHeaderCodecStream()
		{
			string mpeg4SourceFileName = Util.TestdataPath + "skating-dog.3gp";
			const string invalidMpeg4ShortHeaderCopyFileName = "InvalidMpeg4ShortHeaderCopyFileName";

			try
			{
				// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
				File.Delete(invalidMpeg4ShortHeaderCopyFileName);
				File.Copy(mpeg4SourceFileName, invalidMpeg4ShortHeaderCopyFileName);

				// Step 2: overwrite a byte of the codec stream to make it corrupt;
				using (FileStream fileStream = File.OpenWrite(invalidMpeg4ShortHeaderCopyFileName))
				{
					fileStream.Seek(0x6A9FL, 0L);
					fileStream.WriteByte(0x91);
					fileStream.Close();
				}

				// Step 3: has disappeared;
				// Step 4: scan the file;
				Util.Scan(invalidMpeg4ShortHeaderCopyFileName, _projectIssue2814, "3GPP/QT/MP4", default(KeyValuePair<string, string>), "MPEG-4 Video/H.263", default(KeyValuePair<string, string>));

				// Step 5: validate the result
				// Two codec streams are expected as the result:
				// - one containing the headers before the corrupt header and
				// - one containing the headers after the corrupt header
				Assert.That(_projectIssue2814.GetInputFiles().Count, Is.EqualTo(1));
				IList<IDataBlock> dataBlocks = _projectIssue2814.GetDataBlocks(_projectIssue2814.GetInputFiles()[0]);

				// Note: There may be more than one data block detected, because Defraser will
				//       rescan the 'mdat' block of any corrupt 3GPP file using *ALL* detectors!
				Assert.That(dataBlocks.Count, Is.GreaterThanOrEqualTo(2));

				// Check the data block itself and its content
				Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0), Is.EqualTo(0x6745E), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(3), Is.EqualTo(2));

				// Video stream before corrupt header
				Util.TestStream(dataBlocks[0].CodecStreams[0], Is.EqualTo(CodecID.H263), Is.EqualTo(0x5560), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(37));
				Util.TestFirstHeader(dataBlocks[0].CodecStreams[0], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x1C), Is.EqualTo(0x1C54));
				Util.TestLastHeader(dataBlocks[0].CodecStreams[0], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x6583), Is.EqualTo(0x479));

				// Video stream (separate block after rescanning 'mdat') after corrupt header
				Util.TestStream(dataBlocks[1], Is.EqualTo(CodecID.H263), Is.EqualTo(0x5C7D1), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(652));
				Util.TestFirstHeader(dataBlocks[1], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x6583), Is.EqualTo(0x479));
				Util.TestLastHeader(dataBlocks[1], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x62CB7), Is.EqualTo(0x9D));

				// Audio stream
				Util.TestStream(dataBlocks[0].CodecStreams[1], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x1520), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1));
				Util.TestOneHeader(dataBlocks[0].CodecStreams[1], Is.EqualTo("Data"), Is.EqualTo(0x1C70), Is.EqualTo(0x1520));
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			File.Delete(invalidMpeg4ShortHeaderCopyFileName);
		}
	}
}
