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

namespace Assets.Tests.Cities {

    [TestFixture]
    public class ProductionLogicTests : ZenjectUnitTestFixture {

        private Mock<IResourceGenerationLogic> GenerationLogicMock;
        private Mock<IProductionLogicConfig>   ConfigMock;

        [SetUp]
        public void CommonInstall() {
            GenerationLogicMock = new Mock<IResourceGenerationLogic>();
            ConfigMock          = new Mock<IProductionLogicConfig>();

            Container.Bind<IResourceGenerationLogic>().FromInstance(GenerationLogicMock.Object);
            Container.Bind<IProductionLogicConfig>()  .FromInstance(ConfigMock         .Object);

            Container.Bind<ProductionLogic>().AsSingle();
        }

        [Test(Description = "When GetProductionProgressPerTurnOnProject is called, it returns the expected " +
            "production yield of the city as determined by ResourceGenerationLogic")]
        public void GetProductionProgressPerTurnOnProject_ExactlyEqualsProduction() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;
            var project = new Mock<IProductionProject>().Object;

            var totalYield = new ResourceSummary(food: 5, gold: 12, production: 13);
            GenerationLogicMock.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(totalYield);

            Assert.AreEqual(totalYield[ResourceType.Production], productionLogic.GetProductionProgressPerTurnOnProject(city, project),
                "GetProductionProgressPerTurnOnProject returned an unexpected value");
        }

        [Test(Description = "When GetGoldCostToHurryProject is called, it returns the production left to " +
            "complete the project times Config.HurryGoldPerProduction, rounded down")]
        public void GetGoldCostToHurryProject_DeterminedByProjectAndConfig() {
            var productionLogic = Container.Resolve<ProductionLogic>();

            var city = new Mock<ICity>().Object;

            var projectMock = new Mock<IProductionProject>();
            projectMock.SetupGet(project => project.ProductionToComplete).Returns(100);
            projectMock.SetupGet(project => project.Progress).Returns(27);

            ConfigMock.SetupGet(config => config.HurryCostPerProduction).Returns(3.15f);

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

    }

}
