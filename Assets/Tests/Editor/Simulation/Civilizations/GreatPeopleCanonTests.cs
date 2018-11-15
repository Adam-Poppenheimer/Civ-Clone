using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Civilizations {

    public class GreatPeopleCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationConfig> MockCivConfig;
        private Mock<IUnitConfig>         MockUnitConfig;
        private CivilizationSignals       CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCivConfig  = new Mock<ICivilizationConfig>();
            MockUnitConfig = new Mock<IUnitConfig>();
            CivSignals     = new CivilizationSignals();

            Container.Bind<ICivilizationConfig>().FromInstance(MockCivConfig .Object);
            Container.Bind<IUnitConfig>        ().FromInstance(MockUnitConfig.Object);
            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<GreatPersonCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetPointsTowardsTypeForCiv_DefaultsAtZero() {
            var civ = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            Assert.AreEqual(
                0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ),
                "GreatArtist had an unexpected default value"
            );

            Assert.AreEqual(
                0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer had an unexpected default value"
            );

            Assert.AreEqual(
                0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ),
                "GreatGeneral had an unexpected default value"
            );
        }

        [Test]
        public void AddPointsTowardsTypeForCiv_AddsToExistingValueForType() {
            var civ = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ, 10);
            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ, 20);
            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ, 30);

            Assert.AreEqual(
                10, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ),
                "GreatArtist had an unexpected value"
            );

            Assert.AreEqual(
                20, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer had an unexpected value"
            );

            Assert.AreEqual(
                30, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ),
                "GreatGeneral had an unexpected value"
            );
        }

        [Test]
        public void SetPointsTowardsTypeForCiv_OverridesExistingValueForType() {
            var civ = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ, 10);
            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ, 20);
            greatPeopleCanon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ, 30);

            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ, 1);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ, 2);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ, 3);

            Assert.AreEqual(
                1, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist,   civ),
                "GreatArtist had an unexpected value"
            );

            Assert.AreEqual(
                2, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer had an unexpected value"
            );

            Assert.AreEqual(
                3, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral,  civ),
                "GreatGeneral had an unexpected value"
            );
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeIsCivilian_StartsAtCivilianStartingCost() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(200);

            MockUnitConfig.Setup(config => config.GreatPeopleCivilianTypes).Returns(
                new List<GreatPersonType>() { GreatPersonType.GreatArtist, GreatPersonType.GreatEngineer }
            );

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            Assert.AreEqual(
                100, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                100, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeIsCivilian_RisesExponentiallyWithCivilianPredecessors() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(200);

            MockCivConfig.Setup(config => config.GreatPersonPredecessorCostMultiplier).Returns(2f);

            MockUnitConfig.Setup(config => config.GreatPeopleCivilianTypes).Returns(
                new List<GreatPersonType>() {
                    GreatPersonType.GreatArtist, GreatPersonType.GreatEngineer, GreatPersonType.GreatScientist
                }
            );

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatScientist });

            Assert.AreEqual(
                200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatScientist });

            Assert.AreEqual(
                400, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                400, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeIsMilitary_StartsAtMilitaryStartingCost() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(200);

            MockUnitConfig.Setup(config => config.GreatPeopleMilitaryTypes).Returns(
                new List<GreatPersonType>() { GreatPersonType.GreatArtist, GreatPersonType.GreatEngineer }
            );

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            Assert.AreEqual(
                200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeIsMilitary_RisesExponentiallyWithMilitaryPredecessors() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(300);

            MockCivConfig.Setup(config => config.GreatPersonPredecessorCostMultiplier).Returns(2f);

            MockUnitConfig.Setup(config => config.GreatPeopleMilitaryTypes).Returns(
                new List<GreatPersonType>() {
                    GreatPersonType.GreatArtist, GreatPersonType.GreatEngineer, GreatPersonType.GreatScientist
                }
            );

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatScientist });

            Assert.AreEqual(
                600, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                600, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatScientist });

            Assert.AreEqual(
                1200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatArtist, civ),
                "GreatArtist needs an unexpected number of points"
            );
            
            Assert.AreEqual(
                1200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatEngineer, civ),
                "GreatEngineer needs an unexpected number of points"
            );
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeNotAligned_StartsAtCivilianStartingCost() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(300);

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            Assert.AreEqual(100, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatAdmiral, civ));
        }

        [Test]
        public void GetPointsNeededForTypeForCiv_AndTypeNotAligned_RisesExponentiallyWithSameTypePredecessors() {
            var civ = BuildCiv();

            MockCivConfig.Setup(config => config.CivilianGreatPersonStartingCost).Returns(100);
            MockCivConfig.Setup(config => config.MilitaryGreatPersonStartingCost).Returns(300);

            MockCivConfig.Setup(config => config.GreatPersonPredecessorCostMultiplier).Returns(2f);

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatAdmiral });

            Assert.AreEqual(200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatAdmiral, civ));

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatEngineer });

            Assert.AreEqual(200, greatPeopleCanon.GetPointsNeededForTypeForCiv(GreatPersonType.GreatAdmiral, civ));
        }

        [Test]
        public void GreatPersonBornFired_PredecessorListForTypeIncremented() {
            var civ = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civ, Type = GreatPersonType.GreatAdmiral });
            
            Assert.AreEqual(1, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatAdmiral, civ));
        }

        [Test]
        public void Clear_AccruedPointsClearedForAllTypesAndAllCivs() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civOne, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civOne, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civTwo, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civTwo, 10f);

            greatPeopleCanon.Clear();

            Assert.AreEqual(0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civOne), "GreatAdmiral for civOne not cleared");
            Assert.AreEqual(0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civOne), "GreatEngineer for civOne not cleared");
            Assert.AreEqual(0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civTwo), "GreatAdmiral for civTwo not cleared");
            Assert.AreEqual(0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civTwo), "GreatEngineer for civTwo not cleared");
        }

        [Test]
        public void Clear_PredecessorCountClearedForAllTypesAndAllCivs() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civOne, Type = GreatPersonType.GreatAdmiral });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civOne, Type = GreatPersonType.GreatArtist });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civTwo, Type = GreatPersonType.GreatAdmiral });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civTwo, Type = GreatPersonType.GreatArtist });

            greatPeopleCanon.Clear();

            Assert.AreEqual(0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatAdmiral, civOne), "GreatAdmiral not cleared for civOne");
            Assert.AreEqual(0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatArtist, civOne),  "GreatArtist not cleared for civOne");
            Assert.AreEqual(0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatAdmiral, civTwo), "GreatAdmiral not cleared for civTwo");
            Assert.AreEqual(0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatArtist, civTwo),  "GreatArtist not cleared for civTwo");
        }

        [Test]
        public void ClearCiv_AccruedPointsClearedForAllTypesOnArguedCiv() {
            var civOne = BuildCiv("Civ One");
            var civTwo = BuildCiv("Civ Two");

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civOne, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civOne, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral,  civTwo, 10f);
            greatPeopleCanon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civTwo, 10f);

            greatPeopleCanon.ClearCiv(civOne);

            Assert.AreEqual(
                0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civOne),
                "GreatAdmiral for civOne not cleared"
            );
            Assert.AreEqual(
                0, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civOne),
                "GreatEngineer for civOne not cleared"
            );

            Assert.AreEqual(
                10, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civTwo),
                "GreatAdmiral for civTwo unexpectedly cleared"
            );
            Assert.AreEqual(
                10, greatPeopleCanon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civTwo),
                "GreatEngineer for civTwo unexpectedly cleared"
            );
        }

        [Test]
        public void ClearCiv_PredecessorCountClearedForAllTypesOnArguedCiv() {
            var civOne = BuildCiv();
            var civTwo = BuildCiv();

            var greatPeopleCanon = Container.Resolve<GreatPersonCanon>();

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civOne, Type = GreatPersonType.GreatAdmiral });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civOne, Type = GreatPersonType.GreatArtist });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civTwo, Type = GreatPersonType.GreatAdmiral });
            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() { Owner = civTwo, Type = GreatPersonType.GreatArtist });

            greatPeopleCanon.ClearCiv(civOne);

            Assert.AreEqual(
                0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatAdmiral, civOne),
                "GreatAdmiral not cleared for civOne"
            );
            Assert.AreEqual(
                0, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatArtist, civOne),
                "GreatArtist not cleared for civOne"
            );

            Assert.AreEqual(
                1, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatAdmiral, civTwo),
                "GreatAdmiral not cleared for civTwo"
            );
            Assert.AreEqual(
                1, greatPeopleCanon.GetPredecessorsOfTypeForCiv(GreatPersonType.GreatArtist, civTwo),
                "GreatArtist not cleared for civTwo"
            );
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(string name = "Civ") {
            var mock = new Mock<ICivilization>();

            mock.Name = name;

            return mock.Object;
        }

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
