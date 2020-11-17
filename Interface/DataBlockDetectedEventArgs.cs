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
using Defraser.Util;

namespace Defraser.Interface
{
	public sealed class DataBlockDetectedEventArgs : EventArgs, IEquatable<DataBlockDetectedEventArgs>
	{
		#region Properties
		/// <value>The data block that was detected.</value>
		public IDataBlock DataBlock { get; private set; }
		/// <value>
		/// Double-scan the payload, because the codec streams could not be validated
		/// and might be corrupt and/or contain other results.
		/// </value>
		public bool DoubleScanPayload { get; set; }
		#endregion Properties

		/// <summary>
		/// Creates a new event args of the given <paramref name="dataBlock"/>.
		/// </summary>
		/// <param name="dataBlock">the data block that was detected</param>
		public DataBlockDetectedEventArgs(IDataBlock dataBlock)
		{
			PreConditions.Argument("dataBlock").Value(dataBlock).IsNotNull();

			DataBlock = dataBlock;
		}

		public override string ToString()
		{
			return string.Format("{0}[DataBlock={1}]", GetType().Name, DataBlock);
		}

		#region Equals method
		public override bool Equals(object obj)
		{
			return Equals(obj as DataBlockDetectedEventArgs);
		}

		public bool Equals(DataBlockDetectedEventArgs other)
		{
			if (other == null) return false;
			if (other == this) return true;

			return other.DataBlock.Equals(DataBlock) && (other.DoubleScanPayload == DoubleScanPayload);
		}

		public override int GetHashCode()
		{
			return DataBlock.GetHashCode() ^ (DoubleScanPayload ? 1 : 0);
		}
		#endregion Equals method
	}
}
