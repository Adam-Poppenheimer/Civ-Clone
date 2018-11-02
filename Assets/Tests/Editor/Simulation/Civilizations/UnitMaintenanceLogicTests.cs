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
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFreeUnitsLogic      = new Mock<IFreeUnitsLogic>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            Container.Bind<IFreeUnitsLogic>                              ().FromInstance(MockFreeUnitsLogic     .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

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

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
