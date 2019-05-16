using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    public class UnitGarrisonLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<IUnitPositionCanon>                       MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IUnitPositionCanon>                      ().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<UnitGarrisonLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsUnitGarrisoned_TrueIfAnyCityAtUnitLocation() {
            var unit = BuildUnit(UnitType.Melee);

            BuildCell(
                new List<IUnit>() { unit },
                new List<ICity>() { BuildCity(), BuildCity() }
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsTrue(garrisonLogic.IsUnitGarrisoned(unit));
        }

        [Test]
        public void IsUnitGarrisoned_FalseIfNoCityAtUnitLocation() {
            var unit = BuildUnit(UnitType.Melee);

            BuildCell(
                new List<IUnit>() { unit },
                new List<ICity>()
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsFalse(garrisonLogic.IsUnitGarrisoned(unit));
        }

        [Test]
        public void IsCityGarrisoned_TrueIfAnyMilitaryUnitsAtCityLocation() {
            var city = BuildCity();

            BuildCell(
                new List<IUnit>() { BuildUnit(UnitType.Melee), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.City) },
                new List<ICity>() { city }
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsTrue(garrisonLogic.IsCityGarrisoned(city));
        }

        [Test]
        public void IsCityGarrisoned_FalseIfOnlyCiviliansAtCityLocation() {
            var city = BuildCity();

            BuildCell(
                new List<IUnit>() { BuildUnit(UnitType.Civilian) },
                new List<ICity>() { city }
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsFalse(garrisonLogic.IsCityGarrisoned(city));
        }

        [Test]
        public void IsCityGarrisoned_FalseIfOnlyCityUnitsAtCityLocation() {
            var city = BuildCity();

            BuildCell(
                new List<IUnit>() { BuildUnit(UnitType.City) },
                new List<ICity>() { city }
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsFalse(garrisonLogic.IsCityGarrisoned(city));
        }

        [Test]
        public void IsCityGarrisoned_FalseIfNoUnitsAtCityLocation() {
            var city = BuildCity();

            BuildCell(
                new List<IUnit>(),
                new List<ICity>() { city }
            );

            var garrisonLogic = Container.Resolve<UnitGarrisonLogic>();

            Assert.IsFalse(garrisonLogic.IsCityGarrisoned(city));
        }

        #endregion

        #region utilities

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private IUnit BuildUnit(UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        private IHexCell BuildCell(List<IUnit> units, List<ICity> cities) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(cities);

            foreach(var unit in units) {
                MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCell);
            }

            foreach(var city in cities) {
                MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCell);
            }

            return newCell;
        }

        #endregion

        #endregion

    }

}
