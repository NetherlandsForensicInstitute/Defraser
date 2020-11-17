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
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestEnum
	{
		[Flags]
		private enum MockFlag
		{
			FirstFlag = 0x1,
			SecondFlag = 0x4
		}

		[Test]
		public void TestIsFlagSet()
		{
			Assert.IsTrue(IsFlagSet(0x7, MockFlag.FirstFlag));
			Assert.IsTrue(IsFlagSet(0x7, MockFlag.SecondFlag));
			Assert.IsFalse(IsFlagSet(0x2, MockFlag.SecondFlag));
		}

		/// <summary>
		/// Returns whether the given <paramref name="flag"/> is set in <paramref name="flags"/>.
		/// </summary>
		/// <param name="flags">the flags</param>
		/// <param name="flag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		private static bool IsFlagSet(uint flags, object flag)
		{
			return (flags & (int)flag) != 0;
		}
	}
}
