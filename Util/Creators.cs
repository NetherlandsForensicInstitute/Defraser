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

namespace Defraser.Util
{
	/// <summary>
	/// The factory method for creating objects of type <typeparamref name="T"/>.
	/// <param></param>
	/// <typeparam name="T">The type of object to create</typeparam>
	/// <returns>The newly created object</returns>
	public delegate T Creator<T>();

	/// <summary>
	/// The factory method for creating objects of type <typeparamref name="T"/>.
	/// using a single <paramref name="arg1"/>.
	/// <param></param>
	/// <typeparam name="T">The type of object to create</typeparam>
	/// <typeparam name="T1">The type of the constructor argument</typeparam>
	/// <param name="arg1">The argument for the object to create</param>
	/// <returns>The newly created object</returns>
	public delegate T Creator<T, T1>(T1 arg1);

	/// <summary>
	/// The factory method for creating objects of type <typeparamref name="T"/>.
	/// using arguments <paramref name="arg1"/> and <paramref name="arg2"/>.
	/// <param></param>
	/// <typeparam name="T">The type of object to create</typeparam>
	/// <typeparam name="T1">The type of the first constructor argument</typeparam>
	/// <typeparam name="T2">The type of the second constructor argument</typeparam>
	/// <param name="arg1">The first argument for the object to create</param>
	/// <param name="arg2">The second argument for the object to create</param>
	/// <returns>The newly created object</returns>
	public delegate T Creator<T, T1, T2>(T1 arg1, T2 arg2);

	/// <summary>
	/// The factory method for creating objects of type <typeparamref name="T"/>.
	/// using arguments <paramref name="arg1"/>, <paramref name="arg2"/> and
	/// <paramref name="arg3"/>.
	/// <param></param>
	/// <typeparam name="T">The type of object to create</typeparam>
	/// <typeparam name="T1">The type of the first constructor argument</typeparam>
	/// <typeparam name="T2">The type of the second constructor argument</typeparam>
	/// <typeparam name="T3">The type of the third constructor argument</typeparam>
	/// <param name="arg1">The first argument for the object to create</param>
	/// <param name="arg2">The second argument for the object to create</param>
	/// <param name="arg3">The third argument for the object to create</param>
	/// <returns>The newly created object</returns>
	public delegate T Creator<T, T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

	/// <summary>
	/// The factory method for creating objects of type <typeparamref name="T"/>.
	/// using arguments <paramref name="arg1"/> and <paramref name="arg2"/>,
	/// <paramref name="arg3"/> and <paramref name="arg4"/>.
	/// <param></param>
	/// <typeparam name="T">The type of object to create</typeparam>
	/// <typeparam name="T1">The type of the first constructor argument</typeparam>
	/// <typeparam name="T2">The type of the second constructor argument</typeparam>
	/// <typeparam name="T3">The type of the third constructor argument</typeparam>
	/// <typeparam name="T3">The type of the fourth constructor argument</typeparam>
	/// <param name="arg1">The first argument for the object to create</param>
	/// <param name="arg2">The second argument for the object to create</param>
	/// <param name="arg3">The third argument for the object to create</param>
	/// <param name="arg3">The fourth argument for the object to create</param>
	/// <returns>The newly created object</returns>
	public delegate T Creator<T, T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
