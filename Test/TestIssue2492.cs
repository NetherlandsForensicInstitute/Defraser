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
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	/// <summary>
	/// Issue 2492 - 3GPP detector - feed content of media data header to all
	///              selected codecs when chunkoffset header is missing.
	/// </summary>
	[TestFixture]
	public class TestIssue2492FeedMediaDataHeaderToSelectedCodecsWhenChunkOffsetHeaderMissing
	{
		private const string FileNameProjectIssue2492 = @"./issue2492.dpr";
		private IProject _projectIssue2492;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			File.Delete(FileNameProjectIssue2492);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			File.Delete(FileNameProjectIssue2492);
		}

		[SetUp] // Executed for each test in this fixture
		public void CreateProject()
		{
			_projectIssue2492 = TestFramework.ProjectManager.CreateProject(FileNameProjectIssue2492, "S. Holmes", DateTime.Now, "Scan file 1");
		}

		[TearDown] // Executed for each test in this fixture
		public void CloseProject()
		{
			if (_projectIssue2492 != null)
			{
				TestFramework.ProjectManager.CloseProject(_projectIssue2492);
				_projectIssue2492 = null;
			}
		}

		[Test, Category("Regression")]
		public void TestFor3GppContainingShortHeaderMpeg4CodecStream()
		{
			string mpeg4SourceFileName = Util.TestdataPath + "skating-dog.3gp";
			const string invalidMpeg4CopyFileName = "InvalidMpeg4CopyFileName";

			// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
			File.Delete(invalidMpeg4CopyFileName);
			File.Copy(mpeg4SourceFileName, invalidMpeg4CopyFileName);

			// Step 2: overwrite the start code of the chunk offset
			//         codec stream headers to make it corrupt;
			using (FileStream fileStream = File.OpenWrite(invalidMpeg4CopyFileName))
			{
				byte[] zeroArray = new byte[8];
				fileStream.Seek(0x63BF2L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				fileStream.Seek(0x64850L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				fileStream.Close();
			}

			// Step 3: setup the codec detector so that 'Max Offset Between Header = 0';
			// Step 4: scan the file;
			Util.Scan(invalidMpeg4CopyFileName, _projectIssue2492, "3GPP/QT/MP4", default(KeyValuePair<string, string>), "MPEG-4 Video/H.263", default(KeyValuePair<string, string>));

			// Step 5: validate the result
			// Two codec streams are expected as the result:
			// - one containing the headers before the corrupt header and
			// - one containing the headers after the corrupt header
			Assert.That(_projectIssue2492.GetInputFiles().Count, Is.EqualTo(1));
			IList<IDataBlock> dataBlocks = _projectIssue2492.GetDataBlocks(_projectIssue2492.GetInputFiles()[0]);
			Assert.That(dataBlocks.Count, Is.GreaterThanOrEqualTo(5));

			// Note: The flavor of the 3GP data blocks is unknown, e.g. MPEG-4, QuickTime.

			// Check the first data block itself and its content
			Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0), Is.EqualTo(0x63BF2L), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(3), Is.EqualTo(0));

			// Check the second data block containing the H263 result
			Util.TestStream(dataBlocks[1], Is.EqualTo(CodecID.H263), Is.EqualTo(0x1C), Is.EqualTo(0x62D38), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(689), Is.EqualTo(0));
			Util.TestFirstHeader(dataBlocks[1], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x1C), Is.EqualTo(0x1C54));
			Util.TestLastHeader(dataBlocks[1], Is.EqualTo("VopWithShortHeader"), Is.EqualTo(0x62CB7), Is.EqualTo(0x9D));

			// Check the third data block
			Util.TestStream(dataBlocks[2], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x62D54), Is.EqualTo(0xE9E), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1/*20*/), Is.EqualTo(0));

			// Check the fourth data block
			Util.TestStream(dataBlocks[3], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x646C6), Is.EqualTo(0x18A), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1/*17*/), Is.EqualTo(0));

			// Check the fifth data block
			Util.TestStream(dataBlocks[4], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x6743C), Is.EqualTo(0x22), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1/*2*/), Is.EqualTo(0));

			File.Delete(invalidMpeg4CopyFileName);
		}

		[Test, Category("Regression")]
		public void TestFor3GppContainingMpeg4CodecStreamWithEsds()
		{
			string mpeg4SourceFileName = Util.TestdataPath + "3GP_mp4v.3gp";
			const string invalidMpeg4CopyFileName = "InvalidMpeg4CopyFileName";

			// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
			File.Delete(invalidMpeg4CopyFileName);
			File.Copy(mpeg4SourceFileName, invalidMpeg4CopyFileName);

			// Step 2: overwrite the start code of the chunk offset
			//         codec stream headers to make it corrupt;
			using (FileStream fileStream = File.OpenWrite(invalidMpeg4CopyFileName))
			{
				byte[] zeroArray = new byte[8];
				// Chunk offset header 1
				fileStream.Seek(0x247L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				// Chunk offset header 2
				fileStream.Seek(0xA78L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				fileStream.Close();
			}

			// Step 3: setup the codec detector so that 'Max Offset Between Header = 0';
			// Step 4: scan the file;
			Util.Scan(invalidMpeg4CopyFileName, _projectIssue2492, "3GPP/QT/MP4", default(KeyValuePair<string, string>), "MPEG-4 Video/H.263", default(KeyValuePair<string, string>));

			// Step 5: validate the result
			// Two codec streams are expected as the result:
			// - one containing the headers before the corrupt header and
			// - one containing the headers after the corrupt header
			Assert.That(_projectIssue2492.GetInputFiles().Count, Is.EqualTo(1));
			IList<IDataBlock> dataBlocks = _projectIssue2492.GetDataBlocks(_projectIssue2492.GetInputFiles()[0]);
			Assert.That(dataBlocks.Count, Is.GreaterThanOrEqualTo(5));

			// Check the first data block
			Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0), Is.EqualTo(0x247), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(2), Is.EqualTo(0));

			// Check the second data block and its content
			Util.TestStream(dataBlocks[1], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x2CB), Is.EqualTo(0x7AD), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1), Is.EqualTo(1));
			Util.TestStream(dataBlocks[1].CodecStreams[0], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0x20), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1));
			Util.TestFirstHeader(dataBlocks[1].CodecStreams[0], Is.EqualTo("VisualObjectSequenceStart"), Is.EqualTo(0x485), Is.EqualTo(0x5));
			Util.TestLastHeader(dataBlocks[1].CodecStreams[0], Is.EqualTo("VideoObjectLayer"), Is.EqualTo(0x497), Is.EqualTo(0xE));

			// Check the third data block
			Util.TestStream(dataBlocks[2], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0xB74), Is.EqualTo(0x18), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(3), Is.EqualTo(0));

			// Check the fourth data block
			Util.TestStream(dataBlocks[3], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0xB8C), Is.EqualTo(0x10), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(2), Is.EqualTo(0));

			// Check the fifth data block containing the result of rescanning the 'mdat' block
			Util.TestStream(dataBlocks[4], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0x22048), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(146));
			Util.TestFirstHeader(dataBlocks[4], Is.EqualTo("Vop"), Is.EqualTo(0xEBC), Is.EqualTo(0xB51));
			Util.TestLastHeader(dataBlocks[4], Is.EqualTo("Vop"), Is.EqualTo(0x22282), Is.EqualTo(0xC82));

			File.Delete(invalidMpeg4CopyFileName);
		}

		[Test, Category("Regression")]
		public void TestFor3GppContainingMpeg4CodecStreamWithoutEsds()
		{
			string mpeg4SourceFileName = Util.TestdataPath + "3GP_mp4v.3gp";
			const string invalidMpeg4CopyFileName = "InvalidMpeg4CopyFileName";

			// Step 1: make a copy of a movie containing a valid MPEG-4 short header codec stream;
			File.Delete(invalidMpeg4CopyFileName);
			File.Copy(mpeg4SourceFileName, invalidMpeg4CopyFileName);

			// Step 2: overwrite the start code of the chunk offset
			//         codec stream headers to make it corrupt;
			using (FileStream fileStream = File.OpenWrite(invalidMpeg4CopyFileName))
			{
				byte[] zeroArray = new byte[8];
				// Chunk offset header 1
				fileStream.Seek(0x247L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				// Chunk offset header 2
				fileStream.Seek(0xA78L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				// Elementary Stream Descriptor (esds)
				fileStream.Seek(0x463L, 0L);
				fileStream.Write(zeroArray, 0, zeroArray.Length);
				fileStream.Close();
			}

			// Step 3: setup the codec detector so that 'Max Offset Between Header = 0';
			// Step 4: scan the file;
			Util.Scan(invalidMpeg4CopyFileName, _projectIssue2492, "3GPP/QT/MP4", default(KeyValuePair<string, string>), "MPEG-4 Video/H.263", default(KeyValuePair<string, string>));

			// Step 5: validate the result
			// Two codec streams are expected as the result:
			// - one containing the headers before the corrupt header and
			// - one containing the headers after the corrupt header
			Assert.That(_projectIssue2492.GetInputFiles().Count, Is.EqualTo(1));
			IList<IDataBlock> dataBlocks = _projectIssue2492.GetDataBlocks(_projectIssue2492.GetInputFiles()[0]);
			Assert.That(dataBlocks.Count, Is.GreaterThanOrEqualTo(7));

			// Check the four first data blocks
			Util.TestStream(dataBlocks[0], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0), Is.EqualTo(0x247), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(2), Is.EqualTo(0));
			Util.TestStream(dataBlocks[1], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x2CB), Is.EqualTo(0x198), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1), Is.EqualTo(0));
			Util.TestStream(dataBlocks[2], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0x485), Is.EqualTo(0x20), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(1), Is.EqualTo(0));
			Util.TestStream(dataBlocks[3], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0x4A8), Is.EqualTo(0x5D0), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(4), Is.EqualTo(0));

			// Check the last three data blocks
			Util.TestStream(dataBlocks[4], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0xB74), Is.EqualTo(0x18), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(3), Is.EqualTo(0));
			// TODO: Why is the stream broken up here!?
			Util.TestStream(dataBlocks[5], Is.EqualTo(CodecID.Unknown), Is.EqualTo(0xB8C), Is.EqualTo(0x10), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(2), Is.EqualTo(0));
			Util.TestStream(dataBlocks[6], Is.EqualTo(CodecID.Mpeg4Video), Is.EqualTo(0xEBC), Is.EqualTo(0x22048), Is.False, Is.Null, Is.EqualTo(0), Is.EqualTo(146), Is.EqualTo(0));
			Util.TestFirstHeader(dataBlocks[6], Is.EqualTo("Vop"), Is.EqualTo(0xEBC), Is.EqualTo(0xB51));
			Util.TestLastHeader(dataBlocks[6], Is.EqualTo("Vop"), Is.EqualTo(0x22282), Is.EqualTo(0xC82));

			File.Delete(invalidMpeg4CopyFileName);
		}
	}
}
