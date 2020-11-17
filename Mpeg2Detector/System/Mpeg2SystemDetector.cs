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
using System.Runtime.Serialization;
using Autofac;
using Autofac.Builder;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;
using Defraser.Util;

namespace Defraser.Detector.Mpeg2.System
{
	[DataContract]
	public sealed class Mpeg2SystemDetector : IDetector
	{
		#region Configuration
		internal enum ConfigurationKey
		{
			MaxSystemHeaderCount,
			MinSystemHeaderCount,
			PictureHeaderMaxLengthOfExtraInformation,
			PesPacketMaxUnknownByteCount,
			ExtraByteCountAllowedBetweenHeaders,
			UseSystemClockReferenceValidation,
			MaxTimeGapSeconds,
			MaxZeroByteStuffingLength
		}

		static Mpeg2SystemDetector()
		{
			Configurable = new Configurable<ConfigurationKey>();
			Configurable.Add(ConfigurationKey.MaxSystemHeaderCount, (uint)150000);
			Configurable.Add(ConfigurationKey.MinSystemHeaderCount, (uint)2);
			Configurable.Add(ConfigurationKey.PesPacketMaxUnknownByteCount, (uint)1000);
			Configurable.Add(ConfigurationKey.PictureHeaderMaxLengthOfExtraInformation, (uint)1000);
			Configurable.Add(ConfigurationKey.ExtraByteCountAllowedBetweenHeaders, (uint)0);
			Configurable.Add(ConfigurationKey.UseSystemClockReferenceValidation, true);
			Configurable.Add(ConfigurationKey.MaxTimeGapSeconds, 1.0);
			Configurable.Add(ConfigurationKey.MaxZeroByteStuffingLength, (uint)32);
		}

		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Configuration

		#region Properties
		public string Name { get { return "MPEG-1/2 Systems"; } }
		public string Description { get { return "MPEG-1 and 2 Program Stream detector"; } }
		public string OutputFileExtension { get { return ".mpg"; } }
		public IDictionary<string, string[]> Columns { get { return _detectorColumns.Columns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return new[] { CodecID.Mpeg1System, CodecID.Mpeg2System }; } }
		public Type DetectorType { get { return GetType(); } }
		public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		#endregion Properties

		private readonly IDetectorColumns _detectorColumns;
		private readonly Creator<ICarverState, IDataReader, IDataBlockBuilder, IScanContext> _createCarver;

		public Mpeg2SystemDetector()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new DetectorCommonModule());
			builder.RegisterModule(new Mpeg2SystemModule());
			IContainer container = builder.Build();

			_detectorColumns = container.Resolve<IDetectorColumns>();
			_createCarver = container.Resolve<Creator<ICarverState, IDataReader, IDataBlockBuilder, IScanContext>>();
		}

		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			var bitStreamDataReader = new BitStreamDataReader(dataReader);
			return _createCarver(bitStreamDataReader, dataBlockBuilder, context).Carve(dataReader.Length);
		}
	}
}
