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

//using System;
//using System.Collections.Generic;
//using System.Text;

//using NUnit.Framework;
//using Defraser;
//using System.IO;

//namespace TestDefraser
//{
//    [TestFixture]
//    public class Test3GppDetector
//    {
//        private const string ProjectInvestigator = "Paulus Pietsma";
//        private readonly DateTime ProjectCreationDate = DateTime.Now;
//        private const string ProjectDescription = "<description>";
//        private const string FileNameTwoCans = Util.TestdataPath + "skating-dog.3gp";
//        private const string FileNameBackUp = Util.TestdataPath + "dancing-skeleton.3gp";

//        private IProject _twoCans;
//        private IProject _backUp;
//        private Framework _framework;
//        IDetectorInfo _mpegDetector;

//        [TestFixtureSetUp]
//        public void TestFixtureSetup()
//        {
//            _framework = Framework.Instance;
//            _framework.Initialize(@"../../../Phone3GPPDetector/bin/Debug");

//            _mpegDetector = _framework.Detectors[0];

//            _twoCans = _framework.CreateProject(@"./SkatingDog.dpr", ProjectInvestigator, ProjectCreationDate, ProjectDescription);
//            _framework.DetectData(_mpegDetector, _twoCans, FileNameTwoCans, null);

//            _backUp = _framework.CreateProject(@"./DancingSkeleton.dpr", ProjectInvestigator, ProjectCreationDate, ProjectDescription);
//            _framework.DetectData(_mpegDetector, _backUp, FileNameBackUp, null);
//        }

//        [TestFixtureTearDown]
//        public void TestFixtureTeardown()
//        {
//            _framework.CloseProject(_twoCans);
//            _framework.CloseProject(_backUp);

//            File.Delete(@"./SkatingDog.dpr");
//            File.Delete(@"./DancingSkeleton.dpr");
//        }

//        [Test]
//        public void TestSkatingDogDataBlockCount()
//        {
//            Assert.AreEqual(2, _twoCans.GetDataBlocks(_twoCans.GetInputFiles()[0], _twoCans.GetDetectors(_twoCans.GetInputFiles()[0])[0]).Length);
//        }

//        [Test]
//        public void TestDancingSkeletonDataBlockCount()
//        {
//            Assert.AreEqual(1, _backUp.GetDataBlocks(_twoCans.GetInputFiles()[0], _backUp.GetDetectors(_backUp.GetInputFiles()[0])[0]).Length);
//        }

//    }
//}
