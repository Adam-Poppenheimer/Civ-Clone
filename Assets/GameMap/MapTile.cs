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
            get { throw new NotImplementedException(); }
        }

        #endregion

        [SerializeField] private TileConfig TileConfig;

        private ITileEventBroadcaster EventBroadcaster;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(TileConfig displayConfig, ITileEventBroadcaster eventBroadcaster) {
            TileConfig = displayConfig;
            EventBroadcaster = eventBroadcaster;
        }

        #region Unity message methods

        private void OnValidate() {
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
