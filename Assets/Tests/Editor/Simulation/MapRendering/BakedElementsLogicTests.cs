using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class BakedElementsLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IRiverCanon>                 MockRiverCanon;
        private Mock<IImprovementLocationCanon>   MockImprovementLocationCanon;
        private Mock<ICivilizationTerritoryLogic> MockCivTerritoryLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverCanon               = new Mock<IRiverCanon>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockCivTerritoryLogic        = new Mock<ICivilizationTerritoryLogic>();

            Container.Bind<IRiverCanon>                ().FromInstance(MockRiverCanon              .Object);
            Container.Bind<IImprovementLocationCanon>  ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<ICivilizationTerritoryLogic>().FromInstance(MockCivTerritoryLogic       .Object);

            Container.Bind<BakedElementsLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetBakedElementsInCells_DefaultsToNone() {
            var cells = new List<IHexCell>();

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            Assert.AreEqual(BakedElementFlags.None, elementsLogic.GetBakedElementsInCells(cells));
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellHasOasis_FlagsSimpleLand_AndSimpleWater() {
            var cells = new List<IHexCell>() {
                BuildCell(feature: CellFeature.Oasis),
                BuildCell(feature: CellFeature.Oasis)
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.SimpleLand ) == BakedElementFlags.SimpleLand,  "SimpleLand not flagged");
            Assert.IsTrue((elements & BakedElementFlags.SimpleWater) == BakedElementFlags.SimpleWater, "SimpleWater not flagged");

            Assert.AreEqual(elements, BakedElementFlags.SimpleLand | BakedElementFlags.SimpleWater, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellHasRivers_FlagsRiverbanks() {
            var cells = new List<IHexCell>() {
                BuildCell(hasRiver: true),
                BuildCell(),
                BuildCell(hasRiver: true),
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.Riverbanks) == BakedElementFlags.Riverbanks, "Riverbanks not flagged");

            Assert.AreEqual(elements, BakedElementFlags.Riverbanks, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellHasRoads_FlagsRoads() {
            var cells = new List<IHexCell>() {
                BuildCell(hasRoads: true),
                BuildCell(),
                BuildCell(hasRoads: true),
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.Roads) == BakedElementFlags.Roads, "Roads not flagged");

            Assert.AreEqual(elements, BakedElementFlags.Roads, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellHasFarmProducingImprovements_FlagsFarmland() {
            var producingImprovement    = BuildImprovement(BuildImprovementTemplate(true));
            var nonProducingImprovement = BuildImprovement(BuildImprovementTemplate(false));

            var cells = new List<IHexCell>() {
                BuildCell(improvements: new List<IImprovement>() { producingImprovement, nonProducingImprovement }),
                BuildCell(),
                BuildCell(improvements: new List<IImprovement>() { nonProducingImprovement, nonProducingImprovement })
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.Farmland) == BakedElementFlags.Farmland, "Farmland not flagged");

            Assert.AreEqual(elements, BakedElementFlags.Farmland, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellClaimedByACiv_FlagsCulture() {
            var cells = new List<IHexCell>() {
                BuildCell(),
                BuildCell(civClaiming: BuildCiv()),
                BuildCell(civClaiming: BuildCiv()),
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.Culture) == BakedElementFlags.Culture, "Culture not flagged");

            Assert.AreEqual(elements, BakedElementFlags.Culture, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_AndAnyCellHasMarsh_FlagsSimpleWater() {
            var cells = new List<IHexCell>() {
                BuildCell(vegetation: CellVegetation.Marsh),
                BuildCell(),
                BuildCell(vegetation: CellVegetation.Marsh),
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.IsTrue((elements & BakedElementFlags.SimpleWater) == BakedElementFlags.SimpleWater, "SimpleWater not flagged");

            Assert.AreEqual(elements, BakedElementFlags.SimpleWater, "Unexpected flags present");
        }

        [Test]
        public void GetBakedElementsInCells_MultipleContributionsStack() {
            var cells = new List<IHexCell>() {
                BuildCell(feature: CellFeature.Oasis),
                BuildCell(hasRiver: true, hasRoads: true),
                BuildCell(vegetation: CellVegetation.Marsh),
                BuildCell(hasRoads: true)
            };

            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            var elements = elementsLogic.GetBakedElementsInCells(cells);

            Assert.AreEqual(
                BakedElementFlags.SimpleLand | BakedElementFlags.SimpleWater | BakedElementFlags.Roads | BakedElementFlags.Riverbanks,
                elements, "Unexpected flags"
            );
        }

        [Test]
        public void GetBakedElementsInCells_ThrowsIfCellsNull() {
            var elementsLogic = Container.Resolve<BakedElementsLogic>();

            Assert.Throws<ArgumentNullException>(() => elementsLogic.GetBakedElementsInCells(null));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IImprovementTemplate BuildImprovementTemplate(bool producesFarmland) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.ProducesFarmland).Returns(producesFarmland);

            return mockTemplate.Object;
        }

        private IImprovement BuildImprovement(IImprovementTemplate template) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(template);

            return mockImprovement.Object;
        }

        private IHexCell BuildCell(
            CellFeature feature = CellFeature.None, CellVegetation vegetation = CellVegetation.None,
            bool hasRiver = false, bool hasRoads = false,
            IEnumerable<IImprovement> improvements = null, ICivilization civClaiming = null            
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Feature)   .Returns(feature);
            mockCell.Setup(cell => cell.Vegetation).Returns(vegetation);
            mockCell.Setup(cell => cell.HasRoads)  .Returns(hasRoads);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(hasRiver);

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(
                improvements != null ? improvements : new List<IImprovement>()
            );

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell)).Returns(civClaiming);

            return newCell;
        }

        #endregion

        #endregion

    }

}
