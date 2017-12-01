using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.UI;

namespace Assets.Tests.UI {

    [TestFixture]
    public class ResourceSummaryDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties

        public static IEnumerable TestCases {
            get {
                yield return new ResourceSummary(food: 1, production: 2, gold: 3, culture: 4);
            }
        }

        #endregion

        #region instance fields and properties

        private Text FoodField;
        private Text GoldField;
        private Text ProductionField;
        private Text CultureField;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            FoodField = Container.InstantiateComponentOnNewGameObject<Text>();
            Container.Bind<Text>().WithId("Food Field").FromInstance(FoodField);

            GoldField = Container.InstantiateComponentOnNewGameObject<Text>();
            Container.Bind<Text>().WithId("Gold Field").FromInstance(GoldField);

            ProductionField = Container.InstantiateComponentOnNewGameObject<Text>();
            Container.Bind<Text>().WithId("Production Field").FromInstance(ProductionField);

            CultureField = Container.InstantiateComponentOnNewGameObject<Text>();
            Container.Bind<Text>().WithId("Culture Field").FromInstance(CultureField);

            Container.Bind<ResourceSummaryDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When DisplaySummary is called, all fields should be set properly")]
        [TestCaseSource("TestCases")]
        public void DisplaySummary_NonNullArgumentSetsFields(ResourceSummary summaryToDisplay) {
            var display = Container.Resolve<ResourceSummaryDisplay>();

            display.DisplaySummary(summaryToDisplay);

            Assert.AreEqual(summaryToDisplay[ResourceType.Food].ToString(), FoodField.text,
                "FoodField.text has an unexpected value");

            Assert.AreEqual(summaryToDisplay[ResourceType.Gold].ToString(), GoldField.text,
                "GoldField.text has an unexpected value");

            Assert.AreEqual(summaryToDisplay[ResourceType.Production].ToString(), ProductionField.text,
                "ProductionField.text has an unexpected value");

            Assert.AreEqual(summaryToDisplay[ResourceType.Culture].ToString(), CultureField.text,
                "CultureField.text has an unexpected value");
        }

        #endregion

        #endregion

    }

}
