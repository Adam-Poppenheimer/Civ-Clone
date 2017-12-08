using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Core;

using Assets.UI.Cities;
using Assets.UI.Cities.Growth;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityGrowthDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties



        #endregion

        #region instance fields and properties

        private Mock<IPopulationGrowthLogic> MockGrowthLogic;
        private Mock<IResourceGenerationLogic> MockGenerationLogic;

        private Text CurrentPopulationField;
        private Text CurrentFoodStockpileField;
        private Text FoodUntilNextGrowthField;

        private Text ChangeStatusField;

        private Slider GrowthSlider;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            CurrentPopulationField    = Container.InstantiateComponentOnNewGameObject<Text>();
            CurrentFoodStockpileField = Container.InstantiateComponentOnNewGameObject<Text>();
            FoodUntilNextGrowthField  = Container.InstantiateComponentOnNewGameObject<Text>();
            ChangeStatusField         = Container.InstantiateComponentOnNewGameObject<Text>();

            GrowthSlider = Container.InstantiateComponentOnNewGameObject<Slider>();

            CurrentPopulationField   .text = "";
            CurrentFoodStockpileField.text = "";
            FoodUntilNextGrowthField .text = "";
            ChangeStatusField        .text = "";

            GrowthSlider.minValue = 0;
            GrowthSlider.maxValue = 0;
            GrowthSlider.value = 0;

            Container.Bind<Text>().WithId("Current Population Field")    .FromInstance(CurrentPopulationField);
            Container.Bind<Text>().WithId("Current Food Stockpile Field").FromInstance(CurrentFoodStockpileField);
            Container.Bind<Text>().WithId("Food Until Next Growth Field").FromInstance(FoodUntilNextGrowthField);
            Container.Bind<Text>().WithId("Change Status Field")         .FromInstance(ChangeStatusField);

            Container.Bind<Slider>().WithId("Growth Slider").FromInstance(GrowthSlider);

            MockGrowthLogic     = new Mock<IPopulationGrowthLogic>();
            MockGenerationLogic = new Mock<IResourceGenerationLogic>();

            Container.Bind<IPopulationGrowthLogic>()  .FromInstance(MockGrowthLogic.Object);
            Container.Bind<IResourceGenerationLogic>().FromInstance(MockGenerationLogic.Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<SlotDisplayClickedSignal>();

            Container.Bind<CityGrowthDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and CityToDisplay is null, " +
            "nothing significant should occur")]
        public void OnRefresh_AndNullCityToDisplay_DoesNothing() {
            var growthDisplay = Container.Resolve<CityGrowthDisplay>();

            growthDisplay.Refresh();

            Assert.AreEqual("", CurrentPopulationField.text,    "CurrentPopulationField.text has an unexpected value"   );
            Assert.AreEqual("", CurrentFoodStockpileField.text, "CurrentFoodStockpileField.text has an unexpected value");
            Assert.AreEqual("", FoodUntilNextGrowthField.text,  "FoodUntilNextGrowthField.text has an unexpected value" );
            Assert.AreEqual("", ChangeStatusField.text,         "ChangeStatusField.text has an unexpected value"        );

            Assert.AreEqual(0, GrowthSlider.minValue, "GrowthSlider.minValue has an unexpected value");
            Assert.AreEqual(0, GrowthSlider.maxValue, "GrowthSlider.maxValue has an unexpected value");
            Assert.AreEqual(0, GrowthSlider.value,    "GrowthSlider.value has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay is not null, " +
            "All the fields and the slider should be refreshed with appropriate values")]
        [TestCase(5, 22, 70, 12, 8)]
        public void OnRefresh_AndNonNullCityToDisplay_RefreshesFieldsAndSlider(
            int population, int foodStockpile, int foodForNextGrowth, int foodYield,
            int foodConsumption
        ) {
            var city = BuildCity(population, foodStockpile, foodForNextGrowth, foodYield, foodConsumption);

            var growthDisplay = Container.Resolve<CityGrowthDisplay>();

            growthDisplay.ObjectToDisplay = city;
            growthDisplay.Refresh();

            Assert.AreEqual(population.ToString(), CurrentPopulationField.text,
                "CurrentPopulationField.text has an unexpected value"   );

            Assert.AreEqual(foodStockpile.ToString(), CurrentFoodStockpileField.text,
                "CurrentFoodStockpileField.text has an unexpected value");

            Assert.AreEqual(foodForNextGrowth.ToString(), FoodUntilNextGrowthField.text,
                "FoodUntilNextGrowthField.text has an unexpected value" );

            int turnsUntilGrowth = Mathf.CeilToInt((foodForNextGrowth - foodStockpile) / (float)(foodYield - foodConsumption));

            Assert.AreEqual(string.Format("{0} turns until growth", turnsUntilGrowth), ChangeStatusField.text,
                "ChangeStatusField.text has an unexpected value");

            Assert.AreEqual(0, GrowthSlider.minValue, "GrowthSlider.minValue has an unexpected value");
            Assert.AreEqual(foodForNextGrowth, GrowthSlider.maxValue, "GrowthSlider.maxValue has an unexpected value");
            Assert.AreEqual(foodStockpile, GrowthSlider.value,    "GrowthSlider.value has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay has a " +
            "net food income of zero, TurnsUntilNextChangeField should reflect " +
            "the stagnancy properly")]
        [TestCase(5, 22, 70)]
        public void OnRefresh_AndNetFoodZero_DisplaysStagnancyProperly(
            int population, int foodStockpile, int foodForNextGrowth
        ){
            var city = BuildCity(population, foodStockpile, foodForNextGrowth, 0, 0);

            var growthDisplay = Container.Resolve<CityGrowthDisplay>();

            growthDisplay.ObjectToDisplay = city;
            growthDisplay.Refresh();

            Assert.AreEqual("Stagnation", ChangeStatusField.text, "ChangeStatusField.text has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay has a " +
            "negative net food income, TurnsUntilNextChangeField should reflect " +
            "the turns until starvation properly")]
        [TestCase(5, 22, 70, 4)]
        public void OnRefresh_AndNetFoodNegative_DisplaysStarvationProperly(
            int population, int foodStockpile, int foodForNextGrowth, int foodDeficit
        ) {
            var city = BuildCity(population, foodStockpile, foodForNextGrowth, 0, foodDeficit);

            var growthDisplay = Container.Resolve<CityGrowthDisplay>();

            growthDisplay.ObjectToDisplay = city;
            growthDisplay.Refresh();

            int turnsUntilStarvation = Mathf.CeilToInt(foodStockpile / (float)foodDeficit);

            Assert.AreEqual(string.Format("{0} turns until starvation", turnsUntilStarvation), ChangeStatusField.text,
                "ChangeStatusField.text has an unexpected value");
        }

        #endregion

        #region utilities

        private ICity BuildCity(int population, int foodStockpile, int foodForNextGrowth, int foodYield, int foodConsumption) {
            var mockCity = new Mock<ICity>();
            mockCity.Setup(city => city.Population).Returns(population);
            mockCity.Setup(city => city.FoodStockpile).Returns(foodStockpile);

            MockGrowthLogic
                .Setup(logic => logic.GetFoodStockpileToGrow(mockCity.Object))
                .Returns(foodForNextGrowth);

            MockGrowthLogic
                .Setup(logic => logic.GetFoodConsumptionPerTurn(mockCity.Object))
                .Returns(foodConsumption);

            MockGenerationLogic
                .Setup(logic => logic.GetTotalYieldForCity(mockCity.Object))
                .Returns(new ResourceSummary(food: foodYield));
            
            return mockCity.Object;  
        }

        #endregion

        #endregion

    }

}
