using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class TilePathDrawer : ITilePathDrawer {

        #region instance fields and properties

        private PathIndicator.MemoryPool IndicatorPool;

        private List<PathIndicator> ActiveIndicators = new List<PathIndicator>();

        #endregion

        #region constructors

        [Inject]
        public TilePathDrawer(PathIndicator.MemoryPool indicatorFactory){
            IndicatorPool = indicatorFactory;
        }

        #endregion

        #region instance methods

        #region from ITilePathDrawer

        public void ClearAllPaths() {
            ActiveIndicators.ForEach(indicator => IndicatorPool.Despawn(indicator));
            ActiveIndicators.Clear();
        }

        public void DrawPath(List<IHexCell> path) {
            foreach(var tile in path) {
                var tileCanvas = tile.transform.GetComponentInChildren<Canvas>();
                if(tileCanvas == null) {
                    Debug.LogErrorFormat("Tile {0} lacks a child canvas for displaying UI indicators", tile);
                    return;
                }

                var indicatorForTile = IndicatorPool.Spawn();
                indicatorForTile.gameObject.SetActive(true);

                indicatorForTile.transform.SetParent(tileCanvas.transform, false);

                ActiveIndicators.Add(indicatorForTile);
            }
        }

        #endregion

        #endregion
        
    }

}
