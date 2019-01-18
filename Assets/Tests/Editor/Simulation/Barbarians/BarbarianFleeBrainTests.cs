using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianFleeBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>     MockUnitPositionCanon;
        private Mock<IUnitStrengthEstimator> MockUnitStrengthEstimator;
        private Mock<IBarbarianConfig>       MockBarbarianConfig;
        private Mock<IHexPathfinder>         MockHexPathfinder;
        private Mock<IHexGrid>               MockGrid;
        private Mock<IBarbarianBrainTools>   MockBrainTools;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockUnitPositionCanon     = new Mock<IUnitPositionCanon>();
            MockUnitStrengthEstimator = new Mock<IUnitStrengthEstimator>();
            MockBarbarianConfig       = new Mock<IBarbarianConfig>();
            MockHexPathfinder         = new Mock<IHexPathfinder>();
            MockGrid                  = new Mock<IHexGrid>();
            MockBrainTools            = new Mock<IBarbarianBrainTools>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IUnitPositionCanon>    ().FromInstance(MockUnitPositionCanon    .Object);
            Container.Bind<IUnitStrengthEstimator>().FromInstance(MockUnitStrengthEstimator.Object);
            Container.Bind<IBarbarianConfig>      ().FromInstance(MockBarbarianConfig      .Object);
            Container.Bind<IHexPathfinder>        ().FromInstance(MockHexPathfinder        .Object);
            Container.Bind<IHexGrid>              ().FromInstance(MockGrid                 .Object);
            Container.Bind<IBarbarianBrainTools>  ().FromInstance(MockBrainTools           .Object);

            Container.Bind<BarbarianFleeBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_FollowsLogisticsCurve_WithSigmoidAtUnitStrength_AndConfiguredSlope() {
            var unit = BuildUnit(20.2f, 0f, BuildCell());

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 0f, 0f },
                EnemyPresence = new float[] { 0f, 0f, 0f },
            };

            MockBarbarianConfig.Setup(config => config.FleeUtilityLogisticSlope).Returns(2f);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            Assert.AreEqual(AIMath.NormalizedLogisticCurve(20.2f, 2f, 0f), fleeBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_LogisticXIncreasedByEnemyPresence() {
            BuildCell();

            var unit = BuildUnit(20.2f, 0f, BuildCell());

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 0f, 0f },
                EnemyPresence = new float[] { 0f, 20f, 0f },
            };

            MockBarbarianConfig.Setup(config => config.FleeUtilityLogisticSlope).Returns(2f);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            Assert.AreEqual(AIMath.NormalizedLogisticCurve(20.2f, 2f, 20f), fleeBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_LogisticXDecreasedByAllyPresence() {
            BuildCell();

            var unit = BuildUnit(20.2f, 0f, BuildCell());

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 1f, 0f },
                EnemyPresence = new float[] { 0f, 20f, 0f },
            };

            MockBarbarianConfig.Setup(config => config.FleeUtilityLogisticSlope).Returns(2f);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            Assert.AreEqual(AIMath.NormalizedLogisticCurve(20.2f, 2f, 19f), fleeBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetCommandsForUnit_AndValidLocationsExist_ReturnsASingleMoveUnitCommand() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(20.2f, 3.5f, unitLocation);

            var maps = new InfluenceMaps();

            var reachableCellsDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { BuildCell(), 2f }, { BuildCell(), 3f },
            };

            Func<IHexCell, IHexCell, float> pathfindingCostFunction = (current, next) => 0;

            MockUnitPositionCanon.Setup(canon => canon.GetPathfindingCostFunction(unit, false)).Returns(pathfindingCostFunction);

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(unitLocation, 3.5f, pathfindingCostFunction, AllCells)
            ).Returns(reachableCellsDict);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, It.IsAny<IHexCell>(), false)).Returns(true);

            MockBrainTools.Setup(tools => tools.GetFleeWeightFunction(unit, maps)).Returns(cell => reachableCellsDict[cell]);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            var commands = fleeBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(1, commands.Count, "Unexpected number of commands");

            Assert.IsTrue(commands[0] is MoveUnitCommand, "Command is not a MoveUnitCommand as expected");
        }

        [Test]
        public void GetCommandsForUnit_AndValidLocationsExist_MoveCommandGivenCorrectUnitToMove() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(20.2f, 3.5f, unitLocation);

            var maps = new InfluenceMaps();

            var reachableCellsDict = new Dictionary<IHexCell, float>() {
                { BuildCell(), 1f }, { BuildCell(), 2f }, { BuildCell(), 3f },
            };

            Func<IHexCell, IHexCell, float> pathfindingCostFunction = (current, next) => 0;

            MockUnitPositionCanon.Setup(canon => canon.GetPathfindingCostFunction(unit, false)).Returns(pathfindingCostFunction);

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(unitLocation, 3.5f, pathfindingCostFunction, AllCells)
            ).Returns(reachableCellsDict);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, It.IsAny<IHexCell>(), false)).Returns(true);

            MockBrainTools.Setup(tools => tools.GetFleeWeightFunction(unit, maps)).Returns(cell => reachableCellsDict[cell]);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            var moveCommand = fleeBrain.GetCommandsForUnit(unit, maps)[0] as MoveUnitCommand;

            Assert.AreEqual(unit, moveCommand.UnitToMove);
        }

        [Test]
        public void GetCommandsForUnit_AndValidLocationsExist_SelectsBestCandidateFromFleeWeightFunction() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(20.2f, 3.5f, unitLocation);

            var maps = new InfluenceMaps();

            IHexCell cellOne = BuildCell(), cellTwo = BuildCell(), cellThree = BuildCell();

            var reachableCellsDict = new Dictionary<IHexCell, float>() {
                { cellOne, 1f }, { cellTwo, 4f }, { cellThree, 3f },
            };

            Func<IHexCell, IHexCell, float> pathfindingCostFunction = (current, next) => 0;

            MockUnitPositionCanon.Setup(canon => canon.GetPathfindingCostFunction(unit, false)).Returns(pathfindingCostFunction);

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(unitLocation, 3.5f, pathfindingCostFunction, AllCells)
            ).Returns(reachableCellsDict);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, It.IsAny<IHexCell>(), false)).Returns(true);

            MockBrainTools.Setup(tools => tools.GetFleeWeightFunction(unit, maps)).Returns(cell => reachableCellsDict[cell]);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            var moveCommand = fleeBrain.GetCommandsForUnit(unit, maps)[0] as MoveUnitCommand;

            Assert.AreEqual(cellTwo, moveCommand.DesiredLocation);
        }

        [Test]
        public void GetCommandsForUnit_ExcludesLocationsWhereUnitCannotMove() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(20.2f, 3.5f, unitLocation);

            var maps = new InfluenceMaps();

            IHexCell cellOne = BuildCell(), cellTwo = BuildCell(), cellThree = BuildCell();

            var reachableCellsDict = new Dictionary<IHexCell, float>() {
                { cellOne, 1f }, { cellTwo, 4f }, { cellThree, 3f },
            };

            Func<IHexCell, IHexCell, float> pathfindingCostFunction = (current, next) => 0;

            MockUnitPositionCanon.Setup(canon => canon.GetPathfindingCostFunction(unit, false)).Returns(pathfindingCostFunction);

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(unitLocation, 3.5f, pathfindingCostFunction, AllCells)
            ).Returns(reachableCellsDict);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellOne,   false)).Returns(true);
            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellTwo,   false)).Returns(false);
            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellThree, false)).Returns(true);

            MockBrainTools.Setup(tools => tools.GetFleeWeightFunction(unit, maps)).Returns(cell => reachableCellsDict[cell]);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            var moveCommand = fleeBrain.GetCommandsForUnit(unit, maps)[0] as MoveUnitCommand;

            Assert.AreEqual(cellThree, moveCommand.DesiredLocation);
        }

        [Test]
        public void GetCommandsForUnit_AndNoValidLocations_ReturnsEmptyList() {
            var unitLocation = BuildCell();

            var unit = BuildUnit(20.2f, 3.5f, unitLocation);

            var maps = new InfluenceMaps();

            var reachableCellsDict = new Dictionary<IHexCell, float>();

            Func<IHexCell, IHexCell, float> pathfindingCostFunction = (current, next) => 0;

            MockUnitPositionCanon.Setup(canon => canon.GetPathfindingCostFunction(unit, false)).Returns(pathfindingCostFunction);

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetAllCellsReachableIn(unitLocation, 3.5f, pathfindingCostFunction, AllCells)
            ).Returns(reachableCellsDict);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, It.IsAny<IHexCell>(), false)).Returns(true);

            MockBrainTools.Setup(tools => tools.GetFleeWeightFunction(unit, maps)).Returns(cell => reachableCellsDict[cell]);

            var fleeBrain = Container.Resolve<BarbarianFleeBrain>();

            CollectionAssert.IsEmpty(fleeBrain.GetCommandsForUnit(unit, maps));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(AllCells.Count);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(float estimatedStrength, float currentMovement, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CurrentMovement).Returns(currentMovement);

            var newUnit = mockUnit.Object;

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitDefensiveStrength(newUnit, location))
                                     .Returns(estimatedStrength);

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
