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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Framework
{
	[DataContract]
	public class FragmentContainer : IFragmentContainer
	{
		[DataMember]
		private IList<IFragment> _fragments;

		#region Properties
		public IFragment this[int index]
		{
			get
			{
				PreConditions.Argument("index").Value(index).InRange(0, (_fragments.Count - 1));

				return _fragments[index];
			}
		}
		[DataMember]
		public long Length { get; private set; }
		#endregion Properties

		public FragmentContainer()
		{
			_fragments = new List<IFragment>();
		}

		public void Add(IFragment fragment)
		{
			PreConditions.Argument("fragment").Value(fragment).IsNotNull();

			_fragments.Add(fragment);
			Length += fragment.Length;
		}

		public void Remove(IFragment fragment)
		{
			PreConditions.Argument("fragment").Value(fragment).IsNotNull();

			_fragments.Remove(fragment);
			Length -= fragment.Length;
		}

		public IEnumerator GetEnumerator()
		{
			return _fragments.GetEnumerator();
		}
	}
}
