using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingFactoryTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TemplateValidityCases {
            get {
                yield return new TestCaseData("Template", true, false).Returns(true)
                    .SetName("Template is valid and no building with it exists");

                yield return new TestCaseData("Template", true, true).Returns(false)
                    .SetName("Template is valid and a building with it exists");

                yield return new TestCaseData("Template", false, false).Returns(false)
                    .SetName("Template is invalid and no building with it exists");

                yield return new TestCaseData("Template", false, true).Returns(false)
                    .SetName("Template is invalid and a building with it exists");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockPossessionCanon;
        private Mock<IWorkerSlotFactory>                        MockWorkerSlotFactory;
        private CitySignals                                     CitySignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPossessionCanon   = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockWorkerSlotFactory = new Mock<IWorkerSlotFactory>();
            CitySignals           = new CitySignals();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockPossessionCanon  .Object);
            Container.Bind<IWorkerSlotFactory>                       ().FromInstance(MockWorkerSlotFactory.Object);
            Container.Bind<CitySignals>                              ().FromInstance(CitySignals);

            Container.Bind<BuildingFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a new building is created, it should be initialized with " +
            "the argued IBuildingTemplate")]
        public void BuildingCreated_InitializedWithTemplate() {
            var template = BuildTemplate("Template");

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            var newBuilding = factory.BuildBuilding(template, city);

            Assert.AreEqual(template, newBuilding.Template, "newBuilding has an unexpected template");
        }

        [Test(Description = "When a new building is created, BuildingFactory should call " +
            "into BuildingPossessionCanon and assign the new building to the argued city")]
        public void BuildingCreated_PlacedInCity() {
            var template = BuildTemplate("Template");

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            var newBuilding = factory.BuildBuilding(template, city);

            MockPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newBuilding, city),
                Times.AtLeastOnce, "BuildingFactory did not check BuildingPossessionCanon before " +
                "placing newBuilding into the argued city"
            );

            MockPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newBuilding, city),
                Times.Once, "BuildingFactory did not place newBuilding into the argued city as expected");
        }

        [Test(Description = "All methods should throw an ArgumentNullException on any null arguments")]
        public void AllMethods_ThrowOnNullArguments() {
            var template = BuildTemplate("Template");

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.BuildBuilding(null, city),
                "BuildBuilding failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.BuildBuilding(template, null),
                "BuildBuilding failed to throw on a null city argument");
        }

        [Test(Description = "If Create is called and BuildingPossessionCanon.CanChangeOwnerOfPossession" +
            "would return false, CityFactory should throw a BuildingConstructionException")]
        public void CreateCalled_ThrowsIfCityLocationInvalid() {
            var template = BuildTemplate("Template");

            ICity city = BuildCity();

            MockPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IBuilding>(), city)).Returns(false);

            var factory = Container.Resolve<BuildingFactory>();

            Assert.Throws<BuildingCreationException>(() => factory.BuildBuilding(template, city),
                "BuildingFactory.Create failed to throw as expected");
        }

        [Test]
        public void DestroyBuilding_BuildingPossessionSetToNull() {
            var building = BuildBuilding(BuildTemplate("Template"));

            var factory = Container.Resolve<BuildingFactory>();

            factory.DestroyBuilding(building);

            MockPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(building, null), Times.Once);
        }

        [Test]
        public void DestroyBuilding_RemovedFromAllBuildings() {
            var building = BuildBuilding(BuildTemplate("Template"));

            var factory = Container.Resolve<BuildingFactory>();

            factory.DestroyBuilding(building);

            CollectionAssert.DoesNotContain(factory.AllBuildings, building);
        }

        [Test]
        public void OnCityBeingDestroyed_AllBuildingsBelongingToCityDestroyed() {
            var factory = Container.Resolve<BuildingFactory>();

            var cityOne = BuildCity();
            var cityTwo = BuildCity();

            var buildingOne   = factory.BuildBuilding(BuildTemplate("Template One"),   cityOne);
            var buildingTwo   = factory.BuildBuilding(BuildTemplate("Template Two"),   cityOne);
            var buildingThree = factory.BuildBuilding(BuildTemplate("Template Three"), cityTwo);

            MockPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(cityOne))
                               .Returns(new IBuilding[] { buildingOne, buildingTwo });

            MockPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(cityTwo))
                               .Returns(new IBuilding[] { buildingThree });

            CitySignals.BeingDestroyed.OnNext(cityOne);

            CollectionAssert.DoesNotContain(factory.AllBuildings, buildingOne,   "BuildingOne not destroyed as expected");
            CollectionAssert.DoesNotContain(factory.AllBuildings, buildingTwo,   "BuildingTwo not destroyed as expected");
            CollectionAssert.Contains      (factory.AllBuildings, buildingThree, "BuildingThree unexpectedly destroyed");
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(string name) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.name).Returns(name);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(buildings.ToList().AsReadOnly());

            MockPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IBuilding>(), newCity))
                .Returns(true);

            return newCity;
        }

        #endregion

        #endregion

    }

}
