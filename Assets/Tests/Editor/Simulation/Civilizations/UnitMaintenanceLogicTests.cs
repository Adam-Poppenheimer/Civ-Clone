using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Civilizations {

    public class UnitMaintenanceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetMaintenanceOfUnitsForCivTestData {

            public int UnitCount;

            public int MaintenanceFreeUnits;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable GetMaintenanceOfUnitsForCivTestCases {
            get {
                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 0, MaintenanceFreeUnits = 0
                }).SetName("UnitCount = 0, MaintenanceFreeUnits = 0 | Maintenance = 0").Returns(0);

                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 5, MaintenanceFreeUnits = 0
                }).SetName("UnitCount = 5, MaintenanceFreeUnits = 0 | Maintenance = 25").Returns(25f);

                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 10, MaintenanceFreeUnits = 0
                }).SetName("UnitCount = 10, MaintenanceFreeUnits = 0 | Maintenance = 100").Returns(100f);

                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 10, MaintenanceFreeUnits = 5
                }).SetName("UnitCount = 10, MaintenanceFreeUnits = 5 | Maintenance = 25").Returns(25f);

                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 10, MaintenanceFreeUnits = 10
                }).SetName("UnitCount = 10, MaintenanceFreeUnits = 10 | Maintenance = 0").Returns(0f);

                yield return new TestCaseData(new GetMaintenanceOfUnitsForCivTestData() {
                    UnitCount = 10, MaintenanceFreeUnits = 20
                }).SetName("UnitCount = 10, MaintenanceFreeUnits = 20 | Maintenance = 0").Returns(0f);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IFreeUnitsLogic>                               MockFreeUnitsLogic;
        private Mock<ICivModifiers>                                 MockCivModifiers;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;        
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;

        private Mock<ICivModifier<bool>> MockSuppressGarrisionMaintenanceModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFreeUnitsLogic      = new Mock<IFreeUnitsLogic>();
            MockCivModifiers        = new Mock<ICivModifiers>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();

            MockSuppressGarrisionMaintenanceModifier = new Mock<ICivModifier<bool>>();

            MockCivModifiers.Setup(modifiers => modifiers.SuppressGarrisonMaintenance)
                            .Returns(MockSuppressGarrisionMaintenanceModifier.Object);

            Container.Bind<IFreeUnitsLogic>                              ().FromInstance(MockFreeUnitsLogic     .Object);
            Container.Bind<ICivModifiers>                                ().FromInstance(MockCivModifiers       .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);

            Container.Bind<UnitMaintenanceLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetMaintenanceOfUnitsForCivTestCases")]
        public float GetMaintenanceOfUnitsForCivTests(GetMaintenanceOfUnitsForCivTestData testData) {
            var civ = BuildCiv();

            MockFreeUnitsLogic.Setup(logic => logic.GetMaintenanceFreeUnitsForCiv(civ))
                              .Returns(testData.MaintenanceFreeUnits);

            var units = new List<IUnit>();
            for(int i = 0; i < testData.UnitCount; i++) {
                units.Add(BuildUnit());
            }

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(civ)).Returns(units);

            var maintenanceLogic = Container.Resolve<UnitMaintenanceLogic>();

            return maintenanceLogic.GetMaintenanceOfUnitsForCiv(civ);
        }

        [Test]
        public void GetMaintenanceOfUnitsForCiv_IgnoresCities() {
            var civ = BuildCiv();

            var units = new List<IUnit>() {
                BuildUnit(UnitType.City),
                BuildUnit(UnitType.City),
                BuildUnit(UnitType.City)
            };            

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(civ)).Returns(units);

            var maintenanceLogic = Container.Resolve<UnitMaintenanceLogic>();

            Assert.AreEqual(0, maintenanceLogic.GetMaintenanceOfUnitsForCiv(civ));
        }

        [Test]
        public void GetMaintenanceOfUnitsForCiv_AndGarrisionMaintenanceBeingSuppressed_IgnoresUnitsOnCities() {
            var cellWithCity    = BuildCell(BuildCity());
            var cellWithoutCity = BuildCell();

            var units = new List<IUnit>() {
                BuildUnit(UnitType.Melee, cellWithCity),
                BuildUnit(UnitType.Melee, cellWithoutCity),
                BuildUnit(UnitType.Melee, cellWithoutCity),
            };

            var civ = BuildCiv(units);

            MockSuppressGarrisionMaintenanceModifier.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(true);

            var maintenanceLogic = Container.Resolve<UnitMaintenanceLogic>();

            Assert.AreEqual(4, maintenanceLogic.GetMaintenanceOfUnitsForCiv(civ));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ICivilization BuildCiv(List<IUnit> units) {
            var newCiv = new Mock<ICivilization>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(units);

            return newCiv;
        }

        private IUnit BuildUnit(UnitType type = UnitType.Melee) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private IHexCell BuildCell(params ICity[] cities) {
            var newCell = new Mock<IHexCell>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(cities);

            return newCell;
        }

        private IUnit BuildUnit(UnitType type, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
