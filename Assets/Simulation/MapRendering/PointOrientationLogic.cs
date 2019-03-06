using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationLogic : IPointOrientationLogic {

        #region instance fields and properties

        private HexDirection[] AllDirections;




        private IHexGrid              Grid;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IRiverCanon           RiverCanon;
        private IGeometry2D           Geometry2D;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(
            IHexGrid grid, ICellEdgeContourCanon cellEdgeContourCanon, IRiverCanon riverCanon,
            IGeometry2D geometry2D, IMapRenderConfig renderConfig
        ) {
            Grid                 = grid;
            CellEdgeContourCanon = cellEdgeContourCanon;
            RiverCanon           = riverCanon;
            Geometry2D           = geometry2D;
            RenderConfig         = renderConfig;

            AllDirections = EnumUtil.GetValues<HexDirection>().ToArray();
        }

        #endregion

        #region instance methods

        #region from IPointOrientationLogic

        public PointOrientationData GetOrientationDataForPoint(Vector2 xzPoint) {
            Vector3 xyzPoint = new Vector3(xzPoint.x, 0f, xzPoint.y);

            if(!Grid.HasCellAtLocation(xyzPoint)) {
                return new PointOrientationData();
            }

            IHexCell gridCenter = Grid.GetCellAtLocation(xyzPoint);

            HexDirection gridSextant = GetSextantOfPointInCell(xzPoint, gridCenter);

            PointOrientationData retval;

            if( TryFindValidOrientation(xzPoint, gridCenter, gridSextant,            out retval) ||
                TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Previous(), out retval) ||
                TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Next(),     out retval)
            ) {
                return retval;
            }else {
                return new PointOrientationData();
            }
        }

        #endregion

        private bool TryFindValidOrientation(Vector2 xzPoint, IHexCell gridCenter, HexDirection gridSextant, out PointOrientationData data) {
            data = new PointOrientationData() {
                IsOnGrid = true,
                Sextant  = gridSextant,

                Center    = gridCenter,
                Left      = Grid.GetNeighbor(gridCenter, gridSextant.Previous()),
                Right     = Grid.GetNeighbor(gridCenter, gridSextant),
                NextRight = Grid.GetNeighbor(gridCenter, gridSextant.Next()),
            };

            var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(gridCenter, gridSextant);

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, centerRightContour, gridCenter.AbsolutePositionXZ)) {
                data.CenterWeight = 1f;

                return true;
            }

            IHexCell gridRight = Grid.GetNeighbor(gridCenter, gridSextant);

            if(gridRight == null) {
                return false;
            }

            var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(gridRight, gridSextant.Opposite());

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, rightCenterContour, gridRight.AbsolutePositionXZ)) {
                data.RightWeight = 1f;

                return true;
            }

            if(CellEdgeContourCanon.IsPointBetweenContours(xzPoint, centerRightContour, rightCenterContour)) {
                data.RiverWeight = 1f;

                return true;
            }

            return false;
        }

        private HexDirection GetSextantOfPointInCell(Vector2 xzPoint, IHexCell cell) {
            foreach(var direction in AllDirections) {
                if(Geometry2D.IsPointWithinTriangle(
                    xzPoint, cell.AbsolutePositionXZ,
                    cell.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ (direction),
                    cell.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(direction)
                )) {
                    return direction;
                }
            }

            return HexDirection.NW;
        }

        #endregion

    }

}
