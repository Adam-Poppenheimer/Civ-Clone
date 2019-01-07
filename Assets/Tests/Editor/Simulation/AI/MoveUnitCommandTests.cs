using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.AI {

    public class MoveUnitCommandTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexPathfinder>     MockHexPathfinder;
        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockHexPathfinder     = new Mock<IHexPathfinder>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IHexPathfinder>    ().FromInstance(MockHexPathfinder    .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<MoveUnitCommand>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void StartExecution_AndPathToDesiredLocation_UnitPathSetToThatPath_AndThenPerformsMovement() {
            var unitLocation    = BuildCell();
            var desiredLocation = BuildCell();

            var path = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    unitLocation, desiredLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns(path);

            Mock<IUnit> mockUnitToMove;
            var unitToMove = BuildUnit(unitLocation, out mockUnitToMove);

            var executionSequence = new MockSequence();

            mockUnitToMove.InSequence(executionSequence).SetupSet(unit => unit.CurrentPath = path);
            mockUnitToMove.InSequence(executionSequence).Setup   (unit => unit.PerformMovement(false, It.IsAny<Action>()));

            var moveUnitCommand = Container.Resolve<MoveUnitCommand>();

            moveUnitCommand.UnitToMove      = unitToMove;
            moveUnitCommand.DesiredLocation = desiredLocation;

            moveUnitCommand.StartExecution();

            mockUnitToMove.VerifyAll();
        }

        [Test]
        public void StartExecution_AndPathToDesiredLocation_StatusSetToRunning() {
            var unitLocation    = BuildCell();
            var desiredLocation = BuildCell();

            var path = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    unitLocation, desiredLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns(path);

            var unitToMove = BuildUnit(unitLocation);

            var moveUnitCommand = Container.Resolve<MoveUnitCommand>();

            moveUnitCommand.UnitToMove      = unitToMove;
            moveUnitCommand.DesiredLocation = desiredLocation;

            moveUnitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Running, moveUnitCommand.Status);
        }

        [Test]
        public void StartExecution_AndNoPathToDesiredLocation_StatusSetToFailed() {
            var unitLocation    = BuildCell();
            var desiredLocation = BuildCell();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    unitLocation, desiredLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns((List<IHexCell>)null);

            var unitToMove = BuildUnit(unitLocation);

            var moveUnitCommand = Container.Resolve<MoveUnitCommand>();

            moveUnitCommand.UnitToMove      = unitToMove;
            moveUnitCommand.DesiredLocation = desiredLocation;

            moveUnitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, moveUnitCommand.Status);
        }

        [Test]
        public void UnitFinishesMovement_AndUnitAtDesiredLocation_StatusSetToSucceeded() {
            var unitLocation    = BuildCell();
            var desiredLocation = BuildCell();

            var path = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    unitLocation, desiredLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns(path);

            Mock<IUnit> mockUnitToMove;
            var unitToMove = BuildUnit(unitLocation, out mockUnitToMove);

            mockUnitToMove.Setup(
                unit => unit.PerformMovement(false, It.IsAny<Action>())

            ).Callback(delegate(bool ignoreMoveCosts, Action action) {
                MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unitToMove)).Returns(desiredLocation);
                action();
            });

            var moveUnitCommand = Container.Resolve<MoveUnitCommand>();

            moveUnitCommand.UnitToMove      = unitToMove;
            moveUnitCommand.DesiredLocation = desiredLocation;

            moveUnitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, moveUnitCommand.Status);
        }

        [Test]
        public void UnitFinishesMovement_AndUnitNotAtDesiredLocation_StatusSetToFailed() {
            var unitLocation    = BuildCell();
            var desiredLocation = BuildCell();

            var path = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    unitLocation, desiredLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns(path);

            Mock<IUnit> mockUnitToMove;
            var unitToMove = BuildUnit(unitLocation, out mockUnitToMove);

            mockUnitToMove
                .Setup(unit => unit.PerformMovement(false, It.IsAny<Action>()))
                .Callback(delegate(bool ignoreMoveCosts, Action action) { action(); });

            var moveUnitCommand = Container.Resolve<MoveUnitCommand>();

            moveUnitCommand.UnitToMove      = unitToMove;
            moveUnitCommand.DesiredLocation = desiredLocation;

            moveUnitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, moveUnitCommand.Status);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit(IHexCell location) {
            Mock<IUnit> mock;
            return BuildUnit(location, out mock);
        }

        private IUnit BuildUnit(IHexCell location, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            var newUnit = mock.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
