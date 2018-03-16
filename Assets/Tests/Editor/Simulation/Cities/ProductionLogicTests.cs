using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ProductionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceGenerationLogic>                  MockGenerationLogic;
        private Mock<ICityConfig>                               MockConfig;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGenerationLogic         = new Mock<IResourceGenerationLogic>();
            MockConfig                  = new Mock<ICityConfig>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IResourceGenerationLogic>                 ().FromInstance(MockGenerationLogic        .Object);
            Container.Bind<ICityConfig>                              ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<ProductionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When GetProductionProgressPerTurnOnProject is called, it returns the expected " +
            "production yield of the city as determined by ResourceGenerationLogic")]
        public void GetProductionProgressPerTurnOnProject_ExactlyEqualsProduction() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;
            var project = new Mock<IProductionProject>().Object;

            var totalYield = new ResourceSummary(food: 5, gold: 12, production: 13);
            MockGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city, It.IsAny<ResourceSummary>())).Returns(totalYield);

            Assert.AreEqual(totalYield[ResourceType.Production], productionLogic.GetProductionProgressPerTurnOnProject(city, project),
                "GetProductionProgressPerTurnOnProject returned an unexpected value");
        }

        [Test(Description = "")]
        public void GetProductionProgressPerTurnOnProject_ChangedByBuildingProductionBonus() {
            var landTemplateProject    = BuildProject(BuildUnitTemplate(UnitType.Archery));
            var mountedTemplateProject = BuildProject(BuildUnitTemplate(UnitType.Mounted));

            var buildingInCity = BuildBuilding(
                BuildBuildingTemplate(mountedUnitProductionBonus: 1f, landUnitProductionBonus: 2f)
            );

            var city = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city))
                .Returns(new List<IBuilding>() { buildingInCity });

            var totalYield = new ResourceSummary(production: 10f);
            MockGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city, It.IsAny<ResourceSummary>())).Returns(totalYield);

            var productionLogic = Container.Resolve<ProductionLogic>();

            var expectedAdditionalModifiersFromLandProject    = new ResourceSummary(production: 2f);
            var expectedAdditionalModifiersFromMountedProject = new ResourceSummary(production: 3f);

            productionLogic.GetProductionProgressPerTurnOnProject(city, landTemplateProject);            

            MockGenerationLogic.Verify(logic => logic.GetTotalYieldForCity(city, expectedAdditionalModifiersFromLandProject),
                Times.AtLeastOnce, "GetTotalYieldForCity was passed unexpected arguments for the land template project");

            productionLogic.GetProductionProgressPerTurnOnProject(city, mountedTemplateProject);

            MockGenerationLogic.Verify(logic => logic.GetTotalYieldForCity(city, expectedAdditionalModifiersFromMountedProject),
                Times.AtLeastOnce, "GetTotalYieldForCity was passed unexpected arguments for the mounted template project");
        }

        [Test(Description = "When GetGoldCostToHurryProject is called, it returns the production left to " +
            "complete the project times Config.HurryGoldPerProduction, rounded down")]
        public void GetGoldCostToHurryProject_DeterminedByProjectAndConfig() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;

            var projectMock = new Mock<IProductionProject>();
            projectMock.SetupGet(project => project.ProductionToComplete).Returns(100);
            projectMock.SetupGet(project => project.Progress).Returns(27);

            MockConfig.SetupGet(config => config.HurryCostPerProduction).Returns(3.15f);

            Assert.AreEqual(229, productionLogic.GetGoldCostToHurryProject(city, projectMock.Object), 
                "GetGoldCostToHurryProject returned an unexpected value");
        }

        [Test(Description = "All methods should throw NullArgumentExceptions whenever they are passed any null argument")]
        public void AllMethods_ThrowOnNullArguments() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;
            var project = new Mock<IProductionProject>().Object;

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetGoldCostToHurryProject(null, project),
                "GetGoldCostToHurryProject fails to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetGoldCostToHurryProject(city, null),
                "GetGoldCostToHurryProject fails to throw on a null project argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetProductionProgressPerTurnOnProject(null, project),
                "GetProductionProgressPerTurnOnProject fails to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => productionLogic.GetProductionProgressPerTurnOnProject(city, null),
                "GetProductionProgressPerTurnOnProject fails to throw on a null project argument");
        }

        #endregion

        #region utilities

        private IProductionProject BuildProject(IUnitTemplate unitToConstruct) {
            var mockProject = new Mock<IProductionProject>();

            mockProject.Setup(project => project.UnitToConstruct).Returns(unitToConstruct);

            return mockProject.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private IBuildingTemplate BuildBuildingTemplate(
            float mountedUnitProductionBonus, float landUnitProductionBonus
        ){
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.MountedUnitProductionBonus).Returns(mountedUnitProductionBonus);
            mockTemplate.Setup(template => template.LandUnitProductionBonus   ).Returns(landUnitProductionBonus);

            return mockTemplate.Object;
        }

        private IUnitTemplate BuildUnitTemplate(UnitType type) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);

            return mockTemplate.Object;
        }

        #endregion

        #endregion

    }

}
