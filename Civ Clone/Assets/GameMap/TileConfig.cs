using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.GameMap {

    [CreateAssetMenu(fileName = "New Tile Display Config", menuName = "Civ Clone/Tile Display Config")]
    public class TileConfig : ScriptableObject {

        #region instance fields and properties

        public Material GrasslandMaterial {
            get { return _grasslandMaterial; }
        }
        [SerializeField] private Material _grasslandMaterial;

        public Material PlainsMaterial {
            get { return _plainsMaterial; }
        }
        [SerializeField] private Material _plainsMaterial;

        public Material DesertMaterial {
            get { return _desertMaterial; }
        }
        [SerializeField] private Material _desertMaterial;

        #endregion

        #region instance methods

        public Material GetTerrainMaterial(TerrainType terrain) {
            switch(terrain) {
                case TerrainType.Grassland: return GrasslandMaterial;
                case TerrainType.Plains: return PlainsMaterial;
                case TerrainType.Desert: return DesertMaterial;
                default: return null;
            }
        }

        #endregion

    }

}
