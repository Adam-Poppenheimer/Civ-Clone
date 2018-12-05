using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class RepairAdjacentShipsAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IHexGrid>                                      MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockGrid                = new Mock<IHexGrid>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);

            Container.Bind<RepairAdjacentShipsAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleAbilityOnUnit_TrueOfSomeCommandHasTypeRepairAdjacentShips() {
            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.RepairAdjacentShips },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildRoad }
            );

            var unit = BuildUnit(BuildCiv(), 0, 0, UnitType.Mounted);

            var abilityHandler = Container.Resolve<RepairAdjacentShipsAbilityHandler>();

            Assert.IsTrue(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfNoCommandHasTypeRepairAdjacentShips() {
            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildRoad }
            );

            var unit = BuildUnit(BuildCiv(), 0, 0, UnitType.NavalMelee);

            var abilityHandler = Container.Resolve<RepairAdjacentShipsAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void TryHandleAbilityOnUnit_AndCanHandleAbility_RepairsAllNearbyMilitaryNavalUnitsWithSameOwner() {
            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.RepairAdjacentShips }
            );

            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var unitToTest = BuildUnit(domesticCiv, 50, 100, UnitType.NavalMelee);

            var otherUnitOne   = BuildUnit(domesticCiv, 50,  100, UnitType.NavalMelee);
            var otherUnitTwo   = BuildUnit(domesticCiv, 100, 100, UnitType.NavalMelee);
            var otherUnitThree = BuildUnit(domesticCiv, 50,  100, UnitType.Mounted);
            var otherUnitFour  = BuildUnit(foreignCiv,  50,  100, UnitType.NavalMelee);
            var otherUnitFive  = BuildUnit(domesticCiv, 50,  100, UnitType.NavalMelee);

            var location = BuildCell(unitToTest);

            var nearbyCell = BuildCell(otherUnitOne, otherUnitTwo, otherUnitThree, otherUnitFour);
            BuildCell(otherUnitFive);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 1))
                    .Returns(new List<IHexCell>() { location, nearbyCell });

            var abilityHandler = Container.Resolve<RepairAdjacentShipsAbilityHandler>();

            abilityHandler.TryHandleAbilityOnUnit(ability, unitToTest);

            Assert.AreEqual(100, unitToTest.CurrentHitpoints, "UnitToTest has an unexpected CurrentHitpoints");

            Assert.AreEqual(100, otherUnitOne  .CurrentHitpoints, "OtherUnitOne has an unexpected CurrentHitpoints");
            Assert.AreEqual(100, otherUnitTwo  .CurrentHitpoints, "OtherUnitTwo has an unexpected CurrentHitpoints");
            Assert.AreEqual(50,  otherUnitThree.CurrentHitpoints, "OtherUnitThree has an unexpected CurrentHitpoints");
            Assert.AreEqual(50,  otherUnitFour .CurrentHitpoints, "OtherUnitFour has an unexpected CurrentHitpoints");
        }

        [Test]
        public void TryHandleAbilityOnUnit_ReturnsCorrectResultsIfAbilityCanBeHandled() {
            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.RepairAdjacentShips },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildRoad }
            );

            var unit = BuildUnit(BuildCiv(), 0, 0, UnitType.Mounted);

            var unitLocation = BuildCell(unit);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 1)).Returns(new List<IHexCell>());

            var abilityHandler = Container.Resolve<RepairAdjacentShipsAbilityHandler>();

            Assert.AreEqual(
                new AbilityExecutionResults(true, null),
                abilityHandler.TryHandleAbilityOnUnit(ability, unit)
            );
        }

        [Test]
        public void TryHandleAbilityOnUnit_ReturnsCorrectResultsIfAbilityCannotBeHandled() {
            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildRoad }
            );

            var unit = BuildUnit(BuildCiv(), 0, 0, UnitType.NavalMelee);

            var abilityHandler = Container.Resolve<RepairAdjacentShipsAbilityHandler>();

            Assert.AreEqual(
                new AbilityExecutionResults(false, null),
                abilityHandler.TryHandleAbilityOnUnit(ability, unit)
            );
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(params AbilityCommandRequest[] commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit(ICivilization owner, int currentHipoints, int maxHitpoints, UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();
            mockUnit.Setup(unit => unit.MaxHitpoints).Returns(maxHitpoints);
            mockUnit.Setup(unit => unit.Type)        .Returns(type);

            var newUnit = mockUnit.Object;

            newUnit.CurrentHitpoints = currentHipoints;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IHexCell BuildCell(params IUnit[] units) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);

            foreach(var unit in units) {
                MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCell);
            }

            return newCell;
        }

        #endregion

        #endregion

    }

}
