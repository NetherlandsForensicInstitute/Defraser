/*
 * Copyright (c) 2008-2020, Netherlands Forensic Institute
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
using System.Diagnostics;
using System.Reflection;
using Defraser.Detector.Common;

namespace Defraser.Detector.QT
{
	/// <summary>
	/// MPEG-4/3GPP/QuickTime header types.
	/// </summary>
	public enum AtomName
	{
		Root,
		[Atom(null, new AtomName[0]/* Allowed in all containers */, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		Unknown,
		[Atom(null, new AtomName[0]/* Allowed in all containers */, 0)]	// TODO: only in lists
		TerminatingZero,
		// Top-level atoms
		[Atom("ftyp", new [] { Root }, AtomFlags.SizeAndType)]
		FileType,
		[Atom("pdin", new [] { Root }, AtomFlags.FullAtom)] // TODO implement
		ProgressiveDownloadInformation,
		[Atom("moov", new [] { Root }, AtomFlags.ContainerAtom)]
		Movie,
		// Remark: the MovieHeader is a FullAtom. It is not flagged that way because of an
		// memory optimization: to parse the atom, the value of the version is needed.
		// Instead of putting this field in QtAtom, the FullAtom flag is removed and
		// the version attriute is parsed in the MovieHeader atom itself.
		[Atom("mvhd", new [] { Movie }, AtomFlags.SizeAndType)]
		MovieHeader,
		[Atom("trak", new [] { Movie }, AtomFlags.ContainerAtom | AtomFlags.DuplicatesAllowed)]
		Track,
		// Track atoms
		[Atom("tkhd", new [] { Track }, AtomFlags.FullAtom)]
		TrackHeader,
		[Atom("tref", new [] { Track }, AtomFlags.ContainerAtom)]
		TrackReference,
		[Atom("edts", new [] { Track }, AtomFlags.ContainerAtom)]
		Edit,
		// Edit atoms
		[Atom("elst", new [] { Edit }, AtomFlags.FullAtom)]
		EditList,
		[Atom("load", new [] { Track }, AtomFlags.SizeAndType)]
		TrackLoadSettings,
		[Atom("matt", new [] { Track }, AtomFlags.ContainerAtom)]
		TrackMatte,
		// Track matte atoms
		[Atom("kmat", new [] { TrackMatte }, AtomFlags.FullAtom)]
		CompressedMatte,
		[Atom("imap", new [] { Track }, AtomFlags.ContainerAtom)]
		TrackInputMap,
		[Atom("mdia", new [] { Track }, AtomFlags.ContainerAtom)]
		Media,
		// Track input map atoms
		[Atom("  in", new [] { TrackInputMap }, AtomFlags.ContainerAtom)]
		TrackInput,
		[Atom("  ty", new [] { TrackInput }, AtomFlags.SizeAndType)]	// TODO: implement atom
		InputType,
		[Atom("obid", new [] { TrackInput }, AtomFlags.SizeAndType)]	// TODO: implement atom
		ObjectID,
		// Movie atoms
		[Atom("prfl", new [] { Track, Movie, Root }, AtomFlags.FullAtom)]
		Profile,
		[Atom("clip", new [] { Track, Movie }, AtomFlags.ContainerAtom)]
		Clipping,
		[Atom("ctab", new [] { Movie }, AtomFlags.SizeAndType)]
		ColorTable,
		[Atom("cmov", new [] { Movie }, AtomFlags.ContainerAtom)]
		CompressedMovie,
		[Atom("rmra", new [] { Movie }, AtomFlags.ContainerAtom)]
		ReferenceMovie,
		// Clipping atoms
		[Atom("crgn", new [] { Clipping }, AtomFlags.SizeAndType)]
		ClippingRegion,
		// Media atoms
		[Atom("mdhd", new [] { Media }, AtomFlags.FullAtom)]
		MediaHeader,
		// TODO is the hdlr truly a container atom?
		[Atom("hdlr", new [] { Media, MediaInformation, MetaData }, AtomFlags.FullAtom)]	// TODO: occurs in other atoms?
		HandlerReference,
		[Atom("minf", new [] { Media }, AtomFlags.ContainerAtom)]
		MediaInformation,
		// Media information atoms
		[Atom("vmhd", new [] { MediaInformation }, AtomFlags.FullAtom)]
		VideoMediaInformationHeader,
		[Atom("smhd", new [] { MediaInformation }, AtomFlags.FullAtom)]
		SoundMediaInformationHeader,
		[Atom("gmhd", new [] { MediaInformation }, AtomFlags.ContainerAtom)]
		BaseMediaInformationHeader,
		[Atom("hmhd", new [] { MediaInformation }, AtomFlags.FullAtom)]
		HintMediaInformationHeader,
		[Atom("nmhd", new [] { MediaInformation }, AtomFlags.FullAtom)]
		NullMediaInformationHeader,
		[Atom("gmin", new [] { MediaInformation, BaseMediaInformationHeader }, AtomFlags.FullAtom)]
		BaseMediaInfo,
		[Atom("dinf", new [] { MediaInformation, MetaData }, AtomFlags.ContainerAtom)]
		DataInformation,
		[Atom("dref", new [] { DataInformation }, AtomFlags.FullAtom | AtomFlags.Container)]
		DataReference,
		[Atom("stbl", new [] { MediaInformation }, AtomFlags.ContainerAtom)]
		SampleTable,
		// Sample table atoms
		[Atom("stsd", new [] { SampleTable }, AtomFlags.FullAtom | AtomFlags.Container)]
		SampleDescription,
		[Atom("stts", new [] { SampleTable }, AtomFlags.FullAtom)]
		TimeToSample,
		[Atom("ctts", new [] { SampleTable }, AtomFlags.FullAtom)]
		CompositionTimeToSample,
		[Atom("stsc", new [] { SampleTable }, AtomFlags.FullAtom)]
		SampleToChunk,
		[Atom("stsz", new [] { SampleTable }, AtomFlags.FullAtom)]
		SampleSize,
		[Atom("stz2", new [] { SampleTable }, AtomFlags.FullAtom)]
		CompactSampleSize,
		[Atom("stco|co64", new [] { SampleTable }, AtomFlags.FullAtom)]
		ChunkOffset,
		[Atom("stss", new [] { SampleTable }, AtomFlags.FullAtom)]
		SyncSample,
		[Atom("stsh", new [] { SampleTable }, AtomFlags.FullAtom)]
		ShadowSync,
		[Atom("padb", new [] { SampleTable }, AtomFlags.FullAtom)]
		PaddingBits,
		[Atom("stdp", new [] { SampleTable }, AtomFlags.FullAtom)]
		DegradationPriority,
		[Atom("sdtp", new [] { SampleTable, TrackFragment }, AtomFlags.FullAtom)]	// TODO implement atom
		SampleDependencyType,
		[Atom("subs", new [] { SampleTable, TrackFragment }, AtomFlags.FullAtom)]
		SubSampleInformation,
		[Atom("cslg", new [] { SampleTable }, AtomFlags.SizeAndType)]	// Probabely AtomFlags.FullAtom; no specification available
		CompositionShiftLeastGreatest,
		[Atom("stps", new [] { SampleTable }, AtomFlags.SizeAndType)]	// Probabely AtomFlags.FullAtom; no specification available
		PartialSyncSample,

		// - - - atoms for H.264 support - - -
		[Atom("avcC", new [] { VideoSampleDescription }, AtomFlags.Box)]
		AvcConfigurationBox,
		[Atom("m4ds", new [] { VideoSampleDescription }, AtomFlags.Box)]
		Mpeg4ExtensionDescriptorsBox,
		[Atom("btrt", new [] { VideoSampleDescription }, AtomFlags.Box)]
		Mpeg4BitRateBox,
		[Atom("avcp", new [] { SampleTable }, AtomFlags.SizeAndType)]
		AvcParameterSampleEntry,
		[Atom("avss", new [] { SampleGroupDescriptionBox }, AtomFlags.SizeAndType)]
		AvcSubSequenceEntry,
		[Atom("avll", new [] { SampleGroupDescriptionBox }, AtomFlags.SizeAndType)]
		AvcLayerEntry,
		[Atom("sdep", new [] { SampleTable }, AtomFlags.FullBox)]
		SampleDependencyBox,
		[Atom("roll", new [] { SampleTable }, AtomFlags.SizeAndType)]
		VisualRollRecoveryEntry,
		[Atom("sbgp", new [] { SampleTable, TrackFragment }, AtomFlags.FullAtom)]	// TODO implement atom
		SampleToGroupBox,
		[Atom("sgpd", new [] { SampleTable }, AtomFlags.FullAtom | AtomFlags.Container)]
		SampleGroupDescriptionBox,
		// - - - atoms for H.264 support - - -

		// MPEG-4 Media Header Boxes
		/*
		[Atom("odhd", new [] { HandlerReference }, AtomFlags.FullBox)]
		ObjectDescriptorStream,
		[Atom("crhd", new [] { HandlerReference }, AtomFlags.FullBox)]
		ClockReferenceStream,
		[Atom("sdhd", new [] { HandlerReference }, AtomFlags.FullBox)]
		SceneDescriptionStream,
		[Atom("m7hd", new [] { HandlerReference }, AtomFlags.FullBox)]
		Mpeg7Stream,
		[Atom("ochd", new [] { HandlerReference }, AtomFlags.FullBox)]
		ObjectContentInfoStream,
		[Atom("iphd", new [] { HandlerReference }, AtomFlags.FullBox)]
		IpmpStream,
		[Atom("mjhd", new [] { HandlerReference }, AtomFlags.FullBox)]
		MpegJStream,
		*/
		// MPEG-4 Media Header Boxes

		[Atom("mvex", new [] { Movie }, AtomFlags.ContainerAtom)]	// TODO implement atom
		MovieExtends,
		[Atom("mehd", new [] { MovieExtends }, AtomFlags.FullAtom)]	// TODO implement atom
		MvieExtendsHeader,
		[Atom("trex", new [] { MovieExtends }, AtomFlags.ContainerAtom)]	// TODO implement atom
		TrackExtendsDefaults,
		[Atom("ipmc", new [] { Movie, MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		IpmpControl,
		[Atom("moof", new [] { Root }, AtomFlags.ContainerAtom)]
		MovieFragment,
		[Atom("mfhd", new [] { MovieFragment }, AtomFlags.FullAtom)]	// TODO implement atom
		MovieFragmentHeader,
		[Atom("traf", new [] { MovieFragment }, AtomFlags.ContainerAtom)]
		TrackFragment,
		[Atom("tfhd", new [] { TrackFragment }, AtomFlags.FullAtom)]	// TODO implement atom
		TrackFragmentHeader,
		[Atom("trun", new [] { TrackFragment }, AtomFlags.FullAtom)]	// TODO implement atom
		TrackFragmentRun,
		[Atom("mfra", new [] { Root }, AtomFlags.ContainerAtom)]
		MovieFragmentRandomAccess,
		[Atom("tfra", new [] { MovieFragmentRandomAccess }, AtomFlags.FullAtom)]	// TODO implement atom
		TrackFragmentRandom,
		[Atom("mfro", new [] { MovieFragmentRandomAccess }, AtomFlags.FullAtom)]	// TODO implement atom
		MovieFragmentRandomAccessOffset,
		[Atom("mdat", new [] { Root }, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		MediaData,
		// TODO: merge free, junk and skip atoms
		[Atom("free", new AtomName[0]/* Allowed in all containers */, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		Free,
		[Atom("junk", new AtomName[0]/* Allowed in all containers */, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		Junk,
		[Atom("wide", new AtomName[0]/* Allowed in all containers */, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		Wide,
		[Atom("skip", new AtomName[0]/* Allowed in all containers */, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		Skip,
		[Atom("meta", new [] { Root, Movie, Track, UserData }, AtomFlags.FullAtom)]
		MetaData,
		[Atom("iloc", new [] { MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		ItemLocation,
		[Atom("ipro", new [] { MetaData }, AtomFlags.ContainerAtom | AtomFlags.FullAtom)]	// TODO implement atom
		ItemProtection,
		[Atom("sinf", new [] { ItemProtection }, AtomFlags.ContainerAtom)]	// TODO implement atom
		ProtectionSchemeInformation,
		[Atom("frma", new [] { ProtectionSchemeInformation }, AtomFlags.SizeAndType)]	// TODO implement atom
		OriginalFormat,
		[Atom("imif", new [] { ProtectionSchemeInformation }, AtomFlags.FullAtom)]	// TODO implement atom
		IpmpInformation,
		[Atom("schm", new [] { ProtectionSchemeInformation/* todo,  srpp*/ }, AtomFlags.FullAtom)]	// TODO implement atom
		SchemeType,
		[Atom("schi", new [] { ProtectionSchemeInformation/* todo,  srpp*/ }, AtomFlags.SizeAndType)]	// TODO implement atom
		SchemeInformation,
		[Atom("iinf", new [] { MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		ItemInfo,
		[Atom("xml ", new [] { MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		Xml,
		[Atom("bxml", new [] { MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		BinarayXml,
		[Atom("pitm", new [] { MetaData }, AtomFlags.FullAtom)]	// TODO implement atom
		PrimaryItem,
		// Data information atoms
		[Atom("alis|rsrc|url ", new [] { DataReference }, AtomFlags.FullAtom)]	// Can be the ComponentSubType/HandlerType of a HandlerReference atom
		DataReferenceEntry,
		// Compressed movie atoms
		[Atom("dcom", new [] { CompressedMovie }, AtomFlags.SizeAndType)]
		DataCompression,
		[Atom("cmvd", new [] { CompressedMovie }, AtomFlags.SizeAndType)]
		CompressedMovieData,
		// Reference movie atoms
		[Atom("rmda", new [] { ReferenceMovie }, AtomFlags.ContainerAtom | AtomFlags.DuplicatesAllowed)]
		ReferenceMovieDescriptor,
		// Reference movie descriptor atoms
		[Atom("rdrf", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType)]
		ReferenceMovieDataReference,
		[Atom("rmdr", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType)]
		DataRate,
		[Atom("rmcs", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType)]
		CpuSpeed,
		[Atom("rmvc", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType)]
		VersionCheck,
		[Atom("rmcd", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType | AtomFlags.DuplicatesAllowed)]
		ComponentDetect,
		[Atom("rmqu", new [] { ReferenceMovieDescriptor }, AtomFlags.SizeAndType)]
		Quality,
		// Other atoms
		[Atom("udta", new AtomName[0]/* Allowed in all containers */, AtomFlags.ContainerAtom)]
		UserData,
		// User Data atoms
		[Atom("©arg", new [] { UserData }, AtomFlags.UserDataAtom)]	// TODO not all user data atom contain a Pascal String
		NameOfArranger,
		[Atom("©ark", new [] { UserData }, AtomFlags.UserDataAtom)]
		KeywordsForArranger,
		[Atom("©cok", new [] { UserData }, AtomFlags.UserDataAtom)]
		KeywordsForComposer,
		[Atom("©com", new [] { UserData }, AtomFlags.UserDataAtom)]
		NameOfComposer,
		[Atom("©cpy", new [] { UserData }, AtomFlags.UserDataAtom)]
		CopyrightStatement,
		[Atom("©day", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieCreationDate,
		[Atom("©dir", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieDirectorName,
		[Atom("©ed1", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate1,
		[Atom("©ed2", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate2,
		[Atom("©ed3", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate3,
		[Atom("©ed4", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate4,
		[Atom("©ed5", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate5,
		[Atom("©ed6", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate6,
		[Atom("©ed7", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate7,
		[Atom("©ed8", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate8,
		[Atom("©ed9", new [] { UserData }, AtomFlags.UserDataAtom)]
		EditDate9,
		[Atom("©fmt", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieFormatIndication,
		[Atom("©inf", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieInformation,
		[Atom("©isr", new [] { UserData }, AtomFlags.UserDataAtom)]
		IsrcCode,
		[Atom("©lab", new [] { UserData }, AtomFlags.UserDataAtom)]
		RecordLabelName,
		[Atom("©lal", new [] { UserData }, AtomFlags.UserDataAtom)]
		RecordLabelUrl,
		[Atom("©mak", new [] { UserData }, AtomFlags.UserDataAtom)]
		FileCreatorName,
		[Atom("©mal", new [] { UserData }, AtomFlags.UserDataAtom)]
		FileCreatorUrl,
		[Atom("©nak", new [] { UserData }, AtomFlags.UserDataAtom)]
		ContentTitleKeywords,
		[Atom("©nam", new [] { UserData }, AtomFlags.UserDataAtom)]
		ContentTitle,
		[Atom("©pdk", new [] { UserData }, AtomFlags.UserDataAtom)]
		ProducerKeywords,
		[Atom("©phg", new [] { UserData }, AtomFlags.UserDataAtom)]
		RecordingCopyrightStatement,
		[Atom("©prd", new [] { UserData }, AtomFlags.UserDataAtom)]
		ProducerName,
		[Atom("©prf", new [] { UserData }, AtomFlags.UserDataAtom)]
		PerformersNames,
		[Atom("©prk", new [] { UserData }, AtomFlags.UserDataAtom)]
		MainPerformerKeywords,
		[Atom("©prl", new [] { UserData }, AtomFlags.UserDataAtom)]
		MainPerformerUrl,
		[Atom("©req", new [] { UserData }, AtomFlags.UserDataAtom)]
		SpecialHardwareAndSoftwareRequirements,
		[Atom("©snk", new [] { UserData }, AtomFlags.UserDataAtom)]
		ContentSubtitleKeywords,
		[Atom("©snm", new [] { UserData }, AtomFlags.UserDataAtom)]
		ContentSubtitle,
		[Atom("©src", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieSourceContentCredits,
		[Atom("©swf", new [] { UserData }, AtomFlags.UserDataAtom)]
		SongwriterName,
		[Atom("©swk", new [] { UserData }, AtomFlags.UserDataAtom)]
		SongwriterKeywords,
		[Atom("©swr", new [] { UserData }, AtomFlags.UserDataAtom)]
		NameAndVersionOfSoftwareOrHardwareThatGeneratedMovie,
		[Atom("©wrt", new [] { UserData }, AtomFlags.UserDataAtom)]
		MovieWriterName,
		[Atom("AllF", new [] { UserData }, AtomFlags.UserDataAtom)]
		PlayAllFrames,
		[Atom("hinf", new [] { UserData }, AtomFlags.UserDataAtom)]
		HintTrackInformation,
		[Atom("hnti", new [] { UserData }, AtomFlags.UserDataAtom)]
		HintInfoAtom,
		[Atom("name", new [] { UserData }, AtomFlags.UserDataAtom)]
		ObjectName,
		[Atom("LOOP", new [] { UserData }, AtomFlags.UserDataAtom)]
		LoopingStyle,
		[Atom("ptv ", new [] { UserData }, AtomFlags.UserDataAtom)]
		PrintToVideo,
		[Atom("SelO", new [] { UserData }, AtomFlags.UserDataAtom)]
		PlaySelectionOnly,
		[Atom("WLOC", new [] { UserData }, AtomFlags.UserDataAtom)]
		DefaultWindowLocationForMovie,
		// Track Reference Atoms
		[Atom("chap|sync|scpt|ssrc|hint|dpnd|ipir|mpod", new []{TrackReference}, AtomFlags.SizeAndType|AtomFlags.DuplicatesAllowed)]	// TODO: 'tmcd' is also a track reference type
		TrackReferenceType,
		//
		[Atom("tapt", new [] { Track }, AtomFlags.ContainerAtom)]
		TrackApertureModeDimensions,
		[Atom("clef", new [] { TrackApertureModeDimensions }, AtomFlags.SizeAndType)]
		CleanApertureDimentions,
		[Atom("prof", new [] { TrackApertureModeDimensions }, AtomFlags.SizeAndType)]
		ProductionApertureDimensions,
		[Atom("enof", new [] { TrackApertureModeDimensions }, AtomFlags.SizeAndType)]
		EncodedPixelsDimensions,
		[Atom("iods", new [] { Movie }, AtomFlags.FullAtom)]
		InitialObjectDescriptor,
		// Sample descriptions
		[Atom("tmcd", new[] { SampleDescription, Media, BaseMediaInformationHeader, TrackReference }, AtomFlags.ContainerAtom | AtomFlags.DuplicatesAllowed)]
		TimeCodeSampleDescription,
		// Table 4.1  Some image compression formats (pg.132) of QuickTime File Format specification, 2011-07-03.
		[Atom("cvid|jpeg|smc |rle |rpza|kpcd|png |mjpa|mjpb|SVQ1|SVQ3|mp4v|avc1|dvc |dvcp|gif |h263|tiff|2vuY|yuv2|v308|v408|v216|v410|v210|s263", new[] { SampleDescription }, AtomFlags.ContainerAtom)]
		VideoSampleDescription,
		// Table 4.7  Partial list of supported QuickTime audio formats (pg.150) of QuickTime File Format specification, 2011-07-03.
		[Atom("NONE|twos|sowt|MAC3|MAC6|ima4|fl32|fl64|in24|in32|ulaw|alaw|ms\u0000\u0002|ms\u0000\u0011|dvca|QDMC|QDM2|Qclp|ms\u0000\u0055|.mp3|mp4a|ac-3|samr|sawb|sawp", new[] { SampleDescription }, AtomFlags.ContainerAtom)]
		SoundSampleDescription,
		// TODO: add support for 'raw ' audio/video format
		//[Atom("raw ", new[] { SampleDescription }, AtomFlags.ContainerAtom)]
		//AudioVideoSampleDescription,
		[Atom("mp4s", new [] { SampleDescription }, AtomFlags.ContainerAtom)]
		MpegSampleDescription,
		// Sample description extensions
		[Atom("d263", new [] { VideoSampleDescription }, AtomFlags.ContainerAtom)]
		H263Specific,
		[Atom("bitr", new [] { H263Specific }, AtomFlags.SizeAndType)]
		Bitrate,
		[Atom("damr", new [] { SoundSampleDescription }, AtomFlags.SizeAndType)]
		AmrSpecific,
		[Atom("dawp", new [] { SoundSampleDescription }, AtomFlags.SizeAndType)]
		AmrwpSpecific,
		[Atom("esds", new [] { VideoSampleDescription, SoundSampleDescription, MpegSampleDescription }, AtomFlags.SizeAndType)]
		ElementaryStreamDescriptor,
		[Atom("colr", new [] { VideoSampleDescription }, AtomFlags.Box)]
		ColorParameter,
		[Atom("pasp", new [] { VideoSampleDescription }, AtomFlags.SizeAndType)]
		PixelAspectRatio,
	}

	/// <summary>
	/// Provides extension methods to the <c>AtomName</c> enumeration.
	/// </summary>
	internal static class AtomNameExtensions
	{
		#region Inner class
		/// <summary>
		/// Stores attributes for the AtomName enum type extension methods.
		/// </summary>
		private class AtomAttributes
		{
			public ICollection<uint> AtomTypes { get; private set; }
			public AtomFlags AtomFlags { get; private set; }
			public ICollection<AtomName> SuitableParents { get; private set; }

			public AtomAttributes(ICollection<uint> atomTypes, AtomFlags atomFlags, ICollection<AtomName> suitableParents)
			{
				AtomTypes = atomTypes;
				AtomFlags = atomFlags;
				SuitableParents = suitableParents;
			}
		}
		#endregion Inner class

		private static readonly Dictionary<uint, AtomName> AtomNameForAtomType;
		private static readonly Dictionary<AtomName, AtomAttributes> AtomAttributesForAtomName;

		/// <summary>Static data initialization.</summary>
		static AtomNameExtensions()
		{
			Type enumType = typeof(AtomName);

			ICollection<AtomName> allContainerAtoms = new HashSet<AtomName>();
			AtomNameForAtomType = new Dictionary<uint, AtomName>();
			AtomAttributesForAtomName = new Dictionary<AtomName,AtomAttributes>();

			allContainerAtoms.Add(AtomName.Root);

			// Use reflection to find the attributes describing the codec identifiers
			foreach (AtomName atomName in Enum.GetValues(enumType))
			{
				string name = Enum.GetName(enumType, atomName);

				FieldInfo fieldInfo = enumType.GetField(name);
				AtomAttribute[] attributes = (AtomAttribute[])fieldInfo.GetCustomAttributes(typeof(AtomAttribute), false);

				if (atomName == AtomName.Root)
				{
					// The root container has no metadata and does not occur in any other atom
					AtomAttributesForAtomName.Add(atomName, new AtomAttributes(new uint[0], AtomFlags.Container, new AtomName[0]));
				}
				else if (attributes != null && attributes.Length == 1)
				{
					AtomAttributes atomAttributes = GetAtomAttributes(atomName, attributes[0], allContainerAtoms);
					AtomAttributesForAtomName.Add(atomName, atomAttributes);

					// List all 4CC for this atom
					foreach (uint atomType in atomAttributes.AtomTypes)
					{
						Debug.Assert(!AtomNameForAtomType.ContainsKey(atomType), string.Format("Duplicate 4CC '{0}'", atomType.ToString4CC()));
						AtomNameForAtomType.Add(atomType, atomName);
					}
				}
				else
				{
					Debug.Fail(string.Format("No attributes for {0}. Please add attributes to the AtomName enumeration.", atomName));
				}
			}

			CheckSuitableParents();
		}

		/// <summary>
		/// Gets the atom attributes for the given <paramref name="atomName"/>
		/// using the information from the given <paramref name="attribute"/>.
		/// This adds the atom to the list of container atoms if-and-only-if the
		/// atom is a container atom.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <param name="attribute">the attribute for the given atom name</param>
		/// <param name="allContainerAtoms">the list of all container atoms</param>
		/// <returns>the atom attributes</returns>
		private static AtomAttributes GetAtomAttributes(AtomName atomName, AtomAttribute attribute, ICollection<AtomName> allContainerAtoms)
		{
			// Retrieve list of atom types (4CC)
			List<uint> atomTypes = new List<uint>();
			if (attribute.AtomType != null)
			{
				foreach (string atomType in attribute.AtomType.Split("|".ToCharArray()))
				{
					atomTypes.Add(atomType.To4CC());
				}
			}

			// Retrieve suitable parents, all container atoms if empty list
			ICollection<AtomName> suitableParents = attribute.SuitableParents;
			if (suitableParents.Count == 0)
			{
				suitableParents = allContainerAtoms;
			}

			// Update list of all container atoms
			if ((attribute.AtomFlags & AtomFlags.Container) == AtomFlags.Container)
			{
				allContainerAtoms.Add(atomName);
			}
			return new AtomAttributes(atomTypes.AsReadOnly(), attribute.AtomFlags, suitableParents);
		}

		/// <summary>
		/// Checks that the suitable parents of all atom names are container atoms.
		/// </summary>
		private static void CheckSuitableParents()
		{
			foreach (AtomName atomName in Enum.GetValues(typeof(AtomName)))
			{
				foreach (AtomName suitableParent in atomName.GetSuitableParents())
				{
					//Debug.Assert(suitableParent.IsFlagSet(AtomFlags.Container),
					//        string.Format("Suitable parent {0} of atom {1} must be a container atom.",
					//                suitableParent, atomName));
				}
			}
		}

		/*
		/// <summary>
		/// Gets the atom types field for the given <paramref name="atomName"/>.
		/// This returns an empty list if the atom name refers to <code>Root</code>
		/// or <code>Unknown</code> atoms.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <returns>the atom types field</returns>
		public static ICollection<uint> GetAtomTypes(this AtomName atomName)
		{
			return AtomAttributesForAtomName[atomName].AtomTypes;
		}
		*/

		/// <summary>
		/// Gets the atom type flags for the given <paramref name="atomName"/>.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <returns>the atom flags</returns>
		public static AtomFlags GetAtomFlags(this AtomName atomName)
		{
			return AtomAttributesForAtomName[atomName].AtomFlags;
		}

		/// <summary>
		/// Gets the suitable parents for the given <paramref name="atomName"/>.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <returns>the suitable parents</returns>
		public static ICollection<AtomName> GetSuitableParents(this AtomName atomName)
		{
			return AtomAttributesForAtomName[atomName].SuitableParents;
		}

		/// <summary>
		/// Returns whether the given <paramref name="atomName"/> is a top-level atom type.
		/// Top-level atoms are only allowed in the root. For example, the Movie atom.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <returns>true if it is a top-level atom type, false otherwise</returns>
		public static bool IsTopLevel(this AtomName atomName)
		{
			ICollection<AtomName> suitableParents = atomName.GetSuitableParents();
			return suitableParents.Count == 1 && suitableParents.Contains(AtomName.Root);
		}

		/// <summary>
		/// Returns whether the given <paramref name="atomFlag"/> is set for
		/// the given <paramref name="atomName"/>.
		/// </summary>
		/// <param name="atomName">the atom name</param>
		/// <param name="atomFlag">the flag to test</param>
		/// <returns>true if the flag is set, false otherwise</returns>
		public static bool IsFlagSet(this AtomName atomName, AtomFlags atomFlag)
		{
			return (atomName.GetAtomFlags() & atomFlag) == atomFlag;
		}

		/// <summary>
		/// Returns whether the given <paramref name="atomType"/> is a known atom type.
		/// </summary>
		/// <param name="atomType">the atom type field (4-character-code)</param>
		/// <returns>true if it is a known atom type, false otherwise</returns>
		public static bool IsKnownAtomType(this uint atomType)
		{
			return AtomNameForAtomType.ContainsKey(atomType);
		}

		/// <summary>
		/// Gets the atom name from the given <paramref name="fourCC"/>.
		/// This returns <code>AtomName.Unknown</code> if the FourCC is unknown.
		/// </summary>
		/// <param name="fourCC">the 4-character-code for the atom type field</param>
		/// <returns>the atom name</returns>
		public static AtomName GetAtomName(this uint fourCC)
		{
			AtomName atomName;
			if (!AtomNameForAtomType.TryGetValue(fourCC, out atomName))
			{
				atomName = AtomName.Unknown;
			}
			return atomName;
		}
	}

	/// <summary>
	/// Specifies the 4-character-code, type flags and suitable parent(s) for an atom.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class AtomAttribute : Attribute
	{
		#region Properties
		/// <summary>The atom type field 4CC as string.</summary>
		public string AtomType { get; private set; }
		/// <summary>The atom type flags.</summary>
		public AtomFlags AtomFlags { get; private set; }
		/// <summary>The suitable parent for the atom.</summary>
		public AtomName[] SuitableParents { get; private set; }
		#endregion Properties

		/// <summary>
		/// Creates a new atom attribute.
		/// </summary>
		/// <remarks>
		/// Use an empty list for <param name="suitableParents"/> to specify
		/// all container atoms.
		/// </remarks>
		/// <param name="atomType">the atom type field 4CC as string</param>
		/// <param name="suitableParents">the suitable parent for the atom</param>
		/// <param name="flags">the atom type flags</param>
		public AtomAttribute(string atomType, AtomName[] suitableParents, AtomFlags flags)
		{
			AtomType = atomType;
			AtomFlags = flags;
			SuitableParents = suitableParents;
		}
	}
}
