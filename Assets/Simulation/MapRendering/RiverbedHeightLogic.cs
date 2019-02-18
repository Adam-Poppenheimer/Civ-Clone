using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class RiverbedHeightLogic : IRiverbedHeightLogic {

        #region instance fields and properties

        private IMapRenderConfig    RenderConfig;
        private ICellHeightmapLogic CellHeightmapLogic;
        private IGeometry2D         Geometry2D;
        private IRiverCanon         RiverCanon;
        private ITerrainMixingLogic TerrainMixingLogic;

        #endregion

        #region constructors

        [Inject]
        public RiverbedHeightLogic(
            IMapRenderConfig renderConfig, ICellHeightmapLogic cellHeightmapLogic,
            IGeometry2D geometry2D, IRiverCanon riverCanon, ITerrainMixingLogic terrainMixingLogic
        ) {
            RenderConfig       = renderConfig;
            CellHeightmapLogic = cellHeightmapLogic;
            Geometry2D         = geometry2D;
            RiverCanon         = riverCanon;
            TerrainMixingLogic = terrainMixingLogic;
        }

        #endregion

        #region instance methods

        #region from IRiverbedHeightLogic

        public float GetHeightForRiverEdgeAtPoint(
            IHexCell center, IHexCell right, HexDirection direction, Vector3 position
        ) {
            Vector3 world_CenterSolidMidpoint = center.AbsolutePosition + RenderConfig.GetSolidEdgeMidpoint(direction);

            //A line going from CenterSolidMidpoint to the corresponding solid midpoint on Right
            //Local is relative to world_CenterSolidMidpoint.
            Vector3 local_RightSolidMidpoint = right.AbsolutePosition - world_CenterSolidMidpoint + RenderConfig.GetSolidEdgeMidpoint(direction.Opposite());

            Vector3 local_Point = position - world_CenterSolidMidpoint;

            Vector3 local_PointOntoMidline = Vector3.Project(local_Point, local_RightSolidMidpoint);

            float centerToRightPercent = local_PointOntoMidline.magnitude / local_RightSolidMidpoint.magnitude;

            float riverWeight = 1f - 2f * Mathf.Abs(centerToRightPercent - 0.5f);

            float centerWeight = Mathf.Max(0f, 1f - centerToRightPercent * 2f);
            float rightWeight  = Mathf.Max(0f, centerToRightPercent * 2f - 1f);

            return centerWeight * CellHeightmapLogic.GetHeightForPositionForCell(position, center, direction)
                 + rightWeight  * CellHeightmapLogic.GetHeightForPositionForCell(position, right, direction.Opposite())
                 + riverWeight  * RenderConfig.RiverbedElevation;
        }

        //Our goal here is to mix the height contributions from all three
        //edges. We do this by figuring out the weights of each of the edges
        //of our corner triangle relative to position, and then use those
        //weights to combine height samples from the edges.
        public float GetHeightForRiverPreviousCornerAtPoint(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 position,
            bool hasCenterRightRiver, bool hasCenterLeftRiver, bool hasLeftRightRiver
        ) {
            Vector2 positionXZ = new Vector2(position.x, position.z);

            Vector2 centerCorner = center.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction);
            Vector2 leftCorner   = left  .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Next2());
            Vector2 rightCorner  = right .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Previous2());

            Vector2 positionOntoCenterLeft  = Geometry2D.ProjectPointOntoLineSegment(centerCorner, leftCorner,  positionXZ);
            Vector2 positionOntoCenterRight = Geometry2D.ProjectPointOntoLineSegment(centerCorner, rightCorner, positionXZ);
            Vector2 positionOntoLeftRight   = Geometry2D.ProjectPointOntoLineSegment(leftCorner,   rightCorner, positionXZ);

            float centerLeftWeight, centerRightWeight, leftRightWeight;

            Geometry2D.GetBarycentric2D(
                positionXZ, positionOntoCenterLeft, positionOntoCenterRight, positionOntoLeftRight,
                out centerLeftWeight, out centerRightWeight, out leftRightWeight
            );

            float centerLeftHeight = RiverCanon.HasRiverAlongEdge(center, direction.Previous())
                                   ? GetHeightForRiverEdgeAtPoint(center, left, direction.Previous(), position)
                                   : TerrainMixingLogic.GetMixForEdgeAtPoint(center, left, direction.Previous(), position, EdgeSelector, (a, b) => a + b);

            float centerRightHeight = RiverCanon.HasRiverAlongEdge(center, direction)
                                    ? GetHeightForRiverEdgeAtPoint(center, right, direction, position)
                                    : TerrainMixingLogic.GetMixForEdgeAtPoint(center, right, direction, position, EdgeSelector, (a, b) => a + b);

            float leftRightHeight = RiverCanon.HasRiverAlongEdge(left, direction.Next())
                                  ? GetHeightForRiverEdgeAtPoint(left, right, direction.Next(), position)
                                  : TerrainMixingLogic.GetMixForEdgeAtPoint(left, right, direction.Next(), position, EdgeSelector, (a, b) => a + b);

            return centerLeftWeight  * centerLeftHeight  +
                   centerRightWeight * centerRightHeight +
                   leftRightWeight   * leftRightHeight;
        }

        public float GetHeightForRiverNextCornerAtPoint(
            IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, Vector3 position,
            bool hasCenterRightRiver, bool hasCenterNextRightRiver, bool hasRightNextRightRiver
        ) {
            Vector2 positionXZ = new Vector2(position.x, position.z);

            Vector2 centerCorner     = center   .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction);
            Vector2 rightCorner      = right    .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Next2());
            Vector2 nextRightCorner  = nextRight.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Previous2());

            Vector2 positionOntoCenterRight     = Geometry2D.ProjectPointOntoLineSegment(centerCorner, rightCorner,     positionXZ);
            Vector2 positionOntoCenterNextRight = Geometry2D.ProjectPointOntoLineSegment(centerCorner, nextRightCorner, positionXZ);
            Vector2 positionOntoRightNextRight  = Geometry2D.ProjectPointOntoLineSegment(rightCorner, nextRightCorner,  positionXZ);

            float centerRightWeight, centerNextRightWeight, rightNextRightWeight;
            Geometry2D.GetBarycentric2D(
                positionXZ, positionOntoCenterRight, positionOntoCenterNextRight, positionOntoRightNextRight,
                out centerRightWeight, out centerNextRightWeight, out rightNextRightWeight
            );

            float centerRightHeight = RiverCanon.HasRiverAlongEdge(center, direction)
                                    ? GetHeightForRiverEdgeAtPoint(center, right, direction, position)
                                    : TerrainMixingLogic.GetMixForEdgeAtPoint(center, right, direction, position, EdgeSelector, (a, b) => a + b);

            float centerNextRightHeight = RiverCanon.HasRiverAlongEdge(center, direction.Next())
                                        ? GetHeightForRiverEdgeAtPoint(center, nextRight, direction.Next(), position)
                                        : TerrainMixingLogic.GetMixForEdgeAtPoint(center, nextRight, direction.Next(), position, EdgeSelector, (a, b) => a + b);

            float rightNextRightHeight = RiverCanon.HasRiverAlongEdge(right, direction.Next2())
                                       ? GetHeightForRiverEdgeAtPoint(right, nextRight, direction.Next2(), position)
                                       : TerrainMixingLogic.GetMixForEdgeAtPoint(right, nextRight, direction.Next2(), position, EdgeSelector, (a, b) => a + b);

            return centerRightWeight      * centerRightHeight     +
                   centerNextRightWeight  * centerNextRightHeight +
                   rightNextRightWeight   * rightNextRightHeight;
        }

        #endregion

        private float GetPercentageDownLine(Vector3 position, Vector3 start, Vector3 end) {
            Vector3 startToPosition = position - start;

            Vector3 startToEndLine = end - start;

            Vector3 positionOntoLine = Vector3.Project(startToPosition, startToEndLine);

            return positionOntoLine.magnitude / startToEndLine.magnitude;
        }

        private float EdgeSelector(Vector3 position, IHexCell cell, HexDirection sextant, float weight) {
            return CellHeightmapLogic.GetHeightForPositionForCell(position, cell, sextant) * weight;
        }

        #endregion

    }

}
