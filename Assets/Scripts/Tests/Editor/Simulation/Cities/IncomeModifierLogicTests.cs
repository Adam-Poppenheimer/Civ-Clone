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
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class IncomeModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossession;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossession;
        private Mock<IHexGrid>                                      MockGrid;
        private Mock<ICivilizationHappinessLogic>                   MockCivHappinessLogic;
        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<IGoldenAgeCanon>                               MockGoldenAgeCanon;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockBuildingPossession = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossession     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGrid               = new Mock<IHexGrid>();
            MockCivHappinessLogic  = new Mock<ICivilizationHappinessLogic>();
            MockCivConfig          = new Mock<ICivilizationConfig>();
            MockGoldenAgeCanon     = new Mock<IGoldenAgeCanon>();

            MockGrid.Setup(map => map.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossession.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossession    .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid              .Object);
            Container.Bind<ICivilizationHappinessLogic>                  ().FromInstance(MockCivHappinessLogic .Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig         .Object);
            Container.Bind<IGoldenAgeCanon>                              ().FromInstance(MockGoldenAgeCanon    .Object);

            Container.Bind<IncomeModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When GetYieldMultipliersForCivilization is called, " +
            "it should return ResourceSummary.Empty as a default value")]
        public void GetYieldMultipliersForCivilization_StartsAtEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var civilization = BuildCivilization();

            Assert.AreEqual(YieldSummary.Empty, modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value");
        }

        [Test(Description = "When GetYieldMultipliersForCivilization is called, " +
            "it should add to its base the CivilizationYieldModifiers for all buildings " +
            "in cities belonging to that civilization")]
        public void GetYieldMultipliersForCivilization_ConsidersBuildings() {
            var civilization = BuildCivilization(
                BuildCity(
                    BuildBuilding(new YieldSummary(food: 1),       new YieldSummary(food: -1)),
                    BuildBuilding(new YieldSummary(gold: 2),       YieldSummary.Empty),
                    BuildBuilding(new YieldSummary(production: 3), YieldSummary.Empty)
                ),
                BuildCity(
                    BuildBuilding(new YieldSummary(gold: 4),       YieldSummary.Empty),
                    BuildBuilding(new YieldSummary(production: 5), YieldSummary.Empty),
                    BuildBuilding(new YieldSummary(culture: 6),    new YieldSummary(culture: -6))
                )
            );

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();
            Assert.AreEqual(
                new YieldSummary(food: 1, gold: 6, production: 8, culture: 6),
                modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value"
            );
        }

        [Test(Description = "GetYieldMultipliersForCivilization should return lower production and gold " +
            "modifiers if the argued civilization is unhappy. This should be informed by the total unhappiness " +
            "of the civilization and CivilizationConfig.YieldLossPerUnhappiness")]
        public void GetYieldMultipliersForCivilization_ConsidersHappiness() {
            var civilization = BuildCivilization();

            MockCivHappinessLogic.Setup(logic => logic.GetNetHappinessOfCiv(civilization)).Returns(-15);

            MockCivConfig.Setup(config => config.YieldLossPerUnhappiness).Returns(0.02f);

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 0, gold: -15 * 0.02f, production: -15 * 0.02f, culture: 0, science: 0),
                modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value"
            );
        }

        [Test]
        public void GetYieldMultipliersForCivilization_IncludesGoldenAgeModifiersIfCivInGoldenAge() {
            var civ = BuildCivilization();

            MockCivConfig.Setup(config => config.GoldenAgeProductionModifiers)
                         .Returns(new YieldSummary(food: 2f, gold: 3f));

            MockGoldenAgeCanon.Setup(canon => canon.IsCivInGoldenAge(civ)).Returns(true);

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 2f, gold: 3f),
                modifierLogic.GetYieldMultipliersForCivilization(civ)
            );
        }

        [Test]
        public void GetYieldMultipliersForCivilization_IgnoresGoldenAgeModifiersIfCivNotInGoldenAge() {
            var civ = BuildCivilization();

            MockCivConfig.Setup(config => config.GoldenAgeProductionModifiers)
                         .Returns(new YieldSummary(food: 2f, gold: 3f));

            MockGoldenAgeCanon.Setup(canon => canon.IsCivInGoldenAge(civ)).Returns(false);

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(
                YieldSummary.Empty, modifierLogic.GetYieldMultipliersForCivilization(civ)
            );
        }

        [Test(Description = "When GetYieldMultipliersForCity is called, " + 
            "it should return ResourceSummary.Empty as a default value")]
        public void GetYieldMultipliersForCity_StartsAtEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var city = BuildCity();

            Assert.AreEqual(YieldSummary.Empty, modifierLogic.GetYieldMultipliersForCity(city),
                "GetYieldMultipliersForCity returned an unexpected value");
        }

        [Test(Description = "When GetYieldMultipliersForCity is called, " +
            "it should add to its base the CityYieldModifiers for all buildings " +
            "in that city")]
        public void GetYieldMultipliersForCity_ConsidersBuildings() {
            var city = BuildCity(
                BuildBuilding(new YieldSummary(food:    1), new YieldSummary(food:    -1)),
                BuildBuilding(new YieldSummary(gold:    2), new YieldSummary(gold:    -2)),
                BuildBuilding(new YieldSummary(culture: 3), new YieldSummary(culture: -3))
            );

            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.AreEqual(
                new YieldSummary(food: -1, gold: -2, culture: -3),
                modifierLogic.GetYieldMultipliersForCity(city),
                "GetYieldMultipliersForCity returned an unexpected value"
            );
        }

        [Test(Description = "When GetYieldMultipliersForSlot is called, " +
            "it should always return the default value of ResourceSummary.Empty")]
        public void GetYieldMultipliersForSlot_AlwaysEmpty() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var slot = new Mock<IWorkerSlot>().Object;

            Assert.AreEqual(YieldSummary.Empty, modifierLogic.GetYieldMultipliersForSlot(slot),
                "GetYieldMultipliersForSlot returned an unexpected value");
        }

        [Test(Description = "All methods should throw ArgumentNullExceptions when passed null arguments")]
        public void AllMethods_ThrowOnNullArguments() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForCivilization(null),
                "GetYieldMultipliersForCivilization failed to throw on a null argument");

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForCity(null),
                "GetYieldMultipliersForCity failed to throw on a null argument");

            Assert.Throws<ArgumentNullException>(() => modifierLogic.GetYieldMultipliersForSlot(null),
                "GetYieldMultipliersForSlot failed to throw on a null argument");
        }

        #endregion

        #region utilities

        private IBuilding BuildBuilding(YieldSummary civilizationModifier, YieldSummary cityModifier) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.CivilizationYieldModifier).Returns(civilizationModifier);
            mockBuilding.Setup(building => building.CityYieldModifier        ).Returns(cityModifier);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var city = new Mock<ICity>().Object;

            MockBuildingPossession
                .Setup(canon => canon.GetPossessionsOfOwner(city))
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

        private IWorkerSlot BuildSlot(YieldSummary baseYield) {
            var mockSlot = new Mock<IWorkerSlot>();

            return mockSlot.Object;
        }

        private IHexCell BuildCell(IWorkerSlot slot) {
            var mockCells = new Mock<IHexCell>();

            mockCells.Setup(tile => tile.WorkerSlot).Returns(slot);
            AllCells.Add(mockCells.Object);

            return mockCells.Object;
        }

        #endregion

        #endregion

    }

}
