using System;
using System.Collections;
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

using Assets.Simulation.Cities;
using Assets.Simulation.Core;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.GameMap;
using Assets.Simulation;

using Assets.UI.Cities.Territory;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityExpansionDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TileIndicatorSources {
            get {
                yield return new TestCaseData(150, 62, 7, new Vector3(1f, 2.5f, -3f));
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IBorderExpansionLogic> MockExpansionLogic;
        private Mock<IResourceGenerationLogic> MockGenerationLogic;

        private Text CultureStockpileField;
        private Text CultureNeededField;
        private Text TurnsUntilExpansionField;
        private Slider ExpansionProgressSlider;

        private RectTransform NextTileIndicator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockExpansionLogic = new Mock<IBorderExpansionLogic>();
            MockGenerationLogic = new Mock<IResourceGenerationLogic>();

            Container.Bind<IBorderExpansionLogic>().FromInstance(MockExpansionLogic.Object);
            Container.Bind<IResourceGenerationLogic>().FromInstance(MockGenerationLogic.Object);

            CultureStockpileField    = Container.InstantiateComponentOnNewGameObject<Text>  ();
            CultureNeededField       = Container.InstantiateComponentOnNewGameObject<Text>  ();
            TurnsUntilExpansionField = Container.InstantiateComponentOnNewGameObject<Text>  ();
            ExpansionProgressSlider  = Container.InstantiateComponentOnNewGameObject<Slider>();

            NextTileIndicator = Container.InstantiateComponentOnNewGameObject<RectTransform>();

            CultureStockpileField   .text = "";
            CultureNeededField      .text = "";
            TurnsUntilExpansionField.text = "";

            ExpansionProgressSlider.minValue = 0;
            ExpansionProgressSlider.maxValue = 0;
            ExpansionProgressSlider.value = 0;

            Container.Bind<Text>  ().WithId("Culture Stockpile Field")    .FromInstance(CultureStockpileField);
            Container.Bind<Text>  ().WithId("Culture Needed Field")       .FromInstance(CultureNeededField);
            Container.Bind<Text>  ().WithId("Turns Until Expansion Field").FromInstance(TurnsUntilExpansionField);
            Container.Bind<Slider>().WithId("Expansion Progress Slider")   .FromInstance(ExpansionProgressSlider);

            Container.Bind<RectTransform>().WithId("Next Tile Indicator").FromInstance(NextTileIndicator);

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<IObservable<ICity>>().WithId("Select Requested Signal").FromMock();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<IObservable<ICity>>().WithId("Deselect Requested Signal").FromMock();

            Container.Bind<CityExpansionDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and CityToDisplay is null, " + 
            "nothing significant should happen")]
        public void OnRefresh_AndCityToDisplayIsNull_DoesNothing() {
            var expansionDisplay = Container.Resolve<CityExpansionDisplay>();

            expansionDisplay.Refresh();

            Assert.AreEqual("", CultureStockpileField   .text, "CultureStockpileField.text has an unexpected value"   );
            Assert.AreEqual("", CultureNeededField      .text, "CultureNeededField.text has an unexpected value"      );
            Assert.AreEqual("", TurnsUntilExpansionField.text, "TurnsUntilExpansionField.text has an unexpected value");

            Assert.AreEqual(0, ExpansionProgressSlider.minValue, "ExpansionProgressSlider.minValue has an unexpected value");
            Assert.AreEqual(0, ExpansionProgressSlider.maxValue, "ExpansionProgressSlider.maxValue has an unexpected value");
            Assert.AreEqual(0, ExpansionProgressSlider.value,    "ExpansionProgressSlider.value has an unexpected value"   );

            Assert.AreEqual(Vector3.zero, NextTileIndicator.position, "NextTileIndicator.position has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay is not null, " +
            "All fields and the slider should be provided the correct information " +
            "from ExpansionLogic and ResourceGenerationLogic")]
        [TestCase(150, 62, 7)]
        public void OnRefresh_AndCityToDisplayNotNull_RefreshesFieldsProperly(
            int cultureNeeded, int cultureStockpile, int cultureIncome
        ){
            var nextTile = BuildTile(Vector3.zero);

            var city = BuildCity(cultureNeeded, cultureStockpile, cultureIncome, nextTile);

            var expansionDisplay = Container.Resolve<CityExpansionDisplay>();

            expansionDisplay.ObjectToDisplay = city;
            expansionDisplay.Refresh();

            Assert.AreEqual(cultureStockpile.ToString(), CultureStockpileField.text,
                "CultureStockpileField.text has an unexpected value");

            Assert.AreEqual(cultureNeeded.ToString(), CultureNeededField.text,
                "CultureNeededField.text has an unexpected value");

            var turnsUntilExpansion = Mathf.CeilToInt((cultureNeeded - cultureStockpile) / (float)cultureIncome);

            Assert.AreEqual(turnsUntilExpansion.ToString(), TurnsUntilExpansionField.text,
                "TurnsUntilExpansionField.text has an unexpected value");

            Assert.AreEqual(0, ExpansionProgressSlider.minValue, "ExpansionProgressSlider.minValue has an unexpected value");

            Assert.AreEqual(cultureNeeded, ExpansionProgressSlider.maxValue, "ExpansionProgressSlider.maxValue has an unexpected value");

            Assert.AreEqual(cultureStockpile, ExpansionProgressSlider.value, "ExpansionProgressSlider.value has an unexpected value");
        }

        [Test(Description = "When Refresh is called and CityToDisplay has no culture income, "+ 
            "TurnsUntilExpansionField should be cleared with a non-numeric value")]
        [TestCase(150, 62)]
        public void OnRefresh_AndZeroCultureIncome_RefreshesFieldsProperly(
            int cultureNeeded, int cultureStockpile
        ) {
            var nextTile = BuildTile(Vector3.zero);

            var city = BuildCity(cultureNeeded, cultureStockpile, 0, nextTile);

            var expansionDisplay = Container.Resolve<CityExpansionDisplay>();

            expansionDisplay.ObjectToDisplay = city;
            expansionDisplay.Refresh();

            Assert.AreEqual("--", TurnsUntilExpansionField.text, "TurnsUntilExpansionField.text was not cleared correctly");

        }

        [Test(Description = "When Refresh is called and CityToDisplay is not null, " +
            "NextTileIndicator should be relocated to the screen-space coordinates " +
            "of the next tile to acquire as provided by ExpansionLogic")]
        [TestCaseSource("TileIndicatorSources")]
        public void OnRefresh_AndCityToDisplayNotNull_SetsNextTileIndicatorProperly(
            int cultureNeeded, int cultureStockpile, int cultureIncome, Vector3 nextTilePosition
        ){
            var nextTile = BuildTile(nextTilePosition);

            var city = BuildCity(cultureNeeded, cultureStockpile, cultureIncome, nextTile);

            var expansionDisplay = Container.Resolve<CityExpansionDisplay>();

            expansionDisplay.ObjectToDisplay = city;
            expansionDisplay.Refresh();

            var expectedIndicatorLocation = Camera.main.WorldToScreenPoint(nextTilePosition);

            Assert.AreEqual(expectedIndicatorLocation, NextTileIndicator.transform.position,
                "NextTileIndicator has an unexpected location");
        }

        #endregion

        #region utilities

        private IMapTile BuildTile(Vector3 position) {
            var mockTile = new Mock<IMapTile>();

            var transform = new GameObject().transform;
            transform.position = position;

            mockTile.Setup(tile => tile.transform).Returns(transform);

            return mockTile.Object;
        }

        private ICity BuildCity(int cultureNeeded, int cultureStockpile, int cultureIncome, IMapTile nextTile) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.CultureStockpile).Returns(cultureStockpile);

            var newCity = mockCity.Object;

            MockExpansionLogic
                .Setup(logic => logic.GetNextTileToPursue(newCity))
                .Returns(nextTile);

            MockExpansionLogic
                .Setup(logic => logic.GetCultureCostOfAcquiringTile(newCity, nextTile))
                .Returns(cultureNeeded);

            MockGenerationLogic
                .Setup(logic => logic.GetTotalYieldForCity(newCity))
                .Returns(new ResourceSummary(culture: cultureIncome));

            return newCity;
        }

        #endregion

        #endregion

    }

}
