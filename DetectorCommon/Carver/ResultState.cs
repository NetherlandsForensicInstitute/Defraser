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
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Common.Carver
{
	internal abstract class ResultState<T> : IResultState where T : IResult
	{
		private readonly IState _parentState;
		private readonly IResultBuilder<T> _resultBuilder;
		private readonly IActiveState _activeState;
		private readonly Creator<IResultAttributeBuilder> _createAttributeBuilder;
		private readonly Creator<IAttributeState, IState, IResultAttributeBuilder> _createAttributeState;
		private string _name;

		#region Properties
		public object Name { get { return _name; } set { _name = _resultBuilder.Name = (value is string) ? (string)value : Enum.GetName(value.GetType(), value); } }
		public bool Valid { get; private set; }
		#endregion Properties

		protected ResultState(IState parentState, IResultBuilder<T> resultBuilder, IActiveState activeState,
		                      Creator<IResultAttributeBuilder> createAttributeBuilder, Creator<IAttributeState, IState, IResultAttributeBuilder> createAttributeState)
		{
			_parentState = parentState;
			_resultBuilder = resultBuilder;
			_activeState = activeState;
			_createAttributeBuilder = createAttributeBuilder;
			_createAttributeState = createAttributeState;

			Valid = true;
		}

		public void Parse<TReader>(IAttributeParser<TReader> parser, TReader reader) where TReader : IReader
		{
			IResultAttributeBuilder attributeBuilder = _createAttributeBuilder();
			IAttributeState attributeState = _createAttributeState(this, attributeBuilder);
			IState previousState = _activeState.ChangeState(attributeState);
			try
			{
				parser.Parse(reader, attributeState);
			}
			finally
			{
				_activeState.ChangeState(previousState);
			}

			// TODO: 'Name' must be set on 'attributeBuilder' !!

			if (!attributeState.Valid)
			{
				attributeBuilder.Invalidate();
			}

			_resultBuilder.AddAttribute(attributeBuilder.Build());
		}

		public void Invalidate()
		{
			Valid = false;

			_parentState.Invalidate();
		}

		public void Reset()
		{
			_resultBuilder.Reset();
			_parentState.Recover();

			Valid = true;
		}

		public void Recover()
		{
			_parentState.Recover();
		}

		public void AddAttribute<TName>(TName name, object value)
		{
			_resultBuilder.AddAttribute(new FormattedAttribute<TName, object>(name, value));
		}

		public void AddInvalidAttribute<TName>(TName name, object value)
		{
			Invalidate();

			_resultBuilder.AddAttribute(new FormattedAttribute<TName, object>(name, value) {Valid = false});
		}

		public void AddAttribute<TName>(TName name, object value, IResultFormatter formatter)
		{
			var attribute = new FormattedAttribute<TName, object>(name, value, formatter);

			var validityResultFormatter = formatter as IValidityResultFormatter;
			if ((validityResultFormatter != null) && !validityResultFormatter.IsValid(value))
			{
				attribute.Valid = false;

				Invalidate();
			}

			_resultBuilder.AddAttribute(attribute);
		}
	}
}
