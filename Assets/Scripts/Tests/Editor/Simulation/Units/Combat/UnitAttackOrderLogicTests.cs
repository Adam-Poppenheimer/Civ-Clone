using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class UnitAttackOrderLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<UnitAttackOrderLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetNextAttackTargetOnCell_AndCellEmpty_ReturnsNull() {
            var cell = BuildCell();

            var orderLogic = Container.Resolve<UnitAttackOrderLogic>();

            Assert.IsNull(orderLogic.GetNextAttackTargetOnCell(cell));
        }

        [Test]
        public void GetNextAttackTargetOnCell_ReturnsFirstCityIfOneExists() {
            var units = new IUnit[] {
                BuildUnit(UnitType.Melee),    BuildUnit(UnitType.NavalMelee), BuildUnit(UnitType.City),
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.City)
            };

            var cell = BuildCell(units);

            var orderLogic = Container.Resolve<UnitAttackOrderLogic>();

            Assert.AreEqual(units[2], orderLogic.GetNextAttackTargetOnCell(cell));
        }

        [Test]
        public void GetNextAttackTargetOnCell_ReturnsFirstNavalMilitaryUnitIfNoCities() {
            var units = new IUnit[] {
                BuildUnit(UnitType.Melee), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.NavalRanged),
                BuildUnit(UnitType.NavalMelee)
            };

            var cell = BuildCell(units);

            var orderLogic = Container.Resolve<UnitAttackOrderLogic>();

            Assert.AreEqual(units[2], orderLogic.GetNextAttackTargetOnCell(cell));
        }

        [Test]
        public void GetNextAttackTargetOnCell_ReturnsFirstLandMilitaryUnitIfNoCitiesOrNavalMilitary() {
            var units = new IUnit[] {
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Mounted), BuildUnit(UnitType.Civilian),
                BuildUnit(UnitType.Melee)
            };

            var cell = BuildCell(units);

            var orderLogic = Container.Resolve<UnitAttackOrderLogic>();

            Assert.AreEqual(units[1], orderLogic.GetNextAttackTargetOnCell(cell));
        }

        [Test]
        public void GetNextAttackTargetOnCell_ReturnsFirstCivilianIfNoCitiesOrMilitaryUnits() {
            var units = new IUnit[] {
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian)
            };

            var cell = BuildCell(units);

            var orderLogic = Container.Resolve<UnitAttackOrderLogic>();

            Assert.AreEqual(units[0], orderLogic.GetNextAttackTargetOnCell(cell));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        private IHexCell BuildCell(params IUnit[] units) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);

            return newCell;
        }

        #endregion

        #endregion

    }

}
