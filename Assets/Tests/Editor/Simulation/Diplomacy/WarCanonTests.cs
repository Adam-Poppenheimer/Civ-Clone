using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Tests.Simulation.Diplomacy {

    [TestFixture]
    public class WarCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class WarCanonPairTestData {

            public CivilizationTestData Attacker;

            public CivilizationTestData Defender;

            public bool BeginAtWar;

        }

        public class WarCanonMultilateralTestData {

            public CivilizationTestData FocusedCiv;

            public List<RelativeCivilizationTestData> OtherCivs;

        }

        public class CivilizationTestData {



        }

        public class RelativeCivilizationTestData {

            public CivilizationTestData UnderlyingCiv;

            public bool AtWarWithFocusedCiv;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable AreAtWarTestCases {
            get {
                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = false
                }).SetName("DeclareWar not called on civilizations").Returns(false);

                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = true
                }).SetName("DeclareWar called on civilizations").Returns(true);
            }
        }

        public static IEnumerable AreAtPeaceTestCases {
            get {
                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = false
                }).SetName("DeclareWar not called on civilizations").Returns(true);

                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = true
                }).SetName("DeclareWar called on civilizations").Returns(false);
            }
        }

        public static IEnumerable CanDeclareWarTestCases {
            get {
                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = false
                }).SetName("DeclareWar not called on civilizations").Returns(true);

                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = true
                }).SetName("DeclareWar called on civilizations").Returns(false);
            }
        }

        public static IEnumerable CanEstablishPeaceTestCases {
            get {
                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = false
                }).SetName("DeclareWar not called on civilizations").Returns(false);

                yield return new TestCaseData(new WarCanonPairTestData() {
                    Attacker = new CivilizationTestData(),
                    Defender = new CivilizationTestData(),
                    BeginAtWar = true
                }).SetName("DeclareWar called on civilizations").Returns(true);
            }
        }

        public static IEnumerable GetCivsAtWarWithCiv_AcquiresAllBelligerentsTestCases {
            get {
                yield return new TestCaseData(new WarCanonMultilateralTestData() {
                    FocusedCiv = new CivilizationTestData(),
                    OtherCivs = new List<RelativeCivilizationTestData>() {
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                    }
                }).SetName("3 civs at war, 2 civs at peace");

                yield return new TestCaseData(new WarCanonMultilateralTestData() {
                    FocusedCiv = new CivilizationTestData(),
                    OtherCivs = new List<RelativeCivilizationTestData>() {
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                    }
                }).SetName("0 civs at war, 5 civs at peace");
            }
        }

        public static IEnumerable GetCivsAtPeaceWithCiv_AcquiresAllNonBelligerentsTestCases {
            get {
                yield return new TestCaseData(new WarCanonMultilateralTestData() {
                    FocusedCiv = new CivilizationTestData(),
                    OtherCivs = new List<RelativeCivilizationTestData>() {
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = false
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                    }
                }).SetName("3 civs at war, 2 civs at peace");

                yield return new TestCaseData(new WarCanonMultilateralTestData() {
                    FocusedCiv = new CivilizationTestData(),
                    OtherCivs = new List<RelativeCivilizationTestData>() {
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                        new RelativeCivilizationTestData() {
                            UnderlyingCiv = new CivilizationTestData(),
                            AtWarWithFocusedCiv = true
                        },
                    }
                }).SetName("5 civs at war, 0 civs at peace");
            }
        }

        #endregion

        #region instance fields and properties

        private List<ICivilization> AllCivilizations = new List<ICivilization>();

        private Mock<ICivilizationFactory> MockCivFactory;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivilizations.Clear();

            MockCivFactory = new Mock<ICivilizationFactory>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivilizations.AsReadOnly());

            Container.Bind<ICivilizationFactory>().FromInstance(MockCivFactory.Object);

            Container.Bind<WarCanon>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("AreAtWarTestCases")]
        [Test(Description = "")]
        public bool AreAtWarTests(WarCanonPairTestData testData) {
            var attacker = BuildCivilization(testData.Attacker);
            var defender = BuildCivilization(testData.Defender);

            var warCanon = Container.Resolve<WarCanon>();

            if(testData.BeginAtWar) {
                warCanon.DeclareWar(attacker, defender);
            }

            return warCanon.AreAtWar(attacker, defender);
        }

        [TestCaseSource("AreAtPeaceTestCases")]
        [Test(Description = "")]
        public bool AreAtPeaceTests(WarCanonPairTestData testData) {
            var attacker = BuildCivilization(testData.Attacker);
            var defender = BuildCivilization(testData.Defender);

            var warCanon = Container.Resolve<WarCanon>();

            if(testData.BeginAtWar) {
                warCanon.DeclareWar(attacker, defender);
            }

            return warCanon.AreAtPeace(attacker, defender);
        }

        [TestCaseSource("CanDeclareWarTestCases")]
        [Test(Description = "")]
        public bool CanDeclareWarTests(WarCanonPairTestData testData) {
            var attacker = BuildCivilization(testData.Attacker);
            var defender = BuildCivilization(testData.Defender);

            var warCanon = Container.Resolve<WarCanon>();

            if(testData.BeginAtWar) {
                warCanon.DeclareWar(attacker, defender);
            }

            return warCanon.CanDeclareWar(attacker, defender);
        }

        [TestCaseSource("CanEstablishPeaceTestCases")]
        [Test(Description = "")]
        public bool CanEstablishPeaceTests(WarCanonPairTestData testData) {
            var attacker = BuildCivilization(testData.Attacker);
            var defender = BuildCivilization(testData.Defender);

            var warCanon = Container.Resolve<WarCanon>();

            if(testData.BeginAtWar) {
                warCanon.DeclareWar(attacker, defender);
            }

            return warCanon.CanEstablishPeace(attacker, defender);
        }

        [TestCaseSource("GetCivsAtWarWithCiv_AcquiresAllBelligerentsTestCases")]
        [Test(Description = "")]
        public void GetCivsAtWarWithCiv_AcquiresAllBelligerents(WarCanonMultilateralTestData testData) {
            var focusedCiv = BuildCivilization(testData.FocusedCiv);

            var warCanon = Container.Resolve<WarCanon>();

            var expectedBelligerents = new List<ICivilization>();

            foreach(var otherCivData in testData.OtherCivs) {
                var otherCiv = BuildCivilization(otherCivData.UnderlyingCiv);

                if(otherCivData.AtWarWithFocusedCiv) {
                    warCanon.DeclareWar(focusedCiv, otherCiv);

                    expectedBelligerents.Add(otherCiv);
                }
            }

            CollectionAssert.AreEquivalent(expectedBelligerents, warCanon.GetCivsAtWarWithCiv(focusedCiv));
        }

        [TestCaseSource("GetCivsAtPeaceWithCiv_AcquiresAllNonBelligerentsTestCases")]
        [Test(Description = "")]
        public void GetCivsAtPeaceWithCiv_AcquiresAllNonBelligerents(WarCanonMultilateralTestData testData) {
            var focusedCiv = BuildCivilization(testData.FocusedCiv);

            var warCanon = Container.Resolve<WarCanon>();

            var expectedNonbelligerents = new List<ICivilization>();

            foreach(var otherCivData in testData.OtherCivs) {
                var otherCiv = BuildCivilization(otherCivData.UnderlyingCiv);

                if(otherCivData.AtWarWithFocusedCiv) {
                    warCanon.DeclareWar(focusedCiv, otherCiv);
                }else {
                    expectedNonbelligerents.Add(otherCiv);
                }
            }

            CollectionAssert.AreEquivalent(expectedNonbelligerents, warCanon.GetCivsAtPeaceWithCiv(focusedCiv));
        }

        [Test(Description = "When DeclareWar is called on arguments for which CanDeclareWar would be false, " +
            "DeclareWar should throw an InvalidOperationException")]
        public void DeclareWar_ThrowsWhenCannotDeclareWar() {
            var attacker = BuildCivilization(new CivilizationTestData());
            var defender = BuildCivilization(new CivilizationTestData());

            var warCanon = Container.Resolve<WarCanon>();

            warCanon.DeclareWar(attacker, defender);

            Assert.Throws<InvalidOperationException>(
                () => warCanon.DeclareWar(attacker, defender),
                "DeclareWar failed to throw an exception on an attacker/defender pair that should've been invalid"
            );
        }

        [Test(Description = "When EstablishPeace is called on arguments for which CanEstablishPeace would be false, " + 
            "EstablishPeace should throw an InvalidOperationException")]
        public void EstablishPeace_ThrowsWhenCannotEstablishPeace() {
            var attacker = BuildCivilization(new CivilizationTestData());
            var defender = BuildCivilization(new CivilizationTestData());

            var warCanon = Container.Resolve<WarCanon>();

            Assert.Throws<InvalidOperationException>(
                () => warCanon.EstablishPeace(attacker, defender),
                "EstablishPeace failed to throw an exception on a civilization pair that should've been invalid"
            );
        }

        [Test(Description = "")]
        public void Clear_AllWarRelationsRemoved() {
            var attackerOne   = BuildCivilization(new CivilizationTestData());
            var attackerTwo   = BuildCivilization(new CivilizationTestData());
            var attackerThree = BuildCivilization(new CivilizationTestData());

            var defenderOne   = BuildCivilization(new CivilizationTestData());
            var defenderTwo   = BuildCivilization(new CivilizationTestData());
            var defenderThree = BuildCivilization(new CivilizationTestData());

            var warCanon = Container.Resolve<WarCanon>();

            warCanon.DeclareWar(attackerOne,   defenderOne);
            warCanon.DeclareWar(attackerTwo,   defenderTwo);
            warCanon.DeclareWar(attackerThree, defenderThree);

            warCanon.Clear();

            Assert.IsFalse(warCanon.AreAtWar(attackerOne,   defenderOne),   "attackerOne and defenderOne are still at war");
            Assert.IsFalse(warCanon.AreAtWar(attackerTwo,   defenderTwo),   "attackerTwo and defenderTwo are still at war");
            Assert.IsFalse(warCanon.AreAtWar(attackerThree, defenderThree), "attackerThree and defenderThree are still at war");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(CivilizationTestData civData) {
            var mockCiv = new Mock<ICivilization>();
            mockCiv.Name = "Mock Civilization " + AllCivilizations.Count.ToString();

            var newCiv = mockCiv.Object;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
