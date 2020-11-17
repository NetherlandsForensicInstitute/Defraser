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
using Defraser.Util;

namespace Defraser.Detector.Common.Carver
{
	internal sealed class ReaderState : IReaderState
	{
		private readonly IActiveState _activeState;
		private readonly IResultNodeCallback _resultNodeCallback;
		private readonly IResultNode _rootResultNode;
		private readonly IResultMetadata _resultMetadata;
		private readonly IScanContext _scanContext;
		private readonly Creator<IResultNodeBuilder> _createResultNodeBuilder;
		private readonly Creator<IResultNodeState, IState, IResultNodeBuilder> _createResultNodeReaderState;

		#region Properties
		public bool Valid { get; private set; }
		public IState ActiveState { get { return _activeState.State; } }
		#endregion Properties

		public ReaderState(IActiveState activeState, IResultNodeCallback resultNodeCallback, IResultNode rootResultNode, IResultMetadata resultMetadata, IScanContext scanContext,
		                   Creator<IResultNodeBuilder> createResultNodeBuilder, Creator<IResultNodeState, IState, IResultNodeBuilder> createResultNodeReaderState)
		{
			_activeState = activeState;
			_resultNodeCallback = resultNodeCallback;
			_rootResultNode = rootResultNode;
			_resultMetadata = resultMetadata;
			_scanContext = scanContext;
			_createResultNodeBuilder = createResultNodeBuilder;
			_createResultNodeReaderState = createResultNodeReaderState;

			_scanContext.Results = rootResultNode;

			Valid = true;

			_activeState.ChangeState(this);
		}

		public void Parse<T>(IResultParser<T> parser, T reader) where T : IReader
		{
			long offset = reader.Position;

			IResultNodeBuilder resultNodeBuilder = _createResultNodeBuilder();
			IResultNodeState resultState = _createResultNodeReaderState(this, resultNodeBuilder);
			IState previousState = _activeState.ChangeState(resultState);
			try
			{
				parser.Parse(reader, resultState);
			}
			finally
			{
				_activeState.ChangeState(previousState);
			}

			if (Valid && (reader.Position > offset))
			{
				resultNodeBuilder.DataPacket = reader.GetDataPacket(offset, (reader.Position - offset));
				resultNodeBuilder.Metadata = _resultMetadata;

				if (!resultState.Valid)
				{
					resultNodeBuilder.Invalidate();
				}

				_resultNodeCallback.AddNode(_rootResultNode, resultNodeBuilder.Build(), resultState);
			}
		}

		public void Invalidate()
		{
			Valid = false;
		}

		public void Reset()
		{
			_rootResultNode.Children.Clear();
			_resultNodeCallback.Reset();

			Valid = true;
		}

		public void Recover()
		{
			Valid = true;
		}
	}
}
