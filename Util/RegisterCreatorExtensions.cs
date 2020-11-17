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

using Autofac;
using Autofac.Builder;
using Autofac.Registrars;

namespace Defraser.Util
{
	public static class RegisterCreatorExtensions
	{
		public static IConcreteRegistrar RegisterCreator<T>(this ContainerBuilder builder)
		{
			return builder.Register<Creator<T>>(c =>
			{
				IContext threadSafeContext = c.Resolve<IContext>();
				return () => threadSafeContext.Resolve<T>();
			});
		}

		public static IConcreteRegistrar RegisterCreator<T, T1>(this ContainerBuilder builder)
		{
			return builder.Register<Creator<T, T1>>(c =>
			{
				IContext threadSafeContext = c.Resolve<IContext>();
				return x => threadSafeContext.Resolve<T>(new TypedParameter(typeof(T1), x));
			});
		}

		public static IConcreteRegistrar RegisterCreator<T, T1, T2>(this ContainerBuilder builder)
		{
			return builder.Register<Creator<T, T1, T2>>(c =>
			{
				IContext threadSafeContext = c.Resolve<IContext>();
				return (x, y) => threadSafeContext.Resolve<T>(new TypedParameter(typeof(T1), x), new TypedParameter(typeof(T2), y));
			});
		}

		public static IConcreteRegistrar RegisterCreator<T, T1, T2, T3>(this ContainerBuilder builder)
		{
			return builder.Register<Creator<T, T1, T2, T3>>(c =>
			{
				IContext threadSafeContext = c.Resolve<IContext>();
				return (x, y, z) => threadSafeContext.Resolve<T>(new TypedParameter(typeof(T1), x), new TypedParameter(typeof(T2), y), new TypedParameter(typeof(T3), z));
			});
		}

		public static IConcreteRegistrar RegisterCreator<T, T1, T2, T3, T4>(this ContainerBuilder builder)
		{
			return builder.Register<Creator<T, T1, T2, T3, T4>>(c =>
			{
				IContext threadSafeContext = c.Resolve<IContext>();
				return (x, y, z, w) => threadSafeContext.Resolve<T>(new TypedParameter(typeof(T1), x), new TypedParameter(typeof(T2), y), new TypedParameter(typeof(T3), z), new TypedParameter(typeof(T3), w));
			});
		}
	}
}
