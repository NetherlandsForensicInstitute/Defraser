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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Defraser.Detector.Common;
using Defraser.Detector.QT;
using Defraser.Interface;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Defraser.Test
{
	[TestFixture]
	public class TestQtDetector
	{
		#region Inner classes
		sealed class EqualityComparer : IEqualityComparer<QtAtom>
		{
			public bool Equals(QtAtom x, QtAtom y)
			{
				return x.GetType() == y.GetType();
			}

			public int GetHashCode(QtAtom obj)
			{
				throw new NotImplementedException();
			}
		}
		#endregion Inner classes

		private const string PathName = ".";
		private readonly Dictionary<string, AtomAttribute> _attributeForAtoms = new Dictionary<string, AtomAttribute>();
		private IDetector _detector;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			TestFramework.DetectorFactory.Initialize(PathName);

			foreach (IDetector detector in TestFramework.DetectorFactory.Detectors) 
			{
				if (detector.SupportedFormats.Contains(CodecID.QuickTime))
				{
					_detector = detector;
					break;
				}
			}

			Type enumType = typeof(AtomName);

			// Use reflection to find the attributes describing the codec identifiers
			foreach (AtomName atomName in Enum.GetValues(enumType))
			{
				string name = Enum.GetName(enumType, atomName);

				FieldInfo fieldInfo = enumType.GetField(name);
				AtomAttribute[] attributes = (AtomAttribute[])fieldInfo.GetCustomAttributes(typeof(AtomAttribute), false);

				if (atomName == AtomName.Root) continue;

				Assert.IsNotNull(attributes, "Could not get attributes for atom {0}", name);
				Assert.That(attributes.Length, Is.EqualTo(1), string.Format("Atom '{0}' has '{1}' attributes instead of '1'", name, attributes.Length));

				if (attributes[0].AtomType != null)
				{
					foreach (string atomType in attributes[0].AtomType.Split("|".ToCharArray()))
					{
						Assert.That(_attributeForAtoms.ContainsKey(atomType), Is.Not.True, string.Format("Double 4CC found: '{0}'.", atomType));

						_attributeForAtoms.Add(atomType, attributes[0]);
					}
				}
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{}

		[Test]
		public void TestAtom4CCsAreOfLength4()
		{
			foreach (string atom4CC in _attributeForAtoms.Keys)
			{
				Assert.That(atom4CC.Length, Is.EqualTo(4), string.Format("The length of the 4CC should be equal to '4'. The length is {0}.", atom4CC.Length));
			}
		}

		[Test]
		public void TestAtom4CCsAreValid()
		{
			foreach (string atom4CC in _attributeForAtoms.Keys)
			{
				Assert.That(atom4CC.To4CC().IsValid4CC(), Is.True, string.Format("4CC '{0}' is not valid.", atom4CC));
			}
		}

		[Test]
		public void TestAtom4CCIsKnownAtomType()
		{
			foreach (string atom4CC in _attributeForAtoms.Keys)
			{
				Assert.That(atom4CC.To4CC().IsKnownAtomType(), Is.True, string.Format("4CC '{0}' is unknown.", atom4CC));
			}
		}

		[Test]
		public void TestAtomParentHaveContainerFlagSet()
		{
			// Check that container flag is set for the parent atom
			foreach (var attributeForAtom in _attributeForAtoms)
			{
				//string parent = string.Empty;
				AtomAttribute atomAttribute = attributeForAtom.Value;
				AtomName[] suitableParentsAtomNames = atomAttribute.SuitableParents;

				foreach (AtomName suitableParentsAtomName in suitableParentsAtomNames)
				{
					// Note: Although 'meta' is implemented as a full atom, it is still considered a container atom in some cases!
					Assert.That(suitableParentsAtomName.IsFlagSet(AtomFlags.Container) || (suitableParentsAtomName == AtomName.MetaData), Is.True, string.Format("The parent ({0}) of atom '{1}' must have the container flag set", Enum.GetName(typeof(AtomName), suitableParentsAtomName), attributeForAtom.Key));
				}
			}
		}

		[Test]
		public void TestAtomIsCreatedByFactoryMethod()
		{
			// Create a collection containing all classes derived from QtAtom
			// from the QuickTime/MP4/3GPP plug-in.
			IEnumerable<QtAtom> atomsFromComponent = GetAtomsFromComponent();

			// Call the QtAtom factory method to find out which classes are created
			HashSet<QtAtom> atomsCreatedByFactory = new HashSet<QtAtom>();
			QtAtom dummyAtom = new QtAtom(Enumerable.Repeat(_detector, 1));

			foreach (AtomName atomName in Enum.GetValues(typeof(AtomName)))
			{
				QtAtom atom = QtParser.CreateAtom(dummyAtom, atomName, 0U);
				Assert.IsNotNull(atom);

				if (!atomsCreatedByFactory.Contains(atom, new EqualityComparer()))
				{
					atomsCreatedByFactory.Add(atom);
				}
			}

			// Report any atom from component not created by the factory method
			foreach (QtAtom atom in atomsFromComponent)
			{
				if (!atomsCreatedByFactory.Contains(atom, new EqualityComparer()))
				{
					Assert.Fail("Atom not created by factory: {0}", atom.GetType().Name);
				}
			}

			// Atoms created by factory not in Component
			foreach (QtAtom atom in atomsCreatedByFactory)
			{
				if (!atomsFromComponent.Contains(atom, new EqualityComparer()))
				{
					Assert.Fail("Atom not in component: {0}", atom.GetType().Name);
				}
			}
		}

		private IEnumerable<QtAtom> GetAtomsFromComponent()
		{
			QtAtom dummyAtom = new QtAtom(Enumerable.Repeat(_detector, 1));
			HashSet<QtAtom> atoms = new HashSet<QtAtom>();

			Assert.That(File.Exists(string.Format(Path.Combine(PathName, "QTDetector.dll"))), Is.True, "Could not find the file 'QTDetector.dll'");

			// Using 'LoadFile()' instead of 'LoadFrom()' causes the
			// 'GetDetector()' method to fail for unknown reason.
			Assembly assembly = Assembly.LoadFrom("QTDetector.dll");

			foreach (Type type in assembly.GetTypes())
			{
				if (typeof(QtAtom).IsAssignableFrom(type) && !type.IsAbstract /*&& type != typeof(QtAtom)*/)
				{
					QtAtom atom = null;
					// Try to get constructor with previousAtom as the only argument
					ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] {typeof(QtAtom)}, null);
					if (constructorInfo != null)
					{
						atom = (QtAtom)constructorInfo.Invoke(BindingFlags.Default, null, new object[] { dummyAtom }, CultureInfo.CurrentCulture);
					}
					if (constructorInfo == null)
					{
						// If the above fails, try to get the constructor with
						// previousAtom (QtAom) and atomName (AtomName) as the arguments.
						constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(QtAtom), typeof(AtomName) }, null);

						if(constructorInfo != null)
						{
							atom = (QtAtom)constructorInfo.Invoke(BindingFlags.Default, null, new object[] { dummyAtom, AtomName.Unknown }, CultureInfo.CurrentCulture);
						}
					}
					if(constructorInfo == null)
					{
						// If the above fails, try to get the constructor with
						// previousAtom (QtAom) and atomType (AtomType) as the arguments.
						constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(QtAtom), typeof(ChunkOffset.AtomType) }, null);

						if (constructorInfo != null)
						{
							atom = (QtAtom)constructorInfo.Invoke(BindingFlags.Default, null, new object[] { dummyAtom, ChunkOffset.AtomType.ChunkOffset32Bit }, CultureInfo.CurrentCulture);
						}
					}

					Assert.IsNotNull(atom);

					if (!atoms.Contains(atom, new EqualityComparer()))
					{
						atoms.Add(atom);
					}
					else
					{
						Assert.Fail("atom '{0}' already added.", Enum.GetName(typeof(AtomName),atom.HeaderName));
					}
				}
			}
			return atoms;
		}
	}
}
