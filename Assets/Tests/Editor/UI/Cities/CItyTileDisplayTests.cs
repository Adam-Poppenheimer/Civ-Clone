using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Core;

using Assets.UI.Cities;
using Assets.UI.Cities.Territory;

namespace Assets.Tests.UI.Cities {

    [TestFixture]
    public class CityTileDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITilePossessionCanon> MockPossessionCanon;

        private List<Mock<IMapTile>> AllTileMocks = new List<Mock<IMapTile>>();

        private List<Mock<IWorkerSlotDisplay>> AllSlotDisplayMocks = new List<Mock<IWorkerSlotDisplay>>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTileMocks.Clear();
            AllSlotDisplayMocks.Clear();

            MockPossessionCanon = new Mock<ITilePossessionCanon>();
            MockPossessionCanon
                .Setup(canon => canon.GetTilesOfCity(It.IsAny<ICity>()))
                .Returns(AllTileMocks.Select(mock => mock.Object));

            Container.Bind<ITilePossessionCanon>().FromInstance(MockPossessionCanon.Object);

            Container.BindFactory<IWorkerSlotDisplay, WorkerSlotDisplayFactory>().FromMethod(BuildSlotDisplay);

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<IObservable<ICity>>().WithId("Select Requested Signal").FromMock();
            Container.Bind<IObservable<ICity>>().WithId("Deselect Requested Signal").FromMock();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<ISubject<ICity>>().WithId("Select Requested Subject").To<Subject<ICity>>().AsSingle();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<CityTileDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and ObjectToDisplay is null, " +
            "no exceptions should be thrown and nothing significant should happen")]
        public void OnRefresh_DoesNothingOnNullCity() {
            var tileDisplay = Container.Resolve<CityTileDisplay>();

            Assert.DoesNotThrow(() => tileDisplay.Refresh(),
                "CityTileDisplay.Refresh incorrectly threw when ObjectToDisplay was null");

            Assert.AreEqual(0, AllSlotDisplayMocks.Count, "A nonzero number of slot displays were instantiated");
        }

        [Test(Description = "When Refresh is called and ObjectToDisplay is not null, " +
            "CityTileDisplay should instantiate a slot display for every tile in that " +
            "city that isn't suppressed. Each display should be passed the slot of a " +
            "single tile and refreshed")]
        public void OnRefresh_SlotDisplaysInstantiatedAndConfigured() {
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, true,  BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();

            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;
            tileDisplay.Refresh();

            var slotsOfDisplays = AllSlotDisplayMocks.Select(mock => mock.Object.SlotToDisplay);
            var slotsOfUnsuppressedTiles = AllTileMocks
                .Where(mock => !mock.Object.SuppressSlot)
                .Select(mock => mock.Object.WorkerSlot);

            CollectionAssert.AreEquivalent(slotsOfUnsuppressedTiles, slotsOfDisplays,
                "The collection of slot display mocks are not displaying the expected slots");

            foreach(var displayMock in AllSlotDisplayMocks) {
                Assert.IsTrue(displayMock.Object.gameObject.activeSelf,
                    "A slot display's GameObject was not activated");

                displayMock.Verify(display => display.Refresh(), Times.AtLeastOnce,
                    "A slot display was not refreshed at least once");
            }
        }

        [Test(Description = "When Refresh is called, all active worker slot displays " +
            "should be moved to the screen-space coordinates of the tiles whose slots " +
            "they're displaying")]
        public void OnRefresh_DisplaysPositionedOverTiles() {
            BuildTile(new Vector3( 10, 10, -10), false, BuildSlot());
            BuildTile(new Vector3(-20, 20,  20), false, BuildSlot());
            BuildTile(new Vector3( 30,-30,  30), false, BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();

            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;
            tileDisplay.Refresh();

            foreach(var displayMock in AllSlotDisplayMocks) {
                var tileContainingSlot = 
                    AllTileMocks.Select(mock => mock.Object)
                                .Where(tile => tile.WorkerSlot == displayMock.Object.SlotToDisplay)
                                .First();

                Assert.AreEqual(
                    Camera.main.WorldToScreenPoint(tileContainingSlot.transform.position),
                    displayMock.Object.gameObject.transform.position,
                    "A slot display is not at the screen space coordinates of the tile it's supposed to be representing"
                );
            }
        }

        [Test(Description = "When Refresh is called more than once, CityTileDisplay " +
            "should try to reuse as many previously instantiated slot displays as it can, " +
            "reconfiguring and refreshing old displays when possible")]
        public void OnRefresh_ReusesExistingDisplays() {
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();

            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;
            tileDisplay.Refresh();

            AllTileMocks.Clear();

            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            AllSlotDisplayMocks.ForEach(mock => mock.ResetCalls());

            tileDisplay.Refresh();

            Assert.AreEqual(4, AllSlotDisplayMocks.Count, "TileDisplay has instantiated " +
                "an unexpected number of slot displays");

            var slotsOfDisplays = AllSlotDisplayMocks.Select(mock => mock.Object.SlotToDisplay);
            var slotsOfUnsuppressedTiles = AllTileMocks
                .Where(mock => !mock.Object.SuppressSlot)
                .Select(mock => mock.Object.WorkerSlot);

            CollectionAssert.AreEquivalent(slotsOfUnsuppressedTiles, slotsOfDisplays,
                "The collection of slot display mocks are not displaying the expected slots");

            foreach(var displayMock in AllSlotDisplayMocks) {
                Assert.IsTrue(displayMock.Object.gameObject.activeSelf,
                    "A slot display's GameObject was not activated");

                displayMock.Verify(display => display.Refresh(), Times.AtLeastOnce,
                    "A slot display was not refreshed at least once");
            }
        }

        [Test(Description = "When Refresh is called more than once, CityTileDisplay " +
            "should deactivate any slot displays that aren't being used")]
        public void OnRefresh_UnusedDisplaysDeactivated() {
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();

            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;
            tileDisplay.Refresh();

            AllTileMocks.Clear();

            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            tileDisplay.Refresh();

            var activeDisplays = AllSlotDisplayMocks.Select(mock => mock.Object).Where(display => display.gameObject.activeSelf);

            Assert.AreEqual(2, activeDisplays.Count(), "There are an unexpected number of active slot displays");

            CollectionAssert.AreEquivalent(
                AllTileMocks.Select(mock => mock.Object.WorkerSlot),
                activeDisplays.Select(display => display.SlotToDisplay),
                "The active slot displays do not represent the slots of all tiles"
            );
        }

        [Test(Description = "When CityDistributionPerformedSignal fires and ObjectToDisplay " +
            "is null, no exception should be thrown and nothing significant should happen")]
        public void DistributionSignalFired_DoesNothingOnNullCity() {
            Container.Resolve<CityTileDisplay>();

            var distributionSignal = Container.Resolve<CityDistributionPerformedSignal>();

            Assert.DoesNotThrow(() => distributionSignal.Fire(null),
                "CityTileDisplay unexpectedly threw when CityDistributionPerformedSignal fired " +
                "and ObjectToDisplay was null");
        }

        [Test(Description = "When CityDistributionPerformedSignal fires, ObjectToDisplay " +
            "is not null, and the city the signal passes is equal to ObjecToDisplay, " + 
            "the display should refresh itself")]
        public void DistributionSignalFired_RefreshesOnSameCity() {
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();
            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;

            var distributionSignal = Container.Resolve<CityDistributionPerformedSignal>();
            distributionSignal.Fire(tileDisplay.ObjectToDisplay);

            Assert.AreEqual(3, AllSlotDisplayMocks.Count, 
                "CityTileDisplay failed to refresh itself after CityDistributionPerformedSignal fired");
        }

        [Test(Description = "When CityDistributionPerformedSignal fires and ObjectToDisplay " +
            "does not equal the signalled city, CityTileDisplay should not refresh")]
        public void DistributionSignalFired_DoesNotRefreshOnDifferentCity() {
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());
            BuildTile(Vector3.zero, false, BuildSlot());

            var tileDisplay = Container.Resolve<CityTileDisplay>();
            tileDisplay.ObjectToDisplay = new Mock<ICity>().Object;

            var distributionSignal = Container.Resolve<CityDistributionPerformedSignal>();
            distributionSignal.Fire(new Mock<ICity>().Object);

            Assert.AreEqual(0, AllSlotDisplayMocks.Count,
                "CityTileDisplay falsely refreshed itself after CityDistributionPerformedSignal fired");
        }

        #endregion

        #region utilities

        private Mock<IMapTile> BuildTile(Vector3 position, bool suppressSlot,
            IWorkerSlot slot) {
            var mockTile = new Mock<IMapTile>();

            var newTransform = new GameObject().transform;
            newTransform.position = position;

            mockTile.Setup(tile => tile.transform)   .Returns(newTransform);
            mockTile.Setup(tile => tile.SuppressSlot).Returns(suppressSlot);
            mockTile.Setup(tile => tile.WorkerSlot)  .Returns(slot);

            AllTileMocks.Add(mockTile);

            return mockTile;
        }

        private IWorkerSlot BuildSlot() {
            return new Mock<IWorkerSlot>().Object;
        }

        private IWorkerSlotDisplay BuildSlotDisplay(DiContainer container) {
            var mockDisplay = new Mock<IWorkerSlotDisplay>();

            mockDisplay.SetupAllProperties();
            mockDisplay.Setup(display => display.gameObject).Returns(new GameObject());

            AllSlotDisplayMocks.Add(mockDisplay);
            return mockDisplay.Object;
        }

        #endregion

        #endregion

    }

}
