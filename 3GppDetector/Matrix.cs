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

namespace Defraser.Detector.QT
{
	public class Matrix
	{
		public enum LAttribute
		{
			A, B, U,
			C, D, V,
			X, Y, W,
		}

		public double A { get; private set; }
		public double B { get; private set; }
		public double U { get; private set; }
		public double C { get; private set; }
		public double D { get; private set; }
		public double V { get; private set; }
		public double X { get; private set; }
		public double Y { get; private set; }
		public double W { get; private set; }

		internal void Parse(QtParser parser)
		{
			A = parser.GetFixed16_16(LAttribute.A);
			B = parser.GetFixed16_16(LAttribute.B);
			U = parser.GetFixed2_30(LAttribute.U);
			C = parser.GetFixed16_16(LAttribute.C);
			D = parser.GetFixed16_16(LAttribute.D);
			V = parser.GetFixed2_30(LAttribute.V);
			X = parser.GetFixed16_16(LAttribute.X);
			Y = parser.GetFixed16_16(LAttribute.Y);
			W = parser.GetFixed2_30(LAttribute.W);
		}
	}
}
