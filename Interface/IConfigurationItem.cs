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

namespace Defraser.Interface
{
	public interface IConfigurationItem
	{
		object DefaultValue { get; }
		object UserValue { get; /* Use SetUserValue to set the user value */ }
		object Value { get; }
		string Description { get; }
		bool HasUserValueSpecified { get; }

		/// <summary>
		/// Reset the UserValue, so that the DefaultValue will be used again.
		/// </summary>
		void ResetDefault();

		/// <summary>
		/// Check <paramref name="value"/> if it can be parsed to the same type as <code>DefaultValue</code>
		/// </summary>
		/// <param name="value">The value to check</param>
		/// <returns>true if the value can be parsed to the same type as <code>DefaultValue</code></returns>
		bool IsValidUserInput(string value);

		/// <summary>
		/// Parse <paramref name="value"/> to the same type as <code>DefaultValue</code> and write
		/// the result to <code>UserValue</code>.
		/// </summary>
		/// <param name="value">The string to parse</param>
		void SetUserValue(string value);
	}
}
