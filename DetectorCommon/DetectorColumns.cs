/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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
using System.Linq;
using System.Text;

namespace Defraser.Detector.Common
{
	public class DetectorColumns<THeader, THeaderName> : IDetectorColumns
	{
		public IDictionary<string, string[]> Columns { get; private set; }

		/// <summary>
		/// Retrieves the columns names for each header using reflection.
		///
		/// This method will search for an enumeration named <c>Attribute</c>
		/// in the header classes of the detector's assembly.
		/// The header classes are found using reflection and named according
		/// to the header names listed in <typeparamref name="THeaderName"/>.
		/// Subclasses inherit the attributes of their base class.
		///
		/// Unlisted headers will use the attributes of <typeparamref name="THeaderName"/>.
		/// </summary>
		/// <typeparam name="THeaderName">the header name enumeration type</typeparam>
		/// <typeparam name="THeader">the header type</typeparam>
		/// <returns>the column names</returns>
		public DetectorColumns()
		{
			Type enumType = typeof (THeaderName);
			string detectorNamespace = enumType.Namespace;
			string detectorAssembly = enumType.Assembly.FullName;

			// Use reflection to find attributes for each header
			Dictionary<string, string[]> columns = new Dictionary<string, string[]>();
			foreach (THeaderName headerName in Enum.GetValues(typeof (THeaderName)))
			{
				string name = Enum.GetName(enumType, headerName);
				if (name == "Root")
				{
					// Root nodes do not have attributes
					continue;
				}

				// Find header type, use base header type if not found
				Type headerType = Type.GetType(detectorNamespace + "." + name + "," + detectorAssembly);
				if (headerType == null)
				{
					headerType = typeof (THeader);
				}

				// Recursively find attributes in header class and its base classes
				List<string> attributes = new List<string>();
				while (headerType != null && typeof (THeader).IsAssignableFrom(headerType))
				{
					Type attributeType = headerType.GetNestedType("Attribute");
					if (attributeType != null && attributeType.IsEnum)
					{
						attributes.AddRange(Enum.GetNames(attributeType));
					}
					headerType = headerType.BaseType;
				}

				columns.Add(name, attributes.ToArray());
			}
			Columns = columns;
		}
	}
}
