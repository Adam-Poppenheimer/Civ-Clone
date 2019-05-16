using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Improvements {

    public class ImprovementConstructionExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>        MockUnitPositionCanon;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;
        private Mock<IImprovementWorkLogic>     MockImprovementWorkLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockImprovementWorkLogic     = new Mock<IImprovementWorkLogic>();

            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IImprovementWorkLogic>    ().FromInstance(MockImprovementWorkLogic    .Object);

            Container.Bind<ImprovementConstructionExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformConstruction_UnitLockedIntoConstruction_AndHasMovement_WorksOnFirstImprovement() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(true, 1f);
            var improvementOne = BuildImprovement(false, 4f, false, out mockImprovement);
            var improvementTwo = BuildImprovement(false, 3f, false, out mockImprovement);

            BuildCell(unit, improvementOne, improvementTwo);

            MockImprovementWorkLogic.Setup(logic => logic.GetWorkOfUnitOnImprovement(unit, improvementOne)).Returns(10f);
            MockImprovementWorkLogic.Setup(logic => logic.GetWorkOfUnitOnImprovement(unit, improvementTwo)).Returns(11f);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(14f, improvementOne.WorkInvested, "Unexpected WorkInvested on improvementOne");
            Assert.AreEqual(3f,  improvementTwo.WorkInvested, "Unexpected WorkInvested on improvementTwo");
        }

        [Test]
        public void PerformConstruction_AndUnitWorksOnImprovement_LosesMovement() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(true, 6f);
            var improvement = BuildImprovement(false, 4f, false, out mockImprovement);

            BuildCell(unit, improvement);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(0f, unit.CurrentMovement);
        }

        [Test]
        public void PerformConstruction_AndUnitDoesNotWorkOnImprovement_DoesNotLoseMovement() {
            var unit = BuildUnit(true, 6f);

            BuildCell(unit);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(6f, unit.CurrentMovement);
        }

        [Test]
        public void PerformConstruction_AndUnitWorksOnImprovement_ConstructsImprovementIfReady() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(true, 6f);

            BuildCell(unit, BuildImprovement(false, 4f, true, out mockImprovement));

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            mockImprovement.Verify(improvement => improvement.Construct(), Times.Once);
        }

        [Test]
        public void PerformConstruction_DoesntWorkOnImprovement_IfUnitHasNoMovement() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(true, 0f);
            var improvementOne = BuildImprovement(false, 4f, false, out mockImprovement);

            BuildCell(unit, improvementOne);

            MockImprovementWorkLogic.Setup(logic => logic.GetWorkOfUnitOnImprovement(unit, improvementOne)).Returns(10f);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(4f, improvementOne.WorkInvested, "Unexpected WorkInvested on improvementOne");
        }

        [Test]
        public void PerformConstruction_DoesntWorkOnImprovement_IfUnitNotLockedIntoConstruction() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(false, 5f);
            var improvementOne = BuildImprovement(false, 4f, false, out mockImprovement);

            BuildCell(unit, improvementOne);

            MockImprovementWorkLogic.Setup(logic => logic.GetWorkOfUnitOnImprovement(unit, improvementOne)).Returns(10f);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(4f, improvementOne.WorkInvested, "Unexpected WorkInvested on improvementOne");
        }

        [Test]
        public void PerformConstruction_DoesntWorkOnImprovement_IfFirstImprovementConstructed() {
            Mock<IImprovement> mockImprovement;

            var unit = BuildUnit(true, 5f);
            var improvementOne = BuildImprovement(true, 4f, false, out mockImprovement);

            BuildCell(unit, improvementOne);

            MockImprovementWorkLogic.Setup(logic => logic.GetWorkOfUnitOnImprovement(unit, improvementOne)).Returns(10f);

            var constructionExecuter = Container.Resolve<ImprovementConstructionExecuter>();

            constructionExecuter.PerformImprovementConstruction(unit);

            Assert.AreEqual(4f, improvementOne.WorkInvested, "Unexpected WorkInvested on improvementOne");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(bool lockedIntoConstruction, float currentMovement) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.LockedIntoConstruction).Returns(lockedIntoConstruction);

            var newUnit = mockUnit.Object;

            newUnit.CurrentMovement = currentMovement;

            return newUnit;
        }

        private IImprovement BuildImprovement(
            bool isConstructed, float workInvested, bool isReadyToConstruct, out Mock<IImprovement> mockImprovement
        ) {
            mockImprovement = new Mock<IImprovement>();

            mockImprovement.SetupAllProperties();

            mockImprovement.Setup(improvement => improvement.IsConstructed     ).Returns(isConstructed);
            mockImprovement.Setup(improvement => improvement.IsReadyToConstruct).Returns(isReadyToConstruct);

            var newImprovement = mockImprovement.Object;

            newImprovement.WorkInvested = workInvested;

            return newImprovement;
        }

        private IHexCell BuildCell(IUnit unit, params IImprovement[] improvements) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCell);

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(improvements);

            return newCell;
        }

        #endregion

        #endregion

    }

}
