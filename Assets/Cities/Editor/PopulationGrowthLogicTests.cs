using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

namespace Assets.Cities.Editor {

    [TestFixture]
    public class PopulationGrowthLogicTests : ZenjectUnitTestFixture {

        private Mock<IPopulationGrowthConfig> ConfigMock;

        [SetUp]
        public void CommonInstall() {
            ConfigMock = new Mock<IPopulationGrowthConfig>();

            Container.Bind<IPopulationGrowthConfig>().FromInstance(ConfigMock.Object);

            Container.Bind<PopulationGrowthLogic>().AsSingle();
        }

        [Test(Description = "GetFoodConsumptionPerTurn should return the city's population times " + 
            "the value specified in GrowthLogicConfig")]
        public void GetFoodConsumptionPerTurn_ConfigInformsFoodPerPerson() {
            var mockCityOne = new Mock<ICity>();
            var mockCityTwo = new Mock<ICity>();
            var mockCityThree = new Mock<ICity>();

            mockCityOne.SetupAllProperties();
            mockCityTwo.SetupAllProperties();
            mockCityThree.SetupAllProperties();

            mockCityOne.Object.Population = 1;
            mockCityTwo.Object.Population = 5;
            mockCityThree.Object.Population = 0;

            ConfigMock.SetupGet(config => config.FoodConsumptionPerPerson).Returns(2);

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(2, growthLogic.GetFoodConsumptionPerTurn(mockCityOne.Object),
                "GetFoodConsumptionPerTurn on CityOne returned an incorrect value");

            Assert.AreEqual(10, growthLogic.GetFoodConsumptionPerTurn(mockCityTwo.Object),
                "GetFoodConsumptionPerTurn on CityTwo returned an incorrect value");

            Assert.AreEqual(0, growthLogic.GetFoodConsumptionPerTurn(mockCityThree.Object),
                "GetFoodConsumptionPerTurn on CityThree returned an incorrect value");
        }

        [Test(Description = "GetFoodStockpileSubtractionAfterGrowth should return the food required for " +
            "the city to grow from its current size")]
        public void GetFoodStockpileSubtractionAfterGrowth_ReturnsFoodToGrow() {
            var mockCity = new Mock<ICity>();
            mockCity.SetupAllProperties();
            mockCity.Object.Population = 5;

            ConfigMock.SetupGet(config => config.BaseStockpile).Returns(15);
            ConfigMock.SetupGet(config => config.PreviousPopulationCoefficient).Returns(6);
            ConfigMock.SetupGet(config => config.PreviousPopulationExponent).Returns(1.8f);

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(51, growthLogic.GetFoodStockpileSubtractionAfterGrowth(mockCity.Object), 
                "GetFoodStockpileSubtractionAfterGrowth on a population of 5 returned an incorrect value");

            mockCity.Object.Population = 10;
            Assert.AreEqual(121, growthLogic.GetFoodStockpileSubtractionAfterGrowth(mockCity.Object),
                "GetFoodStockpileSubtractionAfterGrowth on a population of 10 returned an incorrect value");
        }

        [Test(Description = "GetFoodStockpileAfterStarvation should always return zero, regardless " +
            "of the city's population")]
        public void GetFoodStockpileAfterStarvation_ReturnsZero() {
            var mockCityOne = new Mock<ICity>();
            var mockCityTwo = new Mock<ICity>();
            var mockCityThree = new Mock<ICity>();

            mockCityOne.SetupAllProperties();
            mockCityTwo.SetupAllProperties();
            mockCityThree.SetupAllProperties();

            mockCityOne.Object.Population = 5;
            mockCityTwo.Object.Population = 10;
            mockCityThree.Object.Population = 21;

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(mockCityOne.Object),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityOne");

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(mockCityTwo.Object),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityTwo");

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(mockCityThree.Object),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityThree");
        }

        [Test(Description = "GetFoodStockpileToGrow should respond to the module's config and " + 
            "operate under the following equation, where n is the current population: \n\n" + 
            "\tBaseStockpile + PreviousPopulationCoefficient * (n - 1) + (n - 1)^PreviousPopulationExponent\n\n" +
            "The returned value should be rounded down")]
        public void GetFoodStockpileToGrow_FollowsEquationAndConfig() {
            var mockCity = new Mock<ICity>();
            mockCity.SetupAllProperties();
            mockCity.Object.Population = 5;

            ConfigMock.SetupGet(config => config.BaseStockpile).Returns(15);
            ConfigMock.SetupGet(config => config.PreviousPopulationCoefficient).Returns(6);
            ConfigMock.SetupGet(config => config.PreviousPopulationExponent).Returns(1.8f);

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(51, growthLogic.GetFoodStockpileToGrow(mockCity.Object), 
                "GetFoodStockpileToGrow on a population of 5 returned an incorrect value");

            mockCity.Object.Population = 10;
            Assert.AreEqual(121, growthLogic.GetFoodStockpileToGrow(mockCity.Object),
                "GetFoodStockpileToGrow on a population of 10 returned an incorrect value");
        }

    }

}
