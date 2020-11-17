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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Defraser.DataStructures;
using Defraser.Detector.Common;
using Defraser.Interface;

namespace Defraser.Detector.Mpeg4
{
	public class Mpeg4Header : Header<Mpeg4Header, Mpeg4HeaderName, Mpeg4Parser>
	{
		/// <summary>The possible Attributes of a detected header.</summary>
		public enum Attribute
		{
			StartCode,
			//ByteEntropy,
			ZeroByteStuffing
		}

		#region Properties
		public override CodecID DataFormat
		{
			get
			{
				Mpeg4Header header = Root.FindChildren(new[] { Mpeg4HeaderName.Vop, Mpeg4HeaderName.VopWithShortHeader, Mpeg4HeaderName.VideoObjectLayer }, true);
				bool shortVideo = (header != null) && (header.HeaderName == Mpeg4HeaderName.VopWithShortHeader);
				return shortVideo ? CodecID.H263 : CodecID.Mpeg4Video;
			}
		}

		/// <summary> The actual number of zero stuffing bytes.</summary>
		internal byte ZeroByteStuffing { get; private set; }
		internal uint StartCode { get; private set; }
		internal VideoObjectLayer Vol
		{
			get
			{
				// In MPEG-4 every VOP is a child of a VOL header (or group-of-VOP)
				var nonGopParent = (Parent is GroupOfVop) ? Parent.Parent : Parent;
				var vol = nonGopParent as VideoObjectLayer;
				if (vol != null)
				{
					return vol;
				}

				// Before parsing is complete, the DataBlock contains the previous header.
				// The previous header can be a VOL and more likly a VOP.
				// When it is a VOP, the parent of that VOP would be the VOL.
				var grandparent = nonGopParent.Parent;
				var nonGopGrandparent = (grandparent is GroupOfVop) ? grandparent.Parent : grandparent;
				return nonGopGrandparent as VideoObjectLayer;
			}
		}
		#endregion Properties

		public Mpeg4Header(IEnumerable<IDetector> detectors)
			: base(detectors, Mpeg4HeaderName.Root)
		{
		}

		public Mpeg4Header(Mpeg4Header previousHeader, Mpeg4HeaderName mpeg4HeaderName)
			: base(previousHeader, mpeg4HeaderName)
		{
		}

		public override bool Parse(Mpeg4Parser parser)
		{
			if (!base.Parse(parser)) return false;

			if (HeaderName.IsFlagSet(HeaderFlags.StartCode))
			{
				StartCode = parser.GetStartCode(Attribute.StartCode, this is VopWithShortHeader);
			}
			return this.Valid;
		}

		public override bool ParseEnd(Mpeg4Parser parser)
		{
			this.ZeroByteStuffing = (byte)parser.GetZeroByteStuffing(Attribute.ZeroByteStuffing);

			if (parser.ReadOverflow)
			{
				this.Valid = false;
				return false;
			}

			if (base.ParseEnd(parser) == false) return false;

			//Attributes.Add(new FormattedAttribute<Attribute, float>(Attribute.ByteEntropy, (float)ByteEntropy(parser), "{0:0.00}"));

			return true;
		}

		public override bool IsBackToBack(Mpeg4Header previousHeader)
		{
			bool previousHeaderIsVop = previousHeader is Vop;
			bool currentHeaderIsVop = this is Vop;
			if (currentHeaderIsVop || previousHeaderIsVop) return true;

			if (this is VideoObjectLayer || previousHeaderIsVop) return true;

			if (currentHeaderIsVop && previousHeader is VisualObjectSequenceStart) return true;

			// Put Picture headers in the same result when their temporal
			// reference values are in a close range.
			VopWithShortHeader currentVopWithShortHeader = this as VopWithShortHeader;
			VopWithShortHeader previousVopWithShortHeader = previousHeader as VopWithShortHeader;
			if (currentVopWithShortHeader != null && previousVopWithShortHeader != null)
			{
				byte currentTemporalReference = currentVopWithShortHeader.TemporalReference;
				byte previousTemporalReference = previousVopWithShortHeader.TemporalReference;

				byte temporalReferenceValueDeltaMax = (byte)Mpeg4Detector.Configurable[Mpeg4Detector.ConfigurationKey.TemporalReferenceValueDeltaMax];

				if (((previousTemporalReference - currentTemporalReference <= temporalReferenceValueDeltaMax) && (previousTemporalReference > currentTemporalReference)) ||
					((currentTemporalReference > 255 - temporalReferenceValueDeltaMax) && (previousTemporalReference < temporalReferenceValueDeltaMax) && (previousTemporalReference + 255 - currentTemporalReference < temporalReferenceValueDeltaMax)))
				{
					return true;
				}
			}

			return base.IsBackToBack(previousHeader);
		}

		/// <summary>
		/// Trims the zero byte stuffing from the end of the header.
		/// This removes the zero byte stuffing from the data packet
		/// and removes the corresponding attribute.
		/// </summary>
		/// <param name="parser">the parser</param>
		public void TrimZeroByteStuffing(Mpeg4Parser parser)
		{
			if (ZeroByteStuffing > 0)
			{
				IResultAttribute zeroByteStuffingAttribute = FindAttributeByName(Enum.GetName(typeof(Attribute), Attribute.ZeroByteStuffing));

				Debug.Assert(Attributes.Count > 0 && zeroByteStuffingAttribute != null);

				// Trim the zero byte stuffing
				DataPacket = parser.GetDataPacket(Offset, Length - ZeroByteStuffing);
				this.ZeroByteStuffing = 0;

				// Removes the corresponding attribute
				Attributes.Remove(zeroByteStuffingAttribute);
			}
		}

		public override bool IsSuitableParent(Mpeg4Header parent)
		{
			// Header is allowed in its suitable parent (correct files)
			if (HeaderName.GetSuitableParents().Contains(parent.HeaderName))
			{
				return true;
			}

			// Header ends up in the root if no suitable parent was found (partial files)
			if (!parent.IsRoot)
			{
				return false;
			}

			// Atoms of partial files are allowed in the root if empty
			if (!parent.HasChildren())
			{
				return true;
			}

			// Root should not already contain top-level atoms (FileType, MediaData or Movie)
			// Otherwise, the top-level parent of this header should have been in the root as well
			foreach (Mpeg4Header header in parent.Children)
			{
				if (header.HeaderName.IsTopLevel())
				{
					return false;
				}
			}

			return true;
		}
	}
}
