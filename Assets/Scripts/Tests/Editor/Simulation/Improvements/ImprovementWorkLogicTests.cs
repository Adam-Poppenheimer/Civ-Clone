using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Improvements {

    public class ImprovementWorkLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivModifiers>                                 MockCivModifiers;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        private Mock<ICivModifier<float>> MockImprovementBuildSpeedModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCivModifiers        = new Mock<ICivModifiers>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            MockImprovementBuildSpeedModifier = new Mock<ICivModifier<float>>();

            MockCivModifiers.Setup(modifiers => modifiers.ImprovementBuildSpeed)
                            .Returns(MockImprovementBuildSpeedModifier.Object);

            Container.Bind<ICivModifiers>                                ().FromInstance(MockCivModifiers       .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

            Container.Bind<ImprovementWorkLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetWorkOfUnitOnImprovement_ReturnsImprovementBuildSpeedOfUnitOwner() {
            var owner = BuildCiv(5f);

            var unit = BuildUnit(owner);

            var improvement = BuildImprovement();

            var workLogic = Container.Resolve<ImprovementWorkLogic>();

            Assert.AreEqual(5f, workLogic.GetWorkOfUnitOnImprovement(unit, improvement));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(float improvementBuildSpeed) {
            var newCiv = new Mock<ICivilization>().Object;

            MockImprovementBuildSpeedModifier.Setup(modifier => modifier.GetValueForCiv(newCiv))
                                             .Returns(improvementBuildSpeed);

            return newCiv;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IImprovement BuildImprovement() {
            return new Mock<IImprovement>().Object;
        }

        #endregion

        #endregion

    }

}
