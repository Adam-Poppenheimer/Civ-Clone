using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.GameMap;
using Assets.Simulation.Core;

using Assets.UI.Units;
using Assets.UI.GameMap;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class UnitMovementDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITilePathDrawer> MockPathDrawer;

        private Mock<IMapHexGrid> MockMap;

        private Mock<IUnitPositionCanon> MockPositionCanon;

        private MapTileSignals TileSignals;

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPathDrawer    = new Mock<ITilePathDrawer>();
            MockMap           = new Mock<IMapHexGrid>();
            MockPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<ITilePathDrawer>()      .FromInstance(MockPathDrawer.Object);
            Container.Bind<IMapHexGrid>()          .FromInstance(MockMap.Object);
            Container.Bind<IUnitPositionCanon>()   .FromInstance(MockPositionCanon.Object);
            Container.Bind<IUnitTerrainCostLogic>().FromMock();

            Container.Bind<ISubject<IUnit>>                         ().WithId("Unit Clicked Signal"   ).FromMock();
            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>().WithId("Unit Begin Drag Signal").FromMock();
            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>().WithId("Unit Drag Signal"      ).FromMock();

            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>()
                .WithId("Unit End Drag Signal")
                .To<Subject<Tuple<IUnit, PointerEventData>>>()
                .AsSingle();
            
            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TileClickedSignal>();
            Container.DeclareSignal<TilePointerEnterSignal>();
            Container.DeclareSignal<TilePointerExitSignal>();

            Container.Bind<MapTileSignals>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<UnitMovementDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When MapTileSignals.PointerEnterSignal fires, UnitMovementDisplay " +
            "should set ProspectiveTravelGoal to the map just entered")]
        public void TilePointerEnterFired_ProspectiveTravelGoalChanged() {
            var unit = BuildUnit(BuildTile());

            var enteredTile = BuildTile();

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unit.gameObject };

            var tileEnterSignal = Container.Resolve<TilePointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile, eventData);

            Assert.AreEqual(enteredTile, movementDisplay.ProspectiveTravelGoal,
                "MovementDisplay.ProspectiveTravelGoal has an unexpected value");
        }

        [Test(Description = "When MapTileSignals.PointerEnterSignal fires, UnitMovementDisplay " +
            "should call IMapHexGrid.GetShortestPathTo to set ProspectivePath")]
        public void TilePointerEnterFired_ProspectivePathChanged() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            var pathBetween = BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unit.gameObject };

            var tileEnterSignal = Container.Resolve<TilePointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile, eventData);

            CollectionAssert.AreEqual(pathBetween, movementDisplay.ProspectivePath,
                "MovementDisplay.ProspectivePath has an unexpected value");
        }

        [Test(Description = "When MapTileSignals.PointerEnterSignal fires and ProspectivePath " +
            "is given a non-null value, UnitMovementDisplay should call PathDrawer.ClearAllPaths() " +
            "followed by PathDrawer.DrawPath on ProspectivePath")]
        public void TilePointerEnterFired_DrawsPathIfOneExists() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();            

            var pathBetween = BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unit.gameObject };

            var tileEnterSignal = Container.Resolve<TilePointerEnterSignal>();

            var executionSequence = new MockSequence();

            MockPathDrawer.InSequence(executionSequence).Setup(drawer => drawer.ClearAllPaths());
            MockPathDrawer.InSequence(executionSequence).Setup(drawer => drawer.DrawPath(pathBetween));

            tileEnterSignal.Fire(enteredTile, eventData);

            MockPathDrawer.VerifyAll();
            MockPathDrawer.ResetCalls();

            MockMap.Setup(map => map.GetShortestPathBetween(unitLocation, enteredTile, It.IsAny<Func<IMapTile, int>>()))
                .Returns(null as List<IMapTile>);

            tileEnterSignal.Fire(enteredTile, eventData);

            MockPathDrawer.Verify(drawer => drawer.ClearAllPaths(), Times.Once,
                "PathDrawer.ClearAllPaths was not called when no valid path existed between the tiles");

            MockPathDrawer.Verify(drawer => drawer.DrawPath(It.IsAny<List<IMapTile>>()), Times.Never,
                "PathDrawer.DrawPath was incorrectly called when no valid path existed between the tiles");
        }

        [Test(Description = "When MapTileSignals.PointerEnterSignal fires, UnitMovementDisplay " +
            "shouldn't do anything significant unless the argued eventData object records an " +
            "active drag on ObjectToDisplay.gameObject")]
        public void TilePointerEnterFired_DoesNothingIfNotDraggingUnit() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true };

            var tileEnterSignal = Container.Resolve<TilePointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile, eventData);

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "MovementDisplay.ProspectiveTravelGoal is not null");
            Assert.Null(movementDisplay.ProspectivePath,       "MovementDisplay.ProspectivePath is not null");

            MockPathDrawer.Verify(drawer => drawer.ClearAllPaths(), Times.Never,
                "PathDrawer.ClearAllPaths was incorrectly called");

            MockPathDrawer.Verify(drawer => drawer.DrawPath(It.IsAny<List<IMapTile>>()), Times.Never,
                "PathDrawer.DrawPath was incorrectly called");
        }

        [Test(Description = "")]
        public void TilePointerEnteredFired_DoesSmartThingsOnSameTile() {
            throw new NotImplementedException();
        }

        [Test(Description = "When MapTileSignals.PointerExitSignal fires, UnitMovementDisplay " +
            "should null ProspectiveTravelGoal, ProspectivePath, and call PathDrawer.ClearAllPaths()")]
        public void TilePointerExitFired_EverythingCleared() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unit.gameObject };

            Container.Resolve<TilePointerEnterSignal>().Fire(enteredTile, eventData);

            MockPathDrawer.ResetCalls();

            Container.Resolve<TilePointerExitSignal>().Fire(enteredTile, eventData);

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "ProspectiveTravelGoal is not null");
            Assert.Null(movementDisplay.ProspectivePath,       "ProspectivePath is not null");

            MockPathDrawer.Verify(drawer => drawer.ClearAllPaths(), Times.Once,
                "PathDrawer.ClearAllPaths was not called exactly once");
        }

        [Test(Description = "When UnitSignals.UnitEndDragSignal fires, UnitMovementDisplay " +
            "call PathDrawer.ClearAllPaths")]
        public void UnitEndDragFired_PathDrawerCleared() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unit.gameObject };

            Container.Resolve<TilePointerEnterSignal>().Fire(enteredTile, eventData);

            MockPathDrawer.ResetCalls();

            Container.ResolveId<ISubject<Tuple<IUnit, PointerEventData>>>("Unit End Drag Signal")
                .OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            MockPathDrawer.Verify(drawer => drawer.ClearAllPaths(), Times.Once, "PathDrawer.ClearAllPaths was not called");
        }


        [Test(Description = "When UnitSignals.UnitEndDragSignal fires, UnitMovementDisplay " +
            "should assign ProspectivePath to ObjectToDisplay and call ObjectToDisplay.PerformMovement." +
            "After doing that, it should set ProspectiveTravelGoal and ProspectivePath to null")]
        public void UnitEndDragFired_SetsUnitPath_PerformsMovement_AndNullsProspects() {
            var unitLocation = BuildTile();
            var unitMock = BuildMockUnit(unitLocation);

            var enteredTile = BuildTile();

            var pathBetween = BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unitMock.Object;

            var executionSequence = new MockSequence();

            unitMock.InSequence(executionSequence).SetupSet(unit => unit.CurrentPath = pathBetween).Verifiable();
            unitMock.InSequence(executionSequence).Setup(unit => unit.PerformMovement());

            var eventData = new PointerEventData(EventSystem.current) { dragging = true, pointerDrag = unitMock.Object.gameObject };

            Container.Resolve<TilePointerEnterSignal>().Fire(enteredTile, eventData);

            Container.ResolveId<ISubject<Tuple<IUnit, PointerEventData>>>("Unit End Drag Signal")
                .OnNext(new Tuple<IUnit, PointerEventData>(unitMock.Object, eventData));

            unitMock.VerifyAll();

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "MovementDisplay.ProspectiveTravelGoal was not nulled out");
            Assert.Null(movementDisplay.ProspectivePath,       "MovementDisplay.ProspectivePath was not nulled out");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(IMapTile location) {
            return BuildMockUnit(location).Object;            
        }

        private Mock<IUnit> BuildMockUnit(IMapTile location) {
            var mockUnit = new Mock<IUnit>();
            mockUnit.Setup(unit => unit.gameObject).Returns(new GameObject());

            MockPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);

            return mockUnit;
        }

        private IMapTile BuildTile() {
            return new Mock<IMapTile>().Object;
        }

        private List<IMapTile> BuildPath(IMapTile start, IMapTile end, int interveningTileCount) {
            var newPath = new List<IMapTile>();

            newPath.Add(start);
            for(int i = 0; i < interveningTileCount; ++i) {
                newPath.Add(BuildTile());
            }
            newPath.Add(end);

            MockMap.Setup(map => map.GetShortestPathBetween(start, end, It.IsAny<Func<IMapTile, int>>())).Returns(newPath);

            return newPath;
        }

        #endregion

        #endregion

    }

}
