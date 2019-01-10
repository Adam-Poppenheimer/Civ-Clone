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

        private Mock<IBarbarianWanderBrain> MockWanderBrain;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockWanderBrain = new Mock<IBarbarianWanderBrain>();

            Container.Bind<IBarbarianWanderBrain>().FromInstance(MockWanderBrain.Object);

            Container.Bind<BarbarianUnitBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCommandsForUnit_ReturnsCommandsOfWanderBrain() {
            var unit = BuildUnit();
            var wanderMaps = new BarbarianInfluenceMaps();

            var commandList = new List<IUnitCommand>();

            MockWanderBrain.Setup(wanderBrain => wanderBrain.GetWanderCommandsForUnit(unit, wanderMaps))
                           .Returns(commandList);

            var unitBrain = Container.Resolve<BarbarianUnitBrain>();

            Assert.AreEqual(commandList, unitBrain.GetCommandsForUnit(unit, wanderMaps));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
