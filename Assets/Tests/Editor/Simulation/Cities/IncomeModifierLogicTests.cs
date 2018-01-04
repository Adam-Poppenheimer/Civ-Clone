using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class IncomeModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBuildingPossessionCanon> MockBuildingPossession;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossession;
        private Mock<IImprovementLocationCanon> MockImprovementPositionCanon;

        private Mock<IHexGrid> MockGrid;

        private List<IHexCell> AllTiles = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTiles.Clear();

            MockBuildingPossession       = new Mock<IBuildingPossessionCanon>();
            MockCityPossession           = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockImprovementPositionCanon = new Mock<IImprovementLocationCanon>();
            MockGrid                      = new Mock<IHexGrid>();

            MockGrid.Setup(map => map.Tiles).Returns(AllTiles.AsReadOnly());

            Container.Bind<IBuildingPossessionCanon>                     ().FromInstance(MockBuildingPossession      .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossession          .Object);
            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementPositionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                    .Object);

            Container.Bind<IncomeModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetRealBaseYieldForSlot should return the slot's base yield " +
            "as a default value, if no other modifiers apply")]
        public void GetRealBaseYieldForSlot_ReturnsBaseYieldAsDefault() {
            var slot = BuildSlot(new ResourceSummary(food: 1, production: 2));

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(slot.BaseYield, modifierLogic.GetRealBaseYieldForSlot(slot),
                "GetRealBaseYieldForSlot returned an unexpected value");
        }

        [Test(Description = "When GetRealBaseYieldForSlot is called, it should determine " +
            "whether its slot belongs to some tile. If it does, it should search for any " +
            "completed improvements on that tile and add their bonus yield to GetRealBaseYieldForSlot's " +
            "return value")]
        public void GetRealBaseYieldForSlot_ConsidersImprovementsOnTileSlots() {
            var untiledSlot = BuildSlot(new ResourceSummary(food: 1, production: 2, gold: 3));
            var tiledSlot   = BuildSlot(new ResourceSummary(food: 1, production: 2, gold: 3));

            var tile = BuildTile(tiledSlot);

            var improvement = BuildImprovement(tile, new ResourceSummary(food: 2, production: 1), true);

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(untiledSlot.BaseYield, modifierLogic.GetRealBaseYieldForSlot(untiledSlot),
                "GetRealBaseYieldForSlot returned an unexpected value for untiledSlot");

            Assert.AreEqual(
                tiledSlot.BaseYield + improvement.Template.BonusYield,
                modifierLogic.GetRealBaseYieldForSlot(tiledSlot),
                "GetRealBaseYieldForSlot returned an unexpected value for tiledSlot"
            );
        }

        [Test(Description = "GetRealBaseYieldForSlot should not consider bonus yield from improvements " +
            "that are not complete")]
        public void GetRealBaseYieldForSlot_IgnoresIncompleteImprovements() {
            var tile = BuildTile(BuildSlot(ResourceSummary.Empty));

            var improvement = BuildImprovement(tile, new ResourceSummary(food: 4), false);

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(ResourceSummary.Empty, modifierLogic.GetRealBaseYieldForSlot(tile.WorkerSlot),
                "GetRealBaseYieldForSlot returned an unexpected value");
        }

        [Test(Description = "When GetYieldMultipliersForCivilization is called, " +
            "it should return ResourceSummary.Empty as a default value")]
        public void GetYieldMultipliersForCivilization_StartsAtEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var civilization = BuildCivilization();

            Assert.AreEqual(ResourceSummary.Empty, modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value");
        }

        [Test(Description = "When GetYieldMultipliersForCivilization is called, " +
            "it should add to its base the CivilizationYieldModifiers for all buildings " +
            "in cities belonging to that civilization")]
        public void GetYieldMultipliersForCivilization_ConsidersBuildings() {
            var civilization = BuildCivilization(
                BuildCity(
                    BuildBuilding(new ResourceSummary(food: 1),       new ResourceSummary(food: -1)),
                    BuildBuilding(new ResourceSummary(gold: 2),       ResourceSummary.Empty),
                    BuildBuilding(new ResourceSummary(production: 3), ResourceSummary.Empty)
                ),
                BuildCity(
                    BuildBuilding(new ResourceSummary(gold: 4),       ResourceSummary.Empty),
                    BuildBuilding(new ResourceSummary(production: 5), ResourceSummary.Empty),
                    BuildBuilding(new ResourceSummary(culture: 6),    new ResourceSummary(culture: -6))
                )
            );

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();
            Assert.AreEqual(
                new ResourceSummary(food: 1, gold: 6, production: 8, culture: 6),
                modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value"
            );
        }

        [Test(Description = "When GetYieldMultipliersForCity is called, " + 
            "it should return ResourceSummary.Empty as a default value")]
        public void GetYieldMultipliersForCity_StartsAtEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var city = BuildCity();

            Assert.AreEqual(ResourceSummary.Empty, modifierLogic.GetYieldMultipliersForCity(city),
                "GetYieldMultipliersForCity returned an unexpected value");
        }

        [Test(Description = "When GetYieldMultipliersForCity is called, " +
            "it should add to its base the CityYieldModifiers for all buildings " +
            "in that city")]
        public void GetYieldMultipliersForCity_ConsidersBuildings() {
            var city = BuildCity(
                BuildBuilding(new ResourceSummary(food:    1), new ResourceSummary(food:    -1)),
                BuildBuilding(new ResourceSummary(gold:    2), new ResourceSummary(gold:    -2)),
                BuildBuilding(new ResourceSummary(culture: 3), new ResourceSummary(culture: -3))
            );

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(
                new ResourceSummary(food: -1, gold: -2, culture: -3),
                modifierLogic.GetYieldMultipliersForCity(city),
                "GetYieldMultipliersForCity returned an unexpected value"
            );
        }

        [Test(Description = "When GetYieldMultipliersForSlot is called, " +
            "it should always return the default value of ResourceSummary.Empty")]
        public void GetYieldMultipliersForSlot_AlwaysEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var slot = new Mock<IWorkerSlot>().Object;

            Assert.AreEqual(ResourceSummary.Empty, modifierLogic.GetYieldMultipliersForSlot(slot),
                "GetYieldMultipliersForSlot returned an unexpected value");
        }

        [Test(Description = "All methods should throw ArgumentNullExceptions when passed null arguments")]
        public void AllMethods_ThrowOnNullArguments() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetRealBaseYieldForSlot(null),
                "GetRealBaseYieldForSlot failed to throw on a null argument");

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForCivilization(null),
                "GetYieldMultipliersForCivilization failed to throw on a null argument");

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForCity(null),
                "GetYieldMultipliersForCity failed to throw on a null argument");

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForSlot(null),
                "GetYieldMultipliersForSlot failed to throw on a null argument");
        }

        #endregion

        #region utilities

        private IBuilding BuildBuilding(ResourceSummary civilizationModifier, ResourceSummary cityModifier) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();
            mockTemplate.Setup(template => template.CivilizationYieldModifier).Returns(civilizationModifier);
            mockTemplate.Setup(template => template.CityYieldModifier        ).Returns(cityModifier);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var city = new Mock<ICity>().Object;

            MockBuildingPossession
                .Setup(canon => canon.GetBuildingsInCity(city))
                .Returns(buildings.ToList().AsReadOnly());

            return city;
        }

        private ICivilization BuildCivilization(params ICity[] cities) {
            var civilization = new Mock<ICivilization>().Object;

            MockCityPossession
                .Setup(canon => canon.GetPossessionsOfOwner(civilization))
                .Returns(cities);

            return civilization;
        }

        private IImprovement BuildImprovement(IHexCell location, ResourceSummary bonusYield, bool isComplete) {
            var mockImprovment = new Mock<IImprovement>();

            var mockTemplate = new Mock<IImprovementTemplate>();
            mockTemplate.Setup(template => template.BonusYield).Returns(bonusYield);

            mockImprovment.Setup(improvement => improvement.Template  ).Returns(mockTemplate.Object);
            mockImprovment.Setup(improvement => improvement.IsComplete).Returns(isComplete);

            MockImprovementPositionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { mockImprovment.Object });

            return mockImprovment.Object;
        }

        private IWorkerSlot BuildSlot(ResourceSummary baseYield) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.BaseYield).Returns(baseYield);

            return mockSlot.Object;
        }

        private IHexCell BuildTile(IWorkerSlot slot) {
            var mockTile = new Mock<IHexCell>();

            mockTile.Setup(tile => tile.WorkerSlot).Returns(slot);
            AllTiles.Add(mockTile.Object);

            return mockTile.Object;
        }

        #endregion

        #endregion

    }

}
