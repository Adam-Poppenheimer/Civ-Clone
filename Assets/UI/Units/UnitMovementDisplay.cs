using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.GameMap;

using Assets.UI.GameMap;

namespace Assets.UI.Units {

    public class UnitMovementDisplay : UnitDisplayBase {

        #region instance fields and properties 

        private IDisposable UnitEndDragSubscription;
        private IDisposable MapTilePointerEnterSubscription;
        private IDisposable MapTilePointerExitSubscription;

        public IMapTile ProspectiveTravelGoal { get; private set; }

        public List<IMapTile> ProspectivePath { get; private set; }

        private UnitSignals           UnitSignals;
        private MapTileSignals        MapTileSignals;
        private ITilePathDrawer       PathDrawer;
        private IMapHexGrid           Map;
        private IUnitPositionCanon    PositionCanon;
        private IUnitTerrainCostLogic TerrainCostLogic;        

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitSignals signals, MapTileSignals mapTileSignals,
            ITilePathDrawer pathDrawer, IMapHexGrid map, IUnitPositionCanon positionCanon,
            IUnitTerrainCostLogic terrainCostLogic
        ){
            UnitSignals      = signals;
            PathDrawer       = pathDrawer;
            Map              = map;
            PositionCanon    = positionCanon;
            MapTileSignals   = mapTileSignals;
            TerrainCostLogic = terrainCostLogic;
        }

        #region Unity message methods

        public void OnEnable() {
            UnitEndDragSubscription = UnitSignals.UnitEndDragSignal.Subscribe(OnUnitEndDragFired);

            MapTileSignals.PointerEnterSignal.Listen(OnTilePointerEnterFired);
            MapTileSignals.PointerExitSignal .Listen(OnTilePointerExitFired);
        }

        public void OnDisable() {
            UnitEndDragSubscription.Dispose();

            MapTileSignals.PointerEnterSignal.Unlisten(OnTilePointerEnterFired);
            MapTileSignals.PointerExitSignal .Unlisten(OnTilePointerExitFired);
        }

        #endregion

        private void OnUnitEndDragFired(Tuple<IUnit, PointerEventData> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                PathDrawer.ClearAllPaths();              

                ObjectToDisplay.CurrentPath = ProspectivePath;
                ObjectToDisplay.PerformMovement();

                ProspectiveTravelGoal = null;
                ProspectivePath = null;
            }
        }

        private void OnTilePointerEnterFired(IMapTile tile, PointerEventData eventData) {
            if(eventData.dragging && eventData.pointerDrag == ObjectToDisplay.gameObject) {
                ProspectiveTravelGoal = tile;

                var unitLocation = PositionCanon.GetOwnerOfPossession(ObjectToDisplay);

                if(unitLocation == ProspectiveTravelGoal) {
                    ProspectivePath = null;
                }else {
                    ProspectivePath = Map.GetShortestPathBetween(
                        unitLocation, ProspectiveTravelGoal, 
                        tileInMap => TerrainCostLogic.GetCostToMoveUnitIntoTile(ObjectToDisplay, tileInMap)
                    );
                }                

                PathDrawer.ClearAllPaths();

                if(ProspectivePath != null) {                    
                    PathDrawer.DrawPath(ProspectivePath);
                }
            }
        }

        private void OnTilePointerExitFired(IMapTile tile, PointerEventData eventData) {
            if(eventData.dragging && eventData.pointerDrag == ObjectToDisplay.gameObject) {
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
