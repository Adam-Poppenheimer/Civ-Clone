using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Technology;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.AI {

    public class UnitStrengthEstimatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        
        private Mock<IUnitUpgradeLogic>                 MockUnitUpgradeLogic;
        private Mock<ICivilizationFactory>              MockCivFactory;
        private Mock<IUnitPositionCanon>                MockUnitPositionCanon;
        private Mock<IUnitComparativeStrengthEstimator> MockComparativeStrengthEstimator;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockUnitUpgradeLogic             = new Mock<IUnitUpgradeLogic>();
            MockCivFactory                   = new Mock<ICivilizationFactory>();
            MockUnitPositionCanon            = new Mock<IUnitPositionCanon>();
            MockComparativeStrengthEstimator = new Mock<IUnitComparativeStrengthEstimator>();

            MockCivFactory.Setup(civ => civ.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<IUnitUpgradeLogic>                ().FromInstance(MockUnitUpgradeLogic            .Object);
            Container.Bind<ICivilizationFactory>             ().FromInstance(MockCivFactory                  .Object);
            Container.Bind<IUnitPositionCanon>               ().FromInstance(MockUnitPositionCanon           .Object);
            Container.Bind<IUnitComparativeStrengthEstimator>().FromInstance(MockComparativeStrengthEstimator.Object);

            Container.Bind<UnitStrengthEstimator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void EstimateUnitStrength_AveragesComparativeEstimates_AgainstCuttingEdgeTemplates() {
            var location = BuildCell();
            var unit     = BuildUnit(BuildUnitTemplate(), location);

            var templateOne   = BuildUnitTemplate();
            var templateTwo   = BuildUnitTemplate();
            var templateThree = BuildUnitTemplate();

            MockUnitUpgradeLogic.Setup(logic => logic.GetCuttingEdgeUnitsForCivs(It.IsAny<ReadOnlyCollection<ICivilization>>()))
                                .Returns(new List<IUnitTemplate>() { templateOne, templateTwo, templateThree });

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeStrength(unit, It.Is<IUnit>(defender => defender.Template == templateOne), location)
            ).Returns(25f);

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeStrength(unit, It.Is<IUnit>(defender => defender.Template == templateTwo), location)
            ).Returns(10f);

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeStrength(unit, It.Is<IUnit>(defender => defender.Template == templateThree), location)
            ).Returns(55f);

            var strengthEstimator = Container.Resolve<UnitStrengthEstimator>();

            Assert.AreEqual((25f + 10f + 55f) / 3, strengthEstimator.EstimateUnitStrength(unit));
        }

        [Test]
        public void EstimateUnitDefensiveStrength_AveragesComparativeEstimates_AgainstCuttingEdgeTemplates() {
            var location = BuildCell();
            var unit     = BuildUnit(BuildUnitTemplate(), null);

            var templateOne   = BuildUnitTemplate();
            var templateTwo   = BuildUnitTemplate();
            var templateThree = BuildUnitTemplate();

            MockUnitUpgradeLogic.Setup(logic => logic.GetCuttingEdgeUnitsForCivs(It.IsAny<ReadOnlyCollection<ICivilization>>()))
                                .Returns(new List<IUnitTemplate>() { templateOne, templateTwo, templateThree });

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeDefensiveStrength(
                    unit, It.Is<IUnit>(defender => defender.Template == templateOne), location
                )
            ).Returns(25f);

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeDefensiveStrength(
                    unit, It.Is<IUnit>(defender => defender.Template == templateTwo), location
                )
            ).Returns(10f);

            MockComparativeStrengthEstimator.Setup(
                estimator => estimator.EstimateComparativeDefensiveStrength(
                    unit, It.Is<IUnit>(defender => defender.Template == templateThree), location
                )
            ).Returns(55f);

            var strengthEstimator = Container.Resolve<UnitStrengthEstimator>();

            Assert.AreEqual((25f + 10f + 55f) / 3, strengthEstimator.EstimateUnitDefensiveStrength(unit, location));
        }

        [Test]
        public void EstimateUnitStrength_ThrowsOnNullUnit() {
            var strengthEstimator = Container.Resolve<UnitStrengthEstimator>();

            Assert.Throws<ArgumentNullException>(() => strengthEstimator.EstimateUnitStrength(null));
        }

        [Test]
        public void EstimateUnitDefensiveStrength_ThrowsOnNullUnit() {
            var location = BuildCell();

            var strengthEstimator = Container.Resolve<UnitStrengthEstimator>();

            Assert.Throws<ArgumentNullException>(() => strengthEstimator.EstimateUnitDefensiveStrength(null, location));
        }

        [Test]
        public void EstimateUnitDefensiveStrength_ThrowsOnNullLocation() {
            var unit = BuildUnit(BuildUnitTemplate(), BuildCell());

            var strengthEstimator = Container.Resolve<UnitStrengthEstimator>();

            Assert.Throws<ArgumentNullException>(() => strengthEstimator.EstimateUnitDefensiveStrength(unit, null));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnitTemplate BuildUnitTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        private IUnit BuildUnit(IUnitTemplate template, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Template).Returns(template);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
