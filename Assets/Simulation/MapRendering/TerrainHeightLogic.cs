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
        private IRiverCanon            RiverCanon;
        private ITerrainMixingLogic    TerrainMixingLogic;
        private IRiverbedHeightLogic   RiverbedHeightLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(
            IHexGrid grid, IPointOrientationLogic pointOrientationLogic, ICellHeightmapLogic cellHeightmapLogic,
            IRiverCanon riverCanon, ITerrainMixingLogic terrainMixingLogic, IRiverbedHeightLogic riverbedHeightLogic
        ) {
            Grid                  = grid;
            PointOrientationLogic = pointOrientationLogic;
            CellHeightmapLogic    = cellHeightmapLogic;
            RiverCanon            = riverCanon;
            TerrainMixingLogic    = terrainMixingLogic;
            RiverbedHeightLogic   = riverbedHeightLogic;
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

                bool hasCenterRightRiver = RiverCanon.HasRiverAlongEdge(center, sextant);

                if(orientation == PointOrientation.Edge) {
                    if(hasCenterRightRiver) {
                        return RiverbedHeightLogic.GetHeightForRiverEdgeAtPoint(center, right, sextant, position);
                    }else {
                        return TerrainMixingLogic.GetMixForEdgeAtPoint(
                            center, right, sextant, position, HeightSelector, (a, b) => a + b
                        );
                    }
                }

                if(orientation == PointOrientation.PreviousCorner) {
                    var left = Grid.GetNeighbor(center, sextant.Previous());

                    bool hasCenterLeftRiver = RiverCanon.HasRiverAlongEdge(center, sextant.Previous());
                    bool hasleftRightRiver  = RiverCanon.HasRiverAlongEdge(right,  sextant.Previous2());

                    if( hasCenterRightRiver || hasCenterLeftRiver || hasleftRightRiver) {

                        return RiverbedHeightLogic.GetHeightForRiverPreviousCornerAtPoint(
                            center, left, right, sextant, position, hasCenterRightRiver,
                            hasCenterLeftRiver, hasleftRightRiver
                        );

                    }else {
                        return TerrainMixingLogic.GetMixForPreviousCornerAtPoint(
                            center, left, right, sextant, position, HeightSelector, (a, b) => a + b
                        );
                    }
                }

                if(orientation == PointOrientation.NextCorner) {
                    var nextRight = Grid.GetNeighbor(center, sextant.Next());

                    bool hasCenterNextRightRiver = RiverCanon.HasRiverAlongEdge(center, sextant.Next());
                    bool hasRightNextRightRiver  = RiverCanon.HasRiverAlongEdge(right,  sextant.Next2());

                    if(hasCenterRightRiver || hasCenterNextRightRiver || hasRightNextRightRiver) {

                        return RiverbedHeightLogic.GetHeightForRiverNextCornerAtPoint(
                            center, right, nextRight, sextant, position, hasCenterRightRiver,
                            hasCenterNextRightRiver, hasRightNextRightRiver
                        );

                    }else {
                        return TerrainMixingLogic.GetMixForNextCornerAtPoint(
                            center, right, nextRight, sextant, position, HeightSelector, (a, b) => a + b
                        );
                    }
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
