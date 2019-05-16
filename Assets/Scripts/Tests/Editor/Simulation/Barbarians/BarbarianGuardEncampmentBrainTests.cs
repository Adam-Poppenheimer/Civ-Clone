using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Barbarians;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianGuardEncampmentBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>       MockUnitPositionCanon;
        private Mock<IHexGrid>                 MockGrid;
        private Mock<IBarbarianConfig>         MockBarbarianConfig;
        private Mock<IEncampmentLocationCanon> MockEncampmentLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon       = new Mock<IUnitPositionCanon>();
            MockGrid                    = new Mock<IHexGrid>();
            MockBarbarianConfig         = new Mock<IBarbarianConfig>();
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();

            Container.Bind<IUnitPositionCanon>      ().FromInstance(MockUnitPositionCanon      .Object);
            Container.Bind<IHexGrid>                ().FromInstance(MockGrid                   .Object);
            Container.Bind<IBarbarianConfig>        ().FromInstance(MockBarbarianConfig        .Object);
            Container.Bind<IEncampmentLocationCanon>().FromInstance(MockEncampmentLocationCanon.Object);

            Container.Bind<IHexPathfinder>().FromInstance(new Mock<IHexPathfinder>().Object);

            Container.Bind<BarbarianGuardEncampmentBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_AndUnitOnEncampment_ReturnsStayInEncampmentUtility() {
            var unit       = BuildUnit();
            var encampment = BuildEncampment();

            BuildCell(unit, encampment);

            MockBarbarianConfig.Setup(config => config.StayInEncampmentUtility)     .Returns(4.3f);
            MockBarbarianConfig.Setup(config => config.HeadTowardsEncampmentUtility).Returns(1.5f);
            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius)      .Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();
            
            Assert.AreEqual(4.3f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitNearUnoccupiedEncampment_ReturnsHeadTowardsEncampmentUtility() {
            var unit       = BuildUnit();
            var encampment = BuildEncampment();

            var unitLocation = BuildCell(unit, null);
            var encampmentLocation = BuildCell(null, encampment);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2)).Returns(new List<IHexCell>() { encampmentLocation });

            MockBarbarianConfig.Setup(config => config.StayInEncampmentUtility)     .Returns(4.3f);
            MockBarbarianConfig.Setup(config => config.HeadTowardsEncampmentUtility).Returns(1.5f);
            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius)      .Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();
            
            Assert.AreEqual(1.5f, brain.GetUtilityForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitNearOccupiedEncampment_ReturnsZero() {
            var unitToTest = BuildUnit();
            var otherUnit  = BuildUnit();
            var encampment = BuildEncampment();

            var unitLocation = BuildCell(unitToTest, null);
            var encampmentLocation = BuildCell(otherUnit, encampment);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2)).Returns(new List<IHexCell>() { encampmentLocation });

            MockBarbarianConfig.Setup(config => config.StayInEncampmentUtility)     .Returns(4.3f);
            MockBarbarianConfig.Setup(config => config.HeadTowardsEncampmentUtility).Returns(1.5f);
            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius)      .Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();
            
            Assert.AreEqual(0f, brain.GetUtilityForUnit(unitToTest, new InfluenceMaps()));
        }

        [Test]
        public void GetUtilityForUnit_AndNoNearbyEncampments_ReturnsZero() {
            var unitToTest = BuildUnit();

            var unitLocation = BuildCell(unitToTest, null);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2)).Returns(new List<IHexCell>());

            MockBarbarianConfig.Setup(config => config.StayInEncampmentUtility)     .Returns(4.3f);
            MockBarbarianConfig.Setup(config => config.HeadTowardsEncampmentUtility).Returns(1.5f);
            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius)      .Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();
            
            Assert.AreEqual(0f, brain.GetUtilityForUnit(unitToTest, new InfluenceMaps()));
        }

        [Test]
        public void GetCommandsForUnit_AndUnitOnEncampment_ReturnsEmptyList() {
            var unit       = BuildUnit();
            var encampment = BuildEncampment();

            BuildCell(unit, encampment);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();

            CollectionAssert.IsEmpty(brain.GetCommandsForUnit(unit, new InfluenceMaps()));
        }

        [Test]
        public void GetCommandsForUnit_AndNearbyUnoccupiedEncampments_ReturnsMovementCommandToFirstOne() {
            var unit          = BuildUnit();
            var encampmentOne = BuildEncampment();
            var encampmentTwo = BuildEncampment();

            var unitLocation    = BuildCell(unit, null);
            var nearbyCellOne   = BuildCell(null, null);
            var nearbyCellTwo   = BuildCell(null, encampmentOne);            
            var nearbyCellThree = BuildCell(null, encampmentTwo);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2))
                    .Returns(new List<IHexCell>() { nearbyCellOne, nearbyCellTwo, nearbyCellThree });

            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius).Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();

            var commands = brain.GetCommandsForUnit(unit, new InfluenceMaps());

            Assert.AreEqual(1, commands.Count, "Unexpected number of commands");

            var moveCommand = commands[0] as MoveUnitCommand;

            Assert.IsNotNull(moveCommand, "Command is not of type MoveUnitCommand");

            Assert.AreEqual(unit,          moveCommand.UnitToMove,      "MoveUnitCommand has an unexpected UnitToMove");
            Assert.AreEqual(nearbyCellTwo, moveCommand.DesiredLocation, "MoveUnitCommand has an unexpected DesiredLocation");
        }

        [Test]
        public void GetCommandsForUnit_AndNearbyOccupiedEncampments_ReturnsEmptyList() {
            var unitToTest = BuildUnit();
            var otherUnit  = BuildUnit();
            var encampment = BuildEncampment();

            var unitLocation = BuildCell(unitToTest, null);
            var nearbyCell   = BuildCell(otherUnit,  encampment);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2)).Returns(new List<IHexCell>() { nearbyCell });

            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius).Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();

            CollectionAssert.IsEmpty(brain.GetCommandsForUnit(unitToTest, new InfluenceMaps()));
        }

        [Test]
        public void GetCommandsForUnit_NoNearbyEncampments_ReturnsEmptyList() {
            var unitToTest = BuildUnit();

            var unitLocation = BuildCell(unitToTest, null);
            var nearbyCell   = BuildCell(null, null);

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 2)).Returns(new List<IHexCell>() { nearbyCell });

            MockBarbarianConfig.Setup(config => config.DefendEncampmentRadius).Returns(2);

            var brain = Container.Resolve<BarbarianGuardEncampmentBrain>();

            CollectionAssert.IsEmpty(brain.GetCommandsForUnit(unitToTest, new InfluenceMaps()));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private IEncampment BuildEncampment() {
            return new Mock<IEncampment>().Object;
        }

        private IHexCell BuildCell(IUnit unit, IEncampment encampment) {
            var newCell = new Mock<IHexCell>().Object;

            if(unit != null) {
                MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCell);
                MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(new List<IUnit>() { unit });
            }else {
                MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(new List<IUnit>());
            }

            if(encampment != null) {
                MockEncampmentLocationCanon.Setup(canon => canon.GetOwnerOfPossession(encampment)).Returns(newCell);
                MockEncampmentLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(new List<IEncampment>() { encampment });
            }else {
                MockEncampmentLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(new List<IEncampment>());
            }
            
            return newCell;
        }

        #endregion

        #endregion

    }

}
