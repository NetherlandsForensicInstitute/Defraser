/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights Reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Institute nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE INSTITUTE AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE INSTITUTE OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using Defraser.Detector.Common;

namespace Defraser.Detector.Avi
{
	public enum FrameFieldType
	{
		Short,
		Int
	}

	internal class Frame<TEnum> : CompositeAttribute<TEnum, string, AviParser>
	{
		private readonly FrameFieldType _frameFieldType;

		public enum LAttribute
		{
			LeftEdge,
			TopEdge,
			RightEdge,
			BottomEdge
		}

		public Frame(TEnum attributeName, FrameFieldType frameFieldType)
			: base(attributeName, string.Empty, "{0}")
		{
			_frameFieldType = frameFieldType;
		}

		public override bool Parse(AviParser parser)
		{
			if (_frameFieldType == FrameFieldType.Short)
			{
				short leftEdge = parser.GetShort(LAttribute.LeftEdge);
				short topEdge = parser.GetShort(LAttribute.TopEdge);
				short rightEdge = parser.GetShort(LAttribute.RightEdge);
				short bottomEdge = parser.GetShort(LAttribute.BottomEdge);
				TypedValue = string.Format("({0}, {1}, {2}, {3})", leftEdge, topEdge, rightEdge, bottomEdge);
			}
			else
			{
				int leftEdge = parser.GetInt(LAttribute.LeftEdge);
				int topEdge = parser.GetInt(LAttribute.TopEdge);
				int rightEdge = parser.GetInt(LAttribute.RightEdge);
				int bottomEdge = parser.GetInt(LAttribute.BottomEdge);
				TypedValue = string.Format("({0}, {1}, {2}, {3})", leftEdge, topEdge, rightEdge, bottomEdge);
			}
			return Valid;
		}
	}
}
