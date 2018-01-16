using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private GameObject CityPrefab;

        private DiContainer CityInjectionContainer;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private Mock<IHexGrid> MockGrid;

        private Mock<IPossessionRelationship<ICity, IHexCell>> MockTilePossessionCanon;

        private Mock<IWorkerDistributionLogic> MockDistributionLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            CityPrefab = new GameObject("City Prefab");

            CityPrefab.AddComponent<City>();
            CityPrefab.transform.localPosition = new Vector3(1f, 2f, 3f);

            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(CityPrefab);

            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGrid                 = new Mock<IHexGrid>();
            MockTilePossessionCanon = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockDistributionLogic   = new Mock<IWorkerDistributionLogic>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>                         ().FromInstance(MockTilePossessionCanon.Object);
            Container.Bind<IWorkerDistributionLogic>                     ().FromInstance(MockDistributionLogic  .Object);

            Container.Bind<IPopulationGrowthLogic>   ().FromMock();
            Container.Bind<IProductionLogic>         ().FromMock();
            Container.Bind<IResourceGenerationLogic> ().FromMock();
            Container.Bind<IBorderExpansionLogic>    ().FromMock();
            
            Container.Bind<IProductionProjectFactory>().FromMock();

            Container.Bind<ISubject<ICity>>().WithId("City Clicked Subject").FromMock();

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<CityFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Create is called, the GameObject the new city is attached to " +
            "should have its transform parented under the argued location's transform. That city " +
            "should preserve the local position of its prefab")]
        public void CityCreated_ParentedToLocation_AndLocalPositionPreserved() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            var newCity = factory.Create(tile, civilization);

            Assert.AreEqual(tile.transform, newCity.transform.parent,
                "The new city's transform has an unexpected parent");

            Assert.AreEqual(CityPrefab.transform.localPosition, newCity.transform.localPosition,
                "The new city's transform has an unexpected local position");
        }

        [Test(Description = "Every city the factory creates should have a starting population of 1")]
        public void CityCreated_PopulationSetToOne() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            var newCity = factory.Create(tile, civilization);

            Assert.AreEqual(1, newCity.Population, "newCity.Population has an unexpected value");
        }

        [Test(Description = "Every city the factory creates should have its location initialized " +
            "properly. That location should have its SuppressSlot field set to true")]
        public void CityCreated_LocationSetAndSuppressed() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            var newCity = factory.Create(tile, civilization);

            Assert.AreEqual(tile, newCity.Location, "newCity.Location has an unexpected value");
            Assert.IsTrue(newCity.Location.SuppressSlot, "newCity.Location is not suppressed");
        }

        [Test(Description = "When Create is called, CityFactory should attempt to assign the " +
            "argued location and all neighboring tiles to the newly created city. It should not " +
            "throw an exception if any of the neighbors cannot be assigned")]
        public void CityCreated_LocationAndNeighborsAssignedIfPossible() { 
            var location = BuildTile(true);
            var civilization = BuildCivilization();

            var validNeighbors = BuildNeighborsFor(location, 3, true);

            var factory = Container.Resolve<CityFactory>();

            var cityWithValidNeighbors = factory.Create(location, civilization);

            MockTilePossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(location, cityWithValidNeighbors), Times.AtLeastOnce,
                "CityFactory failed to call TilePossessionCanon.CanChangeOwnerOfPossession on its location");

            MockTilePossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(location, cityWithValidNeighbors), Times.Once,
                "CityFactory failed to call TilePossessionCanon.ChangeOwnerOfPossession on its location");

            foreach(var neighbor in validNeighbors) {
                MockTilePossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(neighbor, cityWithValidNeighbors), Times.AtLeastOnce,
                "CityFactory failed to call TilePossessionCanon.CanChangeOwnerOfPossession on one of its neighbors");

                MockTilePossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(neighbor, cityWithValidNeighbors), Times.Once,
                    "CityFactory failed to call TilePossessionCanon.ChangeOwnerOfPossession on a valid neighbor");
            }

            var invalidNeighbors = BuildNeighborsFor(location, 3, false);
            
            var cityWithInvalidNeighbors = factory.Create(location, civilization);

            foreach(var neighbor in invalidNeighbors) {
                MockTilePossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(neighbor, cityWithInvalidNeighbors), Times.AtLeastOnce,
                "CityFactory failed to call TilePossessionCanon.CanChangeOwnerOfTile on one of its neighbors");

                MockTilePossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(neighbor, cityWithInvalidNeighbors), Times.Never,
                    "CityFactory unexpectedly called TilePossessionCanon.ChangeOwnerOfTile on and invalid neighbor");
            }
        }

        [Test(Description = "When Create is called, CityFactory should attempt to assign the " +
            "newly created city to the argued civilization. It should throw an exception if " + 
            "this is not possible")]
        public void CityCreated_AssignedToCivilization_AndThrowsIfImpossible() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            MockCityPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<ICity>(), civilization))
                .Returns(true);

            var nonExceptionalCity = factory.Create(tile, civilization);

            MockCityPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(nonExceptionalCity, civilization),
                Times.AtLeastOnce, "CityFactory did not check CityPossessionCanon on nonExceptionalCity as expected");

            MockCityPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(nonExceptionalCity, civilization),
                Times.Once, "CityFactory failed to assign nonExceptionalCity to the argued civilization");

            MockCityPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<ICity>(), civilization))
                .Returns(false);

            Assert.Throws<CityCreationException>(() => factory.Create(tile, civilization),
                "CityFactory.Create failed to throw when assignment of the " + 
                "newly created city to the argued civilization was impossible"
            );
        }

        [Test(Description = "When Create is called, the newly created city should have its " +
            "ResourceFocus initialized to ResourceFocusType.TotalYield, and should then have " +
            "its distribution performed")]
        public void CityCreated_FocusInitializedAndDistributionPerformed() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            var newCity = factory.Create(tile, civilization);

            Assert.AreEqual(ResourceFocusType.TotalYield, newCity.ResourceFocus, "newCity.ResourceFocus has an unexpected value");

            MockDistributionLogic.Verify(
                logic => logic.DistributeWorkersIntoSlots(1, It.IsAny<IEnumerable<IWorkerSlot>>(), newCity, newCity.ResourceFocus),
                Times.Once,
                "WorkerDistributionLogic.DistributeWorkersIntoSlots was not called as expected"
            );
        }

        [Test(Description = "When Create is called, the newly created city should appear in " +
            "the factory's AllCities collection")]
        public void CityCreated_AddedToAllCities() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            var newCity = factory.Create(tile, civilization);

            CollectionAssert.Contains(factory.AllCities, newCity, "AllCities does not contain the newly created factory");
        }

        [Test(Description = "Create should throw an ArgumentNullException on any null argument")]
        public void CreateCalled_ThrowsOnNullArguments() {
            var tile = BuildTile(true);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.Create(null, civilization),
                "CityFactory.Create failed to throw on a null location argument");

            Assert.Throws<ArgumentNullException>(() => factory.Create(tile, null),
                "CityFactory.Create failed to throw on a null owner argument");
        }

        [Test(Description = "When Create is called and CityFactory cannot assign the argued " +
            "location to the newly created city, it should throw a CityCreationException")]
        public void CreateCalled_ThrowsWhenLocationCannotBeAssigned() {
            var tile = BuildTile(false);
            var civilization = BuildCivilization();

            BuildNeighborsFor(tile, 1, true);

            var factory = Container.Resolve<CityFactory>();

            Assert.Throws<CityCreationException>(() => factory.Create(tile, civilization),
                "CityFactory.Create failed to throw on a tile on which a new city cannot be located");
        }

        #endregion

        #region utilities

        private IHexCell BuildTile(bool canBeAssigned) {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Setup(tile => tile.transform).Returns(new GameObject().transform);

            MockTilePossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(mockTile.Object, It.IsAny<ICity>()))
                .Returns(canBeAssigned);

            return mockTile.Object;
        }

        private ICivilization BuildCivilization() {
            var civilizationMock = new Mock<ICivilization>();
            
            MockCityPossessionCanon.Setup(
                canon => canon.CanChangeOwnerOfPossession(It.IsAny<ICity>(), civilizationMock.Object)
            ).Returns(true);

            return civilizationMock.Object;
        }

        private List<IHexCell> BuildNeighborsFor(IHexCell centeredTile, int neighborCount, bool canBeAssigned) {
            var retval = new List<IHexCell>();

            for(int i = 0; i < neighborCount; ++i) {
                retval.Add(BuildTile(canBeAssigned));
            }

            MockGrid.Setup(map => map.GetNeighbors(centeredTile)).Returns(retval);

            return retval;
        }

        #endregion

        #endregion

    }

}
