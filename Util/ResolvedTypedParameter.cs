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
using System.Reflection;
using Autofac;

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="ResolvedTypedParameter"/> class combines the functionality
	/// of <see cref="ResolvedParameter"/> and <see cref="TypedParameter"/>.
	/// It is effectively a <see cref="TypedParameter"/> where the value is supplied
	/// by a <i>valueProvider</i> delegate. This function supplies a value given
	/// the <see cref="IContext"/>, similar to <see cref="ResolvedParameter"/>.
	/// </summary>
	public sealed class ResolvedTypedParameter : Parameter
	{
		private readonly Func<IContext, object> _valueProvider;

		#region Properties
		/// <summary>The type of (constructor) parameters to match.</summary>
		public Type Type { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new <see cref="ResolvedTypedParameter"/>.
		/// </summary>
		/// <param name="type">The type of (constructor) parameters to match</param>
		/// <param name="valueProvider">The function that supplies the value given the context</param>
		public ResolvedTypedParameter(Type type, Func<IContext, object> valueProvider)
		{
			Type = type;
			_valueProvider = valueProvider;
		}

		public override bool CanSupplyValue(ParameterInfo pi, IContext context, out Func<object> valueProvider)
		{
			if (pi.ParameterType.IsAssignableFrom(Type))
			{
				valueProvider = () => _valueProvider(context);
				return true;
			}
			else
			{
				valueProvider = null;
				return false;
			}
		}
	}
}
