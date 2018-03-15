using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class PopulationGrowthLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityConfig>                                   ConfigMock;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICivilizationHappinessLogic>                   MockCivilizationHappinessLogic;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            ConfigMock                     = new Mock<ICityConfig>();
            MockCityPossessionCanon        = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCivilizationHappinessLogic = new Mock<ICivilizationHappinessLogic>();
            MockBuildingPossessionCanon    = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ICityConfig>                                  ().FromInstance(ConfigMock                    .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon       .Object);
            Container.Bind<ICivilizationHappinessLogic>                  ().FromInstance(MockCivilizationHappinessLogic.Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon   .Object);

            Container.Bind<PopulationGrowthLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetFoodConsumptionPerTurn should return the city's population times " + 
            "the value specified in GrowthLogicConfig")]
        public void GetFoodConsumptionPerTurn_ConfigInformsFoodPerPerson() {
            var cityOne   = BuildCity(1, BuildCivilization(0));
            var cityTwo   = BuildCity(5, BuildCivilization(0));
            var cityThree = BuildCity(0, BuildCivilization(0));

            ConfigMock.SetupGet(config => config.FoodConsumptionPerPerson).Returns(2);

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(2, growthLogic.GetFoodConsumptionPerTurn(cityOne),
                "GetFoodConsumptionPerTurn on CityOne returned an incorrect value");

            Assert.AreEqual(10, growthLogic.GetFoodConsumptionPerTurn(cityTwo),
                "GetFoodConsumptionPerTurn on CityTwo returned an incorrect value");

            Assert.AreEqual(0, growthLogic.GetFoodConsumptionPerTurn(cityThree),
                "GetFoodConsumptionPerTurn on CityThree returned an incorrect value");
        }

        [Test(Description = "GetFoodStockpileAfterGrowth should return a ratio of the city's " +
            "current food stockpile based on the FoodStockpilePreservationBonus properties of all " +
            "the city's buildings. This ratio should be clamped between 0 and 1")]
        public void GetFoodStockpileAfterGrowth_BasedOnStockpileAndBuildings() {
            var buildingsOne = new List<IBuilding>() {
                BuildBuilding(0.5f), BuildBuilding(0.25f)
            };

            var buildingsTwo = new List<IBuilding>() {
                BuildBuilding(1.5f), BuildBuilding(1.25f)
            };

            var buildingsThree = new List<IBuilding>() {
                BuildBuilding(0.25f), BuildBuilding(-1f)
            };

            var cityOne   = BuildCity(1, BuildCivilization(0), buildingsOne);
            var cityTwo   = BuildCity(1, BuildCivilization(0), buildingsTwo);
            var cityThree = BuildCity(1, BuildCivilization(0), buildingsThree);

            cityOne  .FoodStockpile = 100;
            cityTwo  .FoodStockpile = 200;
            cityThree.FoodStockpile = 300;

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(100 * (0.25f + 0.5f), growthLogic.GetFoodStockpileAfterGrowth(cityOne),   "Returned incorrect value for cityOne");
            Assert.AreEqual(200,                  growthLogic.GetFoodStockpileAfterGrowth(cityTwo),   "Returned incorrect value for cityTwo");
            Assert.AreEqual(0,                    growthLogic.GetFoodStockpileAfterGrowth(cityThree), "Returned incorrect value for cityThree");
        }

        [Test(Description = "GetFoodStockpileAfterStarvation should always return zero, regardless " +
            "of the city's population")]
        public void GetFoodStockpileAfterStarvation_ReturnsZero() {
            var cityOne   = BuildCity(5, BuildCivilization(0));
            var cityTwo   = BuildCity(10, BuildCivilization(0));
            var cityThree = BuildCity(21, BuildCivilization(0));

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(cityOne),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityOne");

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(cityTwo),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityTwo");

            Assert.AreEqual(0, growthLogic.GetFoodStockpileAfterStarvation(cityThree),
                "GrowthLogic's GetFoodStockpileAfterStarvation failed to return 0 on CityThree");
        }

        [Test(Description = "GetFoodStockpileToGrow should respond to the module's config and " + 
            "operate under the following equation, where n is the current population: \n\n" + 
            "\tBaseStockpile + PreviousPopulationCoefficient * (n - 1) + (n - 1)^PreviousPopulationExponent\n\n" +
            "The returned value should be rounded down")]
        public void GetFoodStockpileToGrow_FollowsEquationAndConfig() {
            var cityOne = BuildCity(5, BuildCivilization(0));
            var cityTwo = BuildCity(10, BuildCivilization(0));

            ConfigMock.SetupGet(config => config.BaseGrowthStockpile).Returns(15);
            ConfigMock.SetupGet(config => config.GrowthPreviousPopulationCoefficient).Returns(6);
            ConfigMock.SetupGet(config => config.GrowthPreviousPopulationExponent).Returns(1.8f);

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(51, growthLogic.GetFoodStockpileToGrow(cityOne), 
                "GetFoodStockpileToGrow on a population of 5 returned an incorrect value");

            Assert.AreEqual(121, growthLogic.GetFoodStockpileToGrow(cityTwo),
                "GetFoodStockpileToGrow on a population of 10 returned an incorrect value");
        }

        [Test(Description = "GetFoodStockpileAdditionFromIncome should return the argued income " +
            "when the argued city's owner has a non-negative net happiness. If its net happiness " +
            "is between -1 and -10, then it should return a quarter of the argued income. " +
            "Otherwise, it should return 0")]
        public void GetFoodStockpileAdditionFromIncome_ModifiedByOwnerHappiness() {
            var cityOne   = BuildCity(1, BuildCivilization(0));
            var cityTwo   = BuildCity(1, BuildCivilization(-7));
            var cityThree = BuildCity(1, BuildCivilization(-11));

            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.AreEqual(
                10, growthLogic.GetFoodStockpileAdditionFromIncome(cityOne, 10),
                "GetFoodStockpileAdditionFromIncome returned an unexpected value on a city whose owner has 0 happiness"
            );
            Assert.AreEqual(
                2.5, growthLogic.GetFoodStockpileAdditionFromIncome(cityTwo, 10),
                "GetFoodStockpileAdditionFromIncome returned an unexpected value on a city whose owner has -7 happiness"
            );
            Assert.AreEqual(
                0, growthLogic.GetFoodStockpileAdditionFromIncome(cityThree, 10),
                "GetFoodStockpileAdditionFromIncome returned an unexpected value on a city whose owner has -11 happiness"
            );
        }

        [Test(Description = "")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var growthLogic = Container.Resolve<PopulationGrowthLogic>();

            Assert.Throws<ArgumentNullException>(() => growthLogic.GetFoodConsumptionPerTurn(null),
                "GetFoodConsumptionPerTurn fails to throw on a null city");

            Assert.Throws<ArgumentNullException>(() => growthLogic.GetFoodStockpileToGrow(null),
                "GetFoodStockpileToGrow fails to throw on a null city");

            Assert.Throws<ArgumentNullException>(() => growthLogic.GetFoodStockpileAfterGrowth(null),
                "GetFoodStockpileSubtractionAfterGrowth fails to throw on a null city");

            Assert.Throws<ArgumentNullException>(() => growthLogic.GetFoodStockpileAfterStarvation(null),
                "GetFoodStockpileAfterStarvation fails to throw on a null city");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(int netHappiness) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCivilizationHappinessLogic.Setup(logic => logic.GetNetHappinessOfCiv(newCiv)).Returns(netHappiness);

            return newCiv;
        }

        private ICity BuildCity(int population, ICivilization owner, List<IBuilding> buildings = null) {
            var mockCity = new Mock<ICity>();

            mockCity.SetupAllProperties();

            var newCity = mockCity.Object;

            newCity.Population = population;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private IBuilding BuildBuilding(float foodStockpilePreservationBonus) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.FoodStockpilePreservationBonus).Returns(foodStockpilePreservationBonus);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
