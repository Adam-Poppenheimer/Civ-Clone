using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Moq;
using Zenject;

using Assets.Simulation;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Diplomacy;

namespace Assets.Tests.Simulation.Improvements {

    public class ImprovementDamageExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IImprovementLocationCanon>                       MockImprovementLocationCanon;
        private Mock<IHexGrid>                                        MockGrid;
        private Mock<IPossessionRelationship<ICivilization, IUnit>>   MockUnitPossessionCanon;
        private Mock<ICivilizationTerritoryLogic>                     MockCivTerritoryLogic;
        private Mock<IUnitFactory>                                    MockUnitFactory;
        private Mock<IUnitPositionCanon>                              MockUnitPositionCanon;
        private Mock<IWarCanon>                                       MockWarCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockWarCanon                 = new Mock<IWarCanon>();
            MockUnitPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivTerritoryLogic        = new Mock<ICivilizationTerritoryLogic>();
            MockUnitFactory              = new Mock<IUnitFactory>();
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockGrid                     = new Mock<IHexGrid>();

            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                    .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon     .Object);
            Container.Bind<ICivilizationTerritoryLogic>                  ().FromInstance(MockCivTerritoryLogic       .Object);
            Container.Bind<IUnitFactory>                                 ().FromInstance(MockUnitFactory             .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon                .Object);

            Container.Bind<ImprovementDamageExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformDamageOnUnitFromImprovements_TakesDamageMultiplerFromMostDamagingNearbyImprovement() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            MockWarCanon.Setup(canon => canon.AreAtWar(domesticCiv, foreignCiv)).Returns(true);

            var unitLocation = BuildCell(foreignCiv);

            var unitToTest = BuildUnit(domesticCiv, unitLocation, 90, 100);

            var nearbyCells = new List<IHexCell>() {
                unitLocation, BuildCell(foreignCiv), BuildCell(foreignCiv), BuildCell(foreignCiv),
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 1)).Returns(nearbyCells);

            BuildImprovement(unitLocation, 0.1f);
            BuildImprovement(nearbyCells[1], 0.2f);
            BuildImprovement(nearbyCells[2], 0.3f);
            BuildImprovement(nearbyCells[3], 0f);

            var damageExecuter = Container.Resolve<ImprovementDamageExecuter>();

            damageExecuter.PerformDamageOnUnitFromImprovements(unitToTest);

            Assert.AreEqual(60f, unitToTest.CurrentHitpoints);
        }

        [Test]
        public void PerformDamageOnUnitFromImprovements_IgnoresImprovementsOnCellsOwnedByNonBelligerentCivs() {
            var domesticCiv           = BuildCiv();
            var foreignPeacefulCiv    = BuildCiv();
            var foreignBelligerentCiv = BuildCiv();

            MockWarCanon.Setup(canon => canon.AreAtWar(domesticCiv, foreignBelligerentCiv)).Returns(true);

            var unitLocation = BuildCell(foreignBelligerentCiv);

            var unitToTest = BuildUnit(domesticCiv, unitLocation, 90, 100);

            var nearbyCells = new List<IHexCell>() {
                unitLocation,
                BuildCell(foreignBelligerentCiv),
                BuildCell(foreignPeacefulCiv),
                BuildCell(foreignPeacefulCiv),
            };

            MockGrid.Setup(grid => grid.GetCellsInRadius(unitLocation, 1)).Returns(nearbyCells);

            BuildImprovement(unitLocation, 0.1f);
            BuildImprovement(nearbyCells[1], 0.2f);
            BuildImprovement(nearbyCells[2], 0.3f);
            BuildImprovement(nearbyCells[3], 0f);

            var damageExecuter = Container.Resolve<ImprovementDamageExecuter>();

            damageExecuter.PerformDamageOnUnitFromImprovements(unitToTest);

            Assert.AreEqual(70f, unitToTest.CurrentHitpoints);
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildCell(ICivilization owner) {
            var newCell = new Mock<IHexCell>().Object;

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell)).Returns(owner);

            return newCell;
        }

        private IUnit BuildUnit(ICivilization owner, IHexCell location, int currentHitpoints, int maxHitpoints) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();
            mockUnit.Setup(unit => unit.MaxHitpoints).Returns(maxHitpoints);

            var newUnit = mockUnit.Object;

            newUnit.CurrentHitpoints = currentHitpoints;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);
            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IImprovement BuildImprovement(IHexCell location, float adjacentEnemyDamagePercentage) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.AdjacentEnemyDamagePercentage).Returns(adjacentEnemyDamagePercentage);

            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(mockTemplate.Object);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                        .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        #endregion

        #endregion

    }

}
