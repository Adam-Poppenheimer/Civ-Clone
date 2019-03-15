using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MountainHeightmapLogic : IMountainHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig      RenderConfig;
        private INoiseGenerator       NoiseGenerator;
        private IHexGrid              Grid;
        private IHillsHeightmapLogic  HillsHeightmapLogic;
        private IRiverCanon           RiverCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IGeometry2D           Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IHexGrid grid, IHillsHeightmapLogic hillsHeightmapLogic,
            IRiverCanon riverCanon, ICellEdgeContourCanon cellEdgeContourCanon,
            IGeometry2D geometry2D
        ) {
            RenderConfig         = renderConfig;
            NoiseGenerator       = noiseGenerator;
            Grid                 = grid;
            HillsHeightmapLogic  = hillsHeightmapLogic;
            RiverCanon           = riverCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
            Geometry2D           = geometry2D;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapLogic

        
        public float GetHeightForPoint(Vector2 xzPoint, IHexCell center, HexDirection sextant) {
            var centerLeftContour      = CellEdgeContourCanon.GetContourForCellEdge(center, sextant.Previous());
            var centerRightContour     = CellEdgeContourCanon.GetContourForCellEdge(center, sextant);
            var centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, sextant.Next());

            Vector2 pointOnContour = CellEdgeContourCanon.GetClosestPointOnContour(
                xzPoint, centerRightContour
            );

            float distanceFromCenter = Mathf.Min(
                Vector2.Distance(centerLeftContour.Last(),       center.AbsolutePositionXZ),
                Vector2.Distance(pointOnContour,                 center.AbsolutePositionXZ),
                Vector2.Distance(centerNextRightContour.First(), center.AbsolutePositionXZ)
            );

            float distanceFromPoint = Mathf.Min(
                Vector2.Distance(centerLeftContour.Last(),       xzPoint),
                Vector2.Distance(pointOnContour,                 xzPoint),
                Vector2.Distance(centerNextRightContour.First(), xzPoint)
            );

            float edgeToPeakLerp = distanceFromPoint / distanceFromCenter;

            float firstCornerEdgeHeight = 0f, secondCornerEdgeHeight = 0f, rightEdgeHeight;

            IHexCell left      = Grid.GetNeighbor(center, sextant.Previous());
            IHexCell right     = Grid.GetNeighbor(center, sextant);
            IHexCell nextRight = Grid.GetNeighbor(center, sextant.Next());

            float flatlandsHeight = RenderConfig.FlatlandsBaseElevation + NoiseGenerator.SampleNoise(xzPoint, NoiseType.FlatlandsHeight).x;

            if(left == null) {
                firstCornerEdgeHeight += RenderConfig.FlatlandsBaseElevation;

            }else if(left.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant.Previous())) {
                firstCornerEdgeHeight += flatlandsHeight;

            }else {
                firstCornerEdgeHeight += HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant.Previous());
            }

            if(right == null) {
                firstCornerEdgeHeight  += RenderConfig.FlatlandsBaseElevation;
                secondCornerEdgeHeight += RenderConfig.FlatlandsBaseElevation;

                rightEdgeHeight = RenderConfig.FlatlandsBaseElevation;

            }else if(right.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant)) {
                firstCornerEdgeHeight  += flatlandsHeight;
                secondCornerEdgeHeight += flatlandsHeight;

                rightEdgeHeight = flatlandsHeight;

            }else {
                float hillsHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant);

                firstCornerEdgeHeight  += hillsHeight;
                secondCornerEdgeHeight += hillsHeight;

                rightEdgeHeight = hillsHeight;
            }

            if(nextRight == null) {
                secondCornerEdgeHeight += RenderConfig.FlatlandsBaseElevation;

            }else if(nextRight.Shape == CellShape.Flatlands || RiverCanon.HasRiverAlongEdge(center, sextant.Next())) {
                secondCornerEdgeHeight += flatlandsHeight;

            }else {
                secondCornerEdgeHeight += HillsHeightmapLogic.GetHeightForPoint(xzPoint, center, sextant.Next());
            }

            firstCornerEdgeHeight  /= 2f;
            secondCornerEdgeHeight /= 2f;

            Vector2 firstToLastSpan = centerRightContour.Last() - centerRightContour.First();
            Vector2 firstToPoint     = xzPoint                  - centerRightContour.First();

            Vector2 pointOntoSpawn = firstToPoint.Project(firstToLastSpan);

            float firstToLastLerp = pointOntoSpawn.magnitude / firstToLastSpan.magnitude;

            float edgeHeight = firstCornerEdgeHeight  * Mathf.Clamp01(1f - firstToLastLerp * 2f)
                             + rightEdgeHeight        * (1f - 2 * Mathf.Abs(firstToLastLerp - 0.5f))
                             + secondCornerEdgeHeight * Mathf.Clamp01(firstToLastLerp * 2f - 1f);

            return Mathf.Lerp(edgeHeight, RenderConfig.MountainPeakElevation, edgeToPeakLerp);
        }

        #endregion

        #endregion
        
    }

}
