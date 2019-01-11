using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianUnitBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<IBarbarianGoalBrain> GoalBrains = new List<IBarbarianGoalBrain>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            GoalBrains.Clear();

            Container.Bind<List<IBarbarianGoalBrain>>().FromInstance(GoalBrains);

            Container.Bind<BarbarianUnitBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCommandsForUnit_ReturnsCommandsOfHighestUtilityGoalBrain() {
            var unit = BuildUnit();
            var wanderMaps = new BarbarianInfluenceMaps();

            var commandListOne   = new List<IUnitCommand>();
            var commandListTwo   = new List<IUnitCommand>();
            var commandListThree = new List<IUnitCommand>();
            var commandListFour  = new List<IUnitCommand>();
            var commandListFive  = new List<IUnitCommand>();

            BuildGoalBrain(0f,    commandListOne);
            BuildGoalBrain(0.5f,  commandListTwo);
            BuildGoalBrain(0.3f,  commandListThree);
            BuildGoalBrain(0.9f,  commandListFour);
            BuildGoalBrain(0.75f, commandListFive);

            var unitBrain = Container.Resolve<BarbarianUnitBrain>();

            Assert.AreEqual(commandListFour, unitBrain.GetCommandsForUnit(unit, wanderMaps));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private IBarbarianGoalBrain BuildGoalBrain(float utility, List<IUnitCommand> commandsReturned) {
            var mockBrain = new Mock<IBarbarianGoalBrain>();

            mockBrain.Setup(
                brain => brain.GetUtilityForUnit(It.IsAny<IUnit>(), It.IsAny<BarbarianInfluenceMaps>())
            ).Returns(utility);

            mockBrain.Setup(
                brain => brain.GetCommandsForUnit(It.IsAny<IUnit>(), It.IsAny<BarbarianInfluenceMaps>())
            ).Returns(commandsReturned);

            var newBrain = mockBrain.Object;

            GoalBrains.Add(newBrain);

            return newBrain;
        }

        #endregion

        #endregion

    }

}
