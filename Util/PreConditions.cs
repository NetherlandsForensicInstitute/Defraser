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
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Defraser.Util
{
	/// <summary>
	/// The <see cref="PreConditions"/> provides several methods for checking
	/// preconditions for method arguments, property setters, indexers and
	/// object state invariants.
	/// </summary>
	/// <remarks>
	/// Replace by the <i>Code Contracts</i> framework when it becomes available
	/// with the release of C# 4.0.
	/// <see cref="http://research.microsoft.com/en-us/projects/contracts/"/>
	/// </remarks>
	public static class PreConditions
	{
		#region Inner classes
		public sealed class PreCondition<T> where T : class
		{
			private readonly T _operand;

			#region Properties
			public T And { get { return _operand; } }
			#endregion Properties

			internal PreCondition(T operand)
			{
				_operand = operand;
			}
		}

		public sealed class ArgumentPreConditionBuilder
		{
			#region Inner classes
			public abstract class Operand
			{
				#region Properties
				protected string Name { get; private set; }
				#endregion Properties
	
				internal Operand(string name)
				{
					Name = name;
				}
			}

			public sealed class IntOperand : Operand
			{
				private readonly int _value;
	
				internal IntOperand(string name, int value)
					: base(name)
				{
					_value = value;
				}
	
				public PreCondition<IntOperand> IsNotNegative()
				{
					if (_value < 0)
					{
						throw new ArgumentOutOfRangeException(Name, _value, string.Format("{0} can not be negative", Name));
					}
					return CreatePreCondition(this);
				}
	
				public PreCondition<IntOperand> InRange(int min, int max)
				{
					if ((_value < min) || (_value > max))
					{
						throw new ArgumentOutOfRangeException(Name, _value, string.Format("{0} should be in range [{1},{2}]", Name, min, max));
					}
					return CreatePreCondition(this);
				}
			}
	
			public sealed class LongOperand : Operand
			{
				private readonly long _value;
	
				internal LongOperand(string name, long value)
					: base(name)
				{
					_value = value;
				}
	
				public PreCondition<LongOperand> IsNotNegative()
				{
					if (_value < 0L)
					{
						throw new ArgumentOutOfRangeException(Name, _value, string.Format("{0} can not be negative", Name));
					}
					return CreatePreCondition(this);
				}
	
				public PreCondition<LongOperand> InRange(long min, long max)
				{
					if ((_value < min) || (_value > max))
					{
						throw new ArgumentOutOfRangeException(Name, _value, string.Format("{0} should be in range [{1},{2}]", Name, min, max));
					}
					return CreatePreCondition(this);
				}
			}
	
			public sealed class ObjectOperand : Operand
			{
				private readonly object _value;
	
				internal ObjectOperand(string name, object value)
					: base(name)
				{
					_value = value;
				}
	
				public PreCondition<ObjectOperand> IsNotNull()
				{
					if (_value == null)
					{
						throw new ArgumentNullException(Name);
					}
					return CreatePreCondition(this);
				}

				public PreCondition<ObjectOperand> IsDefinedOnEnum(Type type)
				{
					if (!Enum.IsDefined(type, _value))
					{
						throw new InvalidEnumArgumentException(Name, (int)_value, type);
					}
					return CreatePreCondition(this);
				}
			}
	
			public sealed class StringOperand : Operand
			{
				private readonly string _value;
	
				internal StringOperand(string name, string value)
					: base(name)
				{
					_value = value;
				}
	
				public PreCondition<StringOperand> IsNotNull()
				{
					if (_value == null)
					{
						throw new ArgumentNullException(Name);
					}
					return CreatePreCondition(this);
				}
	
				public PreCondition<StringOperand> IsNotEmpty()
				{
					if (_value.Length == 0)
					{
						throw new ArgumentException(string.Format("{0} can not be empty", Name), Name);
					}
					return CreatePreCondition(this);
				}
	
				public PreCondition<StringOperand> IsExistingDirectory()
				{
					if (!Directory.Exists(_value))
					{
						throw new DirectoryNotFoundException(string.Format("{0} \"{1}\" does not exist", Name, _value));
					}
					return CreatePreCondition(this);
				}

				public PreCondition<StringOperand> IsExistingFile()
				{
					if (!File.Exists(_value))
					{
						throw new FileNotFoundException(string.Format("{0} \"{1}\" does not exist", Name, _value));
					}
					return CreatePreCondition(this);
				}
			}
	
			public sealed class EnumerableOperand<T> : Operand
			{
				private readonly IEnumerable<T> _value;
	
				internal EnumerableOperand(string name, IEnumerable<T> value)
					: base(name)
				{
					_value = value;
				}
	
				public PreCondition<EnumerableOperand<T>> IsNotNull()
				{
					if (_value == null)
					{
						throw new ArgumentNullException(Name);
					}
					return CreatePreCondition(this);
				}
	
				public PreCondition<EnumerableOperand<T>> IsNotEmpty()
				{
					if (_value.Count() == 0)
					{
						throw new ArgumentException(string.Format("Collection {0} can not be empty", Name), Name);
					}
					return CreatePreCondition(this);
				}

				// TODO: only for non-value types!!
				public PreCondition<EnumerableOperand<T>> DoesNotContainNull()
				{
					if (_value.Contains(default(T)))
					{
						throw new ArgumentException(string.Format("Collection {0} contains null", Name), Name);
					}
					return CreatePreCondition(this);
				}
			}
			#endregion Inner classes

			#region Properties
			public string Name { get; private set; }
			#endregion Properties

			internal ArgumentPreConditionBuilder(string name)
			{
				Name = name;
			}

			public IntOperand Value(int value)
			{
				return new IntOperand(Name, value);
			}

			public LongOperand Value(long value)
			{
				return new LongOperand(Name, value);
			}

			public ObjectOperand Value(object value)
			{
				return new ObjectOperand(Name, value);
			}

			public StringOperand Value(string value)
			{
				return new StringOperand(Name, value);
			}

			public EnumerableOperand<T> Value<T>(IEnumerable<T> value)
			{
				return new EnumerableOperand<T>(Name, value);
			}

			public PreCondition<ArgumentPreConditionBuilder> IsInvalidIf(bool condition, string message)
			{
				if (condition)
				{
					throw new ArgumentException(message, Name);
				}
				return CreatePreCondition(this);
			}

			// TODO: enum
		}

		public sealed class ObjectPreConditionBuilder
		{
			private readonly object _obj;

			internal ObjectPreConditionBuilder(object obj)
			{
				_obj = obj;
			}

			public PreCondition<ObjectPreConditionBuilder> IsDisposedIf(bool condition)
			{
				if (condition)
				{
					throw new ObjectDisposedException(string.Format("{0} has been disposed", _obj.GetType().Name));
				}
				return CreatePreCondition(this);
			}
		}

		public sealed class OperationPreConditionBuilder
		{
			internal OperationPreConditionBuilder()
			{
			}

			public PreCondition<OperationPreConditionBuilder> IsInvalidIf(bool condition, string message)
			{
				if (condition)
				{
					throw new InvalidOperationException(message);
				}
				return CreatePreCondition(this);
			}
		}
		#endregion Inner classes

		/// <summary>
		/// Creates a precondition for the argument with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the argument to check</param>
		/// <returns>The <see cref="ArgumentPreConditionBuilder"/></returns>
		public static ArgumentPreConditionBuilder Argument(string name)
		{
			return new ArgumentPreConditionBuilder(name);
		}

		/// <summary>
		/// Creates a precondition for (this) object <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>The <see cref="ObjectPreConditionBuilder"/></returns>
		public static ObjectPreConditionBuilder Object(object obj)
		{
			return new ObjectPreConditionBuilder(obj);
		}

		/// <summary>
		/// Creates a precondition for the current operation (method or property).
		/// </summary>
		/// <returns>The <see cref="OperationPreConditionBuilder"/></returns>
		public static OperationPreConditionBuilder Operation()
		{
			return new OperationPreConditionBuilder();
		}

		private static PreCondition<T> CreatePreCondition<T>(T operand) where T : class
		{
			return new PreCondition<T>(operand);
		}
	}
}
