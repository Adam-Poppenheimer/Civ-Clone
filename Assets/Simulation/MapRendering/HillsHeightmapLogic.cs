using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class HillsHeightmapLogic : IHillsHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig      RenderConfig;
        private INoiseGenerator       NoiseGenerator;
        private IRiverCanon           RiverCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;

        #endregion

        #region constructors

        [Inject]
        public HillsHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IRiverCanon riverCanon, ICellEdgeContourCanon cellEdgeContourCanon
        ) {
            RenderConfig         = renderConfig;
            NoiseGenerator       = noiseGenerator;
            RiverCanon           = riverCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
        }

        #endregion

        #region instance methods

        #region from IHillsHeightmapLogic

        public float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            float hillsHeight = RenderConfig.HillsBaseElevation + NoiseGenerator.SampleNoise(xzPoint, NoiseType.HillsHeight).x;

            Vector2 nearestContourPoint;

            if(TryGetNearestContourPoint(xzPoint, cell, sextant, out nearestContourPoint)) {
                Vector2 contourToPoint  = xzPoint - nearestContourPoint;
                Vector2 contourToCenter = cell.AbsolutePositionXZ - nearestContourPoint;

                float hillsWeight     = Mathf.Sqrt(contourToPoint.magnitude / contourToCenter.magnitude);
                float flatlandsWeight = 1f - hillsWeight;
                        
                float flatlandsHeight = RenderConfig.FlatlandsBaseElevation + NoiseGenerator.SampleNoise(xzPoint, NoiseType.FlatlandsHeight).x;

                return hillsHeight * hillsWeight + flatlandsHeight * flatlandsWeight;
            }else {
                return hillsHeight;
            }
        }

        #endregion

        private bool TryGetNearestContourPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant, out Vector2 nearestContourPoint) {
            bool retval = false;

            nearestContourPoint = Vector2.zero;
            float nearestDistance = float.MaxValue;

            if(RiverCanon.HasRiverAlongEdge(cell, sextant)) {
                retval = true;

                var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(cell, sextant);

                Vector2 candidate = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerRightContour);

                float candidateDistance = Vector2.Distance(xzPoint, candidate);

                if(candidateDistance < nearestDistance) {
                    nearestContourPoint = candidate;
                    nearestDistance = candidateDistance;
                }
            }

            if(RiverCanon.HasRiverAlongEdge(cell, sextant.Previous())) {
                retval = true;

                var centerLeftContour = CellEdgeContourCanon.GetContourForCellEdge(cell, sextant.Previous());

                Vector2 candidate = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerLeftContour);

                float candidateDistance = Vector2.Distance(xzPoint, candidate);

                if(candidateDistance < nearestDistance) {
                    nearestContourPoint = candidate;
                    nearestDistance = candidateDistance;
                }
            }

            if(RiverCanon.HasRiverAlongEdge(cell, sextant.Next())) {
                retval = true;

                var centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(cell, sextant.Next());

                Vector2 candidate = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerNextRightContour);

                float candidateDistance = Vector2.Distance(xzPoint, candidate);

                if(candidateDistance < nearestDistance) {
                    nearestContourPoint = candidate;
                    nearestDistance = candidateDistance;
                }
            }

            return retval;
        }

        #endregion
        
    }

}
