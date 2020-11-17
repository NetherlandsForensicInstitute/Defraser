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
using Defraser.Detector.Common;

namespace Defraser.Detector.Common
{
	public sealed class DefaultDetectorColumnsBuilder : IDetectorColumnsBuilder
	{
		private readonly IDetectorColumnsBuilder _builder;
		private readonly string[] _defaultColumns;

		public DefaultDetectorColumnsBuilder(IDetectorColumnsBuilder builder, string[] defaultColumns)
		{
			_builder = builder;
			_defaultColumns = defaultColumns;
		}

		public IDetectorColumnsBuilder WithDefaultColumns(string[] defaultColumns)
		{
			return new DefaultDetectorColumnsBuilder(this, defaultColumns);
		}

		public void AddColumnNames(string header, string[] columns)
		{
			_builder.AddColumnNames(header, MergeArrays(_defaultColumns, columns));
		}

		private static T[] MergeArrays<T>(T[] firstArray, T[] secondArray)
		{
			var mergedArray = new T[firstArray.Length + secondArray.Length];
			Array.Copy(firstArray, mergedArray, firstArray.Length);
			Array.Copy(secondArray, 0, mergedArray, firstArray.Length, secondArray.Length);
			return mergedArray;
		}

		public IDetectorColumns Build()
		{
			return _builder.Build();
		}
	}
}
