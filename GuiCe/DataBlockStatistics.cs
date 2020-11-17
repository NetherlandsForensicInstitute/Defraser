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
using System.Collections.ObjectModel;
using Defraser.Interface;
using Defraser.Util;
using Infralution.Controls.VirtualTree;

namespace Defraser.GuiCe
{
	class DataBlockStatistics : DetailsBase
	{
		override internal string Title { get { return "Data block header statistics"; } }
		override internal string Column0Caption { get { return "Name"; } }
		override internal string Column1Caption { get { return "Count"; } }

		private IDataBlock _dataBlock;

		public DataBlockStatistics(IDataBlock dataBlock)
		{
			_dataBlock = dataBlock;
		}

		override internal IList GetChildrenForRow(Row row)
		{
			IDataBlock dataBlock = row.Item as IDataBlock;
			string detail = row.Item as string;

			if (dataBlock != null)
			{
				IList<Pair<string, string>> dataBlockDetails = new List<Pair<string, string>>();

				// Header / count
				// TODO replace dummy data
				dataBlockDetails.Add(new Pair<string, string>("Groep of picture headers", "1234"));
				dataBlockDetails.Add(new Pair<string, string>("Sequence start", "3"));
				dataBlockDetails.Add(new Pair<string, string>("VOP", "10000"));

				return new ReadOnlyCollection<Pair<string, string>>(dataBlockDetails);
			}
			return null;
		}
	}
}
