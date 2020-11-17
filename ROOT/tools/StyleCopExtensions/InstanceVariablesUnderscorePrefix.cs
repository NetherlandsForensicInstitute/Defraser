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

using Microsoft.StyleCop;
using Microsoft.StyleCop.CSharp;

namespace StyleCopExtensions
{
	/// <summary>
	/// This StyleCop Rule makes sure that instance variables are prefixed with an underscore.
	/// <see cref="http://scottwhite.blogspot.com/2008/11/creating-custom-stylecop-rules-in-c.html"/>
	/// </summary>
	[SourceAnalyzer(typeof(CsParser))]
	public class InstanceVariablesUnderscorePrefix : SourceAnalyzer
	{
		public override void AnalyzeDocument(CodeDocument codeDocument)
		{
			Param.RequireNotNull(codeDocument, "codeDocument");

			CsDocument csharpDocument = (CsDocument)codeDocument;

			if (csharpDocument.RootElement != null && !csharpDocument.RootElement.Generated)
			{
				csharpDocument.WalkDocument(VisitElement, null, null);
			}
		}

		/// <summary>
		/// The VisitElement function handles the main logic for this rule.
		/// First it makes sure that the member isn't Generated code, which you
		/// have no control over so no sense in applying rules to. Secondly
		/// it's making sure that the member's visibility isn't Public or
		/// Internal. Finally we make sure that the instance variable starts
		/// with an underscore prefix.
		/// </summary>
		/// <param name="element">the element the rule is applied on</param>
		/// <param name="parentElement"></param>
		/// <param name="context"></param>
		/// <returns>true</returns>
		private bool VisitElement(CsElement element, CsElement parentElement, object context)
		{
			// Flag a violation if the instance variables are not prefixed with an underscore.
			if (!element.Generated &&
				element.ElementType == ElementType.Field &&
				element.ActualAccess != AccessModifierType.Public &&
				element.ActualAccess != AccessModifierType.Internal &&
				element.Declaration.Name.ToCharArray()[0] != '_')
			{
				AddViolation(element, "InstanceVariablesUnderscorePrefix");
			}
			return true;
		}
	}
}
