using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CityConquestLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<ICityFactory>                                  MockCityFactory;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;

        private CitySignals                                         CitySignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCityFactory         = new Mock<ICityFactory>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();

            CitySignals             = new CitySignals();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<CitySignals>                                  ().FromInstance(CitySignals);

            Container.Bind<CityConquestLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "HandleCityCaptureFromCombat should result in a captured city if and only if: " +
            "\n1. The defender is of type City" +
            "\n2. The defender has zero or negative hitpoints" + 
            "\n3. CombatInfo.CombatType equals CombatType.Melee" + 
            "\nThis should result in the conquest of the city for which defender is the CombatFacade. " +
            "That city should be transferred to the civilization that owns the attacker.")]
        public void HandleCityCaptureFromCombat_AttackerChangesDefendingOwnerOfDefendingCity() {
            var attackerOwner = BuildCivilization();

            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 0, UnitType.City), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            MockCityPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityBeingCaptured, attackerOwner),
                Times.Once, "City was not transferred to the attacker's owner as expected"
            );
        }

        [Test(Description = "When HandleCityCaptureFromCombat is called and the attacker " +
            "conquers a city, that attacker should have its current path set to the city's location " +
            "and have its PerformMovement method called, with movement costs being ignored")]
        public void HandleCityCaptureFromCombat_AttackerMovesIntoCity() {
            var attackerOwner = BuildCivilization();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee, out attackerMock);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 0, UnitType.City), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            CollectionAssert.AreEqual(
                new List<IHexCell>() { cityLocation }, attacker.CurrentPath,
                "CurrentPath was not set correctly"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(true), Times.Once,
                "PerformMovement was not called correctly"
            );
        }

        [Test(Description = "When HandleCityCaptureFromCombat is called and the attacker " +
            "conquers a city, all units previously on that city not of type City should be " +
            "moved to the null location and destroyed")]
        public void HandleCityCaptureFromCombat_AllOccupantsDislocatedAndDestroyed() {
            var attackerOwner = BuildCivilization();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee, out attackerMock);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            Mock<IUnit> mockMeleeInCity;
            Mock<IUnit> mockCivilianInCity;
            Mock<IUnit> mockCombatFacade;

            var meleeInCity    = BuildUnit(cityOwner, 100, UnitType.Melee,    out mockMeleeInCity);
            var civilianInCity = BuildUnit(cityOwner, 100, UnitType.Civilian, out mockCivilianInCity);
            var combatFacade   = BuildUnit(cityOwner, 0,   UnitType.City,     out mockCombatFacade);

            var unitsInCity = new List<IUnit>() {
                meleeInCity, civilianInCity, combatFacade
            };

            var cityBeingCaptured = BuildCity(cityLocation, cityOwner, combatFacade, unitsInCity);

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(cityLocation))
                .Returns(unitsInCity);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            mockMeleeInCity   .Verify(unit => unit.Destroy(), Times.Once,  "MeleeInCity was not destroyed");
            mockCivilianInCity.Verify(unit => unit.Destroy(), Times.Once,  "CivilianInCity was not destroyed");
            mockCombatFacade  .Verify(unit => unit.Destroy(), Times.Never, "CombatFacade was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void HandleCityCaptureFromCombat_DoesNothingIfDefenderHasPositiveHitpoints() {
            var attackerOwner = BuildCivilization();

            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 50, UnitType.City), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            MockCityPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityBeingCaptured, attackerOwner),
                Times.Never, "City was unexpectedly transferred to the attacker's owner"
            );
        }

        [Test(Description = "")]
        public void HandleCityCaptureFromCombat_DoesNothingIfDefenderNotOfTypeCity() {
            var attackerOwner = BuildCivilization();

            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 0, UnitType.Melee), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            MockCityPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityBeingCaptured, attackerOwner),
                Times.Never, "City was unexpectedly transferred to the attacker's owner"
            );
        }

        [Test(Description = "")]
        public void HandleCityCaptureFromCombat_DoesNothingIfCombatTypeNotMelee() {
            var attackerOwner = BuildCivilization();

            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 0, UnitType.City), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            MockCityPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityBeingCaptured, attackerOwner),
                Times.Never, "City was unexpectedly transferred to the attacker's owner"
            );
        }

        [Test]
        public void HandleCityCaptureFromCombat_CityCaptureSignalFired() {
            var attackerOwner = BuildCivilization();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(attackerOwner, 0, UnitType.Melee, out attackerMock);

            var cityOwner = BuildCivilization();
            var cityLocation = BuildHexCell();

            var cityBeingCaptured = BuildCity(
                cityLocation, cityOwner, BuildUnit(cityOwner, 0, UnitType.City), new List<IUnit>()
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var conquestLogic = Container.Resolve<CityConquestLogic>();

            CitySignals.CityCapturedSignal.Subscribe(delegate(CityCaptureData captureData) {
                Assert.AreEqual(cityBeingCaptured, captureData.City,     "Incorrect City passed");
                Assert.AreEqual(cityOwner,         captureData.OldOwner, "Incorrect OldOwner passed");
                Assert.AreEqual(attackerOwner,     captureData.NewOwner, "Incorrect NewOwner passed");

                Assert.Pass();
            });

            conquestLogic.HandleCityCaptureFromCombat(attacker, cityBeingCaptured.CombatFacade, combatInfo);

            Assert.Fail("Event not evoked");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICity BuildCity(
            IHexCell location, ICivilization owner, IUnit combatFacade, List<IUnit> occupyingUnits
        ){
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.CombatFacade).Returns(combatFacade);

            var newCity = mockCity.Object;

            MockCityLocationCanon  .Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location)).Returns(occupyingUnits);

            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>() { newCity }.AsReadOnly());

            return newCity;
        }

        private IUnit BuildUnit(ICivilization owner, int hitpoints, UnitType type) {
            Mock<IUnit> mock;
            return BuildUnit(owner, hitpoints, type, out mock);
        }

        private IUnit BuildUnit(ICivilization owner, int hitpoints, UnitType type, out Mock<IUnit> unitMock) {
            unitMock = new Mock<IUnit>();

            unitMock.SetupAllProperties();

            unitMock.Setup(unit => unit.CurrentHitpoints).Returns(hitpoints);
            unitMock.Setup(unit => unit.Type)     .Returns(type);

            var newUnit = unitMock.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
