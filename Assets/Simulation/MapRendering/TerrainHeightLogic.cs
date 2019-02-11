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
        private IHeightMixingLogic     HeightMixingLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(
            IHexGrid grid, IPointOrientationLogic pointOrientationLogic,
            ICellHeightmapLogic cellHeightmapLogic, IHeightMixingLogic heightMixingLogic
        ) {
            Grid                  = grid;
            PointOrientationLogic = pointOrientationLogic;
            CellHeightmapLogic    = cellHeightmapLogic;
            HeightMixingLogic     = heightMixingLogic;
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
                    return HeightMixingLogic.GetMixForEdgeAtPoint(center, right, sextant, position);
                }

                if(orientation == PointOrientation.PreviousCorner) {
                    var left = Grid.GetNeighbor(center, sextant.Previous());

                    return HeightMixingLogic.GetMixForPreviousCornerAtPoint(center, left, right, sextant, position);
                }

                if(orientation == PointOrientation.NextCorner) {
                    var nextRight = Grid.GetNeighbor(center, sextant.Next());

                    return HeightMixingLogic.GetMixForNextCornerAtPoint(center, right, nextRight, sextant, position);
                }
            }

            return 0f;
        }

        #endregion

        #endregion
        
    }

}
