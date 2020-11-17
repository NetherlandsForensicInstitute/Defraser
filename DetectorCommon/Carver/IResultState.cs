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

using Defraser.Interface;

namespace Defraser.Detector.Common.Carver
{
	public interface IResultState : IState
	{
		#region Properties
		// FIXME: getter is a temporary hack!!
		/// <summary>The <see cref="Defraser.Interface.IResult.Name">name</see> of the result.</summary>
		object Name { get; set; }
		#endregion Properties

		/// <summary>
		/// Adds an attribute to the result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">the name of the attribute to add</param>
		/// <param name="value">the value of the attribute</param>
		void AddAttribute<T>(T name, object value);

		/// <summary>
		/// Adds an invalid attribute to the result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">the name of the attribute to add</param>
		/// <param name="value">the value of the attribute</param>
		/// <see cref="IResult.Valid"/>
		void AddInvalidAttribute<T>(T name, object value);

		/// <summary>
		/// Adds an attribute to the result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">the name of the attribute to add</param>
		/// <param name="value">the value of the attribute</param>
		/// <param name="formatter">the formatter used for displaying <paramref name="value"/></param>
		void AddAttribute<T>(T name, object value, IResultFormatter formatter);

		/// <summary>
		/// Parse a new thing = child = recursive
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		/// <param name="reader"></param>
		void Parse<T>(IAttributeParser<T> parser, T reader) where T : IReader;

		// Complete the being constructed <see cref="Defraser.Interface.IResultNode"/>, when you could not fully validate the <see cref="Defraser.Interface.IResultNode"/>, but you do not want to <see cref="IReader.Invalidate"/> the complete stream.
	}
}
