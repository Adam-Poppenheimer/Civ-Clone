using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MountainHeightmapLogic : IMountainHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig         RenderConfig;
        private IHillsHeightmapLogic     HillsHeightmapLogic;
        private IRiverCanon              RiverCanon;
        private ICellEdgeContourCanon    CellEdgeContourCanon;
        private IFlatlandsHeightmapLogic FlatlandsHeightmapLogic;
        private IHexGrid                 Grid;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapLogic(
            IMapRenderConfig renderConfig, IHillsHeightmapLogic hillsHeightmapLogic,
            IRiverCanon riverCanon, ICellEdgeContourCanon cellEdgeContourCanon,
            IFlatlandsHeightmapLogic flatlandsHeightmapLogic, IHexGrid grid
        ) {
            RenderConfig            = renderConfig;
            HillsHeightmapLogic     = hillsHeightmapLogic;
            RiverCanon              = riverCanon;
            CellEdgeContourCanon    = cellEdgeContourCanon;
            FlatlandsHeightmapLogic = flatlandsHeightmapLogic;
            Grid                    = grid;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapLogic

        
        public float GetHeightForPoint(Vector2 xzPoint, IHexCell center, HexDirection sextant) {
            Profiler.BeginSample("MountainHeightmapLogic.GetHeightForPoint()");

            var centerLeftContour      = CellEdgeContourCanon.GetContourForCellEdge(center, sextant.Previous());
            var centerRightContour     = CellEdgeContourCanon.GetContourForCellEdge(center, sextant);
            var centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, sextant.Next());

            Vector2 pointOntoContourCR = CellEdgeContourCanon.GetClosestPointOnContour(
                xzPoint, centerRightContour
            );

            float distanceFromCenter = Mathf.Min(
                Vector2.Distance(centerLeftContour.Last(),       center.AbsolutePositionXZ),
                Vector2.Distance(pointOntoContourCR,             center.AbsolutePositionXZ),
                Vector2.Distance(centerNextRightContour.First(), center.AbsolutePositionXZ)
            );

            float distanceFromPoint = Mathf.Min(
                Vector2.Distance(centerLeftContour.Last(),       xzPoint),
                Vector2.Distance(pointOntoContourCR,             xzPoint),
                Vector2.Distance(centerNextRightContour.First(), xzPoint)
            );

            float edgeToPeakLerp = distanceFromPoint / distanceFromCenter;

            float leftEdgeHeight, nextRightEdgeHeight, rightEdgeHeight;

            IHexCell left      = Grid.GetNeighbor(center, sextant.Previous());
            IHexCell right     = Grid.GetNeighbor(center, sextant);
            IHexCell nextRight = Grid.GetNeighbor(center, sextant.Next());

            float flatlandsHeight = FlatlandsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant);

            if(left == null) {
                leftEdgeHeight = RenderConfig.FlatlandsBaseElevation;

            }else if(left.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant.Previous())) {
                leftEdgeHeight = flatlandsHeight;

            }else {
                leftEdgeHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant.Previous());
            }

            if(right == null) {
                rightEdgeHeight = RenderConfig.FlatlandsBaseElevation;

            }else if(right.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant)) {
                rightEdgeHeight = flatlandsHeight;

            }else {
                rightEdgeHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant);;
            }

            if(nextRight == null) {
                nextRightEdgeHeight = RenderConfig.FlatlandsBaseElevation;

            }else if(nextRight.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant.Next())) {
                nextRightEdgeHeight = flatlandsHeight;

            }else {
                nextRightEdgeHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant.Next());
            }

            Vector2 pointOntoContourCL  = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerLeftContour);
            Vector2 pointOntoContourCNR = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerNextRightContour);

            float sqDistFromCL  = (xzPoint - pointOntoContourCL ).sqrMagnitude;
            float sqDistFromCR  = (xzPoint - pointOntoContourCR ).sqrMagnitude;
            float sqDistFromCNR = (xzPoint - pointOntoContourCNR).sqrMagnitude;

            float leftWeight      = 1f / (1 + sqDistFromCL);
            float rightWeight     = 1f / (1 + sqDistFromCR);
            float nextRightWeight = 1f / (1 + sqDistFromCNR);

            float totalWeight = leftWeight + rightWeight + nextRightWeight;

            leftWeight      /= totalWeight;
            rightWeight     /= totalWeight;
            nextRightWeight /= totalWeight;

            float edgeHeight = leftWeight      * leftEdgeHeight
                             + rightWeight     * rightEdgeHeight
                             + nextRightWeight * nextRightEdgeHeight;

            Profiler.EndSample();
            return Mathf.Lerp(edgeHeight, RenderConfig.MountainPeakElevation, edgeToPeakLerp);
        }

        #endregion

        #endregion
        
    }

}
