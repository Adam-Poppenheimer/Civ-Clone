using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    public class CityProductionResolverTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private CitySignals                                         CitySignals;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICityFactory>                                  MockCityFactory;
        private Mock<IBuildingProductionValidityLogic>              MockBuildingProductionValidityLogic;
        private CoreSignals                                         CoreSignals;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            CitySignals                         = new CitySignals();
            MockCityPossessionCanon             = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityFactory                     = new Mock<ICityFactory>();
            MockBuildingProductionValidityLogic = new Mock<IBuildingProductionValidityLogic>();
            CoreSignals                         = new CoreSignals();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<CitySignals>                                  ().FromInstance(CitySignals);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon            .Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory                    .Object);
            Container.Bind<IBuildingProductionValidityLogic>             ().FromInstance(MockBuildingProductionValidityLogic.Object);
            Container.Bind<CoreSignals>                                  ().FromInstance(CoreSignals);

            Container.Bind<CityProductionResolver>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void MakeProductionRequest_ImmediatelyExecutesUnitProjects() {
            var city = BuildCity(null);

            Mock<IProductionProject> mockUnitProject;
            var unitProject = BuildUnitProject(out mockUnitProject);
            
            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(unitProject, city);

            mockUnitProject.Verify(project => project.Execute(city), Times.Once);
        }

        [Test]
        public void MakeProductionRequest_ImmediatelyExecutesNormalBuildingProjects() {
            var city = BuildCity(null);

            Mock<IProductionProject> mockBuildingProject;
            var buildingProject = BuildBuildingProject(BuildTemplate(BuildingType.Normal), 0, out mockBuildingProject);
            
            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(buildingProject, city);

            mockBuildingProject.Verify(project => project.Execute(city), Times.Once);
        }

        [Test]
        public void MakeProductionRequest_DoesNotImmediatelyExecuteNationalWonderProjects() {
            var city = BuildCity(null);

            Mock<IProductionProject> mockBuildingProject;
            var buildingProject = BuildBuildingProject(BuildTemplate(BuildingType.NationalWonder), 0, out mockBuildingProject);
            
            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(buildingProject, city);

            mockBuildingProject.Verify(project => project.Execute(city), Times.Never);
        }

        [Test]
        public void MakeProductionRequest_DoesNotImmediatelyExecuteWorldWonderProjects() {
            var city = BuildCity(null);

            Mock<IProductionProject> mockBuildingProject;
            var buildingProject = BuildBuildingProject(BuildTemplate(BuildingType.WorldWonder), 0, out mockBuildingProject);
            
            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(buildingProject, city);

            mockBuildingProject.Verify(project => project.Execute(city), Times.Never);
        }

        [Test]
        public void ResolveBuildingConstructionRequests_ExecutesHighestProgressWorldWonderGlobally() {
            var cityOne = BuildCity(null);
            var cityTwo = BuildCity(null);

            BuildCiv(cityOne);
            BuildCiv(cityTwo);

            var wonderTemplate = BuildTemplate(BuildingType.WorldWonder);

            Mock<IProductionProject> projectOneMock, projectTwoMock;
            var projectOne = BuildBuildingProject(wonderTemplate, 20, out projectOneMock);
            var projectTwo = BuildBuildingProject(wonderTemplate, 30, out projectTwoMock);

            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(projectOne, cityOne);
            resolver.MakeProductionRequest(projectTwo, cityTwo);
            
            resolver.ResolveBuildingConstructionRequests();

            projectOneMock.Verify(project => project.Execute(It.IsAny<ICity>()), Times.Never, "ProjectOne was incorrectly executed");
            projectTwoMock.Verify(project => project.Execute(cityTwo),           Times.Once,  "ProjectTwo was not executed as expected");
        }

        [Test]
        public void ResolveBuildingConstructionRequests_ExecutesHighestProgressNationalWonderPerCiv() {
            var cityOne   = BuildCity(null);
            var cityTwo   = BuildCity(null);
            var cityThree = BuildCity(null);
            var cityFour  = BuildCity(null);

            BuildCiv(cityOne,   cityTwo);
            BuildCiv(cityThree, cityFour);

            var wonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            Mock<IProductionProject> projectOneMock, projectTwoMock, projectThreeMock, projectFourMock;
            var projectOne   = BuildBuildingProject(wonderTemplate, 20, out projectOneMock);
            var projectTwo   = BuildBuildingProject(wonderTemplate, 30, out projectTwoMock);
            var projectThree = BuildBuildingProject(wonderTemplate, 35, out projectThreeMock);
            var projectFour  = BuildBuildingProject(wonderTemplate, 25, out projectFourMock);

            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(projectOne,   cityOne);
            resolver.MakeProductionRequest(projectTwo,   cityTwo);
            resolver.MakeProductionRequest(projectThree, cityThree);
            resolver.MakeProductionRequest(projectFour,  cityFour);
            
            resolver.ResolveBuildingConstructionRequests();

            projectOneMock  .Verify(project => project.Execute(It.IsAny<ICity>()), Times.Never, "ProjectOne was incorrectly executed");
            projectTwoMock  .Verify(project => project.Execute(cityTwo),           Times.Once,  "ProjectTwo was not executed as expected");
            projectThreeMock.Verify(project => project.Execute(cityThree),         Times.Once,  "ProjectThree was not executed as expected");
            projectFourMock .Verify(project => project.Execute(It.IsAny<ICity>()), Times.Never, "ProjectFour was incorrectly executed");
        }

        [Test]
        public void ResolveBuildingConstructionRequests_RepeatedCallsDoNothing() {
            var cityOne = BuildCity(null);

            BuildCiv(cityOne);

            var wonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            Mock<IProductionProject> projectOneMock;
            var projectOne = BuildBuildingProject(wonderTemplate, 20, out projectOneMock);

            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(projectOne, cityOne);

            resolver.ResolveBuildingConstructionRequests();

            projectOneMock.ResetCalls();

            resolver.ResolveBuildingConstructionRequests();
            resolver.ResolveBuildingConstructionRequests();
            resolver.ResolveBuildingConstructionRequests();

            projectOneMock.Verify(project => project.Execute(It.IsAny<ICity>()), Times.Never);
        }

        [Test]
        public void OnCityGainedBuildingFired_CitiesWithInvalidBuildingProjectsLoseThoseProjects() {
            var validTemplate   = BuildTemplate(BuildingType.Normal);
            var invalidTemplate = BuildTemplate(BuildingType.Normal);

            Mock<IProductionProject> validProjectMock, invalidProjectMock;
            var validProject   = BuildBuildingProject(validTemplate,   0, out validProjectMock);
            var invalidProject = BuildBuildingProject(invalidTemplate, 0, out invalidProjectMock);

            var validCity   = BuildCity(validProject);
            var invalidCity = BuildCity(invalidProject);
            var nullCity    = BuildCity(null);

            MockBuildingProductionValidityLogic.Setup(logic => logic.IsTemplateValidForCity(validTemplate, validCity))    .Returns(true);
            MockBuildingProductionValidityLogic.Setup(logic => logic.IsTemplateValidForCity(invalidTemplate, invalidCity)).Returns(false);

            Container.Resolve<CityProductionResolver>();

            CitySignals.CityGainedBuildingSignal.OnNext(new UniRx.Tuple<ICity, IBuilding>());

            Assert.AreEqual(validProject, validCity  .ActiveProject, "ValidCity has an unexpected ActiveProject");
            Assert.AreEqual(null,         invalidCity.ActiveProject, "InvalidCity has an unexpected ActiveProject");
            Assert.AreEqual(null,         nullCity   .ActiveProject, "NullCity has an unexpected ActiveProject");
        }

        [Test]
        public void OnRoundBeganFired_ResolveBuildingConstructionRequestsCalled() {
            var cityOne = BuildCity(null);

            BuildCiv(cityOne);

            var wonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            Mock<IProductionProject> projectOneMock;
            var projectOne = BuildBuildingProject(wonderTemplate, 20, out projectOneMock);

            var resolver = Container.Resolve<CityProductionResolver>();

            resolver.MakeProductionRequest(projectOne, cityOne);

            CoreSignals.RoundBegan.OnNext(10);

            projectOneMock.Verify(project => project.Execute(cityOne), Times.Once);
        }

        #endregion

        #region utilities

        private ICity BuildCity(IProductionProject activeProject) {
            var mockCity = new Mock<ICity>();

            mockCity.SetupAllProperties();

            var newCity = mockCity.Object;

            newCity.ActiveProject = activeProject;

            AllCities.Add(newCity);

            return newCity;
        }

        private IProductionProject BuildUnitProject(out Mock<IProductionProject> mock) {
            mock = new Mock<IProductionProject>();

            var unitTemplate = new Mock<IUnitTemplate>().Object;

            mock.Setup(project => project.UnitToConstruct).Returns(unitTemplate);

            return mock.Object;
        }

        private IProductionProject BuildBuildingProject(
            IBuildingTemplate template, int progress, out Mock<IProductionProject> mock
        ) {
            mock = new Mock<IProductionProject>();

            mock.Setup(project => project.BuildingToConstruct).Returns(template);
            mock.Setup(project => project.Progress)           .Returns(progress);

            return mock.Object;
        }

        private IBuildingTemplate BuildTemplate(BuildingType type) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
