using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class BuildImprovementOngoingAbilityTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);
        }

        #endregion

        #region tests

        [Test(Description = "When BeginExecution is called, SourceUnit should have its " +
            "CurrentMovement set to zero")]
        public void BeginExecution_MovementSetToZero() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.BeginExecution();

            Assert.AreEqual(0, unit.CurrentMovement);
        }

        [Test(Description = "When BeginExecution is called, ImprovementToConstruct should " +
            "have its WorkInvested increased by one")]
        public void BeginExecution_WorkInvestedIncremented() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.BeginExecution();

            Assert.AreEqual(1, improvement.WorkInvested);
        }

        [Test(Description = "When TickExecution is called, and ImprovementToConstruct " +
            "is not complete, SourceUnit should have its CurrentMovement set to zero")]
        public void TickExecution_AndImprovementNotComplete_MovementSetToZero() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.TickExecution();

            Assert.AreEqual(0, unit.CurrentMovement);
        }

        [Test(Description = "When TickExecution is called, and ImprovementToConstruct " +
            "is not complete, ImprovementToConstruct should have its WorkInvested increased by one")]
        public void TickExecution_AndImprovementNotComplete_WorkInvestedIncremented() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.TickExecution();

            Assert.AreEqual(1, improvement.WorkInvested);
        }

        [Test(Description = "When TickExecution is called, it should do nothing if " +
            "SourceUnit's current movement is zero")]
        public void TickExecution_DoesNothingIfCurrentMovementZero() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 0);
            var improvement = BuildImprovement(tile, false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.TickExecution();

            Assert.AreEqual(0, improvement.WorkInvested);
        }

        [Test(Description = "When TickExecution is called, it should do nothing if " +
            "IsReadyToTerminate would return true")]
        public void TickExecution_DoesNothingIfReadyToTerminate() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, true);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            ongoingAbility.TickExecution();

            Assert.AreEqual(2, unit.CurrentMovement);
        }

        [Test(Description = "IsReadyToTerminate should return true if ImprovementToConstruct.IsComplete is true")]
        public void IsReadyToTerminate_TrueIfImprovementComplete() {
            var tile = BuildTile();

            var unit = BuildUnit(tile, 2);
            var improvement = BuildImprovement(tile, true);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            Assert.IsTrue(ongoingAbility.IsReadyToTerminate());
        }

        [Test(Description = "IsReadyToTerminate should return true if SourceUnit " +
            "is not at ImprovementToConstruct's location")]
        public void IsReadyToTerminate_TrueIfUnitNotOnTile() {
            var unit = BuildUnit(BuildTile(), 2);
            var improvement = BuildImprovement(BuildTile(), false);

            var ongoingAbility = Container.Instantiate<BuildImprovementOngoingAbility>(new object[] { unit, improvement });

            Assert.IsTrue(ongoingAbility.IsReadyToTerminate());
        }

        #endregion

        #region utilities

        private IHexCell BuildTile() {
            return new Mock<IHexCell>().Object;
        }

        private IImprovement BuildImprovement(IHexCell location, bool isComplete) {
            var mockImprovement = new Mock<IImprovement>();
            var newImprovement = mockImprovement.Object;

            mockImprovement.SetupAllProperties();
            mockImprovement.Setup(improvement => improvement.IsComplete).Returns(isComplete);

            MockImprovementLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newImprovement)).Returns(location);

            return newImprovement;
        }

        private IUnit BuildUnit(IHexCell location, int currentMovement) {
            var mockUnit = new Mock<IUnit>();
            var newUnit = mockUnit.Object;

            mockUnit.SetupAllProperties();

            newUnit.CurrentMovement = currentMovement;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
