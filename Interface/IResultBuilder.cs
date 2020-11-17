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
	/// <summary>
	/// The base interface for <see cref="IResult"/> builders.
	/// </summary>
	/// <typeparam name="T">The type of result being built</typeparam>
	/// <seealso cref="IResultNodeBuilder"/>
	/// <seealso cref="IResultAttributeBuilder"/>
	public interface IResultBuilder<T> : IBuilder<T> where T : IResult
	{
		#region Properties
		/// <summary>The <see cref="IResult.Name">name</see> of the result.</summary>
		string Name { set; }
		#endregion Properties

		/// <summary>
		/// Resets this builder.
		/// This resets the <i>Valid</i> property to <code>true</code> and removes
		/// any attributes that have were <see cref="AddAttribute">added</see>.
		/// </summary>
		/// <remarks>
		/// Other properties, such as <see cref="Name"/>, are left untouched,
		/// as these are directly accessible through their corresponding setters.
		/// </remarks>
		void Reset();

		/// <summary>
		/// Invalidates the result being built.
		/// </summary>
		/// <seealso cref="IResult.Valid"/>
		void Invalidate();

		/// <summary>
		/// Adds the given <paramref name="attribute"/> to the result.
		/// </summary>
		/// <param name="attribute">the attribute to add</param>
		void AddAttribute(IResultAttribute attribute);
	}
}
