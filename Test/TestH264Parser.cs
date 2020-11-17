/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Detector.H264;
using Defraser.Detector.H264.State;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class TestH264Parser
	{
		private IH264State _state;
		private IResultNodeState _resultState;

		[SetUp]
		public void SetUp()
		{
			_state = new H264State(new SequenceStates(), new PictureStates());
			_resultState = MockRepository.GenerateStub<IResultNodeState>();
		}

		[Test]
		public void TestGetExpGolombCoded()
		{
			Assert.That(GetExpGolombCoded(0x80), Is.EqualTo(0));
			Assert.That(GetExpGolombCoded(0xFF), Is.EqualTo(0));
			Assert.That(GetExpGolombCoded(0x40), Is.EqualTo(1));
			Assert.That(GetExpGolombCoded(0x5F), Is.EqualTo(1));
			Assert.That(GetExpGolombCoded(0x60), Is.EqualTo(2));
			Assert.That(GetExpGolombCoded(0x20), Is.EqualTo(3));
			Assert.That(GetExpGolombCoded(0x27), Is.EqualTo(3));
			Assert.That(GetExpGolombCoded(0x28), Is.EqualTo(4));
			Assert.That(GetExpGolombCoded(0x30), Is.EqualTo(5));
			Assert.That(GetExpGolombCoded(0x38), Is.EqualTo(6));
			Assert.That(GetExpGolombCoded(0x10), Is.EqualTo(7));
			Assert.That(GetExpGolombCoded(0x12), Is.EqualTo(8));
			Assert.That(GetExpGolombCoded(0x14), Is.EqualTo(9));
			Assert.That(GetExpGolombCoded(0x14), Is.EqualTo(9));
			Assert.That(GetExpGolombCoded(0x00, 0x01, 0xff, 0xff), Is.EqualTo(65534));//0(x15)1(x15) 2^16-2
		}

		[Test]
		public void TestGetSignedExpGolombCoded()
		{
			Assert.That(GetSignedExpGolombCoded(0x80), Is.EqualTo(0));
			Assert.That(GetSignedExpGolombCoded(0xFF), Is.EqualTo(0));
			Assert.That(GetSignedExpGolombCoded(0x40), Is.EqualTo(1));
			Assert.That(GetSignedExpGolombCoded(0x5F), Is.EqualTo(1));	// 1
			Assert.That(GetSignedExpGolombCoded(0x60), Is.EqualTo(-1));	// 2
			Assert.That(GetSignedExpGolombCoded(0x20), Is.EqualTo(2));	// 3
			Assert.That(GetSignedExpGolombCoded(0x27), Is.EqualTo(2));	// 3
			Assert.That(GetSignedExpGolombCoded(0x28), Is.EqualTo(-2));	// 4
			Assert.That(GetSignedExpGolombCoded(0x30), Is.EqualTo(3));	// 5
			Assert.That(GetSignedExpGolombCoded(0x38), Is.EqualTo(-3));	// 6
			Assert.That(GetSignedExpGolombCoded(0x10), Is.EqualTo(4));	// 7
			Assert.That(GetSignedExpGolombCoded(0x12), Is.EqualTo(-4));	// 8
			Assert.That(GetSignedExpGolombCoded(0x14), Is.EqualTo(5));	// 9
			Assert.That(GetSignedExpGolombCoded(0x00, 0x01, 0xff, 0xff), Is.EqualTo(-32767));//0(x15)1(x15) 2^15-1
		}

		private uint GetExpGolombCoded(params byte[] bitstream)
		{
			IDataReader dataReader = new MockDataReader(bitstream);
			var bitStreamDataReader = new BitStreamDataReader(dataReader);
			INalUnitReader reader = new NalUnitReader(null, bitStreamDataReader, _state);
			reader.Result = _resultState;
			return reader.GetExpGolombCoded();
		}

		private int GetSignedExpGolombCoded(params byte[] bitstream)
		{
			IDataReader dataReader = new MockDataReader(bitstream);
			var bitStreamDataReader = new BitStreamDataReader(dataReader);
			INalUnitReader reader = new NalUnitReader(null, bitStreamDataReader, _state);
			reader.Result = _resultState;
			return reader.GetSignedExpGolombCoded();
		}
	}
}
