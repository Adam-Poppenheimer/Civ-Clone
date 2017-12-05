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

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class IncomeModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBuildingPossessionCanon> MockBuildingPossession;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossession;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossession = new Mock<IBuildingPossessionCanon>();
            MockCityPossession     = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<IBuildingPossessionCanon>()                     .FromInstance(MockBuildingPossession.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossession.Object);

            Container.Bind<IncomeModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When GetYieldMultipliersForCivilization is called, " +
            "it should return ResourceSummary.Ones as a default value")]
        public void GetYieldMultipliersForCivilization_StartsAtOne() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var civilization = BuildCivilization();

            Assert.AreEqual(ResourceSummary.Ones, modifierLogic.GetYieldMultipliersForCivilization(civilization),
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
                new ResourceSummary(food: 1, gold: 6, production: 8, culture: 6) + ResourceSummary.Ones,
                modifierLogic.GetYieldMultipliersForCivilization(civilization),
                "GetYieldMultipliersForCivilization returned an unexpected value"
            );
        }

        [Test(Description = "When GetYieldMultipliersForCity is called, " + 
            "it should return ResourceSummary.Ones as a default value")]
        public void GetYieldMultipliersForCity_StartsAtOne() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var city = BuildCity();

            Assert.AreEqual(ResourceSummary.Ones, modifierLogic.GetYieldMultipliersForCity(city),
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
                new ResourceSummary(food: -1, gold: -2, culture: -3) + ResourceSummary.Ones,
                modifierLogic.GetYieldMultipliersForCity(city),
                "GetYieldMultipliersForCity returned an unexpected value"
            );
        }

        [Test(Description = "When GetYieldMultipliersForSlot is called, " +
            "it should always return the default value of ResourceSummary.Ones")]
        public void GetYieldMultipliersForSlot_AlwaysOne() {
            var modifierLogic = Container.Resolve<IncomeModifierLogic>();

            var slot = new Mock<IWorkerSlot>().Object;

            Assert.AreEqual(ResourceSummary.Ones, modifierLogic.GetYieldMultipliersForSlot(slot),
                "GetYieldMultipliersForSlot returned an unexpected value");
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

        #endregion

        #endregion

    }

}
