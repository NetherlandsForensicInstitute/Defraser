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
using System.Runtime.Serialization;

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="ISerializationContext"/> provides dependencies for objects
	/// a certain <see cref="Type"/> during deserialization.
	/// <para>
	/// It is more powerful than using the <see cref="OnDeserializedAttribute"/>,
	/// since the dependencies for the serialization context itself can be resolved
	/// using Dependency Injection by registering a serialization context with the
	/// IoC container. Using the <see cref="OnDeserializedAttribute"/> would require
	/// a service locator to perform such a task.
	/// </para>
	public interface ISerializationContext
	{
		#region Properties
		/// <summary>The <see cref="Type"/> of objects supported by this context.</summary>
		Type Type { get; }
		#endregion Properties

		/// <summary>
		/// Completes deserialization of object <paramref name="obj"/>.
		/// This generally injects the dependencies that could not be serialized
		/// into object <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj">The object being deserialized</param>
		void CompleteDeserialization(object obj);
	}
}
