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

using System.Linq;
using Defraser.Interface;

namespace Defraser.GuiCe
{
    public static class IFragmentExtensions
    {
        /// <summary>
        /// Returns a formatted string of the name of a row, e.g. displayed in the FileTree or ProjectKeyframeOverview.
        /// </summary>
        /// <param name="detectable"></param>
        /// <returns></returns>
        public static string GetDescriptiveName(this IFragment detectable)
        {
            string name = detectable.DataFormat.GetDescriptiveName();
            ICodecStream codecStream = detectable as ICodecStream;
            if (codecStream != null && !string.IsNullOrEmpty(codecStream.Name))
            {
                name += string.Format(" ({0})", codecStream.Name);
            }

            if ((detectable.DataFormat == CodecID.Unknown) && (detectable.Detectors != null && detectable.Detectors.Count() > 0) && !(detectable.Detectors.First() is ICodecDetector))
            {
                name = detectable.Detectors.First().Name;
            }

            if (detectable.IsFragmented || detectable.FragmentContainer != null)
            {
                name += string.Format(" (fragment {0})", detectable.FragmentIndex + 1);
            }
            return name;
        }

    }
}
