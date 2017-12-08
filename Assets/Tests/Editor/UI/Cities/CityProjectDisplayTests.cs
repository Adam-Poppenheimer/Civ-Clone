using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;

using Assets.UI;
using Assets.UI.Cities.Production;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityProjectDisplayTests : ZenjectUnitTestFixture {

        #region internal types

        private class TestData {

            public static IEnumerable SingleProjectCases {
                get {
                    yield return new TestCaseData(new ProjectData("Active Project", 100, 25, 10));
                }
            }

            public static IEnumerable DoubleProjectCases {
                get {
                    yield return new TestCaseData(
                        new ProjectData("First Project", 100, 25, 10),
                        new ProjectData("Second Project", 250, 0, 15)
                    );
                }
            }

        }
        
        public class ProjectData {

            public string Name;
            public int ToComplete;
            public int Progress;
            public int ProductionPerTurn;

            public ProjectData(string name, int toComplete, int progress, int productionPerTurn) {
                Name = name;
                ToComplete = toComplete;
                Progress = progress;
                ProductionPerTurn = productionPerTurn;
            }

            public override string ToString() {
                return string.Format("Name: {0}, ToComplete: {1}, Progress: {2}, ProductionPerTurn: {3}",
                    Name, ToComplete, Progress, ProductionPerTurn);
            }

        }

        #endregion

        #region instance fields and properties

        private Text ProjectNameField;
        private Text ProjectCostField;
        private Text TurnsLeftField;
        private Slider ProductionProgressSlider;

        private Mock<IProductionLogic> MockProductionLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            ProjectNameField         = Container.InstantiateComponentOnNewGameObject<Text>();
            ProjectCostField         = Container.InstantiateComponentOnNewGameObject<Text>();
            TurnsLeftField           = Container.InstantiateComponentOnNewGameObject<Text>();
            ProductionProgressSlider = Container.InstantiateComponentOnNewGameObject<Slider>();

            Container.Bind<Text>()  .WithId("Project Name Field")        .FromInstance(ProjectNameField);
            Container.Bind<Text>()  .WithId("Project Cost Field")        .FromInstance(ProjectCostField);
            Container.Bind<Text>()  .WithId("Turns Left Field")          .FromInstance(TurnsLeftField);
            Container.Bind<Slider>().WithId("Production Progress Slider").FromInstance(ProductionProgressSlider);

            ProductionProgressSlider.minValue = 0;
            ProductionProgressSlider.maxValue = 0;
            ProductionProgressSlider.value = 0;

            MockProductionLogic = new Mock<IProductionLogic>();

            Container.Bind<IProductionLogic>().FromInstance(MockProductionLogic.Object);

            Container.Bind<CityProjectDisplay>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.DeclareSignal<CityProjectChangedSignal>();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and CityToDisplay is null, " +
            "none of the fields or sliders should be changed")]
        public void RefreshCalled_DoesNothingOnNullCity() {
            var projectDisplay = Container.Resolve<CityProjectDisplay>();

            projectDisplay.Refresh();

            Assert.AreEqual("", ProjectNameField.text,
                "ProjectNameField has an unexpected value");

            Assert.AreEqual("", ProjectCostField.text,
                "ProjectCostField has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.minValue,
                "ProductionProgressSlider.minValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.maxValue,
                "ProductionProgressSlider.maxValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.value,
                "ProductionProgressSlider.value has an unexpected value");

            Assert.AreEqual("", TurnsLeftField.text,
                "TurnsLeftField has an unexpected value");
        }

        [Test(Description = "When Refresh is called, and CityToDisplay has a null current project, " +
            "all fields and the slider should be cleared of any values")]
        public void RefreshCalled_ClearsFieldsOnNullProject() {
            var cityMock = BuildCity(null);

            var projectDisplay = Container.Resolve<CityProjectDisplay>();

            projectDisplay.ObjectToDisplay = cityMock.Object;
            projectDisplay.Refresh();

            Assert.AreEqual("--", ProjectNameField.text,
                "ProjectNameField has an unexpected value");

            Assert.AreEqual("--", ProjectCostField.text,
                "ProjectCostField has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.minValue,
                "ProductionProgressSlider.minValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.maxValue,
                "ProductionProgressSlider.maxValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.value,
                "ProductionProgressSlider.value has an unexpected value");

            Assert.AreEqual("--", TurnsLeftField.text,
                "TurnsLeftField has an unexpected value");
        }

        [   Test(Description = "When Refresh is called and CityToDisplay has a non-null current project, " +
                "all fields and the slider should be given appropriate values from ProductionLogic"),
            TestCaseSource(typeof(CityProjectDisplayTests.TestData), "SingleProjectCases")
        ]
        public void RefreshCalled_FillsFieldsOnNonNullProject(ProjectData data) {
            var cityMock = BuildCity(BuildProject(data.Name, data.ToComplete, data.Progress));
            var city = cityMock.Object;

            var projectDisplay = Container.Resolve<CityProjectDisplay>();

            MockProductionLogic
                .Setup(logic => logic.GetProductionProgressPerTurnOnProject(It.IsAny<ICity>(), It.IsAny<IProductionProject>()))
                .Returns(data.ProductionPerTurn);

            projectDisplay.ObjectToDisplay = city;
            projectDisplay.Refresh();

            Assert.AreEqual(city.ActiveProject.Name, ProjectNameField.text,
                "ProjectNameField was not set correctly");

            Assert.AreEqual(city.ActiveProject.ProductionToComplete.ToString(), ProjectCostField.text,
                "ProjectCostField was not set correctly");

            Assert.AreEqual(0, ProductionProgressSlider.minValue,
                "ProductionProgressSlider.minValue was not set correctly");

            Assert.AreEqual(city.ActiveProject.ProductionToComplete, ProductionProgressSlider.maxValue,
                "ProductionProgressSlider.maxValue was not set correctly");

            Assert.AreEqual(city.ActiveProject.Progress, ProductionProgressSlider.value,
                "ProductionProgressSlider.value was not set correctly");

            int expectedTurnsLeft = Mathf.CeilToInt((data.ToComplete - data.Progress) / (float)data.ProductionPerTurn);

            Assert.AreEqual(expectedTurnsLeft.ToString(), TurnsLeftField.text,
                "TurnsLeftField was not set correctly");
        }

        [   Test(Description = "When CityProjectChangedSignal is fired and the passed city is " +
                "different from CityToDisplay, nothing should happen"),
            TestCaseSource(typeof(CityProjectDisplayTests.TestData), "SingleProjectCases")
        ]
        public void ProjectChangedSignalFired_DoesNothingIfDifferentCity(ProjectData data) {
            var firstCityMock = BuildCity(BuildProject(data.Name, data.ToComplete, data.Progress));
            var firstCity = firstCityMock.Object;

            Container.Resolve<CityProjectDisplay>();

            Container.Resolve<CityProjectChangedSignal>().Fire(firstCity, firstCity.ActiveProject);

            Assert.AreEqual("", ProjectNameField.text,
                "ProjectNameField has an unexpected value");

            Assert.AreEqual("", ProjectCostField.text,
                "ProjectCostField has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.minValue,
                "ProductionProgressSlider.minValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.maxValue,
                "ProductionProgressSlider.maxValue has an unexpected value");

            Assert.AreEqual(0, ProductionProgressSlider.value,
                "ProductionProgressSlider.value has an unexpected value");

            Assert.AreEqual("", TurnsLeftField.text,
                "TurnsLeftField has an unexpected value");
        }

        [   Test(Description = "When CityProjectChangedSignal is fired and the passed city is " +
                "CityToDisplay, CityProjectDisplay should be refreshed"),
            TestCaseSource(typeof(CityProjectDisplayTests.TestData), "DoubleProjectCases")
        ]
        public void ProjectChangedSignalFired_RefreshesIfSameCity(ProjectData projectOne, ProjectData projectTwo) {
            var cityMock = BuildCity(BuildProject(projectOne.Name, projectOne.ToComplete, projectOne.Progress));
            var actualCity = cityMock.Object;

            var secondProject = BuildProject(projectTwo.Name, projectTwo.ToComplete, projectTwo.Progress);

            var projectDisplay = Container.Resolve<CityProjectDisplay>();

            MockProductionLogic
                .Setup(logic => logic.GetProductionProgressPerTurnOnProject(It.IsAny<ICity>(), It.IsAny<IProductionProject>()))
                .Returns(projectOne.ProductionPerTurn);

            projectDisplay.ObjectToDisplay = actualCity;
            projectDisplay.Refresh();

            cityMock.Setup(city => city.ActiveProject).Returns(secondProject);

            MockProductionLogic
                .Setup(logic => logic.GetProductionProgressPerTurnOnProject(It.IsAny<ICity>(), It.IsAny<IProductionProject>()))
                .Returns(projectTwo.ProductionPerTurn);

            Container.Resolve<CityProjectChangedSignal>().Fire(actualCity, secondProject);

            Assert.AreEqual(projectTwo.Name, ProjectNameField.text,
                "ProjectNameField was not set correctly");

            Assert.AreEqual(projectTwo.ToComplete.ToString(), ProjectCostField.text,
                "ProjectCostField was not set correctly");

            Assert.AreEqual(0, ProductionProgressSlider.minValue,
                "ProductionProgressSlider.minValue was not set correctly");

            Assert.AreEqual(projectTwo.ToComplete, ProductionProgressSlider.maxValue,
                "ProductionProgressSlider.maxValue was not set correctly");

            Assert.AreEqual(projectTwo.Progress, ProductionProgressSlider.value,
                "ProductionProgressSlider.value was not set correctly");

            int expectedTurnsLeft = Mathf.CeilToInt((projectTwo.ToComplete - projectTwo.Progress) / (float)projectTwo.ProductionPerTurn);

            Assert.AreEqual(expectedTurnsLeft.ToString(), TurnsLeftField.text,
                "TurnsLeftField was not set correctly");
        }

        #endregion

        #region utilities

        private Mock<ICity> BuildCity(IProductionProject activeProject) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.ActiveProject).Returns(activeProject);

            return mockCity;
        }

        private IProductionProject BuildProject(string name, int toComplete, int progress) {
            var projectMock = new Mock<IProductionProject>();

            projectMock.Setup(project => project.Name)                .Returns(name);
            projectMock.Setup(project => project.ProductionToComplete).Returns(toComplete);
            projectMock.Setup(project => project.Progress)            .Returns(progress);

            return projectMock.Object;
        }

        #endregion

        #endregion

    }

}
