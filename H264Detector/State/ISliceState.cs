/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All Rights reserved.
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

namespace Defraser.Detector.H264.State
{
	internal interface ISliceState
	{
		#region Properties
		IPictureState PictureState { get; }
		uint FirstMacroBlockInSlice { get; }
		/// <summary>
		/// The (collapsed!) slice type of the current slice.
		/// If required to be equal for all slices, it is available as <see cref="IPictureState.PictureSliceType"/>.
		/// </summary>
		SliceType SliceType { get; }
		bool IntraCoded { get; }
		bool FieldPicFlag { get; }
		bool MbAffFrameFlag { get; }
		uint ActiveReferencePictureCount0 { get; }
		uint ActiveReferencePictureCount1 { get; }
		uint CabacInitIdc { get; }
		int SliceQPy { get; }
		uint SliceGroupChangeCycle { get; }
		#endregion Properties

		/// <summary>
		/// Returns the address and availability status of the macroblock to the left
		/// of the current macroblock with <paramref name="currMbAddr"/>.
		/// </summary>
		/// <param name="currMbAddr">the address of the current macroblock</param>
		/// <returns>the address of <em>mbAddrA</em> or <code>uint.MaxValue</code> if not available</returns>
		/// <see>6.4.8 Derivation process for neighbouring macroblock addresses and their availability</see>
		uint GetMbAddrA(uint currMbAddr);

		/// <summary>
		/// Returns the address and availability status of the macroblock above the
		/// current macroblock with <paramref name="currMbAddr"/>.
		/// </summary>
		/// <param name="currMbAddr">the address of the current macroblock</param>
		/// <returns>the address of <em>mbAddrB</em> or <code>uint.MaxValue</code> if not available</returns>
		/// <see>6.4.8 Derivation process for neighbouring macroblock addresses and their availability</see>
		uint GetMbAddrB(uint currMbAddr);
	}
}
