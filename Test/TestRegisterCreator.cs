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
using System.Threading;
using Autofac;
using Autofac.Builder;
using Defraser.Util;
using NUnit.Framework;

namespace Defraser.Test
{
	[TestFixture]
	public class TestRegisterCreator
	{
		#region Inner classes
		private sealed class Dummy
		{
			public Dummy()
			{
			}
		}

		private sealed class Dependency
		{
			public Dependency()
			{
			}
		}
		#endregion Inner classes

		#region Test data
		/// <summary>The number of threads to use for the test.</summary>
		private const int NumberOfThreads = 16;
		#endregion Test data

		#region Objects under test
		private IContainer _container;
		private Creator<Dummy> _createDummy;
		#endregion Objects under test

		#region Test initialization and cleanup
		[SetUp]
		public void SetUp()
		{
			var builder = new ContainerBuilder();

			builder.Register<Dummy>().FactoryScoped();
			builder.Register<Dependency>().FactoryScoped();

			builder.RegisterCreator<Dummy>();

			_container = builder.Build();
			_createDummy = _container.Resolve<Creator<Dummy>>();
		}

		[TearDown]
		public void TearDown()
		{
			_container = null;
			_createDummy = null;
		}
		#endregion Test initialization and cleanup

		#region Test for RegisterCreator() method
		[Test]
		public void ThreadSafety()
		{
			IList<Thread> threads = new List<Thread>();
			bool cancelled = false;
			bool failed = false;

			for (int i = 0; i < NumberOfThreads; i++)
			{
				Thread thread = new Thread(new ThreadStart(() => CreateDummies(ref cancelled, ref failed)));
				threads.Add(thread);
				thread.Name = "#" + i;
				thread.Start();
			}

			Thread.Sleep(2000);

			cancelled = true;

			foreach (Thread thread in threads)
			{
				thread.Join();
			}

			Assert.IsFalse(failed);
		}

		private void CreateDummies(ref bool cancelled, ref bool failed)
		{
			try
			{
				while (!cancelled)
				{
					Dummy dummy = _createDummy();
				}
			}
			catch (Exception)
			{
				cancelled = true;
				failed = true;
			}
		}
		#endregion Test for RegisterCreator() method
	}
}
