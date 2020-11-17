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

using System.Collections.Generic;
using Defraser.GuiCe;
using Defraser.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Defraser.Test
{
	[TestFixture]
	public class ThumbUtilTest
	{

		[Test]
		public void ZeroThumbnailsIfZeroAvailable()
		{
			Assert.AreEqual(0, ThumbUtil.CheckMaxThumbCount(new List<IResultNode>(), 10).Count);
		}

		[Test]
		public void ZeroThumbnailsIfZeroWanted()
		{
			Assert.AreEqual(0,ThumbUtil.CheckMaxThumbCount(new List<IResultNode>(new []{MockRepository.GenerateStub<IResultNode>()}), 0 ).Count);
		}

		[Test]
		public void ThumbSelectionKeepsHeadAndLast()
		{
			var thumbs = FillWith(10);
			var filtered = ThumbUtil.CheckMaxThumbCount(thumbs, 5);
			Assert.AreSame(thumbs[0], filtered[0]);
			Assert.AreSame(thumbs[9], filtered[4]);
			Assert.AreEqual(5,filtered.Count);
		}

		[Test]
		public void ThumbSelectionWithOnlyHeadAndLast()
		{
			var thumbs = FillWith(3);
			var filtered = ThumbUtil.CheckMaxThumbCount(thumbs, 2);
			Assert.AreSame(thumbs[0], filtered[0]);
			Assert.AreSame(thumbs[2], filtered[1]);
			Assert.AreEqual(2,filtered.Count);
		}

		[Test]
		public void ThumbSelectionRemovesCorrectOnes()
		{
			var thumbs = FillWith(5);
			var filtered = ThumbUtil.CheckMaxThumbCount(thumbs, 3);
			Assert.AreSame(thumbs[0], filtered[0]);
			Assert.AreSame(thumbs[2], filtered[1]);
			Assert.AreSame(thumbs[4], filtered[2]);
			Assert.AreEqual(3,filtered.Count);
		}

		[Test]
		public void ThumbSelectionRemovesCorrectOnes2()
		{
			var thumbs = FillWith(7);
			var filtered = ThumbUtil.CheckMaxThumbCount(thumbs, 4);
			Assert.AreSame(thumbs[0], filtered[0]);
			Assert.AreSame(thumbs[2], filtered[1]);
			Assert.AreSame(thumbs[4], filtered[2]);
			Assert.AreSame(thumbs[6], filtered[3]);
			Assert.AreEqual(4,filtered.Count);
		}

		[Test]
		public void ThumbSelectionRemovesCorrectOnes3()
		{
			var thumbs = FillWith(7);
			var filtered = ThumbUtil.CheckMaxThumbCount(thumbs, 3);
			Assert.AreSame(thumbs[0], filtered[0]);
			Assert.AreSame(thumbs[3], filtered[1]);
			Assert.AreSame(thumbs[6], filtered[2]);
			Assert.AreEqual(3, filtered.Count);
		}

		private static List<IResultNode> FillWith(int count)
		{
			var thumbs = new List<IResultNode>();
			for (int i = 0; i < count; i++)
			{
				thumbs.Add(MockRepository.GenerateStub<IResultNode>());
			}
			return thumbs;
		}
	}
}
