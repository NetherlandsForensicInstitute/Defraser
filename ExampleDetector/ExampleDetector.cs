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
using Autofac.Builder;
using Defraser.Detector.Common;
using Defraser.Detector.Common.Carver;
using Defraser.Interface;

namespace Defraser.Detector.Example
{
	/// <summary>
	/// ExampleDetector is a template class for detector development for Defraser. The ExampleDetector
	/// searches for an ExampleHeader and when it finds one it will process it.
	/// </summary>
	public sealed class ExampleDetector : ICodecDetector
	{
		private static readonly CodecID[] NoSupportedFormats = new CodecID[0];

		#region Configuration
		internal enum ConfigurationKey
		{
			// Here you define the keys for the configuration items
		}

		static ExampleDetector()
		{
			Configurable = new Configurable<ConfigurationKey>();
			// Initialize configuration items here
		}

		internal static Configurable<ConfigurationKey> Configurable { get; private set; }
		#endregion Configuration

		#region Properties
		public string Name { get { return "Example"; } }
		public string Description { get { return "Example detector to be used as a template for detector development"; } }
		public string OutputFileExtension { get { return ".example"; } }
		public IDictionary<string, string[]> Columns { get { return _detectorColumns.Columns; } }
		public IEnumerable<CodecID> SupportedFormats { get { return NoSupportedFormats; } }
		public Type DetectorType { get { return GetType(); } }
		public ICollection<IConfigurationItem> Configuration { get { return Configurable.Configuration; } }
		public IEnumerable<IReferenceHeader> ReferenceHeaders { get; set; }
		#endregion Properties

		private readonly IDetectorColumns _detectorColumns;

		public ExampleDetector()
		{
			IDetectorColumnsBuilder detectorColumnsBuilder = new DetectorColumnsBuilder();
			_detectorColumns = detectorColumnsBuilder.Build();
		}

		/// <summary>
		/// This method looks for the beginning of an ExampleHeader and if one is found it will parse it
		/// by calling Parser.Parse().
		/// </summary>
		public IDataBlock DetectData(IDataReader dataReader, IDataBlockBuilder dataBlockBuilder, IScanContext context)
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule(new DetectorCommonModule());
			builder.RegisterModule(new ExampleDetectorModule());

			// parameters
			builder.Register(this).As<IDetector>();
			builder.Register(new ByteStreamDataReader(dataReader)).As<ByteStreamDataReader>().As<IDataReader>().ExternallyOwned();
			builder.Register(dataBlockBuilder).As<IDataBlockBuilder>();
			builder.Register(context).As<IScanContext>();

			using (var container = builder.Build())
			{
				return container.Resolve<ICarverState>().Carve(dataReader.Length);
			}
		}

		public bool IsKeyFrame(IResultNode resultNode)
		{
			return false;
		}

		public IDataPacket GetVideoHeaders(IResultNode headerPacket)
		{
			return null;
		}

		public IDataPacket GetVideoData(IResultNode resultNode)
		{
			return null;
		}

		public IDataPacket FindReferenceHeader(IDataReader dataReader, ICodecParametersSpec context)
		{
			return null;
		}
	}
}
