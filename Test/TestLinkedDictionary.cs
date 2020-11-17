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
using Defraser.Interface;
using Defraser.Util;
using NUnit.Framework;

namespace Defraser.Test
{
	/// <summary>
	/// Unit tests for the <see cref="LinkedDictionary{TKey,T}"/> class.
	/// </summary>
	[TestFixture]
	public class TestLinkedDictionary
	{
		[Test]
		public void TestThatSameMetadataCollectionsAreEqual()
		{
			var metadata1 = new LinkedDictionary<ProjectMetadataKey, string>();
			IDictionary<ProjectMetadataKey, string> metadata2 = new Dictionary<ProjectMetadataKey, string>();

			metadata1[ProjectMetadataKey.DateCreated] = "20090629";
			metadata1[ProjectMetadataKey.DateLastModified] = "20090629";
			metadata1[ProjectMetadataKey.FileVersion] = "123";
			metadata1[ProjectMetadataKey.InvestigatorName] = "foo";

			metadata2[ProjectMetadataKey.DateCreated] = "20090629";
			metadata2[ProjectMetadataKey.DateLastModified] = "20090629";
			metadata2[ProjectMetadataKey.FileVersion] = "123";
			metadata2[ProjectMetadataKey.InvestigatorName] = "foo";

			Assert.IsTrue(metadata1.IsEqual(metadata2));
		}

		[Test]
		public void TestThatDifferentMetadataCollectionsAreNotEqual()
		{
			var metadata1 = new LinkedDictionary<ProjectMetadataKey, string>();
			IDictionary<ProjectMetadataKey, string> metadata2 = new Dictionary<ProjectMetadataKey, string>();

			metadata1[ProjectMetadataKey.DateCreated] = "20090629";
			metadata1[ProjectMetadataKey.DateLastModified] = "20090629";
			metadata1[ProjectMetadataKey.FileVersion] = "123";
			metadata1[ProjectMetadataKey.InvestigatorName] = "foo";

			metadata2[ProjectMetadataKey.DateCreated] = "20090629";
			metadata2[ProjectMetadataKey.DateLastModified] = "20090629";
			metadata2[ProjectMetadataKey.FileVersion] = "123";
			metadata2[ProjectMetadataKey.InvestigatorName] = "foobar";

			Assert.IsFalse(metadata1.IsEqual(metadata2));
		}
	}
}
