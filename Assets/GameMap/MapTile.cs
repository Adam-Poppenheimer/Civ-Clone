using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Grids;

namespace Assets.GameMap {

    public class MapTile : MonoBehaviour, IMapTile {

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

        [SerializeField] private TileConfig TileConfig;

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(TileConfig displayConfig) {
            TileConfig = displayConfig;
        }

        #region Unity message methods

        private void OnValidate() {
            Refresh();
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
