using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationInSextantLogic : IPointOrientationInSextantLogic {

        #region instance fields and properties

        private IHexGrid                     Grid;
        private ICellEdgeContourCanon        CellEdgeContourCanon;
        private IRiverCanon                  RiverCanon;
        private IGeometry2D                  Geometry2D;
        private IPointOrientationWeightLogic PointOrientationWeightLogic;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationInSextantLogic(
            IHexGrid grid, ICellEdgeContourCanon cellEdgeContourCanon, IRiverCanon riverCanon,
            IGeometry2D geometry2D, IPointOrientationWeightLogic pointOrientationWeightLogic
        ) {
            Grid                        = grid;
            CellEdgeContourCanon        = cellEdgeContourCanon;
            RiverCanon                  = riverCanon;
            Geometry2D                  = geometry2D;
            PointOrientationWeightLogic = pointOrientationWeightLogic;
        }

        #endregion

        #region instance methods

        #region from IPointOrientationInSextantLogic

        public bool TryFindValidOrientation(Vector2 xzPoint, IHexCell gridCenter, HexDirection gridSextant, out PointOrientationData data) {
            data = new PointOrientationData() {
                IsOnGrid = true,
                Sextant  = gridSextant,

                Center    = gridCenter,
                Left      = Grid.HasNeighbor(gridCenter, gridSextant.Previous()) ? Grid.GetNeighbor(gridCenter, gridSextant.Previous()) : null,
                Right     = Grid.HasNeighbor(gridCenter, gridSextant)            ? Grid.GetNeighbor(gridCenter, gridSextant)            : null,
                NextRight = Grid.HasNeighbor(gridCenter, gridSextant.Next())     ? Grid.GetNeighbor(gridCenter, gridSextant.Next())     : null,
            };

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, gridCenter, gridSextant)) {
                if(RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)) {
                    PointOrientationWeightLogic.ApplyLandBesideRiverWeights(xzPoint, data, false);

                }else {
                    PointOrientationWeightLogic.ApplyLandBesideLandWeights(xzPoint, data);
                }

                return true;
            }

            if(data.Right == null) {
                return false;
            }

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, data.Right, gridSextant.Opposite())) {
                if(RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)) {
                    PointOrientationWeightLogic.ApplyLandBesideRiverWeights(xzPoint, data, true);

                }else {
                    PointOrientationWeightLogic.ApplyLandBesideLandWeights(xzPoint, data);
                }

                return true;
            }

            var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(gridCenter, gridSextant);
            var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(data.Right, gridSextant.Opposite());

            if( CellEdgeContourCanon.IsPointBetweenContours(xzPoint, centerRightContour, rightCenterContour) &&
                RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)
            ) {
                PointOrientationWeightLogic.ApplyRiverWeights(xzPoint, data);

                return true;
            }

            if(data.Left != null) {
                bool previousIsConfluence = RiverCanon.HasRiverAlongEdge(data.Center, gridSextant.Previous())
                                         && RiverCanon.HasRiverAlongEdge(data.Center, gridSextant)
                                         && RiverCanon.HasRiverAlongEdge(data.Left,   gridSextant.Next());

                var leftCenterContour = CellEdgeContourCanon.GetContourForCellEdge(data.Left, gridSextant.Next2());

                if( previousIsConfluence &&
                    Geometry2D.IsPointWithinTriangle(xzPoint, centerRightContour.First(), leftCenterContour.First(), rightCenterContour.Last())
                ) {
                    PointOrientationWeightLogic.GetRiverCornerWeights(
                        xzPoint, data.Center, data.Left, data.Right, data.Sextant,
                        out data.CenterWeight, out data.LeftWeight, out data.RightWeight, out data.RiverWeight
                    );

                    return true;
                }
            }

            if(data.NextRight != null) {
                bool nextIsConfluence = RiverCanon.HasRiverAlongEdge(data.Center,    gridSextant)
                                     && RiverCanon.HasRiverAlongEdge(data.Center,    gridSextant.Next())
                                     && RiverCanon.HasRiverAlongEdge(data.NextRight, gridSextant.Previous());

                var nextRightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(data.NextRight, gridSextant.Previous2());

                if( nextIsConfluence &&
                    Geometry2D.IsPointWithinTriangle(xzPoint, centerRightContour.Last(), rightCenterContour.First(), nextRightCenterContour.Last())
                ) {
                    PointOrientationWeightLogic.GetRiverCornerWeights(
                        xzPoint, data.Center, data.Right, data.NextRight, data.Sextant,
                        out data.CenterWeight, out data.RightWeight, out data.NextRightWeight, out data.RiverWeight
                    );

                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

    }

}
