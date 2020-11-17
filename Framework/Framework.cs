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
using System.Reflection;

namespace Defraser
{
	/// <summary>
	/// Provides utility methods to the framework.
	/// </summary>
	public static class Framework
	{
		#region Properties
		public static string ApplicationName
		{
			get
			{
				if (Assembly.GetEntryAssembly() == null) return "<application name>"; // Loaded from unmanaged application
				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static string ApplicationVersion
		{
			get
			{
				if (Assembly.GetEntryAssembly() == null) return "<version>"; // Loaded from unmanaged application

				// Return only the first 3 digits of the version number
				string fourDigitVersionString = Assembly.GetEntryAssembly().GetName().Version.ToString();
				string threeDigitVersionString = fourDigitVersionString.Substring(0, fourDigitVersionString.LastIndexOf('.'));
				return threeDigitVersionString;
			}
		}
		#endregion Properties

		/// <summary>
		/// Writes <paramref name="dataPacket"/> to <paramref name="fileName"/>.
		/// </summary>
		/// <param name="dataPacket">the data packet to write</param>
		/// <param name="fileName">the name of the file to write to</param>
		public static void WriteDataPacket(IDataPacket dataPacket, IDataReaderPool dataReaderPool, string fileName)
		{
			byte[] buffer = new byte[512 * 1024];

			using (FileStream outputFileStream = new FileStream(fileName, FileMode.Create))
			using (IDataReader dataReader = dataReaderPool.CreateDataReader(dataPacket))
			{
				int count;

				while ((count = dataReader.Read(buffer, 0, buffer.Length)) > 0)
				{
					outputFileStream.Write(buffer, 0, count);
					dataReader.Position += count;
				}
			}
		}
	}
}
