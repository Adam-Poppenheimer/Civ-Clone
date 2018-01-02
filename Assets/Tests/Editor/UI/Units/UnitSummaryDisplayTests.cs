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

using Assets.UI.Units;
using Assets.Simulation.Units;
using Assets.Simulation.Core;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class UnitSummaryDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData("Test Unit 1", 2, 5, UnitType.LandCivilian, 57, 100 ).SetName("Test Unit 1");
                yield return new TestCaseData("Test Unit 2", 6, 5, UnitType.LandCivilian, 100, 100).SetName("Test Unit 2");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitConfig> MockConfig;

        private Text NameField;
        private Text CurrentMovementField;
        private Text MaxMovementField;
        private Text TypeField;

        private Slider HealthSlider;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IUnitConfig>();

            Container.Bind<IUnitConfig>().FromInstance(MockConfig.Object);

            NameField            = Container.InstantiateComponentOnNewGameObject<Text>();
            CurrentMovementField = Container.InstantiateComponentOnNewGameObject<Text>();
            MaxMovementField     = Container.InstantiateComponentOnNewGameObject<Text>();
            TypeField            = Container.InstantiateComponentOnNewGameObject<Text>();

            HealthSlider = Container.InstantiateComponentOnNewGameObject<Slider>();

            NameField           .text = "";
            CurrentMovementField.text = "";
            MaxMovementField    .text = "";
            TypeField           .text = "";            

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = 0;
            HealthSlider.value    = 0;

            Container.Bind<Text>().WithId("Name Field"            ).FromInstance(NameField);
            Container.Bind<Text>().WithId("Current Movement Field").FromInstance(CurrentMovementField);
            Container.Bind<Text>().WithId("Max Movement Field"    ).FromInstance(MaxMovementField);
            Container.Bind<Text>().WithId("Type Field"            ).FromInstance(TypeField);

            Container.Bind<Slider>().WithId("Health Slider").FromInstance(HealthSlider);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<UnitSignals>().FromMock();

            Container.Bind<UnitSummaryDisplay>().FromNewComponentOnNewGameObject().AsSingle();            
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and ObjectToDisplay is null, nothing significant " +
            "should happen")]        
        public void Refresh_DoesNothingOnNullUnit() {
            var summaryDisplay = Container.Resolve<UnitSummaryDisplay>();

            summaryDisplay.Refresh();

            Assert.AreEqual("", NameField           .text, "NameField.text has an unexpected value"           );
            Assert.AreEqual("", CurrentMovementField.text, "CurrentMovementField.text has an unexpected value");
            Assert.AreEqual("", MaxMovementField    .text, "MaxMovementField.text has an unexpected value"    );
            Assert.AreEqual("", TypeField           .text, "TypeField.text has an unexpected value"           );

            Assert.AreEqual(0, HealthSlider.minValue, "HealthSlider.minValue has an unexpected value");
            Assert.AreEqual(0, HealthSlider.maxValue, "HealthSlider.maxValue has an unexpected value");
            Assert.AreEqual(0, HealthSlider.value,    "HealthSlider.value has an unexpected value");
        }

        [Test(Description = "When Refresh is called and ObjectToDisplay is not null, " +
            "all of the fields and sliders should have their values set to reflect " +
            "the selected unit's properties")]
        [TestCaseSource("TestCases")]
        public void Refresh_SetsFieldsOnNonNullUnit(string name, int currentMovement, int maxMovement,
            UnitType type, int currentHealth, int maxHealth
        ){
            var unit = BuildUnit(name, currentMovement, maxMovement, type, currentHealth);

            MockConfig.Setup(config => config.MaxHealth).Returns(maxHealth);

            var summaryDisplay = Container.Resolve<UnitSummaryDisplay>();

            summaryDisplay.ObjectToDisplay = unit;
            summaryDisplay.Refresh();

            Assert.AreEqual(name,                       NameField           .text, "NameField.text had an unexpected value");
            Assert.AreEqual(currentMovement.ToString(), CurrentMovementField.text, "CurrentMovementField.text had an unexpected value");
            Assert.AreEqual(maxMovement    .ToString(), MaxMovementField    .text, "MaxMovementField.text had an unexpected value");
            Assert.AreEqual(type           .ToString(), TypeField           .text, "TypeField.text had an unexpected value");

            Assert.AreEqual(0,             HealthSlider.minValue, "HealthSlider.minValue had an unexpected value");
            Assert.AreEqual(maxHealth,     HealthSlider.maxValue, "HealthSlider.maxValue had an unexpected value");
            Assert.AreEqual(currentHealth, HealthSlider.value,    "HealthSlider.value had an unexpected value");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(
            string name, int currentMovement, int maxMovement,
            UnitType type, int currentHealth
        ){
            var mockUnit = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.MaxMovement).Returns(maxMovement);
            mockTemplate.Setup(template => template.Type)       .Returns(type);
            mockTemplate.Setup(template => template.Name)       .Returns(name);

            mockUnit.Setup(unit => unit.Template)       .Returns(mockTemplate.Object);
            mockUnit.Setup(unit => unit.Health)         .Returns(currentHealth);
            mockUnit.Setup(unit => unit.CurrentMovement).Returns(currentMovement);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
