using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using UnityCustomUtilities.Grids;

using Assets.Cities;
using Assets.GameMap.UI;

namespace Assets.GameMap {

    public class MapTile : MonoBehaviour, IMapTile, IPointerClickHandler {

        #region instance fields and properties

        #region from IMapTile

        public HexCoords Coords {
            get { return _coords; }
            set { _coords = value; }
        }
        [SerializeField, HideInInspector] private HexCoords _coords;

        public TerrainType Terrain {
            get { return _terrain; }
            set { _terrain = value; }
        }
        [SerializeField] private TerrainType _terrain;

        public TerrainShape Shape {
            get { return _shape; }
            set { _shape = value; }
        }
        [SerializeField] private TerrainShape _shape;

        public TerrainFeatureType Feature {
            get { return _feature; }
            set { _feature = value; }
        }
        [SerializeField] private TerrainFeatureType _feature;

        public IWorkerSlot WorkerSlot {
            get {
                if(_workerSlot == null) {
                    _workerSlot = new WorkerSlot(ResourceLogic.GetYieldOfTile(this));
                    _workerSlot.IsOccupiable = !TileConfig.UnoccupiableTerrains.Contains(Terrain);
                }
                return _workerSlot;
            }
        }
        private WorkerSlot _workerSlot;

        #endregion

        [SerializeField] private ITileConfig TileConfig;

        private ITileEventBroadcaster EventBroadcaster;

        private ITileResourceLogic ResourceLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITileConfig displayConfig, ITileEventBroadcaster eventBroadcaster,
            ITileResourceLogic resourceLogic) {
            TileConfig = displayConfig;
            EventBroadcaster = eventBroadcaster;

            ResourceLogic = resourceLogic;
        }

        #region Unity message methods

        private void OnValidate() {
            Refresh();
        }

        private void Start() {
            Refresh();
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
             EventBroadcaster.BroadcastTileClicked(this, eventData);
        }

        #endregion

        public void Refresh() {
            if(TileConfig != null) {
                var meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = TileConfig.GetTerrainMaterial(Terrain);
            }            
        }

        #endregion

    }

}
