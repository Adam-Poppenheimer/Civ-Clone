using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianWanderBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                         MockGrid;
        private Mock<IUnitPositionCanon>               MockUnitPositionCanon;
        private Mock<IWeightedRandomSampler<IHexCell>> MockCellRandomSampler;
        private Mock<IBarbarianBrainTools>             MockBrainTools;
        private Mock<IBarbarianConfig>                 MockBarbarianConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid              = new Mock<IHexGrid>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCellRandomSampler = new Mock<IWeightedRandomSampler<IHexCell>>();
            MockBrainTools        = new Mock<IBarbarianBrainTools>();
            MockBarbarianConfig   = new Mock<IBarbarianConfig>();

            Container.Bind<IHexGrid>                        ().FromInstance(MockGrid             .Object);
            Container.Bind<IUnitPositionCanon>              ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IWeightedRandomSampler<IHexCell>>().FromInstance(MockCellRandomSampler.Object);
            Container.Bind<IBarbarianBrainTools>            ().FromInstance(MockBrainTools       .Object);
            Container.Bind<IBarbarianConfig>                ().FromInstance(MockBarbarianConfig  .Object);

            Container.Bind<IHexPathfinder>().FromMock();

            Container.Bind<BarbarianWanderBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUtilityForUnit_AndUnitCivilian_ReturnsZero() {
            var unit = BuildUnit(UnitType.Civilian);

            var maps = new BarbarianInfluenceMaps();

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            Assert.AreEqual(0f, wanderBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetUtilityForUnit_AndUnitNotCivilian_ReturnsConfiguredWanderGoalUtility() {
            var unit = BuildUnit(UnitType.NavalMelee);

            var maps = new BarbarianInfluenceMaps();

            MockBarbarianConfig.Setup(config => config.WanderGoalUtility).Returns(0.5f);

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            Assert.AreEqual(0.5f, wanderBrain.GetUtilityForUnit(unit, maps));
        }

        [Test]
        public void GetWanderCommandsForUnit_AndSomeValidCandidateExists_ReturnsListWithOneMoveUnitCommand() {
            var unitLocation  = BuildCell();
            var bestCandidate = BuildCell();

            var nearbyCells = new List<IHexCell>();

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            var unit = BuildUnit(unitLocation, 3);

            var maps = new BarbarianInfluenceMaps();

            Func<IHexCell, int> wanderWeightFunction = cell => 1;

            MockBrainTools.Setup(tools => tools.GetWanderWeightFunction(unit, maps)).Returns(wanderWeightFunction);

            MockCellRandomSampler.Setup(
                sampler => sampler.SampleElementsFromSet(nearbyCells, 1, wanderWeightFunction)
            ).Returns(new List<IHexCell>() { bestCandidate });

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            var commandList = wanderBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(1, commandList.Count, "Unexpected number of commands returned");
            Assert.IsTrue(commandList[0] is MoveUnitCommand, "Command not of type MoveUnitCommand");
        }

        [Test]
        public void GetWanderCommandsForUnit_AndSomeValidCandidateExists_ReturnedCommandHasCorrectUnitToMove() {
            var unitLocation  = BuildCell();
            var bestCandidate = BuildCell();

            var nearbyCells = new List<IHexCell>();

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            var unit = BuildUnit(unitLocation, 3);

            var maps = new BarbarianInfluenceMaps();

            Func<IHexCell, int> wanderWeightFunction = cell => 1;

            MockBrainTools.Setup(tools => tools.GetWanderWeightFunction(unit, maps)).Returns(wanderWeightFunction);

            MockCellRandomSampler.Setup(
                sampler => sampler.SampleElementsFromSet(nearbyCells, 1, wanderWeightFunction)
            ).Returns(new List<IHexCell>() { bestCandidate });

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            var commandList = wanderBrain.GetCommandsForUnit(unit, maps);

            var moveCommand = commandList[0] as MoveUnitCommand;

            Assert.AreEqual(unit, moveCommand.UnitToMove);
        }

        [Test]
        public void GetWanderCommandsForUnit_AndSomeValidCandidateExists_ReturnedCommandHasCorrectDesiredLocation() {
            var unitLocation  = BuildCell();
            var bestCandidate = BuildCell();

            var nearbyCells = new List<IHexCell>();

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            var unit = BuildUnit(unitLocation, 3);

            var maps = new BarbarianInfluenceMaps();

            Func<IHexCell, int> wanderWeightFunction = cell => 1;

            MockBrainTools.Setup(tools => tools.GetWanderWeightFunction(unit, maps)).Returns(wanderWeightFunction);

            MockCellRandomSampler.Setup(
                sampler => sampler.SampleElementsFromSet(nearbyCells, 1, wanderWeightFunction)
            ).Returns(new List<IHexCell>() { bestCandidate });

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            var commandList = wanderBrain.GetCommandsForUnit(unit, maps);

            var moveCommand = commandList[0] as MoveUnitCommand;

            Assert.AreEqual(bestCandidate, moveCommand.DesiredLocation);
        }

        [Test]
        public void GetWanderCommandsForUnit_AndNoValidCandidateExists_ReturnsEmptySet() {
            var unitLocation  = BuildCell();

            var nearbyCells = new List<IHexCell>();

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 3)).Returns(nearbyCells);

            var unit = BuildUnit(unitLocation, 3);

            var maps = new BarbarianInfluenceMaps();

            Func<IHexCell, int> wanderWeightFunction = cell => 1;

            MockBrainTools.Setup(tools => tools.GetWanderWeightFunction(unit, maps)).Returns(wanderWeightFunction);

            MockCellRandomSampler.Setup(
                sampler => sampler.SampleElementsFromSet(nearbyCells, 1, wanderWeightFunction)
            ).Returns(new List<IHexCell>());

            var wanderBrain = Container.Resolve<BarbarianWanderBrain>();

            var commandList = wanderBrain.GetCommandsForUnit(unit, maps);

            Assert.AreEqual(0, commandList.Count);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit(IHexCell location, int maxMovement) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.MaxMovement).Returns(maxMovement);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IUnit BuildUnit(UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
