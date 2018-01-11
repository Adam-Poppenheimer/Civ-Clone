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
using Assets.Simulation.HexMap;
using Assets.Simulation.Core;

using Assets.UI.Units;
using Assets.UI.HexMap;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class UnitMovementDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITilePathDrawer> MockPathDrawer;

        private Mock<IHexGrid> MockGrid;

        private Mock<IUnitPositionCanon> MockPositionCanon;

        private HexCellSignals TileSignals;

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPathDrawer    = new Mock<ITilePathDrawer>();
            MockGrid           = new Mock<IHexGrid>();
            MockPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<ITilePathDrawer>()      .FromInstance(MockPathDrawer.Object);
            Container.Bind<IHexGrid>()             .FromInstance(MockGrid.Object);
            Container.Bind<IUnitPositionCanon>()   .FromInstance(MockPositionCanon.Object);
            Container.Bind<IUnitTerrainCostLogic>().FromMock();
            
            UnitSignals = new UnitSignals();
            Container.Bind<UnitSignals>().FromInstance(UnitSignals);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CellClickedSignal>();
            Container.DeclareSignal<CellPointerEnterSignal>();
            Container.DeclareSignal<CellPointerExitSignal>();

            Container.Bind<HexCellSignals>().AsSingle();

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

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile);

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

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile);

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

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            var executionSequence = new MockSequence();

            MockPathDrawer.InSequence(executionSequence).Setup(drawer => drawer.ClearPath());
            MockPathDrawer.InSequence(executionSequence).Setup(drawer => drawer.DrawPath(pathBetween));

            tileEnterSignal.Fire(enteredTile);

            MockPathDrawer.VerifyAll();
            MockPathDrawer.ResetCalls();

            MockGrid.Setup(map => map.GetShortestPathBetween(unitLocation, enteredTile, It.IsAny<Func<IHexCell, IHexCell, int>>()))
                .Returns(null as List<IHexCell>);

            tileEnterSignal.Fire(enteredTile);

            MockPathDrawer.Verify(drawer => drawer.ClearPath(), Times.Once,
                "PathDrawer.ClearAllPaths was not called when no valid path existed between the tiles");

            MockPathDrawer.Verify(drawer => drawer.DrawPath(It.IsAny<List<IHexCell>>()), Times.Never,
                "PathDrawer.DrawPath was incorrectly called when no valid path existed between the tiles");
        }

        [Test(Description = "When MapTileSignals.PointerEnterSignal fires, UnitMovementDisplay " +
            "shouldn't do anything significant unless it's in the middle of a drag event involving " +
            "its ObjectToDisplay")]
        public void TilePointerEnterFired_DoesNothingIfNotDraggingUnit() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile);

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "MovementDisplay.ProspectiveTravelGoal is not null");
            Assert.Null(movementDisplay.ProspectivePath,       "MovementDisplay.ProspectivePath is not null");

            MockPathDrawer.Verify(drawer => drawer.ClearPath(), Times.Never,
                "PathDrawer.ClearAllPaths was incorrectly called");

            MockPathDrawer.Verify(drawer => drawer.DrawPath(It.IsAny<List<IHexCell>>()), Times.Never,
                "PathDrawer.DrawPath was incorrectly called");
        }

        [Test(Description = "When MapTileSignals.TilePointerEntered fires on the same tile " +
            "where ObjectToDisplay is located, ProspectivePath should be set to null")]
        public void TilePointerEnteredFired_NullsPathOnSameTile() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile);
            tileEnterSignal.Fire(unitLocation);

            Assert.Null(movementDisplay.ProspectivePath, "ProspectivePath is unexpectedly non-null");
        }

        [Test(Description = "When MapTileSignals.TilePointerEntered fires on a tile that " +
            "is not a valid location for ObjectToDisplay, ProspectivePath should be set to null")]
        public void TilePointerEnterFired_NullPathOnInvalidLocation() {
            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var enteredTile = BuildTile();

            BuildPath(unitLocation, enteredTile, 3);

            var movementDisplay = Container.Resolve<UnitMovementDisplay>();
            movementDisplay.OnEnable();
            movementDisplay.ObjectToDisplay = unit;

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            var tileEnterSignal = Container.Resolve<CellPointerEnterSignal>();

            tileEnterSignal.Fire(enteredTile);

            MockPositionCanon.Setup(canon => canon.CanPlaceUnitOfTypeAtLocation(unit.Template.Type, enteredTile)).Returns(false);

            tileEnterSignal.Fire(enteredTile);

            Assert.Null(movementDisplay.ProspectivePath, "ProspectivePath is unexpectedly non-null");
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

            var eventData = new PointerEventData(EventSystem.current) { pointerDrag = unit.gameObject };

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            Container.Resolve<CellPointerEnterSignal>().Fire(enteredTile);

            MockPathDrawer.ResetCalls();

            Container.Resolve<CellPointerExitSignal>().Fire(enteredTile);

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "ProspectiveTravelGoal is not null");
            Assert.Null(movementDisplay.ProspectivePath,       "ProspectivePath is not null");

            MockPathDrawer.Verify(drawer => drawer.ClearPath(), Times.Once,
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

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            Container.Resolve<CellPointerEnterSignal>().Fire(enteredTile);

            MockPathDrawer.ResetCalls();

            UnitSignals.UnitEndDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unit, eventData));

            MockPathDrawer.Verify(drawer => drawer.ClearPath(), Times.Once, "PathDrawer.ClearAllPaths was not called");
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

            UnitSignals.UnitBeginDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unitMock.Object, eventData));

            Container.Resolve<CellPointerEnterSignal>().Fire(enteredTile);

            UnitSignals.UnitEndDragSignal.OnNext(new Tuple<IUnit, PointerEventData>(unitMock.Object, eventData));

            unitMock.VerifyAll();

            Assert.Null(movementDisplay.ProspectiveTravelGoal, "MovementDisplay.ProspectiveTravelGoal was not nulled out");
            Assert.Null(movementDisplay.ProspectivePath,       "MovementDisplay.ProspectivePath was not nulled out");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(IHexCell location) {
            return BuildMockUnit(location).Object;            
        }

        private Mock<IUnit> BuildMockUnit(IHexCell location, UnitType type = UnitType.LandMilitary) {
            var mockUnit = new Mock<IUnit>();
            mockUnit.Setup(unit => unit.gameObject).Returns(new GameObject());

            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.Type).Returns(type);

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            MockPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);
            MockPositionCanon.Setup(canon => canon.CanPlaceUnitOfTypeAtLocation(type, It.IsAny<IHexCell>())).Returns(true);

            return mockUnit;
        }

        private IHexCell BuildTile() {
            return new Mock<IHexCell>().Object;
        }

        private List<IHexCell> BuildPath(IHexCell start, IHexCell end, int interveningTileCount) {
            var newPath = new List<IHexCell>();

            newPath.Add(start);
            for(int i = 0; i < interveningTileCount; ++i) {
                newPath.Add(BuildTile());
            }
            newPath.Add(end);

            MockGrid.Setup(map => map.GetShortestPathBetween(start, end, It.IsAny<Func<IHexCell, IHexCell, int>>())).Returns(newPath);

            return newPath;
        }

        #endregion

        #endregion

    }

}
