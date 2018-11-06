using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ProductionProjectTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBuildingFactory>                              MockBuildingFactory;
        private Mock<IUnitFactory>                                  MockUnitFactory;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IStartingExperienceLogic>                      MockStartingExperienceLogic;

        private List<IUnit> AllUnits = new List<IUnit>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllUnits.Clear();

            MockBuildingFactory         = new Mock<IBuildingFactory>();
            MockUnitFactory             = new Mock<IUnitFactory>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockStartingExperienceLogic = new Mock<IStartingExperienceLogic>();

            MockUnitFactory.Setup(factory => factory.BuildUnit(
                It.IsAny<IHexCell>(), It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>()
            )).Returns<IHexCell, IUnitTemplate, ICivilization>(
                (location, template, owner) => BuildUnit()
            );
        }

        #endregion

        #region tests

        [Test]
        public void Name_IsNameOfBuildingIfBuildingNotNull() {
            var buildingTemplate = BuildBuildingTemplate("Building Template One", 10);

            var projectToTest = BuildProductionProject(buildingTemplate);

            Assert.AreEqual(buildingTemplate.name, projectToTest.Name);
        }

        [Test]
        public void Name_IsNameOfUnitIfBuildingNull() {
            var unitTemplate = BuildUnitTemplate("Unit Template One", 200);

            var projectToTest = BuildProductionProject(unitTemplate);

            Assert.AreEqual(unitTemplate.name, projectToTest.Name);
        }

        [Test]
        public void ProductionToComplete_IsCostOfBuildingIfBuildingNotNull() {
            var buildingTemplate = BuildBuildingTemplate("Building Template One", 10);

            var projectToTest = BuildProductionProject(buildingTemplate);

            Assert.AreEqual(buildingTemplate.ProductionCost, projectToTest.ProductionToComplete);
        }

        [Test]
        public void ProductionToComplete_IsCostOfUnitIfBuildingNull() {
            var unitTemplate = BuildUnitTemplate("Unit Template One", 200);

            var projectToTest = BuildProductionProject(unitTemplate);

            Assert.AreEqual(unitTemplate.ProductionCost, projectToTest.ProductionToComplete);
        }

        [Test]
        public void Execute_AndIsBuilding_PassesCorrectArgumentsToBuildingFactory() {
            var buildingTemplate = BuildBuildingTemplate("Building Template One", 10);

            var city = BuildCity(BuildCivilization(), BuildCell());

            var projectToTest = BuildProductionProject(buildingTemplate);

           projectToTest.Execute(city);

            MockBuildingFactory.Verify(factory => factory.BuildBuilding(buildingTemplate, city), Times.Once);
        }

        [Test]
        public void Execute_AndIsUnit_PassesCorrectArgumentsToUnitFactory() {
            var unitTemplate = BuildUnitTemplate("Unit Template One", 200);

            var owner    = BuildCivilization();
            var location = BuildCell();            

            var city = BuildCity(owner, location);

            var projectToTest = BuildProductionProject(unitTemplate);

            projectToTest.Execute(city);

            MockUnitFactory.Verify(factory => factory.BuildUnit(location, unitTemplate, owner), Times.Once);
        }

        [Test]
        public void Execute_AndIsUnit_SetsExperienceBasedOnStartingExperienceLogic() {
            var unitTemplate = BuildUnitTemplate("Unit Template One", 200);          

            var city = BuildCity(BuildCivilization(), BuildCell());

            MockStartingExperienceLogic.Setup(
                logic => logic.GetStartingExperienceForUnit(It.IsAny<IUnit>(), city)
            ).Returns(50);

            var projectToTest = BuildProductionProject(unitTemplate);

            projectToTest.Execute(city);

            Assert.AreEqual(50, AllUnits[0].Experience);
        }

        #endregion

        #region utilities

        private ProductionProject BuildProductionProject(IBuildingTemplate buildingToConstruct) {
            return new ProductionProject(buildingToConstruct, MockBuildingFactory.Object);
        }

        private ProductionProject BuildProductionProject(IUnitTemplate unitToConstruct) {
            return new ProductionProject(
                unitToConstruct, MockUnitFactory.Object, MockCityPossessionCanon.Object,
                MockCityLocationCanon.Object, MockStartingExperienceLogic.Object
            );
        }

        private IBuildingTemplate BuildBuildingTemplate(string name, int productionCost) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.name)          .Returns(name);
            mockTemplate.Setup(template => template.ProductionCost).Returns(productionCost);

            return mockTemplate.Object;
        }

        private IUnitTemplate BuildUnitTemplate(string name, int productionCost) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.name)          .Returns(name);
            mockTemplate.Setup(template => template.ProductionCost).Returns(productionCost);

            return mockTemplate.Object;
        }

        private ICity BuildCity(ICivilization owner, IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);
            MockCityLocationCanon  .Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit() {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            var newUnit = mockUnit.Object;

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
