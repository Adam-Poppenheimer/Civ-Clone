﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

using Assets.UI.HexMap;

namespace Assets.UI.Units {

    public class UnitMovementDisplay : UnitDisplayBase {

        #region instance fields and properties 
        private IDisposable UnitBeginDragSubscription;
        private IDisposable UnitEndDragSubscription;

        private IDisposable MapTilePointerEnterSubscription;
        private IDisposable MapTilePointerExitSubscription;

        public IHexCell ProspectiveTravelGoal { get; private set; }

        public List<IHexCell> ProspectivePath { get; private set; }

        private bool IsDragging;

        private UnitSignals           UnitSignals;
        private HexCellSignals        MapTileSignals;
        private ITilePathDrawer       PathDrawer;
        private IHexGrid              Grid;
        private IUnitPositionCanon    PositionCanon;
        private IUnitTerrainCostLogic TerrainCostLogic;        

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitSignals signals, HexCellSignals mapTileSignals,
            ITilePathDrawer pathDrawer, IHexGrid grid, IUnitPositionCanon positionCanon,
            IUnitTerrainCostLogic terrainCostLogic
        ){
            UnitSignals      = signals;
            PathDrawer       = pathDrawer;
            Grid             = grid;
            PositionCanon    = positionCanon;
            MapTileSignals   = mapTileSignals;
            TerrainCostLogic = terrainCostLogic;
        }

        #region Unity message methods

        protected override void DoOnEnable() {
            UnitBeginDragSubscription = UnitSignals.UnitBeginDragSignal.Subscribe(OnUnitBeginDragFired);
            UnitEndDragSubscription   = UnitSignals.UnitEndDragSignal  .Subscribe(OnUnitEndDragFired);

            MapTileSignals.PointerEnterSignal.Listen(OnTilePointerEnterFired);
            MapTileSignals.PointerExitSignal .Listen(OnTilePointerExitFired);
        }

        protected override void DoOnDisable() {
            UnitBeginDragSubscription.Dispose();
            UnitEndDragSubscription  .Dispose();

            MapTileSignals.PointerEnterSignal.Unlisten(OnTilePointerEnterFired);
            MapTileSignals.PointerExitSignal .Unlisten(OnTilePointerExitFired);
        }

        #endregion

        private void OnUnitBeginDragFired(Tuple<IUnit, PointerEventData> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Debug.Log("Unit Begin Drag");
                IsDragging = true;
            }
        }

        private void OnUnitEndDragFired(Tuple<IUnit, PointerEventData> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Debug.Log("Unit End Drag");
                PathDrawer.ClearAllPaths();              

                ObjectToDisplay.CurrentPath = ProspectivePath;
                ObjectToDisplay.PerformMovement();

                ProspectiveTravelGoal = null;
                ProspectivePath = null;

                IsDragging = false;
            }
        }

        private void OnTilePointerEnterFired(IHexCell tile) {
            if(IsDragging) {
                Debug.Log("Tile Pointer Enter");
                ProspectiveTravelGoal = tile;

                var unitLocation = PositionCanon.GetOwnerOfPossession(ObjectToDisplay);

                if( unitLocation == ProspectiveTravelGoal ||
                    !PositionCanon.CanPlaceUnitOfTypeAtLocation(ObjectToDisplay.Template.Type, ProspectiveTravelGoal)
                ){
                    ProspectivePath = null;
                }else {
                    ProspectivePath = Grid.GetShortestPathBetween(
                        unitLocation, ProspectiveTravelGoal, 
                        delegate(IHexCell tileInMap) {
                            if(PositionCanon.CanPlaceUnitOfTypeAtLocation(ObjectToDisplay.Template.Type, tileInMap)) {
                               return  TerrainCostLogic.GetCostToMoveUnitIntoTile(ObjectToDisplay, tileInMap);
                            }else {
                                return -1;
                            }
                        }
                    );
                }                

                PathDrawer.ClearAllPaths();

                if(ProspectivePath != null) {                    
                    PathDrawer.DrawPath(ProspectivePath);
                }
            }
        }

        private void OnTilePointerExitFired(IHexCell tile) {
            if(IsDragging) {
                Debug.Log("Tile Pointer Exit");
                if(ProspectiveTravelGoal == tile) {
                    ProspectiveTravelGoal = null;
                    ProspectivePath = null;
                    PathDrawer.ClearAllPaths();
                }
            }            
        }

        #endregion

    }

}
