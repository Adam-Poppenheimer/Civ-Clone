using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainHeightLogic : ITerrainHeightLogic {

        #region instance fields and properties

        private IHexGrid               Grid;
        private IPointOrientationLogic PointOrientationLogic;
        private ICellHeightmapLogic    CellHeightmapLogic;
        private ITerrainMixingLogic    TerrainMixingLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(
            IHexGrid grid, IPointOrientationLogic pointOrientationLogic,
            ICellHeightmapLogic cellHeightmapLogic, ITerrainMixingLogic terrainMixingLogic
        ) {
            Grid                  = grid;
            PointOrientationLogic = pointOrientationLogic;
            CellHeightmapLogic    = cellHeightmapLogic;
            TerrainMixingLogic    = terrainMixingLogic;
        }

        #endregion

        #region instance methods

        #region from ITerrainHeightLogic

        public float GetHeightForPosition(Vector3 position) {
            if(Grid.HasCellAtLocation(position)) {
                var center = Grid.GetCellAtLocation(position);

                HexDirection sextant = PointOrientationLogic.GetSextantOfPointForCell(position, center);

                PointOrientation orientation = PointOrientationLogic.GetOrientationOfPointInCell(position, center, sextant);

                if(orientation == PointOrientation.Center) {
                    return CellHeightmapLogic.GetHeightForPositionForCell(position, center, sextant);
                }

                IHexCell right = Grid.GetNeighbor(center, sextant);

                if(orientation == PointOrientation.Edge) {
                    return TerrainMixingLogic.GetMixForEdgeAtPoint(
                        center, right, sextant, position, HeightSelector, (a, b) => a + b
                    );
                }

                if(orientation == PointOrientation.PreviousCorner) {
                    var left = Grid.GetNeighbor(center, sextant.Previous());

                    return TerrainMixingLogic.GetMixForPreviousCornerAtPoint(
                        center, left, right, sextant, position, HeightSelector, (a, b) => a + b
                    );
                }

                if(orientation == PointOrientation.NextCorner) {
                    var nextRight = Grid.GetNeighbor(center, sextant.Next());

                    return TerrainMixingLogic.GetMixForNextCornerAtPoint(
                        center, right, nextRight, sextant, position, HeightSelector, (a, b) => a + b
                    );
                }
            }

            return 0f;
        }

        private float HeightSelector(Vector3 position, IHexCell cell, HexDirection sextant, float weight) {
            return CellHeightmapLogic.GetHeightForPositionForCell(position, cell, sextant) * weight;
        }

        #endregion

        #endregion
        
    }

}
